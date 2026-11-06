"""Screen feature-constructed rewrites without granting gameplay acceptance."""
from pathlib import Path
from collections import Counter
import argparse,copy,json,math
import numpy as np
from layout_learning_v3 import read,hashfile
from global_composition_v3 import Geometry
from composition_field_v3 import posed_field,native_frame,stroke_heading
from layout_novelty_v3 import LayoutNoveltyIndex

def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',required=True,type=Path);p.add_argument('--pack',required=True,type=Path);p.add_argument('--geometry-report',required=True,type=Path);a=p.parse_args();w=a.workspace.resolve();levels=read(w/'data/source500_native.json')['levels'];templates={t['id']:t for t in read(w/'data/source_templates.json')['templates']};sources={l['sourceId']:l for l in levels};extras=read(w/'data/novelty_extras/source_parity_pack.json')['levels'];nt=dict(templates)
 for t in read(w/'data/novelty_extras/source_templates.json')['templates']:nt[t['id']]=t
 novelty=LayoutNoveltyIndex(levels+extras,nt);geo=Geometry(templates);geometry={l['id']:l for l in read(a.geometry_report)['levels']};pack=read(a.pack);rows=[];passed=[];rejection=Counter()
 for candidate in pack['levels']:
  raw=geometry[candidate['id']];reasons=[];decision=candidate['assemblyProvenance']['decisions'][-1];ref=sources[candidate['assemblyProvenance']['tileImports'][0]['sourceId']];ps={p['id']:p for p in ref['parts']};removed=decision['removedFrameworkSourceRoots'];imp=candidate['assemblyProvenance']['featureBoundaryImports'][0];newids=set(imp['interiorPartMapping'].values());new=[p for p in candidate['parts'] if p['id'] in newids];old=[ps[i] for i in removed];oldc=Counter(p['kind'] for p in old);newc=Counter(p['kind'] for p in new)
  if not raw['integrityPassed']:reasons.append('nativeIntegrity')
  if not raw.get('rawSeam',{}).get('rawGeometrySeamPassed',False):reasons.append('nativeRawGeometrySeam')
  if not math.ceil(len(old)*.75)<=len(new)<=math.floor(len(old)*1.25):reasons.append('localRootScale')
  if any(oldc[k]>0 and newc[k]==0 for k in ['slider','fixed','butterfly']):reasons.append('removedNativeMechanicFamily')
  if sum(abs(oldc[k]-newc[k]) for k in oldc|newc)/max(1,len(old))>.5:reasons.append('localMechanicMix')
  heading=stroke_heading(ref);low,high,span,center,matrix=native_frame(ref,templates,heading);ol,oh,_,_,_=native_frame({'parts':old},templates,heading);nl,nh,_,_,_=native_frame({'parts':new},templates,heading);ratios=(nh-nl)/np.maximum(oh-ol,.01)
  if any(ratios<.75) or any(ratios>1.25):reasons.append('localNativeExtent')
  oldarea=sum(geo.body(p).area for p in old);newarea=sum(geo.body(p).area for p in new);coverage=newarea/max(oldarea,.001)
  if oldarea>.001 and not .75<=coverage<=1.25:reasons.append('localNativeStrokeCoverage')
  oldids=set(removed);oldedges=sum(e['source'] in oldids and e['target'] in oldids for e in ref['links'])+sum(e['guideOwnerId'] in oldids and e['lockedOwnerId'] in oldids for e in ref['guideLinks']);newedges=sum(e['source'] in newids and e['target'] in newids for e in candidate['links'])+sum(e['guideOwnerId'] in newids and e['lockedOwnerId'] in newids for e in candidate['guideLinks'])
  if oldedges and not .75<=newedges/oldedges<=1.25:reasons.append('localAuthoredRelationDensity')
  row={'id':candidate['id'],'position':candidate['position'],'rejections':reasons,'removedRoots':len(old),'newRoots':len(new),'nativeExtentRatios':ratios.tolist(),'nativeStrokeCoverageRatio':coverage,'oldLocalAuthoredEdges':oldedges,'newLocalAuthoredEdges':newedges}
  if not reasons:
   offset=center@matrix.T;arrays=[]
   for l in [ref,candidate]:
    centered=copy.deepcopy(l)
    for q in centered['parts']:q['x']-=float(offset[0]);q['y']-=float(offset[1])
    arrays.append(posed_field(centered,heading,span,len(ref['parts']),templates)[0])
   delta=float(np.abs(arrays[0]-arrays[1]).sum());row['sourceWholeFieldDelta']=delta
   if delta>2*len(old)/len(ref['parts']):reasons.append('wholeCompositionFieldDelta')
   else:
    result=novelty.check(candidate);row['sourceNovelty507']=result
    if not result['passed']:reasons.append('wholeLayoutNearCopy507')
  row['allPreliminaryGatesPassed']=not reasons;rejection.update(reasons);rows.append(row)
  if not reasons:passed.append(candidate)
 report={'complete':False,'acceptedCount':0,'constructed':len(rows),'allPreliminaryGatesPassed':len(passed),'rejectionCounts':dict(rejection),'limits':['Every survivor still requires visual review, native source replay, source-relative difficulty calibration and independent candidate replay.','Local bounds preserve native coverage and source mechanic mix, not human fun.'],'levels':rows,'hashes':{'pack':hashfile(a.pack),'geometryReport':hashfile(a.geometry_report),'tool':hashfile(Path(__file__))}};(a.pack.parent/'feature_mechanics_novelty507.json').write_text(json.dumps(report,indent=2),encoding='utf8');(a.pack.parent/'preliminary_survivors.json').write_text(json.dumps({'schemaVersion':3,'complete':False,'acceptedCount':0,'status':'PRELIMINARY_ONLY_NATIVE_REPLAY_DIFFICULTY_VISUAL_PENDING','levels':passed},separators=(',',':')),encoding='utf8');print(json.dumps({k:report[k] for k in ['constructed','allPreliminaryGatesPassed','rejectionCounts']},indent=2))
if __name__=='__main__':main()
