using System;

namespace AegisRTS.Demo.PlayablePrototype
{
    public enum PrototypeCombatRole
    {
        Hero,
        Infantry,
        Archer,
        Cavalry,
        Siege,
    }

    /// <summary>World-neutral attack cadence used by the playable prototype's generic combat roles.</summary>
    public readonly struct PrototypeAttackTiming
    {
        public PrototypeAttackTiming(double intervalSeconds, double windupSeconds, double moveCancelBlendSeconds)
        {
            if (intervalSeconds <= 0d || windupSeconds < 0d || windupSeconds > intervalSeconds ||
                moveCancelBlendSeconds < 0d)
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds));
            IntervalSeconds = intervalSeconds;
            WindupSeconds = windupSeconds;
            MoveCancelBlendSeconds = moveCancelBlendSeconds;
        }

        public double IntervalSeconds { get; }
        public double AttacksPerSecond => 1d / IntervalSeconds;
        public double WindupSeconds { get; }
        public double RecoverySeconds => IntervalSeconds - WindupSeconds;
        public double MoveCancelableBackswingSeconds => RecoverySeconds;
        public double MoveCancelBlendSeconds { get; }
    }

    public static class PrototypeCombatTuning
    {
        public static PrototypeAttackTiming Get(PrototypeCombatRole role)
        {
            switch (role)
            {
                case PrototypeCombatRole.Hero: return new PrototypeAttackTiming(0.80d, 0.25d, 0.06d);
                case PrototypeCombatRole.Infantry: return new PrototypeAttackTiming(0.95d, 0.30d, 0.07d);
                case PrototypeCombatRole.Archer: return new PrototypeAttackTiming(1.10d, 0.38d, 0.06d);
                case PrototypeCombatRole.Cavalry: return new PrototypeAttackTiming(1.25d, 0.40d, 0.08d);
                case PrototypeCombatRole.Siege: return new PrototypeAttackTiming(2.20d, 1.05d, 0.12d);
                default: throw new ArgumentOutOfRangeException(nameof(role));
            }
        }
    }
}
