"""Regression checks for source evidence that the seam screen must not trust blindly."""
import copy
import json
from pathlib import Path
import unittest

from assembly_integrity_audit_v3 import NativeAssemblyIntegrityAudit
from source_physical_composition_v3 import composition_edges

ROOT = Path(__file__).resolve().parents[1]


class NativeIntegrityTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        bundled = (ROOT / "data/source500_native.json").exists()
        folder = ROOT / "data" if bundled else ROOT / "Research/physics_v3"
        cls.sources = json.loads((folder / ("source500_native.json" if bundled else "source_parity_pack.json")).read_text(encoding="utf8"))["levels"]
        cls.templates = {t["id"]: t for t in json.loads((folder / "source_templates.json").read_text(encoding="utf8"))["templates"]}
        cls.audit = NativeAssemblyIntegrityAudit(cls.templates, cls.sources)
        cls.composition = {str(level["sourceId"]): composition_edges(level, cls.templates) for level in cls.sources}
        cls.composition_audit = NativeAssemblyIntegrityAudit(cls.templates, cls.sources, cls.composition)

    def fixture(self, source_id):
        level = copy.deepcopy(next(l for l in self.sources if str(l["sourceId"]) == str(source_id)))
        level["assemblyProvenance"] = {"tileImports": [{"sourceId": level["sourceId"],
            "partMapping": {p["id"]: p["id"] for p in level["parts"]}, "rotation": 0., "translation": [0., 0.]}]}
        for part in level["parts"]:
            part["atomicProvenance"] = {"sourceId": level["sourceId"], "partId": part["id"]}
        for bomb in level["bombs"]:
            bomb["atomicProvenance"] = {"sourceId": level["sourceId"], "sourceColliderId": bomb["sourceColliderId"]}
        return level

    def test_native_source_fixtures_pass(self):
        for source in self.sources:
            with self.subTest(source=source["sourceId"]):
                self.assertTrue(self.audit.check(self.fixture(source["sourceId"]))["nativeAssemblyIntegrityPassed"])

    def assert_rejected(self, level, kind):
        report = self.audit.check(level)
        self.assertFalse(report["nativeAssemblyIntegrityPassed"])
        self.assertIn(kind, {error["kind"] for error in report["errors"]})

    def test_false_import_claim_does_not_allow_repositioned_root(self):
        level = self.fixture(10010)
        level["parts"][0]["x"] += .3
        self.assert_rejected(level, "nonRigidImportedRoot")

    def test_locked_root_cannot_drop_its_native_guide(self):
        level = self.fixture(10010)
        guide = level["guideLinks"][0]["guideOwnerId"]
        level["parts"] = [p for p in level["parts"] if p["id"] != guide]
        level["guideLinks"] = [e for e in level["guideLinks"] if e["guideOwnerId"] != guide]
        del level["assemblyProvenance"]["tileImports"][0]["partMapping"][guide]
        self.assert_rejected(level, "truncatedNativeGuideAssembly")

    def test_fabricated_relation_is_rejected(self):
        level = self.fixture(10010)
        edge = copy.deepcopy(level["links"][0])
        edge["relationKind"] = 999
        level["links"].append(edge)
        self.assert_rejected(level, "fabricatedRelation")

    def test_native_input_geometry_cannot_be_shrunk(self):
        level = self.fixture(10001)
        level["parts"][0]["inputShapes"][0]["paths"][0]["points"][0]["x"] += .1
        self.assert_rejected(level, "changedNativePartContract")

    def test_bomb_cannot_be_replaced_by_smaller_collider(self):
        level = self.fixture(10109)
        level["bombs"][0]["physicalShape"]["radius"] *= .5
        self.assert_rejected(level, "changedNativeBombGeometry")

    def test_imported_native_bomb_cannot_be_deleted(self):
        level = self.fixture(10109)
        level["bombs"].pop()
        self.assert_rejected(level, "missingNativeBomb")

    def feature_fixture(self):
        level = self.fixture(10001)
        edge = level["links"][0]
        source, target = edge["source"], edge["target"]
        level["assemblyProvenance"]["tileImports"][0]["partMapping"] = {source: source}
        level["assemblyProvenance"]["featureBoundaryImports"] = [{"sourceId": level["sourceId"],
            "rotation": 0., "translation": [0., 0.], "interiorPartMapping": {target: target},
            "boundaryWitnesses": [{"kind": "pin", "sourcePartId": source, "targetPartId": source,
                "sourceShapeIndices": edge["sourceShapeIndices"].copy(),
                "targetShapeIndices": edge["sourceShapeIndices"].copy(), "relationIndices": [0]}]}]
        return level

    def test_native_feature_boundary_certificate_passes(self):
        report = self.audit.check(self.feature_fixture())
        self.assertTrue(report["nativeAssemblyIntegrityPassed"], report["errors"])

    def test_head_only_boundary_cannot_omit_native_stem(self):
        level = self.feature_fixture()
        witness = level["assemblyProvenance"]["featureBoundaryImports"][0]["boundaryWitnesses"][0]
        witness["sourceShapeIndices"].pop(0)
        witness["targetShapeIndices"].pop(0)
        self.assert_rejected(level, "incompleteNativeBoundaryRelation")

    def test_nonmatching_boundary_feature_is_rejected(self):
        level = self.feature_fixture()
        witness = level["assemblyProvenance"]["featureBoundaryImports"][0]["boundaryWitnesses"][0]
        witness["targetShapeIndices"] = list(reversed(witness["targetShapeIndices"]))
        self.assert_rejected(level, "changedNativeBoundaryFeature")

    def test_unrelated_authored_edge_index_is_not_a_certificate(self):
        level = self.feature_fixture()
        witness = level["assemblyProvenance"]["featureBoundaryImports"][0]["boundaryWitnesses"][0]
        witness["relationIndices"].append(99)
        self.assert_rejected(level, "unprovenBoundaryRelationIndex")

    def composition_fixture(self):
        level = self.fixture(10010)
        level["assemblyProvenance"]["tileImports"][0]["partMapping"].pop(1)
        edge = level["links"][3]
        parts = {p["id"]: p for p in level["parts"]}
        body = [i for i, s in enumerate(self.templates[parts[2]["templateId"]]["physicalShapes"]) if s["role"] == "body"]
        records = self.composition["10010"]
        index = next(i for i, r in enumerate(records) if r["source"] == 2 and r["target"] == 1 and r["strong"])
        level["assemblyProvenance"]["featureBoundaryImports"] = [{"sourceId": "10010",
            "rotation": 0., "translation": [0., 0.], "interiorPartMapping": {1: 1},
            "boundaryWitnesses": [
                {"kind": "pin", "sourcePartId": 9, "targetPartId": 9,
                 "sourceShapeIndices": edge["sourceShapeIndices"].copy(),
                 "targetShapeIndices": edge["sourceShapeIndices"].copy(), "relationIndices": [3]},
                {"kind": "compositionBody", "sourcePartId": 2, "targetPartId": 2,
                 "sourceShapeIndices": body.copy(), "targetShapeIndices": body.copy(), "relationIndices": []}],
            "compositionBoundaryPairs": [{"interiorSourcePartId": 1, "boundarySourcePartId": 2,
                "sourceCompositionRecordIndex": index, "kind": records[index]["kind"]}]}]
        return level

    def composition_rejected(self, level, kind, audit=None):
        result = (audit or self.composition_audit).check(level)
        self.assertFalse(result["nativeAssemblyIntegrityPassed"])
        self.assertIn(kind, {e["kind"] for e in result["errors"]})

    def test_exact_native_composition_body_certificate_passes(self):
        result = self.composition_audit.check(self.composition_fixture())
        self.assertTrue(result["nativeAssemblyIntegrityPassed"], result["errors"])

    def test_composition_evidence_absent_fails_closed(self):
        self.composition_rejected(self.composition_fixture(), "unprovenCompositionBoundaryPair", self.audit)

    def test_composition_body_cannot_claim_a_gameplay_binding(self):
        level = self.composition_fixture()
        level["assemblyProvenance"]["featureBoundaryImports"][0]["boundaryWitnesses"][1]["relationIndices"] = [3]
        self.composition_rejected(level, "invalidCompositionBodyWitness")

    def test_composition_body_cannot_certify_unrelated_pins(self):
        level = self.composition_fixture()
        witness = level["assemblyProvenance"]["featureBoundaryImports"][0]["boundaryWitnesses"][1]
        part = next(p for p in level["parts"] if p["id"] == 2)
        # Even an identical native extra feature is outside a body-only witness.
        witness["sourceShapeIndices"] = list(range(len(self.templates[part["templateId"]]["physicalShapes"])))
        witness["targetShapeIndices"] = witness["sourceShapeIndices"].copy()
        if len(witness["sourceShapeIndices"]) == 1:
            witness["sourceShapeIndices"] = []
            witness["targetShapeIndices"] = []
            self.composition_rejected(level, "invalidBoundaryShapeMapping")
        else:
            self.composition_rejected(level, "invalidCompositionBodyWitness")

    def test_fabricated_saved_composition_record_is_recomputed(self):
        level = self.composition_fixture()
        pair = level["assemblyProvenance"]["featureBoundaryImports"][0]["compositionBoundaryPairs"][0]
        records = copy.deepcopy(self.composition)
        records["10010"][pair["sourceCompositionRecordIndex"]]["evidence"]["exactSolidOutlineGap"] += .5
        audit = NativeAssemblyIntegrityAudit(self.templates, self.sources, records)
        self.composition_rejected(level, "unprovenCompositionBoundaryPair", audit)

    def test_weak_proximity_is_not_a_body_seam_certificate(self):
        level = self.composition_fixture()
        pair = level["assemblyProvenance"]["featureBoundaryImports"][0]["compositionBoundaryPairs"][0]
        records = self.composition["10010"]
        index = next(i for i, r in enumerate(records) if r["source"] == 2 and r["target"] == 1 and not r["strong"])
        pair.update(sourceCompositionRecordIndex=index, kind=records[index]["kind"])
        self.composition_rejected(level, "unprovenCompositionBoundaryPair")

    def test_body_certificate_cannot_move_native_boundary(self):
        level = self.composition_fixture()
        next(p for p in level["parts"] if p["id"] == 2)["x"] += .1
        self.composition_rejected(level, "nonRigidFeatureBoundary")


if __name__ == "__main__":
    unittest.main()
