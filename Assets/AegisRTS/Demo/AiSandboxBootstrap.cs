using System;
using System.Collections.Generic;
using System.Linq;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.AI;
using AegisRTS.Gameplay.Armies;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Economy;
using AegisRTS.Gameplay.Factions;
using AegisRTS.Gameplay.Heroes;
using AegisRTS.Gameplay.Recruitment;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Siege;
using AegisRTS.Gameplay.Territory;
using AegisRTS.Gameplay.Units;
using UnityEngine;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Demo
{
    /// <summary>Phase 10 autonomous economy-to-capture Utility AI acceptance sandbox.</summary>
    [DisallowMultipleComponent]
    public sealed class AiSandboxBootstrap : MonoBehaviour, IAiWorldQuery, IAiActionExecutor, IUnitSpawnSink
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly List<EntityId> _units = new List<EntityId>();
        private readonly List<GameObject> _unitVisuals = new List<GameObject>();
        private CommandBus _commands;
        private FactionSystem _factions;
        private EconomySystem _economy;
        private RecruitmentSystem _recruitment;
        private ArmySystem _armies;
        private SiegeSystem _sieges;
        private DefinitionId _resourceId;
        private DefinitionId _unitDefinitionId;
        private EntityId _homeSettlementId, _targetSettlementId, _homeTerritoryId, _targetTerritoryId;
        private EntityId _armyId, _siegeId, _gateId;
        private bool _economyReady, _armyDeployed;
        private readonly AiStrategicMapAnalyzer _map = new AiStrategicMapAnalyzer();
        private ulong _nextUnitId = 10100;

        public AiSystem AI { get; private set; }
        public EntityId AiFactionId { get; private set; }
        public EntityId DefenderFactionId { get; private set; }
        public int DecisionEventCount { get; private set; }
        public int SpawnedUnitCount => _units.Count;
        public bool AcceptancePassed { get; private set; }
        public string ActionHistory { get; private set; } = string.Empty;

        private void Awake()
        {
            var events = new EventBus(); _commands = new CommandBus(); _factions = new FactionSystem(events);
            var territories = new TerritorySystem(_factions, events); var settlements = new SettlementSystem(_factions, territories, events);
            var combat = new CombatSystem(events); var heroes = new HeroSystem();
            _armies = new ArmySystem(heroes, eventBus: events);
            _resourceId = new DefinitionId("neutral.supplies"); _unitDefinitionId = new DefinitionId("neutral.siege-infantry");
            AiFactionId = new EntityId(10001); DefenderFactionId = new EntityId(10002);
            _homeSettlementId = new EntityId(10010); _targetSettlementId = new EntityId(10011);
            _homeTerritoryId = new EntityId(10020); _targetTerritoryId = new EntityId(10021);
            _armyId = new EntityId(10030); _siegeId = new EntityId(10040); _gateId = new EntityId(10041);

            _factions.Register(AiFactionId, new FactionProfile("faction.ai", "neutral.balanced-ai"));
            _factions.Register(DefenderFactionId, new FactionProfile("faction.defender"));
            _factions.SetDiplomacy(AiFactionId, DefenderFactionId, DiplomacyStatus.War);
            territories.RegisterNode(_homeTerritoryId, new TerritoryNodeProfile("territory.ai.home", 20, _homeSettlementId), AiFactionId);
            territories.RegisterNode(_targetTerritoryId, new TerritoryNodeProfile("territory.ai.target", 60, _targetSettlementId), DefenderFactionId);
            territories.Connect(_homeTerritoryId, _targetTerritoryId);
            settlements.Register(_homeSettlementId, new SettlementProfile("settlement.ai.home", 300, 80,
                new CaptureRule(CaptureRuleType.ClearDefenders)), AiFactionId);
            settlements.Register(_targetSettlementId, new SettlementProfile("settlement.ai.target", 500, 120,
                new CaptureRule(CaptureRuleType.CaptureZone)), DefenderFactionId);

            _economy = new EconomySystem(true, events);
            _economy.RegisterAccount(_homeSettlementId, populationCapacity: 10);
            var unit = new UnitDefinition(_unitDefinitionId, "AI Siege Infantry", 120, 4, "PF_Unit_Placeholder",
                new[] { new ResourceCost(_resourceId, 20) }, Array.Empty<DefinitionId>(), Tags("unit"),
                0.08, 1, null, null);
            _recruitment = new RecruitmentSystem(new[] { unit }, _economy, sink: this, eventBus: events);
            _sieges = new SiegeSystem(new CombatSiegeAttackerQuery(combat), new RecordingSiegeNavigationSink(),
                new SettlementSiegeCaptureSink(settlements), eventBus: events);
            _disposables.Add(new RecruitmentCommandRouter(_commands, _recruitment));
            _disposables.Add(new ArmyCommandRouter(_commands, _armies));
            _disposables.Add(new SiegeCommandRouter(_commands, _sieges));
            _disposables.Add(new FactionArmyEventBridge(events, _factions, _armies));
            _disposables.Add(events.Subscribe<AiDecisionMadeEvent>(_ => DecisionEventCount++));

            AI = new AiSystem(eventBus: events);
            AI.Register(AiFactionId, new AiProfile("neutral.balanced-ai", 0.7, 0.5, 0.7, 0.6, 0.9,
                0.08, 3, 12), this, this);
            CreateWorldVisuals(); ConfigureCamera();

            Combat = combat; Settlements = settlements; Territories = territories;
        }

        public CombatSystem Combat { get; private set; }
        public SettlementSystem Settlements { get; private set; }
        public TerritorySystem Territories { get; private set; }

        private void Update()
        {
            double delta = Time.deltaTime;
            _economy.Tick(delta); _recruitment.Tick(delta); _sieges.Tick(delta); AI.Tick(delta);
            if (!AcceptancePassed && Settlements.TryGetState(_targetSettlementId, out SettlementSnapshot target) &&
                target.OwnerId == AiFactionId)
                AcceptancePassed = _economyReady && _units.Count >= 3 && _armies.TryGetState(_armyId, out _) &&
                    _sieges.TryGetState(_siegeId, out SiegeSnapshot siege) && siege.State == SiegeState.Completed;
        }

        public AiWorldSnapshot Observe(EntityId factionId)
        {
            _economy.TryGetState(_homeSettlementId, out EconomyAccountSnapshot economy);
            double stockpile = economy.Resources != null && economy.Resources.TryGetValue(_resourceId, out double value) ? value : 0d;
            bool armyReady = _armies.TryGetState(_armyId, out ArmySnapshot army);
            bool siegeExists = _sieges.TryGetState(_siegeId, out SiegeSnapshot siege);
            bool siegeActive = siegeExists && siege.State != SiegeState.Preparing && siege.State != SiegeState.Completed && siege.State != SiegeState.Failed;
            bool breach = siegeExists && (siege.State == SiegeState.Breached || siege.State == SiegeState.InnerAreaContested || siege.State == SiegeState.CaptureAvailable || siege.State == SiegeState.Completed);
            bool capture = siegeExists && siege.State == SiegeState.CaptureAvailable;
            bool captured = Settlements.TryGetState(_targetSettlementId, out SettlementSnapshot target) && target.OwnerId == AiFactionId;
            int settlements = _factions.TryGetState(AiFactionId, out FactionSnapshot faction) ? faction.SettlementIds.Count : 0;
            IReadOnlyList<EntityId> route = _map.FindRoute(Territories, _homeTerritoryId, _targetTerritoryId);
            return new AiWorldSnapshot(factionId, stockpile, _economyReady ? 30 : 0, _units.Count, armyReady ? 1 : 0,
                settlements, captured ? 0 : 1, armyReady ? army.UnitCount * 10 : _units.Count * 10, 15,
                _targetSettlementId, route, _economyReady,
                _recruitment.QueuedCount > 0, armyReady, _armyDeployed, siegeActive, breach, capture, captured);
        }

        public AiActionResult Execute(EntityId factionId, AiActionType action, AiWorldSnapshot world)
        {
            AppendHistory(action);
            if (action == AiActionType.DevelopEconomy)
            {
                if (!_economyReady) { _economy.AddProduction(_homeSettlementId, new[] { new ResourceProduction(_resourceId, 30) }); _economyReady = true; return AiActionResult.Progress(); }
            }
            else if (action == AiActionType.Recruit)
            {
                CommandDispatchResult result = _commands.Dispatch(new RecruitUnitCommand(_homeSettlementId, AiFactionId, _unitDefinitionId));
                return result.WasHandled ? AiActionResult.Progress() : AiActionResult.Waiting();
            }
            else if (action == AiActionType.Wait) return AiActionResult.Waiting();
            else if (action == AiActionType.AssembleArmy)
            {
                CommandDispatchResult result = _commands.Dispatch(new CreateArmyCommand(_armyId, AiFactionId, _units));
                return result.WasHandled ? AiActionResult.Progress() : AiActionResult.Rejected(result.Error);
            }
            else if (action == AiActionType.MoveToTarget)
            {
                CommandDispatchResult result = _commands.Dispatch(new AttackSettlementArmyCommand(_armyId, _targetSettlementId));
                if (result.WasHandled) { _armyDeployed = true; MoveUnitVisuals(); return AiActionResult.Progress(); }
                return AiActionResult.Rejected(result.Error);
            }
            else if (action == AiActionType.StartSiege)
            {
                _sieges.Register(_siegeId, new SiegeProfile(_targetSettlementId, AiFactionId, DefenderFactionId,
                    SiegeMode.Assault, _armyId));
                _sieges.RegisterStructure(_siegeId, _gateId, new DefenseStructureProfile("structure.ai.gate",
                    DefenseStructureKind.Gate, SiegeArea.Gates, DefenderFactionId, 150, 20));
                CommandDispatchResult result = _commands.Dispatch(new StartSiegeCommand(_siegeId));
                return result.WasHandled ? AiActionResult.Progress() : AiActionResult.Rejected(result.Error);
            }
            else if (action == AiActionType.Breach)
            {
                CommandDispatchResult result = _commands.Dispatch(new AttackDefenseStructureCommand(_siegeId, _units[0], _gateId));
                return result.WasHandled ? AiActionResult.Progress() : AiActionResult.Rejected(result.Error);
            }
            else if (action == AiActionType.AdvanceToObjective)
            {
                bool inner = _commands.Dispatch(new EnterSiegeAreaCommand(_siegeId, SiegeArea.InnerArea)).WasHandled;
                bool objective = _commands.Dispatch(new EnterSiegeAreaCommand(_siegeId, SiegeArea.CaptureObjective)).WasHandled;
                return inner && objective ? AiActionResult.Progress() : AiActionResult.Rejected("Could not enter capture objective.");
            }
            else if (action == AiActionType.Capture)
            {
                CommandDispatchResult result = _commands.Dispatch(new CaptureSiegeCommand(_siegeId));
                return result.WasHandled ? AiActionResult.Progress() : AiActionResult.Rejected(result.Error);
            }
            else if (action == AiActionType.Recover)
            { _economy.AddResource(_homeSettlementId, _resourceId, 20); return AiActionResult.Progress(); }
            else if (action == AiActionType.HoldPosition || action == AiActionType.ProtectSiege)
                return AiActionResult.Progress();
            return AiActionResult.Waiting();
        }

        public void SpawnUnit(EntityId settlementId, EntityId factionId, DefinitionId unitId)
        {
            EntityId entityId = new EntityId(_nextUnitId++); _units.Add(entityId); _armies.RegisterMember(entityId, factionId);
            Combat.Register(entityId, new CombatantProfile(unitId.Value, factionId, 120,
                new AttackProfile(180, DamageType.Physical, 2, 1, 0, targetTags: new[] { "structure" }),
                tags: new[] { "unit", "siege" }), new WorldPoint(-5 + _units.Count, 0, -2));
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule); visual.name = $"AI_Unit_{_units.Count}";
            visual.transform.SetParent(transform); visual.transform.position = new Vector3(-5 + _units.Count, 1, -2);
            visual.transform.localScale = new Vector3(0.55f, 0.8f, 0.55f); SetColor(visual, new Color(0.2f, 0.65f, 1)); _unitVisuals.Add(visual);
        }

        private void AppendHistory(AiActionType action)
        { if (!ActionHistory.EndsWith(action.ToString(), StringComparison.Ordinal)) ActionHistory = string.IsNullOrEmpty(ActionHistory) ? action.ToString() : ActionHistory + " → " + action; }
        private void MoveUnitVisuals()
        { for (int i = 0; i < _unitVisuals.Count; i++) _unitVisuals[i].transform.position = new Vector3(-1 + i, 1, 3); }

        private void CreateWorldVisuals()
        {
            CreateMarker("AI_Home", new Vector3(-5, 0.5f, 0), new Color(0.2f, 0.65f, 1));
            CreateMarker("AI_Target", new Vector3(4, 0.5f, 4), new Color(0.9f, 0.25f, 0.2f));
            GameObject route = GameObject.CreatePrimitive(PrimitiveType.Cube); route.name = "AI_Route";
            route.transform.SetParent(transform); route.transform.position = new Vector3(-0.5f, 0.08f, 2);
            route.transform.localScale = new Vector3(10, 0.12f, 0.3f); route.transform.rotation = Quaternion.Euler(0, -24, 0);
            SetColor(route, new Color(0.8f, 0.7f, 0.2f));
        }
        private void CreateMarker(string name, Vector3 position, Color color)
        { GameObject value = GameObject.CreatePrimitive(PrimitiveType.Cylinder); value.name = name; value.transform.SetParent(transform); value.transform.position = position; value.transform.localScale = new Vector3(1.5f, 0.5f, 1.5f); SetColor(value, color); }
        private static ContentTag[] Tags(params string[] values)
        { var result = new ContentTag[values.Length]; for (int i = 0; i < values.Length; i++) result[i] = new ContentTag(values[i]); return result; }
        private static void SetColor(GameObject target, Color color)
        { Renderer renderer = target.GetComponent<Renderer>(); var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block); block.SetColor(Shader.PropertyToID("_BaseColor"), color); renderer.SetPropertyBlock(block); }
        private static void ConfigureCamera()
        { Camera camera = Camera.main; if (camera == null) return; camera.transform.position = new Vector3(0, 14, -14); camera.transform.rotation = Quaternion.Euler(43, 0, 0); }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 16, 760, 210), GUI.skin.box);
            GUILayout.Label("Phase 10 Utility AI — Strategic / Operational / Tactical / Unit");
            GUILayout.Label(AI?.GetDebugSummary() ?? "AI unavailable");
            if (AI != null && AI.TryGetState(AiFactionId, out AiAgentSnapshot state))
            {
                string route = string.Join(" → ", state.Route.Select(id => id.ToString()));
                string scores = string.Join(" | ", state.Scores.Take(5).Select(value => $"{value.Action}:{value.Score:0}"));
                GUILayout.Label($"Goal: {state.Goal} | Layer: {state.Layer} | Action: {state.Action} | Target: {state.TargetId}");
                GUILayout.Label($"Strength: {state.Strength:0} | Threat: {state.Threat:0} | Route: {route}");
                GUILayout.Label($"Scores: {scores}");
            }
            GUILayout.Label($"Units: {SpawnedUnitCount} | Decisions: {DecisionEventCount} | Acceptance: {(AcceptancePassed ? "PASS" : "RUNNING")}");
            GUILayout.EndArea();
        }

        private void OnDestroy()
        { for (int i = _disposables.Count - 1; i >= 0; i--) _disposables[i].Dispose(); _disposables.Clear(); }
    }
}
