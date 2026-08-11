using System;
using System.Collections.Generic;
using AegisRTS.Gameplay.Content.Definitions;

namespace AegisRTS.Gameplay.Content
{
    /// <summary>Read-only, typed lookup index built from one validated content pack.</summary>
    public sealed class ContentCatalog
    {
        private readonly Dictionary<DefinitionId, IDefinition> _definitions;

        internal ContentCatalog(ContentPack pack)
        {
            Pack = pack ?? throw new ArgumentNullException(nameof(pack));
            _definitions = new Dictionary<DefinitionId, IDefinition>();
            foreach (IDefinition definition in pack.EnumerateDefinitions())
            {
                _definitions.Add(definition.Id, definition);
            }
        }

        public ContentPack Pack { get; }

        public int DefinitionCount => _definitions.Count;

        /// <summary>Looks up an exact definition type by stable ID.</summary>
        public bool TryGet<TDefinition>(DefinitionId id, out TDefinition definition)
            where TDefinition : class, IDefinition
        {
            if (_definitions.TryGetValue(id, out IDefinition value) && value is TDefinition typed)
            {
                definition = typed;
                return true;
            }

            definition = null;
            return false;
        }

        /// <summary>Returns an exact definition type or throws when the ID is absent or has another type.</summary>
        public TDefinition GetRequired<TDefinition>(DefinitionId id)
            where TDefinition : class, IDefinition
        {
            if (TryGet(id, out TDefinition definition))
            {
                return definition;
            }

            throw new KeyNotFoundException($"Definition '{id}' was not found as {typeof(TDefinition).Name}.");
        }

        /// <summary>Returns a concise state string suitable for diagnostics tools.</summary>
        public string GetDebugSummary() => $"Pack={Pack.Id}, Definitions={DefinitionCount}";
    }
}
