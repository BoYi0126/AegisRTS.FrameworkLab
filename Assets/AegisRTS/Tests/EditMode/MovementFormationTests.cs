using System.Collections.Generic;
using System.Linq;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Formation;
using AegisRTS.Gameplay.Movement;
using AegisRTS.Gameplay.Units;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class MovementFormationTests
    {
        [Test]
        public void LineFormation_CreatesSymmetricDistinctSlots()
        {
            IReadOnlyList<FormationSlot> slots = FormationPlanner.Plan(
                new WorldPoint(10, 0, 20), 4, FormationType.Line, 2, 0, 1);

            Assert.That(slots.Count, Is.EqualTo(4));
            Assert.That(slots.Select(slot => slot.Destination).Distinct().Count(), Is.EqualTo(4));
            Assert.That(slots[0].Destination.X, Is.EqualTo(7));
            Assert.That(slots[3].Destination.X, Is.EqualTo(13));
        }

        [Test]
        public void BoxFormation_FiftyUnitsNeverShareDestination()
        {
            IReadOnlyList<FormationSlot> slots = FormationPlanner.Plan(
                new WorldPoint(0, 0, 0), 50, FormationType.Box, 1.8, 1, 0);

            Assert.That(slots.Count, Is.EqualTo(50));
            Assert.That(slots.Select(slot => slot.Destination).Distinct().Count(), Is.EqualTo(50));
            Assert.That(slots.Select(slot => slot.Destination.X).Distinct().Count(), Is.GreaterThan(1));
            Assert.That(slots.Select(slot => slot.Destination.Z).Distinct().Count(), Is.GreaterThan(1));
        }

        [Test]
        public void MoveCommand_AssignsOneFormationSlotPerRegisteredActor()
        {
            var navigation = new FakeNavigationAdapter();
            var movement = new MovementSystem(navigation);
            EntityId[] actors = RegisterUnits(movement, navigation, 50);

            MovementCommandResult result = movement.IssueMove(
                new MoveUnitsCommand(actors, new WorldPoint(20, 0, 5), formation: FormationType.Box));

            Assert.That(result.AcceptedActorCount, Is.EqualTo(50));
            Assert.That(navigation.Destinations.Values.Distinct().Count(), Is.EqualTo(50));
            CollectionAssert.AreEquivalent(Enumerable.Range(0, 50), navigation.FormationSlots.Values);
        }

        [Test]
        public void UnreachableDestination_IsReportedWithoutLeavingActiveOrder()
        {
            var navigation = new FakeNavigationAdapter { RejectDestinations = true };
            var movement = new MovementSystem(navigation);
            EntityId[] actors = RegisterUnits(movement, navigation, 1);

            MovementCommandResult result = movement.IssueMove(new MoveUnitsCommand(actors, new WorldPoint(5, 0, 5)));

            Assert.That(result.WasAccepted, Is.False);
            Assert.That(movement.TryGetState(actors[0], out MovementStateSnapshot state), Is.True);
            Assert.That(state.Status, Is.EqualTo(MovementStatus.Unreachable));
            Assert.That(state.FormationSlotIndex, Is.EqualTo(-1));
        }

        [Test]
        public void QueuedMove_StartsAfterCurrentDestinationArrives()
        {
            var navigation = new FakeNavigationAdapter();
            var movement = new MovementSystem(navigation);
            EntityId[] actors = RegisterUnits(movement, navigation, 1);
            movement.IssueMove(new MoveUnitsCommand(actors, new WorldPoint(5, 0, 0)));
            movement.IssueMove(new MoveUnitsCommand(actors, new WorldPoint(10, 0, 0), queue: true));
            Assert.That(navigation.SetDestinationCallCount, Is.EqualTo(1));

            navigation.SetArrived(actors[0], new WorldPoint(5, 0, 0));
            movement.Tick(0.1);

            Assert.That(navigation.SetDestinationCallCount, Is.EqualTo(2));
            Assert.That(navigation.Destinations[actors[0]].X, Is.EqualTo(10));
            movement.TryGetState(actors[0], out MovementStateSnapshot state);
            Assert.That(state.Status, Is.EqualTo(MovementStatus.Moving));
            Assert.That(state.QueuedOrderCount, Is.EqualTo(0));
        }

        [Test]
        public void StuckAgent_RepathsThreeTimesThenStops()
        {
            var navigation = new FakeNavigationAdapter();
            var movement = new MovementSystem(navigation);
            EntityId[] actors = RegisterUnits(movement, navigation, 1);
            movement.IssueMove(new MoveUnitsCommand(actors, new WorldPoint(20, 0, 0)));

            for (int attempt = 0; attempt < 4; attempt++) movement.Tick(2.1);

            movement.TryGetState(actors[0], out MovementStateSnapshot state);
            Assert.That(state.Status, Is.EqualTo(MovementStatus.Stuck));
            Assert.That(state.RepathCount, Is.EqualTo(3));
            Assert.That(navigation.SetDestinationCallCount, Is.EqualTo(4));
            Assert.That(navigation.StopCallCount, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void StopAndHold_ClearQueuedMovement()
        {
            var navigation = new FakeNavigationAdapter();
            var movement = new MovementSystem(navigation);
            EntityId[] actors = RegisterUnits(movement, navigation, 1);
            movement.IssueMove(new MoveUnitsCommand(actors, new WorldPoint(5, 0, 0)));
            movement.IssueMove(new MoveUnitsCommand(actors, new WorldPoint(10, 0, 0), queue: true));

            movement.IssueHold(new HoldUnitsCommand(actors));
            movement.TryGetState(actors[0], out MovementStateSnapshot held);
            Assert.That(held.Status, Is.EqualTo(MovementStatus.Holding));
            Assert.That(held.QueuedOrderCount, Is.EqualTo(0));

            movement.IssueStop(new StopUnitsCommand(actors));
            movement.TryGetState(actors[0], out MovementStateSnapshot stopped);
            Assert.That(stopped.Status, Is.EqualTo(MovementStatus.Idle));
        }

        private static EntityId[] RegisterUnits(MovementSystem movement, FakeNavigationAdapter navigation, int count)
        {
            var result = new EntityId[count];
            for (int index = 0; index < count; index++)
            {
                var entityId = new EntityId((ulong)index + 1UL);
                var position = new WorldPoint(index, 0, 0);
                movement.Register(entityId, position);
                navigation.Register(entityId, position);
                result[index] = entityId;
            }
            return result;
        }

        private sealed class FakeNavigationAdapter : INavigationAdapter
        {
            private readonly Dictionary<EntityId, NavigationAgentSnapshot> _snapshots =
                new Dictionary<EntityId, NavigationAgentSnapshot>();

            public Dictionary<EntityId, WorldPoint> Destinations { get; } = new Dictionary<EntityId, WorldPoint>();
            public Dictionary<EntityId, int> FormationSlots { get; } = new Dictionary<EntityId, int>();
            public bool RejectDestinations { get; set; }
            public int SetDestinationCallCount { get; private set; }
            public int StopCallCount { get; private set; }

            public void Register(EntityId entityId, WorldPoint position) =>
                _snapshots[entityId] = MovingSnapshot(position, 10);

            public NavigationDestinationResult SetDestination(EntityId entityId, WorldPoint destination, int formationSlotIndex)
            {
                SetDestinationCallCount++;
                if (RejectDestinations) return NavigationDestinationResult.Failure("Rejected by test adapter.");
                Destinations[entityId] = destination;
                FormationSlots[entityId] = formationSlotIndex;
                WorldPoint position = _snapshots[entityId].Position;
                _snapshots[entityId] = MovingSnapshot(position, 10);
                return NavigationDestinationResult.Success(destination, 2);
            }

            public void Stop(EntityId entityId) => StopCallCount++;

            public bool TryGetSnapshot(EntityId entityId, out NavigationAgentSnapshot snapshot) =>
                _snapshots.TryGetValue(entityId, out snapshot);

            public void SetArrived(EntityId entityId, WorldPoint position) =>
                _snapshots[entityId] = new NavigationAgentSnapshot(
                    position, default, 0, NavigationPathState.Complete, true);

            private static NavigationAgentSnapshot MovingSnapshot(WorldPoint position, double remaining) =>
                new NavigationAgentSnapshot(position, default, remaining, NavigationPathState.Complete, true);
        }
    }
}
