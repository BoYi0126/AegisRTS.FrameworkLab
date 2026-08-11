using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Movement;
using AegisRTS.Gameplay.Units;
using AegisRTS.Presentation.Camera;
using AegisRTS.Presentation.Input;
using AegisRTS.Presentation.Movement;
using AegisRTS.Presentation.Selection;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Demo
{
    /// <summary>Composes the RTS input, selection, camera, movement, and navigation acceptance sandbox.</summary>
    [DisallowMultipleComponent]
    public sealed class RtsSandboxBootstrap : MonoBehaviour
    {
        private readonly List<IDisposable> _commandRegistrations = new List<IDisposable>();
        private readonly List<EntityId> _debugUnitIds = new List<EntityId>();
        private EntityIdGenerator _ids;
        private SelectionService _selection;
        private CommandBus _commands;
        private MovementSystem _movement;
        private NavMeshMovementAdapter _navigation;
        private UnityRtsInputAdapter _input;

        public int DebugUnitCount { get; private set; }
        public string LastCommandSummary { get; private set; } = "No command issued";
        public SelectionService Selection => _selection;
        public CommandBus Commands => _commands;
        public MovementSystem Movement => _movement;
        public NavMeshMovementAdapter Navigation => _navigation;
        public bool NavigationReady { get; private set; }

        private void Awake()
        {
            _ids = new EntityIdGenerator();
            _selection = new SelectionService();
            _commands = new CommandBus();
            CreateGroundAndObstacles();
            BuildNavigation();
            ComposeMovement();
            SpawnAcceptanceActors();
            RegisterCommandHandlers();
            ComposeCameraAndInput();
        }

        public CommandDispatchResult IssueAcceptanceMove(WorldPoint destination)
        {
            return _commands.Dispatch(new MoveUnitsCommand(_debugUnitIds, destination));
        }

        private void CreateGroundAndObstacles()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_Debug";
            ground.transform.SetParent(transform);
            ground.transform.localScale = new Vector3(10f, 1f, 10f);
            SetColor(ground, new Color(0.16f, 0.23f, 0.18f));

            CreateObstacle("Obstacle_Center", new Vector3(0f, 1.5f, 0f), new Vector3(3f, 3f, 22f));
            CreateObstacle("Obstacle_North", new Vector3(12f, 1.5f, 13f), new Vector3(21f, 3f, 3f));
            CreateObstacle("Obstacle_South", new Vector3(-10f, 1.5f, -18f), new Vector3(18f, 3f, 3f));
        }

        private void CreateObstacle(string objectName, Vector3 position, Vector3 scale)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = objectName;
            obstacle.transform.SetParent(transform);
            obstacle.transform.position = position;
            obstacle.transform.localScale = scale;
            SetColor(obstacle, new Color(0.32f, 0.29f, 0.25f));
        }

        private void BuildNavigation()
        {
            NavMeshSurface surface = gameObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = ~0;
            surface.ignoreNavMeshAgent = true;
            surface.ignoreNavMeshObstacle = true;
            surface.BuildNavMesh();
            NavigationReady = surface.navMeshData != null;
        }

        private void ComposeMovement()
        {
            _navigation = gameObject.AddComponent<NavMeshMovementAdapter>();
            _movement = new MovementSystem(_navigation);
            gameObject.AddComponent<UnityMovementDriver>().Initialize(_movement);
        }

        private void SpawnAcceptanceActors()
        {
            for (int index = 0; index < 50; index++)
            {
                float x = -36f + (index % 10) * 1.9f;
                float z = -8f + (index / 10) * 1.9f;
                UnitySelectableView unit = CreateSelectable(
                    $"Friendly_Unit_{index + 1:00}", PrimitiveType.Capsule, new Vector3(x, 0f, z),
                    "unit.debug.infantry", SelectableKind.Unit, SelectionAffiliation.Friendly,
                    new Color(0.1f, 0.45f, 0.95f));
                if (!_navigation.Register(unit, _movement))
                    throw new InvalidOperationException($"Could not register debug unit {unit.EntityId} on the runtime NavMesh.");
                _debugUnitIds.Add(unit.EntityId);
                DebugUnitCount++;
            }

            CreateSelectable(
                "Friendly_Target", PrimitiveType.Cube, new Vector3(25f, 0f, -8f),
                "unit.debug.support", SelectableKind.Unit, SelectionAffiliation.Friendly,
                new Color(0.15f, 0.75f, 0.95f));
            CreateSelectable(
                "Enemy_Target", PrimitiveType.Cube, new Vector3(25f, 0f, 2f),
                "unit.debug.enemy", SelectableKind.Unit, SelectionAffiliation.Enemy,
                new Color(0.9f, 0.15f, 0.12f));
            GameObject settlement = CreateSelectable(
                "Settlement_Target", PrimitiveType.Cylinder, new Vector3(25f, 0f, 20f),
                "settlement.debug", SelectableKind.Settlement, SelectionAffiliation.Neutral,
                new Color(0.9f, 0.65f, 0.08f)).gameObject;
            settlement.transform.GetChild(0).localScale = new Vector3(2.5f, 1f, 2.5f);
        }

        private UnitySelectableView CreateSelectable(
            string objectName, PrimitiveType primitive, Vector3 position, string definitionId,
            SelectableKind kind, SelectionAffiliation affiliation, Color color)
        {
            var root = new GameObject(objectName);
            root.transform.SetParent(transform);
            root.transform.position = position;
            GameObject visual = GameObject.CreatePrimitive(primitive);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, primitive == PrimitiveType.Cube ? 0.75f : 1f, 0f);
            if (primitive == PrimitiveType.Cube) visual.transform.localScale = new Vector3(1.2f, 1.5f, 1.2f);

            var view = root.AddComponent<UnitySelectableView>();
            view.Configure(_ids.Next(), definitionId, kind, affiliation, _selection, color);
            return view;
        }

        private void RegisterCommandHandlers()
        {
            _commandRegistrations.Add(_commands.RegisterHandler<MoveUnitsCommand>(command =>
            {
                MovementCommandResult result = _movement.IssueMove(command);
                SetLastCommand(
                    $"Move {result.AcceptedActorCount}/{command.ActorIds.Count} actor(s) as {command.Formation} to {command.Destination}, Queue={command.Queue}");
            }));
            _commandRegistrations.Add(_commands.RegisterHandler<AttackTargetCommand>(command =>
                SetLastCommand($"Attack {command.TargetId} with {command.ActorIds.Count} actor(s), Queue={command.Queue}")));
            _commandRegistrations.Add(_commands.RegisterHandler<FollowTargetCommand>(command =>
                SetLastCommand($"Follow {command.TargetId} with {command.ActorIds.Count} actor(s), Queue={command.Queue}")));
            _commandRegistrations.Add(_commands.RegisterHandler<InteractTargetCommand>(command =>
                SetLastCommand($"Interact with {command.TargetId} using {command.ActorIds.Count} actor(s), Queue={command.Queue}")));
            _commandRegistrations.Add(_commands.RegisterHandler<StopUnitsCommand>(command =>
            {
                _movement.IssueStop(command);
                SetLastCommand($"Stop {command.ActorIds.Count} actor(s)");
            }));
            _commandRegistrations.Add(_commands.RegisterHandler<HoldUnitsCommand>(command =>
            {
                _movement.IssueHold(command);
                SetLastCommand($"Hold {command.ActorIds.Count} actor(s), Queue={command.Queue}");
            }));
        }

        private void ComposeCameraAndInput()
        {
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                mainCamera = cameraObject.AddComponent<UnityEngine.Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            RtsCameraController cameraController = mainCamera.GetComponent<RtsCameraController>();
            if (cameraController == null) cameraController = mainCamera.gameObject.AddComponent<RtsCameraController>();
            cameraController.Initialize(new RtsCameraRigModel(pivotX: 0d, pivotZ: 0d, zoom: 36d));

            GameObject inputObject = new GameObject("RTS_InputAdapter");
            inputObject.transform.SetParent(transform);
            _input = inputObject.AddComponent<UnityRtsInputAdapter>();
            _input.Initialize(_selection, _commands, cameraController);
        }

        private void SetLastCommand(string summary)
        {
            LastCommandSummary = summary;
            Debug.Log($"[Phase04] {summary}", this);
        }

        private static void SetColor(GameObject target, Color color)
        {
            Renderer renderer = target.GetComponentInChildren<Renderer>();
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor(Shader.PropertyToID("_BaseColor"), color);
            renderer.SetPropertyBlock(block);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16f, 16f, 620f, 175f), GUI.skin.box);
            GUILayout.Label("Phase 04 Movement / Navigation / Formation");
            GUILayout.Label("LMB select | RMB move | Shift queue | Tab Line/Box | X stop | H hold");
            GUILayout.Label("WASD/edge/MMB pan | Wheel zoom | F focus | Scene gizmos: path/destination/velocity");
            GUILayout.Label($"Selected: {_selection?.SelectedIds.Count ?? 0} | Formation: {_input?.ActiveFormation}");
            GUILayout.Label($"Movement: {_movement?.GetDebugSummary()} | Navigation: {_navigation?.GetDebugSummary()}");
            GUILayout.Label($"Last: {LastCommandSummary}");
            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            foreach (IDisposable registration in _commandRegistrations) registration.Dispose();
            _commandRegistrations.Clear();
        }
    }
}
