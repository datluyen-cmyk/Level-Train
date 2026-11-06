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


def execute(client, code):
    result = client.command('call', 'execute_code ' + json.dumps({'action': 'execute', 'code': code}))
    value = result.get('structuredContent')
    if value is None:
        value = json.loads(next(c['text'] for c in result['content'] if c['type'] == 'text'))
    if not value.get('success'):
        raise RuntimeError(value)
    return value['data']['result']


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--request', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    parser.add_argument('--project', type=Path, default=Path(__file__).resolve().parents[1] / 'RotateRings Test')
    args = parser.parse_args()
    request = args.request.resolve()
    config = json.loads(request.read_text(encoding='utf-8-sig'))
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=False)
    shutil.copyfile(request, output / 'request.json')
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
