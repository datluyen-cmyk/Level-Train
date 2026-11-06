"""Export a human-reviewable index without changing accepted levels or models."""
from pathlib import Path
import csv
import json
import statistics

root = Path(__file__).resolve().parents[1] / 'RotateRings Test' / 'LevelTraining'
levels = json.loads((root / 'batches/final-300/generated_levels.json').read_text(encoding='utf8'))['levels']
with (root / 'Level Design Index.csv').open('w', encoding='utf-8-sig', newline='') as stream:
    writer = csv.writer(stream)
    writer.writerow(['Level', 'Reference ID', 'Pieces', 'Rings', 'Sliders', 'Links', 'Keys', 'Locks', 'Saved gestures', 'Computer score', 'Target score', 'Relative difference', 'Target evidence', 'Nearest source distance'])
    for level in levels:
        parts = level['state']['parts']
        a = level['acceptance']
        writer.writerow([level['position'], level['referenceId'], len(parts), sum(p['kind']=='ring' for p in parts), sum(p['kind']=='slider' for p in parts), len(level['state']['links']), sum(bool(p['key']) for p in parts), sum(bool(p['lockKey']) for p in parts), len(level['solution']), a['score'], a['targetScore'], a['relativeDifference'], a['targetMeaning'], a['nearestDistance']])
summary = {
    'count': len(levels),
    'piecesMin': min(len(x['state']['parts']) for x in levels),
    'piecesMax': max(len(x['state']['parts']) for x in levels),
    'stagesWithSliders': sum(any(p['kind']=='slider' for p in x['state']['parts']) for x in levels),
    'stagesWithKeys': sum(any(p['key'] for p in x['state']['parts']) for x in levels),
    'firstSliderStage': next(x['position'] for x in levels if any(p['kind']=='slider' for p in x['state']['parts'])),
    'firstKeyStage': next(x['position'] for x in levels if any(p['key'] for p in x['state']['parts'])),
    'meanComputerScoreRelativeDifference': statistics.mean(x['acceptance']['relativeDifference'] for x in levels),
    'maxComputerScoreRelativeDifference': max(x['acceptance']['relativeDifference'] for x in levels),
    'measuredReducedReferenceTargets': sum(x['acceptance']['targetMeaning']=='reduced reconstruction' for x in levels),
    'estimatedReferenceTargets': sum(x['acceptance']['targetMeaning']!='reduced reconstruction' for x in levels),
    'humanDifficultyAndFun': 'unmeasured',
}
(root / 'design_summary.json').write_text(json.dumps(summary, indent=2), encoding='utf8')
playtest = root / 'Playtest Log.csv'
if not playtest.exists():
    with playtest.open('w', encoding='utf-8-sig', newline='') as stream:
        writer = csv.writer(stream)
        writer.writerow(['Level', 'Player', 'Completed', 'Time seconds', 'Attempts', 'Hints used', 'Fun 1-5', 'Difficulty 1-5', 'Input or visual issues', 'Notes'])
        writer.writerows([[x['position']] + [''] * 9 for x in levels])
print(json.dumps(summary, indent=2))
