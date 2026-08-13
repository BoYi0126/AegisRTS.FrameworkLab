"""Render Phase 03 review evidence without saving the opened v004 source."""
import argparse,csv,hashlib,json,math,struct
from pathlib import Path
import bmesh,bpy
from mathutils import Vector

STATUS="READY FOR PHASE03 REVIEW"

def args():
    p=argparse.ArgumentParser();p.add_argument("--output-root",required=True);p.add_argument("--baseline-blend",required=True)
    v=__import__('sys').argv;return p.parse_args(v[v.index('--')+1:] if '--' in v else [])
def sha(path):
    h=hashlib.sha256()
    with open(path,'rb') as f:
        for c in iter(lambda:f.read(1048576),b''):h.update(c)
    return h.hexdigest().upper()
def png_size(path):
    with open(path,'rb') as f:b=f.read(24)
    return struct.unpack('>II',b[16:24])
def tri(o):o.data.calc_loop_triangles();return len(o.data.loop_triangles)
def bounds(obs):
    p=[o.matrix_world@Vector(c) for o in obs for c in o.bound_box]
    return Vector((min(v.x for v in p),min(v.y for v in p),min(v.z for v in p))),Vector((max(v.x for v in p),max(v.y for v in p),max(v.z for v in p)))
def topo(o):
    bm=bmesh.new();bm.from_mesh(o.data);r=(sum(not e.is_manifold for e in bm.edges),sum(e.is_boundary for e in bm.edges),sum(not e.link_faces for e in bm.edges),sum(f.calc_area()<=1e-12 for f in bm.faces));bm.free();return r
def look(o,target):o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()
def mat(name,color,rough=.75,metal=0):
    m=bpy.data.materials.get(name) or bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
    n=m.node_tree.nodes.get('Principled BSDF');n.inputs['Base Color'].default_value=(*color,1);n.inputs['Roughness'].default_value=rough;n.inputs['Metallic'].default_value=metal;return m
def override(obs,m):
    old={}
    for o in obs:old[o.name]=[s.material for s in o.material_slots];o.data.materials.clear();o.data.materials.append(m)
    return old
def restore(obs,old):
    for o in obs:
        o.data.materials.clear()
        for m in old.get(o.name,[]):
            if m:o.data.materials.append(m)
def area(name,loc,energy,size,color,target):
    d=bpy.data.lights.new(name+'Data','AREA');d.energy=energy;d.shape='DISK';d.size=size;d.color=color;o=bpy.data.objects.new(name,d);bpy.context.scene.collection.objects.link(o);o.location=loc;look(o,target)
def setup(low,high):
    s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.image_settings.file_format='PNG';s.render.resolution_percentage=100;s.render.film_transparent=False
    s.world.use_nodes=True;bg=s.world.node_tree.nodes['Background'];bg.inputs['Color'].default_value=(.025,.03,.04,1);bg.inputs['Strength'].default_value=.6
    c=(low+high)*.5;span=max((high-low).x,(high-low).z);d=bpy.data.cameras.new('P03CameraData');cam=bpy.data.objects.new('P03Camera',d);bpy.context.scene.collection.objects.link(cam);d.type='ORTHO';d.ortho_scale=2.22;s.camera=cam
    area('P03Key',c+Vector((-2.8,-3.2,3.4))*span,980,span*2.2,(1,.9,.78),c);area('P03Fill',c+Vector((2.3,-1.1,2.1))*span,500,span*1.8,(.7,.82,1),c);area('P03Rim',c+Vector((.6,3,2.8))*span,650,span*1.6,(.76,.86,1),c)
    bpy.ops.mesh.primitive_plane_add(size=6,location=(0,0,low.z-.004));g=bpy.context.object;g.name='P03Ground';g.data.materials.append(mat('MAT_P03_Ground',(.10,.115,.14),.9));return s,cam,g
def render(s,cam,center,direction,path,scale=2.22,res=(768,768)):
    d=Vector(direction).normalized();cam.location=Vector(center)+d*7;cam.data.ortho_scale=scale;look(cam,center);s.render.resolution_x,s.render.resolution_y=res;s.render.filepath=str(path);path.parent.mkdir(parents=True,exist_ok=True);bpy.ops.render.render(write_still=True)
def wire_dupes(obs,m):
    out=[]
    for src in obs:
        o=src.copy();o.data=src.data.copy();bpy.context.scene.collection.objects.link(o);o.name='WIRE_'+src.name;o.data.materials.clear();o.data.materials.append(m);mod=o.modifiers.new('ReviewWire','WIREFRAME');mod.thickness=.0015;mod.use_replace=True;out.append(o)
    return out
def compare_baseline(s,cam,center,current,baseline_path,out,clay):
    with bpy.data.libraries.load(str(baseline_path),link=False) as (a,b):b.objects=[n for n in a.objects]
    loaded=[o for o in b.objects if o]
    for o in loaded:
        if not o.users_collection:bpy.context.scene.collection.objects.link(o)
    base=[o for o in loaded if o.type=='MESH']
    for o in base:o.data.materials.clear();o.data.materials.append(clay)
    cur_old=override(current,clay);cur_world={o:o.matrix_world.copy() for o in current};base_world={o:o.matrix_world.copy() for o in base}
    for o in current:m=o.matrix_world.copy();m.translation.x+=1.08;o.matrix_world=m
    for o in base:m=o.matrix_world.copy();m.translation.x-=1.08;o.matrix_world=m
    bpy.context.view_layer.update();cc=Vector((0,center.y,center.z))
    render(s,cam,cc,(0,-1,.06),out/'P02R1_vs_v004_Front.png',2.25,(1536,768));render(s,cam,cc,(-1,-1,.14),out/'P02R1_vs_v004_3Q.png',2.35,(1536,768))
    for o,m in cur_world.items():o.matrix_world=m
    for o,m in base_world.items():o.matrix_world=m
    restore(current,cur_old)
    for o in loaded:bpy.data.objects.remove(o,do_unlink=True)
    bpy.context.view_layer.update()
def main():
    a=args();root=Path(a.output_root).resolve();dirs={n:root/'Screenshots'/n for n in ('Clay','MaterialID','Silhouette','Wireframe','Detail','Comparison','ScreenSize','Unity')};manifest=root/'Manifests';blenddir=root/'Blender'
    for p in list(dirs.values())+[manifest,blenddir]:p.mkdir(parents=True,exist_ok=True)
    s=bpy.context.scene
    if s.get('SourceVersion')!='CHR_Infantry_A_v004' or s.get('ReviewStatus')!=STATUS:raise RuntimeError('Not the v004 Phase03 review candidate')
    meshes=[o for o in s.objects if o.type=='MESH'];arms=[o for o in s.objects if o.type=='ARMATURE']
    if len(arms)!=1 or len(arms[0].data.bones)!=23:raise RuntimeError('Rig contract mismatch')
    low,high=bounds(meshes);dim=high-low;center=(low+high)*.5;rows=[];tot=[0,0,0,0]
    for o in sorted(meshes,key=lambda x:x.name):
        t=topo(o);tot=[tot[i]+t[i] for i in range(4)];rows.append({'ObjectName':o.name,'Collection':';'.join(c.name for c in o.users_collection),'Vertices':len(o.data.vertices),'Triangles':tri(o),'Materials':';'.join(x.material.name for x in o.material_slots if x.material),'Parent':o.parent.name if o.parent else '','AttachmentBone':o.get('AttachmentBone',''),'BindingStatus':o.get('BindingStatus',''),'NonManifoldEdges':t[0],'BoundaryEdges':t[1],'LooseEdges':t[2],'ZeroAreaFaces':t[3]})
    with (manifest/'Object_Manifest.csv').open('w',newline='',encoding='utf-8-sig') as f:w=csv.DictWriter(f,fieldnames=rows[0].keys());w.writeheader();w.writerows(rows)
    bones=[{'Bone':b.name,'Parent':b.parent.name if b.parent else '','Deform':b.use_deform} for b in arms[0].data.bones]
    with (manifest/'Bone_Manifest.csv').open('w',newline='',encoding='utf-8-sig') as f:w=csv.DictWriter(f,fieldnames=bones[0].keys());w.writeheader();w.writerows(bones)
    summary={'status':STATUS,'source_version':s.get('SourceVersion'),'opened_file':bpy.data.filepath,'opened_file_sha256':sha(bpy.data.filepath),'blender_version':bpy.app.version_string,'saved_by_review_script':False,'height_m':dim.z,'bounds':{'min':list(low),'max':list(high),'dimensions':list(dim)},'mesh_count':len(meshes),'vertices':sum(len(o.data.vertices) for o in meshes),'triangles':sum(tri(o) for o in meshes),'material_ids':sorted({x.material.name for o in meshes for x in o.material_slots if x.material}),'armatures':len(arms),'bones':len(bones),'actions':len(bpy.data.actions),'collections':sorted(c.name for c in bpy.data.collections),'topology':dict(zip(('non_manifold_edges','boundary_edges','loose_edges','zero_area_faces'),tot)),'baseline':str(Path(a.baseline_blend).resolve()),'baseline_sha256':sha(Path(a.baseline_blend).resolve()),'deferred':['Final Texture','Final UV','Final Skinning','Animation Polish','Formal LOD','Runtime Prefab replacement']}
    (manifest/'Geometry_Summary.json').write_text(json.dumps(summary,indent=2,ensure_ascii=False),encoding='utf-8')
    s,cam,ground=setup(low,high);clay=mat('MAT_P03_Clay',(.48,.52,.58),.82);black=mat('MAT_P03_Black',(.001,.001,.001),1);white=mat('MAT_P03_White',(.96,.96,.96),1);wire=mat('MAT_P03_Wire',(.005,.007,.01),1)
    original=override(meshes,clay)
    for name,d in [('01_Clay_Front',(0,-1,.06)),('02_Clay_Left',(-1,0,.06)),('03_Clay_Back',(0,1,.06)),('04_Clay_3Q_Front',(-1,-1,.14)),('05_Clay_3Q_Back',(1,1,.14))]:render(s,cam,center,d,dirs['Clay']/(name+'.png'))
    restore(meshes,original)
    render(s,cam,center,(0,-1,.06),dirs['MaterialID']/'MaterialID_Front.png');render(s,cam,center,(-1,-1,.14),dirs['MaterialID']/'MaterialID_3Q.png')
    override(meshes,black);bg=s.world.node_tree.nodes['Background'];bg.inputs['Color'].default_value=(.98,.98,.98,1);ground.data.materials.clear();ground.data.materials.append(white)
    for name,d in [('Silhouette_Front',(0,-1,.06)),('Silhouette_Left',(-1,0,.06)),('Silhouette_Back',(0,1,.06)),('Silhouette_3Q',(-1,-1,.14))]:render(s,cam,center,d,dirs['Silhouette']/(name+'.png'))
    for size in (128,64,32):render(s,cam,center,(0,-1,.06),dirs['ScreenSize']/f'Silhouette_{size}px.png',dim.z*256/size,(256,256))
    override(meshes,clay);bg.inputs['Color'].default_value=(.88,.90,.93,1);dupes=wire_dupes(meshes,wire)
    for name,d in [('Wireframe_Front',(0,-1,.06)),('Wireframe_3Q',(-1,-1,.14)),('Wireframe_Back',(0,1,.06))]:render(s,cam,center,d,dirs['Wireframe']/(name+'.png'))
    for o in dupes:bpy.data.objects.remove(o,do_unlink=True)
    details=[('Detail_Chest',(0,-1,.02),(0,-.02,1.28),.75),('Detail_Shoulder',(-1,-1,.05),(-.28,0,1.36),.63),('Detail_Waist',(0,-1,.02),(0,0,.88),.72),('Detail_Shield_Front',(0,-1,.02),(-.59,-.15,.84),1.05),('Detail_Shield_Back',(0,1,.02),(-.59,-.10,.84),1.05),('Detail_Boot',(0,-1,.02),(.15,-.05,.18),.55),('Detail_Sword',(0,-1,.02),(.80,-.02,.72),1.15)]
    for name,d,c,scale in details:render(s,cam,Vector(c),d,dirs['Detail']/(name+'.png'),scale)
    for size in (128,64,32):render(s,cam,center,(0,-1,.06),dirs['ScreenSize']/f'Clay_{size}px.png',dim.z*256/size,(256,256))
    restore(meshes,original);compare_baseline(s,cam,center,meshes,Path(a.baseline_blend).resolve(),dirs['Comparison'],clay)
    (dirs['Comparison']/'COMPOSITION_SOURCE.txt').write_text('P02R1 comparisons are direct Blender orthographic renders. L1 comparison sheets are composed from the approved concept and v004 clay views.\n',encoding='utf-8')
    (dirs['Unity']/'MANUAL_UNITY_REVIEW_REQUIRED.txt').write_text('MANUAL UNITY REVIEW REQUIRED\nNo Runtime Prefab was created or replaced. Import CHR_Infantry_A_v004 into an isolated review path, create PF_Unit_Infantry_v004_Review, and capture Close / Medium / RTS_Normal / Far under current URP lighting.\n',encoding='utf-8')
    images=[]
    for p in sorted((root/'Screenshots').rglob('*.png')):
        w,h=png_size(p);images.append({'Path':p.relative_to(root).as_posix(),'Width':w,'Height':h,'Bytes':p.stat().st_size,'SHA256':sha(p)})
    with (manifest/'Screenshot_Manifest.csv').open('w',newline='',encoding='utf-8-sig') as f:w=csv.DictWriter(f,fieldnames=images[0].keys());w.writeheader();w.writerows(images)
    print('AEGIS_PHASE03_RENDER_COMPLETE',json.dumps({'images':len(images),'summary':summary},ensure_ascii=False))
if __name__=='__main__':main()
