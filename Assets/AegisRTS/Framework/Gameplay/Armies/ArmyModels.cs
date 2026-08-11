using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Content;
using AegisRTS.Gameplay.Formation;
using AegisRTS.Gameplay.Units;

namespace AegisRTS.Gameplay.Armies
{
    public enum ArmyOrderType { Idle, Move, Attack, AttackSettlement, Defend, Retreat }

    public sealed class ArmyRuleOptions
    {
        public ArmyRuleOptions(bool moraleEnabled, bool supplyEnabled, double initialMorale = 100d, double initialSupply = 100d)
        {
            if (initialMorale < 0d || initialMorale > 100d || initialSupply < 0d || initialSupply > 100d)
                throw new ArgumentOutOfRangeException(nameof(initialMorale));
            MoraleEnabled = moraleEnabled;
            SupplyEnabled = supplyEnabled;
            InitialMorale = initialMorale;
            InitialSupply = initialSupply;
        }

        public bool MoraleEnabled { get; }
        public bool SupplyEnabled { get; }
        public double InitialMorale { get; }
        public double InitialSupply { get; }

        public static ArmyRuleOptions From(GameRuleSet rules) => rules == null
            ? new ArmyRuleOptions(false, false)
            : new ArmyRuleOptions(rules.MoraleEnabled, rules.SupplyEnabled);
    }

    public readonly struct ArmyOrder
    {
        public ArmyOrder(ArmyOrderType type, WorldPoint destination, EntityId targetId, FormationType formation)
        { Type = type; Destination = destination; TargetId = targetId; Formation = formation; }
        public ArmyOrderType Type { get; }
        public WorldPoint Destination { get; }
        public EntityId TargetId { get; }
        public FormationType Formation { get; }
        public static ArmyOrder Idle(FormationType formation) => new ArmyOrder(ArmyOrderType.Idle, default, default, formation);
    }

    public readonly struct ArmySnapshot
    {
        public ArmySnapshot(EntityId armyId, EntityId factionId, EntityId commanderId,
            IReadOnlyList<EntityId> unitIds, FormationType formation, bool moraleEnabled, double morale,
            bool supplyEnabled, double supply, ArmyOrder order)
        {
            ArmyId = armyId;
            FactionId = factionId;
            CommanderId = commanderId;
            UnitIds = unitIds ?? throw new ArgumentNullException(nameof(unitIds));
            Formation = formation;
            MoraleEnabled = moraleEnabled;
            Morale = morale;
            SupplyEnabled = supplyEnabled;
            Supply = supply;
            Order = order;
        }
        public EntityId ArmyId { get; }
        public EntityId FactionId { get; }
        public EntityId CommanderId { get; }
        public IReadOnlyList<EntityId> UnitIds { get; }
        public FormationType Formation { get; }
        public bool MoraleEnabled { get; }
        public double Morale { get; }
        public bool SupplyEnabled { get; }
        public double Supply { get; }
        public ArmyOrder Order { get; }
        public int UnitCount => UnitIds.Count;
    }

    public readonly struct ArmyCommandResult
    {
        private ArmyCommandResult(bool succeeded, string error, EntityId armyId, int affectedUnitCount)
        { Succeeded = succeeded; Error = error ?? string.Empty; ArmyId = armyId; AffectedUnitCount = affectedUnitCount; }
        public bool Succeeded { get; }
        public string Error { get; }
        public EntityId ArmyId { get; }
        public int AffectedUnitCount { get; }
        public static ArmyCommandResult Success(EntityId armyId, int affectedUnitCount) =>
            new ArmyCommandResult(true, string.Empty, armyId, affectedUnitCount);
        public static ArmyCommandResult Failure(string error) =>
            new ArmyCommandResult(false, string.IsNullOrWhiteSpace(error) ? "Army command failed." : error, default, 0);
    }

    public interface IArmyQuery
    {
        bool TryGetState(EntityId armyId, out ArmySnapshot snapshot);
        bool TryGetArmyForUnit(EntityId unitId, out EntityId armyId);
        IReadOnlyList<ArmySnapshot> Snapshot();
        string GetDebugSummary();
    }

    /// <summary>Optional boundary for propagating authoritative army membership to another runtime system.</summary>
    public interface IArmyMembershipSink
    {
        void SetArmy(EntityId unitId, EntityId armyId);
    }

    public sealed class ArmyCreatedEvent : IEvent
    {
        public ArmyCreatedEvent(EntityId armyId, EntityId factionId, int unitCount)
        { ArmyId = armyId; FactionId = factionId; UnitCount = unitCount; }
        public EntityId ArmyId { get; }
        public EntityId FactionId { get; }
        public int UnitCount { get; }
    }

    public sealed class ArmiesMergedEvent : IEvent
    {
        public ArmiesMergedEvent(EntityId targetArmyId, EntityId absorbedArmyId, int unitCount)
        { TargetArmyId = targetArmyId; AbsorbedArmyId = absorbedArmyId; UnitCount = unitCount; }
        public EntityId TargetArmyId { get; }
        public EntityId AbsorbedArmyId { get; }
        public int UnitCount { get; }
    }

    public sealed class ArmySplitEvent : IEvent
    {
        public ArmySplitEvent(EntityId sourceArmyId, EntityId newArmyId, int movedUnitCount)
        { SourceArmyId = sourceArmyId; NewArmyId = newArmyId; MovedUnitCount = movedUnitCount; }
        public EntityId SourceArmyId { get; }
        public EntityId NewArmyId { get; }
        public int MovedUnitCount { get; }
    }

    public sealed class ArmyCommanderAssignedEvent : IEvent
    {
        public ArmyCommanderAssignedEvent(EntityId armyId, EntityId commanderId)
        { ArmyId = armyId; CommanderId = commanderId; }
        public EntityId ArmyId { get; }
        public EntityId CommanderId { get; }
    }

    public sealed class ArmyOrderIssuedEvent : IEvent
    {
        public ArmyOrderIssuedEvent(EntityId armyId, ArmyOrder order, int actorCount)
        { ArmyId = armyId; Order = order; ActorCount = actorCount; }
        public EntityId ArmyId { get; }
        public ArmyOrder Order { get; }
        public int ActorCount { get; }
    }
}
