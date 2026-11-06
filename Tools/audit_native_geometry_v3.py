"""Check every native scene collider/Rigidbody against the V3 source export."""
from pathlib import Path
from collections import Counter
import json,hashlib
import UnityPy
ROOT=Path(__file__).resolve().parents[1]
def read(p):return json.loads(p.read_text(encoding='utf8'))
def main():
 normalized=read(ROOT/'Research/normalized_corpus.json')['levels'];pack=read(ROOT/'Research/physics_v3/full500/source_parity_pack.json');byid={l['sourceId']:l for l in pack['levels']};summary=Counter();unassigned=[];area_names=[];rb_missing=[];types=Counter();levelrecords=[]
 for source in normalized:
  l=byid[source['id']];raw=read(ROOT/'Research/levels'/f"{source['id']}.json");env=UnityPy.load(str(ROOT/'Research/extracted/assets/yoo/DefaultPackage'/source['provenance']['bundle']));objects={o.path_id:{'type':o.type.name,'tree':o.read_typetree()} for o in env.objects if o.type.name in ['GameObject','Rigidbody2D','PolygonCollider2D','BoxCollider2D','CircleCollider2D','CapsuleCollider2D','EdgeCollider2D']};markers={c['data']['m_GameObject']['m_PathID']:c['data'] for c in raw['components'] if c['class']=='ColliderRoleMarker'};assigned={b['sourceColliderId'] for p in l['parts'] for b in p['sourceColliderBindings']}|{b['sourceColliderId'] for b in l.get('bombs',[])};rbids={p['sourceRigidbody']['sourceComponentId'] for p in l['parts'] if p['sourceRigidbody']['present']};unknown=[]
  for oid,obj in objects.items():
   typ=obj['type'];d=obj['tree']
   if typ=='GameObject':
    if 'area' in d['m_Name'].lower() or 'layer' in d['m_Name'].lower():area_names.append({'sourceId':l['sourceId'],'sourceGameObjectId':oid,'name':d['m_Name'],'layer':d['m_Layer']})
    continue
   if typ=='Rigidbody2D':
    summary['nativeRigidbodyCount']+=1
    if oid not in rbids:rb_missing.append({'sourceId':l['sourceId'],'sourceRigidbodyId':oid,'tree':d})
    continue
   types[typ]+=1;go=objects[d['m_GameObject']['m_PathID']]['tree'];layer=go['m_Layer'];role=markers.get(d['m_GameObject']['m_PathID'],{}).get('role',0);summary['nativeColliderCount']+=1
   if oid in assigned:summary['exportedColliderCount']+=1;continue
   category='inputLayer6' if layer==6 else 'disabledNativeCollider' if not d['m_Enabled'] or not go['m_IsActive'] else 'requiresAudit'
   record={'sourceId':l['sourceId'],'position':l['position'],'sourceColliderId':oid,'sourceColliderType':typ,'sourceGameObject':go['m_Name'],'sourceLayer':layer,'trigger':bool(d['m_IsTrigger']),'enabled':bool(d['m_Enabled']),'active':bool(go['m_IsActive']),'sourceRole':role,'classification':category,'nativeTree':d};unknown.append(record);summary['unassigned_'+category]+=1
  unassigned.extend(unknown);levelrecords.append({'sourceId':l['sourceId'],'position':l['position'],'unassignedColliderCount':len(unknown)})
  if len(levelrecords)%100==0:print('NATIVE INVENTORY',len(levelrecords),'/500',flush=True)
 out={'sourceCount':500,'scope':'Every nativeCollider2D/Rigidbody2D in all500 scene bundles, compared to exact per-instance V3 bindings. Input colliders retained in audit, not mistaken for gameplay bodies.','summary':dict(summary),'nativeColliderClasses':dict(types),'missingRigidbodyMappings':rb_missing,'unassignedColliders':unassigned,'areaOrLayerGameObjects':area_names,'levels':levelrecords};path=ROOT/'Research/physics_v3/source500_native_geometry_audit.json';path.write_text(json.dumps(out,separators=(',',':')),encoding='utf8');print(json.dumps({'summary':dict(summary),'classes':dict(types),'missingRigidbodies':len(rb_missing),'unassignedRequiredAudit':sum(x['classification']=='requiresAudit' for x in unassigned),'areaLayerNames':dict(Counter(x['name'] for x in area_names))},indent=2))
if __name__=='__main__':main()
