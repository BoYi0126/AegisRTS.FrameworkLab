using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Factions;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Siege;
using AegisRTS.Gameplay.Territory;
using AegisRTS.Gameplay.Units;
using UnityEngine;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Samples.BasicSiege
{
    public sealed class BasicSiegeSample : MonoBehaviour
    {
        public bool AcceptancePassed { get; private set; }

        private void Awake()
        {
            var events = new EventBus(); var attackerFaction = new EntityId(1); var defenderFaction = new EntityId(2);
            var settlementId = new EntityId(10); var territoryId = new EntityId(11); var armyId = new EntityId(20);
            var attackerId = new EntityId(21); var siegeId = new EntityId(30); var gateId = new EntityId(31);
            var factions = new FactionSystem(events); factions.Register(attackerFaction, new FactionProfile("sample.attackers"));
            factions.Register(defenderFaction, new FactionProfile("sample.defenders")); factions.AssignArmy(armyId, attackerFaction);
            var territories = new TerritorySystem(factions, events);
            territories.RegisterNode(territoryId, new TerritoryNodeProfile("sample.fortress", 100, settlementId), defenderFaction);
            var settlements = new SettlementSystem(factions, territories, events);
            settlements.Register(settlementId, new SettlementProfile("sample.fortress", 100, 100,
                new CaptureRule(CaptureRuleType.CaptureZone)), defenderFaction);
            var combat = new CombatSystem(events);
            combat.Register(attackerId, new CombatantProfile("sample.ram", attackerFaction, 200,
                new AttackProfile(200, DamageType.Physical, 2, 0, 0, targetTags: new[] { "structure" }), tags: new[] { "unit", "siege" }),
                new WorldPoint(0, 0, 0));
            var sieges = new SiegeSystem(new CombatSiegeAttackerQuery(combat), new RecordingSiegeNavigationSink(),
                new SettlementSiegeCaptureSink(settlements), eventBus: events);
            sieges.Register(siegeId, new SiegeProfile(settlementId, attackerFaction, defenderFaction, SiegeMode.Assault, armyId));
            sieges.RegisterStructure(siegeId, gateId, new DefenseStructureProfile("sample.gate", DefenseStructureKind.Gate,
                SiegeArea.Gates, defenderFaction, 100, 5));
            bool started = sieges.Execute(new StartSiegeCommand(siegeId)).Succeeded;
            bool breached = sieges.Execute(new AttackDefenseStructureCommand(siegeId, attackerId, gateId)).Succeeded;
            bool inner = sieges.Execute(new EnterSiegeAreaCommand(siegeId, SiegeArea.InnerArea)).Succeeded;
            bool objective = sieges.Execute(new EnterSiegeAreaCommand(siegeId, SiegeArea.CaptureObjective)).Succeeded;
            bool captured = sieges.Execute(new CaptureSiegeCommand(siegeId)).Succeeded;
            AcceptancePassed = started && breached && inner && objective && captured &&
                settlements.TryGetState(settlementId, out SettlementSnapshot state) && state.OwnerId == attackerFaction;
            CreateMarker("Battering Ram", new Vector3(-2, 0.6f, 0), new Vector3(2, 1, 1), Color.blue);
            CreateMarker("Broken Gate", new Vector3(2, 1.5f, 0), new Vector3(0.6f, 3, 4), Color.red); CreateCamera();
        }

        private static void CreateMarker(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject value = GameObject.CreatePrimitive(PrimitiveType.Cube); value.name = name;
            value.transform.position = position; value.transform.localScale = scale;
            Renderer renderer = value.GetComponent<Renderer>(); var block = new MaterialPropertyBlock(); block.SetColor(Shader.PropertyToID("_BaseColor"), color); renderer.SetPropertyBlock(block);
        }

        private static void CreateCamera()
        {
            GameObject value = new GameObject("Main Camera"); Camera camera = value.AddComponent<Camera>(); value.AddComponent<AudioListener>();
            value.tag = "MainCamera"; camera.transform.position = new Vector3(0, 8, -11); camera.transform.rotation = Quaternion.Euler(35, 0, 0);
        }

        private void OnGUI() => GUILayout.Label($"Basic Siege | Breach and capture: {(AcceptancePassed ? "PASS" : "FAIL")}");
    }
}
