using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Content.Definitions;

namespace AegisRTS.Gameplay.Settlements
{
    public enum CaptureRuleType { ClearDefenders, CaptureZone, DestroyCore, KillCommander, Mixed }

    [Flags]
    public enum CaptureCondition
    {
        None = 0,
        DefendersCleared = 1 << 0,
        ZoneControlled = 1 << 1,
        CoreDestroyed = 1 << 2,
        CommanderKilled = 1 << 3,
    }

    public sealed class CaptureRule
    {
        public CaptureRule(CaptureRuleType type, CaptureCondition mixedRequirements = CaptureCondition.None)
        {
            Type = type;
            RequiredConditions = type == CaptureRuleType.ClearDefenders ? CaptureCondition.DefendersCleared
                : type == CaptureRuleType.CaptureZone ? CaptureCondition.ZoneControlled
                : type == CaptureRuleType.DestroyCore ? CaptureCondition.CoreDestroyed
                : type == CaptureRuleType.KillCommander ? CaptureCondition.CommanderKilled
                : mixedRequirements;
            if (RequiredConditions == CaptureCondition.None) throw new ArgumentException("Capture rule requires at least one condition.", nameof(mixedRequirements));
        }
        public CaptureRuleType Type { get; }
        public CaptureCondition RequiredConditions { get; }
        public bool IsSatisfied(CaptureCondition completed) => (completed & RequiredConditions) == RequiredConditions;
    }

    public sealed class SettlementProfile
    {
        public SettlementProfile(string definitionId, double initialPopulation, double maxDefense, CaptureRule captureRule)
        {
            if (string.IsNullOrWhiteSpace(definitionId)) throw new ArgumentException("Definition ID is required.", nameof(definitionId));
            if (!IsFiniteNonNegative(initialPopulation) || maxDefense <= 0d || double.IsNaN(maxDefense) || double.IsInfinity(maxDefense))
                throw new ArgumentOutOfRangeException(nameof(initialPopulation));
            DefinitionId = definitionId.Trim(); InitialPopulation = initialPopulation; MaxDefense = maxDefense;
            CaptureRule = captureRule ?? throw new ArgumentNullException(nameof(captureRule));
        }
        public string DefinitionId { get; }
        public double InitialPopulation { get; }
        public double MaxDefense { get; }
        public CaptureRule CaptureRule { get; }
        private static bool IsFiniteNonNegative(double value) => value >= 0d && !double.IsNaN(value) && !double.IsInfinity(value);

        public static SettlementProfile FromDefinition(SettlementDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            CaptureRuleType type = definition.CaptureRule == "capture-zone" ? CaptureRuleType.CaptureZone
                : definition.CaptureRule == "destroy-core" ? CaptureRuleType.DestroyCore
                : definition.CaptureRule == "kill-commander" ? CaptureRuleType.KillCommander
                : definition.CaptureRule == "mixed" ? CaptureRuleType.Mixed
                : CaptureRuleType.ClearDefenders;
            CaptureCondition conditions = CaptureCondition.None;
            foreach (string condition in definition.CaptureConditions)
            {
                if (condition == "defenders-cleared") conditions |= CaptureCondition.DefendersCleared;
                else if (condition == "zone-controlled") conditions |= CaptureCondition.ZoneControlled;
                else if (condition == "core-destroyed") conditions |= CaptureCondition.CoreDestroyed;
                else if (condition == "commander-killed") conditions |= CaptureCondition.CommanderKilled;
            }
            return new SettlementProfile(definition.Id.ToString(), definition.InitialPopulation, definition.MaxDefense,
                new CaptureRule(type, conditions));
        }
    }

    public readonly struct SettlementSnapshot
    {
        public SettlementSnapshot(EntityId settlementId, SettlementProfile profile, EntityId ownerId, double population,
            double defense, IReadOnlyList<EntityId> garrisonIds, IReadOnlyDictionary<string, double> resources,
            IReadOnlyList<string> buildingIds, IReadOnlyList<string> recruitmentQueue)
        {
            SettlementId = settlementId; Profile = profile; OwnerId = ownerId; Population = population; Defense = defense;
            GarrisonIds = garrisonIds; Resources = resources; BuildingIds = buildingIds; RecruitmentQueue = recruitmentQueue;
        }
        public EntityId SettlementId { get; }
        public SettlementProfile Profile { get; }
        public EntityId OwnerId { get; }
        public double Population { get; }
        public double Defense { get; }
        public IReadOnlyList<EntityId> GarrisonIds { get; }
        public IReadOnlyDictionary<string, double> Resources { get; }
        public IReadOnlyList<string> BuildingIds { get; }
        public IReadOnlyList<string> RecruitmentQueue { get; }
    }

    public readonly struct SettlementCommandResult
    {
        private SettlementCommandResult(bool succeeded, string error, EntityId settlementId)
        { Succeeded = succeeded; Error = error ?? string.Empty; SettlementId = settlementId; }
        public bool Succeeded { get; }
        public string Error { get; }
        public EntityId SettlementId { get; }
        public static SettlementCommandResult Success(EntityId id) => new SettlementCommandResult(true, string.Empty, id);
        public static SettlementCommandResult Failure(string error) => new SettlementCommandResult(false,
            string.IsNullOrWhiteSpace(error) ? "Settlement command failed." : error, default);
    }

    public interface ISettlementQuery
    {
        bool TryGetState(EntityId settlementId, out SettlementSnapshot snapshot);
        IReadOnlyList<SettlementSnapshot> Snapshot();
        string GetDebugSummary();
    }

    public sealed class SettlementOwnerChangedEvent : IEvent
    {
        public SettlementOwnerChangedEvent(EntityId settlementId, EntityId previousOwnerId, EntityId newOwnerId, EntityId capturingArmyId)
        { SettlementId = settlementId; PreviousOwnerId = previousOwnerId; NewOwnerId = newOwnerId; CapturingArmyId = capturingArmyId; }
        public EntityId SettlementId { get; }
        public EntityId PreviousOwnerId { get; }
        public EntityId NewOwnerId { get; }
        public EntityId CapturingArmyId { get; }
    }
}
