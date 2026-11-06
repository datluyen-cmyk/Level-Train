"""Whole native asset variants on an unchanged native body scaffold.

Meaningful attachment/dependency changes are proposed and reported, never
accepted merely because they share an original board's body arrangement.
"""
from pathlib import Path
from collections import Counter,defaultdict
import argparse,copy,json,math
import joblib,numpy as np
from shapely import affinity,union_all
from assembly_contact_audit_v3 import NativeGeometrySeamAudit,shape_polygon
from native_feature_socket_v3 import body_key as native_body_key,feature_shape_map,shape_key
BODY_KEYS={}
def body_key(p,templates):
 key=p['templateId']
 if key not in BODY_KEYS:BODY_KEYS[key]=native_body_key(p,templates)
 return BODY_KEYS[key]
from macro_assembly_v3 import socket_clean
from layout_learning_v3 import read,hashfile,relation_rows,frame

def pin_key(p,templates):
 return tuple(shape_key(s) for s in templates[p['templateId']]['physicalShapes'] if s['role']!='body')
def world(g,p):return affinity.translate(affinity.rotate(g,p['angle'],origin=(0,0)),p['x'],p['y'])
def special_root(p):return p.get('chainLockCount',0)>0 or bool(p.get('key')) or bool(p.get('lockKey'))
def state(p):return tuple(json.dumps(p.get(k),sort_keys=True) for k in ['kind','z','chainLockCount','collectible','scoreEligible','key','lockKey'])
def clone_root(donor,sid,actual_id,pose):
 p=copy.deepcopy(donor);old=p['id'];p.update(id=actual_id,x=pose['x'],y=pose['y'],angle=pose['angle'],sourceId='variant:'+str(actual_id),sourceOriginal=False,atomicProvenance={'sourceId':sid,'partId':old,'templateId':p['templateId']})
 for k in ['sourceNativeData','sourceRigidbodyNativeData','sourcePartData']:p.pop(k,None)
 for b in p['sourceColliderBindings']:b['sourceColliderId']=actual_id*1000+b['shapeIndex']+1
 for i,s in enumerate(p['inputShapes']):s['sourceColliderId']=actual_id*1000+900+i
 if p['sourceRigidbody'].get('present'):p['sourceRigidbody']['sourceComponentId']=actual_id*1000+990
 return p

def prepare(levels,templates):
 vocab=defaultdict(list);by={l['sourceId']:l for l in levels};shapes={};source_edges={}
 for l in levels:
  guide={i for e in l['guideLinks'] for i in [e['guideOwnerId'],e['lockedOwnerId']]};bombowners={b['ownerPartId'] for b in l['bombs']};ps={p['id']:p for p in l['parts']}
  for p in l['parts']:
   if p['id'] in guide or p['id'] in bombowners or special_root(p):continue
   outgoing=[(i,e) for i,e in enumerate(l['links']) if e['source']==p['id']];vocab[(body_key(p,templates),state(p))].append((l['sourceId'],p['id'],outgoing))
 for t in templates.values():
  shapes[t['id']]={'solid':union_all([shape_polygon(s) for s in t['physicalShapes'] if not s['trigger'] and s.get('sourceEnabled',True) and s.get('sourceLayer',7)==7]),'body':union_all([shape_polygon(s) for s in t['physicalShapes'] if s['role']=='body']),'shapes':[shape_polygon(s) for s in t['physicalShapes']]}
 return vocab,by,shapes

def options(reference,templates,vocab,sources,local,limit=80,rotate_from_patterns=False,model=None):
 ps={p['id']:p for p in reference['parts']};bodykeys={i:body_key(p,templates) for i,p in ps.items()};worldsolids={i:world(local[p['templateId']]['solid'],p) for i,p in ps.items()};worldbodies={i:world(local[p['templateId']]['body'],p) for i,p in ps.items()};guide={i for e in reference['guideLinks'] for i in [e['guideOwnerId'],e['lockedOwnerId']]};bombowners={b['ownerPartId'] for b in reference['bombs']};result={};reject=Counter()
 for target_id,target in ps.items():
  if target_id in guide or target_id in bombowners or special_root(target):continue
  oldpin=pin_key(target,templates);oldout={e['target'] for e in reference['links'] if e['source']==target_id};seen=set();choices=[]
  for sid,did,outgoing in vocab.get((bodykeys[target_id],state(target)),[]):
   if sid==reference['sourceId']:continue
   donor=next(p for p in sources[sid]['parts'] if p['id']==did);pk=pin_key(donor,templates);donorps={p['id']:p for p in sources[sid]['parts']};dt=templates[donor['templateId']]
   active={e['sourceAttachmentId'] for _,e in outgoing};nativepins={q['attachmentId'] for q in dt['physicalShapes'] if q['role']=='head_trigger'}
   if not nativepins.issubset(active):reject['unusedNativeDonorPin']+=1;continue
   if abs(len(outgoing)-len(oldout))>1:continue
   angles=[target['angle']]
   if rotate_from_patterns:
    angles=[]
    for _,e in outgoing:
     original_target=donorps[e['target']];dx=original_target['x']-donor['x'];dy=original_target['y']-donor['y'];spacing=math.hypot(dx,dy)
     if spacing<1e-5:continue
     for j,q in ps.items():
      if j==target_id or bodykeys[j]!=body_key(original_target,templates):continue
      ax=q['x']-target['x'];ay=q['y']-target['y'];actualspacing=math.hypot(ax,ay)
      if not .75<=actualspacing/spacing<=1.25:continue
      angle=donor['angle']+math.degrees(math.atan2(ay,ax)-math.atan2(dy,dx));delta=(angle-target['angle']+180)%360-180
      if abs(delta)>=45:angles.append(target['angle']+delta)
    if not outgoing and model is not None:
     probe={**reference,'parts':[clone_root(donor,sid,target_id,target) if q['id']==target_id else q for q in reference['parts']],'links':[e for e in reference['links'] if e['source']!=target_id]};incoming=[r for r in relation_rows(probe,templates) if r['relation']=='pin' and r['targetPart']==target_id]
     for row in incoming:
      _,indices=model.neighbors([row['context']]);parent=ps[row['sourcePart']]
      for record in indices[0]:
       y=model.y[record];angle=parent['angle']+math.degrees(math.atan2(y[3],y[2]));delta=(angle-target['angle']+180)%360-180
       if abs(delta)>=45:angles.append(target['angle']+delta)
    angles=sorted({round(a,6) for a in angles})
   for angle in angles:
    rotated=abs((angle-target['angle']+180)%360-180)>1e-5;identity=(pk,round(angle%360,5))
    if identity in seen or (not rotated and pk==oldpin):continue
    seen.add(identity);pose=clone_root(donor,sid,target_id,{**target,'angle':angle});mapped={};edges=[];possible=True
    for index,e in outgoing:
     oldtarget=e['target'];pattern_target=donorps[oldtarget];key=body_key(pattern_target,templates);head=world(union_all([local[donor['templateId']]['shapes'][i] for i in e['sourceShapeIndices'] if dt['physicalShapes'][i]['role']=='head_trigger']),pose);targets=[]
     for j,q in ps.items():
      if j==target_id or bodykeys[j]!=key or (oldtarget in mapped and mapped[oldtarget]!=j):continue
      body=worldbodies[j]
      if head.distance(body)>1e-5:continue
      indexmap=feature_shape_map(templates[pattern_target['templateId']],templates[q['templateId']],e['targetShapeIndices'])
      if indexmap is not None:targets.append((j,indexmap,float(head.intersection(body).area)))
     if not targets:possible=False;break
     j,im,_=max(targets,key=lambda q:(q[0] not in oldout,q[2]));mapped[oldtarget]=j;edge=copy.deepcopy(e);edge['source']=target_id;edge['target']=j;edge['targetShapeIndices']=[im[i] for i in e['targetShapeIndices']];edge['targetAttachmentId']=-1;edges.append({'edge':edge,'sourcePatternSourceId':sid,'sourcePatternEdgeIndex':index,'targetOriginalId':oldtarget})
    if not possible:reject['noActualHeadContactForCompleteSourcePattern']+=1;continue
    newout={e['edge']['target'] for e in edges}
    if newout==oldout and not rotated:reject['noMechanicalPoseOrBranchChange']+=1;continue
    solid=world(local[pose['templateId']]['solid'],pose);oldbody=world(local[target['templateId']]['body'],target);debt=0.
    for j,other in worldsolids.items():
     if j==target_id:continue
     overlap=solid.intersection(other);debt+=overlap.area if rotated else overlap.difference(oldbody.intersection(other).buffer(1e-6)).area
    if debt>1e-5:reject['newWholeAssetSolidCrossesNativeFramework']+=1;continue
    pose_nll=None
    if model is not None:
     probe={**reference,'parts':[pose if q['id']==target_id else q for q in reference['parts']],'links':[e for e in reference['links'] if e['source']!=target_id]+[e['edge'] for e in edges]};learned_rows=[r for r in relation_rows(probe,templates) if r['relation']=='pin' and target_id in [r['sourcePart'],r['targetPart']]]
     if learned_rows:pose_nll=float(-model.loglikelihood([r['context'] for r in learned_rows],[r['pose'] for r in learned_rows]).mean())
    choices.append({'conditionalPoseNLL':pose_nll,'sourceId':sid,'donorId':did,'part':pose,'edges':edges,'dependencySymmetricDifference':len(oldout^newout),'oldTargets':sorted(oldout),'newTargets':sorted(newout),'bodyPreserved':not rotated,'angleDeltaDegrees':float((angle-target['angle']+180)%360-180),'angleSource':('saved conditional native pin-to-receiver relative angle component' if not outgoing else 'source authored relative center vector aligned to actual native recipient vector') if rotate_from_patterns else 'unchanged source scaffold'})
  print('ROOT_VARIANTS',reference['position'],target_id,len(choices),flush=True)
  if choices:
   ranked=sorted(choices,key=lambda q:(q['conditionalPoseNLL'] if q['conditionalPoseNLL'] is not None else 0.,-q['dependencySymmetricDifference'],-abs(q['angleDeltaDegrees']),q['sourceId']));buckets=Counter();selected=[]
   for c in ranked:
    key=(round(c['part']['angle']/15),tuple(c['newTargets']))
    if buckets[key]>=4:continue
    buckets[key]+=1;selected.append(c)
    if len(selected)>=limit:break
   result[target_id]=selected
 return result,reject

def candidate(reference,options,templates,sources,local,model,packhash,modelhash,number,attempt,selected_override=None):
 ps={p['id']:p for p in reference['parts']};out=copy.deepcopy(reference);out.update(id=number,sourceId='native-variant-'+str(number),sourceOriginal=False,complete=False,validationStatus='INCOMPLETE_NATIVE_BODY_SCAFFOLD_VARIANT');parts={p['id']:p for p in out['parts']};available=sorted(options,key=lambda i:(-len(options[i]),i));chosen={};donors=set();desired=max(1,math.ceil(len(ps)*.25))
 for i in available:
  choices=options[i];choice=next((choices[(attempt+k)%len(choices)] for k in range(len(choices)) if choices[(attempt+k)%len(choices)]['sourceId'] not in donors),None)
  if choice is None:continue
  chosen[i]=choice;donors.add(choice['sourceId'])
  if len(chosen)>=desired:break
 if selected_override is not None:chosen=selected_override
 if not chosen:return None
 for i,p in parts.items():
  p['atomicProvenance']={'sourceId':reference['sourceId'],'partId':i,'templateId':p['templateId']}
  for k in ['sourceNativeData','sourceRigidbodyNativeData','sourcePartData']:p.pop(k,None)
 for i,c in chosen.items():parts[i]=copy.deepcopy(c['part'])
 out['parts']=list(parts.values());imports=[{'sourceId':reference['sourceId'],'partMapping':{i:i for i in ps if i not in chosen},'rotation':0.,'translation':[0.,0.]}];retarget=[{'sourceId':c['sourceId'],'partMapping':{c['donorId']:i}} for i,c in chosen.items()];boundary={};claims=[];all_edges=[];substitutions=[]
 def witness(sid,old,new,required):
  sourcepart=next(p for p in sources[sid]['parts'] if p['id']==old);st=templates[sourcepart['templateId']];tt=templates[parts[new]['templateId']];body=[i for i,s in enumerate(st['physicalShapes']) if s['role']=='body'];bodymap=feature_shape_map(st,tt,body);shape_map=feature_shape_map(st,tt,required)
  if bodymap is None or shape_map is None:raise ValueError('Unsupported native body/feature mapping')
  key=(sid,old);prior=boundary.get(key)
  if prior is not None and prior['targetPartId']!=new:raise ValueError('Conflicting whole-source pattern correspondence')
  if prior is None:prior={'kind':'pin','sourcePartId':old,'targetPartId':new,'sourceShapeIndices':[],'targetShapeIndices':[],'sourceBodyShapeIndices':body,'targetBodyShapeIndices':[bodymap[i] for i in body],'relationIndices':[]};boundary[key]=prior
  oldmap=dict(zip(prior['sourceShapeIndices'],prior['targetShapeIndices']));oldmap.update(shape_map);prior['sourceShapeIndices']=sorted(oldmap);prior['targetShapeIndices']=[oldmap[i] for i in prior['sourceShapeIndices']]
  return shape_map
 for index,e in enumerate(reference['links']):
  if e['source'] in chosen:continue
  edge=copy.deepcopy(e)
  if e['target'] in chosen:
   witness(reference['sourceId'],e['source'],e['source'],e['sourceShapeIndices']);mapping=witness(reference['sourceId'],e['target'],e['target'],e['targetShapeIndices']);edge['targetShapeIndices']=[mapping[i] for i in e['targetShapeIndices']];claims.append({'sourceRelationSourceId':reference['sourceId'],'sourceRelationIndex':index,'actualSource':edge['source'],'actualTarget':edge['target'],'actualSourceShapeIndices':edge['sourceShapeIndices'],'actualTargetShapeIndices':edge['targetShapeIndices']})
  all_edges.append(edge)
 for i,c in chosen.items():
  for row in c['edges']:
   edge=row['edge'];sid=row['sourcePatternSourceId'];old=row['targetOriginalId'];witness(sid,old,edge['target'],sources[sid]['links'][row['sourcePatternEdgeIndex']]['targetShapeIndices']);all_edges.append(edge);claims.append({'sourceRelationSourceId':sid,'sourceRelationIndex':row['sourcePatternEdgeIndex'],'actualSource':i,'actualTarget':edge['target'],'actualSourceShapeIndices':edge['sourceShapeIndices'],'actualTargetShapeIndices':edge['targetShapeIndices']})
  st=templates[ps[i]['templateId']];tt=templates[parts[i]['templateId']];indices=[j for j,s in enumerate(st['physicalShapes']) if s['role']=='body'];bm=feature_shape_map(st,tt,indices);
  if c['bodyPreserved']:substitutions.append({'sourceId':reference['sourceId'],'sourcePartId':i,'actualPartId':i,'sourceBodyShapeIndices':indices,'targetBodyShapeIndices':[bm[j] for j in indices]})
 out['links']=all_edges
 for e in all_edges:
  if e['source'] not in chosen and e['target'] not in chosen:continue
  a,b=parts[e['source']],parts[e['target']];st=templates[a['templateId']];head=world(union_all([local[a['templateId']]['shapes'][i] for i in e['sourceShapeIndices'] if st['physicalShapes'][i]['role']=='head_trigger']),a);body=world(union_all([local[b['templateId']]['shapes'][i] for i in e['targetShapeIndices']]),b)
  if head.distance(body)>1e-5:raise ValueError('Changed native pose disconnects an actual declared pin head/body contact')
 for b in out['bombs']:b['atomicProvenance']={'sourceId':reference['sourceId'],'sourceColliderId':b['sourceColliderId']}
 measurements=[];rows={(r['sourcePart'],r['targetPart']):r for r in relation_rows(out,templates) if r['relation']=='pin'}
 for claim in claims:
  row=rows[(claim['actualSource'],claim['actualTarget'])];distance,indices=model.neighbors([row['context']]);pose=np.asarray(row['pose']);errors=np.linalg.norm(model.y[indices[0]]-pose,axis=1);best=int(np.argmin(errors));record=int(indices[0][best]);claim.update(learnedRecordIndex=record,modelSha256=modelhash);measurements.append({'sourcePart':claim['actualSource'],'targetPart':claim['actualTarget'],'relation':'pin','learnedRelationRecordIndex':record,'learnedPose':model.y[record].tolist(),'actualPose':pose.tolist(),'poseResidualL2':float(errors[best])})
 bounds=[]
 for sid in sorted({sid for sid,_ in boundary}):bounds.append({'sourceId':sid,'boundaryWitnesses':[b for (source,_),b in boundary.items() if source==sid]})
 oldedges={(e['source'],e['target']) for e in reference['links']};newedges={(e['source'],e['target']) for e in out['links']};out['assemblyProvenance']={'method':'whole native asset variants at native source framework centers; source-pattern angle/dependency proposals, no acceptance','tileImports':imports,'retargetedAssetImports':retarget,'bodyScaffoldSubstitutions':substitutions,'learnedBoundaryFeatureWitnesses':bounds,'learnedRelationPlacements':claims,'learnedPlacementCertificate':{'sourcePackSha256':packhash,'localPoseModelSha256':modelhash,'relationWitnesses':measurements,'guideRigidGroups':[[i] for i in chosen],'limits':'Raw geometry/active pattern candidates only, shared body scaffold flagged nearcopy; no source difficulty or replay labels'},'decisions':[{'kind':'nativeBodyScaffoldAssetVariant','modifiedRootIds':list(chosen),'changedRootFraction':len(chosen)/len(ps),'authoredDependencySymmetricDifference':len(oldedges^newedges),'oldAuthoredEdgeCount':len(oldedges),'newAuthoredEdgeCount':len(newedges),'unchangedSourceBodyPoses':all(c['bodyPreserved'] for c in chosen.values()),'spatialNearCopyExpected':all(c['bodyPreserved'] for c in chosen.values()),'changedAttachmentAndDependencyEvidence':[{'partId':i,'sourceId':c['sourceId'],'donorPartId':c['donorId'],'oldTargets':c['oldTargets'],'newTargets':c['newTargets'],'angleDeltaDegrees':c['angleDeltaDegrees'],'angleSource':c['angleSource'],'conditionalPoseNLL':c['conditionalPoseNLL']} for i,c in chosen.items()]}]};return out

def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',required=True,type=Path);p.add_argument('--positions',nargs='+',type=int,default=[26,51,76,126,176,251]);p.add_argument('--attempts',type=int,default=4);p.add_argument('--rotate-from-patterns',action='store_true');p.add_argument('--output',required=True,type=Path);a=p.parse_args();w=a.workspace;levels=read(w/'data/source500_native.json')['levels'];ts={t['id']:t for t in read(w/'data/source_templates.json')['templates']};vocab,sources,local=prepare(levels,ts);modelpath=w/'models/composition_pose.joblib';model=joblib.load(modelpath)['model'];out=[];coverage=[];audit=NativeGeometrySeamAudit(ts,levels)
 for position in a.positions:
  reference=next(l for l in levels if l['position']==position);opts,reject=options(reference,ts,vocab,sources,local,rotate_from_patterns=a.rotate_from_patterns,model=model);coverage.append({'position':position,'referenceSourceId':reference['sourceId'],'nativeRootCount':len(reference['parts']),'rootsWithActiveNovelVariant':len(opts),'variantOptions':sum(map(len,opts.values())),'rejections':dict(reject)});print('VARIANTCOVERAGE',coverage[-1],flush=True)
  for attempt in range(a.attempts):
   try:c=candidate(reference,opts,ts,sources,local,model,hashfile(w/'data/source500_native.json'),hashfile(modelpath),6000001+len(out),attempt)
   except ValueError as e:print('CERTIFICATE_REJECT',str(e),flush=True);continue
   if c is None:continue
   c['preliminaryRawSeam']=audit.check(c);out.append(c);print('VARIANT',c['id'],position,c['assemblyProvenance']['decisions'][0]['modifiedRootIds'],'raw',c['preliminaryRawSeam']['rawGeometrySeamPassed'],flush=True)
 a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps({'schemaVersion':3,'complete':False,'acceptedCount':0,'status':'WHOLE_NATIVE_ASSET_VARIANT_PILOT_REQUIRES_INDEPENDENT_INTEGRITY_RAW_REPLAY_DIFFICULTY_AND_NOVELTY_REVIEW','coverage':coverage,'levels':out,'hashes':{'sourcePack':hashfile(w/'data/source500_native.json'),'templates':hashfile(w/'data/source_templates.json'),'model':hashfile(modelpath),'tool':hashfile(Path(__file__))}},separators=(',',':')))
if __name__=='__main__':main()
