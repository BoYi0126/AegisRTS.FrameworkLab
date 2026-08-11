using System;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Content.Definitions;

namespace AegisRTS.Gameplay.Recruitment
{
    public sealed class RecruitUnitCommand : ICommand
    {
        public RecruitUnitCommand(EntityId settlementId, EntityId factionId, DefinitionId unitId)
        { if (!settlementId.IsValid || !factionId.IsValid || !unitId.IsValid) throw new ArgumentException("Settlement, faction, and unit IDs must be valid."); SettlementId = settlementId; FactionId = factionId; UnitId = unitId; }
        public EntityId SettlementId { get; }
        public EntityId FactionId { get; }
        public DefinitionId UnitId { get; }
    }

    public readonly struct RecruitmentRequestResult
    {
        private RecruitmentRequestResult(bool succeeded, string error) { Succeeded = succeeded; Error = error ?? string.Empty; }
        public bool Succeeded { get; }
        public string Error { get; }
        public static RecruitmentRequestResult Success() => new RecruitmentRequestResult(true, string.Empty);
        public static RecruitmentRequestResult Failure(string error) => new RecruitmentRequestResult(false, error);
    }

    public interface IUnitSpawnSink
    {
        void SpawnUnit(EntityId settlementId, EntityId factionId, DefinitionId unitId);
    }

    public sealed class UnitRecruitedEvent : IEvent
    {
        public UnitRecruitedEvent(EntityId settlementId, EntityId factionId, DefinitionId unitId)
        { SettlementId = settlementId; FactionId = factionId; UnitId = unitId; }
        public EntityId SettlementId { get; }
        public EntityId FactionId { get; }
        public DefinitionId UnitId { get; }
    }
}
