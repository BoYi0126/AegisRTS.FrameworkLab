using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Formation;
using AegisRTS.Gameplay.Units;

namespace AegisRTS.Presentation.Selection
{
    public enum SelectableKind { Unit, Hero, Structure, Settlement }
    public enum SelectionAffiliation { Friendly, Enemy, Neutral }
    public enum SelectionModifier { Replace, Add, Toggle, Remove }

    public readonly struct SelectableDescriptor
    {
        public SelectableDescriptor(EntityId entityId, string definitionId, SelectableKind kind, SelectionAffiliation affiliation)
        {
            if (!entityId.IsValid) throw new ArgumentException("Entity identifier must be valid.", nameof(entityId));
            if (string.IsNullOrWhiteSpace(definitionId)) throw new ArgumentException("Definition identifier is required.", nameof(definitionId));
            EntityId = entityId;
            DefinitionId = definitionId.Trim();
            Kind = kind;
            Affiliation = affiliation;
        }

        public EntityId EntityId { get; }
        public string DefinitionId { get; }
        public SelectableKind Kind { get; }
        public SelectionAffiliation Affiliation { get; }
    }

    public sealed class SelectionChangedEvent : IEvent
    {
        public SelectionChangedEvent(IReadOnlyList<EntityId> selectedIds) => SelectedIds = selectedIds ?? throw new ArgumentNullException(nameof(selectedIds));
        public IReadOnlyList<EntityId> SelectedIds { get; }
    }

    public interface ISelectionQuery
    {
        IReadOnlyList<EntityId> SelectedIds { get; }
        bool IsSelected(EntityId entityId);
        bool TryGetDescriptor(EntityId entityId, out SelectableDescriptor descriptor);
    }

    /// <summary>Owns deterministic selection and control-group state without depending on Unity objects.</summary>
    public sealed class SelectionService : ISelectionQuery
    {
        private readonly Dictionary<EntityId, SelectableDescriptor> _registry = new Dictionary<EntityId, SelectableDescriptor>();
        private readonly HashSet<EntityId> _selected = new HashSet<EntityId>();
        private readonly Dictionary<int, EntityId[]> _controlGroups = new Dictionary<int, EntityId[]>();
        private readonly EventBus _eventBus;

        public SelectionService(EventBus eventBus = null) => _eventBus = eventBus;

        public IReadOnlyList<EntityId> SelectedIds
        {
            get
            {
                var result = new List<EntityId>(_selected);
                result.Sort();
                return result.AsReadOnly();
            }
        }

        public int RegisteredCount => _registry.Count;
        public bool IsSelected(EntityId entityId) => _selected.Contains(entityId);
        public bool TryGetDescriptor(EntityId entityId, out SelectableDescriptor descriptor) => _registry.TryGetValue(entityId, out descriptor);

        public void Register(SelectableDescriptor descriptor)
        {
            if (_registry.ContainsKey(descriptor.EntityId))
            {
                throw new InvalidOperationException($"Entity {descriptor.EntityId} is already registered for selection.");
            }
            _registry.Add(descriptor.EntityId, descriptor);
        }

        public bool Unregister(EntityId entityId)
        {
            bool removed = _registry.Remove(entityId);
            if (_selected.Remove(entityId)) PublishChanged();
            return removed;
        }

        public void Clear() => ApplySelection(Array.Empty<EntityId>(), SelectionModifier.Replace);

        public void Select(EntityId entityId, SelectionModifier modifier = SelectionModifier.Replace) =>
            ApplySelection(new[] { entityId }, modifier);

        public void SelectMany(IEnumerable<EntityId> entityIds, SelectionModifier modifier = SelectionModifier.Replace) =>
            ApplySelection(entityIds, modifier);

        public void SelectSameDefinition(EntityId seedId, IEnumerable<EntityId> candidates, SelectionModifier modifier = SelectionModifier.Replace)
        {
            if (!_registry.TryGetValue(seedId, out SelectableDescriptor seed)) return;
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));

            var matching = new List<EntityId>();
            foreach (EntityId candidateId in candidates)
            {
                if (_registry.TryGetValue(candidateId, out SelectableDescriptor candidate) &&
                    string.Equals(candidate.DefinitionId, seed.DefinitionId, StringComparison.Ordinal) &&
                    candidate.Affiliation == seed.Affiliation)
                {
                    matching.Add(candidateId);
                }
            }
            ApplySelection(matching, modifier);
        }

        public void AssignControlGroup(int index)
        {
            ValidateControlGroup(index);
            var snapshot = new List<EntityId>(_selected);
            snapshot.Sort();
            _controlGroups[index] = snapshot.ToArray();
        }

        public bool RecallControlGroup(int index, SelectionModifier modifier = SelectionModifier.Replace)
        {
            ValidateControlGroup(index);
            if (!_controlGroups.TryGetValue(index, out EntityId[] group)) return false;
            ApplySelection(group, modifier);
            return true;
        }

        public string GetDebugSummary() => $"Registered={_registry.Count}, Selected={_selected.Count}, ControlGroups={_controlGroups.Count}";

        private void ApplySelection(IEnumerable<EntityId> entityIds, SelectionModifier modifier)
        {
            if (entityIds == null) throw new ArgumentNullException(nameof(entityIds));
            var incoming = new HashSet<EntityId>();
            foreach (EntityId id in entityIds)
            {
                if (_registry.ContainsKey(id)) incoming.Add(id);
            }

            var before = new HashSet<EntityId>(_selected);
            if (modifier == SelectionModifier.Replace) _selected.Clear();
            foreach (EntityId id in incoming)
            {
                switch (modifier)
                {
                    case SelectionModifier.Replace:
                    case SelectionModifier.Add: _selected.Add(id); break;
                    case SelectionModifier.Toggle:
                        if (!_selected.Remove(id)) _selected.Add(id);
                        break;
                    case SelectionModifier.Remove: _selected.Remove(id); break;
                    default: throw new ArgumentOutOfRangeException(nameof(modifier));
                }
            }

            if (!before.SetEquals(_selected)) PublishChanged();
        }

        private void PublishChanged()
        {
            if (_eventBus == null) return;
            _eventBus.Publish(new SelectionChangedEvent(SelectedIds));
        }

        private static void ValidateControlGroup(int index)
        {
            if (index < 0 || index > 9) throw new ArgumentOutOfRangeException(nameof(index), "Control group must be between 0 and 9.");
        }
    }

    public readonly struct ContextTarget
    {
        private ContextTarget(WorldPoint point, bool hasEntity, SelectableDescriptor descriptor)
        {
            Point = point;
            HasEntity = hasEntity;
            Descriptor = descriptor;
        }

        public WorldPoint Point { get; }
        public bool HasEntity { get; }
        public SelectableDescriptor Descriptor { get; }
        public static ContextTarget Ground(WorldPoint point) => new ContextTarget(point, false, default);
        public static ContextTarget Entity(WorldPoint point, SelectableDescriptor descriptor) => new ContextTarget(point, true, descriptor);
    }

    /// <summary>Maps a context target to the same gameplay commands used by AI and tests.</summary>
    public static class ContextCommandResolver
    {
        public static ICommand Resolve(
            IReadOnlyList<EntityId> actors,
            ContextTarget target,
            bool queue,
            FormationType formation = FormationType.Box)
        {
            if (actors == null) throw new ArgumentNullException(nameof(actors));
            if (actors.Count == 0) return null;
            if (!target.HasEntity) return new MoveUnitsCommand(actors, target.Point, queue, formation);
            if (target.Descriptor.Kind == SelectableKind.Settlement)
                return new InteractTargetCommand(actors, target.Descriptor.EntityId, queue);
            if (target.Descriptor.Affiliation == SelectionAffiliation.Enemy)
                return new AttackTargetCommand(actors, target.Descriptor.EntityId, queue);
            if (target.Descriptor.Affiliation == SelectionAffiliation.Friendly)
                return new FollowTargetCommand(actors, target.Descriptor.EntityId, queue);
            return null;
        }
    }
}
