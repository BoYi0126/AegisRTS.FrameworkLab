using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AegisRTS.Gameplay.Objectives
{
    /// <summary>Loads GameMode, start setup, objectives, triggers, and actions from JSON.</summary>
    public sealed class ScenarioJsonLoader
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true, PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip,
        };

        public ScenarioDefinition Load(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Scenario JSON is required.", nameof(json));
            try
            {
                ScenarioDocument document = JsonSerializer.Deserialize<ScenarioDocument>(json, Options);
                if (document == null || document.GameMode == null) throw new FormatException("Scenario and gameMode are required.");
                GameModeDocument mode = document.GameMode;
                var gameMode = new GameModeDefinition(mode.Id, mode.DisplayName,
                    Parse<GameModeType>(mode.Type, "game mode"), mode.AllowedSystems, mode.Rules,
                    mode.AllRequiredObjectivesForVictory, mode.AnyRequiredObjectiveFailureIsDefeat);
                var objectives = (document.Objectives ?? Array.Empty<ObjectiveDocument>()).Select(item =>
                    new ObjectiveDefinition(item.Id, item.DisplayName, Parse<ObjectiveType>(item.Type, "objective"),
                        item.FactId, item.TargetValue, item.InitiallyActive, item.Optional,
                        item.RequiredDurationSeconds, item.FailureFactId, item.FailureThreshold)).ToArray();
                var triggers = (document.Triggers ?? Array.Empty<TriggerDocument>()).Select(item =>
                    new TriggerDefinition(item.Id, Parse<TriggerConditionType>(item.Condition, "trigger condition"),
                        item.SubjectId, item.Threshold, Actions(item.Actions), item.Repeatable)).ToArray();
                var definition = new ScenarioDefinition(document.Id, document.DisplayName, gameMode,
                    objectives, triggers, Actions(document.StartSetup));
                ValidateReferences(definition);
                return definition;
            }
            catch (Exception exception) when (exception is JsonException || exception is ArgumentException ||
                                               exception is InvalidOperationException || exception is FormatException)
            {
                throw new FormatException("Scenario JSON is invalid.", exception);
            }
        }

        private static ScenarioActionDefinition[] Actions(ActionDocument[] documents) =>
            (documents ?? Array.Empty<ActionDocument>()).Select(item => new ScenarioActionDefinition(
                Parse<ScenarioActionType>(item.Type, "scenario action"), item.TargetId, item.Value)).ToArray();

        private static T Parse<T>(string value, string label) where T : struct
        {
            string normalized = (value ?? string.Empty).Replace("-", string.Empty).Replace("_", string.Empty).Replace(" ", string.Empty);
            foreach (string name in Enum.GetNames(typeof(T)))
                if (string.Equals(name, normalized, StringComparison.OrdinalIgnoreCase) && Enum.TryParse(name, out T result)) return result;
            throw new FormatException($"Unknown {label} '{value}'.");
        }

        private static void ValidateReferences(ScenarioDefinition definition)
        {
            var objectives = new HashSet<string>(definition.Objectives.Select(item => item.Id), StringComparer.Ordinal);
            foreach (ScenarioActionDefinition action in definition.StartSetup) ValidateAction(action, objectives);
            foreach (TriggerDefinition trigger in definition.Triggers)
            {
                if ((trigger.ConditionType == TriggerConditionType.ObjectiveCompleted || trigger.ConditionType == TriggerConditionType.ObjectiveFailed) &&
                    !objectives.Contains(trigger.SubjectId))
                    throw new FormatException($"Trigger '{trigger.Id}' references unknown objective '{trigger.SubjectId}'.");
                foreach (ScenarioActionDefinition action in trigger.Actions) ValidateAction(action, objectives);
            }
        }

        private static void ValidateAction(ScenarioActionDefinition action, ISet<string> objectives)
        {
            bool objectiveAction = action.Type == ScenarioActionType.ActivateObjective ||
                                   action.Type == ScenarioActionType.CompleteObjective ||
                                   action.Type == ScenarioActionType.FailObjective;
            bool factAction = action.Type == ScenarioActionType.AddFact || action.Type == ScenarioActionType.SetFact;
            if (objectiveAction && !objectives.Contains(action.TargetId))
                throw new FormatException($"Action references unknown objective '{action.TargetId}'.");
            if (factAction && string.IsNullOrWhiteSpace(action.TargetId))
                throw new FormatException("Fact action target ID is required.");
            if (action.Type == ScenarioActionType.EmitSignal && string.IsNullOrWhiteSpace(action.TargetId))
                throw new FormatException("Signal action target ID is required.");
        }

        public sealed class ScenarioDocument
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public GameModeDocument GameMode { get; set; }
            public ObjectiveDocument[] Objectives { get; set; }
            public TriggerDocument[] Triggers { get; set; }
            public ActionDocument[] StartSetup { get; set; }
        }
        public sealed class GameModeDocument
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string Type { get; set; }
            public string[] AllowedSystems { get; set; }
            public Dictionary<string, bool> Rules { get; set; }
            public bool AllRequiredObjectivesForVictory { get; set; } = true;
            public bool AnyRequiredObjectiveFailureIsDefeat { get; set; } = true;
        }
        public sealed class ObjectiveDocument
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string Type { get; set; }
            public string FactId { get; set; }
            public double TargetValue { get; set; }
            public bool InitiallyActive { get; set; } = true;
            public bool Optional { get; set; }
            public double RequiredDurationSeconds { get; set; }
            public string FailureFactId { get; set; }
            public double FailureThreshold { get; set; }
        }
        public sealed class TriggerDocument
        {
            public string Id { get; set; }
            public string Condition { get; set; }
            public string SubjectId { get; set; }
            public double Threshold { get; set; }
            public bool Repeatable { get; set; }
            public ActionDocument[] Actions { get; set; }
        }
        public sealed class ActionDocument
        {
            public string Type { get; set; }
            public string TargetId { get; set; }
            public double Value { get; set; }
        }
    }
}
