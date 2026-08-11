using System;
using AegisRTS.Core.Diagnostics;
using AegisRTS.Gameplay.Content.Validation;

namespace AegisRTS.Gameplay.Content
{
    /// <summary>Validates and atomically switches the active content catalog.</summary>
    public sealed class ContentPackService
    {
        private const string DiagnosticCategory = "ContentPack";
        private readonly ContentPackValidator _validator;
        private readonly IDiagnosticSink _diagnostics;

        public ContentPackService(ContentPackValidator validator = null, IDiagnosticSink diagnostics = null)
        {
            _validator = validator ?? new ContentPackValidator();
            _diagnostics = diagnostics ?? NullDiagnosticSink.Instance;
        }

        public ContentCatalog ActiveCatalog { get; private set; }

        public bool HasActivePack => ActiveCatalog != null;

        /// <summary>
        /// Validates a pack and replaces the active catalog only when validation succeeds.
        /// </summary>
        public ContentPackLoadResult Load(ContentPack pack, IContentAssetCatalog assets)
        {
            if (pack == null)
            {
                throw new ArgumentNullException(nameof(pack));
            }

            if (assets == null)
            {
                throw new ArgumentNullException(nameof(assets));
            }

            ContentValidationResult validation = _validator.Validate(pack, assets);
            if (!validation.IsValid)
            {
                _diagnostics.Record(
                    DiagnosticSeverity.Error,
                    DiagnosticCategory,
                    $"Rejected pack {pack.Id} with {validation.Issues.Count} issue(s).");
                return new ContentPackLoadResult(validation, null);
            }

            var catalog = new ContentCatalog(pack);
            ActiveCatalog = catalog;
            _diagnostics.Record(
                DiagnosticSeverity.Info,
                DiagnosticCategory,
                $"Loaded pack {pack.Id} with {catalog.DefinitionCount} definition(s).");
            return new ContentPackLoadResult(validation, catalog);
        }

        public void Clear()
        {
            ActiveCatalog = null;
            _diagnostics.Record(DiagnosticSeverity.Trace, DiagnosticCategory, "Cleared active content pack.");
        }

        public string GetDebugSummary() => HasActivePack
            ? ActiveCatalog.GetDebugSummary()
            : "Pack=None, Definitions=0";
    }
}
