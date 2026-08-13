"""Generate Phase 02 review evidence from CHR_Infantry_A_v003.blend.

The script mutates only Blender's in-memory review scene and never saves the source.
"""

import argparse
import csv
import hashlib
import json
import math
import os
import struct
from pathlib import Path

import bmesh
import bpy
from mathutils import Vector


def arguments():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output-root", required=True)
    parser.add_argument("--v002-blend", required=True)
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


def png_dimensions(path):
    with open(path, "rb") as handle:
        signature = handle.read(24)
    return struct.unpack(">II", signature[16:24])


def mesh_triangles(obj):
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)


def world_bounds(objects):
    points = [obj.matrix_world @ Vector(corner) for obj in objects for corner in obj.bound_box]
    low = Vector((min(point.x for point in points), min(point.y for point in points), min(point.z for point in points)))
    high = Vector((max(point.x for point in points), max(point.y for point in points), max(point.z for point in points)))
    return low, high


def look_at(obj, target):
    obj.rotation_euler = (Vector(target) - obj.location).to_track_quat("-Z", "Y").to_euler()


def material(name, color, roughness=.75, metallic=0.0):
    value = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    value.diffuse_color = (*color, 1.0)
    value.use_nodes = True
    node = value.node_tree.nodes.get("Principled BSDF")
    node.inputs["Base Color"].default_value = (*color, 1.0)
    node.inputs["Roughness"].default_value = roughness
    node.inputs["Metallic"].default_value = metallic
    return value


def set_override(objects, override):
    prior = {}
    for obj in objects:
        prior[obj.name] = [slot.material for slot in obj.material_slots]
        obj.data.materials.clear()
        obj.data.materials.append(override)
    return prior


def restore_materials(objects, prior):
    for obj in objects:
        obj.data.materials.clear()
        for item in prior.get(obj.name, []):
            if item:
                obj.data.materials.append(item)


def add_area(name, location, energy, size, color, target):
    data = bpy.data.lights.new(name + "_Data", "AREA")
    data.energy = energy
    data.shape = "DISK"
    data.size = size
    data.color = color
    obj = bpy.data.objects.new(name, data)
    bpy.context.scene.collection.objects.link(obj)
    obj.location = location
    look_at(obj, target)
    return obj


def setup_scene(low, high):
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.render.resolution_percentage = 100
    scene.render.resolution_x = 768
    scene.render.resolution_y = 768
    scene.render.image_settings.color_mode = "RGBA"
    scene.world.use_nodes = True
    background = scene.world.node_tree.nodes.get("Background")
    background.inputs["Color"].default_value = (.028, .033, .042, 1.0)
    background.inputs["Strength"].default_value = .62

    center = (low + high) * .5
    span = max((high - low).x, (high - low).z)
    data = bpy.data.cameras.new("Phase02_ReviewCamera_Data")
    camera = bpy.data.objects.new("Phase02_ReviewCamera", data)
    bpy.context.scene.collection.objects.link(camera)
    data.type = "ORTHO"
    data.ortho_scale = 2.22
    scene.camera = camera
    add_area("Phase02_Key", center + Vector((-2.8, -3.2, 3.4)) * span, 920, span * 2.2,
             (1.0, .90, .78), center)
    add_area("Phase02_Fill", center + Vector((2.3, -1.1, 2.1)) * span, 480, span * 1.8,
             (.70, .82, 1.0), center)
    add_area("Phase02_Rim", center + Vector((.6, 3.0, 2.8)) * span, 620, span * 1.6,
             (.76, .86, 1.0), center)

    bpy.ops.mesh.primitive_plane_add(size=6.0, location=(0, 0, low.z - .004))
    ground = bpy.context.object
    ground.name = "Phase02_ReviewGround"
    ground.data.materials.append(material("MAT_Phase02_Ground", (.10, .115, .14), .90))
    return scene, camera, center, ground


def render(scene, camera, center, direction, path, ortho_scale=2.22, resolution=(768, 768)):
    direction = Vector(direction).normalized()
    camera.location = center + direction * 7.0
    camera.data.ortho_scale = ortho_scale
    look_at(camera, center)
    scene.render.resolution_x, scene.render.resolution_y = resolution
    scene.render.filepath = str(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.render.render(write_still=True)


def wire_duplicates(objects, wire_material):
    values = []
    for source in objects:
        duplicate = source.copy()
        duplicate.data = source.data.copy()
        bpy.context.scene.collection.objects.link(duplicate)
        duplicate.name = "REVIEW_WIRE_" + source.name
        duplicate.data.materials.clear()
        duplicate.data.materials.append(wire_material)
        modifier = duplicate.modifiers.new("ReviewWireframe", "WIREFRAME")
        modifier.thickness = .0018
        modifier.use_replace = True
        values.append(duplicate)
    return values


def topology_stats(obj):
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    non_manifold = sum(1 for edge in bm.edges if not edge.is_manifold)
    boundary = sum(1 for edge in bm.edges if edge.is_boundary)
    loose = sum(1 for edge in bm.edges if not edge.link_faces)
    zero_area = sum(1 for face in bm.faces if face.calc_area() <= 1e-12)
    bm.free()
    return non_manifold, boundary, loose, zero_area


def comparison(scene, camera, center, v003_meshes, v003_armature, v002_path, comparison_dir, clay):
    collection = bpy.data.collections.new("REVIEW_V002_COMPARISON")
    bpy.context.scene.collection.children.link(collection)
    with bpy.data.libraries.load(str(v002_path), link=False) as (available, requested):
        requested.objects = [name for name in available.objects if name == "Armature" or "LOD0" in name]
    loaded = [obj for obj in requested.objects if obj]
    for obj in loaded:
        if not obj.users_collection:
            collection.objects.link(obj)
    baseline_meshes = [obj for obj in loaded if obj.type == "MESH"]
    baseline_armatures = [obj for obj in loaded if obj.type == "ARMATURE"]
    for obj in baseline_meshes:
        obj.data.materials.clear()
        obj.data.materials.append(clay)
    v003_prior = set_override(v003_meshes, clay)
    v003_world = {obj: obj.matrix_world.copy() for obj in v003_meshes}
    baseline_world = {obj: obj.matrix_world.copy() for obj in baseline_meshes}
    for obj in v003_meshes:
        moved = obj.matrix_world.copy()
        moved.translation.x += 1.08
        obj.matrix_world = moved
    for obj in baseline_meshes:
        moved = obj.matrix_world.copy()
        moved.translation.x -= 1.08
        obj.matrix_world = moved
    bpy.context.view_layer.update()
    compare_center = Vector((0, center.y, center.z))
    render(scene, camera, compare_center, (0, -1, .06), comparison_dir / "v002_vs_v003_Front.png",
           2.25, (1536, 768))
    render(scene, camera, compare_center, (-1, -1, .14), comparison_dir / "v002_vs_v003_3Q.png",
           2.35, (1536, 768))
    for obj, world in v003_world.items():
        obj.matrix_world = world
    for obj, world in baseline_world.items():
        obj.matrix_world = world
    restore_materials(v003_meshes, v003_prior)
    for obj in loaded:
        bpy.data.objects.remove(obj, do_unlink=True)
    bpy.data.collections.remove(collection)
    bpy.context.view_layer.update()


def main():
    args = arguments()
    root = Path(args.output_root).resolve()
    clay_dir = root / "Screenshots" / "Clay"
    silhouette_dir = root / "Screenshots" / "Silhouette"
    wireframe_dir = root / "Screenshots" / "Wireframe"
    comparison_dir = root / "Screenshots" / "Comparison"
    screen_dir = root / "Screenshots" / "ScreenSize"
    unity_dir = root / "Screenshots" / "Unity"
    manifests = root / "Manifests"
    blender_dir = root / "Blender"
    for path in (clay_dir, silhouette_dir, wireframe_dir, comparison_dir, screen_dir, unity_dir,
                 manifests, blender_dir):
        path.mkdir(parents=True, exist_ok=True)

    scene = bpy.context.scene
    if scene.get("SourceVersion") != "CHR_Infantry_A_v003":
        raise RuntimeError("Opened Blender file is not the v003 Primary Forms candidate")
    source_meshes = [obj for obj in scene.objects if obj.type == "MESH"]
    armatures = [obj for obj in scene.objects if obj.type == "ARMATURE"]
    if len(armatures) != 1:
        raise RuntimeError(f"Expected one armature, found {len(armatures)}")
    low, high = world_bounds(source_meshes)
    dimensions = high - low
    review_center = Vector((0, (low.y + high.y) * .5, (low.z + high.z) * .5))

    rows = []
    topology = {"non_manifold_edges": 0, "boundary_edges": 0, "loose_edges": 0, "zero_area_faces": 0}
    for obj in sorted(source_meshes, key=lambda value: value.name):
        non_manifold, boundary, loose, zero_area = topology_stats(obj)
        topology["non_manifold_edges"] += non_manifold
        topology["boundary_edges"] += boundary
        topology["loose_edges"] += loose
        topology["zero_area_faces"] += zero_area
        rows.append({
            "ObjectName": obj.name,
            "Collection": ";".join(collection.name for collection in obj.users_collection),
            "Vertices": len(obj.data.vertices),
            "Triangles": mesh_triangles(obj),
            "Materials": ";".join(slot.material.name for slot in obj.material_slots if slot.material),
            "Parent": "" if obj.parent is None else obj.parent.name,
            "BindingStatus": obj.get("BindingStatus", ""),
            "AttachmentBone": obj.get("AttachmentBone", ""),
            "NonManifoldEdges": non_manifold,
            "BoundaryEdges": boundary,
            "LooseEdges": loose,
            "ZeroAreaFaces": zero_area,
        })
    with (manifests / "Object_Manifest.csv").open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=list(rows[0].keys()))
        writer.writeheader()
        writer.writerows(rows)

    bones = []
    for bone in armatures[0].data.bones:
        bones.append({"Bone": bone.name, "Parent": "" if bone.parent is None else bone.parent.name,
                      "Deform": bone.use_deform})
    with (manifests / "Bone_Manifest.csv").open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=["Bone", "Parent", "Deform"])
        writer.writeheader()
        writer.writerows(bones)

    summary = {
        "status": "READY FOR REVIEW",
        "source_status": scene.get("ProductionStatus"),
        "asset_id": scene.get("AssetId"),
        "source_version": scene.get("SourceVersion"),
        "opened_file": bpy.data.filepath,
        "opened_file_sha256": sha256(bpy.data.filepath),
        "blender_version": bpy.app.version_string,
        "saved_by_review_script": False,
        "bounds_m": {"min": list(low), "max": list(high), "dimensions": list(dimensions),
                     "height_z": dimensions.z},
        "mesh_count": len(source_meshes),
        "vertices": sum(len(obj.data.vertices) for obj in source_meshes),
        "triangles": sum(mesh_triangles(obj) for obj in source_meshes),
        "material_count": len({slot.material.name for obj in source_meshes for slot in obj.material_slots if slot.material}),
        "armature_count": len(armatures),
        "bone_count": len(bones),
        "actions": len(bpy.data.actions),
        "collections": sorted(collection.name for collection in bpy.data.collections
                              if collection.name in {"CHR_Infantry_A_v003", "GEO_BODY", "GEO_ARMOR", "GEO_WEAPON", "RIG", "REVIEW"}),
        "topology": topology,
        "notes": ["Face normals were recalculated by the deterministic build script.",
                  "Binding is a static A-pose / planned rigid-attachment review setup, not final skinning.",
                  "No UV, texture, animation, LOD or Unity runtime acceptance is implied."],
    }
    (manifests / "Geometry_Summary.json").write_text(json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8")

    scene, camera, center, ground = setup_scene(low, high)
    clay = material("MAT_Phase02_Clay", (.48, .52, .58), .82)
    black = material("MAT_Phase02_Silhouette", (.001, .001, .001), 1.0)
    white = material("MAT_Phase02_WhiteGround", (.96, .96, .96), 1.0)
    wire = material("MAT_Phase02_Wire", (.006, .008, .012), 1.0)

    original = set_override(source_meshes, clay)
    clay_views = [
        ("01_Clay_Front", (0, -1, .06)), ("02_Clay_Left", (-1, 0, .06)),
        ("03_Clay_Back", (0, 1, .06)), ("04_Clay_3Q_Front", (-1, -1, .14)),
        ("05_Clay_3Q_Back", (1, 1, .14)), ("06_Clay_Right", (1, 0, .06)),
    ]
    for name, direction in clay_views:
        render(scene, camera, review_center, direction, clay_dir / f"{name}.png")

    set_override(source_meshes, black)
    world_background = scene.world.node_tree.nodes.get("Background")
    world_background.inputs["Color"].default_value = (.98, .98, .98, 1.0)
    world_background.inputs["Strength"].default_value = .8
    ground.data.materials.clear()
    ground.data.materials.append(white)
    silhouette_views = [
        ("Silhouette_Front", (0, -1, .06)), ("Silhouette_Left", (-1, 0, .06)),
        ("Silhouette_Back", (0, 1, .06)), ("Silhouette_3Q", (-1, -1, .14)),
    ]
    for name, direction in silhouette_views:
        render(scene, camera, review_center, direction, silhouette_dir / f"{name}.png")
    for target in (128, 64, 32):
        ortho = dimensions.z * 256 / target
        render(scene, camera, review_center, (0, -1, .06), screen_dir / f"Silhouette_{target}px.png",
               ortho, (256, 256))

    set_override(source_meshes, clay)
    world_background.inputs["Color"].default_value = (.88, .90, .93, 1.0)
    world_background.inputs["Strength"].default_value = .7
    duplicates = wire_duplicates(source_meshes, wire)
    for name, direction in (("Wireframe_Front", (0, -1, .06)),
                            ("Wireframe_Side", (-1, 0, .06)),
                            ("Wireframe_3Q", (-1, -1, .14))):
        render(scene, camera, review_center, direction, wireframe_dir / f"{name}.png")
    for obj in duplicates:
        bpy.data.objects.remove(obj, do_unlink=True)

    # The side-by-side sheets are composed after this Blender render pass by
    # compose_primary_forms_comparison.ps1.  That keeps the preserved v002
    # file read-only and avoids importing baseline datablocks into this scene.
    (comparison_dir / "COMPOSITION_SOURCE.txt").write_text(
        "Comparison sheets are composed from the immutable v002 review captures "
        "and the v003 captures generated by this script.\n"
        "Run compose_primary_forms_comparison.ps1 after this Blender render pass.\n",
        encoding="utf-8",
    )
    restore_materials(source_meshes, original)

    images = []
    for path in sorted((root / "Screenshots").rglob("*.png")):
        width, height = png_dimensions(path)
        images.append({"Path": path.relative_to(root).as_posix(), "Width": width, "Height": height,
                       "Bytes": path.stat().st_size, "SHA256": sha256(path)})
    with (manifests / "Screenshot_Manifest.csv").open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=["Path", "Width", "Height", "Bytes", "SHA256"])
        writer.writeheader()
        writer.writerows(images)
    (unity_dir / "UNITY_REVIEW_CAPTURE_MANUAL_REQUIRED.txt").write_text(
        "UNITY REVIEW CAPTURE: MANUAL REQUIRED\nPhase 02 Blender evidence is complete. The formal runtime Prefab was not modified.\n",
        encoding="utf-8")
    print("AEGIS_PHASE02_REVIEW_RENDER_COMPLETE", json.dumps({"images": len(images), "summary": summary}, ensure_ascii=False))


if __name__ == "__main__":
    main()
