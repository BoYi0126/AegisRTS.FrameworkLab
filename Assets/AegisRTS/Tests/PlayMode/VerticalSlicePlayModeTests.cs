using System.Collections;
using AegisRTS.Demo;
using AegisRTS.Gameplay.VerticalSlice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AegisRTS.Tests.PlayMode
{
    public sealed class VerticalSlicePlayModeTests
    {
        [UnityTest]
        public IEnumerator VerticalSlice_ThreeKingdomsCompletesFullLoop()
        {
            SceneManager.LoadScene("VerticalSlice_01", LoadSceneMode.Single); yield return new WaitForSeconds(1.2f);
            VerticalSliceBootstrap bootstrap = Object.FindAnyObjectByType<VerticalSliceBootstrap>();
            Assert.That(bootstrap, Is.Not.Null); Assert.That(bootstrap.ActiveWorldId, Is.EqualTo("three-kingdoms"));
            Assert.That(bootstrap.AcceptancePassed, Is.True, bootstrap.Loop?.LastMessage);
            Assert.That(bootstrap.Simulation.AI.AgentCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator VerticalSlice_FantasyUsesSameRuntimeAndLifecycleControls()
        {
            SceneManager.LoadScene("VerticalSlice_01", LoadSceneMode.Single); yield return new WaitForSeconds(1.2f);
            VerticalSliceBootstrap bootstrap = Object.FindAnyObjectByType<VerticalSliceBootstrap>();
            Assert.That(bootstrap.SwitchWorldAndRestart(true), Is.True); yield return new WaitForSeconds(1.2f);
            Assert.That(bootstrap.ActiveWorldId, Is.EqualTo("fantasy")); Assert.That(bootstrap.AcceptancePassed, Is.True);
            bootstrap.Session.ReturnToMenu(); Assert.That(bootstrap.Session.LoadGame(), Is.True);
            Assert.That(bootstrap.Session.Pause(), Is.True); Assert.That(bootstrap.Session.OpenSettings(), Is.True);
            Assert.That(bootstrap.Session.ApplySettings(new GameSettings(0.8d, 24d, true)), Is.True);
            Assert.That(bootstrap.Session.Resume(), Is.True);
        }
    }
}
