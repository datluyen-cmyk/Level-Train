"""Source-derived Pull contract engine, separated from the collision oracle.

No generated levels or source solutions are certified by this module yet. Native
Unity query replay is the required oracle for parity; the Shapely oracle is an
explicit exploratory geometry tool and cannot create accepted training labels.
Every collider retains its source identity. No scene or component cap is used.
"""
from __future__ import annotations
from dataclasses import dataclass,field
from pathlib import Path
from typing import Protocol
import argparse,copy,hashlib,json,math

RULE_VERSION='source-native-pull-contract-v3.0-unverified'
SKIN=.01
FRAME_MOVE=.36
SUBSTEP=.12
OVERLAP_AREA_LAYER_MASK=128 # Original overlap-area GameObject layer7.

def dot(a,b):return a[0]*b[0]+a[1]*b[1]
def add(a,b,k=1.):return (a[0]+b[0]*k,a[1]+b[1]*k)
def rotate(v,a):
 r=math.radians(a);c,s=math.cos(r),math.sin(r);return (c*v[0]-s*v[1],s*v[0]+c*v[1])
def pull_axis(movement,angle):
 """GetLockedDragDirection: VectorUp, VectorLeft, then legacy flags."""
 t=movement['lockDirectionType']
 if t==0:return rotate((0.,1.),angle)
 if t==1:return rotate((-1.,0.),angle)
 left,right,up,down=(bool(movement.get(k)) for k in ['lockLeft','lockRight','lockUp','lockDown'])
 horizontal_locked=left and right;vertical_locked=up and down
 if not horizontal_locked and vertical_locked:
  if not right:return (1.,0.)
  if not left:return (-1.,0.)
 if horizontal_locked and not vertical_locked:
  if not up:return (0.,1.)
  if not down:return (0.,-1.)
 return (0.,1.)

@dataclass(frozen=True)
class ColliderRef:
 owner:int
 index:int
 source_id:int

@dataclass(frozen=True)
class CastHit:
 collider:ColliderRef
 distance:float

@dataclass(frozen=True)
class Distance:
 distance:float
 normal:tuple[float,float]
 valid:bool=True

@dataclass(frozen=True)
class Binding:
 collider:ColliderRef
 partner:int
 guide_id:str|None=None

@dataclass
class Session:
 poses:dict[int,dict]
 removed:set[int]=field(default_factory=set)
 active:dict[int,dict[ColliderRef,Binding]]=field(default_factory=dict)
 configured:dict[int,dict[ColliderRef,Binding]]=field(default_factory=dict)
 trace:list=field(default_factory=list)
 preserve_exit:set[int]=field(default_factory=set)

class CollisionOracle(Protocol):
 certification:str
 def overlaps(self,scene,session,owner:int)->set[ColliderRef]:...
 def cast(self,scene,session,owner:int,direction:tuple,distance:float)->list[CastHit]:...
 def distance(self,scene,session,owner:int,other:ColliderRef)->Distance:...
 def release_settlement(self,scene,session,owner:int)->None:...

class Scene:
 def __init__(self,level,templates):
  self.level=level;self.parts={p['id']:p for p in level['parts']};self.templates=templates
  self.by_source={p['sourceId']:p['id'] for p in level['parts']};self.shapes={};self.roles={};self.bindings={};self.guide_owner={}
  for p in self.parts.values():
   t=self.template(p['id']);self.shapes[p['id']]=t['physicalShapes']
   for b in p['sourceColliderBindings']:
    ref=ColliderRef(p['id'],b['shapeIndex'],b['sourceColliderId']);self.roles[ref]=b['sourceRole'];self.bindings[(p['id'],b['shapeIndex'])]=ref
  for e in level.get('guideLinks',[]):self.guide_owner[e['guidePartId']]=e['guideOwnerId']
 def template(self,pid):return self.templates[self.parts[pid]['templateId']]
 def body_ref(self,pid):return next(self.bindings[(pid,i)] for i,s in enumerate(self.shapes[pid]) if s['role']=='body')
 def shape(self,ref):return self.shapes[ref.owner][ref.index]
 def new_session(self,oracle):
  s=Session({p['id']:{k:p[k] for k in ['x','y','angle']} for p in self.parts.values()})
  for p in self.parts.values():
   if p['kind']=='slider':s.configured[p['id']]={};s.active[p['id']]={}
  # Pull<-Ring passive bindings reference only native RingRelative blocks,
  # never the hard pin stem or an unrelated collider of the same ring.
  for e in self.level.get('links',[]):
   a,b=self.parts[e['source']],self.parts[e['target']]
   if a['kind']=='ring' and b['kind']=='slider':owner,partner,side=b['id'],a['id'],'source'
   elif a['kind']=='slider' and b['kind']=='ring':owner,partner,side=a['id'],b['id'],'target'
   else:continue
   for index in e[side+'ShapeIndices']:
    if self.shapes[partner][index]['role'] not in ['head_trigger','head_wall']:continue
    ref=self.bindings[(partner,index)];s.configured[owner][ref]=Binding(ref,partner)
  for e in self.level.get('guideLinks',[]):
   owner=e['lockedOwnerId'];partner=e['guideOwnerId']
   if self.parts[owner]['kind']!='slider':continue
   for index in e['guideShapeIndices']:
    ref=self.bindings[(partner,index)];s.configured[owner][ref]=Binding(ref,partner,e['guidePartId'])
  # Native CloneToActive filters all static bindings by overlapped partner
  # part IDs, not by exact partner collider at this initialization stage.
  for owner,configured in s.configured.items():
   partner_ids={ref.owner for ref in oracle.overlaps(self,s,owner)}
   s.active[owner]={ref:b for ref,b in configured.items() if b.partner in partner_ids}
  return s

def clean_pull_bindings(scene,s,oracle,pid,activate=True):
 overlaps=oracle.overlaps(scene,s,pid);partners={r.owner for r in overlaps};active=s.active[pid]
 # Trigger/contact enter reactivates configured exact collider bindings.
 if activate:
  for ref in overlaps:
   if ref in s.configured[pid] and ref.owner not in s.removed:active[ref]=s.configured[pid][ref]
 if pid in s.preserve_exit:return
 for ref,b in list(active.items()):
  if b.partner in s.removed or (ref not in overlaps if b.guide_id is not None else b.partner not in partners):del active[ref]

def cast_hit_allowed(scene,s,pid,ref):
 if ref.owner==pid or ref.owner in s.removed:return False
 shape=scene.shape(ref)
 if not shape.get('sourceEnabled',True):return False
 if not ((1<<shape.get('sourceLayer',7))&OVERLAP_AREA_LAYER_MASK):return False
 if shape['trigger']:return False # original castContactFilter.useTriggers=false
 if scene.roles[ref]=='CatchGuideBody':return False
 return ref not in s.active[pid]

def allowed_hits(scene,s,oracle,pid,direction,distance):
 return sorted((h for h in oracle.cast(scene,s,pid,direction,distance) if h.distance<distance and cast_hit_allowed(scene,s,pid,h.collider)),key=lambda h:h.distance)

def _move_one_step(scene,s,oracle,pid,direction,step):
 raw_hits=oracle.cast(scene,s,pid,direction,step)
 hits=sorted((h for h in raw_hits if h.distance<step and cast_hit_allowed(scene,s,pid,h.collider)),key=lambda h:h.distance)
 nearest=hits[0] if hits else None;amount=step;blocked=False;escape=False
 # Source code subtracts skin whenever the raw cast returned any hits, even
 # when the allow filter rejected every one of those hits.
 if raw_hits and not nearest:amount=max(0.,step-SKIN)
 if nearest:
  if nearest.distance<=SKIN:
   d=oracle.distance(scene,s,pid,nearest.collider)
   if d.valid and d.distance<0 and dot(direction,d.normal)<0:
    amount=min(step,-d.distance+SKIN);escape=True
  if not escape:
   blocked=nearest.distance+SKIN<step;amount=max(0.,nearest.distance-SKIN)
 s.poses[pid]['x']+=direction[0]*amount;s.poses[pid]['y']+=direction[1]*amount
 clean_pull_bindings(scene,s,oracle,pid)
 evt={'event':'pullSubstep','owner':pid,'distance':amount,'requested':step,'blocked':blocked,'initialOverlapEscape':escape,'pose':dict(s.poses[pid]),'activeColliderIds':sorted(r.source_id for r in s.active[pid])}
 if nearest:evt['nearestAllowedColliderId']=nearest.collider.source_id;evt['nearestAllowedDistance']=nearest.distance
 s.trace.append(evt)
 # Native TryMoveOneStep returns zero after overlapEscape moved the body.
 return (0. if escape else amount),blocked,escape

def _rollback(scene,s,oracle,pid,axis,direction_sign,start):
 direction=(-axis[0]*direction_sign,-axis[1]*direction_sign)
 hits=allowed_hits(scene,s,oracle,pid,direction,.05+SKIN)
 nearest=hits[0].distance if hits else 0.
 travel=abs(dot((s.poses[pid]['x']-start[0],s.poses[pid]['y']-start[1]),axis))
 amount=min(max(0.,.05-nearest),travel)
 s.poses[pid]['x']+=direction[0]*amount;s.poses[pid]['y']+=direction[1]*amount
 s.trace.append({'event':'stopRollback','owner':pid,'distance':amount,'nearestReverseCastDistance':nearest})
 return amount

def apply_pull_gesture(scene,s,oracle,pid,axis_distance,frames=120,dt=1/60,release=True):
 if pid not in scene.parts or pid in s.removed or scene.parts[pid]['kind']!='slider':return {'valid':False,'reason':'not an active Pull'}
 if scene.parts[pid].get('chainLockCount',0):return {'valid':False,'reason':'chain input gating awaits native LockFeature state; scene retained'}
 if scene.level.get('bombs'):return {'valid':False,'reason':'bomb translation gate/contacts await parity; scene retained'}
 p=scene.parts[pid];axis=pull_axis(scene.template(pid)['movement'],s.poses[pid]['angle']);sign=math.copysign(1,axis_distance);direction=(axis[0]*sign,axis[1]*sign);start=(s.poses[pid]['x'],s.poses[pid]['y']);remaining=abs(axis_distance);blocked=False;moved=0.;threshold=min(p['sourceNativeData']['moveThreshold'],.03)
 if remaining<threshold:return {'valid':False,'reason':'below native effective move threshold'}
 for frame in range(frames):
  projected=dot((s.poses[pid]['x']-start[0],s.poses[pid]['y']-start[1]),direction);remaining=abs(axis_distance)-projected
  if blocked or remaining<threshold:break
  budget=min(remaining,FRAME_MOVE*dt*60)
  while budget>1e-5:
   step=min(SUBSTEP,budget);distance,blocked,escape=_move_one_step(scene,s,oracle,pid,direction,step)
   if distance<=1e-5:break
   moved+=distance;remaining-=distance;budget-=distance
   if blocked:break
  if blocked:break
 if release:
  if blocked:_rollback(scene,s,oracle,pid,axis,sign,start)
  # Original release resolves overlaps before checking HasActivePullBindings.
  # An oracle must implement this; silently skipping it would mislabel solves.
  oracle.release_settlement(scene,s,pid);clean_pull_bindings(scene,s,oracle,pid)
  if not s.active[pid]:s.removed.add(pid)
  s.trace.append({'event':'pullRelease','owner':pid,'solved':pid in s.removed,'pose':dict(s.poses[pid]),'activeColliderIds':sorted(r.source_id for r in s.active[pid])})
 moved=dot((s.poses[pid]['x']-start[0],s.poses[pid]['y']-start[1]),direction)
 return {'valid':True,'part':pid,'axisWorld':axis,'moved':moved,'blocked':blocked,'removed':pid in s.removed,'certification':oracle.certification,'sourceLabelsAllowed':False}

class NativeQueryReplay:
 """Replay exact Unity queries keyed by a hash of full poses and query args.

 Missing query data fails explicitly. This backend never substitutes a reduced
 polygon estimate when a native cast/distance/release response is unavailable.
 """
 certification='native-query-replay; representative runtime parity pending'
 def __init__(self,responses):self.responses=responses;self.requests={}
 def query(self,kind,scene,s,pid,**args):
  request={'kind':kind,'sourceLevelId':scene.level['sourceId'],'owner':pid,'bodySourceColliderId':scene.body_ref(pid).source_id,'poses':s.poses,'removed':sorted(s.removed),'activeBindings':{str(owner):sorted(r.source_id for r in active) for owner,active in s.active.items()},'preserveExitOwners':sorted(s.preserve_exit),'args':args};encoded=json.dumps(request,sort_keys=True,separators=(',',':'));key=hashlib.sha256(encoded.encode()).hexdigest();self.requests[key]=request
  if key not in self.responses:raise MissingNativeQuery(key,request)
  return self.responses[key]
 def overlaps(self,scene,s,pid):return {scene.bindings[(x['owner'],x['shapeIndex'])] for x in self.query('overlaps',scene,s,pid,layerMask=OVERLAP_AREA_LAYER_MASK,useTriggers=True)}
 def cast(self,scene,s,pid,direction,distance):return [CastHit(scene.bindings[(x['owner'],x['shapeIndex'])],x['distance']) for x in self.query('cast',scene,s,pid,direction=direction,distance=distance,layerMask=OVERLAP_AREA_LAYER_MASK,useTriggers=False)]
 def distance(self,scene,s,pid,other):
  d=self.query('distance',scene,s,pid,other={'owner':other.owner,'shapeIndex':other.index});return Distance(d['distance'],tuple(d['normal']),d['valid'])
 def release_settlement(self,scene,s,pid):s.poses[pid].update(self.query('releaseSettlement',scene,s,pid)['pose'])

class MissingNativeQuery(RuntimeError):
 def __init__(self,key,request):super().__init__('Missing exact native query '+key);self.key=key;self.request=request

class ExploratoryPolygonOracle:
 """Static overlap diagnostics only; not native simulation certification.

 Circle-vs-polygon distance uses analytic radius, not polygonized circles.
 Native polygon skin=.01 is prior measured evidence. Box skin is uncalibrated.
 Cast, penetration normal and release resolution require native responses.
 """
 certification='exploratory geometry only; no casts or solved labels'
 def __init__(self):self.cache={}
 def geometry(self,scene,s,ref):
  from shapely.geometry import Polygon,Point
  from shapely import affinity,make_valid
  shape=scene.shape(ref);p=s.poses[ref.owner];key=(scene.parts[ref.owner]['templateId'],ref.index)
  if shape.get('shapeType')=='circle':
   c=shape['center'];q=rotate((c['x'],c['y']),p['angle']);return Point(q[0]+p['x'],q[1]+p['y']),shape['radius']
  if key not in self.cache:
   paths=shape.get('paths',[{'points':shape.get('points',[])}]);self.cache[key]=make_valid(Polygon([(v['x'],v['y']) for v in paths[0]['points']],[[ (v['x'],v['y']) for v in path['points']] for path in paths[1:]]))
  g=affinity.rotate(self.cache[key],p['angle'],origin=(0,0));g=affinity.translate(g,p['x'],p['y']);return g,(.01 if shape.get('sourceColliderType')=='PolygonCollider2D' else 0.)
 def overlaps(self,scene,s,pid):
  from shapely import dwithin
  own=scene.body_ref(pid);g,skin=self.geometry(scene,s,own);result=set()
  for ref in scene.roles:
   if ref.owner==pid or ref.owner in s.removed:continue
   shape=scene.shape(ref)
   if not shape.get('sourceEnabled',True) or not ((1<<shape.get('sourceLayer',7))&OVERLAP_AREA_LAYER_MASK):continue
   other,r=self.geometry(scene,s,ref)
   if dwithin(g,other,max(0,skin+r)):result.add(ref)
  return result
 def cast(self,*args,**kwargs):raise RuntimeError('Exploratory overlap oracle has no certified native cast')
 def distance(self,*args,**kwargs):raise RuntimeError('Native penetration normal required for initial-overlap escape')
 def release_settlement(self,*args,**kwargs):raise RuntimeError('Native ResolveReleaseOverlap required before solve')

def inspect_pack(pack,templates):
 oracle=ExploratoryPolygonOracle();result=[]
 for level in pack['levels']:
  scene=Scene(level,templates);s=scene.new_session(oracle);records=[]
  for pid in s.active:
   records.append({'partId':pid,'sourceId':scene.parts[pid]['sourceId'],'axisWorld':pull_axis(scene.template(pid)['movement'],s.poses[pid]['angle']),'configuredColliderIds':sorted(r.source_id for r in s.configured[pid]),'exploratoryInitialActiveColliderIds':sorted(r.source_id for r in s.active[pid]),'signedProbeDistances':[-.12,.12,-1.,1.]})
  result.append({'sourceLevelId':level['sourceId'],'position':level['position'],'pulls':records})
 return {'ruleVersion':RULE_VERSION,'sourceLabelsAllowed':False,'parityStatus':'unverified','collisionStatus':oracle.certification,'levels':result}

def main():
 root=Path(__file__).resolve().parents[1];local=(root/'data/source500_native.json').exists();data=root/'data' if local else root/'Research/physics_v3'
 p=argparse.ArgumentParser();p.add_argument('--pack',type=Path,default=data/('source500_native.json' if local else 'source_parity_pack.json'));p.add_argument('--templates',type=Path,default=data/'source_templates.json');p.add_argument('--output',type=Path,default=data/'pull_initial_diagnostics.json');a=p.parse_args();pack=json.loads(a.pack.read_text());ts={t['id']:t for t in json.loads(a.templates.read_text())['templates']};report=inspect_pack(pack,ts);a.output.write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps({'levels':len(report['levels']),'pulls':sum(len(l['pulls']) for l in report['levels']),'sourceLabelsAllowed':False,'output':str(a.output)},indent=2))
if __name__=='__main__':main()
