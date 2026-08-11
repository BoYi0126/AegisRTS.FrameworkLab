using System;
using System.Collections.Generic;
using System.Linq;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.AI;
using AegisRTS.Gameplay.Content;
using AegisRTS.Gameplay.Factions;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Territory;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class AiSystemTests
    {
        private static readonly EntityId Faction = new EntityId(1);
        private static readonly EntityId Target = new EntityId(2);

        [Test]
        public void AiProfile_ValidatesNormalizedPersonalityAndCadence()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AiProfile("invalid", 1.1, 0, 0, 0, 0));
            var profile = Profile();
            Assert.That(profile.Aggression, Is.EqualTo(0.8));
            Assert.That(profile.DesiredArmySize, Is.EqualTo(3));
        }

        [Test]
        public void ContentPack_LoadsDistinctWorldAiProfiles()
        {
            ContentPack neutral = ContentPackTestFactory.LoadDemoPack("DemoNeutral");
            ContentPack fantasy = ContentPackTestFactory.LoadDemoPack("DemoFantasy");
            Assert.That(neutral.AiProfiles.Count, Is.EqualTo(1));
            Assert.That(fantasy.AiProfiles[0].Id.Value, Is.EqualTo("fantasy.arcane-ai"));
            Assert.That(AiProfile.FromDefinition(fantasy.AiProfiles[0]).SiegePreference, Is.EqualTo(0.9));
        }

        [Test]
        public void UtilityPlanner_PrioritizesEconomyBeforeRecruitment()
        {
            var planner = new UtilityAiPlanner();
            AiWorldSnapshot world = World(economyReady: false);
            Assert.That(planner.Score(Profile(), world, 0)[0].Action, Is.EqualTo(AiActionType.DevelopEconomy));
        }

        [Test]
        public void UtilityPlanner_UsesOperationalThenTacticalSiegeStages()
        {
            var planner = new UtilityAiPlanner();
            Assert.That(planner.Score(Profile(), World(economyReady: true, unitCount: 3), 0)[0].Action,
                Is.EqualTo(AiActionType.AssembleArmy));
            Assert.That(planner.Score(Profile(), World(economyReady: true, unitCount: 3, armyReady: true, armyDeployed: true, siegeActive: true), 0)[0].Action,
                Is.EqualTo(AiActionType.Breach));
            Assert.That(planner.Score(Profile(), World(economyReady: true, unitCount: 3, armyReady: true, armyDeployed: true, siegeActive: true, breachOpen: true), 0)[0].Action,
                Is.EqualTo(AiActionType.AdvanceToObjective));
        }

        [Test]
        public void UtilityPlanner_HoldsCapturedTargetInsteadOfReenteringObjective()
        {
            var world = new AiWorldSnapshot(Faction, 100, 5, 3, 1, 2, 0, 30, 15, Target,
                new[] { new EntityId(10) }, true, false, true, true, false, true, false, true);
            Assert.That(new UtilityAiPlanner().Score(Profile(), world, 0)[0].Action, Is.EqualTo(AiActionType.HoldPosition));
        }

        [Test]
        public void UtilityPlanner_ExposesAllFourDecisionLayersForDebugging()
        {
            IReadOnlyList<AiActionScore> scores = new UtilityAiPlanner().Score(Profile(), World(), 0);
            Assert.That(scores.Select(value => value.Layer).Distinct(), Is.EquivalentTo(new[]
                { AiDecisionLayer.Strategic, AiDecisionLayer.Operational, AiDecisionLayer.Tactical, AiDecisionLayer.Unit }));
        }

        [Test]
        public void AiSystem_RespectsDecisionIntervalAndPublishesDecision()
        {
            var events = new EventBus(); int decisions = 0; events.Subscribe<AiDecisionMadeEvent>(_ => decisions++);
            var pipeline = new PipelineWorld(); var ai = new AiSystem(eventBus: events);
            ai.Register(Faction, Profile(interval: 1), pipeline, pipeline);
            ai.Tick(0); ai.Tick(0.5); ai.Tick(0.49);
            Assert.That(decisions, Is.EqualTo(1));
            ai.Tick(0.02);
            Assert.That(decisions, Is.EqualTo(2));
        }

        [Test]
        public void Snapshot_ContainsGoalScoresTargetStrengthThreatAndRoute()
        {
            var pipeline = new PipelineWorld { Strength = 8, Threat = 3 };
            var ai = new AiSystem(); ai.Register(Faction, Profile(), pipeline, pipeline); ai.Tick(0);
            Assert.That(ai.TryGetState(Faction, out AiAgentSnapshot state), Is.True);
            Assert.That(state.Scores, Is.Not.Empty);
            Assert.That(state.TargetId, Is.EqualTo(Target));
            Assert.That(state.Strength, Is.EqualTo(8));
            Assert.That(state.Threat, Is.EqualTo(3));
            Assert.That(state.Route, Is.EqualTo(new[] { new EntityId(10), new EntityId(11) }));
        }

        [Test]
        public void StrategicMap_SelectsHighestValueEnemyAndBuildsDeterministicRoute()
        {
            var factions = new FactionSystem(); EntityId enemy = new EntityId(9);
            factions.Register(Faction, new FactionProfile("ai")); factions.Register(enemy, new FactionProfile("enemy"));
            var territories = new TerritorySystem(factions); var settlements = new SettlementSystem(factions, territories);
            EntityId homeSettlement = new EntityId(20), lowSettlement = new EntityId(21), highSettlement = new EntityId(22);
            EntityId home = new EntityId(30), low = new EntityId(31), high = new EntityId(32);
            territories.RegisterNode(home, new TerritoryNodeProfile("home", 10, homeSettlement), Faction);
            territories.RegisterNode(low, new TerritoryNodeProfile("low", 20, lowSettlement), enemy);
            territories.RegisterNode(high, new TerritoryNodeProfile("high", 50, highSettlement), enemy);
            territories.Connect(home, low); territories.Connect(low, high);
            var rule = new CaptureRule(CaptureRuleType.CaptureZone);
            settlements.Register(homeSettlement, new SettlementProfile("home", 1, 1, rule), Faction);
            settlements.Register(lowSettlement, new SettlementProfile("low", 1, 1, rule), enemy);
            settlements.Register(highSettlement, new SettlementProfile("high", 1, 1, rule), enemy);
            var map = new AiStrategicMapAnalyzer();
            Assert.That(map.SelectEnemySettlement(Faction, settlements, territories), Is.EqualTo(highSettlement));
            Assert.That(map.FindRoute(territories, home, high), Is.EqualTo(new[] { home, low, high }));
        }

        [Test]
        public void RepeatedNoProgress_TriggersRecoveryAndClearsStallCounter()
        {
            var world = new StalledWorld(); var ai = new AiSystem();
            ai.Register(Faction, Profile(interval: 0.1, maxStalls: 2), world, world);
            for (int i = 0; i < 3; i++) ai.Tick(0.1);
            Assert.That(world.LastAction, Is.EqualTo(AiActionType.Recover));
            Assert.That(ai.TryGetState(Faction, out AiAgentSnapshot state), Is.True);
            Assert.That(state.StalledDecisionCount, Is.Zero);
        }

        [Test]
        public void LongRunningAi_CompletesEconomyRecruitArmySiegeCaptureWithoutDeadlock()
        {
            var pipeline = new PipelineWorld(); var ai = new AiSystem();
            ai.Register(Faction, Profile(interval: 0.05), pipeline, pipeline);
            for (int i = 0; i < 1000; i++) ai.Tick(0.05);
            Assert.That(pipeline.TargetCaptured, Is.True);
            Assert.That(pipeline.Actions, Does.Contain(AiActionType.DevelopEconomy));
            Assert.That(pipeline.Actions, Does.Contain(AiActionType.Recruit));
            Assert.That(pipeline.Actions, Does.Contain(AiActionType.AssembleArmy));
            Assert.That(pipeline.Actions, Does.Contain(AiActionType.StartSiege));
            Assert.That(pipeline.Actions, Does.Contain(AiActionType.Breach));
            Assert.That(pipeline.Actions, Does.Contain(AiActionType.Capture));
            Assert.That(ai.TryGetState(Faction, out AiAgentSnapshot state), Is.True);
            Assert.That(state.StalledDecisionCount, Is.Zero);
        }

        private static AiProfile Profile(double interval = 0.5, int maxStalls = 5) =>
            new AiProfile("test.ai", 0.8, 0.5, 0.7, 0.6, 0.9, interval, 3, maxStalls);

        private static AiWorldSnapshot World(bool economyReady = false, int unitCount = 0,
            bool armyReady = false, bool armyDeployed = false, bool siegeActive = false,
            bool breachOpen = false, bool captureAvailable = false) =>
            new AiWorldSnapshot(Faction, 100, 5, unitCount, armyReady ? 1 : 0, 1, 1, unitCount, 2,
                Target, new[] { new EntityId(10), new EntityId(11) }, economyReady, false, armyReady,
                armyDeployed, siegeActive, breachOpen, captureAvailable, false);

        private class PipelineWorld : IAiWorldQuery, IAiActionExecutor
        {
            public bool EconomyReady, ArmyReady, ArmyDeployed, SiegeActive, BreachOpen, CaptureAvailable, TargetCaptured;
            public int Units;
            public double Strength = 3, Threat = 2;
            public List<AiActionType> Actions { get; } = new List<AiActionType>();
            public AiActionType LastAction { get; protected set; }

            public AiWorldSnapshot Observe(EntityId factionId) => new AiWorldSnapshot(factionId, EconomyReady ? 100 : 0,
                EconomyReady ? 5 : 0, Units, ArmyReady ? 1 : 0, TargetCaptured ? 2 : 1, TargetCaptured ? 0 : 1,
                Strength, Threat, Target, new[] { new EntityId(10), new EntityId(11) }, EconomyReady, false,
                ArmyReady, ArmyDeployed, SiegeActive, BreachOpen, CaptureAvailable, TargetCaptured);

            public virtual AiActionResult Execute(EntityId factionId, AiActionType action, AiWorldSnapshot world)
            {
                LastAction = action; Actions.Add(action);
                if (action == AiActionType.DevelopEconomy) EconomyReady = true;
                else if (action == AiActionType.Recruit) Units++;
                else if (action == AiActionType.AssembleArmy) ArmyReady = true;
                else if (action == AiActionType.MoveToTarget) ArmyDeployed = true;
                else if (action == AiActionType.StartSiege) SiegeActive = true;
                else if (action == AiActionType.Breach) BreachOpen = true;
                else if (action == AiActionType.AdvanceToObjective) CaptureAvailable = true;
                else if (action == AiActionType.Capture) TargetCaptured = true;
                else if (action == AiActionType.HoldPosition || action == AiActionType.Recover) return AiActionResult.Progress();
                else return AiActionResult.Waiting();
                return AiActionResult.Progress();
            }
        }

        private sealed class StalledWorld : PipelineWorld
        {
            public override AiActionResult Execute(EntityId factionId, AiActionType action, AiWorldSnapshot world)
            { LastAction = action; return action == AiActionType.Recover ? AiActionResult.Progress() : AiActionResult.Waiting(); }
        }
    }
}
