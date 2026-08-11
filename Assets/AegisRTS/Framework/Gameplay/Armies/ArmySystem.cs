using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Formation;
using AegisRTS.Gameplay.Heroes;

namespace AegisRTS.Gameplay.Armies
{
    /// <summary>Pure C# army composition, commander, optional rules, and order state.</summary>
    public sealed class ArmySystem : IArmyQuery
    {
        private readonly Dictionary<EntityId, EntityId> _memberFactions = new Dictionary<EntityId, EntityId>();
        private readonly Dictionary<EntityId, EntityId> _armyByMember = new Dictionary<EntityId, EntityId>();
        private readonly Dictionary<EntityId, ArmyRecord> _armies = new Dictionary<EntityId, ArmyRecord>();
        private readonly HeroSystem _heroes;
        private readonly ArmyRuleOptions _rules;
        private readonly IArmyOrderExecutor _orders;
        private readonly IArmyMembershipSink _membershipSink;
        private readonly IArmySettlementTargetValidator _settlementTargets;
        private readonly EventBus _events;

        public ArmySystem(HeroSystem heroes, ArmyRuleOptions rules = null, IArmyOrderExecutor orderExecutor = null,
            IArmyMembershipSink membershipSink = null, EventBus eventBus = null,
            IArmySettlementTargetValidator settlementTargetValidator = null)
        {
            _heroes = heroes ?? throw new ArgumentNullException(nameof(heroes));
            _rules = rules ?? new ArmyRuleOptions(false, false);
            _orders = orderExecutor ?? new StateOnlyArmyOrderExecutor();
            _membershipSink = membershipSink ?? new NullArmyMembershipSink();
            _settlementTargets = settlementTargetValidator ?? new AllowAllSettlementTargets();
            _events = eventBus;
        }

        public int ArmyCount => _armies.Count;
        public int RegisteredMemberCount => _memberFactions.Count;

        public void RegisterMember(EntityId unitId, EntityId factionId)
        {
            if (!unitId.IsValid || !factionId.IsValid) throw new ArgumentException("Unit and faction IDs must be valid.");
            if (_memberFactions.ContainsKey(unitId)) throw new InvalidOperationException($"Army member {unitId} is already registered.");
            if (_heroes.TryGetState(unitId, out HeroSnapshot hero) && hero.Profile.FactionId != factionId)
                throw new InvalidOperationException("Hero and unit faction must match.");
            _memberFactions.Add(unitId, factionId);
        }

        public bool UnregisterMember(EntityId unitId)
        {
            if (_armyByMember.ContainsKey(unitId)) return false;
            return _memberFactions.Remove(unitId);
        }

        public ArmyCommandResult Validate(CreateArmyCommand command)
        {
            if (command == null) return ArmyCommandResult.Failure("Command is required.");
            if (_armies.ContainsKey(command.ArmyId)) return ArmyCommandResult.Failure($"Army {command.ArmyId} already exists.");
            foreach (EntityId unitId in command.UnitIds)
            {
                if (!_memberFactions.TryGetValue(unitId, out EntityId faction)) return ArmyCommandResult.Failure($"Unit {unitId} is not registered.");
                if (faction != command.FactionId) return ArmyCommandResult.Failure($"Unit {unitId} belongs to another faction.");
                if (_armyByMember.ContainsKey(unitId)) return ArmyCommandResult.Failure($"Unit {unitId} already belongs to an army.");
            }
            return ValidateCommander(command.CommanderId, command.FactionId, command.UnitIds);
        }

        public ArmyCommandResult Execute(CreateArmyCommand command)
        {
            ArmyCommandResult validation = Validate(command);
            if (!validation.Succeeded) return validation;
            var record = new ArmyRecord(command.ArmyId, command.FactionId, command.UnitIds, command.CommanderId,
                command.Formation, _rules.InitialMorale, _rules.InitialSupply);
            _armies.Add(record.ArmyId, record);
            foreach (EntityId member in record.UnitIds) AssignMember(member, record.ArmyId);
            _events?.Publish(new ArmyCreatedEvent(record.ArmyId, record.FactionId, record.UnitIds.Count));
            if (record.CommanderId.IsValid) _events?.Publish(new ArmyCommanderAssignedEvent(record.ArmyId, record.CommanderId));
            return ArmyCommandResult.Success(record.ArmyId, record.UnitIds.Count);
        }

        public ArmyCommandResult Validate(MergeArmiesCommand command)
        {
            if (command == null) return ArmyCommandResult.Failure("Command is required.");
            if (command.TargetArmyId == command.AbsorbedArmyId) return ArmyCommandResult.Failure("An army cannot merge with itself.");
            if (!_armies.TryGetValue(command.TargetArmyId, out ArmyRecord target) || !_armies.TryGetValue(command.AbsorbedArmyId, out ArmyRecord absorbed))
                return ArmyCommandResult.Failure("Both armies must exist.");
            return target.FactionId == absorbed.FactionId
                ? ArmyCommandResult.Success(target.ArmyId, target.UnitIds.Count + absorbed.UnitIds.Count)
                : ArmyCommandResult.Failure("Only armies from the same faction can merge.");
        }

        public ArmyCommandResult Execute(MergeArmiesCommand command)
        {
            ArmyCommandResult validation = Validate(command);
            if (!validation.Succeeded) return validation;
            ArmyRecord target = _armies[command.TargetArmyId];
            ArmyRecord absorbed = _armies[command.AbsorbedArmyId];
            int targetCount = target.UnitIds.Count;
            int absorbedCount = absorbed.UnitIds.Count;
            target.Morale = Weighted(target.Morale, targetCount, absorbed.Morale, absorbedCount);
            target.Supply = Weighted(target.Supply, targetCount, absorbed.Supply, absorbedCount);
            foreach (EntityId member in absorbed.UnitIds)
            {
                target.UnitIds.Add(member);
                AssignMember(member, target.ArmyId);
            }
            target.UnitIds.Sort();
            _armies.Remove(absorbed.ArmyId);
            _events?.Publish(new ArmiesMergedEvent(target.ArmyId, absorbed.ArmyId, target.UnitIds.Count));
            return ArmyCommandResult.Success(target.ArmyId, target.UnitIds.Count);
        }

        public ArmyCommandResult Validate(SplitArmyCommand command)
        {
            if (command == null) return ArmyCommandResult.Failure("Command is required.");
            if (_armies.ContainsKey(command.NewArmyId)) return ArmyCommandResult.Failure($"Army {command.NewArmyId} already exists.");
            if (!_armies.TryGetValue(command.SourceArmyId, out ArmyRecord source)) return ArmyCommandResult.Failure("Source army does not exist.");
            if (command.UnitIds.Count >= source.UnitIds.Count) return ArmyCommandResult.Failure("A split must leave at least one unit in the source army.");
            foreach (EntityId unitId in command.UnitIds)
                if (!source.UnitIds.Contains(unitId)) return ArmyCommandResult.Failure($"Unit {unitId} is not in the source army.");
            if (Contains(command.UnitIds, source.CommanderId)) return ArmyCommandResult.Failure("Assign a different source commander before moving its commander.");
            return ValidateCommander(command.CommanderId, source.FactionId, command.UnitIds);
        }

        public ArmyCommandResult Execute(SplitArmyCommand command)
        {
            ArmyCommandResult validation = Validate(command);
            if (!validation.Succeeded) return validation;
            ArmyRecord source = _armies[command.SourceArmyId];
            foreach (EntityId member in command.UnitIds) source.UnitIds.Remove(member);
            var created = new ArmyRecord(command.NewArmyId, source.FactionId, command.UnitIds, command.CommanderId,
                source.Formation, source.Morale, source.Supply);
            _armies.Add(created.ArmyId, created);
            foreach (EntityId member in created.UnitIds) AssignMember(member, created.ArmyId);
            _events?.Publish(new ArmySplitEvent(source.ArmyId, created.ArmyId, created.UnitIds.Count));
            if (created.CommanderId.IsValid) _events?.Publish(new ArmyCommanderAssignedEvent(created.ArmyId, created.CommanderId));
            return ArmyCommandResult.Success(created.ArmyId, created.UnitIds.Count);
        }

        public ArmyCommandResult Validate(AssignArmyCommanderCommand command)
        {
            if (command == null) return ArmyCommandResult.Failure("Command is required.");
            if (!_armies.TryGetValue(command.ArmyId, out ArmyRecord army)) return ArmyCommandResult.Failure("Army does not exist.");
            return ValidateCommander(command.CommanderId, army.FactionId, army.UnitIds);
        }

        public ArmyCommandResult Execute(AssignArmyCommanderCommand command)
        {
            ArmyCommandResult validation = Validate(command);
            if (!validation.Succeeded) return validation;
            ArmyRecord army = _armies[command.ArmyId];
            army.CommanderId = command.CommanderId;
            _events?.Publish(new ArmyCommanderAssignedEvent(army.ArmyId, army.CommanderId));
            return ArmyCommandResult.Success(army.ArmyId, 1);
        }

        public ArmyCommandResult Validate(ArmyOrderCommand command)
        {
            if (command == null) return ArmyCommandResult.Failure("Command is required.");
            return _armies.TryGetValue(command.ArmyId, out ArmyRecord army) && army.UnitIds.Count > 0
                ? ArmyCommandResult.Success(army.ArmyId, army.UnitIds.Count)
                : ArmyCommandResult.Failure("Army does not exist or has no units.");
        }

        public ArmyCommandResult Validate(AttackSettlementArmyCommand command)
        {
            ArmyCommandResult validation = Validate((ArmyOrderCommand)command);
            if (!validation.Succeeded) return validation;
            ArmyRecord army = _armies[command.ArmyId];
            return _settlementTargets.Validate(command.SettlementId, army.FactionId, out string error)
                ? validation
                : ArmyCommandResult.Failure(error);
        }

        public ArmyCommandResult Execute(MoveArmyCommand command) => ExecuteOrder(command,
            army => new ArmyOrder(ArmyOrderType.Move, command.Destination, default, army.Formation),
            army => _orders.Move(army.UnitIds.AsReadOnly(), command.Destination, army.Formation));

        public ArmyCommandResult Execute(AttackArmyCommand command) => ExecuteOrder(command,
            army => new ArmyOrder(ArmyOrderType.Attack, default, command.TargetId, army.Formation),
            army => _orders.Attack(army.UnitIds.AsReadOnly(), command.TargetId));

        public ArmyCommandResult Execute(AttackSettlementArmyCommand command) => ExecuteOrder(command,
            army => new ArmyOrder(ArmyOrderType.AttackSettlement, default, command.SettlementId, army.Formation),
            army => _orders.AttackSettlement(army.UnitIds.AsReadOnly(), command.SettlementId));

        public ArmyCommandResult Execute(DefendArmyCommand command) => ExecuteOrder(command,
            army => new ArmyOrder(ArmyOrderType.Defend, command.Position, default, army.Formation),
            army => _orders.Defend(army.UnitIds.AsReadOnly(), command.Position, army.Formation));

        public ArmyCommandResult Execute(RetreatArmyCommand command) => ExecuteOrder(command,
            army => new ArmyOrder(ArmyOrderType.Retreat, command.Destination, default, army.Formation),
            army => _orders.Retreat(army.UnitIds.AsReadOnly(), command.Destination, army.Formation));

        public bool AdjustMorale(EntityId armyId, double delta)
        {
            if (!_rules.MoraleEnabled || !_armies.TryGetValue(armyId, out ArmyRecord army) || !IsFinite(delta)) return false;
            army.Morale = Clamp(army.Morale + delta);
            return true;
        }

        public bool AdjustSupply(EntityId armyId, double delta)
        {
            if (!_rules.SupplyEnabled || !_armies.TryGetValue(armyId, out ArmyRecord army) || !IsFinite(delta)) return false;
            army.Supply = Clamp(army.Supply + delta);
            return true;
        }

        public bool TryGetState(EntityId armyId, out ArmySnapshot snapshot)
        {
            if (!_armies.TryGetValue(armyId, out ArmyRecord army)) { snapshot = default; return false; }
            snapshot = CreateSnapshot(army);
            return true;
        }

        public bool TryGetArmyForUnit(EntityId unitId, out EntityId armyId) => _armyByMember.TryGetValue(unitId, out armyId);

        public IReadOnlyList<ArmySnapshot> Snapshot()
        {
            var result = new List<ArmySnapshot>(_armies.Count);
            foreach (ArmyRecord army in _armies.Values) result.Add(CreateSnapshot(army));
            result.Sort((left, right) => left.ArmyId.CompareTo(right.ArmyId));
            return result.AsReadOnly();
        }

        public string GetDebugSummary()
        {
            int units = 0;
            int commanded = 0;
            foreach (ArmyRecord army in _armies.Values) { units += army.UnitIds.Count; if (army.CommanderId.IsValid) commanded++; }
            return $"Armies={_armies.Count}, Units={units}, Commanded={commanded}, Morale={_rules.MoraleEnabled}, Supply={_rules.SupplyEnabled}";
        }

        private ArmyCommandResult ExecuteOrder(ArmyOrderCommand command, Func<ArmyRecord, ArmyOrder> createOrder,
            Func<ArmyRecord, ArmyOrderExecutionResult> execute)
        {
            ArmyCommandResult validation = command is AttackSettlementArmyCommand attackSettlement
                ? Validate(attackSettlement)
                : Validate(command);
            if (!validation.Succeeded) return validation;
            ArmyRecord army = _armies[command.ArmyId];
            ArmyOrderExecutionResult execution = execute(army);
            if (!execution.Accepted) return ArmyCommandResult.Failure(execution.Error);
            army.Order = createOrder(army);
            _events?.Publish(new ArmyOrderIssuedEvent(army.ArmyId, army.Order, execution.AcceptedActorCount));
            return ArmyCommandResult.Success(army.ArmyId, execution.AcceptedActorCount);
        }

        private ArmyCommandResult ValidateCommander(EntityId commanderId, EntityId factionId, IReadOnlyList<EntityId> members)
        {
            if (!commanderId.IsValid) return ArmyCommandResult.Success(default, 0);
            if (!Contains(members, commanderId)) return ArmyCommandResult.Failure("Commander must be a member of the army.");
            return _heroes.CanCommand(commanderId, factionId)
                ? ArmyCommandResult.Success(default, 1)
                : ArmyCommandResult.Failure("Commander must be a same-faction registered hero.");
        }

        private void AssignMember(EntityId memberId, EntityId armyId)
        {
            _armyByMember[memberId] = armyId;
            if (_heroes.IsHero(memberId)) _heroes.AssignArmy(memberId, armyId);
            _membershipSink.SetArmy(memberId, armyId);
        }

        private ArmySnapshot CreateSnapshot(ArmyRecord army) => new ArmySnapshot(
            army.ArmyId, army.FactionId, army.CommanderId, new List<EntityId>(army.UnitIds).AsReadOnly(), army.Formation,
            _rules.MoraleEnabled, _rules.MoraleEnabled ? army.Morale : 0d,
            _rules.SupplyEnabled, _rules.SupplyEnabled ? army.Supply : 0d, army.Order);

        private static double Weighted(double first, int firstCount, double second, int secondCount) =>
            (first * firstCount + second * secondCount) / (firstCount + secondCount);
        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        private static double Clamp(double value) => Math.Max(0d, Math.Min(100d, value));
        private static bool Contains(IReadOnlyList<EntityId> values, EntityId expected)
        {
            foreach (EntityId value in values) if (value == expected) return true;
            return false;
        }

        private sealed class ArmyRecord
        {
            public ArmyRecord(EntityId armyId, EntityId factionId, IEnumerable<EntityId> units, EntityId commanderId,
                FormationType formation, double morale, double supply)
            {
                ArmyId = armyId; FactionId = factionId; CommanderId = commanderId; Formation = formation;
                UnitIds = new List<EntityId>(units); UnitIds.Sort(); Morale = morale; Supply = supply;
                Order = ArmyOrder.Idle(formation);
            }
            public EntityId ArmyId { get; }
            public EntityId FactionId { get; }
            public EntityId CommanderId { get; set; }
            public List<EntityId> UnitIds { get; }
            public FormationType Formation { get; }
            public double Morale { get; set; }
            public double Supply { get; set; }
            public ArmyOrder Order { get; set; }
        }
    }
}
