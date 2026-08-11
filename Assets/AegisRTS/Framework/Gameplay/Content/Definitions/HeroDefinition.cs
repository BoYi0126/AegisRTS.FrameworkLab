using System.Collections.Generic;

namespace AegisRTS.Gameplay.Content.Definitions
{
    public sealed class HeroDefinition : DefinitionBase
    {
        public HeroDefinition(
            DefinitionId id,
            string displayName,
            double maxHealth,
            double movementSpeed,
            string prefabId,
            IEnumerable<ResourceCost> costs,
            IEnumerable<DefinitionId> abilityIds,
            IEnumerable<ContentTag> tags)
            : base(id, displayName, tags)
        {
            MaxHealth = maxHealth;
            MovementSpeed = movementSpeed;
            PrefabId = prefabId ?? string.Empty;
            Costs = Copy(costs);
            AbilityIds = Copy(abilityIds);
        }

        public double MaxHealth { get; }

        public double MovementSpeed { get; }

        public string PrefabId { get; }

        public IReadOnlyList<ResourceCost> Costs { get; }

        public IReadOnlyList<DefinitionId> AbilityIds { get; }
    }
}
