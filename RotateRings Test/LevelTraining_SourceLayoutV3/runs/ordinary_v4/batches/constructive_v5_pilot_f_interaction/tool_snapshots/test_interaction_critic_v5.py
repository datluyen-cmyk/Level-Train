import unittest
import networkx as nx
import numpy as np
from interaction_critic_v5 import graph_statistics, separated_components


class StaticInteractionFeatures(unittest.TestCase):
    def test_chain_bottleneck_and_disconnected_reachability_are_distinct(self):
        chain = graph_statistics(nx.path_graph(4))
        disconnected = graph_statistics(nx.empty_graph(4))
        self.assertEqual(chain[7], 1.)
        self.assertEqual(chain[8], .5)
        self.assertEqual(chain[11], 1.)
        self.assertEqual(disconnected[2], 1.)
        self.assertEqual(disconnected[11], 0.)

    def test_synthetic_component_scatter_preserves_internal_native_pose(self):
        level = {"parts": [{"id": i, "kind": "ring", "x": float(i), "y": 0., "angle": float(i)*3} for i in range(4)],
            "links": [{"source": 0, "target": 1}, {"source": 2, "target": 3}], "guideLinks": []}
        moved = separated_components(level, np.random.default_rng(1))
        for a, b in ((0, 1), (2, 3)):
            self.assertEqual(moved["parts"][b]["x"]-moved["parts"][a]["x"], 1.)
        self.assertEqual([p["angle"] for p in moved["parts"]], [p["angle"] for p in level["parts"]])
        self.assertEqual(level["parts"][0]["x"], 0.)


if __name__=="__main__":
    unittest.main()
