"""Export P035R3 A-pose and L1 comparison-pose FBXs with attachment transforms."""
import argparse
import importlib.util
from pathlib import Path

import bpy
from mathutils import Vector


def arguments():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", required=True)
    parser.add_argument("--pose", choices=("apose", "l1pose"), required=True)
    argv = __import__("sys").argv
    return parser.parse_args(argv[argv.index("--") + 1 :] if "--" in argv else [])


def load_render():
    path = Path(__file__).resolve().with_name("render_infantry_v004_p035r3_review.py")
    spec = importlib.util.spec_from_file_location("p035r3_render_export", path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def bake_l1_review_rest_pose(meshes, armature, sword_root):
    render = load_render()
    r2 = render.load_r2_build()
    for obj in meshes:
        if obj.name not in render.SWORD_PARTS:
            obj.matrix_world = r2.object_pose_matrix(obj, armature, "final") @ obj.matrix_world
    right_pose = r2.arm_pose(armature, "R", "after")
    left_pose = r2.arm_pose(armature, "L", "after")
    armature_inverse = armature.matrix_world.inverted()
    socket = armature.data.bones["Socket_R_Hand"]
    socket_head_world = armature.matrix_world @ socket.head_local
    socket_tail_world = armature.matrix_world @ socket.tail_local
    desired_sword_world = right_pose["lower_matrix"] @ sword_root.matrix_world
    bpy.context.view_layer.objects.active = armature
    armature.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    edit = armature.data.edit_bones
    for side, pose in (("Left", left_pose), ("Right", right_pose)):
        edit[side + "UpperArm"].head = armature_inverse @ pose["shoulder"]
        edit[side + "UpperArm"].tail = armature_inverse @ pose["elbow"]
        edit[side + "LowerArm"].head = armature_inverse @ pose["elbow"]
        edit[side + "LowerArm"].tail = armature_inverse @ pose["wrist"]
        edit[side + "Hand"].head = armature_inverse @ pose["wrist"]
        edit[side + "Hand"].tail = armature_inverse @ pose["hand_end"]
    edit["Socket_R_Hand"].head = armature_inverse @ (right_pose["lower_matrix"] @ socket_head_world)
    edit["Socket_R_Hand"].tail = armature_inverse @ (right_pose["lower_matrix"] @ socket_tail_world)
    bpy.ops.object.mode_set(mode="OBJECT")
    sword_root.matrix_world = desired_sword_world
    bpy.context.view_layer.update()


def main():
    args = arguments()
    scene = bpy.context.scene
    if scene.get("SourceVersion") != "CHR_Infantry_A_v004_P035R3":
        raise RuntimeError("Expected CHR_Infantry_A_v004_P035R3")
    meshes = [obj for obj in scene.objects if obj.type == "MESH" and not obj.name.startswith("REVIEW_")]
    armature = next(obj for obj in scene.objects if obj.type == "ARMATURE")
    sword_root = scene.objects["WPN_SwordRoot_R"]
    # Unity Humanoid requires mapped transform names to be unique. These are
    # visual mesh node aliases only; source object names and geometry stay intact.
    scene.objects["Head"].name = "GEO_Infantry_Head_Visual"
    scene.objects["Neck"].name = "GEO_Infantry_Neck_Visual"
    if args.pose == "l1pose":
        bake_l1_review_rest_pose(meshes, armature, sword_root)
    bpy.ops.object.select_all(action="DESELECT")
    export_objects = meshes + [armature, sword_root]
    for obj in export_objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = armature
    output = Path(args.output).resolve()
    output.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.export_scene.fbx(
        filepath=str(output), use_selection=True, object_types={"ARMATURE", "MESH", "EMPTY"},
        apply_scale_options="FBX_SCALE_ALL", use_space_transform=True, add_leaf_bones=False,
        bake_anim=False, path_mode="AUTO",
    )
    print(f"AEGIS_P035R3_FBX_COMPLETE pose={args.pose} path={output} objects={len(export_objects)}")


if __name__ == "__main__":
    main()
