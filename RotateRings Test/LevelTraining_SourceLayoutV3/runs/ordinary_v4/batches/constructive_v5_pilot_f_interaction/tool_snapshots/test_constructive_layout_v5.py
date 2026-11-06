"""Constructor invariants with labeled small fixtures; no engine or fitting."""
from types import SimpleNamespace
import copy
import unittest

import numpy as np

from constructive_layout_v5 import native_closure, closed_library, empty_level, relation_options, posed_field_safe, continuation_options, joint_options


class ConstructorContract(unittest.TestCase):
    def level(self):
        return {"sourceId": "ordinary-fixture", "parts": [{"id": i} for i in range(1, 7)],
            "links": [{"source": 1, "target": 2}, {"source": 5, "target": 1}],
            "guideLinks": [{"guideOwnerId": 3, "lockedOwnerId": 2}, {"guideOwnerId": 3, "lockedOwnerId": 4}]}

    def test_outgoing_and_all_guide_slots_close_recursively(self):
        self.assertEqual(native_closure(self.level(), [1]), [1, 2, 3, 4])

    def test_absent_incoming_pin_owner_is_not_invented(self):
        self.assertNotIn(5, native_closure(self.level(), [1]))

    def test_full_board_motif_cannot_seed(self):
        source = self.level()
        tiles = [{"sourceId": source["sourceId"], "partIds": [1], "descriptor": [1], "center": [0., 0.], "heading": 0., "width": 1.},
            {"sourceId": source["sourceId"], "partIds": [1, 5, 6], "descriptor": [3], "center": [0., 0.], "heading": 0., "width": 1.}]
        saved = SimpleNamespace(tiles=tiles, levels={source["sourceId"]: source})
        model, eligible, unsupported = closed_library(saved, {})
        self.assertEqual(eligible, [0])
        self.assertEqual(unsupported["closureEqualsWholeSourceBoard"], 1)
        self.assertEqual(saved.tiles[0]["partIds"], [1])
        self.assertEqual(model.tiles[0]["partIds"], [1, 2, 3, 4])

    def test_empty_start_contains_no_reference_geometry(self):
        target = {"position": 18, "field": np.zeros((10, 2, 2)), "targetParts": 20.5, "span": 10., "heading": 0.}
        out = empty_level(1, 18, target, {})
        self.assertEqual([len(out[k]) for k in ("parts", "links", "guideLinks", "bombs")], [0]*4)
        self.assertFalse(out["assemblyProvenance"]["sourceScaffoldUsed"])
        self.assertEqual(out["assemblyProvenance"]["organizationTarget"]["targetParts"], 20.5)

    def test_saved_relation_pose_exactly_defines_transform(self):
        class Relation:
            def sample_pose(self, context, rng):
                return {"pose": [2., -1., 0., 1.], "learnedRecordIndex": 7}
        tile = {"descriptor": [3], "heading": 15., "center": [4., 5.], "width": 2.}
        model = SimpleNamespace(tiles=[tile], pose_model=Relation())
        anchor = {"center": [10., 20.], "heading": 0., "width": 1., "prototypeDescriptor": [2]}
        option = relation_options(model, {}, [anchor], [0], [0.], [1.], np.random.default_rng(1), 1, "bound-model")[0]
        self.assertEqual(option["rotation"], 75.)
        self.assertEqual(option["sampledRelativePose"], [2., -1., 0., 1.])
        self.assertEqual(option["recipientContext"], [1., 2, 3])
        from macro_assembly_v3 import pose_matrix
        center = pose_matrix(option["rotation"])@np.array(tile["center"])+option["translation"]
        np.testing.assert_allclose(center, [14., 18.], atol=1e-12)

    def test_field_rounded_outer_pixel_is_safely_bounds_checked(self):
        from unittest.mock import patch
        from composition_field_v3 import H, W
        level = {"parts": [{"id": 1, "kind": "ring", "x": .649, "y": 0., "angle": 0.}], "links": [], "guideLinks": []}
        with patch("constructive_layout_v5.native_body_fields", return_value=np.zeros((4, H, W))), patch("constructive_layout_v5.relation_fields", return_value=np.zeros((2, H, W))):
            maps, angles = posed_field_safe(level, 0., 1., 1., {})
        self.assertEqual(maps.shape, (10, H, W))
        self.assertEqual(float(maps[:4].sum()), 0.)
        self.assertEqual(float(angles.sum()), 1.)

    def test_native_triple_uses_parent_incoming_frame_without_pose_jitter(self):
        class Pattern:
            def sample_pose(self, context, rng):
                return {"pose": [0., 1., 1., 0.], "learnedRecordIndex": 3}
        tile = {"descriptor": [4], "heading": 20., "center": [1., 2.], "width": 1.}
        model = SimpleNamespace(tiles=[tile])
        clusters = [{"center": [0., 0.], "heading": 0., "width": 1., "prototypeDescriptor": [2], "parentClusterIndex": None},
            {"center": [2., 0.], "heading": 0., "width": 1., "prototypeDescriptor": [3], "parentClusterIndex": 0}]
        option = continuation_options(model, {"model": Pattern()}, {}, clusters, [0], [0.], [1.], np.random.default_rng(1), 1, "bound-pattern")[0]
        from macro_assembly_v3 import pose_matrix
        center = pose_matrix(option["rotation"])@np.array(tile["center"])+option["translation"]
        np.testing.assert_allclose(center, [2., 2.], atol=1e-12)
        self.assertEqual(option["parentClusterIndex"], 0)
        self.assertEqual(option["anchorClusterIndex"], 1)
        self.assertEqual(option["continuationRecordIndex"], 3)

    def test_joint_pair_draw_controls_both_tile_identity_and_exact_native_pose(self):
        class Joint:
            def sample(self, query, rng):
                self.query = query
                return {"recordIndex": 9, "neighborProbability": .25,
                    "record": {"targetTile": 1, "pose": [2., -1., 0., 1.]}}
        estimator = Joint()
        tiles = [{"descriptor": [900], "heading": 0., "center": [0., 0.], "width": 30.},
            {"descriptor": [3], "heading": 15., "center": [4., 5.], "width": 2.}]
        anchor = {"center": [10., 20.], "heading": 0., "width": 1., "prototypeDescriptor": [2]}
        options = joint_options(SimpleNamespace(tiles=tiles), {"models": {"pair": estimator}}, [anchor], {0, 1},
            [1.], np.random.default_rng(1), 1, 0, "joint-hash")
        option = options[0]
        self.assertEqual(option["tileIndex"], 1)
        self.assertEqual(estimator.query, [1., 2])
        self.assertEqual(option["jointRecordIndex"], 9)
        from macro_assembly_v3 import pose_matrix
        center = pose_matrix(option["rotation"])@np.asarray(tiles[1]["center"])+option["translation"]
        np.testing.assert_allclose(center, [14., 18.], atol=1e-12)

    def test_joint_triple_draw_uses_known_incoming_frame_and_selected_target(self):
        class Joint:
            def sample(self, query, rng):
                self.query = query
                return {"recordIndex": 7, "neighborProbability": .5,
                    "record": {"targetTile": 0, "pose": [0., 1., 1., 0.]}}
        estimator = Joint()
        tile = {"descriptor": [999], "heading": 20., "center": [1., 2.], "width": 40.}
        clusters = [{"center": [0., 0.], "heading": 0., "width": 1., "prototypeDescriptor": [2], "parentClusterIndex": None},
            {"center": [2., 0.], "heading": 0., "width": 1., "prototypeDescriptor": [3], "parentClusterIndex": 0}]
        option = joint_options(SimpleNamespace(tiles=[tile]), {"models": {"triple": estimator}}, clusters, {0},
            [1.], np.random.default_rng(1), 0, 1, "joint-hash")[0]
        self.assertNotIn(999, estimator.query)
        self.assertAlmostEqual(estimator.query[-2], np.log(2.))
        from macro_assembly_v3 import pose_matrix
        center = pose_matrix(option["rotation"])@np.asarray(tile["center"])+option["translation"]
        np.testing.assert_allclose(center, [2., 2.], atol=1e-12)
        self.assertEqual(option["jointRecordIndex"], 7)


if __name__ == "__main__":
    unittest.main()
