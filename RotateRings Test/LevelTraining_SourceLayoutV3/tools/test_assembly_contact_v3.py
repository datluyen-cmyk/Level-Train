"""Real-source raw geometry calibration regressions; no physics labels."""
from pathlib import Path
import copy,json,math,sys,unittest
from assembly_contact_audit_v3 import NativeGeometrySeamAudit

ROOT=Path(__file__).resolve().parents[1]
DATA=ROOT/'data' if (ROOT/'data/source500_native.json').exists() else ROOT/'Research/physics_v3/full500'

class SourceSeamTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  pack=DATA/('source500_native.json' if (DATA/'source500_native.json').exists() else 'source_parity_pack.json');cls.levels=json.loads(pack.read_text(encoding='utf8'))['levels'];cls.templates={t['id']:t for t in json.loads((DATA/'source_templates.json').read_text(encoding='utf8'))['templates']};cls.audit=NativeGeometrySeamAudit(cls.templates,cls.levels)
 def source(self,sid):
  l=copy.deepcopy(next(l for l in self.levels if l['sourceId']==sid));l['assemblyProvenance']={'tileImports':[{'sourceId':sid,'partMapping':{p['id']:p['id'] for p in l['parts']}}]}
  for b in l['bombs']:b['atomicProvenance']={'sourceId':sid,'sourceColliderId':b['sourceColliderId']}
  return l
 def test_all_representative_source_pairs_retain_authored_penetration(self):
  for sid in ['10001','10010','10012','10154','10109','10166','10190','140004']:
   with self.subTest(source=sid):self.assertTrue(self.audit.check(self.source(sid))['rawGeometrySeamPassed'])
 def test_imported_pair_evidence_is_rigid_transform_invariant(self):
  l=self.source('10166');r=math.radians(37);c,s=math.cos(r),math.sin(r)
  for p in l['parts']:x,y=p['x'],p['y'];p['x']=c*x-s*y+9;p['y']=s*x+c*y-5;p['angle']+=37
  self.assertTrue(self.audit.check(l)['rawGeometrySeamPassed'])
 def test_duplicate_atomic_root_cannot_inflate_part_count(self):
  l=self.source('10010');p=copy.deepcopy(next(p for p in l['parts'] if p['kind']=='ring'));p['id']=max(p['id'] for p in l['parts'])+1;l['parts'].append(p);report=self.audit.check(l);self.assertFalse(report['rawGeometrySeamPassed']);self.assertTrue(report['duplicateWholeRootPairs']);self.assertTrue(report['unexpectedSolidOverlapCount'])
 def test_missing_shape_binding_fails(self):
  l=self.source('10010');self.assertTrue(l['links']);l['links'][0]['sourceShapeIndices']=[99999];report=self.audit.check(l);self.assertFalse(report['rawGeometrySeamPassed']);self.assertTrue(report['missingFeatureBindings'])
 def test_bomb_provenance_cannot_be_omitted(self):
  l=self.source('10109');self.assertTrue(l['bombs']);l['bombs'][0].pop('atomicProvenance');report=self.audit.check(l);self.assertFalse(report['rawGeometrySeamPassed']);self.assertTrue(report['unmappedBombs'])

if __name__=='__main__':unittest.main()
