using System;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Units;

namespace AegisRTS.Gameplay.Abilities
{
    public enum AbilityTargetType { Self, Unit, Point, Area, Direction, Settlement }
    public enum AbilityActivationType { Active, Passive, Aura, Triggered, Toggle }

    public sealed class AbilityProfile
    {
        public AbilityProfile(
            string id,
            AbilityTargetType targetType,
            AbilityActivationType activationType,
            double cooldownSeconds,
            double range,
            double radius = 0d,
            double damage = 0d,
            DamageType damageType = DamageType.Physical,
            StatusEffectProfile statusEffect = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Ability ID is required.", nameof(id));
            if (cooldownSeconds < 0d || range < 0d || radius < 0d || damage < 0d)
                throw new ArgumentOutOfRangeException(nameof(cooldownSeconds));
            Id = id.Trim();
            TargetType = targetType;
            ActivationType = activationType;
            CooldownSeconds = cooldownSeconds;
            Range = range;
            Radius = radius;
            Damage = damage;
            DamageType = damageType;
            StatusEffect = statusEffect;
        }

        public string Id { get; }
        public AbilityTargetType TargetType { get; }
        public AbilityActivationType ActivationType { get; }
        public double CooldownSeconds { get; }
        public double Range { get; }
        public double Radius { get; }
        public double Damage { get; }
        public DamageType DamageType { get; }
        public StatusEffectProfile StatusEffect { get; }
    }

    public sealed class UseAbilityCommand : ICommand
    {
        public UseAbilityCommand(
            EntityId casterId,
            string abilityId,
            EntityId targetId = default,
            WorldPoint targetPoint = default,
            double directionX = 0d,
            double directionZ = 1d)
        {
            if (!casterId.IsValid) throw new ArgumentException("Caster ID must be valid.", nameof(casterId));
            if (string.IsNullOrWhiteSpace(abilityId)) throw new ArgumentException("Ability ID is required.", nameof(abilityId));
            if (double.IsNaN(directionX) || double.IsInfinity(directionX) ||
                double.IsNaN(directionZ) || double.IsInfinity(directionZ))
                throw new ArgumentOutOfRangeException(nameof(directionX));
            CasterId = casterId;
            AbilityId = abilityId.Trim();
            TargetId = targetId;
            TargetPoint = targetPoint;
            DirectionX = directionX;
            DirectionZ = directionZ;
        }

        public EntityId CasterId { get; }
        public string AbilityId { get; }
        public EntityId TargetId { get; }
        public WorldPoint TargetPoint { get; }
        public double DirectionX { get; }
        public double DirectionZ { get; }
    }

    public sealed class AbilityUsedEvent : IEvent
    {
        public AbilityUsedEvent(EntityId casterId, string abilityId, EntityId targetId, WorldPoint targetPoint)
        {
            CasterId = casterId;
            AbilityId = abilityId;
            TargetId = targetId;
            TargetPoint = targetPoint;
        }

        public EntityId CasterId { get; }
        public string AbilityId { get; }
        public EntityId TargetId { get; }
        public WorldPoint TargetPoint { get; }
    }
}
