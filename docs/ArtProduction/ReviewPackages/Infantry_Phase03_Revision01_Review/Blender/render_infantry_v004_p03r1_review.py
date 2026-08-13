"""Render P03R1 Blender review evidence; never saves the opened blend."""
import argparse,csv,hashlib,importlib.util,json,struct
from pathlib import Path
import bpy
from mathutils import Vector

STATUS='READY FOR PHASE03 REVISION REVIEW'
def args():
 p=argparse.ArgumentParser();p.add_argument('--output-root',required=True);p.add_argument('--baseline-mode',action='store_true');p.add_argument('--quick-mode',action='store_true');v=__import__('sys').argv;return p.parse_args(v[v.index('--')+1:] if '--' in v else [])
def load_base():
 p=Path(__file__).resolve().parents[2]/'v003'/'Source'/'render_infantry_v003_p02r1_review.py';s=importlib.util.spec_from_file_location('p03render',p);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
def sha(p):
 h=hashlib.sha256()
 with open(p,'rb') as f:
  for c in iter(lambda:f.read(1048576),b''):h.update(c)
 return h.hexdigest().upper()
def pngsize(p):
 with open(p,'rb') as f:b=f.read(24)
 return struct.unpack('>II',b[16:24])
def main():
 a=args();base=load_base();root=Path(a.output_root).resolve();scene=bpy.context.scene;version=scene.get('SourceVersion')
 expected='CHR_Infantry_A_v004' if a.baseline_mode else 'CHR_Infantry_A_v004_P03R1'
 if version!=expected:raise RuntimeError(f'Expected {expected}, got {version}')
 meshes=[o for o in scene.objects if o.type=='MESH'];arms=[o for o in scene.objects if o.type=='ARMATURE'];material_ids=sorted({s.material.name for o in meshes for s in o.material_slots if s.material});low,high=base.world_bounds(meshes);dim=high-low;center=(low+high)*.5
 dirs={n:root/'Screenshots'/n for n in ('Clay','Detail','Silhouette','ScreenSize','MaterialID','Comparison','L1Comparison','Unity')};man=root/'Manifests'
 for p in list(dirs.values())+([man] if not a.baseline_mode else []):p.mkdir(parents=True,exist_ok=True)
 scene,cam,setup_center,ground=base.setup_scene(low,high);clay=base.material('MAT_P03R1_Clay',(.48,.52,.58),.82);black=base.material('MAT_P03R1_Black',(.001,.001,.001),1);white=base.material('MAT_P03R1_White',(.96,.96,.96),1)
 original=base.set_override(meshes,clay)
 if not a.baseline_mode:
  for n,d in [('01_Clay_Front',(0,-1,.06)),('02_Clay_Left',(-1,0,.06)),('03_Clay_Back',(0,1,.06)),('04_Clay_3Q_Front',(-1,-1,.14)),('05_Clay_3Q_Back',(1,1,.14))]:base.render(scene,cam,center,d,dirs['Clay']/(n+'.png'))
 else:
  base.render(scene,cam,center,(0,-1,.06),dirs['Clay']/'01_Clay_Front.png');base.render(scene,cam,center,(-1,-1,.14),dirs['Clay']/'04_Clay_3Q_Front.png')
 details=[('Detail_WaistCloth',(0,-1,.02),(0,-.02,.87),.70),('Detail_Scarf',(0,-1,.02),(0,-.02,1.42),.58),('Detail_UpperArm',(-1,-1,.04),(-.36,-.01,1.34),.58),('Detail_Shield_Back',(0,1,.02),(-.59,-.08,.88),1.02),('Detail_Shield_Back_WithArm',(0,1,.02),(-.62,-.02,1.07),.74),('Detail_Boot',(0,-1,.02),(.15,-.05,.17),.53)]
 for n,d,c,scale in details:base.render(scene,cam,Vector(c),d,dirs['Detail']/(n+'.png'),scale)
 if a.baseline_mode:
  print('AEGIS_P03R1_BASELINE_RENDER_COMPLETE');return
 if a.quick_mode:
  print('AEGIS_P03R1_QUICK_RENDER_COMPLETE');return
 base.restore_materials(meshes,original)
 for n,d in [('MaterialID_Front',(0,-1,.06)),('MaterialID_3Q',(-1,-1,.14)),('MaterialID_Back',(0,1,.06))]:base.render(scene,cam,center,d,dirs['MaterialID']/(n+'.png'))
 base.set_override(meshes,black);bg=scene.world.node_tree.nodes['Background'];bg.inputs['Color'].default_value=(.98,.98,.98,1);ground.data.materials.clear();ground.data.materials.append(white)
 for n,d in [('Silhouette_Front',(0,-1,.06)),('Silhouette_Left',(-1,0,.06)),('Silhouette_Back',(0,1,.06)),('Silhouette_3Q',(-1,-1,.14))]:base.render(scene,cam,center,d,dirs['Silhouette']/(n+'.png'))
 for size in (128,64,32):base.render(scene,cam,center,(0,-1,.06),dirs['ScreenSize']/f'Silhouette_{size}px.png',dim.z*256/size,(256,256))
 base.set_override(meshes,clay);bg.inputs['Color'].default_value=(.88,.90,.93,1)
 for size in (128,64,32):base.render(scene,cam,center,(0,-1,.06),dirs['ScreenSize']/f'Clay_{size}px.png',dim.z*256/size,(256,256))
 rows=[];total=[0,0,0,0]
 for o in sorted(meshes,key=lambda x:x.name):
  t=base.topology_stats(o);total=[total[i]+t[i] for i in range(4)];rows.append({'ObjectName':o.name,'Collection':';'.join(c.name for c in o.users_collection),'Vertices':len(o.data.vertices),'Triangles':base.mesh_triangles(o),'Materials':';'.join(s.material.name for s in o.material_slots if s.material),'Parent':o.parent.name if o.parent else '','AttachmentBone':o.get('AttachmentBone',''),'BindingStatus':o.get('BindingStatus',''),'NonManifoldEdges':t[0],'BoundaryEdges':t[1],'LooseEdges':t[2],'ZeroAreaFaces':t[3]})
 with (man/'Object_Manifest.csv').open('w',newline='',encoding='utf-8-sig') as f:w=csv.DictWriter(f,fieldnames=rows[0].keys());w.writeheader();w.writerows(rows)
 bones=[{'Bone':b.name,'Parent':b.parent.name if b.parent else '','Deform':b.use_deform} for b in arms[0].data.bones]
 with (man/'Bone_Manifest.csv').open('w',newline='',encoding='utf-8-sig') as f:w=csv.DictWriter(f,fieldnames=bones[0].keys());w.writeheader();w.writerows(bones)
 summary={'status':STATUS,'source_version':version,'opened_file':bpy.data.filepath,'opened_file_sha256':sha(bpy.data.filepath),'blender_version':bpy.app.version_string,'saved_by_review_script':False,'height_m':dim.z,'bounds':{'min':list(low),'max':list(high),'dimensions':list(dim)},'mesh_count':len(meshes),'vertices':sum(len(o.data.vertices) for o in meshes),'triangles':sum(base.mesh_triangles(o) for o in meshes),'material_ids':material_ids,'armatures':len(arms),'bones':len(bones),'empties':len([o for o in scene.objects if o.type=='EMPTY']),'actions':len(bpy.data.actions),'collections':sorted(c.name for c in bpy.data.collections),'topology':dict(zip(('non_manifold_edges','boundary_edges','loose_edges','zero_area_faces'),total)),'protected_v004_sha256':scene.get('SourceBaselineSHA256'),'deferred':['Phase 04','Final UV','Final Texture','Animation Polish','Final Skinning','Formal LOD','Runtime Prefab replacement']}
 (man/'Geometry_Summary.json').write_text(json.dumps(summary,indent=2,ensure_ascii=False),encoding='utf-8')
 images=[]
 for p in sorted((root/'Screenshots').rglob('*.png')):
  w,h=pngsize(p);images.append({'Path':p.relative_to(root).as_posix(),'Width':w,'Height':h,'Bytes':p.stat().st_size,'SHA256':sha(p)})
 with (man/'Screenshot_Manifest.csv').open('w',newline='',encoding='utf-8-sig') as f:w=csv.DictWriter(f,fieldnames=images[0].keys());w.writeheader();w.writerows(images)
 print('AEGIS_P03R1_RENDER_COMPLETE',json.dumps({'images':len(images),'summary':summary},ensure_ascii=False))
if __name__=='__main__':main()
