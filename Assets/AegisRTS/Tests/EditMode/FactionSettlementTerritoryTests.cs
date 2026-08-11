using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Armies;
using AegisRTS.Gameplay.Factions;
using AegisRTS.Gameplay.Heroes;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Territory;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class FactionSettlementTerritoryTests
    {
        private static readonly EntityId FactionA = new EntityId(100);
        private static readonly EntityId FactionB = new EntityId(200);

        [Test]
        public void Faction_TracksResourcesTechnologyDiplomacyAndAiProfile()
        {
            var events = new EventBus();
            var factions = new FactionSystem(events);
            factions.Register(FactionA, new FactionProfile("faction.a", "ai.aggressive"));
            factions.Register(FactionB, new FactionProfile("faction.b"));
            int diplomacyEvents = 0;
            events.Subscribe<DiplomacyChangedEvent>(_ => diplomacyEvents++);

            Assert.That(factions.AddResource(FactionA, "resource.supplies", 100), Is.True);
            Assert.That(factions.AddResource(FactionA, "resource.supplies", -25), Is.True);
            Assert.That(factions.AddResource(FactionA, "resource.supplies", -100), Is.False);
            Assert.That(factions.UnlockTechnology(FactionA, "tech.logistics"), Is.True);
            Assert.That(factions.SetDiplomacy(FactionA, FactionB, DiplomacyStatus.War), Is.True);

            FactionSnapshot first = State(factions, FactionA);
            Assert.That(first.Resources["resource.supplies"], Is.EqualTo(75));
            Assert.That(first.TechnologyIds, Is.EqualTo(new[] { "tech.logistics" }));
            Assert.That(first.Diplomacy[FactionB], Is.EqualTo(DiplomacyStatus.War));
            Assert.That(State(factions, FactionB).Diplomacy[FactionA], Is.EqualTo(DiplomacyStatus.War));
            Assert.That(first.Profile.AiProfileId, Is.EqualTo("ai.aggressive"));
            Assert.That(diplomacyEvents, Is.EqualTo(1));
        }

        [Test]
        public void TerritoryGraph_HasBidirectionalConnectionsVisibilityOwnerAndValue()
        {
            FactionSystem factions = CreateFactions();
            var territories = new TerritorySystem(factions);
            EntityId first = new EntityId(1);
            EntityId second = new EntityId(2);
            territories.RegisterNode(first, new TerritoryNodeProfile("territory.first", 10), FactionA);
            territories.RegisterNode(second, new TerritoryNodeProfile("territory.second", 20), FactionB);

            Assert.That(territories.Connect(first, second), Is.True);
            Assert.That(territories.SetVisibility(first, FactionB, TerritoryVisibility.Visible), Is.True);
            Assert.That(Territory(territories, first).ConnectionIds, Is.EqualTo(new[] { second }));
            Assert.That(Territory(territories, second).ConnectionIds, Is.EqualTo(new[] { first }));
            Assert.That(Territory(territories, first).Visibility[FactionB], Is.EqualTo(TerritoryVisibility.Visible));
            Assert.That(Territory(territories, second).Profile.Value, Is.EqualTo(20));
            Assert.That(State(factions, FactionA).TerritoryIds, Is.EqualTo(new[] { first }));
        }

        [Test]
        public void ThreeSettlements_ChangeOwnerAndAutomaticallyUpdateFactionTerritory()
        {
            var events = new EventBus();
            FactionSystem factions = CreateFactions(events);
            var territories = new TerritorySystem(factions, events);
            var settlements = new SettlementSystem(factions, territories, events);
            int settlementEvents = 0;
            int territoryEvents = 0;
            events.Subscribe<SettlementOwnerChangedEvent>(_ => settlementEvents++);
            events.Subscribe<TerritoryOwnerChangedEvent>(_ => territoryEvents++);
            EntityId[] settlementIds = { new EntityId(11), new EntityId(12), new EntityId(13) };
            EntityId[] territoryIds = { new EntityId(21), new EntityId(22), new EntityId(23) };
            CaptureRule[] rules =
            {
                new CaptureRule(CaptureRuleType.ClearDefenders),
                new CaptureRule(CaptureRuleType.CaptureZone),
                new CaptureRule(CaptureRuleType.Mixed, CaptureCondition.CoreDestroyed | CaptureCondition.CommanderKilled),
            };
            CaptureCondition[] evidence =
            {
                CaptureCondition.DefendersCleared,
                CaptureCondition.ZoneControlled,
                CaptureCondition.CoreDestroyed | CaptureCondition.CommanderKilled,
            };

            for (int index = 0; index < 3; index++)
            {
                territories.RegisterNode(territoryIds[index], new TerritoryNodeProfile($"territory.{index}", index + 1, settlementIds[index]), FactionA);
                settlements.Register(settlementIds[index], new SettlementProfile($"settlement.{index}", 1000, 100, rules[index]), FactionA);
                Assert.That(settlements.Execute(new CaptureSettlementCommand(settlementIds[index], FactionB, evidence[index])).Succeeded, Is.True);
            }

            Assert.That(State(factions, FactionA).SettlementIds, Is.Empty);
            Assert.That(State(factions, FactionA).TerritoryIds, Is.Empty);
            Assert.That(State(factions, FactionB).SettlementIds, Is.EqualTo(settlementIds));
            Assert.That(State(factions, FactionB).TerritoryIds, Is.EqualTo(territoryIds));
            foreach (EntityId settlementId in settlementIds) Assert.That(Settlement(settlements, settlementId).OwnerId, Is.EqualTo(FactionB));
            foreach (EntityId territoryId in territoryIds) Assert.That(Territory(territories, territoryId).OwnerId, Is.EqualTo(FactionB));
            Assert.That(settlementEvents, Is.EqualTo(3));
            Assert.That(territoryEvents, Is.EqualTo(3));
        }

        [TestCase(CaptureRuleType.ClearDefenders, CaptureCondition.DefendersCleared)]
        [TestCase(CaptureRuleType.CaptureZone, CaptureCondition.ZoneControlled)]
        [TestCase(CaptureRuleType.DestroyCore, CaptureCondition.CoreDestroyed)]
        [TestCase(CaptureRuleType.KillCommander, CaptureCondition.CommanderKilled)]
        public void StandardCaptureRules_RequireTheirMatchingCondition(CaptureRuleType type, CaptureCondition required)
        {
            var rule = new CaptureRule(type);
            Assert.That(rule.IsSatisfied(CaptureCondition.None), Is.False);
            Assert.That(rule.IsSatisfied(required), Is.True);
        }

        [Test]
        public void CaptureCommandRouter_RejectsIncompleteMixedCaptureBeforeMutation()
        {
            FactionSystem factions = CreateFactions();
            var territories = new TerritorySystem(factions);
            var settlements = new SettlementSystem(factions, territories);
            EntityId settlementId = new EntityId(1);
            settlements.Register(settlementId, new SettlementProfile("settlement.mixed", 100, 50,
                new CaptureRule(CaptureRuleType.Mixed, CaptureCondition.ZoneControlled | CaptureCondition.CoreDestroyed)), FactionA);
            var commands = new CommandBus();
            using (var router = new SettlementCommandRouter(commands, settlements))
            {
                Assert.That(commands.Dispatch(new CaptureSettlementCommand(settlementId, FactionB, CaptureCondition.ZoneControlled)).WasHandled, Is.False);
                Assert.That(Settlement(settlements, settlementId).OwnerId, Is.EqualTo(FactionA));
                Assert.That(commands.Dispatch(new CaptureSettlementCommand(settlementId, FactionB,
                    CaptureCondition.ZoneControlled | CaptureCondition.CoreDestroyed)).WasHandled, Is.True);
                Assert.That(router.LastResult.Succeeded, Is.True);
            }
            Assert.That(Settlement(settlements, settlementId).OwnerId, Is.EqualTo(FactionB));
        }

        [Test]
        public void Settlement_TracksPopulationGarrisonResourcesBuildingsRecruitmentAndDefense()
        {
            FactionSystem factions = CreateFactions();
            var territories = new TerritorySystem(factions);
            var settlements = new SettlementSystem(factions, territories);
            EntityId settlementId = new EntityId(1);
            settlements.Register(settlementId, new SettlementProfile("settlement.a", 500, 100,
                new CaptureRule(CaptureRuleType.ClearDefenders)), FactionA);

            Assert.That(settlements.AdjustPopulation(settlementId, 50), Is.True);
            Assert.That(settlements.SetGarrison(settlementId, new[] { new EntityId(3), new EntityId(2) }), Is.True);
            Assert.That(settlements.AddResource(settlementId, "resource.food", 40), Is.True);
            Assert.That(settlements.AddBuilding(settlementId, "building.wall"), Is.True);
            Assert.That(settlements.EnqueueRecruitment(settlementId, "unit.guard"), Is.True);
            Assert.That(settlements.SetDefense(settlementId, 120), Is.True);
            SettlementSnapshot state = Settlement(settlements, settlementId);
            Assert.That(state.Population, Is.EqualTo(550));
            Assert.That(state.GarrisonIds, Is.EqualTo(new[] { new EntityId(2), new EntityId(3) }));
            Assert.That(state.Resources["resource.food"], Is.EqualTo(40));
            Assert.That(state.BuildingIds, Is.EqualTo(new[] { "building.wall" }));
            Assert.That(state.RecruitmentQueue, Is.EqualTo(new[] { "unit.guard" }));
            Assert.That(state.Defense, Is.EqualTo(100));
        }

        [Test]
        public void AttackSettlement_RequiresExistingForeignHostileTarget()
        {
            FactionSystem factions = CreateFactions();
            var territories = new TerritorySystem(factions);
            var settlements = new SettlementSystem(factions, territories);
            EntityId ownSettlement = new EntityId(10);
            EntityId enemySettlement = new EntityId(11);
            settlements.Register(ownSettlement, new SettlementProfile("settlement.own", 100, 50, new CaptureRule(CaptureRuleType.ClearDefenders)), FactionA);
            settlements.Register(enemySettlement, new SettlementProfile("settlement.enemy", 100, 50, new CaptureRule(CaptureRuleType.ClearDefenders)), FactionB);
            var heroes = new HeroSystem();
            EntityId hero = new EntityId(1);
            EntityId armyId = new EntityId(20);
            heroes.Register(hero, new HeroProfile("hero.a", FactionA, 50));
            var armies = new ArmySystem(heroes, settlementTargetValidator: new SettlementArmyTargetValidator(settlements, factions));
            armies.RegisterMember(hero, FactionA);
            armies.Execute(new CreateArmyCommand(armyId, FactionA, new[] { hero }, hero));

            Assert.That(armies.Validate(new AttackSettlementArmyCommand(armyId, ownSettlement)).Succeeded, Is.False);
            Assert.That(armies.Validate(new AttackSettlementArmyCommand(armyId, enemySettlement)).Succeeded, Is.False);
            factions.SetDiplomacy(FactionA, FactionB, DiplomacyStatus.War);
            Assert.That(armies.Execute(new AttackSettlementArmyCommand(armyId, enemySettlement)).Succeeded, Is.True);
            Assert.That(armies.Validate(new AttackSettlementArmyCommand(armyId, new EntityId(999))).Succeeded, Is.False);
        }

        [Test]
        public void ArmyLifecycleEvents_AutomaticallyUpdateFactionArmyIndex()
        {
            var events = new EventBus();
            FactionSystem factions = CreateFactions(events);
            var heroes = new HeroSystem();
            EntityId hero = new EntityId(1);
            EntityId unit = new EntityId(2);
            EntityId firstArmy = new EntityId(10);
            EntityId splitArmy = new EntityId(11);
            heroes.Register(hero, new HeroProfile("hero.a", FactionA, 50));
            var armies = new ArmySystem(heroes, eventBus: events);
            armies.RegisterMember(hero, FactionA);
            armies.RegisterMember(unit, FactionA);
            using (var bridge = new FactionArmyEventBridge(events, factions, armies))
            {
                armies.Execute(new CreateArmyCommand(firstArmy, FactionA, new[] { hero, unit }, hero));
                armies.Execute(new SplitArmyCommand(firstArmy, splitArmy, new[] { unit }));
                Assert.That(State(factions, FactionA).ArmyIds, Is.EqualTo(new[] { firstArmy, splitArmy }));
                armies.Execute(new MergeArmiesCommand(firstArmy, splitArmy));
                Assert.That(State(factions, FactionA).ArmyIds, Is.EqualTo(new[] { firstArmy }));
            }
        }

        private static FactionSystem CreateFactions(EventBus events = null)
        {
            var factions = new FactionSystem(events);
            factions.Register(FactionA, new FactionProfile("faction.a"));
            factions.Register(FactionB, new FactionProfile("faction.b"));
            return factions;
        }

        private static FactionSnapshot State(FactionSystem factions, EntityId id)
        { Assert.That(factions.TryGetState(id, out FactionSnapshot state), Is.True); return state; }
        private static SettlementSnapshot Settlement(SettlementSystem settlements, EntityId id)
        { Assert.That(settlements.TryGetState(id, out SettlementSnapshot state), Is.True); return state; }
        private static TerritorySnapshot Territory(TerritorySystem territories, EntityId id)
        { Assert.That(territories.TryGetState(id, out TerritorySnapshot state), Is.True); return state; }
    }
}
