using System;
using System.IO;
using System.Linq;
using AegisRTS.Demo.PlayablePrototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AegisRTS.Editor
{
    /// <summary>
    /// Runs a lightweight visual/runtime validation in the already-open Editor.
    /// Create the request file at the repository root, then refresh Unity.
    /// </summary>
    [InitializeOnLoad]
    public static class PlayablePrototypeSmokeValidation
    {
        private const string RunningKey = "AegisRTS.PlayableSmoke.Running";
        private const string OpenedSceneKey = "AegisRTS.PlayableSmoke.OpenedScene";
        private const string PreviousScenePathKey = "AegisRTS.PlayableSmoke.PreviousScenePath";
        private static int _framesRemaining;
        private static bool _captureRequested;
        private static bool _failed;

        static PlayablePrototypeSmokeValidation()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            if (SessionState.GetBool(RunningKey, false) && EditorApplication.isPlaying)
                StartRuntimeValidation();
        }

        [MenuItem("AegisRTS/Playable Prototype/Run Infantry Smoke Validation")]
        public static void RunFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[PlayablePrototype Smoke] Wait until Play Mode has stopped before starting validation.");
                return;
            }

            Begin();
        }

        private static void Begin()
        {
            Scene previous = SceneManager.GetActiveScene();
            SessionState.SetString(PreviousScenePathKey, previous.path ?? string.Empty);
            Scene prototype = SceneManager.GetSceneByPath(PlayablePrototypeSceneBuilder.ScenePath);
            bool openedScene = !prototype.IsValid() || !prototype.isLoaded;
            if (openedScene)
                prototype = EditorSceneManager.OpenScene(PlayablePrototypeSceneBuilder.ScenePath, OpenSceneMode.Additive);

            SessionState.SetBool(OpenedSceneKey, openedScene);
            SessionState.SetBool(RunningKey, true);
            _failed = false;
            SceneManager.SetActiveScene(prototype);
            Debug.Log("[PlayablePrototype Smoke] Starting Play Mode validation.");
            EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(RunningKey, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
                StartRuntimeValidation();
            else if (state == PlayModeStateChange.EnteredEditMode)
                RestoreEditorScene();
        }

        private static void StartRuntimeValidation()
        {
            _framesRemaining = 12;
            _captureRequested = false;
            EditorApplication.update -= TickRuntimeValidation;
            EditorApplication.update += TickRuntimeValidation;
        }

        private static void TickRuntimeValidation()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.update -= TickRuntimeValidation;
                return;
            }

            if (_framesRemaining-- > 0) return;
            if (!_captureRequested)
            {
                try
                {
                    ValidateAndCapture();
                    _captureRequested = true;
                    _framesRemaining = 12;
                    return;
                }
                catch (Exception exception)
                {
                    _failed = true;
                    Debug.LogError($"[PlayablePrototype Smoke] FAIL: {exception}");
                }
            }

            EditorApplication.update -= TickRuntimeValidation;
            EditorApplication.ExitPlaymode();
        }

        private static void ValidateAndCapture()
        {
            PlayablePrototypeBootstrap bootstrap = UnityEngine.Object.FindAnyObjectByType<PlayablePrototypeBootstrap>();
            Require(bootstrap != null, "Bootstrap was not created.");
            Require(bootstrap.BootSucceeded, bootstrap.LastUiMessage);
            bootstrap.DismissTutorialNow();

            PrototypeUnitArtView[] unitArt = UnityEngine.Object.FindObjectsByType<PrototypeUnitArtView>(FindObjectsInactive.Exclude);
            PrototypeUnitArtView[] archers = unitArt.Where(value => value.GetComponentsInChildren<Renderer>(true)
                .Any(renderer => renderer.name.IndexOf("Bow", StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();
            PrototypeUnitArtView[] infantry = unitArt.Except(archers).ToArray();
            Require(infantry.Length == 2, $"Expected 2 infantry art instances, found {infantry.Length}.");
            Require(archers.Length == 2, $"Expected 2 archer art instances, found {archers.Length}.");
            Require(infantry.All(value => value.TeamColorRenderers.Length >= 3), "Each infantry must expose team-color renderers across all LOD levels.");
            Require(infantry.All(value => value.GetComponent<LODGroup>() == null && value.GetComponentInChildren<LODGroup>() != null),
                "Each infantry must contain an LODGroup below its gameplay root.");
            Require(infantry.All(value => value.SelectionAnchor != null && value.HealthBarAnchor != null),
                "Selection and health-bar anchors are required.");
            Require(infantry.All(value => value.AnimatorView != null && value.AnimatorView.Animator != null),
                "Each infantry must expose an L3 Animator bridge.");
            Require(infantry.All(value => value.AnimatorView.Animator.avatar != null &&
                                          value.AnimatorView.Animator.avatar.isHuman &&
                                          value.AnimatorView.Animator.avatar.isValid),
                "Each infantry must use a valid Humanoid Avatar.");
            Require(archers.All(value => value.ProjectileSocket != null),
                "Each archer must expose Socket_Projectile.");
            Require(archers.All(value => value.TeamColorRenderers.Length >= 3 && value.AnimatorView != null &&
                                          value.AnimatorView.Animator.avatar != null &&
                                          value.AnimatorView.Animator.avatar.isHuman &&
                                          value.AnimatorView.Animator.avatar.isValid),
                "Each archer must expose LOD team color and a valid Humanoid Animator.");
            Require(bootstrap.ProjectileVisuals != null,
                "The event-driven projectile presentation must be composed.");

            string outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "AegisRTS.BuildValidation"));
            Directory.CreateDirectory(outputDirectory);
            string screenshot = Path.Combine(outputDirectory, "Units_GameView.png");
            ScreenCapture.CaptureScreenshot(screenshot, 1);
            Debug.Log($"[PlayablePrototype Smoke] PASS: 2 infantry + 2 archer instances, Humanoid/LOD/team colors/anchors/projectile socket present. Screenshot: {screenshot}");
        }

        private static void RestoreEditorScene()
        {
            EditorApplication.update -= TickRuntimeValidation;
            Scene prototype = SceneManager.GetSceneByPath(PlayablePrototypeSceneBuilder.ScenePath);
            string previousPath = SessionState.GetString(PreviousScenePathKey, string.Empty);
            Scene previous = string.IsNullOrEmpty(previousPath)
                ? FindLoadedSceneOtherThan(prototype)
                : SceneManager.GetSceneByPath(previousPath);
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            if (SessionState.GetBool(OpenedSceneKey, false) && prototype.IsValid() && prototype.isLoaded)
                EditorSceneManager.CloseScene(prototype, true);

            SessionState.SetBool(RunningKey, false);
            SessionState.EraseBool(OpenedSceneKey);
            SessionState.EraseString(PreviousScenePathKey);
            Debug.Log("[PlayablePrototype Smoke] Editor scene restored.");
            if (Application.isBatchMode) EditorApplication.Exit(_failed ? 1 : 0);
        }

        private static Scene FindLoadedSceneOtherThan(Scene excluded)
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene candidate = SceneManager.GetSceneAt(index);
                if (candidate.IsValid() && candidate.isLoaded && candidate != excluded) return candidate;
            }
            return default;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
