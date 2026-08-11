using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AegisRTS.Core.Performance
{
    public sealed class ScheduledTickSnapshot
    {
        public ScheduledTickSnapshot(string id, double frequencyHz, ulong executionCount, double accumulatedSeconds)
        { Id = id; FrequencyHz = frequencyHz; ExecutionCount = executionCount; AccumulatedSeconds = accumulatedSeconds; }
        public string Id { get; }
        public double FrequencyHz { get; }
        public ulong ExecutionCount { get; }
        public double AccumulatedSeconds { get; }
    }

    public sealed class TickScheduler
    {
        private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
        public void Register(string id, double frequencyHz, Action<double> tick, int maximumCatchUpTicks = 4)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Tick ID is required.", nameof(id));
            if (frequencyHz <= 0d || double.IsNaN(frequencyHz) || double.IsInfinity(frequencyHz)) throw new ArgumentOutOfRangeException(nameof(frequencyHz));
            if (tick == null) throw new ArgumentNullException(nameof(tick)); if (maximumCatchUpTicks <= 0) throw new ArgumentOutOfRangeException(nameof(maximumCatchUpTicks));
            if (_entries.ContainsKey(id)) throw new InvalidOperationException($"Tick '{id}' is already registered.");
            _entries.Add(id, new Entry(id, frequencyHz, tick, maximumCatchUpTicks));
        }
        public bool Unregister(string id) => _entries.Remove(id ?? string.Empty);
        public void Advance(double deltaSeconds)
        {
            if (deltaSeconds < 0d || double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds)) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            foreach (Entry entry in _entries.Values.OrderBy(item => item.Id, StringComparer.Ordinal))
            {
                entry.Accumulator += deltaSeconds; int executions = 0;
                while (entry.Accumulator + 1e-12d >= entry.Interval && executions < entry.MaximumCatchUpTicks)
                { entry.Accumulator -= entry.Interval; entry.Tick(entry.Interval); entry.ExecutionCount++; executions++; }
                if (executions == entry.MaximumCatchUpTicks && entry.Accumulator >= entry.Interval) entry.Accumulator %= entry.Interval;
            }
        }
        public IReadOnlyList<ScheduledTickSnapshot> Snapshot() => new ReadOnlyCollection<ScheduledTickSnapshot>(_entries.Values
            .OrderBy(item => item.Id, StringComparer.Ordinal).Select(item => new ScheduledTickSnapshot(item.Id, item.FrequencyHz, item.ExecutionCount, item.Accumulator)).ToList());
        private sealed class Entry
        {
            public Entry(string id, double frequency, Action<double> tick, int max) { Id = id; FrequencyHz = frequency; Interval = 1d / frequency; Tick = tick; MaximumCatchUpTicks = max; }
            public string Id { get; } public double FrequencyHz { get; } public double Interval { get; } public Action<double> Tick { get; } public int MaximumCatchUpTicks { get; }
            public double Accumulator; public ulong ExecutionCount;
        }
    }

    public sealed class ObjectPool<T> where T : class
    {
        private readonly Func<T> _factory; private readonly Action<T> _onRent, _onReturn; private readonly Stack<T> _available;
        public ObjectPool(Func<T> factory, int initialCapacity = 0, int maximumRetained = 256, Action<T> onRent = null, Action<T> onReturn = null)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory)); if (initialCapacity < 0 || maximumRetained <= 0 || initialCapacity > maximumRetained) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            MaximumRetained = maximumRetained; _onRent = onRent; _onReturn = onReturn; _available = new Stack<T>(maximumRetained);
            for (int i = 0; i < initialCapacity; i++) _available.Push(Create());
        }
        public int MaximumRetained { get; }
        public int AvailableCount => _available.Count;
        public int ActiveCount { get; private set; }
        public int CreatedCount { get; private set; }
        public T Rent() { T value = _available.Count > 0 ? _available.Pop() : Create(); ActiveCount++; _onRent?.Invoke(value); return value; }
        public bool Return(T value)
        {
            if (value == null) return false; if (ActiveCount <= 0) throw new InvalidOperationException("Pool has no active items to return.");
            ActiveCount--; _onReturn?.Invoke(value); if (_available.Count >= MaximumRetained) return false; _available.Push(value); return true;
        }
        private T Create() { T value = _factory(); if (value == null) throw new InvalidOperationException("Pool factory returned null."); CreatedCount++; return value; }
    }
}
