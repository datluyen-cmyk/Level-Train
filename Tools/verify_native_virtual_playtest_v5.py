"""Verify native report bindings against retained byte-exact rule snapshots."""
from pathlib import Path
import argparse
import hashlib
import json
import struct

RULES = (
    'RotateRings/Scripts/RotateRingsCollisionWorld.cs',
    'RotateRings/Scripts/RotateRingsBombContacts.cs',
    'RotateRings/Scripts/RotateRingsLevelData.cs',
    'RotateRings/Scripts/RotateRingsRuntime.cs',
    'RotateRings/Scripts/RotateRingsNativeInterlockPolicy.cs',
    'RotateRings/Resources/SourceOverlapQueries.json',
    'RotateRings/Editor/SourceSolverV3.cs',
)


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def net_string(value):
    data = value.encode('utf-8')
    count, prefix = len(data), bytearray()
    while count >= 128:
        prefix.append((count & 127) | 128)
        count >>= 7
    return bytes(prefix) + bytes([count]) + data


def verify(directory, check_live=True):
    directory = Path(directory)
    report_path = directory / 'native_report.json'
    report = json.loads(report_path.read_text(encoding='utf-8-sig'))
    snapshots = directory / 'native_snapshots'
    manifest = json.loads((directory / 'snapshot_manifest.json').read_text())
    for path, digest in manifest.items():
        if sha(snapshots / path) != digest:
            raise ValueError('Altered native snapshot: ' + path)
    stream = net_string(report['unityVersion']) + struct.pack('<f', report['fixedDeltaTime'])
    for name in RULES:
        data = (snapshots / name).read_bytes()
        stream += net_string(name) + struct.pack('<i', len(data)) + data
    computed_rules = hashlib.sha256(stream).hexdigest()
    if computed_rules != report['rulesHash']:
        raise ValueError('Native rule bundle mismatch')
    if sha(snapshots / 'RotateRings/Editor/SourceVirtualPlaytestV5.cs') != report['policySuiteHash']:
        raise ValueError('Native player policy snapshot mismatch')
    request = json.loads((directory / 'request.json').read_text(encoding='utf-8-sig'))
    for key, digest_key in [('sourcePackPath', 'inputPackSha256'), ('templatesPath', 'templatesSha256'),
                            ('candidatePackPath', 'candidatePackSha256')]:
        if not request.get(key):
            continue
        if request[key] != report['config'][key]:
            raise ValueError('Request path differs from native report: ' + key)
        if check_live and sha(request[key]) != report[digest_key]:
            raise ValueError('Changed input pack/template: ' + key)
    # JsonUtility emits defaults in addition to authored fields.
    for key, value in request.items():
        if report['config'].get(key) != value:
            raise ValueError('Native request mismatch: ' + key)
    return {'status': 'HASH_BINDINGS_VERIFIED', 'nativeReportSha256': sha(report_path),
            'rulesHash': computed_rules, 'policySuiteHash': report['policySuiteHash'],
            'checkedLiveInputs': check_live,
            'semantics': 'Checks provenance bytes and request identity, not physics parity or human realism.'}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('directory', type=Path)
    parser.add_argument('--snapshots-only', action='store_true')
    args = parser.parse_args()
    print(json.dumps(verify(args.directory, not args.snapshots_only), indent=2))


if __name__ == '__main__':
    main()
