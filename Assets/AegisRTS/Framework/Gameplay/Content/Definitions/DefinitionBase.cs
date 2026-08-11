using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AegisRTS.Gameplay.Content.Definitions
{
    /// <summary>Copies authored values into an immutable definition surface.</summary>
    public abstract class DefinitionBase : IDefinition
    {
        protected DefinitionBase(DefinitionId id, string displayName, IEnumerable<ContentTag> tags)
        {
            if (!id.IsValid)
            {
                throw new ArgumentException("A valid definition ID is required.", nameof(id));
            }

            Id = id;
            DisplayName = displayName ?? string.Empty;
            Tags = Copy(tags);
        }

        public DefinitionId Id { get; }

        public string DisplayName { get; }

        public IReadOnlyList<ContentTag> Tags { get; }

        protected static IReadOnlyList<T> Copy<T>(IEnumerable<T> values)
        {
            return new ReadOnlyCollection<T>(new List<T>(values ?? Array.Empty<T>()));
        }
    }
}
