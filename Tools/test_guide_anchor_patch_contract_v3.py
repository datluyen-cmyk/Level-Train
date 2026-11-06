"""Anchored saved-pattern contract regressions, separate from gameplay."""
from pathlib import Path
import argparse
import copy
import json
import unittest

import joblib

import guide_neighborhood_motifs_v3 as base
from guide_anchor_role_motifs_v3 import pair_context
from guide_anchor_patch_contract_v3 import AnchoredGuidePatchAudit
from mixed_native_guide_role_pilot_v3 import guide_choice
from guide_neighborhood_pilot_v3 import payload_compact
from native_asset_variant_v3 import candidate
from layout_learning_v3 import hashfile


class AnchoredPatchContract(unittest.TestCase):
    workspace = None

    @classmethod
    def setUpClass(cls):
        w = cls.workspace
        with (w/"data/source500_native.json").open() as stream:
            levels = json.load(stream, object_hook=payload_compact)["levels"]
        templates = {t["id"]: t for t in json.loads((w/"data/source_templates.json").read_text())["templates"]}
        reference = next(l for l in levels if l["position"] == 126)
        compact = {tid: base.compact_object(t) for tid, t in templates.items()}
        us = base.units(reference, compact)
        context = pair_context(reference, compact, us[0]["primary"], us[1]["primary"])
        model_path = w/"models/guide_anchor_role_motifs.joblib"
        bundle = joblib.load(model_path)
        indices = bundle["model"].neighbors([context["context"]])[1][0]
        index = next(int(i) for i in indices if bundle["records"][int(i)]["sourceId"] != reference["sourceId"])
        record = bundle["records"][index]
        chosen = guide_choice(reference, context, record)
        local_path = w/"models/composition_pose.joblib"
        local_model = joblib.load(local_path)["model"]
        cls.level = candidate(reference, {}, templates, {l["sourceId"]: l for l in levels}, {}, local_model,
                              hashfile(w/"data/source500_native.json"), hashfile(local_path), 7999992, 0, selected_override=chosen)
        ap = cls.level["assemblyProvenance"]
        ap["learnedPlacementCertificate"]["guideRigidGroups"] = [context["otherUnitIds"]]
        ap["anchoredGuidePatchCertificate"] = {"sourcePackSha256": hashfile(w/"data/source500_native.json"), "modelSha256": hashfile(model_path),
            "referenceSourceId": reference["sourceId"], "anchorPrimaryId": context["anchorPrimaryId"], "otherPrimaryId": context["otherPrimaryId"],
            "recipientContext": context["context"], "learnedRecordIndex": index, "donorSourceId": record["sourceId"]}
        cls.audit = AnchoredGuidePatchAudit(levels, templates, w/"data/source500_native.json", model_path)
        cls.anchor = context["anchorPrimaryId"]
        cls.other = context["otherPrimaryId"]
        cls.guide = next(i for i in context["otherUnitIds"] if i != cls.other)

    def test_actual_saved_pattern_rigid_unit_passes(self):
        self.assertTrue(self.audit.check(self.level)["passed"])

    def test_anchor_pose_change_rejected(self):
        changed = copy.deepcopy(self.level)
        next(p for p in changed["parts"] if p["id"] == self.anchor)["angle"] += 1.
        self.assertFalse(self.audit.check(changed)["passed"])

    def test_independent_xy_jitter_rejected(self):
        changed = copy.deepcopy(self.level)
        next(p for p in changed["parts"] if p["id"] == self.other)["x"] += .02
        self.assertFalse(self.audit.check(changed)["passed"])

    def test_guide_not_rigidly_following_pull_rejected(self):
        changed = copy.deepcopy(self.level)
        next(p for p in changed["parts"] if p["id"] == self.guide)["angle"] += 1.
        self.assertFalse(self.audit.check(changed)["passed"])

    def test_forged_context_rejected(self):
        changed = copy.deepcopy(self.level)
        changed["assemblyProvenance"]["anchoredGuidePatchCertificate"]["recipientContext"][0] += 1.
        self.assertFalse(self.audit.check(changed)["passed"])

    def test_forged_donor_source_rejected(self):
        changed = copy.deepcopy(self.level)
        changed["assemblyProvenance"]["anchoredGuidePatchCertificate"]["donorSourceId"] = "forged"
        self.assertFalse(self.audit.check(changed)["passed"])

    def test_truncated_complete_native_unit_rejected(self):
        changed = copy.deepcopy(self.level)
        changed["assemblyProvenance"]["learnedPlacementCertificate"]["guideRigidGroups"][0].pop()
        self.assertFalse(self.audit.check(changed)["passed"])

    def test_stale_saved_model_corpus_binding_rejected(self):
        audit = copy.copy(self.audit)
        audit.bundle = {**audit.bundle, "sourcePackSha256": "stale"}
        self.assertFalse(audit.check(self.level)["passed"])


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True, type=Path)
    args, remaining = parser.parse_known_args()
    AnchoredPatchContract.workspace = args.workspace.resolve()
    unittest.main(argv=[__file__, *remaining])
