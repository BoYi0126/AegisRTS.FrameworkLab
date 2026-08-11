using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Armies;
using AegisRTS.Gameplay.Formation;
using AegisRTS.Gameplay.Heroes;
using AegisRTS.Gameplay.Units;
using UnityEngine;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Demo
{
    /// <summary>Runs the Phase 06 hero plus twenty infantry army composition acceptance flow.</summary>
    [DisallowMultipleComponent]
    public sealed class ArmySandboxBootstrap : MonoBehaviour
    {
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly List<GameObject> _visuals = new List<GameObject>();
        private EntityIdGenerator _ids;
        private EventBus _events;
        private CommandBus _commands;
        private HeroSystem _heroes;
        private ArmySystem _armies;
        private ArmyCommandRouter _router;

        public HeroSystem Heroes => _heroes;
        public ArmySystem Armies => _armies;
        public CommandBus Commands => _commands;
        public EntityId AcceptanceArmyId { get; private set; }
        public EntityId AcceptanceCommanderId { get; private set; }
        public int ArmyVisualCount => _visuals.Count;
        public int CreatedEventCount { get; private set; }
        public int SplitEventCount { get; private set; }
        public int MergeEventCount { get; private set; }
        public int CommanderEventCount { get; private set; }
        public int OrderEventCount { get; private set; }
        public bool AcceptancePassed { get; private set; }

        private void Awake()
        {
            _ids = new EntityIdGenerator(2000);
            _events = new EventBus();
            _commands = new CommandBus();
            _heroes = new HeroSystem();
            _armies = new ArmySystem(_heroes, new ArmyRuleOptions(true, true), eventBus: _events);
            _router = new ArmyCommandRouter(_commands, _armies);
            SubscribeEvents();
            RunAcceptanceScenario();
        }

        private void SubscribeEvents()
        {
            _subscriptions.Add(_events.Subscribe<ArmyCreatedEvent>(_ => CreatedEventCount++));
            _subscriptions.Add(_events.Subscribe<ArmySplitEvent>(_ => SplitEventCount++));
            _subscriptions.Add(_events.Subscribe<ArmiesMergedEvent>(_ => MergeEventCount++));
            _subscriptions.Add(_events.Subscribe<ArmyCommanderAssignedEvent>(_ => CommanderEventCount++));
            _subscriptions.Add(_events.Subscribe<ArmyOrderIssuedEvent>(_ => OrderEventCount++));
        }

        private void RunAcceptanceScenario()
        {
            EntityId faction = new EntityId(9000);
            var members = new List<EntityId>();
            EntityId firstHero = SpawnMember("Army_Hero", new Vector3(-9, 1, -5), faction, new Color(1f, 0.75f, 0.1f));
            members.Add(firstHero);
            for (int index = 0; index < 20; index++)
            {
                float x = -8f + (index % 10) * 1.7f;
                float z = -7f - (index / 10) * 1.7f;
                members.Add(SpawnMember($"Army_Infantry_{index + 1:00}", new Vector3(x, 0.6f, z), faction,
                    new Color(0.18f, 0.55f, 0.95f)));
            }

            EntityId secondHero = members[1];
            _heroes.Register(firstHero, new HeroProfile("hero.debug.commander", faction, 90, new[] { "ability.debug.rally" }));
            _heroes.Register(secondHero, new HeroProfile("hero.debug.captain", faction, 65));
            EntityId firstArmy = _ids.Next();
            EntityId splitArmy = _ids.Next();

            _commands.Dispatch(new CreateArmyCommand(firstArmy, faction, members, firstHero, FormationType.Box));
            var splitMembers = new List<EntityId>();
            for (int index = 1; index <= 10; index++) splitMembers.Add(members[index]);
            _commands.Dispatch(new SplitArmyCommand(firstArmy, splitArmy, splitMembers, secondHero));
            _commands.Dispatch(new MergeArmiesCommand(firstArmy, splitArmy));
            _commands.Dispatch(new AssignArmyCommanderCommand(firstArmy, secondHero));
            _commands.Dispatch(new MoveArmyCommand(firstArmy, new WorldPoint(3, 0, -4)));
            _commands.Dispatch(new DefendArmyCommand(firstArmy, new WorldPoint(3, 0, -4)));
            _commands.Dispatch(new RetreatArmyCommand(firstArmy, new WorldPoint(-6, 0, -8)));

            AcceptanceArmyId = firstArmy;
            AcceptanceCommanderId = secondHero;
            AcceptancePassed = _armies.TryGetState(firstArmy, out ArmySnapshot final) &&
                final.UnitCount == 21 && final.CommanderId == secondHero && _armies.ArmyCount == 1;
            ApplyFinalVisuals();
        }

        private EntityId SpawnMember(string objectName, Vector3 position, EntityId faction, Color color)
        {
            EntityId entityId = _ids.Next();
            GameObject visual = GameObject.CreatePrimitive(objectName.Contains("Hero") ? PrimitiveType.Capsule : PrimitiveType.Cube);
            visual.name = objectName;
            visual.transform.SetParent(transform);
            visual.transform.position = position;
            visual.transform.localScale = objectName.Contains("Hero") ? new Vector3(0.8f, 1.2f, 0.8f) : Vector3.one * 0.75f;
            SetColor(visual, color);
            _visuals.Add(visual);
            _armies.RegisterMember(entityId, faction);
            return entityId;
        }

        private void ApplyFinalVisuals()
        {
            for (int index = 0; index < _visuals.Count; index++)
                SetColor(_visuals[index], index == 1 ? new Color(0.2f, 1f, 0.45f) : new Color(0.18f, 0.55f, 0.95f));
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
            GUILayout.BeginArea(new Rect(16, 180, 590, 145), GUI.skin.box);
            GUILayout.Label("Phase 06 Hero / Army / Command");
            GUILayout.Label(_heroes?.GetDebugSummary() ?? "Heroes unavailable");
            GUILayout.Label(_armies?.GetDebugSummary() ?? "Armies unavailable");
            GUILayout.Label($"Create {CreatedEventCount} | Split {SplitEventCount} | Merge {MergeEventCount} | Commander {CommanderEventCount} | Orders {OrderEventCount}");
            GUILayout.Label($"Hero + 20 infantry acceptance: {(AcceptancePassed ? "PASS" : "FAIL")}");
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
