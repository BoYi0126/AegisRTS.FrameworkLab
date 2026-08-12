using System;
using AegisRTS.Gameplay.Combat;
using UnityEngine;

namespace AegisRTS.Demo.PlayablePrototype
{
    /// <summary>Presentation-only bridge from authoritative snapshots to infantry animation parameters.</summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeUnitAnimatorView : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int MoveRate = Animator.StringToHash("MoveRate");
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int Die = Animator.StringToHash("Die");
        private static readonly int IsDead = Animator.StringToHash("IsDead");

        [SerializeField] private Animator animator;
        [SerializeField] private float deathDurationSeconds = 1.3f;
        [SerializeField] private float referenceMovementSpeed = 4.5f;
        [SerializeField] private float referenceClipRate = 1.8f;
        private CombatantState _previousState = CombatantState.Idle;
        private double _previousHealth = double.NaN;
        private bool _dead;

        public Animator Animator => animator;
        public float DeathDurationSeconds => deathDurationSeconds;
        public int AttackImpactCount { get; private set; }
        public int FootstepCount { get; private set; }
        public int DeathSettledCount { get; private set; }

        public void Configure(Animator target, float deathDuration = 1.3f)
        {
            animator = target;
            deathDurationSeconds = Mathf.Max(0.1f, deathDuration);
            if (animator != null) animator.applyRootMotion = false;
        }

        public void Refresh(bool moving, double worldSpeed, CombatantSnapshot combat)
        {
            if (animator == null || _dead) return;
            animator.SetFloat(Speed, moving ? 1f : 0f, 0.08f, Time.deltaTime);
            float rate = moving
                ? Mathf.Clamp((float)(Math.Max(0.1d, worldSpeed) / referenceMovementSpeed) * referenceClipRate, 0.65f, 2.4f)
                : referenceClipRate;
            animator.SetFloat(MoveRate, rate);

            bool enteringAttack = combat.State == CombatantState.Windup && _previousState != CombatantState.Windup;
            if (enteringAttack) animator.SetTrigger(Attack);
            if (!double.IsNaN(_previousHealth) && combat.Health < _previousHealth && combat.IsAlive)
                animator.SetTrigger(Hit);

            _previousState = combat.State;
            _previousHealth = combat.Health;
        }

        public void PlayDeath()
        {
            if (_dead || animator == null) return;
            _dead = true;
            animator.SetFloat(Speed, 0f);
            animator.SetBool(IsDead, true);
            animator.SetTrigger(Die);
        }

        // Animation Events are visual timing signals only. Gameplay damage remains authoritative in CombatSystem.
        public void AttackImpact() => AttackImpactCount++;
        public void Footstep_L() => FootstepCount++;
        public void Footstep_R() => FootstepCount++;
        public void DeathSettled() => DeathSettledCount++;
    }
}
