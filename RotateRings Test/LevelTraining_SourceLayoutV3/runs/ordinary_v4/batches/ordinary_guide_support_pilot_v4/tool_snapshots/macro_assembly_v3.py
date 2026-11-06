"""Learn native macro tiles and compose genuinely new, unvalidated assemblies.

This is a structured proposal stage. No exported proposal is a playable/accepted
level until native seams, independent replay, novelty and difficulty pass.
"""
from pathlib import Path
from collections import Counter,defaultdict
import argparse,copy,json,math,sys,hashlib
import numpy as np
import networkx as nx
import joblib
from scipy.special import logsumexp
from sklearn.preprocessing import StandardScaler
from sklearn.neighbors import NearestNeighbors
from layout_learning_v3 import read,hashfile,frame,level_context,ConditionalPoseModel,SEED
from global_composition_v3 import graph_for,context,arrangement,Geometry,NAMES
from assembly_contact_audit_v3 import NativeGeometrySeamAudit
from composition_field_v3 import posed_field,frontier_centers,stroke_heading,native_frame
from assembly_integrity_audit_v3 import NativeAssemblyIntegrityAudit

KINDS=['ring','slider','fixed','butterfly']
_SOCKET_TEMPLATE_KEYS={}

def socket_clean(value):
 """Drop only provenance and RGB tint; preserve alpha and every dimension."""
 if isinstance(value,dict):
  result={}
  for k,v in value.items():
   if k in ['sourceColliderId','sourceComponentId','fixtureVisualSource','sourceName']:continue
   if k in ['sourceVertexColor','vertexColor']:result[k]={'a':v.get('a',1.)};continue
   result[k]=socket_clean(v)
  return result
 if isinstance(value,list):return [socket_clean(v) for v in value]
 return value

def socket_key(part,templates,level):
 tid=part['templateId']
 if tid not in _SOCKET_TEMPLATE_KEYS:_SOCKET_TEMPLATE_KEYS[tid]=hashlib.sha256(json.dumps(socket_clean({k:v for k,v in templates[tid].items() if k!='id'}),sort_keys=True,separators=(',',':')).encode()).hexdigest()
 input_identity=json.dumps(socket_clean(part['inputShapes']),sort_keys=True,separators=(',',':'));rb_identity=json.dumps(socket_clean(part['sourceRigidbody']),sort_keys=True,separators=(',',':'));return (_SOCKET_TEMPLATE_KEYS[tid],input_identity,rb_identity,part['chainLockCount'],part['collectible'],bomb_socket_key(level,part['id']))

def bomb_socket_key(level,part_id):
 return tuple(sorted(json.dumps(b['physicalShape'],sort_keys=True,separators=(',',':')) for b in level['bombs'] if b['ownerPartId']==part_id))

def pose_matrix(angle):
 r=math.radians(angle);return np.array([[math.cos(r),-math.sin(r)],[math.sin(r),math.cos(r)]])

def board_heading(level):
 """Stable whole-board major axis, choosing its upper half-plane sign."""
 xy=np.array([[p['x'],p['y']] for p in level['parts'] if p['kind'] in ['ring','slider']]);center=xy.mean(axis=0);cov=np.cov((xy-center).T) if len(xy)>1 else np.eye(2);values,axes=np.linalg.eigh(cov);axis=axes[:,int(np.argmax(values))]
 if axis[1]<-1e-7 or (abs(axis[1])<=1e-7 and axis[0]<0):axis=-axis
 return math.degrees(math.atan2(axis[1],axis[0]))

def tile_descriptor(parts,templates):
 counts=Counter(p['kind'] for p in parts);xy=np.array([[p['x'],p['y']] for p in parts]);extent=np.ptp(xy,axis=0);width=max(.01,float(np.median([templates[p['templateId']].get('width',0) for p in parts if p['kind'] in ['ring','slider']] or [.5])));angles=np.radians([p['angle'] for p in parts]);return [len(parts),*[counts[k] for k in KINDS],*sorted((extent/width).tolist()),float(np.mean(np.abs(np.cos(2*angles)))),float(np.mean(np.abs(np.sin(2*angles)))),sum(p['chainLockCount']>0 for p in parts)]

def macro_tiles(level,templates,composition_graph=None,retain_all_boundary=False,boundary_graph=None):
 """Source-native topology communities; spatial neighborhoods join free roots.

 Communities are an analysis representation, never a limit on generated part
 counts. Boundary roots permit exact whole-template overlap between tiles.
 """
 ps={p['id']:p for p in level['parts']};native_graph=graph_for(level).to_undirected();g=native_graph if composition_graph is None else composition_graph;unit=max(.01,float(np.median([templates[p['templateId']].get('width',0) for p in ps.values() if p['kind'] in ['ring','slider']])));weighted=nx.Graph();weighted.add_nodes_from(g)
 for a,b,data in g.edges(data=True):weighted.add_edge(a,b,weight=data.get('weight',4.))
 # Actual source nearest-neighbor organization is retained even when it has no
 # authored static pin relation. Such edges are never exported as gameplay pins.
 for a in ps:
  nearest=sorted((b for b in ps if b!=a),key=lambda b:(ps[b]['x']-ps[a]['x'])**2+(ps[b]['y']-ps[a]['y'])**2)[:2]
  for b in nearest:
   if not weighted.has_edge(a,b):weighted.add_edge(a,b,weight=1/(1+math.hypot(ps[a]['x']-ps[b]['x'],ps[a]['y']-ps[b]['y'])/unit))
 communities=list(nx.community.greedy_modularity_communities(weighted,weight='weight')) if weighted.number_of_edges() else [set(g)]
 tiles=[]
 for core in communities:
  selected=set(core);boundary_graph=g if boundary_graph is None else boundary_graph
  # One nearest native boundary socket per core root; preserves real branches.
  for a in core:
   outside=sorted((b for b in boundary_graph.neighbors(a) if b not in core),key=lambda b:(ps[b]['x']-ps[a]['x'])**2+(ps[b]['y']-ps[a]['y'])**2)
   selected.update(outside if retain_all_boundary else outside[:1])
  # A butterfly or fixed guide is atomic with every native guide binding. Never
  # discard a second butterfly slot to make a smaller/easier puzzle.
  changed=True
  while changed:
   old=len(selected)
   for e in level['guideLinks']:
    if e['guideOwnerId'] in selected or e['lockedOwnerId'] in selected:selected.update([e['guideOwnerId'],e['lockedOwnerId']])
   changed=old!=len(selected)
  selected=sorted(selected);owner=min(core,key=lambda a:sum(math.hypot(ps[a]['x']-ps[b]['x'],ps[a]['y']-ps[b]['y']) for b in core));anchor=ps[owner];center=np.mean([[ps[a]['x'],ps[a]['y']] for a in core],axis=0);angles=np.radians([ps[a]['angle'] for a in core]);heading=math.degrees(math.atan2(np.sin(angles).mean(),np.cos(angles).mean()))
  tiles.append({'sourceId':level['sourceId'],'position':level['position'],'core':sorted(core),'partIds':selected,'owner':owner,'center':center.tolist(),'heading':heading,'width':unit,'descriptor':tile_descriptor([ps[a] for a in selected],templates),'boundaryNativeEdges':[[a,b] for a,b in native_graph.edges if (a in core)!=(b in core)],'boundaryCompositionEvidenceEdges':[[a,b] for a,b in g.edges if (a in core)!=(b in core)],'sourceRelativePoses':[{'partId':a,'xy':frame(ps[a]['x']-anchor['x'],ps[a]['y']-anchor['y'],anchor['angle']),'angle':(ps[a]['angle']-anchor['angle'])%360} for a in selected]})
 return tiles

class MacroAssemblyModel:
 def fit(self,levels,templates,families,tile_builder=None):
  tile_builder=tile_builder or macro_tiles
  self.tiles=[];self.levels={l['sourceId']:l for l in levels};self.families=families;self.tile_contexts=[];relations=[]
  for level in levels:
   tiles=tile_builder(level,templates);base=len(self.tiles);self.tiles.extend(tiles)
   for tile in tiles:self.tile_contexts.append(level_context(level)+tile['descriptor'])
   for i,a in enumerate(tiles):
    others=sorted((j for j in range(len(tiles)) if j!=i),key=lambda j:math.dist(a['center'],tiles[j]['center']))[:2]
    for j in others:
     b=tiles[j];unit=max(a['width'],b['width']);xy=frame(b['center'][0]-a['center'][0],b['center'][1]-a['center'][1],a['heading']);phi=math.radians(b['heading']-a['heading']);relations.append({'sourceId':level['sourceId'],'sourceTile':base+i,'targetTile':base+j,'context':level_context(level)+a['descriptor']+b['descriptor'],'baseline':level_context(level)+[len(a['partIds']),len(b['partIds'])],'pose':[xy[0]/unit,xy[1]/unit,math.cos(phi),math.sin(phi)]})
  self.relations=relations;self.context_scaler=StandardScaler().fit(self.tile_contexts);self.normalized_tile_contexts=self.context_scaler.transform(self.tile_contexts);self.nn=NearestNeighbors(n_neighbors=min(64,len(self.tiles))).fit(self.normalized_tile_contexts);self.pose_model=ConditionalPoseModel().fit([r['context'] for r in relations],[r['pose'] for r in relations]);self.by_socket=defaultdict(list)
  for i,tile in enumerate(self.tiles):
   ps={p['id']:p for p in self.levels[tile['sourceId']]['parts']}
   for part_id in tile['partIds']:
    p=ps[part_id];self.by_socket[socket_key(p,templates,self.levels[tile['sourceId']])].append((i,part_id))
  return self
 def sample_tile(self,reference,rng):
  # Sample real multi-root structure from a learned empirical neighborhood in
  # progression, mechanics, topology and tile geometry, not counts+pose shuffle.
  tiles=macro_tiles(reference,self.templates);target=tiles[int(rng.integers(len(tiles)))];x=level_context(reference)+target['descriptor'];d,ix=self.nn.kneighbors(self.context_scaler.transform([x]));w=-.5*d[0]**2;return int(rng.choice(ix[0],p=np.exp(w-logsumexp(w))))

def evaluate_macro(model,splits):
 folds=[]
 for split in splits['splits']:
  tr=set(split['trainSourceIds']);te=set(split['heldOutSourceIds']);train=[r for r in model.relations if r['sourceId'] in tr];test=[r for r in model.relations if r['sourceId'] in te];record={'fold':split['fold'],'trainingRelations':len(train),'heldOutRelations':len(test)}
  for name,key in [('nativeMacroGeometry','context'),('countMechanicsProgressionBaseline','baseline')]:
   m=ConditionalPoseModel().fit([r[key] for r in train],[r['pose'] for r in train]);nll=-m.loglikelihood([r[key] for r in test],[r['pose'] for r in test]);err=m.reconstruction_error([r[key] for r in test],[r['pose'] for r in test]);by=defaultdict(list)
   for r,n,e in zip(test,nll,err):by[r['sourceId']].append((float(n),float(e)))
   record[name]={'levelMeanNLL':float(np.mean([np.mean(v,axis=0)[0] for v in by.values()])),'levelMeanOracleBest32XYErrorWidthUnits':float(np.mean([np.mean(v,axis=0)[1] for v in by.values()]))}
  folds.append(record)
 return {'status':'native source macro relation proposal; no physics or quality labels','folds':folds,'summary':{name:{key:float(np.mean([r[name][key] for r in folds])) for key in folds[0][name]} for name in ['nativeMacroGeometry','countMechanicsProgressionBaseline']},'limitations':['Macro partition is deterministic analysis of source topology and source nearest-neighbor organization. It is not an authored semantic annotation.','Relation reconstruction is oracle best-of32, not single-sample accuracy.','Some original boards have one macro tile and supply no intertile relation labels. They remain in tile corpus and native validation corpus.','Full native seam initialization, replay and difficulty gates remain mandatory.']}

def instantiate_tile(model,tile_index,out,rotation,translation,anchor_pair=None):
 tile=model.tiles[tile_index];level=model.levels[tile['sourceId']];ps={p['id']:p for p in level['parts']};mapping={};prefix='tile'+str(len(out['assemblyProvenance']['decisions']))+':';matrix=pose_matrix(rotation);existing={p['id']:p for p in out['parts']};shared=set()
 if anchor_pair:mapping[anchor_pair[0]]=anchor_pair[1]
 # Overlapping authored macro neighborhoods can share more than their selected
 # socket. Unify numerically identical complete native roots; never stack a
 # second copy on the same part to increase the apparent puzzle size.
 for old in tile['partIds']:
  if old in mapping:shared.add(old);continue
  source=ps[old];xy=matrix@np.array([source['x'],source['y']])+translation;angle=(source['angle']+rotation)%360
  for target in existing.values():
   if socket_key(source,model.templates,level)!=socket_key(target,model.templates,out):continue
   if math.hypot(xy[0]-target['x'],xy[1]-target['y'])>1e-4 or abs((angle-target['angle']+180)%360-180)>.005:continue
   if bomb_socket_key(level,old)!=bomb_socket_key(out,target['id']):continue
   mapping[old]=target['id'];shared.add(old);break
 for old in tile['partIds']:
  if old in mapping:continue
  p=copy.deepcopy(ps[old]);new=max(existing.keys(),default=0)+1;existing[new]=p;mapping[old]=new;p['id']=new;p['sourceId']=prefix+ps[old]['sourceId'];p['sourceOriginal']=False;p['atomicProvenance']={'sourceId':tile['sourceId'],'partId':old,'templateId':p['templateId']};v=matrix@np.array([p['x'],p['y']])+translation;p['x'],p['y']=map(float,v);p['angle']=(p['angle']+rotation)%360
  # Native blobs contain source-scene pointers; preserve them in the corpus,
  # not as stale references in a newly assembled scene.
  for key in ['sourceNativeData','sourceRigidbodyNativeData','sourcePartData']:p.pop(key,None)
  for binding in p['sourceColliderBindings']:
   binding['sourceColliderId']=new*1000+binding['shapeIndex']+1
   if p['kind'] in ['fixed','butterfly']:binding['sourceFeatureId']=prefix+binding['sourceFeatureId']
  for i,s in enumerate(p['inputShapes']):s['sourceColliderId']=new*1000+900+i
  if p['sourceRigidbody'].get('present'):p['sourceRigidbody']['sourceComponentId']=new*1000+990
  if 'slotGuideIds' in p:p['slotGuideIds']=[prefix+s for s in p['slotGuideIds']]
  out['parts'].append(p)
 seenlinks={(e['source'],e['target'],tuple(e['sourceShapeIndices']),tuple(e['targetShapeIndices'])) for e in out['links']}
 for edge in level['links']:
  if edge['source'] not in mapping or edge['target'] not in mapping:continue
  e=copy.deepcopy(edge);e['source']=mapping[e['source']];e['target']=mapping[e['target']];key=(e['source'],e['target'],tuple(e['sourceShapeIndices']),tuple(e['targetShapeIndices']))
  if key not in seenlinks:out['links'].append(e);seenlinks.add(key)
 seenguides={(e['guideOwnerId'],e['lockedOwnerId'],tuple(e['guideShapeIndices'])) for e in out['guideLinks']}
 for edge in level['guideLinks']:
  if edge['guideOwnerId'] not in mapping or edge['lockedOwnerId'] not in mapping:continue
  e=copy.deepcopy(edge);e['guideOwnerId']=mapping[e['guideOwnerId']];e['lockedOwnerId']=mapping[e['lockedOwnerId']]
  owner=existing[e['guideOwnerId']];locked=existing[e['lockedOwnerId']];e['hostPartId']=owner['sourceId'];e['lockedPartId']=locked['sourceId'];e['guidePartId']=next(b['sourceFeatureId'] for b in owner['sourceColliderBindings'] if b['shapeIndex']==e['guideShapeIndices'][0]);key=(e['guideOwnerId'],e['lockedOwnerId'],tuple(e['guideShapeIndices']))
  if key not in seenguides:out['guideLinks'].append(e);seenguides.add(key)
 # Carry owned bombs with their exact local collider/visuals; standalone bombs
 # follow the nearest source root's macro tile, preserving world relative pose.
 for bomb in level['bombs']:
  owner=bomb['ownerPartId'];nearest=owner if owner>=0 else min(ps,key=lambda i:math.dist([ps[i]['x'],ps[i]['y']],list(bomb['worldPosition'].values())))
  if nearest not in mapping or nearest in shared:continue
  b=copy.deepcopy(bomb);b['sourceColliderId']=1000000+len(out['bombs']);b['ownerPartId']=mapping[owner] if owner>=0 else -1;b['atomicProvenance']={'sourceId':tile['sourceId'],'sourceColliderId':bomb['sourceColliderId']}
  if owner<0:
   s=b['physicalShape'];v=matrix@np.array([s['center']['x'],s['center']['y']])+translation;s['center']=dict(zip(['x','y'],map(float,v)));v=matrix@np.array(list(b['worldPosition'].values()))+translation;b['worldPosition']=dict(zip(['x','y'],map(float,v)))
   for q in b['catchVisual']['quads']:
    v=matrix@np.array([q['localPosition']['x'],q['localPosition']['y']])+translation;q['localPosition'].update(dict(zip(['x','y'],map(float,v))));q['localEuler']['z']+=rotation
  for key in ['sourceNativeData','sourceTransform']:b.pop(key,None)
  out['bombs'].append(b)
 out['assemblyProvenance'].setdefault('tileImports',[]).append({'sourceId':tile['sourceId'],'tileIndex':tile_index,'partMapping':mapping,'rotation':float(rotation),'translation':list(map(float,translation)),'unifiedExistingRoots':len(shared)})
 return mapping

def proposal_legacy(model,reference,global_model,templates,rng,proposal_id,max_tiles=12):
 model.templates=templates;target=global_model['globalStylePrior'].sample_target(context(reference),rng);out={'id':proposal_id,'sourceId':'proposal-'+str(proposal_id),'position':reference['position'],'parts':[],'links':[],'guideLinks':[],'bombs':[],'complete':False,'validationStatus':'UNVALIDATED_STRUCTURED_PROPOSAL','sourceOriginal':False,'assemblyProvenance':{'method':'learned source macro tiles + exact native atomic socket unification + learned intertile pose proposals','targetReferencePosition':reference['position'],'targetGlobalStyleRecord':target['sourceStyleRecord'],'decisions':[]}};decisions=out['assemblyProvenance']['decisions'];target_count=len(reference['parts']);seed=model.sample_tile(reference,rng);tile=model.tiles[seed];owner=next(p for p in model.levels[tile['sourceId']]['parts'] if p['id']==tile['owner']);rotation=-tile['heading'];translation=-(pose_matrix(rotation)@np.array(tile['center']));instantiate_tile(model,seed,out,rotation,translation);decisions.append({'kind':'seedMacro','sourceId':tile['sourceId'],'tileIndex':seed,'core':tile['core'],'rotation':rotation,'translation':translation.tolist()});usedfamilies={model.families[tile['sourceId']]};usedtiles={seed};clusters=[{'parts':list(range(1,len(out['parts'])+1)),'center':[0.,0.],'heading':0.,'width':tile['width'],'descriptor':tile['descriptor']}]
 proposal_contexts=np.asarray([level_context(reference)+t['descriptor'] for t in model.tiles]);context_cost=np.linalg.norm(model.context_scaler.transform(proposal_contexts)-model.normalized_tile_contexts,axis=1)
 while (len(out['parts'])<target_count or len(usedfamilies)<2) and len(decisions)<max_tiles:
  options=[]
  for anchor in out['parts']:
   if anchor['kind'] not in ['ring','slider']:continue
   for index,source_anchor in model.by_socket.get(socket_key(anchor,templates,out),[]):
    if index in usedtiles:continue
    tile=model.tiles[index];newcount=len(tile['partIds'])-1
    if not newcount:continue
    # Empirical source macro frequency conditional on native tile geometry,
    # mechanics/progression; geometry/poses never randomly permuted.
    distance=context_cost[index];growth=abs(len(out['parts'])+newcount-target_count)/max(2,target_count);family_bonus=.4 if model.families[tile['sourceId']] not in usedfamilies else 0;options.append((index,source_anchor,anchor['id'],-.5*distance**2-growth+family_bonus))
  # With no compatible atomic socket, place a new complete macro tile from a
  # learned actual intertile relation. No uniform random XY placement.
  if not options:
   index=model.sample_tile(reference,rng)
   if index in usedtiles:break
   tile=model.tiles[index];cluster=clusters[int(rng.integers(len(clusters)))];query=level_context(reference)+cluster['descriptor']+tile['descriptor'];sample=model.pose_model.sample_pose(query,rng);xy=sample['pose'];angle=math.degrees(math.atan2(xy[3],xy[2]));rotation=cluster['heading']+angle-tile['heading'];center=np.array(cluster['center'])+pose_matrix(cluster['heading'])@np.array(xy[:2])*max(cluster['width'],tile['width']);translation=center-pose_matrix(rotation)@np.array(tile['center']);before=len(out['parts']);instantiate_tile(model,index,out,rotation,translation);clusters.append({'parts':[p['id'] for p in out['parts'][before:]],'center':center.tolist(),'heading':cluster['heading']+angle,'width':tile['width'],'descriptor':tile['descriptor']});decision={'kind':'learnedIntertileRelation','relationRecordIndex':sample['learnedRecordIndex'],'sourceId':tile['sourceId'],'tileIndex':index,'rotation':rotation,'translation':translation.tolist()}
  else:
   scores=np.array([x[3] for x in options]);index,source_anchor,target_anchor,_=options[int(rng.choice(len(options),p=np.exp(scores-logsumexp(scores))))];tile=model.tiles[index];a=next(p for p in model.levels[tile['sourceId']]['parts'] if p['id']==source_anchor);b=next(p for p in out['parts'] if p['id']==target_anchor);rotation=b['angle']-a['angle'];translation=np.array([b['x'],b['y']])-pose_matrix(rotation)@np.array([a['x'],a['y']]);instantiate_tile(model,index,out,rotation,translation,(source_anchor,target_anchor));decision={'kind':'exactWholeTemplateSocket','sourceId':tile['sourceId'],'tileIndex':index,'sourceAnchor':source_anchor,'targetAnchor':target_anchor,'sharedTemplateId':a['templateId'],'rotation':rotation,'translation':translation.tolist()}
  decisions.append(decision);usedtiles.add(index);usedfamilies.add(model.families[tile['sourceId']])
 out['assemblyProvenance']['sourceFamilyCount']=len(usedfamilies);out['assemblyProvenance']['allSourceTileCount']=len(usedtiles);out['assemblyProvenance']['nativeReplayRequired']=True
 try:
  style=arrangement(out,templates,Geometry(templates));target_array=np.array([target['globalStyle'][k] for k in NAMES]);prior=global_model['globalStylePrior'];error=float(np.mean(abs(prior.style_scaler.transform([style])[0]-prior.style_scaler.transform([target_array])[0])));score=float(global_model['syntheticPerturbationRanker'].predict_proba([context(out)+style.tolist()])[0,1]);out['proposalRanking']={'syntheticSourceStyleScore':score,'globalStyleTargetMAE':error,'countDeviation':abs(len(out['parts'])-target_count)/max(1,target_count),'humanQualityStatus':'unmeasured','difficultyStatus':'unmeasured','nativeSeams':'unverified'}
 except (ValueError,IndexError,np.linalg.LinAlgError) as e:out['proposalRanking']={'error':str(e)}
 return out

def proposal(model,reference,global_model,templates,rng,proposal_id,max_tiles=16):
 """Bounded structured frontier search; all geometry rejection stays visible."""
 model.templates=templates;lc=level_context(reference);count=len(reference['parts']);minimum=max(2,math.ceil(count*.85));maximum=max(2,math.floor(count*1.15));target=global_model['globalStylePrior'].sample_target(context(reference),rng);prior=global_model['globalStylePrior'];targetarray=np.array([target['globalStyle'][k] for k in NAMES]);targetnorm=prior.style_scaler.transform([targetarray])[0];geo=Geometry(templates);audit=NativeGeometrySeamAudit(templates,list(model.levels.values()));integrity=NativeAssemblyIntegrityAudit(templates,list(model.levels.values()));reference_context=context(reference);contextcost=np.linalg.norm(model.context_scaler.transform([lc+t['descriptor'] for t in model.tiles])-model.normalized_tile_contexts,axis=1);rejections=Counter();used=set();families=set();clusters=[];source_board_headings={sid:stroke_heading(l) for sid,l in model.levels.items()};target_board_heading=stroke_heading(reference);field_model=getattr(model,'field_model',None);whole_field=field_model.sample(reference,templates,rng) if field_model else None
 out={'id':proposal_id,'sourceId':'proposal-'+str(proposal_id),'position':reference['position'],'parts':[],'links':[],'guideLinks':[],'bombs':[],'complete':False,'validationStatus':'UNVALIDATED_STRUCTURED_PROPOSAL','sourceOriginal':False,'assemblyProvenance':{'method':'native source macro frontier assembly + learned continuation patterns + exact atomic template unification','targetReferencePosition':reference['position'],'targetGlobalStyleRecord':target['sourceStyleRecord'],'targetWholeBoardHeading':target_board_heading,'orientationContract':'retain native macro orientation relative to source whole-board major axis; reject socket alignment >8degrees from target board frame modulo180','declaredSearchBudget':{'maxFrontierSteps':max_tiles,'optionsPerStep':24,'minimumParts':minimum,'maximumParts':maximum,'rawGeometryToleranceWidthSquared':1e-5},'decisions':[]}}
 if whole_field:out['assemblyProvenance']['wholeBoardFieldTarget']={k:v for k,v in whole_field.items() if k not in ['field','orientation']};out['assemblyProvenance']['orientationContract']='native source macro orientation relative to dominant4fold global stroke frame; weak radial distributions retain original frame'
 def topology_signature(level):
  graph=graph_for(level);n=max(1,len(graph));components=list(nx.weakly_connected_components(graph));movables=[p['id'] for p in level['parts'] if p['kind'] in ['ring','slider']];return np.array([graph.number_of_edges()/n,len(components)/n,max(map(len,components),default=0)/n,sum(graph.degree(i)==0 for i in movables)/max(1,len(movables)),sum(graph.in_degree(i)>1 for i in graph)/n,sum(graph.out_degree(i)>1 for i in graph)/n])
 reference_topology=topology_signature(reference)
 def score(draft):
  style=arrangement(draft,templates,geo);style_error=float(np.mean(abs(prior.style_scaler.transform([style])[0]-targetnorm)));source_style=float(global_model['syntheticPerturbationRanker'].predict_proba([context(draft)+style.tolist()])[0,1]);inventory=Counter(p['kind'] for p in draft['parts']);target_inventory=Counter(p['kind'] for p in reference['parts']);inventory_error=sum(abs(inventory[k]-target_inventory[k]) for k in KINDS)/max(1,count);topology=np.array(context(draft)[-6:])-np.array(reference_context[-6:]);value=source_style-style_error*.25-inventory_error*.5-float(np.mean(abs(topology)))*.1;ranking={'syntheticSourceStyleScore':source_style,'globalStyleTargetMAE':style_error,'inventoryDeviation':inventory_error,'countDeviation':abs(len(draft['parts'])-count)/max(1,count),'humanQualityStatus':'unmeasured','difficultyStatus':'unmeasured','nativeSeams':'rawPolygonScreenOnly'}
  if whole_field:
   actual,angles=posed_field(draft,whole_field['heading'],whole_field['span'],count,templates);error=float(np.abs(actual-whole_field['field']).sum());angle_error=float(np.abs(angles-whole_field['orientation']).sum());value-=error*2+angle_error*.1;ranking.update(wholeBoardFieldL1=error,wholeBoardOrientationL1=angle_error)
  actual_topology=topology_signature(draft);topology_error=float(np.abs(actual_topology-reference_topology).mean());value-=topology_error*1.5;ranking.update(authoredTopologyMAE=topology_error,authoredTopologyVector=actual_topology.tolist(),sourceReferenceTopologyVector=reference_topology.tolist(),topologyMeaning='authored graph density/component/branch proxy only; does not claim active native constraints or true initial free parts')
  return value,ranking
 seeds=[i for i,t in enumerate(model.tiles) if len(t['partIds'])<=maximum];seedweights=-.5*contextcost[seeds]**2;seed=int(rng.choice(seeds,p=np.exp(seedweights-logsumexp(seedweights))));tile=model.tiles[seed];rotation=target_board_heading-source_board_headings[tile['sourceId']];translation=-(pose_matrix(rotation)@np.array(tile['center']));instantiate_tile(model,seed,out,rotation,translation);out['assemblyProvenance']['decisions'].append({'kind':'seedMacro','sourceId':tile['sourceId'],'tileIndex':seed,'core':tile['core'],'rotation':rotation,'translation':translation.tolist()});used.add(seed);families.add(model.families[tile['sourceId']]);clusters.append({'tileIndex':seed,'center':[0.,0.],'heading':tile['heading']+rotation,'width':tile['width'],'descriptor':tile['descriptor']});best_rank=None
 for step in range(max_tiles-1):
  if len(out['parts'])>=count:break
  options=[]
  for anchor in out['parts']:
   if anchor['kind'] not in ['ring','slider']:continue
   for index,source_anchor in model.by_socket.get(socket_key(anchor,templates,out),[]):
    if index in used:continue
    options.append({'kind':'exactWholeTemplateSocket','tileIndex':index,'sourceAnchor':source_anchor,'targetAnchor':anchor['id'],'prior':-.5*contextcost[index]**2})
  if options:
   weights=np.array([o['prior'] for o in options]);weights=np.exp(weights-logsumexp(weights));sampled=rng.choice(len(options),size=min(24,len(options)),replace=False,p=weights);options=[options[int(i)] for i in sampled]
  # Always test a few actual learned intertile arrangements, including when a
  # template socket exists. This permits new cluster organization and corridors.
  for _ in range(8):
   index=model.sample_tile(reference,rng)
   if index in used:continue
   tile=model.tiles[index];cluster=clusters[-1];sample=model.pose_model.sample_pose(lc+cluster['descriptor']+tile['descriptor'],rng);xy=sample['pose'];incoming_angle=cluster['heading'];spacing=max(cluster['width'],tile['width']);kind='learnedIntertileRelation';record=sample['learnedRecordIndex'];pattern=getattr(model,'pattern_model',None)
   if pattern and len(clusters)>1:
    from macro_pattern_v3 import relation_context
    prev=clusters[-2];v=np.array(cluster['center'])-np.array(prev['center']);spacing=float(np.linalg.norm(v))
    if spacing>1e-5:
     incoming_angle=math.degrees(math.atan2(v[1],v[0]));sample=pattern['model'].sample_pose(relation_context(reference,prev,cluster,tile,incoming_angle,spacing),rng);xy=sample['pose'];kind='learnedMacroContinuation';record=sample['learnedRecordIndex']
   if kind=='learnedIntertileRelation':
    original=model.relations[record];ta,tb=model.tiles[original['sourceTile']],model.tiles[original['targetTile']];delta=np.array(tb['center'])-np.array(ta['center']);delta=pose_matrix(target_board_heading-source_board_headings[original['sourceId']])@delta;delta*=max(cluster['width'],tile['width'])/max(ta['width'],tb['width']);center=np.array(cluster['center'])+delta
   else:center=np.array(cluster['center'])+pose_matrix(incoming_angle)@np.array(xy[:2])*spacing
   rotation=target_board_heading-source_board_headings[tile['sourceId']];heading=tile['heading']+rotation;translation=center-pose_matrix(rotation)@np.array(tile['center']);options.append({'kind':kind,'tileIndex':index,'learnedRecordIndex':record,'rotation':rotation,'translation':translation.tolist(),'center':center.tolist(),'heading':heading,'sourceBoardHeading':source_board_headings[tile['sourceId']],'prior':-.5*contextcost[index]**2})
  if whole_field:
   current,_=posed_field(out,whole_field['heading'],whole_field['span'],count,templates)
   for _ in range(8):
    index=model.sample_tile(reference,rng)
    if index in used:continue
    tile=model.tiles[index];sourceparts={p['id']:p for p in model.levels[tile['sourceId']]['parts']};rotation=target_board_heading-source_board_headings[tile['sourceId']];centers,origin=frontier_centers(whole_field,current,[sourceparts[i] for i in tile['partIds']],rotation,count,limit=3)
    for center in centers:
     translation=center-pose_matrix(rotation)@origin;options.append({'kind':'learnedWholeBoardFieldPlacement','tileIndex':index,'rotation':rotation,'translation':translation.tolist(),'center':center.tolist(),'heading':tile['heading']+rotation,'sourceBoardHeading':source_board_headings[tile['sourceId']],'prior':-.5*contextcost[index]**2})
  candidates=[]
  for option in options:
   index=option['tileIndex'];tile=model.tiles[index];draft=copy.deepcopy(out)
   if option['kind']=='exactWholeTemplateSocket':
    a=next(p for p in model.levels[tile['sourceId']]['parts'] if p['id']==option['sourceAnchor']);b=next(p for p in out['parts'] if p['id']==option['targetAnchor']);rotation=b['angle']-a['angle'];alignment_error=abs((source_board_headings[tile['sourceId']]+rotation-target_board_heading+90)%180-90)
    if alignment_error>8:rejections['wholeBoardHeadingIncompatible']+=1;continue
    translation=np.array([b['x'],b['y']])-pose_matrix(rotation)@np.array([a['x'],a['y']]);anchor_pair=(a['id'],b['id'])
   else:rotation=option['rotation'];translation=np.array(option['translation']);anchor_pair=None
   instantiate_tile(model,index,draft,rotation,translation,anchor_pair)
   if len(draft['parts'])==len(out['parts']):rejections['zeroNewRootsAfterExactUnification']+=1;continue
   if len(draft['parts'])>maximum:rejections['referenceScaleExceeded']+=1;continue
   if whole_field:
    low,high,_,_,_=native_frame(draft,templates,whole_field['heading']);limit=.6*whole_field['span']
    if min(low)<-limit or max(high)>limit:rejections['outsideWholeBoardNativeEnvelope']+=1;continue
   seam=audit.check(draft)
   if not seam['rawGeometrySeamPassed']:
    rejections['newNativeGeometrySeamConflict']+=1;continue
   constructed=integrity.check(draft)
   if not constructed['nativeAssemblyIntegrityPassed']:rejections['nativeAssemblyIntegrityConflict']+=1;continue
   try:value,ranking=score(draft)
   except (ValueError,IndexError,np.linalg.LinAlgError):rejections['nonfiniteStyleGeometry']+=1;continue
   # A small prior term breaks close style ties. It is the learned tile context
   # likelihood, not random global placement or a source layout replay.
   value+=float(option['prior'])*.01;candidates.append((value,draft,option,ranking,rotation,translation))
  if not candidates:rejections['frontierStalled']+=1;break
  _,out,option,best_rank,rotation,translation=max(candidates,key=lambda x:x[0]);index=option['tileIndex'];tile=model.tiles[index];decision={k:v for k,v in option.items() if k!='prior'};decision.update(sourceId=tile['sourceId'],rotation=float(rotation),translation=list(map(float,translation)));out['assemblyProvenance']['decisions'].append(decision);used.add(index);families.add(model.families[tile['sourceId']]);center=pose_matrix(rotation)@np.array(tile['center'])+translation;clusters.append({'tileIndex':index,'center':center.tolist(),'heading':tile['heading']+rotation,'width':tile['width'],'descriptor':tile['descriptor']})
 if best_rank is None:
  try:_,best_rank=score(out)
  except (ValueError,IndexError,np.linalg.LinAlgError):best_rank={'error':'degenerate style geometry'}
 out['proposalRanking']=best_rank;out['rawGeometrySeamAudit']=audit.check(out);out['nativeAssemblyIntegrityAudit']=integrity.check(out);out['assemblyProvenance'].update(sourceFamilyCount=len(families),allSourceTileCount=len(used),nativeReplayRequired=True,rejectionCounts=dict(rejections),referenceScaleSatisfied=minimum<=len(out['parts'])<=maximum,multipleSourceFamilies=len(families)>1)
 return out

def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',type=Path,required=True);p.add_argument('--train',action='store_true');p.add_argument('--proposals',type=int,default=0);p.add_argument('--reference-limit',type=int,default=300);p.add_argument('--seed',type=int,default=SEED);p.add_argument('--output',type=Path);args=p.parse_args();w=args.workspace.resolve();packfile=w/'data/source500_native.json';tfile=w/'data/source_templates.json';levels=read(packfile)['levels'];templates={t['id']:t for t in read(tfile)['templates']};splits=read(w/'models/family_splits.json');modelpath=w/'models/macro_assembly.joblib'
 if args.train:
  model=MacroAssemblyModel().fit(levels,templates,splits['families']);report=evaluate_macro(model,splits);joblib.dump(model,modelpath,compress=3);report.update(sourceCount=len(levels),tileCount=len(model.tiles),macroRelationCount=len(model.relations),hashes={'sourcePack':hashfile(packfile),'templates':hashfile(tfile),'trainer':hashfile(Path(__file__)),'model':hashfile(modelpath)});(w/'models/macro_assembly_evaluation.json').write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps({k:report[k] for k in ['sourceCount','tileCount','macroRelationCount','summary']},indent=2),flush=True)
 if args.proposals:
  if not args.output:raise ValueError('--output must name an explicit incomplete preview destination')
  model=joblib.load(modelpath);global_model=joblib.load(w/'models/global_composition.joblib');rng=np.random.default_rng(args.seed);proposals=[]
  if (w/'models/macro_pattern.joblib').exists():model.pattern_model=joblib.load(w/'models/macro_pattern.joblib')
  if (w/'models/whole_board_field.joblib').exists():model.field_model=joblib.load(w/'models/whole_board_field.joblib')
  for i in range(args.proposals):
   reference=levels[min(len(levels),args.reference_limit)-1 if args.proposals==1 else min(min(len(levels),args.reference_limit)-1,int(i*min(len(levels),args.reference_limit)/args.proposals))];candidate=proposal(model,reference,global_model,templates,rng,3000001+i);proposals.append(candidate);print('PROPOSAL',i+1,'parts',len(candidate['parts']),'families',candidate['assemblyProvenance']['sourceFamilyCount'],flush=True)
  args.output.parent.mkdir(parents=True,exist_ok=True);args.output.write_text(json.dumps({'schemaVersion':3,'complete':False,'acceptedCount':0,'status':'INCOMPLETE_NATIVE_REPLAY_AND_NOVELTY_REQUIRED','seed':args.seed,'sourceCount':len(levels),'levels':proposals,'hashes':{'sourcePack':hashfile(packfile),'templates':hashfile(tfile),'model':hashfile(modelpath),'globalModel':hashfile(w/'models/global_composition.joblib'),'proposer':hashfile(Path(__file__))}},separators=(',',':')),encoding='utf8')

if __name__=='__main__':
 sys.modules.setdefault('macro_assembly_v3',sys.modules[__name__]);MacroAssemblyModel.__module__='macro_assembly_v3';main()

