using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Settlements;

namespace AegisRTS.Gameplay.Siege
{
    public interface ISiegeQuery
    {
        bool TryGetState(EntityId siegeId, out SiegeSnapshot snapshot);
        IReadOnlyList<SiegeSnapshot> Snapshot();
        string GetDebugSummary();
    }

    /// <summary>Pure C# siege areas, structures, breaches, objectives, and outcome orchestration.</summary>
    public sealed class SiegeSystem : ISiegeQuery
    {
        private readonly Dictionary<EntityId, SiegeRecord> _sieges = new Dictionary<EntityId, SiegeRecord>();
        private readonly ISiegeAttackerQuery _attackers;
        private readonly ISiegeNavigationSink _navigation;
        private readonly ISiegeCaptureSink _capture;
        private readonly ISiegeRule _rules;
        private readonly EventBus _events;

        public SiegeSystem(ISiegeAttackerQuery attackers, ISiegeNavigationSink navigation = null,
            ISiegeCaptureSink capture = null, ISiegeRule rules = null, EventBus eventBus = null)
        {
            _attackers = attackers ?? throw new ArgumentNullException(nameof(attackers));
            _navigation = navigation; _capture = capture; _rules = rules ?? new DefaultSiegeRule(); _events = eventBus;
        }

        public int SiegeCount => _sieges.Count;

        public void Register(EntityId siegeId, SiegeProfile profile)
        {
            if (!siegeId.IsValid) throw new ArgumentException("Siege ID must be valid.", nameof(siegeId));
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (_sieges.ContainsKey(siegeId)) throw new InvalidOperationException($"Siege {siegeId} already exists.");
            _sieges.Add(siegeId, new SiegeRecord(siegeId, profile));
        }

        public bool RegisterStructure(EntityId siegeId, EntityId structureId, DefenseStructureProfile profile)
        {
            if (!structureId.IsValid || profile == null || !_sieges.TryGetValue(siegeId, out SiegeRecord siege) ||
                siege.State != SiegeState.Preparing || siege.Structures.ContainsKey(structureId) ||
                profile.FactionId != siege.Profile.DefenderFactionId) return false;
            siege.Structures.Add(structureId, new StructureRecord(structureId, profile));
            return true;
        }

        public bool RegisterDefenders(EntityId siegeId, IEnumerable<EntityId> defenderIds, EntityId commanderId = default)
        {
            if (!_sieges.TryGetValue(siegeId, out SiegeRecord siege) || siege.State != SiegeState.Preparing || defenderIds == null) return false;
            var next = new HashSet<EntityId>();
            foreach (EntityId id in defenderIds) if (id.IsValid) next.Add(id);
            if (commanderId.IsValid && !next.Contains(commanderId)) return false;
            siege.DefenderIds.Clear(); foreach (EntityId id in next) siege.DefenderIds.Add(id);
            siege.CommanderId = commanderId;
            return true;
        }

        public SiegeActionResult Validate(StartSiegeCommand command)
        {
            if (command == null) return SiegeActionResult.Failure("Command is required.");
            if (!_sieges.TryGetValue(command.SiegeId, out SiegeRecord siege)) return SiegeActionResult.Failure("Siege does not exist.");
            if (siege.State != SiegeState.Preparing) return SiegeActionResult.Failure("Siege has already started.");
            return SiegeActionResult.Success();
        }

        public SiegeActionResult Execute(StartSiegeCommand command)
        {
            SiegeActionResult result = Validate(command); if (!result.Succeeded) return result;
            SiegeRecord siege = _sieges[command.SiegeId]; siege.State = SiegeState.Active;
            _events?.Publish(new SiegeStartedEvent(siege.SiegeId, siege.Profile.Mode)); return SiegeActionResult.Success();
        }

        public SiegeActionResult Validate(AttackDefenseStructureCommand command)
        {
            if (command == null) return SiegeActionResult.Failure("Command is required.");
            if (!TryActive(command.SiegeId, out SiegeRecord siege)) return SiegeActionResult.Failure("Siege is not active.");
            if (!siege.Structures.TryGetValue(command.StructureId, out StructureRecord structure) || structure.IsDestroyed)
                return SiegeActionResult.Failure("Defense structure is missing or destroyed.");
            if (!_attackers.TryGetAttacker(command.AttackerId, out SiegeAttackerSnapshot attacker) || attacker.Attack == null)
                return SiegeActionResult.Failure("Attacker is unavailable.");
            if (attacker.FactionId != siege.Profile.AttackerFactionId || attacker.FactionId == structure.Profile.FactionId)
                return SiegeActionResult.Failure("Only the attacking faction can damage this structure.");
            if (!CanTarget(attacker.Attack.TargetTags, structure.Profile.Tags)) return SiegeActionResult.Failure("Attack cannot target this structure.");
            return SiegeActionResult.Success();
        }

        public SiegeActionResult Execute(AttackDefenseStructureCommand command)
        {
            SiegeActionResult result = Validate(command); if (!result.Succeeded) return result;
            SiegeRecord siege = _sieges[command.SiegeId]; StructureRecord structure = siege.Structures[command.StructureId];
            _attackers.TryGetAttacker(command.AttackerId, out SiegeAttackerSnapshot attacker);
            double damage = attacker.Attack.Damage <= 0d ? 0d : attacker.Attack.DamageType == DamageType.Physical
                ? Math.Max(1d, attacker.Attack.Damage - structure.Profile.Armor) : attacker.Attack.Damage;
            structure.Health = Math.Max(0d, structure.Health - damage);
            _events?.Publish(new DefenseStructureDamagedEvent(siege.SiegeId, structure.StructureId, damage, structure.Health));
            if (structure.IsDestroyed) DestroyStructure(siege, structure);
            return SiegeActionResult.Success();
        }

        public SiegeActionResult Validate(SetGateStateCommand command)
        {
            if (command == null) return SiegeActionResult.Failure("Command is required.");
            if (!TryActive(command.SiegeId, out SiegeRecord siege) || !siege.Structures.TryGetValue(command.GateId, out StructureRecord gate))
                return SiegeActionResult.Failure("Active siege gate does not exist.");
            if (gate.Profile.Kind != DefenseStructureKind.Gate || gate.IsDestroyed || command.State == GateState.Destroyed)
                return SiegeActionResult.Failure("Gate state cannot be changed.");
            bool valid = gate.GateState == GateState.Closed && command.State == GateState.Opening ||
                gate.GateState == GateState.Opening && command.State == GateState.Open ||
                gate.GateState == GateState.Open && command.State == GateState.Closing ||
                gate.GateState == GateState.Closing && command.State == GateState.Closed;
            return valid ? SiegeActionResult.Success() : SiegeActionResult.Failure("Gate state transition is invalid.");
        }

        public SiegeActionResult Execute(SetGateStateCommand command)
        {
            SiegeActionResult result = Validate(command); if (!result.Succeeded) return result;
            StructureRecord gate = _sieges[command.SiegeId].Structures[command.GateId]; gate.GateState = command.State;
            _events?.Publish(new GateStateChangedEvent(command.SiegeId, command.GateId, command.State));
            if (command.State == GateState.Open) _navigation?.RefreshAfterBreach(command.SiegeId, command.GateId, SiegeArea.Gates);
            return SiegeActionResult.Success();
        }

        public SiegeActionResult Validate(EnterSiegeAreaCommand command)
        {
            if (command == null) return SiegeActionResult.Failure("Command is required.");
            if (!TryActive(command.SiegeId, out SiegeRecord siege)) return SiegeActionResult.Failure("Siege is not active.");
            return _rules.CanEnter(CreateSnapshot(siege), command.Area);
        }

        public SiegeActionResult Execute(EnterSiegeAreaCommand command)
        {
            SiegeActionResult result = Validate(command); if (!result.Succeeded) return result;
            SiegeRecord siege = _sieges[command.SiegeId]; siege.CurrentArea = command.Area;
            if (command.Area == SiegeArea.InnerArea) siege.State = SiegeState.InnerAreaContested;
            if (command.Area == SiegeArea.CaptureObjective)
            { siege.State = SiegeState.CaptureAvailable; siege.Conditions |= CaptureCondition.ZoneControlled; }
            _events?.Publish(new SiegeAreaEnteredEvent(command.SiegeId, command.Area)); return SiegeActionResult.Success();
        }

        public SiegeActionResult Validate(ReportSiegeConditionCommand command)
        {
            if (command == null || !TryActive(command.SiegeId, out _)) return SiegeActionResult.Failure("Siege is not active.");
            return SiegeActionResult.Success();
        }

        public SiegeActionResult Execute(ReportSiegeConditionCommand command)
        {
            SiegeActionResult result = Validate(command); if (!result.Succeeded) return result;
            SiegeRecord siege = _sieges[command.SiegeId];
            siege.Conditions |= command.Condition; return SiegeActionResult.Success();
        }

        public SiegeActionResult Validate(CompleteSiegeWaveCommand command)
        {
            if (command == null || !TryActive(command.SiegeId, out SiegeRecord siege) || siege.Profile.Mode != SiegeMode.WaveDefense)
                return SiegeActionResult.Failure("Wave defense siege is not active.");
            return SiegeActionResult.Success();
        }

        public SiegeActionResult Execute(CompleteSiegeWaveCommand command)
        {
            SiegeActionResult result = Validate(command); if (!result.Succeeded) return result;
            SiegeRecord siege = _sieges[command.SiegeId];
            siege.CompletedWaves++;
            if (siege.Profile.RequiredWaves > 0 && siege.CompletedWaves >= siege.Profile.RequiredWaves)
                Complete(siege, siege.Profile.DefenderFactionId, false);
            return SiegeActionResult.Success();
        }

        public SiegeActionResult Validate(CaptureSiegeCommand command)
        {
            if (command == null) return SiegeActionResult.Failure("Command is required.");
            if (!TryActive(command.SiegeId, out SiegeRecord siege)) return SiegeActionResult.Failure("Siege is not active.");
            if (_capture == null) return SiegeActionResult.Failure("Settlement capture sink is unavailable.");
            return _rules.CanCapture(CreateSnapshot(siege));
        }

        public SiegeActionResult Execute(CaptureSiegeCommand command)
        {
            SiegeActionResult result = Validate(command); if (!result.Succeeded) return result;
            SiegeRecord siege = _sieges[command.SiegeId];
            result = _capture.Capture(siege.Profile.SettlementId, siege.Profile.AttackerFactionId,
                siege.Conditions, siege.Profile.CapturingArmyId);
            if (!result.Succeeded) return result;
            Complete(siege, siege.Profile.AttackerFactionId, false); return SiegeActionResult.Success();
        }

        public void NotifyUnitDied(EntityId entityId)
        {
            foreach (SiegeRecord siege in _sieges.Values)
            {
                if (!TryActive(siege.SiegeId, out _)) continue;
                if (entityId == siege.CommanderId) siege.Conditions |= CaptureCondition.CommanderKilled;
                if (siege.DefenderIds.Remove(entityId) && siege.DefenderIds.Count == 0)
                    siege.Conditions |= CaptureCondition.DefendersCleared;
            }
        }

        public void Tick(double deltaSeconds)
        {
            if (deltaSeconds < 0d || double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds)) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            foreach (SiegeRecord siege in _sieges.Values)
            {
                if (!TryActive(siege.SiegeId, out _)) continue;
                siege.ElapsedSeconds += deltaSeconds;
                if (siege.Profile.TimeLimitSeconds > 0d && siege.ElapsedSeconds >= siege.Profile.TimeLimitSeconds)
                    Complete(siege, siege.Profile.DefenderFactionId, siege.Profile.Mode != SiegeMode.Survival && siege.Profile.Mode != SiegeMode.WaveDefense);
            }
        }

        public bool TryGetState(EntityId siegeId, out SiegeSnapshot snapshot)
        { if (!_sieges.TryGetValue(siegeId, out SiegeRecord siege)) { snapshot = default; return false; } snapshot = CreateSnapshot(siege); return true; }

        public IReadOnlyList<SiegeSnapshot> Snapshot()
        { var values = new List<SiegeSnapshot>(); foreach (SiegeRecord siege in _sieges.Values) values.Add(CreateSnapshot(siege)); values.Sort((a, b) => a.SiegeId.CompareTo(b.SiegeId)); return values.AsReadOnly(); }

        public string GetDebugSummary()
        { int active = 0, structures = 0, breaches = 0; foreach (SiegeRecord siege in _sieges.Values) { if (IsActive(siege.State)) active++; structures += siege.Structures.Count; if (HasPassage(siege)) breaches++; } return $"Sieges={_sieges.Count}, Active={active}, Structures={structures}, Passages={breaches}"; }

        private void DestroyStructure(SiegeRecord siege, StructureRecord structure)
        {
            if (structure.Profile.Kind == DefenseStructureKind.Gate) structure.GateState = GateState.Destroyed;
            _events?.Publish(new DefenseStructureDestroyedEvent(siege.SiegeId, structure.StructureId, structure.Profile.Kind));
            if (structure.Profile.Kind == DefenseStructureKind.Wall || structure.Profile.Kind == DefenseStructureKind.Gate)
            {
                siege.State = SiegeState.Breached;
                _events?.Publish(new BreachCreatedEvent(siege.SiegeId, structure.StructureId));
                _navigation?.RefreshAfterBreach(siege.SiegeId, structure.StructureId, SiegeArea.Breach);
            }
            if (structure.Profile.Kind == DefenseStructureKind.Core) siege.Conditions |= CaptureCondition.CoreDestroyed;
        }

        private void Complete(SiegeRecord siege, EntityId winner, bool failed)
        { siege.WinningFactionId = winner; siege.State = failed ? SiegeState.Failed : SiegeState.Completed; _events?.Publish(new SiegeCompletedEvent(siege.SiegeId, winner)); }
        private bool TryActive(EntityId siegeId, out SiegeRecord siege) => _sieges.TryGetValue(siegeId, out siege) && IsActive(siege.State);
        private static bool IsActive(SiegeState state) => state == SiegeState.Active || state == SiegeState.Breached ||
            state == SiegeState.InnerAreaContested || state == SiegeState.CaptureAvailable;
        private static bool CanTarget(IReadOnlyList<string> requiredTags, IReadOnlyList<string> targetTags)
        { if (requiredTags == null || requiredTags.Count == 0) return true; foreach (string required in requiredTags) foreach (string actual in targetTags) if (string.Equals(required, actual, StringComparison.Ordinal)) return true; return false; }
        private static bool HasPassage(SiegeRecord siege)
        { foreach (StructureRecord value in siege.Structures.Values) if ((value.Profile.Kind == DefenseStructureKind.Gate && (value.IsDestroyed || value.GateState == GateState.Open)) || (value.Profile.Kind == DefenseStructureKind.Wall && value.IsDestroyed)) return true; return false; }

        private static SiegeSnapshot CreateSnapshot(SiegeRecord siege)
        {
            var structures = new List<DefenseStructureSnapshot>();
            foreach (StructureRecord value in siege.Structures.Values) structures.Add(new DefenseStructureSnapshot(value.StructureId, value.Profile, value.Health, value.GateState));
            structures.Sort((a, b) => a.StructureId.CompareTo(b.StructureId));
            return new SiegeSnapshot(siege.SiegeId, siege.Profile, siege.State, siege.CurrentArea, siege.Conditions,
                siege.ElapsedSeconds, siege.CompletedWaves, siege.WinningFactionId, structures.AsReadOnly());
        }

        private sealed class SiegeRecord
        {
            public SiegeRecord(EntityId id, SiegeProfile profile) { SiegeId = id; Profile = profile; State = SiegeState.Preparing; CurrentArea = SiegeArea.OuterArea; }
            public EntityId SiegeId { get; }
            public SiegeProfile Profile { get; }
            public SiegeState State { get; set; }
            public SiegeArea CurrentArea { get; set; }
            public CaptureCondition Conditions { get; set; }
            public double ElapsedSeconds { get; set; }
            public int CompletedWaves { get; set; }
            public EntityId WinningFactionId { get; set; }
            public EntityId CommanderId { get; set; }
            public HashSet<EntityId> DefenderIds { get; } = new HashSet<EntityId>();
            public Dictionary<EntityId, StructureRecord> Structures { get; } = new Dictionary<EntityId, StructureRecord>();
        }

        private sealed class StructureRecord
        {
            public StructureRecord(EntityId id, DefenseStructureProfile profile) { StructureId = id; Profile = profile; Health = profile.MaxHealth; GateState = profile.Kind == DefenseStructureKind.Gate ? GateState.Closed : GateState.Destroyed; }
            public EntityId StructureId { get; }
            public DefenseStructureProfile Profile { get; }
            public double Health { get; set; }
            public GateState GateState { get; set; }
            public bool IsDestroyed => Health <= 0d;
        }

        private sealed class DefaultSiegeRule : ISiegeRule
        {
            public SiegeActionResult CanEnter(SiegeSnapshot siege, SiegeArea targetArea)
            {
                bool passage = false;
                foreach (DefenseStructureSnapshot structure in siege.Structures)
                    if (structure.Profile.Kind == DefenseStructureKind.Gate && (structure.IsDestroyed || structure.GateState == GateState.Open) ||
                        structure.Profile.Kind == DefenseStructureKind.Wall && structure.IsDestroyed) passage = true;
                if ((targetArea == SiegeArea.InnerArea || targetArea == SiegeArea.Breach) && !passage)
                    return SiegeActionResult.Failure("A gate or wall passage is required.");
                if (targetArea == SiegeArea.CaptureObjective && siege.CurrentArea != SiegeArea.InnerArea)
                    return SiegeActionResult.Failure("Attackers must enter the inner area first.");
                return SiegeActionResult.Success();
            }

            public SiegeActionResult CanCapture(SiegeSnapshot siege) => siege.CurrentArea == SiegeArea.CaptureObjective
                ? SiegeActionResult.Success() : SiegeActionResult.Failure("Capture objective is not controlled.");
        }
    }
}
