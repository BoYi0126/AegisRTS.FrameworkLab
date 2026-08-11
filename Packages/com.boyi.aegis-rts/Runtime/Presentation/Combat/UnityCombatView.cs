using AegisRTS.Gameplay.Combat;
using UnityEngine;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Presentation.Combat
{
    /// <summary>Unity-only rendering of an authoritative combat snapshot.</summary>
    [DisallowMultipleComponent]
    public sealed class UnityCombatView : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private Renderer _renderer;
        private Transform _healthFill;
        private Color _aliveColor;
        private Vector3 _aliveScale;

        public EntityId EntityId { get; private set; }
        public CombatantSnapshot LastSnapshot { get; private set; }

        public void Configure(EntityId entityId, Color aliveColor)
        {
            EntityId = entityId;
            _aliveColor = aliveColor;
            _aliveScale = transform.localScale;
            _renderer = GetComponentInChildren<Renderer>();
            CreateHealthBar();
            SetColor(aliveColor);
        }

        public void Sync(CombatantSnapshot snapshot)
        {
            LastSnapshot = snapshot;
            float healthRatio = snapshot.MaxHealth <= 0d ? 0f : Mathf.Clamp01((float)(snapshot.Health / snapshot.MaxHealth));
            if (_healthFill != null) _healthFill.localScale = new Vector3(healthRatio, 1f, 1f);
            transform.localScale = snapshot.IsAlive ? _aliveScale : new Vector3(_aliveScale.x, _aliveScale.y * 0.2f, _aliveScale.z);
            SetColor(snapshot.IsAlive ? Color.Lerp(new Color(0.3f, 0.05f, 0.05f), _aliveColor, healthRatio) : Color.gray);
        }

        private void CreateHealthBar()
        {
            var background = GameObject.CreatePrimitive(PrimitiveType.Cube);
            background.name = "HealthBar_Background";
            Destroy(background.GetComponent<Collider>());
            background.transform.SetParent(transform, false);
            background.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            background.transform.localScale = new Vector3(1.25f, 0.1f, 0.12f);
            SetRendererColor(background.GetComponent<Renderer>(), Color.black);

            var fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fill.name = "HealthBar_Fill";
            Destroy(fill.GetComponent<Collider>());
            fill.transform.SetParent(background.transform, false);
            fill.transform.localPosition = new Vector3(0f, 0f, -0.15f);
            fill.transform.localScale = Vector3.one;
            SetRendererColor(fill.GetComponent<Renderer>(), new Color(0.15f, 0.9f, 0.2f));
            _healthFill = fill.transform;
        }

        private void SetColor(Color color)
        {
            if (_renderer != null) SetRendererColor(_renderer, color);
        }

        private static void SetRendererColor(Renderer renderer, Color color)
        {
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor(BaseColor, color);
            renderer.SetPropertyBlock(block);
        }
    }
}
