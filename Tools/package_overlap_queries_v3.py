"""Package verified source query contracts without changing source geometry."""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
report_paths = [ROOT / 'Research/physics_v3/source_overlap_handler_query_catalog_v4.json',
                ROOT / 'Research/physics_v3/source500_pull_overlap_query_catalog_v3.json']
by_template = {}
sources = []
for report_path in report_paths:
    report = json.loads(report_path.read_text(encoding='utf8'))
    assert report['safeForRuntimeImport'] and not report['conflicts'] and not report['unresolvedReferences']
    assert report['sourceCount'] == 500
    sources.append({'path': report_path.name, 'sha256': hashlib.sha256(report_path.read_bytes()).hexdigest()})
    for tid, rows in report['templateQueries'].items():
        queries = []
        for row in rows:
            assert not row['conflict'] and len(row['contractVariants']) == 1
            queries.append({'shapeIndex': row['shapeIndex'], **row['contractVariants'][0]})
        assert tid not in by_template, f'Conflicting Ring/Pull template: {tid}'
        by_template[tid] = queries
templates = [{'templateId': tid, 'queries': queries} for tid, queries in sorted(by_template.items())]
payload = {'schemaVersion': 2, 'sourceCount': 500, 'sourceCatalogs': sources, 'templates': templates}
raw = json.dumps(payload, separators=(',', ':')).encode('utf8')
for path in [ROOT / 'RotateRings Test/Assets/RotateRings/Resources/SourceOverlapQueries.json',
             ROOT / 'RotateRings Test/LevelTraining_SourceLayoutV3/data/source_overlap_queries.json']:
    path.write_bytes(raw)
print(json.dumps({'templates': len(templates), 'queries': sum(len(t['queries']) for t in templates),
                  'runtimeSha256': hashlib.sha256(raw).hexdigest()}))
