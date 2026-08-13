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
    /// <summary>Builds the isolated P035R1 review prefabs, scene, measurements, and captures.</summary>
    public static class InfantryPhase035Revision01ReviewBuilder
    {
        private const string Root = "Assets/AegisRTS/Review/InfantryPhase035Revision01";
        private const string AposeFbx = Root + "/SK_Infantry_A_v004_P035R1_Apose_Review.fbx";
        private const string L1PoseFbx = Root + "/SK_Infantry_A_v004_P035R1_L1Pose_Review.fbx";
        private const string AposePrefab = Root + "/PF_Unit_Infantry_P035R1_Review.prefab";
        private const string L1PosePrefab = Root + "/PF_Unit_Infantry_P035R1_L1Pose_Review.prefab";
        private const string ScenePath = Root + "/SCN_Infantry_P035R1_Review.unity";
        private const string OutputRelative = "docs/ArtProduction/ReviewPackages/Infantry_Phase03_5_Revision01_Review/Screenshots/Unity";
        private const string ManifestRelative = "docs/ArtProduction/ReviewPackages/Infantry_Phase03_5_Revision01_Review/Manifests/Unity_Capture_Result.json";
        private const string ExpectedP035Apose = "Assets/AegisRTS/Review/InfantryPhase035/SK_Infantry_A_v004_P035_Apose_Review.fbx";
        private const string ExpectedP035L1 = "Assets/AegisRTS/Review/InfantryPhase035/SK_Infantry_A_v004_P035_L1Pose_Review.fbx";

        public static void BuildAndCapture()
        {
            Directory.CreateDirectory(FullPath(Root));
            ConfigureImporter(AposeFbx);
            ConfigureImporter(L1PoseFbx);
            GameObject aposeAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AposeFbx);
            GameObject l1PoseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(L1PoseFbx);
            if (aposeAsset == null || l1PoseAsset == null)
                throw new FileNotFoundException("One or both P035R1 review FBXs were not imported.");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Dictionary<string, Material> materials = CreateMaterials();
            GameObject apose = (GameObject)PrefabUtility.InstantiatePrefab(aposeAsset);
            GameObject l1pose = (GameObject)PrefabUtility.InstantiatePrefab(l1PoseAsset);
            apose.name = "CHR_Infantry_A_v004_P035R1_Apose_Review";
            l1pose.name = "CHR_Infantry_A_v004_P035R1_L1Pose_Review";
            Renderer[] aposeRenderers = apose.GetComponentsInChildren<Renderer>(true);
            Renderer[] l1Renderers = l1pose.GetComponentsInChildren<Renderer>(true);
            ApplyMaterials(aposeRenderers, materials);
            ApplyMaterials(l1Renderers, materials);
            GroundByBoots(apose, aposeRenderers);
            GroundByBoots(l1pose, l1Renderers);
            Bounds aposeBounds = CalculateBounds(aposeRenderers);
            Bounds l1Bounds = CalculateBounds(l1Renderers);
            float aposeBootGround = CalculateBootGround(aposeRenderers);
            float l1BootGround = CalculateBootGround(l1Renderers);
            Validate(aposeBounds, aposeBootGround, "A-pose", 1.80f, 1.86f);
            // The review-only downward sword and longer arm enlarge posed
            // renderer bounds; the production source/body height remains the
            // A-pose value above. Keep this gate bounded but do not mistake
            // weapon reach for a character-height regression.
            Validate(l1Bounds, l1BootGround, "L1-pose", 1.80f, 2.02f);

            PrefabUtility.SaveAsPrefabAsset(apose, AposePrefab);
            PrefabUtility.SaveAsPrefabAsset(l1pose, L1PosePrefab);
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "ReviewGround";
            ground.transform.localScale = new Vector3(2.4f, 1f, 2.4f);
            ground.GetComponent<Renderer>().sharedMaterial = materials["Ground"];
            CreateLighting(out Camera camera);
            string output = FullPath(OutputRelative);
            Directory.CreateDirectory(output);

            l1pose.SetActive(false);
            // Prime shader/material state after synchronous FBX/material imports;
            // the first graphics-enabled batch render can otherwise capture
            // transient import-preview colors instead of the assigned review set.
            Capture(camera, new Vector3(0f, aposeBounds.size.y * .48f, 0f), 4.2f, 30f,
                Path.Combine(output, "Unity_Apose_Warmup.png"));
            Capture(camera, new Vector3(0f, aposeBounds.size.y * .48f, 0f), 4.2f, 30f,
                Path.Combine(output, "Unity_Apose_Close.png"));
            File.Delete(Path.Combine(output, "Unity_Apose_Warmup.png"));
            apose.SetActive(false);
            l1pose.SetActive(true);
            Capture(camera, new Vector3(0f, l1Bounds.size.y * .48f, 0f), 4.2f, 30f,
                Path.Combine(output, "Unity_L1Pose_Close.png"));
            Capture(camera, new Vector3(0f, l1Bounds.size.y * .48f, 0f), 7.5f, 35f,
                Path.Combine(output, "Unity_L1Pose_RTS_Normal.png"));

            apose.SetActive(true);
            l1pose.SetActive(true);
            apose.transform.position = new Vector3(-1.25f, 0f, 0f);
            l1pose.transform.position = new Vector3(1.25f, 0f, 0f);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();

            string json = "{\n" +
                "  \"status\": \"READY FOR PHASE03_5 REVISION REVIEW\",\n" +
                $"  \"unity_version\": \"{Application.unityVersion}\",\n" +
                $"  \"scene\": \"{ScenePath}\",\n" +
                $"  \"apose_prefab\": \"{AposePrefab}\",\n" +
                $"  \"l1pose_prefab\": \"{L1PosePrefab}\",\n" +
                $"  \"apose_height_m\": {aposeBounds.size.y:F6},\n" +
                $"  \"l1pose_height_m\": {l1Bounds.size.y:F6},\n" +
                $"  \"apose_boot_ground_y_m\": {aposeBootGround:F6},\n" +
                $"  \"l1pose_boot_ground_y_m\": {l1BootGround:F6},\n" +
                $"  \"l1pose_overall_min_y_m\": {l1Bounds.min.y:F6},\n" +
                $"  \"apose_renderers\": {aposeRenderers.Length},\n" +
                $"  \"l1pose_renderers\": {l1Renderers.Length},\n" +
                "  \"posed_arm_landmarks_source\": \"Measurements/3D_L1Pose_Arm_Landmarks_After.json\",\n" +
                "  \"camera\": \"Perspective, 35 degree FOV, 7.5 m RTS Normal\",\n" +
                $"  \"p035_baseline_assets_exist\": {ToJsonBool(AssetDatabase.LoadAssetAtPath<GameObject>(ExpectedP035Apose) != null && AssetDatabase.LoadAssetAtPath<GameObject>(ExpectedP035L1) != null)},\n" +
                "  \"runtime_prefab_replaced\": false,\n" +
                "  \"captures\": [\"Unity_Apose_Close.png\", \"Unity_L1Pose_Close.png\", \"Unity_L1Pose_RTS_Normal.png\"]\n" +
                "}\n";
            Directory.CreateDirectory(Path.GetDirectoryName(FullPath(ManifestRelative)) ?? output);
            File.WriteAllText(FullPath(ManifestRelative), json);
            Debug.Log($"[P035R1 Review] READY: A={aposeBounds.size.y:F6}m, L1={l1Bounds.size.y:F6}m, renderers={aposeRenderers.Length}/{l1Renderers.Length}.");
        }

        private static void ConfigureImporter(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new InvalidOperationException($"No ModelImporter for {path}");
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.SaveAndReimport();
        }

        private static Dictionary<string, Material> CreateMaterials()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var colors = new Dictionary<string, Color>
            {
                ["Cloth"] = new Color(.18f, .32f, .46f), ["Leather"] = new Color(.24f, .13f, .08f),
                ["Metal"] = new Color(.32f, .36f, .42f), ["Skin"] = new Color(.55f, .32f, .21f),
                ["Team"] = new Color(.12f, .38f, .67f), ["Wood"] = new Color(.28f, .16f, .09f),
                ["Ground"] = new Color(.12f, .14f, .17f)
            };
            var result = new Dictionary<string, Material>();
            foreach (KeyValuePair<string, Color> pair in colors)
            {
                string path = $"{Root}/MAT_P035R1_{pair.Key}.mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(shader) { name = $"MAT_P035R1_{pair.Key}" };
                    AssetDatabase.CreateAsset(material, path);
                }
                material.color = pair.Value;
                material.SetFloat("_Smoothness", pair.Key == "Metal" ? .55f : .15f);
                EditorUtility.SetDirty(material);
                result[pair.Key] = material;
            }
            return result;
        }

        private static void ApplyMaterials(IEnumerable<Renderer> renderers, IReadOnlyDictionary<string, Material> materials)
        {
            foreach (Renderer renderer in renderers)
            {
                Material[] assigned = renderer.sharedMaterials.Select(source =>
                {
                    string name = source == null ? string.Empty : source.name;
                    foreach (string key in new[] { "Cloth", "Leather", "Metal", "Skin", "Team", "Wood" })
                        if (name.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0) return materials[key];
                    return materials["Metal"];
                }).ToArray();
                if (assigned.Length == 0) assigned = new[] { materials["Metal"] };
                renderer.sharedMaterials = assigned;
            }
        }

        private static void GroundByBoots(GameObject root, Renderer[] renderers)
        {
            Renderer[] boots = renderers.Where(value => value.name.IndexOf("Boot", StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
            float minimum = (boots.Length > 0 ? boots : renderers).Min(value => value.bounds.min.y);
            root.transform.position += new Vector3(0f, -minimum, 0f);
        }

        private static float CalculateBootGround(IEnumerable<Renderer> renderers)
        {
            Renderer[] boots = renderers.Where(value => value.name.IndexOf("Boot", StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
            if (boots.Length == 0) throw new InvalidOperationException("Review model has no Boot renderer for ground validation.");
            return boots.Min(value => value.bounds.min.y);
        }

        private static void Validate(Bounds bounds, float bootGround, string label, float minimumHeight, float maximumHeight)
        {
            if (bounds.size.y < minimumHeight || bounds.size.y > maximumHeight)
                throw new InvalidOperationException($"{label} review height outside tolerance: {bounds.size.y:F6}");
            if (Mathf.Abs(bootGround) > .002f)
                throw new InvalidOperationException($"{label} boot ground mismatch: {bootGround:F6}");
        }

        private static void CreateLighting(out Camera camera)
        {
            GameObject cameraObject = new GameObject("ReviewCamera");
            camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.045f, .055f, .07f);
            camera.nearClipPlane = .05f;
            camera.farClipPlane = 50f;
            foreach ((string name, Vector3 rotation, float intensity, Color color) in new[]
            {
                ("Key", new Vector3(42f, -32f, 0f), 1.35f, new Color(1f, .91f, .80f)),
                ("Fill", new Vector3(38f, 145f, 0f), .70f, new Color(.68f, .82f, 1f)),
                ("Rim", new Vector3(55f, 205f, 0f), .90f, new Color(.75f, .86f, 1f))
            })
            {
                GameObject lightObject = new GameObject("Review" + name);
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = intensity;
                light.color = color;
                lightObject.transform.eulerAngles = rotation;
            }
        }

        private static Bounds CalculateBounds(IReadOnlyList<Renderer> renderers)
        {
            Bounds result = renderers[0].bounds;
            for (int i = 1; i < renderers.Count; i++) result.Encapsulate(renderers[i].bounds);
            return result;
        }

        private static void Capture(Camera camera, Vector3 target, float distance, float fov, string path)
        {
            Vector3 direction = new Vector3(.54f, .48f, -1f).normalized;
            camera.transform.position = target + direction * distance;
            camera.transform.LookAt(target);
            camera.fieldOfView = fov;
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

        private static string ToJsonBool(bool value) => value ? "true" : "false";

        private static string FullPath(string repositoryRelative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", repositoryRelative));
    }
}
