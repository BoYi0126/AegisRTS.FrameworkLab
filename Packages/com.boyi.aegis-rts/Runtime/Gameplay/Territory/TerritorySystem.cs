using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Factions;

namespace AegisRTS.Gameplay.Territory
{
    /// <summary>Pure C# territory graph, ownership, visibility, and strategic value state.</summary>
    public sealed class TerritorySystem : ITerritoryQuery
    {
        private readonly Dictionary<EntityId, TerritoryRecord> _territories = new Dictionary<EntityId, TerritoryRecord>();
        private readonly Dictionary<EntityId, EntityId> _territoryBySettlement = new Dictionary<EntityId, EntityId>();
        private readonly FactionSystem _factions;
        private readonly EventBus _events;

        public TerritorySystem(FactionSystem factions, EventBus eventBus = null)
        { _factions = factions ?? throw new ArgumentNullException(nameof(factions)); _events = eventBus; }
        public int TerritoryCount => _territories.Count;

        public void RegisterNode(EntityId territoryId, TerritoryNodeProfile profile, EntityId ownerId)
        {
            if (!territoryId.IsValid) throw new ArgumentException("Territory ID must be valid.", nameof(territoryId));
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (!_factions.Contains(ownerId)) throw new InvalidOperationException("Territory owner faction must exist.");
            if (_territories.ContainsKey(territoryId)) throw new InvalidOperationException($"Territory {territoryId} is already registered.");
            if (profile.SettlementId.IsValid && _territoryBySettlement.ContainsKey(profile.SettlementId))
                throw new InvalidOperationException("A settlement can map to only one territory node.");
            var record = new TerritoryRecord(territoryId, profile, ownerId);
            _territories.Add(territoryId, record);
            if (profile.SettlementId.IsValid) _territoryBySettlement.Add(profile.SettlementId, territoryId);
            _factions.TransferTerritory(territoryId, default, ownerId);
        }

        public bool Connect(EntityId firstId, EntityId secondId)
        {
            if (firstId == secondId || !_territories.TryGetValue(firstId, out TerritoryRecord first) ||
                !_territories.TryGetValue(secondId, out TerritoryRecord second)) return false;
            bool changed = first.Connections.Add(secondId);
            second.Connections.Add(firstId);
            return changed;
        }

        public bool SetOwner(EntityId territoryId, EntityId newOwnerId)
        {
            if (!_territories.TryGetValue(territoryId, out TerritoryRecord territory) || !_factions.Contains(newOwnerId)) return false;
            if (territory.OwnerId == newOwnerId) return true;
            EntityId previous = territory.OwnerId;
            if (!_factions.TransferTerritory(territoryId, previous, newOwnerId)) return false;
            territory.OwnerId = newOwnerId;
            _events?.Publish(new TerritoryOwnerChangedEvent(territoryId, previous, newOwnerId));
            return true;
        }

        public bool SetOwnerForSettlement(EntityId settlementId, EntityId newOwnerId) =>
            _territoryBySettlement.TryGetValue(settlementId, out EntityId territoryId) && SetOwner(territoryId, newOwnerId);

        public bool SetVisibility(EntityId territoryId, EntityId factionId, TerritoryVisibility visibility)
        {
            if (!_territories.TryGetValue(territoryId, out TerritoryRecord territory) || !_factions.Contains(factionId)) return false;
            territory.Visibility[factionId] = visibility;
            return true;
        }

        public bool TryGetState(EntityId territoryId, out TerritorySnapshot snapshot)
        {
            if (!_territories.TryGetValue(territoryId, out TerritoryRecord territory)) { snapshot = default; return false; }
            snapshot = CreateSnapshot(territory); return true;
        }

        public bool TryGetTerritoryForSettlement(EntityId settlementId, out EntityId territoryId) =>
            _territoryBySettlement.TryGetValue(settlementId, out territoryId);

        public IReadOnlyList<TerritorySnapshot> Snapshot()
        {
            var result = new List<TerritorySnapshot>(_territories.Count);
            foreach (TerritoryRecord territory in _territories.Values) result.Add(CreateSnapshot(territory));
            result.Sort((left, right) => left.TerritoryId.CompareTo(right.TerritoryId));
            return result.AsReadOnly();
        }

        public string GetDebugSummary()
        {
            int connections = 0, visible = 0;
            foreach (TerritoryRecord territory in _territories.Values)
            { connections += territory.Connections.Count; foreach (TerritoryVisibility value in territory.Visibility.Values) if (value == TerritoryVisibility.Visible) visible++; }
            return $"Territories={_territories.Count}, Connections={connections / 2}, VisibleEntries={visible}";
        }

        private static TerritorySnapshot CreateSnapshot(TerritoryRecord territory)
        {
            var connections = new List<EntityId>(territory.Connections); connections.Sort();
            return new TerritorySnapshot(territory.TerritoryId, territory.Profile, territory.OwnerId,
                connections.AsReadOnly(), new Dictionary<EntityId, TerritoryVisibility>(territory.Visibility));
        }

        private sealed class TerritoryRecord
        {
            public TerritoryRecord(EntityId territoryId, TerritoryNodeProfile profile, EntityId ownerId)
            { TerritoryId = territoryId; Profile = profile; OwnerId = ownerId; }
            public EntityId TerritoryId { get; }
            public TerritoryNodeProfile Profile { get; }
            public EntityId OwnerId { get; set; }
            public HashSet<EntityId> Connections { get; } = new HashSet<EntityId>();
            public Dictionary<EntityId, TerritoryVisibility> Visibility { get; } = new Dictionary<EntityId, TerritoryVisibility>();
        }
    }
}
