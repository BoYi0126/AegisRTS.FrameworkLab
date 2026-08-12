using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AegisRTS.Demo.PlayablePrototype;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AegisRTS.Editor
{
    public static class PlayablePrototypeSceneBuilder
    {
        public const string ScenePath = "Assets/AegisRTS/Demo/PlayablePrototype/PlayablePrototype_01.unity";
        public const string MaterialPath = "Assets/AegisRTS/Demo/PlayablePrototype/PrototypeUnlit.mat";

        [MenuItem("AegisRTS/Playable Prototype/Rebuild Scene")]
        public static void RebuildScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("PlayablePrototype_01");
            PlayablePrototypeBootstrap bootstrap = root.AddComponent<PlayablePrototypeBootstrap>();
            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("contentPack").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/AegisRTS/Content/PrototypeNeutral/ContentPack.json");
            serialized.FindProperty("scenario").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/AegisRTS/Content/PrototypeNeutral/Scenario.json");
            serialized.FindProperty("theme").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/AegisRTS/Content/PrototypeNeutral/Theme.json");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) throw new InvalidOperationException("URP Unlit shader is unavailable.");
                material = new Material(shader) { name = "Prototype Unlit" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            serialized.FindProperty("prototypeMaterial").objectReferenceValue = material;
            serialized.FindProperty("startImmediately").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(ScenePath, true),
            };
            scenes.AddRange(EditorBuildSettings.scenes.Where(value =>
                !string.Equals(value.path, ScenePath, StringComparison.Ordinal)));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log($"[PlayablePrototype] Scene rebuilt at {ScenePath}.");
        }

        public static void BuildWindowsDevelopment()
        {
            RebuildScene();
            string directory = Environment.GetEnvironmentVariable("AEGIS_PP_BUILD_DIR");
            if (string.IsNullOrWhiteSpace(directory))
                directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "AegisRTS.BuildValidation"));
            Directory.CreateDirectory(directory);
            string executable = Path.Combine(directory, "PlayablePrototype_01.exe");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = executable,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging,
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Windows Development Build failed: {report.summary.result}.");
            Debug.Log($"[PlayablePrototype] Windows Development Build PASS: {executable}, {report.summary.totalSize} bytes.");
        }
    }
}
