using System;
using System.Linq;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Units;
using AegisRTS.Presentation.Camera;
using AegisRTS.Presentation.Selection;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class RtsInputSelectionCameraTests
    {
        [Test]
        public void Selection_ClickAndShiftToggle_UpdatesDeterministically()
        {
            var service = CreateSelection(out SelectableDescriptor first, out SelectableDescriptor second, out _);
            service.Select(first.EntityId);
            service.Select(second.EntityId, SelectionModifier.Toggle);
            CollectionAssert.AreEqual(new[] { first.EntityId, second.EntityId }, service.SelectedIds);

            service.Select(first.EntityId, SelectionModifier.Toggle);
            CollectionAssert.AreEqual(new[] { second.EntityId }, service.SelectedIds);
        }

        [Test]
        public void Selection_BoxAndDoubleClickSameDefinition_SelectExpectedActors()
        {
            var service = CreateSelection(out SelectableDescriptor first, out SelectableDescriptor second, out SelectableDescriptor enemy);
            service.SelectMany(new[] { first.EntityId, enemy.EntityId });
            CollectionAssert.AreEqual(new[] { first.EntityId, enemy.EntityId }, service.SelectedIds);

            service.SelectSameDefinition(first.EntityId, new[] { first.EntityId, second.EntityId, enemy.EntityId });
            CollectionAssert.AreEqual(new[] { first.EntityId, second.EntityId }, service.SelectedIds);
        }

        [Test]
        public void Selection_ControlGroupStoresSnapshotAndFiltersDespawnedEntities()
        {
            var service = CreateSelection(out SelectableDescriptor first, out SelectableDescriptor second, out _);
            service.SelectMany(new[] { first.EntityId, second.EntityId });
            service.AssignControlGroup(1);
            service.Clear();
            service.Unregister(second.EntityId);

            Assert.That(service.RecallControlGroup(1), Is.True);
            CollectionAssert.AreEqual(new[] { first.EntityId }, service.SelectedIds);
        }

        [Test]
        public void Selection_ChangedEventPublishesOnlyForActualChanges()
        {
            var events = new EventBus();
            var service = new SelectionService(events);
            var descriptor = Descriptor(1, "unit.spear", SelectionAffiliation.Friendly);
            service.Register(descriptor);
            int publishCount = 0;
            using IDisposable subscription = events.Subscribe<SelectionChangedEvent>(_ => publishCount++);

            service.Select(descriptor.EntityId);
            service.Select(descriptor.EntityId);
            Assert.That(publishCount, Is.EqualTo(1));
            Assert.That(service.Revision, Is.EqualTo(1));
        }

        [Test]
        public void SelectionCommandContext_ChoosesDomesticUnitsAndSiegeDeterministically()
        {
            var selection = new SelectionService();
            var city = new SelectableDescriptor(new EntityId(10), "settlement.city", SelectableKind.Settlement, SelectionAffiliation.Friendly);
            var infantry = new SelectableDescriptor(new EntityId(11), "unit.infantry", SelectableKind.Unit, SelectionAffiliation.Friendly);
            var enemyGate = new SelectableDescriptor(new EntityId(12), "structure.gate", SelectableKind.Structure, SelectionAffiliation.Enemy);
            selection.Register(city);
            selection.Register(infantry);
            selection.Register(enemyGate);

            selection.Select(city.EntityId);
            Assert.That(SelectionCommandContextResolver.Resolve(selection), Is.EqualTo(SelectionCommandContext.Domestic));
            selection.Select(enemyGate.EntityId);
            Assert.That(SelectionCommandContextResolver.Resolve(selection), Is.EqualTo(SelectionCommandContext.Siege));
            selection.SelectMany(new[] { city.EntityId, infantry.EntityId });
            Assert.That(SelectionCommandContextResolver.Resolve(selection), Is.EqualTo(SelectionCommandContext.UnitSettings),
                "A mixed box selection must prioritize controllable units over buildings.");
            selection.Clear();
            Assert.That(SelectionCommandContextResolver.Resolve(selection), Is.EqualTo(SelectionCommandContext.None));
        }

        [TestCase(SelectionAffiliation.Enemy, SelectableKind.Unit, typeof(AttackTargetCommand))]
        [TestCase(SelectionAffiliation.Friendly, SelectableKind.Unit, typeof(FollowTargetCommand))]
        [TestCase(SelectionAffiliation.Neutral, SelectableKind.Settlement, typeof(InteractTargetCommand))]
        public void ContextCommand_TargetResolvesExpectedIntent(SelectionAffiliation affiliation, SelectableKind kind, Type expectedType)
        {
            var target = new SelectableDescriptor(new EntityId(9), "target", kind, affiliation);
            ICommand command = ContextCommandResolver.Resolve(
                new[] { new EntityId(1), new EntityId(2) },
                ContextTarget.Entity(new WorldPoint(3, 0, 4), target),
                true);

            Assert.That(command, Is.TypeOf(expectedType));
            Assert.That(((UnitCommand)command).Queue, Is.True);
        }

        [Test]
        public void ContextCommand_GroundResolvesMoveAndCopiesActorIds()
        {
            var source = new[] { new EntityId(1), new EntityId(2) };
            var command = (MoveUnitsCommand)ContextCommandResolver.Resolve(
                source, ContextTarget.Ground(new WorldPoint(3, 0, 4)), false);
            source[0] = new EntityId(99);

            Assert.That(command.Destination, Is.EqualTo(new WorldPoint(3, 0, 4)));
            CollectionAssert.AreEqual(new[] { new EntityId(1), new EntityId(2) }, command.ActorIds);
        }

        [Test]
        public void Camera_PanZoomAndFocus_AreClampedToBounds()
        {
            var camera = new RtsCameraRigModel(
                pivotX: 0, pivotZ: 0, zoom: 20,
                minimumX: -10, maximumX: 10, minimumZ: -5, maximumZ: 5,
                minimumZoom: 8, maximumZoom: 30);

            camera.Pan(100, -100);
            camera.ZoomBy(100);
            Assert.That(camera.PivotX, Is.EqualTo(10));
            Assert.That(camera.PivotZ, Is.EqualTo(-5));
            Assert.That(camera.Zoom, Is.EqualTo(30));

            camera.Focus(-100, 100);
            camera.ZoomBy(-100);
            Assert.That(camera.PivotX, Is.EqualTo(-10));
            Assert.That(camera.PivotZ, Is.EqualTo(5));
            Assert.That(camera.Zoom, Is.EqualTo(8));
        }

        [Test]
        public void CommandBus_PlayerAndAiCanDispatchSameUnitCommandType()
        {
            var bus = new CommandBus();
            int handled = 0;
            using IDisposable registration = bus.RegisterHandler<MoveUnitsCommand>(_ => handled++);
            var actor = new[] { new EntityId(1) };

            bus.Dispatch(new MoveUnitsCommand(actor, new WorldPoint(1, 0, 1)));
            bus.Dispatch(new MoveUnitsCommand(actor, new WorldPoint(2, 0, 2), true));
            Assert.That(handled, Is.EqualTo(2));
        }

        private static SelectionService CreateSelection(
            out SelectableDescriptor first,
            out SelectableDescriptor second,
            out SelectableDescriptor enemy)
        {
            var service = new SelectionService();
            first = Descriptor(1, "unit.spear", SelectionAffiliation.Friendly);
            second = Descriptor(2, "unit.spear", SelectionAffiliation.Friendly);
            enemy = Descriptor(3, "unit.spear", SelectionAffiliation.Enemy);
            service.Register(first);
            service.Register(second);
            service.Register(enemy);
            return service;
        }

        private static SelectableDescriptor Descriptor(ulong id, string definitionId, SelectionAffiliation affiliation) =>
            new SelectableDescriptor(new EntityId(id), definitionId, SelectableKind.Unit, affiliation);
    }
}
