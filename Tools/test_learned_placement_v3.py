"""Tamper checks for the learned-placement certificate, independent of its optimizer."""
import copy
import json
from pathlib import Path
import unittest

from learned_placement_audit_v3 import NativeLearnedPlacementAudit

ROOT = Path(__file__).resolve().parents[1]
BUNDLE = ROOT if (ROOT / "data/source500_native.json").exists() else ROOT / "RotateRings Test/LevelTraining_SourceLayoutV3"


class LearnedPlacementTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        load = lambda path: json.loads(path.read_text(encoding="utf8"))
        cls.level = load(BUNDLE / "batches/incomplete_learned_retarget_certificate/proposals.json")["levels"][0]
        sources = BUNDLE / "data/source500_native.json"
        cls.audit = NativeLearnedPlacementAudit(load(sources)["levels"],
            {t["id"]: t for t in load(BUNDLE / "data/source_templates.json")["templates"]},
            BUNDLE / "models/composition_pose.joblib", sources)

    def check_rejected(self, level, kind):
        report = self.audit.check(level)
        self.assertFalse(report["nativeLearnedPlacementIntegrityPassed"])
        self.assertIn(kind, {error["kind"] for error in report["errors"]})

    def test_actual_learned_proposal_certificate(self):
        report = self.audit.check(self.level)
        self.assertTrue(report["nativeLearnedPlacementIntegrityPassed"], report["errors"])

    def test_reported_fit_cannot_be_forged(self):
        level = copy.deepcopy(self.level)
        row = next(row for row in level["assemblyProvenance"]["learnedPlacementCertificate"]["relationWitnesses"] if row["relation"] == "pin")
        row["poseResidualL2"] += 1.
        self.check_rejected(level, "incorrectReportedLearnedPoseResidual")

    def test_source_model_hash_cannot_be_replaced(self):
        level = copy.deepcopy(self.level)
        level["assemblyProvenance"]["learnedPlacementCertificate"]["localPoseModelSha256"] = "unverified"
        self.check_rejected(level, "learnedModelOrCorpusHashMismatch")

    def test_new_relations_need_saved_model_witnesses(self):
        level = copy.deepcopy(self.level)
        level["assemblyProvenance"]["learnedRelationPlacements"] = []
        self.check_rejected(level, "unwitnessedLearnedRelation")

    def test_native_input_shape_cannot_be_changed_during_retarget(self):
        level = copy.deepcopy(self.level)
        moved = next(iter(level["assemblyProvenance"]["retargetedAssetImports"][0]["partMapping"].values()))
        part = next(p for p in level["parts"] if p["id"] == moved and p["inputShapes"])
        part["inputShapes"][0]["paths"][0]["points"][0]["x"] += .05
        self.check_rejected(level, "nativeAssetContract")

    def test_boundary_body_evidence_cannot_be_omitted(self):
        level = copy.deepcopy(self.level)
        witness = level["assemblyProvenance"]["learnedBoundaryFeatureWitnesses"][0]["boundaryWitnesses"][0]
        witness.pop("sourceBodyShapeIndices", None)
        self.check_rejected(level, "incompleteLearnedBoundaryBody")

    def test_pattern_cannot_point_at_uncertified_native_feature(self):
        level = copy.deepcopy(self.level)
        claim = level["assemblyProvenance"]["learnedRelationPlacements"][0]
        edge = next(e for e in level["links"] if e["source"] == claim["actualSource"] and e["target"] == claim["actualTarget"])
        edge["sourceShapeIndices"] = edge["sourceShapeIndices"][:-1]
        claim["actualSourceShapeIndices"] = edge["sourceShapeIndices"].copy()
        self.check_rejected(level, "unprovenLearnedPatternFeatureMapping")

    def test_scaffold_certificate_cannot_move_the_native_body(self):
        level = copy.deepcopy(self.level)
        part = level["parts"][0]
        origin = part["atomicProvenance"]
        original = next(p for p in self.audit.sources[str(origin["sourceId"])]["parts"] if p["id"] == origin["partId"])
        indices = [i for i, s in enumerate(self.audit.templates[part["templateId"]]["physicalShapes"]) if s["role"] == "body"]
        level["assemblyProvenance"]["bodyScaffoldSubstitutions"] = [{"sourceId": origin["sourceId"],
            "sourcePartId": original["id"], "actualPartId": part["id"],
            "sourceBodyShapeIndices": indices, "targetBodyShapeIndices": indices}]
        part["x"] += 1.
        self.check_rejected(level, "nonNativeBodyScaffoldPose")


if __name__ == "__main__":
    unittest.main()
