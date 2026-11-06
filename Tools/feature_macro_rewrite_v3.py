"""Rewrite source native neighborhoods using certified boundary feature sockets."""
from pathlib import Path
from collections import defaultdict,Counter
import argparse,copy,json,math
import numpy as np
import joblib
from native_feature_socket_v3 import body_key,feature_shape_map,remap_donor_relation
from macro_assembly_v3 import instantiate_tile,pose_matrix
from macro_rewrite_v3 import source_regions,framework,prune_interior
from layout_learning_v3 import read,hashfile,level_context,SEED

def construct(model,reference,templates,region,index,donor_anchor,target_anchor,proposal_id):
 """Return an unvalidated candidate and explicit edge/feature witnesses."""
 base,source_mapping=framework(model,reference,proposal_id);draft=prune_interior(base,reference,source_mapping,set(region['interior']));tile=model.tiles[index];donor=model.levels[tile['sourceId']];sourceparts={p['id']:p for p in donor['parts']};targetparts={p['id']:p for p in reference['parts']};a,b=sourceparts[donor_anchor],targetparts[target_anchor];rotation=b['angle']-a['angle'];translation=np.array([b['x'],b['y']])-pose_matrix(rotation)@np.array([a['x'],a['y']]);boundary_map={};used=set()
 for tid in region['boundary']:
  target=targetparts[tid];key=body_key(target,templates);matches=[]
  for pid in tile['partIds']:
   p=sourceparts[pid]
   if pid in used or body_key(p,templates)!=key:continue
   xy=pose_matrix(rotation)@np.array([p['x'],p['y']])+translation
   if math.dist(xy,[target['x'],target['y']])<=1e-4 and abs((p['angle']+rotation-target['angle']+180)%360-180)<=.005:matches.append(pid)
  if not matches:return None,'bodyBoundaryPoseMismatch'
  boundary_map[matches[0]]=source_mapping[tid];used.add(matches[0])
 interior=[i for i in tile['partIds'] if i not in boundary_map]
 if not interior:return None,'noDonorInterior'
 selected=set(interior)|set(boundary_map);shape_maps={};witness_relations=defaultdict(list)
 for edge in donor['guideLinks']:
  a,b=edge['guideOwnerId'],edge['lockedOwnerId']
  if (a in interior)!=(b in interior):return None,'atomicNativeGuideCrossesFeatureBoundary'
 for edge_index,edge in enumerate(donor['links']):
  if edge['source'] not in selected or edge['target'] not in selected:continue
  if edge['source'] in boundary_map and edge['target'] in boundary_map:continue
  for side in ['source','target']:
   owner=edge[side]
   if owner not in boundary_map:continue
   target=next(p for p in draft['parts'] if p['id']==boundary_map[owner]);mapping=feature_shape_map(templates[sourceparts[owner]['templateId']],templates[target['templateId']],edge[side+'ShapeIndices'])
   if mapping is None:return None,'nativeBoundaryFeatureGeometryMismatch'
   prior=shape_maps.setdefault(owner,{})
   if any(k in prior and prior[k]!=v for k,v in mapping.items()):return None,'inconsistentFeatureIndexMapping'
   prior.update(mapping);witness_relations[owner].append(edge_index)
 composition_pairs=[]
 for record_index,e in enumerate(getattr(model,'physical_composition_records',{}).get(tile['sourceId'],[])):
  if not e.get('strong',False):continue
  for owner,other in [(e['source'],e['target']),(e['target'],e['source'])]:
   if owner not in boundary_map or other not in interior:continue
   source_template=templates[sourceparts[owner]['templateId']];target_template=templates[targetparts[next(tid for tid in region['boundary'] if source_mapping[tid]==boundary_map[owner])]['templateId']];indices=[i for i,q in enumerate(source_template['physicalShapes']) if q['role']=='body'];mapping=feature_shape_map(source_template,target_template,indices)
   if not mapping:return None,'nativeCompositionBodyMismatch'
   prior=shape_maps.setdefault(owner,{})
   if any(k in prior and prior[k]!=v for k,v in mapping.items()):return None,'inconsistentCompositionBodyMapping'
   prior.update(mapping);composition_pairs.append({'interiorSourcePartId':other,'boundarySourcePartId':owner,'sourceCompositionRecordIndex':record_index,'kind':e['kind']})
 if any(owner not in shape_maps for owner in boundary_map):return None,'boundaryHasNoAuthoredRelationOrStrongNativeCompositionEvidence'
 temporary={'sourceId':tile['sourceId'],'partIds':interior};model.tiles.append(temporary)
 try:interior_mapping=instantiate_tile(model,len(model.tiles)-1,draft,rotation,translation)
 finally:model.tiles.pop()
 # This source import is intentionally feature-bounded, not a false full-owner
 # template identity claim. Every new root remains a full original asset.
 imported=draft['assemblyProvenance']['tileImports'].pop();owner_map={**interior_mapping,**boundary_map};witnesses=[]
 for owner,target in boundary_map.items():
  indices=sorted(shape_maps[owner]);witnesses.append({'sourcePartId':owner,'targetPartId':target,'sourceShapeIndices':indices,'targetShapeIndices':[shape_maps[owner][i] for i in indices],'relationIndices':sorted(set(witness_relations[owner])),'kind':'pin' if witness_relations[owner] else 'compositionBody'})
 certificate={'sourceId':tile['sourceId'],'rotation':float(rotation),'translation':list(map(float,translation)),'interiorPartMapping':interior_mapping,'boundaryWitnesses':witnesses,'compositionBoundaryPairs':composition_pairs};draft['assemblyProvenance'].setdefault('featureBoundaryImports',[]).append(certificate);newparts={p['id']:p for p in draft['parts']}
 for index_e,edge in enumerate(donor['links']):
  if edge['source'] not in owner_map or edge['target'] not in owner_map:continue
  if edge['source'] in boundary_map and edge['target'] in boundary_map:continue
  if edge['source'] not in boundary_map and edge['target'] not in boundary_map:continue # ordinary interior edge already imported
  draft['links'].append(remap_donor_relation(edge,owner_map,shape_maps,templates,newparts))
 draft['assemblyProvenance']['decisions'].append({'kind':'nativeFeatureSocketMacroRewrite','sourceId':tile['sourceId'],'tileIndex':index,'removedFrameworkSourceRoots':region['interior'],'retainedBoundarySourceRoots':region['boundary'],'removedFraction':region['removedFraction'],'boundaryWitnessCount':len(witnesses),'sourceAnchor':donor_anchor,'targetAnchor':target_anchor,'rotation':float(rotation),'translation':list(map(float,translation))});draft['sourceId']='feature-rewrite-'+str(proposal_id);draft['validationStatus']='INCOMPLETE_FEATURE_SOCKET_REWRITE';return draft,None

def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',required=True,type=Path);p.add_argument('--positions',nargs='+',type=int,default=[26,51,76,126,176,251]);p.add_argument('--model-name',default='macro_assembly.joblib');p.add_argument('--budget',type=int,default=256);p.add_argument('--output',required=True,type=Path);args=p.parse_args();w=args.workspace.resolve();levels=read(w/'data/source500_native.json')['levels'];templates={t['id']:t for t in read(w/'data/source_templates.json')['templates']};model=joblib.load(w/'models'/args.model_name);model.templates=templates;vocabulary=defaultdict(list)
 for i,tile in enumerate(model.tiles):
  ps={p['id']:p for p in model.levels[tile['sourceId']]['parts']}
  for pid in tile['partIds']:
   if ps[pid]['kind'] in ['ring','slider']:vocabulary[body_key(ps[pid],templates)].append((i,pid))
 proposals=[];reports=[]
 for position in args.positions:
  reference=next(l for l in levels if l['position']==position);ps={p['id']:p for p in reference['parts']};regions=source_regions(model,reference);reject=Counter();attempts=0;successful=0;lc=level_context(reference);contextcost=np.linalg.norm(model.context_scaler.transform([lc+t['descriptor'] for t in model.tiles])-model.normalized_tile_contexts,axis=1)
  for region in regions:
   if not region['boundary']:continue
   anchor=min(region['boundary'],key=lambda i:len(vocabulary.get(body_key(ps[i],templates),[])));donors=[];targetcounts=Counter(ps[i]['kind'] for i in region['interior'])
   for i,donor_anchor in vocabulary.get(body_key(ps[anchor],templates),[]):
    tile=model.tiles[i]
    if tile['sourceId']==reference['sourceId']:continue
    predicted=len(tile['partIds'])-len(region['boundary'])
    if not math.ceil(len(region['interior'])*.75)<=predicted<=math.floor(len(region['interior'])*1.25):reject['preconditionLocalDonorScale']+=1;continue
    donorparts={p['id']:p for p in model.levels[tile['sourceId']]['parts']};counts=Counter(donorparts[pid]['kind'] for pid in tile['partIds']);boundarycounts=Counter(ps[pid]['kind'] for pid in region['boundary']);counts.subtract(boundarycounts)
    if any(targetcounts[k]>0 and counts[k]<=0 for k in ['slider','fixed','butterfly']):reject['preconditionNativeMechanicFamily']+=1;continue
    donors.append((i,donor_anchor))
   donors.sort(key=lambda item:contextcost[item[0]])
   for index,donor_anchor in donors:
    if attempts>=args.budget:break
    attempts+=1;candidate,reason=construct(model,reference,templates,region,index,donor_anchor,anchor,5000001+len(proposals))
    if candidate is None:reject[reason]+=1;continue
    successful+=1;candidate['assemblyProvenance'].update(nativeReplayRequired=True,referenceScaleSatisfied=False,featureConstructionOnly=True);proposals.append(candidate)
   if attempts>=args.budget:break
  reports.append({'sourcePosition':position,'attempts':attempts,'constructedFeatureCandidates':successful,'rejections':dict(reject)});print('FEATURECOVERAGE',reports[-1],flush=True)
 args.output.parent.mkdir(parents=True,exist_ok=True);args.output.write_text(json.dumps({'schemaVersion':3,'complete':False,'acceptedCount':0,'status':'FEATURE_CONSTRUCTION_ONLY_INDEPENDENT_INTEGRITY_AND_RAW_SEAM_PENDING','levels':proposals,'coverage':reports,'hashes':{'sourcePack':hashfile(w/'data/source500_native.json'),'templates':hashfile(w/'data/source_templates.json'),'tool':hashfile(Path(__file__))}},separators=(',',':')),encoding='utf8')
if __name__=='__main__':main()
