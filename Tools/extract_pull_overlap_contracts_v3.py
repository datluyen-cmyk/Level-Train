"""Extract native PullObject overlap-query contracts per source template.

Reads the source500 parity pack and normalized Unity level object trees. This is
a static extraction only: it does not invoke Unity or modify source data.
"""
from pathlib import Path
from collections import defaultdict
import argparse
import hashlib
import json

FILTER_FIELDS = (
    'useTriggers', 'useLayerMask', 'layerMask', 'useDepth', 'useOutsideDepth',
    'minDepth', 'maxDepth', 'useNormalAngle', 'useOutsideNormalAngle',
    'minNormalAngle', 'maxNormalAngle',
)
PART_ID_FIELD = '<PhysicPartId>k__BackingField'


def read(path):
    return json.loads(Path(path).read_text(encoding='utf-8'))


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def path_id(value):
    return int(value.get('m_PathID', 0)) if isinstance(value, dict) else 0


def filter_contract(handler):
    filt = (handler or {}).get('contactFilter', {})
    return {key: filt.get(key) for key in FILTER_FIELDS}


def pull_components_for_part(raw, source_part_id):
    """Find PullObject MonoBehaviour object records by serialized part ID."""
    pull_game_objects = {
        path_id(component.get('data', {}).get('m_GameObject'))
        for component in raw.get('components', [])
        if component.get('class') == 'PullObject'
        and (component.get('data', {}).get('partData') or {}).get(PART_ID_FIELD) == source_part_id
    }
    matches = []
    for component_id, entry in raw.get('objects', {}).items():
        if entry.get('type') != 'MonoBehaviour':
            continue
        data = entry.get('data', {})
        part_data = data.get('partData') or {}
        if (path_id(data.get('m_GameObject')) in pull_game_objects
                and part_data.get(PART_ID_FIELD) == source_part_id
                and 'overlap2DHandler' in data):
            matches.append((int(component_id), data))
    return matches


def part_queries(level, part, raw):
    source_part_id = part.get('sourceId')
    bindings = {int(b['sourceColliderId']): b
                for b in part.get('sourceColliderBindings', [])}
    queries = []
    unresolved = []
    if not source_part_id:
        return queries, [{'partId': part.get('id'), 'reason': 'PullObject part has no sourceId'}]

    components = pull_components_for_part(raw, source_part_id)
    if len(components) != 1:
        return queries, [{'partId': part.get('id'), 'sourcePartId': source_part_id,
                          'matchingPullComponentCount': len(components),
                          'reason': 'expected exactly one PullObject component resolved by serialized PhysicPartId'}]

    original_component_id, data = components[0]
    handler = data.get('overlap2DHandler') or {}
    collider_id = path_id(handler.get('collider2D'))
    binding = bindings.get(collider_id)
    if not binding:
        unresolved.append({'partId': part.get('id'), 'sourcePartId': source_part_id,
                           'originalComponentId': original_component_id,
                           'sourceColliderId': collider_id,
                           'reason': 'PullObject overlap handler collider did not map to an exported native shape'})
        return queries, unresolved

    queries.append({
        'queryOwner': 'PullObject',
        'originalComponentId': original_component_id,
        'gameObjectPathId': path_id(data.get('m_GameObject')),
        'sourceFeatureId': binding.get('sourceFeatureId'),
        'shapeIndex': int(binding['shapeIndex']),
        'sourceRole': binding.get('sourceRole'),
        'contactFilter': filter_contract(handler),
        'maxOverlapCollider': handler.get('maxOverlapCollider'),
        'sourceColliderId': collider_id,
    })
    return queries, unresolved


def main():
    root = Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser()
    parser.add_argument('--source-pack', type=Path,
                        default=root / 'RotateRings Test/LevelTraining_SourceLayoutV3/data/source500_native.json')
    parser.add_argument('--normalized-levels', type=Path, default=root / 'Research/levels')
    parser.add_argument('--output', type=Path,
                        default=root / 'Research/physics_v3/source500_pull_overlap_query_catalog_v3.json')
    parser.add_argument('--source-ids', nargs='*')
    args = parser.parse_args()

    pack = read(args.source_pack)
    selected = [level for level in pack['levels']
                if not args.source_ids or str(level['sourceId']) in set(args.source_ids)]
    contracts = defaultdict(dict)
    observations = defaultdict(list)
    unresolved = []
    pull_count = 0
    source_samples = []
    raw_hashes = {}

    for index, level in enumerate(selected):
        raw_path = args.normalized_levels / Path(level['provenance']['rawPath']).name
        if not raw_path.is_file():
            unresolved.append({'sourceId': str(level['sourceId']), 'rawPath': str(raw_path),
                               'reason': 'normalized raw source level file does not exist'})
            continue
        raw = read(raw_path)
        raw_hashes[str(raw_path)] = sha(raw_path)
        source_queries = []
        for part in level.get('parts', []):
            if part.get('sourceClass') != 'PullObject':
                continue
            pull_count += 1
            queries, missing = part_queries(level, part, raw)
            unresolved.extend({'sourceId': str(level['sourceId']), **item} for item in missing)
            source_queries.append({'partId': part['id'], 'sourceId': part.get('sourceId'),
                                   'templateId': part['templateId'], 'queries': queries})
            for query in queries:
                key = (str(part['templateId']), int(query['shapeIndex']))
                identity = {field: query[field] for field in
                            ('queryOwner', 'sourceRole', 'contactFilter', 'maxOverlapCollider')}
                signature = json.dumps(identity, sort_keys=True, separators=(',', ':'))
                contracts[key][signature] = identity
                observations[key].append({
                    'sourceId': str(level['sourceId']), 'partId': part['id'],
                    'sourcePartId': part.get('sourceId'),
                    'sourceFeatureId': query['sourceFeatureId'],
                    'sourceColliderId': query['sourceColliderId'],
                    'originalComponentId': query['originalComponentId'],
                    'gameObjectPathId': query['gameObjectPathId'],
                    'rawPath': str(raw_path),
                })
        if str(level['sourceId']) in {'10001', '10057'}:
            source_samples.append({'sourceId': str(level['sourceId']),
                                   'sourcePosition': level['position'],
                                   'pullQueries': source_queries})
        if (index + 1) % 100 == 0:
            print(f'EXTRACTED {index + 1} / {len(selected)}', flush=True)

    by_template = defaultdict(list)
    conflicts = []
    for (template_id, shape_index), variants in sorted(contracts.items()):
        row = {
            'shapeIndex': shape_index,
            'contractVariants': list(variants.values()),
            'observationCount': len(observations[(template_id, shape_index)]),
            'sampleObservations': observations[(template_id, shape_index)][:8],
        }
        if len(variants) > 1:
            row['conflict'] = True
            conflicts.append({'templateId': template_id, **row})
        else:
            row['conflict'] = False
        by_template[template_id].append(row)

    catalog = {
        'schemaVersion': 1,
        'sourceCount': len(selected),
        'pullObjectCount': pull_count,
        'queryShapeCount': sum(len(rows) for rows in by_template.values()),
        'contactFilterFields': list(FILTER_FIELDS),
        'templateQueries': dict(by_template),
        'conflicts': conflicts,
        'unresolvedReferences': unresolved,
        'safeForRuntimeImport': not conflicts and not unresolved,
        'semantics': ('Serialized PullObject Overlap2DHandler query collider shapeIndex and ContactFilter2D contract. '
                      'maxOverlapCollider is the native result-buffer capacity, not a filter. '
                      'originalComponentId and rawPath are preserved on source observations.'),
        'sourceSamples': source_samples,
        'hashes': {
            'sourcePack': sha(args.source_pack),
            'normalizedLevelFiles': raw_hashes,
            'extractor': sha(Path(__file__)),
        },
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(catalog, separators=(',', ':')), encoding='utf-8')
    summary = {
        'sourceCount': catalog['sourceCount'],
        'pullObjectCount': pull_count,
        'queryShapeCount': catalog['queryShapeCount'],
        'templateCount': len(by_template),
        'conflictCount': len(conflicts),
        'unresolvedCount': len(unresolved),
        'safeForRuntimeImport': catalog['safeForRuntimeImport'],
        'output': str(args.output),
    }
    print(json.dumps(summary, indent=2))
    if conflicts or unresolved:
        raise SystemExit('Pull overlap contract extraction found conflicts or unresolved references')


if __name__ == '__main__':
    main()
