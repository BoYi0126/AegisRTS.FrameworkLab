using System;
using System.Collections.Generic;
using System.Linq;
using AegisRTS.Core.Events;

namespace AegisRTS.Gameplay.Objectives
{
    /// <summary>Runs one data-authored scenario without depending on concrete gameplay systems.</summary>
    public sealed class ScenarioSystem
    {
        public const string ElapsedSecondsFact = "scenario.elapsed-seconds";
        private readonly EventBus _events;
        private readonly Dictionary<string, ObjectiveRuntime> _objectives =
            new Dictionary<string, ObjectiveRuntime>(StringComparer.Ordinal);
        private readonly Dictionary<string, double> _facts =
            new Dictionary<string, double>(StringComparer.Ordinal);
        private readonly HashSet<string> _firedTriggers = new HashSet<string>(StringComparer.Ordinal);
        private ScenarioDefinition _definition;
        private ScenarioStatus _status = ScenarioStatus.Idle;
        private double _elapsedSeconds;

        public ScenarioSystem(EventBus eventBus = null) { _events = eventBus ?? new EventBus(); }

        public ScenarioStatus Status => _status;
        public ScenarioDefinition Definition => _definition;

        public void Start(ScenarioDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (_status == ScenarioStatus.Running) throw new InvalidOperationException("A scenario is already running.");
            _definition = definition; _status = ScenarioStatus.Running; _elapsedSeconds = 0d;
            _objectives.Clear(); _facts.Clear(); _firedTriggers.Clear();
            _facts[ElapsedSecondsFact] = 0d;
            foreach (ObjectiveDefinition item in definition.Objectives)
            {
                var runtime = new ObjectiveRuntime(item,
                    item.InitiallyActive ? ObjectiveStatus.Active : ObjectiveStatus.Locked);
                _objectives.Add(item.Id, runtime);
            }
            _events.Publish(new ScenarioStartedEvent(definition.Id, definition.GameMode.Type));
            foreach (ScenarioActionDefinition action in definition.StartSetup) Execute(action);
            Pump(0d);
        }

        public void Update(double deltaSeconds)
        {
            if (double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds) || deltaSeconds < 0d)
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (_status != ScenarioStatus.Running) return;
            _elapsedSeconds += deltaSeconds; _facts[ElapsedSecondsFact] = _elapsedSeconds;
            Pump(deltaSeconds);
        }

        public bool AddFact(string factId, double amount)
        {
            ValidateFact(factId, amount);
            if (_status != ScenarioStatus.Running) return false;
            _facts.TryGetValue(factId, out double current);
            _facts[factId] = current + amount; Pump(0d); return true;
        }

        public bool SetFact(string factId, double value)
        {
            ValidateFact(factId, value);
            if (_status != ScenarioStatus.Running) return false;
            _facts[factId] = value; Pump(0d); return true;
        }

        public double GetFact(string factId) =>
            !string.IsNullOrWhiteSpace(factId) && _facts.TryGetValue(factId, out double value) ? value : 0d;

        public bool IsSystemAllowed(string systemId) =>
            _definition != null && _definition.GameMode.IsSystemAllowed(systemId);

        public bool TryGetSnapshot(out ScenarioSnapshot snapshot)
        {
            if (_definition == null) { snapshot = null; return false; }
            snapshot = new ScenarioSnapshot(_definition, _status, _elapsedSeconds,
                _objectives.Values.Select(item => new ObjectiveSnapshot(
                    item.Definition, item.Status, GetFact(item.Definition.FactId), item.HeldSeconds)), _facts);
            return true;
        }

        public string GetDebugSummary()
        {
            if (!TryGetSnapshot(out ScenarioSnapshot snapshot)) return "Scenario=None, Status=Idle";
            string objectives = string.Join(", ", snapshot.Objectives.Select(item =>
                $"{item.Definition.Id}:{item.Status}({item.Value:0.##}/{item.Definition.TargetValue:0.##})"));
            return $"Scenario={snapshot.Definition.Id}, Mode={snapshot.Definition.GameMode.Type}, Status={snapshot.Status}, Time={snapshot.ElapsedSeconds:0.##}, Objectives=[{objectives}]";
        }

        private void Pump(double deltaSeconds)
        {
            var firedThisPump = new HashSet<string>(StringComparer.Ordinal);
            for (int pass = 0; pass < 16 && _status == ScenarioStatus.Running; pass++)
            {
                bool changed = EvaluateTriggers(firedThisPump);
                changed |= EvaluateObjectives(pass == 0 ? deltaSeconds : 0d);
                changed |= EvaluateAutomaticResult();
                if (!changed) return;
            }
            if (_status == ScenarioStatus.Running && EvaluateTriggers(firedThisPump))
                throw new InvalidOperationException("Scenario trigger/action cascade exceeded the safety limit.");
        }

        private bool EvaluateTriggers(ISet<string> firedThisPump)
        {
            bool changed = false;
            foreach (TriggerDefinition trigger in _definition.Triggers)
            {
                if ((!trigger.Repeatable && _firedTriggers.Contains(trigger.Id)) || firedThisPump.Contains(trigger.Id)) continue;
                if (!IsTriggered(trigger)) continue;
                firedThisPump.Add(trigger.Id); if (!trigger.Repeatable) _firedTriggers.Add(trigger.Id);
                foreach (ScenarioActionDefinition action in trigger.Actions) changed |= Execute(action);
            }
            return changed;
        }

        private bool IsTriggered(TriggerDefinition trigger)
        {
            switch (trigger.ConditionType)
            {
                case TriggerConditionType.OnStart: return _elapsedSeconds == 0d;
                case TriggerConditionType.ElapsedSecondsAtLeast: return _elapsedSeconds >= trigger.Threshold;
                case TriggerConditionType.FactAtLeast: return GetFact(trigger.SubjectId) >= trigger.Threshold;
                case TriggerConditionType.FactAtMost: return GetFact(trigger.SubjectId) <= trigger.Threshold;
                case TriggerConditionType.ObjectiveCompleted:
                    return HasObjectiveStatus(trigger.SubjectId, ObjectiveStatus.Completed);
                case TriggerConditionType.ObjectiveFailed:
                    return HasObjectiveStatus(trigger.SubjectId, ObjectiveStatus.Failed);
                default: return false;
            }
        }

        private bool EvaluateObjectives(double deltaSeconds)
        {
            bool changed = false;
            foreach (ObjectiveRuntime runtime in _objectives.Values)
            {
                if (runtime.Status != ObjectiveStatus.Active) continue;
                ObjectiveDefinition objective = runtime.Definition;
                if (!string.IsNullOrEmpty(objective.FailureFactId) &&
                    GetFact(objective.FailureFactId) >= objective.FailureThreshold)
                {
                    changed |= SetObjectiveStatus(runtime, ObjectiveStatus.Failed); continue;
                }
                if (GetFact(objective.FactId) < objective.TargetValue)
                {
                    if (runtime.HeldSeconds > 0d) { runtime.HeldSeconds = 0d; changed = true; }
                    continue;
                }
                if (objective.RequiredDurationSeconds > 0d)
                {
                    double previous = runtime.HeldSeconds;
                    runtime.HeldSeconds = Math.Min(objective.RequiredDurationSeconds, runtime.HeldSeconds + deltaSeconds);
                    changed |= runtime.HeldSeconds != previous;
                    if (runtime.HeldSeconds < objective.RequiredDurationSeconds) continue;
                }
                changed |= SetObjectiveStatus(runtime, ObjectiveStatus.Completed);
            }
            return changed;
        }

        private bool EvaluateAutomaticResult()
        {
            ObjectiveRuntime[] required = _objectives.Values.Where(item => !item.Definition.Optional).ToArray();
            if (_definition.GameMode.AnyRequiredObjectiveFailureIsDefeat &&
                required.Any(item => item.Status == ObjectiveStatus.Failed))
                return Complete(ScenarioStatus.Defeat);
            if (_definition.GameMode.AllRequiredObjectivesForVictory && required.Length > 0 &&
                required.All(item => item.Status == ObjectiveStatus.Completed))
                return Complete(ScenarioStatus.Victory);
            return false;
        }

        private bool Execute(ScenarioActionDefinition action)
        {
            bool changed;
            switch (action.Type)
            {
                case ScenarioActionType.ActivateObjective: changed = ChangeObjective(action.TargetId, ObjectiveStatus.Active); break;
                case ScenarioActionType.CompleteObjective: changed = ChangeObjective(action.TargetId, ObjectiveStatus.Completed); break;
                case ScenarioActionType.FailObjective: changed = ChangeObjective(action.TargetId, ObjectiveStatus.Failed); break;
                case ScenarioActionType.AddFact:
                    _facts.TryGetValue(action.TargetId, out double current); _facts[action.TargetId] = current + action.Value; changed = true; break;
                case ScenarioActionType.SetFact: _facts[action.TargetId] = action.Value; changed = true; break;
                case ScenarioActionType.EmitSignal: changed = true; break;
                case ScenarioActionType.Victory: changed = Complete(ScenarioStatus.Victory); break;
                case ScenarioActionType.Defeat: changed = Complete(ScenarioStatus.Defeat); break;
                default: changed = false; break;
            }
            if (changed) _events.Publish(new ScenarioActionExecutedEvent(_definition.Id, action.Type, action.TargetId));
            return changed;
        }

        private bool ChangeObjective(string objectiveId, ObjectiveStatus status)
        {
            if (!_objectives.TryGetValue(objectiveId ?? string.Empty, out ObjectiveRuntime runtime))
                throw new InvalidOperationException($"Scenario action references unknown objective '{objectiveId}'.");
            return SetObjectiveStatus(runtime, status);
        }

        private bool SetObjectiveStatus(ObjectiveRuntime runtime, ObjectiveStatus status)
        {
            if (runtime.Status == status || runtime.Status == ObjectiveStatus.Completed || runtime.Status == ObjectiveStatus.Failed) return false;
            runtime.Status = status;
            _events.Publish(new ObjectiveStatusChangedEvent(_definition.Id, runtime.Definition.Id, status));
            return true;
        }

        private bool Complete(ScenarioStatus status)
        {
            if (_status != ScenarioStatus.Running) return false;
            _status = status; _events.Publish(new ScenarioCompletedEvent(_definition.Id, status)); return true;
        }

        private bool HasObjectiveStatus(string id, ObjectiveStatus status) =>
            _objectives.TryGetValue(id ?? string.Empty, out ObjectiveRuntime runtime) && runtime.Status == status;

        private static void ValidateFact(string factId, double value)
        {
            if (string.IsNullOrWhiteSpace(factId)) throw new ArgumentException("Fact ID is required.", nameof(factId));
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value));
        }

        private sealed class ObjectiveRuntime
        {
            public ObjectiveRuntime(ObjectiveDefinition definition, ObjectiveStatus status) { Definition = definition; Status = status; }
            public ObjectiveDefinition Definition { get; }
            public ObjectiveStatus Status { get; set; }
            public double HeldSeconds { get; set; }
        }
    }
}
