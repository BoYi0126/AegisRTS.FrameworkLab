using System.Collections;
using AegisRTS.Demo;
using AegisRTS.Gameplay.AI;
using AegisRTS.Gameplay.Settlements;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AegisRTS.Tests.PlayMode
{
    public sealed class AiSandboxPlayModeTests
    {
        [UnityTest]
        public IEnumerator SandboxAi_AutonomouslyCompletesEconomyRecruitArmySiegeCapture()
        {
            SceneManager.LoadScene("Sandbox_AI", LoadSceneMode.Single); yield return new WaitForSeconds(3f);
            AiSandboxBootstrap bootstrap = Object.FindAnyObjectByType<AiSandboxBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.AcceptancePassed, Is.True);
            Assert.That(bootstrap.SpawnedUnitCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(bootstrap.ActionHistory, Does.Contain("DevelopEconomy"));
            Assert.That(bootstrap.ActionHistory, Does.Contain("Recruit"));
            Assert.That(bootstrap.ActionHistory, Does.Contain("AssembleArmy"));
            Assert.That(bootstrap.ActionHistory, Does.Contain("StartSiege"));
            Assert.That(bootstrap.ActionHistory, Does.Contain("Capture"));
        }

        [UnityTest]
        public IEnumerator SandboxAi_LongRunMaintainsCapturedTargetWithoutDeadlock()
        {
            SceneManager.LoadScene("Sandbox_AI", LoadSceneMode.Single); yield return new WaitForSeconds(5f);
            AiSandboxBootstrap bootstrap = Object.FindAnyObjectByType<AiSandboxBootstrap>();
            Assert.That(bootstrap.AcceptancePassed, Is.True);
            Assert.That(bootstrap.Settlements.TryGetState(new AegisRTS.Core.Entities.EntityId(10011), out SettlementSnapshot target), Is.True);
            Assert.That(target.OwnerId, Is.EqualTo(bootstrap.AiFactionId));
            Assert.That(bootstrap.AI.TryGetState(bootstrap.AiFactionId, out AiAgentSnapshot ai), Is.True);
            Assert.That(ai.StalledDecisionCount, Is.Zero, $"Action={ai.Action}; History={bootstrap.ActionHistory}");
            Assert.That(ai.Action, Is.EqualTo(AiActionType.HoldPosition));
        }
    }
}
