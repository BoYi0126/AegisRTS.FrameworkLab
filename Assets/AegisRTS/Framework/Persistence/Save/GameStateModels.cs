using System;

namespace AegisRTS.Persistence.Save
{
    public sealed class NamedValueState { public string Id { get; set; } = ""; public double Value { get; set; } }
    public sealed class VectorState { public double X { get; set; } public double Y { get; set; } public double Z { get; set; } }

    public sealed class FactionSaveState
    {
        public ulong Id { get; set; } public string DefinitionId { get; set; } = ""; public string AiProfileId { get; set; } = "";
        public NamedValueState[] Resources { get; set; } = Array.Empty<NamedValueState>();
        public string[] TechnologyIds { get; set; } = Array.Empty<string>();
        public ulong[] SettlementIds { get; set; } = Array.Empty<ulong>(); public ulong[] TerritoryIds { get; set; } = Array.Empty<ulong>(); public ulong[] ArmyIds { get; set; } = Array.Empty<ulong>();
    }
    public sealed class SettlementSaveState
    {
        public ulong Id { get; set; } public string DefinitionId { get; set; } = ""; public ulong OwnerId { get; set; }
        public double Population { get; set; } public double Defense { get; set; }
        public ulong[] GarrisonIds { get; set; } = Array.Empty<ulong>(); public NamedValueState[] Resources { get; set; } = Array.Empty<NamedValueState>();
        public string[] BuildingIds { get; set; } = Array.Empty<string>(); public string[] RecruitmentQueue { get; set; } = Array.Empty<string>();
    }
    public sealed class UnitSaveState
    {
        public ulong Id { get; set; } public string DefinitionId { get; set; } = ""; public ulong FactionId { get; set; }
        public double MaxHealth { get; set; } public double Health { get; set; } public string CombatState { get; set; } = "";
        public VectorState Position { get; set; } = new VectorState(); public ulong ArmyId { get; set; }
    }
    public sealed class HeroSaveState
    {
        public ulong UnitId { get; set; } public double Leadership { get; set; } public ulong ArmyId { get; set; }
        public string[] AbilityIds { get; set; } = Array.Empty<string>();
    }
    public sealed class ArmySaveState
    {
        public ulong Id { get; set; } public ulong FactionId { get; set; } public ulong CommanderId { get; set; }
        public ulong[] UnitIds { get; set; } = Array.Empty<ulong>(); public double Morale { get; set; } public double Supply { get; set; }
        public string OrderType { get; set; } = ""; public VectorState Destination { get; set; } = new VectorState(); public ulong TargetId { get; set; }
    }
    public sealed class ResourceAccountSaveState
    {
        public ulong AccountId { get; set; } public NamedValueState[] Balances { get; set; } = Array.Empty<NamedValueState>();
        public double PopulationUsed { get; set; } public double PopulationCapacity { get; set; }
    }
    public sealed class BuildingSaveState
    {
        public ulong SettlementId { get; set; } public string DefinitionId { get; set; } = "";
        public double RemainingSeconds { get; set; } public bool Completed { get; set; }
    }
    public sealed class TechnologySaveState
    {
        public ulong FactionId { get; set; } public string DefinitionId { get; set; } = "";
        public double RemainingSeconds { get; set; } public bool Completed { get; set; }
    }
    public sealed class ObjectiveSaveState
    {
        public string Id { get; set; } = ""; public string Status { get; set; } = "";
        public double Value { get; set; } public double HeldSeconds { get; set; }
    }
    public sealed class ClockSaveState
    {
        public double TotalSeconds { get; set; } public double TotalUnscaledSeconds { get; set; }
        public double DeltaSeconds { get; set; } public double UnscaledDeltaSeconds { get; set; }
        public ulong TickCount { get; set; } public bool Paused { get; set; } public double Speed { get; set; } = 1d;
    }
    public sealed class RandomSaveState { public int Seed { get; set; } public ulong DrawCount { get; set; } public ulong InternalState { get; set; } }
    public sealed class ExtensionSaveState { public string Id { get; set; } = ""; public string Json { get; set; } = ""; }

    public sealed class GameStateDocument
    {
        public FactionSaveState[] Factions { get; set; } = Array.Empty<FactionSaveState>();
        public SettlementSaveState[] Settlements { get; set; } = Array.Empty<SettlementSaveState>();
        public UnitSaveState[] Units { get; set; } = Array.Empty<UnitSaveState>();
        public HeroSaveState[] Heroes { get; set; } = Array.Empty<HeroSaveState>();
        public ArmySaveState[] Armies { get; set; } = Array.Empty<ArmySaveState>();
        public ResourceAccountSaveState[] ResourceAccounts { get; set; } = Array.Empty<ResourceAccountSaveState>();
        public BuildingSaveState[] Buildings { get; set; } = Array.Empty<BuildingSaveState>();
        public TechnologySaveState[] Technologies { get; set; } = Array.Empty<TechnologySaveState>();
        public ObjectiveSaveState[] Objectives { get; set; } = Array.Empty<ObjectiveSaveState>();
        public ClockSaveState Clock { get; set; } = new ClockSaveState();
        public RandomSaveState Random { get; set; } = new RandomSaveState();
        public ExtensionSaveState[] Extensions { get; set; } = Array.Empty<ExtensionSaveState>();
    }

    public sealed class SaveMetadata
    {
        public string SaveVersion { get; set; } = ""; public string FrameworkVersion { get; set; } = "";
        public string ContentVersion { get; set; } = ""; public string ScenarioId { get; set; } = "";
        public DateTimeOffset Timestamp { get; set; }
    }
    public sealed class SaveEnvelope
    {
        public SaveMetadata Metadata { get; set; } = new SaveMetadata();
        public GameStateDocument State { get; set; } = new GameStateDocument();
        public string Checksum { get; set; } = "";
    }
}
