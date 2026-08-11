using System.Collections;
using AegisRTS.Demo;
using AegisRTS.Presentation.Camera;
using AegisRTS.Presentation.Input;
using AegisRTS.Presentation.Selection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AegisRTS.Tests.PlayMode
{
    public sealed class RtsSandboxPlayModeTests
    {
        [UnityTest]
        public IEnumerator SandboxRts_ComposesTwentyDebugUnitsSelectionInputAndCamera()
        {
            SceneManager.LoadScene("Sandbox_RTS", LoadSceneMode.Single);
            yield return null;

            RtsSandboxBootstrap bootstrap = Object.FindAnyObjectByType<RtsSandboxBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.DebugUnitCount, Is.EqualTo(20));
            Assert.That(Object.FindObjectsByType<UnitySelectableView>().Length, Is.GreaterThanOrEqualTo(20));
            Assert.That(Object.FindAnyObjectByType<UnityRtsInputAdapter>(), Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<RtsCameraController>(), Is.Not.Null);
        }
    }
}
