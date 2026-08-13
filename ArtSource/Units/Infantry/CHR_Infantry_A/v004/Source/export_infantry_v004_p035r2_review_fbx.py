"""Export isolated P035R2 A-pose and review-only L1-pose FBXs."""
import argparse
import importlib.util
from pathlib import Path

import bpy


def arguments():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", required=True)
    parser.add_argument("--pose", choices=("apose", "l1pose"), required=True)
    argv = __import__("sys").argv
    return parser.parse_args(argv[argv.index("--") + 1 :] if "--" in argv else [])


def load_render():
    path = Path(__file__).resolve().with_name("render_infantry_v004_p035r2_review.py")
    spec = importlib.util.spec_from_file_location("p035r2_render_export", path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def main():
    args = arguments()
    scene = bpy.context.scene
    if scene.get("SourceVersion") != "CHR_Infantry_A_v004_P035R2":
        raise RuntimeError("Expected CHR_Infantry_A_v004_P035R2")
    meshes = [obj for obj in scene.objects if obj.type == "MESH" and not obj.name.startswith("REVIEW_")]
    armatures = [obj for obj in scene.objects if obj.type == "ARMATURE"]
    if len(armatures) != 1:
        raise RuntimeError("Expected one armature")
    if args.pose == "l1pose":
        load_render().apply_pose(meshes, armatures[0], "final")
    bpy.ops.object.select_all(action="DESELECT")
    for obj in meshes + armatures:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = armatures[0]
    output = Path(args.output).resolve()
    output.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.export_scene.fbx(filepath=str(output), use_selection=True, object_types={"ARMATURE", "MESH"}, apply_scale_options="FBX_SCALE_ALL", use_space_transform=True, add_leaf_bones=False, bake_anim=False, path_mode="AUTO")
    print(f"AEGIS_P035R2_FBX_COMPLETE pose={args.pose} path={output} objects={len(meshes)+1}")


if __name__ == "__main__":
    main()
