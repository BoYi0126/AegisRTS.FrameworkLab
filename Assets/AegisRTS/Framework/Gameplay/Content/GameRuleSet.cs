namespace AegisRTS.Gameplay.Content
{
    /// <summary>Switches optional framework rules without hard-coding a world or campaign.</summary>
    public sealed class GameRuleSet
    {
        public GameRuleSet(
            bool moraleEnabled,
            bool supplyEnabled,
            bool heroCaptureEnabled,
            bool heroPermanentDeath,
            bool populationEnabled,
            bool fogOfWarEnabled,
            bool destructibleWalls)
        {
            MoraleEnabled = moraleEnabled;
            SupplyEnabled = supplyEnabled;
            HeroCaptureEnabled = heroCaptureEnabled;
            HeroPermanentDeath = heroPermanentDeath;
            PopulationEnabled = populationEnabled;
            FogOfWarEnabled = fogOfWarEnabled;
            DestructibleWalls = destructibleWalls;
        }

        public bool MoraleEnabled { get; }

        public bool SupplyEnabled { get; }

        public bool HeroCaptureEnabled { get; }

        public bool HeroPermanentDeath { get; }

        public bool PopulationEnabled { get; }

        public bool FogOfWarEnabled { get; }

        public bool DestructibleWalls { get; }
    }
}
