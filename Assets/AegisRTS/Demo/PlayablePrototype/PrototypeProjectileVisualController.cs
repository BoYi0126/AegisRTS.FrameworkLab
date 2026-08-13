using System;
using System.Collections.Generic;
using AegisRTS.Core.Events;
using AegisRTS.Core.Performance;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Units;
using UnityEngine;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Demo.PlayablePrototype
{
    /// <summary>
    /// Presentation-only observer for authoritative projectile events. It never applies damage or advances combat.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeProjectileVisualController : MonoBehaviour
    {
        public const string ArrowResourcePath = "AegisRTS/Projectiles/PRJ_Arrow_Basic_v001";
        private readonly List<Flight> _flights = new List<Flight>();
        private readonly List<Impact> _impacts = new List<Impact>();
        private IDisposable _subscription;
        private ObjectPool<GameObject> _arrowPool;
        private ObjectPool<GameObject> _impactPool;
        private Func<EntityId, Transform> _resolveRoot;
        private Func<EntityId, Transform> _resolveSocket;
        private GameObject _arrowPrefab;
        private Material _impactMaterial;

        public int ProjectileVisualCount { get; private set; }
        public int ImpactVisualCount { get; private set; }
        public int ActiveProjectileCount => _arrowPool?.ActiveCount ?? 0;
        public int ProjectileObjectCreatedCount => _arrowPool?.CreatedCount ?? 0;
        public int PooledProjectileCount => _arrowPool?.AvailableCount ?? 0;

        public void Initialize(EventBus events, Func<EntityId, Transform> resolveRoot,
            Func<EntityId, Transform> resolveSocket)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            Shutdown();
            _resolveRoot = resolveRoot ?? throw new ArgumentNullException(nameof(resolveRoot));
            _resolveSocket = resolveSocket ?? throw new ArgumentNullException(nameof(resolveSocket));
            _arrowPrefab = Resources.Load<GameObject>(ArrowResourcePath);
            if (_arrowPrefab == null)
                throw new InvalidOperationException($"Projectile prefab was not found at Resources/{ArrowResourcePath}.");
            _arrowPool = new ObjectPool<GameObject>(CreateArrow, maximumRetained: 64,
                onRent: value => value.SetActive(true), onReturn: value => value.SetActive(false));
            _impactPool = new ObjectPool<GameObject>(CreateImpact, maximumRetained: 32,
                onRent: value => value.SetActive(true), onReturn: value => value.SetActive(false));
            _subscription = events.Subscribe<ProjectileLaunchedEvent>(Launch);
        }

        public void Shutdown()
        {
            _subscription?.Dispose();
            _subscription = null;
            _flights.Clear();
            _impacts.Clear();
            _arrowPool = null;
            _impactPool = null;
            _resolveRoot = null;
            _resolveSocket = null;
        }

        private void Launch(ProjectileLaunchedEvent value)
        {
            if (_arrowPool == null) return;
            Transform sourceSocket = _resolveSocket(value.SourceId);
            Transform sourceRoot = _resolveRoot(value.SourceId);
            Transform targetRoot = _resolveRoot(value.TargetId);
            Vector3 origin = sourceSocket != null ? sourceSocket.position :
                sourceRoot != null ? sourceRoot.position + Vector3.up * 1.15f : ToVector(value.Origin) + Vector3.up * 1.15f;
            Vector3 destination = targetRoot != null ? targetRoot.position + Vector3.up : ToVector(value.Destination) + Vector3.up;
            float distance = Vector3.Distance(origin, destination);
            float duration = Mathf.Clamp(distance / Mathf.Max(0.1f, (float)value.Speed), 0.08f, 2f);
            GameObject arrow = _arrowPool.Rent();
            arrow.name = $"Arrow_{value.SourceId}_{value.TargetId}";
            arrow.transform.position = origin;
            Orient(arrow.transform, destination - origin);
            _flights.Add(new Flight(arrow, origin, destination, duration));
            ProjectileVisualCount++;
        }

        private void Update()
        {
            TickFlights(Time.deltaTime);
            TickImpacts(Time.deltaTime);
        }

        private void TickFlights(float deltaTime)
        {
            if (_arrowPool == null) return;
            for (int index = _flights.Count - 1; index >= 0; index--)
            {
                Flight flight = _flights[index];
                flight.Elapsed += deltaTime;
                float ratio = Mathf.Clamp01(flight.Elapsed / flight.Duration);
                Vector3 position = Evaluate(flight, ratio);
                float nextRatio = Mathf.Min(1f, ratio + 0.02f);
                flight.Visual.transform.position = position;
                Orient(flight.Visual.transform, Evaluate(flight, nextRatio) - position);
                if (ratio < 1f) continue;
                Vector3 impactPosition = flight.Destination;
                _arrowPool.Return(flight.Visual);
                _flights.RemoveAt(index);
                SpawnImpact(impactPosition);
            }
        }

        private void TickImpacts(float deltaTime)
        {
            if (_impactPool == null) return;
            for (int index = _impacts.Count - 1; index >= 0; index--)
            {
                Impact impact = _impacts[index];
                impact.Remaining -= deltaTime;
                float ratio = Mathf.Clamp01(impact.Remaining / 0.22f);
                impact.Visual.transform.localScale = Vector3.one * (0.12f + 0.24f * ratio);
                if (impact.Remaining > 0f) continue;
                _impactPool.Return(impact.Visual);
                _impacts.RemoveAt(index);
            }
        }

        private void SpawnImpact(Vector3 position)
        {
            GameObject visual = _impactPool.Rent();
            visual.transform.position = position;
            visual.transform.localScale = Vector3.one * 0.34f;
            _impacts.Add(new Impact(visual, 0.22f));
            ImpactVisualCount++;
        }

        private GameObject CreateArrow()
        {
            GameObject value = Instantiate(_arrowPrefab, transform, false);
            value.name = "Arrow_Pooled";
            value.SetActive(false);
            return value;
        }

        private GameObject CreateImpact()
        {
            GameObject value = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            value.name = "ArrowImpact_Pooled";
            value.transform.SetParent(transform, false);
            Collider collider = value.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            Renderer renderer = value.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            _impactMaterial ??= new Material(shader) { name = "ArrowImpact_Runtime" };
            if (_impactMaterial.HasProperty("_BaseColor")) _impactMaterial.SetColor("_BaseColor", new Color(1f, 0.72f, 0.12f, 1f));
            if (_impactMaterial.HasProperty("_Color")) _impactMaterial.SetColor("_Color", new Color(1f, 0.72f, 0.12f, 1f));
            renderer.sharedMaterial = _impactMaterial;
            value.SetActive(false);
            return value;
        }

        private static Vector3 Evaluate(Flight flight, float ratio)
        {
            Vector3 linear = Vector3.Lerp(flight.Origin, flight.Destination, ratio);
            float arc = Mathf.Sin(ratio * Mathf.PI) * Mathf.Clamp(Vector3.Distance(flight.Origin, flight.Destination) * 0.04f, 0.12f, 0.45f);
            return linear + Vector3.up * arc;
        }

        private static void Orient(Transform target, Vector3 direction)
        {
            if (direction.sqrMagnitude > 0.000001f) target.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private static Vector3 ToVector(WorldPoint value) => new Vector3((float)value.X, (float)value.Y, (float)value.Z);

        private void OnDestroy()
        {
            Shutdown();
            if (_impactMaterial != null) Destroy(_impactMaterial);
        }

        private sealed class Flight
        {
            public Flight(GameObject visual, Vector3 origin, Vector3 destination, float duration)
            { Visual = visual; Origin = origin; Destination = destination; Duration = duration; }
            public GameObject Visual { get; }
            public Vector3 Origin { get; }
            public Vector3 Destination { get; }
            public float Duration { get; }
            public float Elapsed { get; set; }
        }

        private sealed class Impact
        {
            public Impact(GameObject visual, float remaining) { Visual = visual; Remaining = remaining; }
            public GameObject Visual { get; }
            public float Remaining { get; set; }
        }
    }
}
