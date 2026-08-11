using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AegisRTS.Core.Performance
{
    public readonly struct PerformanceSample
    {
        public PerformanceSample(double frameMs, double simulationMs, double aiMs, double navigationMs,
            int unitCount, int projectileCount, long gcAllocatedBytes, long memoryBytes)
        {
            if (!FiniteNonNegative(frameMs) || !FiniteNonNegative(simulationMs) || !FiniteNonNegative(aiMs) || !FiniteNonNegative(navigationMs)) throw new ArgumentOutOfRangeException(nameof(frameMs));
            if (unitCount < 0 || projectileCount < 0 || gcAllocatedBytes < 0 || memoryBytes < 0) throw new ArgumentOutOfRangeException(nameof(unitCount));
            FrameMs = frameMs; SimulationMs = simulationMs; AiMs = aiMs; NavigationMs = navigationMs;
            UnitCount = unitCount; ProjectileCount = projectileCount; GcAllocatedBytes = gcAllocatedBytes; MemoryBytes = memoryBytes;
        }
        public double FrameMs { get; }
        public double SimulationMs { get; }
        public double AiMs { get; }
        public double NavigationMs { get; }
        public int UnitCount { get; }
        public int ProjectileCount { get; }
        public long GcAllocatedBytes { get; }
        public long MemoryBytes { get; }
        private static bool FiniteNonNegative(double value) => value >= 0d && !double.IsNaN(value) && !double.IsInfinity(value);
    }

    public sealed class PerformanceMetricsSnapshot
    {
        public PerformanceMetricsSnapshot(int sampleCount, double averageFps, double averageFrameMs, double p95FrameMs,
            double averageSimulationMs, double averageAiMs, double averageNavigationMs, int peakUnits,
            int peakProjectiles, long peakGcAllocatedBytes, long peakMemoryBytes)
        {
            SampleCount = sampleCount; AverageFps = averageFps; AverageFrameMs = averageFrameMs; P95FrameMs = p95FrameMs;
            AverageSimulationMs = averageSimulationMs; AverageAiMs = averageAiMs; AverageNavigationMs = averageNavigationMs;
            PeakUnits = peakUnits; PeakProjectiles = peakProjectiles; PeakGcAllocatedBytes = peakGcAllocatedBytes; PeakMemoryBytes = peakMemoryBytes;
        }
        public int SampleCount { get; }
        public double AverageFps { get; }
        public double AverageFrameMs { get; }
        public double P95FrameMs { get; }
        public double AverageSimulationMs { get; }
        public double AverageAiMs { get; }
        public double AverageNavigationMs { get; }
        public int PeakUnits { get; }
        public int PeakProjectiles { get; }
        public long PeakGcAllocatedBytes { get; }
        public long PeakMemoryBytes { get; }
    }

    public sealed class PerformanceMetricsCollector
    {
        private readonly Queue<PerformanceSample> _samples; public PerformanceMetricsCollector(int capacity = 300)
        { if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity)); Capacity = capacity; _samples = new Queue<PerformanceSample>(capacity); }
        public int Capacity { get; }
        public int Count => _samples.Count;
        public void Record(PerformanceSample sample) { if (_samples.Count == Capacity) _samples.Dequeue(); _samples.Enqueue(sample); }
        public PerformanceMetricsSnapshot Snapshot()
        {
            PerformanceSample[] values = _samples.ToArray(); if (values.Length == 0) return new PerformanceMetricsSnapshot(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            double averageFrame = values.Average(item => item.FrameMs); double[] frames = values.Select(item => item.FrameMs).OrderBy(item => item).ToArray();
            int p95 = Math.Min(frames.Length - 1, (int)Math.Ceiling(frames.Length * 0.95d) - 1);
            return new PerformanceMetricsSnapshot(values.Length, averageFrame <= 0 ? 0 : 1000d / averageFrame, averageFrame, frames[p95],
                values.Average(item => item.SimulationMs), values.Average(item => item.AiMs), values.Average(item => item.NavigationMs),
                values.Max(item => item.UnitCount), values.Max(item => item.ProjectileCount), values.Max(item => item.GcAllocatedBytes), values.Max(item => item.MemoryBytes));
        }
    }

    public sealed class PerformanceBudget
    {
        public PerformanceBudget(double minimumAverageFps, double maximumP95FrameMs, double maximumSimulationMs,
            double maximumAiMs, double maximumNavigationMs, long maximumGcBytesPerFrame, long maximumMemoryBytes)
        {
            MinimumAverageFps = minimumAverageFps; MaximumP95FrameMs = maximumP95FrameMs; MaximumSimulationMs = maximumSimulationMs;
            MaximumAiMs = maximumAiMs; MaximumNavigationMs = maximumNavigationMs;
            MaximumGcBytesPerFrame = maximumGcBytesPerFrame; MaximumMemoryBytes = maximumMemoryBytes;
        }
        public double MinimumAverageFps { get; }
        public double MaximumP95FrameMs { get; }
        public double MaximumSimulationMs { get; }
        public double MaximumAiMs { get; }
        public double MaximumNavigationMs { get; }
        public long MaximumGcBytesPerFrame { get; }
        public long MaximumMemoryBytes { get; }
    }

    public sealed class PerformanceBudgetResult
    {
        public PerformanceBudgetResult(IEnumerable<string> violations) { Violations = new ReadOnlyCollection<string>(new List<string>(violations)); }
        public IReadOnlyList<string> Violations { get; }
        public bool Passed => Violations.Count == 0;
    }

    public static class PerformanceBudgetEvaluator
    {
        public static PerformanceBudgetResult Evaluate(PerformanceMetricsSnapshot value, PerformanceBudget budget)
        {
            if (value == null) throw new ArgumentNullException(nameof(value)); if (budget == null) throw new ArgumentNullException(nameof(budget)); var issues = new List<string>();
            if (value.AverageFps < budget.MinimumAverageFps) issues.Add("average-fps"); if (value.P95FrameMs > budget.MaximumP95FrameMs) issues.Add("p95-frame-ms");
            if (value.AverageSimulationMs > budget.MaximumSimulationMs) issues.Add("simulation-ms"); if (value.AverageAiMs > budget.MaximumAiMs) issues.Add("ai-ms");
            if (value.AverageNavigationMs > budget.MaximumNavigationMs) issues.Add("navigation-ms"); if (value.PeakGcAllocatedBytes > budget.MaximumGcBytesPerFrame) issues.Add("gc-bytes");
            if (value.PeakMemoryBytes > budget.MaximumMemoryBytes) issues.Add("memory-bytes"); return new PerformanceBudgetResult(issues);
        }
    }
}
