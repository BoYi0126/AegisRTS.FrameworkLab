"""Render orthographic Phase 03.5 A-pose and temporary L1 comparison-pose evidence.

The opened blend is never saved.  The comparison pose is applied only to in-memory
object matrices because the current review meshes are static object-bound geometry.
"""
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


def load_base():
    path = Path(__file__).resolve().parents[2] / "v003" / "Source" / "render_infantry_v003_p02r1_review.py"
    spec = importlib.util.spec_from_file_location("p035_base", path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def side_name(name, side):
    return name.endswith("_" + side) or ("_" + side + "_") in name


def around(point, angle):
    return Matrix.Translation(point) @ Matrix.Rotation(angle, 4, "Y") @ Matrix.Translation(-point)


def apply_l1_pose(meshes, armature):
    original = {obj: obj.matrix_world.copy() for obj in meshes}
    bones = armature.data.bones
    for side, sign in (("L", -1.0), ("R", 1.0)):
        shoulder = armature.matrix_world @ bones[f"{('Left' if side == 'L' else 'Right')}UpperArm"].head_local
        elbow = armature.matrix_world @ bones[f"{('Left' if side == 'L' else 'Right')}LowerArm"].head_local
        hand = armature.matrix_world @ bones[f"{('Left' if side == 'L' else 'Right')}Hand"].head_local
        upper = around(shoulder, math.radians(46.0 * sign))
        new_elbow = upper @ elbow
        lower = around(new_elbow, math.radians(6.0 * sign))
        combined = lower @ upper
        upper_group = [obj for obj in meshes if side_name(obj.name, side) and (obj.name.startswith("UpperArm_") or obj.name.startswith("Elbow_"))]
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
                # Keep shield construction frontal.  The sword receives an
                # additional review-only outward/downward presentation angle
                # so its tip remains above ground after the arm is lowered.
                obj.matrix_world = moved if side == "L" else around(new_hand, math.radians(-25.0)) @ moved
    bpy.context.view_layer.update()
    return original


def restore_pose(original):
    for obj, matrix in original.items():
        obj.matrix_world = matrix
    bpy.context.view_layer.update()


def render_set(base, scene, camera, center, directory, prefix, scale):
    views = (
        ("Front", (0, -1, 0)),
        ("Left", (-1, 0, 0)),
        ("Back", (0, 1, 0)),
        ("3Q", (-1, -1, .10)),
    )
    for name, direction in views:
        base.render(scene, camera, center, direction, directory / f"{prefix}_{name}.png", scale, (768, 768))


def main():
    args = arguments()
    root = Path(args.output_root).resolve()
    expected = "CHR_Infantry_A_v004_P03R1" if args.mode == "before" else "CHR_Infantry_A_v004_P035"
    if bpy.context.scene.get("SourceVersion") != expected:
        raise RuntimeError(f"Expected {expected}, found {bpy.context.scene.get('SourceVersion')}")
    base = load_base()
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH" and not obj.name.startswith("REVIEW_")]
    armatures = [obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE"]
    if len(armatures) != 1:
        raise RuntimeError(f"Expected one armature, found {len(armatures)}")
    low, high = base.world_bounds(meshes)
    center = Vector(((low.x + high.x) * .5, (low.y + high.y) * .5, (low.z + high.z) * .5))
    scene, camera, _, ground = base.setup_scene(low, high)
    scene.render.film_transparent = True
    scene.render.image_settings.color_mode = "RGBA"
    ground.hide_render = True
    clay = base.material("MAT_P035_Clay", (.52, .56, .62), .84)
    base.set_override(meshes, clay)
    scale = 2.08

    if args.mode == "before":
        diagnostic = root / "Screenshots" / "Diagnostic"
        comparison = root / "Screenshots" / "Comparison"
        render_set(base, scene, camera, center, diagnostic, "Apose", scale)
        original = apply_l1_pose(meshes, armatures[0])
        posed_low, posed_high = base.world_bounds(meshes)
        posed_center = (posed_low + posed_high) * .5
        render_set(base, scene, camera, posed_center, diagnostic, "L1Pose", scale)
        restore_pose(original)
        comparison.mkdir(parents=True, exist_ok=True)
        for source, target in (
            ("Apose_Front.png", "Before_Apose_Front.png"),
            ("Apose_3Q.png", "Before_Apose_3Q.png"),
            ("L1Pose_Front.png", "Before_L1Pose_Front.png"),
            ("L1Pose_3Q.png", "Before_L1Pose_3Q.png"),
        ):
            shutil.copy2(diagnostic / source, comparison / target)
    else:
        apose = root / "Screenshots" / "Apose"
        l1pose = root / "Screenshots" / "L1Pose"
        comparison = root / "Screenshots" / "Comparison"
        screen = root / "Screenshots" / "ScreenSize"
        render_set(base, scene, camera, center, apose, "Final_Apose", scale)
        original = apply_l1_pose(meshes, armatures[0])
        posed_low, posed_high = base.world_bounds(meshes)
        posed_center = (posed_low + posed_high) * .5
        render_set(base, scene, camera, posed_center, l1pose, "Final_L1Pose", scale)
        comparison.mkdir(parents=True, exist_ok=True)
        for source, target in (
            (apose / "Final_Apose_Front.png", "After_Apose_Front.png"),
            (apose / "Final_Apose_3Q.png", "After_Apose_3Q.png"),
            (l1pose / "Final_L1Pose_Front.png", "After_L1Pose_Front.png"),
            (l1pose / "Final_L1Pose_3Q.png", "After_L1Pose_3Q.png"),
        ):
            shutil.copy2(source, comparison / target)
        for size in (64, 32):
            base.render(scene, camera, posed_center, (0, -1, 0), screen / f"Final_L1Pose_{size}px.png",
                        (posed_high.z - posed_low.z) * 256 / size, (256, 256))
        restore_pose(original)

    print("AEGIS_P035_RENDER_COMPLETE", json.dumps({"mode": args.mode, "source": expected}, ensure_ascii=False))


if __name__ == "__main__":
    main()
