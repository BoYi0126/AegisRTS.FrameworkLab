using System.Collections.Generic;

namespace AegisRTS.Gameplay.Content.Definitions
{
    /// <summary>Common read-only contract for all static gameplay definitions.</summary>
    public interface IDefinition
    {
        DefinitionId Id { get; }

        string DisplayName { get; }

        IReadOnlyList<ContentTag> Tags { get; }
    }
}
