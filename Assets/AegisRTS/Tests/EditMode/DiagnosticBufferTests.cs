using AegisRTS.Core.Diagnostics;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class DiagnosticBufferTests
    {
        [Test]
        public void Record_WhenFull_DropsOldestEntry()
        {
            var diagnostics = new DiagnosticBuffer(2);

            diagnostics.Record(DiagnosticSeverity.Info, "Test", "First");
            diagnostics.Record(DiagnosticSeverity.Warning, "Test", "Second");
            diagnostics.Record(DiagnosticSeverity.Error, "Test", "Third");

            var snapshot = diagnostics.Snapshot();
            Assert.That(snapshot.Count, Is.EqualTo(2));
            Assert.That(snapshot[0].Message, Is.EqualTo("Second"));
            Assert.That(snapshot[1].Message, Is.EqualTo("Third"));

            diagnostics.Clear();
            Assert.That(diagnostics.Count, Is.Zero);
        }
    }
}
