using System;
using System.Collections.Generic;
using System.Text.Json;
using AegisRTS.Gameplay.Content.Definitions;

namespace AegisRTS.Gameplay.Content.Serialization
{
    /// <summary>Converts a content-authored JSON document into immutable definition objects.</summary>
    public sealed class ContentPackJsonLoader
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        /// <summary>Deserializes a JSON content pack without activating it.</summary>
        public ContentPack Load(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("Content pack JSON is required.", nameof(json));
            }

            try
            {
                ContentPackDocument document = JsonSerializer.Deserialize<ContentPackDocument>(json, Options);
                if (document == null)
                {
                    throw new ContentPackFormatException("Content pack JSON produced no document.");
                }

                return Convert(document);
            }
            catch (ContentPackFormatException)
            {
                throw;
            }
            catch (Exception exception) when (
                exception is JsonException ||
                exception is ArgumentException ||
                exception is InvalidOperationException)
            {
                throw new ContentPackFormatException("Content pack JSON is invalid.", exception);
            }
        }

        private static ContentPack Convert(ContentPackDocument document)
        {
            var resources = new List<ResourceDefinition>();
            foreach (ResourceDocument item in document.Resources ?? Array.Empty<ResourceDocument>())
            {
                resources.Add(new ResourceDefinition(Id(item.Id), item.DisplayName, Tags(item.Tags)));
            }

            var abilities = new List<AbilityDefinition>();
            foreach (AbilityDocument item in document.Abilities ?? Array.Empty<AbilityDocument>())
            {
                abilities.Add(new AbilityDefinition(
                    Id(item.Id), item.DisplayName, item.CooldownSeconds, item.Range, Tags(item.Tags)));
            }

            var units = new List<UnitDefinition>();
            foreach (ActorDocument item in document.Units ?? Array.Empty<ActorDocument>())
            {
                units.Add(new UnitDefinition(
                    Id(item.Id), item.DisplayName, item.MaxHealth, item.MovementSpeed, item.PrefabId,
                    Costs(item.Costs), Ids(item.AbilityIds), Tags(item.Tags), item.RecruitmentSeconds,
                    item.PopulationCost, Ids(item.PrerequisiteBuildingIds), Ids(item.PrerequisiteTechnologyIds)));
            }

            var heroes = new List<HeroDefinition>();
            foreach (ActorDocument item in document.Heroes ?? Array.Empty<ActorDocument>())
            {
                heroes.Add(new HeroDefinition(
                    Id(item.Id), item.DisplayName, item.MaxHealth, item.MovementSpeed, item.PrefabId,
                    Costs(item.Costs), Ids(item.AbilityIds), Tags(item.Tags), item.Leadership));
            }

            var buildings = new List<BuildingDefinition>();
            foreach (BuildingDocument item in document.Buildings ?? Array.Empty<BuildingDocument>())
            {
                buildings.Add(new BuildingDefinition(
                    Id(item.Id), item.DisplayName, item.MaxHealth, item.PrefabId,
                    Costs(item.Costs), Tags(item.Tags), item.BuildSeconds, Ids(item.PrerequisiteBuildingIds),
                    Ids(item.PrerequisiteTechnologyIds), Production(item.Production), item.PopulationCapacity));
            }

            var technologies = new List<TechnologyDefinition>();
            foreach (TechnologyDocument item in document.Technologies ?? Array.Empty<TechnologyDocument>())
            {
                technologies.Add(new TechnologyDefinition(
                    Id(item.Id), item.DisplayName, Costs(item.Costs), Ids(item.PrerequisiteIds), Tags(item.Tags),
                    item.ResearchSeconds, Modifiers(item.Modifiers)));
            }

            var settlements = new List<SettlementDefinition>();
            foreach (SettlementDocument item in document.Settlements ?? Array.Empty<SettlementDocument>())
            {
                settlements.Add(new SettlementDefinition(
                    Id(item.Id), item.DisplayName, item.MaxHealth, item.PrefabId,
                    Ids(item.StartingResourceIds), Tags(item.Tags), item.InitialPopulation,
                    item.MaxDefense, item.CaptureRule, item.CaptureConditions));
            }

            var defenseStructures = new List<DefenseStructureDefinition>();
            foreach (DefenseStructureDocument item in document.DefenseStructures ?? Array.Empty<DefenseStructureDocument>())
            {
                defenseStructures.Add(new DefenseStructureDefinition(
                    Id(item.Id), item.DisplayName, item.StructureTypeId, item.SiegeAreaId,
                    item.MaxHealth, item.Armor, item.PrefabId, Tags(item.Tags)));
            }

            var aiProfiles = new List<AiProfileDefinition>();
            foreach (AiProfileDocument item in document.AiProfiles ?? Array.Empty<AiProfileDocument>())
            {
                aiProfiles.Add(new AiProfileDefinition(Id(item.Id), item.DisplayName, item.Aggression,
                    item.DefenseBias, item.EconomyBias, item.RiskTolerance, item.SiegePreference,
                    item.DecisionIntervalSeconds, item.DesiredArmySize, Tags(item.Tags)));
            }

            GameRuleSet rules = document.Rules == null
                ? null
                : new GameRuleSet(
                    document.Rules.MoraleEnabled,
                    document.Rules.SupplyEnabled,
                    document.Rules.HeroCaptureEnabled,
                    document.Rules.HeroPermanentDeath,
                    document.Rules.PopulationEnabled,
                    document.Rules.FogOfWarEnabled,
                    document.Rules.DestructibleWalls);

            return new ContentPack(
                Id(document.Id),
                document.DisplayName,
                Tags(document.DeclaredTags),
                resources,
                units,
                heroes,
                abilities,
                buildings,
                technologies,
                settlements,
                defenseStructures,
                aiProfiles,
                rules);
        }

        private static DefinitionId Id(string value) => new DefinitionId(value);

        private static IEnumerable<DefinitionId> Ids(IEnumerable<string> values)
        {
            foreach (string value in values ?? Array.Empty<string>())
            {
                yield return Id(value);
            }
        }

        private static IEnumerable<ContentTag> Tags(IEnumerable<string> values)
        {
            foreach (string value in values ?? Array.Empty<string>())
            {
                yield return new ContentTag(value);
            }
        }

        private static IEnumerable<ResourceCost> Costs(IEnumerable<ResourceCostDocument> values)
        {
            foreach (ResourceCostDocument value in values ?? Array.Empty<ResourceCostDocument>())
            {
                yield return new ResourceCost(Id(value.ResourceId), value.Amount);
            }
        }

        private static IEnumerable<ResourceProduction> Production(IEnumerable<ResourceProductionDocument> values)
        {
            foreach (ResourceProductionDocument value in values ?? Array.Empty<ResourceProductionDocument>())
                yield return new ResourceProduction(Id(value.ResourceId), value.AmountPerSecond);
        }

        private static IEnumerable<TechnologyModifier> Modifiers(IEnumerable<TechnologyModifierDocument> values)
        {
            foreach (TechnologyModifierDocument value in values ?? Array.Empty<TechnologyModifierDocument>())
                yield return new TechnologyModifier(value.StatId, value.Additive, value.Multiplier);
        }
    }

    internal sealed class ContentPackDocument
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string[] DeclaredTags { get; set; }
        public ResourceDocument[] Resources { get; set; }
        public ActorDocument[] Units { get; set; }
        public ActorDocument[] Heroes { get; set; }
        public AbilityDocument[] Abilities { get; set; }
        public BuildingDocument[] Buildings { get; set; }
        public TechnologyDocument[] Technologies { get; set; }
        public SettlementDocument[] Settlements { get; set; }
        public DefenseStructureDocument[] DefenseStructures { get; set; }
        public AiProfileDocument[] AiProfiles { get; set; }
        public GameRuleSetDocument Rules { get; set; }
    }

    internal class DefinitionDocument
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string[] Tags { get; set; }
    }

    internal sealed class ResourceDocument : DefinitionDocument
    {
    }

    internal sealed class ActorDocument : DefinitionDocument
    {
        public double MaxHealth { get; set; }
        public double MovementSpeed { get; set; }
        public string PrefabId { get; set; }
        public ResourceCostDocument[] Costs { get; set; }
        public string[] AbilityIds { get; set; }
        public double Leadership { get; set; }
        public double RecruitmentSeconds { get; set; }
        public double PopulationCost { get; set; }
        public string[] PrerequisiteBuildingIds { get; set; }
        public string[] PrerequisiteTechnologyIds { get; set; }
    }

    internal sealed class AbilityDocument : DefinitionDocument
    {
        public double CooldownSeconds { get; set; }
        public double Range { get; set; }
    }

    internal sealed class BuildingDocument : DefinitionDocument
    {
        public double MaxHealth { get; set; }
        public string PrefabId { get; set; }
        public ResourceCostDocument[] Costs { get; set; }
        public double BuildSeconds { get; set; }
        public string[] PrerequisiteBuildingIds { get; set; }
        public string[] PrerequisiteTechnologyIds { get; set; }
        public ResourceProductionDocument[] Production { get; set; }
        public double PopulationCapacity { get; set; }
    }

    internal sealed class TechnologyDocument : DefinitionDocument
    {
        public ResourceCostDocument[] Costs { get; set; }
        public string[] PrerequisiteIds { get; set; }
        public double ResearchSeconds { get; set; }
        public TechnologyModifierDocument[] Modifiers { get; set; }
    }

    internal sealed class SettlementDocument : DefinitionDocument
    {
        public double MaxHealth { get; set; }
        public string PrefabId { get; set; }
        public string[] StartingResourceIds { get; set; }
        public double InitialPopulation { get; set; }
        public double MaxDefense { get; set; }
        public string CaptureRule { get; set; }
        public string[] CaptureConditions { get; set; }
    }

    internal sealed class DefenseStructureDocument : DefinitionDocument
    {
        public string StructureTypeId { get; set; }
        public string SiegeAreaId { get; set; }
        public double MaxHealth { get; set; }
        public double Armor { get; set; }
        public string PrefabId { get; set; }
    }

    internal sealed class AiProfileDocument : DefinitionDocument
    {
        public double Aggression { get; set; }
        public double DefenseBias { get; set; }
        public double EconomyBias { get; set; }
        public double RiskTolerance { get; set; }
        public double SiegePreference { get; set; }
        public double DecisionIntervalSeconds { get; set; }
        public int DesiredArmySize { get; set; }
    }

    internal sealed class ResourceCostDocument
    {
        public string ResourceId { get; set; }
        public double Amount { get; set; }
    }

    internal sealed class ResourceProductionDocument
    {
        public string ResourceId { get; set; }
        public double AmountPerSecond { get; set; }
    }

    internal sealed class TechnologyModifierDocument
    {
        public string StatId { get; set; }
        public double Additive { get; set; }
        public double Multiplier { get; set; } = 1d;
    }

    internal sealed class GameRuleSetDocument
    {
        public bool MoraleEnabled { get; set; }
        public bool SupplyEnabled { get; set; }
        public bool HeroCaptureEnabled { get; set; }
        public bool HeroPermanentDeath { get; set; }
        public bool PopulationEnabled { get; set; }
        public bool FogOfWarEnabled { get; set; }
        public bool DestructibleWalls { get; set; }
    }
}
