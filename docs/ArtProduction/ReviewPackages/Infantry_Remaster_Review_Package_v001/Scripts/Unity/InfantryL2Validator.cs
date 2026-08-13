using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public static class InfantryL2Validator
{
    [MenuItem("Tools/AegisRTS/L2 Validate Selected Infantry")]
    public static void ValidateSelected()
    {
        var go = Selection.activeGameObject;
        if (go == null) { Debug.LogError("Select the imported infantry root GameObject first."); return; }

        var renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) { Debug.LogError("No Renderer found below selected object."); return; }

        Bounds b = renderers[0].bounds;
        foreach (var r in renderers.Skip(1)) b.Encapsulate(r.bounds);
        var localMinY = go.transform.InverseTransformPoint(new Vector3(b.center.x, b.min.y, b.center.z)).y;
        var localMaxY = go.transform.InverseTransformPoint(new Vector3(b.center.x, b.max.y, b.center.z)).y;

        long tris = 0;
        foreach (var mf in go.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.sharedMesh == null) continue;
            for (int s = 0; s < mf.sharedMesh.subMeshCount; s++)
                tris += (long)mf.sharedMesh.GetIndexCount(s) / 3;
        }
        foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr.sharedMesh == null) continue;
            for (int s = 0; s < smr.sharedMesh.subMeshCount; s++)
                tris += (long)smr.sharedMesh.GetIndexCount(s) / 3;
        }

        var mats = new HashSet<Material>();
        foreach (var r in renderers) foreach (var m in r.sharedMaterials) if (m != null) mats.Add(m);

        Debug.Log($"[L2 Infantry] Root scale={go.transform.localScale}, height={localMaxY-localMinY:F3}m, minY={localMinY:F3}m, triangles={tris}, unique materials={mats.Count}");
        Debug.Log("Expected: Scale=(1,1,1), visual height 1.75–1.85m, foot minY≈0, LOD0 2500–6000 or LOD1 1000–2500 triangles, <=2 materials.");
    }

    [MenuItem("Tools/AegisRTS/L2 Capture 31m Preview")]
    public static void Capture31m()
    {
        var go = Selection.activeGameObject;
        if (go == null) { Debug.LogError("Select the imported infantry root GameObject first."); return; }
        Directory.CreateDirectory("Assets/L2Validation");
        Capture(go, 960, 540, 31f, "Assets/L2Validation/Infantry_960x540_31m.png");
        Capture(go, 1920, 1080, 31f, "Assets/L2Validation/Infantry_1920x1080_31m.png");
        Capture(go, 960, 540, 40f, "Assets/L2Validation/Infantry_960x540_40m.png");
        Capture(go, 960, 540, 8f, "Assets/L2Validation/Infantry_960x540_8m.png");
        AssetDatabase.Refresh();
        Debug.Log("L2 validation screenshots written to Assets/L2Validation/");
    }

    static void Capture(GameObject go, int width, int height, float distance, string path)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        Bounds b = renderers[0].bounds;
        foreach (var r in renderers.Skip(1)) b.Encapsulate(r.bounds);
        Vector3 target = new Vector3(b.center.x, go.transform.position.y + 0.90f, b.center.z);
        float pitch = 55f * Mathf.Deg2Rad;
        Vector3 camPos = target + new Vector3(0f, distance * Mathf.Sin(pitch), -distance * Mathf.Cos(pitch));

        var camGO = new GameObject("__L2CaptureCamera");
        var cam = camGO.AddComponent<Camera>();
        cam.fieldOfView = 60f;
        cam.transform.position = camPos;
        cam.transform.rotation = Quaternion.LookRotation(target - camPos, Vector3.up);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.18f, 0.18f);

        var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        RenderTexture.active = null;
        cam.targetTexture = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(camGO);
    }
}
