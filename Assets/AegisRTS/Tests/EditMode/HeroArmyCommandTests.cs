using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Armies;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Formation;
using AegisRTS.Gameplay.Heroes;
using AegisRTS.Gameplay.Units;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class HeroArmyCommandTests
    {
        private static readonly EntityId FactionA = new EntityId(100);
        private static readonly EntityId FactionB = new EntityId(200);

        [Test]
        public void HeroSystem_AddsLeadershipAndAbilityComponentWithoutCombatState()
        {
            var heroes = new HeroSystem();
            EntityId heroId = new EntityId(1);
            heroes.Register(heroId, new HeroProfile("hero.debug", FactionA, 75, new[] { "ability.rally" }));

            Assert.That(heroes.TryGetState(heroId, out HeroSnapshot state), Is.True);
            Assert.That(state.Profile.Leadership, Is.EqualTo(75));
            Assert.That(state.Profile.AbilityIds, Is.EqualTo(new[] { "ability.rally" }));
            Assert.That(state.IsAssigned, Is.False);
            Assert.That(heroes.GetDebugSummary(), Does.Contain("Heroes=1"));
        }

        [Test]
        public void HeroAndTwentyInfantry_CreateSplitMergeAndChangeCommander()
        {
            var events = new EventBus();
            var heroes = new HeroSystem();
            var armies = new ArmySystem(heroes, eventBus: events);
            EntityId firstHero = new EntityId(1);
            EntityId secondHero = new EntityId(2);
            heroes.Register(firstHero, new HeroProfile("hero.first", FactionA, 80));
            heroes.Register(secondHero, new HeroProfile("hero.second", FactionA, 65));
            var members = new List<EntityId> { firstHero };
            for (ulong value = 2; value <= 21; value++) members.Add(new EntityId(value));
            foreach (EntityId member in members) armies.RegisterMember(member, FactionA);
            int splitEvents = 0;
            int mergeEvents = 0;
            events.Subscribe<ArmySplitEvent>(_ => splitEvents++);
            events.Subscribe<ArmiesMergedEvent>(_ => mergeEvents++);
            EntityId firstArmy = new EntityId(500);
            EntityId secondArmy = new EntityId(501);

            Assert.That(armies.Execute(new CreateArmyCommand(firstArmy, FactionA, members, firstHero)).Succeeded, Is.True);
            Assert.That(State(armies, firstArmy).UnitCount, Is.EqualTo(21));
            var splitMembers = new List<EntityId>();
            for (ulong value = 2; value <= 11; value++) splitMembers.Add(new EntityId(value));
            Assert.That(armies.Execute(new SplitArmyCommand(firstArmy, secondArmy, splitMembers, secondHero)).Succeeded, Is.True);
            Assert.That(State(armies, firstArmy).UnitCount, Is.EqualTo(11));
            Assert.That(State(armies, secondArmy).UnitCount, Is.EqualTo(10));
            Assert.That(armies.Execute(new MergeArmiesCommand(firstArmy, secondArmy)).Succeeded, Is.True);
            Assert.That(armies.Execute(new AssignArmyCommanderCommand(firstArmy, secondHero)).Succeeded, Is.True);

            Assert.That(armies.ArmyCount, Is.EqualTo(1));
            Assert.That(State(armies, firstArmy).UnitCount, Is.EqualTo(21));
            Assert.That(State(armies, firstArmy).CommanderId, Is.EqualTo(secondHero));
            Assert.That(heroes.TryGetState(secondHero, out HeroSnapshot heroState), Is.True);
            Assert.That(heroState.ArmyId, Is.EqualTo(firstArmy));
            Assert.That(splitEvents, Is.EqualTo(1));
            Assert.That(mergeEvents, Is.EqualTo(1));
        }

        [Test]
        public void InvalidCommanderAndCrossFactionMerge_AreRejectedBeforeMutation()
        {
            var heroes = new HeroSystem();
            var armies = new ArmySystem(heroes);
            EntityId hero = new EntityId(1);
            EntityId unitA = new EntityId(2);
            EntityId unitB = new EntityId(3);
            heroes.Register(hero, new HeroProfile("hero.a", FactionA, 50));
            armies.RegisterMember(hero, FactionA);
            armies.RegisterMember(unitA, FactionA);
            armies.RegisterMember(unitB, FactionB);

            Assert.That(armies.Execute(new CreateArmyCommand(new EntityId(10), FactionA, new[] { hero, unitA }, hero)).Succeeded, Is.True);
            Assert.That(armies.Execute(new CreateArmyCommand(new EntityId(11), FactionB, new[] { unitB })).Succeeded, Is.True);
            Assert.That(armies.Validate(new MergeArmiesCommand(new EntityId(10), new EntityId(11))).Succeeded, Is.False);
            Assert.That(armies.Validate(new AssignArmyCommanderCommand(new EntityId(10), unitA)).Succeeded, Is.False);
            Assert.That(armies.ArmyCount, Is.EqualTo(2));
        }

        [Test]
        public void MoraleAndSupply_OnlyChangeWhenOptionalRulesAreEnabled()
        {
            EntityId hero = new EntityId(1);
            EntityId armyId = new EntityId(10);
            var disabledHeroes = new HeroSystem();
            disabledHeroes.Register(hero, new HeroProfile("hero.a", FactionA, 50));
            var disabled = new ArmySystem(disabledHeroes, new ArmyRuleOptions(false, false));
            disabled.RegisterMember(hero, FactionA);
            disabled.Execute(new CreateArmyCommand(armyId, FactionA, new[] { hero }, hero));
            Assert.That(disabled.AdjustMorale(armyId, -30), Is.False);
            Assert.That(disabled.AdjustSupply(armyId, -30), Is.False);

            var enabledHeroes = new HeroSystem();
            enabledHeroes.Register(hero, new HeroProfile("hero.a", FactionA, 50));
            var enabled = new ArmySystem(enabledHeroes, new ArmyRuleOptions(true, true));
            enabled.RegisterMember(hero, FactionA);
            enabled.Execute(new CreateArmyCommand(armyId, FactionA, new[] { hero }, hero));
            Assert.That(enabled.AdjustMorale(armyId, -130), Is.True);
            Assert.That(enabled.AdjustSupply(armyId, -25), Is.True);
            Assert.That(State(enabled, armyId).Morale, Is.Zero);
            Assert.That(State(enabled, armyId).Supply, Is.EqualTo(75));
        }

        [Test]
        public void ArmyRuntimeState_RestoresMoraleSupplyAndCurrentOrder()
        {
            var heroes = new HeroSystem();
            var armies = new ArmySystem(heroes, new ArmyRuleOptions(true, true));
            EntityId hero = new EntityId(1);
            EntityId armyId = new EntityId(10);
            heroes.Register(hero, new HeroProfile("hero.a", FactionA, 50));
            armies.RegisterMember(hero, FactionA);
            armies.Execute(new CreateArmyCommand(armyId, FactionA, new[] { hero }, hero));
            var order = new ArmyOrder(ArmyOrderType.Retreat, Point(-8), EntityId.Invalid, FormationType.Line);

            Assert.That(armies.RestoreRuntimeState(armyId, 45, 61, order), Is.True);
            ArmySnapshot restored = State(armies, armyId);
            Assert.That(restored.Morale, Is.EqualTo(45));
            Assert.That(restored.Supply, Is.EqualTo(61));
            Assert.That(restored.Order.Type, Is.EqualTo(ArmyOrderType.Retreat));
            Assert.That(restored.Order.Destination, Is.EqualTo(Point(-8)));
        }

        [Test]
        public void SharedCommandBus_ValidatesAndRoutesAllArmyOrders()
        {
            var events = new EventBus();
            var commands = new CommandBus();
            var heroes = new HeroSystem();
            var executor = new RecordingOrderExecutor();
            var armies = new ArmySystem(heroes, orderExecutor: executor, eventBus: events);
            EntityId hero = new EntityId(1);
            EntityId unit = new EntityId(2);
            EntityId armyId = new EntityId(10);
            heroes.Register(hero, new HeroProfile("hero.a", FactionA, 50));
            armies.RegisterMember(hero, FactionA);
            armies.RegisterMember(unit, FactionA);
            int orderEvents = 0;
            events.Subscribe<ArmyOrderIssuedEvent>(_ => orderEvents++);
            using (var router = new ArmyCommandRouter(commands, armies))
            {
                Assert.That(commands.Dispatch(new CreateArmyCommand(armyId, FactionA, new[] { hero, unit }, hero)).WasHandled, Is.True);
                Assert.That(commands.Dispatch(new MoveArmyCommand(armyId, Point(5))).WasHandled, Is.True);
                Assert.That(commands.Dispatch(new AttackArmyCommand(armyId, new EntityId(99))).WasHandled, Is.True);
                Assert.That(commands.Dispatch(new AttackSettlementArmyCommand(armyId, new EntityId(98))).WasHandled, Is.True);
                Assert.That(commands.Dispatch(new DefendArmyCommand(armyId, Point(3))).WasHandled, Is.True);
                Assert.That(commands.Dispatch(new RetreatArmyCommand(armyId, Point(-5))).WasHandled, Is.True);
                Assert.That(commands.Dispatch(new MoveArmyCommand(new EntityId(404), Point(1))).WasHandled, Is.False);
            }

            Assert.That(executor.Calls, Is.EqualTo(new[] { "Move", "Attack", "AttackSettlement", "Defend", "Retreat" }));
            Assert.That(orderEvents, Is.EqualTo(5));
            Assert.That(State(armies, armyId).Order.Type, Is.EqualTo(ArmyOrderType.Retreat));
            Assert.That(commands.RegisteredHandlerCount, Is.Zero);
            Assert.That(commands.RegisteredValidatorCount, Is.Zero);
        }

        [Test]
        public void FailedOrderExecution_DoesNotReplaceCurrentOrder()
        {
            var heroes = new HeroSystem();
            var executor = new RecordingOrderExecutor { Reject = true };
            var armies = new ArmySystem(heroes, orderExecutor: executor);
            EntityId hero = new EntityId(1);
            EntityId armyId = new EntityId(10);
            heroes.Register(hero, new HeroProfile("hero.a", FactionA, 50));
            armies.RegisterMember(hero, FactionA);
            armies.Execute(new CreateArmyCommand(armyId, FactionA, new[] { hero }, hero));

            Assert.That(armies.Execute(new MoveArmyCommand(armyId, Point(5))).Succeeded, Is.False);
            Assert.That(State(armies, armyId).Order.Type, Is.EqualTo(ArmyOrderType.Idle));
        }

        [Test]
        public void ArmyMembership_PropagatesToExistingCombatSnapshot()
        {
            var heroes = new HeroSystem();
            var combat = new CombatSystem();
            EntityId hero = new EntityId(1);
            EntityId armyId = new EntityId(10);
            heroes.Register(hero, new HeroProfile("hero.a", FactionA, 50));
            combat.Register(hero, new CombatantProfile("hero.a", FactionA, 100,
                new AttackProfile(10, DamageType.Physical, 2, 1, 0)), Point(0));
            var armies = new ArmySystem(heroes, membershipSink: new CombatArmyMembershipSink(combat));
            armies.RegisterMember(hero, FactionA);

            Assert.That(armies.Execute(new CreateArmyCommand(armyId, FactionA, new[] { hero }, hero)).Succeeded, Is.True);
            Assert.That(combat.TryGetState(hero, out CombatantSnapshot combatant), Is.True);
            Assert.That(combatant.ArmyId, Is.EqualTo(armyId));
        }

        [Test]
        public void UnregisterMember_RemovesAssignedUnitAndCommanderFromArmyAndCombat()
        {
            var heroes = new HeroSystem();
            var combat = new CombatSystem();
            EntityId hero = new EntityId(1);
            EntityId unit = new EntityId(2);
            EntityId armyId = new EntityId(10);
            heroes.Register(hero, new HeroProfile("hero.a", FactionA, 50));
            combat.Register(hero, new CombatantProfile("hero.a", FactionA, 100,
                new AttackProfile(10, DamageType.Physical, 2, 1, 0)), Point(0));
            combat.Register(unit, new CombatantProfile("unit.a", FactionA, 100,
                new AttackProfile(10, DamageType.Physical, 2, 1, 0)), Point(0));
            var armies = new ArmySystem(heroes, membershipSink: new CombatArmyMembershipSink(combat));
            armies.RegisterMember(hero, FactionA);
            armies.RegisterMember(unit, FactionA);
            armies.Execute(new CreateArmyCommand(armyId, FactionA, new[] { hero, unit }, hero));

            Assert.That(armies.UnregisterMember(unit), Is.True);
            Assert.That(armies.TryGetArmyForUnit(unit, out _), Is.False);
            CollectionAssert.DoesNotContain((System.Collections.ICollection)State(armies, armyId).UnitIds, unit);
            Assert.That(combat.TryGetState(unit, out CombatantSnapshot unitCombat), Is.True);
            Assert.That(unitCombat.ArmyId.IsValid, Is.False);
            Assert.That(armies.UnregisterMember(hero), Is.True);
            Assert.That(State(armies, armyId).CommanderId.IsValid, Is.False);
            Assert.That(heroes.TryGetState(hero, out HeroSnapshot heroState), Is.True);
            Assert.That(heroState.ArmyId.IsValid, Is.False);
        }

        private static ArmySnapshot State(ArmySystem armies, EntityId armyId)
        {
            Assert.That(armies.TryGetState(armyId, out ArmySnapshot state), Is.True);
            return state;
        }

        private static WorldPoint Point(double x) => new WorldPoint(x, 0, 0);

        private sealed class RecordingOrderExecutor : IArmyOrderExecutor
        {
            public List<string> Calls { get; } = new List<string>();
            public bool Reject { get; set; }
            public ArmyOrderExecutionResult Move(IReadOnlyList<EntityId> units, WorldPoint destination, FormationType formation) => Record("Move", units.Count);
            public ArmyOrderExecutionResult Attack(IReadOnlyList<EntityId> units, EntityId targetId) => Record("Attack", units.Count);
            public ArmyOrderExecutionResult AttackSettlement(IReadOnlyList<EntityId> units, EntityId targetId) => Record("AttackSettlement", units.Count);
            public ArmyOrderExecutionResult Defend(IReadOnlyList<EntityId> units, WorldPoint position, FormationType formation) => Record("Defend", units.Count);
            public ArmyOrderExecutionResult Retreat(IReadOnlyList<EntityId> units, WorldPoint destination, FormationType formation) => Record("Retreat", units.Count);
            private ArmyOrderExecutionResult Record(string name, int count)
            {
                Calls.Add(name);
                return Reject ? ArmyOrderExecutionResult.Failure("Rejected by test.") : ArmyOrderExecutionResult.Success(count);
            }
        }
    }
}
