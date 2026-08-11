using System.Collections.Generic;

namespace AegisRTS.Gameplay.Content.Definitions
{
    /// <summary>Content-authored Utility AI personality and decision cadence.</summary>
    public sealed class AiProfileDefinition : DefinitionBase
    {
        public AiProfileDefinition(DefinitionId id, string displayName, double aggression, double defenseBias,
            double economyBias, double riskTolerance, double siegePreference, double decisionIntervalSeconds,
            int desiredArmySize, IEnumerable<ContentTag> tags)
            : base(id, displayName, tags)
        { Aggression = aggression; DefenseBias = defenseBias; EconomyBias = economyBias; RiskTolerance = riskTolerance;
            SiegePreference = siegePreference; DecisionIntervalSeconds = decisionIntervalSeconds; DesiredArmySize = desiredArmySize; }
        public double Aggression { get; }
        public double DefenseBias { get; }
        public double EconomyBias { get; }
        public double RiskTolerance { get; }
        public double SiegePreference { get; }
        public double DecisionIntervalSeconds { get; }
        public int DesiredArmySize { get; }
    }
}
