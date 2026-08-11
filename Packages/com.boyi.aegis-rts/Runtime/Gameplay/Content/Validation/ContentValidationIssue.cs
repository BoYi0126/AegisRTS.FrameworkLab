using System;

namespace AegisRTS.Gameplay.Content.Validation
{
    /// <summary>One actionable content validation failure.</summary>
    public readonly struct ContentValidationIssue
    {
        public ContentValidationIssue(ContentValidationIssueCode code, string subjectId, string message)
        {
            Code = code;
            SubjectId = subjectId ?? string.Empty;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public ContentValidationIssueCode Code { get; }

        public string SubjectId { get; }

        public string Message { get; }

        public override string ToString() => $"[{Code}] [{SubjectId}] {Message}";
    }
}
