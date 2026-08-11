namespace AegisRTS.Core.Random
{
    /// <summary>
    /// Provides deterministic random values to simulation systems.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>Returns an unsigned value across the full 32-bit range.</summary>
        uint NextUInt();

        /// <summary>Returns an integer in the range [0, <paramref name="maxExclusive"/>).</summary>
        int NextInt(int maxExclusive);

        /// <summary>Returns an integer in the range [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
        int NextInt(int minInclusive, int maxExclusive);

        /// <summary>Returns a float in the range [0, 1).</summary>
        float NextFloat();

        /// <summary>Returns a double in the range [0, 1).</summary>
        double NextDouble();

        /// <summary>Returns true with the supplied probability.</summary>
        bool NextBool(double probability = 0.5d);
    }
}
