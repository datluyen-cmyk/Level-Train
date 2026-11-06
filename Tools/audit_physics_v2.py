"""Independent delivery audit: immutable original pieces and accepted v2 batch only."""
from pathlib import Path
import collections,hashlib,importlib.util,json,sys
ROOT=Path(__file__).resolve().parents[1]
BUNDLE=ROOT/'RotateRings Test/LevelTraining_PhysicsV2'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
    batch=Path(sys.argv[1]) if len(sys.argv)>1 else BUNDLE/'batches/final-300'
    pack=read(batch/'generated_levels.json');catalog={t['id']:t for t in read(BUNDLE/'templates.json')['templates']}
    original=read(BUNDLE/'corpus.json')['levels'];refs={str(l['id']) for l in original}
    adapter_path=BUNDLE/'adapters/rotate_rings_physics_v2.py'
    spec=importlib.util.spec_from_file_location('v2_audit',adapter_path);a=importlib.util.module_from_spec(spec);spec.loader.exec_module(a)
    errors=[];parts=0;pins=0;used=set();seen=set();sourcefp={a.fingerprint(l) for l in original};source_descriptors=[a.descriptor(l) for l in original];prior_descriptors=[]
    def counter_distance(x,y):
        keys=x.keys()|y.keys();return 1-sum(min(x[k],y[k]) for k in keys)/max(1,sum(max(x[k],y[k]) for k in keys))
    if not pack.get('complete') or pack.get('schemaVersion')!=2:errors.append('incomplete or nonphysical pack')
    if len(pack['levels'])<300:errors.append('fewer than 300 accepted levels')
    if len(refs)<500:errors.append('fewer than 500 distinct original levels')
    minima=[]
    for level in pack['levels']:
        state=a.state(level)
        for piece in state['parts']:
            parts+=1;tid=piece.get('templateId');template=catalog.get(tid);used.add(tid)
            if template is None:errors.append([level['id'],piece['id'],'unknown template']);continue
            for key,value in template.items():
                if key in ['id','sourceRefs']:continue
                if piece.get(key)!=value:errors.append([level['id'],piece['id'],'mutated template field',key])
            pins+=len(piece.get('attachments',[]))
            if any(t.get('isInnerMount') or t.get('type')!=1 for t in piece.get('attachments',[])):errors.append([level['id'],'excluded attachment type'])
        fp=a.fingerprint(level)
        if fp in sourcefp or fp in seen:errors.append([level['id'],'canonical equivalent'])
        seen.add(fp)
        description=a.descriptor(level)
        distances=[counter_distance(description,x) for x in source_descriptors]
        minimum=min(distances);minima.append(minimum)
        if minimum<.1:errors.append([level['id'],'source near copy',minimum])
        for priorid,previous in prior_descriptors:
            if counter_distance(description,previous)<.1:errors.append([level['id'],'output near copy',priorid])
        prior_descriptors.append((level['id'],description))
        if not level.get('acceptance',{}).get('physicalReplay'):errors.append([level['id'],'missing physical replay gate'])
    report={'passed':not errors,'originalCount':len(original),'distinctOriginalCount':len(refs),'newLevels':len(pack['levels']),'wholeOriginalPieceInstances':parts,'originalAttachmentInstances':pins,'usedTemplateCount':len(used),'minimumOriginalDistance':min(minima,default=None),'templateMutationErrors':errors,'packSha256':sha(batch/'generated_levels.json'),'catalogSha256':sha(BUNDLE/'templates.json'),'adapterSha256':sha(adapter_path),'scope':'Whole immutable source pieces, conservative ring subset. Human difficulty and fun unmeasured.'}
    out=BUNDLE/'audits/final_pack_audit.json';out.parent.mkdir(exist_ok=True);out.write_text(json.dumps(report,indent=2),encoding='utf8')
    print(json.dumps(report,indent=2));return 0 if report['passed'] else 1
if __name__=='__main__':raise SystemExit(main())
