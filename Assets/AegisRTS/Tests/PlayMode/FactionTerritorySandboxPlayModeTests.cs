using System.Collections;
using AegisRTS.Demo;
using AegisRTS.Gameplay.Factions;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Territory;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AegisRTS.Tests.PlayMode
{
    public sealed class FactionTerritorySandboxPlayModeTests
    {
        [UnityTest]
        public IEnumerator SandboxSiege_ComposesThreeSettlementsAndTerritoryGraph()
        {
            SceneManager.LoadScene("Sandbox_Siege", LoadSceneMode.Single);
            yield return null;

            FactionTerritorySandboxBootstrap bootstrap = Object.FindAnyObjectByType<FactionTerritorySandboxBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.SettlementVisualCount, Is.EqualTo(3));
            Assert.That(bootstrap.ConnectionVisualCount, Is.EqualTo(2));
            Assert.That(bootstrap.Settlements.SettlementCount, Is.EqualTo(3));
            Assert.That(bootstrap.Territories.TerritoryCount, Is.EqualTo(3));
            Assert.That(bootstrap.Factions.FactionCount, Is.EqualTo(2));
            Assert.That(bootstrap.AcceptancePassed, Is.True);
        }

        [UnityTest]
        public IEnumerator SandboxSiege_CapturesAllSettlementsAndAutomaticallyTransfersTerritory()
        {
            SceneManager.LoadScene("Sandbox_Siege", LoadSceneMode.Single);
            yield return null;
            FactionTerritorySandboxBootstrap bootstrap = Object.FindAnyObjectByType<FactionTerritorySandboxBootstrap>();

            Assert.That(bootstrap.Factions.TryGetState(bootstrap.InitialFactionId, out FactionSnapshot previous), Is.True);
            Assert.That(previous.SettlementIds, Is.Empty);
            Assert.That(previous.TerritoryIds, Is.Empty);
            Assert.That(bootstrap.Factions.TryGetState(bootstrap.CapturingFactionId, out FactionSnapshot current), Is.True);
            Assert.That(current.SettlementIds.Count, Is.EqualTo(3));
            Assert.That(current.TerritoryIds.Count, Is.EqualTo(3));
            foreach (SettlementSnapshot settlement in bootstrap.Settlements.Snapshot())
                Assert.That(settlement.OwnerId, Is.EqualTo(bootstrap.CapturingFactionId));
            foreach (TerritorySnapshot territory in bootstrap.Territories.Snapshot())
                Assert.That(territory.OwnerId, Is.EqualTo(bootstrap.CapturingFactionId));
            Assert.That(bootstrap.SettlementCaptureEventCount, Is.EqualTo(3));
            Assert.That(bootstrap.TerritoryOwnerEventCount, Is.EqualTo(3));
        }
    }
}
