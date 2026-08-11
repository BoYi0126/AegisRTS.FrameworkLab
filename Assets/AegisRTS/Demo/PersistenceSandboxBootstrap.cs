using System;
using System.Globalization;
using AegisRTS.Core.Random;
using AegisRTS.Core.Time;
using AegisRTS.Gameplay.Debugging;
using AegisRTS.Persistence.Replay;
using AegisRTS.Persistence.Save;
using UnityEngine;

namespace AegisRTS.Demo
{
    /// <summary>Phase 13 battle-state save/reload, replay, and debug acceptance.</summary>
    [DisallowMultipleComponent]
    public sealed class PersistenceSandboxBootstrap : MonoBehaviour, IGameStateCaptureSource, IGameStateRestoreSink,
        IReplayCommandSink, IDebugCommandExecutor
    {
        private GameStateDocument _state;
        public bool SaveReloadPassed { get; private set; }
        public bool ReplayPassed { get; private set; }
        public bool DebugConsolePassed { get; private set; }
        public bool AcceptancePassed => SaveReloadPassed && ReplayPassed && DebugConsolePassed;
        public int ReplayCommandCount { get; private set; }
        public int DebugCommandCount { get; private set; }
        public int SerializedCharacterCount { get; private set; }
        public string StateFingerprint { get; private set; } = string.Empty;

        private void Awake()
        {
            var service = new GameStateSaveService("1.0.0", "0.13.0", "demo.1"); var coordinator = new GameStateCoordinator(service);
            _state = CreateBattleState(); string before = service.Fingerprint(_state);
            SaveMetadata metadata = service.CreateMetadata("scenario.siege.broken-gate", DateTimeOffset.Parse("2026-08-11T00:00:00Z", CultureInfo.InvariantCulture));
            string json = coordinator.Save(this, metadata); SerializedCharacterCount = json.Length;
            _state.Units[0].Health = 1; _state.ResourceAccounts[0].Balances[0].Value = 0;
            SaveEnvelope loaded = coordinator.Load(json, this); string after = service.Fingerprint(_state); StateFingerprint = after;
            SaveReloadPassed = before == after && _state.Units[0].Health == 45 && _state.Objectives[0].Status == "Active";

            var recorder = new ReplayRecorder(loaded, loaded.State.Random.Seed);
            recorder.Record(10, "unit.damage", "{\"unitId\":100,\"amount\":5}"); recorder.Record(12, "resource.give", "{\"accountId\":10,\"amount\":20}");
            ReplayDocument replay = new ReplayJsonSerializer().Deserialize(new ReplayJsonSerializer().Serialize(recorder.Build()));
            service.ValidateEnvelope(replay.InitialState); var player = new ReplayPlayer(replay, this); player.AdvanceTo(11); player.AdvanceTo(12);
            ReplayPassed = player.IsComplete && ReplayCommandCount == 2 && player.Seed == loaded.State.Random.Seed;

            var console = new DebugConsole(this, true);
            DebugConsolePassed = console.CommandNames.Count == 9 && console.Execute("damage 100 5").Succeeded &&
                                 console.Execute("give_resource 10 neutral.supplies 20").Succeeded &&
                                 !console.Execute("unknown value").Succeeded;
        }

        public GameStateDocument CaptureGameState() => _state;
        public void RestoreGameState(GameStateDocument state) => _state = state ?? throw new ArgumentNullException(nameof(state));
        public void Execute(ReplayCommandRecord command) { ReplayCommandCount++; }
        DebugCommandResult IDebugCommandExecutor.Execute(DebugCommandRequest request) { DebugCommandCount++; return DebugCommandResult.Success(request.Type.ToString()); }

        private static GameStateDocument CreateBattleState()
        {
            var clock = new GameClock(); clock.SetSpeed(1.5); clock.Advance(2); clock.Pause(); clock.Advance(1); GameClockState clockState = clock.CaptureState();
            var random = new SeededRandom(13013); random.NextUInt(); random.NextUInt(); SeededRandomState randomState = random.CaptureState();
            return new GameStateDocument
            {
                Factions = new[] { new FactionSaveState { Id = 1, DefinitionId = "faction.player", Resources = new[] { Value("neutral.supplies", 120) }, SettlementIds = new ulong[] { 10 }, ArmyIds = new ulong[] { 20 }, TechnologyIds = new[] { "neutral.iron" } } },
                Settlements = new[] { new SettlementSaveState { Id = 10, DefinitionId = "neutral.frontier", OwnerId = 1, Population = 40, Defense = 80, GarrisonIds = new ulong[] { 100 }, Resources = new[] { Value("neutral.supplies", 120) }, BuildingIds = new[] { "neutral.barracks" } } },
                Units = new[] { new UnitSaveState { Id = 100, DefinitionId = "neutral.swordsman", FactionId = 1, MaxHealth = 100, Health = 45, CombatState = "Attacking", ArmyId = 20, Position = new VectorState { X = 4, Y = 0, Z = 8 } } },
                Heroes = new[] { new HeroSaveState { UnitId = 100, Leadership = 75, ArmyId = 20, AbilityIds = new[] { "neutral.rally" } } },
                Armies = new[] { new ArmySaveState { Id = 20, FactionId = 1, CommanderId = 100, UnitIds = new ulong[] { 100 }, Morale = 82, Supply = 66, OrderType = "AttackSettlement", TargetId = 10 } },
                ResourceAccounts = new[] { new ResourceAccountSaveState { AccountId = 10, Balances = new[] { Value("neutral.supplies", 120) }, PopulationUsed = 18, PopulationCapacity = 30 } },
                Buildings = new[] { new BuildingSaveState { SettlementId = 10, DefinitionId = "neutral.barracks", Completed = true } },
                Technologies = new[] { new TechnologySaveState { FactionId = 1, DefinitionId = "neutral.iron", Completed = true } },
                Objectives = new[] { new ObjectiveSaveState { Id = "capture-frontier", Status = "Active", Value = 0.5, HeldSeconds = 2 } },
                Clock = new ClockSaveState { TotalSeconds = clockState.TotalSeconds, TotalUnscaledSeconds = clockState.TotalUnscaledSeconds, DeltaSeconds = clockState.DeltaSeconds, UnscaledDeltaSeconds = clockState.UnscaledDeltaSeconds, TickCount = clockState.TickCount, Paused = clockState.Paused, Speed = clockState.Speed },
                Random = new RandomSaveState { Seed = randomState.Seed, DrawCount = randomState.DrawCount, InternalState = randomState.InternalState },
            };
        }
        private static NamedValueState Value(string id, double value) => new NamedValueState { Id = id, Value = value };
    }
}
