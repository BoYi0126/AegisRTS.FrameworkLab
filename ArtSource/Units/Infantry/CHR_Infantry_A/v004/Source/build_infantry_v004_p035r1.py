"""Build the focused P03.5 Revision 01 arm/head/hand correction from immutable P035."""
import argparse
import hashlib
import importlib.util
import json
import math
from pathlib import Path

import bpy
from mathutils import Matrix, Vector


STATUS = "READY FOR PHASE03_5 REVISION REVIEW"
SOURCE_VERSION = "CHR_Infantry_A_v004_P035"
OUTPUT_VERSION = "CHR_Infantry_A_v004_P035R1"
EXPECTED_SHA = "234383811F66F26DE29C8DBEF5E31C1B65D58B6B1E07C3FD08F3FF0AAF46422B"
UPPER_ARM_RATIO = 0.176
FOREARM_RATIO = 0.165
HAND_WIDTH_RATIO = 0.060
HEAD_WIDTH_RATIO = 0.120
HAND_LENGTH_SCALE = 0.92
HAND_LATERAL_REFIT_FACTOR = 0.90
NECK_WIDTH_SCALE = 0.96
POSE_UPPER_DEGREES = 46.0
POSE_LOWER_DEGREES = 6.0


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


def load_p035_module():
    path = Path(__file__).resolve().with_name("build_infantry_v004_p035.py")
    spec = importlib.util.spec_from_file_location("p035_build", path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def side_name(name, side):
    return name.endswith("_" + side) or ("_" + side + "_") in name


def around(point, angle):
    return Matrix.Translation(point) @ Matrix.Rotation(angle, 4, "Y") @ Matrix.Translation(-point)


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


def revised_arm_points(points, height):
    upper_axis = (points["elbow"] - points["shoulder"]).normalized()
    lower_axis = (points["wrist"] - points["elbow"]).normalized()
    hand_vector = points["hand_end"] - points["wrist"]
    elbow = points["shoulder"] + upper_axis * (height * UPPER_ARM_RATIO)
    wrist = elbow + lower_axis * (height * FOREARM_RATIO)
    hand_end = wrist + hand_vector * HAND_LENGTH_SCALE
    return {
        "shoulder": points["shoulder"].copy(),
        "elbow": elbow,
        "wrist": wrist,
        "hand_end": hand_end,
    }


def posed_arm(points, side):
    sign = -1.0 if side == "L" else 1.0
    upper = around(points["shoulder"], math.radians(POSE_UPPER_DEGREES * sign))
    elbow = upper @ points["elbow"]
    lower = around(elbow, math.radians(POSE_LOWER_DEGREES * sign))
    combined = lower @ upper
    wrist = combined @ points["wrist"]
    hand_end = combined @ points["hand_end"]
    return {
        "shoulder": points["shoulder"].copy(),
        "elbow": elbow,
        "wrist": wrist,
        "palm": (wrist + hand_end) * 0.5,
        "hand_end": hand_end,
    }


def posed_measurement(armature, height, ground):
    sides = {}
    for side, label in (("L", "left_shield_side"), ("R", "right_sword_side")):
        values = posed_arm(arm_points(armature, side), side)
        sides[label] = {
            "world_m": {key: list(value) for key, value in values.items()},
            "vertical_normalized": {key: (value.z - ground) / height for key, value in values.items()},
        }
    return {
        "pose": "REVIEW_ONLY_POSE_L1_COMPARE",
        "height_reference_m": height,
        "ground_m": ground,
        "upper_arm_rotation_degrees": POSE_UPPER_DEGREES,
        "lower_arm_rotation_degrees": POSE_LOWER_DEGREES,
        "sides": sides,
    }


def transform_vertices(obj, transform):
    inverse = obj.matrix_world.inverted()
    for vertex in obj.data.vertices:
        vertex.co = inverse @ transform(obj.matrix_world @ vertex.co)
    obj.data.update()


def axial_map(point, origin, axis, axial_scale, lateral_scale=1.0):
    offset = point - origin
    along = axis * offset.dot(axis)
    lateral = offset - along
    return origin + along * axial_scale + lateral * lateral_scale


def scale_about(point, origin, scale_xyz):
    offset = point - origin
    return origin + Vector((offset.x * scale_xyz[0], offset.y * scale_xyz[1], offset.z * scale_xyz[2]))


def object_center(base, obj):
    low, high = base.bounds([obj])
    return (low + high) * 0.5


def refit_arm_meshes(base, meshes, armature, side, height):
    old = arm_points(armature, side)
    new = revised_arm_points(old, height)
    upper_axis = (old["elbow"] - old["shoulder"]).normalized()
    lower_axis = (old["wrist"] - old["elbow"]).normalized()
    upper_scale = (new["elbow"] - new["shoulder"]).length / (old["elbow"] - old["shoulder"]).length
    lower_scale = (new["wrist"] - new["elbow"]).length / (old["wrist"] - old["elbow"]).length
    elbow_delta = new["elbow"] - old["elbow"]
    wrist_delta = new["wrist"] - old["wrist"]
    hand_axis = (old["hand_end"] - old["wrist"]).normalized()
    current_hand_width = base.group_dimensions(meshes, lambda name: name in (f"Hand_{side}", f"Thumb_{side}")).x
    # The hand axis carries a large X component in the source A-pose, so the
    # longitudinal 8% shortening also contributes to the measured front width.
    # The additional refit factor brings the combined Hand+Thumb bounds to the
    # requested ~0.060H without shortening the palm by more than 10%.
    hand_lateral_scale = height * HAND_WIDTH_RATIO / current_hand_width * HAND_LATERAL_REFIT_FACTOR

    for obj in meshes:
        name = obj.name
        if not side_name(name, side):
            continue
        if name == f"UpperArm_{side}":
            transform_vertices(obj, lambda point: axial_map(point, old["shoulder"], upper_axis, upper_scale))
        elif name == f"Elbow_{side}":
            transform_vertices(obj, lambda point: point + elbow_delta)
        elif name == f"Forearm_{side}" or "Bracer" in name:
            transform_vertices(
                obj,
                lambda point: new["elbow"] + axial_map(point, old["elbow"], lower_axis, lower_scale) - old["elbow"],
            )
        elif name in (f"Hand_{side}", f"Thumb_{side}"):
            def hand_map(point):
                offset = point - old["wrist"]
                along = hand_axis * offset.dot(hand_axis)
                lateral = offset - along
                return new["wrist"] + along * HAND_LENGTH_SCALE + lateral * hand_lateral_scale
            transform_vertices(obj, hand_map)

    old_palm = (old["wrist"] + old["hand_end"]) * 0.5
    new_palm = (new["wrist"] + new["hand_end"]) * 0.5
    equipment_key = "Shield" if side == "L" else "Sword"
    equipment_delta = new_palm - old_palm
    for obj in meshes:
        if equipment_key in obj.name:
            transform_vertices(obj, lambda point: point + equipment_delta)
    return {
        "old": old,
        "new": new,
        "upper_scale": upper_scale,
        "lower_scale": lower_scale,
        "hand_lateral_scale": hand_lateral_scale,
        "equipment_delta": equipment_delta,
        "wrist_delta": wrist_delta,
    }


def revise_head_and_neck(base, meshes, height):
    head = next(obj for obj in meshes if obj.name == "Head")
    head_width = base.group_dimensions(meshes, lambda name: name == "Head").x
    head_scale = height * HEAD_WIDTH_RATIO / head_width
    head_center = object_center(base, head)
    transform_vertices(head, lambda point: scale_about(point, head_center, (head_scale, head_scale, head_scale)))
    neck = next(obj for obj in meshes if obj.name == "Neck")
    neck_center = object_center(base, neck)
    transform_vertices(neck, lambda point: scale_about(point, neck_center, (NECK_WIDTH_SCALE, NECK_WIDTH_SCALE, 1.0)))
    return head_scale


def revise_skeleton(armature, arm_changes):
    bpy.context.view_layer.objects.active = armature
    armature.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    for side, prefix in (("L", "Left"), ("R", "Right")):
        change = arm_changes[side]
        old = change["old"]
        new = change["new"]
        for bone_name, head, tail in (
            (prefix + "UpperArm", new["shoulder"], new["elbow"]),
            (prefix + "LowerArm", new["elbow"], new["wrist"]),
            (prefix + "Hand", new["wrist"], new["hand_end"]),
        ):
            bone = armature.data.edit_bones[bone_name]
            bone.head = armature.matrix_world.inverted() @ head
            bone.tail = armature.matrix_world.inverted() @ tail
    bpy.ops.object.mode_set(mode="OBJECT")


def rebuild_review_action(armature):
    old = bpy.data.actions.get("REVIEW_ONLY_POSE_L1_COMPARE")
    if old is not None:
        bpy.data.actions.remove(old)
    action = bpy.data.actions.new("REVIEW_ONLY_POSE_L1_COMPARE")
    action.use_fake_user = True
    action["REVIEW_ONLY"] = True
    action["Purpose"] = "P035R1 arm-landmark comparison; never export as gameplay idle"
    action["UpperArmRotationDegrees"] = POSE_UPPER_DEGREES
    action["LowerArmRotationDegrees"] = POSE_LOWER_DEGREES
    marker = action.pose_markers.new("POSE_L1_COMPARE")
    marker.frame = 1
    armature.animation_data_create()
    armature.animation_data.action = action
    bpy.context.scene.frame_set(1)
    for name, degrees in (
        ("LeftUpperArm", -POSE_UPPER_DEGREES),
        ("LeftLowerArm", -POSE_LOWER_DEGREES),
        ("RightUpperArm", POSE_UPPER_DEGREES),
        ("RightLowerArm", POSE_LOWER_DEGREES),
    ):
        pose_bone = armature.pose.bones[name]
        pose_bone.rotation_mode = "XYZ"
        pose_bone.rotation_euler = (0.0, math.radians(degrees), 0.0)
        pose_bone.keyframe_insert("rotation_euler", frame=1, group=name)
    armature.animation_data.action = None
    for pose_bone in armature.pose.bones:
        pose_bone.rotation_mode = "QUATERNION"
        pose_bone.rotation_quaternion = (1.0, 0.0, 0.0, 0.0)


def vector_dict(values):
    return {key: list(value) if isinstance(value, Vector) else value for key, value in values.items()}


def main():
    args = arguments()
    source = Path(bpy.data.filepath).resolve()
    if sha256(source) != EXPECTED_SHA:
        raise RuntimeError("P035 input hash mismatch; refusing to build")
    scene = bpy.context.scene
    if scene.get("SourceVersion") != SOURCE_VERSION:
        raise RuntimeError(f"Expected {SOURCE_VERSION}, found {scene.get('SourceVersion')}")
    base = load_p035_module()
    meshes = [obj for obj in scene.objects if obj.type == "MESH"]
    armatures = [obj for obj in scene.objects if obj.type == "ARMATURE"]
    if len(meshes) != 98 or len(armatures) != 1:
        raise RuntimeError(f"Unexpected baseline: meshes={len(meshes)}, armatures={len(armatures)}")
    armature = armatures[0]
    before = base.measurement(meshes, armature)
    before_posed = posed_measurement(armature, before["character_height_m"], before["bounds"]["min"][2])

    arm_changes = {
        side: refit_arm_meshes(base, meshes, armature, side, before["character_height_m"])
        for side in ("L", "R")
    }
    head_scale = revise_head_and_neck(base, meshes, before["character_height_m"])
    revise_skeleton(armature, arm_changes)
    rebuild_review_action(armature)
    bpy.context.view_layer.update()
    after = base.measurement(meshes, armature)
    after_posed = posed_measurement(armature, after["character_height_m"], after["bounds"]["min"][2])

    if not 1.814010849 <= after["character_height_m"] <= 1.834010849:
        raise RuntimeError(f"Height lock failed: {after['character_height_m']}")
    if len(armature.data.bones) != 23:
        raise RuntimeError("Bone count changed")
    if not 0.172 <= after["segment_lengths_normalized"]["upper_arm_length"] <= 0.180:
        raise RuntimeError("UpperArm ratio outside revision target")
    if not 0.162 <= after["segment_lengths_normalized"]["forearm_length"] <= 0.168:
        raise RuntimeError("Forearm ratio outside revision target")
    if not 0.057 <= after["horizontal_measurements_normalized"]["hand_width"] <= 0.062:
        raise RuntimeError(
            f"Hand width outside revision target: {after['horizontal_measurements_normalized']['hand_width']}"
        )
    if not 0.118 <= after["horizontal_measurements_normalized"]["head_width"] <= 0.122:
        raise RuntimeError("Head width outside revision target")

    scene["SourceVersion"] = OUTPUT_VERSION
    scene["SourceBaseline"] = SOURCE_VERSION
    scene["SourceBaselineSHA256"] = EXPECTED_SHA
    scene["ReviewStatus"] = STATUS
    scene["ProductionNeutralPose"] = "POSE_SOURCE_A"
    scene["ReviewOnlyPose"] = "REVIEW_ONLY_POSE_L1_COMPARE"
    scene["FinalUV"] = False
    scene["FinalTexture"] = False
    scene["FinalSkinning"] = False
    scene["AnimationPolish"] = False
    scene["FormalLOD"] = False
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
    before_path = measurements / "3D_L1Pose_Arm_Landmarks_Before.json"
    after_path = measurements / "3D_L1Pose_Arm_Landmarks_After.json"
    before_path.write_text(json.dumps(before_posed, indent=2), encoding="utf-8")
    after_path.write_text(json.dumps(after_posed, indent=2), encoding="utf-8")

    result = {
        "status": STATUS,
        "input": str(source),
        "input_sha256": EXPECTED_SHA,
        "output": str(output),
        "output_sha256": output_hash,
        "blender_version": bpy.app.version_string,
        "geometry": {
            "height_m": after["character_height_m"],
            "meshes": len(meshes),
            "vertices": sum(len(obj.data.vertices) for obj in meshes),
            "triangles": sum(base.mesh_triangles(obj) for obj in meshes),
            "bones": len(armature.data.bones),
            "review_actions": 1,
        },
        "targets": {
            "upper_arm_ratio": UPPER_ARM_RATIO,
            "forearm_ratio": FOREARM_RATIO,
            "combined_shoulder_to_wrist_ratio": UPPER_ARM_RATIO + FOREARM_RATIO,
            "hand_width_ratio": HAND_WIDTH_RATIO,
            "head_width_ratio": HEAD_WIDTH_RATIO,
        },
        "corrections": {
            "upper_arm_scale": arm_changes["L"]["upper_scale"],
            "forearm_scale": arm_changes["L"]["lower_scale"],
            "hand_lateral_scale": arm_changes["L"]["hand_lateral_scale"],
            "hand_length_scale": HAND_LENGTH_SCALE,
            "head_uniform_scale": head_scale,
            "neck_width_scale": NECK_WIDTH_SCALE,
            "left_equipment_translation_m": list(arm_changes["L"]["equipment_delta"]),
            "right_equipment_translation_m": list(arm_changes["R"]["equipment_delta"]),
            "shoulder_origin": "PRESERVED",
            "hip_knee_torso": "PRESERVED",
            "chest_width": "PRESERVED",
            "helmet_outer_silhouette": "PRESERVED",
            "shield_geometry_and_size": "PRESERVED",
            "sword_geometry_and_size": "PRESERVED",
        },
        "before": before,
        "after": after,
        "posed_before": before_posed,
        "posed_after": after_posed,
        "deferred": [
            "Phase 04", "Final UV", "Final Texture", "Final Skinning",
            "Animation Polish", "Formal LOD", "Runtime Prefab replacement",
        ],
    }
    (documentation / "P035R1_BUILD_RESULT.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print("AEGIS_P035R1_BUILD_COMPLETE", json.dumps(result["geometry"]))


if __name__ == "__main__":
    main()
