using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;

namespace AegisRTS.Gameplay.AI
{
    /// <summary>Deterministic, interval-driven Utility AI coordinator with deadlock recovery.</summary>
    public sealed class AiSystem
    {
        private readonly Dictionary<EntityId, Agent> _agents = new Dictionary<EntityId, Agent>();
        private readonly UtilityAiPlanner _planner;
        private readonly EventBus _events;

        public AiSystem(UtilityAiPlanner planner = null, EventBus eventBus = null)
        { _planner = planner ?? new UtilityAiPlanner(); _events = eventBus; }

        public int AgentCount => _agents.Count;

        public void Register(EntityId factionId, AiProfile profile, IAiWorldQuery world, IAiActionExecutor executor)
        {
            if (!factionId.IsValid) throw new ArgumentException("Faction ID must be valid.", nameof(factionId));
            if (profile == null || world == null || executor == null) throw new ArgumentNullException(nameof(profile));
            if (_agents.ContainsKey(factionId)) throw new InvalidOperationException($"AI faction {factionId} is already registered.");
            _agents.Add(factionId, new Agent(factionId, profile, world, executor));
        }

        public bool Unregister(EntityId factionId) => _agents.Remove(factionId);

        public void Tick(double deltaSeconds)
        {
            if (deltaSeconds < 0d || double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds)) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            foreach (Agent agent in _agents.Values)
            {
                agent.DecisionRemaining -= deltaSeconds;
                if (agent.DecisionRemaining > 0d) continue;
                agent.DecisionRemaining = agent.Profile.DecisionIntervalSeconds;
                Decide(agent);
            }
        }

        public bool TryGetState(EntityId factionId, out AiAgentSnapshot snapshot)
        { if (!_agents.TryGetValue(factionId, out Agent agent)) { snapshot = default; return false; } snapshot = CreateSnapshot(agent); return true; }

        public IReadOnlyList<AiAgentSnapshot> Snapshot()
        { var result = new List<AiAgentSnapshot>(); foreach (Agent agent in _agents.Values) result.Add(CreateSnapshot(agent)); result.Sort((a, b) => a.FactionId.CompareTo(b.FactionId)); return result.AsReadOnly(); }

        public string GetDebugSummary()
        { int decisions = 0, stalled = 0; foreach (Agent agent in _agents.Values) { decisions += agent.DecisionCount; stalled += agent.StalledDecisionCount; } return $"Agents={_agents.Count}, Decisions={decisions}, Stalled={stalled}"; }

        public bool TryCaptureRuntimeState(EntityId factionId, out AiRuntimeStateSnapshot snapshot)
        {
            if (!_agents.TryGetValue(factionId, out Agent agent)) { snapshot = default; return false; }
            snapshot = new AiRuntimeStateSnapshot(agent.DecisionRemaining, agent.DecisionCount,
                agent.StalledDecisionCount, agent.Goal, agent.Layer, agent.Action, agent.LastError);
            return true;
        }

        public bool RestoreRuntimeState(EntityId factionId, AiRuntimeStateSnapshot snapshot)
        {
            if (!_agents.TryGetValue(factionId, out Agent agent)) return false;
            if (snapshot.DecisionRemaining < 0d || snapshot.DecisionRemaining > agent.Profile.DecisionIntervalSeconds ||
                double.IsNaN(snapshot.DecisionRemaining) || double.IsInfinity(snapshot.DecisionRemaining) ||
                snapshot.DecisionCount < 0 || snapshot.StalledDecisionCount < 0)
                throw new ArgumentOutOfRangeException(nameof(snapshot));
            agent.DecisionRemaining = snapshot.DecisionRemaining;
            agent.DecisionCount = snapshot.DecisionCount;
            agent.StalledDecisionCount = snapshot.StalledDecisionCount;
            agent.Goal = snapshot.Goal;
            agent.Layer = snapshot.Layer;
            agent.Action = snapshot.Action;
            agent.LastError = snapshot.LastError;
            return true;
        }

        private void Decide(Agent agent)
        {
            AiWorldSnapshot world = agent.World.Observe(agent.FactionId);
            IReadOnlyList<AiActionScore> scores = _planner.Score(agent.Profile, world, agent.StalledDecisionCount);
            AiActionScore selected = scores[0];
            AiActionResult result = agent.Executor.Execute(agent.FactionId, selected.Action, world);
            agent.WorldState = world; agent.Scores = scores; agent.Goal = selected.Goal; agent.Layer = selected.Layer;
            agent.Action = selected.Action; agent.DecisionCount++; agent.LastError = result.Error;
            if (result.MadeProgress) agent.StalledDecisionCount = 0; else agent.StalledDecisionCount++;
            _events?.Publish(new AiDecisionMadeEvent(agent.FactionId, selected.Goal, selected.Layer,
                selected.Action, selected.Score, result.MadeProgress));
        }

        private static AiAgentSnapshot CreateSnapshot(Agent agent) => new AiAgentSnapshot(
            agent.FactionId, agent.Profile, agent.Goal, agent.Layer, agent.Action, agent.Scores,
            agent.WorldState.TargetSettlementId, agent.WorldState.Strength, agent.WorldState.Threat,
            agent.WorldState.Route, agent.DecisionCount, agent.StalledDecisionCount, agent.LastError);

        private sealed class Agent
        {
            public Agent(EntityId id, AiProfile profile, IAiWorldQuery world, IAiActionExecutor executor)
            { FactionId = id; Profile = profile; World = world; Executor = executor; DecisionRemaining = 0d;
                Scores = Array.Empty<AiActionScore>(); Goal = AiStrategicGoal.Economy; Action = AiActionType.Wait; }
            public EntityId FactionId { get; }
            public AiProfile Profile { get; }
            public IAiWorldQuery World { get; }
            public IAiActionExecutor Executor { get; }
            public double DecisionRemaining { get; set; }
            public AiWorldSnapshot WorldState { get; set; }
            public IReadOnlyList<AiActionScore> Scores { get; set; }
            public AiStrategicGoal Goal { get; set; }
            public AiDecisionLayer Layer { get; set; }
            public AiActionType Action { get; set; }
            public int DecisionCount { get; set; }
            public int StalledDecisionCount { get; set; }
            public string LastError { get; set; } = string.Empty;
        }
    }
}
