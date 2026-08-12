using System;
using System.Collections.Generic;
using System.Linq;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Economy;
using AegisRTS.Gameplay.Siege;
using AegisRTS.Gameplay.Units;
using AegisRTS.Gameplay.VerticalSlice;
using AegisRTS.Presentation.Camera;
using AegisRTS.Presentation.Input;
using AegisRTS.Presentation.Selection;
using AegisRTS.Presentation.UI;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Demo.PlayablePrototype
{
    public enum PrototypeCommandTab
    {
        Domestic,
        Army,
        UnitSettings,
        Siege,
    }

    /// <summary>Unity composition root for the manual, system-first playable prototype.</summary>
    [DisallowMultipleComponent]
    public sealed class PlayablePrototypeBootstrap : MonoBehaviour, IGameSessionBackend
    {
        [SerializeField] private TextAsset contentPack;
        [SerializeField] private TextAsset scenario;
        [SerializeField] private TextAsset theme;
        [SerializeField] private Material prototypeMaterial;
        [SerializeField] private bool startImmediately = true;

        private readonly Dictionary<EntityId, UnitView> _views = new Dictionary<EntityId, UnitView>();
        private readonly List<Material> _runtimeMaterials = new List<Material>();
        private readonly PrototypeGameStateAdapter _save = new PrototypeGameStateAdapter();
        private Transform _worldRoot;
        private Transform _unitRoot;
        private GameObject _inputObject;
        private SelectionService _selection;
        private UnityRtsInputAdapter _input;
        private NavMeshSurface _navigationSurface;
        private PrototypeUnityNavigationAdapter _unityNavigation;
        private GameObject _fortressGate;
        private GameObject _fortressStronghold;
        private PrototypeHudAdapter _hud;
        private HudThemeDefinition _themeDefinition;
        private bool _highContrastTheme;
        private bool _showSettings;
        private bool _showWelcome;
        private bool _showDebug;
        private PrototypeCommandTab _commandTab;
        private long _lastSelectionRevision;
        private Vector2 _commandScroll;
        private string _uiMessage = "Ready.";
        private Vector2 _notificationScroll;
        private GUIStyle _panelStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _headingStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _mutedStyle;
        private GUIStyle _statusStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _primaryButtonStyle;
        private PrototypeSaveData _pendingRestore;
        private PrototypeSaveData _startupRestore;
        private PrototypeSaveData _pendingStartupApply;
        private static PrototypeSaveData s_sceneReloadRestore;

        public GameSessionController Session { get; private set; }
        public PrototypeSystemComposition Composition { get; private set; }
        public bool BootSucceeded { get; private set; }
        public bool NavigationReady => _unityNavigation != null && _unityNavigation.IsReady;
        public bool UsesUnityNavMesh => Composition != null && Composition.Navigation.UsesUnityNavMesh;
        public IHudQuery HudQuery => _hud;
        public IHudCommandSink HudCommandSink => _hud;
        public SelectionService Selection => _selection;
        public string ActiveThemeName => _highContrastTheme ? "High Contrast" : _themeDefinition?.DisplayName ?? "Default";
        public int ViewCount => _views.Count;
        public string LastUiMessage => _uiMessage;
        public bool TutorialVisible => _showWelcome;
        public PrototypeCommandTab ActiveCommandTab => _commandTab;

        private void Awake()
        {
            ConfigureCamera();
            _startupRestore = s_sceneReloadRestore;
            s_sceneReloadRestore = null;
            Session = new GameSessionController(this);
            if (startImmediately) Session.NewGame();
        }

        private void Update()
        {
            if (_pendingRestore != null)
            {
                s_sceneReloadRestore = _pendingRestore;
                _pendingRestore = null;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
                return;
            }
            if (_pendingStartupApply != null)
            {
                PrototypeSaveData restore = _pendingStartupApply;
                _pendingStartupApply = null;
                try
                {
                    Composition.RestoreState(restore);
                    RefreshViews();
                    if (Composition.TryGetSiegeStructure(PrototypeSystemComposition.FortressGateId, out DefenseStructureSnapshot gate) &&
                        gate.IsDestroyed)
                        RefreshNavigationAfterGateBreach();
                    _uiMessage = "Save loaded and views restored.";
                }
                catch (Exception exception) { FailUi(exception.Message); }
            }
            if (Session == null || Session.State != GameSessionState.Playing || Composition == null) return;
            if (_showWelcome) { RefreshViews(); return; }
            Composition.Tick(Time.deltaTime);
            RefreshViews();
            if (Composition.IsVictory) Session.Win();
            else if (Composition.IsDefeat) Session.Lose();
        }

        private void LateUpdate()
        {
            if (_selection == null || _selection.Revision == _lastSelectionRevision) return;
            _lastSelectionRevision = _selection.Revision;
            switch (SelectionCommandContextResolver.Resolve(_selection))
            {
                case SelectionCommandContext.Domestic: _commandTab = PrototypeCommandTab.Domestic; break;
                case SelectionCommandContext.UnitSettings: _commandTab = PrototypeCommandTab.UnitSettings; break;
                case SelectionCommandContext.Siege: _commandTab = PrototypeCommandTab.Siege; break;
            }
        }

        public bool NewGame()
        {
            PrototypeSaveData restore = _startupRestore;
            _startupRestore = null;
            bool started = BuildSession(null);
            if (started && restore != null)
            {
                _pendingStartupApply = restore;
                _showWelcome = false;
                if (_inputObject != null) _inputObject.SetActive(true);
            }
            return started;
        }

        public bool LoadGame()
        {
            if (!_save.HasSlot) return FailUi("No save slot exists.");
            try
            {
                PrototypeSaveData data = _save.ParseAndValidate(_save.ReadSlot(), "prototype.neutral", "scenario.prototype-conquest");
                return BuildSession(data);
            }
            catch (Exception exception) { return FailUi(exception.Message); }
        }

        public bool RestartGame() => BuildSession(null);

        public bool RestartNow()
        {
            if (Session != null &&
                (Session.State == GameSessionState.Victory || Session.State == GameSessionState.Defeat))
                return Session.Restart();

            return BuildSession(null);
        }

        public bool LoadNow()
        {
            if (!_save.HasSlot) return FailUi("No save slot exists.");
            try
            {
                PrototypeSaveData data = _save.ParseAndValidate(_save.ReadSlot(), "prototype.neutral", "scenario.prototype-conquest");
                _pendingRestore = data;
                _uiMessage = "Loading save...";
                return true;
            }
            catch (Exception exception) { return FailUi(exception.Message); }
        }

        public bool SaveNow()
        {
            if (Composition == null) return FailUi("No active game to save.");
            try
            {
                _save.SaveToSlot(Composition);
                _uiMessage = $"Saved. Fingerprint {_save.Fingerprint(Composition).Substring(0, 12)}";
                return true;
            }
            catch (Exception exception) { return FailUi(exception.Message); }
        }

        private bool BuildSession(PrototypeSaveData restore)
        {
            if (contentPack == null || scenario == null || theme == null) return FailUi("Prototype ContentPack, Scenario, or Theme asset is missing.");
            DisposeSession();
            try
            {
                _selection = new SelectionService();
                CreateGrayboxWorld();
                BuildNavigation();
                Composition = new PrototypeSystemComposition(contentPack.text, scenario.text, _unityNavigation);
                Composition.EntitySpawned += SpawnUnitView;
                Composition.EntityRemoved += RemoveUnitView;
                Composition.FortressGateBreached += RefreshNavigationAfterGateBreach;
                Composition.FortressGateRepaired += RefreshNavigationAfterGateRepair;
                Composition.FortressCaptured += RefreshFortressOwnership;
                foreach (PrototypeEntityRecord record in Composition.Registry.Snapshot()) SpawnUnitView(record);
                if (restore != null) Composition.RestoreState(restore);
                _hud = new PrototypeHudAdapter(Composition, _selection);
                _themeDefinition = new HudThemeJsonLoader().Load(theme.text);
                _showSettings = false;
                _showWelcome = restore == null;
                _showDebug = false;
                _commandTab = PrototypeCommandTab.Domestic;
                _lastSelectionRevision = _selection.Revision;
                ComposeInput();
                SetTutorialVisible(_showWelcome);
                BootSucceeded = true;
                _uiMessage = restore == null ? "New game started." : "Save loaded and views rebound.";
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                DisposeSession();
                return FailUi(exception.Message);
            }
        }

        public void DismissTutorialNow() => SetTutorialVisible(false);

        private void SetTutorialVisible(bool visible)
        {
            _showWelcome = visible;
            if (_inputObject != null) _inputObject.SetActive(!visible);
        }

        private void CreateGrayboxWorld()
        {
            _worldRoot = new GameObject("PlayablePrototype_World").transform;
            _worldRoot.SetParent(transform);
            _unitRoot = new GameObject("Units").transform;
            _unitRoot.SetParent(_worldRoot);
            CreateMarker("Ground", PrimitiveType.Plane, Vector3.zero, new Vector3(5f, 1f, 4f), new Color(0.18f, 0.24f, 0.19f));
            CreateSelectableMarker("Player City", PrimitiveType.Cube, new Vector3(-16f, 1.5f, 0f), new Vector3(5f, 3f, 7f),
                new Color(0.15f, 0.45f, 0.9f), PrototypeSystemComposition.PlayerCityId,
                "settlement.player-city", SelectableKind.Settlement, SelectionAffiliation.Friendly);
            CreateSelectableMarker("Neutral Village", PrimitiveType.Cylinder, new Vector3(0f, 1f, -6f), new Vector3(4f, 2f, 4f),
                new Color(0.75f, 0.65f, 0.2f), PrototypeSystemComposition.VillageId,
                "settlement.village", SelectableKind.Settlement, SelectionAffiliation.Neutral);
            CreateMarker("Enemy Fortress Courtyard", PrimitiveType.Cube, new Vector3(16f, 0.05f, 3f), new Vector3(7f, 0.1f, 8f), new Color(0.48f, 0.15f, 0.13f));
            CreateMarker("Fortress Back Wall", PrimitiveType.Cube, new Vector3(19.5f, 2f, 3f), new Vector3(1f, 4f, 9f), new Color(0.6f, 0.16f, 0.13f));
            CreateMarker("Fortress North Wall", PrimitiveType.Cube, new Vector3(16f, 2f, 7f), new Vector3(7f, 4f, 1f), new Color(0.6f, 0.16f, 0.13f));
            CreateMarker("Fortress South Wall", PrimitiveType.Cube, new Vector3(16f, 2f, -1f), new Vector3(7f, 4f, 1f), new Color(0.6f, 0.16f, 0.13f));
            CreateMarker("Fortress Front South Wall", PrimitiveType.Cube, new Vector3(12.5f, 2f, 0.25f), new Vector3(1f, 4f, 2.5f), new Color(0.6f, 0.16f, 0.13f));
            CreateMarker("Fortress Front North Wall", PrimitiveType.Cube, new Vector3(12.5f, 2f, 5.75f), new Vector3(1f, 4f, 2.5f), new Color(0.6f, 0.16f, 0.13f));
            _fortressGate = CreateSelectableMarker("Fortress Gate", PrimitiveType.Cube, new Vector3(12.5f, 1.6f, 3f),
                new Vector3(1f, 3.2f, 3f), new Color(0.3f, 0.24f, 0.18f), PrototypeSystemComposition.FortressGateId,
                "structure.gate", SelectableKind.Structure, SelectionAffiliation.Enemy);
            _fortressStronghold = CreateSelectableMarker("Fortress Stronghold", PrimitiveType.Cube, new Vector3(17.5f, 1.7f, 3f),
                new Vector3(2.4f, 3.4f, 3.6f), new Color(0.72f, 0.18f, 0.14f), PrototypeSystemComposition.EnemyFortressId,
                "settlement.enemy-fortress", SelectableKind.Settlement, SelectionAffiliation.Enemy);
            CreateMarker("Road", PrimitiveType.Cube, new Vector3(0f, 0.08f, 1.5f), new Vector3(27f, 0.12f, 2f), new Color(0.35f, 0.31f, 0.25f));
            CreateMarker("North Choke", PrimitiveType.Cube, new Vector3(1f, 1.5f, 10f), new Vector3(18f, 3f, 2f), new Color(0.28f, 0.27f, 0.25f));
            CreateMarker("South Choke", PrimitiveType.Cube, new Vector3(-2f, 1.5f, -12f), new Vector3(18f, 3f, 2f), new Color(0.28f, 0.27f, 0.25f));
        }

        private void BuildNavigation()
        {
            _navigationSurface = _worldRoot.gameObject.AddComponent<NavMeshSurface>();
            _navigationSurface.collectObjects = CollectObjects.Children;
            _navigationSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            _navigationSurface.layerMask = ~0;
            _navigationSurface.ignoreNavMeshAgent = true;
            _navigationSurface.ignoreNavMeshObstacle = true;
            _navigationSurface.BuildNavMesh();
            if (_navigationSurface.navMeshData == null) throw new InvalidOperationException("Prototype NavMesh build failed.");
            _unityNavigation = _worldRoot.gameObject.AddComponent<PrototypeUnityNavigationAdapter>();
            _unityNavigation.Initialize(_navigationSurface);
        }

        private void RefreshNavigationAfterGateBreach()
        {
            if (_fortressGate != null) _fortressGate.SetActive(false);
            if (_unityNavigation == null || _navigationSurface == null) return;
            _unityNavigation.RefreshAfterWorldChange(_navigationSurface.BuildNavMesh);
        }

        private void RefreshNavigationAfterGateRepair()
        {
            if (_fortressGate != null) _fortressGate.SetActive(true);
            if (_unityNavigation == null || _navigationSurface == null) return;
            _unityNavigation.RefreshAfterWorldChange(_navigationSurface.BuildNavMesh);
        }

        private void RefreshFortressOwnership()
        {
            if (_fortressStronghold == null) return;
            Renderer renderer = _fortressStronghold.GetComponent<Renderer>();
            if (renderer == null) return;
            Material material = renderer.material;
            material.color = new Color(0.15f, 0.45f, 0.9f);
            renderer.material = material;
            if (!_runtimeMaterials.Contains(material)) _runtimeMaterials.Add(material);
            UnitySelectableView selectable = _fortressStronghold.GetComponent<UnitySelectableView>();
            if (selectable != null) selectable.SetAffiliation(SelectionAffiliation.Friendly, new Color(0.15f, 0.45f, 0.9f));
        }

        private void SpawnUnitView(PrototypeEntityRecord record)
        {
            if (_views.ContainsKey(record.EntityId) || _unitRoot == null) return;
            var root = new GameObject($"{record.DefinitionId}_{record.EntityId.Value}");
            root.transform.SetParent(_unitRoot);
            root.transform.position = ToVector(record.SpawnPosition);
            PrimitiveType primitive = record.IsHero ? PrimitiveType.Capsule :
                record.DefinitionId.Contains("siege") ? PrimitiveType.Cube : PrimitiveType.Capsule;
            GameObject visual = GameObject.CreatePrimitive(primitive);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, record.IsHero ? 1.1f : 0.8f, 0f);
            visual.transform.localScale = record.IsHero ? new Vector3(1.1f, 1.1f, 1.1f) : new Vector3(0.8f, 0.8f, 0.8f);
            Color color = record.FactionId == PrototypeSystemComposition.PlayerFactionId
                ? new Color(0.12f, 0.52f, 0.95f)
                : new Color(0.92f, 0.18f, 0.15f);
            SetColor(visual, color);
            var selectable = root.AddComponent<UnitySelectableView>();
            selectable.Configure(record.EntityId, record.DefinitionId,
                record.IsHero ? SelectableKind.Hero : SelectableKind.Unit,
                record.FactionId == PrototypeSystemComposition.PlayerFactionId ? SelectionAffiliation.Friendly : SelectionAffiliation.Enemy,
                _selection, color);

            GameObject healthBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            healthBack.name = "Health_Back";
            healthBack.transform.SetParent(root.transform, false);
            healthBack.transform.localPosition = new Vector3(0f, 2.35f, 0f);
            healthBack.transform.localScale = new Vector3(1.25f, 0.12f, 0.12f);
            SetColor(healthBack, Color.black);
            GameObject healthFill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            healthFill.name = "Health_Fill";
            healthFill.transform.SetParent(root.transform, false);
            healthFill.transform.localPosition = new Vector3(0f, 2.35f, -0.08f);
            healthFill.transform.localScale = new Vector3(1.2f, 0.08f, 0.08f);
            SetColor(healthFill, new Color(0.2f, 0.9f, 0.25f));
            _views.Add(record.EntityId, new UnitView(root, healthFill.transform));
        }

        private void RemoveUnitView(EntityId entityId)
        {
            if (!_views.TryGetValue(entityId, out UnitView view)) return;
            _views.Remove(entityId);
            _selection?.Unregister(entityId);
            if (view.Root != null) Destroy(view.Root);
        }

        private void RefreshViews()
        {
            foreach (KeyValuePair<EntityId, UnitView> item in _views.ToArray())
            {
                if (!Composition.Movement.TryGetState(item.Key, out var movement) ||
                    !Composition.Combat.TryGetState(item.Key, out CombatantSnapshot combat)) continue;
                item.Value.Root.transform.position = ToVector(movement.Position);
                float ratio = (float)Math.Max(0d, Math.Min(1d, combat.Health / combat.MaxHealth));
                item.Value.HealthFill.localScale = new Vector3(1.2f * ratio, 0.08f, 0.08f);
                item.Value.HealthFill.localPosition = new Vector3(-0.6f * (1f - ratio), 2.35f, -0.08f);
            }
        }

        private void ComposeInput()
        {
            Camera mainCamera = Camera.main;
            var controller = mainCamera.GetComponent<RtsCameraController>();
            if (controller == null) controller = mainCamera.gameObject.AddComponent<RtsCameraController>();
            controller.Initialize(new RtsCameraRigModel(0d, 0d, 31d));
            _inputObject = new GameObject("Prototype_RTS_Input");
            _inputObject.transform.SetParent(transform);
            _input = _inputObject.AddComponent<UnityRtsInputAdapter>();
            _input.Initialize(_selection, Composition.Commands, controller);
            _input.SetPointerBlocker(IsPointerOverProductHud);
        }

        private bool IsPointerOverProductHud(Vector2 screenPoint)
        {
            if (_showWelcome || Session == null || Session.State != GameSessionState.Playing) return true;
            float guiY = Screen.height - screenPoint.y;
            float dockHeight = Screen.height < 620 ? 165f : 196f;
            return guiY <= 198f || guiY >= Screen.height - dockHeight - 10f;
        }

        private static void ConfigureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject value = new GameObject("Main Camera");
                value.tag = "MainCamera";
                camera = value.AddComponent<Camera>();
                value.AddComponent<AudioListener>();
            }
            camera.transform.position = new Vector3(0f, 27f, -25f);
            camera.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
            camera.backgroundColor = new Color(0.055f, 0.075f, 0.105f);
        }

        private GameObject CreateMarker(string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color)
        {
            GameObject marker = GameObject.CreatePrimitive(primitive);
            marker.name = name;
            marker.transform.SetParent(_worldRoot);
            marker.transform.position = position;
            marker.transform.localScale = scale;
            SetColor(marker, color);
            return marker;
        }

        private GameObject CreateSelectableMarker(
            string name,
            PrimitiveType primitive,
            Vector3 position,
            Vector3 scale,
            Color color,
            EntityId entityId,
            string definitionId,
            SelectableKind kind,
            SelectionAffiliation affiliation)
        {
            GameObject marker = CreateMarker(name, primitive, position, scale, color);
            UnitySelectableView selectable = marker.AddComponent<UnitySelectableView>();
            selectable.Configure(entityId, definitionId, kind, affiliation, _selection, color);
            return marker;
        }

        private void SetColor(GameObject target, Color color)
        {
            Renderer renderer = target.GetComponentInChildren<Renderer>();
            Material material = prototypeMaterial != null ? new Material(prototypeMaterial) : null;
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
                if (shader != null) material = new Material(shader);
            }
            if (material != null)
            {
                material.name = $"Prototype_{target.name}";
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
                if (material.HasProperty("_Color")) material.SetColor("_Color", color);
                renderer.sharedMaterial = material;
                _runtimeMaterials.Add(material);
            }
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor(Shader.PropertyToID("_BaseColor"), color);
            renderer.SetPropertyBlock(block);
        }

        private bool FailUi(string message)
        {
            _uiMessage = message;
            Debug.LogError($"[PlayablePrototype] {message}", this);
            return false;
        }

        private void DisposeSession()
        {
            if (Composition != null)
            {
                Composition.EntitySpawned -= SpawnUnitView;
                Composition.EntityRemoved -= RemoveUnitView;
                Composition.FortressGateBreached -= RefreshNavigationAfterGateBreach;
                Composition.FortressGateRepaired -= RefreshNavigationAfterGateRepair;
                Composition.FortressCaptured -= RefreshFortressOwnership;
                Composition.Dispose();
                Composition = null;
            }
            _views.Clear();
            foreach (Material material in _runtimeMaterials) if (material != null) DestroySessionObject(material);
            _runtimeMaterials.Clear();
            if (_inputObject != null)
            {
                _inputObject.SetActive(false);
                DestroySessionObject(_inputObject);
            }
            if (_worldRoot != null)
            {
                _worldRoot.gameObject.SetActive(false);
                DestroySessionObject(_worldRoot.gameObject);
            }
            _inputObject = null;
            _worldRoot = null;
            _unitRoot = null;
            _navigationSurface = null;
            _unityNavigation = null;
            _fortressGate = null;
            _fortressStronghold = null;
            _hud = null;
            _themeDefinition = null;
            _pendingRestore = null;
            _pendingStartupApply = null;
            BootSucceeded = false;
        }

        private static void DestroySessionObject(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

        private void OnGUI()
        {
            if (Session == null) return;
            EnsureGuiStyles();
            if (_showWelcome && Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                WelcomeDismissHitRect().Contains(Event.current.mousePosition))
            {
                DismissTutorialNow();
                Event.current.Use();
            }
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F1) SetTutorialVisible(!_showWelcome);
            if (_showWelcome && Event.current.type == EventType.KeyDown &&
                (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter ||
                 Event.current.keyCode == KeyCode.Escape))
            {
                DismissTutorialNow();
                Event.current.Use();
            }
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F3) _showDebug = !_showDebug;
            if (Session.State == GameSessionState.MainMenu) { DrawMainMenu(); return; }
            if (Composition == null) return;
            bool modalVisible = _showWelcome || Session.State == GameSessionState.Victory || Session.State == GameSessionState.Defeat;
            bool previousEnabled = GUI.enabled;
            if (modalVisible) GUI.enabled = false;
            DrawTopBar();
            DrawObjectiveCard();
            DrawEventCard();
            DrawWorldLabels();
            DrawCommandDock();
            if (_showDebug) DrawDebugPanel();
            if (_showSettings) DrawSettingsPanel();
            GUI.enabled = previousEnabled;
            if (_showWelcome) { DrawWelcome(); return; }
            if (Session.State == GameSessionState.Victory || Session.State == GameSessionState.Defeat) DrawEndState();
        }

        private void OnGuiLegacy()
        {
            if (Session == null) return;
            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = ResolveThemeColor();
            if (Session.State == GameSessionState.MainMenu)
            {
                GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 180f, Screen.height * 0.5f - 120f, 360f, 240f), GUI.skin.box);
                GUILayout.Label("PLAYABLE PROTOTYPE 01");
                GUILayout.Label("System-first RTS validation build");
                if (GUILayout.Button("New Game", GUILayout.Height(42f))) Session.NewGame();
                GUI.enabled = _save.HasSlot;
                if (GUILayout.Button("Load Game", GUILayout.Height(42f))) Session.LoadGame();
                GUI.enabled = true;
                GUILayout.Label(_uiMessage);
                GUILayout.EndArea();
                GUI.backgroundColor = previousBackground;
                return;
            }

            GUILayout.BeginArea(new Rect(12f, 12f, Mathf.Min(560f, Screen.width * 0.46f), Screen.height - 24f), GUI.skin.box);
            GUILayout.Label($"PlayablePrototype_01 | Session: {Session.State}");
            GUILayout.Label("LMB select / drag | RMB move or attack | Shift queue | X stop | H hold | WASD camera");
            if (Composition != null)
            {
                _commandScroll = GUILayout.BeginScrollView(_commandScroll);
                GUILayout.TextArea(Composition.GetDebugSummary(), GUILayout.Height(120f));
                GUILayout.Label($"Selected: {_selection?.SelectedIds.Count ?? 0} | Formation: {_input?.ActiveFormation}");
                if (_hud != null)
                {
                    HudSnapshot snapshot = _hud.Query();
                    DrawHudPanel(snapshot, HudPanelId.ResourceBar);
                    DrawHudPanel(snapshot, HudPanelId.SelectionPanel);
                    DrawHudPanel(snapshot, HudPanelId.ArmyPanel);
                    DrawHudPanel(snapshot, HudPanelId.SettlementPanel);
                    DrawHudPanel(snapshot, HudPanelId.AbilityBar);
                    DrawHudPanel(snapshot, HudPanelId.Objective);
                }
                DrawGameplayCommands();
                GUILayout.EndScrollView();
            }
            GUILayout.Space(8f);
            GUILayout.BeginHorizontal();
            if (Session.State == GameSessionState.Playing && GUILayout.Button("Pause")) Session.Pause();
            if (Session.State == GameSessionState.Paused && GUILayout.Button("Resume")) Session.Resume();
            if (GUILayout.Button("Settings")) _showSettings = !_showSettings;
            if (GUILayout.Button("Restart")) RestartNow();
            if (GUILayout.Button("Menu")) { DisposeSession(); Session.ReturnToMenu(); }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save")) SaveNow();
            GUI.enabled = _save.HasSlot;
            if (GUILayout.Button("Load")) LoadNow();
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            if (_showSettings)
            {
                GUILayout.Label($"Theme: {(_highContrastTheme ? "High Contrast" : _themeDefinition?.DisplayName ?? "Default")}");
                if (GUILayout.Button("Toggle Theme")) ToggleThemeNow();
                GUILayout.Label($"Resolution: {Screen.width}×{Screen.height} | Scrollable HUD enabled");
            }
            GUILayout.Label($"Result: {_uiMessage}");
            GUILayout.EndArea();

            if (Composition != null)
            {
                float width = Mathf.Min(390f, Screen.width * 0.32f);
                GUILayout.BeginArea(new Rect(Screen.width - width - 12f, 12f, width, Mathf.Min(430f, Screen.height - 24f)), GUI.skin.box);
                GUILayout.Label("Notifications");
                _notificationScroll = GUILayout.BeginScrollView(_notificationScroll);
                foreach (string notification in Composition.Notifications.Reverse()) GUILayout.Label(notification);
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
            GUI.backgroundColor = previousBackground;
        }

        private void DrawGameplayCommands()
        {
            GUILayout.Label("Stronghold / Production");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Build Economy")) RunHud("build.economy");
            if (GUILayout.Button("Research Siege")) RunHud("research.siege");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Recruit Infantry")) RunHud("recruit.infantry");
            if (GUILayout.Button("Recruit Archer")) RunHud("recruit.archer");
            if (GUILayout.Button("Recruit Cavalry")) RunHud("recruit.cavalry");
            if (GUILayout.Button("Recruit Siege")) RunHud("recruit.siege");
            GUILayout.EndHorizontal();
            GUILayout.Label("Army / Battle / Conquest");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Hero Army")) RunHud("army.create");
            if (GUILayout.Button("Move Army")) RunHud("army.move");
            if (GUILayout.Button("Attack Enemy")) RunHud("army.attack");
            if (GUILayout.Button("Defend")) RunHud("army.defend");
            if (GUILayout.Button("Retreat")) RunHud("army.retreat");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Selected")) RunHud("army.add-selected");
            if (GUILayout.Button("Split Selected")) RunHud("army.split-selected");
            if (GUILayout.Button("Merge Detachment")) RunHud("army.merge");
            if (GUILayout.Button("Lieutenant Commands")) RunHud("army.commander");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Siege")) RunHud("siege.start");
            if (GUILayout.Button("Breach Gate")) RunHud("siege.breach");
            if (GUILayout.Button("Enter Objective")) RunHud("siege.enter");
            if (GUILayout.Button("Attack Stronghold")) RunHud("siege.capture");
            GUILayout.EndHorizontal();
            if (Debug.isDebugBuild && GUILayout.Button("Debug: Trigger Defeat")) Composition.TriggerDefeat();
        }

        private void DrawMainMenu()
        {
            DrawDimmer();
            GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 230f, Screen.height * 0.5f - 155f, 460f, 310f), _panelStyle);
            GUILayout.Space(20f);
            GUILayout.Label("AEGIS RTS", _titleStyle);
            GUILayout.Label("可玩原型", _headingStyle);
            GUILayout.Space(12f);
            GUILayout.Label("建立部隊、突破城門、佔領敵方堡壘。", _bodyStyle);
            GUILayout.Space(18f);
            if (GUILayout.Button("開始新遊戲", _primaryButtonStyle, GUILayout.Height(48f))) Session.NewGame();
            GUI.enabled = _save.HasSlot;
            if (GUILayout.Button("載入進度", _buttonStyle, GUILayout.Height(42f))) Session.LoadGame();
            GUI.enabled = true;
            GUILayout.Space(10f);
            GUILayout.Label(HumanizeMessage(_uiMessage), _mutedStyle);
            GUILayout.EndArea();
        }

        private void DrawTopBar()
        {
            Composition.TryGetPlayerEconomy(out EconomyAccountSnapshot economy);
            bool compact = Screen.width < 1100;
            GUILayout.BeginArea(new Rect(12f, 10f, Screen.width - 24f, 48f), _panelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label(compact ? "AEGIS" : "AEGIS RTS", _headingStyle, GUILayout.Width(compact ? 72f : 125f));
            GUILayout.Label(compact ? $"材料 {Resource(economy, "resource.material"):0}" : $"材料  {Resource(economy, "resource.material"):0}  +{Production(economy, "resource.material"):0}/秒",
                _bodyStyle, GUILayout.Width(compact ? 90f : 160f));
            GUILayout.Label(compact ? $"補給 {Resource(economy, "resource.supply"):0}" : $"補給  {Resource(economy, "resource.supply"):0}  +{Production(economy, "resource.supply"):0}/秒",
                _bodyStyle, GUILayout.Width(compact ? 90f : 155f));
            GUILayout.Label($"人口 {economy.PopulationUsed:0}/{economy.PopulationCapacity:0}", _bodyStyle, GUILayout.Width(compact ? 80f : 105f));
            GUILayout.FlexibleSpace();
            if (Composition.EnemyAttackRemainingSeconds > 0d)
                GUILayout.Label(compact ? $"進攻 {Composition.EnemyAttackRemainingSeconds:0}s" : $"敵軍進攻倒數  {Composition.EnemyAttackRemainingSeconds:0} 秒",
                    _statusStyle, GUILayout.Width(compact ? 95f : 175f));
            else GUILayout.Label("敵軍已出擊", _statusStyle, GUILayout.Width(compact ? 85f : 105f));
            if (Session.State == GameSessionState.Playing && GUILayout.Button("暫停", _buttonStyle, GUILayout.Width(compact ? 48f : 58f))) Session.Pause();
            else if (Session.State == GameSessionState.Paused && GUILayout.Button("繼續", _primaryButtonStyle, GUILayout.Width(compact ? 48f : 58f))) Session.Resume();
            if (GUILayout.Button(compact ? "說明" : "說明 F1", _buttonStyle, GUILayout.Width(compact ? 52f : 72f))) SetTutorialVisible(true);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawObjectiveCard()
        {
            float height = Screen.height < 620 ? 100f : 118f;
            GUILayout.BeginArea(new Rect(12f, 68f, Mathf.Min(355f, Screen.width * 0.38f), height), _panelStyle);
            GUILayout.Label("目前任務", _headingStyle);
            GUILayout.Label(CurrentGuidance(), _bodyStyle);
            GUILayout.Space(3f);
            GUILayout.Label("藍色＝我方　紅色＝敵軍　棕色＝城門", _mutedStyle);
            GUILayout.EndArea();
        }

        private void DrawEventCard()
        {
            float width = Mathf.Min(315f, Screen.width * 0.34f);
            float height = Screen.height < 620 ? 100f : 118f;
            GUILayout.BeginArea(new Rect(Screen.width - width - 12f, 68f, width, height), _panelStyle);
            GUILayout.Label("戰況", _headingStyle);
            string[] messages = Composition.Notifications.Reverse().Select(HumanizeEvent)
                .Where(value => !string.IsNullOrEmpty(value)).Distinct().Take(3).ToArray();
            if (messages.Length == 0) GUILayout.Label("一切就緒，先建立經濟。", _bodyStyle);
            foreach (string message in messages) GUILayout.Label("• " + message, _bodyStyle);
            GUILayout.EndArea();
        }

        private void DrawCommandDock()
        {
            float height = Screen.height < 620 ? 165f : 196f;
            GUILayout.BeginArea(new Rect(12f, Screen.height - height - 10f, Screen.width - 24f, height), _panelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(Mathf.Min(250f, Screen.width * 0.22f)));
            GUILayout.Label("已選取", _headingStyle);
            int selectedCount = _selection?.SelectedIds.Count ?? 0;
            if (selectedCount == 0) GUILayout.Label("尚未選取單位\n左鍵點藍色單位，或拖曳框選。", _mutedStyle);
            else
            {
                GUILayout.Label($"已選取 {selectedCount} 個物件", _bodyStyle);
                foreach (EntityId id in _selection.SelectedIds.Take(2))
                {
                    if (Composition.Registry.TryGet(id, out PrototypeEntityRecord record))
                        GUILayout.Label($"• {FriendlyName(record.DefinitionId)}", _bodyStyle);
                    else if (_selection.TryGetDescriptor(id, out SelectableDescriptor descriptor))
                        GUILayout.Label($"• {FriendlyName(descriptor.DefinitionId)}", _bodyStyle);
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label("右鍵地面移動｜右鍵敵人攻擊", _mutedStyle);
            GUILayout.EndVertical();
            GUILayout.Space(12f);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_commandTab == PrototypeCommandTab.Domestic, "內政", GUI.skin.button)) _commandTab = PrototypeCommandTab.Domestic;
            if (GUILayout.Toggle(_commandTab == PrototypeCommandTab.Army, "軍隊指令", GUI.skin.button)) _commandTab = PrototypeCommandTab.Army;
            if (GUILayout.Toggle(_commandTab == PrototypeCommandTab.UnitSettings, "兵種設定", GUI.skin.button)) _commandTab = PrototypeCommandTab.UnitSettings;
            if (GUILayout.Toggle(_commandTab == PrototypeCommandTab.Siege, "攻城行動", GUI.skin.button)) _commandTab = PrototypeCommandTab.Siege;
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
            if (_commandTab == PrototypeCommandTab.Domestic) DrawEconomyCommands();
            else if (_commandTab == PrototypeCommandTab.Army) DrawArmyCommands();
            else if (_commandTab == PrototypeCommandTab.UnitSettings) DrawEngagementCommands();
            else DrawSiegeCommands();
            GUILayout.Space(5f);
            GUILayout.Label(HumanizeMessage(_uiMessage), _statusStyle);
            GUILayout.EndVertical();
            GUILayout.Space(12f);
            GUILayout.BeginVertical(GUILayout.Width(Mathf.Min(195f, Screen.width * 0.17f)));
            GUILayout.Label("遊戲", _headingStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("儲存", _buttonStyle)) SaveNow();
            GUI.enabled = _save.HasSlot;
            if (GUILayout.Button("載入", _buttonStyle)) LoadNow();
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            if (GUILayout.Button("重新開始", _buttonStyle)) RestartNow();
            if (GUILayout.Button("設定", _buttonStyle)) _showSettings = !_showSettings;
            if (GUILayout.Button("主選單", _buttonStyle)) { DisposeSession(); Session.ReturnToMenu(); }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawEconomyCommands()
        {
            bool economyBuilt = Composition.Buildings.IsBuilt(PrototypeSystemComposition.PlayerCityId, new DefinitionId("building.economy"));
            bool siegeResearched = Composition.Technologies.IsResearched(PrototypeSystemComposition.PlayerFactionId, new DefinitionId("technology.siege"));
            GUILayout.Label("所有兵種由主堡直接訓練；經濟中心是可選的資源升級。", _mutedStyle);
            GUILayout.BeginHorizontal();
            CommandButton(economyBuilt ? "✓ 經濟升級" : "可選：建造經濟中心", "build.economy", !economyBuilt);
            CommandButton(siegeResearched ? "✓ 攻城科技" : "1  研究攻城科技", "research.siege", !siegeResearched);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            CommandButton("主堡招募步兵", "recruit.infantry", true);
            CommandButton("主堡招募弓兵", "recruit.archer", true);
            CommandButton("主堡招募騎兵", "recruit.cavalry", true);
            CommandButton(Composition.FindPlayerSiegeUnit().IsValid ? "✓ 攻城兵器已就緒" : "2  主堡製造攻城兵器",
                "recruit.siege", siegeResearched && !Composition.FindPlayerSiegeUnit().IsValid);
            GUILayout.EndHorizontal();
            DrawQueueSummary();
        }

        private void DrawArmyCommands()
        {
            bool hasArmy = Composition.Armies.TryGetState(PrototypeSystemComposition.PlayerArmyId, out _);
            GUILayout.BeginHorizontal();
            CommandButton(hasArmy ? "✓ 英雄軍團已建立" : "建立英雄軍團", "army.create", !hasArmy);
            CommandButton("向前推進", "army.move", hasArmy);
            CommandButton("攻擊最近敵軍", "army.attack", hasArmy);
            CommandButton("原地防守", "army.defend", hasArmy);
            CommandButton("撤回基地", "army.retreat", hasArmy);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            CommandButton("加入已選單位", "army.add-selected", hasArmy && (_selection?.SelectedIds.Count ?? 0) > 0);
            CommandButton("拆分已選部隊", "army.split-selected", hasArmy && (_selection?.SelectedIds.Count ?? 0) > 0);
            CommandButton("合併分隊", "army.merge", hasArmy);
            CommandButton("副官接任指揮", "army.commander", hasArmy);
            GUILayout.EndHorizontal();
        }

        private void DrawEngagementCommands()
        {
            bool hasSelection = (_selection?.SelectedIds.Count ?? 0) > 0;
            UnitEngagementMode? commonMode = null;
            bool mixed = false;
            if (hasSelection)
            {
                foreach (EntityId id in _selection.SelectedIds)
                {
                    if (!Composition.Combat.TryGetState(id, out CombatantSnapshot combat)) continue;
                    if (!commonMode.HasValue) commonMode = combat.EngagementMode;
                    else if (commonMode.Value != combat.EngagementMode) mixed = true;
                }
            }
            GUILayout.Label(!hasSelection
                ? "請先選取我方兵種單位。"
                : mixed ? "目前選取包含不同姿態。" : $"目前姿態：{EngagementModeName(commonMode ?? UnitEngagementMode.Normal)}", _mutedStyle);
            GUILayout.BeginHorizontal();
            CommandButton("堅守陣地 ×0.5", "engagement.hold-ground", hasSelection);
            CommandButton("普通 ×1.0", "engagement.normal", hasSelection);
            CommandButton("攻擊 ×1.5", "engagement.aggressive", hasSelection);
            CommandButton("反擊（受擊才行動）", "engagement.retaliate", hasSelection);
            GUILayout.EndHorizontal();
            GUILayout.Label("前三種會主動索敵；反擊只在受擊後行動。手動攻擊仍優先。", _mutedStyle);
        }

        private void DrawSiegeCommands()
        {
            bool hasSiegeUnit = Composition.FindPlayerSiegeUnit().IsValid;
            bool siegeStarted = Composition.Sieges.TryGetState(PrototypeSystemComposition.PlayerSiegeId, out SiegeSnapshot siege);
            bool gateOpen = siegeStarted && Composition.TryGetSiegeStructure(PrototypeSystemComposition.FortressGateId, out DefenseStructureSnapshot gate) && gate.IsDestroyed;
            bool inside = siegeStarted && (siege.CurrentArea == SiegeArea.InnerArea || siege.CurrentArea == SiegeArea.CaptureObjective);
            double repair = Composition.GateRepairRemainingSeconds;
            GUILayout.Label(repair > 0d
                ? $"城牆不可摧毀；守軍將在 {repair:0.0} 秒後修復城門，請儘快破門並進入。"
                : "城牆不可摧毀；城門可破壞與修復。主堡失守時轉移所有權，不會消失。", _mutedStyle);
            GUILayout.BeginHorizontal();
            CommandButton(siegeStarted ? "✓ 1  攻城開始" : "1  開始攻城", "siege.start", hasSiegeUnit && !siegeStarted);
            CommandButton(gateOpen ? "✓ 2  城門已破" : "2  轟擊城門", "siege.breach", siegeStarted && !gateOpen);
            CommandButton(inside ? "✓ 3  已進入內院" : "3  進入內院", "siege.enter", gateOpen && !inside);
            CommandButton(Composition.IsVictory ? "✓ 4  主堡已接管" : "4  攻擊並接管主堡", "siege.capture", inside && !Composition.IsVictory);
            GUILayout.EndHorizontal();
        }

        private void CommandButton(string label, string commandId, bool enabled)
        {
            bool previous = GUI.enabled;
            GUI.enabled = enabled && Session.State == GameSessionState.Playing;
            if (GUILayout.Button(label, enabled ? _primaryButtonStyle : _buttonStyle, GUILayout.Height(34f))) RunHud(commandId);
            GUI.enabled = previous;
        }

        private void DrawQueueSummary()
        {
            var queues = new List<string>();
            queues.AddRange(Composition.Buildings.SnapshotQueue().Where(value => value.SettlementId == PrototypeSystemComposition.PlayerCityId)
                .Select(value => $"建造中 {FriendlyName(value.BuildingId.Value)} {value.RemainingSeconds:0.0}s"));
            queues.AddRange(Composition.Technologies.SnapshotQueue().Where(value => value.FactionId == PrototypeSystemComposition.PlayerFactionId)
                .Select(value => $"研究中 {FriendlyName(value.TechnologyId.Value)} {value.RemainingSeconds:0.0}s"));
            queues.AddRange(Composition.Recruitment.SnapshotQueue().Where(value => value.FactionId == PrototypeSystemComposition.PlayerFactionId)
                .Select(value => $"招募中 {FriendlyName(value.UnitId.Value)} {value.RemainingSeconds:0.0}s"));
            if (queues.Count > 0) GUILayout.Label(string.Join("　｜　", queues), _statusStyle);
        }

        private void DrawWelcome()
        {
            DrawDimmer();
            Rect panel = WelcomePanelRect();
            GUILayout.BeginArea(panel, _panelStyle);
            GUILayout.Space(12f);
            GUILayout.Label("先看這裡：你的目標只有一個", _titleStyle);
            GUILayout.Label("突破右側紅色要塞，壓制並接管主堡。", _headingStyle);
            GUILayout.Space(14f);
            GUILayout.Label("1　主堡可直接招募一般兵種；先研究攻城科技，再由主堡製造攻城兵器。", _bodyStyle);
            GUILayout.Space(8f);
            GUILayout.Label("2　藍色是你的單位。左鍵點選／拖曳框選，右鍵地面移動，右鍵紅色敵軍攻擊。", _bodyStyle);
            GUILayout.Space(8f);
            GUILayout.Label("3　切換到「攻城行動」：破壞可修復城門、進入內院、攻擊主堡並接管城市。", _bodyStyle);
            GUILayout.Space(14f);
            GUILayout.Label("敵軍會先發展經濟，90 秒後才主動進攻。說明開啟期間遊戲完全暫停。", _statusStyle);
            GUILayout.Label("快捷鍵：WASD 相機｜X 停止｜H 原地防守｜Shift 命令排隊｜F1 說明｜F3 Debug", _mutedStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("我了解了，開始遊戲", _primaryButtonStyle, GUILayout.Height(48f))) DismissTutorialNow();
            GUILayout.EndArea();
        }

        private static Rect WelcomePanelRect()
        {
            float width = Mathf.Min(560f, Screen.width - 40f);
            float height = Mathf.Min(430f, Screen.height - 40f);
            return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        }

        private static Rect WelcomeDismissHitRect()
        {
            Rect panel = WelcomePanelRect();
            return new Rect(panel.x + 12f, panel.yMax - 74f, panel.width - 24f, 62f);
        }

        private void DrawEndState()
        {
            DrawDimmer();
            bool victory = Session.State == GameSessionState.Victory;
            GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 210f, Screen.height * 0.5f - 125f, 420f, 250f), _panelStyle);
            GUILayout.Space(18f);
            GUILayout.Label(victory ? "勝利" : "戰敗", _titleStyle);
            GUILayout.Label(victory ? "敵方堡壘已被你佔領。" : "指揮官已陣亡。重新整理軍隊，再試一次。", _bodyStyle);
            GUILayout.Space(18f);
            if (GUILayout.Button("重新開始", _primaryButtonStyle, GUILayout.Height(44f))) RestartNow();
            if (GUILayout.Button("返回主選單", _buttonStyle, GUILayout.Height(38f))) { DisposeSession(); Session.ReturnToMenu(); }
            GUILayout.EndArea();
        }

        private void DrawSettingsPanel()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 280f, 198f, 268f, 150f), _panelStyle);
            GUILayout.Label("顯示設定", _headingStyle);
            GUILayout.Label($"解析度：{Screen.width} × {Screen.height}", _bodyStyle);
            GUILayout.Label($"配色：{ActiveThemeName}", _bodyStyle);
            if (GUILayout.Button("切換高對比", _buttonStyle)) ToggleThemeNow();
            if (GUILayout.Button("關閉", _buttonStyle)) _showSettings = false;
            GUILayout.EndArea();
        }

        private void DrawDebugPanel()
        {
            GUILayout.BeginArea(new Rect(12f, 195f, Mathf.Min(520f, Screen.width * 0.5f), 220f), _panelStyle);
            GUILayout.Label("開發資訊（F3 關閉）", _headingStyle);
            _commandScroll = GUILayout.BeginScrollView(_commandScroll);
            GUILayout.Label(Composition.GetDebugSummary(), _mutedStyle);
            if (Debug.isDebugBuild && GUILayout.Button("測試戰敗畫面", _buttonStyle)) Composition.TriggerDefeat();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawWorldLabels()
        {
            DrawWorldLabel(new Vector3(-16f, 4.5f, 0f), "我方基地", new Color(0.35f, 0.75f, 1f));
            DrawWorldLabel(new Vector3(0f, 3.5f, -6f), "中立村莊", new Color(1f, 0.82f, 0.3f));
            DrawWorldLabel(new Vector3(16f, 6f, 3f), "敵方堡壘", new Color(1f, 0.38f, 0.32f));
        }

        private void DrawWorldLabel(Vector3 world, string text, Color color)
        {
            Camera camera = Camera.main;
            if (camera == null) return;
            Vector3 point = camera.WorldToScreenPoint(world);
            if (point.z <= 0f) return;
            float labelY = Screen.height - point.y - 10f;
            float cardBottom = Screen.height < 620 ? 175f : 205f;
            float dockTop = Screen.height - (Screen.height < 620 ? 165f : 196f) - 10f;
            if (labelY < cardBottom || labelY + 24f > dockTop) return;
            Color previous = GUI.color;
            GUI.color = color;
            GUI.Label(new Rect(point.x - 55f, labelY, 110f, 24f), text, _headingStyle);
            GUI.color = previous;
        }

        private string CurrentGuidance()
        {
            bool siegeResearched = Composition.Technologies.IsResearched(PrototypeSystemComposition.PlayerFactionId, new DefinitionId("technology.siege"));
            if (!siegeResearched) return "第 1 步：主堡可直接招募一般兵種；先研究攻城科技。";
            if (!Composition.FindPlayerSiegeUnit().IsValid) return "第 2 步：由主堡製造一台攻城兵器，不需要兵營。";
            if (!Composition.Armies.TryGetState(PrototypeSystemComposition.PlayerArmyId, out _)) return "第 3 步：切換「軍隊指令」，建立英雄軍團。";
            if (!Composition.Sieges.TryGetState(PrototypeSystemComposition.PlayerSiegeId, out SiegeSnapshot siege))
                return "第 4 步：切換「攻城行動」，開始攻城。";
            if (Composition.TryGetSiegeStructure(PrototypeSystemComposition.FortressGateId, out DefenseStructureSnapshot gate) && gate.Health > 0d)
                return $"轟擊城門。城門耐久：{gate.Health:0}；守軍修復倒數：{Composition.GateRepairRemainingSeconds:0.0} 秒。";
            if (siege.CurrentArea != SiegeArea.InnerArea && siege.CurrentArea != SiegeArea.CaptureObjective)
                return "城門已破！現在進入內院。";
            if (!Composition.IsVictory) return "最後一步：攻擊主堡；耐久歸零後會完整接管，不摧毀建築。";
            return "任務完成：敵方主堡與城市所有權已轉移。";
        }

        private static double Resource(EconomyAccountSnapshot economy, string id) =>
            economy.Resources != null && economy.Resources.TryGetValue(new DefinitionId(id), out double value) ? value : 0d;

        private static double Production(EconomyAccountSnapshot economy, string id) =>
            economy.Production != null && economy.Production.TryGetValue(new DefinitionId(id), out double value) ? value : 0d;

        private static string FriendlyName(string id)
        {
            switch (id)
            {
                case "building.economy": return "經濟中心";
                case "building.recruitment": return "兵營";
                case "technology.siege": return "攻城科技";
                case "unit.infantry": return "步兵";
                case "unit.archer": return "弓兵";
                case "unit.cavalry": return "騎兵";
                case "unit.siege": return "攻城兵器";
                case "hero.commander": return "指揮官";
                case "hero.lieutenant": return "副官";
                case "hero.opponent": return "敵方指揮官";
                case "settlement.player-city": return "我方主堡";
                case "settlement.village": return "中立村莊";
                case "settlement.enemy-fortress": return "敵方主堡";
                case "structure.gate": return "城門";
                default: return id;
            }
        }

        private static string EngagementModeName(UnitEngagementMode mode)
        {
            switch (mode)
            {
                case UnitEngagementMode.HoldGround: return "堅守陣地";
                case UnitEngagementMode.Normal: return "普通";
                case UnitEngagementMode.Aggressive: return "攻擊";
                case UnitEngagementMode.Retaliate: return "反擊";
                default: return mode.ToString();
            }
        }

        private static string HumanizeEvent(string message)
        {
            if (!string.IsNullOrWhiteSpace(message) &&
                message.Contains("Enemy fortress captured", StringComparison.OrdinalIgnoreCase))
                return "敵方堡壘已被我方佔領。";
            if (!string.IsNullOrWhiteSpace(message) &&
                message.Contains("owner changed", StringComparison.OrdinalIgnoreCase))
                return "堡壘領地已轉為我方控制。";
            if (!string.IsNullOrWhiteSpace(message) &&
                message.Contains("Save restored", StringComparison.OrdinalIgnoreCase))
                return "存檔已載入，戰場狀態已恢復。";
            if (!string.IsNullOrWhiteSpace(message) &&
                message.Contains("Research complete", StringComparison.OrdinalIgnoreCase))
                return "攻城科技研究完成。";
            if (string.IsNullOrWhiteSpace(message) || message.StartsWith("Damage ", StringComparison.Ordinal) ||
                message.StartsWith("Projectile ", StringComparison.Ordinal)) return string.Empty;
            if (message.Contains("Building complete: building.economy")) return "經濟中心建造完成";
            if (message.Contains("Building complete: building.recruitment")) return "兵營建造完成";
            if (message.Contains("Technology complete")) return "攻城科技研究完成";
            if (message.Contains("Recruit complete: Infantry")) return "步兵已加入戰場";
            if (message.Contains("Recruit complete: Archer")) return "弓兵已加入戰場";
            if (message.Contains("Recruit complete: Cavalry")) return "騎兵已加入戰場";
            if (message.Contains("Recruit complete: Siege")) return "攻城兵器已就緒";
            if (message.Contains("Armies merged")) return "敵軍完成部隊整編";
            if (message.Contains("Recruitment queued")) return "敵軍正在招募援軍";
            if (message.Contains("gate breached", StringComparison.OrdinalIgnoreCase)) return "城門已被突破，通往內院的道路開啟";
            if (message.Contains("Scenario completed: Victory")) return "勝利：敵方堡壘已佔領";
            if (message.Contains("Scenario completed: Defeat")) return "戰敗：我方指揮官已陣亡";
            if (message.StartsWith("Unit ", StringComparison.Ordinal) && message.Contains("defeated")) return "一個單位已陣亡";
            return message.Length > 58 ? message.Substring(0, 58) + "…" : message;
        }

        private static string HumanizeMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message) &&
                message.Contains("Enemy fortress captured", StringComparison.OrdinalIgnoreCase)) return "敵方堡壘已被我方佔領。";
            if (!string.IsNullOrWhiteSpace(message) &&
                message.Contains("Army order accepted", StringComparison.OrdinalIgnoreCase)) return "軍團命令已下達。";
            if (!string.IsNullOrWhiteSpace(message) &&
                message.Contains("Construction queued", StringComparison.OrdinalIgnoreCase)) return "建設已排入佇列。";
            if (!string.IsNullOrWhiteSpace(message) &&
                message.Contains("Research queued", StringComparison.OrdinalIgnoreCase)) return "研究已排入佇列。";
            if (!string.IsNullOrWhiteSpace(message) &&
                message.Contains("Recruitment queued", StringComparison.OrdinalIgnoreCase)) return "招募已排入佇列。";
            if (!string.IsNullOrWhiteSpace(message) &&
                message.Contains("Army created", StringComparison.OrdinalIgnoreCase)) return "英雄軍團已建立。";
            if (!string.IsNullOrWhiteSpace(message) &&
                message.Contains("Siege started", StringComparison.OrdinalIgnoreCase)) return "攻城戰已開始。";
            if (string.IsNullOrWhiteSpace(message)) return "等待指令。";
            if (message == "New game started.") return "新遊戲已開始。";
            if (message.StartsWith("Saved.", StringComparison.Ordinal)) return "遊戲已儲存。";
            if (message.StartsWith("Save loaded", StringComparison.Ordinal)) return "進度已載入。";
            if (message.Contains("already built or queued")) return "這個項目已完成或正在進行。";
            if (message.Contains("Missing building prerequisite")) return "需要先完成前一項建築。";
            if (message.Contains("Missing technology prerequisite")) return "需要先研究攻城科技。";
            if (message.Contains("Insufficient resources")) return "資源不足。";
            if (message.Contains("Create the player army first")) return "請先建立英雄軍團。";
            if (message.Contains("siege unit is required")) return "需要先製造攻城兵器。";
            return message;
        }

        private void EnsureGuiStyles()
        {
            if (_panelStyle != null) return;
            _panelStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(12, 12, 10, 10) };
            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 25, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }, wordWrap = true,
            };
            _headingStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = Color.white }, wordWrap = true,
            };
            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14, normal = { textColor = new Color(0.93f, 0.96f, 0.98f) }, wordWrap = true,
            };
            _mutedStyle = new GUIStyle(_bodyStyle) { normal = { textColor = new Color(0.67f, 0.74f, 0.8f) } };
            _statusStyle = new GUIStyle(_bodyStyle)
            {
                fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.79f, 0.28f) }, wordWrap = true,
            };
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 13, wordWrap = true };
            _primaryButtonStyle = new GUIStyle(_buttonStyle)
            {
                fontStyle = FontStyle.Bold, normal = { textColor = Color.white },
            };
        }

        private static void DrawDimmer()
        {
            Color previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.68f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void RunHud(string commandId)
        {
            if (_hud == null) { _uiMessage = "HUD command adapter is unavailable."; return; }
            HudCommandResult result = _hud.Dispatch(new HudCommand(commandId));
            _uiMessage = result.Succeeded ? Composition.LastCommandSummary : result.Error;
        }

        public void ToggleThemeNow() => _highContrastTheme = !_highContrastTheme;

        private static void DrawHudPanel(HudSnapshot snapshot, HudPanelId panelId)
        {
            if (snapshot == null || !snapshot.TryGetPanel(panelId, out HudPanelViewModel panel) || !panel.Visible) return;
            GUILayout.Label(panel.Title);
            foreach (HudEntry entry in panel.Entries) GUILayout.Label($"{entry.Label}: {entry.Value}");
        }

        private Color ResolveThemeColor()
        {
            if (_highContrastTheme) return Color.black;
            if (_themeDefinition != null && ColorUtility.TryParseHtmlString(_themeDefinition.Panel, out Color parsed)) return parsed;
            return Color.white;
        }

        private void OnDestroy() => DisposeSession();
        private static Vector3 ToVector(WorldPoint value) => new Vector3((float)value.X, (float)value.Y, (float)value.Z);

        private sealed class UnitView
        {
            public UnitView(GameObject root, Transform healthFill) { Root = root; HealthFill = healthFill; }
            public GameObject Root { get; }
            public Transform HealthFill { get; }
        }
    }
}
