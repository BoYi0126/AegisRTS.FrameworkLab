"""Import one P035R3 review FBX into a clean Blender scene and validate hierarchy."""
import argparse
import hashlib
import json
from pathlib import Path

import bpy


SWORD_PARTS = (
    "GEO_Infantry_Sword_GripContact", "Sword", "Sword_Grip", "Sword_Guard",
    "Sword_Pommel", "GEO_Infantry_Sword_BladeSpine", "GEO_Infantry_Sword_GripWraps",
)


def arguments():
    parser = argparse.ArgumentParser(); parser.add_argument("--fbx", required=True); parser.add_argument("--output", required=True)
    argv = __import__("sys").argv; return parser.parse_args(argv[argv.index("--") + 1 :] if "--" in argv else [])


def sha256(path):
    digest = hashlib.sha256()
    with open(path, "rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""): digest.update(chunk)
    return digest.hexdigest().upper()


def main():
    args = arguments(); fbx = Path(args.fbx).resolve(); output = Path(args.output).resolve()
    bpy.ops.wm.read_factory_settings(use_empty=True); bpy.ops.import_scene.fbx(filepath=str(fbx))
    armatures = [o for o in bpy.context.scene.objects if o.type == "ARMATURE"]
    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    if len(armatures) != 1: raise RuntimeError("FBX armature count mismatch")
    armature = armatures[0]; root = bpy.data.objects.get("WPN_SwordRoot_R"); socket = armature.data.bones.get("Socket_R_Hand")
    hierarchy_ok = bool(socket and socket.parent and socket.parent.name == "RightHand" and root and root.parent == armature and root.parent_type == "BONE" and root.parent_bone == "Socket_R_Hand" and all(bpy.data.objects.get(name) and bpy.data.objects[name].parent == root for name in SWORD_PARTS))
    result = {
        "status": "PASS", "fbx": str(fbx), "fbx_sha256": sha256(fbx), "bytes": fbx.stat().st_size,
        "blender_reimport_version": bpy.app.version_string, "armatures": len(armatures), "meshes": len(meshes),
        "vertices": sum(len(o.data.vertices) for o in meshes), "triangles": sum(len(p.vertices)-2 for o in meshes for p in o.data.polygons),
        "bones": len(armature.data.bones), "right_hand_exists": "RightHand" in armature.data.bones,
        "socket_exists": socket is not None, "socket_parent": socket.parent.name if socket and socket.parent else "",
        "socket_deform_note": "FBX does not preserve Blender use_deform; source manifest is authoritative",
        "sword_root_parent": root.parent.name if root and root.parent else "", "sword_root_parent_type": root.parent_type if root else "", "sword_root_parent_bone": root.parent_bone if root else "",
        "sword_parts": [{"name": name, "parent": bpy.data.objects[name].parent.name if bpy.data.objects.get(name) and bpy.data.objects[name].parent else ""} for name in SWORD_PARTS],
        "attachment_survives_export_reimport": hierarchy_ok,
    }
    if not hierarchy_ok or len(meshes) != 98 or len(armature.data.bones) != 24: raise RuntimeError(f"FBX validation failed: {result}")
    output.parent.mkdir(parents=True, exist_ok=True); output.write_text(json.dumps(result, indent=2), encoding="utf-8")
    print("AEGIS_P035R3_FBX_REIMPORT_COMPLETE", json.dumps({"path": str(fbx), "hierarchy": hierarchy_ok, "meshes": len(meshes), "bones": len(armature.data.bones)}))


if __name__ == "__main__": main()
