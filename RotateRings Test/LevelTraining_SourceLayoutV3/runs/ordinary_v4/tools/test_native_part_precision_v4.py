"""Native part precision screen: real pilot and labeled equivalent fixtures."""
from pathlib import Path
import copy
import json
import unittest

from audit_native_part_precision_v4 import compare


class NativePartPrecision(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        w = Path(__file__).resolve().parents[1]
        cls.candidates = json.loads((w/"batches/ordinary_native_part_support_pilot_v4b/proposals.json").read_text())["levels"]
        cls.sources = {l["position"]: l for l in json.loads((w/"data/source500_native.json").read_text())["levels"]}
        cls.templates = {t["id"]: t for t in json.loads((w/"data/source_templates.json").read_text())["templates"]}

    def source_fixture(self):
        candidate = self.candidates[0]
        source = self.sources[candidate["position"]]
        out = copy.deepcopy(source)
        out["id"] = "unchanged-source-test-fixture"
        out["softNativePartPolicyV4"] = copy.deepcopy(candidate["softNativePartPolicyV4"])
        return source, out

    def test_identical_source_is_noop(self):
        a, b = self.source_fixture()
        self.assertTrue(compare(a, b, self.templates)["nativeInitialSemanticNoOpWithinPrecision"])

    def test_template_id_rename_cannot_create_geometric_novelty(self):
        a, b = self.source_fixture()
        tid = b["softNativePartPolicyV4"]["sourceRelativeRawMetrics"]["changedRootIds"][0]
        part = next(p for p in b["parts"] if p["id"] == tid)
        templates = dict(self.templates)
        templates["equivalent-native-template-test-fixture"] = templates[part["templateId"]]
        part["templateId"] = "equivalent-native-template-test-fixture"
        self.assertTrue(compare(a, b, templates)["nativeInitialSemanticNoOpWithinPrecision"])

    def test_subulp_rotation_noise_is_noop(self):
        a, b = self.source_fixture()
        tid = b["softNativePartPolicyV4"]["sourceRelativeRawMetrics"]["changedRootIds"][0]
        next(p for p in b["parts"] if p["id"] == tid)["angle"] += 1e-7
        self.assertTrue(compare(a, b, self.templates)["nativeInitialSemanticNoOpWithinPrecision"])

    def test_all_executed_pilot_parts_have_initial_native_changes(self):
        for candidate in self.candidates:
            self.assertFalse(compare(self.sources[candidate["position"]], candidate, self.templates)["nativeInitialSemanticNoOpWithinPrecision"], candidate["id"])


if __name__ == "__main__":
    unittest.main()
