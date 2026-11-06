"""Independent post-batch arithmetic/scale/model-target audit (not engine or human certification)."""
from pathlib import Path
import hashlib,importlib.util,json,joblib,math,statistics
root=Path(__file__).resolve().parents[1]/'RotateRings Test/LevelTraining_PhysicsV2'
def read(p):return json.loads(p.read_text(encoding='utf8'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
config=read(root/'level_training.json');manifest=read(root/'models/manifest.json');pack=read(root/'batches/final-300/generated_levels.json');batch=read(root/'batches/final-300/validation_report.json');sources=read(root/config['corpus'])['levels'];byid={l['id']:l for l in sources};proposal=joblib.load(root/'models/proposal.joblib');reg=joblib.load(root/'models/difficulty.joblib');spec=importlib.util.spec_from_file_location('auditadapter',root/config['adapter']);a=importlib.util.module_from_spec(spec);spec.loader.exec_module(a);errors=[];ratios=[];deltas=[];rows=[]
for path,h in manifest['hashes'].items():
 if sha(root/path)!=h:errors.append(['manifest',path])
if not pack['complete'] or len(pack['levels'])!=300 or not batch['complete'] or batch['packSha256']!=sha(root/'batches/final-300/generated_levels.json'):errors.append('incomplete or pack hash mismatch')
for i,l in enumerate(pack['levels']):
 ref=sources[i];rf=a.features(ref);f=a.features(l);n=f['parts'];path=l['solution'];score=len(path)+sum(abs(x['delta']) for x in path)/180+.65*f['depth']+.35*f['branching'];target=proposal['labelScores'].get(ref['id'],float(reg['model'].predict([[rf[k] for k in reg['features']]])[0])*rf['parts']);ratio=score/max(target,1e-10);ratios.append(ratio);deltas.extend(abs(x['delta']) for x in path)
 if l['referenceId']!=ref['id']:errors.append([i+1,'reference mismatch'])
 if abs(score-l['acceptance']['computerScore'])>1e-7 or abs(target-l['acceptance']['targetComputerScore'])>1e-7:errors.append([i+1,'score target arithmetic mismatch'])
 if abs(score-target)/max(1,target)>config['acceptance']['computerScoreRelativeTolerance']+1e-10:errors.append([i+1,'score tolerance'])
 low=max(2,min(84,round(rf['parts']*.9)));high=max(2,min(84,round(rf['parts']*1.1)))
 if not low<=n<=high:errors.append([i+1,'source scale range',n,low,high])
 rows.append({'position':i+1,'sourceParts':rf['parts'],'generatedParts':n,'score':score,'target':target,'ratio':ratio,'components':f['components'],'branching':f['branching'],'depth':f['depth']})
report={'passed':not errors,'errors':errors,'count':len(rows),'sourceCount':len(sources),'wholePieceCount':sum(r['generatedParts'] for r in rows),'sourceFirst300PartsMean':statistics.mean(r['sourceParts'] for r in rows),'generatedPartsMean':statistics.mean(r['generatedParts'] for r in rows),'generatedPartsMaximum':max(r['generatedParts'] for r in rows),'scoreRatio':{'minimum':min(ratios),'mean':statistics.mean(ratios),'maximum':max(ratios)},'signedGestures':len(deltas),'angularTravelDegrees':sum(deltas),'selectedPredictor':reg.get('selectedPredictor'),'packSha256':sha(root/'batches/final-300/generated_levels.json'),'manifestSha256':sha(root/'models/manifest.json'),'modelValidationSha256':sha(root/'model_validation.json'),'rows':rows,'scope':'Independent arithmetic and selected-model/scale gate audit; physics replay, engine and human difficulty are separate evidence.'}
out=root/'audits/training_score_audit.json';out.parent.mkdir(exist_ok=True);out.write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps({k:v for k,v in report.items() if k!='rows'},indent=2));raise SystemExit(0 if report['passed'] else 1)
