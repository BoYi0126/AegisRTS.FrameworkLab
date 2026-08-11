using System;
using System.Collections.Generic;
using System.IO;
using AegisRTS.Gameplay.Content;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Content.Serialization;

namespace AegisRTS.Tests.EditMode
{
    internal static class ContentPackTestFactory
    {
        public static readonly ContentAssetCatalog Assets = new ContentAssetCatalog(new[]
        {
            "PF_Unit_Placeholder",
            "PF_Hero_Placeholder",
            "PF_Structure_Placeholder",
            "PF_Settlement_Placeholder",
        });

        public static PackParts CreateValidParts()
        {
            var resourceId = new DefinitionId("test.resource");
            var abilityId = new DefinitionId("test.ability");
            var technologyId = new DefinitionId("test.technology");
            return new PackParts
            {
                Resources =
                {
                    new ResourceDefinition(resourceId, "Resource", Tags("resource")),
                },
                Abilities =
                {
                    new AbilityDefinition(abilityId, "Ability", 1d, 5d, Tags("ability")),
                },
                Units =
                {
                    new UnitDefinition(
                        new DefinitionId("test.unit"), "Unit", 100d, 4d, "PF_Unit_Placeholder",
                        new[] { new ResourceCost(resourceId, 10d) },
                        new[] { abilityId },
                        Tags("unit")),
                },
                Heroes =
                {
                    new HeroDefinition(
                        new DefinitionId("test.hero"), "Hero", 200d, 4d, "PF_Hero_Placeholder",
                        new[] { new ResourceCost(resourceId, 20d) },
                        new[] { abilityId },
                        Tags("hero")),
                },
                Buildings =
                {
                    new BuildingDefinition(
                        new DefinitionId("test.building"), "Building", 500d, "PF_Structure_Placeholder",
                        new[] { new ResourceCost(resourceId, 30d) }, Tags("structure")),
                },
                Technologies =
                {
                    new TechnologyDefinition(
                        technologyId, "Technology", new[] { new ResourceCost(resourceId, 15d) },
                        Array.Empty<DefinitionId>(), Tags("technology")),
                },
                Settlements =
                {
                    new SettlementDefinition(
                        new DefinitionId("test.settlement"), "Settlement", 1000d, "PF_Settlement_Placeholder",
                        new[] { resourceId }, Tags("settlement")),
                },
            };
        }

        public static ContentPack LoadDemoPack(string folder)
        {
            string root = FindProjectRoot();
            string path = Path.Combine(root, "Assets", "AegisRTS", "Content", folder, "ContentPack.json");
            return new ContentPackJsonLoader().Load(File.ReadAllText(path));
        }

        public static ContentTag[] Tags(params string[] values)
        {
            var tags = new ContentTag[values.Length];
            for (int index = 0; index < values.Length; index++)
            {
                tags[index] = new ContentTag(values[index]);
            }

            return tags;
        }

        private static string FindProjectRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "Assets", "AegisRTS")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate the AegisRTS Unity project root.");
        }
    }

    internal sealed class PackParts
    {
        public List<ResourceDefinition> Resources { get; } = new List<ResourceDefinition>();
        public List<UnitDefinition> Units { get; } = new List<UnitDefinition>();
        public List<HeroDefinition> Heroes { get; } = new List<HeroDefinition>();
        public List<AbilityDefinition> Abilities { get; } = new List<AbilityDefinition>();
        public List<BuildingDefinition> Buildings { get; } = new List<BuildingDefinition>();
        public List<TechnologyDefinition> Technologies { get; } = new List<TechnologyDefinition>();
        public List<SettlementDefinition> Settlements { get; } = new List<SettlementDefinition>();

        public ContentPack Build()
        {
            return new ContentPack(
                new DefinitionId("test.pack"),
                "Test Pack",
                ContentPackTestFactory.Tags(
                    "resource", "unit", "hero", "ability", "structure", "technology", "settlement"),
                Resources,
                Units,
                Heroes,
                Abilities,
                Buildings,
                Technologies,
                Settlements,
                new GameRuleSet(false, true, false, false, true, false, true));
        }
    }
}
