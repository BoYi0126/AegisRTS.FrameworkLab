"""Render P035R2-before and P035R3 sword attachment evidence."""
import argparse
import importlib.util
import json
from pathlib import Path

import bpy
from mathutils import Vector


SWORD_PARTS = {
    "GEO_Infantry_Sword_GripContact", "Sword", "Sword_Grip", "Sword_Guard",
    "Sword_Pommel", "GEO_Infantry_Sword_BladeSpine", "GEO_Infantry_Sword_GripWraps",
}


def arguments():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output-root", required=True)
    parser.add_argument("--mode", choices=("before", "apose", "l1pose", "follow_up", "follow_down", "follow_3q"), required=True)
    argv = __import__("sys").argv
    return parser.parse_args(argv[argv.index("--") + 1 :] if "--" in argv else [])


def load_module(path, name):
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def load_base():
    path = Path(__file__).resolve().parents[2] / "v003" / "Source" / "render_infantry_v003_p02r1_review.py"
    return load_module(path, "p035r3_render_base")


def load_r2_build():
    return load_module(Path(__file__).resolve().with_name("build_infantry_v004_p035r2.py"), "p035r2_pose_for_r3")


def apply_review_pose(meshes, armature, mode):
    r2 = load_r2_build()
    originals = {obj: obj.matrix_world.copy() for obj in meshes}
    if mode == "apose":
        return originals
    for obj in meshes:
        if obj.name not in SWORD_PARTS:
            obj.matrix_world = r2.object_pose_matrix(obj, armature, "final") @ obj.matrix_world
    for name, degrees in (("LeftUpperArm", -10.0), ("LeftLowerArm", -20.0), ("RightUpperArm", 46.0), ("RightLowerArm", 6.0)):
        bone = armature.pose.bones[name]
        bone.rotation_mode = "XYZ"
        bone.rotation_euler = (0.0, __import__("math").radians(degrees), 0.0)
    if mode in ("follow_up", "follow_down", "follow_3q"):
        degrees = 15.0 if mode in ("follow_up", "follow_3q") else -15.0
        pose = r2.arm_pose(armature, "R", "after")
        hand_matrix = r2.around(pose["wrist"], __import__("math").radians(degrees))
        for obj in meshes:
            if obj.name.startswith(("Hand_R", "Thumb_R")):
                obj.matrix_world = hand_matrix @ obj.matrix_world
        bone = armature.pose.bones["RightHand"]
        bone.rotation_mode = "XYZ"
        bone.rotation_euler = (0.0, __import__("math").radians(degrees), 0.0)
    bpy.context.view_layer.update()
    return originals


def render_close(base, scene, camera, output, direction=(0, -1, 0), scale=.72):
    base.render(scene, camera, Vector((.83, -.045, 1.045)), direction, output, scale, (768, 768))


def main():
    args = arguments()
    root = Path(args.output_root).resolve()
    version = bpy.context.scene.get("SourceVersion")
    expected = "CHR_Infantry_A_v004_P035R2" if args.mode == "before" else "CHR_Infantry_A_v004_P035R3"
    if version != expected:
        raise RuntimeError(f"Expected {expected}, found {version}")
    base = load_base()
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH" and not obj.name.startswith("REVIEW_")]
    armature = next(obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE")
    low, high = base.world_bounds(meshes)
    scene, camera, _, ground = base.setup_scene(low, high)
    scene.render.film_transparent = True
    scene.render.image_settings.color_mode = "RGBA"
    ground.hide_render = True
    base.set_override(meshes, base.material("MAT_P035R3_Clay", (.52, .56, .62), .84))
    pose_mode = "l1pose" if args.mode == "before" else args.mode
    apply_review_pose(meshes, armature, pose_mode)

    if args.mode == "before":
        render_close(base, scene, camera, root / "Screenshots" / "Comparison" / "P035R2_L1Pose_SwordGrip_Close.png")
    elif args.mode == "apose":
        render_close(base, scene, camera, root / "Screenshots" / "Apose" / "Apose_SwordGrip_Close.png")
        render_close(base, scene, camera, root / "Screenshots" / "Apose" / "Blender_SwordGrip_Close.png", (-1, -1, .08), .78)
    elif args.mode == "l1pose":
        render_close(base, scene, camera, root / "Screenshots" / "L1Pose" / "L1Pose_SwordGrip_Close.png")
        base.render(scene, camera, Vector((0, 0, .92)), (-1, -1, .10), root / "Screenshots" / "L1Pose" / "L1Pose_RTS_3Q.png", 2.12, (768, 768))
        render_close(base, scene, camera, root / "Screenshots" / "Follow" / "SwordFollow_Neutral.png", (-1, -1, .08), .78)
    elif args.mode == "follow_up":
        render_close(base, scene, camera, root / "Screenshots" / "Follow" / "SwordFollow_TestUp.png")
    elif args.mode == "follow_down":
        render_close(base, scene, camera, root / "Screenshots" / "Follow" / "SwordFollow_TestDown.png")
    else:
        render_close(base, scene, camera, root / "Screenshots" / "Follow" / "SwordFollow_3Q.png", (-1, -1, .08), .78)
    print("AEGIS_P035R3_RENDER_COMPLETE", json.dumps({"mode": args.mode, "source": version}))


if __name__ == "__main__":
    main()
