using AegisRTS.Gameplay.Content.Validation;

namespace AegisRTS.Gameplay.Content
{
    /// <summary>Reports validation and the optional catalog produced by a load attempt.</summary>
    public sealed class ContentPackLoadResult
    {
        internal ContentPackLoadResult(ContentValidationResult validation, ContentCatalog catalog)
        {
            Validation = validation;
            Catalog = catalog;
        }

        public bool Succeeded => Catalog != null;

        public ContentValidationResult Validation { get; }

        public ContentCatalog Catalog { get; }
    }
}
