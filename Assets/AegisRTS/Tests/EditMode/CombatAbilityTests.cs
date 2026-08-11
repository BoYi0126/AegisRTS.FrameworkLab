using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Abilities;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Units;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class CombatAbilityTests
    {
        private static readonly EntityId Blue = new EntityId(101);
        private static readonly EntityId Red = new EntityId(102);

        [Test]
        public void MeleeAttack_UsesWindupCooldownAndDamagePipeline()
        {
            var events = new EventBus();
            var combat = new CombatSystem(events);
            EntityId attacker = new EntityId(1);
            EntityId target = new EntityId(2);
            DamageAppliedEvent damageEvent = null;
            events.Subscribe<DamageAppliedEvent>(value => damageEvent = value);
            Register(combat, attacker, Blue, Point(0), Attack(50, range: 2, cooldown: 1, windup: 0.25),
                abilities: null, defense: null, tags: new[] { "ground" });
            Register(combat, target, Red, Point(1), Attack(0), abilities: null,
                defense: new DefenseProfile(armor: 10, physicalResistance: 0.25), tags: new[] { "ground" });

            Assert.That(combat.IssueAttack(new AttackTargetCommand(new[] { attacker }, target)), Is.EqualTo(1));
            combat.Tick(0.1);
            Assert.That(State(combat, target).Health, Is.EqualTo(100));
            combat.Tick(0.3);

            Assert.That(State(combat, target).Health, Is.EqualTo(70).Within(0.001));
            Assert.That(damageEvent.Amount, Is.EqualTo(30).Within(0.001));
            Assert.That(State(combat, attacker).AttackCooldownRemaining, Is.GreaterThan(0));
        }

        [Test]
        public void RangedProjectile_TravelsBeforeApplyingDamage()
        {
            var events = new EventBus();
            var combat = new CombatSystem(events);
            EntityId attacker = new EntityId(1);
            EntityId target = new EntityId(2);
            int launched = 0;
            events.Subscribe<ProjectileLaunchedEvent>(_ => launched++);
            Register(combat, attacker, Blue, Point(0), Attack(25, range: 12, projectileSpeed: 5));
            Register(combat, target, Red, Point(10), Attack(0));

            combat.IssueAttack(new AttackTargetCommand(new[] { attacker }, target));
            combat.Tick(0.01);

            Assert.That(launched, Is.EqualTo(1));
            Assert.That(combat.ActiveProjectileCount, Is.EqualTo(1));
            Assert.That(State(combat, target).Health, Is.EqualTo(100));
            combat.Tick(2.1);
            Assert.That(State(combat, target).Health, Is.EqualTo(75));
        }

        [Test]
        public void SplashImpact_DamagesEnemiesInRadiusButNotAlliesOrDistantUnits()
        {
            var combat = new CombatSystem();
            EntityId attacker = new EntityId(1);
            EntityId target = new EntityId(2);
            EntityId nearbyEnemy = new EntityId(3);
            EntityId nearbyAlly = new EntityId(4);
            EntityId distantEnemy = new EntityId(5);
            Register(combat, attacker, Blue, Point(0), Attack(20, range: 15, splash: 2));
            Register(combat, target, Red, Point(10), Attack(0));
            Register(combat, nearbyEnemy, Red, Point(11), Attack(0));
            Register(combat, nearbyAlly, Blue, Point(11), Attack(0));
            Register(combat, distantEnemy, Red, Point(14), Attack(0));

            combat.IssueAttack(new AttackTargetCommand(new[] { attacker }, target));
            combat.Tick(0.01);

            Assert.That(State(combat, target).Health, Is.EqualTo(80));
            Assert.That(State(combat, nearbyEnemy).Health, Is.EqualTo(80));
            Assert.That(State(combat, nearbyAlly).Health, Is.EqualTo(100));
            Assert.That(State(combat, distantEnemy).Health, Is.EqualTo(100));
        }

        [Test]
        public void TargetTags_BlockInvalidAttackTargets()
        {
            var combat = new CombatSystem();
            EntityId attacker = new EntityId(1);
            EntityId airTarget = new EntityId(2);
            Register(combat, attacker, Blue, Point(0), Attack(10, targetTags: new[] { "ground" }));
            Register(combat, airTarget, Red, Point(1), Attack(0), tags: new[] { "air" });

            Assert.That(combat.IssueAttack(new AttackTargetCommand(new[] { attacker }, airTarget)), Is.Zero);
        }

        [Test]
        public void StunSlowRootShieldAndDot_UpdateStateSafely()
        {
            var events = new EventBus();
            var combat = new CombatSystem(events);
            EntityId source = new EntityId(1);
            EntityId target = new EntityId(2);
            int deaths = 0;
            events.Subscribe<UnitDiedEvent>(_ => deaths++);
            Register(combat, source, Blue, Point(0), Attack(20));
            Register(combat, target, Red, Point(1), Attack(10));

            combat.ApplyStatus(source, target, new StatusEffectProfile("slow", StatusEffectKind.Slow, 3, 0.4));
            Assert.That(State(combat, target).MovementSpeedMultiplier, Is.EqualTo(0.6).Within(0.001));
            combat.ApplyStatus(source, target, new StatusEffectProfile("root", StatusEffectKind.Root, 1, 0));
            Assert.That(State(combat, target).MovementSpeedMultiplier, Is.Zero);
            combat.ApplyStatus(source, target, new StatusEffectProfile("shield", StatusEffectKind.Shield, 4, 15));
            combat.ApplyStatus(source, target, new StatusEffectProfile("burn", StatusEffectKind.DamageOverTime, 3, 40, 1));

            combat.Tick(1.1);
            Assert.That(State(combat, target).Health, Is.EqualTo(75));
            combat.Tick(2.1);
            Assert.That(State(combat, target).IsAlive, Is.False);
            Assert.That(deaths, Is.EqualTo(1));
        }

        [Test]
        public void StunnedAttacker_CannotBeginAnAttackUntilStatusExpires()
        {
            var combat = new CombatSystem();
            EntityId attacker = new EntityId(1);
            EntityId target = new EntityId(2);
            Register(combat, attacker, Blue, Point(0), Attack(20));
            Register(combat, target, Red, Point(1), Attack(0));
            combat.ApplyStatus(target, attacker, new StatusEffectProfile("stun", StatusEffectKind.Stun, 1, 0));
            combat.IssueAttack(new AttackTargetCommand(new[] { attacker }, target));

            combat.Tick(0.5);
            Assert.That(State(combat, target).Health, Is.EqualTo(100));
            Assert.That(State(combat, attacker).State, Is.EqualTo(CombatantState.Stunned));
            combat.Tick(0.6);
            Assert.That(State(combat, target).Health, Is.EqualTo(80));
        }

        [Test]
        public void ActiveAreaAbility_AppliesDamageStatusAndCooldown()
        {
            var combat = new CombatSystem();
            EntityId caster = new EntityId(1);
            EntityId first = new EntityId(2);
            EntityId second = new EntityId(3);
            var burn = new StatusEffectProfile("burn", StatusEffectKind.DamageOverTime, 2, 5, 1);
            var ability = new AbilityProfile("firestorm", AbilityTargetType.Area, AbilityActivationType.Active,
                cooldownSeconds: 4, range: 10, radius: 2, damage: 15, damageType: DamageType.Magical, statusEffect: burn);
            combat.RegisterAbility(ability);
            Register(combat, caster, Blue, Point(0), Attack(0), abilities: new[] { "firestorm" });
            Register(combat, first, Red, Point(5), Attack(0));
            Register(combat, second, Red, Point(6), Attack(0));
            var command = new UseAbilityCommand(caster, "firestorm", targetPoint: Point(5));

            Assert.That(combat.IssueAbility(command), Is.True);
            Assert.That(combat.IssueAbility(command), Is.False);
            Assert.That(State(combat, first).Health, Is.EqualTo(85));
            Assert.That(State(combat, second).StatusCount, Is.EqualTo(1));
            Assert.That(State(combat, caster).AbilityCooldowns["firestorm"], Is.EqualTo(4));
            Assert.That(State(combat, second).Statuses[0].Id, Is.EqualTo("burn"));
            combat.Tick(4.1);
            Assert.That(combat.IssueAbility(command), Is.True);
        }

        [Test]
        public void SelfAbility_BuffsFollowingAttackAndDeathPublishesOnce()
        {
            var events = new EventBus();
            var combat = new CombatSystem(events);
            EntityId caster = new EntityId(1);
            EntityId target = new EntityId(2);
            int deaths = 0;
            events.Subscribe<UnitDiedEvent>(_ => deaths++);
            var buff = new StatusEffectProfile("rage", StatusEffectKind.Buff, 3, 0.5);
            combat.RegisterAbility(new AbilityProfile("rage", AbilityTargetType.Self, AbilityActivationType.Active,
                3, 0, statusEffect: buff));
            Register(combat, caster, Blue, Point(0), Attack(80), abilities: new[] { "rage" });
            Register(combat, target, Red, Point(1), Attack(0));

            Assert.That(combat.IssueAbility(new UseAbilityCommand(caster, "rage")), Is.True);
            combat.IssueAttack(new AttackTargetCommand(new[] { caster }, target));
            combat.Tick(0.01);
            combat.Tick(2);

            Assert.That(State(combat, target).IsAlive, Is.False);
            Assert.That(deaths, Is.EqualTo(1));
        }

        private static AttackProfile Attack(double damage, double range = 2, double cooldown = 1,
            double windup = 0, double projectileSpeed = 0, double splash = 0, IEnumerable<string> targetTags = null) =>
            new AttackProfile(damage, DamageType.Physical, range, cooldown, windup, projectileSpeed, splash, targetTags);

        private static void Register(CombatSystem combat, EntityId id, EntityId faction, WorldPoint position,
            AttackProfile attack, IEnumerable<string> abilities = null, DefenseProfile defense = null,
            IEnumerable<string> tags = null) =>
            combat.Register(id, new CombatantProfile("unit.test", faction, 100, attack, defense,
                tags: tags ?? new[] { "ground" }, abilityIds: abilities), position);

        private static CombatantSnapshot State(CombatSystem combat, EntityId id)
        {
            Assert.That(combat.TryGetState(id, out CombatantSnapshot state), Is.True);
            return state;
        }

        private static WorldPoint Point(double x) => new WorldPoint(x, 0, 0);
    }
}
