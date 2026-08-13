using System;
using System.Collections.Generic;
using System.IO;
using AegisRTS.Demo.PlayablePrototype;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AegisRTS.Editor
{
    /// <summary>Builds the static L2 infantry runtime prefab from imported glTF LOD sources.</summary>
    [InitializeOnLoad]
    public static class InfantryArtPrefabBuilder
    {
        private const string Root = "Assets/AegisRTS/Content/Shared/Art/Units/Infantry";
        private const string Lod0Source = Root + "/Models/CHR_Infantry_A_v001_LOD0.glb";
        private const string Lod1Source = Root + "/Models/CHR_Infantry_A_v001_LOD1.glb";
        private const string BaseColorTexture = Root + "/Textures/T_Infantry_A_BaseColor_1K.png";
        private const string BaseMaterialPath = Root + "/Materials/MAT_Infantry_Base.mat";
        private const string TeamMaterialPath = Root + "/Materials/MAT_Infantry_TeamColor.mat";
        private const string MeshFolder = Root + "/Meshes";
        private const string PrefabPath = Root + "/Resources/AegisRTS/Units/Infantry/PF_Unit_Infantry.prefab";
        private static int _remainingAutomaticAttempts = 8;

        static InfantryArtPrefabBuilder()
        {
            EditorApplication.delayCall += BuildAutomaticallyWhenReady;
        }

        [MenuItem("Tools/AegisRTS/Art/Rebuild Infantry L2 Prefab")]
        public static void Rebuild()
        {
            Build(true);
        }

        private static void BuildAutomaticallyWhenReady()
        {
            if (Application.isPlaying || AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null) return;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Lod0Source) == null ||
                AssetDatabase.LoadAssetAtPath<GameObject>(Lod1Source) == null)
            {
                if (_remainingAutomaticAttempts-- > 0) EditorApplication.delayCall += BuildAutomaticallyWhenReady;
                return;
            }

            Build(false);
        }

        private static void Build(bool replaceExisting)
        {
            GameObject lod0Source = AssetDatabase.LoadAssetAtPath<GameObject>(Lod0Source);
            GameObject lod1Source = AssetDatabase.LoadAssetAtPath<GameObject>(Lod1Source);
            if (lod0Source == null || lod1Source == null)
            {
                Debug.LogError("[Infantry Art] LOD0/LOD1 glTF assets are not imported yet.");
                return;
            }

            EnsureFolder(MeshFolder);
            EnsureFolder(Path.GetDirectoryName(PrefabPath)?.Replace('\\', '/'));
            Material baseMaterial = CreateOrUpdateBaseMaterial();
            Material teamMaterial = CreateOrUpdateTeamMaterial();
            if (baseMaterial == null || teamMaterial == null) return;

            string[] meshPaths =
            {
                MeshFolder + "/SM_Infantry_A_LOD0_Base.asset",
                MeshFolder + "/SM_Infantry_A_LOD0_Team.asset",
                MeshFolder + "/SM_Infantry_A_LOD1_Base.asset",
                MeshFolder + "/SM_Infantry_A_LOD1_Team.asset",
            };
            if (replaceExisting)
            {
                AssetDatabase.DeleteAsset(PrefabPath);
                foreach (string meshPath in meshPaths) AssetDatabase.DeleteAsset(meshPath);
            }
            else if (Array.Exists(meshPaths, path => AssetDatabase.LoadAssetAtPath<Mesh>(path) != null))
            {
                Debug.LogWarning("[Infantry Art] Derived meshes already exist; use the Rebuild menu to replace them.");
                return;
            }

            var prefabRoot = new GameObject(PrototypeUnitArtCatalog.InfantryPrefabId);
            try
            {
                Transform visualRoot = CreateAnchor(prefabRoot.transform, "VisualRoot", 0f);
                LODRenderers lod0 = CreateCombinedLod(lod0Source, visualRoot, "LOD0", meshPaths[0], meshPaths[1],
                    baseMaterial, teamMaterial);
                LODRenderers lod1 = CreateCombinedLod(lod1Source, visualRoot, "LOD1", meshPaths[2], meshPaths[3],
                    baseMaterial, teamMaterial);
                if (!lod0.IsValid || !lod1.IsValid) throw new InvalidOperationException("Imported glTF has no usable base/team meshes.");

                var lodGroup = visualRoot.gameObject.AddComponent<LODGroup>();
                lodGroup.SetLODs(new[]
                {
                    new LOD(0.04f, lod0.All),
                    new LOD(0.008f, lod1.All),
                });
                lodGroup.fadeMode = LODFadeMode.CrossFade;
                lodGroup.animateCrossFading = true;
                lodGroup.RecalculateBounds();

                Transform selection = CreateAnchor(prefabRoot.transform, "SelectionAnchor", 0.02f);
                Transform health = CreateAnchor(prefabRoot.transform, "HealthBarAnchor", 2.10f);
                CreateAnchor(prefabRoot.transform, "GroundContact", 0f);
                var collider = prefabRoot.AddComponent<CapsuleCollider>();
                collider.radius = 0.38f;
                collider.height = 1.8f;
                collider.center = new Vector3(0f, 0.9f, 0f);

                var artView = prefabRoot.AddComponent<PrototypeUnitArtView>();
                artView.Configure(selection, health, new[] { lod0.Team, lod1.Team });
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Infantry Art] Built {PrefabPath}: LOD0={lod0.Triangles} tris, LOD1={lod1.Triangles} tris, 2 renderers per LOD.");
            }
            catch (Exception exception)
            {
                foreach (string meshPath in meshPaths) AssetDatabase.DeleteAsset(meshPath);
                Debug.LogException(exception);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefabRoot);
            }
        }

        private static LODRenderers CreateCombinedLod(GameObject source, Transform parent, string name,
            string baseMeshPath, string teamMeshPath, Material baseMaterial, Material teamMaterial)
        {
            GameObject imported = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (imported == null) return default;
            imported.name = "__ImportedSource";
            imported.transform.SetParent(parent, false);
            imported.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            imported.transform.localScale = Vector3.one;

            var baseParts = new List<CombineInstance>();
            var teamParts = new List<CombineInstance>();
            foreach (MeshFilter filter in imported.GetComponentsInChildren<MeshFilter>(true))
            {
                MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                Mesh mesh = filter.sharedMesh;
                if (renderer == null || mesh == null) continue;
                Material[] materials = renderer.sharedMaterials;
                for (int subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
                {
                    Material material = materials.Length == 0 ? null : materials[Math.Min(subMesh, materials.Length - 1)];
                    var combine = new CombineInstance
                    {
                        mesh = mesh,
                        subMeshIndex = subMesh,
                        transform = parent.worldToLocalMatrix * filter.transform.localToWorldMatrix,
                    };
                    if (material != null && material.name.IndexOf("TeamColor", StringComparison.OrdinalIgnoreCase) >= 0)
                        teamParts.Add(combine);
                    else
                        baseParts.Add(combine);
                }
            }
            UnityEngine.Object.DestroyImmediate(imported);

            GameObject lodRoot = new GameObject(name);
            lodRoot.transform.SetParent(parent, false);
            MeshRenderer baseRenderer = CreateCombinedRenderer(lodRoot.transform, "Base", baseParts, baseMeshPath, baseMaterial);
            MeshRenderer teamRenderer = CreateCombinedRenderer(lodRoot.transform, "TeamColor", teamParts, teamMeshPath, teamMaterial);
            int triangles = CountTriangles(baseRenderer) + CountTriangles(teamRenderer);
            return new LODRenderers(baseRenderer, teamRenderer, triangles);
        }

        private static MeshRenderer CreateCombinedRenderer(Transform parent, string name, List<CombineInstance> parts,
            string meshPath, Material material)
        {
            if (parts.Count == 0) return null;
            var mesh = new Mesh { name = Path.GetFileNameWithoutExtension(meshPath), indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(parts.ToArray(), true, true, false);
            mesh.RecalculateBounds();
            MeshUtility.Optimize(mesh);
            AssetDatabase.CreateAsset(mesh, meshPath);

            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return renderer;
        }

        private static Material CreateOrUpdateBaseMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BaseColorTexture);
            if (shader == null || texture == null)
            {
                Debug.LogError("[Infantry Art] URP Lit shader or BaseColor texture is missing.");
                return null;
            }
            Material material = AssetDatabase.LoadAssetAtPath<Material>(BaseMaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "MAT_Infantry_Base" };
                AssetDatabase.CreateAsset(material, BaseMaterialPath);
            }
            material.shader = shader;
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.2f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateOrUpdateTeamMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return null;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(TeamMaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "MAT_Infantry_TeamColor" };
                AssetDatabase.CreateAsset(material, TeamMaterialPath);
            }
            material.shader = shader;
            material.SetTexture("_BaseMap", null);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.2f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Transform CreateAnchor(Transform parent, string name, float localY)
        {
            var value = new GameObject(name).transform;
            value.SetParent(parent, false);
            value.localPosition = new Vector3(0f, localY, 0f);
            return value;
        }

        private static int CountTriangles(Renderer renderer)
        {
            MeshFilter filter = renderer != null ? renderer.GetComponent<MeshFilter>() : null;
            if (filter == null || filter.sharedMesh == null) return 0;
            int count = 0;
            for (int index = 0; index < filter.sharedMesh.subMeshCount; index++)
                count += (int)filter.sharedMesh.GetIndexCount(index) / 3;
            return count;
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || AssetDatabase.IsValidFolder(path)) return;
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }

        private readonly struct LODRenderers
        {
            public LODRenderers(MeshRenderer baseRenderer, MeshRenderer teamRenderer, int triangles)
            {
                Base = baseRenderer;
                Team = teamRenderer;
                Triangles = triangles;
            }

            public MeshRenderer Base { get; }
            public MeshRenderer Team { get; }
            public int Triangles { get; }
            public bool IsValid => Base != null && Team != null;
            public Renderer[] All => new Renderer[] { Base, Team };
        }
    }
}
