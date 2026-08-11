using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Formation;

namespace AegisRTS.Gameplay.Units
{
    /// <summary>A Unity-independent world-space point used by gameplay commands.</summary>
    public readonly struct WorldPoint : IEquatable<WorldPoint>
    {
        public WorldPoint(double x, double y, double z)
        {
            if (!IsFinite(x) || !IsFinite(y) || !IsFinite(z))
            {
                throw new ArgumentOutOfRangeException(nameof(x), "World point components must be finite.");
            }

            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public bool Equals(WorldPoint other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        public override bool Equals(object obj) => obj is WorldPoint other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);
        public override string ToString() => $"({X:0.##}, {Y:0.##}, {Z:0.##})";

        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }

    /// <summary>Base class for immutable commands issued to one or more actors.</summary>
    public abstract class UnitCommand : ICommand
    {
        protected UnitCommand(IEnumerable<EntityId> actorIds, bool queue)
        {
            if (actorIds == null)
            {
                throw new ArgumentNullException(nameof(actorIds));
            }

            var unique = new HashSet<EntityId>();
            var snapshot = new List<EntityId>();
            foreach (EntityId actorId in actorIds)
            {
                if (!actorId.IsValid)
                {
                    throw new ArgumentException("Actor identifiers must be valid.", nameof(actorIds));
                }

                if (unique.Add(actorId))
                {
                    snapshot.Add(actorId);
                }
            }

            if (snapshot.Count == 0)
            {
                throw new ArgumentException("At least one actor is required.", nameof(actorIds));
            }

            ActorIds = snapshot.AsReadOnly();
            Queue = queue;
        }

        public IReadOnlyList<EntityId> ActorIds { get; }
        public bool Queue { get; }
    }

    public sealed class MoveUnitsCommand : UnitCommand
    {
        public MoveUnitsCommand(
            IEnumerable<EntityId> actorIds,
            WorldPoint destination,
            bool queue = false,
            FormationType formation = FormationType.Box)
            : base(actorIds, queue)
        {
            Destination = destination;
            Formation = formation;
        }

        public WorldPoint Destination { get; }
        public FormationType Formation { get; }
    }

    public abstract class TargetedUnitCommand : UnitCommand
    {
        protected TargetedUnitCommand(IEnumerable<EntityId> actorIds, EntityId targetId, bool queue)
            : base(actorIds, queue)
        {
            if (!targetId.IsValid)
            {
                throw new ArgumentException("Target identifier must be valid.", nameof(targetId));
            }

            TargetId = targetId;
        }

        public EntityId TargetId { get; }
    }

    public sealed class AttackTargetCommand : TargetedUnitCommand
    {
        public AttackTargetCommand(IEnumerable<EntityId> actorIds, EntityId targetId, bool queue = false)
            : base(actorIds, targetId, queue) { }
    }

    public sealed class FollowTargetCommand : TargetedUnitCommand
    {
        public FollowTargetCommand(IEnumerable<EntityId> actorIds, EntityId targetId, bool queue = false)
            : base(actorIds, targetId, queue) { }
    }

    public sealed class InteractTargetCommand : TargetedUnitCommand
    {
        public InteractTargetCommand(IEnumerable<EntityId> actorIds, EntityId targetId, bool queue = false)
            : base(actorIds, targetId, queue) { }
    }

    public sealed class StopUnitsCommand : UnitCommand
    {
        public StopUnitsCommand(IEnumerable<EntityId> actorIds) : base(actorIds, false) { }
    }

    public sealed class HoldUnitsCommand : UnitCommand
    {
        public HoldUnitsCommand(IEnumerable<EntityId> actorIds, bool queue = false) : base(actorIds, queue) { }
    }
}
