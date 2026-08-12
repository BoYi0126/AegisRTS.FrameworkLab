using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Content.Definitions;

namespace AegisRTS.Gameplay.AI
{
    public readonly struct AiRuntimeStateSnapshot
    {
        public AiRuntimeStateSnapshot(double decisionRemaining, int decisionCount, int stalledDecisionCount,
            AiStrategicGoal goal, AiDecisionLayer layer, AiActionType action, string lastError)
        {
            DecisionRemaining = decisionRemaining;
            DecisionCount = decisionCount;
            StalledDecisionCount = stalledDecisionCount;
            Goal = goal;
            Layer = layer;
            Action = action;
            LastError = lastError ?? string.Empty;
        }
        public double DecisionRemaining { get; }
        public int DecisionCount { get; }
        public int StalledDecisionCount { get; }
        public AiStrategicGoal Goal { get; }
        public AiDecisionLayer Layer { get; }
        public AiActionType Action { get; }
        public string LastError { get; }
    }

    public enum AiDecisionLayer { Strategic, Operational, Tactical, Unit }
    public enum AiStrategicGoal { Economy, Expand, Attack, Defend, Recover }
    public enum AiActionType
    {
        DevelopEconomy, Recruit, AssembleArmy, Reinforce, MoveToTarget, StartSiege,
        SelectTarget, ProtectSiege, Breach, AdvanceToObjective, Capture, Retreat, HoldPosition, Recover, Wait,
    }

    public sealed class AiProfile
    {
        public AiProfile(string id, double aggression, double defenseBias, double economyBias,
            double riskTolerance, double siegePreference, double decisionIntervalSeconds = 0.5d,
            int desiredArmySize = 3, int maximumStalledDecisions = 5)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("AI profile ID is required.", nameof(id));
            ValidateUnit(aggression, nameof(aggression)); ValidateUnit(defenseBias, nameof(defenseBias));
            ValidateUnit(economyBias, nameof(economyBias)); ValidateUnit(riskTolerance, nameof(riskTolerance));
            ValidateUnit(siegePreference, nameof(siegePreference));
            if (!IsFinite(decisionIntervalSeconds) || decisionIntervalSeconds <= 0d || desiredArmySize <= 0 || maximumStalledDecisions <= 0)
                throw new ArgumentOutOfRangeException(nameof(decisionIntervalSeconds));
            Id = id.Trim(); Aggression = aggression; DefenseBias = defenseBias; EconomyBias = economyBias;
            RiskTolerance = riskTolerance; SiegePreference = siegePreference; DecisionIntervalSeconds = decisionIntervalSeconds;
            DesiredArmySize = desiredArmySize; MaximumStalledDecisions = maximumStalledDecisions;
        }
        public string Id { get; }
        public double Aggression { get; }
        public double DefenseBias { get; }
        public double EconomyBias { get; }
        public double RiskTolerance { get; }
        public double SiegePreference { get; }
        public double DecisionIntervalSeconds { get; }
        public int DesiredArmySize { get; }
        public int MaximumStalledDecisions { get; }
        public static AiProfile FromDefinition(AiProfileDefinition definition, int maximumStalledDecisions = 5)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            return new AiProfile(definition.Id.Value, definition.Aggression, definition.DefenseBias,
                definition.EconomyBias, definition.RiskTolerance, definition.SiegePreference,
                definition.DecisionIntervalSeconds, definition.DesiredArmySize, maximumStalledDecisions);
        }
        private static void ValidateUnit(double value, string name)
        { if (!IsFinite(value) || value < 0d || value > 1d) throw new ArgumentOutOfRangeException(name); }
        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }

    public readonly struct AiWorldSnapshot
    {
        public AiWorldSnapshot(EntityId factionId, double resourceStockpile, double incomePerSecond,
            int unitCount, int armyCount, int settlementCount, int enemySettlementCount,
            double strength, double threat, EntityId targetSettlementId, IReadOnlyList<EntityId> route,
            bool economyReady, bool recruitmentQueued, bool armyReady, bool armyDeployed,
            bool siegeActive, bool breachOpen, bool captureAvailable, bool targetCaptured)
        {
            FactionId = factionId; ResourceStockpile = resourceStockpile; IncomePerSecond = incomePerSecond;
            UnitCount = unitCount; ArmyCount = armyCount; SettlementCount = settlementCount;
            EnemySettlementCount = enemySettlementCount; Strength = strength; Threat = threat;
            TargetSettlementId = targetSettlementId; Route = route ?? Array.Empty<EntityId>();
            EconomyReady = economyReady; RecruitmentQueued = recruitmentQueued; ArmyReady = armyReady;
            ArmyDeployed = armyDeployed; SiegeActive = siegeActive; BreachOpen = breachOpen;
            CaptureAvailable = captureAvailable; TargetCaptured = targetCaptured;
        }
        public EntityId FactionId { get; }
        public double ResourceStockpile { get; }
        public double IncomePerSecond { get; }
        public int UnitCount { get; }
        public int ArmyCount { get; }
        public int SettlementCount { get; }
        public int EnemySettlementCount { get; }
        public double Strength { get; }
        public double Threat { get; }
        public EntityId TargetSettlementId { get; }
        public IReadOnlyList<EntityId> Route { get; }
        public bool EconomyReady { get; }
        public bool RecruitmentQueued { get; }
        public bool ArmyReady { get; }
        public bool ArmyDeployed { get; }
        public bool SiegeActive { get; }
        public bool BreachOpen { get; }
        public bool CaptureAvailable { get; }
        public bool TargetCaptured { get; }
    }

    public readonly struct AiActionScore
    {
        public AiActionScore(AiActionType action, AiDecisionLayer layer, AiStrategicGoal goal, double score)
        { Action = action; Layer = layer; Goal = goal; Score = score; }
        public AiActionType Action { get; }
        public AiDecisionLayer Layer { get; }
        public AiStrategicGoal Goal { get; }
        public double Score { get; }
    }

    public readonly struct AiActionResult
    {
        private AiActionResult(bool accepted, bool madeProgress, string error)
        { Accepted = accepted; MadeProgress = madeProgress; Error = error ?? string.Empty; }
        public bool Accepted { get; }
        public bool MadeProgress { get; }
        public string Error { get; }
        public static AiActionResult Progress() => new AiActionResult(true, true, string.Empty);
        public static AiActionResult Waiting() => new AiActionResult(true, false, string.Empty);
        public static AiActionResult Rejected(string error) => new AiActionResult(false, false, error);
    }

    public interface IAiWorldQuery { AiWorldSnapshot Observe(EntityId factionId); }
    public interface IAiActionExecutor
    {
        AiActionResult Execute(EntityId factionId, AiActionType action, AiWorldSnapshot world);
    }

    public readonly struct AiAgentSnapshot
    {
        public AiAgentSnapshot(EntityId factionId, AiProfile profile, AiStrategicGoal goal, AiDecisionLayer layer,
            AiActionType action, IReadOnlyList<AiActionScore> scores, EntityId targetId,
            double strength, double threat, IReadOnlyList<EntityId> route, int decisionCount,
            int stalledDecisionCount, string lastError)
        { FactionId = factionId; Profile = profile; Goal = goal; Layer = layer; Action = action; Scores = scores;
            TargetId = targetId; Strength = strength; Threat = threat; Route = route; DecisionCount = decisionCount;
            StalledDecisionCount = stalledDecisionCount; LastError = lastError ?? string.Empty; }
        public EntityId FactionId { get; }
        public AiProfile Profile { get; }
        public AiStrategicGoal Goal { get; }
        public AiDecisionLayer Layer { get; }
        public AiActionType Action { get; }
        public IReadOnlyList<AiActionScore> Scores { get; }
        public EntityId TargetId { get; }
        public double Strength { get; }
        public double Threat { get; }
        public IReadOnlyList<EntityId> Route { get; }
        public int DecisionCount { get; }
        public int StalledDecisionCount { get; }
        public string LastError { get; }
    }

    public sealed class AiDecisionMadeEvent : IEvent
    {
        public AiDecisionMadeEvent(EntityId factionId, AiStrategicGoal goal, AiDecisionLayer layer,
            AiActionType action, double score, bool madeProgress)
        { FactionId = factionId; Goal = goal; Layer = layer; Action = action; Score = score; MadeProgress = madeProgress; }
        public EntityId FactionId { get; }
        public AiStrategicGoal Goal { get; }
        public AiDecisionLayer Layer { get; }
        public AiActionType Action { get; }
        public double Score { get; }
        public bool MadeProgress { get; }
    }
}
