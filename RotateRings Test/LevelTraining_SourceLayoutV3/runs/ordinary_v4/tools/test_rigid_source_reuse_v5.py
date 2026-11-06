import unittest
from rigid_source_reuse_v5 import rigid_source_reuse


class RigidSourceReuseContract(unittest.TestCase):
    def fixture(self, second_translation):
        return {"parts": [{} for _ in range(5)], "assemblyProvenance": {"tileImports": [
            {"sourceId": "a", "rotation": 10., "translation": [1., 2.], "partMapping": {1:1, 2:2}},
            {"sourceId": "a", "rotation": 370., "translation": second_translation, "partMapping": {3:3, 4:4}},
            {"sourceId": "b", "rotation": 10., "translation": [1., 2.], "partMapping": {1:5}}]}}

    def test_restored_same_source_transform_is_flagged_even_360_equivalent(self):
        report = rigid_source_reuse(self.fixture([1.+1e-14, 2.]))
        self.assertEqual(report["largestMultiImportRigidSourceRootFraction"], .8)
        self.assertTrue(report["requiresPartialSourceOriginalityReview"])
        self.assertFalse(report["numericDonorQuotaApplied"])

    def test_different_source_placement_is_not_punished_as_rigid_reconstruction(self):
        report = rigid_source_reuse(self.fixture([1.1, 2.]))
        self.assertEqual(report["largestMultiImportRigidSourceRootFraction"], 0.)


if __name__=="__main__":
    unittest.main()
