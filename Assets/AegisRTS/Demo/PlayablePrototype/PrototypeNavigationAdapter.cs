using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Movement;
using AegisRTS.Gameplay.Units;

namespace AegisRTS.Demo.PlayablePrototype
{
    /// <summary>
    /// Product navigation lifecycle used by the composition. Headless tests use the deterministic implementation;
    /// the Unity scene injects a NavMesh-backed implementation without changing gameplay commands.
    /// </summary>
    public interface IPrototypeNavigationRuntime : INavigationAdapter
    {
        bool UsesUnityNavMesh { get; }
        void Register(EntityId entityId, WorldPoint position, double speed);
        bool Unregister(EntityId entityId);
        bool SetPosition(EntityId entityId, WorldPoint position);
        void Tick(double deltaSeconds);
        string GetDebugSummary();
    }

    /// <summary>Deterministic navigation boundary used by the prototype and its long-run tests.</summary>
    public sealed class PrototypeNavigationAdapter : IPrototypeNavigationRuntime
    {
        private readonly Dictionary<EntityId, Agent> _agents = new Dictionary<EntityId, Agent>();

        public int AgentCount => _agents.Count;
        public bool UsesUnityNavMesh => false;

        public void Register(EntityId entityId, WorldPoint position, double speed)
        {
            if (!entityId.IsValid) throw new ArgumentException("Entity ID must be valid.", nameof(entityId));
            if (speed <= 0d || double.IsNaN(speed) || double.IsInfinity(speed)) throw new ArgumentOutOfRangeException(nameof(speed));
            if (_agents.ContainsKey(entityId)) throw new InvalidOperationException($"Navigation entity {entityId} is already registered.");
            _agents.Add(entityId, new Agent(position, speed));
        }

        public bool Unregister(EntityId entityId) => _agents.Remove(entityId);

        public bool SetPosition(EntityId entityId, WorldPoint position)
        {
            if (!_agents.TryGetValue(entityId, out Agent agent)) return false;
            agent.Position = position;
            agent.Destination = position;
            agent.Velocity = default;
            agent.Moving = false;
            return true;
        }

        public NavigationDestinationResult SetDestination(EntityId entityId, WorldPoint destination, int formationSlotIndex)
        {
            if (!_agents.TryGetValue(entityId, out Agent agent)) return NavigationDestinationResult.Failure("Navigation agent is not registered.");
            agent.Destination = destination;
            agent.Moving = true;
            return NavigationDestinationResult.Success(destination, 2);
        }

        public void Stop(EntityId entityId)
        {
            if (!_agents.TryGetValue(entityId, out Agent agent)) return;
            agent.Destination = agent.Position;
            agent.Velocity = default;
            agent.Moving = false;
        }

        public bool TryGetSnapshot(EntityId entityId, out NavigationAgentSnapshot snapshot)
        {
            if (!_agents.TryGetValue(entityId, out Agent agent))
            {
                snapshot = default;
                return false;
            }

            snapshot = new NavigationAgentSnapshot(
                agent.Position,
                agent.Velocity,
                Distance(agent.Position, agent.Destination),
                agent.Moving ? NavigationPathState.Complete : NavigationPathState.None,
                true);
            return true;
        }

        public void Tick(double deltaSeconds)
        {
            if (deltaSeconds < 0d || double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds))
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            foreach (Agent agent in _agents.Values)
            {
                if (!agent.Moving)
                {
                    agent.Velocity = default;
                    continue;
                }

                double distance = Distance(agent.Position, agent.Destination);
                if (distance <= 0.001d)
                {
                    agent.Position = agent.Destination;
                    agent.Velocity = default;
                    agent.Moving = false;
                    continue;
                }

                double step = Math.Min(distance, agent.Speed * deltaSeconds);
                double ratio = step / distance;
                WorldPoint previous = agent.Position;
                agent.Position = Lerp(agent.Position, agent.Destination, ratio);
                agent.Velocity = deltaSeconds <= 0d ? default : new WorldPoint(
                    (agent.Position.X - previous.X) / deltaSeconds,
                    (agent.Position.Y - previous.Y) / deltaSeconds,
                    (agent.Position.Z - previous.Z) / deltaSeconds);
                if (step >= distance)
                {
                    agent.Position = agent.Destination;
                    agent.Velocity = default;
                    agent.Moving = false;
                }
            }
        }

        public string GetDebugSummary()
        {
            int moving = 0;
            foreach (Agent agent in _agents.Values) if (agent.Moving) moving++;
            return $"Agents={_agents.Count}, Moving={moving}";
        }

        private static double Distance(WorldPoint left, WorldPoint right)
        {
            double x = right.X - left.X;
            double y = right.Y - left.Y;
            double z = right.Z - left.Z;
            return Math.Sqrt(x * x + y * y + z * z);
        }

        private static WorldPoint Lerp(WorldPoint from, WorldPoint to, double ratio) => new WorldPoint(
            from.X + (to.X - from.X) * ratio,
            from.Y + (to.Y - from.Y) * ratio,
            from.Z + (to.Z - from.Z) * ratio);

        private sealed class Agent
        {
            public Agent(WorldPoint position, double speed)
            {
                Position = position;
                Destination = position;
                Speed = speed;
            }

            public WorldPoint Position { get; set; }
            public WorldPoint Destination { get; set; }
            public WorldPoint Velocity { get; set; }
            public double Speed { get; }
            public bool Moving { get; set; }
        }
    }
}
