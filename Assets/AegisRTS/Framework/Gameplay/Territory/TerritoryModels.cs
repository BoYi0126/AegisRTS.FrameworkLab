using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;

namespace AegisRTS.Gameplay.Territory
{
    public enum TerritoryVisibility { Hidden, Explored, Visible }

    public sealed class TerritoryNodeProfile
    {
        public TerritoryNodeProfile(string definitionId, double value, EntityId settlementId = default)
        {
            if (string.IsNullOrWhiteSpace(definitionId)) throw new ArgumentException("Definition ID is required.", nameof(definitionId));
            if (value < 0d || double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value));
            DefinitionId = definitionId.Trim(); Value = value; SettlementId = settlementId;
        }
        public string DefinitionId { get; }
        public double Value { get; }
        public EntityId SettlementId { get; }
    }

    public readonly struct TerritorySnapshot
    {
        public TerritorySnapshot(EntityId territoryId, TerritoryNodeProfile profile, EntityId ownerId,
            IReadOnlyList<EntityId> connectionIds, IReadOnlyDictionary<EntityId, TerritoryVisibility> visibility)
        { TerritoryId = territoryId; Profile = profile; OwnerId = ownerId; ConnectionIds = connectionIds; Visibility = visibility; }
        public EntityId TerritoryId { get; }
        public TerritoryNodeProfile Profile { get; }
        public EntityId OwnerId { get; }
        public IReadOnlyList<EntityId> ConnectionIds { get; }
        public IReadOnlyDictionary<EntityId, TerritoryVisibility> Visibility { get; }
    }

    public interface ITerritoryQuery
    {
        bool TryGetState(EntityId territoryId, out TerritorySnapshot snapshot);
        bool TryGetTerritoryForSettlement(EntityId settlementId, out EntityId territoryId);
        IReadOnlyList<TerritorySnapshot> Snapshot();
        string GetDebugSummary();
    }

    public sealed class TerritoryOwnerChangedEvent : IEvent
    {
        public TerritoryOwnerChangedEvent(EntityId territoryId, EntityId previousOwnerId, EntityId newOwnerId)
        { TerritoryId = territoryId; PreviousOwnerId = previousOwnerId; NewOwnerId = newOwnerId; }
        public EntityId TerritoryId { get; }
        public EntityId PreviousOwnerId { get; }
        public EntityId NewOwnerId { get; }
    }
}
