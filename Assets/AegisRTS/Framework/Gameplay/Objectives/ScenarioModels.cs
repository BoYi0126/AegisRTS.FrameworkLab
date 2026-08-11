using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AegisRTS.Core.Events;

namespace AegisRTS.Gameplay.Objectives
{
    public enum GameModeType
    {
        Conquest, Siege, Defense, Survival, Wave, Escort, TerritoryControl, HeroScenario,
    }

    public enum ObjectiveType
    {
        Capture, Hold, Destroy, Protect, Reach, Survive, Gather, Recruit, Defeat, Escort,
    }

    public enum ObjectiveStatus { Locked, Active, Completed, Failed }

    public enum ScenarioStatus { Idle, Running, Victory, Defeat }

    public enum TriggerConditionType
    {
        OnStart, ElapsedSecondsAtLeast, FactAtLeast, FactAtMost, ObjectiveCompleted, ObjectiveFailed,
    }

    public enum ScenarioActionType
    {
        ActivateObjective, CompleteObjective, FailObjective, AddFact, SetFact, EmitSignal, Victory, Defeat,
    }

    public sealed class GameModeDefinition
    {
        public GameModeDefinition(string id, string displayName, GameModeType type,
            IEnumerable<string> allowedSystems, IDictionary<string, bool> rules = null,
            bool allRequiredObjectivesForVictory = true, bool anyRequiredObjectiveFailureIsDefeat = true)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Game mode ID is required.", nameof(id));
            Id = id.Trim(); DisplayName = displayName ?? string.Empty; Type = type;
            AllowedSystems = CopyStrings(allowedSystems);
            Rules = new ReadOnlyDictionary<string, bool>(new Dictionary<string, bool>(
                rules ?? new Dictionary<string, bool>(), StringComparer.Ordinal));
            AllRequiredObjectivesForVictory = allRequiredObjectivesForVictory;
            AnyRequiredObjectiveFailureIsDefeat = anyRequiredObjectiveFailureIsDefeat;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public GameModeType Type { get; }
        public IReadOnlyList<string> AllowedSystems { get; }
        public IReadOnlyDictionary<string, bool> Rules { get; }
        public bool AllRequiredObjectivesForVictory { get; }
        public bool AnyRequiredObjectiveFailureIsDefeat { get; }

        public bool IsSystemAllowed(string systemId) =>
            !string.IsNullOrWhiteSpace(systemId) && Contains(AllowedSystems, systemId);

        private static bool Contains(IReadOnlyList<string> values, string value)
        {
            for (int index = 0; index < values.Count; index++)
                if (string.Equals(values[index], value, StringComparison.Ordinal)) return true;
            return false;
        }

        internal static IReadOnlyList<string> CopyStrings(IEnumerable<string> values)
        {
            var result = new List<string>();
            if (values != null)
                foreach (string value in values)
                    if (!string.IsNullOrWhiteSpace(value)) result.Add(value.Trim());
            return new ReadOnlyCollection<string>(result);
        }
    }

    public sealed class ObjectiveDefinition
    {
        public ObjectiveDefinition(string id, string displayName, ObjectiveType type, string factId,
            double targetValue, bool initiallyActive = true, bool optional = false,
            double requiredDurationSeconds = 0d, string failureFactId = null, double failureThreshold = 0d)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Objective ID is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(factId)) throw new ArgumentException("Objective fact ID is required.", nameof(factId));
            if (!Finite(targetValue) || targetValue < 0d) throw new ArgumentOutOfRangeException(nameof(targetValue));
            if (!Finite(requiredDurationSeconds) || requiredDurationSeconds < 0d) throw new ArgumentOutOfRangeException(nameof(requiredDurationSeconds));
            if (!Finite(failureThreshold) || failureThreshold < 0d) throw new ArgumentOutOfRangeException(nameof(failureThreshold));
            Id = id.Trim(); DisplayName = displayName ?? string.Empty; Type = type; FactId = factId.Trim();
            TargetValue = targetValue; InitiallyActive = initiallyActive; Optional = optional;
            RequiredDurationSeconds = requiredDurationSeconds;
            FailureFactId = string.IsNullOrWhiteSpace(failureFactId) ? string.Empty : failureFactId.Trim();
            FailureThreshold = failureThreshold;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public ObjectiveType Type { get; }
        public string FactId { get; }
        public double TargetValue { get; }
        public bool InitiallyActive { get; }
        public bool Optional { get; }
        public double RequiredDurationSeconds { get; }
        public string FailureFactId { get; }
        public double FailureThreshold { get; }
        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }

    public sealed class ScenarioActionDefinition
    {
        public ScenarioActionDefinition(ScenarioActionType type, string targetId = null, double value = 0d)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value));
            Type = type; TargetId = targetId?.Trim() ?? string.Empty; Value = value;
        }
        public ScenarioActionType Type { get; }
        public string TargetId { get; }
        public double Value { get; }
    }

    public sealed class TriggerDefinition
    {
        public TriggerDefinition(string id, TriggerConditionType conditionType, string subjectId,
            double threshold, IEnumerable<ScenarioActionDefinition> actions, bool repeatable = false)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Trigger ID is required.", nameof(id));
            if (double.IsNaN(threshold) || double.IsInfinity(threshold)) throw new ArgumentOutOfRangeException(nameof(threshold));
            Id = id.Trim(); ConditionType = conditionType; SubjectId = subjectId?.Trim() ?? string.Empty;
            Threshold = threshold; Repeatable = repeatable;
            Actions = new ReadOnlyCollection<ScenarioActionDefinition>(new List<ScenarioActionDefinition>(
                actions ?? Array.Empty<ScenarioActionDefinition>()));
        }
        public string Id { get; }
        public TriggerConditionType ConditionType { get; }
        public string SubjectId { get; }
        public double Threshold { get; }
        public bool Repeatable { get; }
        public IReadOnlyList<ScenarioActionDefinition> Actions { get; }
    }

    public sealed class ScenarioDefinition
    {
        public ScenarioDefinition(string id, string displayName, GameModeDefinition gameMode,
            IEnumerable<ObjectiveDefinition> objectives, IEnumerable<TriggerDefinition> triggers = null,
            IEnumerable<ScenarioActionDefinition> startSetup = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Scenario ID is required.", nameof(id));
            GameMode = gameMode ?? throw new ArgumentNullException(nameof(gameMode));
            Id = id.Trim(); DisplayName = displayName ?? string.Empty;
            Objectives = Copy(objectives); Triggers = Copy(triggers); StartSetup = Copy(startSetup);
            ValidateUniqueIds();
        }
        public string Id { get; }
        public string DisplayName { get; }
        public GameModeDefinition GameMode { get; }
        public IReadOnlyList<ObjectiveDefinition> Objectives { get; }
        public IReadOnlyList<TriggerDefinition> Triggers { get; }
        public IReadOnlyList<ScenarioActionDefinition> StartSetup { get; }

        private void ValidateUniqueIds()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (ObjectiveDefinition objective in Objectives)
                if (objective == null || !ids.Add(objective.Id)) throw new ArgumentException("Objective IDs must be unique.");
            ids.Clear();
            foreach (TriggerDefinition trigger in Triggers)
                if (trigger == null || !ids.Add(trigger.Id)) throw new ArgumentException("Trigger IDs must be unique.");
        }

        private static IReadOnlyList<T> Copy<T>(IEnumerable<T> values) =>
            new ReadOnlyCollection<T>(new List<T>(values ?? Array.Empty<T>()));
    }

    public sealed class ObjectiveSnapshot
    {
        public ObjectiveSnapshot(ObjectiveDefinition definition, ObjectiveStatus status, double value, double heldSeconds)
        { Definition = definition; Status = status; Value = value; HeldSeconds = heldSeconds; }
        public ObjectiveDefinition Definition { get; }
        public ObjectiveStatus Status { get; }
        public double Value { get; }
        public double HeldSeconds { get; }
    }

    public sealed class ScenarioSnapshot
    {
        public ScenarioSnapshot(ScenarioDefinition definition, ScenarioStatus status, double elapsedSeconds,
            IEnumerable<ObjectiveSnapshot> objectives, IDictionary<string, double> facts)
        {
            Definition = definition; Status = status; ElapsedSeconds = elapsedSeconds;
            Objectives = new ReadOnlyCollection<ObjectiveSnapshot>(new List<ObjectiveSnapshot>(objectives));
            Facts = new ReadOnlyDictionary<string, double>(new Dictionary<string, double>(facts, StringComparer.Ordinal));
        }
        public ScenarioDefinition Definition { get; }
        public ScenarioStatus Status { get; }
        public double ElapsedSeconds { get; }
        public IReadOnlyList<ObjectiveSnapshot> Objectives { get; }
        public IReadOnlyDictionary<string, double> Facts { get; }
    }

    public sealed class ScenarioStartedEvent : IEvent
    { public ScenarioStartedEvent(string scenarioId, GameModeType mode) { ScenarioId = scenarioId; Mode = mode; } public string ScenarioId { get; } public GameModeType Mode { get; } }
    public sealed class ObjectiveStatusChangedEvent : IEvent
    { public ObjectiveStatusChangedEvent(string scenarioId, string objectiveId, ObjectiveStatus status) { ScenarioId = scenarioId; ObjectiveId = objectiveId; Status = status; } public string ScenarioId { get; } public string ObjectiveId { get; } public ObjectiveStatus Status { get; } }
    public sealed class ScenarioCompletedEvent : IEvent
    { public ScenarioCompletedEvent(string scenarioId, ScenarioStatus status) { ScenarioId = scenarioId; Status = status; } public string ScenarioId { get; } public ScenarioStatus Status { get; } }
    public sealed class ScenarioActionExecutedEvent : IEvent
    { public ScenarioActionExecutedEvent(string scenarioId, ScenarioActionType type, string targetId) { ScenarioId = scenarioId; Type = type; TargetId = targetId; } public string ScenarioId { get; } public ScenarioActionType Type { get; } public string TargetId { get; } }
}
