"""Build P03.5 Revision 03 sword attachment from immutable P035R2."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path

import bpy
from mathutils import Matrix, Vector


STATUS = "READY FOR PHASE03_5 REVISION03 REVIEW"
SOURCE_VERSION = "CHR_Infantry_A_v004_P035R2"
OUTPUT_VERSION = "CHR_Infantry_A_v004_P035R3"
EXPECTED_SHA = "D8DCD84D888204D65385A94CF15B0C07BEA227236D47EA5EC3D54992999E551D"
SOCKET_NAME = "Socket_R_Hand"
SWORD_ROOT_NAME = "WPN_SwordRoot_R"
EXPECTED_SWORD_PARTS = (
    "GEO_Infantry_Sword_GripContact",
    "Sword",
    "Sword_Grip",
    "Sword_Guard",
    "Sword_Pommel",
    "GEO_Infantry_Sword_BladeSpine",
    "GEO_Infantry_Sword_GripWraps",
)


def arguments():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", required=True)
    parser.add_argument("--documentation", required=True)
    parser.add_argument("--measurements", required=True)
    argv = __import__("sys").argv
    return parser.parse_args(argv[argv.index("--") + 1 :] if "--" in argv else [])


def sha256(path):
    digest = hashlib.sha256()
    with open(path, "rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def load_base():
    path = Path(__file__).resolve().with_name("build_infantry_v004_p035.py")
    spec = importlib.util.spec_from_file_location("p035_base_for_r3", path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def world_bounds(objects):
    points = [obj.matrix_world @ Vector(corner) for obj in objects for corner in obj.bound_box]
    low = Vector(tuple(min(point[index] for point in points) for index in range(3)))
    high = Vector(tuple(max(point[index] for point in points) for index in range(3)))
    return low, high


def transform_record(obj):
    return {
        "name": obj.name,
        "parent": obj.parent.name if obj.parent else "",
        "parent_type": obj.parent_type,
        "parent_bone": obj.parent_bone,
        "local_location": list(obj.location),
        "local_rotation_euler_rad": list(obj.rotation_euler),
        "local_scale": list(obj.scale),
        "world_matrix": [list(row) for row in obj.matrix_world],
    }


def bone_record(armature, bone_name):
    bone = armature.data.bones[bone_name]
    return {
        "name": bone.name,
        "parent": bone.parent.name if bone.parent else "",
        "deform": bone.use_deform,
        "head_local": list(bone.head_local),
        "tail_local": list(bone.tail_local),
        "matrix_local": [list(row) for row in bone.matrix_local],
    }


def mesh_fingerprint(obj):
    digest = hashlib.sha256()
    digest.update(obj.name.encode("utf-8"))
    for vertex in obj.data.vertices:
        digest.update(("%.9f,%.9f,%.9f;" % tuple(vertex.co)).encode("ascii"))
    for polygon in obj.data.polygons:
        digest.update((",".join(str(value) for value in polygon.vertices) + ";").encode("ascii"))
    return digest.hexdigest().upper()


def hierarchy_parent_keep_world(obj, parent, parent_type="OBJECT", parent_bone=""):
    world = obj.matrix_world.copy()
    obj.parent = parent
    obj.parent_type = parent_type
    obj.parent_bone = parent_bone
    obj.matrix_world = world


def create_attachment_hierarchy(armature, sword_parts, grip_center):
    collection = sword_parts[0].users_collection[0]
    armature_world_inverse = armature.matrix_world.inverted()
    socket_head = armature_world_inverse @ grip_center
    hand = armature.data.bones["RightHand"]
    direction = (hand.tail_local - hand.head_local).normalized()
    bpy.context.view_layer.objects.active = armature
    armature.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    socket = armature.data.edit_bones.new(SOCKET_NAME)
    socket.head = socket_head
    socket.tail = socket_head + direction * 0.060
    socket.parent = armature.data.edit_bones["RightHand"]
    socket.use_connect = False
    bpy.ops.object.mode_set(mode="OBJECT")
    armature.data.bones[SOCKET_NAME].use_deform = False

    sword_root = bpy.data.objects.new(SWORD_ROOT_NAME, None)
    sword_root.empty_display_type = "PLAIN_AXES"
    sword_root.empty_display_size = 0.090
    collection.objects.link(sword_root)
    sword_root.parent = armature
    sword_root.parent_type = "BONE"
    sword_root.parent_bone = SOCKET_NAME
    sword_root.matrix_world = Matrix.Translation(grip_center)
    sword_root["AssetRole"] = "SwordRoot"
    sword_root["AttachmentSocket"] = SOCKET_NAME

    for obj in sword_parts:
        hierarchy_parent_keep_world(obj, sword_root)
        obj["AttachmentBone"] = SOCKET_NAME
        obj["AttachmentRoot"] = SWORD_ROOT_NAME
        obj["BindingStatus"] = "P035R3_RIGID_SOCKET_ATTACHMENT"
    return sword_root


def main():
    args = arguments()
    source = Path(bpy.data.filepath).resolve()
    if sha256(source) != EXPECTED_SHA:
        raise RuntimeError("P035R2 input hash mismatch; refusing to build")
    scene = bpy.context.scene
    if scene.get("SourceVersion") != SOURCE_VERSION:
        raise RuntimeError(f"Expected {SOURCE_VERSION}, found {scene.get('SourceVersion')}")
    base = load_base()
    meshes = [obj for obj in scene.objects if obj.type == "MESH"]
    armatures = [obj for obj in scene.objects if obj.type == "ARMATURE"]
    if len(meshes) != 98 or len(armatures) != 1:
        raise RuntimeError(f"Unexpected baseline: meshes={len(meshes)}, armatures={len(armatures)}")
    armature = armatures[0]
    if len(armature.data.bones) != 23 or "RightHand" not in armature.data.bones:
        raise RuntimeError("P035R2 Humanoid skeleton baseline mismatch")
    sword_parts = [scene.objects.get(name) for name in EXPECTED_SWORD_PARTS]
    if any(obj is None or obj.type != "MESH" for obj in sword_parts):
        missing = [name for name, obj in zip(EXPECTED_SWORD_PARTS, sword_parts) if obj is None]
        raise RuntimeError(f"Sword part audit failed; missing={missing}")
    discovered = sorted(obj.name for obj in meshes if "Sword" in obj.name)
    if discovered != sorted(EXPECTED_SWORD_PARTS):
        raise RuntimeError(f"Sword part set changed: {discovered}")

    before = [transform_record(obj) for obj in sword_parts]
    fingerprints_before = {obj.name: mesh_fingerprint(obj) for obj in meshes}
    bounds_before = {obj.name: [list(value) for value in world_bounds([obj])] for obj in meshes}
    all_low_before, all_high_before = world_bounds(meshes)
    sword_low, sword_high = world_bounds(sword_parts)
    grip_parts = [obj for obj in sword_parts if "Grip" in obj.name]
    grip_low, grip_high = world_bounds(grip_parts)
    grip_center = (grip_low + grip_high) * 0.5
    hand = armature.data.bones["RightHand"]
    hand_head = armature.matrix_world @ hand.head_local
    hand_tail = armature.matrix_world @ hand.tail_local
    hand_center = (hand_head + hand_tail) * 0.5

    sword_root = create_attachment_hierarchy(armature, sword_parts, grip_center)
    bpy.context.view_layer.update()
    fingerprints_after = {obj.name: mesh_fingerprint(obj) for obj in meshes}
    bounds_after = {obj.name: [list(value) for value in world_bounds([obj])] for obj in meshes}
    all_low_after, all_high_after = world_bounds(meshes)
    if fingerprints_before != fingerprints_after:
        raise RuntimeError("Geometry fingerprint changed during hierarchy repair")
    for name in fingerprints_before:
        before_pair, after_pair = bounds_before[name], bounds_after[name]
        if any(abs(before_pair[i][j] - after_pair[i][j]) > 1e-6 for i in range(2) for j in range(3)):
            raise RuntimeError(f"World transform preservation failed for {name}")
    if (all_low_before - all_low_after).length > 1e-6 or (all_high_before - all_high_after).length > 1e-6:
        raise RuntimeError("Character bounds changed")
    if len(armature.data.bones) != 24:
        raise RuntimeError("Expected exactly one non-deforming socket bone")
    socket = armature.data.bones.get(SOCKET_NAME)
    if socket is None or socket.parent != armature.data.bones["RightHand"] or socket.use_deform:
        raise RuntimeError("Socket is not a non-deforming RightHand child bone")
    if sword_root.parent != armature or sword_root.parent_type != "BONE" or sword_root.parent_bone != SOCKET_NAME or any(obj.parent != sword_root for obj in sword_parts):
        raise RuntimeError("Sword hierarchy validation failed")

    scene["SourceVersion"] = OUTPUT_VERSION
    scene["SourceBaseline"] = SOURCE_VERSION
    scene["SourceBaselineSHA256"] = EXPECTED_SHA
    scene["ReviewStatus"] = STATUS
    scene["ProductionNeutralPose"] = "POSE_SOURCE_A"
    scene["ReviewOnlyPose"] = "REVIEW_ONLY_POSE_L1_COMPARE"
    scene["WeaponSocketContract"] = SOCKET_NAME
    scene["SwordRoot"] = SWORD_ROOT_NAME
    scene["SwordGeometrySizeLocked"] = True
    scene["ShieldGeometrySizeLocked"] = True
    scene["RuntimePrefabReplacement"] = False
    scene["Phase04Started"] = False

    output = Path(args.output).resolve()
    output.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(output), check_existing=False)
    output_hash = sha256(output)
    documentation = Path(args.documentation).resolve()
    measurements = Path(args.measurements).resolve()
    documentation.mkdir(parents=True, exist_ok=True)
    measurements.mkdir(parents=True, exist_ok=True)

    after = [transform_record(obj) for obj in sword_parts]
    hierarchy = {
        "right_hand": "RightHand",
        "socket": bone_record(armature, SOCKET_NAME),
        "sword_root": transform_record(sword_root),
        "sword_parts": after,
        "grip_bounds_m": {"min": list(grip_low), "max": list(grip_high), "center": list(grip_center)},
        "right_hand_m": {"head": list(hand_head), "tail": list(hand_tail), "center": list(hand_center)},
        "grip_center_to_hand_center_m": list(grip_center - hand_center),
        "sword_bounds_m": {"min": list(sword_low), "max": list(sword_high), "dimensions": list(sword_high - sword_low)},
    }
    (measurements / "Sword_Hierarchy_Before.json").write_text(json.dumps(before, indent=2), encoding="utf-8")
    (measurements / "Sword_Attachment_After.json").write_text(json.dumps(hierarchy, indent=2), encoding="utf-8")
    result = {
        "status": STATUS,
        "input": str(source),
        "input_sha256": EXPECTED_SHA,
        "output": str(output),
        "output_sha256": output_hash,
        "blender_version": bpy.app.version_string,
        "geometry": {
            "height_m": all_high_after.z - all_low_after.z,
            "meshes": len(meshes),
            "vertices": sum(len(obj.data.vertices) for obj in meshes),
            "triangles": sum(base.mesh_triangles(obj) for obj in meshes),
            "bones": len(armature.data.bones),
        },
        "hierarchy": hierarchy,
        "locks": {
            "all_mesh_fingerprints_preserved": True,
            "all_world_bounds_preserved": True,
            "body_geometry_changed": False,
            "arm_geometry_changed": False,
            "head_geometry_changed": False,
            "shield_geometry_or_transform_changed": False,
            "sword_geometry_or_size_changed": False,
            "only_attachment_hierarchy_changed": True,
        },
        "runtime": {"runtime_prefab_replaced": False, "equipment_system_added": False},
        "deferred": ["Phase 04", "Final UV", "Final Texture", "Final Skinning", "Animation Polish", "Runtime Prefab replacement", "Generic Equipment system"],
    }
    (documentation / "P035R3_BUILD_RESULT.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print("AEGIS_P035R3_BUILD_COMPLETE", json.dumps({"parts": len(sword_parts), "bones": len(armature.data.bones), "grip_delta": hierarchy["grip_center_to_hand_center_m"]}))


if __name__ == "__main__":
    main()
