import unittest
from incoming_root_grammar_v6 import atomic_units


class AtomicOwnerUnits(unittest.TestCase):
    def test_outgoing_dag_does_not_expand_owner_into_descendants(self):
        level={"parts":[{"id":i}for i in range(4)],"links":[{"source":0,"target":1},{"source":0,"target":2},{"source":2,"target":3}],"guideLinks":[]}
        self.assertEqual(atomic_units(level),[[0],[1],[2],[3]])

    def test_complete_native_guide_and_directed_cycle_are_atomic(self):
        level={"parts":[{"id":i}for i in range(5)],"links":[{"source":0,"target":1},{"source":1,"target":0},{"source":1,"target":2},{"source":3,"target":4}],
            "guideLinks":[{"guideOwnerId":2,"lockedOwnerId":3}]}
        self.assertEqual(atomic_units(level),[[0,1],[2,3],[4]])


if __name__=="__main__":unittest.main()
