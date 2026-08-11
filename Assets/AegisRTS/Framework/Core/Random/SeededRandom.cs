using System;
using System.Globalization;

namespace AegisRTS.Core.Random
{
    /// <summary>
    /// Deterministic PCG-based random source whose sequence is stable across supported runtimes.
    /// </summary>
    public sealed class SeededRandom : IRandomSource
    {
        private const ulong Multiplier = 6364136223846793005UL;
        private const ulong Increment = 1442695040888963407UL;

        private ulong _state;

        /// <summary>Initializes a deterministic sequence from <paramref name="seed"/>.</summary>
        public SeededRandom(int seed)
        {
            Seed = seed;
            _state = 0UL;
            AdvanceState();
            _state = unchecked(_state + (uint)seed);
            AdvanceState();
            DrawCount = 0;
        }

        /// <summary>Gets the seed used to initialize this instance.</summary>
        public int Seed { get; }

        /// <summary>Gets the number of 32-bit values consumed after initialization.</summary>
        public ulong DrawCount { get; private set; }

        /// <inheritdoc />
        public uint NextUInt()
        {
            DrawCount++;
            return AdvanceState();
        }

        /// <inheritdoc />
        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "Upper bound must be greater than zero.");
            }

            return (int)NextUIntBounded((uint)maxExclusive);
        }

        /// <inheritdoc />
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "Upper bound must be greater than lower bound.");
            }

            uint range = (uint)((long)maxExclusive - minInclusive);
            long result = minInclusive + (long)NextUIntBounded(range);
            return (int)result;
        }

        /// <inheritdoc />
        public float NextFloat() => (NextUInt() >> 8) * (1f / 16777216f);

        /// <inheritdoc />
        public double NextDouble()
        {
            ulong high = NextUInt() >> 5;
            ulong low = NextUInt() >> 6;
            return ((high * 67108864d) + low) / 9007199254740992d;
        }

        /// <inheritdoc />
        public bool NextBool(double probability = 0.5d)
        {
            if (double.IsNaN(probability) || probability < 0d || probability > 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(probability), "Probability must be between zero and one.");
            }

            if (probability <= 0d)
            {
                return false;
            }

            if (probability >= 1d)
            {
                return true;
            }

            return NextDouble() < probability;
        }

        /// <summary>Returns a concise state string suitable for diagnostics tools.</summary>
        public string GetDebugSummary()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Seed={0}, Draws={1}, State=0x{2:X16}",
                Seed,
                DrawCount,
                _state);
        }

        private uint NextUIntBounded(uint bound)
        {
            uint threshold = unchecked(0u - bound) % bound;
            while (true)
            {
                uint value = NextUInt();
                if (value >= threshold)
                {
                    return value % bound;
                }
            }
        }

        private uint AdvanceState()
        {
            ulong oldState = _state;
            _state = unchecked((oldState * Multiplier) + Increment);
            uint xorshifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
            int rotation = (int)(oldState >> 59);
            return (xorshifted >> rotation) | (xorshifted << ((-rotation) & 31));
        }
    }
}
