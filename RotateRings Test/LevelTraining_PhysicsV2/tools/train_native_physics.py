"""Revision-specific native template fitting, bounded physical labels, and saved-model generation."""
from pathlib import Path
import argparse,collections,copy,hashlib,importlib.util,json,math,random,statistics,sys,time,subprocess
import joblib,numpy as np
from sklearn.ensemble import ExtraTreesRegressor
from sklearn.metrics import mean_absolute_error,r2_score

def write(path,obj):
    p=Path(path);p.parent.mkdir(parents=True,exist_ok=True);tmp=p.with_suffix(p.suffix+'.tmp');tmp.write_text(json.dumps(obj,indent=2,allow_nan=False),encoding='utf8')
    for retry in range(100):
        try:tmp.replace(p);break
        except PermissionError:
            if retry==99:raise
            time.sleep(.02)
def sha(path):return hashlib.sha256(Path(path).read_bytes()).hexdigest()
def load_adapter(root,c):
    spec=importlib.util.spec_from_file_location('native_adapter',root/c['adapter']);a=importlib.util.module_from_spec(spec);spec.loader.exec_module(a);return a
def read(path):return json.loads(Path(path).read_text(encoding='utf8'))
def fit(config):
    root=Path(config).resolve().parent;c=read(config);a=load_adapter(root,c);sources=read(root/c['corpus'])['levels'];templates=read(root/'templates.json')['templates']
    assert len(sources)>=500 and len({l['id'] for l in sources})>=500
    rng=random.Random(c['seed']);families=collections.defaultdict(list)
    for i,l in enumerate(sources):families[a.fingerprint(l)].append(i)
    keys=sorted(families);rng.shuffle(keys);testfamilies=set(keys[:max(1,len(keys)//5)]);test={i for k in testfamilies for i in families[k]};train=set(range(len(sources)))-test
    feature_names=sorted(a.features(sources[0]));features=[a.features(l) for l in sources]
    proposal_metrics={}
    for key in feature_names:
        truths=[];pred=[];baseline=[]
        for i in sorted(test):
            near=sorted(train,key=lambda j:abs(sources[j]['position']-sources[i]['position']))[:16]
            truths.append(features[i][key]);pred.append(statistics.mean(features[j][key] for j in near));baseline.append(statistics.mean(features[j][key] for j in train))
        proposal_metrics[key]={'conditionalMAE':mean_absolute_error(truths,pred),'globalMeanMAE':mean_absolute_error(truths,baseline)}
    labels=[];status=[];rejections=collections.Counter()
    checkpoint=root/'source_label_progress.json';previous={}
    if checkpoint.exists():previous={r['id']:r for r in read(checkpoint).get('status',[]) if r.get('physicalLabel')}
    for i,l in enumerate(sources):
        if not l['physicsV2']['eligible']:reason='unsupported source mechanics: '+','.join(l['physicsV2']['unsupportedMechanics']);rejections['unsupported mechanics']+=1;status.append({'id':l['id'],'physicalLabel':False,'reason':reason});continue
        errors=a.inventory_errors(l)
        if errors:status.append({'id':l['id'],'physicalLabel':False,'reason':errors});rejections['excluded source template inventory']+=1;continue
        if l['id'] in previous and a.replay(l,previous[l['id']]['solution']):
            item=previous[l['id']];item['index']=i;labels.append(item);status.append(item);continue
        result=a.solve(l,{'maxEffort':120,'maxSeconds':20});replayed=result['solved'] and a.replay(l,result['solution'])
        item={'id':l['id'],'index':i,'physicalLabel':bool(replayed),'reason':result.get('reason'),'parts':len(l['state']['parts'])}
        if replayed:item.update(score=a.difficulty(l,result),cost=result['cost'],solution=result['solution']);labels.append(item)
        else:rejections[result.get('reason','replay failed')]+=1
        status.append(item);write(root/'source_label_progress.json',{'processed':i+1,'labels':len(labels),'status':status});print('source-physics',i+1,'/',len(sources),'labels',len(labels),flush=True)
    xtrain=[[features[r['index']][k] for k in feature_names] for r in labels if r['index'] in train];ytrain=[r['score']/r['parts'] for r in labels if r['index'] in train]
    xtest=[[features[r['index']][k] for k in feature_names] for r in labels if r['index'] in test];ytest=[r['score'] for r in labels if r['index'] in test]
    model=ExtraTreesRegressor(n_estimators=96,min_samples_leaf=2,random_state=c['seed'],n_jobs=-1)
    if len(xtrain)<2:raise RuntimeError('Insufficient supported physical source labels; preserve progress without fabricating targets')
    model.fit(xtrain,ytrain);test_parts=[r['parts'] for r in labels if r['index'] in test];pred=model.predict(xtest)*np.array(test_parts) if xtest else []
    dm={'MAE':float(mean_absolute_error(ytest,pred)) if ytest else None,'meanBaselineMAE':float(mean_absolute_error(ytest,[statistics.mean(ytrain)*n for n in test_parts])) if ytest else None,'R2':float(r2_score(ytest,pred)) if len(ytest)>1 else None,'heldOutRows':len(ytest),'trainingRows':len(ytrain)}
    model.fit([[features[r['index']][k] for k in feature_names] for r in labels],[r['score']/r['parts'] for r in labels])
    eligible={l['id'] for l in sources if l['physicsV2']['eligible']}
    pool=[t for t in templates if t['kind']=='ring' and any(r['levelId'] in eligible for r in t['sourceRefs']) and abs(t['radius']-1.75)<.001 and abs(t['width']-.5)<.001 and t['gap']==90 and len(t['attachments'])<=3 and all(not x['isInnerMount'] and x['type']==1 and x['stemLength']<=1.6 for x in t['attachments']) and {s['attachmentId'] for s in t['physicalShapes'] if s['role']=='head_trigger'}=={x['id'] for x in t['attachments']}]
    proposal={'ruleVersion':a.RULE_VERSION,'sourceCount':len(sources),'rows':[{'id':l['id'],'position':l['position'],'features':f} for l,f in zip(sources,features)],'allowedTemplates':[t['id'] for t in pool],'templateFrequencies':{t['id']:len(t['sourceRefs']) for t in pool},'labelScores':{r['id']:r['score'] for r in labels},'construction':'Learned progression scale and verified whole-piece inventory; new constrained arrangements, collision rejection; no pin construction or stretching','generatedSubset':'ring-only,90-degree physical gap, radius1.75, width.5, 0..3 outer TBar pins, source shortstem<=1.6. Straight pulls and all unimplemented mechanics explicitly excluded.'}
    (root/'models').mkdir(exist_ok=True);joblib.dump({'model':model,'features':feature_names,'targetUnit':'score-per-part'},root/'models/difficulty.joblib');joblib.dump(proposal,root/'models/proposal.joblib')
    report={'ruleVersion':a.RULE_VERSION,'originalCountInspected':len(sources),'supportedSourceLevelInventory':sum(l['physicsV2']['eligible'] for l in sources),'physicallySolvedAndReplayedOriginals':len(labels),'sourceRejectReasons':dict(rejections),'sourceStatus':status,'split':{'method':'whole physical piece/graph WL families before fitting; rigid transforms/color/IDs ignored','trainFamilies':len(keys)-len(testfamilies),'testFamilies':len(testfamilies),'trainSourceIds':[sources[i]['id'] for i in sorted(train)],'testSourceIds':[sources[i]['id'] for i in sorted(test)]},'proposalHeldOutAgainstBaseline':proposal_metrics,'difficulty':dm,'difficultyMeaning':'nonoptimal computer cost+angular travel+graph depth/branching under conservative native-polygon rules; original native/human parity unmeasured','templatePoolCount':len(pool),'templateInventory':dict(collections.Counter(len(t['attachments']) for t in pool)),'generatedSubset':proposal['generatedSubset']}
    write(root/'model_validation.json',report)
    write(root/'models/manifest.json',{'sourceCount':len(sources),'ruleVersion':a.RULE_VERSION,'seed':c['seed'],'hashes':{str(p.relative_to(root)):sha(p) for p in [root/c['adapter'],root/c['corpus'],root/c['references'],root/'templates.json',root/'models/proposal.joblib',root/'models/difficulty.joblib',root/'level_training.json',Path(__file__)]}})
    write(root/'source_label_progress.json',{'processed':len(sources),'labels':len(labels),'status':status})
    subprocess.run([sys.executable,str(root/'tools/refit_saved_labels.py'),'--config',str(Path(config).resolve())],check=True)
    print('MODELS SAVED physical labels',len(labels),'templates',len(pool),flush=True)

def generate(config,output,count=300,seed=20261105,exclude=None):
    root=Path(config).resolve().parent;c=read(config);a=load_adapter(root,c);proposal=joblib.load(root/'models/proposal.joblib');reg=joblib.load(root/'models/difficulty.joblib');rng=random.Random(seed)
    manifest=read(root/'models/manifest.json')
    for path,expected in manifest['hashes'].items():
        if sha(root/path)!=expected:raise RuntimeError('Stale model input changed: '+path)
    out=Path(output);out.mkdir(exist_ok=True,parents=True);alltemplates=read(root/'templates.json')['templates'];allowed=set(proposal['allowedTemplates']);templates=[t for t in alltemplates if t['id'] in allowed]
    sources=read(root/c['corpus'])['levels'];earlier=[]
    exclusions=list(exclude or []);finalpack=root/'batches/final-300/generated_levels.json'
    if finalpack.exists() and Path(output).resolve()!=finalpack.parent.resolve() and str(finalpack) not in exclusions:exclusions.append(str(finalpack))
    for path in exclusions:earlier.extend(read(path)['levels'])
    accepted=[];reject=collections.Counter();source_fps={a.fingerprint(l) for l in sources};seen=source_fps|{a.fingerprint(l) for l in earlier};started=time.time();attempts=0
    source_records=[(a.fingerprint(l),a.descriptor(l)) for l in sources]
    prior_records=[(a.fingerprint(l),a.descriptor(l)) for l in earlier]
    def distance(record,other):
        if record[0]==other[0]:return 0.
        x,y=record[1],other[1];keys=x.keys()|y.keys();return 1-sum(min(x[k],y[k]) for k in keys)/max(1,sum(max(x[k],y[k]) for k in keys))
    def save(complete=False):
        pack={'schemaVersion':2,'ruleVersion':a.RULE_VERSION,'complete':complete,'seed':seed,'levels':accepted}
        write(out/'generated_levels.json',pack)
        if not complete:write(out/'partial_levels.json',pack)
        report={'complete':complete,'generatedCount':len(accepted),'requestedCount':count,'accepted':len(accepted),'requested':count,'attempts':attempts,'rejections':dict(reject),'seconds':time.time()-started,'sourceCount':len(sources),'templatePool':len(templates),'criteria':c['acceptance'],'inputHashes':manifest['hashes'],'exclusionPacks':exclusions,'packSha256':sha(out/'generated_levels.json'),'limits':proposal['generatedSubset']+' Difficulty/fun equivalence remains unmeasured.'}
        write(out/'batch_report.json',report);write(out/'validation_report.json',report)
    for index in range(count):
        ref=sources[index%min(300,len(sources))];rf=a.features(ref);targetn=max(2,min(c['acceptance']['maximumParts'],rf['parts']))
        success=False
        for attempt in range(c['acceptance']['maxAttemptsPerLevel']):
            attempts+=1;n=max(2,min(c['acceptance']['maximumParts'],round(targetn*rng.uniform(.9,1.1))))
            reference_components=rf.get('components',max(1,n/6))
            # Unsupported source isolated fixtures cannot become bare padding.
            # Bound cluster density to source-supported, meaningful interlocks.
            desired_components=max(1,min(reference_components,n/4))
            near=sorted(proposal['rows'],key=lambda row:abs(row['position']-ref['position']))[:16]
            density=statistics.mean(row['features'].get('components',1)/row['features']['parts'] for row in near)
            desired_components=max(1,min((reference_components+n*density)/2,n/4))
            settings={'branchRate':statistics.mean(row['features']['branching']/row['features']['parts'] for row in near),'templateFrequencies':proposal['templateFrequencies'],'turnSamples':proposal.get('turnSamples',[90,120,150,175])}
            candidate,error=a.build_candidate(templates,n,rng,desired_components,settings)
            if error:reject[error]+=1;continue
            if a.inventory_errors(candidate):reject['template inventory mismatch']+=1;continue
            if a.hard_collisions(candidate,a.new_session(candidate)):reject['initial hard overlap']+=1;continue
            fp=a.fingerprint(candidate)
            if fp in seen:reject['canonical duplicate']+=1;continue
            record=(fp,a.descriptor(candidate));minsource=min(distance(record,other) for other in source_records)
            if minsource<c['acceptance']['nearCopyDistance']:reject['source near copy']+=1;continue
            minprior=min((distance(record,other) for other in prior_records),default=1.)
            if minprior<c['acceptance']['nearCopyDistance']:reject['generated near copy']+=1;continue
            result=a.solve(candidate)
            if not result['solved']:reject[result.get('reason','physical solve failed')]+=1;continue
            if not a.replay(candidate,result['solution']):reject['independent physical replay failed']+=1;continue
            score=a.difficulty(candidate,result);f=a.features(candidate);pred=float(reg['model'].predict([[f[k] for k in reg['features']]])[0])*f['parts']
            # Physical reference labels where available; otherwise the fitted source
            # predictor supplies a disclosed computer target, never human difficulty.
            target=proposal['labelScores'].get(ref['id'],float(reg['model'].predict([[rf[k] for k in reg['features']]])[0])*rf['parts'])
            tolerance=c['acceptance']['computerScoreRelativeTolerance']
            if abs(score-target)/max(1,target)>tolerance:reject['computer difficulty outside declared range']+=1;continue
            candidate.update(id=f'physics-{index+1:03}',position=index+1,referenceId=ref['id'],group='native-arrangement-'+fp,solution=result['solution'])
            candidate['acceptance']={'physicalReplay':True,'initialHardOverlap':False,'exactSourceTemplateInventory':True,'sourceDistance':minsource,'generatedDistance':minprior,'computerScore':score,'targetComputerScore':target,'predictedScore':pred,'targetMeaning':'measured conservative polygon source label' if ref['id'] in proposal['labelScores'] else 'physical-label predictor for supported subset; human/native difficulty unmeasured','effort':result['effort'],'partsTarget':targetn}
            candidate.pop('_constructionSolution',None);candidate.pop('_constructionEffort',None)
            for part in candidate['state']['parts']:part.pop('sourceRefs',None)
            accepted.append(candidate);seen.add(fp);prior_records.append(record);success=True;save();print(f'{index+1}/{count}: PASS ({len(accepted)} accepted), attempts={attempts}, seconds={time.time()-started:.1f}',flush=True);break
        if not success:reject['per-position budget exhausted']+=1;save();print(f'{index+1}/{count}: INCOMPLETE ({len(accepted)} accepted)',flush=True);return 2
    save(True);return 0

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--config',required=True);p.add_argument('--train-only',action='store_true');p.add_argument('--output');p.add_argument('--count',type=int,default=300);p.add_argument('--seed',type=int,default=20261105);args=p.parse_args()
    fit(args.config)
    if not args.train_only:raise SystemExit(generate(args.config,args.output or str(Path(args.config).resolve().parent/'batches/final-300'),args.count,args.seed))
