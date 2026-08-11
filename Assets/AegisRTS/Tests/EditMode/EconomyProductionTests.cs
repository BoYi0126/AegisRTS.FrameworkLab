using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Buildings;
using AegisRTS.Gameplay.Content;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Economy;
using AegisRTS.Gameplay.Recruitment;
using AegisRTS.Gameplay.Technology;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class EconomyProductionTests
    {
        private static readonly EntityId Settlement = new EntityId(1);
        private static readonly EntityId Faction = new EntityId(2);
        private static readonly DefinitionId Resource = new DefinitionId("test.resource");

        [Test]
        public void ResourceWallet_SpendsMultipleCostsAtomically()
        {
            var wallet = new ResourceWallet();
            wallet.Deposit(Resource, 10);
            var costs = new[] { new ResourceCost(Resource, 7), new ResourceCost(Resource, 5) };

            Assert.That(wallet.TrySpend(costs), Is.False);
            Assert.That(wallet.GetBalance(Resource), Is.EqualTo(10));
            Assert.That(wallet.TrySpend(new[] { new ResourceCost(Resource, 4) }), Is.True);
            Assert.That(wallet.GetBalance(Resource), Is.EqualTo(6));
        }

        [Test]
        public void EconomySystem_ProducesAnyContentAuthoredResourceId()
        {
            var economy = Economy(false, 10);
            economy.AddProduction(Settlement, new[] { new ResourceProduction(Resource, 2.5) });

            economy.Tick(4);

            Assert.That(Balance(economy, Settlement, Resource), Is.EqualTo(20));
        }

        [Test]
        public void BuildingPipeline_PaysThenAppliesIncomeAndPopulationEffects()
        {
            var economy = Economy(true, 100, 2);
            BuildingDefinition definition = Building("test.mill", 30, 2,
                new[] { new ResourceProduction(Resource, 3d) }, 4d);
            var sink = new RecordingSink();
            var buildings = new BuildingSystem(new[] { definition }, economy, sink: sink);

            Assert.That(buildings.Request(new ConstructBuildingCommand(Settlement, Faction, definition.Id)).Succeeded, Is.True);
            Assert.That(Balance(economy, Settlement, Resource), Is.EqualTo(70));
            buildings.Tick(1); Assert.That(buildings.IsBuilt(Settlement, definition.Id), Is.False);
            buildings.Tick(1); economy.Tick(2);

            Assert.That(buildings.IsBuilt(Settlement, definition.Id), Is.True);
            Assert.That(sink.Buildings, Is.EqualTo(1));
            Assert.That(Balance(economy, Settlement, Resource), Is.EqualTo(76));
            Assert.That(State(economy, Settlement).PopulationCapacity, Is.EqualTo(6));
        }

        [Test]
        public void TechnologyPipeline_RequiresDagParentAndAppliesModifier()
        {
            var economy = Economy(false, 100);
            economy.RegisterAccount(Faction, new[] { new ResourceCost(Resource, 100) });
            TechnologyDefinition parent = Technology("test.parent", 10, 1);
            TechnologyDefinition child = new TechnologyDefinition(new DefinitionId("test.child"), "Child",
                new[] { new ResourceCost(Resource, 20) }, new[] { parent.Id }, Tags("technology"), 1,
                new[] { new TechnologyModifier("unit.damage", 5, 1.1) });
            var modifiers = new TechnologyModifierRegistry();
            var technologies = new TechnologySystem(new[] { parent, child }, economy, modifiers);

            Assert.That(technologies.Validate(new ResearchTechnologyCommand(Faction, child.Id)).Succeeded, Is.False);
            Assert.That(technologies.Request(new ResearchTechnologyCommand(Faction, parent.Id)).Succeeded, Is.True);
            technologies.Tick(1);
            Assert.That(technologies.Request(new ResearchTechnologyCommand(Faction, child.Id)).Succeeded, Is.True);
            technologies.Tick(1);

            Assert.That(technologies.IsResearched(Faction, child.Id), Is.True);
            Assert.That(modifiers.Get(Faction, "unit.damage").Apply(10), Is.EqualTo(16.5).Within(0.0001));
        }

        [Test]
        public void RecruitmentPipeline_ValidatesPaysQueuesTimesAndSpawns()
        {
            var economy = Economy(true, 100, 2);
            var sink = new RecordingSink();
            UnitDefinition unit = Unit("test.soldier", 40, 2, 1);
            var recruitment = new RecruitmentSystem(new[] { unit }, economy, sink: sink);

            Assert.That(recruitment.Request(new RecruitUnitCommand(Settlement, Faction, unit.Id)).Succeeded, Is.True);
            Assert.That(recruitment.QueuedCount, Is.EqualTo(1));
            Assert.That(Balance(economy, Settlement, Resource), Is.EqualTo(60));
            recruitment.Tick(1); Assert.That(sink.Units, Is.Zero);
            recruitment.Tick(1);

            Assert.That(sink.Units, Is.EqualTo(1));
            Assert.That(recruitment.QueuedCount, Is.Zero);
            Assert.That(State(economy, Settlement).PopulationUsed, Is.EqualTo(1));
        }

        [Test]
        public void PopulationRule_CanBeDisabledWithoutChangingRecruitmentCode()
        {
            var economy = Economy(false, 100, 0);
            UnitDefinition unit = Unit("test.large-unit", 10, 0, 99);
            var recruitment = new RecruitmentSystem(new[] { unit }, economy);

            Assert.That(recruitment.Request(new RecruitUnitCommand(Settlement, Faction, unit.Id)).Succeeded, Is.True);
        }

        [Test]
        public void CommandRouter_RejectsUnaffordableRequestBeforeHandlerMutation()
        {
            var economy = Economy(false, 5);
            UnitDefinition unit = Unit("test.expensive", 10, 1, 0);
            var recruitment = new RecruitmentSystem(new[] { unit }, economy);
            var commands = new CommandBus();
            using (new RecruitmentCommandRouter(commands, recruitment))
            {
                Assert.That(commands.Dispatch(new RecruitUnitCommand(Settlement, Faction, unit.Id)).WasHandled, Is.False);
            }
            Assert.That(recruitment.QueuedCount, Is.Zero);
            Assert.That(Balance(economy, Settlement, Resource), Is.EqualTo(5));
        }

        [Test]
        public void FantasyPack_UsesManaThroughTheSameBuildResearchRecruitFlow()
        {
            ContentPack pack = ContentPackTestFactory.LoadDemoPack("DemoFantasy");
            DefinitionId mana = pack.Resources[0].Id;
            var economy = new EconomySystem(pack.Rules.PopulationEnabled);
            economy.RegisterAccount(Settlement, new[] { new ResourceCost(mana, 1000) }, 0, 10);
            economy.RegisterAccount(Faction, new[] { new ResourceCost(mana, 1000) });
            var technologies = new TechnologySystem(pack.Technologies, economy);
            var buildings = new BuildingSystem(pack.Buildings, economy, technologies);
            var sink = new RecordingSink();
            var recruitment = new RecruitmentSystem(pack.Units, economy, buildings, technologies, sink);

            Assert.That(buildings.Request(new ConstructBuildingCommand(Settlement, Faction, pack.Buildings[0].Id)).Succeeded, Is.True);
            buildings.Tick(pack.Buildings[0].BuildSeconds);
            Assert.That(technologies.Request(new ResearchTechnologyCommand(Settlement, Faction, pack.Technologies[0].Id)).Succeeded, Is.True);
            technologies.Tick(pack.Technologies[0].ResearchSeconds);
            Assert.That(recruitment.Request(new RecruitUnitCommand(Settlement, Faction, pack.Units[0].Id)).Succeeded, Is.True);
            recruitment.Tick(pack.Units[0].RecruitmentSeconds);

            Assert.That(sink.Units, Is.EqualTo(1));
            Assert.That(Balance(economy, Settlement, mana), Is.LessThan(1000));
        }

        private static EconomySystem Economy(bool population, double resources, double capacity = 0)
        { var value = new EconomySystem(population); value.RegisterAccount(Settlement, new[] { new ResourceCost(Resource, resources) }, 0, capacity); return value; }
        private static EconomyAccountSnapshot State(EconomySystem economy, EntityId id)
        { Assert.That(economy.TryGetState(id, out EconomyAccountSnapshot state), Is.True); return state; }
        private static double Balance(EconomySystem economy, EntityId id, DefinitionId resource) => State(economy, id).Resources[resource];
        private static UnitDefinition Unit(string id, double cost, double seconds, double population) =>
            new UnitDefinition(new DefinitionId(id), id, 100, 4, "PF_Unit_Placeholder",
                new[] { new ResourceCost(Resource, cost) }, Array.Empty<DefinitionId>(), Tags("unit"), seconds, population, null, null);
        private static BuildingDefinition Building(string id, double cost, double seconds,
            IEnumerable<ResourceProduction> production, double capacity) =>
            new BuildingDefinition(new DefinitionId(id), id, 500, "PF_Structure_Placeholder",
                new[] { new ResourceCost(Resource, cost) }, Tags("structure"), seconds, null, null, production, capacity);
        private static TechnologyDefinition Technology(string id, double cost, double seconds) =>
            new TechnologyDefinition(new DefinitionId(id), id, new[] { new ResourceCost(Resource, cost) },
                Array.Empty<DefinitionId>(), Tags("technology"), seconds, null);
        private static ContentTag[] Tags(params string[] values)
        { var result = new ContentTag[values.Length]; for (int i = 0; i < values.Length; i++) result[i] = new ContentTag(values[i]); return result; }

        private sealed class RecordingSink : IBuildingCompletionSink, ITechnologyCompletionSink, IUnitSpawnSink
        {
            public int Buildings { get; private set; }
            public int Technologies { get; private set; }
            public int Units { get; private set; }
            public void BuildingCompleted(EntityId settlementId, DefinitionId buildingId) => Buildings++;
            public void TechnologyCompleted(EntityId factionId, DefinitionId technologyId) => Technologies++;
            public void SpawnUnit(EntityId settlementId, EntityId factionId, DefinitionId unitId) => Units++;
        }
    }
}
