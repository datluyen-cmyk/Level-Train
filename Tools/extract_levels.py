"""Read serialized original level bundles without running original application."""
from pathlib import Path
import json, hashlib, re, collections, math
import UnityPy

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT/'Research/extracted/assets/yoo/DefaultPackage'
OUT = ROOT/'Research/levels'
OUT.mkdir(exist_ok=True)
levels=[]
counts=collections.Counter()
for i,path in enumerate(sorted(SOURCE.glob('*physiclevelscene_level*.bundle'))):
    env=UnityPy.load(str(path))
    objects={o.path_id: {'type':o.type.name,'data':o.read_typetree()} for o in env.objects if o.type.name in ['GameObject','Transform','MonoBehaviour','MonoScript','Camera']}
    scripts={k:o['data'].get('m_ClassName') for k,o in objects.items() if o['type']=='MonoScript'}
    gos={k:o['data'] for k,o in objects.items() if o['type']=='GameObject'}
    transforms={o['data']['m_GameObject']['m_PathID']:o['data'] for o in objects.values() if o['type']=='Transform'}
    components=[]
    for k,o in objects.items():
        if o['type']!='MonoBehaviour':continue
        d=o['data']; c=scripts.get(d.get('m_Script',{}).get('m_PathID'),'external'); counts[c]+=1
        go=d.get('m_GameObject',{}).get('m_PathID')
        components.append({'class':c,'gameObject':gos.get(go,{}).get('m_Name'),'transform':transforms.get(go),'data':d})
    lid=re.search('physiclevelscene_level(\d+)_',path.name)[1]
    record={'id':lid,'bundle':path.name,'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'components':components,'objects':objects}
    (OUT/f'{lid}.json').write_text(json.dumps(record,separators=(',',':')),encoding='utf-8')
    levels.append({k:v for k,v in record.items() if k!='objects'})
    if i%50==0: print('parsed',i+1,flush=True)
(OUT/'all_levels.json').write_text(json.dumps({'levels':levels},separators=(',',':')),encoding='utf-8')
(OUT/'summary.json').write_text(json.dumps({'count':len(levels),'classes':counts},indent=2),encoding='utf-8')
print(json.dumps({'count':len(levels),'classes':counts},indent=2))
