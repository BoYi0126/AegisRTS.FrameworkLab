using System.Collections;
using AegisRTS.Demo;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AegisRTS.Tests.PlayMode
{
    public sealed class EconomyProductionSandboxPlayModeTests
    {
        [UnityTest]
        public IEnumerator SandboxSiege_CompletesBuildResearchRecruitPipeline()
        {
            SceneManager.LoadScene("Sandbox_Siege", LoadSceneMode.Single);
            yield return new WaitForSeconds(1f);
            EconomyProductionSandboxBootstrap bootstrap = Object.FindAnyObjectByType<EconomyProductionSandboxBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.CompletedBuildings, Is.EqualTo(1));
            Assert.That(bootstrap.CompletedTechnologies, Is.EqualTo(1));
            Assert.That(bootstrap.SpawnedUnits, Is.EqualTo(1));
            Assert.That(bootstrap.AcceptancePassed, Is.True);
        }
    }
}
