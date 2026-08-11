using System;

namespace AegisRTS.Gameplay.Content.Definitions
{
    /// <summary>World-neutral additive and multiplicative modifier for a named gameplay stat.</summary>
    public readonly struct TechnologyModifier
    {
        public TechnologyModifier(string statId, double additive, double multiplier)
        {
            if (string.IsNullOrWhiteSpace(statId)) throw new ArgumentException("A stat ID is required.", nameof(statId));
            StatId = statId.Trim().ToLowerInvariant();
            Additive = additive;
            Multiplier = multiplier;
        }

        public string StatId { get; }
        public double Additive { get; }
        public double Multiplier { get; }
    }
}
