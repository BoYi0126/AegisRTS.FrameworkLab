using System;
using System.Collections.Generic;

namespace AegisRTS.Core.Diagnostics
{
    /// <summary>
    /// Stores a bounded, thread-safe history of the most recent diagnostic entries.
    /// </summary>
    public sealed class DiagnosticBuffer : IDiagnosticSink
    {
        private readonly object _syncRoot = new object();
        private readonly Queue<DiagnosticEntry> _entries;

        /// <summary>Initializes a buffer with the supplied maximum entry count.</summary>
        public DiagnosticBuffer(int capacity = 256)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
            }

            Capacity = capacity;
            _entries = new Queue<DiagnosticEntry>(capacity);
        }

        /// <summary>Gets the maximum number of retained entries.</summary>
        public int Capacity { get; }

        /// <summary>Gets the current number of retained entries.</summary>
        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _entries.Count;
                }
            }
        }

        /// <inheritdoc />
        public void Record(DiagnosticSeverity severity, string category, string message)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("A diagnostic category is required.", nameof(category));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var entry = new DiagnosticEntry(DateTimeOffset.UtcNow, severity, category, message);
            lock (_syncRoot)
            {
                if (_entries.Count == Capacity)
                {
                    _entries.Dequeue();
                }

                _entries.Enqueue(entry);
            }
        }

        /// <summary>Returns an immutable snapshot ordered from oldest to newest.</summary>
        public IReadOnlyList<DiagnosticEntry> Snapshot()
        {
            lock (_syncRoot)
            {
                return _entries.ToArray();
            }
        }

        /// <summary>Removes all retained entries.</summary>
        public void Clear()
        {
            lock (_syncRoot)
            {
                _entries.Clear();
            }
        }
    }
}
