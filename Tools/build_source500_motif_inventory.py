"""Build the visual motif inventory from reviewed source500 contact sheets."""
from pathlib import Path
import hashlib
import json

ROOT = Path(__file__).resolve().parents[1]
QA = ROOT / 'Research/physics_v3/visual_qa'
MANIFESTS = [
    QA / 'source500_motif_sheets_first300/source500_positions_001-300_manifest.json',
    QA / 'source500_motif_sheets_301_500/source500_positions_301-500_manifest.json',
]

# Human visual reads; positions without an entry have no confidently named
# special silhouette from this audit, rather than an inferred generic label.
OBSERVED = {
    14: ('regular 3x4 grid', 'medium'),
    20: ('face-like head with paired eyes and central nose/mouth details', 'high'),
    21: ('tall maze-like layout with repeated parallel corridors', 'medium'),
    30: ('diamond/triangle whole-board outline with central arrangement', 'high'),
    34: ('regular row-and-column lattice', 'medium'),
    38: ('square grid/maze-like arrangement', 'medium'),
    40: ('vehicle-like outline with two wheel-like circles and a frame', 'medium'),
    72: ('regular repeated row-and-column arrangement', 'medium'),
    80: ('rocket with pointed nose, body, side fins, and exhaust detail', 'high'),
    100: ('face/sun with circular head, facial features, and crown rays', 'high'),
    110: ('butterfly with paired wings and central body', 'high'),
    120: ('rectangular device/camera-like outline with circular center', 'medium'),
    130: ('gift box with bow', 'high'),
    140: ('sailboat with mast, sails, and hull/water bands', 'high'),
    150: ('six-fold snowflake/starburst', 'high'),
    170: ('row of houses/buildings with pitched roofs and openings', 'high'),
    180: ('boxy robot/device face with antenna-like stalks and eye circles', 'medium'),
    190: ('round face with eyes, mouth, and short surrounding rays', 'high'),
    260: ('radial flower/sunburst around a center with structured base', 'medium'),
    280: ('circular six-fold geometric/star motif', 'high'),
    286: ('molecule/atom-like linked nodes around central hubs', 'medium'),
    289: ('tall monument/church-like outline with peaked top and base', 'medium'),
    290: ('symmetric winged/person-like outline with a central body', 'medium'),
    296: ('radial polygon/flower outline with connected interior nodes', 'medium'),
    300: ('umbrella-like canopy with a central shaft/handle', 'high'),
    306: ('diamond-shaped emblem enclosed by a circular outline', 'medium'),
    310: ('four-fold star/flower with radial arms', 'medium'),
    316: ('symmetric radial emblem with nested triangular and circular structure', 'medium'),
    320: ('sunburst with a central circular hub and separated rays', 'high'),
    336: ('circular geometric emblem with polygonal interior network', 'medium'),
    340: ('wide rectangular panel/dashboard-like layout with repeated horizontal bands', 'medium'),
    346: ('molecule/network diagram with connected nodes and spokes', 'high'),
    350: ('stylized standing person with head, torso, and limbs', 'high'),
    356: ('triangular/pyramidal network with central interior structure', 'medium'),
    360: ('person-like figure with a large face/head and body', 'medium'),
    366: ('diamond/circular emblem with centered crossed geometry', 'medium'),
    380: ('crown with pointed top and broad base', 'high'),
    386: ('framed circular medallion with central radial figure', 'high'),
    390: ('symmetric face/mask-like arrangement', 'medium'),
    400: ('large stylized smiling face with surrounding rays', 'high'),
    420: ('circular robot/astronaut-like figure with central body', 'medium'),
    429: ('lightbulb outline with filament-like internal pieces and base', 'high'),
    430: ('radial flower/atom-like motif with center and surrounding nodes', 'high'),
    436: ('molecular network arranged around a central node', 'high'),
    443: ('radial molecule/atom-like diagram with linked nodes', 'high'),
    446: ('molecular/network diagram with two central hubs and spokes', 'high'),
    460: ('sailing ship with sails, mast, and water-like bands', 'medium'),
    480: ('train/locomotive with cab, circular wheel, and track-like base', 'high'),
    490: ('butterfly with paired wings and central body', 'high'),
    496: ('diamond/hexagon outline with central radial spokes', 'high'),
    500: ('split scene with two small face-like forms above a house/building scene', 'medium'),
}

# Tentative visual families; deliberately separated from direct observations.
CLUSTERS = {
    'face_head': [20, 100, 180, 190, 350, 360, 390, 400],
    'radial_symmetry': [150, 260, 280, 296, 310, 316, 320, 336, 366, 386, 430, 496],
    'molecular_network': [286, 346, 436, 443, 446],
    'built_object_or_scene': [120, 130, 140, 170, 289, 300, 340, 380, 409, 420, 429, 460, 470, 480, 500],
    'vehicle_or_vessel': [40, 140, 460, 480],
    'grid_or_lattice': [14, 34, 38, 72, 148, 163, 168, 178, 200, 258, 268, 278, 288, 303, 308, 313, 318, 328, 338, 343, 348, 358, 368, 373, 378, 383, 388, 393, 398, 403, 413, 418, 423, 438, 448, 463, 471, 476],
    'butterfly': [110, 490],
}

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    levels = []
    for manifest in MANIFESTS:
        data = json.loads(manifest.read_text(encoding='utf-8'))
        for row in data['levels']:
            pos = int(row['position'])
            obs = OBSERVED.get(pos)
            clusters = [name for name, positions in CLUSTERS.items() if pos in positions]
            levels.append({
                'sourceId': str(row['sourceId']),
                'position': pos,
                'parts': int(row['parts']),
                'observedLabel': obs[0] if obs else None,
                'observedConfidence': obs[1] if obs else None,
                'inferredClusters': clusters,
                'reviewStatus': 'reviewed_visual_contact_sheet',
            })
    levels.sort(key=lambda x: x['position'])
    renderer = ROOT / 'Tools/render_layout_diagnostics_v3.py'
    generator = ROOT / 'Tools/render_source500_motif_sheets.py'
    output = {
        'schemaVersion': 1,
        'scope': {'firstPosition': 1, 'lastPosition': 500},
        'method': 'Exact solids renderer contact sheets; observed silhouettes and inferred clusters are stored separately. Blank observedLabel means no confidently named special silhouette in this visual review.',
        'evidence': {
            'renderer': str(renderer),
            'rendererSha256': sha(renderer),
            'sheetGenerator': str(generator),
            'manifests': [{'path': str(p), 'sha256': sha(p)} for p in MANIFESTS],
            'sheets': [
                'Research/physics_v3/visual_qa/source500_motif_sheets_first300/',
                'Research/physics_v3/visual_qa/source500_motif_sheets_301_500/',
            ],
        },
        'levels': levels,
    }
    out = QA / 'source500_motif_inventory_first500.json'
    out.write_text(json.dumps(output, indent=2), encoding='utf-8')
    print(f'wrote {out} ({len(levels)} positions)')

if __name__ == '__main__':
    main()
