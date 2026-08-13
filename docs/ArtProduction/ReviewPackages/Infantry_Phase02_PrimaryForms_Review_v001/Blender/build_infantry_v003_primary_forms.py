"""Build CHR_Infantry_A_v003 Phase 02 Primary Forms in Blender 5.2.

This script must be launched with CHR_Infantry_A_v002.blend open.  It preserves the
existing armature, sockets and anchors, removes only mesh objects in the new in-memory
copy, builds new versioned geometry, and saves exclusively to the requested v003 path.
It never writes to the v002 source or Unity runtime folders.
"""

import argparse
import hashlib
import json
import math
import os
from pathlib import Path

import bpy
import bmesh
from mathutils import Vector


STATUS = "WIP_MODEL"
ASSET_ID = "unit.infantry"
SOURCE_VERSION = "CHR_Infantry_A_v003"


def arguments():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output-root", required=True)
    parser.add_argument("--expected-v002-sha256", required=True)
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


def ensure_collection(name, parent=None):
    collection = bpy.data.collections.get(name)
    if collection is None:
        collection = bpy.data.collections.new(name)
    if parent is None:
        if collection.name not in bpy.context.scene.collection.children:
            bpy.context.scene.collection.children.link(collection)
    elif collection.name not in parent.children:
        parent.children.link(collection)
    return collection


def move_to_collection(obj, collection):
    for current in list(obj.users_collection):
        current.objects.unlink(obj)
    collection.objects.link(obj)


def make_material(name, color, metallic=0.0, roughness=0.72):
    material = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    material.diffuse_color = (*color, 1.0)
    material.use_nodes = True
    node = material.node_tree.nodes.get("Principled BSDF")
    node.inputs["Base Color"].default_value = (*color, 1.0)
    node.inputs["Metallic"].default_value = metallic
    node.inputs["Roughness"].default_value = roughness
    return material


def new_mesh(name, vertices, faces, collection, material, smooth=True):
    mesh = bpy.data.meshes.new(name + "_Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update(calc_edges=True)
    bm = bmesh.new()
    bm.from_mesh(mesh)
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.to_mesh(mesh)
    bm.free()
    mesh.update(calc_edges=True)
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    if material:
        mesh.materials.append(material)
    for polygon in mesh.polygons:
        polygon.use_smooth = smooth
    return obj


def apply_modifier(obj, modifier):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.select_set(False)


def bevel(obj, width, segments=2, angle=0.52):
    modifier = obj.modifiers.new("PrimaryForm_Bevel", "BEVEL")
    modifier.width = width
    modifier.segments = segments
    modifier.limit_method = "ANGLE"
    modifier.angle_limit = angle
    apply_modifier(obj, modifier)
    return obj


def solidify(obj, thickness):
    modifier = obj.modifiers.new("PrimaryForm_Thickness", "SOLIDIFY")
    modifier.thickness = thickness
    modifier.offset = 0.0
    apply_modifier(obj, modifier)
    return obj


def loft_z(name, rings, segments, collection, material, cap=True, smooth=True, phase=0.0):
    vertices = []
    for z, rx, ry, cx, cy in rings:
        for index in range(segments):
            angle = phase + math.tau * index / segments
            vertices.append((cx + rx * math.cos(angle), cy + ry * math.sin(angle), z))
    faces = []
    for ring_index in range(len(rings) - 1):
        start = ring_index * segments
        next_start = (ring_index + 1) * segments
        for index in range(segments):
            following = (index + 1) % segments
            faces.append((start + index, start + following, next_start + following, next_start + index))
    if cap:
        bottom_center = len(vertices)
        vertices.append((rings[0][3], rings[0][4], rings[0][0]))
        top_center = len(vertices)
        vertices.append((rings[-1][3], rings[-1][4], rings[-1][0]))
        for index in range(segments):
            following = (index + 1) % segments
            faces.append((bottom_center, following, index))
            top_start = (len(rings) - 1) * segments
            faces.append((top_center, top_start + index, top_start + following))
    return new_mesh(name, vertices, faces, collection, material, smooth)


def loft_path(name, points, radii, segments, collection, material, smooth=True):
    front = Vector((0.0, -1.0, 0.0))
    vertices = []
    for index, point_value in enumerate(points):
        point = Vector(point_value)
        if index == 0:
            tangent = Vector(points[1]) - point
        elif index == len(points) - 1:
            tangent = point - Vector(points[index - 1])
        else:
            tangent = Vector(points[index + 1]) - Vector(points[index - 1])
        tangent.normalize()
        radial = front.cross(tangent).normalized()
        if radial.length < 0.5:
            radial = Vector((1.0, 0.0, 0.0))
        depth_axis = tangent.cross(radial).normalized()
        radius, depth = radii[index]
        for segment in range(segments):
            angle = math.tau * segment / segments
            vertex = point + radial * (radius * math.cos(angle)) + depth_axis * (depth * math.sin(angle))
            vertices.append(tuple(vertex))
    faces = []
    for ring in range(len(points) - 1):
        for segment in range(segments):
            following = (segment + 1) % segments
            a = ring * segments + segment
            b = ring * segments + following
            c = (ring + 1) * segments + following
            d = (ring + 1) * segments + segment
            faces.append((a, b, c, d))
    vertices.extend([tuple(points[0]), tuple(points[-1])])
    bottom = len(vertices) - 2
    top = len(vertices) - 1
    for segment in range(segments):
        following = (segment + 1) % segments
        faces.append((bottom, following, segment))
        start = (len(points) - 1) * segments
        faces.append((top, start + segment, start + following))
    return new_mesh(name, vertices, faces, collection, material, smooth)


def ellipsoid(name, location, scale, collection, material, segments=32, rings=20, deform=None):
    if bpy.context.mode != "OBJECT":
        bpy.ops.object.mode_set(mode="OBJECT")
    bpy.ops.object.select_all(action="DESELECT")
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = (1.0, 1.0, 1.0)
    move_to_collection(obj, collection)
    for vertex in obj.data.vertices:
        co = vertex.co
        co.x *= scale[0]
        co.y *= scale[1]
        co.z *= scale[2]
        if deform:
            transformed = deform(co.copy())
            co.x, co.y, co.z = transformed
    if material:
        obj.data.materials.append(material)
    for polygon in obj.data.polygons:
        polygon.use_smooth = True
    return obj


def tapered_box(name, center, bottom, top, depth_front, depth_back, z_bottom, z_top, collection, material, bevel_width=0.012):
    cx, cy = center
    vertices = [
        (cx - bottom / 2, cy - depth_front, z_bottom),
        (cx + bottom / 2, cy - depth_front, z_bottom),
        (cx + bottom / 2, cy + depth_back, z_bottom),
        (cx - bottom / 2, cy + depth_back, z_bottom),
        (cx - top / 2, cy - depth_front, z_top),
        (cx + top / 2, cy - depth_front, z_top),
        (cx + top / 2, cy + depth_back, z_top),
        (cx - top / 2, cy + depth_back, z_top),
    ]
    faces = [(0, 1, 2, 3), (4, 7, 6, 5), (0, 4, 5, 1), (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0)]
    obj = new_mesh(name, vertices, faces, collection, material, False)
    return bevel(obj, bevel_width, 3)


def shield_prism(name, outline, center, thickness, collection, material, bevel_width=0.012):
    cx, cy, cz = center
    half = thickness / 2
    vertices = [(cx + x, cy - half, cz + z) for x, z in outline]
    vertices += [(cx + x, cy + half, cz + z) for x, z in outline]
    count = len(outline)
    faces = [tuple(range(count)), tuple(reversed(range(count, count * 2)))]
    for index in range(count):
        following = (index + 1) % count
        faces.append((index, following, count + following, count + index))
    obj = new_mesh(name, vertices, faces, collection, material, False)
    return bevel(obj, bevel_width, 3)


def shield_frame(name, outer, inner_scale, center, thickness, collection, material):
    cx, cy, cz = center
    half = thickness / 2
    inner = [(x * inner_scale, z * inner_scale) for x, z in outer]
    vertices = []
    for y in (cy - half, cy + half):
        vertices += [(cx + x, y, cz + z) for x, z in outer]
        vertices += [(cx + x, y, cz + z) for x, z in inner]
    n = len(outer)
    faces = []
    for side in (0, 1):
        base = side * n * 2
        for index in range(n):
            following = (index + 1) % n
            faces.append((base + index, base + following, base + n + following, base + n + index))
    for index in range(n):
        following = (index + 1) % n
        faces.append((index, n * 2 + index, n * 2 + following, following))
        faces.append((n + index, n + following, n * 3 + following, n * 3 + index))
    obj = new_mesh(name, vertices, faces, collection, material, False)
    return bevel(obj, 0.009, 3)


def blade(name, guard, tip, base_width, tip_width, thickness, collection, material):
    guard = Vector(guard)
    tip = Vector(tip)
    axis = (tip - guard).normalized()
    front = Vector((0.0, -1.0, 0.0))
    width_axis = front.cross(axis).normalized()
    depth_axis = axis.cross(width_axis).normalized()
    vertices = []
    sections = [(0.0, base_width), (0.12, base_width * 0.95), (0.72, tip_width * 1.25), (0.94, tip_width)]
    for t, width in sections:
        point = guard.lerp(tip, t)
        vertices += [
            tuple(point - width_axis * width / 2 - depth_axis * thickness / 2),
            tuple(point + width_axis * width / 2 - depth_axis * thickness / 2),
            tuple(point + width_axis * width / 2 + depth_axis * thickness / 2),
            tuple(point - width_axis * width / 2 + depth_axis * thickness / 2),
        ]
    tip_index = len(vertices)
    vertices.append(tuple(tip))
    faces = [(0, 3, 2, 1)]
    for section in range(len(sections) - 1):
        a = section * 4
        b = (section + 1) * 4
        faces.extend([(a, a + 1, b + 1, b), (a + 1, a + 2, b + 2, b + 1),
                      (a + 2, a + 3, b + 3, b + 2), (a + 3, a, b, b + 3)])
    last = (len(sections) - 1) * 4
    faces.extend([(last, last + 1, tip_index), (last + 1, last + 2, tip_index),
                  (last + 2, last + 3, tip_index), (last + 3, last, tip_index)])
    obj = new_mesh(name, vertices, faces, collection, material, False)
    return bevel(obj, 0.006, 2)


def shoulder_shell(name, side, layer, collection, material):
    u_count = 8
    v_count = 16
    inner = 0.205 + layer * 0.010
    outer = [0.345, 0.360, 0.350][layer]
    z_base = [1.405, 1.360, 1.315][layer]
    drop = [0.055, 0.075, 0.090][layer]
    vertices = []
    for u_index in range(u_count):
        u = u_index / (u_count - 1)
        x = side * (inner + (outer - inner) * (u ** 0.85))
        for v_index in range(v_count):
            angle = math.radians(-78 + 156 * v_index / (v_count - 1))
            y = -0.015 + 0.145 * math.sin(angle) * (0.92 + 0.08 * u)
            z = z_base + 0.042 * math.cos(angle) - drop * u + 0.012 * math.sin(math.pi * u)
            vertices.append((x, y, z))
    faces = []
    for u in range(u_count - 1):
        for v in range(v_count - 1):
            a = u * v_count + v
            faces.append((a, a + v_count, a + v_count + 1, a + 1))
    obj = new_mesh(name, vertices, faces, collection, material, True)
    solidify(obj, 0.018)
    return bevel(obj, 0.006, 2)


def chest_band(name, z, width, depth, collection, material, back=False):
    segments = 24
    rows = 4
    vertices = []
    for row in range(rows):
        row_t = row / (rows - 1)
        row_z = z + (row_t - 0.5) * 0.065
        for index in range(segments):
            t = index / (segments - 1)
            x = -width + width * 2 * t
            curve = math.sqrt(max(0.0, 1.0 - (x / (width * 1.08)) ** 2))
            y = (depth * curve + 0.012 * math.sin(math.pi * t)) * (1 if back else -1)
            vertices.append((x, y, row_z - 0.014 * abs(x / width)))
    faces = []
    for row in range(rows - 1):
        for index in range(segments - 1):
            a = row * segments + index
            faces.append((a, a + segments, a + segments + 1, a + 1))
    obj = new_mesh(name, vertices, faces, collection, material, True)
    solidify(obj, 0.016)
    return bevel(obj, 0.005, 2)


def tube_polyline(name, points, radius, segments, collection, material):
    radii = [(radius * (0.94 + 0.06 * math.sin(math.pi * i / max(1, len(points) - 1))), radius)
             for i in range(len(points))]
    return loft_path(name, points, radii, segments, collection, material, True)


def bone_parent(obj, armature, bone_name):
    world = obj.matrix_world.copy()
    obj.parent = armature
    obj.parent_type = "BONE"
    obj.parent_bone = bone_name
    obj.matrix_parent_inverse = (armature.matrix_world @ armature.pose.bones[bone_name].matrix).inverted()
    obj.matrix_world = world


def object_parent(obj, armature):
    world = obj.matrix_world.copy()
    obj.parent = armature
    obj.matrix_parent_inverse = armature.matrix_world.inverted()
    obj.matrix_world = world


def triangle_count(obj):
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)


def bounds(objects):
    points = [obj.matrix_world @ Vector(corner) for obj in objects for corner in obj.bound_box]
    low = Vector((min(p.x for p in points), min(p.y for p in points), min(p.z for p in points)))
    high = Vector((max(p.x for p in points), max(p.y for p in points), max(p.z for p in points)))
    return low, high


def build_geometry(body_collection, armor_collection, weapon_collection, materials):
    skin, cloth, armor, leather, wood, steel = materials
    body = []
    armor_objects = []
    weapons = []

    # Tapered under-body with clear chest, waist and pelvis rhythm.
    body.append(loft_z("Body_Base", [
        (0.98, 0.175, 0.120, 0, 0.012), (1.05, 0.185, 0.125, 0, 0.010),
        (1.14, 0.170, 0.120, 0, 0.006), (1.24, 0.205, 0.135, 0, 0.000),
        (1.34, 0.245, 0.150, 0, -0.004), (1.43, 0.265, 0.158, 0, -0.010),
        (1.48, 0.235, 0.145, 0, -0.008)], 32, body_collection, cloth))
    body.append(loft_z("Pelvis", [
        (0.91, 0.175, 0.120, 0, 0.018), (0.98, 0.200, 0.132, 0, 0.012),
        (1.06, 0.192, 0.128, 0, 0.010), (1.11, 0.170, 0.118, 0, 0.008)], 28,
        body_collection, cloth))
    body.append(loft_z("Neck", [(1.44, .075, .070, 0, -.005), (1.50, .082, .076, 0, -.012),
                                 (1.54, .078, .072, 0, -.018)], 24, body_collection, skin))

    def head_deform(co):
        # Establish jaw/chin and a slightly flatter human face plane toward -Y.
        normalized_z = co.z / 0.17
        if normalized_z < -0.20:
            factor = 0.78 + 0.18 * (normalized_z + 1.0) / 0.8
            co.x *= factor
            co.y *= 0.92
        if co.y < -0.02:
            co.y *= 0.86
        return co

    head = ellipsoid("Head", (0, -0.015, 1.585), (0.135, 0.120, 0.170), body_collection, skin,
                     36, 22, head_deform)
    body.append(head)
    # Readable primary facial planes: nose, brows, cheeks and chin.
    body.append(tapered_box("Face_Nose", (0, -0.142), .032, .052, .018, .008, 1.535, 1.625,
                            body_collection, skin, .006))
    body.append(tapered_box("Face_Brow_L", (-.050, -.128), .075, .060, .012, .008, 1.635, 1.660,
                            body_collection, skin, .005))
    body.append(tapered_box("Face_Brow_R", (.050, -.128), .075, .060, .012, .008, 1.635, 1.660,
                            body_collection, skin, .005))
    body.append(ellipsoid("Face_Cheek_L", (-.062, -.112, 1.555), (.045, .024, .050), body_collection, skin, 20, 12))
    body.append(ellipsoid("Face_Cheek_R", (.062, -.112, 1.555), (.045, .024, .050), body_collection, skin, 20, 12))
    body.append(ellipsoid("Face_Chin", (0, -.095, 1.437), (.058, .042, .040), body_collection, skin, 20, 12))

    # A-pose limbs, each with dedicated joint rhythm rather than uniform cylinders.
    for side, suffix in ((-1, "L"), (1, "R")):
        shoulder = Vector((side * .235, 0.000, 1.405))
        elbow = Vector((side * .505, -0.002, 1.245))
        wrist = Vector((side * .735, -0.030, 1.095))
        hand = Vector((side * .805, -0.050, 1.045))
        body.append(loft_path(f"UpperArm_{suffix}",
                              [shoulder, shoulder.lerp(elbow, .32), shoulder.lerp(elbow, .70), elbow],
                              [(.090, .083), (.102, .090), (.082, .075), (.070, .066)], 24,
                              body_collection, cloth))
        body.append(ellipsoid(f"Elbow_{suffix}", elbow, (.077, .070, .073), body_collection, cloth, 24, 14))
        body.append(loft_path(f"Forearm_{suffix}",
                              [elbow, elbow.lerp(wrist, .35), elbow.lerp(wrist, .72), wrist],
                              [(.073, .067), (.082, .072), (.065, .060), (.052, .050)], 24,
                              body_collection, cloth))
        body.append(loft_path(f"Hand_{suffix}",
                              [wrist, wrist.lerp(hand, .45), hand],
                              [(.060, .050), (.067, .054), (.050, .044)], 20,
                              body_collection, skin))
        thumb_center = wrist.lerp(hand, .50) + Vector((side * .018, -.043, -.008))
        body.append(ellipsoid(f"Thumb_{suffix}", thumb_center, (.027, .020, .044), body_collection, skin, 18, 10))

        hip = Vector((side * .145, .015, .985))
        knee = Vector((side * .150, .004, .630))
        ankle = Vector((side * .155, -.012, .205))
        body.append(loft_path(f"Thigh_{suffix}",
                              [hip, hip.lerp(knee, .32), hip.lerp(knee, .70), knee],
                              [(.120, .110), (.137, .122), (.110, .100), (.090, .085)], 26,
                              body_collection, cloth))
        body.append(ellipsoid(f"Knee_{suffix}", knee, (.103, .090, .098), body_collection, armor, 24, 14))
        body.append(loft_path(f"Calf_{suffix}",
                              [knee, knee.lerp(ankle, .28), knee.lerp(ankle, .64), ankle],
                              [(.087, .082), (.113, .100), (.095, .086), (.065, .062)], 26,
                              body_collection, cloth))
        # Rounded custom boot last with explicit toe, instep, heel and sole rhythm.
        boot_center_x = side * .155
        boot = loft_z(f"Boot_{suffix}", [
            (.035, .112, .180, boot_center_x, -.060),
            (.075, .115, .182, boot_center_x, -.062),
            (.115, .104, .152, boot_center_x, -.045),
            (.165, .086, .105, boot_center_x, -.018),
            (.205, .070, .074, boot_center_x, -.004)], 24, body_collection, leather)
        body.append(boot)
        sole = loft_z(f"BootSole_{suffix}", [
            (.000, .120, .195, boot_center_x, -.064),
            (.025, .122, .197, boot_center_x, -.064),
            (.052, .116, .187, boot_center_x, -.060)], 24, body_collection, leather)
        body.append(sole)

        # Curved bracers and major leg-wrap bands; these remain primary readability, not final detail.
        armor_objects.append(loft_path(f"Bracer_{suffix}",
                                       [elbow.lerp(wrist, .34), elbow.lerp(wrist, .52), elbow.lerp(wrist, .72)],
                                       [(.088, .078), (.087, .077), (.073, .067)], 28,
                                       armor_collection, armor))
        for band_index, t in enumerate((.43, .53, .63, .73)):
            center = knee.lerp(ankle, t)
            band = ellipsoid(f"LegWrap_{suffix}_{band_index+1}", center, (.104, .091, .028),
                             armor_collection, cloth, 20, 8)
            armor_objects.append(band)

    # Curved helmet construction.
    dome_rings = []
    for index in range(11):
        t = index / 10
        angle = math.radians(105 - 100 * t)
        radius = math.sin(angle)
        z = 1.585 + .150 * math.cos(angle)
        dome_rings.append((z, .151 * radius, .137 * radius, 0, -.004))
    helmet = loft_z("Helmet", dome_rings, 36, armor_collection, armor, cap=True)
    armor_objects.append(helmet)
    armor_objects.append(loft_z("Helmet_Rim", [(1.520, .156, .143, 0, -.003), (1.548, .158, .145, 0, -.003),
                                                  (1.570, .154, .141, 0, -.003)], 36,
                                         armor_collection, steel))
    armor_objects.append(ellipsoid("Helmet_TopMount", (0, -.004, 1.742), (.050, .046, .040),
                                    armor_collection, steel, 28, 14))
    plume = loft_z("Helmet_Plume", [(1.755, .030, .022, 0, -.002), (1.790, .040, .026, 0, -.002),
                                     (1.820, .026, .020, .003, -.002), (1.830, .006, .005, .008, -.002)], 20,
                   armor_collection, cloth)
    armor_objects.append(plume)

    # Layered shoulder shells, chest shell and four readable lamellar bands.
    for side, suffix in ((-1, "L"), (1, "R")):
        for layer in range(3):
            armor_objects.append(shoulder_shell(f"ShoulderArmor_{suffix}_{layer+1}", side, layer,
                                                armor_collection, armor))
    armor_objects.append(loft_z("ChestArmor", [
        (1.075, .180, .137, 0, -.008), (1.145, .205, .150, 0, -.010),
        (1.255, .242, .168, 0, -.012), (1.365, .268, .178, 0, -.015),
        (1.455, .240, .162, 0, -.013)], 36, armor_collection, armor))
    for index, z in enumerate((1.145, 1.225, 1.305, 1.385)):
        armor_objects.append(chest_band(f"ChestArmor_Lamellar_{index+1}", z, .235 + index * .006,
                                        .176, armor_collection, steel, False))
    armor_objects.append(tapered_box("ChestArmor_Center", (0, -.188), .085, .115, .018, .010,
                                     1.105, 1.425, armor_collection, steel, .009))

    # Scarf volume: neck wrap and diagonal chest fall.
    armor_objects.append(loft_z("Scarf", [(1.455, .105, .100, 0, -.010), (1.495, .122, .112, 0, -.014),
                                           (1.525, .108, .102, 0, -.014)], 28, armor_collection, cloth))
    armor_objects.append(tube_polyline("Scarf_Drape", [(-.105, -.185, 1.455), (-.050, -.205, 1.390),
                                                        (.018, -.205, 1.325), (.080, -.185, 1.265)],
                                           .038, 18, armor_collection, cloth))

    # Belt, tapered front/rear cloth and separated side armor panels.
    armor_objects.append(loft_z("WaistArmor_Belt", [(1.010, .196, .135, 0, .010), (1.055, .205, .140, 0, .008),
                                                      (1.085, .195, .132, 0, .008)], 32,
                                         armor_collection, leather))
    armor_objects.append(tapered_box("WaistCloth", (0, -.142), .150, .205, .024, .014,
                                     .650, 1.030, armor_collection, cloth, .014))
    armor_objects.append(tapered_box("WaistCloth_Rear", (0, .125), .140, .190, .014, .024,
                                     .710, 1.025, armor_collection, cloth, .014))
    for side, suffix in ((-1, "L"), (1, "R")):
        armor_objects.append(tapered_box(f"WaistArmor_{suffix}", (side * .160, .000), .145, .175,
                                         .095, .095, .730, 1.020, armor_collection, armor, .016))

    # Shield construction: shaped board, substantial rim, boss and main reinforcement.
    shield_center = (-.590, -.165, .830)
    shield_outline = [(-.235, .430), (.235, .430), (.300, .265), (.285, -.135),
                      (.145, -.365), (0, -.435), (-.145, -.365), (-.285, -.135), (-.300, .265)]
    shield_board = shield_prism("Shield", shield_outline, shield_center, .062, weapon_collection, wood, .016)
    shield_rim = shield_frame("Shield_Rim", shield_outline, .865, shield_center, .078,
                              weapon_collection, steel)
    boss = ellipsoid("Shield_Boss", (-.590, -.220, .840), (.128, .066, .128), weapon_collection, steel, 32, 18)
    reinforce_v = tapered_box("Shield_Reinforcement_V", (-.590, -.222), .052, .072, .015, .010,
                              .475, 1.225, weapon_collection, steel, .008)
    reinforce_h = tapered_box("Shield_Reinforcement_H", (-.590, -.222), .410, .455, .015, .010,
                              .792, .865, weapon_collection, steel, .008)
    weapons += [shield_board, shield_rim, boss, reinforce_v, reinforce_h]

    # Sword: one-metre class, clearly tapered blade with guard, grip and pommel.
    guard_center = Vector((.790, -.070, 1.040))
    blade_tip = Vector((1.025, -.095, .150))
    sword_blade = blade("Sword", guard_center, blade_tip, .090, .036, .024,
                        weapon_collection, steel)
    grip_end = guard_center + (guard_center - blade_tip).normalized() * .155
    grip = loft_path("Sword_Grip", [guard_center, guard_center.lerp(grip_end, .5), grip_end],
                     [(.035, .030), (.034, .029), (.032, .028)], 20, weapon_collection, leather)
    guard_axis = Vector((1.0, 0.0, .20)).normalized()
    guard_points = [guard_center - guard_axis * .105, guard_center - guard_axis * .050,
                    guard_center, guard_center + guard_axis * .050, guard_center + guard_axis * .105]
    guard = tube_polyline("Sword_Guard", guard_points, .022, 16, weapon_collection, steel)
    sword_axis = (guard_center - blade_tip).normalized()
    pommel = loft_path("Sword_Pommel", [grip_end - sword_axis * .035, grip_end, grip_end + sword_axis * .035],
                       [(.030, .027), (.048, .040), (.030, .027)], 20, weapon_collection, steel)
    weapons += [sword_blade, grip, guard, pommel]

    return body, armor_objects, weapons


def main():
    args = arguments()
    output_root = Path(args.output_root).resolve()
    source_dir = output_root / "Source"
    documentation_dir = output_root / "Documentation"
    source_dir.mkdir(parents=True, exist_ok=True)
    documentation_dir.mkdir(parents=True, exist_ok=True)

    opened = Path(bpy.data.filepath).resolve()
    actual_v002_hash = sha256(opened)
    if opened.name != "CHR_Infantry_A_v002.blend":
        raise RuntimeError(f"Expected v002 source input, opened: {opened}")
    if actual_v002_hash != args.expected_v002_sha256.upper():
        raise RuntimeError(f"v002 checksum mismatch: {actual_v002_hash}")

    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    scene["AssetId"] = ASSET_ID
    scene["SourceVersion"] = SOURCE_VERSION
    scene["ProductionStatus"] = STATUS
    scene["Phase"] = "02_PRIMARY_FORMS"
    scene["RootMotionContract"] = "OFF"
    scene["RuntimePrefabContract"] = "PF_Unit_Infantry (not modified by this candidate)"

    # Remove prototype meshes only in this newly loaded in-memory scene.
    for obj in list(bpy.data.objects):
        if obj.type == "MESH":
            bpy.data.objects.remove(obj, do_unlink=True)
    for material in list(bpy.data.materials):
        bpy.data.materials.remove(material)

    root_collection = ensure_collection("CHR_Infantry_A_v003")
    body_collection = ensure_collection("GEO_BODY", root_collection)
    armor_collection = ensure_collection("GEO_ARMOR", root_collection)
    weapon_collection = ensure_collection("GEO_WEAPON", root_collection)
    rig_collection = ensure_collection("RIG", root_collection)
    ensure_collection("REVIEW", root_collection)

    armature = bpy.data.objects.get("Armature")
    if armature is None or armature.type != "ARMATURE":
        raise RuntimeError("v002 Armature not found")
    move_to_collection(armature, rig_collection)
    armature["Contract"] = "Preserved v002 23-bone Humanoid reference; temporary Phase 02 binding"
    for obj in list(bpy.data.objects):
        if obj.type == "EMPTY":
            move_to_collection(obj, rig_collection)
    legacy_default = bpy.data.collections.get("Collection")
    if legacy_default is not None and not legacy_default.objects and not legacy_default.children:
        bpy.data.collections.remove(legacy_default)

    required_bones = {"Root", "Hips", "Spine", "Chest", "UpperChest", "Neck", "Head",
                      "LeftShoulder", "LeftUpperArm", "LeftLowerArm", "LeftHand",
                      "RightShoulder", "RightUpperArm", "RightLowerArm", "RightHand",
                      "LeftUpperLeg", "LeftLowerLeg", "LeftFoot", "RightUpperLeg", "RightLowerLeg", "RightFoot"}
    missing_bones = sorted(required_bones - set(armature.data.bones.keys()))
    if missing_bones:
        raise RuntimeError(f"Missing preserved bones: {missing_bones}")

    materials = (
        make_material("MAT_v003_Review_Skin", (.49, .37, .30), 0.0, .78),
        make_material("MAT_v003_Review_Cloth", (.28, .33, .40), 0.0, .88),
        make_material("MAT_v003_Review_Armor", (.30, .34, .39), .38, .52),
        make_material("MAT_v003_Review_Leather", (.24, .16, .105), 0.0, .82),
        make_material("MAT_v003_Review_Wood", (.32, .20, .11), 0.0, .76),
        make_material("MAT_v003_Review_Steel", (.38, .42, .47), .62, .38),
    )
    body, armor_objects, weapons = build_geometry(body_collection, armor_collection, weapon_collection, materials)

    # Temporary, explicit Phase 02 binding. No final weights or animations are authored.
    for obj in body + armor_objects:
        object_parent(obj, armature)
        obj["BindingStatus"] = "PHASE02_STATIC_APOSE_REVIEW"
    for obj in weapons:
        # Bone parenting in Blender applies bone-length space scaling to these freshly
        # generated review meshes. Keep their world-space A-pose dimensions exact and
        # register the intended attachment bone explicitly for the later skinning phase.
        object_parent(obj, armature)
        obj["AttachmentBone"] = "LeftHand" if obj.name.startswith("Shield") else "RightHand"
        obj["BindingStatus"] = "PHASE02_RIGID_ATTACHMENT_PLANNED"

    mesh_objects = body + armor_objects + weapons
    low, high = bounds(mesh_objects)
    height = high.z - low.z
    # Ground precisely at Z=0 while preserving target height.
    if abs(low.z) > 0.00001:
        for obj in mesh_objects:
            obj.location.z -= low.z
        bpy.context.view_layer.update()
        low, high = bounds(mesh_objects)
        height = high.z - low.z
    if not (1.80 <= height <= 1.85):
        per_object = []
        for obj in mesh_objects:
            values = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
            per_object.append((obj.name, min(value.z for value in values), max(value.z for value in values)))
        per_object.sort(key=lambda value: value[2], reverse=True)
        raise RuntimeError(f"Primary Forms height out of range: {height:.6f} m; bounds={list(low)}->{list(high)}; extrema={per_object[:6]} / {sorted(per_object, key=lambda value:value[1])[:6]}")

    triangle_total = sum(triangle_count(obj) for obj in mesh_objects)
    vertex_total = sum(len(obj.data.vertices) for obj in mesh_objects)
    if not (20000 <= triangle_total <= 35000):
        raise RuntimeError(f"Primary Forms triangle count outside task envelope: {triangle_total}")

    # Explicitly clear active animation. Existing v002 external clips remain untouched.
    if armature.animation_data:
        armature.animation_data.action = None
    scene.frame_set(0)
    bpy.context.view_layer.update()

    output_blend = source_dir / "CHR_Infantry_A_v003.blend"
    if output_blend.name == opened.name or output_blend.resolve() == opened:
        raise RuntimeError("Refusing to overwrite v002")
    bpy.context.preferences.filepaths.save_version = 0
    bpy.ops.wm.save_as_mainfile(filepath=str(output_blend))

    category_triangles = {
        "Body_Head_Limbs": sum(triangle_count(obj) for obj in body),
        "Armor_Cloth": sum(triangle_count(obj) for obj in armor_objects),
        "Shield": sum(triangle_count(obj) for obj in weapons if obj.name.startswith("Shield")),
        "Sword": sum(triangle_count(obj) for obj in weapons if obj.name.startswith("Sword")),
    }
    result = {
        "status": STATUS,
        "asset_id": ASSET_ID,
        "source_version": SOURCE_VERSION,
        "blender_version": bpy.app.version_string,
        "input_v002": str(opened),
        "input_v002_sha256": actual_v002_hash,
        "output_blend": str(output_blend),
        "output_blend_sha256": sha256(output_blend),
        "character_bounds_m": {"min": list(low), "max": list(high), "height_z": height},
        "mesh_count": len(mesh_objects),
        "vertices": vertex_total,
        "triangles": triangle_total,
        "materials": [material.name for material in materials],
        "material_count": len(materials),
        "armature": armature.name,
        "bone_count": len(armature.data.bones),
        "category_triangles": category_triangles,
        "objects": [
            {"name": obj.name, "vertices": len(obj.data.vertices), "triangles": triangle_count(obj),
             "collection": obj.users_collection[0].name if obj.users_collection else "",
             "parent": obj.parent.name if obj.parent else "", "parent_type": obj.parent_type,
             "parent_bone": obj.parent_bone}
            for obj in sorted(mesh_objects, key=lambda item: item.name)
        ],
        "deferred": ["Final UV", "Final Texture", "Final Team Color Mask", "Final Skinning",
                     "Animation Polish", "Final LOD chain", "Shader rewrite", "Runtime Prefab replacement"],
    }
    (documentation_dir / "BUILD_RESULT.json").write_text(json.dumps(result, indent=2, ensure_ascii=False), encoding="utf-8")
    print("AEGIS_V003_PRIMARY_FORMS_COMPLETE")
    print(json.dumps(result, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
