using System;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Content;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Factions;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Siege;
using AegisRTS.Gameplay.Territory;
using AegisRTS.Gameplay.Units;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class SiegeSystemTests
    {
        private static readonly EntityId SiegeId = new EntityId(1);
        private static readonly EntityId SettlementId = new EntityId(2);
        private static readonly EntityId AttackerFaction = new EntityId(3);
        private static readonly EntityId DefenderFaction = new EntityId(4);
        private static readonly EntityId Attacker = new EntityId(5);
        private static readonly EntityId Gate = new EntityId(6);

        [Test]
        public void DefenseStructureDefinition_LoadsWorldSpecificGateWithoutCoreBranch()
        {
            ContentPack fantasy = ContentPackTestFactory.LoadDemoPack("DemoFantasy");
            Assert.That(fantasy.DefenseStructures.Count, Is.EqualTo(1));
            DefenseStructureDefinition definition = fantasy.DefenseStructures[0];
            DefenseStructureProfile profile = DefenseStructureProfile.FromDefinition(definition, DefenderFaction);
            Assert.That(definition.Id.Value, Is.EqualTo("fantasy.arcane-gate"));
            Assert.That(profile.Kind, Is.EqualTo(DefenseStructureKind.Gate));
            Assert.That(profile.Area, Is.EqualTo(SiegeArea.Gates));
        }

        [Test]
        public void GateState_OnlyAllowsOrderedTransitions()
        {
            SiegeSystem sieges = CreateSiege(out _, out _);
            sieges.Execute(new StartSiegeCommand(SiegeId));
            Assert.That(sieges.Execute(new SetGateStateCommand(SiegeId, Gate, GateState.Open)).Succeeded, Is.False);
            Assert.That(sieges.Execute(new SetGateStateCommand(SiegeId, Gate, GateState.Opening)).Succeeded, Is.True);
            Assert.That(sieges.Execute(new SetGateStateCommand(SiegeId, Gate, GateState.Open)).Succeeded, Is.True);
            Assert.That(State(sieges).Structures[0].GateState, Is.EqualTo(GateState.Open));
        }

        [Test]
        public void OrdinaryCombatUnitTagsAndAttackProfile_DriveSiegeAttack()
        {
            var combat = new CombatSystem();
            combat.Register(Attacker, new CombatantProfile("unit.ram", AttackerFaction, 100,
                new AttackProfile(40, DamageType.Physical, 2, 1, 0, targetTags: new[] { "structure" }),
                tags: new[] { "unit", "siege" }), new WorldPoint(0, 0, 0));
            var navigation = new RecordingSiegeNavigationSink();
            var sieges = new SiegeSystem(new CombatSiegeAttackerQuery(combat), navigation);
            sieges.Register(SiegeId, Profile());
            sieges.RegisterStructure(SiegeId, Gate, GateProfile(100, 10));
            sieges.Execute(new StartSiegeCommand(SiegeId));

            Assert.That(sieges.Execute(new AttackDefenseStructureCommand(SiegeId, Attacker, Gate)).Succeeded, Is.True);
            Assert.That(State(sieges).Structures[0].Health, Is.EqualTo(70));
            Assert.That(navigation.RefreshCount, Is.Zero);
        }

        [Test]
        public void DestroyedGate_CreatesBreachAndRefreshesNavigation()
        {
            var events = new EventBus(); int breaches = 0, destroyed = 0;
            events.Subscribe<BreachCreatedEvent>(_ => breaches++);
            events.Subscribe<DefenseStructureDestroyedEvent>(_ => destroyed++);
            SiegeSystem sieges = CreateSiege(out RecordingSiegeNavigationSink navigation, out _, events, 120);
            sieges.Execute(new StartSiegeCommand(SiegeId));

            Assert.That(sieges.Execute(new AttackDefenseStructureCommand(SiegeId, Attacker, Gate)).Succeeded, Is.True);

            Assert.That(State(sieges).State, Is.EqualTo(SiegeState.Breached));
            Assert.That(State(sieges).Structures[0].GateState, Is.EqualTo(GateState.Destroyed));
            Assert.That(navigation.RefreshCount, Is.EqualTo(1));
            Assert.That(breaches, Is.EqualTo(1));
            Assert.That(destroyed, Is.EqualTo(1));
        }

        [Test]
        public void InnerArea_RequiresOpenOrDestroyedPassage()
        {
            SiegeSystem sieges = CreateSiege(out _, out _, damage: 120);
            sieges.Execute(new StartSiegeCommand(SiegeId));
            Assert.That(sieges.Execute(new EnterSiegeAreaCommand(SiegeId, SiegeArea.InnerArea)).Succeeded, Is.False);
            sieges.Execute(new AttackDefenseStructureCommand(SiegeId, Attacker, Gate));
            Assert.That(sieges.Execute(new EnterSiegeAreaCommand(SiegeId, SiegeArea.InnerArea)).Succeeded, Is.True);
            Assert.That(sieges.Execute(new EnterSiegeAreaCommand(SiegeId, SiegeArea.CaptureObjective)).Succeeded, Is.True);
            Assert.That(State(sieges).CompletedConditions.HasFlag(CaptureCondition.ZoneControlled), Is.True);
        }

        [Test]
        public void Assault_BreaksGateEntersCapturesAndChangesSettlementOwner()
        {
            var events = new EventBus();
            var factions = new FactionSystem(events); factions.Register(AttackerFaction, new FactionProfile("attacker")); factions.Register(DefenderFaction, new FactionProfile("defender"));
            var territories = new TerritorySystem(factions, events); EntityId territoryId = new EntityId(20);
            territories.RegisterNode(territoryId, new TerritoryNodeProfile("territory", 10, SettlementId), DefenderFaction);
            var settlements = new SettlementSystem(factions, territories, events);
            settlements.Register(SettlementId, new SettlementProfile("settlement", 100, 100, new CaptureRule(CaptureRuleType.CaptureZone)), DefenderFaction);
            var attackQuery = new FakeAttackerQuery(AttackerFaction, 120);
            var sieges = new SiegeSystem(attackQuery, new RecordingSiegeNavigationSink(), new SettlementSiegeCaptureSink(settlements), eventBus: events);
            sieges.Register(SiegeId, Profile()); sieges.RegisterStructure(SiegeId, Gate, GateProfile());

            sieges.Execute(new StartSiegeCommand(SiegeId));
            sieges.Execute(new AttackDefenseStructureCommand(SiegeId, Attacker, Gate));
            sieges.Execute(new EnterSiegeAreaCommand(SiegeId, SiegeArea.InnerArea));
            sieges.Execute(new EnterSiegeAreaCommand(SiegeId, SiegeArea.CaptureObjective));
            Assert.That(sieges.Execute(new CaptureSiegeCommand(SiegeId)).Succeeded, Is.True);

            Assert.That(State(sieges).WinningFactionId, Is.EqualTo(AttackerFaction));
            Assert.That(settlements.TryGetState(SettlementId, out SettlementSnapshot settlement), Is.True);
            Assert.That(settlement.OwnerId, Is.EqualTo(AttackerFaction));
            Assert.That(territories.TryGetState(territoryId, out TerritorySnapshot territory), Is.True);
            Assert.That(territory.OwnerId, Is.EqualTo(AttackerFaction));
        }

        [Test]
        public void CombatDeathBridge_ProducesCommanderAndClearDefenderConditions()
        {
            var events = new EventBus(); SiegeSystem sieges = CreateSiege(out _, out _, events);
            EntityId commander = new EntityId(30), guard = new EntityId(31);
            sieges.RegisterDefenders(SiegeId, new[] { commander, guard }, commander);
            sieges.Execute(new StartSiegeCommand(SiegeId));
            using (new SiegeCombatEventBridge(events, sieges))
            {
                events.Publish(new UnitDiedEvent(commander, Attacker));
                events.Publish(new UnitDiedEvent(guard, Attacker));
            }
            Assert.That(State(sieges).CompletedConditions.HasFlag(CaptureCondition.CommanderKilled), Is.True);
            Assert.That(State(sieges).CompletedConditions.HasFlag(CaptureCondition.DefendersCleared), Is.True);
        }

        [Test]
        public void WaveDefenseAndSurvival_CompleteForDefenderThroughSharedSystem()
        {
            var wave = new SiegeSystem(new FakeAttackerQuery(AttackerFaction, 1));
            wave.Register(SiegeId, new SiegeProfile(SettlementId, AttackerFaction, DefenderFaction, SiegeMode.WaveDefense, requiredWaves: 2));
            wave.Execute(new StartSiegeCommand(SiegeId)); wave.Execute(new CompleteSiegeWaveCommand(SiegeId)); wave.Execute(new CompleteSiegeWaveCommand(SiegeId));
            Assert.That(State(wave).WinningFactionId, Is.EqualTo(DefenderFaction));

            var survival = new SiegeSystem(new FakeAttackerQuery(AttackerFaction, 1));
            survival.Register(SiegeId, new SiegeProfile(SettlementId, AttackerFaction, DefenderFaction, SiegeMode.Survival, timeLimitSeconds: 5));
            survival.Execute(new StartSiegeCommand(SiegeId)); survival.Tick(5);
            Assert.That(State(survival).WinningFactionId, Is.EqualTo(DefenderFaction));
            Assert.That(State(survival).State, Is.EqualTo(SiegeState.Completed));
        }

        [Test]
        public void SharedCommandRouter_RejectsCaptureBeforeObjectiveAndDisposesRegistrations()
        {
            var commands = new CommandBus(); SiegeSystem sieges = CreateSiege(out _, out _);
            using (new SiegeCommandRouter(commands, sieges))
            {
                Assert.That(commands.Dispatch(new StartSiegeCommand(SiegeId)).WasHandled, Is.True);
                Assert.That(commands.Dispatch(new CaptureSiegeCommand(SiegeId)).WasHandled, Is.False);
            }
            Assert.That(commands.RegisteredHandlerCount, Is.Zero);
            Assert.That(commands.RegisteredValidatorCount, Is.Zero);
        }

        [TestCase(SiegeMode.Assault)] [TestCase(SiegeMode.Defense)] [TestCase(SiegeMode.WaveDefense)]
        [TestCase(SiegeMode.Survival)] [TestCase(SiegeMode.EscortSiege)] [TestCase(SiegeMode.BossSiege)]
        public void AllRequiredSiegeModes_AreDataOptions(SiegeMode mode)
        {
            var sieges = new SiegeSystem(new FakeAttackerQuery(AttackerFaction, 1));
            sieges.Register(SiegeId, new SiegeProfile(SettlementId, AttackerFaction, DefenderFaction, mode));
            Assert.That(State(sieges).Profile.Mode, Is.EqualTo(mode));
        }

        private static SiegeSystem CreateSiege(out RecordingSiegeNavigationSink navigation, out FakeCaptureSink capture,
            EventBus events = null, double damage = 20)
        {
            navigation = new RecordingSiegeNavigationSink(); capture = new FakeCaptureSink();
            var sieges = new SiegeSystem(new FakeAttackerQuery(AttackerFaction, damage), navigation, capture, eventBus: events);
            sieges.Register(SiegeId, Profile()); sieges.RegisterStructure(SiegeId, Gate, GateProfile()); return sieges;
        }
        private static SiegeProfile Profile() => new SiegeProfile(SettlementId, AttackerFaction, DefenderFaction, SiegeMode.Assault);
        private static DefenseStructureProfile GateProfile(double health = 100, double armor = 0) =>
            new DefenseStructureProfile("test.gate", DefenseStructureKind.Gate, SiegeArea.Gates, DefenderFaction, health, armor);
        private static SiegeSnapshot State(SiegeSystem sieges)
        { Assert.That(sieges.TryGetState(SiegeId, out SiegeSnapshot state), Is.True); return state; }

        private sealed class FakeAttackerQuery : ISiegeAttackerQuery
        {
            private readonly EntityId _faction; private readonly AttackProfile _attack;
            public FakeAttackerQuery(EntityId faction, double damage) { _faction = faction; _attack = new AttackProfile(damage, DamageType.True, 2, 1, 0, targetTags: new[] { "structure" }); }
            public bool TryGetAttacker(EntityId entityId, out SiegeAttackerSnapshot attacker)
            { if (entityId != Attacker) { attacker = default; return false; } attacker = new SiegeAttackerSnapshot(_faction, _attack, new[] { "unit", "siege" }); return true; }
        }
        private sealed class FakeCaptureSink : ISiegeCaptureSink
        { public int Count { get; private set; } public SiegeActionResult Capture(EntityId settlementId, EntityId newOwnerId, CaptureCondition conditions, EntityId capturingArmyId) { Count++; return SiegeActionResult.Success(); } }
    }
}
