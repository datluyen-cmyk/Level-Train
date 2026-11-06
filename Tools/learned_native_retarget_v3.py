"""Joint native-root pose optimization from saved learned relation mixtures.

Moves/rotates complete imported native assets only. This is an experimental
learned constrained placement family, not an exact rigid source import or a
physics certificate. Native Unity binding/replay must later validate survivors.
"""
from pathlib import Path
from collections import Counter
import argparse,copy,json,math
import numpy as np
import networkx as nx
import joblib
from scipy.optimize import least_squares
from shapely import affinity,union_all
from layout_learning_v3 import read,hashfile,relation_rows,frame
from assembly_contact_audit_v3 import shape_polygon
from native_feature_socket_v3 import feature_shape_map

def optimize(candidate,templates,model,source_levels,pack_hash,model_hash,max_evaluations=90):
 out=copy.deepcopy(candidate);imports=out['assemblyProvenance'].pop('featureBoundaryImports');movable={new for imp in imports for new in imp['interiorPartMapping'].values()};ps={p['id']:p for p in out['parts']};initial={i:(ps[i]['x'],ps[i]['y'],ps[i]['angle']) for i in ps};guide=nx.Graph();guide.add_nodes_from(movable)
 for e in out['guideLinks']:
  if e['guideOwnerId'] in movable and e['lockedOwnerId'] in movable:guide.add_edge(e['guideOwnerId'],e['lockedOwnerId'])
 groups=[sorted(g) for g in nx.connected_components(guide)];owner_group={i:j for j,g in enumerate(groups) for i in g};centers=[np.mean([[ps[i]['x'],ps[i]['y']] for i in g],axis=0) for g in groups];rows=[r for r in relation_rows(out,templates) if r['sourcePart'] in movable or r['targetPart'] in movable];learned=[]
 for row in rows:
  distance,indices=model.neighbors([row['context']]);learned.append({'row':row,'recordIndices':indices[0].tolist(),'distances':distance[0].tolist(),'poseMixture':model.y[indices[0]]})
 local={}
 for i,p in ps.items():
  shapes=templates[p['templateId']]['physicalShapes'];local[i]={'solid':union_all([shape_polygon(s) for s in shapes if not s.get('trigger',False) and s.get('sourceLayer',7)==7]),'shapes':[shape_polygon(s) for s in shapes]}
 def apply(values):
  for j,g in enumerate(groups):
   dx,dy,angle=values[j*3:j*3+3];r=math.radians(angle);c,s=math.cos(r),math.sin(r);center=centers[j]
   for i in g:
    x,y,a=initial[i];x-=center[0];y-=center[1];ps[i]['x']=float(center[0]+c*x-s*y+dx);ps[i]['y']=float(center[1]+s*x+c*y+dy);ps[i]['angle']=float(a+angle)
 def world(g,p):return affinity.translate(affinity.rotate(g,p['angle'],origin=(0,0)),p['x'],p['y'])
 # Fixed collision residual set; no source-pair exemptions for newly placed
 # relations. Trigger head/body contact is an explicit endpoint constraint.
 pairs=[(a,b) for a in ps for b in ps if a<b and (a in movable or b in movable) and not(a in owner_group and b in owner_group and owner_group[a]==owner_group[b])]
 collision_weight=[8.]
 def residual(values):
  apply(values);result=[]
  for entry in learned:
   row=entry['row'];a,b=ps[row['sourcePart']],ps[row['targetPart']];unit=max(templates[a['templateId']].get('width',0),templates[b['templateId']].get('width',0),1.);xy=frame(b['x']-a['x'],b['y']-a['y'],a['angle']);phi=math.radians(b['angle']-a['angle']);pose=np.array([xy[0]/unit,xy[1]/unit,math.cos(phi),math.sin(phi)]);mixture=entry['poseMixture'];cost=np.sum(((mixture-pose)/np.array([.45,.45,.35,.35]))**2,axis=1)+.05*np.array(entry['distances'])**2;nearest=int(np.argmin(cost));result.extend(((pose-mixture[nearest])/np.array([.45,.45,.35,.35])*.35).tolist())
  solids={i:world(local[i]['solid'],p) for i,p in ps.items()}
  for a,b in pairs:
   unit=max(.01,templates[ps[a]['templateId']].get('width',0),templates[ps[b]['templateId']].get('width',0));area=solids[a].intersection(solids[b]).area;result.append(math.sqrt(max(0,area))/unit*collision_weight[0])
  for e in out['links']:
   if e['source'] not in movable and e['target'] not in movable:continue
   a,b=ps[e['source']],ps[e['target']];head_indices=[i for i in e['sourceShapeIndices'] if templates[a['templateId']]['physicalShapes'][i]['role']=='head_trigger'];heads=union_all([world(local[a['id']]['shapes'][i],a) for i in head_indices]);target=union_all([world(local[b['id']]['shapes'][i],b) for i in e['targetShapeIndices']]);unit=max(.01,templates[a['templateId']].get('width',0),templates[b['templateId']].get('width',0));result.append(heads.distance(target)/unit*10 if not heads.is_empty and not target.is_empty else 0.)
  # Native source field/framework is the positional scaffold, not a free XY
  # shuffle. The learned mixture may deviate when contact constraints require.
  for j in range(len(groups)):result.extend([values[j*3]*.03,values[j*3+1]*.03,values[j*3+2]/180*.1])
  return np.array(result)
 start=np.zeros(len(groups)*3);lower=np.tile([-2.,-2.,-90.],len(groups));upper=np.tile([2.,2.,90.],len(groups));initial_residual=residual(start);stages=[]
 for weight in [8.,40.,200.]:
  collision_weight[0]=weight;fit=least_squares(residual,start,bounds=(lower,upper),max_nfev=max_evaluations,diff_step=1e-3,ftol=1e-5,xtol=1e-5,gtol=1e-5);start=fit.x;stages.append({'collisionWeight':weight,'evaluations':fit.nfev,'succeededNumerically':bool(fit.success),'residualSquared':float(fit.fun@fit.fun)})
 # Raw polygon crossing area can have a flat plateau (a ring crosses a long
 # straight Pull strip). Use saved learned relation components as alternate
 # native poses, not uniformly random XY or invented pin coordinates.
 restart_records=[];current_cost=float(residual(start)@residual(start))
 for attempt in range(16):
  trial=start.copy();apply(start)
  for j,g in enumerate(groups):
   choices=[entry for entry in learned if entry['row']['relation']=='pin' and ((entry['row']['sourcePart'] in g)!=(entry['row']['targetPart'] in g))]
   if not choices:continue
   entry=choices[attempt%len(choices)];row=entry['row'];record=(attempt*3+j)%len(entry['recordIndices']);y=entry['poseMixture'][record];a,b=ps[row['sourcePart']],ps[row['targetPart']];unit=max(templates[a['templateId']].get('width',0),templates[b['templateId']].get('width',0),1.);relative=math.degrees(math.atan2(y[3],y[2]))
   if b['id'] in g:
    old=b['id'];desired_angle=a['angle']+relative;desired_center=np.array([a['x'],a['y']])+np.array([[math.cos(math.radians(a['angle'])),-math.sin(math.radians(a['angle']))],[math.sin(math.radians(a['angle'])),math.cos(math.radians(a['angle']))]])@y[:2]*unit
   else:
    old=a['id'];desired_angle=b['angle']-relative;desired_center=np.array([b['x'],b['y']])-np.array([[math.cos(math.radians(desired_angle)),-math.sin(math.radians(desired_angle))],[math.sin(math.radians(desired_angle)),math.cos(math.radians(desired_angle))]])@y[:2]*unit
   delta=(desired_angle-initial[old][2]+180)%360-180;r=math.radians(delta);rotation=np.array([[math.cos(r),-math.sin(r)],[math.sin(r),math.cos(r)]]);shift=desired_center-centers[j]-rotation@(np.array(initial[old][:2])-centers[j]);trial[j*3:j*3+3]=np.clip([shift[0],shift[1],delta],lower[j*3:j*3+3],upper[j*3:j*3+3]);restart_records.append({'attempt':attempt,'group':j,'learnedRecordIndex':entry['recordIndices'][record]})
  value=residual(trial);cost=float(value@value)
  if cost<current_cost:start=trial;current_cost=cost
 fit=least_squares(residual,start,bounds=(lower,upper),max_nfev=max_evaluations,diff_step=1e-3,ftol=1e-5,xtol=1e-5,gtol=1e-5);stages.append({'collisionWeight':200.,'phase':'bestSavedLearnedPoseComponentRestart','evaluations':fit.nfev,'succeededNumerically':bool(fit.success),'residualSquared':float(fit.fun@fit.fun),'sampledLearnedComponents':restart_records})
 apply(fit.x);witness=[]
 for entry in learned:
  row=entry['row'];a,b=ps[row['sourcePart']],ps[row['targetPart']];unit=max(templates[a['templateId']].get('width',0),templates[b['templateId']].get('width',0),1.);xy=frame(b['x']-a['x'],b['y']-a['y'],a['angle']);phi=math.radians(b['angle']-a['angle']);pose=np.array([xy[0]/unit,xy[1]/unit,math.cos(phi),math.sin(phi)]);errors=np.linalg.norm(entry['poseMixture']-pose,axis=1);idx=int(np.argmin(errors));witness.append({'sourcePart':a['id'],'targetPart':b['id'],'relation':row['relation'],'learnedRelationRecordIndex':entry['recordIndices'][idx],'learnedPose':entry['poseMixture'][idx].tolist(),'actualPose':pose.tolist(),'poseResidualL2':float(errors[idx])})
 for imp in imports:
  original_parts={p['id']:p for l in source_levels if l['sourceId']==imp['sourceId'] for p in l['parts']}
  for witness in imp['boundaryWitnesses']:
   source_template=templates[original_parts[witness['sourcePartId']]['templateId']];target_template=templates[ps[witness['targetPartId']]['templateId']];body_indices=[i for i,q in enumerate(source_template['physicalShapes']) if q['role']=='body'];body_map=feature_shape_map(source_template,target_template,body_indices)
   if body_map is None:raise ValueError('retarget boundary lacks exact complete body')
   witness['sourceBodyShapeIndices']=body_indices;witness['targetBodyShapeIndices']=[body_map[i] for i in body_indices]
 for imp in imports:
  imp['initialRigidRotation']=imp.pop('rotation');imp['initialRigidTranslation']=imp.pop('translation');imp['optimizedRootPoses']=[{'partId':new,'x':ps[new]['x'],'y':ps[new]['y'],'angle':ps[new]['angle']} for new in imp['interiorPartMapping'].values()]
 out['assemblyProvenance']['retargetedAssetImports']=[{'sourceId':imp['sourceId'],'partMapping':imp['interiorPartMapping']} for imp in imports];placements=[];source_by={l['sourceId']:l for l in source_levels};witness_by={(e['sourcePart'],e['targetPart']):e for e in witness if e['relation']=='pin'}
 for imp in imports:
  mapping={int(k):v for k,v in imp['interiorPartMapping'].items()};mapping.update({w['sourcePartId']:w['targetPartId'] for w in imp['boundaryWitnesses']});source=source_by[imp['sourceId']]
  for edge_index,e in enumerate(source['links']):
   if e['source'] not in mapping or e['target'] not in mapping:continue
   sa,ta=mapping[e['source']],mapping[e['target']]
   if sa not in movable and ta not in movable:continue
   actual=next((q for q in out['links'] if q['source']==sa and q['target']==ta),None);learned_witness=witness_by.get((sa,ta))
   if actual is None or learned_witness is None:continue
   placements.append({'sourceRelationSourceId':imp['sourceId'],'sourceRelationIndex':edge_index,'actualSource':sa,'actualTarget':ta,'actualSourceShapeIndices':actual['sourceShapeIndices'],'actualTargetShapeIndices':actual['targetShapeIndices'],'learnedRecordIndex':learned_witness['learnedRelationRecordIndex'],'modelSha256':model_hash})
 out['assemblyProvenance']['learnedRelationPlacements']=placements;out['assemblyProvenance']['learnedBoundaryFeatureWitnesses']=imports;out['assemblyProvenance']['learnedPlacementCertificate']={'method':'boundedjointleast_squares over complete native guide-connected rootgroups; savedconditionalempiricalposemixture prior + rawsolidcollision/head-to-targetdistance constraints','sourcePackSha256':pack_hash,'localPoseModelSha256':model_hash,'relationWitnesses':witness,'guideRigidGroups':groups,'optimizer':{'maxEvaluationsPerPenaltyStage':max_evaluations,'penaltyStages':stages,'evaluations':sum(s['evaluations'] for s in stages),'succeededNumerically':bool(fit.success),'initialResidualSquaredWeight8':float(initial_residual@initial_residual),'finalResidualSquaredWeight200':float(fit.fun@fit.fun),'status':fit.message,'groupTranslationBoundsWorldUnits':[-2.,2.],'groupRotationBoundsDegrees':[-90.,90.]},'limits':'Raw polygon optimization has no Unity skin/casts/drag/release semantics. Numeric convergence does not certify contacts, source fidelity, solvability or difficulty.'};out['sourceId']='learned-retarget-'+str(out['id']);out['validationStatus']='INCOMPLETE_LEARNED_NATIVE_RETARGET_NO_PHYSICS_CERTIFICATE';out['assemblyProvenance']['decisions'].append({'kind':'learnedNativePoseRetarget','movedRootIds':sorted(movable),'modifiedColliderGeometry':False});return out

def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',required=True,type=Path);p.add_argument('--pack',required=True,type=Path);p.add_argument('--ids',nargs='+',type=int);p.add_argument('--limit',type=int,default=3);p.add_argument('--max-evaluations',type=int,default=90);p.add_argument('--output',required=True,type=Path);a=p.parse_args();w=a.workspace.resolve();templates={t['id']:t for t in read(w/'data/source_templates.json')['templates']};modelpath=w/'models/composition_pose.joblib';model=joblib.load(modelpath)['model'];pack=read(a.pack);candidates=[l for l in pack['levels'] if not a.ids or l['id'] in a.ids][:a.limit];out=[];source_levels=read(w/'data/source500_native.json')['levels']
 for candidate in candidates:
  result=optimize(candidate,templates,model,source_levels,hashfile(w/'data/source500_native.json'),hashfile(modelpath),a.max_evaluations);out.append(result);print('RETARGET',result['id'],result['position'],result['assemblyProvenance']['learnedPlacementCertificate']['optimizer'],flush=True)
 a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps({'schemaVersion':3,'complete':False,'acceptedCount':0,'status':'LEARNED_RETARGET_EXPERIMENT_ONLY_NATIVE_ASSET_INTEGRITY_RAW_GEOMETRY_AND_REPLAY_PENDING','levels':out,'hashes':{'inputPack':hashfile(a.pack),'tool':hashfile(Path(__file__))}},separators=(',',':')),encoding='utf8')
if __name__=='__main__':main()
