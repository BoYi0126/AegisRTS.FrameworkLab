using System.Collections;
using System.Linq;
using AegisRTS.Demo;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AegisRTS.Tests.PlayMode
{
    public sealed class PerformanceSandboxPlayModeTests
    {
        [UnityTest]
        public IEnumerator SandboxPerformance_Explores100To1000UnitsAndUsesThrottlingPoolSpatialIndex()
        {
            SceneManager.LoadScene("Sandbox_AI", LoadSceneMode.Single); yield return null;
            PerformanceSandboxBootstrap bootstrap = Object.FindAnyObjectByType<PerformanceSandboxBootstrap>();
            Assert.That(bootstrap, Is.Not.Null); Assert.That(bootstrap.AcceptancePassed, Is.True);
            Assert.That(bootstrap.Report.Scenarios.Select(item => item.UnitCount), Is.EqualTo(new[] { 100, 300, 500, 1000 }));
            Assert.That(bootstrap.SimulationTicks, Is.EqualTo(30)); Assert.That(bootstrap.AiTicks, Is.EqualTo(5)); Assert.That(bootstrap.NavigationTicks, Is.EqualTo(10));
            Assert.That(bootstrap.PoolReused, Is.True); Assert.That(bootstrap.Report.Metrics.PeakUnits, Is.EqualTo(1000));
        }
    }
}
