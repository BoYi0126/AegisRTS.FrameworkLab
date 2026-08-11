using System;
using System.Collections.Generic;

namespace AegisRTS.Gameplay.AI
{
    /// <summary>Scores staged RTS actions while retaining profile-driven strategic tradeoffs.</summary>
    public sealed class UtilityAiPlanner
    {
        public IReadOnlyList<AiActionScore> Score(AiProfile profile, AiWorldSnapshot world, int stalledDecisions)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            var values = new List<AiActionScore>
            {
                Entry(AiActionType.DevelopEconomy, AiDecisionLayer.Strategic, AiStrategicGoal.Economy,
                    !world.EconomyReady ? 90d + profile.EconomyBias * 10d : 0d),
                Entry(AiActionType.Recruit, AiDecisionLayer.Operational, AiStrategicGoal.Economy,
                    world.EconomyReady && world.UnitCount < profile.DesiredArmySize && !world.RecruitmentQueued ? 85d + profile.EconomyBias * 10d : 0d),
                Entry(AiActionType.Wait, AiDecisionLayer.Unit, AiStrategicGoal.Recover,
                    world.RecruitmentQueued ? 80d : 0d),
                Entry(AiActionType.AssembleArmy, AiDecisionLayer.Operational, AiStrategicGoal.Attack,
                    world.UnitCount >= profile.DesiredArmySize && !world.ArmyReady ? 88d + profile.Aggression * 10d : 0d),
                Entry(AiActionType.MoveToTarget, AiDecisionLayer.Operational, AiStrategicGoal.Expand,
                    world.ArmyReady && !world.ArmyDeployed && world.TargetSettlementId.IsValid ? 82d + profile.RiskTolerance * 12d : 0d),
                Entry(AiActionType.StartSiege, AiDecisionLayer.Operational, AiStrategicGoal.Attack,
                    world.ArmyDeployed && !world.SiegeActive && !world.TargetCaptured ? 85d + profile.SiegePreference * 12d : 0d),
                Entry(AiActionType.Breach, AiDecisionLayer.Tactical, AiStrategicGoal.Attack,
                    world.SiegeActive && !world.BreachOpen ? 88d + profile.SiegePreference * 10d : 0d),
                Entry(AiActionType.ProtectSiege, AiDecisionLayer.Tactical, AiStrategicGoal.Defend,
                    world.SiegeActive && world.Threat > world.Strength ? 84d + profile.DefenseBias * 12d : 0d),
                Entry(AiActionType.AdvanceToObjective, AiDecisionLayer.Tactical, AiStrategicGoal.Attack,
                    world.BreachOpen && !world.CaptureAvailable && !world.TargetCaptured ? 92d + profile.Aggression * 6d : 0d),
                Entry(AiActionType.Capture, AiDecisionLayer.Operational, AiStrategicGoal.Expand,
                    world.CaptureAvailable && !world.TargetCaptured ? 100d : 0d),
                Entry(AiActionType.Retreat, AiDecisionLayer.Tactical, AiStrategicGoal.Recover,
                    world.ArmyReady && world.Threat > world.Strength * (1d + profile.RiskTolerance) ? 86d + (1d - profile.RiskTolerance) * 10d : 0d),
                Entry(AiActionType.HoldPosition, AiDecisionLayer.Unit, AiStrategicGoal.Defend,
                    world.TargetCaptured ? 75d + profile.DefenseBias * 10d : 0d),
                Entry(AiActionType.Recover, AiDecisionLayer.Strategic, AiStrategicGoal.Recover,
                    stalledDecisions >= profile.MaximumStalledDecisions ? 110d : 0d),
            };
            values.Sort((left, right) =>
            { int score = right.Score.CompareTo(left.Score); return score != 0 ? score : left.Action.CompareTo(right.Action); });
            return values.AsReadOnly();
        }

        private static AiActionScore Entry(AiActionType action, AiDecisionLayer layer, AiStrategicGoal goal, double score) =>
            new AiActionScore(action, layer, goal, Math.Max(0d, Math.Min(120d, score)));
    }
}
