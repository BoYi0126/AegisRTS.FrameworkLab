"""Create CHR_Infantry_A_v003_P02R1 from the immutable v003 initial candidate.

This Blender 5.2 script performs only the reviewer-requested Primary Forms fixes.
It refuses an unexpected input hash and saves only the P02R1 filename.
"""

import argparse
import hashlib
import importlib.util
import json
import math
from pathlib import Path

import bmesh
import bpy
from mathutils import Vector


INITIAL_VERSION = "CHR_Infantry_A_v003"
REVISION_VERSION = "CHR_Infantry_A_v003_P02R1"
REVIEW_STATUS = "READY FOR PHASE02 REVISION REVIEW"


def arguments():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output-root", required=True)
    parser.add_argument("--expected-v003-sha256", required=True)
    argv = []
    if "--" in __import__("sys").argv:
        argv = __import__("sys").argv[__import__("sys").argv.index("--") + 1 :]
    return parser.parse_args(argv)


def sha256(path):
    digest = hashlib.sha256()
    with open(path, "rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def load_helpers():
    helper_path = Path(__file__).resolve().parent / "build_infantry_v003_primary_forms.py"
    spec = importlib.util.spec_from_file_location("aegis_v003_build_helpers", helper_path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def delete_objects(names):
    for name in names:
        obj = bpy.data.objects.get(name)
        if obj is not None:
            mesh = obj.data if obj.type == "MESH" else None
            bpy.data.objects.remove(obj, do_unlink=True)
            if mesh is not None and mesh.users == 0:
                bpy.data.meshes.remove(mesh)


def triangle_count(obj):
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)


def world_bounds(objects):
    points = [obj.matrix_world @ Vector(corner) for obj in objects for corner in obj.bound_box]
    low = Vector((min(p.x for p in points), min(p.y for p in points), min(p.z for p in points)))
    high = Vector((max(p.x for p in points), max(p.y for p in points), max(p.z for p in points)))
    return low, high


def recalculate_normals(obj):
    mesh = obj.data
    bm = bmesh.new()
    bm.from_mesh(mesh)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.normal_update()
    bm.to_mesh(mesh)
    bm.free()
    mesh.update()


def parent_for_review(obj, armature, attachment=None):
    world = obj.matrix_world.copy()
    obj.parent = armature
    obj.parent_type = "OBJECT"
    obj.matrix_parent_inverse = armature.matrix_world.inverted()
    obj.matrix_world = world
    if attachment:
        obj["AttachmentBone"] = attachment
        obj["BindingStatus"] = "PHASE02_RIGID_ATTACHMENT_PLANNED"
    else:
        obj["BindingStatus"] = "PHASE02_STATIC_APOSE_REVIEW"
    obj["Revision"] = "P02R1"


def sculpt_continuous_head(head):
    # The original UV-sphere base is reshaped into one continuous stylized head.
    # Face planes and the nose live in this same mesh; all pasted face pieces are removed.
    for vertex in head.data.vertices:
        co = vertex.co
        co.x *= 0.92
        if co.z < -0.035:
            jaw_t = min(1.0, (-co.z - 0.035) / 0.115)
            co.x *= 1.0 - 0.18 * jaw_t
            co.y *= 0.96
        if co.y < 0.0:
            # Broad front plane.
            co.y = max(co.y, -0.154)
            # Integrated brow and nose bridge without separate primitive objects.
            nose_x = max(0.0, 1.0 - abs(co.x) / 0.038)
            nose_z = max(0.0, 1.0 - abs(co.z - 0.010) / 0.090)
            co.y -= 0.028 * nose_x * nose_z
            brow_z = max(0.0, 1.0 - abs(co.z - 0.065) / 0.035)
            brow_x = max(0.0, 1.0 - abs(abs(co.x) - 0.050) / 0.040)
            co.y -= 0.009 * brow_z * brow_x
            cheek_z = max(0.0, 1.0 - abs(co.z + 0.025) / 0.055)
            cheek_x = max(0.0, 1.0 - abs(abs(co.x) - 0.060) / 0.045)
            co.y -= 0.007 * cheek_z * cheek_x
    for polygon in head.data.polygons:
        polygon.use_smooth = True
    head["PrimaryFormsChange"] = "Continuous stylized head and integrated face planes"


def revise_helmet(build, armor_collection, armor_material, steel_material, cloth_material, armature):
    helmet = bpy.data.objects["Helmet"]
    for vertex in helmet.data.vertices:
        co = vertex.co
        co.x *= 0.90
        co.y = -0.004 + (co.y + 0.004) * 0.97
        if co.z > 1.635:
            top_t = min(1.0, (co.z - 1.635) / 0.115)
            co.x *= 1.0 - 0.12 * top_t
        co.z += 0.025
    helmet["PrimaryFormsChange"] = "Narrower crown with wider lower dome"

    rim = bpy.data.objects["Helmet_Rim"]
    for vertex in rim.data.vertices:
        vertex.co.x *= 0.91
        vertex.co.y = -0.003 + (vertex.co.y + 0.003) * 0.88
        vertex.co.z += 0.025
    rim["PrimaryFormsChange"] = "Clear hard lower rim"

    mount = bpy.data.objects["Helmet_TopMount"]
    for vertex in mount.data.vertices:
        vertex.co.x *= 0.72
        vertex.co.y *= 0.78
        vertex.co.z *= 0.82
    mount["PrimaryFormsChange"] = "Reduced plume mount"

    delete_objects(["Helmet_Plume"])
    # Short backward-curved feather primary silhouette: +Y is character rear.
    side = [
        (-0.004, 1.765), (0.028, 1.800), (0.086, 1.825), (0.145, 1.815),
        (0.202, 1.780), (0.174, 1.758), (0.110, 1.768), (0.048, 1.782),
    ]
    half_width = 0.018
    def plume_curve_x(y):
        return max(0.0, y) * 0.28
    vertices = [(plume_curve_x(y) - half_width, y, z) for y, z in side]
    vertices += [(plume_curve_x(y) + half_width, y, z) for y, z in side]
    count = len(side)
    faces = [tuple(range(count)), tuple(reversed(range(count, count * 2)))]
    for index in range(count):
        following = (index + 1) % count
        faces.append((index, following, count + following, count + index))
    plume = build.new_mesh("Helmet_Plume_P02R1", vertices, faces, armor_collection, cloth_material, False)
    plume = build.bevel(plume, 0.006, 2)
    parent_for_review(plume, armature)
    plume["PrimaryFormsChange"] = "Rebuilt short backward-curved L1 plume"
    return plume


def shrink_upper_arm(obj, shoulder, elbow, factor=0.87):
    start = Vector(shoulder)
    end = Vector(elbow)
    axis = end - start
    length_sq = axis.length_squared
    for vertex in obj.data.vertices:
        point = Vector(vertex.co)
        t = max(0.0, min(1.0, (point - start).dot(axis) / length_sq))
        center = start + axis * t
        vertex.co = center + (point - center) * factor
    obj["PrimaryFormsChange"] = "Upper-arm radial volume reduced 13 percent"


def harden_shoulders():
    for side, suffix in ((-1, "L"), (1, "R")):
        for layer in range(3):
            obj = bpy.data.objects[f"ShoulderArmor_{suffix}_{layer + 1}"]
            inner = 0.205 + layer * 0.010
            outer = [0.345, 0.360, 0.350][layer]
            for vertex in obj.data.vertices:
                co = vertex.co
                u = max(0.0, min(1.0, (abs(co.x) - inner) / max(0.001, outer - inner)))
                co.z -= (0.022 + layer * 0.004) * (u ** 1.25)
                co.y = -0.015 + (co.y + 0.015) * (0.91 - 0.03 * u)
            for polygon in obj.data.polygons:
                polygon.use_smooth = False
            obj["PrimaryFormsChange"] = "Preserved three layers; harder planes and stronger outer drop"


def overlap_chest_rows():
    rows = [bpy.data.objects[f"ChestArmor_Lamellar_{index}"] for index in range(1, 5)]
    for index, obj in enumerate(rows):
        center_z = sum(vertex.co.z for vertex in obj.data.vertices) / len(obj.data.vertices)
        width = max(abs(vertex.co.x) for vertex in obj.data.vertices)
        for vertex in obj.data.vertices:
            original_delta_z = vertex.co.z - center_z
            vertex.co.z = center_z + original_delta_z * 1.62
            if vertex.co.y < 0:
                vertex.co.y -= index * 0.004
                segment_wave = math.cos((vertex.co.x / max(0.001, width)) * math.pi * 7.0)
                vertex.co.y -= 0.0035 * segment_wave
                if original_delta_z < 0:
                    vertex.co.z -= 0.005 * (0.5 + 0.5 * segment_wave)
        for polygon in obj.data.polygons:
            polygon.use_smooth = False
        obj["PrimaryFormsChange"] = "Overlapping nested lamellar row; reduced black gap"
    center = bpy.data.objects["ChestArmor_Center"]
    center_x = sum(vertex.co.x for vertex in center.data.vertices) / len(center.data.vertices)
    center_z = sum(vertex.co.z for vertex in center.data.vertices) / len(center.data.vertices)
    for vertex in center.data.vertices:
        vertex.co.x = center_x + (vertex.co.x - center_x) * 0.62
        vertex.co.z = center_z + (vertex.co.z - center_z) * 0.86
        vertex.co.y += 0.008
    center["PrimaryFormsChange"] = "Reduced center strap mass"


def lower_scarf_for_face_clearance():
    scarf = bpy.data.objects["Scarf"]
    for vertex in scarf.data.vertices:
        vertex.co.z = 1.455 + (vertex.co.z - 1.455) * 0.36
        vertex.co.y = -0.010 + (vertex.co.y + 0.010) * 0.94
    scarf["PrimaryFormsChange"] = "Lowered neck wrap to expose continuous face plane"


def spiral_wrap(build, name, center, radii, height, tilt, phase, collection, material):
    segments = 18
    rx, ry = radii
    vertices = []
    for shell_scale, z_offset in ((1.0, -height / 2), (1.0, height / 2),
                                  (0.91, -height / 2), (0.91, height / 2)):
        for index in range(segments):
            angle = math.tau * index / segments
            z = center[2] + z_offset + tilt * math.sin(angle + phase)
            vertices.append((center[0] + rx * shell_scale * math.cos(angle),
                             center[1] + ry * shell_scale * math.sin(angle), z))
    faces = []
    for index in range(segments):
        following = (index + 1) % segments
        faces.append((index, following, segments + following, segments + index))
        faces.append((segments * 2 + index, segments * 3 + index,
                      segments * 3 + following, segments * 2 + following))
        faces.append((segments + index, segments + following,
                      segments * 3 + following, segments * 3 + index))
        faces.append((index, segments * 2 + index, segments * 2 + following, following))
    obj = build.new_mesh(name, vertices, faces, collection, material, True)
    obj["PrimaryFormsChange"] = "Two broad overlapping wraps with a subtle spiral"
    return obj


def rebuild_waist_and_wraps(build, armor_collection, armor_material, cloth_material, armature):
    delete_objects(["WaistCloth", "WaistCloth_Rear", "WaistArmor_L", "WaistArmor_R"] +
                   [f"LegWrap_{side}_{index}" for side in ("L", "R") for index in range(1, 5)])
    created = []
    front_outline = [(-0.105, 0.000), (0.105, 0.000), (0.092, 0.315),
                     (0.060, 0.375), (-0.060, 0.375), (-0.092, 0.315)]
    rear_outline = [(-0.095, 0.000), (0.095, 0.000), (0.082, 0.270),
                    (0.050, 0.320), (-0.050, 0.320), (-0.082, 0.270)]
    front = build.shield_prism("WaistCloth_P02R1", front_outline, (0, -0.145, 0.650), 0.035,
                               armor_collection, cloth_material, 0.010)
    rear = build.shield_prism("WaistCloth_Rear_P02R1", rear_outline, (0, 0.128, 0.705), 0.035,
                              armor_collection, cloth_material, 0.010)
    front["PrimaryFormsChange"] = "Narrow waist origin with shaped flared hem"
    rear["PrimaryFormsChange"] = "Tapered rear cloth matching front armor language"
    created += [front, rear]

    for side, suffix in ((-1, "L"), (1, "R")):
        main = build.tapered_box(f"WaistArmor_{suffix}_Main_P02R1", (side * 0.155, 0.000),
                                 0.130, 0.155, 0.080, 0.080, 0.775, 1.015,
                                 armor_collection, armor_material, 0.012)
        lower = build.tapered_box(f"WaistArmor_{suffix}_Overlap_P02R1", (side * 0.185, 0.004),
                                  0.105, 0.128, 0.072, 0.072, 0.705, 0.895,
                                  armor_collection, armor_material, 0.011)
        main["PrimaryFormsChange"] = "Main hanging thigh plate"
        lower["PrimaryFormsChange"] = "Secondary overlapping plate"
        created += [main, lower]

        for band_index, (z, radius_scale) in enumerate(((0.445, 1.00), (0.350, 0.91)), 1):
            wrap = spiral_wrap(build, f"LegWrap_{suffix}_{band_index}_P02R1",
                               (side * 0.155, -0.007, z), (0.112 * radius_scale, 0.098 * radius_scale),
                               0.090, 0.022, side * 0.55 + band_index * 0.55,
                               armor_collection, cloth_material)
            created.append(wrap)
    for obj in created:
        parent_for_review(obj, armature)
    return created


def plane_legs_and_revise_boots():
    for side, suffix in ((-1, "L"), (1, "R")):
        for name in (f"Thigh_{suffix}", f"Calf_{suffix}"):
            obj = bpy.data.objects[name]
            for vertex in obj.data.vertices:
                co = vertex.co
                dx = co.x - side * 0.15
                angle = math.atan2(co.y, dx)
                plane_factor = 1.0 + 0.055 * math.cos(4.0 * angle)
                co.x = side * 0.15 + dx * plane_factor
                co.y *= plane_factor
                co.y += 0.006 * math.sin((co.z - 0.2) * 8.0)
            obj["PrimaryFormsChange"] = "Added front/back and side plane rhythm"

        for name in (f"Boot_{suffix}", f"BootSole_{suffix}"):
            obj = bpy.data.objects[name]
            for vertex in obj.data.vertices:
                co = vertex.co
                dx = co.x - side * 0.155
                outer = side * dx > 0.0
                width_factor = 0.84 if outer else 0.92
                if co.z > 0.145:
                    width_factor *= 0.88
                co.x = side * 0.155 + dx * width_factor
                if co.z < 0.115 and co.y < -0.065:
                    co.y -= 0.012
            obj["PrimaryFormsChange"] = "Narrower asymmetric toe and ankle transition"


def revise_shield_and_sword():
    shield_center = Vector((-0.590, -0.165, 0.830))
    for name in ("Shield", "Shield_Rim"):
        obj = bpy.data.objects[name]
        for vertex in obj.data.vertices:
            co = vertex.co
            dx = (co.x - shield_center.x) / 0.31
            dz = (co.z - shield_center.z) / 0.45
            bow = max(0.0, 1.0 - dx * dx - dz * dz) * 0.014
            co.y -= bow
        obj["PrimaryFormsChange"] = "Subtle convex bow"
    boss = bpy.data.objects["Shield_Boss"]
    for vertex in boss.data.vertices:
        vertex.co.x *= 0.875
        vertex.co.z *= 0.875
        vertex.co.y *= 0.92
    boss["PrimaryFormsChange"] = "Boss diameter reduced 12.5 percent"

    pommel = bpy.data.objects["Sword_Pommel"]
    center = sum((Vector(vertex.co) for vertex in pommel.data.vertices), Vector()) / len(pommel.data.vertices)
    for vertex in pommel.data.vertices:
        offset = Vector(vertex.co) - center
        vertex.co = center + Vector((offset.x * 0.82, offset.y * 0.82, offset.z * 0.88))
    pommel["PrimaryFormsChange"] = "Less spherical pommel"


def topology_stats(objects):
    result = {"non_manifold_edges": 0, "boundary_edges": 0, "loose_edges": 0, "zero_area_faces": 0}
    for obj in objects:
        bm = bmesh.new()
        bm.from_mesh(obj.data)
        result["non_manifold_edges"] += sum(1 for edge in bm.edges if not edge.is_manifold)
        result["boundary_edges"] += sum(1 for edge in bm.edges if edge.is_boundary)
        result["loose_edges"] += sum(1 for edge in bm.edges if not edge.link_faces)
        result["zero_area_faces"] += sum(1 for face in bm.faces if face.calc_area() <= 1e-10)
        bm.free()
    return result


def main():
    args = arguments()
    opened = Path(bpy.data.filepath).resolve()
    actual_hash = sha256(opened)
    if bpy.context.scene.get("SourceVersion") != INITIAL_VERSION:
        raise RuntimeError("Revision script requires the immutable CHR_Infantry_A_v003 initial candidate")
    if actual_hash != args.expected_v003_sha256.upper():
        raise RuntimeError(f"v003 initial checksum mismatch: {actual_hash}")

    build = load_helpers()
    scene = bpy.context.scene
    armature = bpy.data.objects.get("Armature")
    if armature is None or armature.type != "ARMATURE" or len(armature.data.bones) != 23:
        raise RuntimeError("Preserved 23-bone armature contract not found")
    initial_bones = sorted(bone.name for bone in armature.data.bones)
    initial_empties = sorted(obj.name for obj in scene.objects if obj.type == "EMPTY")

    body_collection = bpy.data.collections["GEO_BODY"]
    armor_collection = bpy.data.collections["GEO_ARMOR"]
    armor_material = bpy.data.materials["MAT_v003_Review_Armor"]
    steel_material = bpy.data.materials["MAT_v003_Review_Steel"]
    cloth_material = bpy.data.materials["MAT_v003_Review_Cloth"]

    delete_objects(["Face_Nose", "Face_Brow_L", "Face_Brow_R", "Face_Cheek_L", "Face_Cheek_R", "Face_Chin"])
    sculpt_continuous_head(bpy.data.objects["Head"])
    revise_helmet(build, armor_collection, armor_material, steel_material, cloth_material, armature)

    shrink_upper_arm(bpy.data.objects["UpperArm_L"], (-0.235, 0.000, 1.405), (-0.505, -0.002, 1.245))
    shrink_upper_arm(bpy.data.objects["UpperArm_R"], (0.235, 0.000, 1.405), (0.505, -0.002, 1.245))
    harden_shoulders()
    overlap_chest_rows()
    lower_scarf_for_face_clearance()
    rebuild_waist_and_wraps(build, armor_collection, armor_material, cloth_material, armature)
    plane_legs_and_revise_boots()
    revise_shield_and_sword()

    meshes = [obj for obj in scene.objects if obj.type == "MESH"]
    for obj in meshes:
        recalculate_normals(obj)
    bpy.context.view_layer.update()
    low, high = world_bounds(meshes)
    height = high.z - low.z
    triangles = sum(triangle_count(obj) for obj in meshes)
    vertices = sum(len(obj.data.vertices) for obj in meshes)
    if not 1.80 <= height <= 1.85:
        raise RuntimeError(f"Revision height outside gate: {height}")
    if not 24000 <= triangles <= 30000:
        raise RuntimeError(f"Revision triangles outside 24K-30K gate: {triangles}")
    if sorted(bone.name for bone in armature.data.bones) != initial_bones:
        raise RuntimeError("Bone contract changed during revision")
    if sorted(obj.name for obj in scene.objects if obj.type == "EMPTY") != initial_empties:
        raise RuntimeError("Socket/anchor contract changed during revision")

    scene["SourceVersion"] = REVISION_VERSION
    scene["ProductionStatus"] = "WIP_MODEL"
    scene["ReviewStatus"] = REVIEW_STATUS
    scene["Phase"] = "02_PRIMARY_FORMS_REVISION_01"
    scene["InitialCandidateSHA256"] = actual_hash
    scene["RevisionDecision"] = "CHANGE_REQUESTED_FIXED_FOR_REVIEW"
    scene["RuntimePrefabContract"] = "PF_Unit_Infantry (not modified by this revision)"

    output_root = Path(args.output_root).resolve()
    source_dir = output_root / "Source"
    documentation_dir = output_root / "Documentation"
    source_dir.mkdir(parents=True, exist_ok=True)
    documentation_dir.mkdir(parents=True, exist_ok=True)
    output_blend = source_dir / "CHR_Infantry_A_v003_P02R1.blend"
    if output_blend.resolve() in {opened, Path("ArtSource/Units/Infantry/CHR_Infantry_A/v002/Source/CHR_Infantry_A_v002.blend").resolve()}:
        raise RuntimeError("Refusing to overwrite protected input")
    bpy.context.preferences.filepaths.save_version = 0
    bpy.ops.wm.save_as_mainfile(filepath=str(output_blend))

    revised_meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    result = {
        "status": "WIP_MODEL",
        "review_status": REVIEW_STATUS,
        "asset_id": scene.get("AssetId"),
        "source_version": REVISION_VERSION,
        "blender_version": bpy.app.version_string,
        "input_v003_initial": str(opened),
        "input_v003_initial_sha256": actual_hash,
        "output_blend": str(output_blend),
        "output_blend_sha256": sha256(output_blend),
        "bounds_m": {"min": list(low), "max": list(high), "height_z": height},
        "mesh_count": len(revised_meshes),
        "vertices": vertices,
        "triangles": triangles,
        "material_count": len({slot.material.name for obj in revised_meshes for slot in obj.material_slots if slot.material}),
        "armature": armature.name,
        "bone_count": len(armature.data.bones),
        "empty_socket_anchor_count": len(initial_empties),
        "collections": sorted(collection.name for collection in bpy.data.collections),
        "topology": topology_stats(revised_meshes),
        "primary_forms_changes": [
            "Continuous stylized head with integrated face planes",
            "Narrower lower-dome helmet and backward-curved plume",
            "Upper-arm radial volume reduced 13 percent",
            "Harder three-layer shoulder plates with stronger outer drop",
            "Overlapping nested lamellar chest rows and reduced center strap",
            "Tapered front/rear cloth and overlapping side waist plates",
            "Planed leg rhythm, two broad spiral wraps, narrower asymmetric boots",
            "Smaller shield boss, subtle shield bow, less spherical sword pommel",
        ],
        "deferred": ["Secondary Forms", "Final UV", "Final Texture", "Final Team Color Mask",
                     "Final Skinning", "Animation Polish", "Final LOD chain", "Unity runtime integration"],
    }
    result_path = documentation_dir / "P02R1_BUILD_RESULT.json"
    result_path.write_text(json.dumps(result, indent=2, ensure_ascii=False), encoding="utf-8")
    print("AEGIS_P02R1_COMPLETE")
    print(json.dumps(result, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
