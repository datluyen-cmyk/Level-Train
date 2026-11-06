"""Hash-bound incomplete pilot audits against every compatible native source."""
from pathlib import Path
import argparse,json,hashlib,collections
from layout_novelty_v3 import LayoutNoveltyIndex,describe,canonical_fingerprint

def read(p):return json.loads(p.read_text(encoding='utf8'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',type=Path,required=True);p.add_argument('--pack',type=Path,required=True);args=p.parse_args();w=args.workspace.resolve();pp=args.pack.resolve();paths=[w/'data/source500_native.json',w/'data/source_templates.json',w/'data/novelty_extras/source_parity_pack.json',w/'data/novelty_extras/source_templates.json'];levels=read(paths[0])['levels'];templates={t['id']:t for t in read(paths[1])['templates']};extras=read(paths[2])['levels'];levels+=extras
 for template in read(paths[3])['templates']:
  if template['id'] in templates and template!=templates[template['id']]:raise ValueError('Native template ID collision: '+template['id'])
  templates[template['id']]=template
 source_index=LayoutNoveltyIndex(levels,templates);earlier=LayoutNoveltyIndex([],templates);pack=read(pp);rows=[]
 for level in pack['levels']:
  source=source_index.check(level);batch=earlier.check(level);earlier.records.append(describe(level,templates));rows.append({'id':level['id'],'position':level['position'],'canonicalFingerprint':canonical_fingerprint(level,templates),'sourceNovelty':source,'earlierPilotNovelty':batch,'rawNativeGeometrySeamPassed':level.get('rawGeometrySeamAudit',{}).get('rawGeometrySeamPassed',False),'referenceScaleSatisfied':level['assemblyProvenance'].get('referenceScaleSatisfied',False),'sourceFamilies':level['assemblyProvenance']['sourceFamilyCount'],'proposalRanking':level['proposalRanking']});print('AUDIT',level['position'],'sourceNovel',source['passed'],'earlierNovel',batch['passed'],flush=True)
 report={'complete':False,'acceptedCount':0,'status':'STRUCTURED_PROPOSAL_ONLY_NATIVE_REPLAY_AND_DIFFICULTY_PENDING','trainingSourceCount':500,'nativeNoveltyExclusionSourceCount':len(levels),'extrasUsedForTraining':False,'sourceReferenceProgression':'pilot positions as recorded; finalnew300musttargetsource1–300','summary':{'proposals':len(rows),'rawGeometrySeamPassed':sum(r['rawNativeGeometrySeamPassed'] for r in rows),'sourceNoveltyPassed':sum(r['sourceNovelty']['passed'] for r in rows),'earlierPilotNoveltyPassed':sum(r['earlierPilotNovelty']['passed'] for r in rows),'referenceScaleSatisfied':sum(r['referenceScaleSatisfied'] for r in rows)},'results':rows,'limits':['Raw geometry audit is a conservative imported-source-pair screen, not Unity contact/cast/release parity.','Canonical and whole-layout alignment novelty are heuristic, not a proof of originality.','No verified source solutions or human difficulty labels.','No pilot is exported to playable accepted content.'],'hashes':{'proposalPack':sha(pp),'auditor':sha(Path(__file__)),'noveltyTool':sha(Path(__file__).with_name('layout_novelty_v3.py')),**{str(path.relative_to(w)).replace('\\','/'):sha(path) for path in paths}}};out=pp.parent/'preliminary_audit507.json';out.write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps(report['summary'],indent=2))
if __name__=='__main__':main()
