using System;
using System.Collections.Generic;
using AegisRTS.Core.Random;
using AegisRTS.Core.Time;
using AegisRTS.Gameplay.Debugging;
using AegisRTS.Persistence.Replay;
using AegisRTS.Persistence.Save;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class PersistenceReplayDebugTests
    {
        [Test]
        public void SeededRandom_CaptureRestoreContinuesExactSequence()
        {
            var random = new SeededRandom(99); random.NextUInt(); SeededRandomState state = random.CaptureState();
            uint expected = random.NextUInt(); var restored = SeededRandom.Restore(state);
            Assert.That(restored.NextUInt(), Is.EqualTo(expected)); Assert.That(restored.DrawCount, Is.EqualTo(random.DrawCount));
        }

        [Test]
        public void GameClock_CaptureRestorePreservesPauseSpeedAndTicks()
        {
            var source = new GameClock(); source.SetSpeed(2); source.Advance(1.5); source.Pause(); source.Advance(1);
            var restored = new GameClock(); restored.Restore(source.CaptureState());
            Assert.That(restored.GetDebugSummary(), Is.EqualTo(source.GetDebugSummary()));
        }

        [Test]
        public void Save_RoundTripsAllRequiredStateAndMetadata()
        {
            GameStateSaveService service = Service(); GameStateDocument state = State();
            SaveMetadata metadata = service.CreateMetadata("scenario.test", DateTimeOffset.Parse("2026-08-11T00:00:00Z"));
            SaveEnvelope loaded = service.Deserialize(service.Serialize(state, metadata));
            Assert.That(service.Fingerprint(loaded.State), Is.EqualTo(service.Fingerprint(state)));
            Assert.That(loaded.Metadata.SaveVersion, Is.EqualTo("1.0.0"));
            Assert.That(loaded.Metadata.FrameworkVersion, Is.EqualTo("0.13.0"));
            Assert.That(loaded.Metadata.ContentVersion, Is.EqualTo("content.1"));
            Assert.That(loaded.Metadata.ScenarioId, Is.EqualTo("scenario.test"));
            Assert.That(loaded.Checksum, Has.Length.EqualTo(64));
        }

        [Test]
        public void Save_RejectsTamperedStateAndIncompatibleVersion()
        {
            GameStateSaveService service = Service(); string json = service.Serialize(State(), service.CreateMetadata("scenario.test"));
            Assert.That(json, Does.Contain("\"health\": 45"));
            Assert.Throws<SaveLoadException>(() => service.Deserialize(json.Replace("\"health\": 45", "\"health\": 44")));
            Assert.Throws<SaveLoadException>(() => new GameStateSaveService("2.0.0", "0.13.0", "content.1").Deserialize(json));
        }

        [Test]
        public void Coordinator_RestoresMutatedBattleState()
        {
            GameStateSaveService service = Service(); var world = new World(State()); var coordinator = new GameStateCoordinator(service);
            string before = service.Fingerprint(world.State); string json = coordinator.Save(world, service.CreateMetadata("scenario.test"));
            world.State.Units[0].Health = 1; coordinator.Load(json, world);
            Assert.That(service.Fingerprint(world.State), Is.EqualTo(before));
        }

        [Test]
        public void MemoryStore_UsesExplicitSlots()
        {
            var store = new MemorySaveStore(); store.Write("quick-1", "json");
            Assert.That(store.TryRead("quick-1", out string value), Is.True); Assert.That(value, Is.EqualTo("json"));
            Assert.That(store.TryRead("missing", out _), Is.False);
        }

        [Test]
        public void Replay_PreservesInitialStateSeedTicksAndStableCommandOrder()
        {
            GameStateSaveService service = Service(); SaveEnvelope initial = service.Deserialize(service.Serialize(State(), service.CreateMetadata("scenario.test")));
            var recorder = new ReplayRecorder(initial, 77); recorder.Record(5, "first"); recorder.Record(5, "second"); recorder.Record(8, "third");
            ReplayDocument replay = new ReplayJsonSerializer().Deserialize(new ReplayJsonSerializer().Serialize(recorder.Build()));
            var sink = new ReplaySink(); var player = new ReplayPlayer(replay, sink);
            Assert.That(player.AdvanceTo(5), Is.EqualTo(2)); Assert.That(player.AdvanceTo(8), Is.EqualTo(1));
            Assert.That(sink.Ids, Is.EqualTo(new[] { "first", "second", "third" })); Assert.That(player.Seed, Is.EqualTo(77));
            service.ValidateEnvelope(player.InitialState);
        }

        [Test]
        public void Replay_RejectsOutOfOrderRecordingAndBackwardPlayback()
        {
            GameStateSaveService service = Service(); SaveEnvelope initial = service.Deserialize(service.Serialize(State(), service.CreateMetadata("scenario.test")));
            var recorder = new ReplayRecorder(initial, 1); recorder.Record(2, "later");
            Assert.Throws<InvalidOperationException>(() => recorder.Record(1, "earlier"));
            var player = new ReplayPlayer(recorder.Build(), new ReplaySink()); player.AdvanceTo(3);
            Assert.Throws<InvalidOperationException>(() => player.AdvanceTo(2));
        }

        [Test]
        public void DebugConsole_ParsesEveryRequiredCommandAndQuotedArguments()
        {
            var sink = new DebugSink(); var console = new DebugConsole(sink, true);
            string[] commands = { "spawn \"unit heavy\"", "kill 1", "damage 1 5", "give_resource 1 gold 10", "capture 2 1", "set_speed 2", "toggle_ai 1", "show_path 1", "show_threat 1" };
            foreach (string command in commands) Assert.That(console.Execute(command).Succeeded, Is.True, command);
            Assert.That(sink.Types, Is.EquivalentTo((DebugCommandType[])Enum.GetValues(typeof(DebugCommandType))));
            Assert.That(sink.Last.Arguments[0], Is.EqualTo("1"));
        }

        [Test]
        public void DebugConsole_IsDisabledByDefaultAndRejectsInvalidInput()
        {
            var console = new DebugConsole(new DebugSink());
            Assert.That(console.Execute("kill 1").Succeeded, Is.False); console.Enabled = true;
            Assert.That(console.Execute("kill").Succeeded, Is.False);
            Assert.That(console.Execute("unknown 1").Succeeded, Is.False);
            Assert.That(console.Execute("spawn \"broken").Succeeded, Is.False);
        }

        [Test]
        public void GameState_ExposesEveryRequiredCategory()
        {
            GameStateDocument state = State();
            Assert.That(state.Factions, Has.Length.EqualTo(1)); Assert.That(state.Settlements, Has.Length.EqualTo(1));
            Assert.That(state.Units, Has.Length.EqualTo(1)); Assert.That(state.Heroes, Has.Length.EqualTo(1)); Assert.That(state.Armies, Has.Length.EqualTo(1));
            Assert.That(state.ResourceAccounts, Has.Length.EqualTo(1)); Assert.That(state.Buildings, Has.Length.EqualTo(1)); Assert.That(state.Technologies, Has.Length.EqualTo(1));
            Assert.That(state.Objectives, Has.Length.EqualTo(1)); Assert.That(state.Clock.TickCount, Is.EqualTo(2)); Assert.That(state.Random.DrawCount, Is.EqualTo(2));
        }

        private static GameStateSaveService Service() => new GameStateSaveService("1.0.0", "0.13.0", "content.1");
        private static GameStateDocument State()
        {
            var clock = new GameClock(); clock.Advance(1); clock.Advance(1); GameClockState c = clock.CaptureState();
            var random = new SeededRandom(9); random.NextUInt(); random.NextUInt(); SeededRandomState r = random.CaptureState();
            return new GameStateDocument
            {
                Factions = new[] { new FactionSaveState { Id = 1, DefinitionId = "faction", Resources = new[] { new NamedValueState { Id = "gold", Value = 10 } } } },
                Settlements = new[] { new SettlementSaveState { Id = 2, DefinitionId = "settlement", OwnerId = 1 } },
                Units = new[] { new UnitSaveState { Id = 3, DefinitionId = "unit", FactionId = 1, MaxHealth = 100, Health = 45 } },
                Heroes = new[] { new HeroSaveState { UnitId = 3, Leadership = 20 } },
                Armies = new[] { new ArmySaveState { Id = 4, FactionId = 1, UnitIds = new ulong[] { 3 } } },
                ResourceAccounts = new[] { new ResourceAccountSaveState { AccountId = 2, Balances = new[] { new NamedValueState { Id = "gold", Value = 10 } } } },
                Buildings = new[] { new BuildingSaveState { SettlementId = 2, DefinitionId = "barracks", Completed = true } },
                Technologies = new[] { new TechnologySaveState { FactionId = 1, DefinitionId = "iron", Completed = true } },
                Objectives = new[] { new ObjectiveSaveState { Id = "win", Status = "Active", Value = 0.5 } },
                Clock = new ClockSaveState { TotalSeconds = c.TotalSeconds, TotalUnscaledSeconds = c.TotalUnscaledSeconds, DeltaSeconds = c.DeltaSeconds, UnscaledDeltaSeconds = c.UnscaledDeltaSeconds, TickCount = c.TickCount, Paused = c.Paused, Speed = c.Speed },
                Random = new RandomSaveState { Seed = r.Seed, DrawCount = r.DrawCount, InternalState = r.InternalState },
            };
        }
        private sealed class World : IGameStateCaptureSource, IGameStateRestoreSink { public World(GameStateDocument state) { State = state; } public GameStateDocument State; public GameStateDocument CaptureGameState() => State; public void RestoreGameState(GameStateDocument state) => State = state; }
        private sealed class ReplaySink : IReplayCommandSink { public readonly List<string> Ids = new List<string>(); public void Execute(ReplayCommandRecord command) => Ids.Add(command.CommandId); }
        private sealed class DebugSink : IDebugCommandExecutor
        {
            public readonly List<DebugCommandType> Types = new List<DebugCommandType>(); public DebugCommandRequest Last;
            public DebugCommandResult Execute(DebugCommandRequest request) { Last = request; Types.Add(request.Type); return DebugCommandResult.Success(); }
        }
    }
}
