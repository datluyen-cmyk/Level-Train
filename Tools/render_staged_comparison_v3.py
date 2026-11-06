"""Freeze and render preliminary source/candidate pairs for visual review."""
import argparse
import hashlib
import json
import math
from pathlib import Path
from PIL import Image, ImageDraw
from render_layout_diagnostics_v3 import panel


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--pack', type=Path, required=True)
    parser.add_argument('--sources', type=Path, required=True)
    parser.add_argument('--templates', type=Path, required=True)
    parser.add_argument('--output-dir', type=Path, required=True)
    args = parser.parse_args()
    args.output_dir.mkdir(parents=True, exist_ok=False)
    contents = args.pack.read_bytes()
    (args.output_dir/'proposals_snapshot.json').write_bytes(contents)
    proposals = json.loads(contents)['levels']
    sources = {l['position']: l for l in json.loads(args.sources.read_text(encoding='utf8'))['levels']}
    templates = {t['id']: t for t in json.loads(args.templates.read_text(encoding='utf8'))['templates']}
    manifest = {'scope': 'SOLID_COLLIDER_DIAGNOSTIC_NOT_UNITY_RENDERING_OR_ACCEPTANCE',
                'acceptedCount': 0, 'proposalCount': len(proposals), 'sheets': [],
                'hashes': {str(args.pack): hashlib.sha256(contents).hexdigest(),
                           **{str(p): hashlib.sha256(p.read_bytes()).hexdigest()
                              for p in [args.sources, args.templates, Path(__file__),
                                        Path(__file__).with_name('render_layout_diagnostics_v3.py')]}}}
    for start in range(0, len(proposals), 6):
        batch = proposals[start:start+6]
        sheet = Image.new('RGB', (1600, math.ceil(len(batch)/2)*550), '#eeeeee')
        rows = []
        for index, candidate in enumerate(batch):
            position = candidate['position']
            source = sources[position]
            for side, level in enumerate((source, candidate)):
                image = panel(level, templates)
                ImageDraw.Draw(image).text((10, 46), f'POSITION {position} | '+('ORIGINAL SOURCE' if side == 0 else 'PRELIMINARY CANDIDATE'), fill='#111111')
                slot = index*2+side
                sheet.paste(image, ((slot % 4)*400, (slot//4)*550))
            rows.append({'position': position, 'sourceId': source['sourceId'],
                         'candidateId': candidate['id'], 'sourcePanel': index*2,
                         'candidatePanel': index*2+1})
        name = f'comparison_{start//6+1:02d}.png'
        sheet.save(args.output_dir/name)
        manifest['sheets'].append({'file': name, 'panels': rows})
    (args.output_dir/'manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf8')
    print(json.dumps({'proposals': len(proposals), 'sheets': len(manifest['sheets']),
                      'snapshotSha256': hashlib.sha256(contents).hexdigest()}))


if __name__ == '__main__':
    main()
