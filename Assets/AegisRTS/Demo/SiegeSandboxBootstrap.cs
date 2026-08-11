using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Factions;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Siege;
using AegisRTS.Gameplay.Territory;
using AegisRTS.Gameplay.Units;
using UnityEngine;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Demo
{
    /// <summary>Phase 09 break-gate, enter-city, capture, and ownership-transfer acceptance.</summary>
    [DisallowMultipleComponent]
    public sealed class SiegeSandboxBootstrap : MonoBehaviour
    {
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private SiegeCommandRouter _router;
        private GameObject _gateVisual;

        public SiegeSystem Sieges { get; private set; }
        public SettlementSystem Settlements { get; private set; }
        public TerritorySystem Territories { get; private set; }
        public RecordingSiegeNavigationSink Navigation { get; private set; }
        public EntityId SiegeId { get; private set; }
        public EntityId SettlementId { get; private set; }
        public EntityId AttackerFactionId { get; private set; }
        public EntityId DefenderFactionId { get; private set; }
        public int BreachEventCount { get; private set; }
        public int CompletionEventCount { get; private set; }
        public bool AcceptancePassed { get; private set; }

        private void Awake()
        {
            var events = new EventBus(); var commands = new CommandBus();
            var factions = new FactionSystem(events); Territories = new TerritorySystem(factions, events);
            Settlements = new SettlementSystem(factions, Territories, events);
            var combat = new CombatSystem(events); Navigation = new RecordingSiegeNavigationSink();
            AttackerFactionId = new EntityId(9001); DefenderFactionId = new EntityId(9002);
            SettlementId = new EntityId(9010); SiegeId = new EntityId(9020);
            EntityId territoryId = new EntityId(9011), ramId = new EntityId(9030), gateId = new EntityId(9040);
            factions.Register(AttackerFactionId, new FactionProfile("faction.siege.attacker"));
            factions.Register(DefenderFactionId, new FactionProfile("faction.siege.defender"));
            Territories.RegisterNode(territoryId, new TerritoryNodeProfile("territory.siege", 50, SettlementId), DefenderFactionId);
            Settlements.Register(SettlementId, new SettlementProfile("settlement.siege", 800, 150,
                new CaptureRule(CaptureRuleType.CaptureZone)), DefenderFactionId);
            combat.Register(ramId, new CombatantProfile("unit.siege.ram", AttackerFactionId, 300,
                new AttackProfile(180, DamageType.Physical, 2, 1, 0, targetTags: new[] { "structure" }),
                tags: new[] { "unit", "siege" }), new WorldPoint(0, 0, -4));

            Sieges = new SiegeSystem(new CombatSiegeAttackerQuery(combat), Navigation,
                new SettlementSiegeCaptureSink(Settlements), eventBus: events);
            Sieges.Register(SiegeId, new SiegeProfile(SettlementId, AttackerFactionId, DefenderFactionId, SiegeMode.Assault));
            Sieges.RegisterStructure(SiegeId, gateId, new DefenseStructureProfile("structure.city-gate",
                DefenseStructureKind.Gate, SiegeArea.Gates, DefenderFactionId, 150, 20));
            _router = new SiegeCommandRouter(commands, Sieges);
            _subscriptions.Add(events.Subscribe<BreachCreatedEvent>(_ => { BreachEventCount++; UpdateGateVisual(); }));
            _subscriptions.Add(events.Subscribe<SiegeCompletedEvent>(_ => CompletionEventCount++));
            BuildVisuals();

            commands.Dispatch(new StartSiegeCommand(SiegeId));
            commands.Dispatch(new AttackDefenseStructureCommand(SiegeId, ramId, gateId));
            commands.Dispatch(new EnterSiegeAreaCommand(SiegeId, SiegeArea.InnerArea));
            commands.Dispatch(new EnterSiegeAreaCommand(SiegeId, SiegeArea.CaptureObjective));
            commands.Dispatch(new CaptureSiegeCommand(SiegeId));

            AcceptancePassed = Sieges.TryGetState(SiegeId, out SiegeSnapshot siege) &&
                siege.State == SiegeState.Completed && siege.WinningFactionId == AttackerFactionId &&
                Settlements.TryGetState(SettlementId, out SettlementSnapshot settlement) &&
                settlement.OwnerId == AttackerFactionId && Navigation.RefreshCount == 1;
        }

        private void BuildVisuals()
        {
            var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube); leftWall.name = "Siege_Wall_Left";
            leftWall.transform.SetParent(transform); leftWall.transform.position = new Vector3(-3, 1.25f, 5);
            leftWall.transform.localScale = new Vector3(5, 2.5f, 0.6f); SetColor(leftWall, new Color(0.35f, 0.38f, 0.42f));
            var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube); rightWall.name = "Siege_Wall_Right";
            rightWall.transform.SetParent(transform); rightWall.transform.position = new Vector3(3, 1.25f, 5);
            rightWall.transform.localScale = new Vector3(5, 2.5f, 0.6f); SetColor(rightWall, new Color(0.35f, 0.38f, 0.42f));
            _gateVisual = GameObject.CreatePrimitive(PrimitiveType.Cube); _gateVisual.name = "Siege_Gate_Destroyed";
            _gateVisual.transform.SetParent(transform); _gateVisual.transform.position = new Vector3(0, 0.35f, 5);
            _gateVisual.transform.localScale = new Vector3(1, 0.7f, 0.5f); SetColor(_gateVisual, new Color(0.2f, 0.12f, 0.08f));
            var objective = GameObject.CreatePrimitive(PrimitiveType.Cylinder); objective.name = "Siege_CaptureObjective";
            objective.transform.SetParent(transform); objective.transform.position = new Vector3(0, 0.1f, 8);
            objective.transform.localScale = new Vector3(1.3f, 0.1f, 1.3f); SetColor(objective, new Color(0.15f, 0.55f, 1f));
        }

        private void UpdateGateVisual()
        { if (_gateVisual != null) { _gateVisual.transform.localScale = new Vector3(1, 0.2f, 0.5f); SetColor(_gateVisual, new Color(0.08f, 0.08f, 0.08f)); } }
        private static void SetColor(GameObject target, Color color)
        { var renderer = target.GetComponent<Renderer>(); var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block); block.SetColor(Shader.PropertyToID("_BaseColor"), color); renderer.SetPropertyBlock(block); }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 312, 620, 125), GUI.skin.box);
            GUILayout.Label("Phase 09 Siege / Defense");
            GUILayout.Label(Sieges?.GetDebugSummary() ?? "Siege unavailable");
            GUILayout.Label($"Breach events: {BreachEventCount} | Navigation refresh: {Navigation?.RefreshCount ?? 0} | Completed: {CompletionEventCount}");
            GUILayout.Label($"Break Gate → Enter → Capture → Owner Change: {(AcceptancePassed ? "PASS" : "FAIL")}");
            GUILayout.EndArea();
        }

        private void OnDestroy()
        { _router?.Dispose(); foreach (IDisposable subscription in _subscriptions) subscription.Dispose(); _subscriptions.Clear(); }
    }
}
