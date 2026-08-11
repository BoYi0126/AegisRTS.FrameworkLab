using System;
using System.Linq;
using AegisRTS.Core.Performance;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class PerformanceToolkitTests
    {
        [Test]
        public void Metrics_ReportsFpsP95SubsystemsCountsGcAndMemory()
        {
            var metrics = new PerformanceMetricsCollector(3);
            metrics.Record(new PerformanceSample(10, 4, 1, 2, 100, 20, 1000, 2000)); metrics.Record(new PerformanceSample(20, 6, 2, 3, 300, 40, 2000, 4000));
            PerformanceMetricsSnapshot value = metrics.Snapshot();
            Assert.That(value.AverageFps, Is.EqualTo(1000d / 15d).Within(0.001)); Assert.That(value.P95FrameMs, Is.EqualTo(20));
            Assert.That(value.AverageSimulationMs, Is.EqualTo(5)); Assert.That(value.AverageAiMs, Is.EqualTo(1.5)); Assert.That(value.AverageNavigationMs, Is.EqualTo(2.5));
            Assert.That(value.PeakUnits, Is.EqualTo(300)); Assert.That(value.PeakProjectiles, Is.EqualTo(40)); Assert.That(value.PeakGcAllocatedBytes, Is.EqualTo(2000)); Assert.That(value.PeakMemoryBytes, Is.EqualTo(4000));
        }

        [Test]
        public void Metrics_UsesBoundedSlidingWindow()
        {
            var metrics = new PerformanceMetricsCollector(2); metrics.Record(Sample(10)); metrics.Record(Sample(20)); metrics.Record(Sample(30));
            Assert.That(metrics.Count, Is.EqualTo(2)); Assert.That(metrics.Snapshot().AverageFrameMs, Is.EqualTo(25));
        }

        [Test]
        public void Budget_ReportsNamedViolationsWithoutHardcodingTargetHardware()
        {
            var snapshot = new PerformanceMetricsSnapshot(1, 30, 33, 40, 12, 8, 9, 1000, 100, 5000, 10000);
            var result = PerformanceBudgetEvaluator.Evaluate(snapshot, new PerformanceBudget(60, 20, 10, 5, 5, 1000, 5000));
            Assert.That(result.Passed, Is.False); Assert.That(result.Violations, Does.Contain("average-fps").And.Contain("navigation-ms").And.Contain("memory-bytes"));
        }

        [Test]
        public void Scheduler_RunsSimulationAiAndNavigationAtDifferentFrequencies()
        {
            int simulation = 0, ai = 0, navigation = 0; var scheduler = new TickScheduler();
            scheduler.Register("simulation", 30, _ => simulation++); scheduler.Register("ai", 5, _ => ai++); scheduler.Register("navigation", 10, _ => navigation++);
            for (int i = 0; i < 60; i++) scheduler.Advance(1d / 60d);
            Assert.That(simulation, Is.EqualTo(30)); Assert.That(ai, Is.EqualTo(5)); Assert.That(navigation, Is.EqualTo(10));
        }

        [Test]
        public void Scheduler_CapsCatchUpWorkAfterLongFrame()
        {
            int ticks = 0; var scheduler = new TickScheduler(); scheduler.Register("simulation", 60, _ => ticks++, maximumCatchUpTicks: 3); scheduler.Advance(1);
            Assert.That(ticks, Is.EqualTo(3)); Assert.That(scheduler.Snapshot()[0].AccumulatedSeconds, Is.LessThan(1d / 60d));
        }

        [Test]
        public void Pool_ReusesObjectsInvokesLifecycleAndCapsRetention()
        {
            int created = 0, rented = 0, returned = 0; var pool = new ObjectPool<Token>(() => { created++; return new Token(); }, maximumRetained: 1, onRent: _ => rented++, onReturn: _ => returned++);
            Token first = pool.Rent(), second = pool.Rent(); Assert.That(created, Is.EqualTo(2));
            Assert.That(pool.Return(first), Is.True); Assert.That(pool.Return(second), Is.False); Token reused = pool.Rent();
            Assert.That(reused, Is.SameAs(first)); Assert.That(rented, Is.EqualTo(3)); Assert.That(returned, Is.EqualTo(2));
        }

        [Test]
        public void SpatialHash_QueriesNearbyItemsInDeterministicOrder()
        {
            var spatial = new SpatialHash<int>(4, sortComparer: System.Collections.Generic.Comparer<int>.Default);
            spatial.Insert(3, new SpatialPoint(0, 0)); spatial.Insert(1, new SpatialPoint(1, 1)); spatial.Insert(2, new SpatialPoint(20, 20));
            Assert.That(spatial.Query(new SpatialPoint(0, 0), 3), Is.EqualTo(new[] { 1, 3 }));
        }

        [Test]
        public void SpatialHash_UpdateAndRemoveMaintainIndex()
        {
            var spatial = new SpatialHash<int>(2); spatial.Insert(1, new SpatialPoint(0, 0)); spatial.Update(1, new SpatialPoint(10, 10));
            Assert.That(spatial.Query(new SpatialPoint(0, 0), 1), Is.Empty); Assert.That(spatial.Query(new SpatialPoint(10, 10), 1), Does.Contain(1));
            Assert.That(spatial.Remove(1), Is.True); Assert.That(spatial.Count, Is.Zero);
        }

        [Test]
        public void LodPolicy_SelectsFullReducedCoarseAndCulledTiers()
        {
            var lod = new SimulationLodPolicy(20, 50, 100);
            Assert.That(lod.Evaluate(10).Tier, Is.EqualTo(SimulationLodTier.Full)); Assert.That(lod.Evaluate(30).Tier, Is.EqualTo(SimulationLodTier.Reduced));
            Assert.That(lod.Evaluate(80).Tier, Is.EqualTo(SimulationLodTier.Coarse)); Assert.That(lod.Evaluate(101).Tier, Is.EqualTo(SimulationLodTier.Culled));
            Assert.That(lod.Evaluate(101).Simulate, Is.False);
        }

        [Test]
        public void StressHarness_Explores100To1000UnitsWithLocalSpatialResults()
        {
            ExploratoryStressReport report = new PerformanceStressHarness().Run(100, 300, 500, 1000);
            Assert.That(report.Scenarios.Select(item => item.UnitCount), Is.EqualTo(new[] { 100, 300, 500, 1000 }));
            Assert.That(report.Scenarios.All(item => item.NeighborResults < (long)item.UnitCount * item.UnitCount / 5), Is.True);
            Assert.That(report.Metrics.PeakUnits, Is.EqualTo(1000)); Assert.That(report.Metrics.AverageSimulationMs, Is.GreaterThan(0));
        }

        private static PerformanceSample Sample(double frame) => new PerformanceSample(frame, 0, 0, 0, 0, 0, 0, 0);
        private sealed class Token { }
    }
}
