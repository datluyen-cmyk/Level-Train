"""Generate a checked batch from saved models without retraining. Trusted local models only."""
from __future__ import annotations
import argparse
import collections
import copy
import json
import math
import pathlib
import random
import sys
import time

from level_training import compare, digest, load_adapter, measured, records, write


def run(config_path, output, count, seed, exclusions):
    import joblib
    if count < 1:
        raise ValueError('count must be positive')
    config_path = pathlib.Path(config_path).resolve()
    root = config_path.parent
    config = json.loads(config_path.read_text(encoding='utf8'))
    adapter_path = (root / config['adapter']).resolve()
    reference_path = (root / config['references']).resolve()
    corpus_path = (root / config['corpus']).resolve()
    manifest_path = root / 'models/manifest.json'
    if not manifest_path.exists():
        raise ValueError('Missing model manifest: run level_training.py once with this version first')
    manifest = json.loads(manifest_path.read_text(encoding='utf8'))
    for key, path in [('config', config_path), ('adapter', adapter_path), ('references', reference_path), ('corpus', corpus_path)]:
        if digest(path) != manifest['inputHashes'][key]:
            raise ValueError('Training input changed: ' + key + '; retrain in a separate workspace')
    helpers = {p.name: digest(p) for p in adapter_path.parent.glob('*.py')}
    if helpers != manifest['helperHashes']:
        raise ValueError('Adapter helpers changed since training')
    if digest(pathlib.Path(__file__).with_name('level_training.py')) != manifest['inputHashes']['runner']:
        raise ValueError('Training runner changed; retrain before generation')
    for name in ['difficulty.joblib', 'proposal.joblib']:
        if digest(root / 'models' / name) != manifest['modelHashes'][name]:
            raise ValueError('Model hash mismatch: ' + name)
    adapter = load_adapter(adapter_path)
    refs = records(reference_path)
    sources = records(corpus_path) + refs
    for path in exclusions:
        sources.extend(records(path))
    for row in sources:
        if adapter.validate_level(row):
            raise ValueError('Unsupported source/exclusion: ' + row['id'])
    out = pathlib.Path(output).resolve()
    if out.exists():
        raise ValueError('Output must be a new directory: ' + str(out))
    # joblib is Python serialization; load only locally produced, trusted artifacts.
    difficulty = joblib.load(root / 'models/difficulty.joblib')
    proposal = joblib.load(root / 'models/proposal.joblib')['model']
    model, names = difficulty['model'], difficulty['features']
    policies = config['policies']
    targets = {}
    for ref in refs:
        result = adapter.solve(ref, copy.deepcopy(policies[0]))
        if not result.get('solved') or not adapter.replay(ref, result['solution']):
            raise ValueError('Reference cannot be replayed: ' + ref['id'])
        targets[ref['id']] = (float(adapter.difficulty(ref, result)), measured(adapter, ref, policies))
    out.mkdir(parents=True)
    rng = random.Random(seed)
    accepted, failures = [], []
    fingerprints = {adapter.fingerprint(row) for row in sources}
    rejections = collections.Counter()
    started = time.perf_counter()
    for index in range(count):
        ref = refs[index % len(refs)]
        target, reference_portfolio = targets[ref['id']]
        gate = {**config['acceptance'], **config.get('perReference', {}).get(ref['id'], {})}
        candidates = []
        for _ in range(int(config['proposalBudget'])):
            candidate = adapter.propose(ref, rng, proposal)
            if candidate is None:
                rejections['proposal'] += 1
                continue
            errors = adapter.validate_level(candidate)
            if hasattr(adapter, 'validate_candidate'):
                errors += adapter.validate_candidate(candidate, ref)
            if errors:
                rejections['constraints'] += 1
                continue
            features = adapter.features(candidate)
            if sorted(features) != sorted(names) or any(not math.isfinite(float(v)) for v in features.values()):
                raise ValueError('Feature schema mismatch or nonfinite value')
            candidates.append((candidate, [features[k] for k in names]))
        ranked = []
        if candidates:
            predictions = model.predict([f for _, f in candidates])
            ranked = sorted(zip(candidates, predictions), key=lambda item: abs(item[1] - target))
        best = None
        for (candidate, _), prediction in ranked[:int(config['oracleCheckBudget'])]:
            fp = adapter.fingerprint(candidate)
            if fp in fingerprints:
                rejections['equivalent'] += 1
                continue
            result = adapter.solve(candidate, copy.deepcopy(policies[0]))
            if not result.get('solved'):
                rejections['solver'] += 1
                continue
            if not adapter.replay(candidate, result['solution']):
                raise ValueError('Candidate replay failed')
            score = float(adapter.difficulty(candidate, result))
            if not math.isfinite(score) or score < 0:
                raise ValueError('Invalid difficulty')
            relative = abs(score - target) / max(abs(target), gate['scoreFloor'])
            if relative > gate['primaryRelativeTolerance']:
                rejections['primaryDifficulty'] += 1
                continue
            portfolio = measured(adapter, candidate, policies)
            comparison = compare(reference_portfolio, portfolio, gate)
            if not comparison['passed']:
                rejections['portfolio'] += 1
                continue
            nearest, novel = 1., True
            for other in sources + accepted:
                distance = float(adapter.novelty_distance(candidate, other))
                if not math.isfinite(distance) or not 0 <= distance <= 1:
                    raise ValueError('Invalid novelty distance')
                nearest = min(nearest, distance)
                threshold = max(gate['nearCopyDistance'], other.get('acceptance', {}).get('criteria', {}).get('nearCopyDistance', 0))
                if distance < threshold:
                    novel = False
                    break
            if not novel:
                rejections['nearCopy'] += 1
                continue
            best = copy.deepcopy(candidate)
            best.update(id=f'batch-{seed}-{index + 1:06d}', group='generated-' + ref['group'], referenceId=ref['id'],
                        solution=result['solution'], predictedScore=float(prediction),
                        acceptance={'targetScore':target,'score':score,'relativeDifference':relative,
                                    'nearestDistance':nearest,'criteria':gate,'referencePortfolio':reference_portfolio,
                                    'portfolio':portfolio,'comparison':comparison})
            fingerprints.add(fp)
            break
        if best is None:
            failures.append({'position':index + 1,'referenceId':ref['id']})
        else:
            accepted.append(best)
        write(out / 'partial_levels.json', {'complete':False,'requestedCount':count,'levels':accepted})
        print(f'{index + 1}/{count}: ' + ('PASS' if best else 'INCOMPLETE'), flush=True)
    complete = len(accepted) == count
    report = {'complete':complete,'requestedCount':count,'generatedCount':len(accepted),'seed':seed,
              'modelManifestSha256':digest(manifest_path),'generatorSha256':digest(__file__),
              'exclusions':{str(pathlib.Path(p).resolve()):digest(p) for p in exclusions},
              'failures':failures,'rejections':dict(rejections),'seconds':time.perf_counter() - started,
              'limits':'Saved scalar model and solver portfolio; virtual-player feedback is not automatically an acceptance gate. Human difficulty unverified.'}
    if complete:
        if not all(adapter.replay(row, row['solution']) for row in accepted):
            raise ValueError('Final replay failed')
        write(out / 'generated_levels.json', {'schemaVersion':1,'datasetKind':config.get('datasetKind','unspecified'),
                                               'seed':seed,'levels':accepted})
        report['packSha256'] = digest(out / 'generated_levels.json')
    write(out / 'validation_report.json', report)
    print('COMPLETE' if complete else 'INCOMPLETE', len(accepted), '/', count, flush=True)
    return 0 if complete else 2


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--config', required=True)
    parser.add_argument('--output', required=True, help='New output directory')
    parser.add_argument('--count', type=int, default=100)
    parser.add_argument('--seed', type=int, default=1)
    parser.add_argument('--exclude', action='append', default=[], help='Prior accepted pack to avoid copying; repeatable')
    args = parser.parse_args()
    try:
        raise SystemExit(run(args.config, args.output, args.count, args.seed, args.exclude))
    except (ValueError, FileNotFoundError) as error:
        print('ERROR:', error, file=sys.stderr)
        raise SystemExit(1)
