"""Build the measured Phase 03.5 proportion candidate from immutable P03R1."""
import argparse
import hashlib
import json
import math
from pathlib import Path

import bpy
from mathutils import Vector


STATUS = "READY FOR PHASE03_5 REVIEW"
EXPECTED_SHA = "C6429918EA147E65713B31EF9D6940EC313C46DA1D2F4404032CA253B4B72F31"
Z_POINTS = (
    (0.000000000, 0.000000000),
    (0.137251094, 0.137251094),
    (0.521554172, 0.455000000),
    (0.805206418, 0.650000000),
    (0.988207877, 0.988207877),
    (1.171209335, 1.171209335),
    (1.381661057, 1.381661057),
    (1.537212253, 1.537212253),
    (1.793414354, 1.793414354),
    (1.824010849, 1.824010849),
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


def z_map(value):
    if value <= Z_POINTS[0][0]:
        return Z_POINTS[0][1] + value - Z_POINTS[0][0]
    for (x0, y0), (x1, y1) in zip(Z_POINTS, Z_POINTS[1:]):
        if value <= x1:
            ratio = (value - x0) / (x1 - x0)
            return y0 + ratio * (y1 - y0)
    return Z_POINTS[-1][1] + value - Z_POINTS[-1][0]


def mesh_triangles(obj):
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)


def object_points(objects):
    return [obj.matrix_world @ Vector(corner) for obj in objects for corner in obj.bound_box]


def bounds(objects):
    points = object_points(objects)
    low = Vector(tuple(min(point[i] for point in points) for i in range(3)))
    high = Vector(tuple(max(point[i] for point in points) for i in range(3)))
    return low, high


def group(meshes, predicate):
    return [obj for obj in meshes if predicate(obj.name)]


def group_dimensions(meshes, predicate):
    selected = group(meshes, predicate)
    low, high = bounds(selected)
    return high - low


def side_name(name, side):
    return name.endswith("_" + side) or ("_" + side + "_") in name


def bone_point(armature, name, endpoint="head"):
    bone = armature.data.bones[name]
    point = bone.head_local if endpoint == "head" else bone.tail_local
    return armature.matrix_world @ point


def segment(armature, bone_name):
    bone = armature.data.bones[bone_name]
    return (bone.tail_local - bone.head_local).length


def measurement(meshes, armature):
    low, high = bounds(meshes)
    height = high.z - low.z
    shoulder_l = bone_point(armature, "LeftUpperArm")
    shoulder_r = bone_point(armature, "RightUpperArm")
    hip = bone_point(armature, "LeftUpperLeg")
    knee = bone_point(armature, "LeftLowerLeg")
    ankle = bone_point(armature, "LeftFoot")
    head_low, head_high = bounds(group(meshes, lambda n: n == "Head"))
    helmet_low, helmet_high = bounds(group(meshes, lambda n: ("Helmet" in n and "Plume" not in n)))
    belt_low, belt_high = bounds(group(meshes, lambda n: n == "WaistArmor_Belt"))
    chest_low, chest_high = bounds(group(meshes, lambda n: n.startswith("ChestArmor") or n.startswith("GEO_Infantry_Chest")))
    plume_high = max((armature.matrix_world @ Vector(corner)).z for obj in group(meshes, lambda n: "Plume" in n) for corner in obj.bound_box)
    dims = {
        "head": group_dimensions(meshes, lambda n: n == "Head"),
        "helmet": group_dimensions(meshes, lambda n: "Helmet" in n),
        "neck": group_dimensions(meshes, lambda n: n == "Neck"),
        "shoulder_armor": group_dimensions(meshes, lambda n: "Shoulder" in n),
        "chest": group_dimensions(meshes, lambda n: n.startswith("ChestArmor") or n.startswith("GEO_Infantry_Chest")),
        "waist": group_dimensions(meshes, lambda n: "WaistArmor" in n or "Waist_Attachment" in n),
        "hip": group_dimensions(meshes, lambda n: n == "Pelvis"),
        "hand_l": group_dimensions(meshes, lambda n: n in ("Hand_L", "Thumb_L")),
        "thigh_l": group_dimensions(meshes, lambda n: n == "Thigh_L"),
        "calf_l": group_dimensions(meshes, lambda n: n == "Calf_L"),
        "boot_l": group_dimensions(meshes, lambda n: n in ("Boot_L", "BootSole_L")),
        "shield": group_dimensions(meshes, lambda n: "Shield" in n),
        "sword": group_dimensions(meshes, lambda n: "Sword" in n),
        "upper_arm_l": group_dimensions(meshes, lambda n: n == "UpperArm_L"),
        "forearm_l": group_dimensions(meshes, lambda n: n == "Forearm_L" or ("Bracer" in n and side_name(n, "L"))),
    }
    vertical = {
        "L00_Ground": low.z,
        "L01_Ankle": ankle.z,
        "L02_Knee": knee.z,
        "L03_Crotch_HipJoint": hip.z,
        "L04_Belt_Waist": (belt_low.z + belt_high.z) * .5,
        "L05_ChestCenter": (chest_low.z + chest_high.z) * .5,
        "L06_ShoulderJoint": (shoulder_l.z + shoulder_r.z) * .5,
        "L07_Chin": head_low.z,
        "L08_HeadTop": head_high.z,
        "L09_HelmetTop": helmet_high.z,
        "L10_PlumeTop": plume_high,
    }
    widths = {
        "head_width": dims["head"].x,
        "helmet_width": dims["helmet"].x,
        "neck_width": dims["neck"].x,
        "anatomical_shoulder_width": abs(shoulder_r.x - shoulder_l.x),
        "armored_shoulder_width": dims["shoulder_armor"].x,
        "chest_width": dims["chest"].x,
        "waist_width": dims["waist"].x,
        "hip_width": dims["hip"].x,
        "upper_arm_max_width": dims["upper_arm_l"].y,
        "forearm_max_width": dims["forearm_l"].y,
        "hand_width": dims["hand_l"].x,
        "thigh_width": dims["thigh_l"].x,
        "calf_width": dims["calf_l"].x,
        "boot_width": dims["boot_l"].x,
    }
    segments = {
        "upper_arm_length": segment(armature, "LeftUpperArm"),
        "forearm_length": segment(armature, "LeftLowerArm"),
        "hand_length": segment(armature, "LeftHand"),
        "torso_length": vertical["L06_ShoulderJoint"] - vertical["L03_Crotch_HipJoint"],
        "upper_leg_length": segment(armature, "LeftUpperLeg"),
        "lower_leg_length": segment(armature, "LeftLowerLeg"),
        "leg_length": vertical["L03_Crotch_HipJoint"] - vertical["L00_Ground"],
        "foot_length": segment(armature, "LeftFoot") + segment(armature, "LeftToes"),
    }
    arm_landmarks = {}
    for side, prefix in (("left", "Left"), ("right", "Right")):
        arm_landmarks[side] = {
            "shoulder_joint": list(bone_point(armature, prefix + "UpperArm")),
            "elbow_joint": list(bone_point(armature, prefix + "LowerArm")),
            "wrist_joint": list(bone_point(armature, prefix + "Hand")),
            "palm_center": list((bone_point(armature, prefix + "Hand") + bone_point(armature, prefix + "Hand", "tail")) * .5),
            "hand_end": list(bone_point(armature, prefix + "Hand", "tail")),
        }
    return {
        "character_height_m": height,
        "bounds": {"min": list(low), "max": list(high), "dimensions": list(high - low)},
        "vertical_landmarks_m": vertical,
        "vertical_landmarks_normalized": {key: (value - low.z) / height for key, value in vertical.items()},
        "arm_landmarks_world_m": arm_landmarks,
        "horizontal_measurements_m": widths,
        "horizontal_measurements_normalized": {key: value / height for key, value in widths.items()},
        "segment_lengths_m": segments,
        "segment_lengths_normalized": {key: value / height for key, value in segments.items()},
        "weapon_measurements_m": {
            "shield_width": dims["shield"].x,
            "shield_height": dims["shield"].z,
            "sword_projected_overall_length": max(dims["sword"]),
        },
    }


def l1_measurements(path, height):
    data = json.loads(Path(path).read_text(encoding="utf-8"))
    char_px = data["character_bounds_px"]["normalized_character_height_px"]
    ground = data["character_bounds_px"]["ground_y"]
    vertical = {key: (ground - item["y"]) / char_px for key, item in data["vertical_landmarks"].items()}
    widths = {key: item["value"] / char_px for key, item in data["horizontal_measurements_px"].items()}
    sword = data["arm_landmarks"]["sword_side"]
    distance = lambda a, b: math.dist((a["x"], a["y"]), (b["x"], b["y"])) / char_px
    segments = {
        "upper_arm_length": distance(sword["shoulder_joint"], sword["elbow_joint"]),
        "forearm_length": distance(sword["elbow_joint"], sword["wrist_joint"]),
        "hand_length": distance(sword["wrist_joint"], sword["hand_end"]),
        "torso_length": vertical["L06_ShoulderJoint"] - vertical["L03_Crotch_HipJoint"],
        "upper_leg_length": vertical["L03_Crotch_HipJoint"] - vertical["L02_Knee"],
        "lower_leg_length": vertical["L02_Knee"] - vertical["L01_Ankle"],
        "leg_length": vertical["L03_Crotch_HipJoint"],
    }
    return {
        "character_height_reference_m": height,
        "vertical_landmarks_normalized": vertical,
        "horizontal_measurements_normalized": widths,
        "segment_lengths_normalized": segments,
        "horizontal_measurements_m_at_1_824m": {key: value * height for key, value in widths.items()},
        "segment_lengths_m_at_1_824m": {key: value * height for key, value in segments.items()},
    }


def transform_vertices(obj, transform):
    inverse = obj.matrix_world.inverted()
    for vertex in obj.data.vertices:
        world = obj.matrix_world @ vertex.co
        vertex.co = inverse @ transform(world)
    obj.data.update()


def scale_world(obj, sx=1.0, sy=1.0, sz=1.0):
    low, high = bounds([obj])
    center = (low + high) * .5
    transform_vertices(obj, lambda point: Vector((center.x + (point.x - center.x) * sx,
                                                     center.y + (point.y - center.y) * sy,
                                                     center.z + (point.z - center.z) * sz)))


def apply_corrections(meshes):
    equipment = [obj for obj in meshes if "Shield" in obj.name or "Sword" in obj.name]
    for obj in meshes:
        if obj in equipment:
            continue
        transform_vertices(obj, lambda point: Vector((point.x, point.y, z_map(point.z))))
    for obj in meshes:
        name = obj.name
        if name == "Head":
            scale_world(obj, .93, .97, .93)
        if name == "Body_Base" or name.startswith("ChestArmor") or name.startswith("GEO_Infantry_Chest"):
            scale_world(obj, .94, 1.0, 1.0)
        if "Shoulder" in name:
            scale_world(obj, .98, 1.0, 1.0)
        if name.startswith(("Hand_", "Thumb_")):
            scale_world(obj, .93, .93, .93)
        if name.startswith("Boot"):
            scale_world(obj, .94, .94, 1.0)


def apply_skeleton(armature):
    bpy.context.view_layer.objects.active = armature
    armature.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    original = {bone.name: (bone.head.copy(), bone.tail.copy()) for bone in armature.data.edit_bones}
    for bone in armature.data.edit_bones:
        if bone.name == "Root":
            continue
        # Connected edit-bone endpoints are shared.  Always map the captured
        # baseline coordinates so a parent-tail update cannot be mapped twice
        # when the child is visited later.
        head, tail = original[bone.name]
        head.z = z_map(head.z)
        tail.z = z_map(tail.z)
        bone.head, bone.tail = head, tail
    bpy.ops.object.mode_set(mode="OBJECT")


def create_review_action(armature):
    action = bpy.data.actions.get("REVIEW_ONLY_POSE_L1_COMPARE") or bpy.data.actions.new("REVIEW_ONLY_POSE_L1_COMPARE")
    action.use_fake_user = True
    action["REVIEW_ONLY"] = True
    action["Purpose"] = "Pose-vs-proportion comparison; never export as gameplay idle"
    marker = action.pose_markers.get("POSE_L1_COMPARE") or action.pose_markers.new("POSE_L1_COMPARE")
    marker.frame = 1
    armature.animation_data_create()
    armature.animation_data.action = action
    bpy.context.scene.frame_set(1)
    for name, degrees in (("LeftUpperArm", -46), ("LeftLowerArm", -6), ("RightUpperArm", 46), ("RightLowerArm", 6)):
        pose_bone = armature.pose.bones[name]
        pose_bone.rotation_mode = "XYZ"
        pose_bone.rotation_euler = (0.0, math.radians(degrees), 0.0)
        pose_bone.keyframe_insert("rotation_euler", frame=1, group=name)
    armature.animation_data.action = None
    for pose_bone in armature.pose.bones:
        pose_bone.rotation_mode = "QUATERNION"
        pose_bone.rotation_quaternion = (1.0, 0.0, 0.0, 0.0)


def main():
    args = arguments()
    source = Path(bpy.data.filepath).resolve()
    if sha256(source) != EXPECTED_SHA:
        raise RuntimeError("P03R1 input hash mismatch; refusing to build")
    scene = bpy.context.scene
    if scene.get("SourceVersion") != "CHR_Infantry_A_v004_P03R1":
        raise RuntimeError("Opened source is not CHR_Infantry_A_v004_P03R1")
    meshes = [obj for obj in scene.objects if obj.type == "MESH"]
    armatures = [obj for obj in scene.objects if obj.type == "ARMATURE"]
    if len(meshes) != 98 or len(armatures) != 1:
        raise RuntimeError(f"Unexpected baseline: meshes={len(meshes)}, armatures={len(armatures)}")
    armature = armatures[0]
    before = measurement(meshes, armature)
    l1 = l1_measurements(Path(args.measurements) / "L1_Landmarks_Front.json", 1.824010849)

    apply_corrections(meshes)
    apply_skeleton(armature)
    create_review_action(armature)
    bpy.context.view_layer.update()
    after = measurement(meshes, armature)
    if not 1.814010849 <= after["character_height_m"] <= 1.834010849:
        raise RuntimeError(f"Height lock failed: {after['character_height_m']}")
    if len(armature.data.bones) != 23:
        raise RuntimeError("Bone count changed")

    scene["SourceVersion"] = "CHR_Infantry_A_v004_P035"
    scene["SourceBaseline"] = "CHR_Infantry_A_v004_P03R1"
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
    (measurements / "3D_Landmarks_Before.json").write_text(json.dumps(before, indent=2), encoding="utf-8")
    final_measurement = {
        "status": STATUS,
        "source_version": "CHR_Infantry_A_v004_P035",
        "measurement_method": "Blender armature bone head/tail world coordinates plus mesh world bounds",
        "data": after,
    }
    (measurements / "3D_Landmarks.json").write_text(json.dumps(final_measurement, indent=2), encoding="utf-8")
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
            "triangles": sum(mesh_triangles(obj) for obj in meshes),
            "bones": len(armature.data.bones),
            "review_actions": 1,
        },
        "diagnostic": {
            "main_pose_difference": "A-pose arm spread magnified apparent arm length and shoulder width; L1 compare pose removes most of that difference.",
            "measured_true_mismatches": ["hip height", "knee height", "upper-leg ratio", "chest width", "head mesh width/height", "hand width", "boot width"],
            "within_tolerance_or_preserved": ["arm segment lengths", "anatomical shoulder width", "helmet construction/width", "shield size", "sword size", "overall height"],
        },
        "corrections": {
            "vertical_piecewise_map_m": [{"before": x, "after": y} for x, y in Z_POINTS],
            "head_mesh_uniform": .93,
            "chest_body_width": .94,
            "shoulder_armor_width": .98,
            "hand_uniform": .93,
            "boot_width_and_length": .94,
            "shield_size": "PRESERVED",
            "sword_size": "PRESERVED",
            "upper_arm_length": "PRESERVED",
            "forearm_length": "PRESERVED",
        },
        "l1_measurements": l1,
        "before": before,
        "after": after,
        "deferred": ["Phase 04", "Final UV", "Final Texture", "Final Skinning", "Animation Polish", "Formal LOD", "Runtime Prefab replacement"],
    }
    (documentation / "P035_BUILD_RESULT.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print("AEGIS_P035_BUILD_COMPLETE", json.dumps(result["geometry"]))


if __name__ == "__main__":
    main()
