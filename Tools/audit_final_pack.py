"""Independent delivered-pack audit; never fits or changes the saved pipeline."""
from pathlib import Path
import collections, hashlib, importlib.util, json

ROOT=Path(__file__).resolve().parents[1]
BUNDLE=ROOT/'RotateRings Test/LevelTraining'
RESEARCH=ROOT/'Research'
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def distance(a,b):
    keys=a.keys()|b.keys()
    return 1-sum(min(a[k],b[k]) for k in keys)/max(1,sum(max(a[k],b[k]) for k in keys))

def main():
    path=BUNDLE/'batches/final-300/generated_levels.json'
    sources=json.loads((BUNDLE/'corpus.json').read_text())['levels']
    extras=json.loads((RESEARCH/'extra_originals_for_novelty.json').read_text())['levels']
    generated=json.loads(path.read_text())['levels']
    spec=importlib.util.spec_from_file_location('final_audit_adapter',BUNDLE/'adapters/rotate_rings_adapter.py')
    adapter=importlib.util.module_from_spec(spec);spec.loader.exec_module(adapter)
    all_originals=sources+extras
    originals={adapter.fingerprint(l):l['id'] for l in all_originals}
    descriptors=[(l['id'],adapter.descriptor(l)) for l in all_originals]
    collisions=[];near=[];duplicates=[];replay_failed=[];comparisons=[];seen={}
    minimum=1.;pair_min=1.
    for level in generated:
        fp=adapter.fingerprint(level);desc=adapter.descriptor(level)
        if fp in originals:collisions.append({'generated':level['id'],'original':originals[fp]})
        if fp in seen:duplicates.append({'generated':level['id'],'other':seen[fp]})
        seen[fp]=level['id']
        nearest=min((distance(desc,d),sid) for sid,d in descriptors)
        minimum=min(minimum,nearest[0])
        row={'generated':level['id'],'nearestOriginal':nearest[1],'distance':nearest[0]}
        comparisons.append(row)
        if nearest[0]<.10:near.append(row)
        if not adapter.replay(level,level['solution']):replay_failed.append(level['id'])
    for i,level in enumerate(generated):
        left=adapter.descriptor(level)
        for other in generated[i+1:]:
            d=distance(left,adapter.descriptor(other));pair_min=min(pair_min,d)
            if d<.10:near.append({'generated':level['id'],'otherGenerated':other['id'],'distance':d})
    report={'passed':len(generated)==300 and not any((collisions,near,duplicates,replay_failed)),
      'generatedCount':len(generated),'originalCount':len(all_originals),'trainingOriginalCount':len(sources),
      'extraNoveltyOnlyOriginalCount':len(extras),'uniqueOriginalIds':len({l['id'] for l in all_originals}),
      'packSha256':sha(path),'adapterSha256':sha(BUNDLE/'adapters/rotate_rings_adapter.py'),
      'canonicalOriginalCollisions':collisions,'canonicalGeneratedDuplicates':duplicates,
      'nearCopyThreshold':.10,'nearCopyCollisions':near,'minimumOriginalDistance':minimum,
      'minimumGeneratedPairDistance':pair_min,'replayCount':len(generated),'replayFailed':replay_failed,
      'comparisons':comparisons,
      'limits':'Replays reconstructed contact-sensitive pin/gap rules only. Graph equivalence is conservative WL; neighborhood distance is heuristic. Native mechanics and human difficulty/fun unmeasured.'}
    (RESEARCH/'final_pack_audit.json').write_text(json.dumps(report,indent=2))
    print(json.dumps({k:v for k,v in report.items() if k!='comparisons'}))

if __name__=='__main__':main()
