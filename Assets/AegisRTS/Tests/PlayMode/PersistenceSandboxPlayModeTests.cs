using System.Collections;
using AegisRTS.Demo;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AegisRTS.Tests.PlayMode
{
    public sealed class PersistenceSandboxPlayModeTests
    {
        [UnityTest]
        public IEnumerator SandboxPersistence_RestoresBattleStateAndReplaysCommands()
        {
            SceneManager.LoadScene("Sandbox_AI", LoadSceneMode.Single); yield return null;
            PersistenceSandboxBootstrap bootstrap = Object.FindAnyObjectByType<PersistenceSandboxBootstrap>();
            Assert.That(bootstrap, Is.Not.Null); Assert.That(bootstrap.AcceptancePassed, Is.True);
            Assert.That(bootstrap.SaveReloadPassed, Is.True); Assert.That(bootstrap.ReplayPassed, Is.True); Assert.That(bootstrap.DebugConsolePassed, Is.True);
            Assert.That(bootstrap.ReplayCommandCount, Is.EqualTo(2)); Assert.That(bootstrap.DebugCommandCount, Is.EqualTo(2));
            Assert.That(bootstrap.SerializedCharacterCount, Is.GreaterThan(1000)); Assert.That(bootstrap.StateFingerprint, Has.Length.EqualTo(64));
        }
    }
}
