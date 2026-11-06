"""Strict atomic socket identity regression; no gameplay solution assertions."""
from pathlib import Path
import copy,json,unittest
from macro_assembly_v3 import socket_key

ROOT=Path(__file__).resolve().parents[1];DATA=ROOT/'data' if (ROOT/'data/source500_native.json').exists() else ROOT/'Research/physics_v3/full500'
class NativeSocketTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  pack=DATA/('source500_native.json' if (DATA/'source500_native.json').exists() else 'source_parity_pack.json');cls.level=json.loads(pack.read_text(encoding='utf8'))['levels'][0];cls.part=next(p for p in cls.level['parts'] if p['kind']=='ring');cls.templates={t['id']:t for t in json.loads((DATA/'source_templates.json').read_text(encoding='utf8'))['templates']}
 def test_rgb_tint_can_share_one_exact_native_atomic_asset(self):
  part=copy.deepcopy(self.part);templates=dict(self.templates);t=copy.deepcopy(templates[part['templateId']]);t['id']='regression-other-palette';t['sourceVertexColor']['r']=.123;templates[t['id']]=t;part['templateId']=t['id'];self.assertEqual(socket_key(self.part,templates,self.level),socket_key(part,templates,self.level))
 def test_input_band_geometry_cannot_be_ignored(self):
  part=copy.deepcopy(self.part);part['inputShapes'][0]['points'][0]['x']+=.01;self.assertNotEqual(socket_key(self.part,self.templates,self.level),socket_key(part,self.templates,self.level))
 def test_collider_geometry_cannot_be_ignored(self):
  part=copy.deepcopy(self.part);templates=dict(self.templates);t=copy.deepcopy(templates[part['templateId']]);t['id']='regression-other-shape';t['physicalShapes'][0]['points'][0]['x']+=.01;templates[t['id']]=t;part['templateId']=t['id'];self.assertNotEqual(socket_key(self.part,templates,self.level),socket_key(part,templates,self.level))
 def test_chain_lock_state_cannot_be_ignored(self):
  part=copy.deepcopy(self.part);part['chainLockCount']+=1;self.assertNotEqual(socket_key(self.part,self.templates,self.level),socket_key(part,self.templates,self.level))
 def test_rigidbody_constraints_cannot_be_ignored(self):
  part=copy.deepcopy(self.part);part['sourceRigidbody']['constraints']=0;self.assertNotEqual(socket_key(self.part,self.templates,self.level),socket_key(part,self.templates,self.level))
if __name__=='__main__':unittest.main()
