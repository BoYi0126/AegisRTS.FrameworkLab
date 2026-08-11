using System;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Buildings;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Factions;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Technology;

namespace AegisRTS.Gameplay.Economy
{
    /// <summary>Projects Phase 08 wallet and completion changes into Phase 07 faction/settlement read models.</summary>
    public sealed class GameplayEconomyStateBridge : IResourceBalanceSink, IBuildingCompletionSink,
        ITechnologyCompletionSink
    {
        private readonly FactionSystem _factions;
        private readonly SettlementSystem _settlements;

        public GameplayEconomyStateBridge(FactionSystem factions, SettlementSystem settlements)
        { _factions = factions ?? throw new ArgumentNullException(nameof(factions)); _settlements = settlements ?? throw new ArgumentNullException(nameof(settlements)); }

        public void ApplyResourceDelta(EntityId accountId, DefinitionId resourceId, double delta)
        {
            if (_settlements.AddResource(accountId, resourceId.Value, delta)) return;
            _factions.AddResource(accountId, resourceId.Value, delta);
        }

        public void BuildingCompleted(EntityId settlementId, DefinitionId buildingId) =>
            _settlements.AddBuilding(settlementId, buildingId.Value);

        public void TechnologyCompleted(EntityId factionId, DefinitionId technologyId) =>
            _factions.UnlockTechnology(factionId, technologyId.Value);
    }
}
