using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Factions;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Territory;
using UnityEngine;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Demo
{
    /// <summary>Phase 07 three-settlement ownership and territory graph acceptance sandbox.</summary>
    [DisallowMultipleComponent]
    public sealed class FactionTerritorySandboxBootstrap : MonoBehaviour
    {
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly List<GameObject> _settlementVisuals = new List<GameObject>();
        private EventBus _events;
        private CommandBus _commands;
        private SettlementCommandRouter _router;

        public FactionSystem Factions { get; private set; }
        public SettlementSystem Settlements { get; private set; }
        public TerritorySystem Territories { get; private set; }
        public EntityId InitialFactionId { get; private set; }
        public EntityId CapturingFactionId { get; private set; }
        public int SettlementVisualCount => _settlementVisuals.Count;
        public int ConnectionVisualCount { get; private set; }
        public int SettlementCaptureEventCount { get; private set; }
        public int TerritoryOwnerEventCount { get; private set; }
        public bool AcceptancePassed { get; private set; }

        private void Awake()
        {
            _events = new EventBus();
            _commands = new CommandBus();
            Factions = new FactionSystem(_events);
            Territories = new TerritorySystem(Factions, _events);
            Settlements = new SettlementSystem(Factions, Territories, _events);
            _router = new SettlementCommandRouter(_commands, Settlements);
            _subscriptions.Add(_events.Subscribe<SettlementOwnerChangedEvent>(_ => SettlementCaptureEventCount++));
            _subscriptions.Add(_events.Subscribe<TerritoryOwnerChangedEvent>(_ => TerritoryOwnerEventCount++));
            BuildAcceptanceScenario();
            ConfigureCamera();
        }

        private void BuildAcceptanceScenario()
        {
            InitialFactionId = new EntityId(7001);
            CapturingFactionId = new EntityId(7002);
            Factions.Register(InitialFactionId, new FactionProfile("faction.debug.defender", "ai.defend"));
            Factions.Register(CapturingFactionId, new FactionProfile("faction.debug.attacker", "ai.attack"));
            Factions.SetDiplomacy(InitialFactionId, CapturingFactionId, DiplomacyStatus.War);
            Factions.AddResource(InitialFactionId, "resource.debug", 100);
            Factions.UnlockTechnology(CapturingFactionId, "technology.debug.capture");

            EntityId[] settlementIds = { new EntityId(7101), new EntityId(7102), new EntityId(7103) };
            EntityId[] territoryIds = { new EntityId(7201), new EntityId(7202), new EntityId(7203) };
            Vector3[] positions = { new Vector3(-6, 1, 0), new Vector3(0, 1, 0), new Vector3(6, 1, 0) };
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

            CreateGround();
            for (int index = 0; index < 3; index++)
            {
                Territories.RegisterNode(territoryIds[index], new TerritoryNodeProfile($"territory.debug.{index + 1}",
                    (index + 1) * 10, settlementIds[index]), InitialFactionId);
                Territories.SetVisibility(territoryIds[index], CapturingFactionId,
                    index == 2 ? TerritoryVisibility.Explored : TerritoryVisibility.Visible);
                Settlements.Register(settlementIds[index], new SettlementProfile($"settlement.debug.{index + 1}",
                    1000 + index * 250, 100 + index * 25, rules[index]), InitialFactionId);
                _settlementVisuals.Add(CreateSettlementVisual($"Settlement_{index + 1}", positions[index], Color.red));
                _commands.Dispatch(new CaptureSettlementCommand(settlementIds[index], CapturingFactionId, evidence[index]));
            }
            Territories.Connect(territoryIds[0], territoryIds[1]);
            Territories.Connect(territoryIds[1], territoryIds[2]);
            CreateConnection(positions[0], positions[1]);
            CreateConnection(positions[1], positions[2]);
            foreach (GameObject visual in _settlementVisuals) SetColor(visual, new Color(0.15f, 0.55f, 1f));

            AcceptancePassed = Factions.TryGetState(CapturingFactionId, out FactionSnapshot faction) &&
                faction.SettlementIds.Count == 3 && faction.TerritoryIds.Count == 3 &&
                Settlements.SettlementCount == 3 && Territories.TerritoryCount == 3;
        }

        private void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Territory_Ground";
            ground.transform.SetParent(transform);
            ground.transform.localScale = new Vector3(2, 1, 1);
            SetColor(ground, new Color(0.12f, 0.18f, 0.14f));
        }

        private GameObject CreateSettlementVisual(string objectName, Vector3 position, Color color)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = objectName;
            visual.transform.SetParent(transform);
            visual.transform.position = position;
            visual.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
            SetColor(visual, color);
            return visual;
        }

        private void CreateConnection(Vector3 start, Vector3 end)
        {
            GameObject connection = GameObject.CreatePrimitive(PrimitiveType.Cube);
            connection.name = $"TerritoryConnection_{ConnectionVisualCount + 1}";
            connection.transform.SetParent(transform);
            connection.transform.position = (start + end) * 0.5f + Vector3.down * 0.65f;
            connection.transform.localScale = new Vector3(Vector3.Distance(start, end) - 2f, 0.12f, 0.25f);
            SetColor(connection, new Color(0.85f, 0.75f, 0.25f));
            ConnectionVisualCount++;
        }

        private static void ConfigureCamera()
        {
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            if (camera == null) return;
            camera.transform.position = new Vector3(0, 13, -14);
            camera.transform.rotation = Quaternion.Euler(42, 0, 0);
        }

        private static void SetColor(GameObject target, Color color)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor(Shader.PropertyToID("_BaseColor"), color);
            renderer.SetPropertyBlock(block);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 16, 620, 155), GUI.skin.box);
            GUILayout.Label("Phase 07 Faction / Settlement / Territory");
            GUILayout.Label(Factions?.GetDebugSummary() ?? "Factions unavailable");
            GUILayout.Label(Settlements?.GetDebugSummary() ?? "Settlements unavailable");
            GUILayout.Label(Territories?.GetDebugSummary() ?? "Territories unavailable");
            GUILayout.Label($"Settlement captures: {SettlementCaptureEventCount} | Territory transfers: {TerritoryOwnerEventCount} | Acceptance: {(AcceptancePassed ? "PASS" : "FAIL")}");
            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            _router?.Dispose();
            foreach (IDisposable subscription in _subscriptions) subscription.Dispose();
            _subscriptions.Clear();
        }
    }
}
