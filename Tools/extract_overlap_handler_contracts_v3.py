"""Extract native RingObject/RingRelative overlap-query contracts per template.

Static export inspection only: reads source parity JSON, template geometry,
and normalized Unity component trees. Does not invoke Unity or rewrite source
data/templates.
"""
from pathlib import Path
from collections import Counter, defaultdict
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
    return json.loads(Path(path).read_text(encoding='utf8'))


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def pid(value):
    return int(value.get('m_PathID', 0)) if isinstance(value, dict) else 0


def filter_contract(handler):
    f = (handler or {}).get('contactFilter', {})
    return {key: f.get(key) for key in FILTER_FIELDS}


def collider_gameobjects(raw):
    """Map source collider component IDs to raw GameObject name/layer records."""
    by_collider = defaultdict(list)
    for game_object_id, entry in raw.get('objects', {}).items():
        if entry.get('type') != 'GameObject':
            continue
        data = entry.get('data', {})
        for component in data.get('m_Component', []):
            component_id = pid(component.get('component'))
            if component_id:
                by_collider[component_id].append({
                    'gameObjectPathId': int(game_object_id),
                    'gameObjectName': data.get('m_Name'),
                    'gameObjectLayer': data.get('m_Layer'),
                    'gameObjectActive': data.get('m_IsActive'),
                })
    return by_collider


def shape_for_collider(part, template, collider_id, unresolved, source_id, relative_id, ref_kind):
    binding = next((b for b in part.get('sourceColliderBindings', [])
                   if int(b['sourceColliderId']) == collider_id), None)
    if binding is None:
        unresolved.append({
            'sourceId': str(source_id), 'partId': part['id'],
            'owner': 'RingRelative', 'componentPathId': relative_id,
            'referenceKind': ref_kind, 'sourceColliderId': collider_id,
            'reason': 'serialized RingRelative collider reference did not map to an exported shape',
        })
        return None, None
    shape_index = int(binding['shapeIndex'])
    physical_shapes = template.get('physicalShapes', [])
    if shape_index < 0 or shape_index >= len(physical_shapes):
        unresolved.append({
            'sourceId': str(source_id), 'partId': part['id'],
            'owner': 'RingRelative', 'componentPathId': relative_id,
            'referenceKind': ref_kind, 'sourceColliderId': collider_id,
            'shapeIndex': shape_index,
            'reason': 'mapped RingRelative collider shapeIndex is outside the template physicalShapes array',
        })
        return None, None
    return shape_index, physical_shapes[shape_index]


def part_queries(level, part, raw, template):
    bindings = {int(b['sourceColliderId']): b for b in part.get('sourceColliderBindings', [])}
    queries = []
    unresolved = []
    native = part.get('sourceNativeData') or {}

    def add_root(owner, component_path, data):
        handler = data.get('overlap2DHandler') or {}
        collider_id = pid(handler.get('collider2D'))
        binding = bindings.get(collider_id)
        if not binding:
            unresolved.append({'partId': part['id'], 'owner': owner,
                               'componentPathId': component_path,
                               'sourceColliderId': collider_id,
                               'reason': 'handler collider not mapped to exported native shape'})
            return
        queries.append({
            'queryOwner': owner,
            'componentPathId': component_path,
            'sourceFeatureId': data.get('featureId') or binding.get('sourceFeatureId'),
            'shapeIndex': int(binding['shapeIndex']),
            'sourceRole': binding.get('sourceRole'),
            'contactFilter': filter_contract(handler),
            'maxOverlapCollider': handler.get('maxOverlapCollider'),
            'sourceColliderId': collider_id,
        })

    add_root('RingObject', pid(native.get('m_GameObject')), native)
    objects = raw.get('objects', {})
    gameobject_by_collider = collider_gameobjects(raw)
    relative_references = native.get('ringRelatives') or []
    for relative_index, ref in enumerate(relative_references):
        component_path = pid(ref)
        entry = objects.get(str(component_path), {})
        data = entry.get('data', {})
        if entry.get('type') != 'MonoBehaviour' or not data.get('overlap2DHandler'):
            unresolved.append({'partId': part['id'], 'owner': 'RingRelative',
                               'componentPathId': component_path,
                               'reason': 'referenced serialized RingRelative component did not resolve'})
            continue

        handler = data.get('overlap2DHandler') or {}
        query_collider_id = pid(handler.get('collider2D'))
        query_binding = bindings.get(query_collider_id)
        if not query_binding:
            unresolved.append({'partId': part['id'], 'owner': 'RingRelative',
                               'componentPathId': component_path,
                               'sourceColliderId': query_collider_id,
                               'reason': 'handler collider not mapped to exported native shape'})
            continue

        self_collider_id = pid(data.get('selfCollider'))
        self_shape_index, _ = shape_for_collider(
            part, template, self_collider_id, unresolved, level['sourceId'],
            component_path, 'selfCollider')

        block_collider_ids = [pid(item) for item in data.get('blockColliders') or []]
        block_shape_indices = []
        wall_shape_indices = []
        block_eligibility = []
        for block_collider_id in block_collider_ids:
            block_shape_index, physical = shape_for_collider(
                part, template, block_collider_id, unresolved, level['sourceId'],
                component_path, 'blockColliders')
            if block_shape_index is None:
                continue
            block_shape_indices.append(block_shape_index)
            go_entries = gameobject_by_collider.get(block_collider_id, [])
            if len(go_entries) != 1:
                unresolved.append({
                    'sourceId': str(level['sourceId']), 'partId': part['id'],
                    'owner': 'RingRelative', 'componentPathId': component_path,
                    'referenceKind': 'blockColliders', 'sourceColliderId': block_collider_id,
                    'gameObjectMatchCount': len(go_entries),
                    'reason': 'could not uniquely resolve raw collider GameObject name/layer',
                })
                go = go_entries[0] if go_entries else {}
            else:
                go = go_entries[0]
            enabled = physical.get('sourceEnabled')
            trigger = physical.get('trigger')
            layer = go.get('gameObjectLayer')
            if layer is None:
                layer = physical.get('sourceLayer')
            exclusions = []
            if enabled is False:
                exclusions.append('disabled')
            elif enabled is None:
                unresolved.append({
                    'sourceId': str(level['sourceId']), 'partId': part['id'],
                    'owner': 'RingRelative', 'componentPathId': component_path,
                    'referenceKind': 'blockColliders', 'sourceColliderId': block_collider_id,
                    'shapeIndex': block_shape_index,
                    'reason': 'template shape is missing serialized sourceEnabled state',
                })
            if trigger is True:
                exclusions.append('trigger')
            elif trigger is None:
                unresolved.append({
                    'sourceId': str(level['sourceId']), 'partId': part['id'],
                    'owner': 'RingRelative', 'componentPathId': component_path,
                    'referenceKind': 'blockColliders', 'sourceColliderId': block_collider_id,
                    'shapeIndex': block_shape_index,
                    'reason': 'template shape is missing serialized trigger state',
                })
            if layer == 6:
                exclusions.append('layer6')
            if go.get('gameObjectName') == 'Bottom':
                exclusions.append('Bottom')
            eligible = enabled is True and trigger is False and layer != 6 and go.get('gameObjectName') != 'Bottom'
            if eligible:
                wall_shape_indices.append(block_shape_index)
            block_eligibility.append({
                'sourceColliderId': block_collider_id,
                'shapeIndex': block_shape_index,
                'sourceRole': bindings[block_collider_id].get('sourceRole'),
                'role': physical.get('role'),
                'attachmentId': physical.get('attachmentId'),
                'sourceEnabled': enabled,
                'trigger': trigger,
                'gameObjectPathId': go.get('gameObjectPathId'),
                'gameObjectName': go.get('gameObjectName'),
                'gameObjectLayer': go.get('gameObjectLayer'),
                'eligibleWallProbe': eligible,
                'exclusionReasons': exclusions,
            })

        self_go_entries = gameobject_by_collider.get(self_collider_id, [])
        query_relative_feature = data.get('featureId')
        source_feature = query_relative_feature or query_binding.get('sourceFeatureId')
        query_go = self_go_entries[0] if len(self_go_entries) == 1 else {}
        if len(self_go_entries) != 1:
            unresolved.append({
                'sourceId': str(level['sourceId']), 'partId': part['id'],
                'owner': 'RingRelative', 'componentPathId': component_path,
                'referenceKind': 'selfCollider', 'sourceColliderId': self_collider_id,
                'gameObjectMatchCount': len(self_go_entries),
                'reason': 'could not uniquely resolve selfCollider GameObject identity',
            })
        queries.append({
            'queryOwner': 'RingRelative',
            'componentPathId': component_path,
            'originalComponentId': component_path,
            'sourceRelativeIndex': relative_index,
            'sourceRelativeFeatureId': query_relative_feature,
            'sourceFeatureId': source_feature,
            'shapeIndex': int(query_binding['shapeIndex']),
            'sourceRole': query_binding.get('sourceRole'),
            'contactFilter': filter_contract(handler),
            'maxOverlapCollider': handler.get('maxOverlapCollider'),
            'sourceColliderId': query_collider_id,
            'selfColliderId': self_collider_id,
            'selfShapeIndex': self_shape_index,
            'selfColliderMatchesQuery': self_collider_id == query_collider_id,
            'selfColliderGameObjectPathId': query_go.get('gameObjectPathId'),
            'selfColliderGameObjectName': query_go.get('gameObjectName'),
            'blockColliderIds': block_collider_ids,
            'blockShapeIndices': block_shape_indices,
            'wallShapeIndices': wall_shape_indices,
            'selfColliderInBlockColliders': self_collider_id in block_collider_ids,
            'blockColliderEligibility': block_eligibility,
        })
    return queries, unresolved


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--source-pack', type=Path, required=True)
    parser.add_argument('--templates', type=Path, required=True)
    parser.add_argument('--normalized-levels', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--source-ids', nargs='*')
    args = parser.parse_args()
    pack = read(args.source_pack)
    templates_data = read(args.templates)
    templates = {str(t['id']): t for t in templates_data['templates']}
    selected_ids = set(args.source_ids or [])
    selected = [level for level in pack['levels']
                if not selected_ids or str(level['sourceId']) in selected_ids]
    contracts = defaultdict(dict)
    observations = defaultdict(list)
    unresolved = []
    ring_count = 0
    ring_object_handler_count = 0
    ring_relative_handler_count = 0
    self_equals_query_count = 0
    self_differs_query_count = 0
    self_in_blocks_count = 0
    self_outside_blocks_count = 0
    block_eligibility_counts = Counter()
    source_samples = []
    raw_hashes = {}
    templates_seen = set()

    for index, level in enumerate(selected):
        raw_path = args.normalized_levels / Path(level['provenance']['rawPath']).name
        raw = read(raw_path)
        raw_hashes[str(raw_path)] = sha(raw_path)
        source_queries = []
        for part in level.get('parts', []):
            if part.get('kind') != 'ring':
                continue
            ring_count += 1
            template_id = str(part['templateId'])
            template = templates.get(template_id)
            if template is None:
                unresolved.append({'sourceId': str(level['sourceId']), 'partId': part['id'],
                                   'templateId': template_id,
                                   'reason': 'templateId not found in supplied source templates'})
                continue
            templates_seen.add(template_id)
            queries, missing = part_queries(level, part, raw, template)
            unresolved.extend({'sourceId': str(level['sourceId']), **item} for item in missing)
            source_queries.append({'partId': part['id'], 'sourceId': part.get('sourceId'),
                                   'templateId': template_id, 'queries': queries})
            for query in queries:
                if query['queryOwner'] == 'RingObject':
                    ring_object_handler_count += 1
                else:
                    ring_relative_handler_count += 1
                    self_equals_query_count += int(query['selfColliderMatchesQuery'])
                    self_differs_query_count += int(not query['selfColliderMatchesQuery'])
                    self_in_blocks_count += int(query['selfColliderInBlockColliders'])
                    self_outside_blocks_count += int(not query['selfColliderInBlockColliders'])
                    for item in query['blockColliderEligibility']:
                        key = ('eligible' if item['eligibleWallProbe'] else
                               ','.join(item['exclusionReasons']) or 'unresolved')
                        block_eligibility_counts[key] += 1
                tid = str(part['templateId'])
                shape_index = int(query['shapeIndex'])
                # Raw component/collider IDs are provenance, not reusable shape
                # contracts. Keep relative identity/feature and collider roles.
                fields = ['queryOwner', 'sourceRole', 'contactFilter', 'maxOverlapCollider']
                if query['queryOwner'] == 'RingRelative':
                    fields += ['selfShapeIndex',
                               'blockShapeIndices', 'wallShapeIndices']
                identity = {key: query[key] for key in fields}
                signature = json.dumps(identity, sort_keys=True, separators=(',', ':'))
                contracts[(tid, shape_index)][signature] = identity
                observations[(tid, shape_index)].append({
                    'sourceId': str(level['sourceId']), 'partId': part['id'],
                    'sourcePartId': part.get('sourceId'),
                    'sourceFeatureId': query['sourceFeatureId'],
                    'sourceRelativeFeatureId': query.get('sourceRelativeFeatureId'),
                    'sourceRelativeIndex': query.get('sourceRelativeIndex'),
                    'sourceColliderId': query['sourceColliderId'],
                    'originalComponentId': query.get('originalComponentId', query['componentPathId']),
                    'componentPathId': query['componentPathId'],
                    'rawPath': str(raw_path),
                    'selfColliderId': query.get('selfColliderId'),
                    'selfShapeIndex': query.get('selfShapeIndex'),
                    'selfColliderMatchesQuery': query.get('selfColliderMatchesQuery'),
                    'selfColliderInBlockColliders': query.get('selfColliderInBlockColliders'),
                    'blockColliderIds': query.get('blockColliderIds'),
                    'blockShapeIndices': query.get('blockShapeIndices'),
                    'wallShapeIndices': query.get('wallShapeIndices'),
                    'blockColliderEligibility': query.get('blockColliderEligibility'),
                })
        if str(level['sourceId']) in {'10001', '10057'}:
            source_samples.append({'sourceId': str(level['sourceId']),
                                   'sourcePosition': level['position'],
                                   'ringQueries': source_queries})
        if (index + 1) % 100 == 0:
            print('EXTRACTED', index + 1, '/', len(selected), flush=True)

    by_template = defaultdict(list)
    conflicts = []
    for (template_id, shape_index), variants in sorted(contracts.items()):
        all_observations = observations[(template_id, shape_index)]
        relative_provenance_fields = (
            'sourceId', 'partId', 'sourcePartId', 'sourceFeatureId',
            'sourceRelativeFeatureId', 'sourceRelativeIndex', 'sourceColliderId',
            'originalComponentId', 'componentPathId', 'rawPath', 'selfColliderId',
            'selfShapeIndex', 'selfColliderMatchesQuery', 'selfColliderInBlockColliders',
            'blockColliderIds', 'blockShapeIndices', 'wallShapeIndices',
        )
        row = {'shapeIndex': shape_index, 'contractVariants': list(variants.values()),
               'observationCount': len(all_observations),
               'sampleObservations': all_observations[:8],
               'sourceRelativeObservations': [
                   {key: item.get(key) for key in relative_provenance_fields}
                   for item in all_observations
                   if item.get('sourceRelativeFeatureId') is not None
               ]}
        if len(variants) > 1:
            row['conflict'] = True
            conflicts.append({'templateId': template_id, **row})
        else:
            row['conflict'] = False
        by_template[template_id].append(row)

    ring_link_count = sum(len(level.get('links', [])) for level in selected)
    catalog = {
        'schemaVersion': 2,
        'sourceCount': len(selected),
        'ringCount': ring_count,
        'authoredRingLinkCount': ring_link_count,
        'ringObjectHandlerCount': ring_object_handler_count,
        'ringRelativeHandlerCount': ring_relative_handler_count,
        'queryShapeCount': sum(len(rows) for rows in by_template.values()),
        'relativeColliderAudit': {
            'selfColliderMatchesQueryCount': self_equals_query_count,
            'selfColliderDiffersFromQueryCount': self_differs_query_count,
            'selfColliderInBlockCollidersCount': self_in_blocks_count,
            'selfColliderOutsideBlockCollidersCount': self_outside_blocks_count,
            'blockColliderWallEligibilityCounts': dict(block_eligibility_counts),
            'wallFilter': {'includeTriggers': False, 'requiredLayer': 7,
                           'excludeGameObjectName': 'Bottom',
                           'requiresEnabled': True},
        },
        'contactFilterFields': list(FILTER_FIELDS),
        'templateQueries': dict(by_template),
        'conflicts': conflicts,
        'unresolvedReferences': unresolved,
        'safeForRuntimeImport': not conflicts and not unresolved,
        'semantics': ('Serialized query collider shapeIndex and ContactFilter2D contract. '
                      'RingRelative contracts also carry relative feature/index, serialized selfCollider '
                      'and blockColliders shape mappings, plus wall-probe eligibility after the native '
                      'enabled/trigger/layer-6/Bottom exclusions. maxOverlapCollider is buffer capacity, not a filter.'),
        'sourceSamples': source_samples,
        'hashes': {
            'sourcePack': sha(args.source_pack),
            'templates': sha(args.templates),
            'normalizedLevelFiles': raw_hashes,
            'extractor': sha(Path(__file__)),
        },
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(catalog, separators=(',', ':')), encoding='utf8')
    summary = {
        'sourceCount': catalog['sourceCount'], 'ringCount': ring_count,
        'authoredRingLinkCount': ring_link_count,
        'ringObjectHandlerCount': ring_object_handler_count,
        'ringRelativeHandlerCount': ring_relative_handler_count,
        'queryShapeCount': catalog['queryShapeCount'],
        'templateCount': len(by_template), 'conflictCount': len(conflicts),
        'unresolvedCount': len(unresolved),
        'selfColliderMatchesQueryCount': self_equals_query_count,
        'selfColliderDiffersFromQueryCount': self_differs_query_count,
        'selfColliderInBlockCollidersCount': self_in_blocks_count,
        'selfColliderOutsideBlockCollidersCount': self_outside_blocks_count,
        'blockColliderWallEligibilityCounts': dict(block_eligibility_counts),
        'safeForRuntimeImport': catalog['safeForRuntimeImport'],
    }
    print(json.dumps(summary, indent=2))
    if conflicts or unresolved:
        raise SystemExit('Ring overlap contract extraction found conflicts or unresolved references')


if __name__ == '__main__':
    main()
