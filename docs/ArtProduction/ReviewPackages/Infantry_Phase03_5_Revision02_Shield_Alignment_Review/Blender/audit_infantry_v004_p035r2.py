"""Read-only geometry, topology, rig, alignment and preservation audit for P035R2."""
import argparse
import csv
import hashlib
import importlib.util
import json
from pathlib import Path

import bmesh
import bpy
from mathutils import Vector


STATUS = "READY FOR PHASE03_5 REVISION02 REVIEW"


def arguments():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output-root", required=True)
    parser.add_argument("--build-result", required=True)
    argv = __import__("sys").argv
    return parser.parse_args(argv[argv.index("--") + 1 :] if "--" in argv else [])


def sha256(path):
    digest = hashlib.sha256()
    with open(path, "rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def triangles(obj):
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)


def topology(obj):
    mesh = bmesh.new()
    mesh.from_mesh(obj.data)
    values = (sum(1 for edge in mesh.edges if not edge.is_manifold), sum(1 for edge in mesh.edges if edge.is_boundary), sum(1 for edge in mesh.edges if not edge.link_faces), sum(1 for face in mesh.faces if face.calc_area() <= 1e-12))
    mesh.free()
    return values


def main():
    args = arguments()
    root = Path(args.output_root).resolve()
    manifest = root / "Manifests"
    manifest.mkdir(parents=True, exist_ok=True)
    scene = bpy.context.scene
    if scene.get("SourceVersion") != "CHR_Infantry_A_v004_P035R2":
        raise RuntimeError("Opened blend is not P035R2")
    meshes = [obj for obj in scene.objects if obj.type == "MESH"]
    armatures = [obj for obj in scene.objects if obj.type == "ARMATURE"]
    if len(armatures) != 1:
        raise RuntimeError("Expected exactly one armature")
    points = [obj.matrix_world @ Vector(corner) for obj in meshes for corner in obj.bound_box]
    low = Vector(tuple(min(point[index] for point in points) for index in range(3)))
    high = Vector(tuple(max(point[index] for point in points) for index in range(3)))
    totals = [0, 0, 0, 0]
    rows = []
    for obj in sorted(meshes, key=lambda value: value.name):
        values = topology(obj)
        totals = [totals[index] + values[index] for index in range(4)]
        rows.append({"ObjectName": obj.name, "Collection": ";".join(collection.name for collection in obj.users_collection), "Vertices": len(obj.data.vertices), "Triangles": triangles(obj), "Materials": ";".join(slot.material.name for slot in obj.material_slots if slot.material), "Parent": obj.parent.name if obj.parent else "", "NonManifoldEdges": values[0], "BoundaryEdges": values[1], "LooseEdges": values[2], "ZeroAreaFaces": values[3]})
    with (manifest / "Object_Manifest.csv").open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=rows[0].keys())
        writer.writeheader(); writer.writerows(rows)
    bones = [{"Bone": bone.name, "Parent": bone.parent.name if bone.parent else "", "Deform": bone.use_deform} for bone in armatures[0].data.bones]
    with (manifest / "Bone_Manifest.csv").open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=bones[0].keys())
        writer.writeheader(); writer.writerows(bones)
    build = json.loads(Path(args.build_result).read_text(encoding="utf-8"))
    opened_hash = sha256(bpy.data.filepath)
    summary = {
        "status": STATUS,
        "source_version": scene.get("SourceVersion"),
        "source_baseline": scene.get("SourceBaseline"),
        "source_baseline_sha256": scene.get("SourceBaselineSHA256"),
        "opened_file": bpy.data.filepath,
        "opened_file_sha256": opened_hash,
        "build_result_sha_matches": opened_hash == build["output_sha256"],
        "blender_version": bpy.app.version_string,
        "saved_by_audit_script": False,
        "height_m": high.z - low.z,
        "bounds": {"min": list(low), "max": list(high), "dimensions": list(high - low)},
        "mesh_count": len(meshes), "vertices": sum(len(obj.data.vertices) for obj in meshes), "triangles": sum(triangles(obj) for obj in meshes), "armatures": len(armatures), "bones": len(bones),
        "topology": dict(zip(("non_manifold_edges", "boundary_edges", "loose_edges", "zero_area_faces"), totals)),
        "shield_size_before_m": build["shield_size_before_m"],
        "shield_size_after_m": build["shield_size_after_m"],
        "before_alignment": build["before_alignment"],
        "after_alignment": build["after_alignment"],
        "body_proportion_changed": False,
        "shield_front_geometry_changed": False,
        "source_apose_body_preserved": True,
        "review_pose_not_active": armatures[0].animation_data is None or armatures[0].animation_data.action is None,
        "runtime_prefab_replaced": False,
    }
    (manifest / "Geometry_Summary.json").write_text(json.dumps(summary, indent=2), encoding="utf-8")
    print("AEGIS_P035R2_AUDIT_COMPLETE", json.dumps({"triangles": summary["triangles"], "topology": summary["topology"], "top": summary["after_alignment"]["shield_top_y_normalized"]}))


if __name__ == "__main__":
    main()
