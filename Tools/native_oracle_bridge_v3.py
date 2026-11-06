"""File-batch native Unity primitive bridge and source Pull parity requests.

--prepare is filesystem-only. --invoke must be coordinated with the agent
controlling Unity. No generation, source solutions, or acceptance labels here.
"""
from pathlib import Path
import argparse,copy,hashlib,json,math,sys,time
from rotate_rings_physics_v3 import Scene,NativeQueryReplay,ExploratoryPolygonOracle,ColliderRef,pull_axis
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Research/physics_v3/native_oracle'
def read(p):return json.loads(Path(p).read_text(encoding='utf8'))
def atomic_json(path,data):
 path=Path(path);path.parent.mkdir(parents=True,exist_ok=True);tmp=path.with_suffix(path.suffix+'.tmp');tmp.write_text(json.dumps(data,separators=(',',':')),encoding='utf8');tmp.replace(path)
def batch_identity(batch):
 semantic={k:batch.get(k) for k in ['schemaVersion','sourcePackSha256','templatesSha256','microFixtures','queries']};return hashlib.sha256(json.dumps(semantic,sort_keys=True,separators=(',',':')).encode()).hexdigest()
def to_wire(query_id,request):
 args=copy.deepcopy(request.get('args',{}))
 if isinstance(args.get('direction'),(list,tuple)):args['direction']={'x':args['direction'][0],'y':args['direction'][1]}
 if 'other' in args:other=args.pop('other');args.update(otherOwner=other['owner'],otherShapeIndex=other['shapeIndex'])
 return {'queryId':query_id,'kind':request['kind'],'sourceLevelId':str(request['sourceLevelId']),'owner':request['owner'],'bodySourceColliderId':request.get('bodySourceColliderId',0),'poses':[{'id':int(pid),**p} for pid,p in request['poses'].items()],'removed':request.get('removed',[]),'activeBindings':[{'owner':int(pid),'colliderIds':ids} for pid,ids in request.get('activeBindings',{}).items()],'preserveExitOwners':request.get('preserveExitOwners',[]),'args':args}
def from_wire(result,kind):
 if not result.get('ok'):raise RuntimeError(result.get('error','native query failed'))
 if kind=='overlaps':return result.get('overlaps',[])
 if kind=='cast':return result.get('hits',[])
 if kind=='distance':
  d=result['distance'];normal=d['normal'];return {'valid':d['valid'],'distance':d['distance'],'normal':[normal['x'],normal['y']]}
 if kind=='releaseSettlement':return {'pose':result['pose']}
 raise ValueError(kind)
def request(scene,s,pid,kind,**args):
 backend=NativeQueryReplay({})
 try:backend.query(kind,scene,s,pid,**args)
 except Exception as error:
  if hasattr(error,'key'):return to_wire(error.key,error.request)
  raise
def square(radius=.5):return [{'x':-radius,'y':-radius},{'x':radius,'y':-radius},{'x':radius,'y':radius},{'x':-radius,'y':radius}]
def micro_fixtures():
 fixtures=[]
 def shape(kind='PolygonCollider2D',layer=7,trigger=False,role='body',source_role='RingBody'):
  return {'sourceColliderType':kind,'sourceLayer':layer,'sourceEnabled':True,'trigger':trigger,'role':role,'sourceRole':source_role,'points':square(),'paths':[{'points':square()}]}
 for name,other_kind,gap,layer,trigger in [('poly-gap005','PolygonCollider2D',.005,7,False),('box-gap005','BoxCollider2D',.005,7,False),('box-wall','BoxCollider2D',.1,7,False),('wall-layer0','BoxCollider2D',.1,0,False),('wall-trigger','BoxCollider2D',.1,7,True),('initial-overlap','BoxCollider2D',-.1,7,False)]:
  fixtures.append({'id':'micro-'+name,'parts':[{'id':1,'x':0.,'y':0.,'angle':0.,'shapes':[{'sourceColliderId':1001,**shape(source_role='StickBody')}]},{'id':2,'x':1.+gap,'y':0.,'angle':0.,'shapes':[{'sourceColliderId':2001,**shape(other_kind,layer,trigger)}]}]})
 fixtures.append({'id':'micro-bound-wall-hard-stem','parts':[{'id':1,'x':0.,'y':0.,'angle':0.,'shapes':[{'sourceColliderId':1001,**shape(source_role='StickBody')}]},{'id':2,'x':1.2,'y':0.,'angle':0.,'shapes':[{'sourceColliderId':2001,**shape('BoxCollider2D',role='head_wall',source_role='RingRelativeWall')},{'sourceColliderId':2002,**shape('BoxCollider2D',role='stem',source_role='HardBlockPin'),'localPosition':{'x':.1,'y':0.}}]}]})
 for fixture in fixtures:
  for part in fixture['parts']:
   part['sourceRigidbody']={'present':part['id']==1,'sourceComponentId':0,'bodyType':1,'simulated':True,'useFullKinematicContacts':True,'useAutoMass':False,'mass':1.,'linearDamping':0.,'angularDamping':0.,'gravityScale':0.,'physicsMaterial':{'fileId':0,'pathId':0},'includeLayers':0,'excludeLayers':0,'interpolation':0,'sleepMode':1,'collisionDetection':1,'constraints':4}
 return fixtures
def prepare():
 packpath=ROOT/'Research/physics_v3/source_parity_pack.json';tpath=ROOT/'Research/physics_v3/source_templates.json';pack=read(packpath);ts={t['id']:t for t in read(tpath)['templates']};queries=[];explore=ExploratoryPolygonOracle()
 for level in pack['levels']:
  scene=Scene(level,ts);s=scene.new_session(explore)
  for pid in s.active:
   queries.append(request(scene,s,pid,'overlaps',layerMask=128,useTriggers=True))
   axis=pull_axis(scene.template(pid)['movement'],s.poses[pid]['angle'])
   for sign in [-1,1]:
    for distance in [.06,.12,.36]:queries.append(request(scene,s,pid,'cast',direction=[axis[0]*sign,axis[1]*sign],distance=distance,layerMask=128,useTriggers=False))
   for ref in s.configured[pid]:queries.append(request(scene,s,pid,'distance',other={'owner':ref.owner,'shapeIndex':ref.index}))
 micros=micro_fixtures()
 for fixture in micros:
  poses=[{k:p[k] for k in ['id','x','y','angle']} for p in fixture['parts']]
  for kind,args in [('cast',{'direction':{'x':1.,'y':0.},'distance':.36,'layerMask':128,'useTriggers':False}),('overlaps',{'layerMask':128,'useTriggers':True}),('distance',{'otherOwner':2,'otherShapeIndex':0})]:
   q={'kind':kind,'sourceLevelId':fixture['id'],'microFixtureId':fixture['id'],'owner':1,'bodySourceColliderId':1001,'poses':poses,'removed':[],'activeBindings':[],'args':args};q['queryId']=hashlib.sha256(json.dumps(q,sort_keys=True).encode()).hexdigest();queries.append(q)
 batch={'schemaVersion':3,'sourcePackPath':str(packpath),'templatesPath':str(tpath),'sourcePackSha256':hashlib.sha256(packpath.read_bytes()).hexdigest(),'templatesSha256':hashlib.sha256(tpath.read_bytes()).hexdigest(),'meaning':'Native Unity primitive measurements and parity requests, not source solution labels','microFixtures':micros,'queries':queries};atomic_json(OUT/'requests.json',batch);print(json.dumps({'requests':len(queries),'representativeSourcePulls':26,'microFixtures':len(micros),'path':str(OUT/'requests.json')},indent=2));return batch
def invoke(request_path,response_path,method,instance=None):
 from unity_mcp import McpClient,DEFAULT_URL
 type_name,sep,method_name=method.rpartition('.')
 if not sep:raise ValueError('Use Namespace.Class.Method identifier')
 # JSON strings are inserted into C# code as literals, never shell commands.
 code='System.Type type=null; foreach(var a in System.AppDomain.CurrentDomain.GetAssemblies()) { type=a.GetType('+json.dumps(type_name)+'); if(type!=null)break; } if(type==null)throw new System.Exception("Native oracle type missing"); var m=type.GetMethod('+json.dumps(method_name)+',System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static); if(m==null)throw new System.Exception("Native oracle method missing"); return m.Invoke(null,new object[]{'+json.dumps(str(Path(request_path).resolve()))+','+json.dumps(str(Path(response_path).resolve()))+'});'
 client=McpClient(DEFAULT_URL)
 if instance:client.command('call','set_active_instance '+json.dumps({'instance':instance}))
 result=client.command('call','execute_code '+json.dumps({'action':'execute','code':code}))
 if result and result.get('isError'):raise RuntimeError(json.dumps(result))
 if result and result.get('structuredContent',{}).get('success') is False:raise RuntimeError(json.dumps(result))
 if not Path(response_path).exists():raise RuntimeError('Oracle returned without writing response file: '+json.dumps(result))
 response=read(response_path);batch=read(request_path);response['requestBatchSha256']=batch_identity(batch);response['sourcePackSha256']=batch['sourcePackSha256'];response['templatesSha256']=batch['templatesSha256'];atomic_json(response_path,response);return response
def validate_responses(batch,response):
 binding_ok=response.get('requestBatchSha256')==batch_identity(batch)
 results={r['queryId']:r for r in response.get('results',[])};missing=[];errors=[];known_checks=[];measurements=[]
 for q in batch['queries']:
  r=results.get(q['queryId'])
  if r is None:missing.append(q['queryId']);continue
  if not r.get('ok'):errors.append({'id':q['queryId'],'error':r.get('error')});continue
  fixture=q.get('microFixtureId')
  if not fixture:continue
  if q['kind']=='cast' and fixture in ['micro-wall-layer0','micro-wall-trigger']:known_checks.append({'fixture':fixture,'check':'layer/trigger excluded by original cast filter','passed':not r.get('hits')})
  if q['kind']=='cast' and fixture=='micro-box-wall':known_checks.append({'fixture':fixture,'check':'layer7 solid blocks within.36','passed':any(h['owner']==2 for h in r.get('hits',[]))})
  if q['kind']=='distance':measurements.append({'fixture':fixture,'nativeDistance':r['distance']})
  if q['kind']=='cast' and fixture=='micro-bound-wall-hard-stem':
   allowed=[h for h in r.get('hits',[]) if h['sourceColliderId']!=2001];known_checks.append({'fixture':fixture,'check':'filter exact bound wall2001; hardstem2002 must remain','passed':any(h['sourceColliderId']==2002 for h in allowed)})
 report={'queryCount':len(batch['queries']),'nativeResponses':len(results),'missingCount':len(missing),'errors':errors,'inputHashBindingVerified':binding_ok,'requestBatchSha256':batch_identity(batch),'sourcePackSha256':batch['sourcePackSha256'],'templatesSha256':batch['templatesSha256'],'checks':known_checks,'measurements':measurements,'primitiveMeasurementsComplete':binding_ok and not missing and not errors,'knownFilterChecksPassed':binding_ok and bool(known_checks) and all(c['passed'] for c in known_checks),'sourceSolutionsLabeled':0,'originalAndroidRuntimeMeasured':False,'scope':'Unity native primitive query measurements; source behavior contract from decompilation, no original runtime play session'};atomic_json(OUT/'primitive_validation.json',report);return report
def main():
 p=argparse.ArgumentParser();p.add_argument('--prepare',action='store_true');p.add_argument('--invoke',action='store_true');p.add_argument('--method',default='SourceNativeOracle.ExecuteBatch');p.add_argument('--instance');p.add_argument('--requests',type=Path,default=OUT/'requests.json');p.add_argument('--responses',type=Path,default=OUT/'responses.json');a=p.parse_args()
 if a.prepare:
  prepare()
  if not a.invoke:return
 if a.invoke:
  batch=read(a.requests);response=invoke(a.requests,a.responses,a.method,a.instance);report=validate_responses(batch,response);print(json.dumps(report,indent=2));return
 if a.responses.exists():print(json.dumps(validate_responses(read(a.requests),read(a.responses)),indent=2))
if __name__=='__main__':main()
