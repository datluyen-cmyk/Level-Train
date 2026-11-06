"""Static all-source500 guide-query / extra-shape audit (no Unity runtime)."""
from pathlib import Path
import hashlib
import json
import math

from shapely import __version__ as shapely_version
from shapely.affinity import affine_transform
from shapely.geometry import Polygon

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'RotateRings Test/LevelTraining_SourceLayoutV3/data/source500_native.json'
TEMPLATES = ROOT / 'RotateRings Test/LevelTraining_SourceLayoutV3/data/source_templates.json'
QUERIES = ROOT / 'RotateRings Test/Assets/RotateRings/Resources/SourceOverlapQueries.json'
RUNTIME = ROOT / 'RotateRings Test/Assets/RotateRings/Scripts/RotateRingsCollisionWorld.cs'

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def shape_polygon(shape):
    paths = shape.get('paths') or ([{'points': shape.get('points') or []}]
                                   if shape.get('points') else [])
    contours = []
    for path in paths:
        points = [(p['x'], p['y']) for p in path.get('points', [])]
        if len(points) >= 3:
            polygon = Polygon(points)
            if not polygon.is_empty:
                contours.append((polygon, points,
                                 1 if polygon.exterior.is_ccw else -1))
    if not contours:
        return Polygon()
    outer = max(contours, key=lambda item: item[0].area)
    holes = [points for polygon, points, winding in contours
             if winding != outer[2] and outer[0].contains(polygon)]
    if holes:
        return Polygon(outer[1], holes)
    result = contours[0][0]
    for polygon, _, _ in contours[1:]:
        result = result.union(polygon)
    return result

def at_native_pose(shape, part):
    polygon = shape_polygon(shape)
    angle = math.radians(part['angle'])
    cosine, sine = math.cos(angle), math.sin(angle)
    return affine_transform(polygon, [cosine, -sine, sine, cosine,
                                      part['x'], part['y']])

def main():
    source_data = json.loads(SOURCE.read_text(encoding='utf-8'))
    template_data = json.loads(TEMPLATES.read_text(encoding='utf-8'))
    query_data = json.loads(QUERIES.read_text(encoding='utf-8'))
    templates = {item['id']: item for item in template_data['templates']}
    queries = {item['templateId']: item['queries'] for item in query_data['templates']}

    guide_link_count = 0
    levels_with_guides = 0
    guide_shape_errors = []
    non_guide_body_targets = []
    ring_links = []
    for level in source_data['levels']:
        links = level.get('guideLinks') or []
        if links:
            levels_with_guides += 1
        guide_link_count += len(links)
        parts = {part['id']: part for part in level['parts']}
        for link in links:
            owner = parts.get(link['lockedOwnerId'])
            guide_owner = parts.get(link['guideOwnerId'])
            if owner is None or guide_owner is None:
                guide_shape_errors.append({'position': level['position'], 'sourceId': level['sourceId'],
                                           'guidePartId': link['guidePartId'], 'error': 'missing owner'})
                continue
            guide_template = templates[guide_owner['templateId']]
            guide_indices = link.get('guideShapeIndices') or []
            for index in guide_indices:
                if index < 0 or index >= len(guide_template['physicalShapes']):
                    guide_shape_errors.append({'position': level['position'], 'sourceId': level['sourceId'],
                                               'guidePartId': link['guidePartId'], 'error': 'bad guide index',
                                               'shapeIndex': index})
                    continue
                shape = guide_template['physicalShapes'][index]
                if (shape['sourceRole'] != 'CatchGuideBody' or shape['sourceLayer'] != 7 or
                        not shape['trigger'] or not shape['sourceEnabled']):
                    non_guide_body_targets.append({'position': level['position'], 'sourceId': level['sourceId'],
                                                   'guidePartId': link['guidePartId'], 'shapeIndex': index,
                                                   'sourceRole': shape['sourceRole'], 'sourceLayer': shape['sourceLayer'],
                                                   'trigger': shape['trigger'], 'sourceEnabled': shape['sourceEnabled']})
            if owner['kind'] != 'ring' or not guide_indices or guide_indices[0] >= len(guide_template['physicalShapes']):
                continue

            owner_template = templates[owner['templateId']]
            guide_shape = guide_template['physicalShapes'][guide_indices[0]]
            guide_geometry = at_native_pose(guide_shape, guide_owner)
            binding_by_index = {binding['shapeIndex']: binding
                                for binding in owner.get('sourceColliderBindings') or []}
            query_roles = {'RingObject', 'RingRelative'}
            eligible = set()
            invalid_query_rows = []
            for query in queries.get(owner['templateId'], []):
                if (query['queryOwner'] not in query_roles or
                        query['contactFilter']['useTriggers'] != 1 or
                        not (query['contactFilter']['layerMask']['m_Bits'] & (1 << guide_shape['sourceLayer']))):
                    continue
                index = query['shapeIndex']
                if index not in binding_by_index or index >= len(owner_template['physicalShapes']):
                    continue
                source_shape = owner_template['physicalShapes'][index]
                if source_shape['sourceRole'] != query['sourceRole']:
                    invalid_query_rows.append({'shapeIndex': index, 'queryRole': query['sourceRole'],
                                               'sourceRole': source_shape['sourceRole']})
                    continue
                eligible.add(index)

            extras = sorted(set(binding_by_index) - eligible)
            if not extras:
                continue
            extra_rows = []
            for index in extras:
                shape = owner_template['physicalShapes'][index]
                geometry = at_native_pose(shape, owner)
                extra_rows.append({
                    'shapeIndex': index,
                    'sourceRole': shape['sourceRole'],
                    'sourceFeatureId': binding_by_index[index].get('sourceFeatureId'),
                    'sourceColliderId': binding_by_index[index].get('sourceColliderId'),
                    'sourceLayer': shape['sourceLayer'],
                    'trigger': shape['trigger'],
                    'sourceEnabled': shape['sourceEnabled'],
                    'sourceColliderType': shape.get('sourceColliderType'),
                    'edgeRadius': shape.get('edgeRadius', 0.0),
                    'nativePoseDistance': geometry.distance(guide_geometry),
                    'nativePoseOverlaps': geometry.intersects(guide_geometry),
                })
            query_rows = []
            for index in sorted(eligible):
                shape = owner_template['physicalShapes'][index]
                geometry = at_native_pose(shape, owner)
                query_rows.append({
                    'shapeIndex': index,
                    'sourceRole': shape['sourceRole'],
                    'nativePoseDistance': geometry.distance(guide_geometry),
                    'nativePoseOverlaps': geometry.intersects(guide_geometry),
                })
            ring_links.append({
                'position': level['position'],
                'sourceId': str(level['sourceId']),
                'guidePartId': link['guidePartId'],
                'guideOwnerId': link['guideOwnerId'],
                'lockedOwnerId': owner['id'],
                'guideKind': guide_owner['kind'],
                'guideShapeIndex': guide_indices[0],
                'extraShapes': extra_rows,
                'validGuideQueryShapes': query_rows,
                'queryIdentityErrors': invalid_query_rows,
            })

    extra_items = [shape for relation in ring_links for shape in relation['extraShapes']]
    query_overlap_relations = [relation for relation in ring_links
                               if any(shape['nativePoseOverlaps'] for shape in relation['validGuideQueryShapes'])]
    report = {
        'schemaVersion': 1,
        'scope': 'source500 positions 1-500; static authored/native pose only',
        'method': {
            'runtimeUsed': False,
            'geometry': 'Shapely polygon intersection on source points/paths transformed by the authored part x/y/angle; opposite-winding enclosed polygon paths are treated as holes.',
            'limits': ['No Physics2D/ColliderDistance2D calls.',
                       'No sampled or swept rotation poses evaluated.',
                       'This establishes authored-pose overlap only; non-overlap extra shapes may become contact during motion.'],
            'shapelyVersion': shapely_version,
        },
        'hashes': {
            'source500NativeSha256': sha(SOURCE),
            'sourceTemplatesSha256': sha(TEMPLATES),
            'sourceOverlapQueriesSha256': sha(QUERIES),
            'collisionWorldSha256': sha(RUNTIME),
        },
        'counts': {
            'sourceLevels': len(source_data['levels']),
            'levelsWithGuideLinks': levels_with_guides,
            'guideLinks': guide_link_count,
            'guideShapeIndexOrTargetErrors': len(guide_shape_errors) + len(non_guide_body_targets),
            'ringGuideLinksWithNonQueryExtraShapes': len(ring_links),
            'ringGuideLinksAcrossPositions': len({row['position'] for row in ring_links}),
            'extraShapeReferences': len(extra_items),
            'extraShapeReferencesOverlappingSelectedGuideAtNativePose': sum(x['nativePoseOverlaps'] for x in extra_items),
            'minExtraShapeDistanceAtNativePose': min((x['nativePoseDistance'] for x in extra_items), default=None),
            'maxExtraShapeDistanceAtNativePose': max((x['nativePoseDistance'] for x in extra_items), default=None),
            'ringGuideLinksWithValidQueryShapeOverlapAtNativePose': len(query_overlap_relations),
        },
        'guideShapeErrors': guide_shape_errors,
        'nonGuideBodyTargets': non_guide_body_targets,
        'ringExtraShapeRelations': ring_links,
    }
    out = ROOT / 'Research/physics_v3/native_guide_binding_initialpose_extras_first500.json'
    out.write_text(json.dumps(report, indent=2), encoding='utf-8')
    print(f'Wrote {out}')
    print(json.dumps(report['counts'], indent=2))

if __name__ == '__main__':
    main()
