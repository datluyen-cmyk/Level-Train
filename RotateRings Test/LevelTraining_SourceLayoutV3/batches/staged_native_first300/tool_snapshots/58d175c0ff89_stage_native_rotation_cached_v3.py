"""Checkpoint one screened native rotation family per requested source position.

No row is accepted: native production replay/difficulty and visual review remain
pending. Unsupported means no result from this declared grammar/search budget.
"""
from pathlib import Path
from collections import Counter,OrderedDict
import argparse,copy,hashlib,inspect,json,platform,time
import joblib,numpy,sklearn,shapely
import native_asset_variant_v3 as variant
import native_asset_joint_variant_v3 as joint
from native_asset_variant_v3 import prepare,options,candidate
from layout_learning_v3 import read,hashfile
from assembly_contact_audit_v3 import NativeGeometrySeamAudit
from learned_placement_audit_v3 import NativeLearnedPlacementAudit
from layout_novelty_v3 import LayoutNoveltyIndex

# Pure memoization only: keys retain their source objects and cached lists/maps
# are returned as copies. This cannot merge donors, modify proposal order, or
# change any geometry/contact/learned/native/novelty acceptance criterion.
import layout_learning_v3 as learning
from functools import lru_cache
import numpy as np

def install_pure_caches():
 original_shape_vector=learning.shape_vector; shape_cache={}
 def shape_vector_cached(template):
  key=id(template)
  if key not in shape_cache:shape_cache[key]=(template,original_shape_vector(template))
  return list(shape_cache[key][1])
 learning.shape_vector=shape_vector_cached
 original_pin_key=variant.pin_key;pin_cache={}
 def pin_key_cached(part,templates):
  template=templates[part['templateId']];key=id(template)
  if key not in pin_cache:pin_cache[key]=(template,original_pin_key(part,templates))
  return pin_cache[key][1]
 variant.pin_key=pin_key_cached
 original_feature_map=variant.feature_shape_map;feature_cache=OrderedDict()
 def feature_map_cached(source,target,indices):
  key=(id(source),id(target),tuple(indices))
  if key not in feature_cache:
   feature_cache[key]=(source,target,original_feature_map(source,target,indices))
   if len(feature_cache)>50000:feature_cache.popitem(last=False)
  else:feature_cache.move_to_end(key)
  value=feature_cache[key][2]
  return None if value is None else dict(value)
 variant.feature_shape_map=feature_map_cached
 original_world=variant.world
 @lru_cache(maxsize=10000)
 def world_cached(geometry,x,y,angle):return original_world(geometry,{'x':x,'y':y,'angle':angle})
 def world(geometry,part):return world_cached(geometry,part['x'],part['y'],part['angle'])
 variant.world=world;joint.world=world
 return {'shapeVectorByTemplate':shape_cache,'pinKeyByTemplate':pin_cache,'featureMapByTemplatePairAndIndices':feature_cache}
PURE_CACHES=install_pure_caches()
def install_model_cache(model):
 original_neighbors=model.neighbors
 @lru_cache(maxsize=20000)
 def neighbors_cached(shape,raw):return original_neighbors(np.frombuffer(raw,dtype=np.float64).reshape(shape))
 def neighbors(contexts):
  array=np.asarray(contexts,dtype=np.float64);distances,indices=neighbors_cached(array.shape,array.tobytes());return distances.copy(),indices.copy()
 model.neighbors=neighbors
 return neighbors_cached


def save(path,value):
 path.parent.mkdir(parents=True,exist_ok=True);temporary=path.with_suffix('.tmp');temporary.write_text(json.dumps(value,separators=(',',':')),encoding='utf8');temporary.replace(path)

def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',required=True,type=Path);p.add_argument('--first',type=int,default=1);p.add_argument('--last',type=int,default=300);p.add_argument('--output',required=True,type=Path);p.add_argument('--max-nodes',type=int,default=100000);p.add_argument('--structural-alternatives',type=int,default=8);p.add_argument('--seed-packs',nargs='*',type=Path,default=[]);a=p.parse_args();w=a.workspace.resolve();out=a.output.resolve();out.mkdir(parents=True,exist_ok=True);levels=read(w/'data/source500_native.json')['levels'];templates={t['id']:t for t in read(w/'data/source_templates.json')['templates']};sources={l['sourceId']:l for l in levels};targets=[l for l in levels if a.first<=l['position']<=a.last];extra=read(w/'data/novelty_extras/source_parity_pack.json')['levels'];nt=dict(templates)
 for t in read(w/'data/novelty_extras/source_templates.json')['templates']:nt[t['id']]=t
 modelpath=w/'models/composition_pose.joblib';model=joblib.load(modelpath)['model'];model_neighbor_cache=install_model_cache(model);sa=NativeGeometrySeamAudit(templates,levels);ia=NativeLearnedPlacementAudit(levels,templates,modelpath,w/'data/source500_native.json');modelhash=hashfile(modelpath);packhash=hashfile(w/'data/source500_native.json');sourcehashes={name:hashfile(w/'data'/name) for name in ['source500_native.json','source_templates.json']};snapshot=out/'tool_snapshots';snapshot.mkdir(exist_ok=True);toolhashes={}
 for name in ['stage_native_rotation_cached_v3.py','stage_native_rotation_v3.py','native_asset_variant_v3.py','native_asset_joint_variant_v3.py','layout_learning_v3.py','assembly_contact_audit_v3.py','assembly_integrity_audit_v3.py','learned_placement_audit_v3.py','native_feature_socket_v3.py','layout_novelty_v3.py']:
  src=Path(__file__).with_name(name);data=src.read_bytes();sha=hashlib.sha256(data).hexdigest();(snapshot/(sha[:12]+'_'+name)).write_bytes(data);toolhashes[name]=sha
 frozen_hashes={'sources':sourcehashes,'model':modelhash,'tools':toolhashes};optionhash=hashlib.sha256(inspect.getsource(options).encode()).hexdigest();started=time.monotonic();rows={};staged={};coveragepath=out/'coverage.json'
 if coveragepath.exists():
  old=read(coveragepath)
  if old.get('hashes',{}).get('sources')!=sourcehashes or old.get('hashes',{}).get('model')!=modelhash:raise ValueError('Existing staging corpus/model hashes changed; use a new output directory')
  rows={int(r['position']):r for r in old['levels']};staged={int(l['position']):l for l in read(out/'proposals.json')['levels']}
  prior_executions=old.get('executionHashes',[])+[old.get('hashes')]
  for row in rows.values():
   if row['status']=='SEARCHING':row.update(status='UNPROCESSED',resumedInterruptedSearch=True)
 else:prior_executions=[]
 for ref in targets:rows.setdefault(ref['position'],{'position':ref['position'],'sourceId':ref['sourceId'],'status':'UNPROCESSED','accepted':False})
 def checkpoint():
  data=[staged[k] for k in sorted(staged)];statuscounts=Counter(r['status'] for r in rows.values());coverage={'complete':False,'acceptedGeneratedCount':0,'targetReferencePositions':[a.first,a.last],'trainingSourceCount':len(levels),'noveltyExclusionCount':len(levels)+len(extra),'stagedReferencePositions':sorted(staged),'stagedCount':len(data),'statusCounts':dict(statuscounts),'levels':[rows[k] for k in sorted(rows)],'declaredSearch':{'maxCSPNodesPerReference':a.max_nodes,'maxStructuralAlternativesPerReference':a.structural_alternatives,'minimumChangedRootFraction':.2,'maximumChangedRootFraction':.25,'pinDensityRatio':[.9,1.1],'pureCacheLimits':{'worldGeometry':10000,'nativeFeatureMaps':50000,'modelNeighborBatches':20000},'allSourceNativeVocabulary':True,'conditionalNativeModelNeighbors':32,'oneStagedFamilyPerReference':True,'gatesUnchanged':['wholeNativeAssetInputRigidbodyState','guideClosure','rawNativeWorldGeometry','actualHeadBodyContacts','wholeLayoutNovelty507','stagedFamilyCrossNovelty']},'limits':['Staging is not accepted generation or physical certification.','Every staged reference still requires current native production replay, source-relative difficulty calibration and visual review.','Unsupported means no candidate from this grammar and declared budget, not an impossibility theorem.','All original guide/bomb/chain/key mechanics are retained; this family keeps their roots unchanged.'],'runtime':{'python':platform.python_version(),'numpy':numpy.__version__,'sklearn':sklearn.__version__,'shapely':shapely.__version__,'elapsedSeconds':time.monotonic()-started,'pureMemoizationCacheSizes':{k:len(v) for k,v in PURE_CACHES.items()}},'hashes':frozen_hashes,'executionHashes':prior_executions};save(out/'proposals.json',{'schemaVersion':3,'complete':False,'acceptedCount':0,'status':'STAGED_NATIVE_GEOMETRY_NOVELTY_ONLY_CURRENT_PHYSICS_REPLAY_DIFFICULTY_VISUAL_PENDING','sourceCount':500,'levels':data,'hashes':frozen_hashes});save(coveragepath,coverage)
 def screen(c,position,origin):
  c=copy.deepcopy(c);native=ia.check(c)
  if not native['nativeLearnedPlacementIntegrityPassed']:return None,{'gate':'nativeIntegrity','errors':native['errors']}
  raw=sa.check(c)
  if not raw['rawGeometrySeamPassed']:return None,{'gate':'rawNativeGeometry','solidPairs':raw['unexpectedSolidOverlapCount'],'sensorPartners':raw['unregisteredSensorPartnerCount']}
  novelty=LayoutNoveltyIndex(levels+extra,nt).check(c)
  if not novelty['passed']:return None,{'gate':'sourceNovelty507','nearCopies':novelty['nearCopies']}
  if staged:
   portfolio=LayoutNoveltyIndex(list(staged.values()),nt).check(c)
   if not portfolio['passed']:return None,{'gate':'stagedFamilyCrossNovelty','nearCopies':portfolio['nearCopies']}
  else:portfolio={'passed':True,'sourcesCompared':0}
  old_id=c['id'];c.update(id=7800000+position,sourceId='staged-native-position-'+str(position),complete=False,validationStatus='STAGED_NATIVE_GEOMETRY_NOVELTY_ONLY_REPLAY_DIFFICULTY_VISUAL_PENDING');c['stagingProvenance']={'originalCandidateId':old_id,'origin':origin,'sourceReferencePosition':position,'sourceReferenceId':next(l['sourceId'] for l in targets if l['position']==position),'nativeAssetIntegrity':native,'rawNativeGeometry':raw,'novelty507':novelty,'crossStagedNovelty':portfolio,'nativePhysicsStatus':'PENDING_CURRENT_RULES_REPLAY','sourceRelativeDifficultyStatus':'PENDING','visualReviewStatus':'PENDING','accepted':False,'hashes':frozen_hashes};return c,None
 # Prefer a single already reviewed research family per reference; all gates
 # are recomputed here with current source tools before it enters staging.
 for seedpath in a.seed_packs:
  for c in read(seedpath)['levels']:
   position=c['position']
   if position not in rows or position in staged:continue
   screened,rejection=screen(c,position,{'kind':'frozenResearchPack','path':str(seedpath.resolve()),'sha256':hashfile(seedpath)})
   if screened:
    staged[position]=screened;rows[position].update(status='STAGED_PRELIMINARY',levelId=screened['id'],accepted=False,origin='frozenResearchPack');save(out/'by_position'/f'{position:03d}.json',{'complete':False,'acceptedCount':0,'levels':[screened]});print('SEEDED',position,screened['id'],flush=True)
 checkpoint();vocab,_,local=prepare(levels,templates)
 for ref in targets:
  position=ref['position']
  if rows[position]['status']!='UNPROCESSED':continue
  row=rows[position];row.update(status='SEARCHING',startedElapsedSeconds=time.monotonic()-started);checkpoint();cache=out/'options_cache'/f'{position:03d}.json';print('SEARCH_REFERENCE',position,ref['sourceId'],len(ref['parts']),flush=True)
  try:
   import math
   protected={i for e in ref['guideLinks'] for i in [e['guideOwnerId'],e['lockedOwnerId']]}|{b['ownerPartId'] for b in ref['bombs']}
   eligible=[p['id'] for p in ref['parts'] if p['id'] not in protected and not variant.special_root(p)]
   minimum=max(1,math.ceil(len(ref['parts'])*.2))
   if len(eligible)<minimum:
    row.update(status='UNSUPPORTED_CURRENT_GRAMMAR_BUDGET',unsupportedReason='insufficient_non_special_roots_for_declared_20percent_changes',eligibleNonSpecialRootIds=eligible,minimumChangedRoots=minimum,nativeRootCount=len(ref['parts']),accepted=False,finishedElapsedSeconds=time.monotonic()-started)
    checkpoint();print('REFERENCE_RESULT',position,row['status'],'capacity_precheck',len(eligible),minimum,'staged',len(staged),flush=True);continue
   if cache.exists() and read(cache).get('optionsFunctionSha256')==optionhash:
    saved=read(cache);opts={int(k):v for k,v in saved['options'].items()};reject=saved['optionRejections']
   else:
    opts,reject=options(ref,templates,vocab,sources,local,rotate_from_patterns=True,model=model);save(cache,{'optionsFunctionSha256':optionhash,'sourcePackSha256':packhash,'options':opts,'optionRejections':dict(reject)})
   chosen,search=joint.solve(ref,opts,templates,local,a.structural_alternatives,a.max_nodes);rejections=[];row.update(editableRootCount=len(opts),nativeRootCount=len(ref['parts']),sourceMechanics=dict(Counter(p['kind'] for p in ref['parts'])),sourceBombs=len(ref['bombs']),sourceChainRoots=sum(p['chainLockCount']>0 for p in ref['parts']),sourceGuideBindings=len(ref['guideLinks']),optionRejections=dict(reject),search=search)
   for index,choice in enumerate(chosen):
    try:c=candidate(ref,opts,templates,sources,local,model,packhash,modelhash,7900000+position*100+index,0,selected_override=choice)
    except ValueError as e:rejections.append({'gate':'jointActualPinContact','detail':str(e)});continue
    screened,rejection=screen(c,position,{'kind':'currentNativeConditionalCSP','structuralAlternativeIndex':index})
    if rejection:rejections.append(rejection);continue
    staged[position]=screened;row.update(status='STAGED_PRELIMINARY',levelId=screened['id'],accepted=False,origin='currentNativeConditionalCSP');save(out/'by_position'/f'{position:03d}.json',{'complete':False,'acceptedCount':0,'levels':[screened]});break
   if position not in staged:row.update(status='UNSUPPORTED_CURRENT_GRAMMAR_BUDGET',accepted=False)
   row['candidateRejections']=rejections
  except Exception as e:row.update(status='SEARCH_ERROR',error=type(e).__name__+': '+str(e),accepted=False)
  row['finishedElapsedSeconds']=time.monotonic()-started;checkpoint();print('REFERENCE_RESULT',position,row['status'],'staged',len(staged),'processed',sum(r['status'] not in ['UNPROCESSED','SEARCHING'] for r in rows.values()),flush=True)
 checkpoint();print('STAGING_FINISHED',len(staged),'accepted',0,flush=True)
if __name__=='__main__':main()
