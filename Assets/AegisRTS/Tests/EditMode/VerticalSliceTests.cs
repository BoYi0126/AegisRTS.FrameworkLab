using System.Collections.Generic;
using System.IO;
using AegisRTS.Gameplay.Content;
using AegisRTS.Gameplay.Content.Serialization;
using AegisRTS.Gameplay.Content.Validation;
using AegisRTS.Gameplay.VerticalSlice;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class VerticalSliceTests
    {
        [TestCase("DemoThreeKingdoms", "three-kingdoms")]
        [TestCase("DemoFantasy", "fantasy")]
        public void WorldData_ProvidesCompleteValidatedVerticalSlice(string folder, string worldId)
        {
            Load(folder, out ContentPack pack, out VerticalSliceDefinition scenario);
            Assert.That(new ContentPackValidator().Validate(pack, ContentPackTestFactory.Assets).IsValid, Is.True);
            VerticalSliceValidationResult validation = new VerticalSliceValidator().Validate(scenario, pack);
            Assert.That(validation.IsValid, Is.True, string.Join("\n", validation.Issues));
            Assert.That(scenario.WorldId, Is.EqualTo(worldId));
            Assert.That(scenario.ResourceIds, Has.Count.EqualTo(2));
            Assert.That(scenario.UnitRoles, Has.Count.EqualTo(4));
            Assert.That(scenario.HeroIds, Has.Count.EqualTo(2));
            Assert.That(pack.Buildings, Has.Count.EqualTo(2));
            Assert.That(pack.Settlements, Has.Count.EqualTo(3));
        }

        [TestCase("DemoThreeKingdoms")]
        [TestCase("DemoFantasy")]
        public void Simulation_ReusesOneFrameworkLoopAndCompletesFullCampaign(string folder)
        {
            Load(folder, out ContentPack pack, out VerticalSliceDefinition scenario);
            using (var simulation = new VerticalSliceSimulation(pack, scenario))
            {
                var loop = new VerticalSliceLoop(simulation); loop.Begin();
                Assert.That(loop.RunToCompletion(), Is.True, loop.LastMessage);
                Assert.That(loop.History, Is.EqualTo(new[]
                {
                    VerticalSliceStage.Start, VerticalSliceStage.Income, VerticalSliceStage.Recruit,
                    VerticalSliceStage.Army, VerticalSliceStage.Move, VerticalSliceStage.FieldBattle,
                    VerticalSliceStage.Siege, VerticalSliceStage.BreakGate, VerticalSliceStage.Enter,
                    VerticalSliceStage.Capture, VerticalSliceStage.Victory,
                }));
                Assert.That(simulation.RecruitedUnitCount, Is.EqualTo(4));
                Assert.That(simulation.CounterattackIssued, Is.True);
                Assert.That(simulation.FieldBattleWon, Is.True);
                Assert.That(simulation.FortressCaptured, Is.True);
            }
        }

        [Test]
        public void Loop_WaitingStepDoesNotSkipStage()
        {
            var executor = new WaitingOnceExecutor(); var loop = new VerticalSliceLoop(executor); loop.Begin();
            Assert.That(loop.Tick().Status, Is.EqualTo(VerticalSliceStepStatus.Waiting));
            Assert.That(loop.CurrentStage, Is.EqualTo(VerticalSliceStage.Start));
            Assert.That(loop.History, Is.Empty);
            Assert.That(loop.Tick().Status, Is.EqualTo(VerticalSliceStepStatus.Completed));
            Assert.That(loop.CurrentStage, Is.EqualTo(VerticalSliceStage.Income));
        }

        [Test]
        public void Loop_DefeatStopsWithoutAdvancingToVictory()
        {
            var loop = new VerticalSliceLoop(new DefeatExecutor()); loop.Begin(); loop.Tick();
            Assert.That(loop.IsDefeated, Is.True);
            Assert.That(loop.IsRunning, Is.False);
            Assert.That(loop.History, Is.EqualTo(new[] { VerticalSliceStage.Defeat }));
        }

        [Test]
        public void Session_HandlesNewGamePauseSettingsVictoryAndRestart()
        {
            var backend = new RecordingBackend(); var session = new GameSessionController(backend);
            Assert.That(session.NewGame(), Is.True); Assert.That(session.Pause(), Is.True);
            Assert.That(session.OpenSettings(), Is.True);
            Assert.That(session.ApplySettings(new GameSettings(0.5d, 30d, false)), Is.True);
            Assert.That(session.State, Is.EqualTo(GameSessionState.Paused)); Assert.That(session.Resume(), Is.True);
            Assert.That(session.Win(), Is.True); Assert.That(session.Restart(), Is.True);
            Assert.That(session.State, Is.EqualTo(GameSessionState.Playing)); Assert.That(backend.Restarts, Is.EqualTo(1));
        }

        [Test]
        public void Session_LoadFailureStaysAtMainMenu()
        {
            var backend = new RecordingBackend { LoadSucceeds = false }; var session = new GameSessionController(backend);
            Assert.That(session.LoadGame(), Is.False); Assert.That(session.State, Is.EqualTo(GameSessionState.MainMenu));
            Assert.That(session.Pause(), Is.False); Assert.That(session.Restart(), Is.False);
        }

        private static void Load(string folder, out ContentPack pack, out VerticalSliceDefinition scenario)
        {
            string root = Path.Combine("Assets", "AegisRTS", "Content", folder);
            pack = new ContentPackJsonLoader().Load(File.ReadAllText(Path.Combine(root, "VerticalSliceContentPack.json")));
            scenario = new VerticalSliceJsonLoader().Load(File.ReadAllText(Path.Combine(root, "VerticalSliceScenario.json")));
        }

        private sealed class WaitingOnceExecutor : IVerticalSliceStepExecutor
        {
            private bool _wait = true;
            public VerticalSliceStepResult Execute(VerticalSliceStage stage)
            { if (_wait) { _wait = false; return VerticalSliceStepResult.Waiting(); } return VerticalSliceStepResult.Completed(); }
        }
        private sealed class DefeatExecutor : IVerticalSliceStepExecutor
        { public VerticalSliceStepResult Execute(VerticalSliceStage stage) => VerticalSliceStepResult.Defeated("defeat"); }
        private sealed class RecordingBackend : IGameSessionBackend
        {
            public int Restarts { get; private set; }
            public bool LoadSucceeds { get; set; } = true;
            public bool NewGame() => true;
            public bool LoadGame() => LoadSucceeds;
            public bool RestartGame() { Restarts++; return true; }
        }
    }
}
