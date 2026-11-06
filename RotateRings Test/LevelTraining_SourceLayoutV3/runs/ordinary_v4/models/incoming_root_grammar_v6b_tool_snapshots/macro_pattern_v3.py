"""Learn source macro continuation patterns in a gesture-independent frame.

Rows contain actual ordered source macro triples, their spatial adjacency and
native tile geometry. Targets are normalized by incoming macro spacing, so
repeated rows/columns/arches do not lose their scale across native stroke widths.
"""
from pathlib import Path
from collections import defaultdict,Counter
import argparse,json,math,sys
import numpy as np
import joblib
from layout_learning_v3 import ConditionalPoseModel,level_context,read,hashfile,SEED

def relation_context(reference,a,b,c,incoming_angle,incoming_distance):
 # All values are known at a proposal frontier before the next position is
 # sampled. No held-out target location enters context.
 phi=math.radians(b['heading']-incoming_angle);unit=max(a['width'],b['width'],c['width']);return level_context(reference)+a['descriptor']+b['descriptor']+c['descriptor']+[math.cos(phi),math.sin(phi),math.log(max(incoming_distance/unit,.001)),float(a['descriptor']==b['descriptor']),float(b['descriptor']==c['descriptor'])]

def rows_from_model(model):
 by=defaultdict(list)
 for i,t in enumerate(model.tiles):by[t['sourceId']].append((i,t))
 rows=[]
 for sid,entries in by.items():
  if len(entries)<3:continue
  level=model.levels[sid];points=np.array([t['center'] for _,t in entries]);dist=np.linalg.norm(points[:,None,:]-points[None,:,:],axis=2)
  for b,(_,tb) in enumerate(entries):
   neighbors=np.argsort(dist[b])[1:5]
   for a in neighbors[:3]:
    ia,ta=entries[int(a)];ib,_=entries[b];v=points[b]-points[a];spacing=float(np.linalg.norm(v))
    if spacing<1e-5:continue
    phi=math.atan2(v[1],v[0]);co,si=math.cos(phi),math.sin(phi)
    for c in neighbors:
     if c==a:continue
     ic,tc=entries[int(c)];out=points[c]-points[b];xy=[(co*out[0]+si*out[1])/spacing,(-si*out[0]+co*out[1])/spacing];angle=math.radians(tc['heading'])-phi;ctx=relation_context(level,ta,tb,tc,math.degrees(phi),spacing);pose=[*xy,math.cos(angle),math.sin(angle)];rows.append({'sourceId':sid,'sourceTile':ia,'centerTile':ib,'targetTile':ic,'context':ctx,'baseline':level_context(level)+[len(ta['partIds']),len(tb['partIds']),len(tc['partIds'])],'pose':pose,'patternFeatures':{'incomingSpacing':spacing,'turnDegrees':math.degrees(math.atan2(xy[1],xy[0])),'spacingRatio':math.hypot(*xy),'sharedNativeRootsAB':len(set(ta['partIds'])&set(tb['partIds'])),'sharedNativeRootsBC':len(set(tb['partIds'])&set(tc['partIds']))}})
 return rows

def train(workspace,macro_model_name='macro_assembly.joblib',output_name='macro_pattern.joblib',report_name='macro_pattern_evaluation.json'):
 model=joblib.load(workspace/'models'/macro_model_name);rows=rows_from_model(model);splits=read(workspace/'models/family_splits.json');folds=[]
 for split in splits['splits']:
  tr=set(split['trainSourceIds']);te=set(split['heldOutSourceIds']);train=[r for r in rows if r['sourceId'] in tr];test=[r for r in rows if r['sourceId'] in te];rec={'fold':split['fold'],'trainingTriples':len(train),'heldOutTriples':len(test)}
  for name,key in [('nativeMacroContinuation','context'),('countMechanicsProgressionBaseline','baseline')]:
   estimator=ConditionalPoseModel().fit([r[key] for r in train],[r['pose'] for r in train]);nll=-estimator.loglikelihood([r[key] for r in test],[r['pose'] for r in test]);errors=estimator.reconstruction_error([r[key] for r in test],[r['pose'] for r in test]);agg=defaultdict(list)
   for r,n,e in zip(test,nll,errors):agg[r['sourceId']].append([n,e])
   rec[name]={'levelMeanNLL':float(np.mean([np.mean(v,axis=0)[0] for v in agg.values()])),'levelMeanBest32XYErrorIncomingSpacingUnits':float(np.mean([np.mean(v,axis=0)[1] for v in agg.values()]))}
  folds.append(rec);print('PATTERNFOLD',split['fold'],rec,flush=True)
 final=ConditionalPoseModel().fit([r['context'] for r in rows],[r['pose'] for r in rows]);path=workspace/'models'/output_name;joblib.dump({'model':final,'records':rows,'sourceCount':len(model.levels),'status':'learned macro continuations only; no native validation'},path,compress=3);report={'sourceCount':len(model.levels),'sourceMacroPatternCount':len(rows),'sourceLevelsWithAtLeast3Tiles':len({r['sourceId'] for r in rows}),'status':'actual native macro triple learning; physical labels absent','targetNormalization':'outgoing macro vector / incoming macro spacing, angular target relative to incoming vector; rotation/translation invariant','selection':'up to4 spatial nearest macro neighbors; all ordered distinct incoming/outgoing frontier choices retained, no target-informed feature','evaluation':'same5 conservative whole-source-family folds; source triples follow parent source; no augmentation before split','hyperparametersDeclaredBeforeEvaluation':{'neighbors':32,'kernelBandwidthXYCosSin':[.45,.45,.35,.35],'contextScaler':'StandardScaler fit train only'},'folds':folds,'summary':{name:{k:float(np.mean([f[name][k] for f in folds])) for k in folds[0][name]} for name in ['nativeMacroContinuation','countMechanicsProgressionBaseline']},'limitations':['Macro triples model a real local-to-macro continuation, not complete-board preference.','Oracle best32 is a mixture reconstruction lower bound, not expected single-sample error.','Different source patterns can share a frontier and remain multimodal.','No style, solvability or human difficulty certification.'],'hashes':{'sourcePack':hashfile(workspace/'data/source500_native.json'),'templates':hashfile(workspace/'data/source_templates.json'),'macroModel':hashfile(workspace/'models'/macro_model_name),'familySplits':hashfile(workspace/'models/family_splits.json'),'trainer':hashfile(Path(__file__)),'model':hashfile(path)}};(workspace/'models'/report_name).write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps(report['summary'],indent=2),flush=True)

def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',required=True,type=Path);p.add_argument('--macro-model-name',default='macro_assembly.joblib');p.add_argument('--output-name',default='macro_pattern.joblib');p.add_argument('--report-name',default='macro_pattern_evaluation.json');args=p.parse_args();train(args.workspace.resolve(),args.macro_model_name,args.output_name,args.report_name)
if __name__=='__main__':main()
