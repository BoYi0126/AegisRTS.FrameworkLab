using System;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Units;

namespace AegisRTS.Gameplay.Movement
{
    public enum NavigationPathState
    {
        None,
        Pending,
        Complete,
        Partial,
        Invalid,
    }

    public enum MovementStatus
    {
        Idle,
        Moving,
        Arrived,
        Unreachable,
        Stuck,
        Holding,
    }

    public readonly struct NavigationDestinationResult
    {
        private NavigationDestinationResult(bool accepted, WorldPoint resolvedDestination, int pathCornerCount, string error)
        {
            Accepted = accepted;
            ResolvedDestination = resolvedDestination;
            PathCornerCount = pathCornerCount;
            Error = error ?? string.Empty;
        }

        public bool Accepted { get; }
        public WorldPoint ResolvedDestination { get; }
        public int PathCornerCount { get; }
        public string Error { get; }

        public static NavigationDestinationResult Success(WorldPoint destination, int pathCornerCount) =>
            new NavigationDestinationResult(true, destination, Math.Max(0, pathCornerCount), string.Empty);

        public static NavigationDestinationResult Failure(string error) =>
            new NavigationDestinationResult(false, default, 0, string.IsNullOrWhiteSpace(error) ? "Destination is unreachable." : error);
    }

    public readonly struct NavigationAgentSnapshot
    {
        public NavigationAgentSnapshot(
            WorldPoint position,
            WorldPoint velocity,
            double remainingDistance,
            NavigationPathState pathState,
            bool isOnNavigation)
        {
            if (remainingDistance < 0d || double.IsNaN(remainingDistance))
                throw new ArgumentOutOfRangeException(nameof(remainingDistance));
            Position = position;
            Velocity = velocity;
            RemainingDistance = remainingDistance;
            PathState = pathState;
            IsOnNavigation = isOnNavigation;
        }

        public WorldPoint Position { get; }
        public WorldPoint Velocity { get; }
        public double RemainingDistance { get; }
        public NavigationPathState PathState { get; }
        public bool IsOnNavigation { get; }
        public double SpeedSquared => Velocity.X * Velocity.X + Velocity.Y * Velocity.Y + Velocity.Z * Velocity.Z;
    }

    /// <summary>Unity-independent boundary implemented by NavMesh or another navigation backend.</summary>
    public interface INavigationAdapter
    {
        NavigationDestinationResult SetDestination(EntityId entityId, WorldPoint destination, int formationSlotIndex);
        void Stop(EntityId entityId);
        bool TryGetSnapshot(EntityId entityId, out NavigationAgentSnapshot snapshot);
    }

    public readonly struct MovementStateSnapshot
    {
        public MovementStateSnapshot(
            EntityId entityId,
            MovementStatus status,
            WorldPoint position,
            WorldPoint destination,
            WorldPoint velocity,
            int formationSlotIndex,
            int queuedOrderCount,
            int repathCount,
            double stuckSeconds)
        {
            EntityId = entityId;
            Status = status;
            Position = position;
            Destination = destination;
            Velocity = velocity;
            FormationSlotIndex = formationSlotIndex;
            QueuedOrderCount = queuedOrderCount;
            RepathCount = repathCount;
            StuckSeconds = stuckSeconds;
        }

        public EntityId EntityId { get; }
        public MovementStatus Status { get; }
        public WorldPoint Position { get; }
        public WorldPoint Destination { get; }
        public WorldPoint Velocity { get; }
        public int FormationSlotIndex { get; }
        public int QueuedOrderCount { get; }
        public int RepathCount { get; }
        public double StuckSeconds { get; }
    }

    public readonly struct MovementCommandResult
    {
        public MovementCommandResult(int acceptedActorCount, int rejectedActorCount)
        {
            AcceptedActorCount = acceptedActorCount;
            RejectedActorCount = rejectedActorCount;
        }

        public int AcceptedActorCount { get; }
        public int RejectedActorCount { get; }
        public bool WasAccepted => AcceptedActorCount > 0;
    }
}
