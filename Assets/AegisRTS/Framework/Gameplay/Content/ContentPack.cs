using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AegisRTS.Gameplay.Content.Definitions;

namespace AegisRTS.Gameplay.Content
{
    /// <summary>Immutable collection of authored definitions and rules for one content world.</summary>
    public sealed class ContentPack
    {
        public ContentPack(
            DefinitionId id,
            string displayName,
            IEnumerable<ContentTag> declaredTags,
            IEnumerable<ResourceDefinition> resources,
            IEnumerable<UnitDefinition> units,
            IEnumerable<HeroDefinition> heroes,
            IEnumerable<AbilityDefinition> abilities,
            IEnumerable<BuildingDefinition> buildings,
            IEnumerable<TechnologyDefinition> technologies,
            IEnumerable<SettlementDefinition> settlements,
            GameRuleSet rules)
        {
            if (!id.IsValid)
            {
                throw new ArgumentException("A valid content pack ID is required.", nameof(id));
            }

            Id = id;
            DisplayName = displayName ?? string.Empty;
            DeclaredTags = Copy(declaredTags);
            Resources = Copy(resources);
            Units = Copy(units);
            Heroes = Copy(heroes);
            Abilities = Copy(abilities);
            Buildings = Copy(buildings);
            Technologies = Copy(technologies);
            Settlements = Copy(settlements);
            Rules = rules;
        }

        public DefinitionId Id { get; }

        public string DisplayName { get; }

        public IReadOnlyList<ContentTag> DeclaredTags { get; }

        public IReadOnlyList<ResourceDefinition> Resources { get; }

        public IReadOnlyList<UnitDefinition> Units { get; }

        public IReadOnlyList<HeroDefinition> Heroes { get; }

        public IReadOnlyList<AbilityDefinition> Abilities { get; }

        public IReadOnlyList<BuildingDefinition> Buildings { get; }

        public IReadOnlyList<TechnologyDefinition> Technologies { get; }

        public IReadOnlyList<SettlementDefinition> Settlements { get; }

        public GameRuleSet Rules { get; }

        /// <summary>Enumerates all definitions in deterministic category order.</summary>
        public IEnumerable<IDefinition> EnumerateDefinitions()
        {
            foreach (ResourceDefinition definition in Resources)
            {
                yield return definition;
            }

            foreach (AbilityDefinition definition in Abilities)
            {
                yield return definition;
            }

            foreach (UnitDefinition definition in Units)
            {
                yield return definition;
            }

            foreach (HeroDefinition definition in Heroes)
            {
                yield return definition;
            }

            foreach (BuildingDefinition definition in Buildings)
            {
                yield return definition;
            }

            foreach (TechnologyDefinition definition in Technologies)
            {
                yield return definition;
            }

            foreach (SettlementDefinition definition in Settlements)
            {
                yield return definition;
            }
        }

        private static IReadOnlyList<T> Copy<T>(IEnumerable<T> values)
        {
            return new ReadOnlyCollection<T>(new List<T>(values ?? Array.Empty<T>()));
        }
    }
}
