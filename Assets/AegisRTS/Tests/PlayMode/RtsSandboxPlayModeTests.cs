using System.Collections;
using System.Collections.Generic;
using AegisRTS.Demo;
using AegisRTS.Gameplay.Movement;
using AegisRTS.Gameplay.Units;
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
        public IEnumerator SandboxRts_ComposesFiftyDebugUnitsSelectionInputCameraAndNavigation()
        {
            SceneManager.LoadScene("Sandbox_RTS", LoadSceneMode.Single);
            yield return null;

            RtsSandboxBootstrap bootstrap = Object.FindAnyObjectByType<RtsSandboxBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.DebugUnitCount, Is.EqualTo(50));
            Assert.That(bootstrap.NavigationReady, Is.True);
            Assert.That(bootstrap.Movement.RegisteredUnitCount, Is.EqualTo(50));
            Assert.That(bootstrap.Navigation.RegisteredAgentCount, Is.EqualTo(50));
            Assert.That(Object.FindObjectsByType<UnitySelectableView>().Length, Is.GreaterThanOrEqualTo(50));
            Assert.That(Object.FindAnyObjectByType<UnityRtsInputAdapter>(), Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<RtsCameraController>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator SandboxRts_FiftyUnitsReceiveDistinctPathsAcrossObstacle()
        {
            SceneManager.LoadScene("Sandbox_RTS", LoadSceneMode.Single);
            yield return null;
            RtsSandboxBootstrap bootstrap = Object.FindAnyObjectByType<RtsSandboxBootstrap>();

            Assert.That(bootstrap.IssueAcceptanceMove(new WorldPoint(25, 0, -8)).WasHandled, Is.True);
            yield return null;

            var destinations = new HashSet<WorldPoint>();
            foreach (MovementStateSnapshot state in bootstrap.Movement.Snapshot())
            {
                Assert.That(state.Status, Is.EqualTo(MovementStatus.Moving));
                destinations.Add(state.Destination);
            }
            Assert.That(destinations.Count, Is.EqualTo(50), "Every unit needs a distinct formation slot.");

            yield return new WaitForSeconds(15f);
            int crossedObstacle = 0;
            int permanentlyBlocked = 0;
            foreach (MovementStateSnapshot state in bootstrap.Movement.Snapshot())
            {
                if (state.Position.X > 3f) crossedObstacle++;
                if (state.Status == MovementStatus.Stuck || state.Status == MovementStatus.Unreachable) permanentlyBlocked++;
            }

            Assert.That(crossedObstacle, Is.GreaterThanOrEqualTo(40));
            Assert.That(permanentlyBlocked, Is.LessThanOrEqualTo(5));
        }
    }
}
