"""Export the P03R1 candidate as a static review-only FBX."""
import argparse,hashlib,json
from pathlib import Path
import bpy
def args():
 p=argparse.ArgumentParser();p.add_argument('--output',required=True);p.add_argument('--expected-blend-sha256',required=True);v=__import__('sys').argv;return p.parse_args(v[v.index('--')+1:] if '--' in v else [])
def sha(p):
 h=hashlib.sha256()
 with open(p,'rb') as f:
  for c in iter(lambda:f.read(1048576),b''):h.update(c)
 return h.hexdigest().upper()
def main():
 a=args();opened=Path(bpy.data.filepath).resolve()
 if bpy.context.scene.get('SourceVersion')!='CHR_Infantry_A_v004_P03R1':raise RuntimeError('Wrong source version')
 if sha(opened)!=a.expected_blend_sha256.upper():raise RuntimeError('Revision blend checksum mismatch')
 out=Path(a.output).resolve();out.parent.mkdir(parents=True,exist_ok=True)
 bpy.ops.object.select_all(action='DESELECT');selected=[]
 for o in bpy.context.scene.objects:
  if o.type in {'MESH','ARMATURE','EMPTY'}:o.select_set(True);selected.append(o)
 bpy.context.view_layer.objects.active=bpy.data.objects['Armature']
 bpy.ops.export_scene.fbx(filepath=str(out),use_selection=True,object_types={'ARMATURE','MESH','EMPTY'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',add_leaf_bones=False,use_armature_deform_only=False,bake_anim=False,path_mode='STRIP',embed_textures=False)
 result={'status':'REVIEW_ONLY','source':str(opened),'source_sha256':sha(opened),'fbx':str(out),'fbx_sha256':sha(out),'bytes':out.stat().st_size,'objects':len(selected),'animation':False,'runtime_replacement':False}
 print('AEGIS_P03R1_FBX_EXPORT_COMPLETE');print(json.dumps(result,indent=2))
if __name__=='__main__':main()
