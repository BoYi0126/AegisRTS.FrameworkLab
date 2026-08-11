using System.Collections.Generic;

namespace AegisRTS.Gameplay.Content.Definitions
{
    public sealed class AbilityDefinition : DefinitionBase
    {
        public AbilityDefinition(
            DefinitionId id,
            string displayName,
            double cooldownSeconds,
            double range,
            IEnumerable<ContentTag> tags)
            : base(id, displayName, tags)
        {
            CooldownSeconds = cooldownSeconds;
            Range = range;
        }

        public double CooldownSeconds { get; }

        public double Range { get; }
    }
}
