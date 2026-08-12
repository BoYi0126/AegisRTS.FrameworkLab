using AegisRTS.Core.Entities;
using UnityEngine;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Presentation.Selection
{
    /// <summary>Bridges a selectable Unity object to the pure selection model.</summary>
    [DisallowMultipleComponent]
    public sealed class UnitySelectableView : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        [SerializeField] private ulong entityId;
        [SerializeField] private string definitionId = "unit.debug";
        [SerializeField] private SelectableKind kind = SelectableKind.Unit;
        [SerializeField] private SelectionAffiliation affiliation = SelectionAffiliation.Friendly;
        [SerializeField] private Renderer targetRenderer;

        private SelectionService _selection;
        private Color _baseColor = Color.white;
        private MaterialPropertyBlock _propertyBlock;

        public EntityId EntityId => new EntityId(entityId);
        public string DefinitionId => definitionId;
        public SelectableKind Kind => kind;
        public SelectionAffiliation Affiliation => affiliation;
        public SelectableDescriptor Descriptor => new SelectableDescriptor(EntityId, definitionId, kind, affiliation);

        public void Configure(
            EntityId id,
            string contentDefinitionId,
            SelectableKind selectableKind,
            SelectionAffiliation selectableAffiliation,
            SelectionService selection,
            Color baseColor)
        {
            entityId = id.Value;
            definitionId = contentDefinitionId;
            kind = selectableKind;
            affiliation = selectableAffiliation;
            _selection = selection;
            _baseColor = baseColor;
            targetRenderer = targetRenderer != null ? targetRenderer : GetComponentInChildren<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            _selection.Register(Descriptor);
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (targetRenderer == null) return;
            if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColor, selected ? Color.Lerp(_baseColor, Color.white, 0.65f) : _baseColor);
            targetRenderer.SetPropertyBlock(_propertyBlock);
        }

        public void SetAffiliation(SelectionAffiliation selectableAffiliation, Color baseColor)
        {
            if (_selection != null && EntityId.IsValid) _selection.Unregister(EntityId);
            affiliation = selectableAffiliation;
            _baseColor = baseColor;
            _selection?.Register(Descriptor);
            SetSelected(false);
        }

        private void OnDestroy()
        {
            if (_selection != null && EntityId.IsValid) _selection.Unregister(EntityId);
        }
    }
}
