using System;
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
        [SerializeField] private Renderer[] targetRenderers = Array.Empty<Renderer>();

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
            Color baseColor,
            Renderer[] selectionRenderers = null)
        {
            entityId = id.Value;
            definitionId = contentDefinitionId;
            kind = selectableKind;
            affiliation = selectableAffiliation;
            _selection = selection;
            _baseColor = baseColor;
            if (selectionRenderers != null && selectionRenderers.Length > 0)
                targetRenderers = selectionRenderers;
            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                targetRenderer = targetRenderer != null ? targetRenderer : GetComponentInChildren<Renderer>();
                targetRenderers = targetRenderer != null ? new[] { targetRenderer } : Array.Empty<Renderer>();
            }
            else
            {
                targetRenderer = targetRenderers[0];
            }
            _propertyBlock = new MaterialPropertyBlock();
            _selection.Register(Descriptor);
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();
            foreach (Renderer renderer in targetRenderers ?? Array.Empty<Renderer>())
            {
                if (renderer == null) continue;
                Color color = selected ? Color.Lerp(_baseColor, Color.white, 0.65f) : _baseColor;
                Material[] materials = renderer.sharedMaterials;
                bool appliedToTeamSlot = false;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Material material = materials[materialIndex];
                    if (material == null || material.name.IndexOf("TeamColor", StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    _propertyBlock.Clear();
                    renderer.GetPropertyBlock(_propertyBlock, materialIndex);
                    _propertyBlock.SetColor(BaseColor, color);
                    renderer.SetPropertyBlock(_propertyBlock, materialIndex);
                    appliedToTeamSlot = true;
                }

                // Generic selectable placeholders may not use the art material naming convention.
                if (appliedToTeamSlot) continue;
                _propertyBlock.Clear();
                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(BaseColor, color);
                renderer.SetPropertyBlock(_propertyBlock);
            }
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
