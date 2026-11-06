"""Empirical native macro rewrite grammar preserving the whole-board framework.

This is local structural rewrite, not neural synthesis. A substantial source
neighborhood is removed; donor macro geometry must fit every retained boundary
socket rigidly. Native guide assemblies remain atomic. Independent integrity,
raw seams and whole-layout novelty507 screen previews; no solution labels.
"""
from pathlib import Path
from collections import Counter
import argparse,copy,itertools,json,math,sys
import numpy as np
import joblib
from layout_learning_v3 import read,hashfile,level_context,SEED
from macro_assembly_v3 import instantiate_tile,socket_key,pose_matrix
from global_composition_v3 import graph_for,Geometry,arrangement,context
from assembly_contact_audit_v3 import NativeGeometrySeamAudit
from assembly_integrity_audit_v3 import NativeAssemblyIntegrityAudit
from layout_novelty_v3 import LayoutNoveltyIndex
from composition_field_v3 import field as complete_field,posed_field,native_frame,stroke_heading

def guide_close(selected,level):
 selected=set(selected)
 while True:
  old=len(selected)
  for e in level['guideLinks']:
   if e['guideOwnerId'] in selected or e['lockedOwnerId'] in selected:selected.update([e['guideOwnerId'],e['lockedOwnerId']])
  if old==len(selected):return selected

def source_regions(model,reference):
 source_id=reference['sourceId'];tiles=[t for t in model.tiles if t['sourceId']==source_id];graph=getattr(model,'composition_graphs',{}).get(source_id,graph_for(reference).to_undirected());regions=[];seen=set();n=len(reference['parts'])
 for a,b in itertools.chain(((t,None) for t in tiles),itertools.combinations(tiles,2)):
  if b is not None and not set(a['partIds'])&set(b['partIds']):continue
  region=guide_close(set(a['partIds'])|(set(b['partIds']) if b else set()),reference);boundary={i for i in region if any(j not in region for j in graph.neighbors(i))};boundary=guide_close(boundary,reference)&region;interior=region-boundary;fraction=len(interior)/max(1,n)
  if not .2<=fraction<=.35 or not interior or tuple(sorted(interior)) in seen:continue
  seen.add(tuple(sorted(interior)));regions.append({'region':sorted(region),'boundary':sorted(boundary),'interior':sorted(interior),'removedFraction':fraction,'sourceMacroCoreRecords':[a['core']]+([b['core']] if b else [])})
 return regions

def framework(model,reference,proposal_id):
 out={'id':proposal_id,'sourceId':'rewrite-'+str(proposal_id),'position':reference['position'],'parts':[],'links':[],'guideLinks':[],'bombs':[],'complete':False,'sourceOriginal':False,'validationStatus':'INCOMPLETE_NATIVE_REWRITE_PREVIEW','assemblyProvenance':{'method':'empirical native macro rewrite; retained whole-source framework with20â€“35%root neighborhood replacement','targetReferencePosition':reference['position'],'decisions':[],'declaredSearchBudget':{'removedRootFraction':[.2,.35],'maximumPartCountRatio':1.15,'minimumPartCountRatio':.85,'nativePoseTolerance':1e-4,'nativeAngleToleranceDegrees':.005}}};temporary={'sourceId':reference['sourceId'],'partIds':[p['id'] for p in reference['parts']]};model.tiles.append(temporary)
 try:mapping=instantiate_tile(model,len(model.tiles)-1,out,0.,np.array([0.,0.]))
 finally:model.tiles.pop()
 out['assemblyProvenance']['decisions'].append({'kind':'retainedNativeWholeBoardFramework','sourceId':reference['sourceId'],'partMapping':mapping,'rotation':0.,'translation':[0.,0.]});return out,mapping

def prune_interior(base,reference,mapping,removed):
 draft=copy.deepcopy(base);remove={mapping[i] for i in removed};draft['parts']=[p for p in draft['parts'] if p['id'] not in remove];draft['links']=[e for e in draft['links'] if e['source'] not in remove and e['target'] not in remove];draft['guideLinks']=[e for e in draft['guideLinks'] if e['guideOwnerId'] not in remove and e['lockedOwnerId'] not in remove];draft['assemblyProvenance']['tileImports'][0]['partMapping']={old:new for old,new in mapping.items() if old not in removed};ps={p['id']:p for p in reference['parts']};retained_bombs=[]
 for bomb in draft['bombs']:
  if bomb['ownerPartId'] in remove:continue
  if bomb['ownerPartId']<0:
   nearest=min(ps,key=lambda i:math.dist([ps[i]['x'],ps[i]['y']],list(bomb['worldPosition'].values())))
   if nearest in removed:continue
  retained_bombs.append(bomb)
 draft['bombs']=retained_bombs;return draft

def rewrite(model,reference,templates,global_model,novelty,rng,proposal_id,budget=160):
 model.templates=templates;base,mapping=framework(model,reference,proposal_id);regions=source_regions(model,reference);reject=Counter();seams=NativeGeometrySeamAudit(templates,list(model.levels.values()));integrity=NativeAssemblyIntegrityAudit(templates,list(model.levels.values()));ps={p['id']:p for p in reference['parts']};regions.sort(key=lambda r:abs(r['removedFraction']-.275));attempts=0;best=None;best_score=-1e9;geo=Geometry(templates);lc=level_context(reference);contextcost=np.linalg.norm(model.context_scaler.transform([lc+t['descriptor'] for t in model.tiles])-model.normalized_tile_contexts,axis=1);heading=stroke_heading(reference);_,_,span,center,matrix=native_frame(reference,templates,heading);offset=center@matrix.T;centered_reference=copy.deepcopy(reference)
 for p in centered_reference['parts']:p['x']-=float(offset[0]);p['y']-=float(offset[1])
 source_maps,_=posed_field(centered_reference,heading,span,len(ps),templates)
 if not regions:reject['noAtomicSourceNeighborhoodInDeclaredRewriteRange']+=1
 for region in regions:
  retained=prune_interior(base,reference,mapping,set(region['interior']));boundary=region['boundary'];candidates=[]
  if boundary:
   # The rarest exact socket is the first rigid anchor. All other boundary
   # roots must exist at their exact original positions in the donor macro.
   anchor=min(boundary,key=lambda i:len(model.by_socket.get(socket_key(ps[i],templates,reference),[])));candidates=[(index,source_anchor) for index,source_anchor in model.by_socket.get(socket_key(ps[anchor],templates,reference),[]) if model.tiles[index]['sourceId']!=reference['sourceId']]
  else:anchor=None;candidates=[(i,None) for i,t in enumerate(model.tiles) if t['sourceId']!=reference['sourceId']]
  candidates.sort(key=lambda item:contextcost[item[0]])
  # A source-derived neighborhood is sampled among the learned nearest32 tile
  # contexts. No root pose or attachment geometry is randomly permuted.
  candidates=candidates[:128]
  for index,source_anchor in candidates:
   if attempts>=budget:break
   attempts+=1;tile=model.tiles[index];donor=model.levels[tile['sourceId']];dp={p['id']:p for p in donor['parts']};selected=[dp[i] for i in tile['partIds']]
   if source_anchor is not None:
    a,b=dp[source_anchor],ps[anchor];rotation=b['angle']-a['angle'];translation=np.array([b['x'],b['y']])-pose_matrix(rotation)@np.array([a['x'],a['y']]);compatible=True
    for boundary_id in boundary:
     expected=ps[boundary_id];matches=[]
     for p in selected:
      if socket_key(p,templates,donor)!=socket_key(expected,templates,reference):continue
      xy=pose_matrix(rotation)@np.array([p['x'],p['y']])+translation
      if math.dist(xy,[expected['x'],expected['y']])<=1e-4 and abs((p['angle']+rotation-expected['angle']+180)%360-180)<=.005:matches.append(p)
     if not matches:compatible=False;break
    if not compatible:reject['incompatibleExactMultiSocketBoundary']+=1;continue
    anchor_pair=(source_anchor,mapping[anchor])
   else:
    # A disconnected original component has no boundary. Preserve its source
    # neighborhood's global center and original board frame for the rewrite.
    rotation=0.;oldcenter=np.mean([[ps[i]['x'],ps[i]['y']] for i in region['interior']],axis=0);newcenter=np.mean([[p['x'],p['y']] for p in selected],axis=0);translation=oldcenter-newcenter;anchor_pair=None
   candidate=copy.deepcopy(retained);before_ids={p['id'] for p in candidate['parts']};import_mapping=instantiate_tile(model,index,candidate,rotation,translation,anchor_pair);n=len(candidate['parts']);newparts=[p for p in candidate['parts'] if p['id'] not in before_ids];oldcounts=Counter(ps[i]['kind'] for i in region['interior']);newcounts=Counter(p['kind'] for p in newparts)
   # Local macro substitution must retain the reference neighborhood's scale
   # and mechanic mixture. Removing a Pull enclosure and inserting two easy
   # rings cannot qualify merely because the full-board count stays within15%.
   if not math.ceil(len(region['interior'])*.75)<=len(newparts)<=math.floor(len(region['interior'])*1.25):reject['localNeighborhoodScaleMismatch']+=1;continue
   if any(oldcounts[k]>0 and newcounts[k]==0 for k in ['slider','fixed','butterfly']):reject['removedNativeMechanicFamily']+=1;continue
   if sum(abs(oldcounts[k]-newcounts[k]) for k in oldcounts|newcounts)/max(1,len(region['interior']))>.5:reject['localMechanicMixMismatch']+=1;continue
   original_region={'parts':[ps[i] for i in region['interior']]};new_region={'parts':newparts};oldlow,oldhigh,_,_,_=native_frame(original_region,templates,heading);newlow,newhigh,_,_,_=native_frame(new_region,templates,heading);oldextent=np.maximum(oldhigh-oldlow,.01);newextent=np.maximum(newhigh-newlow,.01);ratios=newextent/oldextent
   if any(ratios<.75) or any(ratios>1.25):reject['localNativeFootprintExtentMismatch']+=1;continue
   oldarea=sum(geo.body(p).area for p in original_region['parts']);newarea=sum(geo.body(p).area for p in newparts)
   if oldarea>.001 and not .75<=newarea/oldarea<=1.25:reject['localNativeStrokeCoverageMismatch']+=1;continue
   removed_internal=sum(e['source'] in region['interior'] and e['target'] in region['interior'] for e in reference['links'])+sum(e['guideOwnerId'] in region['interior'] and e['lockedOwnerId'] in region['interior'] for e in reference['guideLinks']);newids={p['id'] for p in newparts};new_internal=sum(e['source'] in newids and e['target'] in newids for e in candidate['links'])+sum(e['guideOwnerId'] in newids and e['lockedOwnerId'] in newids for e in candidate['guideLinks'])
   if removed_internal and not .75<=new_internal/removed_internal<=1.25:reject['localAuthoredRelationDensityMismatch']+=1;continue
   if not math.ceil(len(ps)*.85)<=n<=math.floor(len(ps)*1.15):reject['referenceScaleExceeded']+=1;continue
   # Every required retained boundary must truly be reused by the constructor.
   if boundary and not {mapping[i] for i in boundary}.issubset(set(import_mapping.values())):reject['missingRequiredBoundarySocket']+=1;continue
   constructed=integrity.check(candidate)
   if not constructed['nativeAssemblyIntegrityPassed']:reject['nativeAssemblyIntegrity']+=1;continue
   raw=seams.check(candidate)
   if not raw['rawGeometrySeamPassed']:reject['newNativeGeometrySeamConflict']+=1;continue
   centered=copy.deepcopy(candidate)
   for p in centered['parts']:p['x']-=float(offset[0]);p['y']-=float(offset[1])
   maps,_=posed_field(centered,heading,span,len(ps),templates);field_delta=float(abs(maps-source_maps).sum())
   if field_delta>region['removedFraction']*2:reject['wholeBoardCompositionFieldDelta']+=1;continue
   novel=novelty.check(candidate)
   if not novel['passed']:reject['wholeLayoutNearCopy507']+=1;continue
   try:
    style=arrangement(candidate,templates,geo);score=float(global_model['syntheticPerturbationRanker'].predict_proba([context(candidate)+style.tolist()])[0,1])
   except (ValueError,IndexError,np.linalg.LinAlgError):reject['nonfiniteStyleGeometry']+=1;continue
   if score>best_score:
    best_score=score;candidate['rawGeometrySeamAudit']=raw;candidate['nativeAssemblyIntegrityAudit']=constructed;candidate['sourceNovelty507']=novel;candidate['proposalRanking']={'syntheticSourceStyleScore':score,'sourceWholeBoardFieldDelta':field_delta,'localNativeExtentRatios':ratios.tolist(),'localNativeStrokeCoverageRatio':newarea/max(oldarea,.001),'oldLocalAuthoredRelationCount':removed_internal,'newLocalAuthoredRelationCount':new_internal,'humanQualityStatus':'unmeasured','difficultyStatus':'unmeasured'};candidate['assemblyProvenance']['decisions'].append({'kind':'empiricalNativeMacroRewrite','sourceId':tile['sourceId'],'tileIndex':index,'sourceMacroCore':tile['core'],'removedFrameworkSourceRoots':region['interior'],'retainedBoundarySourceRoots':boundary,'sourceRegionCoreRecords':region['sourceMacroCoreRecords'],'removedFraction':region['removedFraction'],'exactBoundarySocketCount':len(boundary),'rotation':float(rotation),'translation':list(map(float,translation))});candidate['assemblyProvenance'].update(sourceFamilyCount=len({model.families[reference['sourceId']],model.families[tile['sourceId']]}),referenceScaleSatisfied=True,multipleSourceFamilies=model.families[reference['sourceId']]!=model.families[tile['sourceId']],nativeReplayRequired=True,rewriteSucceeded=True);best=candidate
  if attempts>=budget:break
 if best is None:
  best=base;best['rawGeometrySeamAudit']=seams.check(best);best['nativeAssemblyIntegrityAudit']=integrity.check(best);best['sourceNovelty507']=novelty.check(best);best['proposalRanking']={'humanQualityStatus':'unmeasured','difficultyStatus':'unmeasured','reason':'No rewrite passed declared native boundaries, scale, geometry and novelty gates'};best['assemblyProvenance'].update(sourceFamilyCount=1,referenceScaleSatisfied=True,multipleSourceFamilies=False,nativeReplayRequired=True,rewriteSucceeded=False)
 best['assemblyProvenance'].update(rewriteAttemptCount=attempts,rewriteRegionsExamined=len(regions),rejectionCounts=dict(reject));return best

def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',required=True,type=Path);p.add_argument('--positions',nargs='+',type=int,default=[1,26,51,76,101,126,151,176,201,226,251,276]);p.add_argument('--seed',type=int,default=SEED);p.add_argument('--budget',type=int,default=160);p.add_argument('--output',required=True,type=Path);a=p.parse_args();w=a.workspace.resolve();levels=read(w/'data/source500_native.json')['levels'];templates={t['id']:t for t in read(w/'data/source_templates.json')['templates']};extras=read(w/'data/novelty_extras/source_parity_pack.json')['levels'];novelty_templates=dict(templates)
 for t in read(w/'data/novelty_extras/source_templates.json')['templates']:novelty_templates[t['id']]=t
 novelty=LayoutNoveltyIndex(levels+extras,novelty_templates);model=joblib.load(w/'models/macro_assembly.joblib');global_model=joblib.load(w/'models/global_composition.joblib');rng=np.random.default_rng(a.seed);rows=[]
 for i,position in enumerate(a.positions):
  reference=next(l for l in levels if l['position']==position);result=rewrite(model,reference,templates,global_model,novelty,rng,4000001+i,a.budget);rows.append(result);print('REWRITE',position,'succeeded',result['assemblyProvenance']['rewriteSucceeded'],'attempts',result['assemblyProvenance']['rewriteAttemptCount'],'rejections',result['assemblyProvenance']['rejectionCounts'],flush=True)
 a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps({'schemaVersion':3,'complete':False,'acceptedCount':0,'status':'INCOMPLETE_EMPIRICAL_NATIVE_MACRO_REWRITE_NO_REPLAY_OR_DIFFICULTY','seed':a.seed,'trainingSourceCount':500,'noveltyExclusionCount':len(levels+extras),'levels':rows,'hashes':{'sourcePack':hashfile(w/'data/source500_native.json'),'templates':hashfile(w/'data/source_templates.json'),'macroModel':hashfile(w/'models/macro_assembly.joblib'),'proposer':hashfile(Path(__file__))}},separators=(',',':')),encoding='utf8')
if __name__=='__main__':main()
