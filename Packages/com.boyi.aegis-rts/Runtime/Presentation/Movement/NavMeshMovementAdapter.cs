using System;
using System.Collections.Generic;
using AegisRTS.Gameplay.Movement;
using AegisRTS.Gameplay.Units;
using AegisRTS.Presentation.Selection;
using UnityEngine;
using UnityEngine.AI;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Presentation.Movement
{
    /// <summary>Validates destinations and maps movement orders to Unity NavMeshAgent instances.</summary>
    [DisallowMultipleComponent]
    public sealed class NavMeshMovementAdapter : MonoBehaviour, INavigationAdapter
    {
        [SerializeField, Min(0.1f)] private float destinationSampleRadius = 3f;
        [SerializeField, Min(0.1f)] private float agentSpeed = 7f;
        [SerializeField, Min(0.1f)] private float agentAcceleration = 18f;
        [SerializeField, Min(0.05f)] private float agentRadius = 0.38f;
        private readonly Dictionary<EntityId, AgentEntry> _agents = new Dictionary<EntityId, AgentEntry>();

        public int RegisteredAgentCount => _agents.Count;

        public bool Register(UnitySelectableView view, MovementSystem movement)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            if (movement == null) throw new ArgumentNullException(nameof(movement));
            if (_agents.ContainsKey(view.EntityId)) return false;

            NavMeshAgent agent = view.GetComponent<NavMeshAgent>();
            if (agent == null) agent = view.gameObject.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            agent.radius = agentRadius;
            agent.height = 2f;
            agent.speed = agentSpeed;
            agent.acceleration = agentAcceleration;
            agent.angularSpeed = 720f;
            agent.stoppingDistance = 0.2f;
            agent.autoBraking = true;
            agent.autoRepath = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = 25 + (int)(view.EntityId.Value % 50UL);

            if (!NavMesh.SamplePosition(view.transform.position, out NavMeshHit hit, destinationSampleRadius, NavMesh.AllAreas))
                return false;
            view.transform.position = hit.position;
            agent.enabled = true;
            if (!agent.Warp(hit.position))
            {
                agent.enabled = false;
                return false;
            }

            _agents.Add(view.EntityId, new AgentEntry(agent));
            movement.Register(view.EntityId, ToWorldPoint(hit.position));
            view.gameObject.AddComponent<UnityMovementAgentView>().Configure(view.EntityId, this, movement);
            return true;
        }

        public bool Unregister(EntityId entityId)
        {
            if (!_agents.Remove(entityId)) return false;
            return true;
        }

        public NavigationDestinationResult SetDestination(EntityId entityId, WorldPoint destination, int formationSlotIndex)
        {
            if (!_agents.TryGetValue(entityId, out AgentEntry entry) || entry.Agent == null || !entry.Agent.enabled)
                return NavigationDestinationResult.Failure($"Navigation agent {entityId} is not registered.");
            if (!entry.Agent.isOnNavMesh)
                return NavigationDestinationResult.Failure($"Navigation agent {entityId} is not on the NavMesh.");

            Vector3 requested = ToVector3(destination);
            if (!NavMesh.SamplePosition(requested, out NavMeshHit hit, destinationSampleRadius, NavMesh.AllAreas))
                return NavigationDestinationResult.Failure($"No NavMesh position near {destination}.");

            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(entry.Agent.nextPosition, hit.position, NavMesh.AllAreas, path) ||
                path.status != NavMeshPathStatus.PathComplete)
                return NavigationDestinationResult.Failure($"No complete path from entity {entityId} to {destination}.");
            if (!entry.Agent.SetPath(path))
                return NavigationDestinationResult.Failure($"NavMeshAgent rejected the path for entity {entityId}.");

            entry.Destination = hit.position;
            entry.FormationSlotIndex = formationSlotIndex;
            entry.PathCorners = path.corners;
            return NavigationDestinationResult.Success(ToWorldPoint(hit.position), entry.PathCorners.Length);
        }

        public void Stop(EntityId entityId)
        {
            if (!_agents.TryGetValue(entityId, out AgentEntry entry) || entry.Agent == null || !entry.Agent.enabled) return;
            if (entry.Agent.isOnNavMesh) entry.Agent.ResetPath();
            entry.PathCorners = Array.Empty<Vector3>();
        }

        public bool TryGetSnapshot(EntityId entityId, out NavigationAgentSnapshot snapshot)
        {
            if (!_agents.TryGetValue(entityId, out AgentEntry entry) || entry.Agent == null || !entry.Agent.enabled)
            {
                snapshot = default;
                return false;
            }

            NavMeshAgent agent = entry.Agent;
            bool isOnNavigation = agent.isOnNavMesh;
            double remaining = isOnNavigation && !float.IsInfinity(agent.remainingDistance)
                ? Math.Max(0d, agent.remainingDistance)
                : double.PositiveInfinity;
            NavigationPathState pathState = NavigationPathState.None;
            if (agent.pathPending) pathState = NavigationPathState.Pending;
            else if (agent.hasPath)
            {
                switch (agent.pathStatus)
                {
                    case NavMeshPathStatus.PathComplete: pathState = NavigationPathState.Complete; break;
                    case NavMeshPathStatus.PathPartial: pathState = NavigationPathState.Partial; break;
                    default: pathState = NavigationPathState.Invalid; break;
                }
            }

            snapshot = new NavigationAgentSnapshot(
                ToWorldPoint(agent.transform.position),
                ToWorldPoint(agent.velocity),
                remaining,
                pathState,
                isOnNavigation);
            return true;
        }

        public string GetDebugSummary()
        {
            int moving = 0;
            foreach (AgentEntry entry in _agents.Values)
            {
                if (entry.Agent != null && entry.Agent.enabled && entry.Agent.hasPath) moving++;
            }
            return $"Agents={_agents.Count}, ActivePaths={moving}";
        }

        private void OnDrawGizmos()
        {
            foreach (AgentEntry entry in _agents.Values)
            {
                if (entry.Agent == null) continue;
                Gizmos.color = Color.green;
                for (int index = 1; index < entry.PathCorners.Length; index++)
                    Gizmos.DrawLine(entry.PathCorners[index - 1], entry.PathCorners[index]);
                if (entry.FormationSlotIndex >= 0)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(entry.Destination + Vector3.up * 0.15f, 0.2f);
                }
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(entry.Agent.transform.position, entry.Agent.transform.position + entry.Agent.velocity * 0.35f);
            }
        }

        private static Vector3 ToVector3(WorldPoint point) => new Vector3((float)point.X, (float)point.Y, (float)point.Z);
        private static WorldPoint ToWorldPoint(Vector3 point) => new WorldPoint(point.x, point.y, point.z);

        private sealed class AgentEntry
        {
            public AgentEntry(NavMeshAgent agent) => Agent = agent;
            public NavMeshAgent Agent { get; }
            public Vector3 Destination { get; set; }
            public int FormationSlotIndex { get; set; } = -1;
            public Vector3[] PathCorners { get; set; } = Array.Empty<Vector3>();
        }
    }

    /// <summary>Unregisters one Unity movement view when its GameObject leaves the scene.</summary>
    [DisallowMultipleComponent]
    public sealed class UnityMovementAgentView : MonoBehaviour
    {
        private EntityId _entityId;
        private NavMeshMovementAdapter _adapter;
        private MovementSystem _movement;

        public void Configure(EntityId entityId, NavMeshMovementAdapter adapter, MovementSystem movement)
        {
            _entityId = entityId;
            _adapter = adapter;
            _movement = movement;
        }

        private void OnDestroy()
        {
            if (!_entityId.IsValid) return;
            _adapter?.Unregister(_entityId);
            _movement?.Unregister(_entityId);
        }
    }
}
