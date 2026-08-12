using System.Collections.Generic;
using AegisRTS.Core.Entities;
using AegisRTS.Presentation.Selection;
using UnityEngine;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Presentation.Camera
{
    /// <summary>Applies RTS pan, edge scroll, middle drag, zoom, bounds, and focus to a Unity camera.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class RtsCameraController : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float panSpeed = 18f;
        [SerializeField, Min(1f)] private float dragSpeed = 0.04f;
        [SerializeField, Min(0f)] private float edgeSize = 14f;
        [SerializeField, Min(0.1f)] private float zoomSpeed = 0.025f;
        [SerializeField, Range(20f, 80f)] private float pitch = 55f;
        [SerializeField] private float yaw = 0f;
        [SerializeField, Min(0f)] private float closeInspectionFocusHeight = 0.9f;
        [SerializeField, Range(20f, 80f)] private float closeInspectionPitch = 38f;

        private RtsCameraRigModel _model;
        private UnityEngine.Camera _camera;
        private int _ignoreInputFrames;

        public RtsCameraRigModel Model => _model;

        public void Initialize(RtsCameraRigModel model)
        {
            _model = model;
            _camera = GetComponent<UnityEngine.Camera>();
            _ignoreInputFrames = 2;
            ApplyTransform();
        }

        public void ProcessInput(Vector2 movement, Vector2 pointer, Vector2 pointerDelta, bool middleDragging, float scroll, float deltaTime)
        {
            if (_model == null) return;
            if (_ignoreInputFrames > 0)
            {
                _ignoreInputFrames--;
                ApplyTransform();
                return;
            }
            Vector2 edge = Vector2.zero;
            if (pointer.x >= 0f && pointer.y >= 0f && pointer.x <= Screen.width && pointer.y <= Screen.height)
            {
                if (pointer.x <= edgeSize) edge.x -= 1f;
                if (pointer.x >= Screen.width - edgeSize) edge.x += 1f;
                if (pointer.y <= edgeSize) edge.y -= 1f;
                if (pointer.y >= Screen.height - edgeSize) edge.y += 1f;
            }

            Vector2 input = Vector2.ClampMagnitude(movement + edge, 1f);
            Quaternion heading = Quaternion.Euler(0f, yaw, 0f);
            Vector3 world = heading * new Vector3(input.x, 0f, input.y);
            float speedScale = Mathf.Lerp(0.65f, 1.5f, (float)((_model.Zoom - _model.MinimumZoom) / (_model.MaximumZoom - _model.MinimumZoom)));
            _model.Pan(world.x * panSpeed * speedScale * deltaTime, world.z * panSpeed * speedScale * deltaTime);

            if (middleDragging)
            {
                Vector3 dragWorld = heading * new Vector3(-pointerDelta.x, 0f, -pointerDelta.y);
                _model.Pan(dragWorld.x * dragSpeed * speedScale, dragWorld.z * dragSpeed * speedScale);
            }

            if (!Mathf.Approximately(scroll, 0f)) _model.ZoomBy(-scroll * zoomSpeed);
            ApplyTransform();
        }

        public bool FocusSelection(ISelectionQuery selection, IEnumerable<UnitySelectableView> views)
        {
            if (_model == null || selection == null || views == null || selection.SelectedIds.Count == 0) return false;
            var selected = new HashSet<EntityId>(selection.SelectedIds);
            Vector3 sum = Vector3.zero;
            int count = 0;
            foreach (UnitySelectableView view in views)
            {
                if (view != null && selected.Contains(view.EntityId)) { sum += view.transform.position; count++; }
            }
            if (count == 0) return false;
            Vector3 center = sum / count;
            _model.Focus(center.x, center.z);
            ApplyTransform();
            return true;
        }

        private void ApplyTransform()
        {
            float inspectionBlend = Mathf.InverseLerp(8f, 2.5f, (float)_model.Zoom);
            Vector3 pivot = new Vector3((float)_model.PivotX,
                Mathf.Lerp(0f, closeInspectionFocusHeight, inspectionBlend), (float)_model.PivotZ);
            float appliedPitch = Mathf.Lerp(pitch, closeInspectionPitch, inspectionBlend);
            Quaternion rotation = Quaternion.Euler(appliedPitch, yaw, 0f);
            transform.SetPositionAndRotation(pivot + rotation * Vector3.back * (float)_model.Zoom, rotation);
        }
    }
}
