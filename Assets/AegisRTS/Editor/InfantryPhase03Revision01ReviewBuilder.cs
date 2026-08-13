using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AegisRTS.Editor
{
    /// <summary>Builds an isolated, review-only P03R1 prefab/scene and captures neutral RTS views.</summary>
    public static class InfantryPhase03Revision01ReviewBuilder
    {
        private const string Root = "Assets/AegisRTS/Review/InfantryPhase03Revision01";
        private const string FbxPath = Root + "/SK_Infantry_A_v004_P03R1_Review.fbx";
        private const string PrefabPath = Root + "/PF_Unit_Infantry_v004_P03R1_Review.prefab";
        private const string ScenePath = Root + "/SCN_Infantry_P03R1_Review.unity";
        private const string OutputRelative = "docs/ArtProduction/ReviewPackages/Infantry_Phase03_Revision01_Review/Screenshots/Unity";
        private const string ManifestRelative = "docs/ArtProduction/ReviewPackages/Infantry_Phase03_Revision01_Review/Manifests/Unity_Capture_Result.json";

        public static void BuildAndCapture()
        {
            Directory.CreateDirectory(Root);
            ConfigureImporter();
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            if (model == null) throw new FileNotFoundException("Review FBX was not imported.", FbxPath);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = (GameObject)PrefabUtility.InstantiatePrefab(model);
            root.name = "CHR_Infantry_A_v004_P03R1_Review";
            Dictionary<string, Material> idMaterials = CreateReviewMaterials();
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException("Review FBX has no renderers.");

            ApplyIdMaterials(renderers, idMaterials);
            Bounds initial = CalculateBounds(renderers);
            root.transform.position += new Vector3(-initial.center.x, -initial.min.y, -initial.center.z);
            Bounds grounded = CalculateBounds(renderers);
            if (grounded.size.y < 1.80f || grounded.size.y > 1.85f)
                throw new InvalidOperationException($"Unexpected imported review height {grounded.size.y:F6} m.");
            if (Mathf.Abs(grounded.min.y) > 0.002f)
                throw new InvalidOperationException($"Review model is not grounded: minY={grounded.min.y:F6}.");

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "ReviewGround";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(3f, 1f, 3f);
            ground.GetComponent<Renderer>().sharedMaterial = idMaterials["Ground"];

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.42f, 0.45f, 0.50f);
            GameObject lightObject = new GameObject("ReviewDirectionalLight");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.94f, 0.86f);
            lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            GameObject cameraObject = new GameObject("ReviewCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.075f, 0.085f, 0.105f);
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 100f;
            camera.allowHDR = true;

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            EditorSceneManager.SaveScene(scene, ScenePath);

            string output = FullPath(OutputRelative);
            Directory.CreateDirectory(output);
            Material clay = idMaterials["Clay"];
            Dictionary<Renderer, Material[]> original = renderers.ToDictionary(r => r, r => r.sharedMaterials);
            foreach (Renderer renderer in renderers)
                renderer.sharedMaterials = Enumerable.Repeat(clay, Math.Max(1, renderer.sharedMaterials.Length)).ToArray();

            Vector3 target = new Vector3(0f, grounded.size.y * 0.48f, 0f);
            Capture(camera, target, 4.2f, 30f, Path.Combine(output, "Unity_Close.png"));
            Capture(camera, target, 7.5f, 35f, Path.Combine(output, "Unity_RTS_Normal.png"));
            Capture(camera, target, 12f, 38f, Path.Combine(output, "Unity_Far.png"));

            foreach (KeyValuePair<Renderer, Material[]> pair in original) pair.Key.sharedMaterials = pair.Value;
            Capture(camera, target, 7.5f, 35f, Path.Combine(output, "Unity_MaterialID_RTS_Normal.png"));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();

            string[] captures = Directory.GetFiles(output, "Unity_*.png").OrderBy(path => path).ToArray();
            if (captures.Length != 4 || captures.Any(path => new FileInfo(path).Length == 0))
                throw new InvalidOperationException("Unity review capture set is incomplete.");

            string json = "{\n" +
                $"  \"status\": \"READY FOR PHASE03 REVISION REVIEW\",\n" +
                $"  \"unity_version\": \"{Application.unityVersion}\",\n" +
                $"  \"scene\": \"{ScenePath}\",\n" +
                $"  \"review_prefab\": \"{PrefabPath}\",\n" +
                $"  \"fbx\": \"{FbxPath}\",\n" +
                $"  \"material\": \"Neutral Clay plus preview Material ID\",\n" +
                $"  \"camera\": \"Perspective, 35 degree FOV at 7.5 m for RTS Normal\",\n" +
                $"  \"imported_height_m\": {grounded.size.y:F6},\n" +
                $"  \"ground_min_y_m\": {grounded.min.y:F6},\n" +
                $"  \"renderer_count\": {renderers.Length},\n" +
                "  \"runtime_prefab_replaced\": false,\n" +
                "  \"captures\": [\"Unity_Close.png\", \"Unity_RTS_Normal.png\", \"Unity_Far.png\", \"Unity_MaterialID_RTS_Normal.png\"]\n" +
                "}\n";
            File.WriteAllText(FullPath(ManifestRelative), json);
            Debug.Log($"[P03R1 Review] READY: height={grounded.size.y:F6}, renderers={renderers.Length}, scene={ScenePath}, captures={captures.Length}.");
        }

        private static void ConfigureImporter()
        {
            AssetDatabase.ImportAsset(FbxPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            ModelImporter importer = AssetImporter.GetAtPath(FbxPath) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Review FBX does not have a ModelImporter.");
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.SaveAndReimport();
        }

        private static Dictionary<string, Material> CreateReviewMaterials()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null) throw new InvalidOperationException("No review-compatible shader found.");
            var specs = new Dictionary<string, Color>
            {
                ["Metal"] = new Color(0.28f, 0.34f, 0.42f), ["Wood"] = new Color(0.34f, 0.17f, 0.075f),
                ["Leather"] = new Color(0.12f, 0.065f, 0.04f), ["Cloth"] = new Color(0.24f, 0.28f, 0.32f),
                ["Skin"] = new Color(0.55f, 0.30f, 0.21f), ["Team"] = new Color(0.58f, 0.055f, 0.045f),
                ["Clay"] = new Color(0.48f, 0.52f, 0.58f), ["Ground"] = new Color(0.18f, 0.20f, 0.24f),
            };
            var result = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, Color> spec in specs)
            {
                string path = $"{Root}/MAT_P03R1_{spec.Key}.mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(shader) { name = $"MAT_P03R1_{spec.Key}" };
                    AssetDatabase.CreateAsset(material, path);
                }
                material.color = spec.Value;
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", spec.Value);
                if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", spec.Key == "Metal" ? 0.58f : 0.18f);
                if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", spec.Key == "Metal" ? 0.72f : 0f);
                EditorUtility.SetDirty(material);
                result[spec.Key] = material;
            }
            return result;
        }

        private static void ApplyIdMaterials(IEnumerable<Renderer> renderers, IReadOnlyDictionary<string, Material> materials)
        {
            foreach (Renderer renderer in renderers)
            {
                Material[] source = renderer.sharedMaterials;
                renderer.sharedMaterials = source.Select(material =>
                {
                    string name = material == null ? string.Empty : material.name;
                    foreach (string key in new[] { "Metal", "Wood", "Leather", "Cloth", "Skin", "Team" })
                        if (name.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0) return materials[key];
                    return materials["Clay"];
                }).ToArray();
            }
        }

        private static Bounds CalculateBounds(IReadOnlyList<Renderer> renderers)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Count; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static void Capture(Camera camera, Vector3 target, float distance, float fov, string path)
        {
            Vector3 direction = new Vector3(0.54f, 0.48f, -1f).normalized;
            camera.transform.position = target + direction * distance;
            camera.transform.LookAt(target);
            camera.fieldOfView = fov;
            // A single-sample target is intentionally used for reliable headless/batch review capture.
            var texture = new RenderTexture(960, 540, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
            RenderTexture previous = RenderTexture.active;
            try
            {
                camera.targetTexture = texture;
                camera.Render();
                RenderTexture.active = texture;
                var image = new Texture2D(960, 540, TextureFormat.RGBA32, false);
                try
                {
                    image.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
                    image.Apply();
                    File.WriteAllBytes(path, image.EncodeToPNG());
                }
                finally { UnityEngine.Object.DestroyImmediate(image); }
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                texture.Release();
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static string FullPath(string repositoryRelative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", repositoryRelative));
    }
}
