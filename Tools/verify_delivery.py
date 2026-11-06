"""Fail-closed delivery gate for PhysicsV2; v1 reports are never accepted."""
from pathlib import Path
import datetime,hashlib,json
ROOT=Path(__file__).resolve().parents[1];PROJECT=ROOT/'RotateRings Test';BUNDLE=PROJECT/'LevelTraining_PhysicsV2';BATCH=BUNDLE/'batches/final-300';RESEARCH=ROOT/'Research/physics_v2'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig')) if p.exists() else {}
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest() if p.exists() else None

def main():
    pack=read(BATCH/'generated_levels.json');batch=read(BATCH/'validation_report.json');training=read(BUNDLE/'model_validation.json');manifest=read(BUNDLE/'models/manifest.json')
    imported=PROJECT/'Assets/RotateRings/Resources/Levels/generated_levels.json';catalog=PROJECT/'Assets/RotateRings/Resources/Levels/physics_templates.json';runtime=read(imported)
    audit=read(BUNDLE/'audits/final_pack_audit.json');replay=read(RESEARCH/'runtime_qa_report.json');collision=read(RESEARCH/'unity_collision_qa.json');pins=read(RESEARCH/'pin_visual_regression.json');inputs=read(RESEARCH/'input_smoke_report.json');gui=read(BUNDLE/'audits/generator_gui_qa.json');visual=read(RESEARCH/'visual_qa_report.json');editor=read(RESEARCH/'final_editor_state.json')
    checks={
      'all500OriginalsInspectedAndRetained':training.get('originalCountInspected',0)>=500 and manifest.get('sourceCount',0)>=500,
      'physicalLabelsDisclosed':training.get('physicallySolvedAndReplayedOriginals',0)>0 and bool(training.get('difficulty')) and bool(training.get('split')),
      'complete300Accepted':pack.get('schemaVersion')==2 and pack.get('complete') is True and batch.get('complete') is True and len(pack.get('levels',[]))>=300 and batch.get('packSha256')==sha(BATCH/'generated_levels.json'),
      'immutableWholeSourcePiecesAndNovelty':audit.get('passed') is True and audit.get('packSha256')==sha(BATCH/'generated_levels.json'),
      'modelsAndInputsFrozen':bool(manifest.get('hashes')) and all(sha(BUNDLE/p)==h for p,h in manifest.get('hashes',{}).items()),
      'importedPhysicalPackAndCatalog':runtime.get('schemaVersion')==2 and len(runtime.get('levels',[]))>=300 and catalog.exists() and sha(imported)==sha(BATCH/'unity_levels.json'),
      'allUnityNativePhysicsReplays':replay.get('complete') is True and len(replay.get('levels',[]))>=300 and all(x.get('passed') for x in replay.get('levels',[])) and replay.get('packSha256')==sha(imported) and replay.get('catalogSha256')==sha(catalog) and replay.get('collisionWorldSha256')==sha(PROJECT/'Assets/RotateRings/Scripts/RotateRingsCollisionWorld.cs') and replay.get('replayCodeSha256')==sha(PROJECT/'Assets/RotateRings/Scripts/RotateRingsReplay.cs'),
      'solidStemAndHeadWallRegression':collision.get('passed') is True and {'stem','head_wall'}.issubset(collision.get('blockingRoles',[])) and collision.get('collisionWorldSha256')==sha(PROJECT/'Assets/RotateRings/Scripts/RotateRingsCollisionWorld.cs') and collision.get('qaHarnessSha256')==sha(PROJECT/'Assets/RotateRings/Editor/RotateRingsCollisionQA.cs'),
      'sourcePinVisibilityPersistence':pins.get('passed') is True,
      'actualRingInputUndoRestartHint':inputs.get('status')=='passed' and all(inputs.get(k) for k in ['dragUsedInputSystem','undoRestoredSnapshot','restartRestoredLevel','hintDidNotChangeGameState']),
      'savedGeneratorGenerationAndCancellation':gui.get('status') in ['pass','passed'] and gui.get('protectedUnchanged') is True,
      'liveVisualEvidence':visual.get('passed') is True and bool(visual.get('captures')),
      'editorReadyNoBuild':editor.get('playing') is False and editor.get('compiling') is False and editor.get('scene')=='Assets/Scenes/RotateRings.unity' and editor.get('consoleErrorCount')==0 and editor.get('colorSpace')=='Gamma' and editor.get('ringShaderError') is False and editor.get('fragmentShaderError') is False,
    }
    paths=[p for p in BUNDLE.rglob('*') if p.is_file() and p.suffix not in ['.pyc','.tmp','.log']]
    paths += [p for p in (PROJECT/'Assets/RotateRings').rglob('*') if p.is_file() and p.suffix in ['.cs','.shader','.json']]
    paths += [p for p in RESEARCH.glob('*.json') if p.is_file()]
    paths += [PROJECT/'README_PLAYTEST.md',PROJECT/'ProjectSettings/ProjectSettings.asset',PROJECT/'Assets/Scenes/RotateRings.unity',Path(__file__)]
    paths += [p for folder in ['Resources/OriginalUI','Resources/OriginalMotion','Original3D/Resources/Original/Textures','Resources/Audio'] for p in (PROJECT/'Assets/RotateRings'/folder).glob('*') if p.is_file() and p.suffix in ['.png','.ttf','.wav','.mp3','.ogg']]
    paths += [PROJECT/row['path'] for row in visual.get('captures',[]) if 'path' in row]
    output=BUNDLE/'delivery_validation.json';paths=[p for p in paths if p!=output]
    report={'passed':all(checks.values()),'verifiedUtc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'checks':checks,'buildCreated':False,'counts':{'originalsInspected':training.get('originalCountInspected',0),'physicallyLabeledOriginals':training.get('physicallySolvedAndReplayedOriginals',0),'newLevels':len(pack.get('levels',[])),'unityReplayPassed':sum(bool(x.get('passed')) for x in replay.get('levels',[]))},'limitations':['Conservative source-template ring subset; unsupported original mechanics excluded.','Computer score and novelty are heuristic; human difficulty and fun are unmeasured.','Source structures use all500; physical difficulty labels use only successfully replayed supported originals.'],'sha256':{str(p.relative_to(ROOT)):sha(p) for p in paths if p.exists()}}
    output.write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps({'passed':report['passed'],'checks':checks,'counts':report['counts']},indent=2));return 0 if report['passed'] else 1
if __name__=='__main__':raise SystemExit(main())
