"""Check delivered reconstructed graphs against every extra original scene.

The 500-stage training set remains unchanged. Seven other APK scenes are retained
only as independent novelty exclusions, including native-unsupported fixtures.
"""
from pathlib import Path
import json,hashlib,importlib.util,collections
import normalize_corpus as normalizer

ROOT=Path(__file__).resolve().parents[1]
RESEARCH=ROOT/'Research';BUNDLE=ROOT/'RotateRings Test/LevelTraining'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def distance(a,b):
    keys=a.keys()|b.keys()
    return 1-sum(min(a[k],b[k]) for k in keys)/max(1,sum(max(a[k],b[k]) for k in keys))

def main():
    corpus=json.loads((BUNDLE/'corpus.json').read_text(encoding='utf8'))['levels'];ids={l['id'] for l in corpus}
    paths=sorted([p for p in (RESEARCH/'levels').glob('*.json') if p.stem.isdigit() and p.stem not in ids],key=lambda p:int(p.stem))
    spec=importlib.util.spec_from_file_location('novelty_adapter',BUNDLE/'adapters/rotate_rings_adapter.py');a=importlib.util.module_from_spec(spec);spec.loader.exec_module(a)
    extras=[]
    keys=('id','kind','x','y','radius','width','gap','angle','color','collectible','scoreEligible','key','lockKey','bomb','axis','length')
    for i,path in enumerate(paths):
        l=normalizer.normalize(json.loads(path.read_text(encoding='utf8')),501+i)
        l['state']['parts']=[{k:p[k] for k in keys if k in p} for p in l['state']['parts']]
        l['state']['fixtures']=[{k:p[k] for k in keys if k in p} for p in l['state']['fixtures']]
        l['state']['links']=[{k:e[k] for k in ('source','target','bearing','pinWidth')} for e in l['state']['links']]
        l['provenance']['corpusRole']='novelty-only; not trained or difficulty-labeled'
        extras.append(l)
    output=RESEARCH/'extra_originals_for_novelty.json';output.write_text(json.dumps({'levels':extras,'originalSceneTotal':len(ids)+len(extras)},separators=(',',':')),encoding='utf8')
    final=BUNDLE/'batches/final-300/generated_levels.json';pack=final if final.exists() else BUNDLE/'batches/final-300/partial_levels.json'
    generated=json.loads(pack.read_text(encoding='utf8'))['levels'];fps={a.fingerprint(l):l['id'] for l in extras};desc=[(l['id'],a.descriptor(l)) for l in extras]
    exact=[];near=[];minimum=1.;comparisons=[]
    for l in generated:
        fp=a.fingerprint(l)
        if fp in fps:exact.append({'generated':l['id'],'source':fps[fp]})
        nearest=min(((distance(a.descriptor(l),d),sid) for sid,d in desc),default=(1.,None))
        minimum=min(minimum,nearest[0]);comparisons.append({'generated':l['id'],'nearestExtraSource':nearest[1],'distance':nearest[0]})
        if nearest[0]<.10:near.append(comparisons[-1])
    report={'passed':not exact and not near,'completePack':final.exists(),'trainingOriginalCount':len(corpus),'extraOriginalCount':len(extras),
      'allOriginalSceneCount':len(corpus)+len(extras),'generatedCount':len(generated),'packSha256':sha(pack),'extrasSha256':sha(output),
      'canonicalEquivalentExtraCollisions':exact,'nearCopyThreshold':.10,'nearCopyExtraCollisions':near,'minimumExtraDistance':minimum,
      'extraIds':[l['id'] for l in extras],'comparisons':comparisons,
      'limits':'Conservative reconstructed graph-equivalence and heuristic neighborhood distance. Extra native fixture/collision semantics not certified. Does not refit models or change 500-stage training corpus.'}
    (RESEARCH/'extra_originals_novelty_audit.json').write_text(json.dumps(report,indent=2),encoding='utf8')
    print(json.dumps({k:v for k,v in report.items() if k!='comparisons'}))
if __name__=='__main__':main()
