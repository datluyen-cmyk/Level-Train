import unittest
import copy
import json
from pathlib import Path

import joblib
import numpy as np

from audit_constructive_layout_v5 import (audit_learned_placement, audit_organization_target,
    audit_tile_closures, native_closure, partial_source_embedding_diagnostics)

ROOT = Path(__file__).resolve().parents[1]


class ConstructiveClosureAuditTests(unittest.TestCase):
    def test_closure_follows_recursive_outgoing_links_and_touched_guides(self):
        source = {
            "links": [{"source": 1, "target": 2}, {"source": 2, "target": 4}],
            "guideLinks": [{"guideOwnerId": 2, "lockedOwnerId": 3}],
        }
        self.assertEqual(native_closure(source, [1]), {1, 2, 3, 4})

    def test_import_must_map_exact_declared_closure(self):
        source = {"sourceId": "s", "links": [{"source": 1, "target": 2}], "guideLinks": []}
        sources = {"s": source}
        level = {"assemblyProvenance": {"tileImports": [{
            "sourceId": "s", "originalPrototypePartIds": [1],
            "nativeOutgoingGuideClosurePartIds": [1, 2], "partMapping": {"1": 10, "2": 11},
        }]}}
        self.assertEqual(audit_tile_closures(level, sources), [])

    def test_motif_that_is_whole_source_is_not_itself_a_closure_error(self):
        source = {"sourceId": "s", "links": [], "guideLinks": []}
        level = {"assemblyProvenance": {"tileImports": [{
            "sourceId": "s", "originalPrototypePartIds": [1, 2],
            "nativeOutgoingGuideClosurePartIds": [1, 2], "partMapping": {"1": 10, "2": 11},
        }]}}
        self.assertEqual(audit_tile_closures(level, {"s": source}), [])


class ConstructiveLearnedProvenanceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        workspace = ROOT / "RotateRings Test/LevelTraining_SourceLayoutV3/runs/ordinary_v4"
        cls.levels = json.loads((workspace/"data/source500_native.json").read_text(encoding="utf8"))["levels"]
        cls.sources = {str(level["sourceId"]): level for level in cls.levels}
        cls.templates = {t["id"]: t for t in json.loads((workspace/"data/source_templates.json").read_text(encoding="utf8"))["templates"]}
        cls.org = joblib.load(workspace/"models/organization_v5.joblib")
        cls.macro = joblib.load(workspace/"models/macro_assembly.joblib")
        cls.continuation = joblib.load(workspace/"models/macro_pattern_v5.joblib")
        cls.allowed = set(cls.org.source_ids)
        sha = lambda path: __import__("hashlib").sha256(path.read_bytes()).hexdigest()
        cls.hashes = {"supportingModelHashes": {"macro_assembly.joblib": sha(workspace/"models/macro_assembly.joblib")},
                      "continuationModelSha256": sha(workspace/"models/macro_pattern_v5.joblib")}
        pilot = json.loads((workspace/"batches/constructive_v5_pilot_a/proposals.json").read_text(encoding="utf8"))
        cls.candidate = pilot["levels"][0]
        pilot_c = json.loads((workspace/"batches/constructive_v5_pilot_c/proposals.json").read_text(encoding="utf8"))
        cls.candidate_c = pilot_c["levels"][0]
        pilot_e = json.loads((workspace/"batches/constructive_v5_pilot_e_joint_conditioned/proposals.json").read_text(encoding="utf8"))
        cls.candidate_e_partial = next(level for level in pilot_e["levels"] if level["id"] == 9300002)

    def placement_errors(self, candidate):
        return audit_learned_placement(candidate, self.sources, self.org, self.macro,
                                       self.templates, self.allowed, self.hashes, self.continuation)

    def test_saved_organization_leaf_and_all_contributors_match(self):
        target = self.candidate["assemblyProvenance"]["organizationTarget"]
        self.assertEqual(audit_organization_target(target, self.candidate["position"],
                                                   self.org, self.allowed), [])

    def test_sharp_empirical_field_target_stays_attributed_to_one_leaf_member(self):
        sharp = joblib.load(ROOT/"RotateRings Test/LevelTraining_SourceLayoutV3/runs/ordinary_v4/models/organization_sharp_v5.joblib")
        target = sharp.sample(35, np.random.default_rng(20261106))
        self.assertEqual(audit_organization_target(target, 35, sharp, self.allowed), [])

    def test_tampered_sharp_field_source_fails(self):
        sharp = joblib.load(ROOT/"RotateRings Test/LevelTraining_SourceLayoutV3/runs/ordinary_v4/models/organization_sharp_v5.joblib")
        target = sharp.sample(35, np.random.default_rng(20261106))
        target["organizationFieldSourceId"] = "not-an-ordinary-source"
        self.assertIn("organizationEmpiricalFieldSourceMismatch",
                      {e["kind"] for e in audit_organization_target(target, 35, sharp, self.allowed)})

    def test_progression_conditioned_empirical_field_target_is_reconstructed(self):
        conditioned = joblib.load(ROOT/"RotateRings Test/LevelTraining_SourceLayoutV3/runs/ordinary_v4/models/organization_conditioned_v5.joblib")
        target = conditioned.sample(35, np.random.default_rng(20261106))
        self.assertEqual(audit_organization_target(target, 35, conditioned, self.allowed), [])

    def test_tampered_progression_probability_fails(self):
        conditioned = joblib.load(ROOT/"RotateRings Test/LevelTraining_SourceLayoutV3/runs/ordinary_v4/models/organization_conditioned_v5.joblib")
        target = conditioned.sample(35, np.random.default_rng(20261106))
        target["sampledFieldProbability"] += 0.1
        self.assertIn("organizationProgressionProbabilityMismatch",
                      {e["kind"] for e in audit_organization_target(target, 35, conditioned, self.allowed)})

    def test_saved_decisions_reconstruct_motif_and_relation_transforms(self):
        self.assertEqual(self.placement_errors(self.candidate), [])

    def test_tampered_relation_record_index_fails(self):
        candidate = copy.deepcopy(self.candidate)
        relation = next(d for d in candidate["assemblyProvenance"]["decisions"]
                        if d["kind"] == "savedConditionalMacroRelation")
        relation["relationRecordIndex"] += 1
        kinds = {e["kind"] for e in self.placement_errors(candidate)}
        self.assertTrue({"sampledPoseDoesNotMatchSavedRecord", "relationRecordNotInConditionalNeighborSet"} & kinds)

    def test_tampered_sampled_pose_fails(self):
        candidate = copy.deepcopy(self.candidate)
        relation = next(d for d in candidate["assemblyProvenance"]["decisions"]
                        if d["kind"] == "savedConditionalMacroRelation")
        relation["sampledRelativePose"][0] += 0.1
        self.assertIn("sampledPoseDoesNotMatchSavedRecord",
                      {e["kind"] for e in self.placement_errors(candidate)})

    def test_tampered_transform_fails(self):
        candidate = copy.deepcopy(self.candidate)
        relation = next(d for d in candidate["assemblyProvenance"]["decisions"]
                        if d["kind"] == "savedConditionalMacroRelation")
        relation["translation"][0] += 0.1
        self.assertIn("decisionTransformDiffersFromImport",
                      {e["kind"] for e in self.placement_errors(candidate)})

    def test_tampered_tree_leaf_fails(self):
        candidate = copy.deepcopy(self.candidate)
        candidate["assemblyProvenance"]["organizationTarget"]["leafId"] += 1
        self.assertIn("organizationLeafMismatch", {e["kind"] for e in self.placement_errors(candidate)})

    def test_saved_native_triple_continuation_reconstructs(self):
        self.assertEqual(self.placement_errors(self.candidate_c), [])

    def test_tampered_triple_record_index_fails(self):
        candidate = copy.deepcopy(self.candidate_c)
        decision = next(d for d in candidate["assemblyProvenance"]["decisions"]
                        if d["kind"] == "savedNativeTripleContinuation")
        decision["continuationRecordIndex"] += 1
        kinds = {e["kind"] for e in self.placement_errors(candidate)}
        self.assertTrue({"sampledPoseDoesNotMatchContinuationRecord",
                         "continuationRecordNotInConditionalNeighborSet"} & kinds)

    def test_tampered_triple_pose_and_transform_fail(self):
        candidate = copy.deepcopy(self.candidate_c)
        decision = next(d for d in candidate["assemblyProvenance"]["decisions"]
                        if d["kind"] == "savedNativeTripleContinuation")
        decision["sampledRelativePose"][0] += 0.1
        kinds = {e["kind"] for e in self.placement_errors(candidate)}
        self.assertIn("sampledPoseDoesNotMatchContinuationRecord", kinds)
        candidate = copy.deepcopy(self.candidate_c)
        decision = next(d for d in candidate["assemblyProvenance"]["decisions"]
                        if d["kind"] == "savedNativeTripleContinuation")
        decision["translation"][0] += 0.1
        kinds = {e["kind"] for e in self.placement_errors(candidate)}
        self.assertTrue({"decisionTransformDiffersFromImport", "learnedPlacementTransformMismatch"} & kinds)

    def test_substantial_exact_partial_source_embedding_is_flagged_with_relation_retention(self):
        rows = partial_source_embedding_diagnostics(self.candidate_e_partial, self.sources)
        match = next(row for row in rows if row["sourceId"] == "10043")
        self.assertTrue(match["partialSourceCopyReviewRequired"])
        self.assertEqual(match["consistentRootCount"], 16)
        self.assertEqual(match["inducedNativeLinkCount"], 11)
        self.assertEqual(match["retainedNativeLinkCount"], 11)
        self.assertEqual(match["inducedNativeGuideCount"], 2)
        self.assertEqual(match["retainedNativeGuideCount"], 2)


if __name__ == "__main__":
    unittest.main()
