"""Joint constraints over empirical whole-native asset/angle variant choices."""
from pathlib import Path
from collections import Counter
import argparse,hashlib,inspect,itertools,json,math
import joblib
from shapely import union_all
from native_asset_variant_semantic_v3 import prepare,options,candidate,world
from layout_learning_v3 import read,hashfile
from assembly_contact_audit_v3 import NativeGeometrySeamAudit
from learned_placement_audit_v3 import NativeLearnedPlacementAudit

def solve(reference,opts,templates,local,max_solutions=4,max_nodes=200000):
 ps={p['id']:p for p in reference['parts']};roots=sorted(opts);n=len(ps);minimum=max(1,math.ceil(n*.2));preferred=max(minimum,math.ceil(n*.25));worldold={i:world(local[p['templateId']]['solid'],p) for i,p in ps.items()};prepared={};compatibility_cache={};reject=Counter();nodes=0;solutions=[]
 def data(i,index):
  key=(i,index)
  if key not in prepared:
   c=opts[i][index];p=c['part'];t=templates[p['templateId']];heads={j:world(g,p) for j,g in enumerate(local[p['templateId']]['shapes']) if t['physicalShapes'][j]['role']=='head_trigger'};prepared[key]={'choice':c,'body':world(local[p['templateId']]['body'],p),'solid':world(local[p['templateId']]['solid'],p),'heads':heads}
  return prepared[key]
 def touching(source,edge,target):
  heads=union_all([source['heads'][j] for j in edge['sourceShapeIndices'] if j in source['heads']]);return heads.distance(target['body'])<=1e-5
 def pair_compatible(a,b):
  ca,cb=a['choice'],b['choice'];i,j=ca['part']['id'],cb['part']['id']
  if ca['sourceId']==cb['sourceId']:
   ma={ca['donorId']:i,**{e['targetOriginalId']:e['edge']['target'] for e in ca['edges']}};mb={cb['donorId']:j,**{e['targetOriginalId']:e['edge']['target'] for e in cb['edges']}}
   if any(ma[k]!=mb[k] for k in ma.keys()&mb.keys()):return False
  if a['solid'].intersection(b['solid']).area>1e-5:return False
  for src,dst,other in [(a,b,j),(b,a,i)]:
   named=[row['edge'] for row in src['choice']['edges'] if row['edge']['target']==other]
   if any(not touching(src,e,dst) for e in named):return False
   if not named and any(h.intersection(dst['solid']).area>1e-5 for h in src['heads'].values()):return False
  return True
 def compatible(i,index,j,other_index):
  key=(i,index,j,other_index) if i<j else (j,other_index,i,index)
  if key not in compatibility_cache:compatibility_cache[key]=pair_compatible(data(i,index),data(j,other_index))
  return compatibility_cache[key]
 for count in range(min(preferred,len(roots)),minimum-1,-1):
  for subset in itertools.combinations(roots,count):
   selected=set(subset);domains={};fixed_edge_count=sum(e['source'] not in selected for e in reference['links'])
   for i in subset:
    domain=[]
    for index,c in enumerate(opts[i]):
     d=data(i,index);valid=True
     for e in reference['links']:
      if e['target']!=i or e['source'] in selected:continue
      p=ps[e['source']];t=templates[p['templateId']];head=world(union_all([local[p['templateId']]['shapes'][j] for j in e['sourceShapeIndices'] if t['physicalShapes'][j]['role']=='head_trigger']),p)
      if head.distance(d['body'])>1e-5:valid=False;break
     if valid:domain.append(index)
    domains[i]=domain
   if any(not d for d in domains.values()):reject['sourceIncomingContactEmptiesDomain']+=1;continue
   queue=[(i,j) for i in subset for j in subset if i!=j]
   while queue:
    i,j=queue.pop();kept=[index for index in domains[i] if any(compatible(i,index,j,k) for k in domains[j])]
    if len(kept)!=len(domains[i]):
     domains[i]=kept
     if not kept:break
     queue.extend((k,i) for k in subset if k not in [i,j])
   if any(not d for d in domains.values()):reject['pairwiseArcConsistencyEmptyDomain']+=1;continue
   active={}
   def visit(remaining):
    nonlocal nodes
    if len(solutions)>=max_solutions or nodes>=max_nodes:return
    edge_count=fixed_edge_count+sum(len(opts[i][index]['edges']) for i,index in active.items());low=edge_count+sum(min(len(opts[i][k]['edges']) for k in domain) for i,domain in remaining.items());high=edge_count+sum(max(len(opts[i][k]['edges']) for k in domain) for i,domain in remaining.items())
    if high<.9*len(reference['links']) or low>1.1*len(reference['links']):reject['sourceRelativePinDensity']+=1;return
    if not remaining:
     chosen={i:opts[i][index] for i,index in active.items()}
     if not any(c['dependencySymmetricDifference']>0 for c in chosen.values()):reject['noChangedActiveDependencyBranch']+=1;return
     signature=tuple((i,round(c['part']['angle']/15),tuple(c['newTargets'])) for i,c in sorted(chosen.items()))
     if not any(sig==signature for _,sig in solutions):solutions.append((chosen,signature))
     return
    i=min(remaining,key=lambda i:len(remaining[i]))
    for index in remaining[i]:
     nodes+=1;following={j:[k for k in domain if compatible(i,index,j,k)] for j,domain in remaining.items() if j!=i}
     if any(not domain for domain in following.values()):reject['jointNativeGeometryOrHeadContact']+=1;continue
     active[i]=index;visit(following);active.pop(i)
     if len(solutions)>=max_solutions or nodes>=max_nodes:return
   visit(domains)
   if len(solutions)>=max_solutions or nodes>=max_nodes:break
  if len(solutions)>=max_solutions or nodes>=max_nodes:break
 return [s for s,_ in solutions],{'visitedNativeOptionCombinations':nodes,'rejections':dict(reject),'solutions':len(solutions),'cachedPairwiseNativeChecks':len(compatibility_cache),'changedRootFractionRange':[.2,.25],'pinDensityRatioRange':[.9,1.1],'limits':'Static candidate contact/geometry constraints and source relative pin density are not native physics, difficulty or fun labels.'}

def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',required=True,type=Path);p.add_argument('--positions',nargs='+',type=int,default=[76]);p.add_argument('--solutions',type=int,default=4);p.add_argument('--max-nodes',type=int,default=200000);p.add_argument('--id-start',type=int,default=7000001);p.add_argument('--output',required=True,type=Path);a=p.parse_args();w=a.workspace;levels=read(w/'data/source500_native.json')['levels'];ts={t['id']:t for t in read(w/'data/source_templates.json')['templates']};vocab,sources,local=prepare(levels,ts);modelpath=w/'models/composition_pose.joblib';model=joblib.load(modelpath)['model'];sa=NativeGeometrySeamAudit(ts,levels);ia=NativeLearnedPlacementAudit(levels,ts,modelpath,w/'data/source500_native.json');out=[];reports=[];a.output.parent.mkdir(parents=True,exist_ok=True);optionhash=hashlib.sha256(inspect.getsource(options).encode()).hexdigest()
 for position in a.positions:
  ref=next(l for l in levels if l['position']==position);cache=a.output.parent/('native_options_pos'+str(position)+'.json')
  if cache.exists() and read(cache).get('optionsFunctionSha256')==optionhash:
   cached=read(cache);opts={int(i):v for i,v in cached['options'].items()};reject=cached['optionRejections'];print('CACHED_NATIVE_OPTIONS',position,flush=True)
  else:
   opts,reject=options(ref,ts,vocab,sources,local,rotate_from_patterns=True,model=model);cache.write_text(json.dumps({'sourcePackSha256':hashfile(w/'data/source500_native.json'),'optionsFunctionSha256':optionhash,'options':opts,'optionRejections':dict(reject)},separators=(',',':')))
  chosen,report=solve(ref,opts,ts,local,a.solutions,a.max_nodes);report.update(position=position,optionRejections=dict(reject));reports.append(report);print('JOINT',report,flush=True)
  for choice in chosen:
   try:c=candidate(ref,opts,ts,sources,local,model,hashfile(w/'data/source500_native.json'),hashfile(modelpath),a.id_start+len(out),0,selected_override=choice)
   except ValueError as e:print('CONSTRUCTION_REJECT',str(e),flush=True);continue
   c['preliminaryRawSeam']=sa.check(c);c['preliminaryLearnedIntegrity']=ia.check(c);out.append(c);print('JOINT_CANDIDATE',c['id'],c['preliminaryRawSeam']['rawGeometrySeamPassed'],c['preliminaryLearnedIntegrity']['nativeLearnedPlacementIntegrityPassed'],flush=True)
 a.output.write_text(json.dumps({'schemaVersion':3,'complete':False,'acceptedCount':0,'status':'JOINT_NATIVE_OPTION_EXPERIMENT_ONLY_REPLAY_DIFFICULTY_NOVELTY_VISUAL_REQUIRED','levels':out,'reports':reports,'hashes':{'sourcePack':hashfile(w/'data/source500_native.json'),'templates':hashfile(w/'data/source_templates.json'),'model':hashfile(modelpath),'jointTool':hashfile(Path(__file__)),'nativeOptionTool':hashfile(Path(__file__).with_name('native_asset_variant_v3.py'))}},separators=(',',':')))
if __name__=='__main__':main()
