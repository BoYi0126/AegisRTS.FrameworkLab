using System.Collections;
using AegisRTS.Demo;
using AegisRTS.Gameplay.Armies;
using AegisRTS.Gameplay.Heroes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AegisRTS.Tests.PlayMode
{
    public sealed class ArmySandboxPlayModeTests
    {
        [UnityTest]
        public IEnumerator SandboxCombat_ComposesHeroAndTwentyInfantryArmy()
        {
            SceneManager.LoadScene("Sandbox_Combat", LoadSceneMode.Single);
            yield return null;

            ArmySandboxBootstrap bootstrap = Object.FindAnyObjectByType<ArmySandboxBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.ArmyVisualCount, Is.EqualTo(21));
            Assert.That(bootstrap.AcceptancePassed, Is.True);
            Assert.That(bootstrap.Armies.ArmyCount, Is.EqualTo(1));
            Assert.That(bootstrap.Heroes.HeroCount, Is.EqualTo(2));
            Assert.That(bootstrap.Armies.TryGetState(bootstrap.AcceptanceArmyId, out ArmySnapshot army), Is.True);
            Assert.That(army.UnitCount, Is.EqualTo(21));
            Assert.That(army.CommanderId, Is.EqualTo(bootstrap.AcceptanceCommanderId));
            Assert.That(army.MoraleEnabled, Is.True);
            Assert.That(army.SupplyEnabled, Is.True);
        }

        [UnityTest]
        public IEnumerator SandboxCombat_RunsSplitMergeCommanderAndOrderCommandFlow()
        {
            SceneManager.LoadScene("Sandbox_Combat", LoadSceneMode.Single);
            yield return null;
            ArmySandboxBootstrap bootstrap = Object.FindAnyObjectByType<ArmySandboxBootstrap>();

            Assert.That(bootstrap.CreatedEventCount, Is.EqualTo(1));
            Assert.That(bootstrap.SplitEventCount, Is.EqualTo(1));
            Assert.That(bootstrap.MergeEventCount, Is.EqualTo(1));
            Assert.That(bootstrap.CommanderEventCount, Is.EqualTo(3));
            Assert.That(bootstrap.OrderEventCount, Is.EqualTo(3));
            Assert.That(bootstrap.Commands.RegisteredHandlerCount, Is.EqualTo(9));
            Assert.That(bootstrap.Commands.RegisteredValidatorCount, Is.EqualTo(9));
            Assert.That(bootstrap.Heroes.TryGetState(bootstrap.AcceptanceCommanderId, out HeroSnapshot commander), Is.True);
            Assert.That(commander.ArmyId, Is.EqualTo(bootstrap.AcceptanceArmyId));
        }
    }
}
