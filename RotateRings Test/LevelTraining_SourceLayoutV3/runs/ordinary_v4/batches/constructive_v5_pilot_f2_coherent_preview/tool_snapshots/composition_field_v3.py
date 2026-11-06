"""Learn complete board density/orientation fields, not aggregate statistics.

Each field retains kind-specific source center organization on a fixed 25x37
grid (columns, rows, bilateral groups, rings). A PCA basis is fit inside each
source-family fold. Generation decodes a mixture of different-family latent
records, then places complete native macro tiles against that whole-board field.
"""
from pathlib import Path
import argparse,json,math,sys
import numpy as np
import joblib
from scipy.ndimage import gaussian_filter,maximum_filter
from sklearn.decomposition import PCA
from sklearn.ensemble import ExtraTreesRegressor
from sklearn.linear_model import Ridge
from sklearn.preprocessing import StandardScaler
from sklearn.neighbors import NearestNeighbors
from shapely import points as shapely_points,distance as shapely_distance,union_all,affinity
from assembly_contact_audit_v3 import shape_polygon
from layout_learning_v3 import read,hashfile,SEED
from global_composition_v3 import context,Geometry

H,W=37,25
KINDS=['ring','slider','fixed','butterfly']
CHANNELS=10
_GEOMETRY={}

def native_body_fields(level,templates,heading,span,center,total_parts):
 """Blur exact native body outlines onto the field without filling ring holes.

Distances are a visual-composition feature only. They do not approximate Unity
casts, contact skin or solver state.
 """
 key=id(templates)
 if key not in _GEOMETRY:_GEOMETRY[key]={}
 geo=_GEOMETRY[key];ys,xs=np.mgrid[0:H,0:W];r=math.radians(heading);matrix=np.array([[math.cos(r),-math.sin(r)],[math.sin(r),math.cos(r)]]);xy=np.c_[((xs.ravel()/(W-1))*1.2-.6)*span,((ys.ravel()/(H-1))*1.2-.6)*span]+np.array(center);world=xy@matrix.T;samplepoints=shapely_points(world[:,0],world[:,1]);maps=np.zeros((4,H,W));sigma=max(span/(W-1)*.45,.01)
 for p in level['parts']:
  tid=p['templateId']
  if tid not in geo:geo[tid]=union_all([shape_polygon(s) for s in templates[tid]['physicalShapes'] if not s.get('trigger',False) and s.get('sourceEnabled',True)])
  body=affinity.translate(affinity.rotate(geo[tid],p['angle'],origin=(0,0)),p['x'],p['y'])
  if body.is_empty:continue
  distances=shapely_distance(samplepoints,body);weights=np.exp(-.5*(distances/sigma)**2).reshape(H,W);weights/=max(weights.sum(),1e-12);maps[KINDS.index(p['kind'])]+=weights/max(1,total_parts)
 return maps

def relation_fields(level,heading,span,center,total_parts):
 maps=np.zeros((2,H,W));ps={p['id']:p for p in level['parts']};r=math.radians(heading);matrix=np.array([[math.cos(r),-math.sin(r)],[math.sin(r),math.cos(r)]])
 relations=[(0,e['source'],e['target']) for e in level['links']]+[(1,e['guideOwnerId'],e['lockedOwnerId']) for e in level['guideLinks']]
 for kind,a,b in relations:
  xy=(np.array([[ps[a]['x'],ps[a]['y']],[ps[b]['x'],ps[b]['y']]])@matrix-np.array(center))/max(span,.01);pix=(xy+.6)/1.2*np.array([W-1,H-1]);steps=max(2,int(np.linalg.norm(pix[1]-pix[0]))*2);segment=np.linspace(pix[0],pix[1],steps);weight=.5/max(1,total_parts)/steps
  for x,y in segment:
   col,row=int(round(x)),int(round(y))
   if 0<=col<W and 0<=row<H:maps[kind,row,col]+=weight
 for k in range(2):maps[k]=gaussian_filter(maps[k],sigma=.7)
 return maps

def stroke_heading(level):
 angles=np.radians([p['angle'] for p in level['parts'] if p['kind'] in ['ring','slider']]);vector=np.mean(np.exp(4j*angles))
 # Source rows and columns have a strong fourth harmonic. Radial layouts have
 # weak orientation; preserve the supplied original source coordinate frame.
 return math.degrees(np.angle(vector))/4 if abs(vector)>.25 else 0.

def native_frame(level,templates,heading):
 r=math.radians(heading);matrix=np.array([[math.cos(r),-math.sin(r)],[math.sin(r),math.cos(r)]]);points=[]
 for p in level['parts']:
  a=math.radians(p['angle']);rotation=np.array([[math.cos(a),-math.sin(a)],[math.sin(a),math.cos(a)]])
  for shape in templates[p['templateId']]['physicalShapes']:
   if shape.get('trigger',False) or not shape.get('sourceEnabled',True):continue
   for path in shape.get('paths',[{'points':shape.get('points',[])}]):
    if path['points']:points.extend(np.array([[v['x'],v['y']] for v in path['points']])@rotation.T+np.array([p['x'],p['y']]))
 if not points:points=[[p['x'],p['y']] for p in level['parts']]
 posed=np.asarray(points)@matrix;low=posed.min(axis=0);high=posed.max(axis=0);return low,high,float(max(high-low)),(low+high)/2,matrix

def xy_in_frame(level,heading,templates):
 low,high,span,center,matrix=native_frame(level,templates,heading);xy=np.array([[p['x'],p['y']] for p in level['parts']])@matrix;return (xy-center)/max(span,.01),span,center,matrix

def field(level,templates,heading=None):
 heading=stroke_heading(level) if heading is None else heading;xy,span,center,matrix=xy_in_frame(level,heading,templates);out=np.zeros((4,H,W));hist=np.zeros((4,24))
 for p,(x,y) in zip(level['parts'],xy):
  k=KINDS.index(p['kind']);col=int(np.clip(round((x+.6)/1.2*(W-1)),0,W-1));row=int(np.clip(round((y+.6)/1.2*(H-1)),0,H-1));out[k,row,col]+=1;hist[k,int(((p['angle']-heading)%360)/15)%24]+=1
 for k in range(4):
  out[k]=gaussian_filter(out[k],sigma=1.0);out[k]/=max(1,len(level['parts']));hist[k]/=max(1,len(level['parts']))
 # Scale both groups to comparable total mass. Latent modeling sees the full
 # maps, rather than only symmetry/mean spacing summaries.
 bodies=native_body_fields(level,templates,heading,span,center,len(level['parts']));edges=relation_fields(level,heading,span,center,len(level['parts']));return np.r_[out.ravel(),bodies.ravel(),edges.ravel(),hist.ravel()*.2],{'heading':heading,'span':span,'center':center.tolist()}

def field_context(level,templates):
 values=[]
 for k in KINDS:
  ts=[templates[p['templateId']] for p in level['parts'] if p['kind']==k];values.extend([float(np.mean([t.get(name,0) for t in ts])) if ts else 0. for name in ['radius','width','length','gap']])
 return context(level)+values

def posed_field(level,heading,span,total_parts,templates):
 """Fixed proposal board frame: do not resize/recenter after each placement."""
 out=np.zeros((4,H,W));hist=np.zeros((4,24));r=math.radians(heading);matrix=np.array([[math.cos(r),-math.sin(r)],[math.sin(r),math.cos(r)]])
 for p in level['parts']:
  x,y=np.array([p['x'],p['y']])@matrix/max(span,.01);k=KINDS.index(p['kind']);col=(x+.6)/1.2*(W-1);row=(y+.6)/1.2*(H-1)
  if 0<=col<W and 0<=row<H:out[k,int(round(row)),int(round(col))]+=1/max(total_parts,1)
  hist[k,int(((p['angle']-heading)%360)/15)%24]+=1/max(total_parts,1)
 for k in range(4):out[k]=gaussian_filter(out[k],sigma=1.)
 bodies=native_body_fields(level,templates,heading,span,[0.,0.],total_parts);edges=relation_fields(level,heading,span,[0.,0.],total_parts);return np.concatenate([out,bodies,edges]),hist

def frontier_centers(target,current,tile_parts,rotation,total_parts,limit=4):
 """Cross-correlate an exact macro center pattern with learned residual field.

The grid belongs to the learned density representation; no source part is moved
onto an invented grid or snapped after placement.
 """
 heading=target['heading'];span=target['span'];angle=math.radians(rotation-heading);matrix=np.array([[math.cos(angle),-math.sin(angle)],[math.sin(angle),math.cos(angle)]]);centroid=np.mean([[p['x'],p['y']] for p in tile_parts],axis=0);delta=(np.array([[p['x'],p['y']] for p in tile_parts])-centroid)@matrix.T/span;residual=target['field'][:4]-current[:4];ys,xs=np.mgrid[0:H,0:W];gain=np.zeros((H,W))
 for p,(dx,dy) in zip(tile_parts,delta):
  sx=np.rint(xs+dx/1.2*(W-1)).astype(int);sy=np.rint(ys+dy/1.2*(H-1)).astype(int);valid=(sx>=0)&(sx<W)&(sy>=0)&(sy<H);scores=np.full((H,W),-1.);scores[valid]=residual[KINDS.index(p['kind']),sy[valid],sx[valid]];gain+=scores
 chosen=[];work=gain.copy()
 for _ in range(limit):
  row,col=np.unravel_index(np.argmax(work),work.shape)
  if not np.isfinite(work[row,col]):break
  frame=np.array([(col/(W-1)*1.2-.6)*span,(row/(H-1)*1.2-.6)*span]);r=math.radians(heading);world=np.array([[math.cos(r),-math.sin(r)],[math.sin(r),math.cos(r)]])@frame;chosen.append(world);work[max(0,row-2):min(H,row+3),max(0,col-2):min(W,col+3)]=-np.inf
 return chosen,centroid

class WholeBoardFieldModel:
 def fit(self,levels,templates,families):
  self.contexts=np.asarray([field_context(l,templates) for l in levels]);data=[field(l,templates) for l in levels];self.fields=np.asarray([r[0] for r in data]);self.frames=[r[1] for r in data];self.source_ids=[l['sourceId'] for l in levels];self.families=[families[s] for s in self.source_ids];self.pca=PCA(n_components=32,random_state=SEED,svd_solver='randomized').fit(self.fields);self.latents=self.pca.transform(self.fields);self.scaler=StandardScaler().fit(self.contexts);self.nn=NearestNeighbors(n_neighbors=32).fit(self.scaler.transform(self.contexts));return self
 def sample(self,reference,templates,rng):
  d,ix=self.nn.kneighbors(self.scaler.transform([field_context(reference,templates)]));a=int(rng.choice(ix[0],p=np.exp(-.5*d[0]**2)/np.exp(-.5*d[0]**2).sum()));choices=[(j,dd) for j,dd in zip(ix[0],d[0]) if self.families[int(j)]!=self.families[a]]
  if not choices:raise RuntimeError('No different-family whole-board field neighbor; withhold this proposal')
  b=int(choices[int(rng.integers(len(choices)))][0]);weight=float(rng.uniform(.35,.65));latent=weight*self.latents[a]+(1-weight)*self.latents[b];target=np.maximum(0,self.pca.inverse_transform([latent])[0]);maps=target[:CHANNELS*H*W].reshape(CHANNELS,H,W)
  for lo,hi in [(0,4),(4,8)]:maps[lo:hi]/=max(maps[lo:hi].sum(),1e-10)
  heading=stroke_heading(reference);span=native_frame(reference,templates,heading)[2];return {'field':maps,'orientation':target[CHANNELS*H*W:].reshape(4,24)/.2,'latentSourceRecords':[self.source_ids[a],self.source_ids[b]],'families':[self.families[a],self.families[b]],'latentWeight':weight,'heading':heading,'span':span,'spanContract':'reference-conditioned complete native solid bounding span, no motif rescaling','basisExplainedVariance':float(self.pca.explained_variance_ratio_.sum())}

def fit(workspace):
 levels=read(workspace/'data/source500_native.json')['levels'];templates={t['id']:t for t in read(workspace/'data/source_templates.json')['templates']};splits=read(workspace/'models/family_splits.json');x=np.asarray([field_context(l,templates) for l in levels]);y=np.asarray([field(l,templates)[0] for l in levels]);by={l['sourceId']:i for i,l in enumerate(levels)};folds=[]
 for split in splits['splits']:
  tr=[by[s] for s in split['trainSourceIds']];te=[by[s] for s in split['heldOutSourceIds']];pca=PCA(n_components=32,svd_solver='randomized',random_state=SEED).fit(y[tr]);z=pca.transform(y[tr]);scaler=StandardScaler().fit(x[tr]);xt=scaler.transform(x[tr]);xh=scaler.transform(x[te]);learned=ExtraTreesRegressor(n_estimators=128,max_depth=12,min_samples_leaf=3,random_state=SEED,n_jobs=1).fit(xt,z);baseline=Ridge(alpha=10).fit(xt,z);record={'fold':split['fold'],'trainingLevels':len(tr),'heldOutLevels':len(te),'trainingPCAExplainedVariance':float(pca.explained_variance_ratio_.sum())}
  for name,pred in [('conditionalWholeField',pca.inverse_transform(learned.predict(xh))),('sameContextLinearBaseline',pca.inverse_transform(baseline.predict(xh))),('trainingMeanFieldBaseline',np.tile(y[tr].mean(axis=0),(len(te),1)))]:
   pred=np.maximum(pred,0);record[name]={'fullFieldMeanAbsoluteError':float(np.mean(np.abs(pred-y[te]))),'kindMapL1PerLevel':float(np.mean(np.abs(pred[:,:4*H*W]-y[te,:4*H*W]).sum(axis=1))),'bodyFootprintMapL1PerLevel':float(np.mean(np.abs(pred[:,4*H*W:8*H*W]-y[te,4*H*W:8*H*W]).sum(axis=1))),'relationMapL1PerLevel':float(np.mean(np.abs(pred[:,8*H*W:CHANNELS*H*W]-y[te,8*H*W:CHANNELS*H*W]).sum(axis=1)))}
  folds.append(record);print('FIELDFOLD',record,flush=True)
 final=WholeBoardFieldModel().fit(levels,templates,splits['families']);path=workspace/'models/whole_board_field.joblib';joblib.dump(final,path,compress=3);report={'status':'full source composition fields, no playability/quality labels','sourceCount':len(levels),'representation':{'grid':[H,W],'channels':[k+'CenterDensity' for k in KINDS]+[k+'NativeSolidFootprint' for k in KINDS]+['authoredPinRelationCenterline','authoredGuideRelationCenterline'],'gaussianSigmaCells':1.,'orientationBinsPerKind':24,'latentComponents':32,'sourceFrame':'dominant4fold native part-angle axis; weak radial distributions retain original source coordinate frame'},'generation':'decode .35–.65 convex latent mix of TWO different source families sampled from32near source contexts; no whole source layout replay, final whole-layout novelty stillmandatory','evaluation':'same5whole-familyfolds, PCA/scaler/model fit trainingonly, heldout actual complete fieldprediction','hyperparametersDeclaredBeforeEvaluation':{'PCA':32,'regressor':'ExtraTrees128/depth12/leaf3','baseline':'Ridge alpha10 onidenticalcontext','seed':SEED},'folds':folds,'summary':{name:{k:float(np.mean([f[name][k] for f in folds])) for k in folds[0][name]} for name in ['conditionalWholeField','sameContextLinearBaseline','trainingMeanFieldBaseline']},'limitations':['Fields retain native solid-footprint occupancy (including holes, stems and fixed guide walls) and authored relation centerline organization. Raster blur is a composition feature only; exact colliders and graph edges come from native macro assemblies.','Mean-field prediction scores do not measure whether a sampled latent mix is attractive or playable.','PCA mixtures can blur incompatible patterns; native contact, novelty and visual review remain required.','Final source difficulty calibration absent.'],'hashes':{'sourcePack':hashfile(workspace/'data/source500_native.json'),'templates':hashfile(workspace/'data/source_templates.json'),'families':hashfile(workspace/'models/family_splits.json'),'trainer':hashfile(Path(__file__)),'model':hashfile(path)}};(workspace/'models/whole_board_field_evaluation.json').write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps(report['summary'],indent=2))

def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',type=Path,required=True);a=p.parse_args();fit(a.workspace.resolve())
if __name__=='__main__':
 sys.modules.setdefault('composition_field_v3',sys.modules[__name__]);WholeBoardFieldModel.__module__='composition_field_v3';main()
