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
            IEnumerable<ContentTag> tags,
            double leadership = 0d)
            : base(id, displayName, tags)
        {
            MaxHealth = maxHealth;
            MovementSpeed = movementSpeed;
            PrefabId = prefabId ?? string.Empty;
            Costs = Copy(costs);
            AbilityIds = Copy(abilityIds);
            Leadership = leadership;
        }

        public double MaxHealth { get; }

        public double MovementSpeed { get; }

        public string PrefabId { get; }

        public IReadOnlyList<ResourceCost> Costs { get; }

        public IReadOnlyList<DefinitionId> AbilityIds { get; }

        public double Leadership { get; }
    }
}
