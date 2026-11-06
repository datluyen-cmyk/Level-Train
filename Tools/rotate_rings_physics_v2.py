"""Native-template conservative physics adapter. No v1 angle-only transitions."""
from __future__ import annotations
import copy,hashlib,json,math,random,statistics,collections,time
from shapely.geometry import Polygon
from shapely import affinity,union_all,make_valid,dwithin
import networkx as nx

RULE_VERSION='native-template-polygon-sweep-2.1-mirror'
MAX_SWEEP_DEG=.5
AREA_EPS=1e-7
POLYGON_SKIN=.01
PENETRATION_TOLERANCE=.0005
CONTACT_DISTANCE=2*POLYGON_SKIN-PENETRATION_TOLERANCE

def default_settings():return {'policies':[{'strategy':'least_turn'}],'acceptance':{'requestedCount':300,'sourceCountMinimum':500,'maxAttemptsPerLevel':250,'maximumParts':84,'maximumSweepStepDegrees':.5,'initialHardOverlapAllowed':False,'templateInventoryMustMatch':True,'canonicalDuplicatesAllowed':False,'nativeUnmodeledMechanics':'reject before labeling'}}
def validate_level(l):
    ps=state(l)['parts'];errors=[]
    if not ps or len({p['id'] for p in ps})!=len(ps):errors.append('empty or duplicate part IDs')
    if any(not math.isfinite(float(p[k])) for p in ps for k in ['x','y','angle','radius','width']):errors.append('nonfinite geometry')
    return errors

def state(l):return l.get('state',l)
def delta(a,b):return (a-b+180)%360-180
def shape_polygon(s):
    paths=[[(v['x'],v['y']) for v in path['points']] for path in s.get('paths',[{'points':s['points']}])]
    return make_valid(Polygon(paths[0],paths[1:]))

_local_cache={}
def local_shapes(p):
    key=p['templateId']
    if key not in _local_cache:
        shapes=[(s,shape_polygon(s)) for s in p['physicalShapes']]
        hard=union_all([g for s,g in shapes if not s['trigger']])
        body=union_all([g for s,g in shapes if s['role']=='body'])
        heads={s['attachmentId']:g for s,g in shapes if s['role']=='head_trigger'}
        extras=union_all([g for s,g in shapes if not s['trigger'] and s['role']!='body'])
        _local_cache[key]=(hard,body,heads,extras)
    return _local_cache[key]
def transform(g,p,a):
    rad=math.radians(a);c,s=math.cos(rad),math.sin(rad)
    return affinity.affine_transform(g,[c,-s,s,c,p['x'],p['y']])
def world_shapes(p,a):
    h,b,t,extras=local_shapes(p);return transform(h,p,a),transform(b,p,a),{i:transform(g,p,a) for i,g in t.items()},transform(extras,p,a)

def new_session(l):
    ps=state(l)['parts'];return {'angles':{p['id']:p['angle'] for p in ps},'removed':set(),'keys':set()}
def geometry(l,session):return {p['id']:world_shapes(p,session['angles'][p['id']]) for p in state(l)['parts'] if p['id'] not in session['removed']}
def intersects(a,b):
    # Unity PolygonCollider2D.Distance includes .01 Box2D radius per polygon.
    # Live parity: raw gap .005447619155 -> Unity distance -.01455232.
    return not a.is_empty and not b.is_empty and bool(dwithin(a,b,CONTACT_DISTANCE-1e-10))
def contacts(l,session,geoms=None):
    geoms=geoms or geometry(l,session);result=[];allowed=binding_allowlist(l)
    for pid,(_,_,heads,_) in geoms.items():
        for aid,head in heads.items():
            for target,(_,body,_,_) in geoms.items():
                if target!=pid and (pid,aid,target) in allowed and intersects(head,body):result.append((pid,aid,target))
    return result
def hard_collisions(l,session):
    geoms=geometry(l,session);ids=list(geoms);out=[]
    for ix,i in enumerate(ids):
        for j in ids[ix+1:]:
            if intersects(geoms[i][0],geoms[j][0]):out.append((i,j,geoms[i][0].intersection(geoms[j][0]).area))
    return out
def binding_allowlist(l):return {(e['source'],e.get('attachmentId',-1),e['target']) for e in state(l).get('links',[])}
def unbound_contact(l,pid,moving,others):
    allowed=binding_allowlist(l);mh,mb,mt,mextras=moving
    for other,(oh,ob,ot,oextras) in others.items():
        if other==pid:continue
        for aid,head in mt.items():
            obstacle=oextras if (pid,aid,other) in allowed else oh
            if intersects(head,obstacle):return True
            if any(intersects(head,h) for h in ot.values()):return True
        for aid,head in ot.items():
            obstacle=mextras if (other,aid,pid) in allowed else mh
            if intersects(head,obstacle):return True
    return False
def inventory_errors(l):
    errors=[]
    for p in state(l)['parts']:
        if p['kind']!='ring':errors.append('unsupported slider/fixture')
        if not p.get('physicalShapes') or not p.get('templateId'):errors.append('missing native template geometry')
        for a in p.get('attachments',[]):
            if a.get('isInnerMount'):errors.append('inner mount excluded from this source-constrained batch')
            if a['type']!=1:errors.append('headed bar excluded from this batch')
        heads={s['attachmentId'] for s in p.get('physicalShapes',[]) if s['role']=='head_trigger'}
        if heads!={a['id'] for a in p.get('attachments',[])}:errors.append('attachment/head inventory mismatch')
    return sorted(set(errors))
def settle(l,session):
    for _ in state(l)['parts']:
        cs=contacts(l,session);incident={i for a,_,b in cs for i in (a,b)}
        gone=[p['id'] for p in state(l)['parts'] if p['id'] not in session['removed'] and p['id'] not in incident]
        if not gone:break
        session['removed'].update(gone)
def won(l,s):return len(s['removed'])==len(state(l)['parts'])
def apply_action(l,session,action):
    pid=action['part'];ps={p['id']:p for p in state(l)['parts']}
    if action.get('kind')!='rotate' or pid not in ps or pid in session['removed']:return {'valid':False,'reason':'invalid rotate target'}
    g=geometry(l,session)
    if any(a==pid for a,_,_ in contacts(l,session,g)):return {'valid':False,'reason':'active outgoing head binding'}
    start=session['angles'][pid];travel=float(action.get('delta',delta(float(action['angle']),start)))
    if abs(travel)<.1:return {'valid':False,'reason':'no angular motion'}
    count=max(1,math.ceil(abs(travel)/MAX_SWEEP_DEG));static=[v[0] for i,v in g.items() if i!=pid]
    obstacles=union_all(static) if static else Polygon()
    for step in range(1,count+1):
        a=start+travel*step/count;moving=world_shapes(ps[pid],a)
        if intersects(moving[0],obstacles):return {'valid':False,'reason':'solid swept collision','blockedAt':a}
        if unbound_contact(l,pid,moving,g):return {'valid':False,'reason':'new unbound or crossing pin contact','blockedAt':a}
    session['angles'][pid]=(start+travel)%360;settle(l,session)
    return {'valid':True,'removed':pid in session['removed']}
def candidates(l,s):
    ps={p['id']:p for p in state(l)['parts']};g=geometry(l,s);cs=contacts(l,s,g);result=[]
    for pid,p in ps.items():
        if pid in s['removed'] or any(a==pid for a,_,_ in cs):continue
        incoming=[(a,aid) for a,aid,b in cs if b==pid]
        targets=[]
        for src,aid in incoming:
            centre=g[src][2][aid].centroid
            targets.append(math.degrees(math.atan2(centre.y-p['y'],centre.x-p['x'])))
        if not targets:targets=[s['angles'][pid]+15]
        for a in targets:
            for offset in [0,-5,5]:
                endpoint=(a+offset)%360;short=delta(endpoint,s['angles'][pid])
                if abs(short)<.1:continue
                result.append({'part':pid,'kind':'rotate','angle':endpoint,'delta':short})
                result.append({'part':pid,'kind':'rotate','angle':endpoint,'delta':short-360*math.copysign(1,short)})
    return result
def solve(l,policy=None):
    if '_constructionSolution' in l:
        path=l['_constructionSolution']
        return {'solved':True,'solution':path,'cost':len(path),'effort':l.get('_constructionEffort',len(path)),'angularTravel':sum(abs(a['delta']) for a in path),'method':'reverse construction; independent replay required'}
    policy=policy or {};s=new_session(l);path=[];effort=0;travel=0;seen=set();started=time.monotonic()
    initial=hard_collisions(l,s)
    if initial:return {'solved':False,'solution':[],'cost':0,'effort':0,'reason':'initial solid overlap','collisions':initial[:8]}
    initial_geoms=geometry(l,s)
    if any(unbound_contact(l,i,g,initial_geoms) for i,g in initial_geoms.items()):return {'solved':False,'solution':[],'cost':0,'effort':0,'reason':'initial crossing pin or unbound contact'}
    for _ in range(len(state(l)['parts'])*3+5):
        if won(l,s):return {'solved':True,'solution':path,'cost':len(path),'effort':effort,'angularTravel':travel}
        key=(tuple(sorted(s['removed'])),tuple(round(s['angles'][p['id']],3) for p in state(l)['parts']))
        if key in seen:break
        seen.add(key);ranked=[]
        for a in candidates(l,s):
            if effort>=policy.get('maxEffort',1000000) or time.monotonic()-started>policy.get('maxSeconds',float('inf')):
                return {'solved':False,'solution':path,'cost':len(path),'effort':effort,'reason':'declared search budget exhausted; solvability inconclusive'}
            nxt=copy.deepcopy(s);evt=apply_action(l,nxt,a);effort+=1
            if not evt['valid']:continue
            benefit=len(nxt['removed'])-len(s['removed']);ranked.append((benefit*100-abs(a['delta'])/360,a,nxt))
            if benefit>0 and policy.get('strategy','least_turn')=='least_turn':break
        if not ranked:break
        _,a,s=max(ranked,key=lambda x:x[0]);path.append(a);travel+=abs(a['delta'])
    return {'solved':False,'solution':path,'cost':len(path),'effort':effort,'reason':'bounded physical search stalled; not native unsolvability proof'}
def replay(l,solution):
    # Separately performs physical sampling; never calls solve/apply_action.
    ps={p['id']:p for p in state(l)['parts']};s=new_session(l)
    if hard_collisions(l,s):return False
    initial=geometry(l,s)
    if any(unbound_contact(l,i,g,initial) for i,g in initial.items()):return False
    for action in solution:
        pid=action['part']
        if pid in s['removed'] or pid not in ps:return False
        g=geometry(l,s)
        if any(a==pid for a,_,_ in contacts(l,s,g)):return False
        start=s['angles'][pid];turn=action.get('delta',delta(action['angle'],start));steps=max(1,math.ceil(abs(turn)/.5))
        for k in range(1,steps+1):
            moving=world_shapes(ps[pid],start+turn*k/steps);moved=moving[0]
            for other,data in g.items():
                if other!=pid and intersects(moved,data[0]):return False
            if unbound_contact(l,pid,moving,g):return False
        s['angles'][pid]=(start+turn)%360;settle(l,s)
    return won(l,s)
def features(l):
    ps=state(l)['parts'];edges=state(l).get('links',[]);g=nx.DiGraph();g.add_nodes_from(p['id'] for p in ps);g.add_edges_from((e['source'],e['target']) for e in edges)
    return {'parts':len(ps),'links':len(edges),'pins':sum(len(p.get('attachments',[])) for p in ps),'branching':sum(g.out_degree(i)>1 for i in g),'depth':nx.dag_longest_path_length(g) if nx.is_directed_acyclic_graph(g) else len(ps),'meanGap':statistics.mean(p['gap'] for p in ps),'meanRadius':statistics.mean(p['radius'] for p in ps),'components':nx.number_weakly_connected_components(g),'largestComponent':max(map(len,nx.weakly_connected_components(g)))}
def fingerprint(l):
    ps=state(l)['parts'];g=nx.Graph()
    for p in ps:
        normal=tuple(sorted((a['type'],round(a['angleDeg']%360,1)%360,round(a['stemLength'],3),a['isInnerMount']) for a in p.get('attachments',[])))
        mirrored=tuple(sorted((a['type'],round((-a['angleDeg'])%360,1)%360,round(a['stemLength'],3),a['isInnerMount']) for a in p.get('attachments',[])))
        attachments=min(normal,mirrored)
        role=(round(p['radius'],3),round(p['width'],3),round(p['gap'],2),attachments)
        g.add_node(p['id'],role=str(role))
    for e in state(l).get('links',[]):g.add_edge(e['source'],e['target'])
    return nx.weisfeiler_lehman_graph_hash(g,node_attr='role',iterations=5)
def descriptor(l):
    ps=state(l)['parts'];g=nx.DiGraph();g.add_nodes_from(p['id'] for p in ps);g.add_edges_from((e['source'],e['target']) for e in state(l).get('links',[]))
    out=collections.Counter()
    for p in ps:
        normal=tuple(sorted((a['type'],round((a['angleDeg']%360)/15)%24,round(a['stemLength']/.1),a['isInnerMount']) for a in p.get('attachments',[])))
        mirrored=tuple(sorted((a['type'],round(((-a['angleDeg'])%360)/15)%24,round(a['stemLength']/.1),a['isInnerMount']) for a in p.get('attachments',[])))
        pins=min(normal,mirrored)
        out[('piece',round(p['radius'],2),round(p['gap']),pins,g.in_degree(p['id']),g.out_degree(p['id']))]+=1
    for e in state(l).get('links',[]):out[('edge',g.in_degree(e['source']),g.out_degree(e['source']),g.in_degree(e['target']),g.out_degree(e['target']))]+=1
    return out
def novelty_distance(a,b):
    if fingerprint(a)==fingerprint(b):return 0.
    x=descriptor(a);y=descriptor(b);keys=x.keys()|y.keys()
    return 1-sum(min(x[k],y[k]) for k in keys)/max(1,sum(max(x[k],y[k]) for k in keys))
def difficulty(l,result):return result['cost']+result.get('angularTravel',0)/180+.65*features(l)['depth']+.35*features(l)['branching']

def _build_component(templates,n,rng,settings=None):
    settings=settings or {}
    """Backward-release construction using unchanged source pieces and exact sweeps."""
    leaves=[t for t in templates if not t['attachments']];branches=[t for t in templates if t['attachments']]
    root=copy.deepcopy(rng.choice([t for t in branches if len(t['attachments'])<=min(3,n-1)]))
    root.update(id=0,templateId=root.pop('id'),x=0.,y=0.,angle=rng.uniform(0,360),color=rng.randrange(6),collectible=True)
    parts=[root];ports=[(0,a['id']) for a in root['attachments']];links=[];plans={};placement_effort=0;history=[];backtracks=0
    while ports:
        # Finish spatially distant ports first; retain all physical heads as obstacles.
        saved_ports=list(ports);ownerid,aid=ports.pop(rng.randrange(len(ports)));owner=parts[ownerid];remaining=n-len(parts)
        if remaining<1:return None,'inventory exceeds requested scale'
        maximum=max(0,min(3,remaining-1-len(ports)))
        minimum=1 if not ports and remaining>1 else 0
        pool=[t for t in templates if minimum<=len(t['attachments'])<=maximum]
        if not pool:return None,'whole-template port inventory cannot reach requested scale'
        att=next(a for a in owner['attachments'] if a['id']==aid);ang=owner['angle']+att['angleDeg'];rad=math.radians(ang);ux,uy=math.cos(rad),math.sin(rad)
        direction=math.radians(att['angleDeg']);vx,vy=math.cos(direction),math.sin(direction)
        local_head=local_shapes(owner)[2][aid];extent=local_head.centroid.x*vx+local_head.centroid.y*vy
        placed=None;prior_geoms=geometry({'state':{'parts':parts,'links':links}},new_session({'state':{'parts':parts,'links':links}}))
        # Near-gap bound poses shorten the guaranteed release sweep. Both signs
        # and many whole templates remain available, including multi-pin pieces.
        for attempt in range(60):
            counts=sorted({len(t['attachments']) for t in pool});br=settings.get('branchRate',.15);weights=[(.25 if k==0 else 1. if k==1 else max(.05,br*4/k)) for k in counts];k=rng.choices(counts,weights)[0]
            choices=[t for t in pool if len(t['attachments'])==k];freq=settings.get('templateFrequencies',{});t=rng.choices(choices,[freq.get(t['id'],1) for t in choices])[0];distance=extent+t['radius'];endpoint=(ang+180)%360
            turns=settings.get('turnSamples',[90,120,150,175]);magnitude=min(175.,max(40.,rng.choice(turns)+rng.uniform(-15,15)));offset=rng.choice([-1,1])*magnitude
            child=copy.deepcopy(t);child.update(id=len(parts),templateId=child.pop('id'),x=owner['x']+ux*distance,y=owner['y']+uy*distance,angle=(endpoint+offset)%360,color=rng.randrange(6),collectible=True)
            temp={'state':{'parts':parts+[child],'links':links+[{'source':ownerid,'target':child['id'],'attachmentId':aid}]}}
            session=new_session(temp);moving=world_shapes(child,child['angle']);geoms={**prior_geoms,child['id']:moving}
            if any(intersects(moving[0],data[0]) for i,data in geoms.items() if i!=child['id']):continue
            if unbound_contact(temp,child['id'],moving,geoms):continue
            if (ownerid,aid,child['id']) not in contacts(temp,session,geoms):continue
            action={'part':child['id'],'kind':'rotate','angle':endpoint,'delta':-offset}
            placement_effort+=1;evt=apply_action(temp,session,action)
            if not evt['valid'] or child['id'] not in session['removed']:continue
            placed=child;plans[child['id']]=action;break
        if placed is None:
            if not history or backtracks>=n:return None,'constructive backtracking budget exhausted'
            backtracks+=1;ports=history.pop();old=parts.pop();plans.pop(old['id'],None);links.pop();continue
        history.append(saved_ports)
        parts.append(placed);links.append({'source':ownerid,'target':placed['id'],'attachmentId':aid})
        ports.extend((placed['id'],a['id']) for a in placed['attachments'])
    if len(parts)!=n:return None,'whole-template port count failed'
    cx=statistics.mean(p['x'] for p in parts);cy=statistics.mean(p['y'] for p in parts)
    for p in parts:p['x']-=cx;p['y']-=cy
    l={'id':'candidate','state':{'parts':parts,'links':links}};session=new_session(l);solution=[]
    if hard_collisions(l,session):return None,'initial solid collision'
    g=geometry(l,session)
    if any(unbound_contact(l,i,geom,g) for i,geom in g.items()):return None,'initial crossing pin or unbound contact'
    for pid in reversed(range(1,n)):
        if pid in session['removed']:continue
        action=plans[pid];evt=apply_action(l,session,action)
        if not evt['valid']:return None,'full reverse construction sweep failed'
        solution.append(action)
    if not won(l,session):return None,'reverse construction not complete'
    l['_constructionSolution']=solution;l['_constructionEffort']=placement_effort
    return l,None

def build_candidate(templates,n,rng,component_target=None,settings=None):
    # Source graphs contain multiple independent clusters. Preserve total scale,
    # while sampling fresh cluster topology and whole-piece poses.
    component_target=component_target or max(1,round(n/rng.uniform(4,8)))
    count=max(1,min(n//2,round(component_target)))
    # The supported subset retains meaningful interlocked groups; no isolated padding.
    count=max(count,math.ceil(n/18));sizes=[2]*count
    for _ in range(n-2*count):
        choices=[i for i,k in enumerate(sizes) if k<18];sizes[rng.choice(choices)]+=1
    if any(k<2 for k in sizes):return None,'component inventory too small'
    parts=[];links=[];plan=[];effort=0;cursor_x=0.;cursor_y=0.;row_height=0.;shelf_width=max(20.,math.sqrt(n)*6.5)
    for index,size in enumerate(sizes):
        comp=None
        for trial in range(16):
            comp,error=_build_component(templates,size,rng,settings)
            if comp:break
        if comp is None:return None,'component construction budget exhausted'
        cps=comp['state']['parts'];offset=len(parts)
        # Bounding-box grid spacing leaves audited clearance between clusters.
        bounds=union_all([world_shapes(p,p['angle'])[0] for p in cps]).bounds
        bw= bounds[2]-bounds[0]+2.;bh=bounds[3]-bounds[1]+2.
        if cursor_x and cursor_x+bw>shelf_width:cursor_y+=row_height;cursor_x=0.;row_height=0.
        xshift=cursor_x-bounds[0]+1.;yshift=cursor_y-bounds[1]+1.;cursor_x+=bw;row_height=max(row_height,bh)
        for p in cps:
            p['id']+=offset;p['x']+=xshift;p['y']+=yshift
        for e in comp['state']['links']:e['source']+=offset;e['target']+=offset
        for a in comp['_constructionSolution']:a['part']+=offset
        parts.extend(cps);links.extend(comp['state']['links']);plan.extend(comp['_constructionSolution']);effort+=comp['_constructionEffort']
    cx=statistics.mean(p['x'] for p in parts);cy=statistics.mean(p['y'] for p in parts)
    for p in parts:p['x']-=cx;p['y']-=cy
    return {'id':'candidate','state':{'parts':parts,'links':links},'_constructionSolution':plan,'_constructionEffort':effort},None
