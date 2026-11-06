from pathlib import Path
import copy,importlib.util,json
root=Path(__file__).resolve().parents[1];spec=importlib.util.spec_from_file_location('a',root/'Tools/rotate_rings_physics_v2.py');a=importlib.util.module_from_spec(spec);spec.loader.exec_module(a)
levels=json.loads((root/'Research/physics_v2/corpus500.json').read_text())['levels']
for original in levels:
 mirror=copy.deepcopy(original)
 for p in mirror['state']['parts']:
  p['x']=-p['x'];p['angle']=(180-p['angle'])%360
  for pin in p.get('attachments',[]):pin['angleDeg']=(-pin['angleDeg'])%360
 assert a.fingerprint(original)==a.fingerprint(mirror),original['id']
 assert a.descriptor(original)==a.descriptor(mirror),original['id']
print('PASS: 500 source graph fingerprints and descriptors reject global mirror equivalence')
