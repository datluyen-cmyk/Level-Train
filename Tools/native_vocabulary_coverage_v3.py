"""Measure actual native atomic and multisocket donor coverage before proposing."""
from pathlib import Path
from collections import Counter,defaultdict
import argparse,json,itertools,math
import numpy as np
import joblib
from macro_assembly_v3 import socket_key,pose_matrix
from macro_rewrite_v3 import source_regions
from layout_learning_v3 import read,hashfile

def measure(w):
 levels=read(w/'data/source500_native.json')['levels'];templates={t['id']:t for t in read(w/'data/source_templates.json')['templates']};model=joblib.load(w/'models/macro_assembly.joblib');model.templates=templates;members=defaultdict(list);body_inventory=defaultdict(list)
 for l in levels:
  for p in l['parts']:
   key=socket_key(p,templates,l);members[key].append((l['sourceId'],p['id'],l['position']));t=templates[p['templateId']];body={k:t.get(k) for k in ['kind','radius','width','gap','length','movement','pullVisual']};body['physicalBody']=[s for s in t['physicalShapes'] if s['role']=='body'];body_inventory[json.dumps(body,sort_keys=True)].append((l['sourceId'],p['id'],p['templateId']))
 records=[]
 for l in levels[:300]:
  ps={p['id']:p for p in l['parts']};regions=source_regions(model,l);counts=Counter();examples=[]
  for region in regions:
   boundary=region['boundary']
   if not boundary:counts['noBoundaryDisconnectedNeighborhoods']+=1;continue
   anchor=min(boundary,key=lambda i:len(model.by_socket.get(socket_key(ps[i],templates,l),[])));donors=[(i,a) for i,a in model.by_socket.get(socket_key(ps[anchor],templates,l),[]) if model.tiles[i]['sourceId']!=l['sourceId']];counts['boundaryNeighborhoods']+=1;counts['anchorDonorMacroCount']+=len(donors)
   if not donors:counts['anchorHasNoExactCrossSourceDonor']+=1
   matched=0
   for index,source_anchor in donors:
    tile=model.tiles[index];dp={p['id']:p for p in model.levels[tile['sourceId']]['parts']};donor=model.levels[tile['sourceId']];a,b=dp[source_anchor],ps[anchor];rotation=b['angle']-a['angle'];translation=np.array([b['x'],b['y']])-pose_matrix(rotation)@np.array([a['x'],a['y']]);remaining=[]
    for bid in boundary:
     target=ps[bid];ok=False
     for pid in tile['partIds']:
      p=dp[pid]
      if socket_key(p,templates,donor)!=socket_key(target,templates,l):continue
      xy=pose_matrix(rotation)@np.array([p['x'],p['y']])+translation
      if math.dist(xy,[target['x'],target['y']])<=1e-4 and abs((p['angle']+rotation-target['angle']+180)%360-180)<=.005:ok=True;break
     if not ok:remaining.append(bid)
    if not remaining:matched+=1
   counts['fullyCompatibleBoundaryDonorMacros']+=matched
   if matched:counts['neighborhoodsWithCompatibleBoundary']+=1
   if len(examples)<3:examples.append({'removedRoots':region['interior'],'boundaryRoots':boundary,'anchorId':anchor,'anchorCrossSourceMacroDonors':len(donors),'allBoundaryCompatibleMacroDonors':matched,'anchorKind':ps[anchor]['kind'],'anchorTemplate':ps[anchor]['templateId']})
  records.append({'sourceId':l['sourceId'],'position':l['position'],'sourceRootCount':len(ps),'rewriteNeighborhoods':len(regions),'coverage':dict(counts),'examples':examples})
 total=Counter()
 for r in records:total.update(r['coverage'])
 report={'status':'source-derived grammar donor coverage diagnostic, not generated labels','trainingSourceCount':500,'targetProgressionPositions':[1,300],'atomicVocabulary':{'rootInstances':sum(len(l['parts']) for l in levels),'fullCatalogTemplateCount':len(templates),'strictNativeInputBodyStateSocketIdentities':len(members),'socketIdentitiesPresentInMultipleSources':sum(len({s for s,_,_ in v})>1 for v in members.values()),'maximumSourceBoardsSharingOneStrictSocket':max(len({s for s,_,_ in v}) for v in members.values()),'sameNativeBodyWithDifferentCompleteTemplates':sum(len({t for _,_,t in v})>1 for v in body_inventory.values()),'bodyFamilies':len(body_inventory)},'first300RewriteCoverage':{'levelsWithDeclared20to35PercentNeighborhood':sum(r['rewriteNeighborhoods']>0 for r in records),'levelsWithExactBoundaryDonor':sum(r['coverage'].get('fullyCompatibleBoundaryDonorMacros',0)>0 for r in records),'levelsWithDisconnectedNeighborhood':sum(r['coverage'].get('noBoundaryDisconnectedNeighborhoods',0)>0 for r in records),'totals':dict(total)},'levels':records,'limitations':['Rigid authored macro donors cannot cover all desired multi-socket frames. Failure must guide vocabulary/grammar research, not justify dropping source mechanics or making arbitrary geometry.','Strict socket identity retains every numeric native geometry value. Small bake-rounding differences may split visually identical assets.','Same-body template families are potential authored variant vocabulary only; their attachments, inputs and bindings still require exact certification.'],'hashes':{'sourcePack':hashfile(w/'data/source500_native.json'),'templates':hashfile(w/'data/source_templates.json'),'macroModel':hashfile(w/'models/macro_assembly.joblib'),'tool':hashfile(Path(__file__))}};path=w/'models/native_vocabulary_coverage.json';path.write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps({k:report[k] for k in ['atomicVocabulary','first300RewriteCoverage']},indent=2))
def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',required=True,type=Path);a=p.parse_args();measure(a.workspace.resolve())
if __name__=='__main__':main()
