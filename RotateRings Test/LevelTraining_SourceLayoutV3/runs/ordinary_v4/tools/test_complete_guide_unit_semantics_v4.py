"""Actual ordinary pilot precision and complete-guide-unit regressions."""
from pathlib import Path
import copy
import json
import unittest

from audit_complete_guide_unit_semantics_v4 import audit


class NativeUnitPrecision(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        w = Path(__file__).resolve().parents[1]
        cls.candidates = {c["id"]: c for c in json.loads((w/"batches/ordinary_guide_support_pilot_v4/proposals.json").read_text())["levels"]}
        cls.sources = {l["position"]: l for l in json.loads((w/"data/source500_native.json").read_text())["levels"]}
        cls.templates = {t["id"]: t for t in json.loads((w/"data/source_templates.json").read_text())["templates"]}

    def result(self, identifier):
        c = self.candidates[identifier]
        return audit(self.sources[c["position"]], c, self.templates)

    def source_fixture(self):
        source = self.sources[21]
        c = copy.deepcopy(source)
        c["id"] = "unchanged-source-test-fixture"
        c["assemblyProvenance"] = copy.deepcopy(self.candidates[8001001]["assemblyProvenance"])
        return source, c

    def test_identical_unit_is_noop(self):
        a, b = self.source_fixture()
        self.assertTrue(audit(a, b, self.templates)["completeUnitInitialPhysicalInputMovementContractNoOp"])

    def test_float32_subulp_pose_noise_is_not_new_geometry(self):
        a, b = self.source_fixture()
        ids = b["assemblyProvenance"]["learnedPlacementCertificate"]["guideRigidGroups"][0]
        for p in b["parts"]:
            if p["id"] in ids:
                p["x"] += 1e-7
                p["angle"] += 1e-7
        self.assertTrue(audit(a, b, self.templates)["completeUnitInitialPhysicalInputMovementContractNoOp"])

    def test_straight_halfturn_cosmetic_root_not_whole_guide_unit(self):
        r = self.result(8001001)
        self.assertIn(1, r["rootGeometryCosmeticWithinNativePrecision"])
        self.assertNotIn(2, r["rootGeometryCosmeticWithinNativePrecision"])
        self.assertFalse(r["completeUnitInitialPhysicalInputMovementContractNoOp"])

    def test_second_straight_halfturn_moves_guide(self):
        r = self.result(8001002)
        self.assertIn(28, r["rootGeometryCosmeticWithinNativePrecision"])
        self.assertNotIn(31, r["rootGeometryCosmeticWithinNativePrecision"])
        self.assertFalse(r["completeUnitInitialPhysicalInputMovementContractNoOp"])

    def test_asymmetric_l_halfturn_is_real_geometry(self):
        r = self.result(8022001)
        self.assertNotIn(5, r["rootGeometryCosmeticWithinNativePrecision"])
        self.assertFalse(r["completeUnitPhysicalInputGeometryNoOp"])

    def test_complete_eight_units_have_actual_geometry_changes(self):
        for identifier in self.candidates:
            self.assertFalse(self.result(identifier)["completeUnitPhysicalInputGeometryNoOp"], identifier)


if __name__ == "__main__":
    unittest.main()
