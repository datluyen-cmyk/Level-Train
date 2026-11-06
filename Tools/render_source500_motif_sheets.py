"""Render source500 contact sheets through render_layout_diagnostics_v3.panel.

The diagnostic renderer remains the single geometry renderer; this wrapper
only chooses the first-300 source rows, labels each panel with source ID and
position, and groups the rendered panels into reviewable sheets.
"""
from pathlib import Path
import argparse
import importlib.util
import json
import math

from PIL import Image, ImageDraw


def load_panel(renderer_path):
    spec = importlib.util.spec_from_file_location('native_solids_renderer', renderer_path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module.panel


def main():
    root = Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser()
    parser.add_argument('--pack', type=Path,
                        default=root / 'RotateRings Test/LevelTraining_SourceLayoutV3/data/source500_native.json')
    parser.add_argument('--templates', type=Path,
                        default=root / 'RotateRings Test/LevelTraining_SourceLayoutV3/data/source_templates.json')
    parser.add_argument('--renderer', type=Path, default=Path(__file__).with_name('render_layout_diagnostics_v3.py'))
    parser.add_argument('--output-dir', type=Path,
                        default=root / 'Research/physics_v3/visual_qa/source500_motif_sheets_first300')
    parser.add_argument('--first-position', type=int, default=1)
    parser.add_argument('--last-position', type=int, default=300)
    parser.add_argument('--columns', type=int, default=4)
    parser.add_argument('--rows', type=int, default=4)
    parser.add_argument('--panel-width', type=int, default=300)
    parser.add_argument('--panel-height', type=int, default=412)
    args = parser.parse_args()

    pack = json.loads(args.pack.read_text(encoding='utf-8'))
    templates_data = json.loads(args.templates.read_text(encoding='utf-8'))
    templates = {item['id']: item for item in templates_data['templates']}
    render_panel = load_panel(args.renderer)
    levels = sorted((level for level in pack['levels']
                     if args.first_position <= int(level.get('position', 0)) <= args.last_position),
                    key=lambda level: int(level['position']))
    per_sheet = args.columns * args.rows
    args.output_dir.mkdir(parents=True, exist_ok=True)
    manifest = []

    for sheet_index, offset in enumerate(range(0, len(levels), per_sheet), 1):
        sheet_levels = levels[offset:offset + per_sheet]
        sheet = Image.new('RGB', (args.columns * args.panel_width,
                                  args.rows * (args.panel_height + 24)), '#eeeeee')
        for panel_index, level in enumerate(sheet_levels):
            panel_source = dict(level)
            panel_source['sourceId'] = f"{level['sourceId']} | pos {int(level['position']):03d}"
            im = render_panel(panel_source, templates, args.panel_width, args.panel_height)
            x = (panel_index % args.columns) * args.panel_width
            y = (panel_index // args.columns) * (args.panel_height + 24)
            sheet.paste(im, (x, y))
            ImageDraw.Draw(sheet).text((x + 8, y + args.panel_height + 3),
                                       f"parts={len(level['parts'])} source={level['sourceId']} position={level['position']}",
                                       fill='#202020')
            manifest.append({
                'sourceId': str(level['sourceId']),
                'position': int(level['position']),
                'parts': len(level['parts']),
                'sheetIndex': sheet_index,
                'panelIndex': panel_index,
            })
        out = args.output_dir / f"source500_positions_{args.first_position:03d}-{args.last_position:03d}_sheet_{sheet_index:02d}.png"
        sheet.save(out)

    manifest_path = args.output_dir / f"source500_positions_{args.first_position:03d}-{args.last_position:03d}_manifest.json"
    manifest_path.write_text(json.dumps({
        'renderSource': str(args.renderer),
        'pack': str(args.pack),
        'templates': str(args.templates),
        'firstPosition': args.first_position,
        'lastPosition': args.last_position,
        'panelSize': [args.panel_width, args.panel_height],
        'sheetLayout': [args.columns, args.rows],
        'levels': manifest,
    }, indent=2), encoding='utf-8')
    print(json.dumps({'renderedLevels': len(levels),
                      'sheetCount': math.ceil(len(levels) / per_sheet),
                      'manifest': str(manifest_path)}, indent=2))


if __name__ == '__main__':
    main()
