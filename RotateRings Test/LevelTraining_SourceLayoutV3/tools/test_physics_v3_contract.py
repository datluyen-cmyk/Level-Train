"""Regress decompiled Pull decisions using explicit collision-query fixtures.

This verifies decision ordering/identity, not Unity geometric parity.
"""
from pathlib import Path
import copy,json,math,unittest
from rotate_rings_physics_v3 import *
from rotate_rings_physics_v3 import _move_one_step,_rollback

class QueryFixture:
 certification='controlled contract fixture; not runtime parity'
 def __init__(self):self.overlap=set();self.hits=[];self.separation=Distance(1.,(1.,0.));self.after_release=None
 def overlaps(self,*args):return set(self.overlap)
 def cast(self,*args):return self.hits
 def distance(self,*args):return self.separation
 def release_settlement(self,scene,s,pid):
  if self.after_release:self.overlap=set(self.after_release)

class ContractTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  root=Path(__file__).resolve().parents[1];data=root/'data' if (root/'data/source500_native.json').exists() else root/'Research/physics_v3';ts=json.loads((data/'source_templates.json').read_text());levels=json.loads((data/('source500_native.json' if (data/'source500_native.json').exists() else 'source_parity_pack.json')).read_text())['levels'];cls.scene=Scene(next(l for l in levels if l['sourceId']=='140004'),{t['id']:t for t in ts['templates']})
 def setUp(self):self.oracle=QueryFixture();self.s=self.scene.new_session(self.oracle);self.pid=next(iter(self.s.active))
 def test_source_native_axes_are_signed_and_rotated(self):
  a=pull_axis({'lockDirectionType':0},90);b=pull_axis({'lockDirectionType':1},90)
  self.assertAlmostEqual(a[0],-1);self.assertAlmostEqual(a[1],0);self.assertAlmostEqual(b[0],0);self.assertAlmostEqual(b[1],-1)
  positive=copy.deepcopy(self.s);negative=copy.deepcopy(self.s)
  apply_pull_gesture(self.scene,positive,self.oracle,self.pid,.36,frames=1,release=False)
  apply_pull_gesture(self.scene,negative,self.oracle,self.pid,-.36,frames=1,release=False)
  for key in ['x','y']:self.assertAlmostEqual(positive.poses[self.pid][key]+negative.poses[self.pid][key],2*self.s.poses[self.pid][key])
 def test_custom_flags_are_locks_in_world_coordinates(self):
  self.assertEqual(pull_axis({'lockDirectionType':2,'lockLeft':True,'lockRight':False,'lockUp':True,'lockDown':True},83),(1.,0.))
  self.assertEqual(pull_axis({'lockDirectionType':2,'lockLeft':True,'lockRight':True,'lockUp':True,'lockDown':False},83),(0.,-1.))
 def test_only_exact_bound_headwalls_are_exempt_not_stems(self):
  found=False
  for ref,role in self.scene.roles.items():
   if self.scene.shape(ref)['role']=='head_wall':
    self.s.active[self.pid][ref]=Binding(ref,ref.owner);self.assertFalse(cast_hit_allowed(self.scene,self.s,self.pid,ref));found=True;break
  self.assertTrue(found)
  stem=next(ref for ref in self.scene.roles if self.scene.shape(ref)['role']=='stem')
  self.assertTrue(cast_hit_allowed(self.scene,self.s,self.pid,stem))
 def test_ignored_raw_cast_still_costs_skin_without_stop(self):
  ref=next(ref for ref in self.scene.roles if self.scene.shape(ref)['role']=='head_wall');self.s.active[self.pid][ref]=Binding(ref,ref.owner);self.oracle.overlap={ref};self.oracle.hits=[CastHit(ref,0.)]
  distance,blocked,escape=_move_one_step(self.scene,self.s,self.oracle,self.pid,(1.,0.),.12)
  self.assertAlmostEqual(distance,.11);self.assertFalse(blocked);self.assertFalse(escape)
 def test_initial_overlap_escape_is_not_blanket_rejection(self):
  ref=next(ref for ref in self.scene.roles if self.scene.shape(ref)['role']=='stem');self.oracle.hits=[CastHit(ref,0.)];self.oracle.separation=Distance(-.2,(-1.,0.));start=self.s.poses[self.pid]['x']
  returned,blocked,escape=_move_one_step(self.scene,self.s,self.oracle,self.pid,(1.,0.),.12)
  self.assertEqual(returned,0);self.assertFalse(blocked);self.assertTrue(escape);self.assertAlmostEqual(self.s.poses[self.pid]['x'],start+.12)
 def test_substep_budget_subtracts_actual_movement(self):
  ref=next(ref for ref in self.scene.roles if self.scene.shape(ref)['role']=='head_wall');self.s.active[self.pid][ref]=Binding(ref,ref.owner);self.oracle.overlap={ref};self.oracle.hits=[CastHit(ref,0.)]
  result=apply_pull_gesture(self.scene,self.s,self.oracle,self.pid,.36,frames=1,release=False)
  self.assertAlmostEqual(result['moved'],.35)
 def test_native_rollback_formula_is_distance_subtraction(self):
  self.s.poses[self.pid]['x']+=1.;ref=next(ref for ref in self.scene.roles if self.scene.shape(ref)['role']=='stem');self.oracle.hits=[CastHit(ref,.02)];now=self.s.poses[self.pid]['x'];amount=_rollback(self.scene,self.s,self.oracle,self.pid,(1.,0.),1.,(now-1.,self.s.poses[self.pid]['y']))
  self.assertAlmostEqual(amount,.03)
 def test_release_resolved_contacts_decide_solve(self):
  # CanCollect does not appear in solver; an active exact binding does.
  ref=next(iter(self.s.configured[self.pid]));self.oracle.after_release={ref}
  result=apply_pull_gesture(self.scene,self.s,self.oracle,self.pid,.36,frames=1)
  self.assertTrue(result['valid']);self.assertFalse(result['removed']);self.assertIn(ref,self.s.active[self.pid])
 def test_missing_native_queries_never_fall_back_to_polygons(self):
  native=NativeQueryReplay({})
  with self.assertRaises(MissingNativeQuery):native.overlaps(self.scene,self.s,self.pid)

if __name__=='__main__':unittest.main()
