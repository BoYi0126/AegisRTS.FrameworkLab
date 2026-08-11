using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AegisRTS.Gameplay.Content.Validation
{
    /// <summary>Immutable result returned by content validation.</summary>
    public sealed class ContentValidationResult
    {
        internal ContentValidationResult(IEnumerable<ContentValidationIssue> issues)
        {
            Issues = new ReadOnlyCollection<ContentValidationIssue>(
                new List<ContentValidationIssue>(issues ?? Array.Empty<ContentValidationIssue>()));
        }

        public bool IsValid => Issues.Count == 0;

        public IReadOnlyList<ContentValidationIssue> Issues { get; }

        public string GetDebugSummary() => IsValid ? "Valid=True, Issues=0" : $"Valid=False, Issues={Issues.Count}";
    }
}
