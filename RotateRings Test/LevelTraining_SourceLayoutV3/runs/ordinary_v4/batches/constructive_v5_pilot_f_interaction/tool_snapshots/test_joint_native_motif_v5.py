"""Known-frontier information and joint-record correlation regression fixtures."""
import unittest
import numpy as np
from joint_native_motif_v5 import pair_query, triple_query, JointNativeSampler


class JointNativeContract(unittest.TestCase):
    def test_known_frontier_queries_have_no_next_tile_argument(self):
        a = {"descriptor": [1., 2.], "heading": 10., "width": 2.}
        b = {"descriptor": [3., 4.], "heading": 20., "width": 3.}
        self.assertEqual(pair_query([.5], b), [.5, 3., 4.])
        query = triple_query([.5], a, b, 20., 6.)
        self.assertEqual(len(query), 9)
        self.assertAlmostEqual(query[-2], np.log(2.))
        self.assertEqual(query[-1], 0.)

    def test_sample_keeps_native_tile_and_pose_in_same_observed_record(self):
        records = [{"query": [0.], "targetTile": 11, "pose": [1., 2., 1., 0.]},
                   {"query": [2.], "targetTile": 22, "pose": [-3., 4., 0., 1.]}]
        model = JointNativeSampler().fit(records)
        rng = np.random.default_rng(3)
        for _ in range(50):
            selected = model.sample([1.], rng)
            self.assertIs(selected["record"], records[selected["recordIndex"]])
            self.assertIn((selected["record"]["targetTile"], tuple(selected["record"]["pose"])),
                {(11, (1., 2., 1., 0.)), (22, (-3., 4., 0., 1.))})


if __name__ == "__main__":
    unittest.main()
