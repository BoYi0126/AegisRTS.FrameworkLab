"""Read-only geometry, hierarchy, and follow audit for P035R3."""
import argparse
import csv
import hashlib
import json
import math
from pathlib import Path

import bmesh
import bpy
from mathutils import Vector


STATUS = "READY FOR PHASE03_5 REVISION03 REVIEW"
SWORD_PARTS = (
    "GEO_Infantry_Sword_GripContact", "Sword", "Sword_Grip", "Sword_Guard",
    "Sword_Pommel", "GEO_Infantry_Sword_BladeSpine", "GEO_Infantry_Sword_GripWraps",
)


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
    mesh = bmesh.new(); mesh.from_mesh(obj.data)
    values = (
        sum(1 for edge in mesh.edges if not edge.is_manifold),
        sum(1 for edge in mesh.edges if edge.is_boundary),
        sum(1 for edge in mesh.edges if not edge.link_faces),
        sum(1 for face in mesh.faces if face.calc_area() <= 1e-12),
    )
    mesh.free(); return values


def follow_sample(armature, sword_root, degrees):
    pose = armature.pose.bones["RightHand"]
    pose.rotation_mode = "XYZ"
    pose.rotation_euler = (0.0, math.radians(degrees), 0.0)
    bpy.context.view_layer.update()
    result = {
        "right_hand_rotation_y_degrees": degrees,
        "sword_root_world_translation_m": list(sword_root.matrix_world.translation),
        "sword_root_world_matrix": [list(row) for row in sword_root.matrix_world],
        "socket_pose_world_matrix": [list(row) for row in (armature.matrix_world @ armature.pose.bones["Socket_R_Hand"].matrix)],
    }
    pose.rotation_euler = (0.0, 0.0, 0.0); bpy.context.view_layer.update()
    return result


def main():
    args = arguments(); root = Path(args.output_root).resolve(); manifest = root / "Manifests"; manifest.mkdir(parents=True, exist_ok=True)
    scene = bpy.context.scene
    if scene.get("SourceVersion") != "CHR_Infantry_A_v004_P035R3":
        raise RuntimeError("Opened blend is not P035R3")
    meshes = [obj for obj in scene.objects if obj.type == "MESH"]
    armature = next(obj for obj in scene.objects if obj.type == "ARMATURE")
    sword_root = scene.objects["WPN_SwordRoot_R"]
    points = [obj.matrix_world @ Vector(corner) for obj in meshes for corner in obj.bound_box]
    low = Vector(tuple(min(point[i] for point in points) for i in range(3))); high = Vector(tuple(max(point[i] for point in points) for i in range(3)))
    totals = [0, 0, 0, 0]; rows = []
    for obj in sorted(meshes, key=lambda value: value.name):
        values = topology(obj); totals = [totals[i] + values[i] for i in range(4)]
        rows.append({"ObjectName": obj.name, "Collection": ";".join(c.name for c in obj.users_collection), "Vertices": len(obj.data.vertices), "Triangles": triangles(obj), "Materials": ";".join(s.material.name for s in obj.material_slots if s.material), "Parent": obj.parent.name if obj.parent else "", "ParentType": obj.parent_type, "ParentBone": obj.parent_bone, "AttachmentBone": obj.get("AttachmentBone", ""), "AttachmentRoot": obj.get("AttachmentRoot", ""), "NonManifoldEdges": values[0], "BoundaryEdges": values[1], "LooseEdges": values[2], "ZeroAreaFaces": values[3]})
    with (manifest / "Object_Manifest.csv").open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=rows[0].keys()); writer.writeheader(); writer.writerows(rows)
    bones = [{"Bone": b.name, "Parent": b.parent.name if b.parent else "", "Deform": b.use_deform} for b in armature.data.bones]
    with (manifest / "Bone_Manifest.csv").open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=bones[0].keys()); writer.writeheader(); writer.writerows(bones)
    build = json.loads(Path(args.build_result).read_text(encoding="utf-8")); opened_hash = sha256(bpy.data.filepath)
    follow = {"neutral": follow_sample(armature, sword_root, 0.0), "up": follow_sample(armature, sword_root, 15.0), "down": follow_sample(armature, sword_root, -15.0)}
    translations = [Vector(follow[key]["sword_root_world_translation_m"]) for key in ("neutral", "up", "down")]
    hierarchy_ok = (
        armature.data.bones["Socket_R_Hand"].parent == armature.data.bones["RightHand"] and
        not armature.data.bones["Socket_R_Hand"].use_deform and
        sword_root.parent == armature and sword_root.parent_type == "BONE" and sword_root.parent_bone == "Socket_R_Hand" and
        all(scene.objects[name].parent == sword_root for name in SWORD_PARTS)
    )
    summary = {
        "status": STATUS, "source_version": scene.get("SourceVersion"), "opened_file": bpy.data.filepath,
        "opened_file_sha256": opened_hash, "build_result_sha_matches": opened_hash == build["output_sha256"],
        "blender_version": bpy.app.version_string, "saved_by_audit_script": False,
        "height_m": high.z - low.z, "bounds": {"min": list(low), "max": list(high), "dimensions": list(high-low)},
        "mesh_count": len(meshes), "vertices": sum(len(o.data.vertices) for o in meshes), "triangles": sum(triangles(o) for o in meshes), "armatures": 1, "bones": len(bones),
        "topology": dict(zip(("non_manifold_edges", "boundary_edges", "loose_edges", "zero_area_faces"), totals)),
        "hierarchy_valid": hierarchy_ok, "socket_non_deforming_in_source": not armature.data.bones["Socket_R_Hand"].use_deform,
        "sword_parts": list(SWORD_PARTS), "follow_samples": follow,
        "follow_translation_delta_up_m": (translations[1]-translations[0]).length,
        "follow_translation_delta_down_m": (translations[2]-translations[0]).length,
        "attachment_follow_valid": (translations[1]-translations[0]).length > .001 and (translations[2]-translations[0]).length > .001,
        "geometry_fingerprints_preserved": build["locks"]["all_mesh_fingerprints_preserved"],
        "world_bounds_preserved": build["locks"]["all_world_bounds_preserved"],
        "runtime_prefab_replaced": False,
    }
    if not hierarchy_ok or not summary["attachment_follow_valid"] or not summary["build_result_sha_matches"]:
        raise RuntimeError("P035R3 hierarchy/follow/hash audit failed")
    (manifest / "Geometry_Hierarchy_Follow_Summary.json").write_text(json.dumps(summary, indent=2), encoding="utf-8")
    print("AEGIS_P035R3_AUDIT_COMPLETE", json.dumps({"meshes": len(meshes), "triangles": summary["triangles"], "bones": len(bones), "up_delta": summary["follow_translation_delta_up_m"], "down_delta": summary["follow_translation_delta_down_m"]}))


if __name__ == "__main__":
    main()
