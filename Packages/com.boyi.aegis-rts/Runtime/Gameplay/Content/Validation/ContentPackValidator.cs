using System;
using System.Collections.Generic;
using System.Linq;
using AegisRTS.Gameplay.Content.Definitions;

namespace AegisRTS.Gameplay.Content.Validation
{
    /// <summary>Validates definition identity, references, values, tags, prefabs, and technology graphs.</summary>
    public sealed class ContentPackValidator
    {
        public ContentValidationResult Validate(ContentPack pack, IContentAssetCatalog assets)
        {
            if (pack == null)
            {
                throw new ArgumentNullException(nameof(pack));
            }

            if (assets == null)
            {
                throw new ArgumentNullException(nameof(assets));
            }

            var issues = new List<ContentValidationIssue>();
            if (string.IsNullOrWhiteSpace(pack.DisplayName))
            {
                Add(issues, ContentValidationIssueCode.InvalidDisplayName, pack.Id, "Content pack display name is required.");
            }

            if (pack.Rules == null)
            {
                Add(issues, ContentValidationIssueCode.MissingRuleSet, pack.Id, "A GameRuleSet is required.");
            }

            HashSet<ContentTag> declaredTags = ValidateDeclaredTags(pack, issues);
            List<IDefinition> definitions = pack.EnumerateDefinitions().ToList();
            ValidateDefinitionIds(definitions, issues);

            foreach (IDefinition definition in definitions)
            {
                if (definition != null)
                {
                    ValidateCommon(definition, declaredTags, issues);
                }
            }

            var resourceIds = new HashSet<DefinitionId>(pack.Resources.Where(item => item != null).Select(item => item.Id));
            var abilityIds = new HashSet<DefinitionId>(pack.Abilities.Where(item => item != null).Select(item => item.Id));
            var technologyIds = new HashSet<DefinitionId>(pack.Technologies.Where(item => item != null).Select(item => item.Id));
            var buildingIds = new HashSet<DefinitionId>(pack.Buildings.Where(item => item != null).Select(item => item.Id));

            foreach (UnitDefinition definition in pack.Units.Where(item => item != null))
            {
                ValidateActor(definition.Id, definition.MaxHealth, definition.MovementSpeed, definition.PrefabId,
                    definition.Costs, definition.AbilityIds, resourceIds, abilityIds, assets, issues);
                ValidateNonNegativeStat(definition.Id, definition.RecruitmentSeconds, "Recruitment duration", issues);
                ValidateNonNegativeStat(definition.Id, definition.PopulationCost, "Population cost", issues);
                ValidateReferences(definition.Id, definition.PrerequisiteBuildingIds, buildingIds, "building", issues);
                ValidateReferences(definition.Id, definition.PrerequisiteTechnologyIds, technologyIds, "technology", issues);
            }

            foreach (HeroDefinition definition in pack.Heroes.Where(item => item != null))
            {
                ValidateActor(definition.Id, definition.MaxHealth, definition.MovementSpeed, definition.PrefabId,
                    definition.Costs, definition.AbilityIds, resourceIds, abilityIds, assets, issues);
                if (!IsFiniteNonNegative(definition.Leadership))
                {
                    Add(issues, ContentValidationIssueCode.InvalidStat, definition.Id,
                        "Hero leadership must be finite and non-negative.");
                }
            }

            foreach (AbilityDefinition definition in pack.Abilities.Where(item => item != null))
            {
                if (!IsFiniteNonNegative(definition.CooldownSeconds))
                {
                    Add(issues, ContentValidationIssueCode.InvalidStat, definition.Id,
                        "Ability cooldown must be finite and non-negative.");
                }

                if (!IsFiniteNonNegative(definition.Range))
                {
                    Add(issues, ContentValidationIssueCode.InvalidStat, definition.Id,
                        "Ability range must be finite and non-negative.");
                }
            }

            foreach (BuildingDefinition definition in pack.Buildings.Where(item => item != null))
            {
                ValidatePositiveStat(definition.Id, definition.MaxHealth, "Building max health", issues);
                ValidatePrefab(definition.Id, definition.PrefabId, assets, issues);
                ValidateCosts(definition.Id, definition.Costs, resourceIds, issues);
                ValidateNonNegativeStat(definition.Id, definition.BuildSeconds, "Build duration", issues);
                ValidateNonNegativeStat(definition.Id, definition.PopulationCapacity, "Population capacity", issues);
                ValidateReferences(definition.Id, definition.PrerequisiteBuildingIds, buildingIds, "building", issues);
                ValidateReferences(definition.Id, definition.PrerequisiteTechnologyIds, technologyIds, "technology", issues);
                foreach (ResourceProduction production in definition.Production)
                {
                    if (!resourceIds.Contains(production.ResourceId))
                        Add(issues, ContentValidationIssueCode.MissingReference, definition.Id, $"Referenced resource '{production.ResourceId}' does not exist.");
                    if (!IsFiniteNonNegative(production.AmountPerSecond))
                        Add(issues, ContentValidationIssueCode.InvalidStat, definition.Id, "Resource production must be finite and non-negative.");
                }
            }

            foreach (TechnologyDefinition definition in pack.Technologies.Where(item => item != null))
            {
                ValidateCosts(definition.Id, definition.Costs, resourceIds, issues);
                ValidateReferences(definition.Id, definition.PrerequisiteIds, technologyIds, "technology", issues);
                ValidateNonNegativeStat(definition.Id, definition.ResearchSeconds, "Research duration", issues);
                foreach (TechnologyModifier modifier in definition.Modifiers)
                    if (double.IsNaN(modifier.Additive) || double.IsInfinity(modifier.Additive) || double.IsNaN(modifier.Multiplier) ||
                        double.IsInfinity(modifier.Multiplier) || modifier.Multiplier <= 0d)
                        Add(issues, ContentValidationIssueCode.InvalidStat, definition.Id, "Technology modifier values are invalid.");
            }

            foreach (SettlementDefinition definition in pack.Settlements.Where(item => item != null))
            {
                ValidatePositiveStat(definition.Id, definition.MaxHealth, "Settlement max health", issues);
                if (!IsFiniteNonNegative(definition.InitialPopulation))
                    Add(issues, ContentValidationIssueCode.InvalidStat, definition.Id, "Settlement population must be finite and non-negative.");
                ValidatePositiveStat(definition.Id, definition.MaxDefense, "Settlement max defense", issues);
                ValidateCaptureRule(definition, issues);
                ValidatePrefab(definition.Id, definition.PrefabId, assets, issues);
                ValidateReferences(definition.Id, definition.StartingResourceIds, resourceIds, "resource", issues);
            }

            foreach (DefenseStructureDefinition definition in pack.DefenseStructures.Where(item => item != null))
            {
                ValidatePositiveStat(definition.Id, definition.MaxHealth, "Defense structure max health", issues);
                ValidateNonNegativeStat(definition.Id, definition.Armor, "Defense structure armor", issues);
                if (string.IsNullOrWhiteSpace(definition.StructureTypeId))
                    Add(issues, ContentValidationIssueCode.InvalidStat, definition.Id, "Defense structure type ID is required.");
                if (!IsKnownSiegeArea(definition.SiegeAreaId))
                    Add(issues, ContentValidationIssueCode.InvalidStat, definition.Id, $"Unknown siege area '{definition.SiegeAreaId}'.");
                ValidatePrefab(definition.Id, definition.PrefabId, assets, issues);
            }

            foreach (AiProfileDefinition definition in pack.AiProfiles.Where(item => item != null))
            {
                ValidateUnitRange(definition.Id, definition.Aggression, "AI aggression", issues);
                ValidateUnitRange(definition.Id, definition.DefenseBias, "AI defense bias", issues);
                ValidateUnitRange(definition.Id, definition.EconomyBias, "AI economy bias", issues);
                ValidateUnitRange(definition.Id, definition.RiskTolerance, "AI risk tolerance", issues);
                ValidateUnitRange(definition.Id, definition.SiegePreference, "AI siege preference", issues);
                ValidatePositiveStat(definition.Id, definition.DecisionIntervalSeconds, "AI decision interval", issues);
                if (definition.DesiredArmySize <= 0)
                    Add(issues, ContentValidationIssueCode.InvalidStat, definition.Id, "AI desired army size must be greater than zero.");
            }

            ValidateTechnologyCycles(pack.Technologies.Where(item => item != null), technologyIds, issues);
            return new ContentValidationResult(issues);
        }

        private static HashSet<ContentTag> ValidateDeclaredTags(
            ContentPack pack,
            ICollection<ContentValidationIssue> issues)
        {
            var declared = new HashSet<ContentTag>();
            foreach (ContentTag tag in pack.DeclaredTags)
            {
                if (!tag.IsValid)
                {
                    Add(issues, ContentValidationIssueCode.MissingTag, pack.Id, "Declared tag cannot be empty.");
                    continue;
                }

                if (!declared.Add(tag))
                {
                    Add(issues, ContentValidationIssueCode.DuplicateTag, pack.Id, $"Tag '{tag}' is declared more than once.");
                }
            }

            return declared;
        }

        private static void ValidateDefinitionIds(
            IEnumerable<IDefinition> definitions,
            ICollection<ContentValidationIssue> issues)
        {
            var ids = new HashSet<DefinitionId>();
            foreach (IDefinition definition in definitions)
            {
                if (definition == null)
                {
                    issues.Add(new ContentValidationIssue(
                        ContentValidationIssueCode.NullDefinition,
                        string.Empty,
                        "Content pack contains a null definition."));
                    continue;
                }

                if (!ids.Add(definition.Id))
                {
                    Add(issues, ContentValidationIssueCode.DuplicateDefinitionId, definition.Id,
                        $"Definition ID '{definition.Id}' is used more than once.");
                }
            }
        }

        private static void ValidateCommon(
            IDefinition definition,
            ISet<ContentTag> declaredTags,
            ICollection<ContentValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(definition.DisplayName))
            {
                Add(issues, ContentValidationIssueCode.InvalidDisplayName, definition.Id,
                    "Definition display name is required.");
            }

            if (definition.Tags.Count == 0)
            {
                Add(issues, ContentValidationIssueCode.MissingTag, definition.Id,
                    "Definition must declare at least one tag.");
            }

            foreach (ContentTag tag in definition.Tags)
            {
                if (!tag.IsValid || !declaredTags.Contains(tag))
                {
                    Add(issues, ContentValidationIssueCode.MissingTag, definition.Id,
                        $"Tag '{tag}' is not declared by the content pack.");
                }
            }
        }

        private static void ValidateActor(
            DefinitionId id,
            double maxHealth,
            double movementSpeed,
            string prefabId,
            IReadOnlyList<ResourceCost> costs,
            IReadOnlyList<DefinitionId> abilityIds,
            ISet<DefinitionId> resourceIds,
            ISet<DefinitionId> knownAbilityIds,
            IContentAssetCatalog assets,
            ICollection<ContentValidationIssue> issues)
        {
            ValidatePositiveStat(id, maxHealth, "Max health", issues);
            if (!IsFiniteNonNegative(movementSpeed))
            {
                Add(issues, ContentValidationIssueCode.InvalidStat, id,
                    "Movement speed must be finite and non-negative.");
            }

            ValidatePrefab(id, prefabId, assets, issues);
            ValidateCosts(id, costs, resourceIds, issues);
            ValidateReferences(id, abilityIds, knownAbilityIds, "ability", issues);
        }

        private static void ValidateCosts(
            DefinitionId ownerId,
            IEnumerable<ResourceCost> costs,
            ISet<DefinitionId> resourceIds,
            ICollection<ContentValidationIssue> issues)
        {
            foreach (ResourceCost cost in costs)
            {
                if (double.IsNaN(cost.Amount) || double.IsInfinity(cost.Amount) || cost.Amount <= 0d)
                {
                    Add(issues, ContentValidationIssueCode.InvalidCost, ownerId,
                        $"Cost for resource '{cost.ResourceId}' must be finite and greater than zero.");
                }

                if (!resourceIds.Contains(cost.ResourceId))
                {
                    Add(issues, ContentValidationIssueCode.MissingReference, ownerId,
                        $"Referenced resource '{cost.ResourceId}' does not exist.");
                }
            }
        }

        private static void ValidateReferences(
            DefinitionId ownerId,
            IEnumerable<DefinitionId> references,
            ISet<DefinitionId> knownIds,
            string referenceType,
            ICollection<ContentValidationIssue> issues)
        {
            foreach (DefinitionId reference in references)
            {
                if (!knownIds.Contains(reference))
                {
                    Add(issues, ContentValidationIssueCode.MissingReference, ownerId,
                        $"Referenced {referenceType} '{reference}' does not exist.");
                }
            }
        }

        private static void ValidatePrefab(
            DefinitionId ownerId,
            string prefabId,
            IContentAssetCatalog assets,
            ICollection<ContentValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(prefabId) || !assets.ContainsPrefab(prefabId))
            {
                Add(issues, ContentValidationIssueCode.MissingPrefab, ownerId,
                    $"Prefab '{prefabId}' does not exist in the asset catalog.");
            }
        }

        private static void ValidatePositiveStat(
            DefinitionId ownerId,
            double value,
            string label,
            ICollection<ContentValidationIssue> issues)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0d)
            {
                Add(issues, ContentValidationIssueCode.InvalidStat, ownerId,
                    $"{label} must be finite and greater than zero.");
            }
        }

        private static bool IsFiniteNonNegative(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0d;

        private static void ValidateNonNegativeStat(DefinitionId ownerId, double value, string label,
            ICollection<ContentValidationIssue> issues)
        {
            if (!IsFiniteNonNegative(value))
                Add(issues, ContentValidationIssueCode.InvalidStat, ownerId, $"{label} must be finite and non-negative.");
        }

        private static void ValidateCaptureRule(SettlementDefinition definition, ICollection<ContentValidationIssue> issues)
        {
            var knownRules = new HashSet<string>(StringComparer.Ordinal)
            { "clear-defenders", "capture-zone", "destroy-core", "kill-commander", "mixed" };
            var knownConditions = new HashSet<string>(StringComparer.Ordinal)
            { "defenders-cleared", "zone-controlled", "core-destroyed", "commander-killed" };
            if (!knownRules.Contains(definition.CaptureRule))
                Add(issues, ContentValidationIssueCode.InvalidStat, definition.Id, $"Unknown capture rule '{definition.CaptureRule}'.");
            foreach (string condition in definition.CaptureConditions)
                if (!knownConditions.Contains(condition))
                    Add(issues, ContentValidationIssueCode.InvalidStat, definition.Id, $"Unknown capture condition '{condition}'.");
            if (definition.CaptureRule == "mixed" && definition.CaptureConditions.Count == 0)
                Add(issues, ContentValidationIssueCode.InvalidStat, definition.Id, "Mixed capture rule requires capture conditions.");
        }

        private static bool IsKnownSiegeArea(string value) =>
            value == "outer-area" || value == "walls" || value == "gates" || value == "towers" ||
            value == "breach" || value == "inner-area" || value == "capture-objective";

        private static void ValidateUnitRange(DefinitionId ownerId, double value, string label,
            ICollection<ContentValidationIssue> issues)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d || value > 1d)
                Add(issues, ContentValidationIssueCode.InvalidStat, ownerId, $"{label} must be between zero and one.");
        }

        private static void ValidateTechnologyCycles(
            IEnumerable<TechnologyDefinition> technologies,
            ISet<DefinitionId> knownTechnologyIds,
            ICollection<ContentValidationIssue> issues)
        {
            Dictionary<DefinitionId, TechnologyDefinition> byId = technologies
                .GroupBy(item => item.Id)
                .ToDictionary(group => group.Key, group => group.First());
            var states = new Dictionary<DefinitionId, int>();
            var reported = new HashSet<DefinitionId>();

            foreach (DefinitionId technologyId in byId.Keys)
            {
                VisitTechnology(technologyId, byId, knownTechnologyIds, states, reported, issues);
            }
        }

        private static void VisitTechnology(
            DefinitionId technologyId,
            IReadOnlyDictionary<DefinitionId, TechnologyDefinition> technologies,
            ISet<DefinitionId> knownTechnologyIds,
            IDictionary<DefinitionId, int> states,
            ISet<DefinitionId> reported,
            ICollection<ContentValidationIssue> issues)
        {
            if (states.TryGetValue(technologyId, out int state))
            {
                if (state == 1 && reported.Add(technologyId))
                {
                    Add(issues, ContentValidationIssueCode.TechnologyCycle, technologyId,
                        $"Technology prerequisite cycle includes '{technologyId}'.");
                }

                return;
            }

            states[technologyId] = 1;
            TechnologyDefinition technology = technologies[technologyId];
            foreach (DefinitionId prerequisiteId in technology.PrerequisiteIds)
            {
                if (knownTechnologyIds.Contains(prerequisiteId) && technologies.ContainsKey(prerequisiteId))
                {
                    VisitTechnology(prerequisiteId, technologies, knownTechnologyIds, states, reported, issues);
                }
            }

            states[technologyId] = 2;
        }

        private static void Add(
            ICollection<ContentValidationIssue> issues,
            ContentValidationIssueCode code,
            DefinitionId subjectId,
            string message)
        {
            issues.Add(new ContentValidationIssue(code, subjectId.ToString(), message));
        }
    }
}
