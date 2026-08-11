using System.Collections;
using AegisRTS.Demo;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AegisRTS.Tests.PlayMode
{
    public sealed class HudSandboxPlayModeTests
    {
        [UnityTest]
        public IEnumerator SandboxHud_SwapsThreeThemesWithoutGameplayMutation()
        {
            SceneManager.LoadScene("Sandbox_AI", LoadSceneMode.Single); yield return null;
            HudSandboxBootstrap bootstrap = Object.FindAnyObjectByType<HudSandboxBootstrap>();
            Assert.That(bootstrap, Is.Not.Null); Assert.That(bootstrap.AcceptancePassed, Is.True);
            Assert.That(bootstrap.LoadedThemeCount, Is.EqualTo(3));
            Assert.That(bootstrap.Presenter.PanelIds.Count, Is.EqualTo(10));
            Assert.That(bootstrap.Presenter.ThemeIds.Count, Is.EqualTo(3));
            Assert.That(bootstrap.CommandCount, Is.Zero);
            Assert.That(bootstrap.GameplayRevision, Is.EqualTo(12));
        }
    }
}
