using System;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;

namespace AegisRTS.Gameplay.Settlements
{
    public sealed class CaptureSettlementCommand : ICommand
    {
        public CaptureSettlementCommand(EntityId settlementId, EntityId newOwnerId, CaptureCondition completedConditions,
            EntityId capturingArmyId = default)
        {
            if (!settlementId.IsValid || !newOwnerId.IsValid) throw new ArgumentException("Settlement and owner IDs must be valid.");
            SettlementId = settlementId; NewOwnerId = newOwnerId; CompletedConditions = completedConditions;
            CapturingArmyId = capturingArmyId;
        }
        public EntityId SettlementId { get; }
        public EntityId NewOwnerId { get; }
        public CaptureCondition CompletedConditions { get; }
        public EntityId CapturingArmyId { get; }
    }
}
