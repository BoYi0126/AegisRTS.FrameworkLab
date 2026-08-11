namespace AegisRTS.Core.Diagnostics
{
    /// <summary>Receives framework diagnostics without coupling Core to a logging implementation.</summary>
    public interface IDiagnosticSink
    {
        /// <summary>Records a diagnostic observation.</summary>
        void Record(DiagnosticSeverity severity, string category, string message);
    }
}
