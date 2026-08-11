using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Formation;
using AegisRTS.Gameplay.Movement;
using AegisRTS.Gameplay.Units;

namespace AegisRTS.Gameplay.Armies
{
    public sealed class CombatArmyMembershipSink : IArmyMembershipSink
    {
        private readonly CombatSystem _combat;
        public CombatArmyMembershipSink(CombatSystem combat) => _combat = combat ?? throw new ArgumentNullException(nameof(combat));
        public void SetArmy(EntityId unitId, EntityId armyId) => _combat.UpdateArmyAssignment(unitId, armyId);
    }

    public readonly struct ArmyOrderExecutionResult
    {
        private ArmyOrderExecutionResult(bool accepted, int acceptedActorCount, string error)
        { Accepted = accepted; AcceptedActorCount = acceptedActorCount; Error = error ?? string.Empty; }
        public bool Accepted { get; }
        public int AcceptedActorCount { get; }
        public string Error { get; }
        public static ArmyOrderExecutionResult Success(int count) => new ArmyOrderExecutionResult(true, Math.Max(0, count), string.Empty);
        public static ArmyOrderExecutionResult Failure(string error) => new ArmyOrderExecutionResult(false, 0,
            string.IsNullOrWhiteSpace(error) ? "Army order was rejected." : error);
    }

    public interface IArmyOrderExecutor
    {
        ArmyOrderExecutionResult Move(IReadOnlyList<EntityId> unitIds, WorldPoint destination, FormationType formation);
        ArmyOrderExecutionResult Attack(IReadOnlyList<EntityId> unitIds, EntityId targetId);
        ArmyOrderExecutionResult AttackSettlement(IReadOnlyList<EntityId> unitIds, EntityId settlementId);
        ArmyOrderExecutionResult Defend(IReadOnlyList<EntityId> unitIds, WorldPoint position, FormationType formation);
        ArmyOrderExecutionResult Retreat(IReadOnlyList<EntityId> unitIds, WorldPoint destination, FormationType formation);
    }

    /// <summary>Coordinates existing Movement and Combat systems without moving their state into ArmySystem.</summary>
    public sealed class GameplayArmyOrderExecutor : IArmyOrderExecutor
    {
        private readonly MovementSystem _movement;
        private readonly CombatSystem _combat;

        public GameplayArmyOrderExecutor(MovementSystem movement, CombatSystem combat)
        { _movement = movement ?? throw new ArgumentNullException(nameof(movement)); _combat = combat ?? throw new ArgumentNullException(nameof(combat)); }

        public ArmyOrderExecutionResult Move(IReadOnlyList<EntityId> unitIds, WorldPoint destination, FormationType formation) =>
            FromMovement(_movement.IssueMove(new MoveUnitsCommand(unitIds, destination, formation: formation)));

        public ArmyOrderExecutionResult Attack(IReadOnlyList<EntityId> unitIds, EntityId targetId) =>
            FromCount(_combat.IssueAttack(new AttackTargetCommand(unitIds, targetId)), "No army member could attack the target.");

        public ArmyOrderExecutionResult AttackSettlement(IReadOnlyList<EntityId> unitIds, EntityId settlementId) =>
            FromCount(_combat.IssueAttack(new AttackTargetCommand(unitIds, settlementId)), "No army member could attack the settlement.");

        public ArmyOrderExecutionResult Defend(IReadOnlyList<EntityId> unitIds, WorldPoint position, FormationType formation) =>
            FromMovement(_movement.IssueMove(new MoveUnitsCommand(unitIds, position, formation: formation)));

        public ArmyOrderExecutionResult Retreat(IReadOnlyList<EntityId> unitIds, WorldPoint destination, FormationType formation) =>
            FromMovement(_movement.IssueMove(new MoveUnitsCommand(unitIds, destination, formation: formation)));

        private static ArmyOrderExecutionResult FromMovement(MovementCommandResult result) => result.WasAccepted
            ? ArmyOrderExecutionResult.Success(result.AcceptedActorCount)
            : ArmyOrderExecutionResult.Failure("No army member accepted the movement order.");

        private static ArmyOrderExecutionResult FromCount(int count, string error) => count > 0
            ? ArmyOrderExecutionResult.Success(count)
            : ArmyOrderExecutionResult.Failure(error);
    }

    internal sealed class StateOnlyArmyOrderExecutor : IArmyOrderExecutor
    {
        public ArmyOrderExecutionResult Move(IReadOnlyList<EntityId> units, WorldPoint destination, FormationType formation) => ArmyOrderExecutionResult.Success(units.Count);
        public ArmyOrderExecutionResult Attack(IReadOnlyList<EntityId> units, EntityId targetId) => ArmyOrderExecutionResult.Success(units.Count);
        public ArmyOrderExecutionResult AttackSettlement(IReadOnlyList<EntityId> units, EntityId settlementId) => ArmyOrderExecutionResult.Success(units.Count);
        public ArmyOrderExecutionResult Defend(IReadOnlyList<EntityId> units, WorldPoint position, FormationType formation) => ArmyOrderExecutionResult.Success(units.Count);
        public ArmyOrderExecutionResult Retreat(IReadOnlyList<EntityId> units, WorldPoint destination, FormationType formation) => ArmyOrderExecutionResult.Success(units.Count);
    }

    internal sealed class NullArmyMembershipSink : IArmyMembershipSink
    {
        public void SetArmy(EntityId unitId, EntityId armyId) { }
    }
}
