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
            : this(id, displayName, maxHealth, prefabId, costs, tags, 0d, null, null, null, 0d)
        {
        }

        public BuildingDefinition(
            DefinitionId id,
            string displayName,
            double maxHealth,
            string prefabId,
            IEnumerable<ResourceCost> costs,
            IEnumerable<ContentTag> tags,
            double buildSeconds,
            IEnumerable<DefinitionId> prerequisiteBuildingIds,
            IEnumerable<DefinitionId> prerequisiteTechnologyIds,
            IEnumerable<ResourceProduction> production,
            double populationCapacity)
            : base(id, displayName, tags)
        {
            MaxHealth = maxHealth;
            PrefabId = prefabId ?? string.Empty;
            Costs = Copy(costs);
            BuildSeconds = buildSeconds;
            PrerequisiteBuildingIds = Copy(prerequisiteBuildingIds);
            PrerequisiteTechnologyIds = Copy(prerequisiteTechnologyIds);
            Production = Copy(production);
            PopulationCapacity = populationCapacity;
        }

        public double MaxHealth { get; }

        public string PrefabId { get; }

        public IReadOnlyList<ResourceCost> Costs { get; }

        public double BuildSeconds { get; }

        public IReadOnlyList<DefinitionId> PrerequisiteBuildingIds { get; }

        public IReadOnlyList<DefinitionId> PrerequisiteTechnologyIds { get; }

        public IReadOnlyList<ResourceProduction> Production { get; }

        public double PopulationCapacity { get; }
    }
}
