"""Extract authored spatial/composition evidence, never random count matching."""
from pathlib import Path
from collections import Counter
import hashlib,json,math
import numpy as np
import networkx as nx
from scipy.spatial import cKDTree,ConvexHull
from shapely.geometry import Polygon,Point
from shapely.ops import nearest_points
from shapely import affinity,make_valid,union_all
ROOT=Path(__file__).resolve().parents[1]
def load(p):return json.loads(p.read_text(encoding='utf8'))
def local(v,a):
 r=math.radians(a);c,s=math.cos(r),math.sin(r);return [c*v[0]+s*v[1],-s*v[0]+c*v[1]]
def angle(a):return (a+180)%360-180
def polygon(shape):
 if shape.get('shapeType')=='circle':return None
 paths=shape.get('paths',[{'points':shape.get('points',[])}]);return make_valid(Polygon([(p['x'],p['y']) for p in paths[0]['points']], [[(p['x'],p['y']) for p in q['points']] for q in paths[1:]]))
def main():
 packpath=ROOT/'Research/physics_v3/full500/source_parity_pack.json';tpath=ROOT/'Research/physics_v3/full500/source_templates.json';pack=load(packpath);templates={t['id']:t for t in load(tpath)['templates']};records=[];motifs=Counter();cache={}
 def body(p):
  tid=p['templateId']
  if tid not in cache:cache[tid]=union_all([polygon(s) for s in templates[tid]['physicalShapes'] if s['role']=='body' and polygon(s) is not None])
  g=affinity.rotate(cache[tid],p['angle'],origin=(0,0));return affinity.translate(g,p['x'],p['y'])
 for l in pack['levels']:
  ps={p['id']:p for p in l['parts']};mov=[p for p in ps.values() if p['kind'] in ['ring','slider']];centers=np.array([[p['x'],p['y']] for p in mov]);mean=centers.mean(axis=0);cov=np.cov((centers-mean).T);values,vectors=np.linalg.eigh(cov);axis=vectors[:,np.argmax(values)];pca_angle=math.degrees(math.atan2(axis[1],axis[0]));unit=float(np.median([max(templates[p['templateId']].get('width',1.),.01) for p in mov]));geoms={p['id']:body(p) for p in mov};tree=cKDTree(centers);neighborhoods=[];edgepairs=set();envelope_nesting=[]
  distances,indices=tree.query(centers,k=min(len(mov),5))
  for i,p in enumerate(mov):
   entries=[]
   for j in np.atleast_1d(indices[i])[1:]:
    q=mov[int(j)];dv=[q['x']-p['x'],q['y']-p['y']];v=local(dv,p['angle']);gap=geoms[p['id']].distance(geoms[q['id']]);entries.append({'neighbor':q['id'],'kind':q['kind'],'localDelta':v,'localDeltaInWidthUnits':[x/unit for x in v],'relativeAngle':angle(q['angle']-p['angle']),'rawOutlineGap':gap});edgepairs.add(tuple(sorted((p['id'],q['id']))))
    if geoms[p['id']].envelope.contains(Point(q['x'],q['y'])):envelope_nesting.append([p['id'],q['id']])
   neighborhoods.append({'owner':p['id'],'nearest':entries})
  # Near-copy rejection and learning retain continuous relative placements,
  # receiving shapes, angles and source topology, not merely these aggregates.
  pin_relations=[];graph=nx.DiGraph();graph.add_nodes_from(ps)
  for e in l['links']:
   a,b=ps[e['source']],ps[e['target']];dv=[b['x']-a['x'],b['y']-a['y']];v=local(dv,a['angle']);entry={'source':a['id'],'target':b['id'],'sourceKind':a['kind'],'targetKind':b['kind'],'sourceFeature':e['sourceFeature'],'targetFeature':e['targetFeature'],'targetCenterInSourceFrame':v,'targetAngleInSourceFrame':angle(b['angle']-a['angle']),'sourceShapeIndices':e['sourceShapeIndices'],'targetShapeIndices':e['targetShapeIndices'],'nativeRelationKind':e['relationKind'],'nativeActivityKind':e['activityKind']};graph.add_edge(a['id'],b['id'])
   # Pinhead location and receiving body are actual authored native polygons.
   heads=[templates[a['templateId']]['physicalShapes'][i] for i in e['sourceShapeIndices'] if templates[a['templateId']]['physicalShapes'][i]['role']=='head_trigger']
   if heads and b['id'] in geoms:
    hg=union_all([polygon(h) for h in heads]);hg=affinity.translate(affinity.rotate(hg,a['angle'],origin=(0,0)),a['x'],a['y']);hc=hg.centroid;nearest=nearest_points(hc,geoms[b['id']])[1];rel=local([hc.x-b['x'],hc.y-b['y']],b['angle']);entry['receivingGeometry']={'headCenterInTargetFrame':rel,'headToTargetRawOutlineDistance':hc.distance(geoms[b['id']]),'headRawOverlapArea':hg.intersection(geoms[b['id']]).area,'nearestTargetOutlinePointInTargetFrame':local([nearest.x-b['x'],nearest.y-b['y']],b['angle']),'targetBodyBoundsLocal':list(cache[b['templateId']].bounds)}
   pin_relations.append(entry)
  guide_placements=[]
  for e in l['guideLinks']:
   fixed,p=ps[e['guideOwnerId']],ps[e['lockedOwnerId']];dv=[fixed['x']-p['x'],fixed['y']-p['y']];v=local(dv,p['angle']);g={'guideOwner':fixed['id'],'guideId':e['guidePartId'],'lockedPart':p['id'],'lockedKind':p['kind'],'guideKind':fixed['kind'],'guideOriginInLockedFrame':v,'guideAngleInLockedFrame':angle(fixed['angle']-p['angle']),'guideShapeIndices':e['guideShapeIndices']}
   if p['kind']=='slider':
    lt=templates[p['templateId']]['movement']['lockDirectionType'];pullaxis=(0,1) if lt==0 else (-1,0);g['axisDistance']=v[0]*pullaxis[0]+v[1]*pullaxis[1];g['perpendicularDistance']=-v[0]*pullaxis[1]+v[1]*pullaxis[0]
   guide_placements.append(g);graph.add_edge(fixed['id'],p['id'],guide=True)
  # Match reflected centers only to same shape family. These are measured
  # composition errors, not claims that a board was consciously symmetric.
  symmetry={}
  for name,direction in [('worldX',(1,0)),('worldY',(0,1)),('principalAxis',tuple(axis))]:
   n=np.array([-direction[1],direction[0]]);errs=[]
   for i,p in enumerate(mov):
    reflected=centers[i]-2*np.dot(centers[i]-mean,n)*n;candidates=[float(np.linalg.norm(reflected-centers[j])) for j,q in enumerate(mov) if q['kind']==p['kind']];errs.append(min(candidates)/unit)
   symmetry[name+'MeanCenterErrorInWidthUnits']=float(np.mean(errs))
  allbody=union_all(list(geoms.values()));bounds=allbody.bounds;boardarea=(bounds[2]-bounds[0])*(bounds[3]-bounds[1]);hullarea=allbody.convex_hull.area;occupied=allbody.area
  branches=sum(graph.out_degree(n)>1 for n in graph);merges=sum(graph.in_degree(n)>1 for n in graph);undirected=graph.to_undirected();components=list(nx.connected_components(undirected));scc=list(nx.strongly_connected_components(graph));cycles=sum(len(c)>1 for c in scc)
  stat={'branchNodes':branches,'mergeNodes':merges,'directedCyclicClusters':cycles,'weakClusterSizes':sorted(map(len,components),reverse=True),'maxStaticDependencyDepth':nx.dag_longest_path_length(graph) if nx.is_directed_acyclic_graph(graph) else None,'releaseOrderStatus':'unverified; graph edges are authored relations, not verified release order','envelopeCenterNestingPairs':envelope_nesting}
  motifs['levelsWithBranches']+=branches>0;motifs['levelsWithMerges']+=merges>0;motifs['levelsWithDirectedCycles']+=cycles>0;motifs['levelsWithMultipleClusters']+=len(components)>1
  # Orientation relative to principal board axis captures parallel/perpendicular
  # arrangements independent of a global whole-board rotation.
  orientations=[{'id':p['id'],'kind':p['kind'],'principalPosition':local([p['x']-mean[0],p['y']-mean[1]],pca_angle),'principalAngle':angle(p['angle']-pca_angle),'templateId':p['templateId']} for p in mov]
  records.append({'sourceId':l['sourceId'],'position':l['position'],'sourceFamily':l.get('sourceFamily'),'widthUnit':unit,'boardPrincipalAngle':pca_angle,'principalLayout':orientations,'neighborhoods':neighborhoods,'pinReceivingRelations':pin_relations,'guidePlacements':guide_placements,'composition':{'symmetry':symmetry,'rawOutlineOccupiedArea':occupied,'boundingWhitespaceFraction':1-occupied/boardarea if boardarea else 0,'convexHullWhitespaceFraction':1-occupied/hullarea if hullarea else 0,'bounds':list(bounds)},'dependencyMotifs':stat})
 out=ROOT/'Research/physics_v3/source500_spatial_grammar.json';report={'sourceCount':len(records),'sourcePackSha256':hashlib.sha256(packpath.read_bytes()).hexdigest(),'templatePackSha256':hashlib.sha256(tpath.read_bytes()).hexdigest(),'status':'Source-authored continuous spatial evidence; not trained model, source solved labels or generator acceptance','trainingContract':{'mustLearn':['source ordered progression and family-separated validation','continuous owner-local relative positions and relative angles','pin head placement inside receiving body and opening geometry','straight/L/U Pull and fixed/butterfly guide composition','principal-axis alignment, reflected-center error and outline whitespace','dependency branching, merge and cluster motifs','verified release order only after native source replay'],'forbidden':['count-matched arbitrary shuffling','replacing mixed shapes with circles','discarding source families to fit simulator','using unverified static graph order as a solution label']},'motifPresence':dict(motifs),'levels':records};out.write_text(json.dumps(report,separators=(',',':')),encoding='utf8');print(json.dumps({'sourceCount':len(records),'motifs':dict(motifs),'bytes':out.stat().st_size,'output':str(out)},indent=2))
if __name__=='__main__':main()
