using System.Collections;
using System.Linq;
using AegisRTS.Demo;
using AegisRTS.Gameplay.Objectives;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AegisRTS.Tests.PlayMode
{
    public sealed class ScenarioSandboxPlayModeTests
    {
        [UnityTest]
        public IEnumerator SandboxScenario_LoadsAndCompletesFourDataOnlyGameModes()
        {
            SceneManager.LoadScene("Sandbox_AI", LoadSceneMode.Single); yield return null;
            ScenarioSandboxBootstrap bootstrap = Object.FindAnyObjectByType<ScenarioSandboxBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.LoadedScenarioCount, Is.EqualTo(4));
            Assert.That(bootstrap.CompletedScenarioCount, Is.EqualTo(4));
            Assert.That(bootstrap.CompletedModes, Is.EquivalentTo(new[]
                { GameModeType.Conquest, GameModeType.Siege, GameModeType.Defense, GameModeType.Survival }));
            Assert.That(bootstrap.AcceptancePassed, Is.True);
            Assert.That(bootstrap.LastDebugSummary, Does.Contain("Victory"));
        }
    }
}
