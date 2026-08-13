"""Build the focused P03.5 Revision 02 shield alignment from immutable P035R1."""
import argparse
import hashlib
import importlib.util
import json
import math
from pathlib import Path

import bpy
from mathutils import Matrix, Vector


STATUS = "READY FOR PHASE03_5 REVISION02 REVIEW"
SOURCE_VERSION = "CHR_Infantry_A_v004_P035R1"
OUTPUT_VERSION = "CHR_Infantry_A_v004_P035R2"
EXPECTED_SHA = "A0CCC9771CD7A62D966891784745F138A9DCBF1230DF5E71A4F5D60900A84D0A"
SHIELD_LOCAL_OFFSET = Vector((0.0, 0.0, 0.080))
SHIELD_REVIEW_OFFSET = Vector((0.0, 0.0, 0.113))
GRIP_FITTING_OFFSET = Vector((0.0, 0.0, -0.040))
STRAP_FITTING_OFFSET = Vector((0.0, 0.0, -0.060))
LEFT_UPPER_DEGREES = -10.0
LEFT_LOWER_DEGREES = -20.0
RIGHT_UPPER_DEGREES = 46.0
RIGHT_LOWER_DEGREES = 6.0
SHIELD_PITCH_DEGREES = -3.0
SHIELD_INWARD_DEGREES = 4.0


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


def load_module(filename, name):
    path = Path(__file__).resolve().with_name(filename)
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def load_base():
    return load_module("build_infantry_v004_p035.py", "p035_base_for_r2")


def around(point, angle, axis="Y"):
    return Matrix.Translation(point) @ Matrix.Rotation(angle, 4, axis) @ Matrix.Translation(-point)


def side_name(name, side):
    return name.endswith("_" + side) or ("_" + side + "_") in name


def arm_points(armature, side):
    prefix = "Left" if side == "L" else "Right"
    bones = armature.data.bones
    matrix = armature.matrix_world
    return {
        "shoulder": matrix @ bones[prefix + "UpperArm"].head_local,
        "elbow": matrix @ bones[prefix + "LowerArm"].head_local,
        "wrist": matrix @ bones[prefix + "Hand"].head_local,
        "hand_end": matrix @ bones[prefix + "Hand"].tail_local,
    }


def pose_angles(mode, side):
    if mode == "before":
        return (-46.0, -6.0) if side == "L" else (46.0, 6.0)
    return (LEFT_UPPER_DEGREES, LEFT_LOWER_DEGREES) if side == "L" else (RIGHT_UPPER_DEGREES, RIGHT_LOWER_DEGREES)


def arm_pose(armature, side, mode):
    points = arm_points(armature, side)
    upper_degrees, lower_degrees = pose_angles(mode, side)
    upper = around(points["shoulder"], math.radians(upper_degrees))
    elbow = upper @ points["elbow"]
    lower = around(elbow, math.radians(lower_degrees))
    combined = lower @ upper
    wrist = combined @ points["wrist"]
    hand_end = combined @ points["hand_end"]
    return {
        "source": points,
        "upper_matrix": upper,
        "lower_matrix": combined,
        "shoulder": points["shoulder"].copy(),
        "elbow": elbow,
        "wrist": wrist,
        "palm": (wrist + hand_end) * 0.5,
        "hand_end": hand_end,
        "upper_degrees": upper_degrees,
        "lower_degrees": lower_degrees,
    }


def shield_pose_matrix(armature, mode):
    pose = arm_pose(armature, "L", mode)
    translation = Matrix.Translation(
        pose["wrist"] - pose["source"]["wrist"] + (Vector((0.0, 0.0, 0.0)) if mode == "before" else SHIELD_REVIEW_OFFSET)
    )
    if mode == "before":
        return translation
    pitch = around(pose["wrist"], math.radians(SHIELD_PITCH_DEGREES), "X")
    inward = around(pose["wrist"], math.radians(SHIELD_INWARD_DEGREES), "Z")
    return inward @ pitch @ translation


def object_pose_matrix(obj, armature, mode):
    if "Shield" in obj.name:
        return shield_pose_matrix(armature, mode)
    for side in ("L", "R"):
        if not side_name(obj.name, side):
            continue
        pose = arm_pose(armature, side, mode)
        if obj.name.startswith(("UpperArm_", "Elbow_")):
            return pose["upper_matrix"]
        if obj.name.startswith(("Forearm_", "Bracer_", "Hand_", "Thumb_")) or "Bracer_" in obj.name:
            return pose["lower_matrix"]
    return Matrix.Identity(4)


def posed_bounds(objects, armature, mode):
    points = []
    for obj in objects:
        pose = object_pose_matrix(obj, armature, mode)
        points.extend(pose @ obj.matrix_world @ Vector(corner) for corner in obj.bound_box)
    low = Vector(tuple(min(point[index] for point in points) for index in range(3)))
    high = Vector(tuple(max(point[index] for point in points) for index in range(3)))
    return low, high


def group_pose_bounds(meshes, armature, mode, predicate):
    return posed_bounds([obj for obj in meshes if predicate(obj.name)], armature, mode)


def alignment_measurement(base, meshes, armature, mode):
    all_low, all_high = base.bounds(meshes)
    height = all_high.z - all_low.z
    shield = [obj for obj in meshes if "Shield" in obj.name]
    shield_low, shield_high = posed_bounds(shield, armature, mode)
    grip_low, grip_high = group_pose_bounds(meshes, armature, mode, lambda name: "Shield_HandGrip" in name)
    strap_low, strap_high = group_pose_bounds(meshes, armature, mode, lambda name: "Shield_ForearmStrap" in name)
    pose = arm_pose(armature, "L", mode)
    normalized = lambda value: (value - all_low.z) / height
    return {
        "source_version": bpy.context.scene.get("SourceVersion"),
        "pose": "REVIEW_ONLY_POSE_L1_COMPARE",
        "character_height_m": height,
        "shield_height_m": base.group_dimensions(meshes, lambda name: "Shield" in name).z,
        "shield_width_m": base.group_dimensions(meshes, lambda name: "Shield" in name).x,
        "shield_top_y_m": shield_high.z,
        "shield_center_y_m": (shield_low.z + shield_high.z) * 0.5,
        "shield_bottom_y_m": shield_low.z,
        "shield_top_y_normalized": normalized(shield_high.z),
        "shield_center_y_normalized": normalized((shield_low.z + shield_high.z) * 0.5),
        "shield_bottom_y_normalized": normalized(shield_low.z),
        "left_shoulder_y_normalized": normalized(pose["shoulder"].z),
        "left_elbow_y_normalized": normalized(pose["elbow"].z),
        "left_wrist_y_normalized": normalized(pose["wrist"].z),
        "shield_grip_center_y_normalized": normalized((grip_low.z + grip_high.z) * 0.5),
        "forearm_strap_center_y_normalized": normalized((strap_low.z + strap_high.z) * 0.5),
        "left_upper_arm_rotation_degrees": pose["upper_degrees"],
        "left_lower_arm_rotation_degrees": pose["lower_degrees"],
        "shield_pitch_degrees": 0.0 if mode == "before" else SHIELD_PITCH_DEGREES,
        "shield_inward_degrees": 0.0 if mode == "before" else SHIELD_INWARD_DEGREES,
    }


def translate_vertices(obj, delta):
    inverse = obj.matrix_world.inverted()
    for vertex in obj.data.vertices:
        vertex.co = inverse @ (obj.matrix_world @ vertex.co + delta)
    obj.data.update()


def revise_shield_fit(meshes):
    for obj in meshes:
        if "Shield" not in obj.name:
            continue
        delta = SHIELD_LOCAL_OFFSET.copy()
        if "Shield_HandGrip" in obj.name:
            delta += GRIP_FITTING_OFFSET
        if "Shield_ForearmStrap" in obj.name:
            delta += STRAP_FITTING_OFFSET
        translate_vertices(obj, delta)


def rebuild_review_action(armature):
    old = bpy.data.actions.get("REVIEW_ONLY_POSE_L1_COMPARE")
    if old is not None:
        bpy.data.actions.remove(old)
    action = bpy.data.actions.new("REVIEW_ONLY_POSE_L1_COMPARE")
    action.use_fake_user = True
    action["REVIEW_ONLY"] = True
    action["Purpose"] = "P035R2 shield/grip alignment comparison; never export as gameplay idle"
    action["ShieldLocalOffsetM"] = list(SHIELD_LOCAL_OFFSET)
    marker = action.pose_markers.new("POSE_L1_COMPARE")
    marker.frame = 1
    armature.animation_data_create()
    armature.animation_data.action = action
    bpy.context.scene.frame_set(1)
    for name, degrees in (
        ("LeftUpperArm", LEFT_UPPER_DEGREES),
        ("LeftLowerArm", LEFT_LOWER_DEGREES),
        ("RightUpperArm", RIGHT_UPPER_DEGREES),
        ("RightLowerArm", RIGHT_LOWER_DEGREES),
    ):
        pose_bone = armature.pose.bones[name]
        pose_bone.rotation_mode = "XYZ"
        pose_bone.rotation_euler = (0.0, math.radians(degrees), 0.0)
        pose_bone.keyframe_insert("rotation_euler", frame=1, group=name)
    armature.animation_data.action = None
    for pose_bone in armature.pose.bones:
        pose_bone.rotation_mode = "QUATERNION"
        pose_bone.rotation_quaternion = (1.0, 0.0, 0.0, 0.0)


def l1_estimate():
    top = (455 - 229) / 322
    center = (455 - 305) / 322
    bottom = (455 - 381) / 322
    return {
        "source": "Unit_03_Infantry_L1_Concept_Final.png",
        "view": "friendly_front",
        "normalization": {"plume_top_y_px": 133, "ground_y_px": 455, "height_px": 322},
        "shield_top": {"y_px": 229, "normalized": top, "confidence": "medium", "estimated": True},
        "shield_center": {"y_px": 305, "normalized": center, "confidence": "medium", "estimated": True},
        "shield_bottom": {"y_px": 381, "normalized": bottom, "confidence": "medium", "estimated": True},
        "note": "Stylized 2D concept; shield landmarks are estimates normalized by ground and full presented height.",
    }


def main():
    args = arguments()
    source = Path(bpy.data.filepath).resolve()
    if sha256(source) != EXPECTED_SHA:
        raise RuntimeError("P035R1 input hash mismatch; refusing to build")
    scene = bpy.context.scene
    if scene.get("SourceVersion") != SOURCE_VERSION:
        raise RuntimeError(f"Expected {SOURCE_VERSION}, found {scene.get('SourceVersion')}")
    base = load_base()
    meshes = [obj for obj in scene.objects if obj.type == "MESH"]
    armatures = [obj for obj in scene.objects if obj.type == "ARMATURE"]
    if len(meshes) != 98 or len(armatures) != 1:
        raise RuntimeError(f"Unexpected baseline: meshes={len(meshes)}, armatures={len(armatures)}")
    armature = armatures[0]
    base_measurement = base.measurement(meshes, armature)
    before = alignment_measurement(base, meshes, armature, "before")
    revise_shield_fit(meshes)
    rebuild_review_action(armature)
    bpy.context.view_layer.update()
    after_measurement = base.measurement(meshes, armature)
    after = alignment_measurement(base, meshes, armature, "after")

    if abs(after_measurement["weapon_measurements_m"]["shield_width"] - 0.6000000238418579) > 1e-6:
        raise RuntimeError("Shield width lock failed")
    if abs(after_measurement["weapon_measurements_m"]["shield_height"] - 0.8624239563941956) > 1e-6:
        raise RuntimeError("Shield height lock failed")
    if not 0.68 <= after["shield_top_y_normalized"] <= 0.74:
        raise RuntimeError(f"Shield top alignment outside target: {after['shield_top_y_normalized']}")
    if not 0.20 <= after["shield_bottom_y_normalized"] <= 0.27:
        raise RuntimeError(f"Shield bottom alignment outside target: {after['shield_bottom_y_normalized']}")
    if len(armature.data.bones) != 23:
        raise RuntimeError("Bone count changed")

    scene["SourceVersion"] = OUTPUT_VERSION
    scene["SourceBaseline"] = SOURCE_VERSION
    scene["SourceBaselineSHA256"] = EXPECTED_SHA
    scene["ReviewStatus"] = STATUS
    scene["ProductionNeutralPose"] = "POSE_SOURCE_A"
    scene["ReviewOnlyPose"] = "REVIEW_ONLY_POSE_L1_COMPARE"
    scene["ShieldLocalOffsetM"] = list(SHIELD_LOCAL_OFFSET)
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
    (measurements / "Shield_Alignment_Before.json").write_text(json.dumps(before, indent=2), encoding="utf-8")
    (measurements / "Shield_Alignment_After.json").write_text(json.dumps(after, indent=2), encoding="utf-8")
    (measurements / "L1_Shield_Alignment_Estimate.json").write_text(json.dumps(l1_estimate(), indent=2), encoding="utf-8")

    result = {
        "status": STATUS,
        "input": str(source),
        "input_sha256": EXPECTED_SHA,
        "output": str(output),
        "output_sha256": output_hash,
        "blender_version": bpy.app.version_string,
        "geometry": {
            "height_m": after_measurement["character_height_m"],
            "meshes": len(meshes),
            "vertices": sum(len(obj.data.vertices) for obj in meshes),
            "triangles": sum(base.mesh_triangles(obj) for obj in meshes),
            "bones": len(armature.data.bones),
        },
        "shield_size_before_m": {"width": before["shield_width_m"], "height": before["shield_height_m"]},
        "shield_size_after_m": {"width": after["shield_width_m"], "height": after["shield_height_m"]},
        "corrections": {
            "shield_local_offset_m": list(SHIELD_LOCAL_OFFSET),
            "shield_review_attachment_offset_m": list(SHIELD_REVIEW_OFFSET),
            "grip_fitting_offset_m": list(GRIP_FITTING_OFFSET),
            "strap_fitting_offset_m": list(STRAP_FITTING_OFFSET),
            "left_upper_rotation_degrees": LEFT_UPPER_DEGREES,
            "left_lower_rotation_degrees": LEFT_LOWER_DEGREES,
            "shield_pitch_degrees": SHIELD_PITCH_DEGREES,
            "shield_inward_degrees": SHIELD_INWARD_DEGREES,
            "pivot_or_socket_changed": False,
            "shield_front_geometry_changed": False,
            "body_proportion_changed": False,
            "sword_changed": False,
        },
        "before_alignment": before,
        "after_alignment": after,
        "preserved_measurement": base_measurement,
        "after_measurement": after_measurement,
        "deferred": ["Phase 04", "Final UV", "Final Texture", "Final Skinning", "Animation Polish", "Runtime Prefab replacement"],
    }
    (documentation / "P035R2_BUILD_RESULT.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print("AEGIS_P035R2_BUILD_COMPLETE", json.dumps({"top": after["shield_top_y_normalized"], "center": after["shield_center_y_normalized"], "bottom": after["shield_bottom_y_normalized"]}))


if __name__ == "__main__":
    main()
