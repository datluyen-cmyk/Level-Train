"""Static source-pose query density flags; not a native overlap certificate."""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path

from shapely import affinity, STRtree
from assembly_contact_audit_v3 import shape_polygon


def read(path):
    return json.loads(path.read_text(encoding='utf8'))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--pack', type=Path, required=True)
    parser.add_argument('--templates', type=Path, required=True)
    parser.add_argument('--queries', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    templates = {t['id']: t for t in read(args.templates)['templates']}
    query_templates = {t['templateId']: t['queries'] for t in read(args.queries)['templates']}
    levels = read(args.pack)['levels']
    local = {}
    histogram = Counter()
    flags = []
    query_count = 0
    for level in levels:
        geometries, records, lookup = [], [], {}
        parts_by_id = {p['id']: p for p in level['parts']}
        for part in level['parts']:
            tid = part['templateId']
            if tid not in local:
                local[tid] = [shape_polygon(s) for s in templates[tid]['physicalShapes']]
            for index, (shape, geometry) in enumerate(zip(templates[tid]['physicalShapes'], local[tid])):
                if not shape['sourceEnabled']:
                    continue
                lookup[(part['id'], index)] = len(geometries)
                geometries.append(affinity.translate(affinity.rotate(geometry, part['angle'], origin=(0, 0)), part['x'], part['y']))
                records.append({'ownerId': part['id'], 'shapeIndex': index,
                                'layer': shape['sourceLayer'], 'trigger': shape['trigger']})
        for bomb_index, bomb in enumerate(level.get('bombs', [])):
            shape = bomb['physicalShape']
            if not shape['sourceEnabled']:
                continue
            geometry = shape_polygon(shape)
            owner = parts_by_id.get(bomb['ownerPartId'])
            if owner is not None:
                geometry = affinity.translate(affinity.rotate(geometry, owner['angle'], origin=(0, 0)), owner['x'], owner['y'])
            geometries.append(geometry)
            records.append({'ownerId': 'bomb:'+str(bomb_index), 'shapeIndex': -1,
                            'layer': shape['sourceLayer'], 'trigger': shape['trigger']})
        tree = STRtree(geometries)
        for part in level['parts']:
            for query in query_templates.get(part['templateId'], []):
                index = lookup.get((part['id'], query['shapeIndex']))
                if index is None:
                    continue
                filt = query['contactFilter']
                if filt.get('useDepth') or filt.get('useNormalAngle'):
                    raise ValueError('Unmodeled filter in static density diagnostic')
                mask = filt['layerMask']
                if isinstance(mask, dict):
                    mask = mask['m_Bits']
                hits = []
                for other_index in tree.query(geometries[index], predicate='intersects'):
                    other_index = int(other_index)
                    if other_index == index:
                        continue
                    other = records[other_index]
                    if filt.get('useLayerMask') and not (mask & (1 << other['layer'])):
                        continue
                    if not filt.get('useTriggers') and other['trigger']:
                        continue
                    # Positive-area intersections are deliberately used for a
                    # density flag; Unity can also report touching boundaries.
                    if geometries[index].intersection(geometries[other_index]).area > 1e-8:
                        hits.append(other)
                query_count += 1
                histogram[len(hits)] += 1
                if len(hits) >= 31:
                    flags.append({'sourceId': level['sourceId'], 'partId': part['id'],
                                  'shapeIndex': query['shapeIndex'],
                                  'queryOwner': query['queryOwner'],
                                  'positiveAreaHitCountExcludingSelf': len(hits), 'hits': hits})
    report = {'schemaVersion': 1, 'scope': 'STATIC_INITIAL_POSE_DENSITY_ONLY',
              'sourceCount': len(levels), 'queryInstanceCount': query_count,
              'positiveAreaHitCountHistogram': dict(sorted(histogram.items())),
              'flagThreshold': 31, 'flaggedQueryCount': len(flags), 'flags': flags,
              'limits': ['Does not execute native Physics2D or determine its buffer ordering.',
                         'Excludes self and boundary-only contacts; edge radii and circle tessellation differ.',
                         'Initial-pose density does not bound later motion-query density.',
                         'Native explicit caller buffers differ from maxOverlapCollider serialization.'],
              'hashes': {str(p): hashlib.sha256(p.read_bytes()).hexdigest()
                         for p in [args.pack, args.templates, args.queries, Path(__file__)]}}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2), encoding='utf8')
    print(json.dumps({k: report[k] for k in ('sourceCount', 'queryInstanceCount', 'positiveAreaHitCountHistogram', 'flaggedQueryCount')}))


if __name__ == '__main__':
    main()
