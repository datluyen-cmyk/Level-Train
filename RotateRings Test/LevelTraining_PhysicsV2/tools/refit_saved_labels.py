"""Refit normalized difficulty on already independently replayed source labels."""
from train_native_physics import *
from sklearn.dummy import DummyRegressor
if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('--config',required=True);args=p.parse_args();root=Path(args.config).resolve().parent;c=read(args.config);a=load_adapter(root,c);report=read(root/'model_validation.json');sources=read(root/c['corpus'])['levels'];features={l['id']:a.features(l) for l in sources};names=sorted(features[sources[0]['id']]);labels=[r for r in report['sourceStatus'] if r['physicalLabel']];test=set(report['split']['testSourceIds']);train=[r for r in labels if r['id'] not in test];held=[r for r in labels if r['id'] in test]
 families=collections.defaultdict(list)
 for i,l in enumerate(sources):families[a.fingerprint(l)].append(i)
 keys=sorted(families);random.Random(c['seed']).shuffle(keys);testfamilies=set(keys[:max(1,len(keys)//5)]);testindices={i for k in testfamilies for i in families[k]};trainindices=set(range(len(sources)))-testindices
 report['split']={'method':'whole physical piece/graph WL families before fitting; color/IDs/rigid transforms ignored; conservative independent-piece mirror normalization (overrejects some distinct layouts)','trainFamilies':len(keys)-len(testfamilies),'testFamilies':len(testfamilies),'trainSourceIds':[sources[i]['id'] for i in sorted(trainindices)],'testSourceIds':[sources[i]['id'] for i in sorted(testindices)]}
 test=set(report['split']['testSourceIds']);train=[r for r in labels if r['id'] not in test];held=[r for r in labels if r['id'] in test];report['ruleVersion']=a.RULE_VERSION
 for r in labels:
  source=next(l for l in sources if l['id']==r['id']);assert a.replay(source,r['solution']),'Label replay changed: '+r['id']
 model=ExtraTreesRegressor(n_estimators=96,min_samples_leaf=2,random_state=c['seed'],n_jobs=-1)
 def xx(rows):return [[features[r['id']][k] for k in names] for r in rows]
 model.fit(xx(train),[r['score']/r['parts'] for r in train]);pred=model.predict(xx(held))*np.array([r['parts'] for r in held]) if held else [];truth=[r['score'] for r in held];mean=statistics.mean(r['score']/r['parts'] for r in train)
 report['difficulty']={'targetUnit':'score-per-part; multiplied by complete part count','MAE':float(mean_absolute_error(truth,pred)) if held else None,'meanPerPartBaselineMAE':float(mean_absolute_error(truth,[mean*r['parts'] for r in held])) if held else None,'R2':float(r2_score(truth,pred)) if len(held)>1 else None,'heldOutRows':len(held),'trainingRows':len(train),'extrapolation':'Piece-count scaling is additive; shape/unsupported mechanics/human difficulty extrapolation unverified.'}
 model.fit(xx(labels),[r['score']/r['parts'] for r in labels]);joblib.dump({'model':model,'features':names,'targetUnit':'score-per-part'},root/'models/difficulty.joblib');proposal=joblib.load(root/'models/proposal.joblib')
 joblib.dump({'model':model,'features':names,'targetUnit':'score-per-part','selection':'diagnostic only; worse than baseline on heldout1'},root/'models/difficulty_extra_trees.joblib')
 diagnostic=dict(report['difficulty']);baseline_mae=diagnostic['meanPerPartBaselineMAE'];report['extraTreesDiagnostic']=diagnostic
 model=DummyRegressor(strategy='mean');model.fit(xx(labels),[r['score']/r['parts'] for r in labels]);joblib.dump({'model':model,'features':names,'targetUnit':'score-per-part','selectedPredictor':'mean-per-part baseline; fitted on6 replayed labels'},root/'models/difficulty.joblib')
 report['difficulty']={**diagnostic,'selectedPredictor':'mean-per-part baseline (lower heldout MAE than ExtraTrees)','MAE':baseline_mae,'R2':None,'finalMeanPerPart':float(model.constant_[0][0]),'limitations':'Only1 heldout source and6 complete physical labels; no reliable generalization or human/native difficulty equivalence claim.'}
 for row in proposal['rows']:row['features']=features[row['id']]
 proposal['ruleVersion']=a.RULE_VERSION;proposal['mirrorCanonicalization']='Each piece attachment pattern normalized against reflection; conservative over-rejection of some distinct independent-piece reflections.'
 proposal['turnSamples']=[abs(action['delta']) for r in labels for action in r['solution']];proposal['sampling']='Empirical all500 progression-neighborhood component density and branch rate; native-template frequency weights; independent source-replayed angular-travel samples; exact-scale +/-10% proposals; constructive constraints and rejection.'
 trainids=set(report['split']['trainSourceIds']);testids=set(report['split']['testSourceIds']);pm={}
 for key in names:
  truth=[];conditional=[];baseline=[]
  for source in sources:
   if source['id'] not in testids:continue
   near=sorted([l for l in sources if l['id'] in trainids],key=lambda l:abs(l['position']-source['position']))[:16];truth.append(features[source['id']][key]);conditional.append(statistics.mean(features[l['id']][key] for l in near));baseline.append(statistics.mean(features[l['id']][key] for l in sources if l['id'] in trainids))
  pm[key]={'conditionalMAE':float(mean_absolute_error(truth,conditional)),'globalMeanMAE':float(mean_absolute_error(truth,baseline))}
 report['proposalHeldOutAgainstBaseline']=pm
 proposal['construction']='Source-conditioned part count and cluster density; new reverse-release constructions of unchanged whole source pieces, exact polygon rejection; no pin stretching or isolated padding.';joblib.dump(proposal,root/'models/proposal.joblib')
 report['sourceSearchBudget']={'maxActions':120,'maxSeconds':20,'meaning':'per-source search bound; timeout is inconclusive, no native unsolvability claim','checkpointLabels':'all successful paths independently replayed under final packaged rules after performance-only transform and distance optimization'};write(root/'model_validation.json',report)
 write(root/'source_label_progress.json',{'processed':len(sources),'labels':len(labels),'status':report['sourceStatus']})
 manifest=read(root/'models/manifest.json');manifest['ruleVersion']=a.RULE_VERSION;manifest['hashes']={path:sha(root/path) for path in manifest['hashes']};manifest['hashes']['tools/refit_saved_labels.py']=sha(__file__);manifest['hashes']['model_validation.json']=sha(root/'model_validation.json');manifest['hashes']['models/difficulty_extra_trees.joblib']=sha(root/'models/difficulty_extra_trees.joblib');manifest['hashes']['tools/generate_batch.py']=sha(root/'tools/generate_batch.py');write(root/'models/manifest.json',manifest);print('NORMALIZED REFIT',len(labels),'labels',report['difficulty'])
