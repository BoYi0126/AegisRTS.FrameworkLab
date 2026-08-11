using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;

namespace AegisRTS.Gameplay.Factions
{
    /// <summary>Pure C# faction resources, ownership indices, technology, diplomacy, and AI profile state.</summary>
    public sealed class FactionSystem : IFactionQuery
    {
        private readonly Dictionary<EntityId, FactionRecord> _factions = new Dictionary<EntityId, FactionRecord>();
        private readonly EventBus _events;

        public FactionSystem(EventBus eventBus = null) => _events = eventBus;
        public int FactionCount => _factions.Count;

        public void Register(EntityId factionId, FactionProfile profile)
        {
            if (!factionId.IsValid) throw new ArgumentException("Faction ID must be valid.", nameof(factionId));
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (_factions.ContainsKey(factionId)) throw new InvalidOperationException($"Faction {factionId} is already registered.");
            _factions.Add(factionId, new FactionRecord(factionId, profile));
        }

        public bool Contains(EntityId factionId) => _factions.ContainsKey(factionId);

        public bool AddResource(EntityId factionId, string resourceId, double delta)
        {
            if (!_factions.TryGetValue(factionId, out FactionRecord faction) || string.IsNullOrWhiteSpace(resourceId) || !IsFinite(delta)) return false;
            string id = resourceId.Trim();
            faction.Resources.TryGetValue(id, out double current);
            if (current + delta < 0d) return false;
            faction.Resources[id] = current + delta;
            return true;
        }

        public bool UnlockTechnology(EntityId factionId, string technologyId)
        {
            if (!_factions.TryGetValue(factionId, out FactionRecord faction) || string.IsNullOrWhiteSpace(technologyId)) return false;
            return faction.Technologies.Add(technologyId.Trim());
        }

        public bool SetDiplomacy(EntityId firstFactionId, EntityId secondFactionId, DiplomacyStatus status)
        {
            if (firstFactionId == secondFactionId || !_factions.TryGetValue(firstFactionId, out FactionRecord first) ||
                !_factions.TryGetValue(secondFactionId, out FactionRecord second)) return false;
            first.Diplomacy[secondFactionId] = status;
            second.Diplomacy[firstFactionId] = status;
            _events?.Publish(new DiplomacyChangedEvent(firstFactionId, secondFactionId, status));
            return true;
        }

        public bool AssignArmy(EntityId armyId, EntityId factionId)
        {
            if (!armyId.IsValid || !_factions.TryGetValue(factionId, out FactionRecord faction)) return false;
            foreach (FactionRecord other in _factions.Values) other.ArmyIds.Remove(armyId);
            faction.ArmyIds.Add(armyId);
            return true;
        }

        public bool RemoveArmy(EntityId armyId)
        {
            bool removed = false;
            foreach (FactionRecord faction in _factions.Values) removed |= faction.ArmyIds.Remove(armyId);
            return removed;
        }

        public bool TransferSettlement(EntityId settlementId, EntityId previousOwnerId, EntityId newOwnerId)
        {
            if (!settlementId.IsValid || !_factions.TryGetValue(newOwnerId, out FactionRecord next)) return false;
            if (previousOwnerId.IsValid && _factions.TryGetValue(previousOwnerId, out FactionRecord previous)) previous.SettlementIds.Remove(settlementId);
            foreach (FactionRecord faction in _factions.Values) if (faction.FactionId != newOwnerId) faction.SettlementIds.Remove(settlementId);
            next.SettlementIds.Add(settlementId);
            return true;
        }

        public bool TransferTerritory(EntityId territoryId, EntityId previousOwnerId, EntityId newOwnerId)
        {
            if (!territoryId.IsValid || !_factions.TryGetValue(newOwnerId, out FactionRecord next)) return false;
            if (previousOwnerId.IsValid && _factions.TryGetValue(previousOwnerId, out FactionRecord previous)) previous.TerritoryIds.Remove(territoryId);
            foreach (FactionRecord faction in _factions.Values) if (faction.FactionId != newOwnerId) faction.TerritoryIds.Remove(territoryId);
            next.TerritoryIds.Add(territoryId);
            return true;
        }

        public bool TryGetState(EntityId factionId, out FactionSnapshot snapshot)
        {
            if (!_factions.TryGetValue(factionId, out FactionRecord faction)) { snapshot = default; return false; }
            snapshot = CreateSnapshot(faction);
            return true;
        }

        public IReadOnlyList<FactionSnapshot> Snapshot()
        {
            var result = new List<FactionSnapshot>(_factions.Count);
            foreach (FactionRecord faction in _factions.Values) result.Add(CreateSnapshot(faction));
            result.Sort((left, right) => left.FactionId.CompareTo(right.FactionId));
            return result.AsReadOnly();
        }

        public string GetDebugSummary()
        {
            int settlements = 0, territories = 0, armies = 0;
            foreach (FactionRecord faction in _factions.Values)
            { settlements += faction.SettlementIds.Count; territories += faction.TerritoryIds.Count; armies += faction.ArmyIds.Count; }
            return $"Factions={_factions.Count}, Settlements={settlements}, Territories={territories}, Armies={armies}";
        }

        private static FactionSnapshot CreateSnapshot(FactionRecord faction)
        {
            var settlements = new List<EntityId>(faction.SettlementIds); settlements.Sort();
            var territories = new List<EntityId>(faction.TerritoryIds); territories.Sort();
            var armies = new List<EntityId>(faction.ArmyIds); armies.Sort();
            var technologies = new List<string>(faction.Technologies); technologies.Sort(StringComparer.Ordinal);
            return new FactionSnapshot(faction.FactionId, faction.Profile,
                new Dictionary<string, double>(faction.Resources, StringComparer.Ordinal), settlements.AsReadOnly(),
                territories.AsReadOnly(), armies.AsReadOnly(), technologies.AsReadOnly(),
                new Dictionary<EntityId, DiplomacyStatus>(faction.Diplomacy));
        }

        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

        private sealed class FactionRecord
        {
            public FactionRecord(EntityId factionId, FactionProfile profile) { FactionId = factionId; Profile = profile; }
            public EntityId FactionId { get; }
            public FactionProfile Profile { get; }
            public Dictionary<string, double> Resources { get; } = new Dictionary<string, double>(StringComparer.Ordinal);
            public HashSet<EntityId> SettlementIds { get; } = new HashSet<EntityId>();
            public HashSet<EntityId> TerritoryIds { get; } = new HashSet<EntityId>();
            public HashSet<EntityId> ArmyIds { get; } = new HashSet<EntityId>();
            public HashSet<string> Technologies { get; } = new HashSet<string>(StringComparer.Ordinal);
            public Dictionary<EntityId, DiplomacyStatus> Diplomacy { get; } = new Dictionary<EntityId, DiplomacyStatus>();
        }
    }
}
