using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Factions;
using AegisRTS.Gameplay.Territory;

namespace AegisRTS.Gameplay.Settlements
{
    /// <summary>Pure C# settlement ownership, population, garrison, production state, defense, and capture.</summary>
    public sealed class SettlementSystem : ISettlementQuery
    {
        private readonly Dictionary<EntityId, SettlementRecord> _settlements = new Dictionary<EntityId, SettlementRecord>();
        private readonly FactionSystem _factions;
        private readonly TerritorySystem _territories;
        private readonly EventBus _events;

        public SettlementSystem(FactionSystem factions, TerritorySystem territories, EventBus eventBus = null)
        { _factions = factions ?? throw new ArgumentNullException(nameof(factions)); _territories = territories ?? throw new ArgumentNullException(nameof(territories)); _events = eventBus; }
        public int SettlementCount => _settlements.Count;

        public void Register(EntityId settlementId, SettlementProfile profile, EntityId ownerId)
        {
            if (!settlementId.IsValid) throw new ArgumentException("Settlement ID must be valid.", nameof(settlementId));
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (!_factions.Contains(ownerId)) throw new InvalidOperationException("Settlement owner faction must exist.");
            if (_settlements.ContainsKey(settlementId)) throw new InvalidOperationException($"Settlement {settlementId} is already registered.");
            var record = new SettlementRecord(settlementId, profile, ownerId);
            _settlements.Add(settlementId, record);
            _factions.TransferSettlement(settlementId, default, ownerId);
            if (_territories.TryGetTerritoryForSettlement(settlementId, out EntityId territoryId)) _territories.SetOwner(territoryId, ownerId);
        }

        public SettlementCommandResult Validate(CaptureSettlementCommand command)
        {
            if (command == null) return SettlementCommandResult.Failure("Command is required.");
            if (!_settlements.TryGetValue(command.SettlementId, out SettlementRecord settlement)) return SettlementCommandResult.Failure("Settlement does not exist.");
            if (!_factions.Contains(command.NewOwnerId)) return SettlementCommandResult.Failure("New owner faction does not exist.");
            if (settlement.OwnerId == command.NewOwnerId) return SettlementCommandResult.Failure("Settlement is already owned by that faction.");
            if (!settlement.Profile.CaptureRule.IsSatisfied(command.CompletedConditions))
                return SettlementCommandResult.Failure($"Capture requires {settlement.Profile.CaptureRule.RequiredConditions}.");
            if (command.CapturingArmyId.IsValid && (!TryFaction(command.NewOwnerId, out FactionSnapshot faction) || !Contains(faction.ArmyIds, command.CapturingArmyId)))
                return SettlementCommandResult.Failure("Capturing army must belong to the new owner faction.");
            return SettlementCommandResult.Success(command.SettlementId);
        }

        public SettlementCommandResult Execute(CaptureSettlementCommand command)
        {
            SettlementCommandResult validation = Validate(command);
            if (!validation.Succeeded) return validation;
            SettlementRecord settlement = _settlements[command.SettlementId];
            EntityId previous = settlement.OwnerId;
            if (_territories.TryGetTerritoryForSettlement(settlement.SettlementId, out EntityId territoryId) &&
                !_territories.SetOwner(territoryId, command.NewOwnerId))
                return SettlementCommandResult.Failure("Mapped territory ownership could not be updated.");
            if (!_factions.TransferSettlement(settlement.SettlementId, previous, command.NewOwnerId))
                return SettlementCommandResult.Failure("Faction settlement ownership could not be updated.");
            settlement.OwnerId = command.NewOwnerId;
            _events?.Publish(new SettlementOwnerChangedEvent(settlement.SettlementId, previous, settlement.OwnerId, command.CapturingArmyId));
            return SettlementCommandResult.Success(settlement.SettlementId);
        }

        public bool SetGarrison(EntityId settlementId, IEnumerable<EntityId> unitIds)
        {
            if (!_settlements.TryGetValue(settlementId, out SettlementRecord settlement) || unitIds == null) return false;
            settlement.GarrisonIds.Clear();
            foreach (EntityId unitId in unitIds) if (unitId.IsValid && !settlement.GarrisonIds.Contains(unitId)) settlement.GarrisonIds.Add(unitId);
            settlement.GarrisonIds.Sort();
            return true;
        }

        public bool AddResource(EntityId settlementId, string resourceId, double delta)
        {
            if (!_settlements.TryGetValue(settlementId, out SettlementRecord settlement) || string.IsNullOrWhiteSpace(resourceId) || !IsFinite(delta)) return false;
            string id = resourceId.Trim(); settlement.Resources.TryGetValue(id, out double current);
            if (current + delta < 0d) return false; settlement.Resources[id] = current + delta; return true;
        }

        public bool AddBuilding(EntityId settlementId, string buildingId) =>
            _settlements.TryGetValue(settlementId, out SettlementRecord settlement) && !string.IsNullOrWhiteSpace(buildingId) && settlement.BuildingIds.Add(buildingId.Trim());

        public bool EnqueueRecruitment(EntityId settlementId, string unitDefinitionId)
        {
            if (!_settlements.TryGetValue(settlementId, out SettlementRecord settlement) || string.IsNullOrWhiteSpace(unitDefinitionId)) return false;
            settlement.RecruitmentQueue.Add(unitDefinitionId.Trim()); return true;
        }

        public bool AdjustPopulation(EntityId settlementId, double delta)
        {
            if (!_settlements.TryGetValue(settlementId, out SettlementRecord settlement) || !IsFinite(delta) || settlement.Population + delta < 0d) return false;
            settlement.Population += delta; return true;
        }

        public bool SetDefense(EntityId settlementId, double defense)
        {
            if (!_settlements.TryGetValue(settlementId, out SettlementRecord settlement) || !IsFinite(defense)) return false;
            settlement.Defense = Math.Max(0d, Math.Min(settlement.Profile.MaxDefense, defense)); return true;
        }

        public bool TryGetState(EntityId settlementId, out SettlementSnapshot snapshot)
        {
            if (!_settlements.TryGetValue(settlementId, out SettlementRecord settlement)) { snapshot = default; return false; }
            snapshot = CreateSnapshot(settlement); return true;
        }

        public IReadOnlyList<SettlementSnapshot> Snapshot()
        {
            var result = new List<SettlementSnapshot>(_settlements.Count);
            foreach (SettlementRecord settlement in _settlements.Values) result.Add(CreateSnapshot(settlement));
            result.Sort((left, right) => left.SettlementId.CompareTo(right.SettlementId)); return result.AsReadOnly();
        }

        public string GetDebugSummary()
        {
            int garrison = 0, recruitment = 0;
            foreach (SettlementRecord settlement in _settlements.Values) { garrison += settlement.GarrisonIds.Count; recruitment += settlement.RecruitmentQueue.Count; }
            return $"Settlements={_settlements.Count}, Garrison={garrison}, RecruitmentQueued={recruitment}";
        }

        private bool TryFaction(EntityId factionId, out FactionSnapshot snapshot) => _factions.TryGetState(factionId, out snapshot);
        private static bool Contains(IReadOnlyList<EntityId> values, EntityId expected)
        { foreach (EntityId value in values) if (value == expected) return true; return false; }
        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

        private static SettlementSnapshot CreateSnapshot(SettlementRecord settlement)
        {
            var garrison = new List<EntityId>(settlement.GarrisonIds); var buildings = new List<string>(settlement.BuildingIds);
            buildings.Sort(StringComparer.Ordinal);
            return new SettlementSnapshot(settlement.SettlementId, settlement.Profile, settlement.OwnerId, settlement.Population,
                settlement.Defense, garrison.AsReadOnly(), new Dictionary<string, double>(settlement.Resources, StringComparer.Ordinal),
                buildings.AsReadOnly(), new List<string>(settlement.RecruitmentQueue).AsReadOnly());
        }

        private sealed class SettlementRecord
        {
            public SettlementRecord(EntityId id, SettlementProfile profile, EntityId owner)
            { SettlementId = id; Profile = profile; OwnerId = owner; Population = profile.InitialPopulation; Defense = profile.MaxDefense; }
            public EntityId SettlementId { get; }
            public SettlementProfile Profile { get; }
            public EntityId OwnerId { get; set; }
            public double Population { get; set; }
            public double Defense { get; set; }
            public List<EntityId> GarrisonIds { get; } = new List<EntityId>();
            public Dictionary<string, double> Resources { get; } = new Dictionary<string, double>(StringComparer.Ordinal);
            public HashSet<string> BuildingIds { get; } = new HashSet<string>(StringComparer.Ordinal);
            public List<string> RecruitmentQueue { get; } = new List<string>();
        }
    }
}
