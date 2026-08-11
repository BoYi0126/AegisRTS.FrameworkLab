using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Abilities;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Units;
using AegisRTS.Presentation.Combat;
using UnityEngine;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Demo
{
    /// <summary>Composes the Phase 05 combat and ability acceptance sandbox.</summary>
    [DisallowMultipleComponent]
    public sealed class CombatSandboxBootstrap : MonoBehaviour
    {
        private readonly List<IDisposable> _registrations = new List<IDisposable>();
        private EntityIdGenerator _ids;
        private EventBus _events;
        private CommandBus _commands;
        private CombatSystem _combat;
        private UnityCombatDriver _driver;
        private EntityId _meleeId;
        private EntityId _archerId;
        private EntityId _meleeTargetId;
        private EntityId _splashTargetId;

        public CombatSystem Combat => _combat;
        public UnityCombatDriver Driver => _driver;
        public CommandBus Commands => _commands;
        public bool AcceptanceScenarioStarted { get; private set; }
        public int SpawnedCombatantCount { get; private set; }

        private void Awake()
        {
            _ids = new EntityIdGenerator();
            _events = new EventBus();
            _commands = new CommandBus();
            _combat = new CombatSystem(_events);
            _driver = gameObject.AddComponent<UnityCombatDriver>();
            _driver.Initialize(_combat, _events);
            CreateArena();
            RegisterProfilesAndAbilities();
            SpawnCombatants();
            RegisterCommandHandlers();
            ConfigureCamera();
            RunAcceptanceScenario();
        }

        public void RunAcceptanceScenario()
        {
            if (AcceptanceScenarioStarted) return;
            AcceptanceScenarioStarted = true;
            _commands.Dispatch(new AttackTargetCommand(new[] { _meleeId }, _meleeTargetId));
            _commands.Dispatch(new AttackTargetCommand(new[] { _archerId }, _splashTargetId));
            _commands.Dispatch(new UseAbilityCommand(_archerId, "ability.debug.frost_burn", targetPoint: new WorldPoint(4, 0, 2)));
        }

        private void RegisterProfilesAndAbilities()
        {
            _combat.RegisterAbility(new AbilityProfile(
                "ability.debug.frost_burn", AbilityTargetType.Area, AbilityActivationType.Active,
                cooldownSeconds: 5, range: 12, radius: 2.5, damage: 8, damageType: DamageType.Magical,
                statusEffect: new StatusEffectProfile("status.debug.burn", StatusEffectKind.DamageOverTime,
                    durationSeconds: 3, magnitude: 8, tickIntervalSeconds: 1, damageType: DamageType.True)));
        }

        private void SpawnCombatants()
        {
            EntityId blue = new EntityId(1001);
            EntityId red = new EntityId(1002);
            _meleeId = Spawn("Blue_Melee", PrimitiveType.Capsule, new Vector3(-4, 0, -2), blue,
                new CombatantProfile("unit.debug.melee", blue, 100,
                    new AttackProfile(26, DamageType.Physical, 2.2, 0.8, 0.2),
                    tags: new[] { "ground", "infantry" }), new Color(0.1f, 0.45f, 0.95f));
            _meleeTargetId = Spawn("Red_Melee_Target", PrimitiveType.Capsule, new Vector3(-2.5f, 0, -2), red,
                new CombatantProfile("unit.debug.target", red, 45,
                    new AttackProfile(8, DamageType.Physical, 2, 1, 0.1), new DefenseProfile(2),
                    tags: new[] { "ground", "infantry" }), new Color(0.9f, 0.15f, 0.12f));
            _archerId = Spawn("Blue_Archer", PrimitiveType.Cylinder, new Vector3(-4, 0, 2), blue,
                new CombatantProfile("unit.debug.archer", blue, 80,
                    new AttackProfile(18, DamageType.Physical, 12, 1.2, 0.15, projectileSpeed: 6, splashRadius: 2.2),
                    tags: new[] { "ground", "ranged" }, abilityIds: new[] { "ability.debug.frost_burn" }),
                new Color(0.1f, 0.75f, 0.95f));
            _splashTargetId = Spawn("Red_Splash_Target", PrimitiveType.Cube, new Vector3(4, 0, 2), red,
                new CombatantProfile("unit.debug.splash", red, 75,
                    new AttackProfile(6, DamageType.Physical, 2, 1, 0), tags: new[] { "ground" }),
                new Color(0.95f, 0.25f, 0.12f));
            Spawn("Red_Splash_Nearby", PrimitiveType.Cube, new Vector3(5.3f, 0, 2.5f), red,
                new CombatantProfile("unit.debug.splash", red, 75,
                    new AttackProfile(6, DamageType.Physical, 2, 1, 0), tags: new[] { "ground" }),
                new Color(0.95f, 0.35f, 0.12f));
            Spawn("Red_Outside_Splash", PrimitiveType.Cube, new Vector3(8, 0, 5), red,
                new CombatantProfile("unit.debug.control", red, 75,
                    new AttackProfile(6, DamageType.Physical, 2, 1, 0), tags: new[] { "ground" }),
                new Color(0.75f, 0.2f, 0.15f));
        }

        private EntityId Spawn(string objectName, PrimitiveType primitive, Vector3 position, EntityId faction,
            CombatantProfile profile, Color color)
        {
            EntityId entityId = _ids.Next();
            GameObject visual = GameObject.CreatePrimitive(primitive);
            visual.name = objectName;
            visual.transform.SetParent(transform);
            visual.transform.position = position + Vector3.up;
            var view = visual.AddComponent<UnityCombatView>();
            view.Configure(entityId, color);
            _combat.Register(entityId, profile, new WorldPoint(visual.transform.position.x, visual.transform.position.y, visual.transform.position.z));
            _driver.RegisterView(view);
            SpawnedCombatantCount++;
            return entityId;
        }

        private void RegisterCommandHandlers()
        {
            _registrations.Add(_commands.RegisterHandler<AttackTargetCommand>(command => _combat.IssueAttack(command)));
            _registrations.Add(_commands.RegisterHandler<UseAbilityCommand>(command => _combat.IssueAbility(command)));
        }

        private void CreateArena()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Combat_Arena";
            ground.transform.SetParent(transform);
            ground.transform.localScale = new Vector3(2, 1, 1.3f);
            SetColor(ground.GetComponent<Renderer>(), new Color(0.12f, 0.18f, 0.14f));
        }

        private static void ConfigureCamera()
        {
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            if (camera == null) return;
            camera.transform.position = new Vector3(2, 15, -14);
            camera.transform.rotation = Quaternion.Euler(42, 0, 0);
        }

        private static void SetColor(Renderer renderer, Color color)
        {
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor(Shader.PropertyToID("_BaseColor"), color);
            renderer.SetPropertyBlock(block);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 16, 590, 155), GUI.skin.box);
            GUILayout.Label("Phase 05 Unit Combat / Damage / Ability / Status");
            GUILayout.Label(_combat?.GetDebugSummary() ?? "Combat unavailable");
            GUILayout.Label($"Damage events: {_driver?.DamageEventCount ?? 0} | Projectiles: {_driver?.ProjectileVisualCount ?? 0}");
            GUILayout.Label($"Statuses: {_driver?.StatusEventCount ?? 0} | Deaths: {_driver?.DeathEventCount ?? 0}");
            GUILayout.Label("Blue capsule: melee | Blue cylinder: projectile + area DoT | Red cubes: splash validation");
            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            foreach (IDisposable registration in _registrations) registration.Dispose();
            _registrations.Clear();
        }
    }
}
