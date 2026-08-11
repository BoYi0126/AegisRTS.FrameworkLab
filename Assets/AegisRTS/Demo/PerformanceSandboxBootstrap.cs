using System.Linq;
using AegisRTS.Core.Performance;
using UnityEngine;

namespace AegisRTS.Demo
{
    /// <summary>Phase 14 exploratory 100/300/500/1000-unit performance baseline.</summary>
    [DisallowMultipleComponent]
    public sealed class PerformanceSandboxBootstrap : MonoBehaviour
    {
        public ExploratoryStressReport Report { get; private set; }
        public int SimulationTicks { get; private set; }
        public int AiTicks { get; private set; }
        public int NavigationTicks { get; private set; }
        public bool PoolReused { get; private set; }
        public bool AcceptancePassed { get; private set; }

        private void Awake()
        {
            Report = new PerformanceStressHarness().Run(100, 300, 500, 1000);
            var scheduler = new TickScheduler(); scheduler.Register("simulation", 30, _ => SimulationTicks++);
            scheduler.Register("ai", 5, _ => AiTicks++); scheduler.Register("navigation", 10, _ => NavigationTicks++);
            for (int i = 0; i < 60; i++) scheduler.Advance(1d / 60d);
            int created = 0; var pool = new ObjectPool<Token>(() => { created++; return new Token(); }, maximumRetained: 8);
            Token first = pool.Rent(); pool.Return(first); Token second = pool.Rent(); PoolReused = ReferenceEquals(first, second) && created == 1;
            AcceptancePassed = Report.Scenarios.Select(item => item.UnitCount).SequenceEqual(new[] { 100, 300, 500, 1000 }) &&
                Report.Scenarios.All(item => item.NeighborResults < (long)item.UnitCount * item.UnitCount / 5) &&
                SimulationTicks == 30 && AiTicks == 5 && NavigationTicks == 10 && PoolReused;
        }
        private sealed class Token { }
    }
}
