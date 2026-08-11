namespace AegisRTS.Gameplay.Content.Definitions
{
    /// <summary>References a resource definition and an authored cost amount.</summary>
    public readonly struct ResourceCost
    {
        public ResourceCost(DefinitionId resourceId, double amount)
        {
            ResourceId = resourceId;
            Amount = amount;
        }

        public DefinitionId ResourceId { get; }

        public double Amount { get; }
    }
}
