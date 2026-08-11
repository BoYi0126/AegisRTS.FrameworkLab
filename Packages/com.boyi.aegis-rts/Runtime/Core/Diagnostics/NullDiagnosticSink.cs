namespace AegisRTS.Core.Diagnostics
{
    /// <summary>Discards diagnostic observations when no sink is configured.</summary>
    public sealed class NullDiagnosticSink : IDiagnosticSink
    {
        private NullDiagnosticSink()
        {
        }

        /// <summary>Gets the shared no-op sink.</summary>
        public static NullDiagnosticSink Instance { get; } = new NullDiagnosticSink();

        /// <inheritdoc />
        public void Record(DiagnosticSeverity severity, string category, string message)
        {
        }
    }
}
