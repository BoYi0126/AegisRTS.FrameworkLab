"""Create CHR_Infantry_A_v004_P03R1 from immutable Phase 03 v004.

Only the five reviewer-requested Secondary Form areas are changed.  The script
refuses an unexpected v004 hash and saves exclusively to a new revision file.
"""
import argparse,hashlib,importlib.util,json,math
from pathlib import Path
import bmesh,bpy
from mathutils import Vector

INPUT_VERSION="CHR_Infantry_A_v004"
OUTPUT_VERSION="CHR_Infantry_A_v004_P03R1"
REVIEW_STATUS="READY FOR PHASE03 REVISION REVIEW"

def args():
    p=argparse.ArgumentParser();p.add_argument('--output-root',required=True);p.add_argument('--expected-v004-sha256',required=True)
    v=__import__('sys').argv;return p.parse_args(v[v.index('--')+1:] if '--' in v else [])
def sha(path):
    h=hashlib.sha256()
    with open(path,'rb') as f:
        for c in iter(lambda:f.read(1048576),b''):h.update(c)
    return h.hexdigest().upper()
def helpers():
    p=Path(__file__).resolve().parents[2]/'v003'/'Source'/'build_infantry_v003_primary_forms.py';s=importlib.util.spec_from_file_location('aegis_geo',p);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
def delete(names):
    for n in names:
        o=bpy.data.objects.get(n)
        if o:
            d=o.data if o.type=='MESH' else None;bpy.data.objects.remove(o,do_unlink=True)
            if d and d.users==0:bpy.data.meshes.remove(d)
def parent(obj,arm,bone=''):
    world=obj.matrix_world.copy();obj.parent=arm;obj.parent_type='OBJECT';obj.matrix_parent_inverse=arm.matrix_world.inverted();obj.matrix_world=world
    obj['BindingStatus']='PHASE03_REVISION_STATIC_APOSE_REVIEW';obj['AttachmentBone']=bone;obj['Revision']='P03R1'
def multi_boxes(build,name,boxes,col,mat,bevel=.004):
    verts=[];faces=[]
    for cx,cy,cz,sx,sy,sz in boxes:
        base=len(verts);verts += [(cx+x*sx/2,cy+y*sy/2,cz+z*sz/2) for x,y,z in ((-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1))]
        faces += [tuple(base+i for i in q) for q in ((0,1,2,3),(4,7,6,5),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0))]
    return build.bevel(build.new_mesh(name,verts,faces,col,mat,False),bevel,2)
def ribbon(name,points,widths,thickness,col,mat,build):
    verts=[]
    for i,pv in enumerate(points):
        p=Vector(pv);t=(Vector(points[min(i+1,len(points)-1)])-Vector(points[max(i-1,0)])).normalized();side=Vector((t.z,0,-t.x)).normalized();
        for yoff in (-thickness/2,thickness/2):
            verts.append(tuple(p-side*widths[i]+Vector((0,yoff,0))));verts.append(tuple(p+side*widths[i]+Vector((0,yoff,0))))
    faces=[]
    for i in range(len(points)-1):
        a=i*4;b=(i+1)*4
        faces += [(a,a+1,b+1,b),(a+2,b+2,b+3,a+3),(a,b,b+2,a+2),(a+1,a+3,b+3,b+1)]
    last=(len(points)-1)*4;faces += [(0,2,3,1),(last,last+1,last+3,last+2)]
    return build.bevel(build.new_mesh(name,verts,faces,col,mat,False),.004,2)
def rebuild_waist(build,col,mat,arm):
    delete(['WaistCloth_P02R1'])
    xnorm=(-1,-.45,0,.45,1);rings=((1.025,.075),(.90,.090),(.76,.102),(.65,.105));front_profiles=(0,-.018,-.035,-.016,0);verts=[]
    for ri,(z,half) in enumerate(rings):
        fall=ri/(len(rings)-1)
        for back in (False,True):
            for xi,xn in enumerate(xnorm):
                hem=(.004*xn+.004*math.cos(xn*math.pi)) if ri==len(rings)-1 else 0
                y=(-.154+front_profiles[xi]*(.35+.65*fall)) if not back else -.128
                verts.append((xn*half,y,z+hem))
    faces=[];cols=len(xnorm)
    for r in range(len(rings)-1):
        a=r*cols*2;b=(r+1)*cols*2
        for x in range(cols-1):
            faces.append((a+x,a+x+1,b+x+1,b+x));faces.append((a+cols+x,b+cols+x,b+cols+x+1,a+cols+x+1))
        faces.append((a,b,b+cols,a+cols));faces.append((a+cols-1,a+cols*2-1,b+cols*2-1,b+cols-1))
    top=0;bottom=(len(rings)-1)*cols*2
    for x in range(cols-1):faces.append((top+x,top+cols+x,top+cols+x+1,top+x+1));faces.append((bottom+x,bottom+x+1,bottom+cols+x+1,bottom+cols+x))
    o=build.bevel(build.new_mesh('GEO_Infantry_WaistCloth_Front_P03R1',verts,faces,col,mat,False),.004,2);parent(o,arm,'Hips');o['RevisionChange']='Rebuilt as thin hanging cloth with central fold, two broad planes, compressed belt attachment and asymmetric hem'
def rebuild_scarf(build,col,mat,arm):
    delete(['Scarf_Drape','GEO_Infantry_Scarf_Fold_A','GEO_Infantry_Scarf_Fold_B','GEO_Infantry_Scarf_RearTermination'])
    points=[(-.130,-.180,1.490),(-.070,-.185,1.462),(0,-.188,1.425),(.060,-.185,1.385),(.110,-.178,1.345)]
    sash=ribbon('GEO_Infantry_Scarf_BroadDrape_P03R1',points,[.035,.040,.038,.033,.023],.016,col,mat,build);parent(sash,arm,'Chest');sash['RevisionChange']='Wide flat cloth sash with one broad turn, armor contact and tapered termination'
    rear=ribbon('GEO_Infantry_Scarf_RearTaper_P03R1',[(.10,.135,1.48),(.095,.145,1.40),(.075,.140,1.325)],[.042,.040,.026],.018,col,mat,build);parent(rear,arm,'Chest');rear['RevisionChange']='Broad rear cloth termination'
def reshape_arm(o,start,end):
    a=Vector(start);b=Vector(end);axis=b-a;l2=axis.length_squared;unit=axis.normalized();front=Vector((0,-1,0));side=unit.cross(front).normalized()
    for v in o.data.vertices:
        p=Vector(v.co);t=max(0,min(1,(p-a).dot(axis)/l2));center=a+axis*t;off=p-center;s=off.dot(side);f=off.dot(front)
        compression=.86+.12*math.sin(math.pi*t);compression*=1-.10*math.exp(-((t-.18)/.10)**2)-.08*math.exp(-((t-.78)/.11)**2)
        s*=compression*1.04;f*=compression*.68;v.co=center+side*s+front*f
    for polygon in o.data.polygons:polygon.use_smooth=False
    o['RevisionChange']='Planed sleeve cross-section, shoulder compression and two broad cloth turns'
def rebuild_shield_back(build,col,metal,leather,arm):
    delete(['GEO_Infantry_Shield_BackBrace','GEO_Infantry_Shield_BackGrip','GEO_Infantry_Shield_ForearmStrap'])
    major=multi_boxes(build,'GEO_Infantry_Shield_BackMajorBrace_P03R1',[(-.59,-.108,.79,.42,.032,.055)],col,metal,.006);parent(major,arm,'LeftHand')
    secondary=multi_boxes(build,'GEO_Infantry_Shield_BackSecondaryBrace_P03R1',[(-.59,-.108,.67,.052,.032,.28)],col,metal,.006);parent(secondary,arm,'LeftHand')
    strap=multi_boxes(build,'GEO_Infantry_Shield_ForearmStrap_P03R1',[(-.60,.040,1.155,.22,.022,.040),(-.705,-.045,1.155,.036,.170,.075),(-.495,-.045,1.155,.036,.170,.075)],col,leather,.006);parent(strap,arm,'LeftLowerArm')
    grip=multi_boxes(build,'GEO_Infantry_Shield_HandGrip_P03R1',[(-.765,.038,1.075,.034,.034,.135),(-.765,-.045,1.005,.065,.170,.025),(-.765,-.045,1.145,.065,.170,.025)],col,leather,.006);parent(grip,arm,'LeftHand')
    major['RevisionChange']='One readable major brace';secondary['RevisionChange']='One subordinate secondary brace';strap['RevisionChange']='Broad forearm strap with shield attachment posts and arm clearance';grip['RevisionChange']='Vertical hand grip aligned to palm with two bases'
def integrate_boots():
    delete(['GEO_Infantry_Boot_UpperPanel_L','GEO_Infantry_Boot_ToePanel_L','GEO_Infantry_Boot_HeelBlock_L','GEO_Infantry_Boot_UpperPanel_R','GEO_Infantry_Boot_ToePanel_R','GEO_Infantry_Boot_HeelBlock_R'])
    for side,sfx in ((-1,'L'),(1,'R')):
        o=bpy.data.objects[f'Boot_{sfx}'];cx=side*.155
        for v in o.data.vertices:
            c=v.co;dx=(c.x-cx)/.11
            if c.z>.12:c.x=cx+(c.x-cx)*.91
            if c.y<-.04:
                toe=max(0,1-abs(c.y+.14)/.18)*max(0,1-dx*dx);instep=max(0,1-abs(c.z-.115)/.075)
                c.z += .011*toe;c.y -= .008*toe*instep
            if c.y>.09 and c.z<.11:c.y-=.008
        o['RevisionChange']='Toe and upper transition sculpted into base leather boot; sole preserved'
def tri(o):o.data.calc_loop_triangles();return len(o.data.loop_triangles)
def bounds(obs):
    p=[o.matrix_world@Vector(c) for o in obs for c in o.bound_box];return Vector((min(v.x for v in p),min(v.y for v in p),min(v.z for v in p))),Vector((max(v.x for v in p),max(v.y for v in p),max(v.z for v in p)))
def topology(obs):
    r=dict(non_manifold_edges=0,boundary_edges=0,loose_edges=0,zero_area_faces=0)
    for o in obs:
        bm=bmesh.new();bm.from_mesh(o.data);r['non_manifold_edges']+=sum(not e.is_manifold for e in bm.edges);r['boundary_edges']+=sum(e.is_boundary for e in bm.edges);r['loose_edges']+=sum(not e.link_faces for e in bm.edges);r['zero_area_faces']+=sum(f.calc_area()<=1e-10 for f in bm.faces);bm.free()
    return r
def main():
    a=args();opened=Path(bpy.data.filepath).resolve();actual=sha(opened);scene=bpy.context.scene
    if scene.get('SourceVersion')!=INPUT_VERSION:raise RuntimeError('P03R1 requires immutable CHR_Infantry_A_v004')
    if actual!=a.expected_v004_sha256.upper():raise RuntimeError(f'v004 checksum mismatch: {actual}')
    build=helpers();arm=bpy.data.objects.get('Armature');bones=sorted(b.name for b in arm.data.bones);empties=sorted(o.name for o in scene.objects if o.type=='EMPTY')
    cloth=bpy.data.collections['GEO_CLOTH'];weapons=bpy.data.collections['GEO_WEAPONS'];team=bpy.data.materials['MATID_Team'];metal=bpy.data.materials['MATID_Metal'];leather=bpy.data.materials['MATID_Leather']
    delete(['GEO_Infantry_WaistCloth_FrontFold']);rebuild_waist(build,cloth,team,arm);rebuild_scarf(build,cloth,team,arm)
    reshape_arm(bpy.data.objects['UpperArm_L'],(-.235,0,1.405),(-.505,-.002,1.245));reshape_arm(bpy.data.objects['UpperArm_R'],(.235,0,1.405),(.505,-.002,1.245))
    rebuild_shield_back(build,weapons,metal,leather,arm);integrate_boots()
    meshes=[o for o in scene.objects if o.type=='MESH']
    for o in meshes:
        bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(o.data);bm.free();o.data.update()
    bpy.context.view_layer.update();lo,hi=bounds(meshes);count=sum(tri(o) for o in meshes)
    if not 1.80<=hi.z-lo.z<=1.85:raise RuntimeError('Height gate failed')
    if not 32000<=count<=36000:raise RuntimeError(f'Triangle gate failed: {count}')
    if sorted(b.name for b in arm.data.bones)!=bones or sorted(o.name for o in scene.objects if o.type=='EMPTY')!=empties:raise RuntimeError('Rig/socket contract changed')
    scene['SourceVersion']=OUTPUT_VERSION;scene['SourceBaseline']=INPUT_VERSION;scene['SourceBaselineSHA256']=actual;scene['ReviewStatus']=REVIEW_STATUS;scene['ProductionStatus']='WIP_MODEL';scene['Phase']='03_SECONDARY_FORMS_REVISION_01';scene['RevisionDecision']='CHANGE_REQUESTED_FIXED_FOR_REVIEW';scene['FinalTexture']=False;scene['FinalUV']=False;scene['AnimationPolish']=False;scene['FormalLOD']=False;scene['RuntimePrefabContract']='PF_Unit_Infantry (not replaced)'
    root=Path(a.output_root).resolve();src=root/'Source';doc=root/'Documentation';src.mkdir(parents=True,exist_ok=True);doc.mkdir(parents=True,exist_ok=True);out=src/'CHR_Infantry_A_v004_P03R1.blend'
    if out==opened:raise RuntimeError('Refusing protected overwrite')
    bpy.context.preferences.filepaths.save_version=0;bpy.ops.wm.save_as_mainfile(filepath=str(out))
    result={'status':REVIEW_STATUS,'source_version':OUTPUT_VERSION,'input':str(opened),'input_sha256':actual,'output':str(out),'output_sha256':sha(out),'blender_version':bpy.app.version_string,'height_m':hi.z-lo.z,'mesh_count':len(meshes),'vertices':sum(len(o.data.vertices) for o in meshes),'triangles':count,'material_ids':sorted({s.material.name for o in meshes for s in o.material_slots if s.material}),'armatures':1,'bones':len(bones),'empties':len(empties),'actions':len(bpy.data.actions),'topology':topology(meshes),'revision_changes':['Front waist cloth integrated folds/thickness/hem','Broad flat scarf drape and tapered termination','Planed/compressed upper-arm sleeve forms','Simplified shield back brace plus readable strap/grip','Boot panels integrated into base boot; soles preserved'],'preserved':['Head','Helmet','Plume direction','Three-layer shoulders','Chest lamellar rows','Shield front/rim/boss','Sword','Primary silhouette'],'deferred':['Phase 04','Final UV','Final Texture','Animation Polish','Final Skinning','Formal LOD','Runtime Prefab replacement']}
    (doc/'P03R1_BUILD_RESULT.json').write_text(json.dumps(result,indent=2,ensure_ascii=False),encoding='utf-8');print('AEGIS_P03R1_COMPLETE');print(json.dumps(result,indent=2,ensure_ascii=False))
if __name__=='__main__':main()
