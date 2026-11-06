"""Normalize static original scene evidence; never assign invented original solutions.

Input: 500 unique stages in recovered TbLevelConfig1 progression order.
The runtime projection is intentionally reduced. Full source fields remain in
hash-bound original scene JSON and relevant part/render fields copied here.
"""
from pathlib import Path
import json,math,hashlib,collections,statistics

ROOT=Path(__file__).resolve().parents[1]
RESEARCH=ROOT/'Research'
VERSION='rotate-rings-scene-normalizer-1.1'

def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def mean(a):return statistics.mean(a) if a else 0.0
def median(a):return statistics.median(a) if a else 0.0
def sd(a):return statistics.pstdev(a) if len(a)>1 else 0.0
def angle(v):return math.degrees(math.atan2(v['y'],v['x']))
def delta(a,b):return (a-b+180)%360-180
def partdata(c):return {k[1:k.index('>')]:v for k,v in c['data'].get('partData',{}).items()}
def go(c):return c['data']['m_GameObject']['m_PathID']
def qangle(q):return math.degrees(math.atan2(2*(q['w']*q['z']+q['x']*q['y']),1-2*(q['y']**2+q['z']**2)))

def graph_stats(parts,links):
    ids={p['id'] for p in parts};adj={i:set() for i in ids};out={i:set() for i in ids};inc={i:0 for i in ids}
    for l in links:
        a,b=l['source'],l['target']
        if a not in ids or b not in ids:continue
        adj[a].add(b);adj[b].add(a);out[a].add(b);inc[b]+=1
    seen=set();components=0
    for i in ids:
        if i in seen:continue
        components+=1;stack=[i];seen.add(i)
        while stack:
            for j in adj[stack.pop()]:
                if j not in seen:seen.add(j);stack.append(j)
    # Tarjan SCCs; longest depth measured on condensation DAG, no recursion cycles.
    index=0;indices={};low={};stack=[];on=set();groups=[]
    def visit(v):
        nonlocal index
        indices[v]=low[v]=index;index+=1;stack.append(v);on.add(v)
        for w in out[v]:
            if w not in indices:visit(w);low[v]=min(low[v],low[w])
            elif w in on:low[v]=min(low[v],indices[w])
        if low[v]==indices[v]:
            group=[]
            while True:
                w=stack.pop();on.remove(w);group.append(w)
                if w==v:break
            groups.append(group)
    for v in ids:
        if v not in indices:visit(v)
    owner={v:i for i,g in enumerate(groups) for v in g};dag={i:set() for i in range(len(groups))}
    for v in ids:
        for w in out[v]:
            if owner[v]!=owner[w]:dag[owner[v]].add(owner[w])
    memo={}
    def depth(v):
        if v not in memo:memo[v]=1+max((depth(w) for w in dag[v]),default=0)
        return memo[v]
    return {'graphComponents':components,'graphCycleRank':max(0,sum(map(len,adj.values()))//2-len(ids)+components),
      'graphDirectedCyclicComponents':sum(len(g)>1 or (len(g)==1 and g[0] in out[g[0]]) for g in groups),
      'graphDepth':max((depth(v) for v in dag),default=0),'maxIncoming':max(inc.values(),default=0),
      'maxOutgoing':max((len(v) for v in out.values()),default=0),'meanDegree':mean([len(v) for v in adj.values()]),
      'branchingParts':sum(len(v)>1 for v in out.values()),'isolatedParts':sum(not a for a in adj.values())}

def normalize(source,position):
    components=source['components'];bygo=collections.defaultdict(dict)
    for c in components:bygo[go(c)][c['class']]=c
    info=next(c['data'] for c in components if c['class']=='GameLevelInfo')
    objects=source['objects']; transforms={int(k):v['data'] for k,v in objects.items() if v['type']=='Transform'}
    worldcache={}
    def world(t):
        if t is None:return (0.,0.,0.,1.,1.)
        key=t['m_GameObject']['m_PathID']
        if key in worldcache:return worldcache[key]
        p=t['m_LocalPosition'];s=t['m_LocalScale'];a=qangle(t['m_LocalRotation'])
        parent=transforms.get(t['m_Father']['m_PathID'])
        if parent:
            x,y,pa,sx,sy=world(parent);r=math.radians(pa)
            px,py=p['x']*sx,p['y']*sy
            result=(x+px*math.cos(r)-py*math.sin(r),y+px*math.sin(r)+py*math.cos(r),a+pa,s['x']*sx,s['y']*sy)
        else:result=(p['x'],p['y'],a,s['x'],s['y'])
        worldcache[key]=result;return result
    parts=[];fixtures=[];source_ids={};warnings=[]
    mapping={'RingObject':'ring','PullObject':'slider','FixedObject':'catch','FixedButterflyObject':'butterfly'}
    for c in components:
        if c['class'] not in mapping:continue
        pd=partdata(c);sid=pd.get('PhysicPartId');pid=pd.get('Id')
        if not sid or not isinstance(pid,int):warnings.append('missing_part_identity:'+str(c['gameObject']));continue
        if sid in source_ids:warnings.append('duplicate_source_id:'+sid)
        source_ids[sid]=pid
        x,y,a,sx,sy=world(c['transform']);kind=mapping[c['class']]
        render=bygo[go(c)].get('RingSegmentController' if kind=='ring' else 'ArrowRibbonController',{}).get('data',{})
        scale=(abs(sx)+abs(sy))/2
        radius=render.get('_radius',0)*scale if kind=='ring' else 1.0
        width=render.get('_width',render.get('_buildSettings',{}).get('width',1.0))*scale
        path=render.get('_pathPoints',[])
        length=sum(math.hypot(v['x']-u['x'],v['y']-u['y']) for u,v in zip(path,path[1:]))*scale
        p={'id':pid,'kind':kind,'sourceId':sid,'sourceClass':c['class'],'gameObject':c['gameObject'],
          'x':x,'y':y,'radius':radius,'width':width,'gap':float(pd.get('GapAngleDeg',0)),
          'angle':(a+render.get('_gapDirectionDeg',0))%360,'color':pd.get('PaletteColor',0),
          # CanCollect is score/reward eligibility in the source, NOT release or win eligibility.
          # Every movable ring/bar registers with SolveService and must be released.
          'collectible':kind in ('ring','slider'),'scoreEligible':bool(pd.get('CanCollect',0)),
          'key':0,'lockKey':0,'bomb':bool(pd.get('HasBomb',0)),
          'axis':a%360,'length':length,'sourcePartData':pd,'sourceNativeData':c['data'],
          'sourceRenderData':render,'sourceTransform':c['transform'],'sourceWorldScale':[sx,sy],
          'sourceVisualGap':render.get('_gapAngleDeg',0),'sourceNativeInnerRadius':pd.get('InnerRadius',0),
          'sourceNativeOuterRadius':pd.get('OuterRadius',0)}
        if kind in ('ring','slider'):parts.append(p)
        else:fixtures.append(p)
    parts.sort(key=lambda p:p['id']);fixtures.sort(key=lambda p:p['id'])
    allparts=parts+fixtures;byid={p['id']:p for p in allparts}
    if len(byid)!=len(allparts):warnings.append('duplicate_numeric_id')
    links=[];unprojected=[]
    for link in info.get('ringLinks',[]):
        src=source_ids.get(link['sourceRingId']);tgt=source_ids.get(link['targetPartId'])
        if src is None or tgt is None:
            unprojected.append(link);warnings.append('unresolved_ring_link');continue
        a,b=byid[src],byid[tgt]
        bearing=math.degrees(math.atan2(a['y']-b['y'],a['x']-b['x']))%360
        l={'source':src,'target':tgt,'bearing':bearing,'pinWidth':max(0.01,a['width']*.83),
          'sourceFeature':link['pinFeatureId'],'targetFeature':link['targetFeatureId'],
          'relationKind':link['relationKind'],'activityKind':link['activityKind'],'sourceNativeData':link}
        if a['kind'] in ('ring','slider') and b['kind'] in ('ring','slider'):links.append(l)
        else:unprojected.append(link)
    rings=[p for p in parts if p['kind']=='ring'];sliders=[p for p in parts if p['kind']=='slider']
    radii=[p['radius'] for p in rings];gaps=[p['gap'] for p in rings];widths=[p['width'] for p in rings]
    xmin=min((p['x']-p['radius'] for p in parts),default=0);xmax=max((p['x']+p['radius'] for p in parts),default=0)
    ymin=min((p['y']-p['radius'] for p in parts),default=0);ymax=max((p['y']+p['radius'] for p in parts),default=0)
    bw=max(1e-6,xmax-xmin);bh=max(1e-6,ymax-ymin);cx=(xmin+xmax)/2;cy=(ymin+ymax)/2;span=max(bw,bh)
    for p in allparts:p['normalizedX']=(p['x']-cx)/span;p['normalizedY']=(p['y']-cy)/span;p['normalizedRadius']=p['radius']/span
    positions=[(p['normalizedX'],p['normalizedY']) for p in parts]
    def symerr(axis):
        if not positions:return 0.
        return mean([min(math.hypot(((-x if axis=='x' else x)-xx),((-y if axis=='y' else y)-yy)) for xx,yy in positions) for x,y in positions])
    efforts=[];clearances=[]
    for l in links:
        p=byid[l['target']]
        if p['kind']=='ring':
            diff=abs(delta(p['angle'],l['bearing']));clear=max(0.,p['gap']/2-9)
            efforts.append(max(0.,diff-clear));clearances.append(clear)
    counts=collections.Counter(c['class'] for c in components)
    locked=sum(p['sourcePartData'].get('LockCount',0)>0 for p in allparts)
    colorlocks=sum(bool(p['sourcePartData'].get('IsPaletteLock',0)) for p in allparts)
    colorkeys=sum(bool(p['sourcePartData'].get('IsPaletteLockKey',0)) for p in allparts)
    mechanics={'rings':len(rings),'sliders':len(sliders),'catches':len([p for p in fixtures if p['kind']=='catch']),
      'butterflies':counts['FixedButterflyObject'],'bombs':counts['BombRelative'],'chainLockedParts':locked,
      'paletteLockedParts':colorlocks,'paletteKeyParts':colorkeys,'guides':len(info.get('guideLinks',[])),
      'authoredRingLinks':len(info.get('ringLinks',[])),'projectedRingLinks':len(links),
      'complexSliderPaths':sum(len(p['sourceRenderData'].get('_pathPoints',[]))>2 for p in sliders)}
    f={'partCount':len(parts),'ringCount':len(rings),'sliderCount':len(sliders),'fixtureCount':len(fixtures),
      'collectibleCount':sum(p['collectible'] for p in parts),'collectibleRingCount':sum(p['collectible'] for p in rings),
      'scoreEligibleCount':sum(p['scoreEligible'] for p in parts),'scoreEligibleRingCount':sum(p['scoreEligible'] for p in rings),
      'anchorRingCount':sum(not p['collectible'] for p in rings),'closedRingCount':sum(p['gap']<=0 for p in rings),
      'linkCount':len(links),'guideLinkCount':mechanics['guides'],'butterflyCount':mechanics['butterflies'],
      'bombCount':mechanics['bombs'],'chainLockedPartCount':locked,'paletteLockCount':colorlocks,'paletteKeyCount':colorkeys,
      'meanRadius':mean(radii),'medianRadius':median(radii),'radiusStd':sd(radii),'minRadius':min(radii,default=0),'maxRadius':max(radii,default=0),
      'meanGap':mean(gaps),'minGap':min(gaps,default=0),'maxGap':max(gaps,default=0),'gapStd':sd(gaps),'meanBandWidth':mean(widths),
      'meanSliderLength':mean([p['length'] for p in sliders]),'complexSliderCount':mechanics['complexSliderPaths'],
      'boardWidth':bw,'boardHeight':bh,'aspect':bw/bh,'density':sum(math.pi*r*r for r in radii)/(bw*bh),
      'symmetryXError':symerr('x'),'symmetryYError':symerr('y'),
      'initialAngularEffortSum':sum(efforts),'initialAngularEffortMean':mean(efforts),'initialAngularEffortMax':max(efforts,default=0),
      'initialOpenPinCount':sum(v<=1e-6 for v in efforts),'meanPinClearance':mean(clearances),**graph_stats(parts,links)}
    assert all(math.isfinite(v) for v in f.values())
    rawpath=RESEARCH/'levels'/f"{source['id']}.json"
    return {'id':source['id'],'group':'original-'+source['id'],'position':position,
      'state':{'parts':parts,'links':links,'fixtures':fixtures,'sourceMechanics':mechanics,
        'guideLinks':info.get('guideLinks',[]),'paletteLockKeyBindings':info.get('paletteLockKeyBindings',[]),
        'unprojectedRingLinks':unprojected,'sourceLevelInfo':info},'features':f,
      'provenance':{'sourceFamily':'RotateRings-1.2.0-APK-TbLevelConfig1','progressionTable':1,'progressionPosition':position,
        'rawPath':str(rawpath.relative_to(RESEARCH)).replace('\\','/'),'rawSha256':sha(rawpath),
        'bundle':source['bundle'],'bundleSha256':source['sha256'],'parserVersion':VERSION},
      'originalDifficulty':{'status':'unmeasured','humanDifficulty':None,'originalSolverCost':None},
      'projection':{'status':'reduced-geometry-unverified','exactOriginalSolvability':'unmeasured',
        'unsupportedMechanics':[k for k in ('catches','butterflies','bombs','chainLockedParts','paletteLockedParts','paletteKeyParts','complexSliderPaths') if mechanics[k]],
        'warnings':warnings}}

def main():
    progression_path=RESEARCH/'rules/main_progressions.json';progression=json.loads(progression_path.read_text())['tables']['1']
    assert len(progression)==500 and len({p['core'] for p in progression})==500
    levels=[];rejected=[]
    for row in sorted(progression,key=lambda r:r['position']):
        path=RESEARCH/'levels'/f"{row['core']}.json"
        try:
            source=json.loads(path.read_text());record=normalize(source,row['position']);levels.append(record)
        except Exception as e:rejected.append({'sourceId':row['core'],'position':row['position'],'reason':str(e)})
    content={'schemaVersion':1,'game':'Rotate Rings','normalizerVersion':VERSION,
      'corpusScope':{'sourceStageCount':len(levels),'requestedMinimum':500,'progressionTable':1,
        'originalDifficultyMeasured':False,'originalSolutionsProvided':False,'rawSourcePreserved':True,
        'notes':'Full original components and native serialized fields retained in hash-bound Research/levels/*.json. Projection is for learned structural statistics and reduced-adapter candidate design, not exact native-runtime labels.'},
      'levels':levels,'rejected':rejected}
    out=RESEARCH/'normalized_corpus.json';out.write_text(json.dumps(content,separators=(',',':')),encoding='utf8')
    feature_names=sorted(levels[0]['features']) if levels else []
    design={'schemaVersion':1,'game':'Rotate Rings','sourceCount':len(levels),'featureNames':feature_names,
      'corpusSha256':sha(out),'normalizerSha256':sha(Path(__file__)),'progressionSha256':sha(progression_path),
      'first300Targets':[l['id'] for l in levels[:300]],
      'rows':[{'id':l['id'],'group':l['group'],'position':l['position'],'features':l['features'],'sourceMechanics':l['state']['sourceMechanics'],'projection':l['projection']} for l in levels],
      'distributions':{k:{'min':min(v),'max':max(v),'mean':mean(v),'median':median(v),'std':sd(v),'p10':sorted(v)[int(.1*(len(v)-1))],'p90':sorted(v)[int(.9*(len(v)-1))]} for k in feature_names for v in [[l['features'][k] for l in levels]]},
      'validation':{'malformedRejected':rejected,'uniqueOriginalIds':len({l['id'] for l in levels}),
        'allFeaturesFinite':all(math.isfinite(v) for l in levels for v in l['features'].values()),
        'exactOriginalSolvability':'unmeasured','sourceSolutions':'not fabricated'}}
    (RESEARCH/'source_design_features.json').write_text(json.dumps(design,indent=2),encoding='utf8')
    print(json.dumps({'count':len(levels),'rejected':rejected,'projectedParts':sum(len(l['state']['parts']) for l in levels),
      'projectedLinks':sum(len(l['state']['links']) for l in levels),'warnings':collections.Counter(w for l in levels for w in l['projection']['warnings']),
      'corpusBytes':out.stat().st_size,'corpusSha256':sha(out),'mechanicTotals':{k:sum(l['state']['sourceMechanics'][k] for l in levels) for k in levels[0]['state']['sourceMechanics']}}))
    if len(levels)!=500:raise SystemExit(2)

if __name__=='__main__':main()
