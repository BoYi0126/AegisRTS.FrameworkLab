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
    /// <summary>Imports the generated Archer L3 delivery and builds its unit and projectile prefabs.</summary>
    public static class ArcherL3PrefabBuilder
    {
        private const string Root = "Assets/AegisRTS/Content/Shared/Art/Units/Archer";
        private const string MasterModel = Root + "/Models/SK_Archer_A_v001.fbx";
        private const string ArrowModel = Root + "/Models/PRJ_Arrow_Basic_v001.fbx";
        private const string AnimationFolder = Root + "/Animations";
        private const string MaterialFolder = Root + "/Materials";
        private const string BaseMaterialPath = MaterialFolder + "/MAT_Archer_Base.mat";
        private const string TeamMaterialPath = MaterialFolder + "/MAT_Archer_TeamColor.mat";
        private const string ArrowMaterialPath = MaterialFolder + "/MAT_Arrow_Base.mat";
        private const string ControllerPath = AnimationFolder + "/AC_Archer.controller";
        private const string PrefabPath = Root + "/Resources/AegisRTS/Units/Archer/PF_Unit_Archer.prefab";
        private const string ArrowPrefabPath = Root + "/Resources/AegisRTS/Projectiles/PRJ_Arrow_Basic_v001.prefab";

        private static readonly ClipSpec[] ClipSpecs =
        {
            new ClipSpec("AN_Archer_Idle", true),
            new ClipSpec("AN_Archer_Move", true,
                new AnimationEvent { functionName = "Footstep_L", time = 1f / 30f },
                new AnimationEvent { functionName = "Footstep_R", time = 13f / 30f }),
            new ClipSpec("AN_Archer_Attack_Ranged", false,
                new AnimationEvent { functionName = "ProjectileRelease", time = 22f / 30f }),
            new ClipSpec("AN_Archer_Hit", false),
            new ClipSpec("AN_Archer_Death", false,
                new AnimationEvent { functionName = "DeathSettled", time = 35f / 30f }),
        };

        [MenuItem("Tools/AegisRTS/Art/Rebuild Archer L3 Prefabs")]
        public static void Rebuild() => BuildAndValidate();

        public static void BuildAndValidate()
        {
            CreateMaterials();
            ConfigureModelImporter(MasterModel, true);
            ConfigureModelImporter(ArrowModel, false);
            Avatar avatar = LoadAvatar(MasterModel);
            if (avatar == null || !avatar.isValid || !avatar.isHuman)
                throw new InvalidOperationException("[Archer L3] Master FBX did not produce a valid Humanoid Avatar.");

            var clips = new Dictionary<string, AnimationClip>(StringComparer.Ordinal);
            foreach (ClipSpec spec in ClipSpecs)
            {
                string path = $"{AnimationFolder}/{spec.Name}.fbx";
                ConfigureAnimationImporter(path, avatar, spec);
                AnimationClip clip = LoadClip(path, spec.Name);
                if (clip == null) throw new InvalidOperationException($"[Archer L3] Missing clip {spec.Name}.");
                clips.Add(spec.Name, clip);
            }

            AnimatorController controller = CreateController(clips);
            BuildUnitPrefab(controller, avatar, clips);
            BuildArrowPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Archer L3] PASS: Humanoid={avatar.isHuman}/{avatar.isValid}, clips={clips.Count}, unit={PrefabPath}, arrow={ArrowPrefabPath}.");
        }

        private static void CreateMaterials()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null) throw new InvalidOperationException("[Archer L3] No supported lit shader was found.");
            SaveMaterial(BaseMaterialPath, shader, new Color(0.28f, 0.20f, 0.12f), 0.16f);
            SaveMaterial(TeamMaterialPath, shader, Color.white, 0.12f);
            SaveMaterial(ArrowMaterialPath, shader, new Color(0.30f, 0.17f, 0.07f), 0.08f);
        }

        private static void SaveMaterial(string path, Shader shader, Color color, float smoothness)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.name = Path.GetFileNameWithoutExtension(path);
            material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
        }

        private static void ConfigureModelImporter(string path, bool humanoid)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new FileNotFoundException("Archer FBX was not imported.", path);
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.importAnimation = humanoid;
            importer.animationType = humanoid ? ModelImporterAnimationType.Human : ModelImporterAnimationType.None;
            if (humanoid) importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importConstraints = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.optimizeGameObjects = false;
            importer.isReadable = false;
            importer.SaveAndReimport();
            LogImportMessages(path);
        }

        private static void ConfigureAnimationImporter(string path, Avatar avatar, ClipSpec spec)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new FileNotFoundException("Archer animation FBX was not imported.", path);
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
                throw new InvalidOperationException($"[Archer L3] {spec.Name} contains no animation take.");
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
            clip.events = spec.Events;
            importer.clipAnimations = new[] { clip };
            importer.SaveAndReimport();
            LogImportMessages(path);
        }

        private static AnimatorController CreateController(IReadOnlyDictionary<string, AnimationClip> clips)
        {
            AssetDatabase.DeleteAsset(ControllerPath);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveRate", AnimatorControllerParameterType.Float);
            controller.AddParameter(new AnimatorControllerParameter
            {
                name = "AttackRate",
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 1f,
            });
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState idle = AddState(machine, "Idle", clips["AN_Archer_Idle"]);
            AnimatorState move = AddState(machine, "Move", clips["AN_Archer_Move"]);
            move.speedParameterActive = true;
            move.speedParameter = "MoveRate";
            AnimatorState attack = AddState(machine, "Attack", clips["AN_Archer_Attack_Ranged"]);
            attack.speedParameterActive = true;
            attack.speedParameter = "AttackRate";
            AnimatorState hit = AddState(machine, "Hit", clips["AN_Archer_Hit"]);
            AnimatorState death = AddState(machine, "Death", clips["AN_Archer_Death"]);
            machine.defaultState = idle;
            AddCondition(idle, move, AnimatorConditionMode.Greater, 0.1f, "Speed");
            AddCondition(move, idle, AnimatorConditionMode.Less, 0.1f, "Speed");
            AddTrigger(machine, attack, "Attack", 0.05f);
            AddTrigger(machine, hit, "Hit", 0.04f);
            AddTrigger(machine, death, "Die", 0.02f);
            AddExit(attack, idle, 0.08f);
            AddExit(hit, idle, 0.06f);
            return controller;
        }

        private static AnimatorState AddState(AnimatorStateMachine machine, string name, Motion motion)
        {
            AnimatorState state = machine.AddState(name);
            state.motion = motion;
            return state;
        }

        private static void AddCondition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float threshold, string parameter)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.12f;
            transition.AddCondition(mode, threshold, parameter);
        }

        private static void AddTrigger(AnimatorStateMachine machine, AnimatorState target, string trigger, float duration)
        {
            AnimatorStateTransition transition = machine.AddAnyStateTransition(target);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
        }

        private static void AddExit(AnimatorState from, AnimatorState to, float duration)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = 0.95f;
            transition.duration = duration;
        }

        private static void BuildUnitPrefab(AnimatorController controller, Avatar avatar,
            IReadOnlyDictionary<string, AnimationClip> clips)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(MasterModel);
            Material baseMaterial = AssetDatabase.LoadAssetAtPath<Material>(BaseMaterialPath);
            Material teamMaterial = AssetDatabase.LoadAssetAtPath<Material>(TeamMaterialPath);
            if (source == null || baseMaterial == null || teamMaterial == null)
                throw new InvalidOperationException("[Archer L3] Required model or material is missing.");
            var root = new GameObject(PrototypeUnitArtCatalog.ArcherPrefabId);
            try
            {
                GameObject visual = UnityEngine.Object.Instantiate(source, root.transform, false);
                visual.name = "VisualRoot";
                // This Blender 5 export is converted to Unity Y-up by the FBX importer.
                visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                visual.transform.localScale = Vector3.one;
                Animator animator = visual.GetComponent<Animator>() ?? visual.AddComponent<Animator>();
                animator.avatar = avatar;
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                var animatorView = visual.AddComponent<PrototypeUnitAnimatorView>();
                animatorView.Configure(animator, clips["AN_Archer_Death"].length, 22f / 30f, 0.06f);
                Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
                ConfigureMaterials(renderers, baseMaterial, teamMaterial);
                AlignToGround(visual.transform, renderers);
                ValidateCharacterBounds(renderers);
                Renderer[] lod0 = FindLod(renderers, "LOD0");
                Renderer[] lod1 = FindLod(renderers, "LOD1");
                Renderer[] lod2 = FindLod(renderers, "LOD2");
                if (lod0.Length == 0 || lod1.Length == 0 || lod2.Length == 0)
                    throw new InvalidOperationException($"[Archer L3] Missing LODs: {lod0.Length}/{lod1.Length}/{lod2.Length}.");
                foreach (SkinnedMeshRenderer skin in renderers.OfType<SkinnedMeshRenderer>()) skin.updateWhenOffscreen = false;
                LODGroup lodGroup = visual.GetComponent<LODGroup>() ?? visual.AddComponent<LODGroup>();
                lodGroup.SetLODs(new[] { new LOD(0.04f, lod0), new LOD(0.012f, lod1), new LOD(0.003f, lod2) });
                lodGroup.fadeMode = LODFadeMode.CrossFade;
                lodGroup.animateCrossFading = true;
                lodGroup.RecalculateBounds();
                Transform selection = CreateAnchor(root.transform, "SelectionAnchor", 0.02f);
                Transform health = CreateAnchor(root.transform, "HealthBarAnchor", 2.10f);
                CreateAnchor(root.transform, "GroundContact", 0f);
                Transform projectile = visual.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(value => value.name == "Socket_Projectile");
                if (projectile == null) throw new InvalidOperationException("[Archer L3] Socket_Projectile is missing.");
                var collider = root.AddComponent<CapsuleCollider>();
                collider.radius = 0.38f;
                collider.height = 1.8f;
                collider.center = new Vector3(0f, 0.9f, 0f);
                Renderer[] teamRenderers = renderers.Where(IsTeamRenderer).ToArray();
                if (teamRenderers.Length < 3) throw new InvalidOperationException("[Archer L3] Team Color renderers are missing.");
                var artView = root.AddComponent<PrototypeUnitArtView>();
                artView.Configure(selection, health, teamRenderers, animatorView, projectile);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                ValidateUnit(lod0, lod1, lod2, clips, renderers);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static void BuildArrowPrefab()
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(ArrowModel);
            Material arrowMaterial = AssetDatabase.LoadAssetAtPath<Material>(ArrowMaterialPath);
            Material teamMaterial = AssetDatabase.LoadAssetAtPath<Material>(TeamMaterialPath);
            if (source == null || arrowMaterial == null || teamMaterial == null)
                throw new InvalidOperationException("[Archer L3] Arrow model or materials are missing.");
            var root = new GameObject("PRJ_Arrow_Basic_v001");
            try
            {
                GameObject visual = UnityEngine.Object.Instantiate(source, root.transform, false);
                visual.name = "VisualRoot";
                visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                visual.transform.localScale = Vector3.one;
                Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    Material material = renderer.name.IndexOf("Fletching", StringComparison.OrdinalIgnoreCase) >= 0
                        ? teamMaterial : arrowMaterial;
                    int count = GetMesh(renderer)?.subMeshCount ?? 1;
                    renderer.sharedMaterials = Enumerable.Repeat(material, count).ToArray();
                }
                if (root.GetComponentInChildren<Collider>() != null)
                    throw new InvalidOperationException("[Archer L3] Presentation projectile must not contain a collider.");
                Bounds bounds = GetLocalBounds(root.transform, renderers);
                visual.transform.localPosition -= bounds.center;
                bounds = GetLocalBounds(root.transform, renderers);
                if (bounds.size.z < 0.75f || bounds.size.z > 0.90f || bounds.size.z <= bounds.size.x || bounds.size.z <= bounds.size.y)
                    throw new InvalidOperationException($"[Archer L3] Arrow must be 0.75-0.90m and point along local Z+. bounds={bounds.size}.");
                if (bounds.center.magnitude > 0.04f)
                    throw new InvalidOperationException($"[Archer L3] Arrow pivot is not centered. center={bounds.center}.");
                PrefabUtility.SaveAsPrefabAsset(root, ArrowPrefabPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static void ConfigureMaterials(IEnumerable<Renderer> renderers, Material baseMaterial, Material teamMaterial)
        {
            foreach (Renderer renderer in renderers)
            {
                Material material = IsTeamRenderer(renderer) ? teamMaterial : baseMaterial;
                int count = GetMesh(renderer)?.subMeshCount ?? 1;
                renderer.sharedMaterials = Enumerable.Repeat(material, count).ToArray();
            }
        }

        private static bool IsTeamRenderer(Renderer renderer) =>
            renderer.name.IndexOf("_Team", StringComparison.OrdinalIgnoreCase) >= 0 ||
            renderer.name.IndexOf("Team_", StringComparison.OrdinalIgnoreCase) >= 0 ||
            renderer.name.IndexOf("Fletching", StringComparison.OrdinalIgnoreCase) >= 0;

        private static Renderer[] FindLod(IEnumerable<Renderer> renderers, string lod) => renderers
            .Where(value => value.name.IndexOf(lod, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();

        private static void AlignToGround(Transform visual, IReadOnlyList<Renderer> renderers)
        {
            Bounds bounds = Encapsulate(renderers);
            visual.localPosition += Vector3.up * -bounds.min.y;
        }

        private static void ValidateCharacterBounds(IReadOnlyList<Renderer> renderers)
        {
            Bounds bounds = Encapsulate(renderers);
            if (bounds.size.y < 1.65f || bounds.size.y > 2.15f || bounds.size.y <= bounds.size.z)
                throw new InvalidOperationException($"[Archer L3] Invalid Unity character bounds={bounds.size}.");
        }

        private static void ValidateUnit(Renderer[] lod0, Renderer[] lod1, Renderer[] lod2,
            IReadOnlyDictionary<string, AnimationClip> clips, IReadOnlyList<Renderer> renderers)
        {
            int[] expected = { 3344, 1280, 542 };
            Renderer[][] levels = { lod0, lod1, lod2 };
            for (int index = 0; index < levels.Length; index++)
            {
                int triangles = levels[index].Sum(value => CountTriangles(GetMesh(value)));
                if (triangles != expected[index])
                    throw new InvalidOperationException($"[Archer L3] LOD{index} has {triangles} triangles; expected {expected[index]}.");
            }
            if (clips["AN_Archer_Attack_Ranged"].events.All(value => value.functionName != "ProjectileRelease"))
                throw new InvalidOperationException("[Archer L3] ProjectileRelease event is missing.");
            if (renderers.Any(value => value.name.IndexOf("Shield", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                       value.name.IndexOf("Sword", StringComparison.OrdinalIgnoreCase) >= 0))
                throw new InvalidOperationException("[Archer L3] Infantry shield/sword geometry remains in the archer prefab.");
            if (!renderers.Any(value => value.name.IndexOf("Bow", StringComparison.OrdinalIgnoreCase) >= 0) ||
                !renderers.Any(value => value.name.IndexOf("Quiver", StringComparison.OrdinalIgnoreCase) >= 0))
                throw new InvalidOperationException("[Archer L3] Bow or quiver is missing.");
        }

        private static Transform CreateAnchor(Transform parent, string name, float y)
        {
            Transform value = new GameObject(name).transform;
            value.SetParent(parent, false);
            value.localPosition = new Vector3(0f, y, 0f);
            return value;
        }

        private static Bounds Encapsulate(IReadOnlyList<Renderer> renderers)
        {
            if (renderers.Count == 0) throw new InvalidOperationException("[Archer L3] No renderers were imported.");
            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Count; index++) bounds.Encapsulate(renderers[index].bounds);
            return bounds;
        }

        private static Bounds GetLocalBounds(Transform root, IReadOnlyList<Renderer> renderers)
        {
            if (renderers.Count == 0) throw new InvalidOperationException("[Archer L3] Arrow has no renderers.");
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (Renderer renderer in renderers)
            {
                Bounds world = renderer.bounds;
                for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 point = root.InverseTransformPoint(world.center + Vector3.Scale(world.extents, new Vector3(x, y, z)));
                    min = Vector3.Min(min, point);
                    max = Vector3.Max(max, point);
                }
            }
            var bounds = new Bounds((min + max) * 0.5f, max - min);
            return bounds;
        }

        private static Mesh GetMesh(Renderer renderer) => renderer is SkinnedMeshRenderer skin
            ? skin.sharedMesh : renderer.GetComponent<MeshFilter>()?.sharedMesh;

        private static int CountTriangles(Mesh mesh)
        {
            if (mesh == null) return 0;
            int count = 0;
            for (int index = 0; index < mesh.subMeshCount; index++) count += (int)mesh.GetIndexCount(index) / 3;
            return count;
        }

        private static Avatar LoadAvatar(string path) => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault();
        private static AnimationClip LoadClip(string path, string expectedName) => AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<AnimationClip>().FirstOrDefault(value => !value.name.StartsWith("__preview__", StringComparison.Ordinal) &&
                                                           (value.name == expectedName || !value.legacy));

        private static void LogImportMessages(string path)
        {
            var importLog = AssetImporter.GetImportLog(path);
            if (importLog?.logEntries == null) return;
            foreach (var entry in importLog.logEntries)
                Debug.LogWarning($"[Archer L3 Import] {path}: {entry.flags}: {entry.message}");
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
