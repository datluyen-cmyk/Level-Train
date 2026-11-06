"""Materialize reviewed ordinary sources in an isolated, non-overwriting run."""
from pathlib import Path
import argparse
import copy
import hashlib
import json


def read(path):
    return json.loads(path.read_text(encoding='utf8'))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write(path, value):
    path.write_text(json.dumps(value, separators=(',', ':')), encoding='utf8')


def main():
    p = argparse.ArgumentParser()
    p.add_argument('--bundle', required=True, type=Path)
    p.add_argument('--curation', required=True, type=Path)
    p.add_argument('--output', required=True, type=Path)
    a = p.parse_args()
    bundle, output = a.bundle.resolve(), a.output.resolve()
    curation = read(a.curation)
    source_path = bundle/'data/source500_native.json'
    templates_path = bundle/'data/source_templates.json'
    if sha(source_path) != curation['sourceCatalogSha256']:
        raise ValueError('Source catalog differs from the reviewed curation')
    if sha(templates_path) != curation['sourceTemplatesSha256']:
        raise ValueError('Template catalog differs from the reviewed curation')
    source, extra = read(source_path), read(bundle/'data/novelty_extras/source_parity_pack.json')
    templates = {t['id']: t for t in read(templates_path)['templates']}
    templates.update({t['id']: t for t in read(bundle/'data/novelty_extras/source_templates.json')['templates']})
    all_levels = source['levels']+extra['levels']
    by_id = {str(l['sourceId']): l for l in all_levels}
    if len(by_id) != len(all_levels):
        raise ValueError('Source catalog has duplicate identities')
    ordinary = set(map(str, curation['ordinarySourceIds']))
    excluded = set(map(str, curation['excludedSourceIds']))
    if ordinary & excluded or ordinary | excluded != set(by_id):
        raise ValueError('Reviewed ordinary/excluded partition does not cover the native catalog')
    if len(ordinary) != curation['actualUniqueOrdinaryCount']:
        raise ValueError('Ordinary count differs from curated identities')
    levels = sorted((copy.deepcopy(by_id[i]) for i in ordinary), key=lambda l: l['position'])
    novelty = [by_id[i] for i in by_id if i not in ordinary]
    if len({l['position'] for l in levels}) != len(levels):
        raise ValueError('Ordinary progression positions are not unique')
    target_ids = list(map(str, curation['trainingSourceIds']))
    if target_ids != [str(l['sourceId']) for l in levels[:len(target_ids)]]:
        raise ValueError('Target queue differs from first eligible originals')
    used = {p['templateId'] for l in levels for p in l['parts']}
    novelty_used = {p['templateId'] for l in novelty for p in l['parts']}
    if not used | novelty_used <= set(templates):
        raise ValueError('Missing native templates')
    output.mkdir(parents=True, exist_ok=False)
    for name in ['data/novelty_extras', 'models', 'reports', 'tools', 'batches', 'logs']:
        (output/name).mkdir(parents=True, exist_ok=True)
    pack = {**source, 'levels': levels, 'trainingScope': 'Reviewed ordinary layouts only; original progression positions retained',
            'actualUniqueOrdinaryCount': len(levels), 'generationEnabled': False,
            'legacyFilenameNotice': 'source500_native.json is a compatibility filename, not a count claim'}
    write(output/'data/source500_native.json', pack)
    write(output/'data/source_templates.json', {'schemaVersion': 3, 'templates': [templates[i] for i in sorted(used)]})
    write(output/'data/novelty_extras/source_parity_pack.json', {
        'schemaVersion': 3, 'levels': novelty, 'trainingAllowed': False,
        'meaning': 'Excluded pictorial sources retained only for duplicate prevention; no layout training or difficulty labels'})
    write(output/'data/novelty_extras/source_templates.json', {
        'schemaVersion': 3, 'templates': [templates[i] for i in sorted(novelty_used)]})
    bound = {**curation, 'sourcePackSha256': sha(output/'data/source500_native.json'),
             'sourcePackHashStatus': 'BOUND_TO_FILTERED_ORDINARY_RUN',
             'filteredSourceTemplatesSha256': sha(output/'data/source_templates.json'),
             'curationManifestSha256': sha(a.curation),
             'sourceCatalogPath': str(source_path), 'buildToolSha256': sha(Path(__file__))}
    write(output/'data/ordinary_source_filter_v4.json', bound)
    manifest = {'schemaVersion': 4, 'status': 'CURATED_DATA_READY_MODELS_NOT_REFITTED',
                'actualUniqueOrdinaryCount': len(levels), 'pictorialExcludedCount': len(novelty),
                'totalNoveltyExclusions': len(by_id), 'requestedTrainingSourceCount': curation['requestedTrainingSourceCount'],
                'requestedMinimum500_unmet': curation['requestedMinimum500_unmet'],
                'targetSourceIds': target_ids, 'generationEnabled': False, 'acceptedGeneratedCount': 0,
                'hashes': {str(f.relative_to(output)).replace('\\', '/'): sha(f) for f in (output/'data').rglob('*.json')}}
    write(output/'data/build_manifest_v4.json', manifest)
    print(json.dumps({'output': str(output), **{k: manifest[k] for k in ['actualUniqueOrdinaryCount', 'pictorialExcludedCount', 'totalNoveltyExclusions']}}))


if __name__ == '__main__':
    main()
