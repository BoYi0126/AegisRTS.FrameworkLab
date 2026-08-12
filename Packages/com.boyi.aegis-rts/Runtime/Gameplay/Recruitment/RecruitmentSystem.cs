using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Buildings;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Economy;
using AegisRTS.Gameplay.Technology;

namespace AegisRTS.Gameplay.Recruitment
{
    public readonly struct RecruitmentQueueSnapshot
    {
        public RecruitmentQueueSnapshot(EntityId settlementId, EntityId factionId, DefinitionId unitId, double remainingSeconds)
        { SettlementId = settlementId; FactionId = factionId; UnitId = unitId; RemainingSeconds = remainingSeconds; }
        public EntityId SettlementId { get; }
        public EntityId FactionId { get; }
        public DefinitionId UnitId { get; }
        public double RemainingSeconds { get; }
    }

    /// <summary>Runs request, validation, cost, queue, timer, and spawn as a deterministic pipeline.</summary>
    public sealed class RecruitmentSystem
    {
        private readonly Dictionary<DefinitionId, UnitDefinition> _definitions = new Dictionary<DefinitionId, UnitDefinition>();
        private readonly List<Job> _jobs = new List<Job>();
        private readonly EconomySystem _economy;
        private readonly IBuildingStatusQuery _buildings;
        private readonly ITechnologyStatusQuery _technologies;
        private readonly IUnitSpawnSink _sink;
        private readonly EventBus _events;

        public RecruitmentSystem(IEnumerable<UnitDefinition> definitions, EconomySystem economy,
            IBuildingStatusQuery buildings = null, ITechnologyStatusQuery technologies = null,
            IUnitSpawnSink sink = null, EventBus eventBus = null)
        {
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _buildings = buildings; _technologies = technologies; _sink = sink; _events = eventBus;
            foreach (UnitDefinition definition in definitions ?? Array.Empty<UnitDefinition>())
                if (definition != null) _definitions.Add(definition.Id, definition);
        }

        public int QueuedCount => _jobs.Count;

        public RecruitmentRequestResult Validate(RecruitUnitCommand command)
        {
            if (command == null) return RecruitmentRequestResult.Failure("Command is required.");
            if (!_economy.ContainsAccount(command.SettlementId)) return RecruitmentRequestResult.Failure("Settlement economy account does not exist.");
            if (!_definitions.TryGetValue(command.UnitId, out UnitDefinition definition)) return RecruitmentRequestResult.Failure("Unit definition does not exist.");
            foreach (DefinitionId prerequisite in definition.PrerequisiteBuildingIds)
                if (_buildings == null || !_buildings.IsBuilt(command.SettlementId, prerequisite)) return RecruitmentRequestResult.Failure($"Missing building prerequisite '{prerequisite}'.");
            foreach (DefinitionId prerequisite in definition.PrerequisiteTechnologyIds)
                if (_technologies == null || !_technologies.IsResearched(command.FactionId, prerequisite)) return RecruitmentRequestResult.Failure($"Missing technology prerequisite '{prerequisite}'.");
            if (!_economy.CanReservePopulation(command.SettlementId, definition.PopulationCost)) return RecruitmentRequestResult.Failure("Population capacity exceeded.");
            if (!_economy.CanAfford(command.SettlementId, definition.Costs)) return RecruitmentRequestResult.Failure("Insufficient resources.");
            return RecruitmentRequestResult.Success();
        }

        public RecruitmentRequestResult Request(RecruitUnitCommand command)
        {
            RecruitmentRequestResult validation = Validate(command); if (!validation.Succeeded) return validation;
            UnitDefinition definition = _definitions[command.UnitId];
            if (!_economy.TryReservePopulation(command.SettlementId, definition.PopulationCost)) return RecruitmentRequestResult.Failure("Population changed before reservation completed.");
            if (!_economy.TrySpend(command.SettlementId, definition.Costs))
            { _economy.ReleasePopulation(command.SettlementId, definition.PopulationCost); return RecruitmentRequestResult.Failure("Resources changed before payment completed."); }
            _jobs.Add(new Job(command.SettlementId, command.FactionId, definition)); return RecruitmentRequestResult.Success();
        }

        public void Tick(double deltaSeconds)
        {
            ValidateDelta(deltaSeconds);
            for (int index = _jobs.Count - 1; index >= 0; index--)
            {
                Job job = _jobs[index]; job.RemainingSeconds -= deltaSeconds;
                if (job.RemainingSeconds > 0d) continue;
                try
                {
                    _sink?.SpawnUnit(job.SettlementId, job.FactionId, job.Definition.Id);
                }
                catch (Exception exception)
                {
                    foreach (ResourceCost cost in job.Definition.Costs)
                        _economy.AddResource(job.SettlementId, cost.ResourceId, cost.Amount);
                    _economy.ReleasePopulation(job.SettlementId, job.Definition.PopulationCost);
                    _jobs.RemoveAt(index);
                    throw new InvalidOperationException($"Recruitment spawn for '{job.Definition.Id}' failed and was rolled back.", exception);
                }
                _events?.Publish(new UnitRecruitedEvent(job.SettlementId, job.FactionId, job.Definition.Id));
                _jobs.RemoveAt(index);
            }
        }

        public string GetDebugSummary() => $"Units={_definitions.Count}, RecruitmentQueued={_jobs.Count}";

        public IReadOnlyList<RecruitmentQueueSnapshot> SnapshotQueue()
        {
            var result = new List<RecruitmentQueueSnapshot>(_jobs.Count);
            foreach (Job job in _jobs)
                result.Add(new RecruitmentQueueSnapshot(job.SettlementId, job.FactionId, job.Definition.Id, Math.Max(0d, job.RemainingSeconds)));
            return result.AsReadOnly();
        }

        /// <summary>Restores an already-paid and population-reserved recruitment job.</summary>
        public void RestoreQueuedJob(EntityId settlementId, EntityId factionId, DefinitionId unitId, double remainingSeconds)
        {
            if (!_economy.ContainsAccount(settlementId)) throw new InvalidOperationException("Settlement economy account does not exist.");
            if (!_definitions.TryGetValue(unitId, out UnitDefinition definition)) throw new InvalidOperationException("Unit definition does not exist.");
            if (remainingSeconds <= 0d || remainingSeconds > definition.RecruitmentSeconds || double.IsNaN(remainingSeconds) || double.IsInfinity(remainingSeconds))
                throw new ArgumentOutOfRangeException(nameof(remainingSeconds));
            foreach (DefinitionId prerequisite in definition.PrerequisiteBuildingIds)
                if (_buildings == null || !_buildings.IsBuilt(settlementId, prerequisite)) throw new InvalidOperationException($"Missing building prerequisite '{prerequisite}'.");
            foreach (DefinitionId prerequisite in definition.PrerequisiteTechnologyIds)
                if (_technologies == null || !_technologies.IsResearched(factionId, prerequisite)) throw new InvalidOperationException($"Missing technology prerequisite '{prerequisite}'.");
            _jobs.Add(new Job(settlementId, factionId, definition, remainingSeconds));
        }

        private static void ValidateDelta(double value)
        { if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d) throw new ArgumentOutOfRangeException(nameof(value)); }

        private sealed class Job
        {
            public Job(EntityId settlementId, EntityId factionId, UnitDefinition definition, double? remainingSeconds = null)
            { SettlementId = settlementId; FactionId = factionId; Definition = definition; RemainingSeconds = remainingSeconds ?? definition.RecruitmentSeconds; }
            public EntityId SettlementId { get; }
            public EntityId FactionId { get; }
            public UnitDefinition Definition { get; }
            public double RemainingSeconds { get; set; }
        }
    }
}
