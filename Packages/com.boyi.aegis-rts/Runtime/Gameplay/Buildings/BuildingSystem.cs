using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Economy;
using AegisRTS.Gameplay.Technology;

namespace AegisRTS.Gameplay.Buildings
{
    public readonly struct BuildingQueueSnapshot
    {
        public BuildingQueueSnapshot(EntityId settlementId, DefinitionId buildingId, double remainingSeconds)
        { SettlementId = settlementId; BuildingId = buildingId; RemainingSeconds = remainingSeconds; }
        public EntityId SettlementId { get; }
        public DefinitionId BuildingId { get; }
        public double RemainingSeconds { get; }
    }

    /// <summary>Runs the cost, prerequisite, timed construction, and completion-effect pipeline.</summary>
    public sealed class BuildingSystem : IBuildingStatusQuery
    {
        private readonly Dictionary<DefinitionId, BuildingDefinition> _definitions = new Dictionary<DefinitionId, BuildingDefinition>();
        private readonly Dictionary<EntityId, HashSet<DefinitionId>> _built = new Dictionary<EntityId, HashSet<DefinitionId>>();
        private readonly List<Job> _jobs = new List<Job>();
        private readonly EconomySystem _economy;
        private readonly ITechnologyStatusQuery _technologies;
        private readonly IBuildingCompletionSink _sink;
        private readonly EventBus _events;

        public BuildingSystem(IEnumerable<BuildingDefinition> definitions, EconomySystem economy,
            ITechnologyStatusQuery technologies = null, IBuildingCompletionSink sink = null, EventBus eventBus = null)
        {
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _technologies = technologies; _sink = sink; _events = eventBus;
            foreach (BuildingDefinition definition in definitions ?? Array.Empty<BuildingDefinition>())
                if (definition != null) _definitions.Add(definition.Id, definition);
        }

        public int QueuedCount => _jobs.Count;

        public BuildingRequestResult Validate(ConstructBuildingCommand command)
        {
            if (command == null) return BuildingRequestResult.Failure("Command is required.");
            if (!_economy.ContainsAccount(command.SettlementId)) return BuildingRequestResult.Failure("Settlement economy account does not exist.");
            if (!_definitions.TryGetValue(command.BuildingId, out BuildingDefinition definition)) return BuildingRequestResult.Failure("Building definition does not exist.");
            if (IsBuilt(command.SettlementId, command.BuildingId) || IsQueued(command.SettlementId, command.BuildingId)) return BuildingRequestResult.Failure("Building is already built or queued.");
            foreach (DefinitionId prerequisite in definition.PrerequisiteBuildingIds)
                if (!IsBuilt(command.SettlementId, prerequisite)) return BuildingRequestResult.Failure($"Missing building prerequisite '{prerequisite}'.");
            foreach (DefinitionId prerequisite in definition.PrerequisiteTechnologyIds)
                if (_technologies == null || !_technologies.IsResearched(command.FactionId, prerequisite)) return BuildingRequestResult.Failure($"Missing technology prerequisite '{prerequisite}'.");
            if (!_economy.CanAfford(command.SettlementId, definition.Costs)) return BuildingRequestResult.Failure("Insufficient resources.");
            return BuildingRequestResult.Success();
        }

        public BuildingRequestResult Request(ConstructBuildingCommand command)
        {
            BuildingRequestResult validation = Validate(command); if (!validation.Succeeded) return validation;
            BuildingDefinition definition = _definitions[command.BuildingId];
            if (!_economy.TrySpend(command.SettlementId, definition.Costs)) return BuildingRequestResult.Failure("Resources changed before payment completed.");
            _jobs.Add(new Job(command.SettlementId, definition)); return BuildingRequestResult.Success();
        }

        public void Tick(double deltaSeconds)
        {
            ValidateDelta(deltaSeconds);
            for (int index = _jobs.Count - 1; index >= 0; index--)
            {
                Job job = _jobs[index]; job.RemainingSeconds -= deltaSeconds;
                if (job.RemainingSeconds > 0d) continue;
                GetSet(job.SettlementId).Add(job.Definition.Id);
                _economy.AddProduction(job.SettlementId, job.Definition.Production);
                _economy.AddPopulationCapacity(job.SettlementId, job.Definition.PopulationCapacity);
                _sink?.BuildingCompleted(job.SettlementId, job.Definition.Id);
                _events?.Publish(new BuildingCompletedEvent(job.SettlementId, job.Definition.Id));
                _jobs.RemoveAt(index);
            }
        }

        public bool IsBuilt(EntityId settlementId, DefinitionId buildingId) =>
            _built.TryGetValue(settlementId, out HashSet<DefinitionId> values) && values.Contains(buildingId);

        public IReadOnlyList<BuildingQueueSnapshot> SnapshotQueue()
        {
            var result = new List<BuildingQueueSnapshot>(_jobs.Count);
            foreach (Job job in _jobs)
                result.Add(new BuildingQueueSnapshot(job.SettlementId, job.Definition.Id, Math.Max(0d, job.RemainingSeconds)));
            return result.AsReadOnly();
        }

        /// <summary>Restores an already-paid construction job without spending its cost a second time.</summary>
        public void RestoreQueuedJob(EntityId settlementId, DefinitionId buildingId, double remainingSeconds)
        {
            if (!_economy.ContainsAccount(settlementId)) throw new InvalidOperationException("Settlement economy account does not exist.");
            if (!_definitions.TryGetValue(buildingId, out BuildingDefinition definition)) throw new InvalidOperationException("Building definition does not exist.");
            if (IsBuilt(settlementId, buildingId) || IsQueued(settlementId, buildingId)) throw new InvalidOperationException("Building is already built or queued.");
            if (remainingSeconds <= 0d || remainingSeconds > definition.BuildSeconds || double.IsNaN(remainingSeconds) || double.IsInfinity(remainingSeconds))
                throw new ArgumentOutOfRangeException(nameof(remainingSeconds));
            foreach (DefinitionId prerequisite in definition.PrerequisiteBuildingIds)
                if (!IsBuilt(settlementId, prerequisite)) throw new InvalidOperationException($"Missing building prerequisite '{prerequisite}'.");
            _jobs.Add(new Job(settlementId, definition, remainingSeconds));
        }

        public string GetDebugSummary() => $"Buildings={_definitions.Count}, ConstructionQueued={_jobs.Count}";

        private bool IsQueued(EntityId settlementId, DefinitionId buildingId) =>
            _jobs.Exists(job => job.SettlementId == settlementId && job.Definition.Id == buildingId);
        private HashSet<DefinitionId> GetSet(EntityId id)
        { if (!_built.TryGetValue(id, out HashSet<DefinitionId> set)) { set = new HashSet<DefinitionId>(); _built.Add(id, set); } return set; }
        private static void ValidateDelta(double value)
        { if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d) throw new ArgumentOutOfRangeException(nameof(value)); }

        private sealed class Job
        {
            public Job(EntityId settlementId, BuildingDefinition definition, double? remainingSeconds = null)
            { SettlementId = settlementId; Definition = definition; RemainingSeconds = remainingSeconds ?? definition.BuildSeconds; }
            public EntityId SettlementId { get; }
            public BuildingDefinition Definition { get; }
            public double RemainingSeconds { get; set; }
        }
    }
}
