using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Units;
using UnityEngine;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Samples.BasicCombat
{
    public sealed class BasicCombatSample : MonoBehaviour
    {
        public bool AcceptancePassed { get; private set; }

        private void Awake()
        {
            var events = new EventBus(); var combat = new CombatSystem(events);
            var attacker = new EntityId(1); var target = new EntityId(2);
            combat.Register(attacker, new CombatantProfile("sample.attacker", new EntityId(10), 100,
                new AttackProfile(150, DamageType.Physical, 3, 0, 0), tags: new[] { "unit" }), new WorldPoint(-1, 0, 0));
            combat.Register(target, new CombatantProfile("sample.target", new EntityId(20), 100,
                new AttackProfile(10, DamageType.Physical, 2, 1, 0), tags: new[] { "unit" }), new WorldPoint(1, 0, 0));
            combat.IssueAttack(new AttackTargetCommand(new[] { attacker }, target)); combat.Tick(0.1);
            AcceptancePassed = combat.TryGetState(target, out CombatantSnapshot state) && !state.IsAlive;
            CreateMarker("Attacker", new Vector3(-1, 0.5f, 0), Color.blue);
            CreateMarker("Defeated Target", new Vector3(1, 0.25f, 0), Color.red); CreateCamera();
        }

        private static void CreateMarker(string name, Vector3 position, Color color)
        {
            GameObject value = GameObject.CreatePrimitive(PrimitiveType.Capsule); value.name = name; value.transform.position = position;
            Renderer renderer = value.GetComponent<Renderer>(); var block = new MaterialPropertyBlock(); block.SetColor(Shader.PropertyToID("_BaseColor"), color); renderer.SetPropertyBlock(block);
        }

        private static void CreateCamera()
        {
            GameObject value = new GameObject("Main Camera"); Camera camera = value.AddComponent<Camera>(); value.AddComponent<AudioListener>();
            value.tag = "MainCamera"; camera.transform.position = new Vector3(0, 6, -9); camera.transform.rotation = Quaternion.Euler(30, 0, 0);
        }

        private void OnGUI() => GUILayout.Label($"Basic Combat | Attack and death flow: {(AcceptancePassed ? "PASS" : "FAIL")}");
    }
}
