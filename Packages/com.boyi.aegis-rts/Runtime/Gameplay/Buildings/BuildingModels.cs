using System;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Content.Definitions;

namespace AegisRTS.Gameplay.Buildings
{
    public sealed class ConstructBuildingCommand : ICommand
    {
        public ConstructBuildingCommand(EntityId settlementId, EntityId factionId, DefinitionId buildingId)
        { if (!settlementId.IsValid || !factionId.IsValid || !buildingId.IsValid) throw new ArgumentException("Settlement, faction, and building IDs must be valid."); SettlementId = settlementId; FactionId = factionId; BuildingId = buildingId; }
        public EntityId SettlementId { get; }
        public EntityId FactionId { get; }
        public DefinitionId BuildingId { get; }
    }

    public readonly struct BuildingRequestResult
    {
        private BuildingRequestResult(bool succeeded, string error) { Succeeded = succeeded; Error = error ?? string.Empty; }
        public bool Succeeded { get; }
        public string Error { get; }
        public static BuildingRequestResult Success() => new BuildingRequestResult(true, string.Empty);
        public static BuildingRequestResult Failure(string error) => new BuildingRequestResult(false, error);
    }

    public interface IBuildingStatusQuery
    {
        bool IsBuilt(EntityId settlementId, DefinitionId buildingId);
    }

    public interface IBuildingCompletionSink
    {
        void BuildingCompleted(EntityId settlementId, DefinitionId buildingId);
    }

    public sealed class BuildingCompletedEvent : IEvent
    {
        public BuildingCompletedEvent(EntityId settlementId, DefinitionId buildingId)
        { SettlementId = settlementId; BuildingId = buildingId; }
        public EntityId SettlementId { get; }
        public DefinitionId BuildingId { get; }
    }
}
