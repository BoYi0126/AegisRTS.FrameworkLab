using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AegisRTS.Demo.PlayablePrototype;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace AegisRTS.Editor
{
    /// <summary>Configures the generated Humanoid FBXs and replaces the static L2 prefab with the animated L3 prefab.</summary>
    public static class InfantryL3PrefabBuilder
    {
        private const string Root = "Assets/AegisRTS/Content/Shared/Art/Units/Infantry";
        private const string MasterModel = Root + "/Models/SK_Infantry_A_v002.fbx";
        private const string AnimationFolder = Root + "/Animations";
        private const string BaseMaterialPath = Root + "/Materials/MAT_Infantry_Base.mat";
        private const string TeamMaterialPath = Root + "/Materials/MAT_Infantry_TeamColor.mat";
        private const string BaseColorTexturePath = Root + "/Textures/T_Infantry_A_BaseColor_1K.png";
        private const string NormalTexturePath = Root + "/Textures/T_Infantry_A_Normal_1K.png";
        private const string OrmTexturePath = Root + "/Textures/T_Infantry_A_ORM_1K.png";
        private const string TeamMaskTexturePath = Root + "/Textures/T_Infantry_A_TeamColorMask_1K.png";
        private const string ControllerPath = AnimationFolder + "/AC_Infantry.controller";
        private const string PrefabPath = Root + "/Resources/AegisRTS/Units/Infantry/PF_Unit_Infantry.prefab";

        private static readonly ClipSpec[] ClipSpecs =
        {
            new ClipSpec("AN_Infantry_Idle", true),
            new ClipSpec("AN_Infantry_Move", true,
                new AnimationEvent { functionName = "Footstep_L", time = 1f / 24f },
                new AnimationEvent { functionName = "Footstep_R", time = 13f / 24f }),
            new ClipSpec("AN_Infantry_Attack_A", false,
                new AnimationEvent { functionName = "AttackImpact", time = 13f / 30f }),
            new ClipSpec("AN_Infantry_Hit", false),
            new ClipSpec("AN_Infantry_Death", false,
                new AnimationEvent { functionName = "DeathSettled", time = 35f / 38f }),
        };

        [MenuItem("Tools/AegisRTS/Art/Rebuild Infantry L3 Prefab")]
        public static void Rebuild() => BuildAndValidate();

        public static void BuildAndValidate()
        {
            ConfigureTextureImporters();
            ConfigureRuntimeMaterials();
            ConfigureMasterImporter();
            Avatar avatar = LoadAvatar(MasterModel);
            if (avatar == null || !avatar.isValid || !avatar.isHuman)
                throw new InvalidOperationException("[Infantry L3] Unity did not create a valid Humanoid Avatar from the master FBX.");

            var clips = new Dictionary<string, AnimationClip>(StringComparer.Ordinal);
            foreach (ClipSpec spec in ClipSpecs)
            {
                string path = $"{AnimationFolder}/{spec.Name}.fbx";
                ConfigureAnimationImporter(path, avatar, spec);
                AnimationClip clip = LoadClip(path, spec.Name);
                if (clip == null) throw new InvalidOperationException($"[Infantry L3] Missing imported clip {spec.Name}.");
                clips.Add(spec.Name, clip);
            }

            AnimatorController controller = CreateController(clips);
            BuildPrefab(controller, avatar, clips);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Infantry L3] PASS: Humanoid={avatar.isHuman}/{avatar.isValid}, clips={clips.Count}, prefab={PrefabPath}.");
        }

        private static void ConfigureTextureImporters()
        {
            ConfigureTextureImporter(BaseColorTexturePath, TextureImporterType.Default, true);
            ConfigureTextureImporter(NormalTexturePath, TextureImporterType.NormalMap, false);
            ConfigureTextureImporter(OrmTexturePath, TextureImporterType.Default, false);
            ConfigureTextureImporter(TeamMaskTexturePath, TextureImporterType.Default, false);
        }

        private static void ConfigureTextureImporter(string path, TextureImporterType type, bool sRgb)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new FileNotFoundException("Infantry texture was not imported.", path);
            importer.textureType = type;
            importer.sRGBTexture = sRgb;
            importer.mipmapEnabled = true;
            importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void ConfigureRuntimeMaterials()
        {
            Material baseMaterial = AssetDatabase.LoadAssetAtPath<Material>(BaseMaterialPath);
            Material teamMaterial = AssetDatabase.LoadAssetAtPath<Material>(TeamMaterialPath);
            Texture2D baseColor = AssetDatabase.LoadAssetAtPath<Texture2D>(BaseColorTexturePath);
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(NormalTexturePath);
            if (baseMaterial == null || teamMaterial == null || baseColor == null || normal == null)
                throw new InvalidOperationException("[Infantry L3] Runtime materials or required textures are missing.");

            baseMaterial.SetTexture("_BaseMap", baseColor);
            baseMaterial.SetColor("_BaseColor", Color.white);
            baseMaterial.SetTexture("_BumpMap", normal);
            baseMaterial.SetFloat("_BumpScale", 1f);
            baseMaterial.EnableKeyword("_NORMALMAP");
            baseMaterial.SetFloat("_Metallic", 0f);
            baseMaterial.SetFloat("_Smoothness", 0.2f);
            baseMaterial.enableInstancing = true;

            teamMaterial.SetTexture("_BaseMap", null);
            teamMaterial.SetColor("_BaseColor", Color.white);
            teamMaterial.SetFloat("_Metallic", 0f);
            teamMaterial.SetFloat("_Smoothness", 0.18f);
            teamMaterial.enableInstancing = true;
            EditorUtility.SetDirty(baseMaterial);
            EditorUtility.SetDirty(teamMaterial);
        }

        private static void ConfigureMasterImporter()
        {
            var importer = AssetImporter.GetAtPath(MasterModel) as ModelImporter;
            if (importer == null) throw new FileNotFoundException("Master infantry FBX was not imported.", MasterModel);
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importConstraints = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.optimizeGameObjects = false;
            importer.isReadable = false;
            importer.SaveAndReimport();
            LogImportMessages(MasterModel);
        }

        private static void ConfigureAnimationImporter(string path, Avatar avatar, ClipSpec spec)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new FileNotFoundException("Animation FBX was not imported.", path);
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = avatar;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importConstraints = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.optimizeGameObjects = false;

            ModelImporterClipAnimation[] defaults = importer.defaultClipAnimations;
            if (defaults == null || defaults.Length == 0)
                throw new InvalidOperationException($"[Infantry L3] {spec.Name} FBX contains no default animation take.");
            ModelImporterClipAnimation clip = defaults.FirstOrDefault(value => value.name == spec.Name) ?? defaults[0];
            clip.name = spec.Name;
            clip.loopTime = spec.Loop;
            clip.loopPose = spec.Loop;
            clip.keepOriginalOrientation = true;
            clip.keepOriginalPositionY = true;
            clip.keepOriginalPositionXZ = true;
            clip.lockRootRotation = true;
            clip.lockRootHeightY = true;
            clip.lockRootPositionXZ = true;
            // ModelImporter serializes AnimationEvent.time as normalized clip time (0..1),
            // unlike the runtime AnimationEvent API where time is expressed in seconds.
            clip.events = spec.Events;
            importer.clipAnimations = new[] { clip };
            importer.SaveAndReimport();
            LogImportMessages(path);
        }

        private static void LogImportMessages(string path)
        {
            var importLog = AssetImporter.GetImportLog(path);
            if (importLog == null || importLog.logEntries == null) return;
            foreach (var entry in importLog.logEntries)
                Debug.LogWarning($"[Infantry L3 Import] {path}: {entry.flags}: {entry.message}");
        }

        private static Avatar LoadAvatar(string path) =>
            AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault();

        private static AnimationClip LoadClip(string path, string expectedName) =>
            AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                .FirstOrDefault(value => !value.name.StartsWith("__preview__", StringComparison.Ordinal) &&
                                         (value.name == expectedName || !value.legacy));

        private static AnimatorController CreateController(IReadOnlyDictionary<string, AnimationClip> clips)
        {
            AssetDatabase.DeleteAsset(ControllerPath);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveRate", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState idle = machine.AddState("Idle"); idle.motion = clips["AN_Infantry_Idle"];
            AnimatorState move = machine.AddState("Move"); move.motion = clips["AN_Infantry_Move"];
            move.speedParameterActive = true;
            move.speedParameter = "MoveRate";
            AnimatorState attack = machine.AddState("Attack"); attack.motion = clips["AN_Infantry_Attack_A"];
            AnimatorState hit = machine.AddState("Hit"); hit.motion = clips["AN_Infantry_Hit"];
            AnimatorState death = machine.AddState("Death"); death.motion = clips["AN_Infantry_Death"];
            machine.defaultState = idle;

            AddConditionTransition(idle, move, AnimatorConditionMode.Greater, 0.1f, "Speed", false);
            AddConditionTransition(move, idle, AnimatorConditionMode.Less, 0.1f, "Speed", false);
            AddTriggerTransition(machine, attack, "Attack", 0.05f);
            AddTriggerTransition(machine, hit, "Hit", 0.04f);
            AddTriggerTransition(machine, death, "Die", 0.02f);
            AddExitTransition(attack, idle, 0.08f);
            AddExitTransition(hit, idle, 0.06f);
            return controller;
        }

        private static void AddConditionTransition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode,
            float threshold, string parameter, bool hasExitTime)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = hasExitTime;
            transition.duration = 0.12f;
            transition.AddCondition(mode, threshold, parameter);
        }

        private static void AddTriggerTransition(AnimatorStateMachine machine, AnimatorState to, string trigger, float duration)
        {
            AnimatorStateTransition transition = machine.AddAnyStateTransition(to);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
        }

        private static void AddExitTransition(AnimatorState from, AnimatorState to, float duration)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = 0.95f;
            transition.duration = duration;
        }

        private static void BuildPrefab(AnimatorController controller, Avatar avatar,
            IReadOnlyDictionary<string, AnimationClip> clips)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(MasterModel);
            Material baseMaterial = AssetDatabase.LoadAssetAtPath<Material>(BaseMaterialPath);
            Material teamMaterial = AssetDatabase.LoadAssetAtPath<Material>(TeamMaterialPath);
            if (source == null || baseMaterial == null || teamMaterial == null)
                throw new InvalidOperationException("[Infantry L3] Master model or runtime materials are missing.");

            var root = new GameObject(PrototypeUnitArtCatalog.InfantryPrefabId);
            try
            {
                GameObject visual = UnityEngine.Object.Instantiate(source, root.transform, false);
                visual.name = "VisualRoot";
                // Blender source is authored Z-up/-Y-forward. The exported FBX retains that source basis,
                // so convert the complete visual hierarchy once at the prefab boundary to Unity Y-up/Z-forward.
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                visual.transform.localScale = Vector3.one;

                Animator animator = visual.GetComponent<Animator>() ?? visual.AddComponent<Animator>();
                animator.avatar = avatar;
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                var animatorView = visual.AddComponent<PrototypeUnitAnimatorView>();
                animatorView.Configure(animator, (float)clips["AN_Infantry_Death"].length);

                Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
                ConfigureMaterials(renderers, baseMaterial, teamMaterial);
                AlignVisualToGround(visual.transform, renderers);
                ValidateUnityOrientation(renderers);
                Renderer[] lod0 = FindLod(renderers, "LOD0");
                Renderer[] lod1 = FindLod(renderers, "LOD1");
                Renderer[] lod2 = FindLod(renderers, "LOD2");
                if (lod0.Length == 0 || lod1.Length == 0 || lod2.Length == 0)
                    throw new InvalidOperationException($"[Infantry L3] Missing LOD renderers: {lod0.Length}/{lod1.Length}/{lod2.Length}.");
                foreach (SkinnedMeshRenderer skin in renderers.OfType<SkinnedMeshRenderer>())
                    skin.updateWhenOffscreen = false;

                LODGroup lodGroup = visual.GetComponent<LODGroup>() ?? visual.AddComponent<LODGroup>();
                lodGroup.SetLODs(new[]
                {
                    new LOD(0.04f, lod0),
                    new LOD(0.012f, lod1),
                    new LOD(0.003f, lod2),
                });
                lodGroup.fadeMode = LODFadeMode.CrossFade;
                lodGroup.animateCrossFading = true;
                lodGroup.RecalculateBounds();

                Transform selection = CreateAnchor(root.transform, "SelectionAnchor", 0.02f);
                Transform health = CreateAnchor(root.transform, "HealthBarAnchor", 2.10f);
                CreateAnchor(root.transform, "GroundContact", 0f);
                var collider = root.AddComponent<CapsuleCollider>();
                collider.radius = 0.38f;
                collider.height = 1.8f;
                collider.center = new Vector3(0f, 0.9f, 0f);

                Renderer[] teamRenderers = renderers.Where(value =>
                    value.name.IndexOf("_Team", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.name.IndexOf("Shield", StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                if (teamRenderers.Length < 3)
                    throw new InvalidOperationException("[Infantry L3] Team Color renderers were not found for all LODs.");
                var artView = root.AddComponent<PrototypeUnitArtView>();
                artView.Configure(selection, health, teamRenderers, animatorView);

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                ValidatePrefab(lod0, lod1, lod2, clips, teamRenderers);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void ConfigureMaterials(IEnumerable<Renderer> renderers, Material baseMaterial, Material teamMaterial)
        {
            foreach (Renderer renderer in renderers)
            {
                int subMeshes = renderer is SkinnedMeshRenderer skin && skin.sharedMesh != null
                    ? skin.sharedMesh.subMeshCount
                    : renderer.GetComponent<MeshFilter>()?.sharedMesh?.subMeshCount ?? 1;
                if (renderer.name.IndexOf("_Team", StringComparison.OrdinalIgnoreCase) >= 0)
                    renderer.sharedMaterials = Enumerable.Repeat(teamMaterial, subMeshes).ToArray();
                else if (renderer.name.IndexOf("Shield", StringComparison.OrdinalIgnoreCase) >= 0 && subMeshes > 1)
                {
                    var materials = Enumerable.Repeat(baseMaterial, subMeshes).ToArray();
                    materials[materials.Length - 1] = teamMaterial;
                    renderer.sharedMaterials = materials;
                }
                else renderer.sharedMaterials = Enumerable.Repeat(baseMaterial, subMeshes).ToArray();
            }
        }

        private static void ValidateUnityOrientation(IReadOnlyList<Renderer> renderers)
        {
            if (renderers.Count == 0) throw new InvalidOperationException("[Infantry L3] No renderers were imported.");
            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Count; index++) bounds.Encapsulate(renderers[index].bounds);
            if (bounds.size.y < 1.65f || bounds.size.y <= bounds.size.z)
                throw new InvalidOperationException(
                    $"[Infantry L3] Invalid Unity orientation: bounds={bounds.size}. Character height must be on Y, not Z.");
            if (bounds.min.y < -0.08f || bounds.max.y > 1.95f)
                throw new InvalidOperationException(
                    $"[Infantry L3] Invalid vertical bounds: minY={bounds.min.y:F3}, maxY={bounds.max.y:F3}.");
        }

        private static void AlignVisualToGround(Transform visual, IReadOnlyList<Renderer> renderers)
        {
            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Count; index++) bounds.Encapsulate(renderers[index].bounds);
            visual.localPosition += Vector3.up * -bounds.min.y;
        }

        private static Renderer[] FindLod(IEnumerable<Renderer> renderers, string lod) => renderers
            .Where(value => value.name.IndexOf(lod, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();

        private static Transform CreateAnchor(Transform parent, string name, float localY)
        {
            Transform value = new GameObject(name).transform;
            value.SetParent(parent, false);
            value.localPosition = new Vector3(0f, localY, 0f);
            return value;
        }

        private static void ValidatePrefab(Renderer[] lod0, Renderer[] lod1, Renderer[] lod2,
            IReadOnlyDictionary<string, AnimationClip> clips, Renderer[] teamRenderers)
        {
            int[] expectedTriangles = { 4376, 1512, 542 };
            Renderer[][] levels = { lod0, lod1, lod2 };
            for (int index = 0; index < levels.Length; index++)
            {
                int triangles = levels[index].Sum(CountTriangles);
                if (triangles != expectedTriangles[index])
                    throw new InvalidOperationException($"[Infantry L3] LOD{index} has {triangles} triangles, expected {expectedTriangles[index]}.");
            }
            if (clips["AN_Infantry_Attack_A"].events.All(value => value.functionName != "AttackImpact"))
                throw new InvalidOperationException("[Infantry L3] AttackImpact Animation Event is missing.");
            if (teamRenderers.Length < 6)
                Debug.LogWarning($"[Infantry L3] Only {teamRenderers.Length} team/Shield renderers were found; verify distant team color manually.");
        }

        private static int CountTriangles(Renderer renderer)
        {
            Mesh mesh = renderer is SkinnedMeshRenderer skin ? skin.sharedMesh : renderer.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh == null) return 0;
            int count = 0;
            for (int index = 0; index < mesh.subMeshCount; index++) count += (int)mesh.GetIndexCount(index) / 3;
            return count;
        }

        private sealed class ClipSpec
        {
            public ClipSpec(string name, bool loop, params AnimationEvent[] events)
            {
                Name = name;
                Loop = loop;
                Events = events ?? Array.Empty<AnimationEvent>();
            }
            public string Name { get; }
            public bool Loop { get; }
            public AnimationEvent[] Events { get; }
        }
    }
}
