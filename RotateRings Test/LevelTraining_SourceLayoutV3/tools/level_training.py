"""Game-independent scalar-difficulty training, generation and evidence runner."""
from __future__ import annotations
import argparse, collections, copy, hashlib, importlib.util, importlib.metadata, json, math, pathlib, random, statistics, sys, time

def digest(path):return hashlib.sha256(pathlib.Path(path).read_bytes()).hexdigest()
def write(path,value):
    path=pathlib.Path(path);path.parent.mkdir(parents=True,exist_ok=True)
    temp=path.with_suffix(path.suffix+'.tmp');temp.write_text(json.dumps(value,indent=2,ensure_ascii=False,allow_nan=False),encoding='utf8');temp.replace(path)
def load_adapter(path):
    path=pathlib.Path(path).resolve();sys.path.insert(0,str(path.parent))
    spec=importlib.util.spec_from_file_location('level_game_adapter',path);module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    for name in ['validate_level','fingerprint','features','solve','replay','difficulty','fit_proposal','propose','novelty_distance']:
        if not callable(getattr(module,name,None)):raise ValueError('Adapter missing '+name)
    return module
def records(path):
    data=json.loads(pathlib.Path(path).read_text(encoding='utf-8-sig'));rows=data['levels']
    if not rows:raise ValueError('Empty dataset: '+str(path))
    if len({str(x['id']) for x in rows})!=len(rows):raise ValueError('Duplicate dataset IDs')
    for row in rows:
        if 'state' not in row or 'group' not in row:raise ValueError('Every record needs state and group')
        row['id']=str(row['id']);row['group']=str(row['group'])
    return rows
def groups_split(levels,adapter,seed):
    parent={x['group']:x['group'] for x in levels}
    def root(x):
        while parent[x]!=x:parent[x]=parent[parent[x]];x=parent[x]
        return x
    canonical={}
    for row in levels:
        fp=adapter.fingerprint(row);g=row['group']
        if fp in canonical:parent[root(g)]=root(canonical[fp])
        canonical[fp]=g
    families=sorted({root(g) for g in parent});random.Random(seed).shuffle(families)
    if len(families)<2:raise ValueError('At least two distinct source families are needed for a holdout; do not fabricate evaluation')
    hold=set(families[:max(1,round(len(families)*.2))])
    train=[i for i,l in enumerate(levels) if root(l['group']) not in hold];test=[i for i,l in enumerate(levels) if root(l['group']) in hold]
    return train,test,{'trainIds':[levels[i]['id'] for i in train],'heldOutIds':[levels[i]['id'] for i in test],
        'trainFamilies':len(families)-len(hold),'heldOutFamilies':len(hold),'method':'source groups unioned by canonical equivalence before splitting'}
def measured(adapter,level,policies):
    runs=[]
    for policy in policies:
        result=adapter.solve(level,copy.deepcopy(policy))
        if result.get('solved') and not adapter.replay(level,result['solution']):raise ValueError('Solver solution failed replay')
        row={'policy':policy,'solved':bool(result.get('solved')),'reason':result.get('reason')}
        if row['solved']:
            row.update(score=float(adapter.difficulty(level,result)),cost=float(result['cost']),effort=float(result.get('effort',0)))
            if any(not math.isfinite(row[k]) or row[k]<0 for k in ['score','cost','effort']):raise ValueError('Invalid solver metric')
        runs.append(row)
    good=[x for x in runs if x['solved']]
    return {'runs':runs,'successRate':len(good)/len(runs),'medianScore':statistics.median(x['score'] for x in good) if good else None,
        'medianCost':statistics.median(x['cost'] for x in good) if good else None}
def compare(a,b,g):
    if a['medianScore'] is None or b['medianScore'] is None:return {'passed':False,'gates':{'hasSolvedRuns':False}}
    relative=abs(a['medianScore']-b['medianScore'])/max(abs(a['medianScore']),g['scoreFloor'])
    cost=abs(a['medianCost']-b['medianCost']);delta=abs(a['successRate']-b['successRate'])
    gates={'score':relative<=g['portfolioRelativeTolerance'],'cost':cost<=max(g['costAbsoluteTolerance'],g['costRelativeTolerance']*a['medianCost']),
        'successRate':delta<=g['maxSuccessRateDelta'],'allCandidateRuns':not g['requireAllCandidatePoliciesSolved'] or b['successRate']==1}
    return {'passed':all(gates.values()),'gates':gates,'relativeScoreDifference':relative,'costDifference':cost,'successRateDifference':delta}
def run(config_path,resume=False,export=None):
    import joblib,numpy as np
    from sklearn.ensemble import ExtraTreesRegressor
    from sklearn.metrics import mean_absolute_error,r2_score
    start=time.perf_counter();config_path=pathlib.Path(config_path).resolve();root=config_path.parent
    config=json.loads(config_path.read_text(encoding='utf8'));resolve=lambda key:(root/config[key]).resolve()
    adapter_path=resolve('adapter');adapter=load_adapter(adapter_path);reference_path=resolve('references');corpus_path=resolve('corpus')
    refs=records(reference_path);sources=records(corpus_path)
    all_fps={adapter.fingerprint(x) for x in sources}
    for l in refs:
        fp=adapter.fingerprint(l)
        if fp not in all_fps:sources.append(l);all_fps.add(fp)
    for l in sources+refs:
        errors=adapter.validate_level(l)
        if errors:raise ValueError(f"Unsupported level {l['id']}: {errors}")
    seed=int(config['seed']);rng=random.Random(seed);policies=config['policies'];base_gate=config['acceptance']
    if not policies or base_gate['scoreFloor']<=0:raise ValueError('Nonempty policies and positive scoreFloor required')
    input_hashes={key:digest(path) for key,path in [('references',reference_path),('corpus',corpus_path),('adapter',adapter_path),('config',config_path),('runner',__file__)]}
    # Include imported local adapter helpers, so changing transition rules invalidates resume.
    helper_hashes={p.name:digest(p) for p in adapter_path.parent.glob('*.py')}
    signature=hashlib.sha256(json.dumps({'inputs':{k:v for k,v in input_hashes.items() if k!='config'},'helpers':helper_hashes,
        'acceptance':base_gate,'perReference':config.get('perReference',{}),'policies':policies,'seed':seed},sort_keys=True).encode()).hexdigest()
    labels=[];usable=[];source_failures=[]
    feature_names=None
    for l in sources:
        result=adapter.solve(l,policies[0])
        if not result.get('solved'):source_failures.append({'id':l['id'],'reason':result.get('reason','solver budget')});continue
        if not adapter.replay(l,result['solution']):raise ValueError('Source replay failed '+l['id'])
        f=adapter.features(l)
        if feature_names is None:feature_names=sorted(f)
        if sorted(f)!=feature_names or any(not math.isfinite(float(v)) for v in f.values()):raise ValueError('Inconsistent/nonfinite features')
        score=float(adapter.difficulty(l,result))
        if not math.isfinite(score) or score<0:raise ValueError('Invalid source difficulty')
        labels.append(score);usable.append(l)
    train,test,split=groups_split(usable,adapter,seed)
    x=np.array([[adapter.features(l)[k] for k in feature_names] for l in usable]);y=np.array(labels)
    model=ExtraTreesRegressor(n_estimators=128,min_samples_leaf=1,random_state=seed,n_jobs=-1)
    model.fit(x[train],y[train]);pred=model.predict(x[test]);baseline=np.repeat(y[train].mean(),len(test))
    proposal,proposal_meta=adapter.fit_proposal([usable[i] for i in train],seed)
    evaluation=adapter.proposal_metrics(proposal,[usable[i] for i in test]) if hasattr(adapter,'proposal_metrics') else {'available':False}
    metrics={'split':split,'mae':float(mean_absolute_error(y[test],pred)),'meanBaselineMAE':float(mean_absolute_error(y[test],baseline)),
        'r2':float(r2_score(y[test],pred)) if len(test)>1 and float(np.var(y[test]))>0 else None,'proposal':evaluation,
        'sourceFailures':source_failures,'inputHashes':input_hashes,'featureNames':feature_names}
    write(root/'model_validation.json',metrics);model.fit(x,y);proposal,proposal_meta=adapter.fit_proposal(usable,seed)
    (root/'models').mkdir(exist_ok=True);joblib.dump({'model':model,'features':feature_names,'validation':metrics},root/'models/difficulty.joblib')
    joblib.dump({'model':proposal,'metadata':proposal_meta},root/'models/proposal.joblib')
    write(root/'models/manifest.json',{'schemaVersion':1,'inputHashes':input_hashes,'helperHashes':helper_hashes,
        'modelHashes':{p.name:digest(p) for p in (root/'models').glob('*.joblib')},'featureNames':feature_names,
        'seed':seed,'python':sys.version,'packages':{name:importlib.metadata.version(name) for name in ['numpy','scipy','scikit-learn','joblib']}})
    accepted=[];rejections={};partial=root/'partial_levels.json';retained={}
    if resume and partial.exists():
        old=json.loads(partial.read_text(encoding='utf8'))
        if old.get('signature')!=signature:raise ValueError('Resume inputs/rules/gates changed; use a new workspace or explicitly revalidate')
        retained={l['referenceId']:l for l in old['levels']}
    def check(candidate,ref,target,pf,g):
        errors=adapter.validate_level(candidate)
        if hasattr(adapter,'validate_candidate'):errors+=adapter.validate_candidate(candidate,ref)
        if errors:return None,'constraints'
        fp=adapter.fingerprint(candidate)
        if fp in all_fps or any(fp==adapter.fingerprint(l) for l in accepted):return None,'equivalent'
        result=adapter.solve(candidate,policies[0])
        if not result.get('solved'):return None,'solver'
        if not adapter.replay(candidate,result['solution']):raise ValueError('Candidate replay failed')
        score=float(adapter.difficulty(candidate,result));relative=abs(score-target)/max(abs(target),g['scoreFloor'])
        if relative>g['primaryRelativeTolerance']:return None,'primaryDifficulty'
        portfolio=measured(adapter,candidate,policies);comparison=compare(pf,portfolio,g)
        if not comparison['passed']:return None,'portfolio'
        nearest=1.
        for other in sources+accepted:
            distance=float(adapter.novelty_distance(candidate,other))
            if not math.isfinite(distance) or not 0<=distance<=1:raise ValueError('Invalid novelty distance')
            nearest=min(nearest,distance)
            threshold=max(g['nearCopyDistance'],other.get('acceptance',{}).get('criteria',{}).get('nearCopyDistance',0))
            if distance<threshold:return None,'nearCopy'
        row=copy.deepcopy(candidate);row.update(referenceId=ref['id'],solution=result['solution'],acceptance={'targetScore':target,'score':score,
            'relativeDifference':relative,'nearestDistance':nearest,'criteria':g,'referencePortfolio':pf,'portfolio':portfolio,'comparison':comparison})
        return row,None
    for ref in refs:
        gate={**base_gate,**config.get('perReference',{}).get(ref['id'],{})};rejected=collections.Counter()
        original=adapter.solve(ref,policies[0])
        if not original.get('solved') or not adapter.replay(ref,original['solution']):raise ValueError('Reference unsupported/unsolved '+ref['id'])
        target=float(adapter.difficulty(ref,original));portfolio=measured(adapter,ref,policies);best=None
        if ref['id'] in retained:best,reason=check(retained[ref['id']],ref,target,portfolio,gate)
        if best is None:
            batch=[]
            for attempt in range(int(config['proposalBudget'])):
                candidate=adapter.propose(ref,rng,proposal)
                if candidate is None:rejected['proposal']+=1;continue
                if adapter.validate_level(candidate):rejected['constraints']+=1;continue
                f=adapter.features(candidate);batch.append((candidate,[f[k] for k in feature_names]))
            if batch:
                predictions=model.predict([f for _,f in batch]);ranked=sorted(zip(batch,predictions),key=lambda z:abs(z[1]-target))
                for (candidate,_),prediction in ranked[:int(config['oracleCheckBudget'])]:
                    best,reason=check(candidate,ref,target,portfolio,gate)
                    if best is not None:best['predictedScore']=float(prediction);break
                    rejected[reason]+=1
        if best is not None:
            best['id']='generated-'+ref['id'];best['group']='generated-'+ref['group'];accepted.append(best)
        rejections[ref['id']]=dict(rejected)
        write(partial,{'signature':signature,'levels':accepted,'requestedCount':len(refs),'complete':False})
        print(ref['id'],'PASS' if best is not None else 'INCOMPLETE',dict(rejected),flush=True)
    complete=len(accepted)==len(refs)
    environment={'python':sys.version,'packages':{name:importlib.metadata.version(name) for name in ['numpy','scipy','scikit-learn','networkx','joblib']}}
    report={'complete':complete,'generatedCount':len(accepted),'requestedCount':len(refs),'inputHashes':input_hashes,'helperHashes':helper_hashes,'environment':environment,'resumedIds':sorted(retained),
        'seed':seed,'signature':signature,'datasetKind':config.get('datasetKind','unspecified'),'proposal':proposal_meta,'modelValidation':metrics,'rejections':rejections,'seconds':time.perf_counter()-start,
        'limits':'Scalar solver/agent proxy with selection portfolio; not human difficulty certification. Check adapter replay independence and novelty limitations.'}
    if complete:
        # Final replay reads the accepted states, independent of search success flags.
        if not all(adapter.replay(l,l['solution']) for l in accepted):raise ValueError('Final replay failed')
        pack={'schemaVersion':1,'signature':signature,'datasetKind':config.get('datasetKind','unspecified'),'levels':accepted};write(root/'generated_levels.json',pack)
        report['packSha256']=digest(root/'generated_levels.json');report['modelHashes']={p.name:digest(p) for p in (root/'models').glob('*.joblib')}
        write(root/'validation_report.json',{'passed':True,'packSha256':report['packSha256'],'replays':len(accepted),'signature':signature,
            'inputHashes':input_hashes,'modelHashes':report['modelHashes'],'environment':environment,'criteria':base_gate,'perReference':config.get('perReference',{}),
            'limits':report['limits']})
        if export:
            dest=pathlib.Path(export).resolve()
            protected={reference_path,corpus_path,adapter_path,config_path,pathlib.Path(__file__).resolve(),root/'generated_levels.json',
                root/'validation_report.json',root/'last_run_report.json',root/'model_validation.json',root/'partial_levels.json',
                root/'models/difficulty.joblib',root/'models/proposal.joblib',root/'models/manifest.json'}|{p.resolve() for p in adapter_path.parent.glob('*.py')}
            if dest in protected:raise ValueError('Cannot export over a source/config/adapter/accepted artifact')
            dest.parent.mkdir(parents=True,exist_ok=True)
            if dest.exists():
                import shutil
                backup=dest.with_name(dest.name+'.backup-'+str(time.time_ns()));shutil.copy2(dest,backup)
            temp=dest.with_suffix(dest.suffix+'.tmp');temp.write_bytes((root/'generated_levels.json').read_bytes());temp.replace(dest)
            report['export']={'path':str(dest),'sha256':digest(dest)}
    write(root/'last_run_report.json',report)
    print('COMPLETE' if complete else 'INCOMPLETE',len(accepted),'/',len(refs),flush=True)
    return 0 if complete else 2
if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--config',required=True);parser.add_argument('--resume',action='store_true');parser.add_argument('--export')
    args=parser.parse_args();raise SystemExit(run(args.config,args.resume,args.export))
