"""Rigid native guide-component placements from saved conditional source poses."""
from pathlib import Path
from collections import Counter
import argparse,copy,itertools,json,math
import joblib,networkx as nx,numpy as np
from shapely import union_all
from assembly_contact_audit_v3 import NativeGeometrySeamAudit,shape_polygon
from learned_placement_audit_v3 import NativeLearnedPlacementAudit
from native_asset_variant_v3 import clone_root,candidate,world,special_root
from layout_learning_v3 import read,hashfile,relation_rows

def group_options(reference,group,templates,local,model,static_ids):
 ps={p['id']:p for p in reference['parts']};center=np.mean([[ps[i]['x'],ps[i]['y']] for i in group],axis=0);rows=[r for r in relation_rows(reference,templates) if r['relation']=='pin' and ((r['sourcePart'] in group)!=(r['targetPart'] in group))];options=[];seen=set();reject=Counter();static={i:world(local[ps[i]['templateId']]['solid'],ps[i]) for i in static_ids}
 for row in rows:
  _,records=model.neighbors([row['context']]);a,b=ps[row['sourcePart']],ps[row['targetPart']];unit=max(1.,templates[a['templateId']].get('width',0),templates[b['templateId']].get('width',0))
  for record in records[0]:
   y=model.y[record];relative=math.degrees(math.atan2(y[3],y[2]))
   if a['id'] in group:
    root=a;desired_angle=b['angle']-relative;r=math.radians(desired_angle);matrix=np.array([[math.cos(r),-math.sin(r)],[math.sin(r),math.cos(r)]]);desired=np.array([b['x'],b['y']])-matrix@y[:2]*unit
   else:
    root=b;desired_angle=a['angle']+relative;r=math.radians(a['angle']);matrix=np.array([[math.cos(r),-math.sin(r)],[math.sin(r),math.cos(r)]]);desired=np.array([a['x'],a['y']])+matrix@y[:2]*unit
   delta=(desired_angle-root['angle']+180)%360-180;r=math.radians(delta);matrix=np.array([[math.cos(r),-math.sin(r)],[math.sin(r),math.cos(r)]]);shift=desired-center-matrix@(np.array([root['x'],root['y']])-center);identity=(round(delta,4),round(float(shift[0]),4),round(float(shift[1]),4))
   if identity in seen:continue
   seen.add(identity)
   if abs(delta)<1e-3 and np.linalg.norm(shift)<1e-3:continue
   moved={};geos={}
   for i in group:
    old=ps[i];xy=center+matrix@(np.array([old['x'],old['y']])-center)+shift;pose={**old,'x':float(xy[0]),'y':float(xy[1]),'angle':old['angle']+delta};part=clone_root(old,reference['sourceId'],i,pose);moved[i]=part;geos[i]={'solid':world(local[part['templateId']]['solid'],part),'body':world(local[part['templateId']]['body'],part)}
   if any(g['solid'].intersection(other).area>1e-5 for g in geos.values() for other in static.values()):reject['newWholeGroupSolidCrossesNativeFramework']+=1;continue
   valid=True
   for e in reference['links']:
    if not ((e['source'] in group)!=(e['target'] in group)):continue
    outside=e['target'] if e['source'] in group else e['source']
    if outside not in static_ids:continue
    sa=moved.get(e['source'],ps[e['source']]);tb=moved.get(e['target'],ps[e['target']]);head=world(union_all([local[sa['templateId']]['shapes'][i] for i in e['sourceShapeIndices'] if templates[sa['templateId']]['physicalShapes'][i]['role']=='head_trigger']),sa);body=world(union_all([local[tb['templateId']]['shapes'][i] for i in e['targetShapeIndices']]),tb)
    if head.distance(body)>1e-5:valid=False;break
   if not valid:reject['lostCrossGroupNativeHeadBodyContact']+=1;continue
   options.append({'parts':moved,'geometry':geos,'angleDelta':delta,'translation':shift.tolist(),'learnedRecordIndex':int(record),'anchoringRelation':[a['id'],b['id']]})
 return options,reject

def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',required=True,type=Path);p.add_argument('--position',required=True,type=int);p.add_argument('--output',required=True,type=Path);p.add_argument('--limit',type=int,default=4);a=p.parse_args();w=a.workspace;levels=read(w/'data/source500_native.json')['levels'];ref=next(l for l in levels if l['position']==a.position);sources={l['sourceId']:l for l in levels};templates={t['id']:t for t in read(w/'data/source_templates.json')['templates']};local={}
 for tid in {p['templateId'] for p in ref['parts']}:
  t=templates[tid];local[tid]={'solid':union_all([shape_polygon(s) for s in t['physicalShapes'] if not s['trigger'] and s.get('sourceEnabled',True) and s.get('sourceLayer',7)==7]),'body':union_all([shape_polygon(s) for s in t['physicalShapes'] if s['role']=='body']),'shapes':[shape_polygon(s) for s in t['physicalShapes']]}
 graph=nx.Graph();graph.add_nodes_from(p['id'] for p in ref['parts']);graph.add_edges_from((e['guideOwnerId'],e['lockedOwnerId']) for e in ref['guideLinks']);ps={p['id']:p for p in ref['parts']};bombowners={b['ownerPartId'] for b in ref['bombs']};groups=[sorted(c) for c in nx.connected_components(graph) if 2<=len(c)<=math.floor(len(ps)*.35) and not any(special_root(ps[i]) or i in bombowners for i in c)];modelpath=w/'models/composition_pose.joblib';model=joblib.load(modelpath)['model'];sa=NativeGeometrySeamAudit(templates,levels);ia=NativeLearnedPlacementAudit(levels,templates,modelpath,w/'data/source500_native.json');out=[];reports=[]
 for count in range(1,len(groups)+1):
  for selected in itertools.combinations(groups,count):
   moved_ids=set(i for group in selected for i in group)
   if not .2<=len(moved_ids)/len(ps)<=.35:continue
   static_ids=set(ps)-moved_ids;pools=[];record={'groups':selected,'movedRoots':len(moved_ids),'options':[]}
   for group in selected:
    options,reject=group_options(ref,set(group),templates,local,model,static_ids);pools.append(options);record['options'].append({'group':group,'candidatePoses':len(options),'rejections':dict(reject)});print('GUIDEGROUP',record['options'][-1],flush=True)
   reports.append(record)
   if any(not pool for pool in pools):continue
   for choices in itertools.product(*pools):
    parts={i:p for choice in choices for i,p in choice['parts'].items()};invalid=False
    for left,right in itertools.combinations(choices,2):
     if any(x['solid'].intersection(y['solid']).area>1e-5 for x in left['geometry'].values() for y in right['geometry'].values()):invalid=True;break
    if invalid:continue
    root_choices={}
    for choice in choices:
     for i,part in choice['parts'].items():
      edges=[{'edge':copy.deepcopy(e),'sourcePatternSourceId':ref['sourceId'],'sourcePatternEdgeIndex':index,'targetOriginalId':e['target']} for index,e in enumerate(ref['links']) if e['source']==i];targets=sorted(e['edge']['target'] for e in edges);root_choices[i]={'sourceId':ref['sourceId'],'donorId':i,'part':part,'edges':edges,'dependencySymmetricDifference':0,'oldTargets':targets,'newTargets':targets,'bodyPreserved':False,'angleDeltaDegrees':choice['angleDelta'],'angleSource':'complete native guide-connected group placement from saved conditional native pin pose record'+str(choice['learnedRecordIndex']),'conditionalPoseNLL':None}
    try:c=candidate(ref,{},templates,sources,local,model,hashfile(w/'data/source500_native.json'),hashfile(modelpath),7500001+len(out),0,selected_override=root_choices)
    except ValueError:continue
    c['assemblyProvenance']['method']='Rigid complete native guide-connected component placements; saved conditional native cross-group pin pose records; endpoint topology retained';c['assemblyProvenance']['retargetedAssetImports']=[{'sourceId':ref['sourceId'],'partMapping':{i:i for i in moved_ids}}];c['assemblyProvenance']['learnedPlacementCertificate']['guideRigidGroups']=[list(g) for g in selected];c['assemblyProvenance']['guideGroupPoseRecords']=[{k:v for k,v in choice.items() if k not in ['parts','geometry']} for choice in choices];c['preliminaryRawSeam']=sa.check(c);c['preliminaryLearnedIntegrity']=ia.check(c);out.append(c);print('GROUP_CANDIDATE',c['id'],c['preliminaryRawSeam']['rawGeometrySeamPassed'],c['preliminaryLearnedIntegrity']['nativeLearnedPlacementIntegrityPassed'],flush=True)
    if len(out)>=a.limit:break
   if len(out)>=a.limit:break
  if len(out)>=a.limit:break
 a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps({'schemaVersion':3,'complete':False,'acceptedCount':0,'status':'WHOLE_NATIVE_GUIDE_GROUP_POSE_EXPERIMENT_NO_REPLAY_DIFFICULTY_NOVELTY_LABELS','sourcePosition':a.position,'guideComponentSizes':sorted([len(c) for c in nx.connected_components(graph)],reverse=True),'levels':out,'reports':reports,'hashes':{'sourcePack':hashfile(w/'data/source500_native.json'),'templates':hashfile(w/'data/source_templates.json'),'model':hashfile(modelpath),'tool':hashfile(Path(__file__))}},separators=(',',':')))
if __name__=='__main__':main()
