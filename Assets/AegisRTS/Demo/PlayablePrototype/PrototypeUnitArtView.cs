using System;
using UnityEngine;

namespace AegisRTS.Demo.PlayablePrototype
{
    /// <summary>Presentation-only anchors and team-color renderers carried by a product unit-art prefab.</summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeUnitArtView : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        [SerializeField] private Transform selectionAnchor;
        [SerializeField] private Transform healthBarAnchor;
        [SerializeField] private Renderer[] teamColorRenderers = Array.Empty<Renderer>();

        public Transform SelectionAnchor => selectionAnchor;
        public Transform HealthBarAnchor => healthBarAnchor;
        public Renderer[] TeamColorRenderers => teamColorRenderers ?? Array.Empty<Renderer>();
        public Renderer PrimaryTeamColorRenderer => TeamColorRenderers.Length > 0 ? TeamColorRenderers[0] : null;

        public void Configure(Transform selection, Transform healthBar, Renderer[] renderers)
        {
            selectionAnchor = selection;
            healthBarAnchor = healthBar;
            teamColorRenderers = renderers ?? Array.Empty<Renderer>();
        }

        public void ApplyTeamColor(Color color)
        {
            foreach (Renderer target in TeamColorRenderers)
            {
                if (target == null) continue;
                var block = new MaterialPropertyBlock();
                target.GetPropertyBlock(block);
                block.SetColor(BaseColor, color);
                block.SetColor(ColorProperty, color);
                target.SetPropertyBlock(block);
            }
        }

        public float GetHealthBarLocalY(Transform gameplayRoot, float fallback)
        {
            if (gameplayRoot == null || healthBarAnchor == null) return fallback;
            return gameplayRoot.InverseTransformPoint(healthBarAnchor.position).y;
        }
    }
}
