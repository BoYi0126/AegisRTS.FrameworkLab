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
            : base(id, displayName, tags)
        {
            Costs = Copy(costs);
            PrerequisiteIds = Copy(prerequisiteIds);
        }

        public IReadOnlyList<ResourceCost> Costs { get; }

        public IReadOnlyList<DefinitionId> PrerequisiteIds { get; }
    }
}
