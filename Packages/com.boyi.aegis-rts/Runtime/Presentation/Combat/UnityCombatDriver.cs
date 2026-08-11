using System;
using System.Collections.Generic;
using AegisRTS.Core.Events;
using AegisRTS.Core.Performance;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Units;
using UnityEngine;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Presentation.Combat
{
    /// <summary>Synchronizes Unity transforms and temporary projectile visuals with the pure C# combat simulation.</summary>
    [DisallowMultipleComponent]
    public sealed class UnityCombatDriver : MonoBehaviour
    {
        private readonly Dictionary<EntityId, UnityCombatView> _views = new Dictionary<EntityId, UnityCombatView>();
        private readonly List<ProjectileVisual> _projectiles = new List<ProjectileVisual>();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private CombatSystem _combat;
        private ObjectPool<GameObject> _projectilePool;

        public int ProjectileVisualCount { get; private set; }
        public int DamageEventCount { get; private set; }
        public int StatusEventCount { get; private set; }
        public int DeathEventCount { get; private set; }
        public int ProjectileObjectCreatedCount => _projectilePool?.CreatedCount ?? 0;
        public int PooledProjectileCount => _projectilePool?.AvailableCount ?? 0;

        public void Initialize(CombatSystem combat, EventBus events)
        {
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            if (events == null) throw new ArgumentNullException(nameof(events));
            _projectilePool = new ObjectPool<GameObject>(CreateProjectileObject, maximumRetained: 64,
                onRent: value => value.SetActive(true), onReturn: value => value.SetActive(false));
            _subscriptions.Add(events.Subscribe<ProjectileLaunchedEvent>(CreateProjectile));
            _subscriptions.Add(events.Subscribe<DamageAppliedEvent>(_ => DamageEventCount++));
            _subscriptions.Add(events.Subscribe<StatusAppliedEvent>(_ => StatusEventCount++));
            _subscriptions.Add(events.Subscribe<UnitDiedEvent>(_ => DeathEventCount++));
        }

        public void RegisterView(UnityCombatView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            _views[view.EntityId] = view;
        }

        private void Update()
        {
            if (_combat == null) return;
            foreach (KeyValuePair<EntityId, UnityCombatView> entry in _views)
            {
                Vector3 position = entry.Value.transform.position;
                _combat.UpdatePosition(entry.Key, new WorldPoint(position.x, position.y, position.z));
            }
            _combat.Tick(Time.deltaTime);
            foreach (KeyValuePair<EntityId, UnityCombatView> entry in _views)
                if (_combat.TryGetState(entry.Key, out CombatantSnapshot snapshot)) entry.Value.Sync(snapshot);
            TickProjectileVisuals(Time.deltaTime);
        }

        private void CreateProjectile(ProjectileLaunchedEvent value)
        {
            GameObject visual = _projectilePool.Rent();
            visual.name = $"Projectile_{value.SourceId}_{value.TargetId}";
            visual.transform.position = ToVector(value.Origin) + Vector3.up;
            visual.transform.localScale = Vector3.one * 0.3f;
            float duration = Mathf.Max(0.05f, Vector3.Distance(ToVector(value.Origin), ToVector(value.Destination)) / (float)value.Speed);
            _projectiles.Add(new ProjectileVisual(visual, visual.transform.position, ToVector(value.Destination) + Vector3.up, duration));
            ProjectileVisualCount++;
        }

        private void TickProjectileVisuals(float deltaTime)
        {
            for (int index = _projectiles.Count - 1; index >= 0; index--)
            {
                ProjectileVisual projectile = _projectiles[index];
                projectile.Elapsed += deltaTime;
                float ratio = Mathf.Clamp01(projectile.Elapsed / projectile.Duration);
                projectile.Visual.transform.position = Vector3.Lerp(projectile.Origin, projectile.Destination, ratio);
                if (ratio < 1f) continue;
                _projectilePool.Return(projectile.Visual);
                _projectiles.RemoveAt(index);
            }
        }

        private GameObject CreateProjectileObject()
        {
            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere); visual.name = "Projectile_Pooled";
            Destroy(visual.GetComponent<Collider>()); visual.transform.SetParent(transform, false);
            visual.transform.localScale = Vector3.one * 0.3f; SetColor(visual.GetComponent<Renderer>(), new Color(1f, 0.65f, 0.05f));
            return visual;
        }

        private static Vector3 ToVector(WorldPoint point) => new Vector3((float)point.X, (float)point.Y, (float)point.Z);

        private static void SetColor(Renderer renderer, Color color)
        {
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor(Shader.PropertyToID("_BaseColor"), color);
            renderer.SetPropertyBlock(block);
        }

        private void OnDestroy()
        {
            foreach (IDisposable subscription in _subscriptions) subscription.Dispose();
            _subscriptions.Clear();
        }

        private sealed class ProjectileVisual
        {
            public ProjectileVisual(GameObject visual, Vector3 origin, Vector3 destination, float duration)
            { Visual = visual; Origin = origin; Destination = destination; Duration = duration; }
            public GameObject Visual { get; }
            public Vector3 Origin { get; }
            public Vector3 Destination { get; }
            public float Duration { get; }
            public float Elapsed { get; set; }
        }
    }
}
