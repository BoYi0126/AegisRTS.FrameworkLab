using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Objectives;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class ScenarioSystemTests
    {
        [Test]
        public void Framework_DeclaresAllDefaultModesAndObjectiveTypes()
        {
            Assert.That(Enum.GetValues(typeof(GameModeType)).Length, Is.EqualTo(8));
            Assert.That(Enum.GetValues(typeof(ObjectiveType)).Length, Is.EqualTo(10));
        }

        [Test]
        public void AuthoredData_LoadsFourDistinctGameModesWithoutCodeVariants()
        {
            ScenarioDefinition[] scenarios = LoadAll();
            Assert.That(scenarios, Has.Length.EqualTo(4));
            Assert.That(scenarios.Select(item => item.GameMode.Type), Is.EquivalentTo(new[]
                { GameModeType.Conquest, GameModeType.Siege, GameModeType.Defense, GameModeType.Survival }));
            Assert.That(scenarios.All(item => item.GameMode.AllowedSystems.Count > 0), Is.True);
            Assert.That(scenarios.Single(item => item.GameMode.Type == GameModeType.Siege)
                .GameMode.IsSystemAllowed("siege"), Is.True);
        }

        [Test]
        public void GenericRuntime_CompletesAllFourAuthoredScenarios()
        {
            foreach (ScenarioDefinition definition in LoadAll())
            {
                var system = new ScenarioSystem(); system.Start(definition);
                CompleteActiveObjectives(system);
                Assert.That(system.Status, Is.EqualTo(ScenarioStatus.Victory), definition.Id);
            }
        }

        [Test]
        public void TriggerAction_UnlocksSiegeCaptureAfterGateDestroyed()
        {
            ScenarioDefinition definition = Load("Siege.json"); var system = new ScenarioSystem(); system.Start(definition);
            AssertStatus(system, "capture-core", ObjectiveStatus.Locked);
            system.SetFact("structures.gates-destroyed", 1);
            AssertStatus(system, "break-gate", ObjectiveStatus.Completed);
            AssertStatus(system, "capture-core", ObjectiveStatus.Active);
            system.SetFact("settlements.captured", 1);
            Assert.That(system.Status, Is.EqualTo(ScenarioStatus.Victory));
        }

        [Test]
        public void HoldObjective_RequiresContinuousDurationAndResetsWhenControlIsLost()
        {
            var system = new ScenarioSystem(); system.Start(Load("Defense.json"));
            system.SetFact("zones.courtyard-controlled", 1); system.Update(2);
            system.SetFact("zones.courtyard-controlled", 0); system.Update(1);
            system.SetFact("zones.courtyard-controlled", 1); system.Update(3);
            Assert.That(system.Status, Is.EqualTo(ScenarioStatus.Running));
            system.Update(2);
            Assert.That(system.Status, Is.EqualTo(ScenarioStatus.Victory));
        }

        [Test]
        public void ProtectFailure_ProducesDefeat()
        {
            var system = new ScenarioSystem(); system.Start(Load("Defense.json"));
            system.SetFact("structures.command-post-destroyed", 1);
            Assert.That(system.Status, Is.EqualTo(ScenarioStatus.Defeat));
            AssertStatus(system, "hold-courtyard", ObjectiveStatus.Failed);
        }

        [Test]
        public void TimedTrigger_ExecutesDataAuthoredActionOnce()
        {
            var system = new ScenarioSystem(); system.Start(Load("Survival.json"));
            system.Update(5); system.Update(1);
            Assert.That(system.GetFact("waves.spawned"), Is.EqualTo(1));
        }

        [Test]
        public void SharedCommandRouter_StartsScenarioAndRecordsGameplayFacts()
        {
            var events = new EventBus(); var commands = new CommandBus(); var system = new ScenarioSystem(events);
            int started = 0, completed = 0;
            events.Subscribe<ScenarioStartedEvent>(_ => started++);
            events.Subscribe<ScenarioCompletedEvent>(_ => completed++);
            using (new ScenarioCommandRouter(commands, system))
            {
                Assert.That(commands.Dispatch(new StartScenarioCommand(Load("Conquest.json"))).WasHandled, Is.True);
                Assert.That(commands.Dispatch(new AddScenarioFactCommand("settlements.captured", 1)).WasHandled, Is.True);
                Assert.That(commands.Dispatch(new SetScenarioFactCommand("late.fact", 1)).WasHandled, Is.False);
            }
            Assert.That(system.Status, Is.EqualTo(ScenarioStatus.Victory));
            Assert.That(started, Is.EqualTo(1)); Assert.That(completed, Is.EqualTo(1));
            Assert.That(commands.RegisteredHandlerCount, Is.Zero);
        }

        [Test]
        public void StartSetup_EmitsNamedCompositionSignalFromData()
        {
            var events = new EventBus(); string signal = string.Empty;
            events.Subscribe<ScenarioActionExecutedEvent>(value =>
            {
                if (value.Type == ScenarioActionType.EmitSignal) signal = value.TargetId;
            });
            new ScenarioSystem(events).Start(Load("Conquest.json"));
            Assert.That(signal, Is.EqualTo("setup.frontier-conquest"));
        }

        [Test]
        public void JsonLoader_RejectsUnknownObjectiveReference()
        {
            const string json = "{\"id\":\"bad\",\"gameMode\":{\"id\":\"mode\",\"type\":\"Siege\"}," +
                "\"objectives\":[{\"id\":\"one\",\"type\":\"Capture\",\"factId\":\"x\",\"targetValue\":1}]," +
                "\"triggers\":[{\"id\":\"bad-trigger\",\"condition\":\"ObjectiveCompleted\",\"subjectId\":\"missing\"}]}";
            Assert.Throws<FormatException>(() => new ScenarioJsonLoader().Load(json));
        }

        [Test]
        public void SnapshotAndDebugSummary_ExposeModeObjectiveProgressAndFacts()
        {
            var system = new ScenarioSystem(); system.Start(Load("Conquest.json"));
            system.SetFact("settlements.captured", 0.5);
            Assert.That(system.TryGetSnapshot(out ScenarioSnapshot snapshot), Is.True);
            Assert.That(snapshot.Definition.GameMode.Type, Is.EqualTo(GameModeType.Conquest));
            Assert.That(snapshot.Objectives[0].Value, Is.EqualTo(0.5));
            Assert.That(system.GetDebugSummary(), Does.Contain("Conquest").And.Contain("capture-frontier"));
        }

        private static void CompleteActiveObjectives(ScenarioSystem system)
        {
            for (int pass = 0; pass < 20 && system.Status == ScenarioStatus.Running; pass++)
            {
                system.TryGetSnapshot(out ScenarioSnapshot snapshot);
                ObjectiveSnapshot active = snapshot.Objectives.FirstOrDefault(item => item.Status == ObjectiveStatus.Active);
                Assert.That(active, Is.Not.Null, $"No active objective in {snapshot.Definition.Id}");
                ObjectiveDefinition objective = active.Definition;
                if (objective.FactId == ScenarioSystem.ElapsedSecondsFact)
                    system.Update(Math.Max(0, objective.TargetValue - snapshot.ElapsedSeconds));
                else
                    system.SetFact(objective.FactId, objective.TargetValue);
                if (objective.RequiredDurationSeconds > 0) system.Update(objective.RequiredDurationSeconds);
            }
        }

        private static void AssertStatus(ScenarioSystem system, string id, ObjectiveStatus expected)
        {
            system.TryGetSnapshot(out ScenarioSnapshot snapshot);
            Assert.That(snapshot.Objectives.Single(item => item.Definition.Id == id).Status, Is.EqualTo(expected));
        }

        private static ScenarioDefinition[] LoadAll() =>
            new[] { "Conquest.json", "Siege.json", "Defense.json", "Survival.json" }.Select(Load).ToArray();

        private static ScenarioDefinition Load(string fileName)
        {
            string directory = Directory.GetCurrentDirectory();
            while (directory != null && !Directory.Exists(Path.Combine(directory, "Assets", "AegisRTS")))
                directory = Directory.GetParent(directory)?.FullName;
            if (directory == null) throw new DirectoryNotFoundException("Unity project root not found.");
            string path = Path.Combine(directory, "Assets", "AegisRTS", "Content", "Scenarios", fileName);
            return new ScenarioJsonLoader().Load(File.ReadAllText(path));
        }
    }
}
