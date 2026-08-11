using System;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Units;
using AegisRTS.Presentation.Camera;
using AegisRTS.Presentation.Selection;
using UnityEngine;
using UnityEngine.InputSystem;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Presentation.Input
{
    /// <summary>Translates Input System actions and raycasts into selection and gameplay commands.</summary>
    [DisallowMultipleComponent]
    public sealed class UnityRtsInputAdapter : MonoBehaviour
    {
        private const float ClickThreshold = 8f;
        private const float DoubleClickSeconds = 0.3f;
        private SelectionService _selection;
        private CommandBus _commands;
        private RtsCameraController _cameraController;
        private InputActionMap _map;
        private InputAction _point;
        private InputAction _select;
        private InputAction _addSelection;
        private InputAction _command;
        private InputAction _cameraMove;
        private InputAction _cameraZoom;
        private InputAction _queueCommand;
        private InputAction _stop;
        private InputAction _hold;
        private InputAction _focus;
        private Vector2 _selectionStart;
        private bool _dragging;
        private EntityId _lastClicked;
        private float _lastClickTime = float.NegativeInfinity;

        public SelectionService Selection => _selection;

        public void Initialize(SelectionService selection, CommandBus commands, RtsCameraController cameraController)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _cameraController = cameraController ?? throw new ArgumentNullException(nameof(cameraController));
            BuildActions();
            _map.Enable();
        }

        private void Update()
        {
            if (_map == null) return;
            Vector2 pointer = _point.ReadValue<Vector2>();
            Vector2 pointerDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
            bool middleDragging = Mouse.current != null && Mouse.current.middleButton.isPressed;
            _cameraController.ProcessInput(
                _cameraMove.ReadValue<Vector2>(), pointer, pointerDelta, middleDragging,
                _cameraZoom.ReadValue<Vector2>().y, Time.unscaledDeltaTime);

            if (_select.WasPressedThisFrame()) { _selectionStart = pointer; _dragging = true; }
            if (_select.WasReleasedThisFrame() && _dragging) { CompleteSelection(pointer); _dragging = false; }
            if (_command.WasPressedThisFrame()) IssueContextCommand(pointer);
            if (_stop.WasPressedThisFrame()) DispatchStop();
            if (_hold.WasPressedThisFrame()) DispatchHold();
            if (_focus.WasPressedThisFrame()) _cameraController.FocusSelection(_selection, FindViews());
            ProcessControlGroups();
        }

        private void CompleteSelection(Vector2 pointer)
        {
            bool additive = _addSelection.IsPressed();
            SelectionModifier modifier = additive ? SelectionModifier.Toggle : SelectionModifier.Replace;
            if (Vector2.Distance(_selectionStart, pointer) <= ClickThreshold)
            {
                UnitySelectableView view = RaycastSelectable(pointer, out _);
                if (view == null)
                {
                    if (!additive) _selection.Clear();
                }
                else if (view.EntityId == _lastClicked && Time.unscaledTime - _lastClickTime <= DoubleClickSeconds)
                {
                    UnitySelectableView[] views = FindViews();
                    var ids = new EntityId[views.Length];
                    for (int i = 0; i < views.Length; i++) ids[i] = views[i].EntityId;
                    _selection.SelectSameDefinition(view.EntityId, ids, modifier);
                }
                else
                {
                    _selection.Select(view.EntityId, modifier);
                }
                _lastClicked = view != null ? view.EntityId : EntityId.Invalid;
                _lastClickTime = Time.unscaledTime;
            }
            else
            {
                Rect rect = ScreenRect(_selectionStart, pointer);
                UnitySelectableView[] views = FindViews();
                var ids = new System.Collections.Generic.List<EntityId>();
                UnityEngine.Camera camera = UnityEngine.Camera.main;
                foreach (UnitySelectableView view in views)
                {
                    Vector3 screen = camera.WorldToScreenPoint(view.transform.position);
                    if (screen.z > 0f && rect.Contains(new Vector2(screen.x, screen.y))) ids.Add(view.EntityId);
                }
                _selection.SelectMany(ids, modifier);
            }
            RefreshSelectionVisuals();
        }

        private void IssueContextCommand(Vector2 pointer)
        {
            if (_selection.SelectedIds.Count == 0) return;
            UnitySelectableView view = RaycastSelectable(pointer, out RaycastHit hit);
            ContextTarget target = view != null
                ? ContextTarget.Entity(ToWorldPoint(hit.point), view.Descriptor)
                : TryRaycast(pointer, out hit)
                    ? ContextTarget.Ground(ToWorldPoint(hit.point))
                    : ContextTarget.Ground(ToWorldPoint(ScreenToGround(pointer)));
            ICommand command = ContextCommandResolver.Resolve(_selection.SelectedIds, target, _queueCommand.IsPressed());
            Dispatch(command);
        }

        private void DispatchStop()
        {
            if (_selection.SelectedIds.Count > 0) _commands.Dispatch(new StopUnitsCommand(_selection.SelectedIds));
        }

        private void DispatchHold()
        {
            if (_selection.SelectedIds.Count > 0) _commands.Dispatch(new HoldUnitsCommand(_selection.SelectedIds, _queueCommand.IsPressed()));
        }

        private void Dispatch(ICommand command)
        {
            switch (command)
            {
                case MoveUnitsCommand value: _commands.Dispatch(value); break;
                case AttackTargetCommand value: _commands.Dispatch(value); break;
                case FollowTargetCommand value: _commands.Dispatch(value); break;
                case InteractTargetCommand value: _commands.Dispatch(value); break;
            }
        }

        private void ProcessControlGroups()
        {
            if (Keyboard.current == null) return;
            for (int index = 0; index <= 9; index++)
            {
                Key key = index == 0 ? Key.Digit0 : (Key)((int)Key.Digit1 + index - 1);
                if (!Keyboard.current[key].wasPressedThisFrame) continue;
                bool assign = Keyboard.current.ctrlKey.isPressed;
                if (assign) _selection.AssignControlGroup(index);
                else _selection.RecallControlGroup(index, _addSelection.IsPressed() ? SelectionModifier.Add : SelectionModifier.Replace);
                RefreshSelectionVisuals();
            }
        }

        private UnitySelectableView RaycastSelectable(Vector2 pointer, out RaycastHit hit)
        {
            if (TryRaycast(pointer, out hit)) return hit.collider.GetComponentInParent<UnitySelectableView>();
            return null;
        }

        private static bool TryRaycast(Vector2 pointer, out RaycastHit hit)
        {
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            if (camera != null) return Physics.Raycast(camera.ScreenPointToRay(pointer), out hit, 500f);
            hit = default;
            return false;
        }

        private static Vector3 ScreenToGround(Vector2 pointer)
        {
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            if (camera == null) return Vector3.zero;
            Ray ray = camera.ScreenPointToRay(pointer);
            var plane = new Plane(Vector3.up, Vector3.zero);
            return plane.Raycast(ray, out float distance) ? ray.GetPoint(distance) : Vector3.zero;
        }

        private static WorldPoint ToWorldPoint(Vector3 point) => new WorldPoint(point.x, point.y, point.z);
        private static UnitySelectableView[] FindViews() => FindObjectsByType<UnitySelectableView>();

        private void RefreshSelectionVisuals()
        {
            foreach (UnitySelectableView view in FindViews()) view.SetSelected(_selection.IsSelected(view.EntityId));
        }

        private void BuildActions()
        {
            _map = new InputActionMap("RTS");
            _point = _map.AddAction("Point", InputActionType.PassThrough, "<Pointer>/position");
            _select = _map.AddAction("Select", InputActionType.Button, "<Mouse>/leftButton");
            _addSelection = _map.AddAction("AddSelection", InputActionType.Button, "<Keyboard>/shift");
            _command = _map.AddAction("Command", InputActionType.Button, "<Mouse>/rightButton");
            _cameraMove = _map.AddAction("CameraMove", InputActionType.Value);
            _cameraMove.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            _cameraZoom = _map.AddAction("CameraZoom", InputActionType.PassThrough, "<Mouse>/scroll");
            InputAction controlGroup = _map.AddAction("ControlGroup", InputActionType.Button);
            for (int index = 0; index <= 9; index++) controlGroup.AddBinding($"<Keyboard>/digit{index}");
            _queueCommand = _map.AddAction("QueueCommand", InputActionType.Button, "<Keyboard>/shift");
            _stop = _map.AddAction("Stop", InputActionType.Button, "<Keyboard>/x");
            _hold = _map.AddAction("Hold", InputActionType.Button, "<Keyboard>/h");
            _focus = _map.AddAction("FocusSelected", InputActionType.Button, "<Keyboard>/f");
        }

        private static Rect ScreenRect(Vector2 start, Vector2 end) => Rect.MinMaxRect(
            Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y), Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y));

        private void OnGUI()
        {
            if (!_dragging || Vector2.Distance(_selectionStart, _point.ReadValue<Vector2>()) <= ClickThreshold) return;
            Vector2 current = _point.ReadValue<Vector2>();
            Rect screen = ScreenRect(_selectionStart, current);
            Rect gui = new Rect(screen.xMin, Screen.height - screen.yMax, screen.width, screen.height);
            Color previous = GUI.color;
            GUI.color = new Color(0.2f, 0.8f, 1f, 0.25f);
            GUI.DrawTexture(gui, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void OnDestroy()
        {
            if (_map != null) { _map.Disable(); _map.Dispose(); }
        }
    }
}
