"""Learn a conditional empirical spatial grammar from all original families.

This model learns complete relation poses with native shape contexts. It does
not generate a game level, certify physics, or provide human difficulty labels.
"""
from pathlib import Path
from collections import Counter,defaultdict
import argparse,hashlib,json,math,copy
import numpy as np
import networkx as nx
from sklearn.model_selection import GroupKFold
from sklearn.preprocessing import StandardScaler
from sklearn.neighbors import NearestNeighbors
from scipy.special import logsumexp
import joblib
SEED=20261105
K=32
BANDWIDTH=np.array([.45,.45,.35,.35])
KINDS=['ring','slider','fixed','butterfly']
def read(p):return json.loads(Path(p).read_text(encoding='utf8'))
def hashfile(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def frame(dx,dy,angle):
 r=math.radians(angle);c,s=math.cos(r),math.sin(r);return [c*dx+s*dy,-s*dx+c*dy]
def shape_vector(t):
 points=[p for s in t['physicalShapes'] for path in s.get('paths',[{'points':s.get('points',[])}]) for p in path['points']]
 extent=[max(p[a] for p in points)-min(p[a] for p in points) if points else 0 for a in ['x','y']]
 return [t.get('radius',0),t.get('width',0),t.get('visualGap',0)/180,t.get('length',0),*extent,len(t.get('attachments',[])),len(t.get('pullVisual',{}).get('pathPoints',[]))]
def coarse_shape(t):
 # Geometry only: ignores source IDs, instance names, palette, materials,
 # collider IDs, global pose and attachment ordinal. Mirror invariant.
 v=shape_vector(t);atts=sorted((a['type'],round(a['stemLength'],1),bool(a.get('isInnerMount'))) for a in t.get('attachments',[]))
 return (t['kind'],tuple(round(x,1) for x in v),tuple(atts))
def family_key(level,templates):
 graph=nx.DiGraph()
 for p in level['parts']:graph.add_node(p['id'],shape=str(coarse_shape(templates[p['templateId']])))
 for e in level['links']:graph.add_edge(e['source'],e['target'],role='pin'+str(e['relationKind']))
 for e in level['guideLinks']:graph.add_edge(e['guideOwnerId'],e['lockedOwnerId'],role='guide')
 # Same source shape inventory + directed topology are conservatively one
 # family even when placements differ. All global mirrors/rotations and
 # cosmetic rearrangements therefore share a split family.
 return nx.weisfeiler_lehman_graph_hash(graph,node_attr='shape',edge_attr='role',iterations=6)
def join_spatial_families(levels,templates):
 """Conservative whole-layout siblings, including altered relation graphs."""
 parent=list(range(len(levels)))
 def find(i):
  while parent[i]!=i:parent[i]=parent[parent[i]];i=parent[i]
  return i
 def join(i,j):parent[find(j)]=find(i)
 topology={};buckets=defaultdict(list);spectra=[]
 for i,l in enumerate(levels):
  h=family_key(l,templates)
  if h in topology:join(i,topology[h])
  else:topology[h]=i
  ps=l['parts'];xy=np.array([[p['x'],p['y']] for p in ps]);d=np.linalg.norm(xy[:,None,:]-xy[None,:,:],axis=2);unit=max(1.,float(np.median([templates[p['templateId']].get('width',0) for p in ps])));spectrum=np.sort(d[np.triu_indices(len(ps),1)])/unit;spectra.append(spectrum);buckets[tuple(sorted(Counter(p['kind'] for p in ps).items()))].append(i)
 joins=[]
 for bucket in buckets.values():
  for offset,i in enumerate(bucket):
   for j in bucket[offset+1:]:
    diff=spectra[i]-spectra[j]
    if len(diff) and float(np.sqrt(np.mean(diff*diff)))<=.075 and float(np.max(np.abs(diff)))<=.25:
     join(i,j);joins.append([levels[i]['sourceId'],levels[j]['sourceId']])
 families={l['sourceId']:'source-layout-family-'+str(find(i)) for i,l in enumerate(levels)}
 return families,joins
def level_context(level):
 counts=Counter(p['kind'] for p in level['parts']);n=len(level['parts'])
 return [math.log1p(n),*[counts[k]/max(1,n) for k in KINDS],len(level['links'])/max(1,n),len(level['guideLinks'])/max(1,n),float(bool(level['bombs'])),sum(p['chainLockCount']>0 for p in level['parts'])/max(1,n),level['position']/500]
def relation_rows(level,templates):
 ps={p['id']:p for p in level['parts']};lc=level_context(level);graph=nx.DiGraph();graph.add_nodes_from(ps);graph.add_edges_from((e['source'],e['target']) for e in level['links']);rows=[]
 relations=[('pin',e['source'],e['target'],e['sourceShapeIndices'],e['relationKind']) for e in level['links']]+[('guide',e['lockedOwnerId'],e['guideOwnerId'],[],0) for e in level['guideLinks']]
 for relation,source,target,indices,rkind in relations:
  a,b=ps[source],ps[target];ta,tb=templates[a['templateId']],templates[b['templateId']];va,vb=shape_vector(ta),shape_vector(tb);unit=max(ta.get('width',0),tb.get('width',0),1.)
  kind=[int(a['kind']==k) for k in KINDS]+[int(b['kind']==k) for k in KINDS];base=[float(relation=='guide'),*kind,*lc];head=[0.,0.,0.]
  shapes=[ta['physicalShapes'][i] for i in indices if ta['physicalShapes'][i]['role']=='head_trigger']
  if shapes:
   pts=[p for s in shapes for path in s.get('paths',[{'points':s.get('points',[])}]) for p in path['points']]
   if pts:head=[float(np.mean([p['x'] for p in pts]))/unit,float(np.mean([p['y'] for p in pts]))/unit,1.]
  context=base+va+vb+head+[graph.in_degree(source),graph.out_degree(source),graph.in_degree(target),graph.out_degree(target),rkind]
  xy=frame(b['x']-a['x'],b['y']-a['y'],a['angle']);phi=math.radians(b['angle']-a['angle']);y=[xy[0]/unit,xy[1]/unit,math.cos(phi),math.sin(phi)]
  rows.append({'sourceId':level['sourceId'],'position':level['position'],'sourcePart':source,'targetPart':target,'relation':relation,'baseline':base,'context':context,'pose':y})
 return rows
class ConditionalPoseModel:
 """Nearest contexts + Gaussian kernel mixture over learned relation poses."""
 def fit(self,x,y):
  self.x=np.asarray(x,float);self.y=np.asarray(y,float);self.scaler=StandardScaler().fit(self.x);self.nn=NearestNeighbors(n_neighbors=min(K,len(self.y)),n_jobs=1).fit(self.scaler.transform(self.x));return self
 def neighbors(self,x):return self.nn.kneighbors(self.scaler.transform(np.asarray(x,float)),return_distance=True)
 def loglikelihood(self,x,y):
  distances,idx=self.neighbors(x);means=self.y[idx];z=(np.asarray(y)[:,None,:]-means)/BANDWIDTH;weights=-.5*distances**2;weights-=logsumexp(weights,axis=1)[:,None];normalizer=np.log(BANDWIDTH).sum()+2*math.log(2*math.pi)
  return logsumexp(weights-.5*np.sum(z*z,axis=2)-normalizer,axis=1)
 def reconstruction_error(self,x,y):
  distances,idx=self.neighbors(x);return np.min(np.linalg.norm(self.y[idx][...,:2]-np.asarray(y)[:,None,:2],axis=2),axis=1)
 def sample_pose(self,context,rng):
  d,i=self.neighbors([context]);logw=-.5*d[0]**2;w=np.exp(logw-logsumexp(logw));index=int(rng.choice(i[0],p=w));return {'pose':self.y[index].tolist(),'learnedRecordIndex':index,'method':'conditional empirical pose mixture; native physics not certified'}
def per_level(values,rows):
 out=defaultdict(list)
 for v,row in zip(values,rows):out[row['sourceId']].append(float(v))
 return {k:float(np.mean(v)) for k,v in out.items()}
def shuffled_layout(level,rng):
 out=copy.deepcopy(level);mov=[p for p in out['parts'] if p['kind'] in ['ring','slider']];poses=[(p['x'],p['y'],p['angle']) for p in mov];permutation=rng.permutation(len(mov))
 for p,i in zip(mov,permutation):p['x'],p['y'],p['angle']=poses[i]
 return out
def evaluate(pack,templates,out):
 levels=pack['levels'];families,spatialjoins=join_spatial_families(levels,templates);rows={l['sourceId']:relation_rows(l,templates) for l in levels};groups=np.array([families[l['sourceId']] for l in levels]);splitter=GroupKFold(n_splits=5);folds=[];split_manifest=[];all_levels={l['sourceId']:l for l in levels};rng=np.random.default_rng(SEED)
 for fold,(train,test) in enumerate(splitter.split(levels,groups=groups)):
  assert not (set(groups[train])&set(groups[test]))
  training=[r for i in train for r in rows[levels[i]['sourceId']]];held=[r for i in test for r in rows[levels[i]['sourceId']]];x=np.array([r['context'] for r in training]);base=np.array([r['baseline'] for r in training]);y=np.array([r['pose'] for r in training]);learned=ConditionalPoseModel().fit(x,y);baseline=ConditionalPoseModel().fit(base,y)
  record={'fold':fold,'trainingLevels':len(train),'heldOutLevels':len(test),'trainingFamilies':len(set(groups[train])),'heldOutFamilies':len(set(groups[test])),'trainingRelations':len(training),'heldOutRelations':len(held)}
  record['models']={};negative_rows={levels[i]['sourceId']:relation_rows(shuffled_layout(levels[i],rng),templates) for i in test}
  for name,model,key in [('learnedSpatialContext',learned,'context'),('countMechanicsOnlyBaseline',baseline,'baseline')]:
   hx=[r[key] for r in held];hy=[r['pose'] for r in held];nll=per_level(-model.loglikelihood(hx,hy),held);error=per_level(model.reconstruction_error(hx,hy),held);paired=[]
   for i in test:
    l=levels[i];negative=negative_rows[l['sourceId']]
    if not negative:continue
    score=float(np.mean(model.loglikelihood([r[key] for r in negative],[r['pose'] for r in negative])));source=-nll[l['sourceId']];paired.append(float(source>score))
   record['models'][name]={'meanLevelRelationNLL':float(np.mean(list(nll.values()))),'meanLevelBestOf32PoseXYErrorInWidthUnits':float(np.mean(list(error.values()))),'sourceBeatsOneSyntheticPoseShuffleFraction':float(np.mean(paired)),'perHeldOutLevelNLL':nll}
  folds.append(record);split_manifest.append({'fold':fold,'trainSourceIds':[levels[i]['sourceId'] for i in train],'heldOutSourceIds':[levels[i]['sourceId'] for i in test],'familyIntersection':sorted(set(groups[train])&set(groups[test]))});print('FOLD',fold,{k:{m:round(v,4) for m,v in q.items() if not isinstance(v,dict)} for k,q in record['models'].items()},flush=True)
 # Only after the family-separated evaluation above, refit all500 originals.
 full=[r for l in levels for r in rows[l['sourceId']]];final=ConditionalPoseModel().fit([r['context'] for r in full],[r['pose'] for r in full]);joblib.dump({'model':final,'relationRecords':full,'familyIds':families,'trainedSourceCount':len(levels),'physicsStatus':'unverified; generation blocked','method':'Conditional empirical full-relation pose proposal'},out/'models/composition_pose.joblib',compress=3)
 manifest={'sourceCount':len(levels),'sourceProvidedGroupsWereAllSingletons':len({l['sourceFamily'] for l in levels})==len(levels),'conservativeGeometryTopologyFamilyCount':len(set(groups)),'families':families,'familySizes':dict(Counter(families.values())),'spatialFamilyJoins':spatialjoins,'familyMethod':'Union of (1) directed native relation+guide graph, node geometry rounded0.1, mirror-invariant attachment geometry; (2) same kind inventory and invariant sorted whole-board pair-distance spectrum within RMS0.075/max0.25 width units, ignoring link/part-angle edits. Global rotation/reflection/color/names/sourceCollider IDs ignored. Conservative grouping may merge distinct layouts; heuristic, not a proof of all near-copy equivalence.','splits':split_manifest};(out/'models/family_splits.json').write_text(json.dumps(manifest,indent=2),encoding='utf8')
 summary={name:{metric:float(np.mean([f['models'][name][metric] for f in folds])) for metric in ['meanLevelRelationNLL','meanLevelBestOf32PoseXYErrorInWidthUnits','sourceBeatsOneSyntheticPoseShuffleFraction']} for name in ['learnedSpatialContext','countMechanicsOnlyBaseline']}
 return {'status':'composition learning only; no physical source labels or accepted generated levels','seed':SEED,'sourceCount':len(levels),'familyCount':len(set(groups)),'relationCount':len(full),'evaluation':'5-fold held-out whole source families, split before fitting; final model refit all500 after evaluation','hyperparametersDeclaredBeforeEvaluation':{'neighbors':K,'kernelBandwidthXYCosSin':BANDWIDTH.tolist(),'contextScaler':'StandardScaler fit train only'},'baseline':'Same empirical pose-mixture estimator with counts, mechanics, relation kind and progression only; no native per-asset geometry, attachment head placement or relation degree context','reconstructionMetric':'Oracle best of learned32 neighbor pose components; not single-prediction accuracy or a solution','syntheticNegatives':'Exactly one held-out random permutation of movable whole poses per source. Counts, mechanics and source native shapes identical. This tests gross composition corruption only; not human playability judgments.','humanDifficultyStatus':'unmeasured','nativePhysicsStatus':'pending; no accepted generation/export','summary':summary,'folds':folds}
def main():
 default_root=Path(__file__).resolve().parents[1];default_data=default_root/'data' if (default_root/'data/source500_native.json').exists() else default_root/'Research/physics_v3'
 p=argparse.ArgumentParser();p.add_argument('--source-root',type=Path,default=default_data);p.add_argument('--workspace',type=Path,required=True);a=p.parse_args();out=a.workspace.resolve();(out/'models').mkdir(exist_ok=True);local=(a.source_root/'source500_native.json').exists();packfile=a.source_root/('source500_native.json' if local else 'full500/source_parity_pack.json');tfile=a.source_root/('source_templates.json' if local else 'full500/source_templates.json');pack=read(packfile);templates={t['id']:t for t in read(tfile)['templates']};report=evaluate(pack,templates,out);report['hashes']={'sourcePack':hashfile(packfile),'templatePack':hashfile(tfile),'trainer':hashfile(__file__),'model':hashfile(out/'models/composition_pose.joblib'),'familySplits':hashfile(out/'models/family_splits.json')};(out/'models/composition_evaluation.json').write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps(report['summary'],indent=2))
if __name__=='__main__':
 import sys
 sys.modules.setdefault('layout_learning_v3',sys.modules[__name__])
 ConditionalPoseModel.__module__='layout_learning_v3'
 main()
