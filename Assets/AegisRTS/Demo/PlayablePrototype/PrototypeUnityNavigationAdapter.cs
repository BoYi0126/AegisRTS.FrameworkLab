using System;
using System.Collections.Generic;
using AegisRTS.Gameplay.Movement;
using AegisRTS.Gameplay.Units;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Demo.PlayablePrototype
{
    /// <summary>
    /// Unity product adapter. NavMeshAgent transforms provide navigation feedback while gameplay reads positions
    /// exclusively through INavigationAdapter snapshots.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeUnityNavigationAdapter : MonoBehaviour, IPrototypeNavigationRuntime
    {
        private const float SampleRadius = 3f;
        private readonly Dictionary<EntityId, AgentEntry> _agents = new Dictionary<EntityId, AgentEntry>();
        private NavMeshSurface _surface;

        public bool UsesUnityNavMesh => true;
        public bool IsReady => _surface != null && _surface.navMeshData != null;
        public int RegisteredAgentCount => _agents.Count;

        public void Initialize(NavMeshSurface surface)
        {
            _surface = surface != null ? surface : throw new ArgumentNullException(nameof(surface));
            if (_surface.navMeshData == null) throw new InvalidOperationException("Prototype NavMesh surface has not been built.");
        }

        public void Register(EntityId entityId, WorldPoint position, double speed)
        {
            if (!entityId.IsValid) throw new ArgumentException("Entity ID must be valid.", nameof(entityId));
            if (speed <= 0d || double.IsNaN(speed) || double.IsInfinity(speed)) throw new ArgumentOutOfRangeException(nameof(speed));
            if (_agents.ContainsKey(entityId)) throw new InvalidOperationException($"Navigation entity {entityId} is already registered.");
            if (!IsReady) throw new InvalidOperationException("Prototype NavMesh is not ready.");
            if (!NavMesh.SamplePosition(ToVector3(position), out NavMeshHit hit, SampleRadius, NavMesh.AllAreas))
                throw new InvalidOperationException($"No NavMesh position exists near {position} for entity {entityId}.");

            var root = new GameObject($"NavAgent_{entityId.Value}");
            root.transform.SetParent(transform, false);
            root.transform.position = hit.position;
            NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            agent.radius = 0.38f;
            agent.height = 2f;
            agent.speed = Mathf.Max(0.1f, (float)speed);
            agent.acceleration = 18f;
            agent.angularSpeed = 720f;
            agent.stoppingDistance = 0.2f;
            agent.autoBraking = true;
            agent.autoRepath = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = 25 + (int)(entityId.Value % 50UL);
            agent.enabled = true;
            if (!agent.Warp(hit.position))
            {
                DestroyAgent(root);
                throw new InvalidOperationException($"NavMeshAgent could not warp entity {entityId} onto the NavMesh.");
            }
            _agents.Add(entityId, new AgentEntry(root, agent));
        }

        public bool Unregister(EntityId entityId)
        {
            if (!_agents.TryGetValue(entityId, out AgentEntry entry)) return false;
            _agents.Remove(entityId);
            DestroyAgent(entry.Root);
            return true;
        }

        public bool SetPosition(EntityId entityId, WorldPoint position)
        {
            if (!_agents.TryGetValue(entityId, out AgentEntry entry) || entry.Agent == null || !entry.Agent.enabled) return false;
            if (!NavMesh.SamplePosition(ToVector3(position), out NavMeshHit hit, SampleRadius, NavMesh.AllAreas)) return false;
            if (entry.Agent.isOnNavMesh) entry.Agent.ResetPath();
            entry.HasDestination = false;
            entry.PathCorners = Array.Empty<Vector3>();
            return entry.Agent.Warp(hit.position);
        }

        public NavigationDestinationResult SetDestination(EntityId entityId, WorldPoint destination, int formationSlotIndex)
        {
            if (!_agents.TryGetValue(entityId, out AgentEntry entry) || entry.Agent == null || !entry.Agent.enabled)
                return NavigationDestinationResult.Failure($"Navigation agent {entityId} is not registered.");
            if (!entry.Agent.isOnNavMesh)
                return NavigationDestinationResult.Failure($"Navigation agent {entityId} is not on the NavMesh.");
            if (!NavMesh.SamplePosition(ToVector3(destination), out NavMeshHit hit, SampleRadius, NavMesh.AllAreas))
                return NavigationDestinationResult.Failure($"No NavMesh position exists near {destination}.");

            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(entry.Agent.nextPosition, hit.position, NavMesh.AllAreas, path) ||
                path.status != NavMeshPathStatus.PathComplete)
                return NavigationDestinationResult.Failure($"No complete path exists from entity {entityId} to {destination}.");
            if (!entry.Agent.SetPath(path))
                return NavigationDestinationResult.Failure($"NavMeshAgent rejected the path for entity {entityId}.");

            entry.Destination = hit.position;
            entry.FormationSlotIndex = formationSlotIndex;
            entry.PathCorners = path.corners;
            entry.HasDestination = true;
            return NavigationDestinationResult.Success(ToWorldPoint(hit.position), path.corners.Length);
        }

        public void Stop(EntityId entityId)
        {
            if (!_agents.TryGetValue(entityId, out AgentEntry entry) || entry.Agent == null || !entry.Agent.enabled) return;
            if (entry.Agent.isOnNavMesh) entry.Agent.ResetPath();
            entry.HasDestination = false;
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
            bool onNavMesh = agent.isOnNavMesh;
            double remaining = onNavMesh && !float.IsInfinity(agent.remainingDistance)
                ? Math.Max(0d, agent.remainingDistance)
                : double.PositiveInfinity;
            NavigationPathState state = NavigationPathState.None;
            if (agent.pathPending) state = NavigationPathState.Pending;
            else if (agent.hasPath)
            {
                if (agent.pathStatus == NavMeshPathStatus.PathComplete) state = NavigationPathState.Complete;
                else if (agent.pathStatus == NavMeshPathStatus.PathPartial) state = NavigationPathState.Partial;
                else state = NavigationPathState.Invalid;
            }
            snapshot = new NavigationAgentSnapshot(ToWorldPoint(agent.transform.position), ToWorldPoint(agent.velocity), remaining, state, onNavMesh);
            return true;
        }

        public void Tick(double deltaSeconds)
        {
            if (deltaSeconds < 0d || double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds))
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            // NavMeshAgent advances in Unity's player loop. MovementSystem pulls its snapshot during the fixed composition tick.
        }

        public void RefreshAfterWorldChange(Action rebuildSurface)
        {
            if (rebuildSurface == null) throw new ArgumentNullException(nameof(rebuildSurface));
            var states = new List<RefreshState>(_agents.Count);
            foreach (KeyValuePair<EntityId, AgentEntry> pair in _agents)
            {
                AgentEntry entry = pair.Value;
                states.Add(new RefreshState(pair.Key, entry.Agent.transform.position, entry.Destination,
                    entry.FormationSlotIndex, entry.HasDestination));
                entry.Agent.enabled = false;
            }

            rebuildSurface();
            foreach (RefreshState state in states)
            {
                AgentEntry entry = _agents[state.EntityId];
                if (!NavMesh.SamplePosition(state.Position, out NavMeshHit hit, SampleRadius, NavMesh.AllAreas))
                    throw new InvalidOperationException($"Entity {state.EntityId} could not be rebound after NavMesh refresh.");
                entry.Agent.transform.position = hit.position;
                entry.Agent.enabled = true;
                if (!entry.Agent.Warp(hit.position))
                    throw new InvalidOperationException($"Entity {state.EntityId} could not warp after NavMesh refresh.");
                entry.HasDestination = false;
                if (state.HasDestination)
                    SetDestination(state.EntityId, ToWorldPoint(state.Destination), state.FormationSlotIndex);
            }
        }

        public string GetDebugSummary()
        {
            int active = 0;
            foreach (AgentEntry entry in _agents.Values)
                if (entry.Agent != null && entry.Agent.enabled && entry.Agent.hasPath) active++;
            return $"UnityNavMesh Agents={_agents.Count}, ActivePaths={active}, Ready={IsReady}";
        }

        private void OnDestroy() => _agents.Clear();

        private static void DestroyAgent(GameObject value)
        {
            if (value == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(value);
            else UnityEngine.Object.DestroyImmediate(value);
        }

        private static Vector3 ToVector3(WorldPoint point) => new Vector3((float)point.X, (float)point.Y, (float)point.Z);
        private static WorldPoint ToWorldPoint(Vector3 point) => new WorldPoint(point.x, point.y, point.z);

        private sealed class AgentEntry
        {
            public AgentEntry(GameObject root, NavMeshAgent agent)
            {
                Root = root;
                Agent = agent;
            }

            public GameObject Root { get; }
            public NavMeshAgent Agent { get; }
            public Vector3 Destination { get; set; }
            public int FormationSlotIndex { get; set; } = -1;
            public Vector3[] PathCorners { get; set; } = Array.Empty<Vector3>();
            public bool HasDestination { get; set; }
        }

        private readonly struct RefreshState
        {
            public RefreshState(EntityId entityId, Vector3 position, Vector3 destination, int formationSlotIndex, bool hasDestination)
            {
                EntityId = entityId;
                Position = position;
                Destination = destination;
                FormationSlotIndex = formationSlotIndex;
                HasDestination = hasDestination;
            }

            public EntityId EntityId { get; }
            public Vector3 Position { get; }
            public Vector3 Destination { get; }
            public int FormationSlotIndex { get; }
            public bool HasDestination { get; }
        }
    }
}
