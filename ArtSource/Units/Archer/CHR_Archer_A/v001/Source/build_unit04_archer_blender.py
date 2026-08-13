import bpy
import hashlib
import importlib.util
import json
import math
import os
import sys


FPS = 30
PROMPT = """Create a game-ready stylized low-poly 3D archer for a Unity 6 URP top-down RTS. Preserve the repository's accepted prototype body proportions and Humanoid contract, remove the infantry shield and sword, use light armor, add a clearly curved 1.15-1.35 m bow, a readable back quiver, and a separate 0.82 m Z-forward arrow. Deliver Idle, Move, Attack_Ranged, Hit, and Death as in-place clips with Root Motion off. Attack_Ranged must expose ProjectileRelease; projectile flight is a separate gameplay visual. Use one shared model with runtime team color, no baked blue/red duplicates, no existing IP, logos, or watermarks."""


def parse_root():
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    for index, value in enumerate(args[:-1]):
        if value == '--package-root':
            return os.path.abspath(args[index + 1])
    return os.path.abspath(os.path.join(os.path.dirname(__file__), '..'))


def load_infantry_tools():
    source = os.path.abspath(os.path.join(
        os.path.dirname(__file__), '..', '..', '..', '..', 'Infantry',
        'CHR_Infantry_A', 'v002', 'Source', 'build_unit03_l3_blender.py'))
    spec = importlib.util.spec_from_file_location('aegis_infantry_build', source)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


BASE = load_infantry_tools()


def source_input_dir(root):
    units = os.path.abspath(os.path.join(root, '..', '..', '..'))
    return os.path.join(units, 'Infantry', 'CHR_Infantry_A', 'v002', 'Input_v001')


def remove_infantry_equipment(meshes):
    heavy_prefixes = (
        'Shield_', 'Team_ShieldPanel', 'Sword_', 'ShoulderPad_',
        'ElbowGuard_', 'ArmSplint_', 'LegSplint_', 'ChestPlate',
    )
    result = []
    for obj in meshes:
        if BASE.src_name(obj).startswith(heavy_prefixes):
            bpy.data.objects.remove(obj, do_unlink=True)
        else:
            result.append(obj)
    return result


def curve_mesh(name, points, bevel, material):
    data = bpy.data.curves.new(name + '_Curve', 'CURVE')
    data.dimensions = '3D'
    data.resolution_u = 1
    data.bevel_depth = bevel
    data.bevel_resolution = 0
    data.resolution_u = 1
    spline = data.splines.new('POLY')
    spline.points.add(len(points) - 1)
    for target, point in zip(spline.points, points):
        target.co = (*point, 1.0)
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.convert(target='MESH')
    obj.data.materials.append(material)
    obj.select_set(False)
    return obj


def cylinder(name, radius, depth, location, material, vertices=8, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=location,
                                       rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(material)
    obj.select_set(False)
    return obj


def cube(name, scale, location, material, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    obj.select_set(False)
    return obj


def create_archer_equipment(lod, arm, base_material, team_material, detail):
    bow_x = -0.73
    bow_y = -0.12
    bow_points = [
        (bow_x + 0.16, bow_y, 0.33),
        (bow_x + 0.03, bow_y, 0.56),
        (bow_x - 0.02, bow_y, 0.90),
        (bow_x + 0.03, bow_y, 1.24),
        (bow_x + 0.16, bow_y, 1.55),
    ]
    bow = curve_mesh(f'SM_Archer_Bow_{lod}', bow_points, 0.026 if detail else 0.034, base_material)
    string = curve_mesh(f'SM_Archer_BowString_{lod}',
                        [bow_points[0], (bow_x - 0.03, bow_y, 0.94), bow_points[-1]],
                        0.010 if detail else 0.014, team_material)
    BASE.bone_parent_object(bow, arm, 'LeftHand')
    BASE.bone_parent_object(string, arm, 'LeftHand')

    quiver = cylinder(f'SM_Archer_Quiver_{lod}', 0.105, 0.62, (0.26, 0.19, 1.04),
                      team_material, 8 if detail else 6, (math.radians(12), 0, 0))
    BASE.bone_parent_object(quiver, arm, 'Chest')
    equipment = [bow, string, quiver]
    if detail:
        for index, x in enumerate((0.21, 0.26, 0.31)):
            arrow = cylinder(f'SM_Archer_QuiverArrow{index}_{lod}', 0.012, 0.52,
                             (x, 0.19, 1.42), base_material, 6, (math.radians(12), 0, 0))
            BASE.bone_parent_object(arrow, arm, 'Chest')
            equipment.append(arrow)
    return equipment


def create_projectile_arrow(base_material, team_material):
    shaft = cylinder('PRJ_Arrow_Basic_v001_Shaft', 0.018, 0.72, (0, 0, 0), base_material, 8,
                     (math.radians(90), 0, 0))
    bpy.ops.mesh.primitive_cone_add(vertices=8, radius1=0.055, radius2=0, depth=0.12,
                                   location=(0, -0.42, 0), rotation=(math.radians(-90), 0, 0))
    head = bpy.context.object
    head.name = 'PRJ_Arrow_Basic_v001_Head'
    head.data.materials.append(base_material)
    fletching_a = cube('PRJ_Arrow_Basic_v001_Fletching_A', (0.055, 0.09, 0.008),
                       (0, 0.31, 0), team_material)
    fletching_b = cube('PRJ_Arrow_Basic_v001_Fletching_B', (0.008, 0.09, 0.055),
                       (0, 0.31, 0), team_material)
    return [shaft, head, fletching_a, fletching_b]


def archer_guard_pose(arm, frame):
    pose = arm.pose.bones
    BASE.key(pose['LeftUpperArm'], frame, (math.radians(-5), 0, math.radians(-28)))
    BASE.key(pose['LeftLowerArm'], frame, (math.radians(-18), 0, math.radians(18)))
    BASE.key(pose['RightUpperArm'], frame, (math.radians(-12), 0, math.radians(32)))
    BASE.key(pose['RightLowerArm'], frame, (math.radians(-28), 0, math.radians(-15)))
    BASE.key(pose['RightHand'], frame, (0, math.radians(-6), 0))


def standing_body_pose(arm, frame):
    """Key an explicit grounded Humanoid baseline so Unity never retargets omitted muscles to a folded pose."""
    pose = arm.pose.bones
    for name in ('Hips', 'Spine', 'Chest', 'Neck', 'Head',
                 'LeftUpperLeg', 'LeftLowerLeg', 'LeftFoot', 'LeftToes',
                 'RightUpperLeg', 'RightLowerLeg', 'RightFoot', 'RightToes'):
        BASE.key(pose[name], frame, (0, 0, 0))


def archer_move_pose(arm, frame, phase):
    BASE.locomotion_pose(arm, frame, phase)
    archer_guard_pose(arm, frame)


def build_actions(arm):
    actions = []
    idle = BASE.new_action(arm, 'AN_Archer_Idle', 0, 89)
    for frame in (0, 44, 89):
        standing_body_pose(arm, frame)
        archer_guard_pose(arm, frame)
    BASE.key(arm.pose.bones['Chest'], 0, (0, 0, 0))
    BASE.key(arm.pose.bones['Chest'], 44, (math.radians(1.5), 0, 0))
    BASE.key(arm.pose.bones['Chest'], 89, (0, 0, 0))
    actions.append(idle)

    move = BASE.new_action(arm, 'AN_Archer_Move', 0, 24)
    for frame, phase in ((0, 0), (6, 1), (12, 2), (18, 3), (24, 0)):
        archer_move_pose(arm, frame, phase)
    actions.append(move)

    attack = BASE.new_action(arm, 'AN_Archer_Attack_Ranged', 0, 36)
    for frame in (0, 10, 18, 21, 22, 36):
        standing_body_pose(arm, frame)
    archer_guard_pose(arm, 0)
    pose = arm.pose.bones
    BASE.key(pose['LeftUpperArm'], 10, (math.radians(-12), math.radians(-4), math.radians(-72)))
    BASE.key(pose['LeftLowerArm'], 10, (math.radians(-8), 0, math.radians(8)))
    BASE.key(pose['RightUpperArm'], 10, (math.radians(-25), math.radians(8), math.radians(70)))
    BASE.key(pose['RightLowerArm'], 10, (math.radians(-62), 0, math.radians(-20)))
    BASE.key(pose['Chest'], 10, (0, math.radians(-4), 0))
    for frame in (18, 21):
        BASE.key(pose['LeftUpperArm'], frame, (math.radians(-12), math.radians(-4), math.radians(-78)))
        BASE.key(pose['LeftLowerArm'], frame, (math.radians(-6), 0, math.radians(5)))
        BASE.key(pose['RightUpperArm'], frame, (math.radians(-30), math.radians(10), math.radians(88)))
        BASE.key(pose['RightLowerArm'], frame, (math.radians(-82), 0, math.radians(-28)))
        BASE.key(pose['Chest'], frame, (0, math.radians(-7), 0))
    BASE.key(pose['RightUpperArm'], 22, (math.radians(-18), 0, math.radians(42)))
    BASE.key(pose['RightLowerArm'], 22, (math.radians(-28), 0, math.radians(-12)))
    archer_guard_pose(arm, 36)
    BASE.key(pose['Chest'], 36, (0, 0, 0))
    actions.append(attack)

    hit = BASE.new_action(arm, 'AN_Archer_Hit', 0, 9)
    for frame in (0, 4, 9):
        standing_body_pose(arm, frame)
    archer_guard_pose(arm, 0)
    archer_guard_pose(arm, 9)
    BASE.key(arm.pose.bones['Chest'], 4, (math.radians(12), 0, math.radians(-7)))
    actions.append(hit)

    death = BASE.new_action(arm, 'AN_Archer_Death', 0, 38)
    standing_body_pose(arm, 0)
    archer_guard_pose(arm, 0)
    BASE.key(pose['Hips'], 0, (0, 0, 0), (0, 0, 0))
    BASE.key(pose['Hips'], 22, (math.radians(44), 0, math.radians(-8)), (0, 0, -0.24))
    BASE.key(pose['Hips'], 35, (math.radians(82), 0, math.radians(-10)), (0, 0, -0.54))
    BASE.key(pose['Hips'], 38, (math.radians(82), 0, math.radians(-10)), (0, 0, -0.54))
    BASE.key(pose['LeftUpperLeg'], 35, (math.radians(-30), 0, math.radians(7)))
    BASE.key(pose['RightUpperLeg'], 35, (math.radians(-18), 0, math.radians(-8)))
    actions.append(death)
    return actions


def export_static(path, objects):
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={'MESH'},
                             apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
                             axis_forward='-Z', axis_up='Y', add_leaf_bones=False,
                             bake_anim=False, path_mode='STRIP', embed_textures=False)


def write_manifest(root):
    destination = os.path.join(root, 'Documentation', 'MANIFEST.json')
    files = []
    for directory, dirnames, filenames in os.walk(root):
        dirnames[:] = [value for value in dirnames if value != '__pycache__']
        for filename in filenames:
            path = os.path.join(directory, filename)
            relative = os.path.relpath(path, root).replace('\\', '/')
            if relative == 'Documentation/MANIFEST.json' or relative.endswith('.pyc'):
                continue
            digest = hashlib.sha256()
            with open(path, 'rb') as source:
                for chunk in iter(lambda: source.read(1024 * 1024), b''):
                    digest.update(chunk)
            files.append({'path': relative, 'bytes': os.path.getsize(path), 'sha256': digest.hexdigest()})
    with open(destination, 'w', encoding='utf-8') as output:
        json.dump({'package': 'Unit_04_Archer_Prototype_L3_v001',
                   'generated_by': 'Source/build_unit04_archer_blender.py',
                   'files': sorted(files, key=lambda item: item['path'].lower())},
                  output, ensure_ascii=False, indent=2)


def main():
    root = parse_root()
    models = os.path.join(root, 'Models')
    animations = os.path.join(root, 'Animations')
    source = os.path.join(root, 'Source')
    documentation = os.path.join(root, 'Documentation')
    for directory in (models, animations, source, documentation):
        os.makedirs(directory, exist_ok=True)
    BASE.clear_scene()
    bpy.context.scene.render.fps = FPS
    bpy.context.scene.unit_settings.system = 'METRIC'
    bpy.context.scene.unit_settings.scale_length = 1.0

    inputs = source_input_dir(root)
    lod0 = remove_infantry_equipment(BASE.import_glb(os.path.join(inputs, 'CHR_Infantry_A_v001_LOD0_Blue.glb'), 'ArcherLOD0'))
    lod1 = remove_infantry_equipment(BASE.import_glb(os.path.join(inputs, 'CHR_Infantry_A_v001_LOD1_Blue.glb'), 'ArcherLOD1'))
    lod2 = remove_infantry_equipment(BASE.import_glb(os.path.join(inputs, 'CHR_Infantry_A_v001_LOD1_Blue.glb'), 'ArcherLOD2'))
    BASE.raise_arms_to_apose(lod0)
    BASE.raise_arms_to_apose(lod1)
    BASE.raise_arms_to_apose(lod2)
    BASE.simplify_lod(lod2, 520)
    arm = BASE.create_humanoid_armature(1.78)
    base_material, team_material = BASE.create_materials(
        os.path.abspath(os.path.join(inputs, '..')))
    base_material.name = 'MAT_Archer_Base'
    team_material.name = 'MAT_Archer_TeamColor'

    built = []
    triangle_report = {}
    for lod_name, meshes, detail in (('LOD0', lod0, True), ('LOD1', lod1, True), ('LOD2', lod2, False)):
        parts = BASE.process_lod(meshes, lod_name, arm, base_material, team_material)
        for part in parts:
            part.name = part.name.replace('Infantry', 'Archer')
        equipment = create_archer_equipment(lod_name, arm, base_material, team_material, detail)
        built.extend(parts + equipment)
        triangle_report[lod_name] = sum(BASE.triangle_count(obj) for obj in parts + equipment)

    anchors = [
        BASE.add_anchor('SelectionAnchor', (0, 0, 0.02)),
        BASE.add_anchor('HealthBarAnchor', (0, 0, 2.10)),
        BASE.add_anchor('GroundContact', (0, 0, 0)),
    ]
    sockets = [
        BASE.add_bone_socket('Socket_Projectile', arm, 'RightHand'),
        BASE.add_bone_socket('Socket_R_Hand', arm, 'RightHand'),
        BASE.add_bone_socket('Socket_L_Hand', arm, 'LeftHand'),
        BASE.add_bone_socket('Socket_Head', arm, 'Head'),
        BASE.add_bone_socket('FX_Hit_Center', arm, 'Chest'),
        BASE.add_bone_socket('FX_Foot_L', arm, 'LeftFoot'),
        BASE.add_bone_socket('FX_Foot_R', arm, 'RightFoot'),
    ]
    arrow_objects = create_projectile_arrow(base_material, team_material)
    actions = build_actions(arm)
    arm.animation_data.action = None
    BASE.clear_pose(arm)
    bpy.context.scene.frame_set(0)
    bpy.context.view_layer.update()
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(source, 'CHR_Archer_A_v001.blend'))
    BASE.export_fbx(os.path.join(models, 'SK_Archer_A_v001.fbx'), arm, built + anchors + sockets, all_actions=True)
    for action in actions:
        arm.animation_data.action = action
        bpy.context.scene.frame_start = int(action.frame_range[0])
        bpy.context.scene.frame_end = int(action.frame_range[1])
        BASE.export_fbx(os.path.join(animations, action.name + '.fbx'), arm, built, all_actions=False)
    arm.animation_data.action = None

    export_static(os.path.join(models, 'PRJ_Arrow_Basic_v001.fbx'), arrow_objects)
    metadata = {
        'tool': 'Blender', 'blender_version': bpy.app.version_string, 'fps': FPS,
        'source_derivative': 'CHR_Infantry_A_v001 body geometry via accepted Infantry v002 pipeline',
        'release_status': 'Prototype only - source rights and final animation quality unverified',
        'lod_triangles': triangle_report, 'rest_pose': 'A-Pose',
        'clips': ['Idle', 'Move', 'Attack_Ranged', 'Hit', 'Death'],
        'projectile_release': {'frame': 22, 'time_seconds': 22 / FPS},
        'projectile': {'length_m': 0.82, 'forward': 'Unity local Z+', 'pivot': 'center'},
        'root_motion': 'Off / Root bone never keyed', 'prompt': PROMPT,
    }
    with open(os.path.join(source, 'BUILD_RESULT.json'), 'w', encoding='utf-8') as output:
        json.dump(metadata, output, ensure_ascii=False, indent=2)
    BASE.clean_generated_sidecars(root)
    backup = os.path.join(source, 'CHR_Archer_A_v001.blend1')
    if os.path.isfile(backup):
        os.remove(backup)
    write_manifest(root)
    print(json.dumps(metadata, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()
