using System;
using System.Linq;
using AegisRTS.Gameplay.Content.Serialization;
using AegisRTS.Gameplay.VerticalSlice;
using UnityEngine;

namespace AegisRTS.Demo
{
    /// <summary>Phase 15 composition root shared by both demonstration worlds.</summary>
    [DisallowMultipleComponent]
    public sealed class VerticalSliceBootstrap : MonoBehaviour, IGameSessionBackend
    {
        [SerializeField] private TextAsset threeKingdomsContent;
        [SerializeField] private TextAsset threeKingdomsScenario;
        [SerializeField] private TextAsset fantasyContent;
        [SerializeField] private TextAsset fantasyScenario;
        [SerializeField] private bool startWithFantasy;
        private Transform _visualRoot;
        private double _nextStepTime;
        private bool _hasSave;

        public GameSessionController Session { get; private set; }
        public VerticalSliceSimulation Simulation { get; private set; }
        public VerticalSliceLoop Loop { get; private set; }
        public string ActiveWorldId => Simulation?.WorldId ?? string.Empty;
        public bool AcceptancePassed => Session != null && Session.State == GameSessionState.Victory &&
            Loop != null && Loop.IsCompleted && Simulation != null && Simulation.FieldBattleWon &&
            Simulation.CounterattackIssued && Simulation.FortressCaptured && Loop.History.Count == 11;

        private void Awake()
        {
            Session = new GameSessionController(this);
            Session.NewGame();
            ConfigureCamera();
        }

        private void Update()
        {
            if (Session.State != GameSessionState.Playing || Loop == null || !Loop.IsRunning || Time.unscaledTimeAsDouble < _nextStepTime) return;
            _nextStepTime = Time.unscaledTimeAsDouble + 0.04d;
            VerticalSliceStepResult result = Loop.Tick();
            if (result.Status == VerticalSliceStepStatus.Defeated) Session.Lose();
            else if (Loop.IsCompleted) Session.Win();
        }

        public bool NewGame() => BuildSimulation();
        public bool LoadGame() => _hasSave && BuildSimulation();
        public bool RestartGame() => BuildSimulation();

        public bool SwitchWorldAndRestart(bool fantasy)
        {
            startWithFantasy = fantasy;
            if (Session.State == GameSessionState.Playing) Session.Lose();
            return Session.Restart();
        }

        private bool BuildSimulation()
        {
            TextAsset content = startWithFantasy ? fantasyContent : threeKingdomsContent;
            TextAsset scenario = startWithFantasy ? fantasyScenario : threeKingdomsScenario;
            if (content == null || scenario == null) return false;
            Simulation?.Dispose();
            Simulation = new VerticalSliceSimulation(new ContentPackJsonLoader().Load(content.text),
                new VerticalSliceJsonLoader().Load(scenario.text));
            Loop = new VerticalSliceLoop(Simulation); Loop.Begin(); _nextStepTime = 0d; _hasSave = true;
            CreateWorldVisuals(startWithFantasy); return true;
        }

        private void CreateWorldVisuals(bool fantasy)
        {
            if (_visualRoot != null) Destroy(_visualRoot.gameObject);
            _visualRoot = new GameObject("VerticalSliceWorld").transform; _visualRoot.SetParent(transform);
            Color player = fantasy ? new Color(0.45f, 0.25f, 0.9f) : new Color(0.15f, 0.55f, 0.9f);
            Color enemy = fantasy ? new Color(0.9f, 0.2f, 0.5f) : new Color(0.9f, 0.25f, 0.15f);
            CreateMarker("Player City", new Vector3(-7, 0.6f, 0), new Vector3(3, 1.2f, 3), player);
            CreateMarker("Village", new Vector3(0, 0.4f, 3), new Vector3(2, 0.8f, 2), new Color(0.85f, 0.7f, 0.2f));
            CreateMarker("Enemy Fortress", new Vector3(7, 1f, 6), new Vector3(4, 2, 4), enemy);
            string[] roles = { "Infantry", "Archer", "Cavalry", "SiegeUnit" };
            for (int i = 0; i < roles.Length; i++) CreateMarker(roles[i], new Vector3(-5 + i * 1.2f, 0.55f, 1.5f),
                new Vector3(0.7f, 1.1f, 0.7f), player, PrimitiveType.Capsule);
            CreateMarker("Player Hero", new Vector3(-6, 0.75f, 1.4f), new Vector3(0.9f, 1.5f, 0.9f), Color.white, PrimitiveType.Capsule);
            CreateMarker("Enemy Hero Counterattack", new Vector3(5, 0.75f, 4.5f), new Vector3(0.9f, 1.5f, 0.9f), enemy, PrimitiveType.Capsule);
            CreateMarker("Road", new Vector3(0, 0.05f, 2.7f), new Vector3(15, 0.1f, 0.5f), new Color(0.35f, 0.3f, 0.2f));
        }

        private void CreateMarker(string name, Vector3 position, Vector3 scale, Color color, PrimitiveType type = PrimitiveType.Cube)
        {
            GameObject marker = GameObject.CreatePrimitive(type); marker.name = name; marker.transform.SetParent(_visualRoot);
            marker.transform.position = position; marker.transform.localScale = scale;
            Renderer renderer = marker.GetComponent<Renderer>(); var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block);
            block.SetColor(Shader.PropertyToID("_BaseColor"), color); renderer.SetPropertyBlock(block);
        }

        private static void ConfigureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject value = new GameObject("Main Camera"); camera = value.AddComponent<Camera>(); value.AddComponent<AudioListener>(); value.tag = "MainCamera";
            }
            camera.transform.position = new Vector3(0, 16, -16); camera.transform.rotation = Quaternion.Euler(42, 0, 0);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 16, 700, 230), GUI.skin.box);
            GUILayout.Label($"Phase 15 Vertical Slice | World: {ActiveWorldId} | Session: {Session?.State}");
            GUILayout.Label($"Loop: {Loop?.CurrentStage} | History: {(Loop == null ? string.Empty : string.Join(" > ", Loop.History.Select(value => value.ToString())))}");
            GUILayout.Label($"Units: {Simulation?.RecruitedUnitCount ?? 0} | AI Counterattack: {Simulation?.CounterattackIssued ?? false} | Field Battle: {Simulation?.FieldBattleWon ?? false}");
            GUILayout.Label($"Fortress Captured: {Simulation?.FortressCaptured ?? false} | Acceptance: {(AcceptancePassed ? "PASS" : "RUNNING")}");
            if (Session != null && Session.State == GameSessionState.Playing && GUILayout.Button("Pause")) Session.Pause();
            if (Session != null && Session.State == GameSessionState.Paused && GUILayout.Button("Resume")) Session.Resume();
            if (Session != null && (Session.State == GameSessionState.Victory || Session.State == GameSessionState.Defeat) && GUILayout.Button("Restart")) Session.Restart();
            GUILayout.EndArea();
        }

        private void OnDestroy() => Simulation?.Dispose();
    }
}
