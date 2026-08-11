using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Armies;
using AegisRTS.Gameplay.Heroes;
using AegisRTS.Gameplay.Units;
using UnityEngine;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Samples.BasicRTS
{
    public sealed class BasicRtsSample : MonoBehaviour
    {
        public bool AcceptancePassed { get; private set; }

        private void Awake()
        {
            var factionId = new EntityId(1); var heroId = new EntityId(10); var armyId = new EntityId(20);
            var heroes = new HeroSystem(); heroes.Register(heroId, new HeroProfile("sample.hero", factionId, 80));
            var armies = new ArmySystem(heroes); var members = new List<EntityId> { heroId };
            for (ulong value = 11; value <= 14; value++) members.Add(new EntityId(value));
            foreach (EntityId member in members) armies.RegisterMember(member, factionId);
            ArmyCommandResult created = armies.Execute(new CreateArmyCommand(armyId, factionId, members, heroId));
            ArmyCommandResult moved = armies.Execute(new MoveArmyCommand(armyId, new WorldPoint(8, 0, 5)));
            AcceptancePassed = created.Succeeded && moved.Succeeded && armies.TryGetState(armyId, out ArmySnapshot state) && state.UnitIds.Count == 5;
            CreateVisuals(members.Count); CreateCamera();
        }

        private static void CreateVisuals(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject unit = GameObject.CreatePrimitive(i == 0 ? PrimitiveType.Capsule : PrimitiveType.Cube);
                unit.name = i == 0 ? "Hero" : $"Unit_{i}"; unit.transform.position = new Vector3(i * 1.2f - 2.4f, 0.5f, 0);
            }
        }

        private static void CreateCamera()
        {
            GameObject value = new GameObject("Main Camera"); Camera camera = value.AddComponent<Camera>(); value.AddComponent<AudioListener>();
            value.tag = "MainCamera"; camera.transform.position = new Vector3(0, 8, -10); camera.transform.rotation = Quaternion.Euler(35, 0, 0);
        }

        private void OnGUI() => GUILayout.Label($"Basic RTS | Hero army and move order: {(AcceptancePassed ? "PASS" : "FAIL")}");
    }
}
