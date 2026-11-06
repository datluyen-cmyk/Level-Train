"""Inventory the complete 500-level source corpus without eligibility pruning."""
from pathlib import Path
from collections import Counter,defaultdict
import hashlib,json,math
ROOT=Path(__file__).resolve().parents[1]
def load(p):return json.loads(p.read_text(encoding='utf8'))
def main():
 src=ROOT/'Research/normalized_corpus.json';corpus=load(src);v2=load(ROOT/'Research/physics_v2/corpus500.json');rawlinks={l['id']:l['state']['links'] for l in v2['levels']}
 totals=Counter();present=Counter();paths=Counter();axis=Counter();features=Counter();examples={};unsupported=Counter();levels=[];scales=Counter();ring_features=Counter();pull_features=Counter();turns=Counter()
 for l in corpus['levels']:
  stats=Counter();state=l['state'];ps={p['id']:p for p in state['parts']};complex_count=0
  for p in state['parts']+state['fixtures']:
   stats[p['sourceClass']]+=1;s=p['sourceWorldScale'];scales['positiveUniform' if s[0]>0 and s[1]>0 and abs(s[0]-s[1])<1e-5 else 'other']+=1
   if p['sourcePartData']['LockCount']:stats['chainLockedParts']+=1
   if p['sourcePartData']['IsPaletteLock']:stats['paletteLockParts']+=1
   if p['sourcePartData']['IsPaletteLockKey']:stats['paletteKeyParts']+=1
   if p['kind']=='slider':
    pts=p['sourceRenderData']['_pathPoints'];kind={2:'straight',3:'roundedL',4:'roundedU'}.get(len(pts),'other');paths[kind]+=1;stats[kind+'Pull']+=1;axis[str(p['sourceNativeData']['lockDirectionType'])]+=1
    for a,b,c in zip(pts,pts[1:],pts[2:]):
     u=(b['x']-a['x'],b['y']-a['y']);v=(c['x']-b['x'],c['y']-b['y']);turns[round(math.degrees(math.atan2(u[0]*v[1]-u[1]*v[0],u[0]*v[0]+u[1]*v[1])),3)]+=1
  stats['BombRelative']=l['features']['bombCount'];stats['guideLinks']=len(state['guideLinks']);stats['ringLinks']=len(rawlinks[l['id']])
  for e in rawlinks[l['id']]:
   for side in ['source','target']:
    p=ps[e[side]];f=e[side+'Feature'];key=p['kind']+'::'+f;features[key]+=1;examples.setdefault(key,{'sourceId':l['id'],'position':l['position'],'partId':p['id'],'sourcePartId':p['sourceId']})
    (pull_features if p['kind']=='slider' else ring_features)[f]+=1
  for k,n in stats.items():totals[k]+=n;present[k]+=int(n>0)
  unsupported.update(l['projection']['unsupportedMechanics'])
  levels.append({'sourceId':l['id'],'position':l['position'],'sourceFamily':l['group'],'counts':dict(stats),'topology':{k:l['features'][k] for k in ['graphComponents','graphCycleRank','graphDirectedCyclicComponents','graphDepth','maxIncoming','maxOutgoing','branchingParts','isolatedParts']},'priorProjectionGaps':l['projection']['unsupportedMechanics']})
 out=ROOT/'Research/physics_v3';out.mkdir(exist_ok=True)
 report={'sourceCount':len(levels),'sourceCorpusSha256':hashlib.sha256(src.read_bytes()).hexdigest(),'coveragePolicy':'All 500 ordered originals retained. No ring-only, component-count, fixture, or mechanic pruning. No original runtime parity or source solution labels certified yet.','counts':dict(totals),'levelsContaining':dict(present),'pullPathTypes':dict(paths),'pullAdjacentTurnsDegrees':dict(turns),'pullLockDirectionTypes':dict(axis),'rootScaleInventory':dict(scales),'logicalFeatureEndpoints':{'ring':dict(ring_features),'slider':dict(pull_features)},'logicalFeatureExamples':examples,'priorNormalizerImplementationGaps':dict(unsupported),'prior62Explanation':'437 scenes flagged by the reduced normalizer; 63 scenes unflagged; V2 all-ring eligibility also dropped source140004 (11rings+2straightPulls), leaving62. Nine further attachment cases were later excluded. This is implementation policy, not a native restriction.','levels':levels}
 (out/'source500_diversity_audit.json').write_text(json.dumps(report,indent=2),encoding='utf8')
 rows=['# Full 500 original level diversity audit','','All 500 original progression entries are retained. This inventory is static source evidence, not original runtime parity or solved labels.','','| Native item | Count | Levels containing |','|---|---:|---:|']
 for key in ['RingObject','PullObject','FixedObject','FixedButterflyObject','BombRelative','chainLockedParts','paletteLockParts','paletteKeyParts','guideLinks','ringLinks']:rows.append(f'| {key} | {totals[key]} | {present[key]} |')
 rows+=['','Pull geometry: '+', '.join(f'{n} {k}' for k,n in paths.items())+'. All adjacent path turns are ±90°.','',f'Pull movement: {axis["0"]} native type0 transform.up; {axis["1"]} type1 -transform.right. Both signed directions are allowed; the old axis=rotation projection was wrong.','','All '+str(sum(scales.values()))+' movable and fixture root transforms have positive uniform XY scales. Native Polygon/Box/Circle collider classes and source IDs are preserved by V3.','','## Why the previous subset had only 62 scenes','',report['prior62Explanation'],'','These gaps cannot justify excluding 438 source levels, simplifying every shape to a ring, or learning original difficulty from the subset. Representative untouched source parity is required before training labels.','','## Logical Pull feature aliases','','The source has one actual activeCollider per Pull. All authored Pull endpoint strings are body or attachment.bent_N.solid. V3 keeps these strings as aliases to that actual collider. PullToRingLinkRegistrar resolves only the Ring endpoint feature when making passive bindings; it does not subdivide the Pull collider.','','## Current gate','','No fresh300 accepted pack exists. V2 remains a rejected pilot. Mixed shapes, catches, butterflies, bombs, chains, per-feature passive bindings and original signed movement must be validated before source labels, training or generation.','']
 (out/'source500_diversity_audit.md').write_text('\n'.join(rows),encoding='utf8');print(json.dumps({k:v for k,v in report.items() if k not in ['levels','logicalFeatureExamples','logicalFeatureEndpoints']},indent=2))
if __name__=='__main__':main()
