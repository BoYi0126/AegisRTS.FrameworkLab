using System;
using AegisRTS.Core.Commands;
using AegisRTS.Gameplay.Armies;
using AegisRTS.Gameplay.Buildings;
using AegisRTS.Gameplay.Economy;
using AegisRTS.Gameplay.Factions;
using AegisRTS.Gameplay.Objectives;
using AegisRTS.Gameplay.Recruitment;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Siege;
using AegisRTS.Gameplay.Technology;
using AegisRTS.Persistence.Save;
using NUnit.Framework;

namespace AegisRTS.Package.Tests
{
    public sealed class FrameworkApiContractTests
    {
        [Test]
        public void SetupAndResourceOperations_ArePublicPackageContracts()
        {
            AssertPublicInstanceMethod(typeof(FactionSystem), "Register");
            AssertPublicInstanceMethod(typeof(SettlementSystem), "Register");
            AssertPublicInstanceMethod(typeof(IUnitSpawnSink), "SpawnUnit");
            AssertPublicInstanceMethod(typeof(EconomySystem), "AddResource");
        }

        [Test]
        public void PlayerAiScenarioAndTests_CanShareCommandContracts()
        {
            AssertPublicInstanceMethod(typeof(CommandBus), "Dispatch");
            AssertCommand<CreateArmyCommand>();
            AssertCommand<RecruitUnitCommand>();
            AssertCommand<ConstructBuildingCommand>();
            AssertCommand<ResearchTechnologyCommand>();
            AssertCommand<StartSiegeCommand>();
            AssertCommand<CaptureSettlementCommand>();
            AssertCommand<StartScenarioCommand>();
        }

        [Test]
        public void SaveAndLoad_ArePublicPackageContracts()
        {
            AssertPublicInstanceMethod(typeof(GameStateCoordinator), "Save");
            AssertPublicInstanceMethod(typeof(GameStateCoordinator), "Load");
        }

        private static void AssertCommand<TCommand>() where TCommand : ICommand
        {
            Assert.That(typeof(TCommand).IsPublic, Is.True, $"{typeof(TCommand).Name} must remain public.");
        }

        private static void AssertPublicInstanceMethod(Type type, string methodName)
        {
            Assert.That(
                type.GetMethod(methodName),
                Is.Not.Null,
                $"{type.FullName}.{methodName} must remain a public package API.");
        }
    }
}
