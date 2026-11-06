"""Data-boundary tests for native strong-composition atomic graft support."""
import json, unittest, sys, copy
from pathlib import Path
import joblib
from types import SimpleNamespace
import numpy as np

sys.path.insert(0,str(Path(__file__).resolve().parent))
from composition_graft_v7 import graft
from assembly_integrity_audit_v3 import NativeAssemblyIntegrityAudit
from native_feature_socket_v3 import feature_shape_map
from macro_assembly_v3 import instantiate_tile, pose_matrix
from constructive_layout_v5 import empty_level
from ordinary_source_filter_guard_v4 import preflight, validate_source_filter, sha
from refit_ordinary_models_v4 import payload_compact
from source_physical_composition_v3 import composition_edges

ROOT=Path(__file__).resolve().parents[1]

class CompositionGraftV7Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.workspace=ROOT
        cls.artifact=joblib.load(ROOT/"models/composition_graft_v7.joblib")
        cls.pack_path=ROOT/"data/source500_native.json";cls.template_path=ROOT/"data/source_templates.json"
        cls.levels=json.loads(cls.pack_path.read_text(encoding="utf8"),object_hook=payload_compact)["levels"]
        cls.filter, _=preflight(ROOT)
        cls.allowed=validate_source_filter(cls.filter,sha(cls.pack_path),[l["sourceId"] for l in cls.levels])
        cls.levels=[l for l in cls.levels if str(l["sourceId"]) in cls.allowed]
        cls.by_source={str(l["sourceId"]):l for l in cls.levels}
        cls.templates={t["id"]:t for t in json.loads(cls.template_path.read_text(encoding="utf8"))["templates"]}

    def test_atomic_graft_keeps_every_source_pin_target_and_composition_boundary(self):
        self.assertEqual(self.artifact["sourceCount"],460)
        self.assertGreater(len(self.artifact["records"]),0)
        for record in self.artifact["records"]:
            unit=self.artifact["units"][record["unitIndex"]]
            pair=record["compositionBoundaryPair"]
            self.assertEqual(pair["evidenceLabel"],"STATICcompositionproxy")
            self.assertTrue(pair["kind"] in ("pull_axis_crosses_ring","pull_hull_contains_ring_center"))
            self.assertNotIn(pair["boundarySourcePartId"],unit["partIds"])
            self.assertIn(pair["boundarySourcePartId"],record["requiredBoundaryPartIds"])
            self.assertTrue(set(unit["outgoingBoundaryPartIds"]).issubset(record["requiredBoundaryPartIds"]))

    def test_native_body_boundary_maps_exactly_and_tampered_record_is_rejected(self):
        record=self.artifact["records"][0];source=self.by_source[record["sourceId"]]
        pair=record["compositionBoundaryPair"];boundary=next(p for p in source["parts"] if p["id"]==pair["boundarySourcePartId"])
        template=self.templates[boundary["templateId"]]
        body=[i for i,s in enumerate(template["physicalShapes"]) if s.get("role")=="body"]
        mapped=feature_shape_map(template,template,body)
        self.assertIsNotNone(mapped);self.assertEqual(set(mapped),set(body));self.assertEqual(set(mapped.values()),set(body))
        physical=self.artifact["physicalCompositionRecords"]
        audit=NativeAssemblyIntegrityAudit(self.templates,self.levels,physical)
        raw=composition_edges(source,self.templates);index=pair["sourceCompositionRecordIndex"]
        self.assertEqual(raw[index]["kind"],pair["kind"])
        self.assertTrue(audit.composition_record_is_native(record["sourceId"],index,raw[index]))
        tampered=dict(raw[index]);tampered["kind"]="forged_static_contact"
        self.assertFalse(audit.composition_record_is_native(record["sourceId"],index,tampered))

    def test_headless_seed_list_excludes_whole_source_boards(self):
        for seed in self.artifact["seedRecords"]:
            unit=self.artifact["units"][seed["unitIndex"]]
            self.assertFalse(unit["wholeSourceUnit"])
            self.assertTrue(any(next(p for p in self.by_source[seed["sourceId"]]["parts"] if p["id"]==i)["kind"] in ("ring","slider") for i in unit["partIds"]))

    def test_composition_only_boundary_graft_passes_integrity_and_tamper_fails(self):
        record=next(r for r in self.artifact["records"] if not self.artifact["units"][r["unitIndex"]]["outgoingBoundaryPartIds"])
        unit=self.artifact["units"][record["unitIndex"]];source=self.by_source[record["sourceId"]]
        boundary_id=int(record["receiverSourcePartId"]);boundary=next(p for p in source["parts"] if p["id"]==boundary_id)
        model=SimpleNamespace(levels={record["sourceId"]:source},templates=self.templates,tiles=[{"sourceId":record["sourceId"],"partIds":[boundary_id]}])
        out=empty_level(987654,1,{},{});rotation=-float(boundary["angle"])
        translation=-pose_matrix(rotation)@np.asarray([boundary["x"],boundary["y"]])
        instantiate_tile(model,0,out,rotation,translation)
        receiver=out["parts"][0]
        draft,error=graft(model,unit,record,out,receiver,self.templates)
        self.assertIsNone(error);self.assertIsNotNone(draft)
        self.assertEqual(len(draft["assemblyProvenance"]["featureBoundaryImports"]),1)
        self.assertEqual(draft["assemblyProvenance"]["featureBoundaryImports"][0]["boundaryWitnesses"][0]["kind"],"compositionBody")
        auditor=NativeAssemblyIntegrityAudit(self.templates,self.levels,self.artifact["physicalCompositionRecords"])
        self.assertTrue(auditor.check(draft)["nativeAssemblyIntegrityPassed"])
        tampered=copy.deepcopy(draft)
        tampered["assemblyProvenance"]["featureBoundaryImports"][0]["compositionBoundaryPairs"][0]["kind"]="forged_static_contact"
        self.assertFalse(auditor.check(tampered)["nativeAssemblyIntegrityPassed"])

if __name__=="__main__":unittest.main()
