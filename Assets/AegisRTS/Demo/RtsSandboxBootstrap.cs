using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Units;
using AegisRTS.Presentation.Camera;
using AegisRTS.Presentation.Input;
using AegisRTS.Presentation.Selection;
using UnityEngine;

namespace AegisRTS.Demo
{
    /// <summary>Composes the Phase 03 services and placeholder acceptance sandbox.</summary>
    [DisallowMultipleComponent]
    public sealed class RtsSandboxBootstrap : MonoBehaviour
    {
        private readonly List<IDisposable> _commandRegistrations = new List<IDisposable>();
        private EntityIdGenerator _ids;
        private SelectionService _selection;
        private CommandBus _commands;

        public int DebugUnitCount { get; private set; }
        public string LastCommandSummary { get; private set; } = "No command issued";
        public SelectionService Selection => _selection;

        private void Awake()
        {
            _ids = new EntityIdGenerator();
            _selection = new SelectionService();
            _commands = new CommandBus();
            RegisterDebugCommandHandlers();
            CreateGround();
            SpawnAcceptanceActors();
            ComposeCameraAndInput();
        }

        private void RegisterDebugCommandHandlers()
        {
            _commandRegistrations.Add(_commands.RegisterHandler<MoveUnitsCommand>(command =>
                SetLastCommand($"Move {command.ActorIds.Count} actor(s) to {command.Destination}, Queue={command.Queue}")));
            _commandRegistrations.Add(_commands.RegisterHandler<AttackTargetCommand>(command =>
                SetLastCommand($"Attack {command.TargetId} with {command.ActorIds.Count} actor(s), Queue={command.Queue}")));
            _commandRegistrations.Add(_commands.RegisterHandler<FollowTargetCommand>(command =>
                SetLastCommand($"Follow {command.TargetId} with {command.ActorIds.Count} actor(s), Queue={command.Queue}")));
            _commandRegistrations.Add(_commands.RegisterHandler<InteractTargetCommand>(command =>
                SetLastCommand($"Interact with {command.TargetId} using {command.ActorIds.Count} actor(s), Queue={command.Queue}")));
            _commandRegistrations.Add(_commands.RegisterHandler<StopUnitsCommand>(command =>
                SetLastCommand($"Stop {command.ActorIds.Count} actor(s)")));
            _commandRegistrations.Add(_commands.RegisterHandler<HoldUnitsCommand>(command =>
                SetLastCommand($"Hold {command.ActorIds.Count} actor(s), Queue={command.Queue}")));
        }

        private void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_Debug";
            ground.transform.SetParent(transform);
            ground.transform.localScale = new Vector3(10f, 1f, 10f);
            SetColor(ground, new Color(0.16f, 0.23f, 0.18f));
        }

        private void SpawnAcceptanceActors()
        {
            for (int index = 0; index < 20; index++)
            {
                float x = -12f + (index % 5) * 3f;
                float z = -8f + (index / 5) * 3f;
                CreateSelectable(
                    $"Friendly_Unit_{index + 1:00}", PrimitiveType.Capsule, new Vector3(x, 1f, z),
                    "unit.debug.infantry", SelectableKind.Unit, SelectionAffiliation.Friendly,
                    new Color(0.1f, 0.45f, 0.95f));
                DebugUnitCount++;
            }

            CreateSelectable(
                "Friendly_Target", PrimitiveType.Cube, new Vector3(10f, 0.75f, -5f),
                "unit.debug.support", SelectableKind.Unit, SelectionAffiliation.Friendly,
                new Color(0.15f, 0.75f, 0.95f));
            CreateSelectable(
                "Enemy_Target", PrimitiveType.Cube, new Vector3(10f, 0.75f, 2f),
                "unit.debug.enemy", SelectableKind.Unit, SelectionAffiliation.Enemy,
                new Color(0.9f, 0.15f, 0.12f));
            GameObject settlement = CreateSelectable(
                "Settlement_Target", PrimitiveType.Cylinder, new Vector3(10f, 1f, 10f),
                "settlement.debug", SelectableKind.Settlement, SelectionAffiliation.Neutral,
                new Color(0.9f, 0.65f, 0.08f));
            settlement.transform.localScale = new Vector3(2.5f, 1f, 2.5f);
        }

        private GameObject CreateSelectable(
            string objectName, PrimitiveType primitive, Vector3 position, string definitionId,
            SelectableKind kind, SelectionAffiliation affiliation, Color color)
        {
            GameObject instance = GameObject.CreatePrimitive(primitive);
            instance.name = objectName;
            instance.transform.SetParent(transform);
            instance.transform.position = position;
            var view = instance.AddComponent<UnitySelectableView>();
            view.Configure(_ids.Next(), definitionId, kind, affiliation, _selection, color);
            return instance;
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
            cameraController.Initialize(new RtsCameraRigModel(pivotX: 0d, pivotZ: 0d, zoom: 28d));

            GameObject inputObject = new GameObject("RTS_InputAdapter");
            inputObject.transform.SetParent(transform);
            inputObject.AddComponent<UnityRtsInputAdapter>().Initialize(_selection, _commands, cameraController);
        }

        private void SetLastCommand(string summary)
        {
            LastCommandSummary = summary;
            Debug.Log($"[Phase03] {summary}", this);
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
            GUILayout.BeginArea(new Rect(16f, 16f, 560f, 150f), GUI.skin.box);
            GUILayout.Label("Phase 03 RTS Input / Selection / Camera");
            GUILayout.Label("LMB click/drag | Shift add/remove | Double-click same type | Ctrl+0-9 assign | 0-9 recall");
            GUILayout.Label("RMB context command | Shift queue | X stop | H hold | WASD/edge/MMB pan | Wheel zoom | F focus");
            GUILayout.Label($"Selected: {_selection?.SelectedIds.Count ?? 0} | Last: {LastCommandSummary}");
            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            foreach (IDisposable registration in _commandRegistrations) registration.Dispose();
            _commandRegistrations.Clear();
        }
    }
}
