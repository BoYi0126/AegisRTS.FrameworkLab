using System.Collections.Generic;

namespace AegisRTS.Gameplay.Content.Definitions
{
    public sealed class UnitDefinition : DefinitionBase
    {
        public UnitDefinition(
            DefinitionId id,
            string displayName,
            double maxHealth,
            double movementSpeed,
            string prefabId,
            IEnumerable<ResourceCost> costs,
            IEnumerable<DefinitionId> abilityIds,
            IEnumerable<ContentTag> tags)
            : this(id, displayName, maxHealth, movementSpeed, prefabId, costs, abilityIds, tags,
                0d, 0d, null, null)
        {
        }

        public UnitDefinition(
            DefinitionId id,
            string displayName,
            double maxHealth,
            double movementSpeed,
            string prefabId,
            IEnumerable<ResourceCost> costs,
            IEnumerable<DefinitionId> abilityIds,
            IEnumerable<ContentTag> tags,
            double recruitmentSeconds,
            double populationCost,
            IEnumerable<DefinitionId> prerequisiteBuildingIds,
            IEnumerable<DefinitionId> prerequisiteTechnologyIds)
            : base(id, displayName, tags)
        {
            MaxHealth = maxHealth;
            MovementSpeed = movementSpeed;
            PrefabId = prefabId ?? string.Empty;
            Costs = Copy(costs);
            AbilityIds = Copy(abilityIds);
            RecruitmentSeconds = recruitmentSeconds;
            PopulationCost = populationCost;
            PrerequisiteBuildingIds = Copy(prerequisiteBuildingIds);
            PrerequisiteTechnologyIds = Copy(prerequisiteTechnologyIds);
        }

        public double MaxHealth { get; }

        public double MovementSpeed { get; }

        public string PrefabId { get; }

        public IReadOnlyList<ResourceCost> Costs { get; }

        public IReadOnlyList<DefinitionId> AbilityIds { get; }

        public double RecruitmentSeconds { get; }

        public double PopulationCost { get; }

        public IReadOnlyList<DefinitionId> PrerequisiteBuildingIds { get; }

        public IReadOnlyList<DefinitionId> PrerequisiteTechnologyIds { get; }
    }
}
