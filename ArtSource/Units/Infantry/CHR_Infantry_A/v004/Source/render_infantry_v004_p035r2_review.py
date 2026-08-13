"""Render P035R1-before and P035R2 shield-alignment evidence without saving."""
import argparse
import importlib.util
import json
import shutil
from pathlib import Path

import bpy
from mathutils import Vector


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
    return load_module(path, "p035r2_render_base")


def load_build():
    return load_module(Path(__file__).resolve().with_name("build_infantry_v004_p035r2.py"), "p035r2_build")


def apply_pose(meshes, armature, mode):
    build = load_build()
    original = {obj: obj.matrix_world.copy() for obj in meshes}
    for obj in meshes:
        obj.matrix_world = build.object_pose_matrix(obj, armature, mode) @ obj.matrix_world
    bpy.context.view_layer.update()
    return original


def restore_pose(original):
    for obj, matrix in original.items():
        obj.matrix_world = matrix
    bpy.context.view_layer.update()


def render_views(base, scene, camera, center, directory):
    views = (
        ("01_L1Pose_Front.png", (0, -1, 0), 2.08),
        ("02_L1Pose_Left.png", (-1, 0, .04), 2.08),
        ("03_L1Pose_Back.png", (0, 1, 0), 2.08),
        ("04_L1Pose_3Q_Front.png", (-1, -1, .10), 2.08),
        ("05_L1Pose_3Q_Back.png", (1, 1, .10), 2.08),
    )
    for name, direction, scale in views:
        base.render(scene, camera, center, direction, directory / name, scale, (768, 768))


def render_focus(base, scene, camera, center, directory):
    targets = (
        ("Shield_Front_Focus.png", Vector((-0.32, center.y, .89)), (0, -1, 0), 1.15),
        ("Shield_3Q_Focus.png", Vector((-0.28, center.y, .93)), (-1, -1, .06), 1.18),
        ("Shield_LeftSide_Focus.png", Vector((-0.30, center.y, .94)), (-1, 0, .03), 1.18),
        ("Shield_Back_WithArm.png", Vector((-0.30, center.y, 1.00)), (0, 1, .02), 1.20),
    )
    for name, target, direction, scale in targets:
        base.render(scene, camera, target, direction, directory / name, scale, (768, 768))


def main():
    args = arguments()
    root = Path(args.output_root).resolve()
    expected = "CHR_Infantry_A_v004_P035R1" if args.mode == "before" else "CHR_Infantry_A_v004_P035R2"
    if bpy.context.scene.get("SourceVersion") != expected:
        raise RuntimeError(f"Expected {expected}, found {bpy.context.scene.get('SourceVersion')}")
    base = load_base()
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH" and not obj.name.startswith("REVIEW_")]
    armatures = [obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE"]
    if len(armatures) != 1:
        raise RuntimeError("Expected one armature")
    low, high = base.world_bounds(meshes)
    scene, camera, _, ground = base.setup_scene(low, high)
    scene.render.film_transparent = True
    scene.render.image_settings.color_mode = "RGBA"
    ground.hide_render = True
    base.set_override(meshes, base.material("MAT_P035R2_Clay", (.52, .56, .62), .84))
    original = apply_pose(meshes, armatures[0], args.mode)
    posed_low, posed_high = base.world_bounds(meshes)
    center = (posed_low + posed_high) * .5
    comparison = root / "Screenshots" / "Comparison"
    comparison.mkdir(parents=True, exist_ok=True)
    if args.mode == "before":
        base.render(scene, camera, center, (0, -1, 0), comparison / "P035R1_L1Pose_Front.png", 2.08, (768, 768))
        base.render(scene, camera, center, (-1, -1, .10), comparison / "P035R1_L1Pose_3Q.png", 2.08, (768, 768))
    else:
        l1pose = root / "Screenshots" / "L1Pose"
        focus = root / "Screenshots" / "ShieldFocus"
        grip = root / "Screenshots" / "Grip"
        screen = root / "Screenshots" / "ScreenSize"
        render_views(base, scene, camera, center, l1pose)
        render_focus(base, scene, camera, center, focus)
        base.render(scene, camera, Vector((-0.38, center.y, 1.05)), (0, 1, .01), grip / "Shield_Grip_Close.png", .82, (768, 768))
        shutil.copy2(l1pose / "01_L1Pose_Front.png", comparison / "P035R2_L1Pose_Front.png")
        shutil.copy2(l1pose / "04_L1Pose_3Q_Front.png", comparison / "P035R2_L1Pose_3Q.png")
        for size in (64, 32):
            base.render(scene, camera, center, (0, -1, 0), screen / f"Final_L1Pose_{size}px.png", (posed_high.z - posed_low.z) * 256 / size, (256, 256))
    restore_pose(original)
    print("AEGIS_P035R2_RENDER_COMPLETE", json.dumps({"mode": args.mode, "source": expected}))


if __name__ == "__main__":
    main()
