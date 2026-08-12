using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Formation;
using AegisRTS.Gameplay.Units;

namespace AegisRTS.Gameplay.Movement
{
    /// <summary>Transforms shared movement commands into validated formation-slot navigation orders.</summary>
    public sealed class MovementSystem
    {
        private const double ArrivalDistance = 0.35d;
        private const double MovingSpeedSquared = 0.01d;
        private const double StuckThresholdSeconds = 2d;
        private const int MaximumRepathAttempts = 3;
        private readonly INavigationAdapter _navigation;
        private readonly Dictionary<EntityId, UnitRecord> _units = new Dictionary<EntityId, UnitRecord>();

        public MovementSystem(INavigationAdapter navigation, double formationSpacing = 1.8d)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            if (formationSpacing <= 0d || double.IsNaN(formationSpacing) || double.IsInfinity(formationSpacing))
                throw new ArgumentOutOfRangeException(nameof(formationSpacing));
            FormationSpacing = formationSpacing;
        }

        public double FormationSpacing { get; }
        public int RegisteredUnitCount => _units.Count;

        public void Register(EntityId entityId, WorldPoint initialPosition)
        {
            if (!entityId.IsValid) throw new ArgumentException("Entity identifier must be valid.", nameof(entityId));
            if (_units.ContainsKey(entityId)) throw new InvalidOperationException($"Movement entity {entityId} is already registered.");
            _units.Add(entityId, new UnitRecord(entityId, initialPosition));
        }

        public bool Unregister(EntityId entityId)
        {
            if (!_units.Remove(entityId)) return false;
            _navigation.Stop(entityId);
            return true;
        }

        public MovementCommandResult IssueMove(MoveUnitsCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            var actors = new List<UnitRecord>();
            foreach (EntityId actorId in command.ActorIds)
            {
                if (_units.TryGetValue(actorId, out UnitRecord record)) actors.Add(record);
            }
            actors.Sort((left, right) => left.EntityId.CompareTo(right.EntityId));
            if (actors.Count == 0) return new MovementCommandResult(0, command.ActorIds.Count);

            double centerX = 0d;
            double centerZ = 0d;
            foreach (UnitRecord actor in actors)
            {
                centerX += actor.Position.X;
                centerZ += actor.Position.Z;
            }
            centerX /= actors.Count;
            centerZ /= actors.Count;

            IReadOnlyList<FormationSlot> slots = FormationPlanner.Plan(
                command.Destination,
                actors.Count,
                command.Formation,
                FormationSpacing,
                command.Destination.X - centerX,
                command.Destination.Z - centerZ);

            int accepted = 0;
            for (int index = 0; index < actors.Count; index++)
            {
                UnitRecord actor = actors[index];
                var order = new MovementOrder(slots[index].Destination, slots[index].Index);
                if (command.Queue && actor.CurrentOrder != null)
                {
                    actor.Orders.Enqueue(order);
                    accepted++;
                    continue;
                }

                if (!command.Queue)
                {
                    actor.Orders.Clear();
                    _navigation.Stop(actor.EntityId);
                }

                if (StartOrder(actor, order)) accepted++;
            }

            return new MovementCommandResult(accepted, command.ActorIds.Count - accepted);
        }

        public void IssueStop(StopUnitsCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            foreach (EntityId actorId in command.ActorIds)
            {
                if (!_units.TryGetValue(actorId, out UnitRecord actor)) continue;
                actor.Orders.Clear();
                actor.CurrentOrder = null;
                actor.Status = MovementStatus.Idle;
                actor.Velocity = default;
                actor.StuckSeconds = 0d;
                _navigation.Stop(actorId);
            }
        }

        public void IssueHold(HoldUnitsCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            foreach (EntityId actorId in command.ActorIds)
            {
                if (!_units.TryGetValue(actorId, out UnitRecord actor)) continue;
                actor.Orders.Clear();
                actor.CurrentOrder = null;
                actor.Status = MovementStatus.Holding;
                actor.Velocity = default;
                actor.StuckSeconds = 0d;
                _navigation.Stop(actorId);
            }
        }

        public void Tick(double deltaSeconds)
        {
            if (deltaSeconds < 0d || double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds))
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));

            foreach (UnitRecord actor in _units.Values)
            {
                if (!_navigation.TryGetSnapshot(actor.EntityId, out NavigationAgentSnapshot navigation)) continue;
                actor.Position = navigation.Position;
                actor.Velocity = navigation.Velocity;
                if (actor.CurrentOrder == null || actor.Status == MovementStatus.Holding) continue;

                if (!navigation.IsOnNavigation ||
                    navigation.PathState == NavigationPathState.Invalid ||
                    navigation.PathState == NavigationPathState.Partial)
                {
                    RepathOrFail(actor);
                    continue;
                }

                if (navigation.PathState != NavigationPathState.Pending &&
                    navigation.RemainingDistance <= ArrivalDistance &&
                    navigation.SpeedSquared <= MovingSpeedSquared)
                {
                    CompleteOrder(actor);
                    continue;
                }

                if (navigation.PathState == NavigationPathState.Complete &&
                    navigation.RemainingDistance > ArrivalDistance &&
                    navigation.SpeedSquared <= MovingSpeedSquared)
                {
                    actor.StuckSeconds += deltaSeconds;
                    if (actor.StuckSeconds >= StuckThresholdSeconds) RepathOrFail(actor);
                }
                else
                {
                    actor.StuckSeconds = 0d;
                }
            }
        }

        public bool TryGetState(EntityId entityId, out MovementStateSnapshot snapshot)
        {
            if (!_units.TryGetValue(entityId, out UnitRecord actor))
            {
                snapshot = default;
                return false;
            }

            snapshot = new MovementStateSnapshot(
                actor.EntityId,
                actor.Status,
                actor.Position,
                actor.CurrentOrder?.Destination ?? actor.Position,
                actor.Velocity,
                actor.CurrentOrder?.FormationSlotIndex ?? -1,
                actor.Orders.Count,
                actor.RepathCount,
                actor.StuckSeconds);
            return true;
        }

        public IReadOnlyList<MovementStateSnapshot> Snapshot()
        {
            var result = new List<MovementStateSnapshot>(_units.Count);
            foreach (EntityId entityId in _units.Keys)
            {
                TryGetState(entityId, out MovementStateSnapshot snapshot);
                result.Add(snapshot);
            }
            result.Sort((left, right) => left.EntityId.CompareTo(right.EntityId));
            return result.AsReadOnly();
        }

        public IReadOnlyList<MovementOrderSnapshot> SnapshotOrders(EntityId entityId)
        {
            if (!_units.TryGetValue(entityId, out UnitRecord actor)) return Array.Empty<MovementOrderSnapshot>();
            var result = new List<MovementOrderSnapshot>(actor.Orders.Count + (actor.CurrentOrder == null ? 0 : 1));
            if (actor.CurrentOrder != null)
                result.Add(new MovementOrderSnapshot(actor.CurrentOrder.Destination, actor.CurrentOrder.FormationSlotIndex));
            foreach (MovementOrder order in actor.Orders)
                result.Add(new MovementOrderSnapshot(order.Destination, order.FormationSlotIndex));
            return result.AsReadOnly();
        }

        /// <summary>Restores exact already-validated destinations without recalculating formation slots.</summary>
        public bool RestoreOrders(EntityId entityId, IReadOnlyList<MovementOrderSnapshot> orders, MovementStatus emptyStatus = MovementStatus.Idle)
        {
            if (!_units.TryGetValue(entityId, out UnitRecord actor)) return false;
            if (orders == null) throw new ArgumentNullException(nameof(orders));
            actor.Orders.Clear();
            actor.CurrentOrder = null;
            actor.Velocity = default;
            actor.RepathCount = 0;
            actor.StuckSeconds = 0d;
            _navigation.Stop(entityId);
            if (orders.Count == 0)
            {
                actor.Status = emptyStatus == MovementStatus.Holding ? MovementStatus.Holding : MovementStatus.Idle;
                return true;
            }

            var first = new MovementOrder(orders[0].Destination, orders[0].FormationSlotIndex);
            if (!StartOrder(actor, first)) return false;
            for (int index = 1; index < orders.Count; index++)
                actor.Orders.Enqueue(new MovementOrder(orders[index].Destination, orders[index].FormationSlotIndex));
            return true;
        }

        public string GetDebugSummary()
        {
            int moving = 0;
            int arrived = 0;
            int blocked = 0;
            foreach (UnitRecord actor in _units.Values)
            {
                if (actor.Status == MovementStatus.Moving) moving++;
                else if (actor.Status == MovementStatus.Arrived) arrived++;
                else if (actor.Status == MovementStatus.Stuck || actor.Status == MovementStatus.Unreachable) blocked++;
            }
            return $"Registered={_units.Count}, Moving={moving}, Arrived={arrived}, Blocked={blocked}";
        }

        private bool StartOrder(UnitRecord actor, MovementOrder order)
        {
            actor.CurrentOrder = order;
            actor.Status = MovementStatus.Moving;
            actor.RepathCount = 0;
            actor.StuckSeconds = 0d;
            NavigationDestinationResult result = _navigation.SetDestination(actor.EntityId, order.Destination, order.FormationSlotIndex);
            if (result.Accepted)
            {
                order.Destination = result.ResolvedDestination;
                return true;
            }

            actor.Status = MovementStatus.Unreachable;
            actor.CurrentOrder = null;
            TryStartNext(actor);
            return false;
        }

        private void CompleteOrder(UnitRecord actor)
        {
            actor.Status = MovementStatus.Arrived;
            actor.CurrentOrder = null;
            actor.RepathCount = 0;
            actor.StuckSeconds = 0d;
            TryStartNext(actor);
        }

        private void RepathOrFail(UnitRecord actor)
        {
            if (actor.CurrentOrder == null) return;
            actor.StuckSeconds = 0d;
            if (actor.RepathCount >= MaximumRepathAttempts)
            {
                actor.Status = MovementStatus.Stuck;
                actor.CurrentOrder = null;
                _navigation.Stop(actor.EntityId);
                TryStartNext(actor);
                return;
            }

            actor.RepathCount++;
            NavigationDestinationResult result = _navigation.SetDestination(
                actor.EntityId,
                actor.CurrentOrder.Destination,
                actor.CurrentOrder.FormationSlotIndex);
            if (result.Accepted)
            {
                actor.CurrentOrder.Destination = result.ResolvedDestination;
                actor.Status = MovementStatus.Moving;
                return;
            }

            actor.Status = MovementStatus.Unreachable;
            actor.CurrentOrder = null;
            TryStartNext(actor);
        }

        private void TryStartNext(UnitRecord actor)
        {
            if (actor.Orders.Count > 0) StartOrder(actor, actor.Orders.Dequeue());
        }

        private sealed class MovementOrder
        {
            public MovementOrder(WorldPoint destination, int formationSlotIndex)
            {
                Destination = destination;
                FormationSlotIndex = formationSlotIndex;
            }

            public WorldPoint Destination { get; set; }
            public int FormationSlotIndex { get; }
        }

        private sealed class UnitRecord
        {
            public UnitRecord(EntityId entityId, WorldPoint position)
            {
                EntityId = entityId;
                Position = position;
                Status = MovementStatus.Idle;
            }

            public EntityId EntityId { get; }
            public Queue<MovementOrder> Orders { get; } = new Queue<MovementOrder>();
            public MovementOrder CurrentOrder { get; set; }
            public MovementStatus Status { get; set; }
            public WorldPoint Position { get; set; }
            public WorldPoint Velocity { get; set; }
            public int RepathCount { get; set; }
            public double StuckSeconds { get; set; }
        }
    }
}
