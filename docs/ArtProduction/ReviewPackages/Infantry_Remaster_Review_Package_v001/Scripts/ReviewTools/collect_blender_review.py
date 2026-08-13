"""Read-only Infantry review collector.

Run Blender with the copied review-package .blend as the input. This script mutates
only Blender's in-memory scene to collect metrics and renders; it never saves a .blend.
"""

import argparse
import csv
import json
import math
import os
from pathlib import Path

import bpy
from mathutils import Vector


def arguments():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output-root", required=True)
    argv = []
    if "--" in __import__("sys").argv:
        argv = __import__("sys").argv[__import__("sys").argv.index("--") + 1 :]
    return parser.parse_args(argv)


def triangles(mesh):
    mesh.calc_loop_triangles()
    return len(mesh.loop_triangles)


def mesh_metrics(obj):
    mesh = obj.data
    group_counts = [0] * len(mesh.vertices)
    for vertex in mesh.vertices:
        group_counts[vertex.index] = sum(1 for group in vertex.groups if group.weight > 0.000001)
    weighted = sum(1 for count in group_counts if count > 0)
    return {
        "vertices": len(mesh.vertices),
        "edges": len(mesh.edges),
        "polygons": len(mesh.polygons),
        "triangles": triangles(mesh),
        "material_slots": len(obj.material_slots),
        "uv_sets": len(mesh.uv_layers),
        "shape_keys": 0 if mesh.shape_keys is None else len(mesh.shape_keys.key_blocks),
        "weighted_vertices": weighted,
        "unweighted_vertices": len(mesh.vertices) - weighted,
        "max_influences": max(group_counts, default=0),
    }


def world_bounds(objects):
    points = []
    for obj in objects:
        if obj.type != "MESH":
            continue
        points.extend(obj.matrix_world @ Vector(corner) for corner in obj.bound_box)
    if not points:
        return Vector((0, 0, 0)), Vector((1, 1, 1))
    low = Vector((min(point.x for point in points), min(point.y for point in points), min(point.z for point in points)))
    high = Vector((max(point.x for point in points), max(point.y for point in points), max(point.z for point in points)))
    return low, high


def is_lod0_review_object(obj):
    if obj.type != "MESH":
        return False
    upper = obj.name.upper()
    if "LOD1" in upper or "LOD2" in upper or "LOD3" in upper:
        return False
    return "LOD0" in upper


def look_at(obj, target):
    obj.rotation_euler = (Vector(target) - obj.location).to_track_quat("-Z", "Y").to_euler()


def make_camera(scene, center, dimensions):
    data = bpy.data.cameras.new("Review_Camera_Data")
    camera = bpy.data.objects.new("Review_Camera", data)
    bpy.context.collection.objects.link(camera)
    scene.camera = camera
    data.type = "ORTHO"
    data.ortho_scale = max(dimensions.x, dimensions.z) * 1.30
    data.lens = 55
    return camera


def add_area(name, location, energy, size, color, target):
    data = bpy.data.lights.new(name + "_Data", "AREA")
    data.energy = energy
    data.shape = "DISK"
    data.size = size
    data.color = color
    light = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(light)
    light.location = location
    look_at(light, target)
    return light


def add_ground(low, high):
    center = (low + high) * 0.5
    bpy.ops.mesh.primitive_plane_add(size=max((high - low).x, (high - low).y, 2.5) * 3.0,
                                     location=(center.x, center.y, low.z - 0.003))
    plane = bpy.context.object
    plane.name = "Review_Ground"
    material = bpy.data.materials.new("Review_Ground_Material")
    material.diffuse_color = (0.12, 0.13, 0.15, 1.0)
    material.use_nodes = True
    node = material.node_tree.nodes.get("Principled BSDF")
    if node:
        node.inputs["Base Color"].default_value = (0.12, 0.13, 0.15, 1.0)
        node.inputs["Roughness"].default_value = 0.78
    plane.data.materials.append(material)
    return plane


def material(name, color, roughness=0.7):
    result = bpy.data.materials.new(name)
    result.diffuse_color = (*color, 1.0)
    result.use_nodes = True
    node = result.node_tree.nodes.get("Principled BSDF")
    if node:
        node.inputs["Base Color"].default_value = (*color, 1.0)
        node.inputs["Roughness"].default_value = roughness
    return result


def set_material_override(objects, override):
    original = {}
    for obj in objects:
        original[obj.name] = [slot.material for slot in obj.material_slots]
        if not obj.material_slots:
            obj.data.materials.append(override)
        else:
            for slot in obj.material_slots:
                slot.material = override
    return original


def restore_materials(objects, original):
    for obj in objects:
        prior = original.get(obj.name, [])
        if not prior:
            obj.data.materials.clear()
            continue
        for index, value in enumerate(prior):
            if index < len(obj.material_slots):
                obj.material_slots[index].material = value


def render_views(scene, camera, center, dimensions, output_dir, names_to_vectors):
    output_dir.mkdir(parents=True, exist_ok=True)
    distance = max(dimensions.x, dimensions.y, dimensions.z) * 4.0
    for name, vector in names_to_vectors:
        direction = Vector(vector).normalized()
        camera.location = center + direction * distance
        look_at(camera, center)
        scene.render.filepath = str(output_dir / (name + ".png"))
        bpy.ops.render.render(write_still=True)


def create_wire_duplicates(objects, wire_material):
    duplicates = []
    for source in objects:
        duplicate = source.copy()
        duplicate.data = source.data.copy()
        bpy.context.collection.objects.link(duplicate)
        duplicate.name = "ReviewWire_" + source.name
        duplicate.data.materials.clear()
        duplicate.data.materials.append(wire_material)
        modifier = duplicate.modifiers.new("Review_Wireframe", "WIREFRAME")
        modifier.thickness = 0.0022
        modifier.use_replace = True
        modifier.material_offset = 0
        duplicates.append(duplicate)
    return duplicates


def main():
    args = arguments()
    root = Path(args.output_root).resolve()
    manifests = root / "Manifests"
    blender_dir = root / "Screenshots" / "Blender"
    wireframe_dir = root / "Screenshots" / "Wireframe"
    manifests.mkdir(parents=True, exist_ok=True)
    blender_dir.mkdir(parents=True, exist_ok=True)
    wireframe_dir.mkdir(parents=True, exist_ok=True)

    scene = bpy.context.scene
    source_objects = list(scene.objects)
    mesh_objects = [obj for obj in source_objects if obj.type == "MESH"]
    armatures = [obj for obj in source_objects if obj.type == "ARMATURE"]
    current_lod0 = [obj for obj in mesh_objects if is_lod0_review_object(obj)]
    if not current_lod0:
        current_lod0 = mesh_objects

    rows = []
    mesh_details = {}
    for obj in source_objects:
        metrics = mesh_metrics(obj) if obj.type == "MESH" else {}
        if metrics:
            mesh_details[obj.name] = metrics
        rows.append({
            "ObjectName": obj.name,
            "Type": obj.type,
            "Parent": "" if obj.parent is None else obj.parent.name,
            "ParentType": obj.parent_type,
            "Bone": obj.parent_bone,
            "Materials": ";".join(slot.material.name for slot in obj.material_slots if slot.material),
            "Vertices": metrics.get("vertices", ""),
            "Triangles": metrics.get("triangles", ""),
            "UVSets": metrics.get("uv_sets", ""),
            "ShapeKeys": metrics.get("shape_keys", ""),
            "Modifiers": ";".join(modifier.type for modifier in obj.modifiers),
            "VisibleRender": not obj.hide_render,
        })
    with (manifests / "Blender_Object_Manifest.csv").open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=list(rows[0].keys()))
        writer.writeheader()
        writer.writerows(rows)

    low, high = world_bounds(current_lod0)
    dimensions = high - low
    center = (low + high) * 0.5
    bones = []
    for armature in armatures:
        for bone in armature.data.bones:
            bones.append({
                "armature": armature.name,
                "bone": bone.name,
                "parent": "" if bone.parent is None else bone.parent.name,
                "deform": bone.use_deform,
            })
    with (manifests / "Blender_Bone_Manifest.csv").open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=["armature", "bone", "parent", "deform"])
        writer.writeheader()
        writer.writerows(bones)

    images = []
    for image in bpy.data.images:
        if image.source == "FILE":
            images.append({"name": image.name, "filepath": bpy.path.abspath(image.filepath), "packed": image.packed_file is not None})

    all_mesh_totals = {
        "vertices": sum(value["vertices"] for value in mesh_details.values()),
        "triangles": sum(value["triangles"] for value in mesh_details.values()),
        "unweighted_vertices": sum(value["unweighted_vertices"] for value in mesh_details.values()),
        "max_influences": max((value["max_influences"] for value in mesh_details.values()), default=0),
    }
    lod0_totals = {
        "vertices": sum(mesh_details[obj.name]["vertices"] for obj in current_lod0),
        "triangles": sum(mesh_details[obj.name]["triangles"] for obj in current_lod0),
        "mesh_count": len(current_lod0),
    }
    summary = {
        "blender_version": bpy.app.version_string,
        "opened_file": bpy.data.filepath,
        "saved_by_script": False,
        "objects": len(source_objects),
        "meshes": len(mesh_objects),
        "armatures": len(armatures),
        "materials": len(bpy.data.materials),
        "actions": len(bpy.data.actions),
        "bones": len(bones),
        "all_mesh_totals": all_mesh_totals,
        "lod0_review_totals": lod0_totals,
        "lod0_review_objects": [obj.name for obj in current_lod0],
        "character_bounds_m": {
            "min": list(low),
            "max": list(high),
            "dimensions": list(dimensions),
            "height_z": dimensions.z,
        },
        "armature_names": [obj.name for obj in armatures],
        "material_names": [value.name for value in bpy.data.materials],
        "action_names": [value.name for value in bpy.data.actions],
        "external_images": images,
        "mesh_details": mesh_details,
    }
    (manifests / "Blender_Technical_Summary.json").write_text(json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8")

    # Review-only render setup. Do not call bpy.ops.wm.save_*.
    for obj in mesh_objects:
        obj.hide_render = obj not in current_lod0
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 768
    scene.render.resolution_y = 768
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.world.color = (0.035, 0.04, 0.055)
    if scene.world.use_nodes:
        background = scene.world.node_tree.nodes.get("Background")
        if background:
            background.inputs["Color"].default_value = (0.035, 0.04, 0.055, 1.0)
            background.inputs["Strength"].default_value = 0.55

    camera = make_camera(scene, center, dimensions)
    add_ground(low, high)
    span = max(dimensions.x, dimensions.y, dimensions.z)
    add_area("Review_Key", center + Vector((-2.5, -3.0, 3.2)) * span, 750, span * 2.0, (1.0, 0.90, 0.78), center)
    add_area("Review_Fill", center + Vector((2.5, -1.0, 1.8)) * span, 420, span * 1.6, (0.72, 0.84, 1.0), center)
    add_area("Review_Rim", center + Vector((0.5, 3.0, 2.6)) * span, 550, span * 1.5, (0.78, 0.88, 1.0), center)

    six_views = [
        ("01_Front", (0, -1, 0.08)),
        ("02_Left", (-1, 0, 0.08)),
        ("03_Right", (1, 0, 0.08)),
        ("04_Back", (0, 1, 0.08)),
        ("05_ThreeQuarter_Front", (-1, -1, 0.18)),
        ("06_ThreeQuarter_Back", (1, 1, 0.18)),
    ]
    render_views(scene, camera, center, dimensions, blender_dir, six_views)

    clay = material("Review_Clay", (0.46, 0.50, 0.56), 0.82)
    originals = set_material_override(current_lod0, clay)
    clay_views = [
        ("Clay_Front", (0, -1, 0.08)),
        ("Clay_Side", (-1, 0, 0.08)),
        ("Clay_Back", (0, 1, 0.08)),
        ("Clay_3Q", (-1, -1, 0.18)),
    ]
    render_views(scene, camera, center, dimensions, blender_dir, clay_views)

    wire_material = material("Review_Wire", (0.015, 0.018, 0.024), 1.0)
    wire_objects = create_wire_duplicates(current_lod0, wire_material)
    render_views(scene, camera, center, dimensions, wireframe_dir,
                 [("Wireframe_Front", (0, -1, 0.08)), ("Wireframe_3Q", (-1, -1, 0.18))])

    # Restore only in memory for clean shutdown; never save the source or copied .blend.
    for obj in wire_objects:
        bpy.data.objects.remove(obj, do_unlink=True)
    restore_materials(current_lod0, originals)
    print("AEGIS_REVIEW_COLLECTION_COMPLETE", json.dumps({"summary": str(manifests / "Blender_Technical_Summary.json"), "renders": 12}))


if __name__ == "__main__":
    main()
