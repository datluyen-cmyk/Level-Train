"""Export untouched original scenes for parity QA; no generation or solvability labels."""
from pathlib import Path
import argparse,copy,hashlib,json,math,re,sys
import UnityPy
from export_native_physics_v2 import mul,point,rotation
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Research/physics_v3';SOURCE=ROOT/'Research/extracted/assets/yoo/DefaultPackage'
def read(p):return json.loads(p.read_text(encoding='utf8'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def export_source(level,v2):
 raw=read(ROOT/'Research/levels'/f"{level['id']}.json");env=UnityPy.load(str(SOURCE/level['provenance']['bundle']))
 role_names=['Unknown','RingBody','HardBlockPin','RingRelativeBody','RingRelativeWall','StickBody','CatchGuideBody','CatchGuideWall']
 types={'Transform','GameObject','Rigidbody2D','PolygonCollider2D','BoxCollider2D','CircleCollider2D','CapsuleCollider2D'}
 objects={o.path_id:{'type':o.type.name,'tree':o.read_typetree()} for o in env.objects if o.type.name in types}
 ts={i:o['tree'] for i,o in objects.items() if o['type']=='Transform'};tbygo={t['m_GameObject']['m_PathID']:t for t in ts.values()};cache={}
 rb_by_go={o['tree']['m_GameObject']['m_PathID']:(i,o['tree']) for i,o in objects.items() if o['type']=='Rigidbody2D'}
 def material_ref(d):
  m=d.get('m_Material',{'m_FileID':0,'m_PathID':0});return {'fileId':m['m_FileID'],'pathId':m['m_PathID']}
 def world(t):
  go=t['m_GameObject']['m_PathID']
  if go in cache:return cache[go]
  a=rotation(t['m_LocalRotation']);s=t['m_LocalScale'];p=t['m_LocalPosition'];c,sn=math.cos(a),math.sin(a);m=(c*s['x'],sn*s['x'],-sn*s['y'],c*s['y'],p['x'],p['y']);father=ts.get(t['m_Father']['m_PathID'])
  if father:m=mul(world(father),m)
  cache[go]=m;return m
 def descendant(go,rootgo):
  while go in tbygo:
   if go==rootgo:return True
   parent=ts.get(tbygo[go]['m_Father']['m_PathID'])
   if parent is None:break
   go=parent['m_GameObject']['m_PathID']
  return False
 def worldz(goid):
  t=tbygo[goid];parent=ts.get(t['m_Father']['m_PathID']);return t['m_LocalPosition']['z']+(worldz(parent['m_GameObject']['m_PathID']) if parent else 0.)
 markers={c['data']['m_GameObject']['m_PathID']:c['data'] for c in raw['components'] if c['class']=='ColliderRoleMarker'}
 parts=[];templates=[];idmap={p['sourceId']:p['id'] for p in level['state']['parts']+level['state']['fixtures']};v2by={p['id']:p for p in v2['state']['parts']}
 for old in level['state']['parts']+level['state']['fixtures']:
  kind='fixed' if old['kind']=='catch' else old['kind'];native=old['sourceNativeData'];go=native['m_GameObject']['m_PathID'];angle=old['angle'];rad=math.radians(angle);c,sn=math.cos(rad),math.sin(rad);scale=old['sourceWorldScale'][0];bindings=[]
  def local(pt,m):
   x,y=point(m,pt);x-=old['x'];y-=old['y'];return {'x':round(c*x+sn*y,7),'y':round(-sn*x+c*y,7)}
  if kind in ('ring','slider'):
   base=v2by[old['id']];keys=['radius','width','gap','length','visualRadius','visualWidth','visualGap','capRatio','extraEnabled','styleIndex','attachments'];template={k:copy.deepcopy(base[k]) for k in keys};template['kind']=kind;shapes=copy.deepcopy(base['physicalShapes'])
   for index,s in enumerate(shapes):
    source=s.pop('sourceColliderPathId',0);obj=objects[source];g=obj['tree']['m_GameObject']['m_PathID'];feature='body' if s['role']=='body' else 'attachment.'+s.get('sourceAttachmentId','')
    role_name=role_names[markers.get(g,{}).get('role',0)];bindings.append({'sourceColliderId':source,'shapeIndex':index,'sourceFeatureId':feature,'sourceRole':role_name});s.pop('sourceAttachmentId',None);s['physicsMaterial']=material_ref(obj['tree']);s['edgeRadius']=obj['tree'].get('m_EdgeRadius',0.);s['localZ']=worldz(g)-worldz(go);s['sourceRole']=role_name
  else:
   template={'kind':kind,'radius':0.,'width':0.,'gap':0.,'length':0.,'visualRadius':0.,'visualWidth':0.,'visualGap':0.,'capRatio':0.,'extraEnabled':False,'styleIndex':0,'attachments':[]};shapes=[];selected={}
   if kind=='fixed':selected[native['selfCollider']['m_PathID']]=('guide_trigger',old['sourceId'])
   else:
    selected[native['bridgeCollider']['m_PathID']]=('guide_bridge','body')
    for ix,ref in enumerate(native.get('slotColliders',[])):selected[ref['m_PathID']]=('guide_trigger',native['slotGuidePartId'+str(ix)])
   for ref in native.get('blockColliders',[]):selected.setdefault(ref['m_PathID'],('guide_wall','body'))
   for source,(role,feature) in selected.items():
    obj=objects[source];d=obj['tree'];g=d['m_GameObject']['m_PathID'];m=world(tbygo[g]);off=d.get('m_Offset',{'x':0.,'y':0.});typ=obj['type'];shape={'role':role,'attachmentId':-1,'trigger':bool(d['m_IsTrigger']),'sourceColliderType':typ,'sourceLayer':objects[g]['tree']['m_Layer'],'sourceEnabled':bool(d['m_Enabled']),'physicsMaterial':material_ref(d),'edgeRadius':d.get('m_EdgeRadius',0.),'localZ':worldz(g)-worldz(go),'sourceRole':role_names[markers.get(g,{}).get('role',0)]}
    if typ=='PolygonCollider2D':paths=d['m_Points']['m_Paths']
    elif typ=='BoxCollider2D':
     sx,sy=d['m_Size']['x']/2,d['m_Size']['y']/2;paths=[[{'x':x,'y':y} for x,y in [(-sx,-sy),(sx,-sy),(sx,sy),(-sx,sy)]]]
    elif typ=='CircleCollider2D':shape.update(shapeType='circle',center=local(off,m),radius=d['m_Radius']*math.hypot(m[0],m[1]));paths=[]
    else:raise RuntimeError('Do not approximate unsupported native collider '+typ)
    if paths:
     paths=[[local({'x':v['x']+off['x'],'y':v['y']+off['y']},m) for v in path] for path in paths];shape.update(points=paths[0],paths=[{'points':p} for p in paths])
    bindings.append({'sourceColliderId':source,'shapeIndex':len(shapes),'sourceFeatureId':feature,'sourceRole':role_names[markers.get(g,{}).get('role',0)]});shapes.append(shape)
  template['physicalShapes']=shapes
  if kind=='slider':
   render=old['sourceRenderData'];settings=copy.deepcopy(render['_buildSettings'])
   for k in ['width','cornerRadius','exitExtraLength']:settings[k]*=scale
   template['pullVisual']={'pathPoints':[{k:p[k]*scale for k in ['x','y','z']} for p in render['_pathPoints']],'buildSettings':settings,'vertexColor':render['_vertexColor'],'decorations':render['_decorations'],'sourceMaterial':{'fileId':render['_sharedMaterial']['m_FileID'],'pathId':render['_sharedMaterial']['m_PathID']}}
   lt=native['lockDirectionType'];axis={'x':0.,'y':1.} if lt==0 else {'x':-1.,'y':0.} if lt==1 else {'x':0.,'y':0.}
   template['movement']={'lockDirectionType':lt,'axisLocal':axis,**{k:bool(native[k]) for k in ['lockLeft','lockRight','lockUp','lockDown']}}
  render=old['sourceRenderData']
  if kind=='ring':
   ref=render['_sharedMaterial'];template['sourceMaterial']={'fileId':ref['m_FileID'],'pathId':ref['m_PathID']};template['sourceVertexColor']=render['_vertexColor']
   for name in ['arcSubdivisions','gapDirectionDeg','ringZ','stemZ','crossBarZ']:template[name]=render['_'+name]
  visuals=[];quads=[]
  for comp in raw['components']:
   cg=comp['data'].get('m_GameObject',{}).get('m_PathID')
   if comp['class'] not in ['QuadPlaneController','FixedButterflyMeshPart','FixedButterflyMeshController'] or not descendant(cg,go):continue
   # Keep exact source visual component settings and transforms, not fabricated fixtures.
   visuals.append({'class':comp['class'],'gameObject':comp['gameObject'],'sourceData':comp['data'],'sourceTransform':comp['transform']})
   if comp['class']=='FixedButterflyMeshController':continue
   d=comp['data'];m=world(tbygo[cg]);pos=local({'x':0.,'y':0.},m);phi=math.degrees(math.atan2(m[1],m[0]))-angle
   ref=d['sharedMaterial'];quad={'sourceClass':comp['class'],'sourceName':comp['gameObject'],'localPosition':{**pos,'z':0.},'localEuler':{'x':0.,'y':0.,'z':phi},'localScale':{'x':math.hypot(m[0],m[1]),'y':math.hypot(m[2],m[3]),'z':1.},'role':d.get('role',-1),'width':d.get('width',d.get('quadWidth',0.)),'height':d.get('height',d.get('quadHeight',0.)),'zOffset':d.get('zOffset',0.),'capStartLength':d.get('capStartLength',0.),'capEndLength':d.get('capEndLength',0.),'orientation':d.get('orientation',0),'bridgeFacingLocalXSign':d.get('bridgeFacingLocalXSign',0),'localCenter':d.get('localCenter',{'x':0.,'y':0.,'z':0.}),'vertexColor':d['vertexColor'],'sourceMaterial':{'fileId':ref['m_FileID'],'pathId':ref['m_PathID']}}
   # World-space Z is additive in these baked XY scene hierarchies.
   quad['localPosition']['z']=worldz(cg)-worldz(go);quads.append(quad)
  if visuals:template['fixtureVisualSource']=visuals
  if quads:template['catchVisual']={'quads':quads}
  canonical=json.dumps(template,sort_keys=True,separators=(',',':'));tid='native3-'+hashlib.sha256(canonical.encode()).hexdigest()[:20];template['id']=tid;templates.append(template)
  part={k:copy.deepcopy(old[k]) for k in ['id','sourceId','x','y','angle','color','scoreEligible']};part.update(z=worldz(go),kind=kind,templateId=tid,collectible=bool(old['sourcePartData']['CanCollect']),sourceOriginal=True,sourceClass=old['sourceClass'],sourceColliderBindings=bindings,chainLockCount=old['sourcePartData']['LockCount'],sourcePartData=old['sourcePartData'],sourceNativeData=native)
  part['sourceRigidbody']={'present':False}
  if go in rb_by_go:
   rid,rd=rb_by_go[go];part['sourceRigidbody']={'present':True,'sourceComponentId':rid,'bodyType':rd['m_BodyType'],'simulated':rd['m_Simulated'],'useFullKinematicContacts':rd['m_UseFullKinematicContacts'],'useAutoMass':rd['m_UseAutoMass'],'mass':rd['m_Mass'],'linearDamping':rd['m_LinearDamping'],'angularDamping':rd['m_AngularDamping'],'gravityScale':rd['m_GravityScale'],'physicsMaterial':material_ref(rd),'includeLayers':rd['m_IncludeLayers']['m_Bits'],'excludeLayers':rd['m_ExcludeLayers']['m_Bits'],'interpolation':rd['m_Interpolate'],'sleepMode':rd['m_SleepingMode'],'collisionDetection':rd['m_CollisionDetection'],'constraints':rd['m_Constraints']};part['sourceRigidbodyNativeData']=rd
  part['inputShapes']=[]
  if kind in ['ring','slider']:
   input_t=ts.get(native['dragArea']['m_PathID'])
   if input_t is None:
    # Native RingObject/PullObject.Awake resolves a null serialized dragArea
    # through transform.Find("DragArea"); mirror that explicit source fallback.
    input_t=next(ts[ref['m_PathID']] for ref in tbygo[go]['m_Children'] if objects[ts[ref['m_PathID']]['m_GameObject']['m_PathID']]['tree']['m_Name']=='DragArea')
   input_go=input_t['m_GameObject']['m_PathID'];input_m=world(input_t)
   for source,obj in objects.items():
    if not obj['type'].endswith('Collider2D') or obj['tree']['m_GameObject']['m_PathID']!=input_go:continue
    d=obj['tree'];assert obj['type']=='PolygonCollider2D',(level['id'],old['id'],'new input collider class',obj['type']);off=d['m_Offset'];paths=[[local({'x':v['x']+off['x'],'y':v['y']+off['y']},input_m) for v in path] for path in d['m_Points']['m_Paths']]
    part['inputShapes'].append({'sourceColliderId':source,'sourceColliderType':obj['type'],'sourceRole':'InputArea','role':'input_area','attachmentId':-1,'trigger':bool(d['m_IsTrigger']),'sourceLayer':objects[input_go]['tree']['m_Layer'],'sourceEnabled':bool(d['m_Enabled']),'points':paths[0],'paths':[{'points':p} for p in paths],'localZ':worldz(input_go)-worldz(go),'physicsMaterial':material_ref(d),'edgeRadius':0.})
   assert len(part['inputShapes'])==1,(level['id'],old['id'],'expected exact one source input collider')
  if kind=='butterfly':part['slotGuideIds']=[native['slotGuidePartId0'],native['slotGuidePartId1']]
  parts.append(part)
 bombs=[]
 for comp in raw['components']:
  if comp['class']!='BombRelative':continue
  go=comp['data']['m_GameObject']['m_PathID'];owner=next((p for p in level['state']['parts'] if descendant(go,p['sourceNativeData']['m_GameObject']['m_PathID'])),None)
  oid=comp['data']['activeCollider']['m_PathID'];d=objects[oid]['tree'];m=world(tbygo[go]);centre=point(m,d.get('m_Offset',{'x':0.,'y':0.}));position=point(m,{'x':0.,'y':0.});base=owner or {'x':0.,'y':0.,'angle':0.};r=math.radians(base['angle']);dx,dy=centre[0]-base['x'],centre[1]-base['y'];typ=objects[oid]['type']
  if typ!='CircleCollider2D':raise RuntimeError('Unmodeled native bomb collider '+typ)
  shape={'shapeType':'circle','sourceColliderType':typ,'role':'bomb_trigger','trigger':bool(d['m_IsTrigger']),'sourceLayer':objects[d['m_GameObject']['m_PathID']]['tree']['m_Layer'],'sourceEnabled':bool(d['m_Enabled']),'physicsMaterial':material_ref(d),'localZ':worldz(go)-(worldz(owner['sourceNativeData']['m_GameObject']['m_PathID']) if owner else 0.),'sourceRole':'Unknown','center':{'x':math.cos(r)*dx+math.sin(r)*dy,'y':-math.sin(r)*dx+math.cos(r)*dy},'radius':d['m_Radius']*math.hypot(m[0],m[1])}
  bomb={'sourceName':comp['gameObject'],'ownerPartId':owner['id'] if owner else -1,'sourceColliderId':oid,'worldPosition':{'x':position[0],'y':position[1]},'physicalShape':shape,'sourceNativeData':comp['data'],'sourceTransform':comp['transform']}
  # Standalone bomb quads are not descendants of any exported movable part.
  # Keep their exact source quad; pose is in world coordinates when owner=-1.
  quads=[]
  for visual in raw['components']:
   if visual['class']!='QuadPlaneController' or visual['data']['m_GameObject']['m_PathID']!=go:continue
   vd=visual['data'];ref=vd['sharedMaterial'];vm=world(tbygo[go]);dx,dy=position[0]-base['x'],position[1]-base['y']
   quads.append({'sourceClass':'QuadPlaneController','sourceName':visual['gameObject'],'role':-1,'width':vd['width'],'height':vd['height'],'orientation':vd.get('orientation',0),'zOffset':vd.get('zOffset',0.),'capStartLength':vd.get('capStartLength',0.),'capEndLength':vd.get('capEndLength',0.),'bridgeFacingLocalXSign':0,'localCenter':{'x':0.,'y':0.,'z':0.},'localPosition':{'x':math.cos(r)*dx+math.sin(r)*dy,'y':-math.sin(r)*dx+math.cos(r)*dy,'z':worldz(go)-(worldz(owner['sourceNativeData']['m_GameObject']['m_PathID']) if owner else 0.)},'localEuler':{'x':0.,'y':0.,'z':math.degrees(math.atan2(vm[1],vm[0]))-base['angle']},'localScale':{'x':math.hypot(vm[0],vm[1]),'y':math.hypot(vm[2],vm[3]),'z':1.},'vertexColor':vd['vertexColor'],'sourceMaterial':{'fileId':ref['m_FileID'],'pathId':ref['m_PathID']}})
  bomb['catchVisual']={'quads':quads};bombs.append(bomb)
 info=next(c['data'] for c in raw['components'] if c['class']=='GameLevelInfo');links=copy.deepcopy(v2['state']['links'])
 for e in links:
  for key in ['sourceNativeData','bearing','pinWidth']:e.pop(key,None)
 # Resolve exact source feature colliders, retaining original instance IDs separately.
 byid={p['id']:p for p in parts};templ={t['id']:t for t in templates}
 for e in links:
  for side,feature in [('source',e.get('sourceFeature','')),('target',e.get('targetFeature',''))]:
   piece=byid[e[side]]
   if piece['kind']=='slider' and re.fullmatch(r'attachment\.bent_\d+\.solid',feature):
    # Authored logical bent features all belong to the Pull activeCollider.
    # PullToRingLinkRegistrar only resolves the ring endpoint feature; it
    # never subdivides the single native Pull PolygonCollider2D by aliases.
    body=next(b for b in piece['sourceColliderBindings'] if b['sourceFeatureId']=='body')
    body.setdefault('sourceFeatureAliases',[])
    if feature not in body['sourceFeatureAliases']:body['sourceFeatureAliases'].append(feature)
    body['aliasProvenance']='GameLevelInfo.ringLinks logical Pull solid alias; native PullObject.activeCollider'
   matches=[b['shapeIndex'] for b in piece['sourceColliderBindings'] if b['sourceFeatureId']==feature or feature in b.get('sourceFeatureAliases',[])];e[side+'ShapeIndices']=matches
   heads=[templ[piece['templateId']]['physicalShapes'][i].get('attachmentId',-1) for i in matches if templ[piece['templateId']]['physicalShapes'][i]['role']=='head_trigger'];e[side+'AttachmentId']=heads[0] if heads else -1
 guides=[]
 for e in info.get('guideLinks',[]):
  host=e.get('hostPartId') or e['guidePartId'];gid=idmap.get(host,-1);owner=byid.get(gid);indices=[b['shapeIndex'] for b in owner['sourceColliderBindings'] if b['sourceFeatureId']==e['guidePartId']] if owner else [];guides.append({**e,'guideOwnerId':gid,'lockedOwnerId':idmap.get(e['lockedPartId'],-1),'guideShapeIndices':indices})
 for e in links:
  assert e['sourceShapeIndices'] and e['targetShapeIndices'],(level['id'],'unresolved ring feature',e)
 for e in guides:
  assert e['guideOwnerId']>=0 and e['lockedOwnerId']>=0 and e['guideShapeIndices'],(level['id'],'unresolved guide',e)
 return {'id':int(level['id']),'sourceId':level['id'],'sourceFamily':level['group'],'position':level['position'],'sourceOriginal':True,'sourceOnlyParityFixture':True,'parityStatus':'unverified; original layout, not a new level or solvability label','parts':parts,'links':links,'guideLinks':guides,'bombs':bombs,'sourceLevelInfo':info,'provenance':level['provenance']},templates

def main():
 global OUT
 p=argparse.ArgumentParser();p.add_argument('--ids',nargs='+',default=['10001','10010','10012','10154','10109','10166','10190','140004']);p.add_argument('--all',action='store_true');p.add_argument('--output',type=Path);args=p.parse_args()
 if args.output:OUT=args.output.resolve()
 OUT.mkdir(exist_ok=True);original=read(ROOT/'Research/normalized_corpus.json')['levels'];v2={l['id']:l for l in read(ROOT/'Research/physics_v2/corpus500.json')['levels']};levels=[];templates={}
 for l in original:
  if not args.all and l['id'] not in args.ids:continue
  level,ts=export_source(l,v2[l['id']]);levels.append(level)
  for t in ts:templates[t['id']]=t
  print('SOURCE PARITY EXPORTED',l['id'],len(level['parts']),'parts',len(level['guideLinks']),'guides',flush=True)
 assert len(levels)==(len(original) if args.all else len(args.ids))
 for name,data in [('source_parity_pack.json',{'schemaVersion':3,'sourceOriginalOnly':True,'complete':False,'parityStatus':'unverified','levels':levels}),('source_templates.json',{'schemaVersion':3,'sourceOriginalOnly':True,'templates':list(templates.values())})]:
  (OUT/name).write_text(json.dumps(data,separators=(',',':')),encoding='utf8')
 manifest={'sourceCount':len(levels),'templateCount':len(templates),'exporterSha256':sha(Path(__file__)),'inputCorpusSha256':sha(ROOT/'Research/normalized_corpus.json'),'nativeGeometryCorpusSha256':sha(ROOT/'Research/physics_v2/corpus500.json'),'hashes':{name:sha(OUT/name) for name in ['source_parity_pack.json','source_templates.json']},'meaning':'Untouched original layouts for representative shape and mechanics parity QA. Not generated content; no solved labels.'};(OUT/'source_parity_manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf8');print(json.dumps(manifest,indent=2))
if __name__=='__main__':main()
