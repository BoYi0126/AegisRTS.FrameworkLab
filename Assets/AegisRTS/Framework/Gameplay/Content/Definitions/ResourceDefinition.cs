using System.Collections.Generic;

namespace AegisRTS.Gameplay.Content.Definitions
{
    public sealed class ResourceDefinition : DefinitionBase
    {
        public ResourceDefinition(DefinitionId id, string displayName, IEnumerable<ContentTag> tags)
            : base(id, displayName, tags)
        {
        }
    }
}
