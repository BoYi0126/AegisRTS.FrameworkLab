"""Render focused P035R1 arm/head/hand evidence without saving the opened blend."""
import argparse
import importlib.util
import json
import math
import shutil
from pathlib import Path

import bpy
from mathutils import Matrix, Vector


def arguments():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output-root", required=True)
    parser.add_argument("--mode", choices=("before", "final"), required=True)
    argv = __import__("sys").argv
    return parser.parse_args(argv[argv.index("--") + 1 :] if "--" in argv else [])


def load_module(path, name):
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def load_base():
    path = Path(__file__).resolve().parents[2] / "v003" / "Source" / "render_infantry_v003_p02r1_review.py"
    return load_module(path, "p035r1_render_base")


def load_build():
    return load_module(Path(__file__).resolve().with_name("build_infantry_v004_p035r1.py"), "p035r1_build")


def side_name(name, side):
    return name.endswith("_" + side) or ("_" + side + "_") in name


def around(point, angle):
    return Matrix.Translation(point) @ Matrix.Rotation(angle, 4, "Y") @ Matrix.Translation(-point)


def apply_l1_pose(meshes, armature):
    original = {obj: obj.matrix_world.copy() for obj in meshes}
    bones = armature.data.bones
    build = load_build()
    for side, sign in (("L", -1.0), ("R", 1.0)):
        prefix = "Left" if side == "L" else "Right"
        shoulder = armature.matrix_world @ bones[prefix + "UpperArm"].head_local
        elbow = armature.matrix_world @ bones[prefix + "LowerArm"].head_local
        hand = armature.matrix_world @ bones[prefix + "Hand"].head_local
        upper = around(shoulder, math.radians(build.POSE_UPPER_DEGREES * sign))
        new_elbow = upper @ elbow
        lower = around(new_elbow, math.radians(build.POSE_LOWER_DEGREES * sign))
        combined = lower @ upper
        upper_group = [obj for obj in meshes if side_name(obj.name, side) and obj.name.startswith(("UpperArm_", "Elbow_"))]
        lower_group = [obj for obj in meshes if side_name(obj.name, side) and (
            obj.name.startswith(("Forearm_", "Bracer_", "Hand_", "Thumb_")) or "Bracer_" in obj.name
        )]
        for obj in upper_group:
            obj.matrix_world = upper @ obj.matrix_world
        for obj in lower_group:
            obj.matrix_world = combined @ obj.matrix_world
        new_hand = combined @ hand
        delta = new_hand - hand
        equipment_key = "Shield" if side == "L" else "Sword"
        for obj in meshes:
            if equipment_key in obj.name:
                moved = obj.matrix_world.copy()
                moved.translation += delta
                # Refit the preserved sword around the new hand landmark. The
                # longer arm needs a slightly steeper review-only presentation
                # angle so the blade remains downward-readable without putting
                # its tip materially below the ground plane.
                obj.matrix_world = moved if side == "L" else around(new_hand, math.radians(-34.0)) @ moved
    bpy.context.view_layer.update()
    return original


def restore_pose(original):
    for obj, matrix in original.items():
        obj.matrix_world = matrix
    bpy.context.view_layer.update()


def render_pair(base, scene, camera, center, directory, prefix, scale):
    for name, direction in (("Front", (0, -1, 0)), ("3Q", (-1, -1, .10))):
        base.render(scene, camera, center, direction, directory / f"{prefix}_{name}.png", scale, (768, 768))


def arm_detail(base, scene, camera, posed_center, directory):
    front_z = posed_center.z + 0.02
    base.render(scene, camera, Vector((0.38, posed_center.y, front_z)), (0, -1, 0),
                directory / "Arm_SwordSide_Front.png", 1.12, (768, 768))
    base.render(scene, camera, Vector((0.32, posed_center.y, front_z)), (-1, -1, .08),
                directory / "Arm_SwordSide_3Q.png", 1.12, (768, 768))
    base.render(scene, camera, Vector((-0.38, posed_center.y, front_z)), (0, -1, 0),
                directory / "Arm_ShieldSide_Front.png", 1.12, (768, 768))


def make_emissive(name, color):
    material = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    material.diffuse_color = (*color, 1.0)
    material.use_nodes = True
    node = material.node_tree.nodes.get("Principled BSDF")
    node.inputs["Base Color"].default_value = (*color, 1.0)
    node.inputs["Emission Color"].default_value = (*color, 1.0)
    node.inputs["Emission Strength"].default_value = 2.5
    return material


def create_curve(name, points, material, front_y):
    curve = bpy.data.curves.new(name + "_Data", "CURVE")
    curve.dimensions = "3D"
    curve.bevel_depth = .008
    curve.bevel_resolution = 2
    spline = curve.splines.new("POLY")
    spline.points.add(len(points) - 1)
    for item, point in zip(spline.points, points):
        item.co = (point.x, front_y, point.z, 1.0)
    obj = bpy.data.objects.new(name, curve)
    bpy.context.scene.collection.objects.link(obj)
    obj.data.materials.append(material)
    return obj


def render_skeleton_overlay(base, scene, camera, armature, meshes, posed_center, output):
    build = load_build()
    front_y = min((obj.matrix_world @ Vector(corner)).y for obj in meshes for corner in obj.bound_box) - .08
    material = make_emissive("MAT_P035R1_ArmLandmarks", (1.0, .16, .06))
    marker_objects = []
    for side in ("L", "R"):
        points = build.posed_arm(build.arm_points(armature, side), side)
        ordered = [points[key] for key in ("shoulder", "elbow", "wrist", "hand_end")]
        marker_objects.append(create_curve(f"REVIEW_ArmSkeleton_{side}", ordered, material, front_y))
        for index, point in enumerate(ordered):
            bpy.ops.mesh.primitive_uv_sphere_add(segments=16, ring_count=8, radius=.022,
                                                location=(point.x, front_y, point.z))
            marker = bpy.context.object
            marker.name = f"REVIEW_ArmLandmark_{side}_{index}"
            marker.data.materials.append(material)
            marker_objects.append(marker)
    base.render(scene, camera, posed_center, (0, -1, 0), output, 2.08, (768, 768))
    for obj in marker_objects:
        bpy.data.objects.remove(obj, do_unlink=True)


def main():
    args = arguments()
    root = Path(args.output_root).resolve()
    expected = "CHR_Infantry_A_v004_P035" if args.mode == "before" else "CHR_Infantry_A_v004_P035R1"
    if bpy.context.scene.get("SourceVersion") != expected:
        raise RuntimeError(f"Expected {expected}, found {bpy.context.scene.get('SourceVersion')}")
    base = load_base()
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH" and not obj.name.startswith("REVIEW_")]
    armatures = [obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE"]
    if len(armatures) != 1:
        raise RuntimeError(f"Expected one armature, found {len(armatures)}")
    low, high = base.world_bounds(meshes)
    center = (low + high) * .5
    scene, camera, _, ground = base.setup_scene(low, high)
    scene.render.film_transparent = True
    scene.render.image_settings.color_mode = "RGBA"
    ground.hide_render = True
    clay = base.material("MAT_P035R1_Clay", (.52, .56, .62), .84)
    base.set_override(meshes, clay)
    scale = 2.08
    comparison = root / "Screenshots" / "Comparison"
    comparison.mkdir(parents=True, exist_ok=True)

    if args.mode == "before":
        original = apply_l1_pose(meshes, armatures[0])
        posed_low, posed_high = base.world_bounds(meshes)
        posed_center = (posed_low + posed_high) * .5
        render_pair(base, scene, camera, posed_center, comparison, "P035_L1Pose", scale)
        restore_pose(original)
    else:
        apose = root / "Screenshots" / "Apose"
        l1pose = root / "Screenshots" / "L1Pose"
        details = root / "Screenshots" / "ArmDetail"
        screen = root / "Screenshots" / "ScreenSize"
        render_pair(base, scene, camera, center, apose, "Final_Apose", scale)
        original = apply_l1_pose(meshes, armatures[0])
        posed_low, posed_high = base.world_bounds(meshes)
        posed_center = (posed_low + posed_high) * .5
        render_pair(base, scene, camera, posed_center, l1pose, "Final_L1Pose", scale)
        for source, target in (("Final_L1Pose_Front.png", "P035R1_L1Pose_Front.png"),
                               ("Final_L1Pose_3Q.png", "P035R1_L1Pose_3Q.png")):
            shutil.copy2(l1pose / source, comparison / target)
        arm_detail(base, scene, camera, posed_center, details)
        for size in (64, 32):
            base.render(scene, camera, posed_center, (0, -1, 0), screen / f"Final_L1Pose_{size}px.png",
                        (posed_high.z - posed_low.z) * 256 / size, (256, 256))
        render_skeleton_overlay(base, scene, camera, armatures[0], meshes, posed_center,
                                root / "Screenshots" / "Overlay" / "Skeleton_Arm_Landmarks_Front.png")
        restore_pose(original)

    print("AEGIS_P035R1_RENDER_COMPLETE", json.dumps({"mode": args.mode, "source": expected}))


if __name__ == "__main__":
    main()
