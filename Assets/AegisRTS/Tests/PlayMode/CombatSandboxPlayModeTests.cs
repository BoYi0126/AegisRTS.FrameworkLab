using System.Collections;
using AegisRTS.Demo;
using AegisRTS.Presentation.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AegisRTS.Tests.PlayMode
{
    public sealed class CombatSandboxPlayModeTests
    {
        [UnityTest]
        public IEnumerator SandboxCombat_ComposesCombatantsAndRunsAcceptanceScenario()
        {
            SceneManager.LoadScene("Sandbox_Combat", LoadSceneMode.Single);
            yield return null;

            CombatSandboxBootstrap bootstrap = Object.FindAnyObjectByType<CombatSandboxBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.AcceptanceScenarioStarted, Is.True);
            Assert.That(bootstrap.SpawnedCombatantCount, Is.EqualTo(6));
            Assert.That(bootstrap.Combat.CombatantCount, Is.EqualTo(6));
            Assert.That(Object.FindObjectsByType<UnityCombatView>().Length, Is.EqualTo(6));
        }

        [UnityTest]
        public IEnumerator SandboxCombat_ProducesProjectileSplashStatusDamageAndDeath()
        {
            SceneManager.LoadScene("Sandbox_Combat", LoadSceneMode.Single);
            yield return null;
            CombatSandboxBootstrap bootstrap = Object.FindAnyObjectByType<CombatSandboxBootstrap>();

            yield return new WaitForSeconds(4f);

            Assert.That(bootstrap.Driver.ProjectileVisualCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(bootstrap.Driver.DamageEventCount, Is.GreaterThanOrEqualTo(5));
            Assert.That(bootstrap.Driver.StatusEventCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(bootstrap.Driver.DeathEventCount, Is.GreaterThanOrEqualTo(1));
        }
    }
}
