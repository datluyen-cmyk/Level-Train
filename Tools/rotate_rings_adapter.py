"""Contact-sensitive pin/gap reconstruction, NOT the unavailable original native Core.

Angles are degrees CCW. Sources cannot start while an outgoing pin is trapped.
Targets may rotate; contact is recomputed (not permanently deleted). This reduced
adapter covers ring/pin, free/anchor rings, one-axis sliders and color keys. Native
butterfly, arbitrary rigid-body collision and explosive bombs are not certified.
"""
from __future__ import annotations
import collections, copy, hashlib, json, math, random, statistics
import networkx as nx

RULE_VERSION='pin-gap-reconstruction-1'
CLEARANCE=9.0

def state(level):return level.get('state',level)
def delta(a,b):return (a-b+180)%360-180
def xyangle(a,b):return math.degrees(math.atan2(a['y']-b['y'],a['x']-b['x']))%360
def validate_level(level):
    s=state(level);ps=s.get('parts',[]);ids={p['id'] for p in ps};errors=[]
    if len(ids)!=len(ps) or not ps:errors.append('empty/duplicate parts')
    for p in ps:
        if p['kind'] not in ('ring','slider'):errors.append('unsupported part')
        if any(not math.isfinite(float(p.get(k,0))) for k in ('x','y','radius','angle','gap')):errors.append('nonfinite geometry')
        if p['radius']<=0 or not 0<=p['gap']<=360:errors.append('invalid radius/gap')
        if p.get('bomb') and not level.get('projection'):errors.append('bomb contact model unverified')
    if any(e['source'] not in ids or e['target'] not in ids or e['source']==e['target'] for e in s.get('links',[])):errors.append('invalid relation')
    return sorted(set(errors))

def graph(level):
    s=state(level);g=nx.DiGraph()
    for p in s['parts']:
        # Cosmetic colors/identifiers/initial angles do not make a new puzzle.
        role=(p['kind'],int(p.get('collectible',True)),round(p.get('gap',90)/15),bool(p.get('key')),bool(p.get('lockKey')))
        g.add_node(p['id'],role=str(role))
    for e in s.get('links',[]):g.add_edge(e['source'],e['target'])
    return g

def fingerprint(level):
    g=graph(level)
    # Conservative WL collisions REJECT some novel graphs; cannot admit an
    # isomorphic cosmetic equivalent. Independent of position/rotation/reflection.
    return nx.weisfeiler_lehman_graph_hash(g,node_attr='role',iterations=5)

def descriptor(level):
    g=graph(level);tokens=collections.Counter()
    for v in g:
        tokens[('node',g.nodes[v]['role'],g.in_degree(v),g.out_degree(v))]+=1
        near=sorted((g.nodes[w]['role'],g.in_degree(w),g.out_degree(w)) for w in set(g.predecessors(v))|set(g.successors(v)))
        tokens[('neighborhood',g.nodes[v]['role'],g.in_degree(v),g.out_degree(v),str(near))]+=1
    for a,b in g.edges:tokens[('edge',g.in_degree(a),g.out_degree(a),g.in_degree(b),g.out_degree(b))]+=1
    return tokens

def novelty_distance(left,right):
    if fingerprint(left)==fingerprint(right):return 0.
    a=descriptor(left);b=descriptor(right);keys=a.keys()|b.keys()
    return 1-sum(min(a[k],b[k]) for k in keys)/max(1,sum(max(a[k],b[k]) for k in keys))

def features(level):
    s=state(level);ps=s['parts'];g=graph(level);n=len(ps)
    rings=[p for p in ps if p['kind']=='ring'];collect=sum(p.get('collectible',True) for p in ps)
    xs=[p['x'] for p in ps];ys=[p['y'] for p in ps];w=max(xs)-min(xs)+2;h=max(ys)-min(ys)+2
    gap=[p['gap'] for p in rings if p['gap']>0]
    undirected=g.to_undirected();components=nx.number_connected_components(undirected)
    depth=nx.dag_longest_path_length(g) if nx.is_directed_acyclic_graph(g) else n
    return {'parts':float(n),'rings':float(len(rings)),'sliders':float(n-len(rings)),'anchors':float(n-collect),
        'links':float(g.number_of_edges()),'components':float(components),'depth':float(depth),
        'branching':float(sum(g.in_degree(v)>1 or g.out_degree(v)>1 for v in g)),
        'maxIn':float(max(dict(g.in_degree()).values(),default=0)),'maxOut':float(max(dict(g.out_degree()).values(),default=0)),
        'sinks':float(sum(g.out_degree(v)==0 for v in g)),'gapMean':statistics.mean(gap) if gap else 0.,
        'closed':float(sum(p.get('gap',90)==0 for p in rings)),'keys':float(sum(bool(p.get('key')) for p in ps)),
        'density':n/(w*h),'aspect':w/h}

def active_links(level,angles,removed):
    s=state(level);ps={p['id']:p for p in s['parts']};out=[]
    for e in s.get('links',[]):
        a=e['source'];b=e['target']
        if a in removed or b in removed:continue
        t=ps[b];bearing=e['bearing']+delta(angles[a],ps[a]['angle'])
        if t['kind']=='slider' or t['gap']<=2*CLEARANCE or abs(delta(angles[b],bearing))>t['gap']/2-CLEARANCE+1e-6:out.append(e)
    return out

def new_session(level):
    return {'angles':{p['id']:p['angle'] for p in state(level)['parts']},'removed':set(),'keys':set()}

def settle(level,session,touched):
    s=state(level);related={touched}
    for e in s.get('links',[]):
        if e['source']==touched:related.add(e['target'])
        if e['target']==touched:related.add(e['source'])
    for _ in range(len(s['parts'])):
        changed=False;active=active_links(level,session['angles'],session['removed'])
        for p in s['parts']:
            pid=p['id']
            if pid not in related or pid in session['removed'] or p['kind']=='slider' or not p.get('collectible',True):continue
            if p.get('lockKey',0) and p['lockKey'] not in session['keys']:continue
            if any(pid in (e['source'],e['target']) for e in active):continue
            session['removed'].add(pid);changed=True
            if p.get('key'):session['keys'].add(p['key'])
        if not changed:break

def apply_action(level,session,action):
    """Checked transition interpreter. Return event; never trust a saved solution."""
    ps={p['id']:p for p in state(level)['parts']};pid=action['part']
    if pid not in ps or pid in session['removed']:return {'valid':False,'reason':'missing/removed'}
    p=ps[pid];lock=p.get('lockKey',0)
    if lock and lock not in session['keys']:return {'valid':False,'reason':'key lock'}
    links=active_links(level,session['angles'],session['removed'])
    if any(e['source']==pid for e in links):return {'valid':False,'reason':'outgoing pin trapped'}
    if p['kind']=='slider':
        if action.get('kind')!='slide':return {'valid':False,'reason':'requires axis slide'}
        session['removed'].add(pid)
    else:
        if action.get('kind')!='rotate':return {'valid':False,'reason':'requires rotation'}
        linked=any(pid in (e['source'],e['target']) for e in links)
        if abs(delta(float(action['angle']),session['angles'][pid])) < (2. if linked else 12.)-1e-5:return {'valid':False,'reason':'gesture too small'}
        session['angles'][pid]=float(action['angle'])%360
        after=active_links(level,session['angles'],session['removed'])
        if p.get('collectible',True) and not any(pid in (e['source'],e['target']) for e in after):session['removed'].add(pid)
    if pid in session['removed'] and p.get('key'):session['keys'].add(p['key'])
    settle(level,session,pid)
    return {'valid':True,'removed':pid in session['removed']}

def won(level,session):return all(not p.get('collectible',True) or p['id'] in session['removed'] for p in state(level)['parts'])

def action_candidates(level,session):
    ps=state(level)['parts'];active=active_links(level,session['angles'],session['removed']);result=[]
    for p in ps:
        pid=p['id']
        if pid in session['removed'] or p.get('lockKey',0) and p['lockKey'] not in session['keys']:continue
        if any(e['source']==pid for e in active):continue
        if p['kind']=='slider':result.append({'part':pid,'kind':'slide','angle':p.get('axis',0)});continue
        incoming=[e for e in active if e['target']==pid]
        bearings=[(e['bearing']+delta(session['angles'][e['source']],next(q['angle'] for q in ps if q['id']==e['source'])))%360 for e in incoming]
        angles=[]
        if not bearings:angles=[(session['angles'][pid]+15)%360]
        elif p['gap']>2*CLEARANCE:
            angles.extend(bearings)
            for a in bearings:
                aligned=[a+delta(b,a) for b in bearings]
                angles.append((min(aligned)+max(aligned))/2%360)
        for angle in set(round(a,6) for a in angles):
            if abs(delta(angle,session['angles'][pid]))<.001:continue
            result.append({'part':pid,'kind':'rotate','angle':angle})
    return result

def solve(level,policy=None):
    policy=policy or {};session=new_session(level);solution=[];effort=0;travel=0;choices=[];seen=set()
    for step in range(int(policy.get('max_actions',len(state(level)['parts'])*5+10))):
        if won(level,session):return {'solved':True,'solution':solution,'cost':len(solution),'effort':effort,'angularTravel':travel,'meanChoices':statistics.mean(choices) if choices else 0}
        key=(tuple(sorted(session['removed'])),tuple((k,round(v,2)) for k,v in sorted(session['angles'].items())))
        if key in seen:break
        seen.add(key);candidates=action_candidates(level,session);ranked=[]
        for a in candidates:
            nxt=copy.deepcopy(session);event=apply_action(level,nxt,a);effort+=1
            if not event['valid']:continue
            removed=len(nxt['removed'])-len(session['removed']);before=len(active_links(level,session['angles'],session['removed']));after=len(active_links(level,nxt['angles'],nxt['removed']))
            benefit=removed*100+(before-after)*8+len(nxt['keys']-session['keys'])*20
            angular=abs(delta(a['angle'],session['angles'][a['part']])) if a['kind']=='rotate' else 45
            rank=benefit-angular/360
            if policy.get('strategy')=='largest_turn':rank=benefit+angular/360
            ranked.append((rank,angular,a,nxt))
        choices.append(sum(x[0]>=99 for x in ranked))
        if not ranked:break
        _,angular,a,session=max(ranked,key=lambda x:(x[0],-x[2]['part']))
        solution.append(a);travel+=angular
    return {'solved':False,'solution':solution,'cost':len(solution),'effort':effort,'reason':'reduced action search stalled; not proof of original unsolvability'}

def replay(level,solution):
    # Separate oracle: explicit state checks and contact tests, no call to solve or
    # apply_action. It intentionally implements the same declared reduced rules.
    s=state(level);ps={p['id']:p for p in s['parts']};angles={i:p['angle'] for i,p in ps.items()};gone=set();keys=set()
    for a in solution:
        i=a['part']
        if i not in ps or i in gone:return False
        p=ps[i]
        if p.get('lockKey',0) and p['lockKey'] not in keys:return False
        def connected(e):
            u,v=e['source'],e['target']
            if u in gone or v in gone:return False
            t=ps[v];bearing=e['bearing']+((angles[u]-ps[u]['angle']+180)%360-180)
            return t['kind']=='slider' or t['gap']<=18 or abs((angles[v]-bearing+180)%360-180)>t['gap']/2-9+1e-6
        if any(e['source']==i and connected(e) for e in s.get('links',[])):return False
        was_linked=any(i in (e['source'],e['target']) and connected(e) for e in s.get('links',[]))
        if p['kind']=='slider':
            if a['kind']!='slide':return False
            gone.add(i)
        else:
            if a['kind']!='rotate':return False
            if abs((float(a['angle'])-angles[i]+180)%360-180)<(2 if was_linked else 12)-1e-5:return False
            angles[i]=float(a['angle'])%360
            if p.get('collectible',True) and not any(i in (e['source'],e['target']) and connected(e) for e in s.get('links',[])):gone.add(i)
        if i in gone and p.get('key'):keys.add(p['key'])
        related={i}
        for e in s.get('links',[]):
            if e['source']==i:related.add(e['target'])
            if e['target']==i:related.add(e['source'])
        for _ in range(len(ps)):
            changed=False
            for j,q in ps.items():
                if j in gone or j not in related or not q.get('collectible',True) or q['kind']=='slider':continue
                if q.get('lockKey',0) and q['lockKey'] not in keys:continue
                if any(j in (e['source'],e['target']) and connected(e) for e in s.get('links',[])):continue
                gone.add(j);changed=True
                if q.get('key'):keys.add(q['key'])
            if not changed:break
    return all(not p.get('collectible',True) or i in gone for i,p in ps.items())

def difficulty(level,result):
    # Non-optimal computer proxy: gestures + angular precision + branching and
    # solver choice effort. Human time/retries/fun remain unmeasured.
    f=features(level)
    return result['cost']+result.get('angularTravel',0)/180+f['depth']*.65+f['branching']*.35+f['keys']*.8

def fit_proposal(levels,seed):
    donors=[]
    for row in levels:
        f=features(row);s=state(row);ps=s['parts'];rs=[p for p in ps if p['kind']=='ring']
        donors.append({'id':row['id'],'position':row.get('position',len(donors)+1),'features':f,
            'gaps':[p['gap'] for p in rs if p['gap']>18],
            'widthRatios':[p.get('width',p['radius']*.28)/p['radius'] for p in rs],
            'collectibles':sum(p.get('collectible',True) for p in ps),
            'mechanics':s.get('sourceMechanics',{})})
    model={'version':RULE_VERSION,'donors':donors,'seed':seed,'fitCount':len(levels),
        'globalMeans':{k:statistics.mean(d['features'][k] for d in donors) for k in donors[0]['features']}}
    return model,{'kind':'empirical conditional joint feature distribution + fresh constrained spatial graph construction',
        'sourceCount':len(levels),'neural':False,'augmentation':'none','mutationOfSourceLayout':False}

def propose(reference,rng,proposal_model):
    donors=proposal_model['donors'];pos=reference.get('position',1)
    if '_byId' not in proposal_model:proposal_model['_byId']={d['id']:d for d in donors}
    known=proposal_model['_byId'].get(reference.get('id'));rf=known['features'] if known else features(reference)
    cache=proposal_model.setdefault('_conditionalCache',{});cache_key=(pos,rf['parts'],rf['sliders'],rf['anchors'])
    if cache_key not in cache:cache[cache_key]=sorted(donors,key=lambda d:abs(d['features']['parts']-rf['parts'])+abs(d['features']['sliders']-rf['sliders'])*.7+abs(d['features']['anchors']-rf['anchors'])*.5+abs(d['position']-pos)*.015)[:18]
    near=cache[cache_key]
    donor=rng.choice(near);f=donor['features']
    # Early source tutorials have a tiny equivalence space. Use a declared 3-5
    # part tutorial family instead of cosmetically duplicating a 2-ring source.
    n=max(3,min(96,round(.75*rf['parts']+.25*f['parts']+rng.choice([-2,-1,0,1,2]))))
    if pos<=3:n=max(3,min(5,n))
    width=max(2,min(7,round(math.sqrt(n)*rng.uniform(.72,1.05))))
    cells={(0,0)}
    while len(cells)<n:
        frontier=list({(q+dq,r+dr) for q,r in cells for dq,dr in [(1,0),(-1,0),(0,1),(0,-1),(1,-1),(-1,1)]}-cells)
        # Fitted density/aspect controls compactness without copying coordinates.
        def cost(c):return (c[0]+c[1]*.5)**2/(width**2)+(c[1]*.866)**2/max(2,n/width)**2
        frontier.sort(key=cost);c=rng.choice(frontier[:max(3,len(frontier)//3)])
        cells.add(c)
    coords=sorted(cells,key=lambda c:(c[0]+c[1],c[1],c[0]));index={c:i for i,c in enumerate(coords)}
    rotation=rng.uniform(0,360);theta=math.radians(rotation);scale=rng.uniform(2.55,2.85)
    parts=[]
    gaps=donor['gaps'] or [90];ratios=donor['widthRatios'] or [.28]
    for i,(q,r) in enumerate(coords):
        xx=(q+r*.5)*scale;yy=r*.866025403784*scale
        x=xx*math.cos(theta)-yy*math.sin(theta);y=xx*math.sin(theta)+yy*math.cos(theta)
        parts.append({'id':i,'kind':'ring','x':round(x,5),'y':round(y,5),'radius':1.,
            'width':round(max(.35,min(.62,rng.choice(ratios))),4),'gap':90.,
            'angle':rng.uniform(0,360),'color':rng.randrange(6),'collectible':True,'key':0,'lockKey':0,'bomb':False,'axis':0.,'length':2.})
    edge_prob=max(.35,min(.9,rf['links']/max(1,n*1.35)))
    links=[]
    for c,i in index.items():
        for step in [(1,0),(0,1)]:
            target=(c[0]+step[0],c[1]+step[1])
            if target in index and rng.random()<edge_prob:
                j=index[target];links.append({'source':i,'target':j,'bearing':xyangle(parts[i],parts[j]),'pinWidth':.22})
    # Keep nontrivial silhouettes: every isolated ring receives a neighboring pin
    # where orientation permits; disconnected components remain a learned motif.
    involved={e[k] for e in links for k in ('source','target')}
    for c,i in index.items():
        if i in involved:continue
        choices=[]
        for d in [(1,0),(0,1),(-1,0),(0,-1)]:
            t=(c[0]+d[0],c[1]+d[1])
            if t in index:
                j=index[t];a,b=(i,j) if d in [(1,0),(0,1)] else (j,i)
                choices.append((a,b))
        if choices:
            a,b=rng.choice(choices);links.append({'source':a,'target':b,'bearing':xyangle(parts[a],parts[b]),'pinWidth':.22});involved.update([a,b])
    g=nx.DiGraph();g.add_nodes_from(range(n));g.add_edges_from((e['source'],e['target']) for e in links)
    # Ring/pull objects are both releasable. CanCollect from the original controls
    # score collection, not removal eligibility. Sliders can also hold relations.
    desired_sliders=round(.75*rf['sliders']+.25*f['sliders'])
    slider_choices=list(g);rng.shuffle(slider_choices)
    for v in slider_choices[:min(desired_sliders,n)]:
        parts[v].update(kind='slider',gap=0.,axis=(rotation+rng.choice([0,60,120,180,240,300]))%360,length=rng.uniform(2.,3.3))
    desired_anchors=round(.85*rf['anchors']+.15*f['anchors'])
    # An anchor can be a target: turning its gap opens the pin but the anchor
    # stays on the board. Restricting anchors to graph roots incorrectly erased
    # a large part of the source game's noncollectible-ring mechanic.
    anchor_choices=[v for v in g if parts[v]['kind']=='ring'];rng.shuffle(anchor_choices)
    for v in anchor_choices[:min(desired_anchors,max(0,n-2))]:parts[v]['collectible']=False
    for p in parts:
        incoming=[e['bearing'] for e in links if e['target']==p['id']]
        # Two incoming pins separated 60 degrees require >=80deg with clearance.
        if p['kind']=='ring':
            sampled=rng.choice(gaps)
            p['gap']=max(82.,min(115.,sampled)) if len(incoming)>1 else max(60.,min(115.,sampled))
            if not incoming and rng.random()<rf['closed']/max(1,rf['rings']):p['gap']=0.
            # No accidental auto-release at initial state: put gap away from pins.
            if incoming:p['angle']=(incoming[0]+rng.uniform(65,260))%360
    # Add sparse cross-branch key gating, verified by solver before acceptance.
    if pos>=24 and rng.random()<min(.45, .08+pos/850):
        collect=[p for p in parts if p['collectible']];keypart=rng.choice([p for p in collect if g.out_degree(p['id'])==0] or collect)
        lockparts=[p for p in collect if p['id']!=keypart['id'] and not nx.has_path(g,keypart['id'],p['id'])]
        if lockparts:keypart['key']=1;rng.choice(lockparts)['lockKey']=1
    cx=statistics.mean(p['x'] for p in parts);cy=statistics.mean(p['y'] for p in parts)
    for p in parts:p['x']=round(p['x']-cx,5);p['y']=round(p['y']-cy,5);p['angle']=round(p['angle'],5)
    return {'id':'candidate','group':'fresh','position':pos,'state':{'parts':parts,'links':links},'proposalDonor':donor['id']}

def default_settings():
    return {'policies':[{'strategy':'least_turn'},{'strategy':'largest_turn'}],
        'acceptance':{'nearCopyDistance':.10,'sourceCountMinimum':500,'requestedCount':300,
            'maxAttemptsPerLevel':240,'countRelativeTolerance':.30,'computerScoreRelativeTolerance':.35,
            'tutorialPolicy':'positions1..3:3..5 parts, limited cosmetic-equivalence space',
            'unsupportedMechanics':['native butterfly slots','explosive contact','arbitrary continuous native collision']}}
