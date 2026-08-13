using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Units;

namespace AegisRTS.Gameplay.Combat
{
    public enum DamageType { Physical, Magical, True }
    public enum CombatantState { Idle, Targeting, Windup, Attacking, Stunned, Dead }
    public enum StatusEffectKind { Buff, Debuff, Stun, Slow, Root, Shield, DamageOverTime }

    /// <summary>Controls autonomous target acquisition and pursuit without overriding explicit attack orders.</summary>
    public enum UnitEngagementMode
    {
        HoldGround,
        Normal,
        Aggressive,
        Retaliate,
    }

    public enum EngagementTargetReason
    {
        None,
        ManualOrder,
        Proactive,
        Retaliation,
    }

    public static class UnitEngagementRules
    {
        public static bool AllowsProactiveAttack(UnitEngagementMode mode) => mode != UnitEngagementMode.Retaliate;

        public static double DefenseRangeMultiplier(UnitEngagementMode mode)
        {
            switch (mode)
            {
                case UnitEngagementMode.HoldGround: return 0.5d;
                case UnitEngagementMode.Normal: return 1d;
                case UnitEngagementMode.Aggressive: return 1.5d;
                case UnitEngagementMode.Retaliate: return 0d;
                default: throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }
    }

    public sealed class SetUnitEngagementModeCommand : UnitCommand
    {
        public SetUnitEngagementModeCommand(IEnumerable<EntityId> actorIds, UnitEngagementMode mode)
            : base(actorIds, false) => Mode = mode;

        public UnitEngagementMode Mode { get; }
    }

    /// <summary>Read-only combat state exposed to presentation, UI, AI, and tests.</summary>
    public interface ICombatQuery
    {
        bool TryGetState(EntityId entityId, out CombatantSnapshot snapshot);
        IReadOnlyList<CombatantSnapshot> Snapshot();
        string GetDebugSummary();
    }

    public sealed class AttackProfile
    {
        public AttackProfile(
            double damage,
            DamageType damageType,
            double range,
            double cooldownSeconds,
            double windupSeconds,
            double projectileSpeed = 0d,
            double splashRadius = 0d,
            IEnumerable<string> targetTags = null)
        {
            if (damage < 0d || range < 0d || cooldownSeconds < 0d || windupSeconds < 0d ||
                projectileSpeed < 0d || splashRadius < 0d)
                throw new ArgumentOutOfRangeException(nameof(damage));
            Damage = damage;
            DamageType = damageType;
            Range = range;
            CooldownSeconds = cooldownSeconds;
            WindupSeconds = windupSeconds;
            ProjectileSpeed = projectileSpeed;
            SplashRadius = splashRadius;
            TargetTags = CopyStrings(targetTags);
        }

        public double Damage { get; }
        public DamageType DamageType { get; }
        public double Range { get; }
        public double CooldownSeconds { get; }
        /// <summary>Total time from one attack start to the next attack start.</summary>
        public double AttackIntervalSeconds => CooldownSeconds;
        /// <summary>Attacks per second. Zero represents an intentionally unthrottled test/profile interval.</summary>
        public double AttacksPerSecond => CooldownSeconds > 0d ? 1d / CooldownSeconds : 0d;
        public double WindupSeconds { get; }
        /// <summary>Post-impact time before the next attack. Movement may cancel its presentation, not its cooldown.</summary>
        public double RecoverySeconds => Math.Max(0d, CooldownSeconds - WindupSeconds);
        /// <summary>Universal orb-walk rule: the entire post-impact recovery animation is move-cancelable.</summary>
        public double MoveCancelableBackswingSeconds => RecoverySeconds;
        public double ProjectileSpeed { get; }
        public double SplashRadius { get; }
        public bool UsesProjectile => ProjectileSpeed > 0d;
        public IReadOnlyList<string> TargetTags { get; }

        private static IReadOnlyList<string> CopyStrings(IEnumerable<string> values)
        {
            var result = new List<string>();
            foreach (string value in values ?? Array.Empty<string>())
                if (!string.IsNullOrWhiteSpace(value)) result.Add(value.Trim());
            return result.AsReadOnly();
        }
    }

    public sealed class DefenseProfile
    {
        public DefenseProfile(double armor = 0d, double physicalResistance = 0d, double magicalResistance = 0d)
        {
            if (armor < 0d || physicalResistance < 0d || physicalResistance > 0.95d ||
                magicalResistance < 0d || magicalResistance > 0.95d)
                throw new ArgumentOutOfRangeException(nameof(armor));
            Armor = armor;
            PhysicalResistance = physicalResistance;
            MagicalResistance = magicalResistance;
        }

        public double Armor { get; }
        public double PhysicalResistance { get; }
        public double MagicalResistance { get; }
    }

    public sealed class StatusEffectProfile
    {
        public StatusEffectProfile(
            string id,
            StatusEffectKind kind,
            double durationSeconds,
            double magnitude,
            double tickIntervalSeconds = 1d,
            DamageType damageType = DamageType.True)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Status ID is required.", nameof(id));
            if (durationSeconds <= 0d || magnitude < 0d || tickIntervalSeconds <= 0d)
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            Id = id.Trim();
            Kind = kind;
            DurationSeconds = durationSeconds;
            Magnitude = magnitude;
            TickIntervalSeconds = tickIntervalSeconds;
            DamageType = damageType;
        }

        public string Id { get; }
        public StatusEffectKind Kind { get; }
        public double DurationSeconds { get; }
        public double Magnitude { get; }
        public double TickIntervalSeconds { get; }
        public DamageType DamageType { get; }
    }

    public sealed class CombatantProfile
    {
        public CombatantProfile(
            string definitionId,
            EntityId factionId,
            double maxHealth,
            AttackProfile attack,
            DefenseProfile defense = null,
            EntityId armyId = default,
            IEnumerable<string> tags = null,
            IEnumerable<string> abilityIds = null)
        {
            if (string.IsNullOrWhiteSpace(definitionId)) throw new ArgumentException("Definition ID is required.", nameof(definitionId));
            if (!factionId.IsValid) throw new ArgumentException("Faction ID must be valid.", nameof(factionId));
            if (maxHealth <= 0d) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            DefinitionId = definitionId.Trim();
            FactionId = factionId;
            ArmyId = armyId;
            MaxHealth = maxHealth;
            Attack = attack ?? throw new ArgumentNullException(nameof(attack));
            Defense = defense ?? new DefenseProfile();
            Tags = Copy(tags);
            AbilityIds = Copy(abilityIds);
        }

        public string DefinitionId { get; }
        public EntityId FactionId { get; }
        public EntityId ArmyId { get; }
        public double MaxHealth { get; }
        public AttackProfile Attack { get; }
        public DefenseProfile Defense { get; }
        public IReadOnlyList<string> Tags { get; }
        public IReadOnlyList<string> AbilityIds { get; }

        private static IReadOnlyList<string> Copy(IEnumerable<string> values)
        {
            var result = new List<string>();
            foreach (string value in values ?? Array.Empty<string>())
                if (!string.IsNullOrWhiteSpace(value)) result.Add(value.Trim());
            return result.AsReadOnly();
        }
    }

    public readonly struct CombatantSnapshot
    {
        public CombatantSnapshot(
            EntityId entityId,
            EntityId factionId,
            EntityId armyId,
            CombatantState state,
            EntityId targetId,
            WorldPoint position,
            double health,
            double maxHealth,
            double attackCooldownRemaining,
            double movementSpeedMultiplier,
            IReadOnlyDictionary<string, double> abilityCooldowns,
            IReadOnlyList<StatusEffectSnapshot> statuses,
            UnitEngagementMode engagementMode,
            EngagementTargetReason targetReason,
            WorldPoint engagementOrigin,
            double defenseRange,
            bool shouldReturnToOrigin)
        {
            EntityId = entityId;
            FactionId = factionId;
            ArmyId = armyId;
            State = state;
            TargetId = targetId;
            Position = position;
            Health = health;
            MaxHealth = maxHealth;
            AttackCooldownRemaining = attackCooldownRemaining;
            MovementSpeedMultiplier = movementSpeedMultiplier;
            AbilityCooldowns = abilityCooldowns ?? throw new ArgumentNullException(nameof(abilityCooldowns));
            Statuses = statuses ?? throw new ArgumentNullException(nameof(statuses));
            EngagementMode = engagementMode;
            TargetReason = targetReason;
            EngagementOrigin = engagementOrigin;
            DefenseRange = defenseRange;
            ShouldReturnToOrigin = shouldReturnToOrigin;
        }

        public EntityId EntityId { get; }
        public EntityId FactionId { get; }
        public EntityId ArmyId { get; }
        public CombatantState State { get; }
        public EntityId TargetId { get; }
        public WorldPoint Position { get; }
        public double Health { get; }
        public double MaxHealth { get; }
        public double AttackCooldownRemaining { get; }
        public double MovementSpeedMultiplier { get; }
        public IReadOnlyDictionary<string, double> AbilityCooldowns { get; }
        public IReadOnlyList<StatusEffectSnapshot> Statuses { get; }
        public UnitEngagementMode EngagementMode { get; }
        public EngagementTargetReason TargetReason { get; }
        public WorldPoint EngagementOrigin { get; }
        public double DefenseRange { get; }
        public bool ShouldReturnToOrigin { get; }
        public bool AllowsProactiveAttack => UnitEngagementRules.AllowsProactiveAttack(EngagementMode);
        public int StatusCount => Statuses.Count;
        public bool IsAlive => State != CombatantState.Dead;
    }

    public readonly struct StatusEffectSnapshot
    {
        public StatusEffectSnapshot(string id, StatusEffectKind kind, double remainingSeconds, double remainingMagnitude)
        { Id = id; Kind = kind; RemainingSeconds = remainingSeconds; RemainingMagnitude = remainingMagnitude; }
        public string Id { get; }
        public StatusEffectKind Kind { get; }
        public double RemainingSeconds { get; }
        public double RemainingMagnitude { get; }
    }

    public sealed class DamageAppliedEvent : IEvent
    {
        public DamageAppliedEvent(EntityId sourceId, EntityId targetId, DamageType damageType, double amount, double remainingHealth)
        { SourceId = sourceId; TargetId = targetId; DamageType = damageType; Amount = amount; RemainingHealth = remainingHealth; }
        public EntityId SourceId { get; }
        public EntityId TargetId { get; }
        public DamageType DamageType { get; }
        public double Amount { get; }
        public double RemainingHealth { get; }
    }

    public sealed class ProjectileLaunchedEvent : IEvent
    {
        public ProjectileLaunchedEvent(EntityId sourceId, EntityId targetId, WorldPoint origin, WorldPoint destination, double speed)
        { SourceId = sourceId; TargetId = targetId; Origin = origin; Destination = destination; Speed = speed; }
        public EntityId SourceId { get; }
        public EntityId TargetId { get; }
        public WorldPoint Origin { get; }
        public WorldPoint Destination { get; }
        public double Speed { get; }
    }

    public sealed class StatusAppliedEvent : IEvent
    {
        public StatusAppliedEvent(EntityId sourceId, EntityId targetId, string statusId, StatusEffectKind kind)
        { SourceId = sourceId; TargetId = targetId; StatusId = statusId; Kind = kind; }
        public EntityId SourceId { get; }
        public EntityId TargetId { get; }
        public string StatusId { get; }
        public StatusEffectKind Kind { get; }
    }

    public sealed class UnitDiedEvent : IEvent
    {
        public UnitDiedEvent(EntityId entityId, EntityId killerId) { EntityId = entityId; KillerId = killerId; }
        public EntityId EntityId { get; }
        public EntityId KillerId { get; }
    }

    public sealed class UnitEngagementModeChangedEvent : IEvent
    {
        public UnitEngagementModeChangedEvent(EntityId entityId, UnitEngagementMode mode, WorldPoint origin)
        { EntityId = entityId; Mode = mode; Origin = origin; }
        public EntityId EntityId { get; }
        public UnitEngagementMode Mode { get; }
        public WorldPoint Origin { get; }
    }

    public sealed class EngagementTargetChangedEvent : IEvent
    {
        public EngagementTargetChangedEvent(EntityId entityId, EntityId targetId, EngagementTargetReason reason)
        { EntityId = entityId; TargetId = targetId; Reason = reason; }
        public EntityId EntityId { get; }
        public EntityId TargetId { get; }
        public EngagementTargetReason Reason { get; }
    }
}
