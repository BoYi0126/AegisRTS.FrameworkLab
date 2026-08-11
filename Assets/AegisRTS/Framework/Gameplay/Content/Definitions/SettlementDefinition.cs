using System.Collections.Generic;

namespace AegisRTS.Gameplay.Content.Definitions
{
    public sealed class SettlementDefinition : DefinitionBase
    {
        public SettlementDefinition(
            DefinitionId id,
            string displayName,
            double maxHealth,
            string prefabId,
            IEnumerable<DefinitionId> startingResourceIds,
            IEnumerable<ContentTag> tags)
            : base(id, displayName, tags)
        {
            MaxHealth = maxHealth;
            PrefabId = prefabId ?? string.Empty;
            StartingResourceIds = Copy(startingResourceIds);
        }

        public double MaxHealth { get; }

        public string PrefabId { get; }

        public IReadOnlyList<DefinitionId> StartingResourceIds { get; }
    }
}
