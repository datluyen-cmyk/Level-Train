"""Source-only whole-family heldout composition-distance calibration; not acceptance."""
from pathlib import Path
import argparse,copy,json
import joblib,numpy as np
from sklearn.preprocessing import StandardScaler
from sklearn.neighbors import NearestNeighbors
from layout_learning_v3 import read,hashfile
from composition_field_v3 import field,field_context,H,W,CHANNELS

def quantiles(values):
 return {str(q):float(np.quantile(values,q)) for q in [.05,.25,.5,.75,.9,.95,.99]}
def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',required=True,type=Path);p.add_argument('--candidates',type=Path);a=p.parse_args();w=a.workspace;levels=read(w/'data/source500_native.json')['levels'];ts={t['id']:t for t in read(w/'data/source_templates.json')['templates']};splits=read(w/'models/family_splits.json');m=joblib.load(w/'models/whole_board_field.joblib');x=np.asarray([field_context(l,ts) for l in levels]);y=np.asarray(m.fields)[:,:CHANNELS*H*W];by={l['sourceId']:i for i,l in enumerate(levels)};rows=[]
 for s in splits['splits']:
  tr=[by[k] for k in s['trainSourceIds']];te=[by[k] for k in s['heldOutSourceIds']];scale=StandardScaler().fit(x[tr]);nn=NearestNeighbors(n_neighbors=min(32,len(tr))).fit(scale.transform(x[tr]));dist,ix=nn.kneighbors(scale.transform(x[te]))
  for test,ds,neighbors in zip(te,dist,ix):
   pool=np.array(tr)[neighbors];d=np.abs(y[pool]-y[test]).sum(axis=1);best=int(np.argmin(d));rows.append({'sourceId':levels[test]['sourceId'],'position':levels[test]['position'],'fold':s['fold'],'nearestContextSourceId':levels[pool[0]]['sourceId'],'nearestContextFieldL1':float(d[0]),'nearestFieldAmong32SourceId':levels[pool[best]]['sourceId'],'nearestFieldAmong32L1':float(d[best]),'sameFamilyAllowed':False})
 report={'status':'SOURCE_ONLY_DISTANCE_CALIBRATION_NO_GATE_CHANGE','acceptedCount':0,'sourceCount':len(levels),'sourceFamilies':len(set(splits['families'].values())),'representation':'10 native normalized board-frame channels, own solid bounding span and stroke heading; orientation histogram excluded','protocol':'Five existing whole-family heldout folds; scaler and nearest32 context index training-only. Measure heldout source normalized field L1 to nearest context and closest field within32 contexts. No candidate used to set quantiles.','importantMetricDifference':'Historical rewrite delta uses candidate/source shared reference frame and raw L1 threshold2*removedfraction. This calibration uses independent board native normalization and distances to a source distribution, not changed-fraction or similarity to one reference. It does not retrospectively pass old rejects.','declaredBeforeEvaluation':{'neighbors':32,'reportedQuantiles':[.05,.25,.5,.75,.9,.95,.99],'hardGate':'none; visual/native/replay/noncopy remain mandatory'},'quantiles':{k:quantiles([r[k] for r in rows]) for k in ['nearestContextFieldL1','nearestFieldAmong32L1']},'levels':rows,'hashes':{'sourcePack':hashfile(w/'data/source500_native.json'),'templates':hashfile(w/'data/source_templates.json'),'familySplits':hashfile(w/'models/family_splits.json'),'fieldModel':hashfile(w/'models/whole_board_field.joblib'),'tool':hashfile(Path(__file__))}}
 if a.candidates:
  families=splits['families'];candidate_rows=[]
  for c in read(a.candidates)['levels']:
   ref=c['assemblyProvenance']['tileImports'][0]['sourceId'];train=[i for i,l in enumerate(levels) if families[l['sourceId']]!=families[ref]];scale=StandardScaler().fit(x[train]);nn=NearestNeighbors(n_neighbors=32).fit(scale.transform(x[train]));ds,ix=nn.kneighbors(scale.transform([field_context(c,ts)]));pool=np.array(train)[ix[0]];cy=field(c,ts)[0][:CHANNELS*H*W];d=np.abs(y[pool]-cy).sum(axis=1);best=int(np.argmin(d));candidate_rows.append({'id':c['id'],'position':c['position'],'excludedSourceFamily':families[ref],'nearestContextSourceId':levels[pool[0]]['sourceId'],'nearestContextFieldL1':float(d[0]),'nearestFieldAmong32SourceId':levels[pool[best]]['sourceId'],'nearestFieldAmong32L1':float(d[best]),'heldoutSourcePercentile':float(np.mean(np.array([r['nearestFieldAmong32L1'] for r in rows])<=d[best])),'accepted':False})
  report['candidateReviewOnly']=candidate_rows;report['hashes']['candidatePack']=hashfile(a.candidates)
 out=w/'models/whole_field_source_distance_calibration.json';out.write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps({k:report[k] for k in ['quantiles','candidateReviewOnly'] if k in report},indent=2))
if __name__=='__main__':main()
