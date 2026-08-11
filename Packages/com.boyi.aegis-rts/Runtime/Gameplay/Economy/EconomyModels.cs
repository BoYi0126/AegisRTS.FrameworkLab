using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Content.Definitions;

namespace AegisRTS.Gameplay.Economy
{
    public readonly struct EconomyAccountSnapshot
    {
        public EconomyAccountSnapshot(EntityId accountId, IReadOnlyDictionary<DefinitionId, double> resources,
            double populationUsed, double populationCapacity)
        { AccountId = accountId; Resources = resources; PopulationUsed = populationUsed; PopulationCapacity = populationCapacity; }
        public EntityId AccountId { get; }
        public IReadOnlyDictionary<DefinitionId, double> Resources { get; }
        public double PopulationUsed { get; }
        public double PopulationCapacity { get; }
    }

    public sealed class ResourceProducedEvent : IEvent
    {
        public ResourceProducedEvent(EntityId accountId, DefinitionId resourceId, double amount)
        { AccountId = accountId; ResourceId = resourceId; Amount = amount; }
        public EntityId AccountId { get; }
        public DefinitionId ResourceId { get; }
        public double Amount { get; }
    }

    public interface IResourceBalanceSink
    {
        void ApplyResourceDelta(EntityId accountId, DefinitionId resourceId, double delta);
    }

    public sealed class NullResourceBalanceSink : IResourceBalanceSink
    {
        public static readonly NullResourceBalanceSink Instance = new NullResourceBalanceSink();
        private NullResourceBalanceSink() { }
        public void ApplyResourceDelta(EntityId accountId, DefinitionId resourceId, double delta) { }
    }
}
