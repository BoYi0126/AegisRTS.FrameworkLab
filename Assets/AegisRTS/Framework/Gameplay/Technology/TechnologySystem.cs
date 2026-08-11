using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Economy;

namespace AegisRTS.Gameplay.Technology
{
    /// <summary>Validates technology DAG prerequisites, pays costs, advances research, and applies modifiers.</summary>
    public sealed class TechnologySystem : ITechnologyStatusQuery
    {
        private readonly Dictionary<DefinitionId, TechnologyDefinition> _definitions = new Dictionary<DefinitionId, TechnologyDefinition>();
        private readonly Dictionary<EntityId, HashSet<DefinitionId>> _researched = new Dictionary<EntityId, HashSet<DefinitionId>>();
        private readonly List<Job> _jobs = new List<Job>();
        private readonly EconomySystem _economy;
        private readonly TechnologyModifierRegistry _modifiers;
        private readonly ITechnologyCompletionSink _sink;
        private readonly EventBus _events;

        public TechnologySystem(IEnumerable<TechnologyDefinition> definitions, EconomySystem economy,
            TechnologyModifierRegistry modifiers = null, ITechnologyCompletionSink sink = null, EventBus eventBus = null)
        {
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _modifiers = modifiers ?? new TechnologyModifierRegistry(); _sink = sink; _events = eventBus;
            foreach (TechnologyDefinition definition in definitions ?? Array.Empty<TechnologyDefinition>())
                if (definition != null) _definitions.Add(definition.Id, definition);
        }

        public int QueuedCount => _jobs.Count;

        public TechnologyRequestResult Validate(ResearchTechnologyCommand command)
        {
            if (command == null) return TechnologyRequestResult.Failure("Command is required.");
            if (!_economy.ContainsAccount(command.AccountId)) return TechnologyRequestResult.Failure("Research economy account does not exist.");
            if (!_definitions.TryGetValue(command.TechnologyId, out TechnologyDefinition definition)) return TechnologyRequestResult.Failure("Technology definition does not exist.");
            if (IsResearched(command.FactionId, command.TechnologyId) || IsQueued(command.FactionId, command.TechnologyId)) return TechnologyRequestResult.Failure("Technology is already researched or queued.");
            foreach (DefinitionId prerequisite in definition.PrerequisiteIds)
                if (!IsResearched(command.FactionId, prerequisite)) return TechnologyRequestResult.Failure($"Missing technology prerequisite '{prerequisite}'.");
            if (!_economy.CanAfford(command.AccountId, definition.Costs)) return TechnologyRequestResult.Failure("Insufficient resources.");
            return TechnologyRequestResult.Success();
        }

        public TechnologyRequestResult Request(ResearchTechnologyCommand command)
        {
            TechnologyRequestResult validation = Validate(command); if (!validation.Succeeded) return validation;
            TechnologyDefinition definition = _definitions[command.TechnologyId];
            if (!_economy.TrySpend(command.AccountId, definition.Costs)) return TechnologyRequestResult.Failure("Resources changed before payment completed.");
            _jobs.Add(new Job(command.FactionId, definition)); return TechnologyRequestResult.Success();
        }

        public void Tick(double deltaSeconds)
        {
            ValidateDelta(deltaSeconds);
            for (int index = _jobs.Count - 1; index >= 0; index--)
            {
                Job job = _jobs[index]; job.RemainingSeconds -= deltaSeconds;
                if (job.RemainingSeconds > 0d) continue;
                GetSet(job.FactionId).Add(job.Definition.Id);
                _modifiers.Apply(job.FactionId, job.Definition.Modifiers);
                _sink?.TechnologyCompleted(job.FactionId, job.Definition.Id);
                _events?.Publish(new TechnologyCompletedEvent(job.FactionId, job.Definition.Id));
                _jobs.RemoveAt(index);
            }
        }

        public bool IsResearched(EntityId factionId, DefinitionId technologyId) =>
            _researched.TryGetValue(factionId, out HashSet<DefinitionId> values) && values.Contains(technologyId);

        public string GetDebugSummary() => $"Technologies={_definitions.Count}, ResearchQueued={_jobs.Count}";

        private bool IsQueued(EntityId factionId, DefinitionId technologyId) =>
            _jobs.Exists(job => job.FactionId == factionId && job.Definition.Id == technologyId);
        private HashSet<DefinitionId> GetSet(EntityId id)
        { if (!_researched.TryGetValue(id, out HashSet<DefinitionId> set)) { set = new HashSet<DefinitionId>(); _researched.Add(id, set); } return set; }
        private static void ValidateDelta(double value)
        { if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d) throw new ArgumentOutOfRangeException(nameof(value)); }

        private sealed class Job
        {
            public Job(EntityId factionId, TechnologyDefinition definition)
            { FactionId = factionId; Definition = definition; RemainingSeconds = definition.ResearchSeconds; }
            public EntityId FactionId { get; }
            public TechnologyDefinition Definition { get; }
            public double RemainingSeconds { get; set; }
        }
    }
}
