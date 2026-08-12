import bpy
import math
import os
import sys
import json
import hashlib
import shutil
from mathutils import Vector, Matrix

FPS = 30
PROMPT = r'''請基於 `CHR_Infantry_A_v001` 補交 L3。
需要可編輯 `.blend` 原始檔及 Unity Humanoid 相容 FBX。角色必須為 A-Pose 或 T-Pose，盾牌與短劍分離並可掛到左右手骨骼。
動畫包含 Idle、Move、Attack_A、Hit、Death，全部 In Place、Root Motion 關閉。
Attack_A 需要 `AttackImpact` 事件時間。
另提供單一灰階 Team Color Mask，不要再輸出藍紅兩套重複網格。
請附生成工具、版本、完整 Prompt、Seed／Job ID、人工修改、第三方素材及商用授權紀錄。'''


def parse_args():
    args = sys.argv
    if '--' in args:
        args = args[args.index('--')+1:]
    else:
        args = []
    kv = {}
    i = 0
    while i < len(args):
        if args[i].startswith('--') and i + 1 < len(args):
            kv[args[i][2:]] = args[i+1]
            i += 2
        else:
            i += 1
    root = os.path.abspath(kv.get('package-root', os.path.join(os.path.dirname(__file__), '..')))
    return root


def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.armatures, bpy.data.actions, bpy.data.materials):
        pass


def import_glb(path, prefix):
    before = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=path)
    imported = [o for o in bpy.data.objects if o not in before]
    meshes = []
    for o in imported:
        if o.type == 'MESH':
            original = o.name.split('.')[0]
            o['source_name'] = original
            o.name = f'{prefix}__{original}'
            meshes.append(o)
        elif o.type in {'EMPTY','LIGHT','CAMERA'}:
            bpy.data.objects.remove(o, do_unlink=True)
    return meshes


def world_bbox(objects):
    pts = []
    for o in objects:
        for c in o.bound_box:
            pts.append(o.matrix_world @ Vector(c))
    xs=[p.x for p in pts]; ys=[p.y for p in pts]; zs=[p.z for p in pts]
    return Vector((min(xs),min(ys),min(zs))), Vector((max(xs),max(ys),max(zs)))


def apply_world_rotation_about(objects, pivot, angle_rad, axis='Y'):
    if axis == 'Y':
        R = Matrix.Rotation(angle_rad, 4, 'Y')
    elif axis == 'X':
        R = Matrix.Rotation(angle_rad, 4, 'X')
    else:
        R = Matrix.Rotation(angle_rad, 4, 'Z')
    T1 = Matrix.Translation(pivot)
    T0 = Matrix.Translation(-pivot)
    M = T1 @ R @ T0
    for o in objects:
        o.matrix_world = M @ o.matrix_world


def src_name(o):
    return o.get('source_name', o.name.split('__')[-1].split('.')[0])


def is_left_arm(name):
    keys = ('ShoulderPad_L','Team_ShoulderBand_L','UpperArm_L','ForeArm_L','Hand_L','ElbowGuard_L','ArmSplint_L')
    return any(name.startswith(k) for k in keys)


def is_right_arm(name):
    keys = ('ShoulderPad_R','Team_ShoulderBand_R','UpperArm_R','ForeArm_R','Hand_R','ElbowGuard_R','ArmSplint_R')
    return any(name.startswith(k) for k in keys)


def is_shield(name):
    return name.startswith('Shield_') or name.startswith('Team_ShieldPanel')


def is_sword(name):
    return name.startswith('Sword_')


def obj_center(o):
    return o.matrix_world.translation.copy()


def raise_arms_to_apose(meshes):
    # glTF import yields Blender Z-up. Raise arms ~60 degrees from the source relaxed pose.
    l_upper = next((o for o in meshes if src_name(o) == 'UpperArm_L'), None)
    r_upper = next((o for o in meshes if src_name(o) == 'UpperArm_R'), None)
    if l_upper:
        bb = [l_upper.matrix_world @ Vector(c) for c in l_upper.bound_box]
        pivot = Vector((sum(p.x for p in bb)/8, sum(p.y for p in bb)/8, max(p.z for p in bb)))
        group=[o for o in meshes if is_left_arm(src_name(o)) or is_shield(src_name(o))]
        apply_world_rotation_about(group, pivot, math.radians(60), 'Y')
    if r_upper:
        bb = [r_upper.matrix_world @ Vector(c) for c in r_upper.bound_box]
        pivot = Vector((sum(p.x for p in bb)/8, sum(p.y for p in bb)/8, max(p.z for p in bb)))
        group=[o for o in meshes if is_right_arm(src_name(o)) or is_sword(src_name(o))]
        apply_world_rotation_about(group, pivot, math.radians(-60), 'Y')


def add_bone(arm, name, head, tail, parent=None, use_connect=False):
    b = arm.edit_bones.new(name)
    b.head = head
    b.tail = tail
    if parent:
        b.parent = arm.edit_bones[parent]
        b.use_connect = use_connect
    return b


def create_humanoid_armature(height=1.80):
    arm_data = bpy.data.armatures.new('Armature_Infantry')
    arm_obj = bpy.data.objects.new('Armature', arm_data)
    bpy.context.collection.objects.link(arm_obj)
    arm_obj.show_in_front = True
    arm_obj.rotation_euler = (0,0,0)
    arm_obj.scale = (1,1,1)
    bpy.context.view_layer.objects.active = arm_obj
    arm_obj.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT')

    z = lambda a: a * height
    add_bone(arm_data,'Root',(0,0,0),(0,0,z(.08)))
    add_bone(arm_data,'Hips',(0,0,z(.44)),(0,0,z(.54)),'Root')
    add_bone(arm_data,'Spine',(0,0,z(.54)),(0,0,z(.64)),'Hips',True)
    add_bone(arm_data,'Chest',(0,0,z(.64)),(0,0,z(.73)),'Spine',True)
    add_bone(arm_data,'UpperChest',(0,0,z(.73)),(0,0,z(.79)),'Chest',True)
    add_bone(arm_data,'Neck',(0,0,z(.79)),(0,0,z(.84)),'UpperChest',True)
    add_bone(arm_data,'Head',(0,0,z(.84)),(0,0,z(.98)),'Neck',True)

    # A-Pose arms, ~30 degrees below horizontal.
    add_bone(arm_data,'LeftShoulder',(-z(.04),0,z(.76)),(-z(.13),0,z(.755)),'UpperChest')
    add_bone(arm_data,'LeftUpperArm',(-z(.13),0,z(.755)),(-z(.265),0,z(.68)),'LeftShoulder',True)
    add_bone(arm_data,'LeftLowerArm',(-z(.265),0,z(.68)),(-z(.40),0,z(.605)),'LeftUpperArm',True)
    add_bone(arm_data,'LeftHand',(-z(.40),0,z(.605)),(-z(.455),0,z(.575)),'LeftLowerArm',True)
    add_bone(arm_data,'RightShoulder',(z(.04),0,z(.76)),(z(.13),0,z(.755)),'UpperChest')
    add_bone(arm_data,'RightUpperArm',(z(.13),0,z(.755)),(z(.265),0,z(.68)),'RightShoulder',True)
    add_bone(arm_data,'RightLowerArm',(z(.265),0,z(.68)),(z(.40),0,z(.605)),'RightUpperArm',True)
    add_bone(arm_data,'RightHand',(z(.40),0,z(.605)),(z(.455),0,z(.575)),'RightLowerArm',True)

    add_bone(arm_data,'LeftUpperLeg',(-z(.075),0,z(.44)),(-z(.08),0,z(.285)),'Hips')
    add_bone(arm_data,'LeftLowerLeg',(-z(.08),0,z(.285)),(-z(.08),0,z(.075)),'LeftUpperLeg',True)
    add_bone(arm_data,'LeftFoot',(-z(.08),0,z(.075)),(-z(.08),-z(.07),z(.035)),'LeftLowerLeg',True)
    add_bone(arm_data,'LeftToes',(-z(.08),-z(.07),z(.035)),(-z(.08),-z(.13),z(.025)),'LeftFoot',True)
    add_bone(arm_data,'RightUpperLeg',(z(.075),0,z(.44)),(z(.08),0,z(.285)),'Hips')
    add_bone(arm_data,'RightLowerLeg',(z(.08),0,z(.285)),(z(.08),0,z(.075)),'RightUpperLeg',True)
    add_bone(arm_data,'RightFoot',(z(.08),0,z(.075)),(z(.08),-z(.07),z(.035)),'RightLowerLeg',True)
    add_bone(arm_data,'RightToes', (z(.08),-z(.07),z(.035)),(z(.08),-z(.13),z(.025)),'RightFoot',True)

    bpy.ops.object.mode_set(mode='POSE')
    for pb in arm_obj.pose.bones:
        pb.rotation_mode='XYZ'
    bpy.ops.object.mode_set(mode='OBJECT')
    arm_obj.select_set(False)
    return arm_obj


def choose_bone(name):
    if name.startswith(('Boot_L',)): return 'LeftFoot'
    if name.startswith(('Shin_L','Knee_L','LegSplint_L')): return 'LeftLowerLeg'
    if name.startswith(('Thigh_L',)): return 'LeftUpperLeg'
    if name.startswith(('Boot_R',)): return 'RightFoot'
    if name.startswith(('Shin_R','Knee_R','LegSplint_R')): return 'RightLowerLeg'
    if name.startswith(('Thigh_R',)): return 'RightUpperLeg'
    if name.startswith(('Pelvis','SkirtPlate','Belt','Buckle')): return 'Hips'
    if name.startswith(('Torso','Lamella_B','ChestSide')): return 'Spine'
    if name.startswith(('ChestPlate','Lamella_F','Team_Scarf')): return 'Chest'
    if name.startswith(('Neck',)): return 'Neck'
    if name.startswith(('Head','Helmet','Cheek')): return 'Head'
    if name.startswith(('ShoulderPad_L','Team_ShoulderBand_L')): return 'LeftShoulder'
    if name.startswith(('UpperArm_L',)): return 'LeftUpperArm'
    if name.startswith(('ForeArm_L','ElbowGuard_L','ArmSplint_L')): return 'LeftLowerArm'
    if name.startswith(('Hand_L',)): return 'LeftHand'
    if name.startswith(('ShoulderPad_R','Team_ShoulderBand_R')): return 'RightShoulder'
    if name.startswith(('UpperArm_R',)): return 'RightUpperArm'
    if name.startswith(('ForeArm_R','ElbowGuard_R','ArmSplint_R')): return 'RightLowerArm'
    if name.startswith(('Hand_R',)): return 'RightHand'
    return 'Chest'


def ensure_armature_modifier(obj, arm):
    mod = obj.modifiers.new('Armature','ARMATURE')
    mod.object = arm


def rigid_bind(obj, bone, arm):
    # bake object transform into mesh first; then bind all vertices rigidly to one bone
    bpy.context.view_layer.objects.active=obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    vg=obj.vertex_groups.new(name=bone)
    vg.add(list(range(len(obj.data.vertices))),1.0,'REPLACE')
    ensure_armature_modifier(obj,arm)
    obj.parent=arm
    obj.matrix_parent_inverse=arm.matrix_world.inverted()
    obj.select_set(False)


def team_object(name):
    return name.startswith('Team_') and not name.startswith('Team_ShieldPanel')


def join_objects(objs, name):
    if not objs:
        return None
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0]
    bpy.ops.object.join()
    obj=objs[0]
    obj.name=name
    obj.select_set(False)
    return obj


def triangle_count(obj):
    return sum(max(0, len(poly.vertices) - 2) for poly in obj.data.polygons)


def simplify_lod(meshes, target_triangles=600):
    """Create a real LOD2 from the imported LOD1 while preserving every named rigid part."""
    source_triangles = sum(triangle_count(obj) for obj in meshes)
    if source_triangles <= target_triangles:
        return source_triangles
    ratio = max(0.05, min(1.0, target_triangles / source_triangles))
    for obj in meshes:
        if triangle_count(obj) <= 4:
            continue
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        modifier = obj.modifiers.new('LOD2_Decimate', 'DECIMATE')
        modifier.ratio = ratio
        modifier.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        obj.select_set(False)
    return sum(triangle_count(obj) for obj in meshes)


def bone_parent_object(obj, arm, bone_name):
    bpy.context.view_layer.objects.active=obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    obj.parent=arm
    obj.parent_type='BONE'
    obj.parent_bone=bone_name
    # preserve world transform after parenting
    obj.matrix_parent_inverse=(arm.matrix_world @ arm.pose.bones[bone_name].matrix).inverted()
    obj.select_set(False)


def assign_source_material(obj, base_material, team_material):
    obj.data.materials.clear()
    obj.data.materials.append(team_material if src_name(obj).startswith('Team_') else base_material)


def process_lod(meshes, lod, arm, base_material, team_material):
    shield=[]; sword=[]; base=[]; team=[]
    for o in meshes:
        n=src_name(o)
        assign_source_material(o, base_material, team_material)
        if is_shield(n): shield.append(o); continue
        if is_sword(n): sword.append(o); continue
        rigid_bind(o, choose_bone(n), arm)
        if team_object(n): team.append(o)
        else: base.append(o)
    base_obj=join_objects(base,f'SK_Infantry_A_{lod}_Base')
    team_obj=join_objects(team,f'SK_Infantry_A_{lod}_Team')
    sh=join_objects(shield,f'SM_Infantry_Shield_{lod}')
    sw=join_objects(sword,f'SM_Infantry_Sword_{lod}')
    if sh: bone_parent_object(sh,arm,'LeftHand')
    if sw: bone_parent_object(sw,arm,'RightHand')
    return [x for x in (base_obj,team_obj,sh,sw) if x]


def create_materials(root):
    texdir=os.path.join(root,'Textures')
    base=bpy.data.materials.get('MAT_Infantry_Base') or bpy.data.materials.new('MAT_Infantry_Base')
    base.use_nodes=True
    bsdf=base.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Roughness'].default_value=.65
    p=os.path.join(texdir,'T_Infantry_A_BaseColor_1K.png')
    if os.path.exists(p):
        img=bpy.data.images.load(p,check_existing=True)
        tex=base.node_tree.nodes.new('ShaderNodeTexImage'); tex.image=img; tex.label='BaseColor'
        base.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color'])
    team=bpy.data.materials.get('MAT_Infantry_TeamColor') or bpy.data.materials.new('MAT_Infantry_TeamColor')
    team.use_nodes=True
    tbsdf=team.node_tree.nodes.get('Principled BSDF')
    tbsdf.inputs['Base Color'].default_value=(1,1,1,1)
    tbsdf.inputs['Roughness'].default_value=.75
    maskp=os.path.join(texdir,'T_Infantry_A_TeamColorMask_1K.png')
    if os.path.exists(maskp):
        img=bpy.data.images.load(maskp,check_existing=True)
        tex=team.node_tree.nodes.new('ShaderNodeTexImage'); tex.image=img; tex.label='TeamColorMask'; tex.image.colorspace_settings.name='Non-Color'
    return base,team


def add_anchor(name, loc):
    o=bpy.data.objects.new(name,None)
    bpy.context.collection.objects.link(o)
    o.location=loc
    return o


def add_bone_socket(name, arm, bone_name):
    socket=bpy.data.objects.new(name,None)
    bpy.context.collection.objects.link(socket)
    socket.empty_display_type='PLAIN_AXES'
    socket.empty_display_size=.06
    socket.matrix_world=arm.matrix_world @ arm.pose.bones[bone_name].matrix
    socket.parent=arm
    socket.parent_type='BONE'
    socket.parent_bone=bone_name
    socket.matrix_parent_inverse=(arm.matrix_world @ arm.pose.bones[bone_name].matrix).inverted()
    return socket


def clear_pose(arm):
    for pb in arm.pose.bones:
        pb.location=(0,0,0)
        pb.rotation_euler=(0,0,0)
        pb.scale=(1,1,1)


def key(pb, frame, rot=None, loc=None):
    if rot is not None:
        pb.rotation_euler=rot
        pb.keyframe_insert('rotation_euler',frame=frame)
    if loc is not None:
        pb.location=loc
        pb.keyframe_insert('location',frame=frame)


def new_action(arm,name,start,end):
    act=bpy.data.actions.get(name) or bpy.data.actions.new(name)
    act.frame_range=(start,end)
    if not arm.animation_data: arm.animation_data_create()
    arm.animation_data.action=act
    clear_pose(arm)
    return act


def combat_pose(arm, frame):
    p=arm.pose.bones
    key(p['LeftUpperArm'],frame,(0.05,0.0,math.radians(-52)))
    key(p['LeftLowerArm'],frame,(math.radians(-18),0.0,math.radians(10)))
    key(p['RightUpperArm'],frame,(math.radians(-8),0.0,math.radians(48)))
    key(p['RightLowerArm'],frame,(math.radians(-18),0.0,math.radians(-8)))
    key(p['RightHand'],frame,(0,math.radians(-8),0))


def locomotion_pose(arm, frame, phase):
    """Author one of four grounded locomotion poses for the rigid low-poly character."""
    p=arm.pose.bones
    contact=math.radians(34)
    passing_knee=math.radians(42)
    trailing_knee=math.radians(24)
    foot_contact=math.radians(14)

    if phase == 0:  # left heel strike / right toe-off
        left_thigh,right_thigh=contact,-contact
        left_knee,right_knee=math.radians(5),trailing_knee
        left_foot,right_foot=-foot_contact,foot_contact
        twist=math.radians(4)
    elif phase == 1:  # right leg passes under the body
        left_thigh,right_thigh=math.radians(-8),math.radians(10)
        left_knee,right_knee=math.radians(14),passing_knee
        left_foot,right_foot=math.radians(6),math.radians(-8)
        twist=0
    elif phase == 2:  # right heel strike / left toe-off
        left_thigh,right_thigh=-contact,contact
        left_knee,right_knee=trailing_knee,math.radians(5)
        left_foot,right_foot=foot_contact,-foot_contact
        twist=math.radians(-4)
    else:  # left leg passes under the body
        left_thigh,right_thigh=math.radians(10),math.radians(-8)
        left_knee,right_knee=passing_knee,math.radians(14)
        left_foot,right_foot=math.radians(-8),math.radians(6)
        twist=0

    key(p['LeftUpperLeg'],frame,(left_thigh,0,0))
    key(p['RightUpperLeg'],frame,(right_thigh,0,0))
    key(p['LeftLowerLeg'],frame,(left_knee,0,0))
    key(p['RightLowerLeg'],frame,(right_knee,0,0))
    key(p['LeftFoot'],frame,(left_foot,0,0))
    key(p['RightFoot'],frame,(right_foot,0,0))
    key(p['Hips'],frame,(0,twist,0))
    key(p['Chest'],frame,(math.radians(5),-twist,0))

    # Keep shield and sword controlled while adding visible counter-motion.
    arm_swing=math.radians(7 if phase in (0,1) else -7)
    key(p['LeftUpperArm'],frame,(0.05+arm_swing,0,math.radians(-48)))
    key(p['LeftLowerArm'],frame,(math.radians(-24),0,math.radians(12)))
    key(p['RightUpperArm'],frame,(math.radians(-12)-arm_swing,0,math.radians(42)))
    key(p['RightLowerArm'],frame,(math.radians(-22),0,math.radians(-10)))
    key(p['RightHand'],frame,(0,math.radians(-8),0))


def build_actions(arm):
    acts=[]
    # Idle 0..89
    a=new_action(arm,'AN_Infantry_Idle',0,89); combat_pose(arm,0); combat_pose(arm,44); combat_pose(arm,89)
    key(arm.pose.bones['Chest'],0,(0,0,0)); key(arm.pose.bones['Chest'],44,(math.radians(1.5),0,0)); key(arm.pose.bones['Chest'],89,(0,0,0)); acts.append(a)
    # Move 0..24 - one grounded stride with heel strikes and passing poses; Root stays unkeyed.
    a=new_action(arm,'AN_Infantry_Move',0,24)
    locomotion_pose(arm,0,0)
    locomotion_pose(arm,6,1)
    locomotion_pose(arm,12,2)
    locomotion_pose(arm,18,3)
    locomotion_pose(arm,24,0)
    acts.append(a)
    # Attack_A 0..26, right-high to left-low slash
    a=new_action(arm,'AN_Infantry_Attack_A',0,26); combat_pose(arm,0)
    p=arm.pose.bones
    key(p['RightUpperArm'],8,(math.radians(-35),math.radians(-10),math.radians(80)))
    key(p['RightLowerArm'],8,(math.radians(-55),0,math.radians(-25)))
    key(p['Chest'],8,(0,0,math.radians(-8)))
    key(p['RightUpperArm'],13,(math.radians(35),math.radians(5),math.radians(20)))
    key(p['RightLowerArm'],13,(math.radians(-20),0,math.radians(30)))
    key(p['Chest'],13,(0,0,math.radians(12)))
    combat_pose(arm,26); key(p['Chest'],26,(0,0,0)); acts.append(a)
    # Hit 0..9
    a=new_action(arm,'AN_Infantry_Hit',0,9); combat_pose(arm,0); combat_pose(arm,9)
    key(arm.pose.bones['Chest'],4,(math.radians(12),0,math.radians(-8))); key(arm.pose.bones['Neck'],4,(math.radians(-8),0,0)); acts.append(a)
    # Death 0..38. Root never keyed/transformed.
    a=new_action(arm,'AN_Infantry_Death',0,38); combat_pose(arm,0)
    key(arm.pose.bones['Hips'],0,(0,0,0),(0,0,0)); key(arm.pose.bones['Hips'],22,(math.radians(45),0,math.radians(8)),(0,0,-0.25)); key(arm.pose.bones['Hips'],35,(math.radians(82),0,math.radians(10)),(0,0,-0.55)); key(arm.pose.bones['Hips'],38,(math.radians(82),0,math.radians(10)),(0,0,-0.55))
    key(arm.pose.bones['LeftUpperLeg'],35,(math.radians(-35),0,math.radians(8))); key(arm.pose.bones['RightUpperLeg'],35,(math.radians(-15),0,math.radians(-10))); acts.append(a)
    # Blender 5 uses layered Action channel bags and no longer exposes Action.fcurves.
    # Newly inserted keys already use BEZIER interpolation, so no compatibility rewrite is needed.
    return acts


def select_for_export(arm, objects):
    bpy.ops.object.select_all(action='DESELECT')
    arm.select_set(True)
    for o in objects:
        if o and o.name in bpy.data.objects: o.select_set(True)
    bpy.context.view_layer.objects.active=arm


def export_fbx(path, arm, objects, all_actions=False):
    select_for_export(arm,objects)
    bpy.ops.export_scene.fbx(
        filepath=path,
        use_selection=True,
        object_types={'ARMATURE','MESH','EMPTY'},
        apply_unit_scale=True,
        apply_scale_options='FBX_SCALE_UNITS',
        axis_forward='-Z',
        axis_up='Y',
        add_leaf_bones=False,
        use_armature_deform_only=False,
        bake_anim=True,
        bake_anim_use_all_bones=True,
        bake_anim_use_nla_strips=False,
        bake_anim_use_all_actions=all_actions,
        bake_anim_force_startend_keying=True,
        bake_anim_step=1.0,
        bake_anim_simplify_factor=0.0,
        path_mode='STRIP',
        embed_textures=False,
    )


def clean_generated_sidecars(root):
    """Remove Blender backups and copied-texture folders; every target is rebuildable."""
    root=os.path.abspath(root)
    for parent in (os.path.join(root,'Models'),os.path.join(root,'Animations')):
        if not os.path.isdir(parent):
            continue
        for name in os.listdir(parent):
            candidate=os.path.abspath(os.path.join(parent,name))
            if name.endswith('.fbm') and os.path.commonpath((root,candidate)) == root:
                shutil.rmtree(candidate)
    backup=os.path.abspath(os.path.join(root,'Source','CHR_Infantry_A_v002.blend1'))
    if os.path.commonpath((root,backup)) == root and os.path.isfile(backup):
        os.remove(backup)


def write_manifest(root):
    """Write reproducible hashes for every delivered file except the manifest itself."""
    manifest_path=os.path.join(root,'Documentation','MANIFEST.json')
    files=[]
    for directory,dirnames,filenames in os.walk(root):
        dirnames[:]=[name for name in dirnames if name != '__pycache__']
        for filename in filenames:
            path=os.path.join(directory,filename)
            relative=os.path.relpath(path,root).replace('\\','/')
            if relative == 'Documentation/MANIFEST.json' or relative.endswith('.pyc'):
                continue
            digest=hashlib.sha256()
            with open(path,'rb') as source:
                for chunk in iter(lambda: source.read(1024*1024),b''):
                    digest.update(chunk)
            files.append({
                'path':relative,
                'bytes':os.path.getsize(path),
                'sha256':digest.hexdigest(),
            })
    manifest={
        'package':'Unit_03_Infantry_L3_v002_CORRECTED',
        'generated_by':'Source/build_unit03_l3_blender.py',
        'files':sorted(files,key=lambda value:value['path'].lower()),
    }
    with open(manifest_path,'w',encoding='utf-8') as output:
        json.dump(manifest,output,ensure_ascii=False,indent=2)


def main():
    root=parse_args()
    input_dir=os.path.join(root,'Input_v001')
    models=os.path.join(root,'Models'); anims=os.path.join(root,'Animations'); srcdir=os.path.join(root,'Source')
    os.makedirs(models,exist_ok=True); os.makedirs(anims,exist_ok=True); os.makedirs(srcdir,exist_ok=True)
    clear_scene()
    bpy.context.scene.render.fps=FPS
    bpy.context.scene.unit_settings.system='METRIC'
    bpy.context.scene.unit_settings.scale_length=1.0

    lod0=import_glb(os.path.join(input_dir,'CHR_Infantry_A_v001_LOD0_Blue.glb'),'LOD0')
    lod1=import_glb(os.path.join(input_dir,'CHR_Infantry_A_v001_LOD1_Blue.glb'),'LOD1')
    lod2=import_glb(os.path.join(input_dir,'CHR_Infantry_A_v001_LOD1_Blue.glb'),'LOD2')
    # Bring both source LODs into a consistent A-Pose before rigging.
    raise_arms_to_apose(lod0); raise_arms_to_apose(lod1); raise_arms_to_apose(lod2)
    lod2_triangles=simplify_lod(lod2,600)
    bb0,bb1=world_bbox(lod0)
    height=bb1.z-bb0.z
    if not (1.6 <= height <= 2.0): height=1.80
    arm=create_humanoid_armature(height)
    base_material,team_material=create_materials(root)
    objs0=process_lod(lod0,'LOD0',arm,base_material,team_material)
    objs1=process_lod(lod1,'LOD1',arm,base_material,team_material)
    objs2=process_lod(lod2,'LOD2',arm,base_material,team_material)

    # Stable non-animated anchors.
    anchors=[
        add_anchor('SelectionAnchor',(0,0,0.02)),
        add_anchor('HealthBarAnchor',(0,0,2.10)),
        add_anchor('GroundContact',(0,0,0)),
    ]
    sockets=[
        add_bone_socket('Socket_R_Hand',arm,'RightHand'),
        add_bone_socket('Socket_L_Hand',arm,'LeftHand'),
        add_bone_socket('Socket_WeaponTip',arm,'RightHand'),
        add_bone_socket('Socket_Head',arm,'Head'),
        add_bone_socket('FX_Hit_Center',arm,'Chest'),
        add_bone_socket('FX_Foot_L',arm,'LeftFoot'),
        add_bone_socket('FX_Foot_R',arm,'RightFoot'),
    ]

    actions=build_actions(arm)
    # Save editable source with A-Pose as the rest state. Actions are stored in the file.
    arm.animation_data.action=None
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(srcdir,'CHR_Infantry_A_v002.blend'))

    all_mesh=[o for o in objs0+objs1+objs2 if o]
    master_objects=all_mesh+anchors+sockets
    # master FBX with all actions
    export_fbx(os.path.join(models,'SK_Infantry_A_v002.fbx'),arm,master_objects,all_actions=True)

    # separate action FBXs, identical skeleton
    for act in actions:
        arm.animation_data.action=act
        bpy.context.scene.frame_start=int(act.frame_range[0])
        bpy.context.scene.frame_end=int(act.frame_range[1])
        export_fbx(os.path.join(anims,act.name+'.fbx'),arm,all_mesh,all_actions=False)
    arm.animation_data.action=None

    # Record deterministic build metadata
    meta={
        'tool':'Blender',
        'blender_version':bpy.app.version_string,
        'fps':FPS,
        'source_lod0':'CHR_Infantry_A_v001_LOD0_Blue.glb',
        'source_lod1':'CHR_Infantry_A_v001_LOD1_Blue.glb',
        'lod_triangles':{
            'LOD0':sum(triangle_count(obj) for obj in objs0),
            'LOD1':sum(triangle_count(obj) for obj in objs1),
            'LOD2':sum(triangle_count(obj) for obj in objs2),
        },
        'lod2_pre_join_triangles':lod2_triangles,
        'rest_pose':'A-Pose',
        'root_motion':'Off / Root bone never keyed',
        'locomotion':{
            'clip':'AN_Infantry_Move',
            'frame_range':[0,24],
            'duration_seconds':24/FPS,
            'poses':['left_contact','right_passing','right_contact','left_passing','left_contact'],
            'footstep_frames':[1,13],
            'reference_world_speed_mps':4.5,
            'reference_animator_rate':1.8,
        },
        'attack_impact':{'frame':13,'time_seconds':13/FPS},
        'seed_job_id':'N/A - deterministic procedural rig/animation build',
        'prompt':PROMPT
    }
    with open(os.path.join(srcdir,'BUILD_RESULT.json'),'w',encoding='utf-8') as f: json.dump(meta,f,ensure_ascii=False,indent=2)
    clean_generated_sidecars(root)
    write_manifest(root)
    print(json.dumps(meta,ensure_ascii=False,indent=2))

if __name__=='__main__':
    main()
