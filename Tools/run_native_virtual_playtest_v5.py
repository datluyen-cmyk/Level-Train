"""Run an immutable native Unity bot request, pumping bounded editor updates.

No simulated outcomes are computed here. All episodes run in SourceSolverV3.
An interrupted client leaves Unity's checkpoint/job intact; never restart blindly.
"""
from pathlib import Path
import argparse
import hashlib
import json
import shutil
import time

from unity_mcp import McpClient, DEFAULT_URL


def execute(client, code, wait_seconds=180):
    """Run C# in the Editor. Unity drops its MCP session during script
    recompiles/domain reloads; the server then answers `no_unity_session`
    with a retry hint, so wait for the editor to reconnect instead of failing."""
    deadline = time.monotonic() + wait_seconds
    announced = False
    while True:
        result = client.command('call', 'execute_code ' + json.dumps({'action': 'execute', 'code': code}))
        value = result.get('structuredContent')
        if value is None:
            value = json.loads(next(c['text'] for c in result['content'] if c['type'] == 'text'))
        if value.get('success'):
            return value['data']['result']
        data = value.get('data') or {}
        if data.get('reason') == 'no_unity_session' and time.monotonic() < deadline:
            if not announced:
                print('Unity MCP session not connected (recompiling or disconnected); waiting...', flush=True)
                announced = True
            time.sleep(max(0.25, float(data.get('retry_after_ms', 1000)) / 1000.0))
            continue
        if data.get('reason') == 'no_unity_session':
            raise RuntimeError('Unity never connected to the MCP server at ' + DEFAULT_URL +
                               '. In Unity: Window > MCP for Unity, Transport = HTTP Local, '
                               'URL http://127.0.0.1:8080, Start Server, and check the Console has no compile errors. '
                               'Diagnose with: python Tools/unity_mcp.py resource mcpforunity://instances')
        raise RuntimeError(value)


DEFAULT_REFERENCE_CACHE = (Path(__file__).resolve().parents[1] /
                           'RotateRings Test/LevelTraining_SourceLayoutV3/runs/bot_reference_cache')
CACHE_CONFIG_KEYS = ('seed', 'users', 'stepLimit', 'wallTimeLimitMs', 'transitionCacheLimit', 'policyVariant')


def file_sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def reference_cache_key(config, manifest):
    """Episodes are a function of seed, profiles, budgets, rules/policy code and
    the source/template bytes; any change gives a new key (old entries unused)."""
    material = {'config': {k: config.get(k) for k in CACHE_CONFIG_KEYS},
                'snapshots': manifest,
                'sourcePack': file_sha(config['sourcePackPath']),
                'templates': file_sha(config['templatesPath'])}
    return hashlib.sha256(json.dumps(material, sort_keys=True).encode('utf-8')).hexdigest()[:32]


def plan_reference_tasks(config, cache_dir):
    """Mark which pairs need no native reference task: cached, or the same
    reference already played by an earlier pair in this request (best-of-N)."""
    seen = set()
    for pair in config.get('pairs', []):
        reference_id = int(pair['referenceId'])
        cached = cache_dir is not None and (cache_dir / f'ref_{reference_id}.json').is_file()
        pair['skipReference'] = bool(cached or reference_id in seen)
        seen.add(reference_id)


def merge_reference_sessions(report, config, cache_dir):
    """Store newly played reference episodes and fill skipped reference tasks.

    Copies keep the original episode (same profile, seed, level hash, actions);
    only referencePairId/sessionId are re-labelled to the pair they serve.
    """
    by_reference = {}
    for session in report['sessions']:
        if session['role'] == 'reference':
            by_reference.setdefault(int(session['levelId']), []).append(session)
    if cache_dir is not None and report.get('complete'):
        cache_dir.mkdir(parents=True, exist_ok=True)
        for reference_id, sessions in by_reference.items():
            path = cache_dir / f'ref_{reference_id}.json'
            if path.exists() or any(s['termination'] == 'engine_error' for s in sessions):
                continue
            if len(sessions) != int(config['users']):
                continue
            temp = path.with_suffix('.tmp')
            temp.write_text(json.dumps({'referenceId': reference_id, 'sessions': sessions}), encoding='utf-8')
            temp.replace(path)
    merged = dict(report)
    merged['sessions'] = list(report['sessions'])
    filled = 0
    for pair in config.get('pairs', []):
        if not pair.get('skipReference'):
            continue
        reference_id = int(pair['referenceId'])
        source = by_reference.get(reference_id)
        if source is None and cache_dir is not None:
            path = cache_dir / f'ref_{reference_id}.json'
            if path.is_file():
                source = json.loads(path.read_text(encoding='utf-8'))['sessions']
        if not source:
            raise RuntimeError(f'Reference {reference_id} was skipped but has no played or cached episodes')
        played_pair_ids = {s['referencePairId'] for s in source}
        for session in source:
            if session['referencePairId'] not in played_pair_ids:
                continue
            copy = dict(session)
            copy['referencePairId'] = pair['referencePairId']
            copy['sessionId'] = f"{pair['referencePairId']}-reference-{session['profileId']}"
            merged['sessions'].append(copy)
            filled += 1
            # Only one source pair's episodes are needed per reference.
        if len(played_pair_ids) > 1:
            raise RuntimeError(f'Reference {reference_id} episodes span multiple pairs unexpectedly')
    if filled:
        role_count = 2 if report.get('candidatePackSha256') else 1
        merged['cachedReferenceSessions'] = filled
        merged['completedSessions'] = len(merged['sessions'])
        merged['totalSessions'] = (len(report['profiles']) * len(config['pairs']) * role_count -
                                   int(report.get('earlyStoppedSessions', 0)))
    return merged


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--request', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--project', type=Path, default=Path(__file__).resolve().parents[1] / 'RotateRings Test')
    parser.add_argument('--reference-cache', type=Path, default=DEFAULT_REFERENCE_CACHE,
                        help='Reuse reference (source) episodes with identical seed/rules/policy/inputs')
    parser.add_argument('--no-reference-cache', action='store_true')
    args = parser.parse_args()
    request = args.request.resolve()
    config = json.loads(request.read_text(encoding='utf-8-sig'))
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=False)
    snapshots = output / 'native_snapshots'
    paths = ['RotateRings/Editor/SourceVirtualPlaytestV5.cs', 'RotateRings/Editor/SourceSolverV3.cs',
             'RotateRings/Scripts/RotateRingsCollisionWorld.cs', 'RotateRings/Scripts/RotateRingsBombContacts.cs',
             'RotateRings/Scripts/RotateRingsLevelData.cs', 'RotateRings/Scripts/RotateRingsRuntime.cs',
             'RotateRings/Scripts/RotateRingsNativeInterlockPolicy.cs', 'RotateRings/Resources/SourceOverlapQueries.json']
    manifest = {}
    for relative in paths:
        src = args.project / 'Assets' / relative
        dst = snapshots / relative
        dst.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(src, dst)
        manifest[relative] = hashlib.sha256(dst.read_bytes()).hexdigest()
    (output / 'snapshot_manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    cache_dir = None
    if not args.no_reference_cache and config.get('pairs'):
        cache_dir = args.reference_cache.resolve() / reference_cache_key(config, manifest)
    plan_reference_tasks(config, cache_dir)
    (output / 'request.json').write_text(json.dumps(config, indent=2), encoding='utf-8')
    skipped = sum(bool(p.get('skipReference')) for p in config.get('pairs', []))
    print(json.dumps({'referenceTasksSkipped': skipped, 'referenceCache': str(cache_dir) if cache_dir else None}), flush=True)
    client = McpClient(DEFAULT_URL)
    if execute(client, 'return SourceSolverV3.GetPlayerBatchStatusV5();') != 'idle':
        raise RuntimeError('Another native player job is active; nothing started.')
    report_path = str(output / 'native_report.json')
    code = 'return SourceSolverV3.StartPlayerBatchV5(' + json.dumps(str(output / 'request.json')) + ',' + json.dumps(report_path) + ');'
    print(execute(client, code), flush=True)
    tick = ('var tick=typeof(SourceSolverV3).GetMethod("TickPlayerV5", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);'
            'var timer=System.Diagnostics.Stopwatch.StartNew(); int n=0;'
            'while(n<200 && timer.ElapsedMilliseconds<3000 && SourceSolverV3.GetPlayerBatchStatusV5()!="idle") { tick.Invoke(null,null); n++; }'
            'return SourceSolverV3.GetPlayerBatchStatusV5();')
    started = last = time.monotonic()
    while True:
        state = execute(client, tick)
        if state == 'idle':
            break
        if time.monotonic() - last >= 20:
            print(state, flush=True)
            last = time.monotonic()
    report = json.loads(Path(report_path).read_text(encoding='utf-8-sig'))
    if not report.get('complete'):
        raise RuntimeError('Native job ended incomplete; inspect retained evidence.')
    from verify_native_virtual_playtest_v5 import verify
    verification = verify(output)
    (output / 'hash_verification.json').write_text(json.dumps(verification, indent=2), encoding='utf-8')
    report = merge_reference_sessions(report, config, cache_dir)
    if report.get('cachedReferenceSessions'):
        (output / 'native_report_with_cached_references.json').write_text(json.dumps(report), encoding='utf-8')
    from summarize_virtual_playtest_v5 import summarize
    summary = summarize(report, requested_users=config['users'])
    summary['clientElapsedSeconds'] = time.monotonic() - started
    summary['hashes'].update(nativeEpisodeReport=hashlib.sha256(Path(report_path).read_bytes()).hexdigest(),
        summarizer=hashlib.sha256(Path(__file__).with_name('summarize_virtual_playtest_v5.py').read_bytes()).hexdigest(),
        runner=hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
        candidatePackSha256=report.get('candidatePackSha256', ''),
        request=hashlib.sha256((output / 'request.json').read_bytes()).hexdigest())
    summary_dir = output / 'summary'
    summary_dir.mkdir()
    (summary_dir / 'summary.json').write_text(json.dumps(summary, indent=2), encoding='utf-8')
    print(json.dumps({'complete': True, 'sessions': len(report['sessions']),
                      'profiles': summary['actualProfilesWithSessions'],
                      'elapsedSeconds': summary['clientElapsedSeconds'], 'output': str(output)}), flush=True)


if __name__ == '__main__':
    main()
