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
            bool destructibleWalls,
            string settlementArchetypeId = "constructed-base",
            bool gateRepairEnabled = false,
            bool strongholdRecruitmentEnabled = false,
            bool captureStrongholdInsteadOfDestroy = false)
        {
            MoraleEnabled = moraleEnabled;
            SupplyEnabled = supplyEnabled;
            HeroCaptureEnabled = heroCaptureEnabled;
            HeroPermanentDeath = heroPermanentDeath;
            PopulationEnabled = populationEnabled;
            FogOfWarEnabled = fogOfWarEnabled;
            DestructibleWalls = destructibleWalls;
            SettlementArchetypeId = string.IsNullOrWhiteSpace(settlementArchetypeId)
                ? "constructed-base"
                : settlementArchetypeId.Trim();
            GateRepairEnabled = gateRepairEnabled;
            StrongholdRecruitmentEnabled = strongholdRecruitmentEnabled;
            CaptureStrongholdInsteadOfDestroy = captureStrongholdInsteadOfDestroy;
        }

        public bool MoraleEnabled { get; }

        public bool SupplyEnabled { get; }

        public bool HeroCaptureEnabled { get; }

        public bool HeroPermanentDeath { get; }

        public bool PopulationEnabled { get; }

        public bool FogOfWarEnabled { get; }

        public bool DestructibleWalls { get; }

        /// <summary>World-neutral rules profile such as constructed-base or fortified-city.</summary>
        public string SettlementArchetypeId { get; }

        public bool GateRepairEnabled { get; }

        public bool StrongholdRecruitmentEnabled { get; }

        public bool CaptureStrongholdInsteadOfDestroy { get; }
    }
}
