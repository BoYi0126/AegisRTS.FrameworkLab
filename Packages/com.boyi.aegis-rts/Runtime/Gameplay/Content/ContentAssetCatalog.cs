using System;
using System.Collections.Generic;

namespace AegisRTS.Gameplay.Content
{
    /// <summary>Case-sensitive prefab ID catalog supplied by a Unity or test adapter.</summary>
    public sealed class ContentAssetCatalog : IContentAssetCatalog
    {
        private readonly HashSet<string> _prefabIds;

        public ContentAssetCatalog(IEnumerable<string> prefabIds)
        {
            _prefabIds = new HashSet<string>(prefabIds ?? Array.Empty<string>(), StringComparer.Ordinal);
        }

        public bool ContainsPrefab(string prefabId) =>
            !string.IsNullOrWhiteSpace(prefabId) && _prefabIds.Contains(prefabId);
    }
}
