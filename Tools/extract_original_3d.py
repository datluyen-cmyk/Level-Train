"""Inventory and export original Unity mesh/material/prefab/animation references."""
from __future__ import annotations
from pathlib import Path
import csv, hashlib, json, re
import UnityPy
from UnityPy.export.MeshExporter import export_mesh_obj
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
BUNDLES=ROOT/"Research"/"extracted"/"assets"/"yoo"/"DefaultPackage"
OUT=ROOT/"Research"/"art3d"
PROJECT=ROOT/"RotateRings Test"/"Assets"/"RotateRings"/"Original3D"
KEEP_TYPES={"Mesh","Material","GameObject","Transform","MeshFilter","MeshRenderer","ParticleSystem","ParticleSystemRenderer","AnimationClip","Texture2D","Sprite","Shader","MonoBehaviour","Animator","Animation"}

def val_name(data): return str(getattr(data,"m_Name",getattr(data,"name","")) or "")
def ptr_id(pointer): return getattr(pointer,"path_id",0) if pointer else 0
def safe(s): return re.sub(r"[^A-Za-z0-9_.-]+","_",s).strip("._")[:90] or "unnamed"
def plain(value,depth=0):
    """Serialize UnityPy typed module objects into reviewable JSON values."""
    if depth>12:return str(value)
    if value is None or isinstance(value,(str,int,float,bool)):return value
    if isinstance(value,(list,tuple)):return [plain(v,depth+1) for v in value]
    if isinstance(value,dict):return {str(k):plain(v,depth+1) for k,v in value.items()}
    if hasattr(value,"path_id"):return {"path_id":ptr_id(value)}
    if hasattr(value,"__dict__"):
        return {k:plain(v,depth+1) for k,v in vars(value).items() if k!="object_reader" and not k.startswith("_")}
    return str(value)
def pair_parts(item):
    if isinstance(item,(list,tuple)) and len(item)>=2:return item[0],item[1]
    return getattr(item,"first",None),getattr(item,"second",None)
def selected_bundles():
    """Focus on reusable ring assets, common level prefabs/materials, and representative real scenes."""
    out=[]
    for p in BUNDLES.iterdir():
        n=p.name.lower()
        if not p.is_file():continue
        if "res_art_level_levelmaterials_" in n or "res_art_level_levelprefab_" in n or "res_art_effect_mesh_" in n:
            out.append(p);continue
        if "res_art_level_texture_ring_" in n or "res_art_level_texture_common_" in n:
            out.append(p);continue
        # Ring break, gameplay and UI prefabs can hold the original particle modules,
        # meshes and textures even when the bundle name does not mention the effect.
        if "res_art_effect_prefab_" in n and any(k in n for k in ("0fcf63", "eff_rr_gp", "maingameplay")):
            out.append(p);continue
        if "res_art_level_physiclevelscene_level10001_" in n or "res_art_level_physiclevelscene_level10002_" in n or "res_art_level_scene_" in n:
            out.append(p)
    return sorted(out)

def inventory():
    OUT.mkdir(parents=True,exist_ok=True)
    rows=[]
    for bundle in selected_bundles():
        try: env=UnityPy.load(str(bundle))
        except Exception as e: rows.append([bundle.name,"ERROR",0,type(e).__name__]);continue
        for obj in env.objects:
            if obj.type.name not in KEEP_TYPES:continue
            try:
                data=obj.read();name=val_name(data)
                extra=""
                if obj.type.name=="Mesh": extra=f"vertices={getattr(getattr(data,'m_VertexData',None),'m_VertexCount',getattr(data,'m_VertexCount',0))} submeshes={len(getattr(data,'m_SubMeshes',[]))}"
                elif obj.type.name=="MeshFilter": extra=f"mesh_path_id={ptr_id(getattr(data,'m_Mesh',None))}"
                elif obj.type.name=="MeshRenderer": extra=f"materials={','.join(str(ptr_id(x)) for x in getattr(data,'m_Materials',[]))}"
                elif obj.type.name=="Material": extra=f"shader_path_id={ptr_id(getattr(data,'m_Shader',None))}"
                elif obj.type.name=="Texture2D": extra=f"{getattr(data,'m_Width',0)}x{getattr(data,'m_Height',0)}"
                elif obj.type.name=="AnimationClip": extra=f"length={getattr(data,'m_MuscleClip',None) is not None}"
                elif obj.type.name=="ParticleSystem": extra=f"duration={getattr(data,'lengthInSec',0)} looping={getattr(data,'looping',False)}"
                rows.append([bundle.name,obj.type.name,obj.path_id,name,extra])
            except Exception as e:rows.append([bundle.name,obj.type.name,obj.path_id,f"<read {type(e).__name__}>",""])
    with (OUT/"inventory.csv").open("w",newline="",encoding="utf-8-sig") as f:
        w=csv.writer(f);w.writerow(["bundle","type","path_id","name","details"]);w.writerows(rows)
    return rows

def export_bundle(bundle,env):
    bslug=safe(bundle.stem)
    objdir=PROJECT/"Meshes"/bslug; objdir.mkdir(parents=True,exist_ok=True)
    artdir=OUT/"textures"/bslug;artdir.mkdir(parents=True,exist_ok=True)
    manifest={"bundle":bundle.name,"bundle_sha256":hashlib.sha256(bundle.read_bytes()).hexdigest(),"objects":[]}
    byid={o.path_id:o for o in env.objects}
    mesh_names={}
    for obj in env.objects:
        if obj.type.name!="Mesh":continue
        try:
            d=obj.read();name=val_name(d) or f"mesh_{obj.path_id}";mesh_names[obj.path_id]=name
            text=export_mesh_obj(d)
            if not text or text is False:continue
            dest=objdir/(safe(name)+f"_{obj.path_id}.obj");dest.write_text(text,encoding="utf-8")
            manifest["objects"].append({"type":"Mesh","name":name,"path_id":obj.path_id,"file":str(dest.relative_to(ROOT)),"vertices":getattr(getattr(d,"m_VertexData",None),"m_VertexCount",getattr(d,"m_VertexCount",0)),"submeshes":len(getattr(d,"m_SubMeshes",[]))})
        except Exception as e: manifest["objects"].append({"type":"Mesh","path_id":obj.path_id,"error":type(e).__name__})
    for obj in env.objects:
        if obj.type.name not in ("Texture2D","Sprite","Material","AnimationClip","Shader","ParticleSystem","ParticleSystemRenderer"):continue
        try:
            d=obj.read();name=val_name(d) or f"{obj.type.name}_{obj.path_id}";record={"type":obj.type.name,"name":name,"path_id":obj.path_id}
            if obj.type.name in ("Texture2D","Sprite"):
                image=d.image
                if image is not None:
                    dest=artdir/(safe(name)+f"_{obj.path_id}.png");image.save(dest)
                    # Keep an importable Unity copy under Resources while retaining the
                    # bundle-organized research export above.
                    resource_dir=PROJECT/"Resources"/"Original"/"Textures"/bslug
                    resource_dir.mkdir(parents=True,exist_ok=True)
                    image.save(resource_dir/dest.name)
                    # Flat Resources aliases make shader texture references loadable
                    # without the source bundle hash being part of runtime code.
                    flat_dir=PROJECT/"Resources"/"Original"/"Textures"
                    flat_dir.mkdir(parents=True,exist_ok=True)
                    flat=flat_dir/(safe(name)+".png")
                    if not flat.exists():image.save(flat)
                    settings=getattr(d,"m_TextureSettings",None)
                    record.update(file=str(dest.relative_to(ROOT)),width=image.width,height=image.height,
                                  color_space=getattr(d,"m_ColorSpace",None),mip_count=getattr(d,"m_MipCount",None),
                                  filter_mode=getattr(settings,"m_FilterMode",None),anisotropy=getattr(settings,"m_Aniso",None),
                                  mip_bias=getattr(settings,"m_MipBias",None),wrap_u=getattr(settings,"m_WrapU",None),
                                  wrap_v=getattr(settings,"m_WrapV",None),wrap_w=getattr(settings,"m_WrapW",None))
            elif obj.type.name=="Material":
                shader=getattr(d,"m_Shader",None);record["shader_path_id"]=ptr_id(shader)
                if shader and ptr_id(shader) in byid:
                    shader_data=shader.read();record["shader_name"]=val_name(shader_data) or getattr(getattr(shader_data,"m_ParsedForm",None),"m_Name","")
                else:record["shader_name"]=""
                props=getattr(d,"m_SavedProperties",None)
                record["saved_properties"]={}
                if props:
                    for group in ("m_TexEnvs","m_Floats","m_Colors","m_Ints"):
                        arr=getattr(props,group,[]) or []
                        if group=="m_TexEnvs":
                            records=[]
                            for x in arr:
                                key,tex_env=pair_parts(x);records.append({"name":str(key),"texture_path_id":ptr_id(getattr(tex_env,"m_Texture",None)),"scale":str(getattr(tex_env,"m_Scale",None)),"offset":str(getattr(tex_env,"m_Offset",None))})
                            record["saved_properties"][group]=records
                        else:
                            record["saved_properties"][group]=[{"name":str(pair_parts(x)[0]),"value":plain(pair_parts(x)[1])} for x in arr]
            elif obj.type.name=="AnimationClip":
                record["sample_rate"]=getattr(d,"m_SampleRate",0);record["wrap_mode"]=getattr(d,"m_WrapMode",0);record["curves"]={}
                for curve_name in ("m_PositionCurves","m_ScaleCurves","m_FloatCurves","m_RotationCurves","m_EulerCurves","m_PPtrCurves"):
                    curves=[]
                    for curve in getattr(d,curve_name,[]) or []:
                        curves.append(str(curve))
                    if curves:record["curves"][curve_name]=curves
            elif obj.type.name=="Shader":
                # UnityPy emits the serialized shader declarations and any
                # recoverable GLES/GLSL program text. Keep this as a research dump;
                # platform blobs can still be binary or otherwise non-editable.
                dumpdir=OUT/"shaders";dumpdir.mkdir(parents=True,exist_ok=True)
                dump=d.export()
                dest=dumpdir/(safe(bundle.stem)+"_"+safe(name)+f"_{obj.path_id}.shaderdump")
                dest.write_text(dump,encoding="utf-8",errors="replace")
                record["shader_name"]=getattr(getattr(d,"m_ParsedForm",None),"m_Name","")
                record["script_file"]=str(dest.relative_to(ROOT))
                record["script_length"]=len(dump)
            elif obj.type.name=="ParticleSystem":
                record["gameobject_path_id"]=ptr_id(getattr(d,"m_GameObject",None))
                record["modules"]={key:plain(getattr(d,key,None)) for key in ("InitialModule","EmissionModule","ShapeModule","VelocityModule","ForceModule","ColorModule","SizeModule","RotationModule","CollisionModule","NoiseModule","SubModule","UVModule","TrailModule")}
                record["duration_seconds"]=getattr(d,"lengthInSec",0)
                record["looping"]=getattr(d,"looping",False)
                record["start_delay"]=plain(getattr(d,"startDelay",None))
            elif obj.type.name=="ParticleSystemRenderer":
                record.update({k:plain(getattr(d,k,None)) for k in ("m_GameObject","m_Materials","m_Mesh","m_RenderMode","m_SortingOrder","m_SortingFudge","m_MinParticleSize","m_MaxParticleSize","m_RenderingLayerMask")})
            manifest["objects"].append(record)
        except Exception as e:manifest["objects"].append({"type":obj.type.name,"path_id":obj.path_id,"error":type(e).__name__})
    # Rebuild the original GameObject/Transform hierarchy, keeping every object/component path ID.
    transforms={};gameobjects={}
    for obj in env.objects:
        if obj.type.name not in ("Transform","GameObject"):continue
        try:
            d=obj.read()
            if obj.type.name=="GameObject":gameobjects[obj.path_id]=d
            else:transforms[obj.path_id]=d
        except Exception:pass
    transform_by_go={ptr_id(getattr(t,"m_GameObject",None)): (tid,t) for tid,t in transforms.items()}
    def node(goid,depth=0):
        if depth>100 or goid not in gameobjects:return None
        go=gameobjects[goid];pair=transform_by_go.get(goid);tid,t=pair if pair else (0,None)
        comps=[]
        for c in getattr(go,"m_Component",[]) or []:
            cp=getattr(c,"component",None);cid=ptr_id(cp);co=byid.get(cid)
            if co:
                try: cd=co.read();comps.append({"type":co.type.name,"path_id":cid,"name":val_name(cd),"mesh_path_id":ptr_id(getattr(cd,"m_Mesh",None)) if co.type.name=="MeshFilter" else None,"material_path_ids":[ptr_id(x) for x in (getattr(cd,"m_Materials",[]) or [])] if co.type.name=="MeshRenderer" else None})
                except Exception:comps.append({"type":co.type.name,"path_id":cid})
        result={"name":val_name(go),"gameobject_path_id":goid,"transform_path_id":tid,"local_position":str(getattr(t,"m_LocalPosition",None)) if t else "","local_rotation":str(getattr(t,"m_LocalRotation",None)) if t else "","local_scale":str(getattr(t,"m_LocalScale",None)) if t else "","components":comps,"children":[]}
        if t:
            for cp in getattr(t,"m_Children",[]) or []:
                child_transform=byid.get(ptr_id(cp))
                if child_transform:
                    try: cgo=ptr_id(getattr(child_transform.read(),"m_GameObject",None));cn=node(cgo,depth+1)
                    except Exception:cn=None
                    if cn:result["children"].append(cn)
        return result
    children_go={}
    for tid,t in transforms.items():
        for child in getattr(t,"m_Children",[]) or []:
            cobj=byid.get(ptr_id(child))
            if cobj:
                try:children_go[ptr_id(getattr(cobj.read(),"m_GameObject",None))]=True
                except Exception:pass
    roots=[]
    for gid in gameobjects:
        if gid not in children_go:
            n=node(gid)
            if n:roots.append(n)
    manifest["hierarchy_roots"]=roots
    return manifest

def main():
    OUT.mkdir(parents=True,exist_ok=True);PROJECT.mkdir(parents=True,exist_ok=True)
    rows=inventory();bundles=selected_bundles();manifests=[]
    for b in bundles:
        try:env=UnityPy.load(str(b));manifests.append(export_bundle(b,env))
        except Exception as e:manifests.append({"bundle":b.name,"error":type(e).__name__})
    with (OUT/"export_manifest.json").open("w",encoding="utf-8") as f:json.dump(manifests,f,indent=2,ensure_ascii=False)
    ring_materials=[{"bundle":m.get("bundle"),"bundle_sha256":m.get("bundle_sha256"),**obj} for m in manifests for obj in m.get("objects",[]) if obj.get("type")=="Material" and obj.get("shader_name")=="HsApp/Effect/XYStyleLine"]
    with (OUT/"ring_materials.json").open("w",encoding="utf-8") as f:json.dump(ring_materials,f,indent=2,ensure_ascii=False)
    counts={}
    for row in rows:counts[row[1]]=counts.get(row[1],0)+1
    print(json.dumps({"bundles":len(bundles),"inventory_rows":len(rows),"type_counts":counts,"exports":sum(len(m.get("objects",[])) for m in manifests)},indent=2))

if __name__=="__main__":main()
