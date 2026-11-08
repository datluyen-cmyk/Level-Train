"""Audit real generated snapshots; never certify solvability or stage a level."""
import argparse
import json
from pathlib import Path

from cohesive_assembly_v11 import CohesiveDesignPolicy


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--workspace', type=Path, required=True)
    parser.add_argument('--packs', nargs='+', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--expect-rejected', nargs='*', type=int, default=[])
    parser.add_argument('--expect-passed', nargs='*', type=int, default=[])
    args = parser.parse_args()
    read = lambda path: json.loads(path.read_text(encoding='utf-8-sig'))
    sources = {l['position']: l for l in read(args.workspace / 'data/source500_native.json')['levels']}
    templates = {t['id']: t for t in read(args.workspace / 'data/source_templates.json')['templates']}
    rows = []
    for pack in args.packs:
        for level in read(pack)['levels']:
            audit = CohesiveDesignPolicy(sources[level['position']], templates).audit(level)
            rows.append({'pack': str(pack.resolve()), 'candidateId': level['id'],
                         'position': level['position'], **audit})
    failures = []
    for position in args.expect_rejected:
        found = [r for r in rows if r['position'] == position]
        if not found or any(r['passed'] for r in found):
            failures.append(f'Expected every snapshot at {position} to be rejected')
    for position in args.expect_passed:
        if not any(r['position'] == position and r['passed'] for r in rows):
            failures.append(f'Expected an eligible snapshot at {position}')
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps({'rows': rows, 'regressionFailures': failures,
                                      'meaning': 'Static composition/provenance screening only; native solution and human design quality unverified.'},
                                     indent=2), encoding='utf-8')
    print(json.dumps({'audited': len(rows), 'passed': sum(r['passed'] for r in rows),
                      'regressionFailures': failures, 'output': str(args.output.resolve())}))
    if failures:
        raise SystemExit(1)


if __name__ == '__main__':
    main()
