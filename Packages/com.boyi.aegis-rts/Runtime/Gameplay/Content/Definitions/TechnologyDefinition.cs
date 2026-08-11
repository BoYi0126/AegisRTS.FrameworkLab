using System.Collections.Generic;

namespace AegisRTS.Gameplay.Content.Definitions
{
    public sealed class TechnologyDefinition : DefinitionBase
    {
        public TechnologyDefinition(
            DefinitionId id,
            string displayName,
            IEnumerable<ResourceCost> costs,
            IEnumerable<DefinitionId> prerequisiteIds,
            IEnumerable<ContentTag> tags)
            : this(id, displayName, costs, prerequisiteIds, tags, 0d, null)
        {
        }

        public TechnologyDefinition(
            DefinitionId id,
            string displayName,
            IEnumerable<ResourceCost> costs,
            IEnumerable<DefinitionId> prerequisiteIds,
            IEnumerable<ContentTag> tags,
            double researchSeconds,
            IEnumerable<TechnologyModifier> modifiers)
            : base(id, displayName, tags)
        {
            Costs = Copy(costs);
            PrerequisiteIds = Copy(prerequisiteIds);
            ResearchSeconds = researchSeconds;
            Modifiers = Copy(modifiers);
        }

        public IReadOnlyList<ResourceCost> Costs { get; }

        public IReadOnlyList<DefinitionId> PrerequisiteIds { get; }

        public double ResearchSeconds { get; }

        public IReadOnlyList<TechnologyModifier> Modifiers { get; }
    }
}
