"""Game-specific extension of the level-training skill for mixed native geometry.

Learns the proposal from ALL 500 original stages. Difficulty labels explicitly
belong to the reduced pin/gap reconstruction, never the original native game.
Family separation precedes synthetic augmentation. Fresh graphs are constructed,
ranked by a learned predictor, then checked with two policies and independent
replay. Full native fixtures stay in corpus but are not silently certified.
"""
from __future__ import annotations
import argparse, collections, copy, hashlib, importlib.util, json, math, pathlib, random, statistics, sys, time
import joblib,numpy as np
from sklearn.ensemble import ExtraTreesRegressor
from sklearn.metrics import mean_absolute_error,r2_score

def write(path,data):
    path=pathlib.Path(path);path.parent.mkdir(exist_ok=True,parents=True)
    temp=path.with_suffix(path.suffix+'.tmp');temp.write_text(json.dumps(data,ensure_ascii=False,indent=2,allow_nan=False),encoding='utf8');temp.replace(path)
def sha(path):return hashlib.sha256(pathlib.Path(path).read_bytes()).hexdigest()
def adapter(root,config):
    path=root/config['adapter'];spec=importlib.util.spec_from_file_location('rotate_adapter',path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
def rows(path):return json.loads(pathlib.Path(path).read_text(encoding='utf8'))['levels']
def hashes(root,c):return {k:sha(root/c[k]) for k in ('adapter','references','corpus')}|{'config':sha(root/'level_training.json'),'pipeline':sha(__file__)}
def split_families(ls,a,seed):
    families=collections.defaultdict(list)
    for i,l in enumerate(ls):families[a.fingerprint(l)].append(i)
    keys=sorted(families);random.Random(seed).shuffle(keys);test=set(keys[:max(1,round(len(keys)*.2))])
    return [i for k in keys if k not in test for i in families[k]],[i for k in keys if k in test for i in families[k]],{'method':'whole conservative WL graph families, color/IDs/rigid transforms ignored; split before augmentation','trainFamilies':len(keys)-len(test),'testFamilies':len(test),'trainIds':[ls[i]['id'] for k in keys if k not in test for i in families[k]],'testIds':[ls[i]['id'] for k in keys if k in test for i in families[k]]}
def compact(row):
    s=row['state'];keys=('id','kind','x','y','radius','width','gap','angle','color','collectible','key','lockKey','bomb','axis','length')
    return {k:copy.deepcopy(v) for k,v in row.items() if k in ('id','group','position','provenance','projection','originalDifficulty','features')}|{'state':{'parts':[{k:copy.deepcopy(p[k]) for k in keys if k in p} for p in s['parts']],'links':[{k:e[k] for k in ('source','target','bearing','pinWidth')} for e in s['links']], 'sourceMechanics':s.get('sourceMechanics',{}), 'fixtureCount':len(s.get('fixtures',[]))}}
def fit(config_path):
    cp=pathlib.Path(config_path).resolve();root=cp.parent;c=json.loads(cp.read_text(encoding='utf8'));a=adapter(root,c);sources=rows(root/c['corpus']);seed=c['seed']
    if len(sources)<500 or len({l['id'] for l in sources})<500:raise ValueError('500 distinct original stages required')
    tr,te,split=split_families(sources,a,seed);rng=random.Random(seed)
    # Held-out proposal evaluation: nearby stage neighborhoods predict graph/
    # mechanic statistics. Global-mean predictor is a deliberately simple baseline.
    evaluate_keys=['links','branching','depth','components','sliders','anchors']
    train_features=[a.features(sources[i]) for i in tr];test_features=[a.features(sources[i]) for i in te]
    predictions=[];actual=[];base=[]
    for i,f in zip(te,test_features):
        near=sorted(tr,key=lambda j:abs(sources[j]['position']-sources[i]['position']))[:12]
        predictions.append([statistics.mean(a.features(sources[j])[k] for j in near) for k in evaluate_keys])
        actual.append([f[k] for k in evaluate_keys]);base.append([statistics.mean(x[k] for x in train_features) for k in evaluate_keys])
    proposal_metrics={'metrics':{k:{'conditionalMAE':float(mean_absolute_error(np.array(actual)[:,j],np.array(predictions)[:,j])),'globalMeanMAE':float(mean_absolute_error(np.array(actual)[:,j],np.array(base)[:,j]))} for j,k in enumerate(evaluate_keys)},'limits':'Features of held-out original families; evaluates statistical proposal calibration, not gameplay win rate.'}
    # Label sources under declared reduced rules. Preserve all failure reasons.
    labels={};source_status=[]
    for i,l in enumerate(sources):
        result=a.solve(l,{'strategy':'least_turn'})
        replay_ok=result['solved'] and a.replay(l,result['solution'])
        unsupported=l.get('projection',{}).get('unsupportedMechanics',[])
        source_status.append({'id':l['id'],'projectionSolved':bool(replay_ok),'originalSolvability':'unmeasured','unsupportedNativeMechanics':unsupported,'reason':result.get('reason')})
        if replay_ok:labels[i]=a.difficulty(l,result)
        if i%100==0:print('source-label',i+1,'/',len(sources),flush=True)
    feature_names=sorted(a.features(sources[0]));xtrain=[];ytrain=[];xtest=[];ytest=[];label_groups=[]
    def vector(l):return [a.features(l)[k] for k in feature_names]
    for i in tr:
        if i in labels:xtrain.append(vector(sources[i]));ytrain.append(labels[i]);label_groups.append(sources[i]['id'])
    for i in te:
        if i in labels:xtest.append(vector(sources[i]));ytest.append(labels[i])
    # Fresh constrained graph augmentations are generated only from training
    # families. They do not inflate the reported count of original stages.
    train_proposal,_=a.fit_proposal([sources[i] for i in tr],seed)
    for n in range(900):
        ref=sources[rng.choice(tr)];candidate=a.propose(ref,rng,train_proposal);r=a.solve(candidate)
        if r['solved'] and a.replay(candidate,r['solution']):xtrain.append(vector(candidate));ytrain.append(a.difficulty(candidate,r))
    reg=ExtraTreesRegressor(n_estimators=128,min_samples_leaf=2,random_state=seed,n_jobs=-1);reg.fit(xtrain,ytrain)
    pred=reg.predict(xtest) if xtest else []
    metrics={'split':split,'originalStageCount':len(sources),'difficultyLabelMeaning':'reconstructed computer gesture/angle/branching proxy; native original and humans unmeasured',
        'projectionSolvedSources':len(labels),'heldOutSolvedSources':len(ytest),'syntheticTrainRows':len(ytrain)-len(label_groups),
        'difficulty':{'mae':float(mean_absolute_error(ytest,pred)) if ytest else None,'meanBaselineMAE':float(mean_absolute_error(ytest,[statistics.mean(ytrain)]*len(ytest))) if ytest else None,'r2':float(r2_score(ytest,pred)) if len(ytest)>1 else None},'proposal':proposal_metrics,'sourceStatus':source_status}
    # Refit final models after holding out/recording evaluation.
    for i in te:
        if i in labels:xtrain.append(vector(sources[i]));ytrain.append(labels[i])
    reg.fit(xtrain,ytrain);proposal,meta=a.fit_proposal(sources,seed)
    (root/'models').mkdir(exist_ok=True)
    joblib.dump({'model':reg,'features':feature_names},root/'models/difficulty.joblib');joblib.dump({'model':proposal,'metadata':meta},root/'models/proposal.joblib')
    write(root/'model_validation.json',metrics)
    write(root/'models/manifest.json',{'inputHashes':hashes(root,c),'seed':seed,'ruleVersion':a.RULE_VERSION,'originalCount':500,'proposal':meta,'modelHashes':{p.name:sha(p) for p in (root/'models').glob('*.joblib')},'python':sys.version})
    print('MODELS SAVED; proposal learned from',len(sources),'original stages; reduced labels',len(labels),flush=True)

def token_distance(left,right):
    keys=left.keys()|right.keys();return 1-sum(min(left[k],right[k]) for k in keys)/max(1,sum(max(left[k],right[k]) for k in keys))

def generate(config_path,output,count=300,seed=None,exclusions=()):
    cp=pathlib.Path(config_path).resolve();root=cp.parent;c=json.loads(cp.read_text(encoding='utf8'));a=adapter(root,c);out=pathlib.Path(output).resolve()
    if out.exists():raise ValueError('Batch output must be a NEW folder')
    out.mkdir(parents=True);manifest=json.loads((root/'models/manifest.json').read_text());current=hashes(root,c)
    if manifest['inputHashes']!=current:raise ValueError('Training inputs/rules/config changed: retrain in a new bundle before generation')
    for name,h in manifest['modelHashes'].items():
        if sha(root/'models'/name)!=h:raise ValueError('Model hash mismatch')
    proposal=joblib.load(root/'models/proposal.joblib')['model'];diff=joblib.load(root/'models/difficulty.joblib');refs=rows(root/c['references']);sources=rows(root/c['corpus'])
    rng=random.Random(c['seed'] if seed is None else seed);gate=c['acceptance'];accepted=[];fingerprints={a.fingerprint(l) for l in sources};descriptors=[a.descriptor(l) for l in sources];rejected=collections.Counter();comparisons=[];start=time.time()
    for path in exclusions:
        for l in rows(path):fingerprints.add(a.fingerprint(l));descriptors.append(a.descriptor(l))
    ref_labels={}
    for i in range(count):
        ref=refs[i%len(refs)];rf=a.features(ref)
        if ref['id'] not in ref_labels:
            r=a.solve(ref);ref_labels[ref['id']]=a.difficulty(ref,r) if r['solved'] and a.replay(ref,r['solution']) else None
        target=ref_labels[ref['id']];pool=[]
        for attempt in range(gate['maxAttemptsPerLevel']):
            l=a.propose(ref,rng,proposal)
            if a.validate_level(l):rejected['constraints']+=1;continue
            f=a.features(l)
            if ref['position']>3 and abs(f['parts']-rf['parts'])>max(2,gate['countRelativeTolerance']*rf['parts']):rejected['partScale']+=1;continue
            if abs(f['sliders']-rf['sliders'])>max(2,rf['sliders']*.25):rejected['mechanicInventory']+=1;continue
            # Candidate ranking uses trained prediction. Source failing reduced
            # search has no invented score; structural target keeps priority.
            expected=target if target is not None else float(diff['model'].predict([[rf[k] for k in diff['features']]])[0])
            pool.append((l,f,expected))
        if not pool:
            print(i+1,'INCOMPLETE no proposals',flush=True);continue
        predictions=diff['model'].predict([[f[k] for k in diff['features']] for _,f,_ in pool]);ranked=sorted(zip(pool,predictions),key=lambda item:abs(item[1]-item[0][2]))
        chosen=None
        for (candidate,f,expected),prediction in ranked:
            fp=a.fingerprint(candidate)
            if fp in fingerprints:rejected['cosmeticEquivalent']+=1;continue
            desc=a.descriptor(candidate);nearest=min(token_distance(desc,d) for d in descriptors)
            if nearest<gate['nearCopyDistance']:rejected['nearCopy']+=1;continue
            r=a.solve(candidate,{'strategy':'least_turn'})
            if not r['solved'] or not a.replay(candidate,r['solution']):rejected['solverReplay']+=1;continue
            score=a.difficulty(candidate,r)
            # Tiny tutorial exception is declared, all others gate against the
            # actual reduced reference if available. Unsolved original native
            # mechanics are structurally conditioned, not certified as equal.
            rel=abs(score-expected)/max(3,expected)
            if ref['position']>3 and rel>gate['computerScoreRelativeTolerance']:rejected['computerDifficulty']+=1;continue
            portfolio=[];ok=True
            for policy in ({'strategy':'least_turn'},{'strategy':'largest_turn'}):
                pr=a.solve(candidate,policy);valid=pr['solved'] and a.replay(candidate,pr['solution']);portfolio.append({'policy':policy,'solved':valid,'cost':pr['cost'],'score':a.difficulty(candidate,pr) if valid else None});ok&=valid
            if not ok:rejected['portfolio']+=1;continue
            # Exact body non-overlap for freshly constructed circular layout.
            ps=candidate['state']['parts']
            if any(math.hypot(p['x']-q['x'],p['y']-q['y'])<p['radius']+q['radius']+(p['width']+q['width'])/2-.015 for k,p in enumerate(ps) for q in ps[k+1:]):rejected['bodyOverlap']+=1;continue
            candidate.update(id='new-'+str(i+1).zfill(3),group='generated-'+fp,referenceId=ref['id'],solution=r['solution'],acceptance={'score':score,'targetScore':expected,'targetMeaning':'reduced reconstruction' if target is not None else 'learned structural estimate; original unsolved under reduced rules','relativeDifference':rel,'nearestDistance':nearest,'predictedScore':float(prediction),'portfolio':portfolio,'sourcePosition':ref['position']})
            chosen=candidate;fingerprints.add(fp);descriptors.append(desc);accepted.append(chosen);comparisons.append({'position':i+1,'referenceId':ref['id'],'sourceParts':rf['parts'],'newParts':f['parts'],'sourceLinks':rf['links'],'newLinks':f['links'],'score':score,'targetScore':expected,'referenceProxyAvailable':target is not None,'nearestDistance':nearest,'keys':f['keys'],'sliders':f['sliders']});break
        write(out/'partial_levels.json',{'levels':accepted,'complete':False,'requestedCount':count,'rejections':dict(rejected)})
        print(i+1,'PASS' if chosen else 'INCOMPLETE',len(accepted),'/',count,flush=True)
    complete=len(accepted)==count
    report={'passed':complete,'requestedCount':count,'generatedCount':len(accepted),'trainingOriginalCount':manifest['originalCount'],'seed':c['seed'] if seed is None else seed,'inputHashes':current,'modelHashes':manifest['modelHashes'],'criteria':gate,'rejections':dict(rejected),'comparisons':comparisons,'seconds':time.time()-start,'limits':'Reduced contact-sensitive pin/gap reconstruction. Original native continuous collision, butterfly slots and bomb contacts unavailable. Original/human win rate, time, retries and fun not measured. Canonical conservative WL hashes reject cosmetic equivalents; neighborhood edit distance is a heuristic, not a proof of minimum graph edit distance. Solvers are nonoptimal.'}
    if complete:
        for l in accepted:
            if not a.replay(l,l['solution']):raise ValueError('Final independent replay failed')
        write(out/'generated_levels.json',{'schemaVersion':1,'levels':accepted});report['packSha256']=sha(out/'generated_levels.json')
        write(out/'validation_report.json',report)
        engine=[]
        for i,l in enumerate(accepted):
            score=l['acceptance']['score'];f=a.features(l)
            title='First Twist' if i<3 else 'Twin Paths' if f['branching']>5 else 'Slide & Spin' if f['sliders'] else 'Pin Garden'
            engine.append({'id':i+1,'title':title,'referenceId':l['referenceId'],'parMoves':len(l['solution']),'difficulty':score,**l['state'],'solution':l['solution']})
        write(out/'unity_levels.json',{'schemaVersion':1,'levels':engine})
    write(out/'last_run_report.json',report);print('COMPLETE' if complete else 'INCOMPLETE',len(accepted),'/',count,flush=True)
    return 0 if complete else 2

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--config',required=True);p.add_argument('--train',action='store_true');p.add_argument('--output');p.add_argument('--count',type=int,default=300);p.add_argument('--seed',type=int);p.add_argument('--exclude',action='append',default=[]);args=p.parse_args()
    if args.train:fit(args.config)
    if args.output:raise SystemExit(generate(args.config,args.output,args.count,args.seed,args.exclude))
