using System;

namespace AegisRTS.Gameplay.Content.Definitions
{
    /// <summary>Data-authored resource income measured per simulation second.</summary>
    public readonly struct ResourceProduction
    {
        public ResourceProduction(DefinitionId resourceId, double amountPerSecond)
        {
            if (!resourceId.IsValid) throw new ArgumentException("A resource ID is required.", nameof(resourceId));
            ResourceId = resourceId;
            AmountPerSecond = amountPerSecond;
        }

        public DefinitionId ResourceId { get; }
        public double AmountPerSecond { get; }
    }
}
