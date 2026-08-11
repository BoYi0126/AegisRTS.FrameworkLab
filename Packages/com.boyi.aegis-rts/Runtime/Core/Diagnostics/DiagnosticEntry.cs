using System;

namespace AegisRTS.Core.Diagnostics
{
    /// <summary>Represents one immutable diagnostic observation.</summary>
    public readonly struct DiagnosticEntry
    {
        public DiagnosticEntry(
            DateTimeOffset timestamp,
            DiagnosticSeverity severity,
            string category,
            string message)
        {
            Timestamp = timestamp;
            Severity = severity;
            Category = category ?? throw new ArgumentNullException(nameof(category));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>Gets when the entry was recorded.</summary>
        public DateTimeOffset Timestamp { get; }

        /// <summary>Gets the entry severity.</summary>
        public DiagnosticSeverity Severity { get; }

        /// <summary>Gets the subsystem category.</summary>
        public string Category { get; }

        /// <summary>Gets the human-readable message.</summary>
        public string Message { get; }

        /// <inheritdoc />
        public override string ToString() => $"[{Timestamp:O}] [{Severity}] [{Category}] {Message}";
    }
}
