"""Conservative raw native-geometry seam audit, never a physics solver.

Legitimate source penetrations remain permitted only for that imported pair at
the original relative pose. New solid intersections and new trigger partners
are returned for native review rather than excused by a generic ring rule.
"""
from collections import defaultdict
import itertools,math
from shapely.geometry import Polygon,Point
from shapely import affinity,make_valid,union_all

def shape_polygon(shape):
 if shape.get('shapeType')=='circle':return Point(shape['center']['x'],shape['center']['y']).buffer(shape['radius'],quad_segs=64)
 paths=shape.get('paths',[{'points':shape.get('points',[])}]);rings=[[(p['x'],p['y']) for p in path['points']] for path in paths]
 return make_valid(Polygon(rings[0],rings[1:])) if rings and len(rings[0])>=3 else Polygon()

class NativeGeometrySeamAudit:
 def __init__(self,templates,source_levels):
  self.templates=templates;self.sources={l['sourceId']:l for l in source_levels};self.local={};self.source_cache={}
 def geometry(self,part):
  tid=part['templateId']
  if tid not in self.local:
   shapes=self.templates[tid]['physicalShapes'];self.local[tid]={'solid':union_all([shape_polygon(s) for s in shapes if s.get('sourceEnabled',True) and s.get('sourceLayer',7)==7 and not s.get('trigger',False)]),'sensors':[(i,shape_polygon(s)) for i,s in enumerate(shapes) if s.get('sourceEnabled',True) and s.get('sourceLayer',7)==7 and s.get('trigger',False)]}
  def pose(g):return affinity.translate(affinity.rotate(g,part['angle'],origin=(0,0)),part['x'],part['y'])
  return {'solid':pose(self.local[tid]['solid']),'sensors':[(i,pose(g)) for i,g in self.local[tid]['sensors']]}
 def source_pair(self,source_id,a,b):
  key=(source_id,min(a,b),max(a,b))
  if key not in self.source_cache:
   ps={p['id']:p for p in self.sources[source_id]['parts']};ga,gb=self.geometry(ps[key[1]]),self.geometry(ps[key[2]]);intersection=ga['solid'].intersection(gb['solid']);self.source_cache[key]={'solidOverlapArea':intersection.area,'solidOverlapGeometry':intersection,'sensorContacts':set([(key[1],i,key[2]) for i,g in ga['sensors'] if g.intersects(gb['solid'])]+[(key[2],i,key[1]) for i,g in gb['sensors'] if g.intersects(ga['solid'])])}
  return self.source_cache[key]
 def check(self,level):
  ps={p['id']:p for p in level['parts']};geos={i:self.geometry(p) for i,p in ps.items()};allowed_overlap=defaultdict(float);allowed_patches=defaultdict(list);allowed_sensors=set();pair_provenance=defaultdict(list)
  def transform(g,imp):return affinity.translate(affinity.rotate(g,imp.get('rotation',0.),origin=(0,0)),*imp.get('translation',[0.,0.]))
  for imp in level.get('assemblyProvenance',{}).get('tileImports',[]):
   mapping={int(a):b for a,b in imp['partMapping'].items()}
   for a,b in itertools.combinations(mapping,2):
    na,nb=mapping[a],mapping[b]
    if na==nb:continue
    pair=tuple(sorted((na,nb)));original=self.source_pair(imp['sourceId'],a,b);allowed_overlap[pair]=max(allowed_overlap[pair],original['solidOverlapArea']);allowed_patches[pair].append(transform(original['solidOverlapGeometry'],imp));pair_provenance[pair].append(imp['sourceId'])
    allowed_sensors.update((mapping[owner],shape,mapping[other]) for owner,shape,other in original['sensorContacts'])
  for imp in level.get('assemblyProvenance',{}).get('featureBoundaryImports',[]):
   source=self.sources[imp['sourceId']];sourceparts={p['id']:p for p in source['parts']};interior={int(a):b for a,b in imp['interiorPartMapping'].items()};witnesses={w['sourcePartId']:w for w in imp['boundaryWitnesses']};mapping={**interior,**{old:w['targetPartId'] for old,w in witnesses.items()}}
   # Complete imported interior pairs retain their exact original contacts.
   for a,b in itertools.combinations(interior,2):
    pair=tuple(sorted((mapping[a],mapping[b])));original=self.source_pair(imp['sourceId'],a,b);allowed_patches[pair].append(transform(original['solidOverlapGeometry'],imp));pair_provenance[pair].append(imp['sourceId']);allowed_sensors.update((mapping[owner],shape,mapping[other]) for owner,shape,other in original['sensorContacts'])
   # Feature-boundary allowance is restricted to one specifically certified
   # authored edge's complete feature geometry. Other donor owner pins cannot
   # excuse a new current-scene collision.
   allowed_edge_indices=set(i for w in witnesses.values() for i in w['relationIndices'])
   for edge_index in allowed_edge_indices:
    e=source['links'][edge_index];a,b=e['source'],e['target']
    if a not in mapping or b not in mapping:continue
    if a not in interior and b not in interior:continue
    world={};indexmaps={}
    for old,side in [(a,'source'),(b,'target')]:
     p=sourceparts[old];template=self.templates[p['templateId']];indices=set(range(len(template['physicalShapes']))) if old in interior else set(witnesses[old]['sourceShapeIndices']);solid=[];sensors=[];indexmap=dict(zip(witnesses[old]['sourceShapeIndices'],witnesses[old]['targetShapeIndices'])) if old in witnesses else {i:i for i in indices}
     for i in indices:
      s=template['physicalShapes'][i]
      if not s.get('sourceEnabled',True) or s.get('sourceLayer',7)!=7:continue
      g=affinity.translate(affinity.rotate(shape_polygon(s),p['angle'],origin=(0,0)),p['x'],p['y'])
      if s.get('trigger',False):sensors.append((i,g))
      else:solid.append(g)
     world[old]={'solid':union_all(solid),'sensors':sensors};indexmaps[old]=indexmap
    pair=tuple(sorted((mapping[a],mapping[b])));allowed_patches[pair].append(transform(world[a]['solid'].intersection(world[b]['solid']),imp));pair_provenance[pair].append(imp['sourceId']+':certifiedFeatureEdge'+str(edge_index))
    for owner,other in [(a,b),(b,a)]:
     for i,g in world[owner]['sensors']:
      if g.intersects(world[other]['solid']):allowed_sensors.add((mapping[owner],indexmaps[owner][i],mapping[other]))
   # Exact native physical-composition boundary pairs are independent of pin
   # links. Only the certified complete boundary body participates; its other
   # attachments cannot excuse a new obstacle. Independent integrity must also
   # ground this pair in the saved source strong composition evidence.
   for binding in imp.get('compositionBoundaryPairs',[]):
    a,b=binding['interiorSourcePartId'],binding['boundarySourcePartId']
    if a not in interior or b not in witnesses:continue
    boundary=sourceparts[b];template=self.templates[boundary['templateId']];indices=[i for i in witnesses[b]['sourceShapeIndices'] if template['physicalShapes'][i]['role']=='body'];body=union_all([shape_polygon(template['physicalShapes'][i]) for i in indices]);body=affinity.translate(affinity.rotate(body,boundary['angle'],origin=(0,0)),boundary['x'],boundary['y']);whole=self.geometry(sourceparts[a]);pair=tuple(sorted((mapping[a],mapping[b])));allowed_patches[pair].append(transform(body.intersection(whole['solid']),imp));pair_provenance[pair].append(imp['sourceId']+':certifiedNativeCompositionBodyPair')
    for shape,g in whole['sensors']:
     if g.intersects(body):allowed_sensors.add((mapping[a],shape,mapping[b]))
  retargeted=level.get('assemblyProvenance',{}).get('retargetedAssetImports',[]);learned_certificate=level.get('assemblyProvenance',{}).get('learnedPlacementCertificate',{})
  if retargeted:
   origin={new:(imp['sourceId'],int(old)) for imp in retargeted for old,new in imp['partMapping'].items()}
   # Only intact native guide-connected groups can retain source contact
   # allowances. Newly optimized ring relations get no source-pair overlap
   # exemption. Verify the complete group's relative rigid transform here.
   for group in learned_certificate.get('guideRigidGroups',[]):
    if len(group)<2 or any(i not in origin for i in group):continue
    source_ids={origin[i][0] for i in group}
    if len(source_ids)!=1:continue
    sid=next(iter(source_ids));sourceparts={p['id']:p for p in self.sources[sid]['parts']};first=group[0];original=sourceparts[origin[first][1]];rotation=ps[first]['angle']-original['angle'];r=math.radians(rotation);c,s=math.cos(r),math.sin(r);translation=[ps[first]['x']-(c*original['x']-s*original['y']),ps[first]['y']-(s*original['x']+c*original['y'])];valid=True
    for new in group:
     old=sourceparts[origin[new][1]];x=c*old['x']-s*old['y']+translation[0];y=s*old['x']+c*old['y']+translation[1]
     if math.hypot(x-ps[new]['x'],y-ps[new]['y'])>1e-4 or abs((ps[new]['angle']-old['angle']-rotation+180)%360-180)>.005:valid=False;break
    if not valid:continue
    imp={'rotation':rotation,'translation':translation}
    for a,b in itertools.combinations(group,2):
     olda,oldb=origin[a][1],origin[b][1];original=self.source_pair(sid,olda,oldb);pair=tuple(sorted((a,b)));allowed_patches[pair].append(transform(original['solidOverlapGeometry'],imp));pair_provenance[pair].append(sid+':intactNativeGuideGroup');reverse={origin[i][1]:i for i in group};allowed_sensors.update((reverse[owner],shape,reverse[other]) for owner,shape,other in original['sensorContacts'])
   for edge in level.get('assemblyProvenance',{}).get('learnedRelationPlacements',[]):
    owner,other=edge['actualSource'],edge['actualTarget'];template=self.templates[ps[owner]['templateId']]
    for shape in edge['actualSourceShapeIndices']:
     if template['physicalShapes'][shape].get('trigger',False):allowed_sensors.add((owner,shape,other))
  # A full-native asset substitution can keep a reference body exactly while
  # changing pins. Preserve only that original body's source-pair patches and
  # source framework sensors; newly imported pins receive no body exemption.
  substitutions=level.get('assemblyProvenance',{}).get('bodyScaffoldSubstitutions',[])
  for sub in substitutions:
   sid=sub['sourceId'];old=sub['sourcePartId'];new=sub['actualPartId'];sourceparts={p['id']:p for p in self.sources[sid]['parts']};original=sourceparts[old];template=self.templates[original['templateId']];body=union_all([shape_polygon(template['physicalShapes'][i]) for i in sub['sourceBodyShapeIndices']]);body=affinity.translate(affinity.rotate(body,original['angle'],origin=(0,0)),original['x'],original['y'])
   for imp in level.get('assemblyProvenance',{}).get('tileImports',[]):
    if imp['sourceId']!=sid:continue
    # This certificate is specifically the untransformed reference scaffold.
    if abs(imp.get('rotation',0.))>1e-5 or math.hypot(*imp.get('translation',[0.,0.]))>1e-5:continue
    for other,actual_other in ((int(a),b) for a,b in imp['partMapping'].items()):
     if actual_other==new:continue
     whole=self.geometry(sourceparts[other]);pair=tuple(sorted((new,actual_other)));allowed_patches[pair].append(body.intersection(whole['solid']));pair_provenance[pair].append(sid+':certifiedUnchangedScaffoldBody')
     for shape,g in whole['sensors']:
      if g.intersects(body):allowed_sensors.add((actual_other,shape,new))
  unexpected=[];sensors=[];duplicates=[];missing_features=[];pair_count=0
  for a,b in itertools.combinations(ps,2):
   pa,pb=ps[a],ps[b];unit=max(.01,self.templates[pa['templateId']].get('width',0),self.templates[pb['templateId']].get('width',0));tol=1e-5*unit*unit;ga,gb=geos[a],geos[b];pair=tuple(sorted((a,b)));area=ga['solid'].intersection(gb['solid']).area;pair_count+=1
   patches=union_all(allowed_patches[pair]);excess=ga['solid'].intersection(gb['solid']).difference(patches.buffer(1e-6) if not patches.is_empty else patches).area
   if excess>tol:unexpected.append({'owners':[a,b],'rawOverlapArea':area,'allowedImportedSourceArea':patches.area,'excessWidthSquared':excess/(unit*unit),'sourcePairEvidence':pair_provenance[pair]})
   if pa['templateId']==pb['templateId'] and math.hypot(pa['x']-pb['x'],pa['y']-pb['y'])<1e-4 and abs((pa['angle']-pb['angle']+180)%360-180)<.005:duplicates.append([a,b])
   for owner,other,go,gother in [(a,b,ga,gb),(b,a,gb,ga)]:
    for shape,g in go['sensors']:
     if g.intersection(gother['solid']).area>tol and (owner,shape,other) not in allowed_sensors:sensors.append({'owner':owner,'shapeIndex':shape,'otherOwner':other,'role':self.templates[ps[owner]['templateId']]['physicalShapes'][shape]['role']})
  for e in level['links']:
   for side in ['source','target']:
    owner=e[side];indices=e[side+'ShapeIndices'];n=len(self.templates[ps[owner]['templateId']]['physicalShapes'])
    if not indices or any(i<0 or i>=n for i in indices):missing_features.append({'owner':owner,'side':side,'indices':indices})
  unexpected_bombs=[];unmapped_bombs=[]
  for bomb in level.get('bombs',[]):
   prov=bomb.get('atomicProvenance',{});source_id=prov.get('sourceId');source_bomb=next((b for b in self.sources.get(source_id,{}).get('bombs',[]) if b['sourceColliderId']==prov.get('sourceColliderId')),None)
   if source_bomb is None:unmapped_bombs.append(bomb.get('sourceColliderId'));continue
   def bomb_geometry(b,parts):
    g=shape_polygon(b['physicalShape']);owner=b['ownerPartId']
    if owner>=0:
     p=parts[owner];g=affinity.translate(affinity.rotate(g,p['angle'],origin=(0,0)),p['x'],p['y'])
    return g
   current=bomb_geometry(bomb,ps);source_ps={p['id']:p for p in self.sources[source_id]['parts']};original=bomb_geometry(source_bomb,source_ps);allowed=defaultdict(list)
   imports=level.get('assemblyProvenance',{}).get('tileImports',[])+[{**i,'partMapping':i['interiorPartMapping']} for i in level.get('assemblyProvenance',{}).get('featureBoundaryImports',[])]
   original_owner=source_bomb['ownerPartId'];anchor=original_owner if original_owner>=0 else min(source_ps,key=lambda i:math.dist([source_ps[i]['x'],source_ps[i]['y']],list(source_bomb['worldPosition'].values())))
   for imp in imports:
    if imp['sourceId']!=source_id:continue
    mapping={int(old):new for old,new in imp['partMapping'].items()}
    if anchor not in mapping:continue
    # A same-area contact at a different world location is not source evidence.
    posed_original=transform(original,imp)
    if posed_original.symmetric_difference(current).area>1e-6:continue
    for old,new in mapping.items():allowed[new].append(transform(original.intersection(self.geometry(source_ps[old])['solid']),imp))
   for owner,part in ps.items():
    if owner==bomb['ownerPartId']:continue
    unit=max(.01,self.templates[part['templateId']].get('width',0));intersection=current.intersection(geos[owner]['solid']);patch=union_all(allowed[owner]);area=intersection.area;excess=intersection.difference(patch.buffer(1e-6) if not patch.is_empty else patch).area
    if excess>1e-5*unit*unit:unexpected_bombs.append({'bombColliderId':bomb['sourceColliderId'],'otherOwner':owner,'rawOverlapArea':area,'allowedSourceOverlapArea':patch.area,'newWorldOverlapArea':excess})
  return {'rawGeometrySeamPassed':not(unexpected or sensors or duplicates or missing_features or unexpected_bombs or unmapped_bombs),'partPairCount':pair_count,'unexpectedSolidOverlapCount':len(unexpected),'unexpectedSolidOverlaps':sorted(unexpected,key=lambda r:-r['excessWidthSquared']),'unregisteredSensorPartnerCount':len(sensors),'unregisteredSensorPartners':sensors,'duplicateWholeRootPairs':duplicates,'missingFeatureBindings':missing_features,'newBombInitialOverlapCount':len(unexpected_bombs),'newBombInitialOverlaps':unexpected_bombs,'unmappedBombs':unmapped_bombs,'meaning':'Raw polygon/trigger seam screening only, source penetrations calibrated pairwise. Bomb circles approximated with256segments solely for seam screening (maximum radial error radius*(1-cos(pi/256))); actual exported native bomb stays CircleCollider2D. Unity collider skin, casts, bindings and playability remain unverified.'}
