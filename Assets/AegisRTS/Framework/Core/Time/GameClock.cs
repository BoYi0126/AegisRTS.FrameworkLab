using System;
using System.Globalization;

namespace AegisRTS.Core.Time
{
    /// <summary>
    /// Advances simulation time from caller-supplied unscaled time.
    /// </summary>
    /// <remarks>
    /// The clock has no Unity dependency. A presentation adapter is responsible for supplying frame time.
    /// </remarks>
    public sealed class GameClock
    {
        /// <summary>Gets the total scaled simulation time.</summary>
        public double TotalSeconds { get; private set; }

        /// <summary>Gets the total caller-supplied time, including time elapsed while paused.</summary>
        public double TotalUnscaledSeconds { get; private set; }

        /// <summary>Gets the scaled delta produced by the latest call to <see cref="Advance"/>.</summary>
        public double DeltaSeconds { get; private set; }

        /// <summary>Gets the unscaled delta supplied to the latest call to <see cref="Advance"/>.</summary>
        public double UnscaledDeltaSeconds { get; private set; }

        /// <summary>Gets the number of times the clock has advanced.</summary>
        public ulong TickCount { get; private set; }

        /// <summary>Gets whether scaled simulation time is paused.</summary>
        public bool IsPaused { get; private set; }

        /// <summary>Gets the positive multiplier applied to unscaled time.</summary>
        public double Speed { get; private set; } = 1d;

        /// <summary>Pauses scaled simulation time without discarding unscaled time.</summary>
        public void Pause() => IsPaused = true;

        /// <summary>Resumes scaled simulation time.</summary>
        public void Resume() => IsPaused = false;

        /// <summary>Sets the positive simulation speed multiplier.</summary>
        public void SetSpeed(double speed)
        {
            if (double.IsNaN(speed) || double.IsInfinity(speed) || speed <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(speed), "Game speed must be finite and greater than zero.");
            }

            Speed = speed;
        }

        /// <summary>Advances the clock using an unscaled time step.</summary>
        public void Advance(double unscaledDeltaSeconds)
        {
            if (double.IsNaN(unscaledDeltaSeconds) ||
                double.IsInfinity(unscaledDeltaSeconds) ||
                unscaledDeltaSeconds < 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(unscaledDeltaSeconds),
                    "Delta time must be finite and non-negative.");
            }

            UnscaledDeltaSeconds = unscaledDeltaSeconds;
            TotalUnscaledSeconds += unscaledDeltaSeconds;
            DeltaSeconds = IsPaused ? 0d : unscaledDeltaSeconds * Speed;
            TotalSeconds += DeltaSeconds;
            TickCount++;
        }

        /// <summary>Restores the clock to its initial running state.</summary>
        public void Reset()
        {
            TotalSeconds = 0d;
            TotalUnscaledSeconds = 0d;
            DeltaSeconds = 0d;
            UnscaledDeltaSeconds = 0d;
            TickCount = 0;
            IsPaused = false;
            Speed = 1d;
        }

        /// <summary>Returns a concise state string suitable for diagnostics tools.</summary>
        public string GetDebugSummary()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Ticks={0}, Total={1:0.###}, Delta={2:0.###}, Speed={3:0.###}, Paused={4}",
                TickCount,
                TotalSeconds,
                DeltaSeconds,
                Speed,
                IsPaused);
        }
    }
}
