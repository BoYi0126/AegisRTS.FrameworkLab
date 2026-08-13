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
        [SerializeField] private PrototypeUnitAnimatorView animatorView;
        [SerializeField] private Transform projectileSocket;

        public Transform SelectionAnchor => selectionAnchor;
        public Transform HealthBarAnchor => healthBarAnchor;
        public Renderer[] TeamColorRenderers => teamColorRenderers ?? Array.Empty<Renderer>();
        public Renderer PrimaryTeamColorRenderer => TeamColorRenderers.Length > 0 ? TeamColorRenderers[0] : null;
        public PrototypeUnitAnimatorView AnimatorView => animatorView;
        public Transform ProjectileSocket => projectileSocket;

        public void Configure(Transform selection, Transform healthBar, Renderer[] renderers,
            PrototypeUnitAnimatorView animation = null, Transform projectile = null)
        {
            selectionAnchor = selection;
            healthBarAnchor = healthBar;
            teamColorRenderers = renderers ?? Array.Empty<Renderer>();
            animatorView = animation;
            projectileSocket = projectile;
        }

        public void ApplyTeamColor(Color color)
        {
            foreach (Renderer target in TeamColorRenderers)
            {
                if (target == null) continue;
                Material[] materials = target.sharedMaterials;
                bool appliedToTeamSlot = false;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Material material = materials[materialIndex];
                    if (material == null || material.name.IndexOf("TeamColor", StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    var block = new MaterialPropertyBlock();
                    target.GetPropertyBlock(block, materialIndex);
                    block.SetColor(BaseColor, color);
                    block.SetColor(ColorProperty, color);
                    target.SetPropertyBlock(block, materialIndex);
                    appliedToTeamSlot = true;
                }

                // Legacy art may expose a dedicated team renderer whose material has not yet adopted the naming contract.
                if (appliedToTeamSlot) continue;
                var fallback = new MaterialPropertyBlock();
                target.GetPropertyBlock(fallback);
                fallback.SetColor(BaseColor, color);
                fallback.SetColor(ColorProperty, color);
                target.SetPropertyBlock(fallback);
            }
        }

        public float GetHealthBarLocalY(Transform gameplayRoot, float fallback)
        {
            if (gameplayRoot == null || healthBarAnchor == null) return fallback;
            return gameplayRoot.InverseTransformPoint(healthBarAnchor.position).y;
        }
    }
}
