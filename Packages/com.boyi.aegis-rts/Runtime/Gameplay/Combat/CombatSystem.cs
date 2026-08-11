using System;
using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Abilities;
using AegisRTS.Gameplay.Units;

namespace AegisRTS.Gameplay.Combat
{
    /// <summary>Pure C# unit combat, projectile, damage, status, ability, and death simulation.</summary>
    public sealed class CombatSystem : ICombatQuery
    {
        private readonly Dictionary<EntityId, Combatant> _combatants = new Dictionary<EntityId, Combatant>();
        private readonly Dictionary<string, AbilityProfile> _abilities = new Dictionary<string, AbilityProfile>(StringComparer.Ordinal);
        private readonly List<Projectile> _projectiles = new List<Projectile>();
        private readonly EventBus _events;

        public CombatSystem(EventBus eventBus = null) => _events = eventBus;

        public int CombatantCount => _combatants.Count;
        public int ActiveProjectileCount => _projectiles.Count;

        public void Register(EntityId entityId, CombatantProfile profile, WorldPoint position)
        {
            if (!entityId.IsValid) throw new ArgumentException("Entity ID must be valid.", nameof(entityId));
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (_combatants.ContainsKey(entityId)) throw new InvalidOperationException($"Combatant {entityId} is already registered.");
            _combatants.Add(entityId, new Combatant(entityId, profile, position));
        }

        public bool Unregister(EntityId entityId) => _combatants.Remove(entityId);

        public void RegisterAbility(AbilityProfile ability)
        {
            if (ability == null) throw new ArgumentNullException(nameof(ability));
            if (_abilities.ContainsKey(ability.Id)) throw new InvalidOperationException($"Ability {ability.Id} is already registered.");
            _abilities.Add(ability.Id, ability);
        }

        public bool UpdatePosition(EntityId entityId, WorldPoint position)
        {
            if (!_combatants.TryGetValue(entityId, out Combatant combatant)) return false;
            combatant.Position = position;
            return true;
        }

        public bool UpdateArmyAssignment(EntityId entityId, EntityId armyId)
        {
            if (!_combatants.TryGetValue(entityId, out Combatant combatant)) return false;
            combatant.ArmyId = armyId;
            return true;
        }

        /// <summary>Exposes immutable authored combat configuration to cross-system adapters.</summary>
        public bool TryGetProfile(EntityId entityId, out CombatantProfile profile)
        {
            if (_combatants.TryGetValue(entityId, out Combatant combatant)) { profile = combatant.Profile; return true; }
            profile = null; return false;
        }

        public int IssueAttack(AttackTargetCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (!_combatants.TryGetValue(command.TargetId, out Combatant target) || !target.IsAlive) return 0;
            int accepted = 0;
            foreach (EntityId actorId in command.ActorIds)
            {
                if (!_combatants.TryGetValue(actorId, out Combatant actor) || !actor.IsAlive ||
                    actor.Profile.FactionId == target.Profile.FactionId || !CanTarget(actor.Profile.Attack, target.Profile.Tags))
                    continue;
                actor.TargetId = target.EntityId;
                actor.State = CombatantState.Targeting;
                accepted++;
            }
            return accepted;
        }

        public bool IssueAbility(UseAbilityCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (!_combatants.TryGetValue(command.CasterId, out Combatant caster) || !caster.IsAlive || IsStunned(caster)) return false;
            if (!_abilities.TryGetValue(command.AbilityId, out AbilityProfile ability)) return false;
            if (!Contains(caster.Profile.AbilityIds, ability.Id)) return false;
            if (ability.ActivationType != AbilityActivationType.Active && ability.ActivationType != AbilityActivationType.Toggle) return false;
            if (caster.AbilityCooldowns.TryGetValue(ability.Id, out double cooldown) && cooldown > 0d) return false;

            WorldPoint targetPoint;
            Combatant explicitTarget = null;
            if (ability.TargetType == AbilityTargetType.Self)
            {
                explicitTarget = caster;
                targetPoint = caster.Position;
            }
            else if (ability.TargetType == AbilityTargetType.Unit || ability.TargetType == AbilityTargetType.Settlement)
            {
                if (!_combatants.TryGetValue(command.TargetId, out explicitTarget) || !explicitTarget.IsAlive) return false;
                targetPoint = explicitTarget.Position;
            }
            else
            {
                targetPoint = command.TargetPoint;
            }

            if (Distance(caster.Position, targetPoint) > ability.Range) return false;
            var recipients = new List<Combatant>();
            if (explicitTarget != null)
            {
                recipients.Add(explicitTarget);
            }
            else
            {
                double radius = Math.Max(0.01d, ability.Radius);
                foreach (Combatant candidate in _combatants.Values)
                {
                    if (!candidate.IsAlive || candidate.Profile.FactionId == caster.Profile.FactionId) continue;
                    if (Distance(candidate.Position, targetPoint) <= radius) recipients.Add(candidate);
                }
            }

            foreach (Combatant recipient in recipients)
            {
                if (ability.Damage > 0d) ApplyDamage(caster, recipient, ability.Damage, ability.DamageType);
                if (ability.StatusEffect != null && recipient.IsAlive) ApplyStatusInternal(caster.EntityId, recipient, ability.StatusEffect);
            }
            caster.AbilityCooldowns[ability.Id] = ability.CooldownSeconds;
            _events?.Publish(new AbilityUsedEvent(caster.EntityId, ability.Id, command.TargetId, targetPoint));
            return true;
        }

        public bool ApplyStatus(EntityId sourceId, EntityId targetId, StatusEffectProfile status)
        {
            if (status == null) throw new ArgumentNullException(nameof(status));
            if (!_combatants.TryGetValue(targetId, out Combatant target) || !target.IsAlive) return false;
            ApplyStatusInternal(sourceId, target, status);
            return true;
        }

        public void Tick(double deltaSeconds)
        {
            if (deltaSeconds < 0d || double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds))
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (deltaSeconds == 0d) return;

            foreach (Combatant combatant in _combatants.Values)
            {
                if (!combatant.IsAlive) continue;
                TickCooldowns(combatant, deltaSeconds);
                TickStatuses(combatant, deltaSeconds);
            }
            // Advance only projectiles that existed at the start of this attack phase. Newly launched
            // projectiles begin travelling on the next simulation tick instead of consuming a full large delta.
            TickProjectiles(deltaSeconds);
            foreach (Combatant combatant in _combatants.Values)
            {
                if (combatant.IsAlive) TickAttack(combatant, deltaSeconds);
            }
        }

        public bool TryGetState(EntityId entityId, out CombatantSnapshot snapshot)
        {
            if (!_combatants.TryGetValue(entityId, out Combatant combatant))
            {
                snapshot = default;
                return false;
            }
            snapshot = CreateSnapshot(combatant);
            return true;
        }

        public IReadOnlyList<CombatantSnapshot> Snapshot()
        {
            var result = new List<CombatantSnapshot>(_combatants.Count);
            foreach (Combatant combatant in _combatants.Values) result.Add(CreateSnapshot(combatant));
            result.Sort((left, right) => left.EntityId.CompareTo(right.EntityId));
            return result.AsReadOnly();
        }

        public string GetDebugSummary()
        {
            int alive = 0;
            int dead = 0;
            int statuses = 0;
            foreach (Combatant combatant in _combatants.Values)
            {
                if (combatant.IsAlive) alive++; else dead++;
                statuses += combatant.Statuses.Count;
            }
            return $"Combatants={_combatants.Count}, Alive={alive}, Dead={dead}, Projectiles={_projectiles.Count}, Statuses={statuses}";
        }

        private void TickAttack(Combatant actor, double deltaSeconds)
        {
            if (IsStunned(actor))
            {
                actor.State = CombatantState.Stunned;
                return;
            }
            if (actor.WindupRemaining > 0d)
            {
                actor.WindupRemaining = Math.Max(0d, actor.WindupRemaining - deltaSeconds);
                if (actor.WindupRemaining <= 0d) ResolveAttack(actor);
                return;
            }
            if (!actor.TargetId.IsValid || !_combatants.TryGetValue(actor.TargetId, out Combatant target) || !target.IsAlive)
            {
                actor.TargetId = EntityId.Invalid;
                actor.State = CombatantState.Idle;
                return;
            }
            if (Distance(actor.Position, target.Position) > actor.Profile.Attack.Range)
            {
                actor.State = CombatantState.Targeting;
                return;
            }
            if (actor.AttackCooldownRemaining > 0d) return;

            actor.State = CombatantState.Windup;
            actor.WindupRemaining = actor.Profile.Attack.WindupSeconds;
            actor.AttackCooldownRemaining = actor.Profile.Attack.CooldownSeconds;
            if (actor.WindupRemaining <= 0d) ResolveAttack(actor);
        }

        private void ResolveAttack(Combatant actor)
        {
            if (!actor.TargetId.IsValid || !_combatants.TryGetValue(actor.TargetId, out Combatant target) || !target.IsAlive)
            {
                actor.State = CombatantState.Idle;
                return;
            }
            actor.State = CombatantState.Attacking;
            AttackProfile attack = actor.Profile.Attack;
            if (attack.UsesProjectile)
            {
                _projectiles.Add(new Projectile(actor.EntityId, target.EntityId, actor.Position, attack));
                _events?.Publish(new ProjectileLaunchedEvent(actor.EntityId, target.EntityId, actor.Position, target.Position, attack.ProjectileSpeed));
            }
            else
            {
                ResolveImpact(actor, target, target.Position, attack);
            }
        }

        private void TickProjectiles(double deltaSeconds)
        {
            for (int index = _projectiles.Count - 1; index >= 0; index--)
            {
                Projectile projectile = _projectiles[index];
                if (!_combatants.TryGetValue(projectile.TargetId, out Combatant target) || !target.IsAlive)
                {
                    _projectiles.RemoveAt(index);
                    continue;
                }
                double distance = Distance(projectile.Position, target.Position);
                double step = projectile.Attack.ProjectileSpeed * deltaSeconds;
                if (distance <= step || distance < 0.05d)
                {
                    if (_combatants.TryGetValue(projectile.SourceId, out Combatant source))
                        ResolveImpact(source, target, target.Position, projectile.Attack);
                    _projectiles.RemoveAt(index);
                    continue;
                }
                projectile.Position = MoveTowards(projectile.Position, target.Position, step / distance);
            }
        }

        private void ResolveImpact(Combatant source, Combatant target, WorldPoint impactPoint, AttackProfile attack)
        {
            if (attack.SplashRadius <= 0d)
            {
                ApplyDamage(source, target, attack.Damage, attack.DamageType);
                return;
            }
            foreach (Combatant candidate in _combatants.Values)
            {
                if (!candidate.IsAlive || candidate.Profile.FactionId == source.Profile.FactionId) continue;
                if (Distance(candidate.Position, impactPoint) <= attack.SplashRadius)
                    ApplyDamage(source, candidate, attack.Damage, attack.DamageType);
            }
        }

        private void ApplyDamage(Combatant source, Combatant target, double baseDamage, DamageType damageType)
        {
            if (!target.IsAlive) return;
            double modifier = 1d;
            foreach (ActiveStatus status in source.Statuses)
            {
                if (status.Profile.Kind == StatusEffectKind.Buff) modifier += status.Profile.Magnitude;
                else if (status.Profile.Kind == StatusEffectKind.Debuff) modifier -= status.Profile.Magnitude;
            }
            double modified = Math.Max(0d, baseDamage * Math.Max(0d, modifier));
            double defended = damageType == DamageType.Physical
                ? Math.Max(0d, modified - target.Profile.Defense.Armor)
                : modified;
            double resistance = damageType == DamageType.Physical
                ? target.Profile.Defense.PhysicalResistance
                : damageType == DamageType.Magical ? target.Profile.Defense.MagicalResistance : 0d;
            double finalDamage = defended * (1d - resistance);

            for (int index = target.Statuses.Count - 1; index >= 0 && finalDamage > 0d; index--)
            {
                ActiveStatus status = target.Statuses[index];
                if (status.Profile.Kind != StatusEffectKind.Shield) continue;
                double absorbed = Math.Min(finalDamage, status.RemainingMagnitude);
                status.RemainingMagnitude -= absorbed;
                finalDamage -= absorbed;
                if (status.RemainingMagnitude <= 0d) target.Statuses.RemoveAt(index);
            }

            target.Health = Math.Max(0d, target.Health - finalDamage);
            _events?.Publish(new DamageAppliedEvent(source.EntityId, target.EntityId, damageType, finalDamage, target.Health));
            if (target.Health > 0d) return;
            target.State = CombatantState.Dead;
            target.TargetId = EntityId.Invalid;
            target.WindupRemaining = 0d;
            _events?.Publish(new UnitDiedEvent(target.EntityId, source.EntityId));
        }

        private void ApplyStatusInternal(EntityId sourceId, Combatant target, StatusEffectProfile profile)
        {
            target.Statuses.RemoveAll(status => string.Equals(status.Profile.Id, profile.Id, StringComparison.Ordinal));
            target.Statuses.Add(new ActiveStatus(sourceId, profile));
            _events?.Publish(new StatusAppliedEvent(sourceId, target.EntityId, profile.Id, profile.Kind));
        }

        private void TickStatuses(Combatant target, double deltaSeconds)
        {
            var damageTicks = new List<StatusDamageTick>();
            for (int index = target.Statuses.Count - 1; index >= 0; index--)
            {
                ActiveStatus status = target.Statuses[index];
                double activeDelta = Math.Min(deltaSeconds, status.RemainingSeconds);
                status.RemainingSeconds -= deltaSeconds;
                if (status.Profile.Kind == StatusEffectKind.DamageOverTime)
                {
                    status.TickRemaining -= activeDelta;
                    while (status.TickRemaining <= 0d)
                    {
                        status.TickRemaining += status.Profile.TickIntervalSeconds;
                        damageTicks.Add(new StatusDamageTick(status.SourceId, status.Profile.Magnitude, status.Profile.DamageType));
                    }
                }
                if (status.RemainingSeconds <= 0d) target.Statuses.RemoveAt(index);
            }

            foreach (StatusDamageTick tick in damageTicks)
            {
                if (!target.IsAlive) break;
                Combatant source = _combatants.TryGetValue(tick.SourceId, out Combatant found) ? found : target;
                ApplyDamage(source, target, tick.Damage, tick.DamageType);
            }
        }

        private static void TickCooldowns(Combatant combatant, double deltaSeconds)
        {
            combatant.AttackCooldownRemaining = Math.Max(0d, combatant.AttackCooldownRemaining - deltaSeconds);
            var keys = new List<string>(combatant.AbilityCooldowns.Keys);
            foreach (string key in keys) combatant.AbilityCooldowns[key] = Math.Max(0d, combatant.AbilityCooldowns[key] - deltaSeconds);
        }

        private static CombatantSnapshot CreateSnapshot(Combatant combatant)
        {
            var cooldowns = new Dictionary<string, double>(combatant.AbilityCooldowns, StringComparer.Ordinal);
            var statuses = new List<StatusEffectSnapshot>(combatant.Statuses.Count);
            foreach (ActiveStatus status in combatant.Statuses)
                statuses.Add(new StatusEffectSnapshot(
                    status.Profile.Id, status.Profile.Kind, Math.Max(0d, status.RemainingSeconds), status.RemainingMagnitude));
            return new CombatantSnapshot(
                combatant.EntityId,
                combatant.Profile.FactionId,
                combatant.ArmyId,
                combatant.State,
                combatant.TargetId,
                combatant.Position,
                combatant.Health,
                combatant.Profile.MaxHealth,
                combatant.AttackCooldownRemaining,
                MovementMultiplier(combatant),
                cooldowns,
                statuses.AsReadOnly());
        }

        private static double MovementMultiplier(Combatant combatant)
        {
            if (HasStatus(combatant, StatusEffectKind.Root) || HasStatus(combatant, StatusEffectKind.Stun)) return 0d;
            double multiplier = 1d;
            foreach (ActiveStatus status in combatant.Statuses)
                if (status.Profile.Kind == StatusEffectKind.Slow) multiplier *= Math.Max(0d, 1d - status.Profile.Magnitude);
            return multiplier;
        }

        private static bool IsStunned(Combatant combatant) => HasStatus(combatant, StatusEffectKind.Stun);
        private static bool HasStatus(Combatant combatant, StatusEffectKind kind) =>
            combatant.Statuses.Exists(status => status.Profile.Kind == kind);

        private static bool CanTarget(AttackProfile attack, IReadOnlyList<string> targetTags)
        {
            if (attack.TargetTags.Count == 0) return true;
            foreach (string required in attack.TargetTags)
                if (Contains(targetTags, required)) return true;
            return false;
        }

        private static bool Contains(IReadOnlyList<string> values, string expected)
        {
            foreach (string value in values)
                if (string.Equals(value, expected, StringComparison.Ordinal)) return true;
            return false;
        }

        private static double Distance(WorldPoint left, WorldPoint right)
        {
            double x = right.X - left.X;
            double y = right.Y - left.Y;
            double z = right.Z - left.Z;
            return Math.Sqrt(x * x + y * y + z * z);
        }

        private static WorldPoint MoveTowards(WorldPoint current, WorldPoint target, double ratio) => new WorldPoint(
            current.X + (target.X - current.X) * ratio,
            current.Y + (target.Y - current.Y) * ratio,
            current.Z + (target.Z - current.Z) * ratio);

        private sealed class Combatant
        {
            public Combatant(EntityId entityId, CombatantProfile profile, WorldPoint position)
            { EntityId = entityId; Profile = profile; ArmyId = profile.ArmyId; Position = position; Health = profile.MaxHealth; State = CombatantState.Idle; }
            public EntityId EntityId { get; }
            public CombatantProfile Profile { get; }
            public EntityId ArmyId { get; set; }
            public WorldPoint Position { get; set; }
            public double Health { get; set; }
            public CombatantState State { get; set; }
            public EntityId TargetId { get; set; }
            public double AttackCooldownRemaining { get; set; }
            public double WindupRemaining { get; set; }
            public List<ActiveStatus> Statuses { get; } = new List<ActiveStatus>();
            public Dictionary<string, double> AbilityCooldowns { get; } = new Dictionary<string, double>(StringComparer.Ordinal);
            public bool IsAlive => State != CombatantState.Dead;
        }

        private sealed class ActiveStatus
        {
            public ActiveStatus(EntityId sourceId, StatusEffectProfile profile)
            { SourceId = sourceId; Profile = profile; RemainingSeconds = profile.DurationSeconds; TickRemaining = profile.TickIntervalSeconds; RemainingMagnitude = profile.Magnitude; }
            public EntityId SourceId { get; }
            public StatusEffectProfile Profile { get; }
            public double RemainingSeconds { get; set; }
            public double TickRemaining { get; set; }
            public double RemainingMagnitude { get; set; }
        }

        private sealed class Projectile
        {
            public Projectile(EntityId sourceId, EntityId targetId, WorldPoint position, AttackProfile attack)
            { SourceId = sourceId; TargetId = targetId; Position = position; Attack = attack; }
            public EntityId SourceId { get; }
            public EntityId TargetId { get; }
            public WorldPoint Position { get; set; }
            public AttackProfile Attack { get; }
        }

        private readonly struct StatusDamageTick
        {
            public StatusDamageTick(EntityId sourceId, double damage, DamageType damageType)
            { SourceId = sourceId; Damage = damage; DamageType = damageType; }
            public EntityId SourceId { get; }
            public double Damage { get; }
            public DamageType DamageType { get; }
        }
    }
}
