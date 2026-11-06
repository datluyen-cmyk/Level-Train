"""Refresh the saved V3 learning checkpoint without opening generation gates."""
from pathlib import Path
import argparse,hashlib,json

def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',type=Path,required=True);args=p.parse_args();w=args.workspace.resolve();manifestpath=w/'models/composition_manifest.json';manifest=json.loads(manifestpath.read_text(encoding='utf8'))
 config=json.loads((w/'level_training.json').read_text(encoding='utf8'))
 source=json.loads((w/config['nativeSourcePack']).read_text(encoding='utf8'))['levels']
 families=json.loads((w/config['familySplits']).read_text(encoding='utf8'))
 manifest.update(phase='Soft source-relative composition research; no accepted generation; current native replay/difficulty calibration pending',acceptedGeneratedCount=0,sourceCount=len(source),sourceFamilyCount=len(set(families['families'].values())),targetGeneratedProgression={'count':config['acceptance']['requestedCount'],'referencePositions':config['acceptance']['targetReferencePositions'],'learnedReferencePositions':[min(l['position'] for l in source),max(l['position'] for l in source)]},learnedStages={'local':'models/composition_pose.joblib','globalStatistics':'models/global_composition.joblib','nativeMacro':'models/macro_assembly.joblib','actualMacroContinuation':'models/macro_pattern.joblib','wholeBoardGeometryField':'models/whole_board_field.joblib','physicalCompositionMacro':'models/macro_physical_assembly.joblib','physicalCompositionContinuation':'models/macro_physical_pattern.joblib'},importantLimits=['Synthetic style scores and learned field reconstruction do not measure human difficulty or fun.','Raw geometry seam checks do not replace Unity skin/casts/binding initialization or independent native replay.','All pilot batches remain incomplete and acceptedCount0.','Tutorials with tiny state spaces can fail novelty; never pad their scale to force a multi-family assembly.','Content statistics are soft source-relative features under the revised user policy, not fixed design gates.'])
 if config.get('activeResearchRun'):
  active=w/config['activeResearchRun'];curated=json.loads((active/'data/ordinary_source_filter_v4.json').read_text(encoding='utf8'));refit=json.loads((active/'models/ordinary_refit_manifest_v4.json').read_text(encoding='utf8'));split=json.loads((active/'models/family_splits.json').read_text(encoding='utf8'))
  manifest['historicalMixedCorpusCount']=len(source)
  manifest['activeResearchRun']=config['activeResearchRun']
  manifest['sourceCount']=curated['actualUniqueOrdinaryCount']
  manifest['sourceFamilyCount']=len(set(split['families'].values()))
  manifest['requestedMinimumTrainingSourcesUnmet']=curated['requestedMinimum500_unmet']
  manifest['pictorialExcludedCount']=len(curated['excludedSourceIds'])
  manifest['activeRefitCertificateSha256']=sha(active/'models/ordinary_refit_manifest_v4.json')
  manifest['learnedStages']={name:config['activeResearchRun']+'/models/'+filename for name,filename in [('local','composition_pose.joblib'),('nativeMacro','macro_assembly.joblib'),('anchoredGuide','guide_anchor_role_motifs.joblib')]}
  manifest['targetGeneratedProgression']={'count':config['acceptance']['requestedCount'],'referenceSourceIds':curated['trainingSourceIds'],'referenceOriginalPositions':curated['trainingPositions'],'meaning':'First eligible ordinary originals in native progression order; excluded pictorial positions skipped'}
 # Hashing files neither runs tests nor recertifies old verification counters.
 # Keep historical fields intact; current evidence lives in hash-bound reports.
 manifest['verificationRefreshNotice']='No tests run by checkpointing. Retained verification fields are historical; each report specifies its tested inputs and rules hash.'
 # Models and reports are bound to their current artifact bytes. The report's
 # own hashes remain the record of which exact inputs produced its metrics.
 paths=[]
 for dirname in ['data','models','tools','adapters','reports']:
  paths.extend(p for p in (w/dirname).rglob('*') if p.is_file() and p!=manifestpath and p.suffix in ['.json','.py','.joblib','.md'])
 # Bind only stable run artifacts. Live baseline checkpoints/batches have their
 # own input/rules hashes and must not be mistaken for a frozen full-run result.
 if config.get('activeResearchRun'):
  for dirname in ['data','models','tools']:
   paths.extend(p for p in (active/dirname).rglob('*') if p.is_file() and p.suffix in ['.json','.py','.joblib','.md'])
 paths.extend(w/name for name in ['README_TRAINING.md','LEVEL_DESIGN_CONSTRAINTS.md','level_training.json','references.json','corpus.json'] if (w/name).exists());manifest['hashes']={str(p.relative_to(w)).replace('\\','/'):sha(p) for p in sorted(paths)};manifestpath.write_text(json.dumps(manifest,indent=2),encoding='utf8');print(json.dumps({'manifest':str(manifestpath),'hashedArtifacts':len(paths),'acceptedGeneratedCount':0,'generationEnabled':config['generationEnabled']}))
if __name__=='__main__':main()
