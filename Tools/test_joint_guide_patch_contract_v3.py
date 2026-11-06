"""Actual saved-model joint-record certificate tests; not gameplay tests."""
from pathlib import Path
import argparse
import copy
import json
import unittest

import joblib
import numpy as np

from guide_joint_patch_contract_v3 import JointGuidePatchAudit, selected_context
from guide_joint_xy_pilot_v3 import construct_joint_candidate
from guide_joint_pose_motifs_v3 import kabsch
from guide_neighborhood_pilot_v3 import payload_compact
import guide_neighborhood_motifs_v3 as base


class JointPatchContract(unittest.TestCase):
    workspace = None

    @classmethod
    def setUpClass(cls):
        w = cls.workspace
        with (w/"data/source500_native.json").open() as stream:
            cls.levels = json.load(stream, object_hook=payload_compact)["levels"]
        cls.templates = {t["id"]: t for t in json.loads((w/"data/source_templates.json").read_text())["templates"]}
        reference = next(l for l in cls.levels if l["position"] == 30)
        compact = {tid: base.compact_object(t) for tid, t in cls.templates.items()}
        context = selected_context(reference, compact, [2, 3, 4, 5])
        path = w/"models/guide_joint_pose_motifs.joblib"
        bundle = joblib.load(path)
        saved = bundle["models"][4]
        indices = saved["model"].neighbors([context["context"]])[1][0]
        index = next(int(i) for i in indices if saved["records"][int(i)]["sourceId"] != reference["sourceId"]
                     and abs(saved["records"][int(i)]["width"]-context["width"]) <= 1e-5)
        local_path = w/"models/composition_pose.joblib"
        local_model = joblib.load(local_path)["model"]
        cls.candidate = construct_joint_candidate(reference, context, saved["records"][index], index, cls.templates,
            {l["sourceId"]: l for l in cls.levels}, {}, local_model, w/"data/source500_native.json", local_path, path, 7999991)
        cls.audit = JointGuidePatchAudit(cls.levels, cls.templates, path, w/"data/source500_native.json")
        cls.primary = context["primaryIds"][0]

    def test_actual_saved_record_rigid_certificate(self):
        self.assertTrue(self.audit.check(self.candidate)["passed"])

    def test_independent_xy_jitter_rejected(self):
        changed = copy.deepcopy(self.candidate)
        next(p for p in changed["parts"] if p["id"] == self.primary)["x"] += .02
        self.assertFalse(self.audit.check(changed)["passed"])

    def test_independent_angle_rejected(self):
        changed = copy.deepcopy(self.candidate)
        next(p for p in changed["parts"] if p["id"] == self.primary)["angle"] += 1.
        self.assertFalse(self.audit.check(changed)["passed"])

    def test_scale_rejected(self):
        changed = copy.deepcopy(self.candidate)
        changed["assemblyProvenance"]["jointGuidePatchCertificate"]["scale"] = 1.01
        self.assertFalse(self.audit.check(changed)["passed"])

    def test_forged_context_rejected(self):
        changed = copy.deepcopy(self.candidate)
        changed["assemblyProvenance"]["jointGuidePatchCertificate"]["recipientContext"][0] += 1.
        self.assertFalse(self.audit.check(changed)["passed"])

    def test_forged_donor_source_rejected(self):
        changed = copy.deepcopy(self.candidate)
        changed["assemblyProvenance"]["jointGuidePatchCertificate"]["donorSourceId"] = "forged-source"
        self.assertFalse(self.audit.check(changed)["passed"])

    def test_duplicate_selected_unit_rejected(self):
        changed = copy.deepcopy(self.candidate)
        selected = changed["assemblyProvenance"]["jointGuidePatchCertificate"]["selectedPrimaryIds"]
        selected[-1] = selected[0]
        self.assertFalse(self.audit.check(changed)["passed"])

    def test_stale_model_corpus_binding_rejected(self):
        audit = copy.copy(self.audit)
        audit.bundle = {**audit.bundle, "sourcePackSha256": "stale-source"}
        self.assertIn("saved_model_corpus_hash_mismatch", audit.check(self.candidate)["errors"])

    def test_per_unit_width_mismatch_rejected_even_with_same_median(self):
        audit = copy.copy(self.audit)
        audit.bundle = dict(audit.bundle)
        audit.bundle["models"] = dict(audit.bundle["models"])
        saved = dict(audit.bundle["models"][4])
        saved["records"] = list(saved["records"])
        index = self.candidate["assemblyProvenance"]["jointGuidePatchCertificate"]["learnedRecordIndex"]
        record = dict(saved["records"][index])
        record["context"] = list(record["context"])
        record["context"][9+6] += .125
        saved["records"][index] = record
        audit.bundle["models"][4] = saved
        self.assertIn("native_per_unit_width_mismatch", audit.check(self.candidate)["errors"])

    def test_truncated_unit_rejected(self):
        changed = copy.deepcopy(self.candidate)
        changed["assemblyProvenance"]["learnedPlacementCertificate"]["guideRigidGroups"][0].pop()
        self.assertFalse(self.audit.check(changed)["passed"])

    def test_kabsch_never_introduces_scale_or_reflection(self):
        xy = np.asarray([[0., 0.], [2., 0.], [1., 3.], [-1., 1.]])
        r = np.asarray([[0., 1.], [-1., 0.]])
        target = xy@r+[7., -2.]
        fit, shift, _, error = kabsch(xy, target)
        self.assertTrue(np.allclose(xy@fit+shift, target))
        self.assertAlmostEqual(np.linalg.det(fit), 1.)
        self.assertLess(error, 1e-8)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True, type=Path)
    args, remaining = parser.parse_known_args()
    JointPatchContract.workspace = args.workspace.resolve()
    unittest.main(argv=[__file__, *remaining])
