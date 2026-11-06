"""Learn global source arrangement/style and local assembly motifs.

Synthetic perturbation ranking measures source-style consistency only. It does
not label fun, human difficulty, physical solvability, or an accepted new level.
"""
from pathlib import Path
from collections import Counter
import argparse,copy,hashlib,itertools,json,math,sys
import numpy as np
import networkx as nx
from shapely.geometry import Polygon,Point
from shapely import affinity,make_valid,union_all
from scipy.spatial import cKDTree
from sklearn.ensemble import ExtraTreesClassifier,ExtraTreesRegressor
from sklearn.linear_model import Ridge
from sklearn.metrics import roc_auc_score,mean_absolute_error
from sklearn.preprocessing import StandardScaler
from sklearn.neighbors import NearestNeighbors
import joblib
from layout_learning_v3 import read,hashfile,level_context,frame,shape_vector,SEED
NAMES=['logAspect','minorExtentWidthUnits','majorExtentWidthUnits','bboxWhitespace','hullWhitespace','symmetryX','symmetryY','symmetryPCA','orientationParallel','orientationCardinal','orientationDiagonal','nearestCenterP10','nearestCenterP50','nearestCenterP90','nearestOutlineP10','nearestOutlineP50','nearestOutlineP90','radialP10','radialP50','radialP90','centerCovAnisotropy','clusterCenterSpreadMajor','clusterCenterSpreadMinor','clusterNearestCenterP50','clusterPairDistanceCV','guideAxisOffsetP50','guidePerpendicularOffsetP50','angleNeighborAlignment','envelopeNestingFraction']
NEGATIVES=['smallJitter','clusterOrientation','clusterSpacing','grossPoseShuffle']
def graph_for(level):
 g=nx.DiGraph();g.add_nodes_from(p['id'] for p in level['parts']);g.add_edges_from((e['source'],e['target']) for e in level['links']);g.add_edges_from((e['guideOwnerId'],e['lockedOwnerId']) for e in level['guideLinks']);return g
def context(level):
 g=graph_for(level);n=max(1,len(g));return level_context(level)+[sum(g.in_degree(i)>1 for i in g)/n,sum(g.out_degree(i)>1 for i in g)/n,sum(len(c)>1 for c in nx.strongly_connected_components(g))/n,len(list(nx.weakly_connected_components(g)))/n,max(dict(g.in_degree()).values(),default=0),max(dict(g.out_degree()).values(),default=0)]
class Geometry:
 def __init__(self,templates):self.templates=templates;self.cache={}
 def body(self,p):
  tid=p['templateId']
  if tid not in self.cache:
   geometries=[]
   for s in self.templates[tid]['physicalShapes']:
    if s['role']!='body':continue
    paths=s.get('paths',[{'points':s.get('points',[])}]);geometries.append(make_valid(Polygon([(v['x'],v['y']) for v in paths[0]['points']], [[(v['x'],v['y']) for v in path['points']] for path in paths[1:]])))
   self.cache[tid]=union_all(geometries)
  return affinity.translate(affinity.rotate(self.cache[tid],p['angle'],origin=(0,0)),p['x'],p['y'])
def quantiles(v):return list(np.quantile(v,[.1,.5,.9])) if len(v) else [0.,0.,0.]
def arrangement(level,templates,geo,authored=None):
 ps={p['id']:p for p in level['parts']};mov=[p for p in ps.values() if p['kind'] in ['ring','slider']];xy=np.array([[p['x'],p['y']] for p in mov]);center=xy.mean(axis=0);unit=max(.01,float(np.median([templates[p['templateId']].get('width',0) for p in mov])));centered=(xy-center)/unit;values,vectors=np.linalg.eigh(np.cov(centered.T));axes=vectors[:,np.argsort(values)[::-1]];principal=centered@axes;extents=np.ptp(principal,axis=0);major=max(extents[0],.01);minor=max(extents[1],.01);pca=math.degrees(math.atan2(axes[1,0],axes[0,0]));angles=np.radians([p['angle']-pca for p in mov]);tree=cKDTree(xy);distance,indices=tree.query(xy,k=min(len(mov),5));nearest=distance[:,1]/unit if len(mov)>1 else np.array([0.]);bodies={p['id']:geo.body(p) for p in mov};gaps=[];alignment=[];nested=0
 for i,p in enumerate(mov):
  for j in np.atleast_1d(indices[i])[1:]:
   q=mov[int(j)];gaps.append(bodies[p['id']].distance(bodies[q['id']])/unit);alignment.append(abs(math.cos(math.radians(p['angle']-q['angle']))));nested+=int(bodies[p['id']].envelope.contains(Point(q['x'],q['y'])))
 if authored:
  comp=authored['composition'];whitespace=[comp['boundingWhitespaceFraction'],comp['convexHullWhitespaceFraction']];symmetry=[comp['symmetry'][k] for k in ['worldXMeanCenterErrorInWidthUnits','worldYMeanCenterErrorInWidthUnits','principalAxisMeanCenterErrorInWidthUnits']]
 else:
  allbody=union_all(list(bodies.values()));b=allbody.bounds;bbox=(b[2]-b[0])*(b[3]-b[1]);hull=allbody.convex_hull.area;whitespace=[1-allbody.area/max(bbox,.001),1-allbody.area/max(hull,.001)];symmetry=[]
  for axis in [np.array([1.,0.]),np.array([0.,1.]),axes[:,0]]:
   normal=np.array([-axis[1],axis[0]]);errors=[]
   for i,p in enumerate(mov):
    mirror=xy[i]-2*np.dot(xy[i]-center,normal)*normal;errors.append(min(np.linalg.norm(mirror-xy[j]) for j,q in enumerate(mov) if q['kind']==p['kind'])/unit)
   symmetry.append(float(np.mean(errors)))
 # Use sorted world-axis errors to remove an arbitrary 90deg board rotation.
 symmetry[:2]=sorted(symmetry[:2])
 graph=graph_for(level);clusters=list(nx.weakly_connected_components(graph));clustercenters=[]
 for cluster in clusters:
  members=[ps[i] for i in cluster if ps[i]['kind'] in ['ring','slider']]
  if members:clustercenters.append(np.mean([[p['x'],p['y']] for p in members],axis=0))
 clustercenters=np.array(clustercenters);clusterprincipal=(clustercenters-center)@axes/unit
 if len(clustercenters)>1:
  pair=np.linalg.norm(clustercenters[:,None,:]-clustercenters[None,:,:],axis=2);nonzero=pair[np.triu_indices(len(pair),1)];nearestcluster=np.partition(pair,1,axis=1)[:,1];clusterstats=[float(np.std(clusterprincipal[:,0])),float(np.std(clusterprincipal[:,1])),float(np.median(nearestcluster))/unit,float(np.std(nonzero)/max(np.mean(nonzero),.001))]
 else:clusterstats=[0.,0.,0.,0.]
 guideaxes=[];guideperps=[]
 for e in level['guideLinks']:
  a,b=ps[e['lockedOwnerId']],ps[e['guideOwnerId']];v=frame(b['x']-a['x'],b['y']-a['y'],a['angle'])
  if a['kind']=='slider':
   axis=(0.,1.) if templates[a['templateId']]['movement']['lockDirectionType']==0 else (-1.,0.);guideaxes.append(abs(v[0]*axis[0]+v[1]*axis[1])/unit);guideperps.append(abs(-v[0]*axis[1]+v[1]*axis[0])/unit)
 style=[math.log(major/minor),minor,major,*whitespace,*symmetry,float(np.mean(np.abs(np.cos(angles)))),float(np.mean(np.abs(np.cos(2*angles)))),float(np.mean(np.abs(np.sin(2*angles)))),*quantiles(nearest),*quantiles(gaps),*quantiles(np.linalg.norm(centered,axis=1)),float(max(values)/max(sum(values),.001)),*clusterstats,float(np.median(guideaxes)) if guideaxes else 0.,float(np.median(guideperps)) if guideperps else 0.,float(np.mean(alignment)) if alignment else 0.,nested/max(1,len(gaps))]
 assert len(style)==len(NAMES),(len(style),len(NAMES));return np.asarray(style,float)
def mutate(level,kind,rng,templates):
 out=copy.deepcopy(level);ps={p['id']:p for p in out['parts']};mov=[p for p in ps.values() if p['kind'] in ['ring','slider']]
 if kind=='grossPoseShuffle':
  poses=[(p['x'],p['y'],p['angle']) for p in mov]
  for p,i in zip(mov,rng.permutation(len(mov))):p['x'],p['y'],p['angle']=poses[i]
  return out
 if kind=='smallJitter':
  unit=max(.01,float(np.median([templates[p['templateId']].get('width',0) for p in mov])))
  for p in ps.values():p['x']+=float(rng.normal(0,.08*unit));p['y']+=float(rng.normal(0,.08*unit))
  return out
 components=[set(c) for c in nx.weakly_connected_components(graph_for(out)) if any(ps[i]['kind'] in ['ring','slider'] for i in c)]
 if len(components)<2:
  # A single authored connected component has no intercluster placement.
  # Use two spatial halves as a marked synthetic perturbation only.
  pts=np.array([[p['x'],p['y']] for p in ps.values()]);axis=int(np.argmax(np.ptp(pts,axis=0)));ordered=sorted(ps,key=lambda i:ps[i][['x','y'][axis]]);mid=max(1,len(ordered)//2);components=[set(ordered[:mid]),set(ordered[mid:])];components=[c for c in components if c]
 center=np.mean([[p['x'],p['y']] for p in ps.values()],axis=0)
 if kind=='clusterSpacing':
  factor=float(rng.choice([.88,1.12]))
  for component in components:
   c=np.mean([[ps[i]['x'],ps[i]['y']] for i in component],axis=0);shift=(c-center)*(factor-1)
   for i in component:ps[i]['x']+=float(shift[0]);ps[i]['y']+=float(shift[1])
  return out
 if kind=='clusterOrientation':
  component=components[int(rng.integers(len(components)))];c=np.mean([[ps[i]['x'],ps[i]['y']] for i in component],axis=0);delta=float(rng.choice([-18.,18.]));r=math.radians(delta);matrix=np.array([[math.cos(r),-math.sin(r)],[math.sin(r),math.cos(r)]])
  for i in component:
   p=ps[i];v=matrix@(np.array([p['x'],p['y']])-c)+c;p['x'],p['y']=map(float,v);p['angle']+=delta
  return out
 raise ValueError(kind)
class GlobalStylePrior:
 def fit(self,contexts,styles):
  self.contexts=np.asarray(contexts);self.styles=np.asarray(styles);self.context_scaler=StandardScaler().fit(self.contexts);self.style_scaler=StandardScaler().fit(self.styles);self.nn=NearestNeighbors(n_neighbors=min(24,len(styles))).fit(self.context_scaler.transform(self.contexts));return self
 def sample_target(self,context,rng):
  d,indices=self.nn.kneighbors(self.context_scaler.transform([context]));w=np.exp(-.5*d[0]**2);w/=sum(w);i=int(rng.choice(indices[0],p=w));return {'globalStyle':dict(zip(NAMES,self.styles[i].tolist())),'sourceStyleRecord':i,'meaning':'learned global arrangement target statistics; not a whole source layout or a playable level'}
def local_motifs(levels,templates):
 library=[]
 for level in levels:
  ps={p['id']:p for p in level['parts']};graph=graph_for(level).to_undirected();seen=set()
  for center in graph:
   neighbors=list(graph.neighbors(center));neighbors.sort(key=lambda i:(ps[i]['x']-ps[center]['x'])**2+(ps[i]['y']-ps[center]['y'])**2)
   selected={center,*neighbors[:2]}
   if len(selected)<2 or tuple(sorted(selected)) in seen:continue
   seen.add(tuple(sorted(selected)));owner=ps[center];parts=[]
   for i in sorted(selected):
    p=ps[i];xy=frame(p['x']-owner['x'],p['y']-owner['y'],owner['angle']);parts.append({'sourcePartId':i,'kind':p['kind'],'templateId':p['templateId'],'x':xy[0],'y':xy[1],'angle':(p['angle']-owner['angle'])%360,'collectible':p['collectible'],'chainLockCount':p['chainLockCount']})
   library.append({'sourceId':level['sourceId'],'position':level['position'],'localOwner':center,'parts':parts,'links':[e for e in level['links'] if e['source'] in selected and e['target'] in selected],'guideLinks':[e for e in level['guideLinks'] if e['guideOwnerId'] in selected and e['lockedOwnerId'] in selected],'meaning':'small authored motif for learned reassembly; native seam and whole-layout novelty validation required'})
 return library
def train(workspace):
 data=workspace/'data';pack=read(data/'source500_native.json');levels=pack['levels'];templates={t['id']:t for t in read(data/'source_templates.json')['templates']};grammar={l['sourceId']:l for l in read(data/'source500_spatial_grammar.json')['levels']};manifest=read(workspace/'models/family_splits.json');geo=Geometry(templates);styles=[];contexts=[];negatives={k:[] for k in NEGATIVES};rng=np.random.default_rng(SEED)
 for i,l in enumerate(levels):
  # Identical extraction path for source and all synthetic variants prevents
  # a cached-source/preprocessing artifact from becoming a fake classifier.
  styles.append(arrangement(l,templates,geo));contexts.append(context(l))
  for kind in NEGATIVES:negatives[kind].append(arrangement(mutate(l,kind,rng,templates),templates,geo))
  if (i+1)%100==0:print('GLOBAL FEATURES',i+1,'/500',flush=True)
 styles=np.asarray(styles);contexts=np.asarray(contexts);negatives={k:np.asarray(v) for k,v in negatives.items()};byid={l['sourceId']:i for i,l in enumerate(levels)};folds=[]
 for split in manifest['splits']:
  train_idx=np.array([byid[i] for i in split['trainSourceIds']]);test_idx=np.array([byid[i] for i in split['heldOutSourceIds']]);scaler=StandardScaler().fit(styles[train_idx]);y=scaler.transform(styles[train_idx]);reg=ExtraTreesRegressor(n_estimators=256,max_depth=12,min_samples_leaf=3,random_state=SEED,n_jobs=1).fit(contexts[train_idx],y);baseline=Ridge(alpha=10).fit(contexts[train_idx],y);test_y=scaler.transform(styles[test_idx]);record={'fold':split['fold'],'trainLevels':len(train_idx),'heldOutLevels':len(test_idx),'styleNormalizedMAE':{'extraTreesConditionedGlobalStyle':float(mean_absolute_error(test_y,reg.predict(contexts[test_idx]))),'countMechanicsTopologyLinearBaseline':float(mean_absolute_error(test_y,baseline.predict(contexts[test_idx])))}}
  # All four synthetic variants are created after family assignment; no
  # variant of a held-out family is used to train the classifier.
  tx=np.concatenate([np.c_[contexts[train_idx],styles[train_idx]]]+[np.c_[contexts[train_idx],negatives[k][train_idx]] for k in NEGATIVES]);labels=np.r_[np.ones(len(train_idx)),np.zeros(len(train_idx)*len(NEGATIVES))];rank=ExtraTreesClassifier(n_estimators=256,max_depth=12,min_samples_leaf=5,class_weight='balanced',random_state=SEED,n_jobs=1).fit(tx,labels);source_scores=rank.predict_proba(np.c_[contexts[test_idx],styles[test_idx]])[:,1];record['syntheticPerturbationRanking']={}
  for kind in NEGATIVES:
   scores=rank.predict_proba(np.c_[contexts[test_idx],negatives[kind][test_idx]])[:,1];auc=roc_auc_score(np.r_[np.ones(len(test_idx)),np.zeros(len(test_idx))],np.r_[source_scores,scores]);wins=float(np.mean((source_scores>scores)+.5*(source_scores==scores)));record['syntheticPerturbationRanking'][kind]={'sourceVsPerturbedAUC':float(auc),'pairedSourceRanksHigherFraction':wins,'countMechanicsOnlyPairedBaselineAUC':.5}
  folds.append(record);print('GLOBAL FOLD',split['fold'],json.dumps(record),flush=True)
 prior=GlobalStylePrior().fit(contexts,styles);tx=np.concatenate([np.c_[contexts,styles]]+[np.c_[contexts,negatives[k]] for k in NEGATIVES]);labels=np.r_[np.ones(len(levels)),np.zeros(len(levels)*len(NEGATIVES))];ranker=ExtraTreesClassifier(n_estimators=256,max_depth=12,min_samples_leaf=5,class_weight='balanced',random_state=SEED,n_jobs=1).fit(tx,labels);motifs=local_motifs(levels,templates);joblib.dump({'globalStylePrior':prior,'syntheticPerturbationRanker':ranker,'contextDescription':'counts/mechanics/progression + proposed topology branch/merge/cluster statistics','featureNames':NAMES,'sourceGlobalStyles':styles,'sourceContexts':contexts,'sourceIds':[l['sourceId'] for l in levels],'sourceMotifs':motifs,'status':'global style and local motif proposal only; native assembly/replay required'},workspace/'models/global_composition.joblib',compress=3)
 report={'status':'global composition/style proxy; no human-quality or difficulty claim','sourceCount':len(levels),'sourceFamilies':manifest['conservativeGeometryTopologyFamilyCount'],'sourceLocalMotifCount':len(motifs),'featureNames':NAMES,'negativeFamilies':{'smallJitter':'independent Gaussian positional jitter sigma.08 source width unit, all counts/shapes/graph unchanged','clusterOrientation':'one authored weak cluster rotated ±18deg, keeping internal arrangement; single connected boards use a marked synthetic spatial half','clusterSpacing':'weak-cluster centroids scaled by.88/1.12 around boardcenter; internal geometry retained; single clusters use spatial halves','grossPoseShuffle':'movable whole poses randomly permuted; counts and geometry inventory retained'},'evaluation':'same5 whole-family folds as local spatial model, including54 similar200xx grids in onefamily; all negatives follow their parent family','hyperparametersDeclaredBeforeEvaluation':{'globalRegressor':'ExtraTrees256/depth12/leaf3','linearBaseline':'Ridge alpha10 on same counts/mechanics/topology inputs','syntheticRanker':'ExtraTrees256/depth12/leaf5 balanced','negativeSeed':SEED},'limitations':['Synthetic perturbations are deliberately altered source arrangements, not proven bad levels.','Hard perturbations can remain plausible and playable; low separation is reported unchanged.','Normalized MAE predicts global statistics, not exact levels or human preference.','Source motif library may include an entire tiny tutorial; whole-layout and cosmetic-equivalence rejection is mandatory before any accepted generation.','No source solution labels, human calibration or accepted new300pack.'],'folds':folds,'summary':{'styleNormalizedMAE':{k:float(np.mean([f['styleNormalizedMAE'][k] for f in folds])) for k in folds[0]['styleNormalizedMAE']},'syntheticRanking':{kind:{metric:float(np.mean([f['syntheticPerturbationRanking'][kind][metric] for f in folds])) for metric in folds[0]['syntheticPerturbationRanking'][kind]} for kind in NEGATIVES}},'hashes':{name:hashfile(path) for name,path in [('sourcePack',data/'source500_native.json'),('templates',data/'source_templates.json'),('spatialGrammar',data/'source500_spatial_grammar.json'),('familySplits',workspace/'models/family_splits.json'),('trainer',Path(__file__)),('model',workspace/'models/global_composition.joblib')]}}
 (workspace/'models/global_composition_evaluation.json').write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps(report['summary'],indent=2))
def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',type=Path,required=True);a=p.parse_args();train(a.workspace.resolve())
if __name__=='__main__':
 sys.modules.setdefault('global_composition_v3',sys.modules[__name__]);GlobalStylePrior.__module__='global_composition_v3';main()
