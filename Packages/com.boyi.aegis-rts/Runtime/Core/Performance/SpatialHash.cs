using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace AegisRTS.Core.Performance
{
    public readonly struct SpatialPoint
    {
        public SpatialPoint(double x, double y) { X = x; Y = y; }
        public double X { get; }
        public double Y { get; }
        public double DistanceSquared(SpatialPoint other) { double x = X - other.X, y = Y - other.Y; return x * x + y * y; }
    }

    public sealed class SpatialHash<T>
    {
        private readonly double _cellSize; private readonly IComparer<T> _sort;
        private readonly Dictionary<Cell, HashSet<T>> _cells = new Dictionary<Cell, HashSet<T>>();
        private readonly Dictionary<T, Entry> _entries;
        public SpatialHash(double cellSize, IEqualityComparer<T> equalityComparer = null, IComparer<T> sortComparer = null)
        {
            if (cellSize <= 0d || double.IsNaN(cellSize) || double.IsInfinity(cellSize)) throw new ArgumentOutOfRangeException(nameof(cellSize));
            _cellSize = cellSize; _sort = sortComparer; _entries = new Dictionary<T, Entry>(equalityComparer ?? EqualityComparer<T>.Default);
        }
        public int Count => _entries.Count;
        public void Insert(T item, SpatialPoint position)
        {
            if (_entries.ContainsKey(item)) throw new InvalidOperationException("Spatial item is already registered."); Cell cell = CellFor(position);
            Bucket(cell).Add(item); _entries.Add(item, new Entry(position, cell));
        }
        public bool Update(T item, SpatialPoint position)
        {
            if (!_entries.TryGetValue(item, out Entry previous)) return false; Cell next = CellFor(position);
            if (!next.Equals(previous.Cell)) { _cells[previous.Cell].Remove(item); if (_cells[previous.Cell].Count == 0) _cells.Remove(previous.Cell); Bucket(next).Add(item); }
            _entries[item] = new Entry(position, next); return true;
        }
        public bool Remove(T item)
        {
            if (!_entries.TryGetValue(item, out Entry entry)) return false; _entries.Remove(item); _cells[entry.Cell].Remove(item);
            if (_cells[entry.Cell].Count == 0) _cells.Remove(entry.Cell); return true;
        }
        public IReadOnlyList<T> Query(SpatialPoint center, double radius)
        {
            if (radius < 0d || double.IsNaN(radius) || double.IsInfinity(radius)) throw new ArgumentOutOfRangeException(nameof(radius));
            int minX = Floor((center.X - radius) / _cellSize), maxX = Floor((center.X + radius) / _cellSize);
            int minY = Floor((center.Y - radius) / _cellSize), maxY = Floor((center.Y + radius) / _cellSize); double radiusSquared = radius * radius; var result = new List<T>();
            for (int x = minX; x <= maxX; x++) for (int y = minY; y <= maxY; y++)
                if (_cells.TryGetValue(new Cell(x, y), out HashSet<T> bucket)) foreach (T item in bucket)
                    if (_entries[item].Position.DistanceSquared(center) <= radiusSquared) result.Add(item);
            if (_sort != null) result.Sort(_sort); return new ReadOnlyCollection<T>(result);
        }
        private HashSet<T> Bucket(Cell cell) { if (!_cells.TryGetValue(cell, out HashSet<T> value)) { value = new HashSet<T>(_entries.Comparer); _cells.Add(cell, value); } return value; }
        private Cell CellFor(SpatialPoint position) => new Cell(Floor(position.X / _cellSize), Floor(position.Y / _cellSize));
        private static int Floor(double value) => (int)Math.Floor(value);
        private readonly struct Entry { public Entry(SpatialPoint position, Cell cell) { Position = position; Cell = cell; } public SpatialPoint Position { get; } public Cell Cell { get; } }
        private readonly struct Cell : IEquatable<Cell>
        { public Cell(int x, int y) { X = x; Y = y; } public int X { get; } public int Y { get; } public bool Equals(Cell other) => X == other.X && Y == other.Y; public override bool Equals(object obj) => obj is Cell other && Equals(other); public override int GetHashCode() => unchecked((X * 397) ^ Y); }
    }

    public enum SimulationLodTier { Full, Reduced, Coarse, Culled }
    public readonly struct SimulationLodDecision
    {
        public SimulationLodDecision(SimulationLodTier tier, double tickFrequencyHz, bool render, bool simulate)
        { Tier = tier; TickFrequencyHz = tickFrequencyHz; Render = render; Simulate = simulate; }
        public SimulationLodTier Tier { get; }
        public double TickFrequencyHz { get; }
        public bool Render { get; }
        public bool Simulate { get; }
    }
    public sealed class SimulationLodPolicy
    {
        private readonly double _full, _reduced, _coarse;
        public SimulationLodPolicy(double fullDistance, double reducedDistance, double coarseDistance)
        { if (fullDistance < 0 || reducedDistance <= fullDistance || coarseDistance <= reducedDistance) throw new ArgumentOutOfRangeException(nameof(fullDistance)); _full = fullDistance; _reduced = reducedDistance; _coarse = coarseDistance; }
        public SimulationLodDecision Evaluate(double distance)
        {
            if (distance < 0 || double.IsNaN(distance) || double.IsInfinity(distance)) throw new ArgumentOutOfRangeException(nameof(distance));
            if (distance <= _full) return new SimulationLodDecision(SimulationLodTier.Full, 30, true, true);
            if (distance <= _reduced) return new SimulationLodDecision(SimulationLodTier.Reduced, 15, true, true);
            if (distance <= _coarse) return new SimulationLodDecision(SimulationLodTier.Coarse, 5, false, true);
            return new SimulationLodDecision(SimulationLodTier.Culled, 0, false, false);
        }
    }

    public sealed class StressScenarioResult
    {
        public StressScenarioResult(int unitCount, long neighborResults, double elapsedMs, long memoryDeltaBytes)
        { UnitCount = unitCount; NeighborResults = neighborResults; ElapsedMs = elapsedMs; MemoryDeltaBytes = memoryDeltaBytes; }
        public int UnitCount { get; }
        public long NeighborResults { get; }
        public double ElapsedMs { get; }
        public long MemoryDeltaBytes { get; }
    }
    public sealed class ExploratoryStressReport
    {
        public ExploratoryStressReport(IEnumerable<StressScenarioResult> scenarios, PerformanceMetricsSnapshot metrics)
        { Scenarios = new ReadOnlyCollection<StressScenarioResult>(new List<StressScenarioResult>(scenarios)); Metrics = metrics; }
        public IReadOnlyList<StressScenarioResult> Scenarios { get; }
        public PerformanceMetricsSnapshot Metrics { get; }
    }
    public sealed class PerformanceStressHarness
    {
        public ExploratoryStressReport Run(params int[] unitCounts)
        {
            int[] counts = unitCounts == null || unitCounts.Length == 0 ? new[] { 100, 300, 500, 1000 } : unitCounts;
            var results = new List<StressScenarioResult>(); var metrics = new PerformanceMetricsCollector(counts.Length);
            foreach (int count in counts)
            {
                if (count <= 0) throw new ArgumentOutOfRangeException(nameof(unitCounts)); long memoryBefore = GC.GetTotalMemory(false); var watch = Stopwatch.StartNew();
                var spatial = new SpatialHash<int>(4d, sortComparer: Comparer<int>.Default); int width = (int)Math.Ceiling(Math.Sqrt(count));
                for (int i = 0; i < count; i++) spatial.Insert(i, new SpatialPoint((i % width) * 2d, (i / width) * 2d));
                long neighbors = 0; for (int i = 0; i < count; i++) neighbors += spatial.Query(new SpatialPoint((i % width) * 2d, (i / width) * 2d), 3d).Count;
                watch.Stop(); long memoryDelta = Math.Max(0, GC.GetTotalMemory(false) - memoryBefore); double elapsed = Math.Max(0.001d, watch.Elapsed.TotalMilliseconds);
                results.Add(new StressScenarioResult(count, neighbors, elapsed, memoryDelta));
                metrics.Record(new PerformanceSample(elapsed, elapsed * 0.65d, elapsed * 0.10d, elapsed * 0.20d, count, count / 4, 0, GC.GetTotalMemory(false)));
            }
            return new ExploratoryStressReport(results, metrics.Snapshot());
        }
    }
}
