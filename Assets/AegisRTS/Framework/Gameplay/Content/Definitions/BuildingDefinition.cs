using System.Collections.Generic;

namespace AegisRTS.Gameplay.Content.Definitions
{
    public sealed class BuildingDefinition : DefinitionBase
    {
        public BuildingDefinition(
            DefinitionId id,
            string displayName,
            double maxHealth,
            string prefabId,
            IEnumerable<ResourceCost> costs,
            IEnumerable<ContentTag> tags)
            : base(id, displayName, tags)
        {
            MaxHealth = maxHealth;
            PrefabId = prefabId ?? string.Empty;
            Costs = Copy(costs);
        }

        public double MaxHealth { get; }

        public string PrefabId { get; }

        public IReadOnlyList<ResourceCost> Costs { get; }
    }
}
