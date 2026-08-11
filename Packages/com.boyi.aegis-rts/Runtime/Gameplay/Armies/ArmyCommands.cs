using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Formation;
using AegisRTS.Gameplay.Units;

namespace AegisRTS.Gameplay.Armies
{
    public sealed class CreateArmyCommand : ICommand
    {
        public CreateArmyCommand(EntityId armyId, EntityId factionId, IEnumerable<EntityId> unitIds,
            EntityId commanderId = default, FormationType formation = FormationType.Box)
        {
            ArmyId = Required(armyId, nameof(armyId));
            FactionId = Required(factionId, nameof(factionId));
            UnitIds = CopyUnits(unitIds);
            CommanderId = commanderId;
            Formation = formation;
        }
        public EntityId ArmyId { get; }
        public EntityId FactionId { get; }
        public IReadOnlyList<EntityId> UnitIds { get; }
        public EntityId CommanderId { get; }
        public FormationType Formation { get; }

        internal static EntityId Required(EntityId value, string parameter)
        {
            if (!value.IsValid) throw new ArgumentException("Entity ID must be valid.", parameter);
            return value;
        }

        internal static IReadOnlyList<EntityId> CopyUnits(IEnumerable<EntityId> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var seen = new HashSet<EntityId>();
            var result = new List<EntityId>();
            foreach (EntityId value in values)
            {
                Required(value, nameof(values));
                if (seen.Add(value)) result.Add(value);
            }
            if (result.Count == 0) throw new ArgumentException("At least one unit is required.", nameof(values));
            result.Sort();
            return result.AsReadOnly();
        }
    }

    public sealed class MergeArmiesCommand : ICommand
    {
        public MergeArmiesCommand(EntityId targetArmyId, EntityId absorbedArmyId)
        { TargetArmyId = CreateArmyCommand.Required(targetArmyId, nameof(targetArmyId)); AbsorbedArmyId = CreateArmyCommand.Required(absorbedArmyId, nameof(absorbedArmyId)); }
        public EntityId TargetArmyId { get; }
        public EntityId AbsorbedArmyId { get; }
    }

    public sealed class SplitArmyCommand : ICommand
    {
        public SplitArmyCommand(EntityId sourceArmyId, EntityId newArmyId, IEnumerable<EntityId> unitIds, EntityId commanderId = default)
        { SourceArmyId = CreateArmyCommand.Required(sourceArmyId, nameof(sourceArmyId)); NewArmyId = CreateArmyCommand.Required(newArmyId, nameof(newArmyId)); UnitIds = CreateArmyCommand.CopyUnits(unitIds); CommanderId = commanderId; }
        public EntityId SourceArmyId { get; }
        public EntityId NewArmyId { get; }
        public IReadOnlyList<EntityId> UnitIds { get; }
        public EntityId CommanderId { get; }
    }

    public sealed class AssignArmyCommanderCommand : ICommand
    {
        public AssignArmyCommanderCommand(EntityId armyId, EntityId commanderId)
        { ArmyId = CreateArmyCommand.Required(armyId, nameof(armyId)); CommanderId = CreateArmyCommand.Required(commanderId, nameof(commanderId)); }
        public EntityId ArmyId { get; }
        public EntityId CommanderId { get; }
    }

    public abstract class ArmyOrderCommand : ICommand
    {
        protected ArmyOrderCommand(EntityId armyId) => ArmyId = CreateArmyCommand.Required(armyId, nameof(armyId));
        public EntityId ArmyId { get; }
    }

    public sealed class MoveArmyCommand : ArmyOrderCommand
    {
        public MoveArmyCommand(EntityId armyId, WorldPoint destination) : base(armyId) => Destination = destination;
        public WorldPoint Destination { get; }
    }

    public sealed class AttackArmyCommand : ArmyOrderCommand
    {
        public AttackArmyCommand(EntityId armyId, EntityId targetId) : base(armyId) => TargetId = CreateArmyCommand.Required(targetId, nameof(targetId));
        public EntityId TargetId { get; }
    }

    public sealed class AttackSettlementArmyCommand : ArmyOrderCommand
    {
        public AttackSettlementArmyCommand(EntityId armyId, EntityId settlementId) : base(armyId) => SettlementId = CreateArmyCommand.Required(settlementId, nameof(settlementId));
        public EntityId SettlementId { get; }
    }

    public sealed class DefendArmyCommand : ArmyOrderCommand
    {
        public DefendArmyCommand(EntityId armyId, WorldPoint position) : base(armyId) => Position = position;
        public WorldPoint Position { get; }
    }

    public sealed class RetreatArmyCommand : ArmyOrderCommand
    {
        public RetreatArmyCommand(EntityId armyId, WorldPoint destination) : base(armyId) => Destination = destination;
        public WorldPoint Destination { get; }
    }
}
