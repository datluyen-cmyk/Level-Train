"""Paired headless virtual-player sessions. Synthetic evidence, never human data."""
from __future__ import annotations
import argparse,collections,copy,hashlib,json,math,pathlib,random,statistics,time
from level_training import load_adapter,records,digest,write

def summarize(rows):
    wins=[r for r in rows if r['won']];n=len(rows);p=len(wins)/n;z=1.96
    center=(p+z*z/(2*n))/(1+z*z/n);radius=z*math.sqrt(p*(1-p)/n+z*z/(4*n*n))/(1+z*z/n)
    return {'sessions':n,'winRate':p,'winRateMonteCarloWilson95':[max(0,center-radius),min(1,center+radius)],
        'budgetExhaustionRate':sum(r['budgetExhausted'] for r in rows)/n,
        'allSessionActionsMedian':statistics.median(r['actions'] for r in rows),
        'successfulSessionActionsMedian':statistics.median(r['actions'] for r in wins) if wins else None,
        'modeledSecondsMedian':statistics.median(r['modeledSeconds'] for r in rows),
        'invalidActionsMean':statistics.mean(r['invalidActions'] for r in rows),
        'hintsMean':statistics.mean(r['hints'] for r in rows),'undoMean':statistics.mean(r['undos'] for r in rows)}

def session(adapter,level,profile,seed):
    episode=adapter.new_episode(copy.deepcopy(level),seed);rng=random.Random(seed);clock=random.Random(seed+19081);memory={}
    actions=invalid=hints=undos=0;seconds=0.;max_actions=int(profile['maxActions']);status=adapter.episode_status(episode)
    while not status['terminated'] and actions<max_actions:
        # Private environment and complete level never enter the policy call.
        observation=copy.deepcopy(adapter.observe(episode,profile))
        action=adapter.agent_action(observation,memory,copy.deepcopy(profile),rng)
        event=adapter.step_episode(episode,action);actions+=1
        invalid+=not event.get('valid',False);hints+=bool(event.get('hintUsed',False));undos+=bool(event.get('undoUsed',False))
        seconds+=clock.lognormvariate(math.log(max(.001,float(profile.get('decisionSeconds',1.)))),float(profile.get('decisionLogSigma',.25)))
        status=adapter.episode_status(episode)
    return {'won':bool(status['won']),'terminationReason':status.get('reason','won' if status['won'] else ('terminated' if status['terminated'] else 'action budget')),'actions':actions,'invalidActions':invalid,'hints':hints,'undos':undos,
        'budgetExhausted':not status['terminated'] and actions>=max_actions,'modeledSeconds':seconds}

def run(config_path,pack_path,users=1000,phase='selection'):
    if not 1<=users<=100000:raise ValueError('Choose a bounded positive virtual-user count (maximum100000 per run)')
    begin=time.perf_counter();config_path=pathlib.Path(config_path).resolve();root=config_path.parent
    config=json.loads(config_path.read_text(encoding='utf8'));adapter_path=(root/config['adapter']).resolve();adapter=load_adapter(adapter_path)
    for name in ['new_episode','observe','agent_action','step_episode','episode_status']:
        if not callable(getattr(adapter,name,None)):raise ValueError('Game-specific player simulation not implemented: '+name)
    settings=config.get('playtest')
    if settings is None and hasattr(adapter,'default_playtest_settings'):settings=adapter.default_playtest_settings()
    if not settings:raise ValueError('Configure game-specific playtest profiles first')
    profiles=[p for p in settings['profiles'] if p.get('split','selection')==phase]
    if not profiles:raise ValueError('No profiles reserved for phase '+phase)
    for profile in profiles:
        if float(profile['weight'])<=0 or int(profile['maxActions'])<=0:raise ValueError('Positive profile weights and action budgets required')
    refs_path=(root/config['references']).resolve();refs={l['id']:l for l in records(refs_path)}
    pack_path=pathlib.Path(pack_path).resolve();pack=json.loads(pack_path.read_text(encoding='utf8'));generated=pack['levels']
    if not generated or len({str(l['referenceId']) for l in generated})!=len(generated) or len({str(l['id']) for l in generated})!=len(generated):raise ValueError('Expected unique generated IDs and one counterpart per referenceId')
    for l in generated:
        if str(l['referenceId']) not in refs:raise ValueError('Unknown referenceId '+str(l['referenceId']))
        for level in [l,refs[str(l['referenceId'])]]:
            errors=adapter.validate_level(level)
            if errors:raise ValueError('Unsupported simulation state: '+str(errors))
    out=root/('playtest-'+phase);out.mkdir(exist_ok=True);seed=int(settings.get('seed',config['seed']));rng=random.Random(seed)
    samples=int(settings.get('bootstrapSamples',200))
    if not 20<=samples<=10000:raise ValueError('Choose20..10000 bootstrap samples')
    assigned=[];cohort_counts=collections.Counter();buckets=collections.defaultdict(list);paired=collections.defaultdict(list)
    with (out/'sessions.jsonl').open('w',encoding='utf8') as stream:
        for user in range(users):
            profile=copy.deepcopy(rng.choices(profiles,[float(p['weight']) for p in profiles])[0])
            for key,bounds in profile.pop('parameterRanges',{}).items():
                low,high=bounds
                if low>high:raise ValueError('Invalid parameter range '+key)
                profile[key]=rng.uniform(low,high)
            assigned.append(profile);cohort_counts[profile['cohort']]+=1
            for index,new in enumerate(generated):
                rid=str(new['referenceId']);reference=refs[rid];session_seed=seed+user*1000003+index*1009
                pair={}
                # Alternating order is recorded; this runner resets memory per session.
                order=[('reference',reference),('generated',new)]
                if user%2:order.reverse()
                for side,level in order:
                    result=session(adapter,level,profile,session_seed)
                    row={'user':user,'cohort':profile['cohort'],'policyFamily':profile['policyFamily'],'referenceId':rid,
                        'levelId':str(level['id']),'side':side,'seed':session_seed,'synthetic':True,**result}
                    stream.write(json.dumps(row,ensure_ascii=False)+'\n');buckets[(rid,side,profile['cohort'])].append(row);pair[side]=row
                paired[rid].append(int(pair['generated']['won'])-int(pair['reference']['won']))
            if (user+1)%max(1,users//10)==0:print('Virtual users',user+1,'/',users,flush=True)
    summaries=[];labels=[];bootstrap=random.Random(seed+9901)
    for new in generated:
        rid=str(new['referenceId']);by_side={};by_cohort={}
        for side,level in [('reference',refs[rid]),('generated',new)]:
            groups={cohort:summarize(buckets[(rid,side,cohort)]) for cohort in cohort_counts}
            by_cohort[side]=groups;all_rows=[r for cohort in cohort_counts for r in buckets[(rid,side,cohort)]];by_side[side]=summarize(all_rows)
            labels.append({'id':str(level['id']),'group':str(refs[rid]['group']),'sourceKind':'synthetic virtual-player telemetry',
                'side':side,'referenceId':rid,'fingerprint':adapter.fingerprint(level),'features':adapter.features(level),
                'metrics':by_side[side],'metricsByCohort':groups})
        differences=paired[rid];draws=sorted(statistics.mean(bootstrap.choices(differences,k=users)) for _ in range(samples))
        summaries.append({'referenceId':rid,'reference':by_side['reference'],'generated':by_side['generated'],'byCohort':by_cohort,
            'pairedWinRateDifference':statistics.mean(differences),'pairedUserBootstrap95':[draws[int(.025*samples)],draws[min(samples-1,int(.975*samples))]]})
    input_hashes={'config':digest(config_path),'references':digest(refs_path),'pack':digest(pack_path),'adapter':digest(adapter_path),'runner':digest(__file__)}
    helper_hashes={p.name:digest(p) for p in adapter_path.parent.glob('*.py')}
    report={'synthetic':True,'phase':phase,'virtualUsers':users,'sessions':users*len(generated)*2,'referencePairs':len(generated),
        'policyFamilies':sorted({p['policyFamily'] for p in assigned}),'cohortCounts':dict(cohort_counts),'seed':seed,'settings':settings,
        'memoryMode':'reset each session; no progression learning','inputHashes':input_hashes,'helperHashes':helper_hashes,
        'results':summaries,'seconds':time.perf_counter()-begin,
        'limits':'Virtual policy models, not actual humans or independent learned models. Reaction time is modeled. Confidence intervals reflect Monte Carlo uncertainty conditional on these policies, not human-population uncertainty. Replay/novelty and engine QA remain separate.'}
    write(out/'profiles.json',{'synthetic':True,'profiles':assigned})
    write(out/'synthetic_labels.json',{'synthetic':True,'phase':phase,'inputHashes':input_hashes,'levels':labels})
    report['outputHashes']={name:digest(out/name) for name in ['sessions.jsonl','profiles.json','synthetic_labels.json']}
    write(out/'report.json',report);print('Wrote',out,'— synthetic sessions:',report['sessions'],flush=True)
    return report
if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--config',required=True);parser.add_argument('--pack',required=True)
    parser.add_argument('--users',type=int,default=1000);parser.add_argument('--phase',choices=['selection','holdout'],default='selection')
    args=parser.parse_args();run(args.config,args.pack,args.users,args.phase)
