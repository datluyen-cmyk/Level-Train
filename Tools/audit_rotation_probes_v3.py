"""Audit the reconstructed ring penetration probes against serialized native lists.

Static set equality only. This does not validate Physics2D transitions or gameplay.
"""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path


def read(path):
    return json.loads(path.read_text(encoding='utf8'))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def audit(pack, templates, query_catalog):
    rows = []
    failures = []
    roles = Counter()
    ring_count = 0
    for level in pack['levels']:
        for part in level['parts']:
            if part['kind'] != 'ring':
                continue
            ring_count += 1
            shapes = templates[part['templateId']]['physicalShapes']
            native = part['sourceNativeData']
            bindings = {int(b['sourceColliderId']): int(b['shapeIndex'])
                        for b in part['sourceColliderBindings']}
            expected = set()
            unresolved = []
            for ref in [native.get('activeCollider', {})] + native.get('hardBlockColliders', []):
                collider_id = int(ref.get('m_PathID', 0))
                if collider_id and collider_id not in bindings:
                    unresolved.append(collider_id)
                elif collider_id:
                    expected.add(bindings[collider_id])
            relative_count = 0
            for query in query_catalog['templateQueries'][part['templateId']]:
                observations = [o for o in query.get('sourceRelativeObservations', [])
                                if str(o['sourceId']) == str(level['sourceId'])
                                and o['partId'] == part['id']]
                for observation in observations:
                    expected.update(observation['blockShapeIndices'])
                    relative_count += 1
            # CollectSelfColliders.TryAddSelfCollider excludes trigger, disabled,
            # and layer 6. AppendHardBlockProbes adds the audited native stems.
            def eligible(index):
                shape = shapes[index]
                return (shape['sourceEnabled'] and not shape['trigger']
                        and shape['sourceLayer'] != 6)
            expected = {i for i in expected if eligible(i)}
            actual = {i for i in range(len(shapes)) if eligible(i)}
            for index in actual:
                roles[shapes[index]['role']] += 1
            row = {'sourceId': level['sourceId'], 'partId': part['id'],
                   'templateId': part['templateId'],
                   'expectedShapeIndices': sorted(expected),
                   'reconstructedShapeIndices': sorted(actual),
                   'relativeCount': relative_count,
                   'serializedRelativeCount': len(native.get('ringRelatives', [])),
                   'unresolvedColliderIds': unresolved,
                   'extraShapeIndices': sorted(actual - expected),
                   'missingShapeIndices': sorted(expected - actual)}
            row['passed'] = (not unresolved and actual == expected
                             and relative_count == row['serializedRelativeCount'])
            rows.append(row)
            if not row['passed']:
                failures.append(row)
    return {'schemaVersion': 1, 'passed': not failures,
            'sourceCount': len(pack['levels']), 'ringCount': ring_count,
            'nativeProbeRoleCounts': dict(roles),
            'failureCount': len(failures), 'failures': failures, 'rings': rows,
            'scope': 'Static serialized set equality for penetration/drag probes; no runtime path or solution certificate.',
            'sourceMethods': ['RingObject.RebuildRotationBlockCaches',
                              'RingObject.AppendHardBlockProbes',
                              'RingDragCollisionEventService.CollectSelfColliders',
                              'RingDragCollisionEventService.TryAddSelfCollider']}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--pack', type=Path, required=True)
    parser.add_argument('--templates', type=Path, required=True)
    parser.add_argument('--queries', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    templates = {t['id']: t for t in read(args.templates)['templates']}
    report = audit(read(args.pack), templates, read(args.queries))
    report['hashes'] = {str(p): sha(p) for p in
                        [args.pack, args.templates, args.queries, Path(__file__)]}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2), encoding='utf8')
    print(json.dumps({k: v for k, v in report.items()
                      if k not in ['rings', 'hashes', 'failures']}))
    if report['failures']:
        print(json.dumps(report['failures'][:5]))


if __name__ == '__main__':
    main()
