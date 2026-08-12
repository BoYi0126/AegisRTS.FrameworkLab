using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Units;

namespace AegisRTS.Demo.PlayablePrototype
{
    public sealed class PrototypeEntityRecord
    {
        public PrototypeEntityRecord(EntityId entityId, string definitionId, EntityId factionId, WorldPoint spawnPosition,
            double movementSpeed, CombatantProfile combatProfile, bool isHero)
        {
            EntityId = entityId;
            DefinitionId = definitionId ?? throw new ArgumentNullException(nameof(definitionId));
            FactionId = factionId;
            SpawnPosition = spawnPosition;
            MovementSpeed = movementSpeed;
            CombatProfile = combatProfile ?? throw new ArgumentNullException(nameof(combatProfile));
            IsHero = isHero;
        }

        public EntityId EntityId { get; }
        public string DefinitionId { get; }
        public EntityId FactionId { get; }
        public WorldPoint SpawnPosition { get; }
        public double MovementSpeed { get; }
        public CombatantProfile CombatProfile { get; }
        public bool IsHero { get; }
    }

    /// <summary>Owns the cross-system identity map; Unity views are attached outside the domain composition.</summary>
    public sealed class PrototypeEntityRegistry
    {
        private readonly Dictionary<EntityId, PrototypeEntityRecord> _records = new Dictionary<EntityId, PrototypeEntityRecord>();

        public int Count => _records.Count;

        public void Register(PrototypeEntityRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (_records.ContainsKey(record.EntityId)) throw new InvalidOperationException($"Prototype entity {record.EntityId} is already registered.");
            _records.Add(record.EntityId, record);
        }

        public bool Remove(EntityId entityId) => _records.Remove(entityId);
        public bool TryGet(EntityId entityId, out PrototypeEntityRecord record) => _records.TryGetValue(entityId, out record);

        public IReadOnlyList<PrototypeEntityRecord> Snapshot()
        {
            var result = new List<PrototypeEntityRecord>(_records.Values);
            result.Sort((left, right) => left.EntityId.CompareTo(right.EntityId));
            return result.AsReadOnly();
        }

        public IReadOnlyList<EntityId> GetFactionEntities(EntityId factionId)
        {
            var result = new List<EntityId>();
            foreach (PrototypeEntityRecord record in _records.Values)
                if (record.FactionId == factionId) result.Add(record.EntityId);
            result.Sort();
            return result.AsReadOnly();
        }
    }
}
