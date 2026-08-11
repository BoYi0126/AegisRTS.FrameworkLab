using System;
using AegisRTS.Gameplay.Movement;
using UnityEngine;

namespace AegisRTS.Presentation.Movement
{
    /// <summary>Advances the pure movement system from the Unity frame loop.</summary>
    [DisallowMultipleComponent]
    public sealed class UnityMovementDriver : MonoBehaviour
    {
        private MovementSystem _movement;

        public void Initialize(MovementSystem movement) =>
            _movement = movement ?? throw new ArgumentNullException(nameof(movement));

        private void Update() => _movement?.Tick(Time.deltaTime);
    }
}
