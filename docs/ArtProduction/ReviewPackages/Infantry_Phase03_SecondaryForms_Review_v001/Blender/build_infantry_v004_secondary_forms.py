"""Build CHR_Infantry_A_v004 Secondary Forms from immutable P02R1.

Blender 5.2 background script.  It refuses an unexpected source checksum, keeps
the approved primary silhouette, adds only medium-scale construction forms, and
saves exclusively to the versioned v004 output path.
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

INPUT_VERSION = "CHR_Infantry_A_v003_P02R1"
OUTPUT_VERSION = "CHR_Infantry_A_v004"
REVIEW_STATUS = "READY FOR PHASE03 REVIEW"


def args():
    p = argparse.ArgumentParser()
    p.add_argument("--output-root", required=True)
    p.add_argument("--expected-input-sha256", required=True)
    argv = __import__("sys").argv
    return p.parse_args(argv[argv.index("--") + 1:] if "--" in argv else [])


def sha256(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest().upper()


def load_helpers():
    path = Path(__file__).resolve().parents[2] / "v003" / "Source" / "build_infantry_v003_primary_forms.py"
    spec = importlib.util.spec_from_file_location("aegis_geometry", path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def material(name, color, metallic=0.0, roughness=.72):
    value = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    value.diffuse_color = (*color, 1.0)
    value.use_nodes = True
    node = value.node_tree.nodes.get("Principled BSDF")
    node.inputs["Base Color"].default_value = (*color, 1.0)
    node.inputs["Metallic"].default_value = metallic
    node.inputs["Roughness"].default_value = roughness
    value["PreviewOnly"] = True
    value["FinalTexture"] = False
    return value


def assign(obj, mat):
    obj.data.materials.clear()
    obj.data.materials.append(mat)


def parent_review(obj, armature, bone=""):
    world = obj.matrix_world.copy()
    obj.parent = armature
    obj.parent_type = "OBJECT"
    obj.matrix_parent_inverse = armature.matrix_world.inverted()
    obj.matrix_world = world
    obj["BindingStatus"] = "PHASE03_STATIC_APOSE_REVIEW"
    obj["AttachmentBone"] = bone
    obj["SecondaryForms"] = True


def multi_boxes(build, name, boxes, collection, mat, bevel=.004):
    vertices, faces = [], []
    for cx, cy, cz, sx, sy, sz in boxes:
        base = len(vertices)
        vertices += [(cx + x*sx/2, cy + y*sy/2, cz + z*sz/2)
                     for x,y,z in ((-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),
                                   (-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1))]
        faces += [tuple(base+i for i in q) for q in
                  ((0,1,2,3),(4,7,6,5),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0))]
    obj = build.new_mesh(name, vertices, faces, collection, mat, False)
    return build.bevel(obj, bevel, 2)


def torus(name, location, major, minor, rotation, collection, mat):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor,
                                     major_segments=20, minor_segments=6,
                                     location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.data.name = name + "_Mesh"
    for c in list(obj.users_collection): c.objects.unlink(obj)
    collection.objects.link(obj)
    assign(obj, mat)
    return obj


def bounds(objects):
    points = [o.matrix_world @ Vector(c) for o in objects for c in o.bound_box]
    return (Vector((min(p.x for p in points), min(p.y for p in points), min(p.z for p in points))),
            Vector((max(p.x for p in points), max(p.y for p in points), max(p.z for p in points))))


def tris(obj):
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)


def topology(objects):
    out = dict(non_manifold_edges=0, boundary_edges=0, loose_edges=0, zero_area_faces=0)
    for obj in objects:
        bm = bmesh.new(); bm.from_mesh(obj.data)
        out["non_manifold_edges"] += sum(not e.is_manifold for e in bm.edges)
        out["boundary_edges"] += sum(e.is_boundary for e in bm.edges)
        out["loose_edges"] += sum(not e.link_faces for e in bm.edges)
        out["zero_area_faces"] += sum(f.calc_area() <= 1e-10 for f in bm.faces)
        bm.free()
    return out


def main():
    a = args(); opened = Path(bpy.data.filepath).resolve(); actual = sha256(opened)
    if bpy.context.scene.get("SourceVersion") != INPUT_VERSION:
        raise RuntimeError("Phase 03 requires CHR_Infantry_A_v003_P02R1")
    if actual != a.expected_input_sha256.upper():
        raise RuntimeError(f"Protected P02R1 checksum mismatch: {actual}")
    build = load_helpers(); scene = bpy.context.scene
    armature = bpy.data.objects.get("Armature")
    if not armature or armature.type != "ARMATURE" or len(armature.data.bones) != 23:
        raise RuntimeError("Expected preserved 23-bone armature")
    bones_before = sorted(b.name for b in armature.data.bones)
    empties_before = sorted(o.name for o in scene.objects if o.type == "EMPTY")

    root = bpy.data.collections.get("CHR_Infantry_A_v003")
    if root: root.name = "CHR_Infantry_A_v004"
    armor = bpy.data.collections["GEO_ARMOR"]
    body = bpy.data.collections["GEO_BODY"]
    weapon = bpy.data.collections.get("GEO_WEAPON")
    if weapon: weapon.name = "GEO_WEAPONS"
    weapon = bpy.data.collections["GEO_WEAPONS"]
    cloth_col = bpy.data.collections.get("GEO_CLOTH") or bpy.data.collections.new("GEO_CLOTH")
    if root and cloth_col.name not in root.children: root.children.link(cloth_col)

    mats = {
        "Metal": material("MATID_Metal", (.32,.38,.45), .72, .36),
        "Wood": material("MATID_Wood", (.34,.17,.075), 0, .82),
        "Leather": material("MATID_Leather", (.12,.065,.04), 0, .78),
        "Cloth": material("MATID_Cloth", (.24,.28,.32), 0, .92),
        "Skin": material("MATID_Skin", (.55,.30,.21), 0, .80),
        "Team": material("MATID_Team", (.58,.055,.045), 0, .84),
    }
    for obj in [o for o in scene.objects if o.type == "MESH"]:
        old = obj.material_slots[0].material.name if obj.material_slots and obj.material_slots[0].material else ""
        key = "Metal" if ("Armor" in old or "Steel" in old) else "Wood" if "Wood" in old else "Leather" if "Leather" in old else "Skin" if "Skin" in old else "Cloth"
        if any(t in obj.name for t in ("Scarf", "WaistCloth", "Plume")): key = "Team"
        assign(obj, mats[key])
        if key in ("Cloth", "Team"):
            for c in list(obj.users_collection): c.objects.unlink(obj)
            cloth_col.objects.link(obj)

    made = []
    def add(obj, bone=""):
        parent_review(obj, armature, bone); made.append(obj); return obj

    # Chest: four continuous overlapping rows remain; five raised divisions per row
    # and two side-return rails make construction readable without dozens of plates.
    chest_boxes=[]
    for z in (1.145,1.205,1.265,1.325):
        for x in (-.19,-.095,0,.095,.19): chest_boxes.append((x,-.186,z,.008,.012,.038))
    add(multi_boxes(build,"GEO_Infantry_ChestArmor_RaisedDivisions",chest_boxes,armor,mats["Metal"],.0025),"Chest")
    add(multi_boxes(build,"GEO_Infantry_ChestArmor_SideReturns",
                    [(-.247,-.075,1.245,.028,.17,.27),(.247,-.075,1.245,.028,.17,.27)],armor,mats["Metal"],.005),"Chest")
    add(multi_boxes(build,"GEO_Infantry_ChestArmor_UpperSupport",
                    [(0,-.172,1.385,.43,.038,.045)],armor,mats["Metal"],.006),"Chest")

    # Shoulder attachment and edge hierarchy.
    for side,sfx,bone in ((-1,"L","LeftUpperArm"),(1,"R","RightUpperArm")):
        add(build.tapered_box(f"GEO_Infantry_ShoulderAnchor_{sfx}",(side*.235,-.005),.075,.095,.095,.085,1.35,1.44,armor,mats["Leather"],.007),bone)
        add(build.loft_path(f"GEO_Infantry_ShoulderOuterEdge_{sfx}",
             [(side*.285,-.13,1.405),(side*.34,-.02,1.37),(side*.31,.12,1.34)],
             [(.014,.012)]*3,10,armor,mats["Metal"],True),bone)
        add(build.tapered_box(f"GEO_Infantry_ShoulderUnderPlate_{sfx}",(side*.30,.015),.10,.13,.09,.09,1.29,1.36,armor,mats["Metal"],.006),bone)

    # Helmet rim/brow, rear guard and plume construction.
    add(build.loft_path("GEO_Infantry_Helmet_BrowBand",
        [(-.105,-.123,1.665),(0,-.143,1.658),(.105,-.123,1.665)],[(.009,.006)]*3,10,armor,mats["Metal"],True),"Head")
    add(build.shield_prism("GEO_Infantry_Helmet_RearGuard",
        [(-.12,0),(.12,0),(.105,-.11),(-.105,-.11)],(0,.105,1.63),.022,armor,mats["Metal"],.006),"Head")
    add(torus("GEO_Infantry_PlumeMount_Ring",(0,.012,1.742),.041,.009,(math.pi/2,0,0),armor,mats["Metal"]),"Head")
    add(build.loft_path("GEO_Infantry_Plume_Division",[(0,.045,1.784),(0,.11,1.79),(0,.175,1.775)],[(.008,.012)]*3,8,cloth_col,mats["Team"],True),"Head")

    # Scarf broad folds and rear termination.
    add(build.loft_path("GEO_Infantry_Scarf_Fold_A",[(-.10,-.115,1.49),(0,-.14,1.475),(.10,-.115,1.46)],[(.012,.010)]*3,8,cloth_col,mats["Team"],True),"Chest")
    add(build.loft_path("GEO_Infantry_Scarf_Fold_B",[(-.08,-.112,1.445),(0,-.14,1.43),(.08,-.112,1.415)],[(.010,.009)]*3,8,cloth_col,mats["Team"],True),"Chest")
    add(build.tapered_box("GEO_Infantry_Scarf_RearTermination",(.08,.12),.075,.09,.018,.022,1.31,1.47,cloth_col,mats["Team"],.006),"Chest")

    # Bracers, hand grip contact, belt and waist attachment.
    for side,sfx,bone in ((-1,"L","LeftLowerArm"),(1,"R","RightLowerArm")):
        add(torus(f"GEO_Infantry_Bracer_Rim_{sfx}",(side*.64,-.035,1.16),.075,.009,(0,math.pi/2,0),armor,mats["Metal"]),bone)
        add(multi_boxes(build,f"GEO_Infantry_Bracer_Strap_{sfx}",[(side*.62,.035,1.16,.035,.025,.13)],armor,mats["Leather"],.004),bone)
    add(build.loft_path("GEO_Infantry_Sword_GripContact",[(.755,-.045,1.09),(.79,-.04,1.04),(.81,-.035,.99)],[(.038,.026)]*3,10,body,mats["Leather"],True),"RightHand")
    add(multi_boxes(build,"GEO_Infantry_Belt_Clasp",[(0,-.16,1.005,.095,.032,.085)],armor,mats["Metal"],.009),"Hips")
    add(multi_boxes(build,"GEO_Infantry_Waist_AttachmentTabs",
        [(-.17,-.13,.96,.055,.025,.09),(.17,-.13,.96,.055,.025,.09),(-.17,.12,.96,.055,.025,.09),(.17,.12,.96,.055,.025,.09)],armor,mats["Leather"],.005),"Hips")
    add(build.loft_path("GEO_Infantry_WaistCloth_FrontFold",[(0,-.17,.97),(-.012,-.17,.81),(.018,-.17,.66)],[(.010,.008)]*3,8,cloth_col,mats["Team"],True),"Hips")

    # Leg wrap termination, knee compression, and boot major panels.
    for side,sfx,bone in ((-1,"L","LeftFoot"),(1,"R","RightFoot")):
        add(build.tapered_box(f"GEO_Infantry_LegWrap_TuckedEnd_{sfx}",(side*.16,-.105),.055,.04,.018,.018,.32,.43,cloth_col,mats["Cloth"],.005),bone)
        add(torus(f"GEO_Infantry_KneeClothBoundary_{sfx}",(side*.15,0,.59),.103,.010,(0,0,0),cloth_col,mats["Cloth"]),bone)
        add(build.tapered_box(f"GEO_Infantry_Boot_UpperPanel_{sfx}",(side*.155,-.005),.13,.105,.075,.070,.105,.205,body,mats["Leather"],.007),bone)
        add(build.ellipsoid(f"GEO_Infantry_Boot_ToePanel_{sfx}",(side*.155,-.135,.105),(.068,.105,.034),body,mats["Leather"],16,8),bone)
        add(multi_boxes(build,f"GEO_Infantry_Boot_HeelBlock_{sfx}",[(side*.155,.12,.075,.16,.10,.09)],body,mats["Leather"],.007),bone)

    # Shield front/back construction.  Existing primary board/rim/reinforcement stay.
    add(torus("GEO_Infantry_Shield_BossBase",(-.59,-.225,.84),.132,.014,(math.pi/2,0,0),weapon,mats["Metal"]),"LeftHand")
    add(multi_boxes(build,"GEO_Infantry_Shield_WoodPanelSeams",
        [(-.69,-.205,.84,.012,.018,.70),(-.49,-.205,.84,.012,.018,.70)],weapon,mats["Wood"],.002),"LeftHand")
    add(multi_boxes(build,"GEO_Infantry_Shield_BackBrace",
        [(-.59,-.115,.84,.40,.035,.055),(-.59,-.115,.84,.055,.035,.66)],weapon,mats["Metal"],.006),"LeftHand")
    add(build.loft_path("GEO_Infantry_Shield_BackGrip",[(-.69,-.075,.78),(-.59,-.045,.84),(-.49,-.075,.90)],[(.025,.018)]*3,12,weapon,mats["Leather"],True),"LeftHand")
    add(build.loft_path("GEO_Infantry_Shield_ForearmStrap",[(-.71,-.07,.96),(-.59,-.035,1.02),(-.47,-.07,.96)],[(.022,.014)]*3,12,weapon,mats["Leather"],True),"LeftLowerArm")
    add(build.shield_prism("GEO_Infantry_Shield_TeamPanel",[(-.12,-.10),(.12,-.10),(.10,.10),(-.10,.10)],(-.59,-.215,.60),.014,weapon,mats["Team"],.006),"LeftHand")

    # Sword spine and broad grip wraps; no ornament or micro detail.
    add(build.loft_path("GEO_Infantry_Sword_BladeSpine",[(.80,-.073,.93),(.955,-.088,.40),(1.01,-.096,.205)],[(.006,.004)]*3,8,weapon,mats["Metal"],True),"RightHand")
    add(multi_boxes(build,"GEO_Infantry_Sword_GripWraps",
        [(x,-.025,z,.075,.050,.018) for x,z in ((.79,1.02),(.79,.985),(.79,.95))],weapon,mats["Leather"],.004),"RightHand")

    meshes=[o for o in scene.objects if o.type=="MESH"]
    for obj in meshes:
        bm=bmesh.new(); bm.from_mesh(obj.data); bmesh.ops.recalc_face_normals(bm,faces=bm.faces); bm.to_mesh(obj.data); bm.free()
        obj.data.update()
    bpy.context.view_layer.update(); low,high=bounds(meshes)
    triangle_count=sum(tris(o) for o in meshes); vertex_count=sum(len(o.data.vertices) for o in meshes)
    if not 1.80 <= high.z-low.z <= 1.85: raise RuntimeError("Primary height changed outside gate")
    if not 28000 <= triangle_count <= 38000: raise RuntimeError(f"Phase03 triangle gate: {triangle_count}")
    if sorted(b.name for b in armature.data.bones)!=bones_before: raise RuntimeError("Bone contract changed")
    if sorted(o.name for o in scene.objects if o.type=="EMPTY")!=empties_before: raise RuntimeError("Socket contract changed")
    if any(o.name.startswith(("Cube","Cylinder","Sphere","Cone","Plane")) for o in scene.objects): raise RuntimeError("Generic object name remains")

    scene["SourceVersion"]=OUTPUT_VERSION; scene["SourceBaseline"]=INPUT_VERSION
    scene["SourceBaselineSHA256"]=actual; scene["ProductionStatus"]="WIP_MODEL"
    scene["ReviewStatus"]=REVIEW_STATUS; scene["Phase"]="03_SECONDARY_FORMS"
    scene["FinalTexture"]=False; scene["FinalUV"]=False; scene["FinalSkinning"]=False
    scene["AnimationPolish"]=False; scene["FormalLOD"]=False
    scene["RuntimePrefabContract"]="PF_Unit_Infantry (not modified)"
    scene["Phase03Authorization"]="User direct execution instruction 2026-08-13"

    outroot=Path(a.output_root).resolve(); source=outroot/"Source"; docs=outroot/"Documentation"
    source.mkdir(parents=True,exist_ok=True); docs.mkdir(parents=True,exist_ok=True)
    output=source/"CHR_Infantry_A_v004.blend"
    if output==opened: raise RuntimeError("Refusing protected overwrite")
    bpy.context.preferences.filepaths.save_version=0
    bpy.ops.wm.save_as_mainfile(filepath=str(output))
    result={"status":REVIEW_STATUS,"source_version":OUTPUT_VERSION,"input":str(opened),
      "input_sha256":actual,"output":str(output),"output_sha256":sha256(output),
      "blender_version":bpy.app.version_string,"height_m":high.z-low.z,"bounds":{"min":list(low),"max":list(high)},
      "mesh_count":len(meshes),"vertices":vertex_count,"triangles":triangle_count,"material_ids":sorted(mats),
      "material_count":6,"armatures":1,"bones":len(armature.data.bones),"empties":len(empties_before),"actions":len(bpy.data.actions),
      "collections":sorted(c.name for c in bpy.data.collections),"topology":topology(meshes),
      "secondary_objects_added":len(made),"preserved_primary_forms":True,
      "deferred":["Final Texture","Final UV","Final Skinning","Animation Polish","Formal LOD","Runtime Prefab replacement"]}
    (docs/"P03_BUILD_RESULT.json").write_text(json.dumps(result,indent=2,ensure_ascii=False),encoding="utf-8")
    print("AEGIS_PHASE03_BUILD_COMPLETE"); print(json.dumps(result,indent=2,ensure_ascii=False))


if __name__ == "__main__": main()
