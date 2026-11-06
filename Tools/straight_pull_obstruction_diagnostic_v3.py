"""Static initial obstruction graph up to each straight Pull's own guide exit.

Restricted to convex straight Pulls with native guide bindings and no other
mechanics. Convex translation sweeps are exact planar sets for these bodies.
This is not a Physics2D simulation, legal path, release or difficulty certificate.
"""
import argparse
from collections import Counter
import hashlib
import json
import math
from pathlib import Path

from shapely import affinity, union_all
from assembly_contact_audit_v3 import shape_polygon


def read(path):
    return json.loads(Path(path).read_text(encoding='utf8'))


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def world(geometry, part):
    return affinity.translate(affinity.rotate(geometry, part['angle'],
                                               origin=(0, 0)), part['x'], part['y'])


def diagnose(level, templates):
    if level.get('links') or level.get('bombs'):
        raise ValueError('Diagnostic requires guide-only board without ring links or bombs')
    if any(p['kind'] not in ('slider', 'fixed') or p.get('chainLockCount', 0)
           or p.get('key') or p.get('lockKey') for p in level['parts']):
        raise ValueError('Unsupported part or state for static straight-Pull diagnostic')
    parts = {p['id']: p for p in level['parts']}
    shapes = {i: [world(shape_polygon(s), p)
                  for s in templates[p['templateId']]['physicalShapes']]
              for i, p in parts.items()}
    pulls = [p for p in parts.values() if p['kind'] == 'slider']
    solids = {}
    for part in pulls:
        template = templates[part['templateId']]
        if len(template['pullVisual']['pathPoints']) != 2:
            raise ValueError('Non-straight Pull is outside this diagnostic')
        solid = union_all([g for g, s in zip(shapes[part['id']], template['physicalShapes'])
                            if not s['trigger'] and s['sourceEnabled'] and s['sourceLayer'] == 7])
        if solid.is_empty or solid.convex_hull.area-solid.area > 1e-6:
            raise ValueError('Pull solid must be convex for an exact endpoint-hull sweep')
        solids[part['id']] = solid
    rows = []
    for part in pulls:
        pid = part['id']
        template = templates[part['templateId']]
        axis = template['movement']['axisLocal']
        angle = math.radians(part['angle'])
        dx = axis['x']*math.cos(angle)-axis['y']*math.sin(angle)
        dy = axis['x']*math.sin(angle)+axis['y']*math.cos(angle)
        norm = math.hypot(dx, dy)
        if norm < 1e-9:
            raise ValueError('Invalid native signed axis')
        dx, dy = dx/norm, dy/norm
        guide_links = [e for e in level['guideLinks'] if e['lockedOwnerId'] == pid]
        if not guide_links:
            raise ValueError('Pull without a serialized native guide')
        guides = union_all([shapes[e['guideOwnerId']][index]
                            for e in guide_links for index in e['guideShapeIndices']])
        query_indices = {b['shapeIndex'] for b in part['sourceColliderBindings']}
        query = union_all([shapes[pid][index] for index in query_indices])
        if abs(query.convex_hull.area-query.area) > 1e-6:
            raise ValueError('Convex guide query is required for monotonic exit bracketing')
        if not query.intersects(guides):
            raise ValueError('Initial guide overlap was not observed')
        for direction in (-1, 1):
            def moved(geometry, amount):
                return affinity.translate(geometry, dx*direction*amount, dy*direction*amount)
            low, high = 0., max(.01, template.get('width', .5))
            while moved(query, high).intersects(guides):
                high *= 2
                if high > 10000:
                    raise ValueError('Could not bracket the geometric guide exit')
            for _ in range(28):
                middle = (low+high)/2
                if moved(query, middle).intersects(guides):
                    low = middle
                else:
                    high = middle
            exit_distance = high
            # With multiple disconnected guide regions, a later overlap can
            # reappear; stop rather than assuming monotonicity in that case.
            if any(moved(query, exit_distance*(1+i/8)).intersects(guides)
                   for i in range(1, 17)):
                raise ValueError('Nonmonotonic guide query is outside diagnostic scope')
            sweep = union_all([solids[pid], moved(solids[pid], exit_distance)]).convex_hull
            blockers = []
            for target in pulls:
                target_id = target['id']
                if target_id == pid:
                    continue
                obstacle = solids[target_id]
                if sweep.intersection(obstacle).area <= 1e-7:
                    continue
                lower, upper = 0., exit_distance
                if solids[pid].intersection(obstacle).area <= 1e-7:
                    for _ in range(24):
                        middle = (lower+upper)/2
                        partial = union_all([solids[pid], moved(solids[pid], middle)]).convex_hull
                        if partial.intersection(obstacle).area > 1e-7:
                            upper = middle
                        else:
                            lower = middle
                else:
                    upper = 0.
                blockers.append({'targetId': target_id, 'firstEntryDistance': upper,
                                 'entryFractionOfOwnGuideExit': upper/exit_distance})
            blockers.sort(key=lambda b: (b['firstEntryDistance'], b['targetId']))
            rows.append({'sourceId': pid, 'direction': direction,
                         'geometricOwnGuideExitDistance': exit_distance,
                         'potentialPullObstaclesBeforeGuideExit': blockers})
    return {'levelId': level['id'], 'referenceSourceId': level.get('sourceId'),
            'pullCount': len(pulls), 'directions': rows,
            'edgeCount': sum(len(r['potentialPullObstaclesBeforeGuideExit']) for r in rows)}


def edges(report):
    return {(r['sourceId'], b['targetId'], r['direction'])
            for r in report['directions'] for b in r['potentialPullObstaclesBeforeGuideExit']}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--pack', type=Path, required=True)
    parser.add_argument('--templates', type=Path, required=True)
    parser.add_argument('--source-id', required=True)
    parser.add_argument('--candidate-pack', type=Path)
    parser.add_argument('--candidate-id', type=int)
    parser.add_argument('--interior-ids', type=int, nargs='*', default=[])
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    templates = {t['id']: t for t in read(args.templates)['templates']}
    source = next(l for l in read(args.pack)['levels'] if str(l['sourceId']) == args.source_id)
    report = {'schemaVersion': 1, 'source': diagnose(source, templates),
              'scope': 'STATIC_INITIAL_GEOMETRY_ONLY_NOT_NATIVE_REPLAY',
              'limits': ['All other Pulls remain fixed at their initial poses.',
                         'Geometric guide separation is a bound, not an observed native release event.',
                         'No native skins, cast filters, initial-overlap rollback or frame integration.',
                         'Edges are potential geometric obstructions, not immutable locks or removal-order labels.'],
              'hashes': {str(p): sha(p) for p in [args.pack, args.templates, Path(__file__)]}}
    if args.candidate_pack:
        candidate = next(l for l in read(args.candidate_pack)['levels'] if l['id'] == args.candidate_id)
        report['candidate'] = diagnose(candidate, templates)
        original, changed = edges(report['source']), edges(report['candidate'])
        interior = set(args.interior_ids)
        def annotate(edge):
            return {'sourceId': edge[0], 'targetId': edge[1], 'direction': edge[2],
                    'interiorInvolved': edge[0] in interior or edge[1] in interior}
        report['comparison'] = {'addedEdges': [annotate(e) for e in sorted(changed-original)],
                                'removedEdges': [annotate(e) for e in sorted(original-changed)],
                                'interiorRootIdsFromVisualAudit': sorted(interior),
                                'interiorRoleStatus': 'MANUAL_VISUAL_ANNOTATION_NOT_GAME_METADATA'}
        report['hashes'][str(args.candidate_pack)] = sha(args.candidate_pack)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2), encoding='utf8')
    print(json.dumps({'sourceEdges': report['source']['edgeCount'],
                      'candidateEdges': report.get('candidate', {}).get('edgeCount'),
                      'added': len(report.get('comparison', {}).get('addedEdges', [])),
                      'removed': len(report.get('comparison', {}).get('removedEdges', []))}))


if __name__ == '__main__':
    main()
