using System.Collections;
using AegisRTS.Demo;
using AegisRTS.Gameplay.Settlements;
using AegisRTS.Gameplay.Siege;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AegisRTS.Tests.PlayMode
{
    public sealed class SiegeSandboxPlayModeTests
    {
        [UnityTest]
        public IEnumerator SandboxSiege_BreaksGateRefreshesNavigationAndCapturesSettlement()
        {
            SceneManager.LoadScene("Sandbox_Siege", LoadSceneMode.Single); yield return null;
            SiegeSandboxBootstrap bootstrap = Object.FindAnyObjectByType<SiegeSandboxBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.AcceptancePassed, Is.True);
            Assert.That(bootstrap.BreachEventCount, Is.EqualTo(1));
            Assert.That(bootstrap.Navigation.RefreshCount, Is.EqualTo(1));
            Assert.That(bootstrap.CompletionEventCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator SandboxSiege_TransfersOwnerAfterCaptureObjectiveControl()
        {
            SceneManager.LoadScene("Sandbox_Siege", LoadSceneMode.Single); yield return null;
            SiegeSandboxBootstrap bootstrap = Object.FindAnyObjectByType<SiegeSandboxBootstrap>();
            Assert.That(bootstrap.Sieges.TryGetState(bootstrap.SiegeId, out SiegeSnapshot siege), Is.True);
            Assert.That(siege.State, Is.EqualTo(SiegeState.Completed));
            Assert.That(siege.CurrentArea, Is.EqualTo(SiegeArea.CaptureObjective));
            Assert.That(bootstrap.Settlements.TryGetState(bootstrap.SettlementId, out SettlementSnapshot settlement), Is.True);
            Assert.That(settlement.OwnerId, Is.EqualTo(bootstrap.AttackerFactionId));
        }
    }
}
