using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Settlements;

namespace AegisRTS.Gameplay.Siege
{
    public enum SiegeArea { OuterArea, Walls, Gates, Towers, Breach, InnerArea, CaptureObjective }
    public enum DefenseStructureKind { Wall, Gate, Tower, Barricade, Trap, Core, Extension }
    public enum GateState { Closed, Opening, Open, Closing, Destroyed }
    public enum SiegeMode { Assault, Defense, WaveDefense, Survival, EscortSiege, BossSiege }
    public enum SiegeState { Preparing, Active, Breached, InnerAreaContested, CaptureAvailable, Completed, Failed }

    public sealed class SiegeProfile
    {
        public SiegeProfile(EntityId settlementId, EntityId attackerFactionId, EntityId defenderFactionId,
            SiegeMode mode, EntityId capturingArmyId = default, double timeLimitSeconds = 0d, int requiredWaves = 0)
        {
            if (!settlementId.IsValid || !attackerFactionId.IsValid || !defenderFactionId.IsValid ||
                attackerFactionId == defenderFactionId) throw new ArgumentException("Siege participants must be valid and opposing.");
            if (timeLimitSeconds < 0d || double.IsNaN(timeLimitSeconds) || double.IsInfinity(timeLimitSeconds) || requiredWaves < 0)
                throw new ArgumentOutOfRangeException(nameof(timeLimitSeconds));
            SettlementId = settlementId; AttackerFactionId = attackerFactionId; DefenderFactionId = defenderFactionId;
            Mode = mode; CapturingArmyId = capturingArmyId; TimeLimitSeconds = timeLimitSeconds; RequiredWaves = requiredWaves;
        }
        public EntityId SettlementId { get; }
        public EntityId AttackerFactionId { get; }
        public EntityId DefenderFactionId { get; }
        public SiegeMode Mode { get; }
        public EntityId CapturingArmyId { get; }
        public double TimeLimitSeconds { get; }
        public int RequiredWaves { get; }
    }

    public sealed class DefenseStructureProfile
    {
        public DefenseStructureProfile(string definitionId, DefenseStructureKind kind, SiegeArea area,
            EntityId factionId, double maxHealth, double armor = 0d, string extensionTypeId = null,
            IEnumerable<string> tags = null, bool repairable = false)
        {
            if (string.IsNullOrWhiteSpace(definitionId)) throw new ArgumentException("Definition ID is required.", nameof(definitionId));
            if (!factionId.IsValid) throw new ArgumentException("Faction ID must be valid.", nameof(factionId));
            if (maxHealth <= 0d || armor < 0d || !IsFinite(maxHealth) || !IsFinite(armor)) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            DefinitionId = definitionId.Trim(); Kind = kind; Area = area; FactionId = factionId;
            MaxHealth = maxHealth; Armor = armor; ExtensionTypeId = extensionTypeId?.Trim() ?? string.Empty;
            var values = new List<string> { "structure", KindTag(kind) };
            foreach (string tag in tags ?? Array.Empty<string>()) if (!string.IsNullOrWhiteSpace(tag) && !values.Contains(tag.Trim())) values.Add(tag.Trim());
            Tags = values.AsReadOnly();
            Repairable = repairable;
        }
        public string DefinitionId { get; }
        public DefenseStructureKind Kind { get; }
        public SiegeArea Area { get; }
        public EntityId FactionId { get; }
        public double MaxHealth { get; }
        public double Armor { get; }
        public string ExtensionTypeId { get; }
        public IReadOnlyList<string> Tags { get; }
        public bool Repairable { get; }

        public static DefenseStructureProfile FromDefinition(DefenseStructureDefinition definition, EntityId factionId)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            DefenseStructureKind kind = ParseKind(definition.StructureTypeId);
            return new DefenseStructureProfile(definition.Id.Value, kind, ParseArea(definition.SiegeAreaId), factionId,
                definition.MaxHealth, definition.Armor, kind == DefenseStructureKind.Extension ? definition.StructureTypeId : null,
                TagValues(definition.Tags), HasTag(definition.Tags, "repairable"));
        }

        private static DefenseStructureKind ParseKind(string value) => value == "wall" ? DefenseStructureKind.Wall
            : value == "gate" ? DefenseStructureKind.Gate : value == "tower" ? DefenseStructureKind.Tower
            : value == "barricade" ? DefenseStructureKind.Barricade : value == "trap" ? DefenseStructureKind.Trap
            : value == "core" ? DefenseStructureKind.Core : DefenseStructureKind.Extension;
        private static SiegeArea ParseArea(string value) => value == "walls" ? SiegeArea.Walls
            : value == "gates" ? SiegeArea.Gates : value == "towers" ? SiegeArea.Towers
            : value == "breach" ? SiegeArea.Breach : value == "inner-area" ? SiegeArea.InnerArea
            : value == "capture-objective" ? SiegeArea.CaptureObjective : SiegeArea.OuterArea;
        private static string KindTag(DefenseStructureKind value) => value.ToString().ToLowerInvariant();
        private static IReadOnlyList<string> TagValues(IReadOnlyList<ContentTag> tags)
        {
            var values = new List<string>();
            if (tags != null) foreach (ContentTag tag in tags) values.Add(tag.Value);
            return values.AsReadOnly();
        }
        private static bool HasTag(IReadOnlyList<ContentTag> tags, string expected)
        {
            if (tags == null) return false;
            foreach (ContentTag tag in tags)
                if (string.Equals(tag.Value, expected, StringComparison.Ordinal)) return true;
            return false;
        }
        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }

    public readonly struct DefenseStructureSnapshot
    {
        public DefenseStructureSnapshot(EntityId structureId, DefenseStructureProfile profile, double health, GateState gateState)
        { StructureId = structureId; Profile = profile; Health = health; GateState = gateState; }
        public EntityId StructureId { get; }
        public DefenseStructureProfile Profile { get; }
        public double Health { get; }
        public GateState GateState { get; }
        public bool IsDestroyed => Health <= 0d;
    }

    public readonly struct SiegeSnapshot
    {
        public SiegeSnapshot(EntityId siegeId, SiegeProfile profile, SiegeState state, SiegeArea currentArea,
            CaptureCondition completedConditions, double elapsedSeconds, int completedWaves,
            EntityId winningFactionId, IReadOnlyList<DefenseStructureSnapshot> structures)
        { SiegeId = siegeId; Profile = profile; State = state; CurrentArea = currentArea; CompletedConditions = completedConditions;
            ElapsedSeconds = elapsedSeconds; CompletedWaves = completedWaves; WinningFactionId = winningFactionId; Structures = structures; }
        public EntityId SiegeId { get; }
        public SiegeProfile Profile { get; }
        public SiegeState State { get; }
        public SiegeArea CurrentArea { get; }
        public CaptureCondition CompletedConditions { get; }
        public double ElapsedSeconds { get; }
        public int CompletedWaves { get; }
        public EntityId WinningFactionId { get; }
        public IReadOnlyList<DefenseStructureSnapshot> Structures { get; }
    }

    public readonly struct SiegeActionResult
    {
        private SiegeActionResult(bool succeeded, string error) { Succeeded = succeeded; Error = error ?? string.Empty; }
        public bool Succeeded { get; }
        public string Error { get; }
        public static SiegeActionResult Success() => new SiegeActionResult(true, string.Empty);
        public static SiegeActionResult Failure(string error) => new SiegeActionResult(false, error);
    }

    public readonly struct SiegeAttackerSnapshot
    {
        public SiegeAttackerSnapshot(EntityId factionId, AttackProfile attack, IReadOnlyList<string> tags)
        { FactionId = factionId; Attack = attack; Tags = tags ?? Array.Empty<string>(); }
        public EntityId FactionId { get; }
        public AttackProfile Attack { get; }
        public IReadOnlyList<string> Tags { get; }
    }

    public interface ISiegeAttackerQuery { bool TryGetAttacker(EntityId entityId, out SiegeAttackerSnapshot attacker); }
    public interface ISiegeNavigationSink { void RefreshAfterBreach(EntityId siegeId, EntityId structureId, SiegeArea openedArea); }
    public interface ISiegeCaptureSink
    {
        SiegeActionResult Capture(EntityId settlementId, EntityId newOwnerId, CaptureCondition conditions, EntityId capturingArmyId);
    }
    public interface ISiegeRule
    {
        SiegeActionResult CanEnter(SiegeSnapshot siege, SiegeArea targetArea);
        SiegeActionResult CanCapture(SiegeSnapshot siege);
    }

    public sealed class StartSiegeCommand : ICommand
    { public StartSiegeCommand(EntityId siegeId) { if (!siegeId.IsValid) throw new ArgumentException("Siege ID must be valid."); SiegeId = siegeId; } public EntityId SiegeId { get; } }
    public sealed class AttackDefenseStructureCommand : ICommand
    { public AttackDefenseStructureCommand(EntityId siegeId, EntityId attackerId, EntityId structureId) { if (!siegeId.IsValid || !attackerId.IsValid || !structureId.IsValid) throw new ArgumentException("Siege, attacker, and structure IDs must be valid."); SiegeId = siegeId; AttackerId = attackerId; StructureId = structureId; } public EntityId SiegeId { get; } public EntityId AttackerId { get; } public EntityId StructureId { get; } }
    public sealed class RepairDefenseStructureCommand : ICommand
    {
        public RepairDefenseStructureCommand(EntityId siegeId, EntityId repairerId, EntityId structureId, double amount)
        {
            if (!siegeId.IsValid || !repairerId.IsValid || !structureId.IsValid)
                throw new ArgumentException("Siege, repairer, and structure IDs must be valid.");
            if (amount <= 0d || double.IsNaN(amount) || double.IsInfinity(amount))
                throw new ArgumentOutOfRangeException(nameof(amount));
            SiegeId = siegeId; RepairerId = repairerId; StructureId = structureId; Amount = amount;
        }
        public EntityId SiegeId { get; }
        public EntityId RepairerId { get; }
        public EntityId StructureId { get; }
        public double Amount { get; }
    }
    public sealed class SetGateStateCommand : ICommand
    { public SetGateStateCommand(EntityId siegeId, EntityId gateId, GateState state) { if (!siegeId.IsValid || !gateId.IsValid) throw new ArgumentException("Siege and gate IDs must be valid."); SiegeId = siegeId; GateId = gateId; State = state; } public EntityId SiegeId { get; } public EntityId GateId { get; } public GateState State { get; } }
    public sealed class EnterSiegeAreaCommand : ICommand
    { public EnterSiegeAreaCommand(EntityId siegeId, SiegeArea area) { if (!siegeId.IsValid) throw new ArgumentException("Siege ID must be valid."); SiegeId = siegeId; Area = area; } public EntityId SiegeId { get; } public SiegeArea Area { get; } }
    public sealed class ReportSiegeConditionCommand : ICommand
    { public ReportSiegeConditionCommand(EntityId siegeId, CaptureCondition condition) { if (!siegeId.IsValid || condition == CaptureCondition.None) throw new ArgumentException("Siege ID and condition must be valid."); SiegeId = siegeId; Condition = condition; } public EntityId SiegeId { get; } public CaptureCondition Condition { get; } }
    public sealed class CompleteSiegeWaveCommand : ICommand
    { public CompleteSiegeWaveCommand(EntityId siegeId) { if (!siegeId.IsValid) throw new ArgumentException("Siege ID must be valid."); SiegeId = siegeId; } public EntityId SiegeId { get; } }
    public sealed class CaptureSiegeCommand : ICommand
    { public CaptureSiegeCommand(EntityId siegeId) { if (!siegeId.IsValid) throw new ArgumentException("Siege ID must be valid."); SiegeId = siegeId; } public EntityId SiegeId { get; } }

    public sealed class SiegeStartedEvent : IEvent
    { public SiegeStartedEvent(EntityId siegeId, SiegeMode mode) { SiegeId = siegeId; Mode = mode; } public EntityId SiegeId { get; } public SiegeMode Mode { get; } }
    public sealed class DefenseStructureDamagedEvent : IEvent
    { public DefenseStructureDamagedEvent(EntityId siegeId, EntityId structureId, double damage, double health) { SiegeId = siegeId; StructureId = structureId; Damage = damage; RemainingHealth = health; } public EntityId SiegeId { get; } public EntityId StructureId { get; } public double Damage { get; } public double RemainingHealth { get; } }
    public sealed class DefenseStructureDestroyedEvent : IEvent
    { public DefenseStructureDestroyedEvent(EntityId siegeId, EntityId structureId, DefenseStructureKind kind) { SiegeId = siegeId; StructureId = structureId; Kind = kind; } public EntityId SiegeId { get; } public EntityId StructureId { get; } public DefenseStructureKind Kind { get; } }
    public sealed class DefenseStructureRepairedEvent : IEvent
    {
        public DefenseStructureRepairedEvent(EntityId siegeId, EntityId structureId, double amount, double health)
        { SiegeId = siegeId; StructureId = structureId; Amount = amount; Health = health; }
        public EntityId SiegeId { get; }
        public EntityId StructureId { get; }
        public double Amount { get; }
        public double Health { get; }
    }
    public sealed class BreachSealedEvent : IEvent
    { public BreachSealedEvent(EntityId siegeId, EntityId structureId) { SiegeId = siegeId; StructureId = structureId; } public EntityId SiegeId { get; } public EntityId StructureId { get; } }
    public sealed class GateStateChangedEvent : IEvent
    { public GateStateChangedEvent(EntityId siegeId, EntityId gateId, GateState state) { SiegeId = siegeId; GateId = gateId; State = state; } public EntityId SiegeId { get; } public EntityId GateId { get; } public GateState State { get; } }
    public sealed class BreachCreatedEvent : IEvent
    { public BreachCreatedEvent(EntityId siegeId, EntityId structureId) { SiegeId = siegeId; StructureId = structureId; } public EntityId SiegeId { get; } public EntityId StructureId { get; } }
    public sealed class SiegeAreaEnteredEvent : IEvent
    { public SiegeAreaEnteredEvent(EntityId siegeId, SiegeArea area) { SiegeId = siegeId; Area = area; } public EntityId SiegeId { get; } public SiegeArea Area { get; } }
    public sealed class SiegeCompletedEvent : IEvent
    { public SiegeCompletedEvent(EntityId siegeId, EntityId winnerId) { SiegeId = siegeId; WinningFactionId = winnerId; } public EntityId SiegeId { get; } public EntityId WinningFactionId { get; } }
}
