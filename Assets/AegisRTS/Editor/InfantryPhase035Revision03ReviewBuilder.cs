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
    /// <summary>Builds isolated P035R3 sword-attachment review assets and captures.</summary>
    public static class InfantryPhase035Revision03ReviewBuilder
    {
        private const string Root = "Assets/AegisRTS/Review/InfantryPhase035Revision03";
        private const string AposeFbx = Root + "/SK_Infantry_A_v004_P035R3_Apose_Review.fbx";
        private const string L1PoseFbx = Root + "/SK_Infantry_A_v004_P035R3_L1Pose_Review.fbx";
        private const string AposePrefab = Root + "/PF_Unit_Infantry_P035R3_Review.prefab";
        private const string L1PosePrefab = Root + "/PF_Unit_Infantry_P035R3_L1Pose_Review.prefab";
        private const string ScenePath = Root + "/SCN_Infantry_P035R3_Review.unity";
        private const string OutputRelative = "docs/ArtProduction/ReviewPackages/Infantry_Phase03_5_Revision03_Sword_Attachment_Review/Screenshots/Unity";
        private const string ManifestRelative = "docs/ArtProduction/ReviewPackages/Infantry_Phase03_5_Revision03_Sword_Attachment_Review/Manifests/Unity_Attachment_Result.json";
        private static readonly string[] SwordParts =
        {
            "GEO_Infantry_Sword_GripContact", "Sword", "Sword_Grip", "Sword_Guard",
            "Sword_Pommel", "GEO_Infantry_Sword_BladeSpine", "GEO_Infantry_Sword_GripWraps"
        };

        public static void BuildAndCapture()
        {
            Directory.CreateDirectory(FullPath(Root));
            ConfigureImporter(AposeFbx); ConfigureImporter(L1PoseFbx);
            GameObject aposeAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AposeFbx);
            GameObject l1Asset = AssetDatabase.LoadAssetAtPath<GameObject>(L1PoseFbx);
            if (aposeAsset == null || l1Asset == null) throw new FileNotFoundException("P035R3 review FBX import failed.");
            Avatar aposeAvatar = AssetDatabase.LoadAllAssetsAtPath(AposeFbx).OfType<Avatar>().FirstOrDefault();
            Avatar l1Avatar = AssetDatabase.LoadAllAssetsAtPath(L1PoseFbx).OfType<Avatar>().FirstOrDefault();
            ValidateAvatar(aposeAvatar, "A-pose"); ValidateAvatar(l1Avatar, "L1-pose");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Dictionary<string, Material> materials = CreateMaterials();
            GameObject apose = (GameObject)PrefabUtility.InstantiatePrefab(aposeAsset);
            GameObject l1pose = (GameObject)PrefabUtility.InstantiatePrefab(l1Asset);
            apose.name = "CHR_Infantry_A_v004_P035R3_Apose_Review";
            l1pose.name = "CHR_Infantry_A_v004_P035R3_L1Pose_Review";
            Renderer[] aposeRenderers = apose.GetComponentsInChildren<Renderer>(true);
            Renderer[] l1Renderers = l1pose.GetComponentsInChildren<Renderer>(true);
            ApplyMaterials(aposeRenderers, materials); ApplyMaterials(l1Renderers, materials);
            GroundByBoots(apose, aposeRenderers); GroundByBoots(l1pose, l1Renderers);
            Bounds aposeBounds = CalculateBounds(aposeRenderers); Bounds l1Bounds = CalculateBounds(l1Renderers);
            ValidateBounds(aposeBounds, CalculateBootGround(aposeRenderers), "A-pose", 1.80f, 1.86f);
            ValidateBounds(l1Bounds, CalculateBootGround(l1Renderers), "L1-pose", 1.80f, 1.96f);
            HierarchyResult aposeHierarchy = ValidateHierarchy(apose);
            HierarchyResult l1Hierarchy = ValidateHierarchy(l1pose);
            PrefabUtility.SaveAsPrefabAsset(apose, AposePrefab);
            PrefabUtility.SaveAsPrefabAsset(l1pose, L1PosePrefab);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "ReviewGround"; ground.transform.localScale = new Vector3(2.4f, 1f, 2.4f);
            ground.GetComponent<Renderer>().sharedMaterial = materials["Ground"];
            CreateLighting(out Camera camera);
            string output = FullPath(OutputRelative); Directory.CreateDirectory(output);

            l1pose.SetActive(false);
            Capture(camera, aposeBounds.center, 4.2f, 30f, Path.Combine(output, "Unity_Apose_Close.png"));
            Capture(camera, aposeHierarchy.SwordRoot.position, 1.35f, 28f, Path.Combine(output, "Unity_SwordGrip_Close.png"));
            apose.SetActive(false); l1pose.SetActive(true);
            Capture(camera, l1Bounds.center, 4.2f, 30f, Path.Combine(output, "Unity_L1Pose_Close.png"));
            Capture(camera, l1Bounds.center, 7.5f, 35f, Path.Combine(output, "Unity_L1Pose_RTS_Normal.png"));
            Capture(camera, l1Hierarchy.SwordRoot.position, 1.35f, 28f, Path.Combine(output, "Unity_L1Pose_SwordGrip_Close.png"));
            Quaternion neutral = l1Hierarchy.RightHand.localRotation;
            l1Hierarchy.RightHand.localRotation = neutral * Quaternion.Euler(0f, 15f, 0f);
            Capture(camera, l1Hierarchy.SwordRoot.position, 1.35f, 28f, Path.Combine(output, "Unity_SwordFollow_TestUp.png"));
            l1Hierarchy.RightHand.localRotation = neutral * Quaternion.Euler(0f, -15f, 0f);
            Capture(camera, l1Hierarchy.SwordRoot.position, 1.35f, 28f, Path.Combine(output, "Unity_SwordFollow_TestDown.png"));
            l1Hierarchy.RightHand.localRotation = neutral;

            apose.SetActive(true); l1pose.SetActive(true);
            apose.transform.position = new Vector3(-1.25f, 0f, 0f); l1pose.transform.position = new Vector3(1.25f, 0f, 0f);
            EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.SaveAssets();
            string json = "{\n" +
                "  \"status\": \"READY FOR PHASE03_5 REVISION03 REVIEW\",\n" +
                $"  \"unity_version\": \"{Application.unityVersion}\",\n" +
                $"  \"scene\": \"{ScenePath}\",\n  \"apose_prefab\": \"{AposePrefab}\",\n  \"l1pose_prefab\": \"{L1PosePrefab}\",\n" +
                $"  \"apose_humanoid_valid\": {Bool(aposeAvatar.isValid && aposeAvatar.isHuman)},\n  \"l1pose_humanoid_valid\": {Bool(l1Avatar.isValid && l1Avatar.isHuman)},\n" +
                $"  \"apose_height_m\": {aposeBounds.size.y:F6},\n  \"l1pose_height_m\": {l1Bounds.size.y:F6},\n" +
                $"  \"apose_renderers\": {aposeRenderers.Length},\n  \"l1pose_renderers\": {l1Renderers.Length},\n" +
                "  \"hierarchy\": \"RightHand/Socket_R_Hand/WPN_SwordRoot_R/[7 sword parts]\",\n" +
                $"  \"apose_hierarchy_valid\": {Bool(aposeHierarchy.Valid)},\n  \"l1pose_hierarchy_valid\": {Bool(l1Hierarchy.Valid)},\n" +
                "  \"right_hand_follow_test_degrees\": [15, -15],\n  \"runtime_prefab_replaced\": false,\n" +
                "  \"captures\": [\"Unity_Apose_Close.png\", \"Unity_SwordGrip_Close.png\", \"Unity_L1Pose_Close.png\", \"Unity_L1Pose_RTS_Normal.png\", \"Unity_L1Pose_SwordGrip_Close.png\", \"Unity_SwordFollow_TestUp.png\", \"Unity_SwordFollow_TestDown.png\"]\n}\n";
            Directory.CreateDirectory(Path.GetDirectoryName(FullPath(ManifestRelative)) ?? output);
            File.WriteAllText(FullPath(ManifestRelative), json);
            Debug.Log($"[P035R3 Review] READY: Humanoid={aposeAvatar.isValid && aposeAvatar.isHuman}/{l1Avatar.isValid && l1Avatar.isHuman}, hierarchy={aposeHierarchy.Valid}/{l1Hierarchy.Valid}.");
        }

        private static void ConfigureImporter(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new InvalidOperationException($"No ModelImporter for {path}");
            importer.globalScale = 1f; importer.useFileScale = true; importer.importAnimation = false;
            importer.importCameras = false; importer.importLights = false;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            HumanDescription description = importer.humanDescription;
            description.human = CreateHumanMapping();
            description.upperArmTwist = .5f; description.lowerArmTwist = .5f;
            description.upperLegTwist = .5f; description.lowerLegTwist = .5f;
            description.armStretch = .05f; description.legStretch = .05f;
            description.feetSpacing = 0f; description.hasTranslationDoF = false;
            importer.humanDescription = description;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.SaveAndReimport();
        }

        private static HumanBone[] CreateHumanMapping()
        {
            var mapping = new Dictionary<string, string>
            {
                {"Hips","Hips"}, {"Spine","Spine"}, {"Chest","Chest"}, {"UpperChest","UpperChest"}, {"Neck","Neck"}, {"Head","Head"},
                {"LeftShoulder","LeftShoulder"}, {"LeftUpperArm","LeftUpperArm"}, {"LeftLowerArm","LeftLowerArm"}, {"LeftHand","LeftHand"},
                {"RightShoulder","RightShoulder"}, {"RightUpperArm","RightUpperArm"}, {"RightLowerArm","RightLowerArm"}, {"RightHand","RightHand"},
                {"LeftUpperLeg","LeftUpperLeg"}, {"LeftLowerLeg","LeftLowerLeg"}, {"LeftFoot","LeftFoot"}, {"LeftToes","LeftToes"},
                {"RightUpperLeg","RightUpperLeg"}, {"RightLowerLeg","RightLowerLeg"}, {"RightFoot","RightFoot"}, {"RightToes","RightToes"}
            };
            return mapping.Select(pair => new HumanBone
            {
                humanName = pair.Key,
                boneName = pair.Value,
                limit = new HumanLimit { useDefaultValues = true }
            }).ToArray();
        }

        private static void ValidateAvatar(Avatar avatar, string label)
        {
            if (avatar == null || !avatar.isValid || !avatar.isHuman)
                throw new InvalidOperationException($"{label} Humanoid avatar is invalid.");
        }

        private static HierarchyResult ValidateHierarchy(GameObject root)
        {
            Transform rightHand = Find(root.transform, "RightHand");
            Transform socket = Find(root.transform, "Socket_R_Hand");
            Transform swordRoot = Find(root.transform, "WPN_SwordRoot_R");
            if (rightHand == null || socket == null || swordRoot == null) throw new InvalidOperationException("Required sword hierarchy node missing.");
            if (socket.parent != rightHand || swordRoot.parent != socket) throw new InvalidOperationException($"Sword hierarchy invalid: {socket.parent?.name}/{swordRoot.parent?.name}");
            foreach (string part in SwordParts)
            {
                Transform child = Find(root.transform, part);
                if (child == null || child.parent != swordRoot) throw new InvalidOperationException($"Sword part parent invalid: {part}");
            }
            if (swordRoot.localScale != Vector3.one || SwordParts.Select(name => Find(root.transform, name)).Any(value => value.localScale != Vector3.one))
                throw new InvalidOperationException("Sword hierarchy contains non-unit scale.");
            return new HierarchyResult(rightHand, socket, swordRoot, true);
        }

        private static Transform Find(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root) { Transform found = Find(child, name); if (found != null) return found; }
            return null;
        }

        private readonly struct HierarchyResult
        {
            public HierarchyResult(Transform rightHand, Transform socket, Transform swordRoot, bool valid) { RightHand = rightHand; Socket = socket; SwordRoot = swordRoot; Valid = valid; }
            public Transform RightHand { get; } public Transform Socket { get; } public Transform SwordRoot { get; } public bool Valid { get; }
        }

        private static Dictionary<string, Material> CreateMaterials()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var colors = new Dictionary<string, Color>{{"Cloth",new Color(.18f,.32f,.46f)},{"Leather",new Color(.24f,.13f,.08f)},{"Metal",new Color(.32f,.36f,.42f)},{"Skin",new Color(.55f,.32f,.21f)},{"Team",new Color(.12f,.38f,.67f)},{"Wood",new Color(.28f,.16f,.09f)},{"Ground",new Color(.12f,.14f,.17f)}};
            var result = new Dictionary<string, Material>();
            foreach (KeyValuePair<string, Color> pair in colors)
            {
                string path = $"{Root}/MAT_P035R3_{pair.Key}.mat"; Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) { material = new Material(shader) { name = $"MAT_P035R3_{pair.Key}" }; AssetDatabase.CreateAsset(material, path); }
                material.color = pair.Value; material.SetFloat("_Smoothness", pair.Key == "Metal" ? .55f : .15f); EditorUtility.SetDirty(material); result[pair.Key] = material;
            }
            return result;
        }

        private static void ApplyMaterials(IEnumerable<Renderer> renderers, IReadOnlyDictionary<string, Material> materials)
        {
            foreach (Renderer renderer in renderers)
            {
                Material[] assigned = renderer.sharedMaterials.Select(source => { string name = source == null ? string.Empty : source.name; foreach (string key in new[] { "Cloth", "Leather", "Metal", "Skin", "Team", "Wood" }) if (name.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0) return materials[key]; return materials["Metal"]; }).ToArray();
                if (assigned.Length == 0) assigned = new[] { materials["Metal"] }; renderer.sharedMaterials = assigned;
            }
        }

        private static void GroundByBoots(GameObject root, Renderer[] renderers) { Renderer[] boots = renderers.Where(value => value.name.IndexOf("Boot", StringComparison.OrdinalIgnoreCase) >= 0).ToArray(); float minimum = (boots.Length > 0 ? boots : renderers).Min(value => value.bounds.min.y); root.transform.position += new Vector3(0f, -minimum, 0f); }
        private static float CalculateBootGround(IEnumerable<Renderer> renderers) { Renderer[] boots = renderers.Where(value => value.name.IndexOf("Boot", StringComparison.OrdinalIgnoreCase) >= 0).ToArray(); if (boots.Length == 0) throw new InvalidOperationException("No Boot renderer."); return boots.Min(value => value.bounds.min.y); }
        private static void ValidateBounds(Bounds bounds, float bootGround, string label, float min, float max) { if (bounds.size.y < min || bounds.size.y > max) throw new InvalidOperationException($"{label} height outside tolerance: {bounds.size.y:F6}"); if (Mathf.Abs(bootGround) > .002f) throw new InvalidOperationException($"{label} boot ground mismatch: {bootGround:F6}"); }
        private static void CreateLighting(out Camera camera) { GameObject cameraObject = new GameObject("ReviewCamera"); camera = cameraObject.AddComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.045f,.055f,.07f); camera.nearClipPlane=.05f; camera.farClipPlane=50f; foreach ((string name, Vector3 rotation, float intensity, Color color) in new[]{("Key",new Vector3(42f,-32f,0f),1.35f,new Color(1f,.91f,.80f)),("Fill",new Vector3(38f,145f,0f),.70f,new Color(.68f,.82f,1f)),("Rim",new Vector3(55f,205f,0f),.90f,new Color(.75f,.86f,1f))}) { GameObject o=new GameObject("Review"+name); Light light=o.AddComponent<Light>(); light.type=LightType.Directional; light.intensity=intensity; light.color=color; o.transform.eulerAngles=rotation; } }
        private static Bounds CalculateBounds(IReadOnlyList<Renderer> renderers) { Bounds result=renderers[0].bounds; for(int i=1;i<renderers.Count;i++) result.Encapsulate(renderers[i].bounds); return result; }
        private static void Capture(Camera camera, Vector3 target, float distance, float fov, string path) { Vector3 direction=new Vector3(.54f,.48f,1f).normalized; camera.transform.position=target+direction*distance; camera.transform.LookAt(target); camera.fieldOfView=fov; var texture=new RenderTexture(960,540,24,RenderTextureFormat.ARGB32){antiAliasing=1}; RenderTexture previous=RenderTexture.active; try { camera.targetTexture=texture; camera.Render(); RenderTexture.active=texture; var image=new Texture2D(960,540,TextureFormat.RGBA32,false); try { image.ReadPixels(new Rect(0,0,960,540),0,0); image.Apply(); File.WriteAllBytes(path,image.EncodeToPNG()); } finally { UnityEngine.Object.DestroyImmediate(image); } } finally { camera.targetTexture=null; RenderTexture.active=previous; texture.Release(); UnityEngine.Object.DestroyImmediate(texture); } }
        private static string Bool(bool value) => value ? "true" : "false";
        private static string FullPath(string relative) => Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));
    }
}
