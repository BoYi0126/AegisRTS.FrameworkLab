using System;
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
            IEnumerable<ContentTag> tags,
            double initialPopulation = 0d,
            double maxDefense = 1d,
            string captureRule = "clear-defenders",
            IEnumerable<string> captureConditions = null)
            : base(id, displayName, tags)
        {
            MaxHealth = maxHealth;
            PrefabId = prefabId ?? string.Empty;
            StartingResourceIds = Copy(startingResourceIds);
            InitialPopulation = initialPopulation;
            MaxDefense = maxDefense;
            CaptureRule = captureRule?.Trim() ?? string.Empty;
            var conditions = new List<string>();
            foreach (string condition in captureConditions ?? Array.Empty<string>())
                if (!string.IsNullOrWhiteSpace(condition)) conditions.Add(condition.Trim());
            CaptureConditions = conditions.AsReadOnly();
        }

        public double MaxHealth { get; }

        public string PrefabId { get; }

        public IReadOnlyList<DefinitionId> StartingResourceIds { get; }

        public double InitialPopulation { get; }

        public double MaxDefense { get; }

        public string CaptureRule { get; }

        public IReadOnlyList<string> CaptureConditions { get; }
    }
}
