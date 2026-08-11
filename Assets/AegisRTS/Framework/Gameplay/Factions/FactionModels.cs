using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;

namespace AegisRTS.Gameplay.Factions
{
    public enum DiplomacyStatus { Neutral, Allied, Hostile, War }

    public sealed class FactionProfile
    {
        public FactionProfile(string definitionId, string aiProfileId = "")
        {
            if (string.IsNullOrWhiteSpace(definitionId)) throw new ArgumentException("Definition ID is required.", nameof(definitionId));
            DefinitionId = definitionId.Trim();
            AiProfileId = aiProfileId?.Trim() ?? string.Empty;
        }
        public string DefinitionId { get; }
        public string AiProfileId { get; }
    }

    public readonly struct FactionSnapshot
    {
        public FactionSnapshot(EntityId factionId, FactionProfile profile,
            IReadOnlyDictionary<string, double> resources, IReadOnlyList<EntityId> settlementIds,
            IReadOnlyList<EntityId> territoryIds, IReadOnlyList<EntityId> armyIds,
            IReadOnlyList<string> technologyIds, IReadOnlyDictionary<EntityId, DiplomacyStatus> diplomacy)
        {
            FactionId = factionId; Profile = profile; Resources = resources; SettlementIds = settlementIds;
            TerritoryIds = territoryIds; ArmyIds = armyIds; TechnologyIds = technologyIds; Diplomacy = diplomacy;
        }
        public EntityId FactionId { get; }
        public FactionProfile Profile { get; }
        public IReadOnlyDictionary<string, double> Resources { get; }
        public IReadOnlyList<EntityId> SettlementIds { get; }
        public IReadOnlyList<EntityId> TerritoryIds { get; }
        public IReadOnlyList<EntityId> ArmyIds { get; }
        public IReadOnlyList<string> TechnologyIds { get; }
        public IReadOnlyDictionary<EntityId, DiplomacyStatus> Diplomacy { get; }
    }

    public interface IFactionQuery
    {
        bool Contains(EntityId factionId);
        bool TryGetState(EntityId factionId, out FactionSnapshot snapshot);
        IReadOnlyList<FactionSnapshot> Snapshot();
        string GetDebugSummary();
    }

    public sealed class DiplomacyChangedEvent : IEvent
    {
        public DiplomacyChangedEvent(EntityId firstFactionId, EntityId secondFactionId, DiplomacyStatus status)
        { FirstFactionId = firstFactionId; SecondFactionId = secondFactionId; Status = status; }
        public EntityId FirstFactionId { get; }
        public EntityId SecondFactionId { get; }
        public DiplomacyStatus Status { get; }
    }
}
