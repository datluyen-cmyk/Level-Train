"""Real saved ordinary-model provenance regressions; creates no proposals."""
from pathlib import Path
import copy
import json
import unittest

import joblib
import numpy as np

from native_part_proposal_soft_v4 import eligible_ids, sampled_macro_audit, captured_angles
from native_asset_variant_v3 import body_key, state
from layout_learning_v3 import level_context


class SavedNativePartContract(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        w = Path(__file__).resolve().parents[1]
        cls.macro = joblib.load(w/"models/macro_assembly.joblib")
        cls.local = joblib.load(w/"models/composition_pose.joblib")["model"]
        cls.templates = {t["id"]: t for t in json.loads((w/"data/source_templates.json").read_text())["templates"]}
        cls.reference = next(l for l in cls.macro.levels.values() if l["position"] == 23)
        options = [(i, t) for i, t in enumerate(cls.macro.tiles) if t["sourceId"] == cls.reference["sourceId"] and eligible_ids(cls.reference)&set(t["partIds"])]
        for index, tile in options:
            neighbors = cls.macro.nn.kneighbors(cls.macro.context_scaler.transform([level_context(cls.reference)+tile["descriptor"]]))[1][0]
            if index in neighbors:
                break
        else:
            raise AssertionError("No actual self conditional macro support for fixture")
        part = min(eligible_ids(cls.reference)&set(tile["partIds"]))
        cls.evidence = {"recipientMacroIndex": index, "donorMacroIndex": index, "recipientPartId": part, "donorPartId": part,
            "donorSourceId": cls.reference["sourceId"], "capturedAngle": {"kind": "recipientNativeBodyPose"}}
        cls.actual = copy.deepcopy(cls.reference)
        next(p for p in cls.actual["parts"] if p["id"] == part)["atomicProvenance"] = {"sourceId": cls.reference["sourceId"], "partId": part}

    def check(self, evidence=None, actual=None):
        return sampled_macro_audit(evidence or self.evidence, self.reference, actual or self.actual, self.macro, self.templates, None)

    def test_real_saved_native_macro_positive(self):
        self.assertTrue(self.check()["passed"])

    def test_independent_angle_tamper_rejected(self):
        actual = copy.deepcopy(self.actual)
        next(p for p in actual["parts"] if p["id"] == self.evidence["recipientPartId"])["angle"] += 1
        with self.assertRaisesRegex(ValueError, "pose differs"):
            self.check(actual=actual)

    def test_independent_xy_tamper_rejected(self):
        actual = copy.deepcopy(self.actual)
        next(p for p in actual["parts"] if p["id"] == self.evidence["recipientPartId"])["x"] += .01
        with self.assertRaisesRegex(ValueError, "pose differs"):
            self.check(actual=actual)

    def test_forged_donor_source_rejected(self):
        evidence = copy.deepcopy(self.evidence)
        evidence["donorSourceId"] = "excluded-test-source"
        with self.assertRaisesRegex(ValueError, "donor source"):
            self.check(evidence=evidence)

    def test_random_angle_method_rejected(self):
        evidence = copy.deepcopy(self.evidence)
        evidence["capturedAngle"] = {"kind": "randomJitter"}
        with self.assertRaisesRegex(ValueError, "Unrecognized"):
            self.check(evidence=evidence)

    def test_macro_outside_saved_support_rejected(self):
        evidence = copy.deepcopy(self.evidence)
        tile = self.macro.tiles[evidence["recipientMacroIndex"]]
        neighbors = self.macro.nn.kneighbors(self.macro.context_scaler.transform([level_context(self.reference)+tile["descriptor"]]))[1][0]
        evidence["donorMacroIndex"] = next(i for i in range(len(self.macro.tiles)) if i not in neighbors)
        with self.assertRaisesRegex(ValueError, "outside saved"):
            self.check(evidence=evidence)

    def test_zero_outgoing_donor_probe_drops_replaced_original_pin_indices(self):
        targets = [(p, [e for e in self.reference["links"] if e["source"] == p["id"]]) for p in self.reference["parts"] if p["id"] in eligible_ids(self.reference)]
        for target, outgoing in targets:
            if not outgoing:
                continue
            maximum_index = max(i for edge in outgoing for i in edge["sourceShapeIndices"])
            for source in self.macro.levels.values():
                for donor in source["parts"]:
                    if donor["id"] not in eligible_ids(source) or any(e["source"] == donor["id"] for e in source["links"]):
                        continue
                    if len(self.templates[donor["templateId"]]["physicalShapes"]) > maximum_index:
                        continue
                    if body_key(target, self.templates) == body_key(donor, self.templates) and state(target) == state(donor):
                        angles = captured_angles(self.reference, target, source, donor, self.templates, self.local, np.random.default_rng(0))
                        self.assertGreaterEqual(len(angles), 1)
                        return
        self.fail("Actual source corpus must supply regression fixture")


if __name__ == "__main__":
    unittest.main()
