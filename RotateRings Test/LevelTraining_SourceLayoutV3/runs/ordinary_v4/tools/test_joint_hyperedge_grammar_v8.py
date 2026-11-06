"""Source-only structural tests for the V8 joint native production grammar."""
from pathlib import Path
from types import SimpleNamespace
import copy
import json
import sys

import numpy as np

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))

from joint_hyperedge_grammar_v8 import build_productions, graft_production
from macro_assembly_v3 import instantiate_tile, pose_matrix
from assembly_integrity_audit_v3 import NativeAssemblyIntegrityAudit
from assembly_contact_audit_v3 import NativeGeometrySeamAudit
from constructive_layout_v5 import empty_level
from construct_joint_hyperedge_v8 import exact_source_reuse
from layout_novelty_semantic_v3 import canonical_fingerprint


def fixture():
    pack = json.loads((ROOT / "data/source500_native.json").read_text(encoding="utf8"))
    source = next(level for level in pack["levels"] if str(level["sourceId"]) == "10004")
    templates = {t["id"]: t for t in json.loads((ROOT / "data/source_templates.json").read_text(encoding="utf8"))["templates"]}
    return source, templates


def test_10004_fork_is_one_joint_production():
    source, templates = fixture()
    records = build_productions(source, templates)
    match = [r for r in records if r["importedPartIds"] == [1, 2, 3] and r["boundaryPartIds"] == [4, 5]]
    assert match, "Expected a production importing owner 3 plus receivers 1/2 with exact boundaries 4/5"
    assert all(source["links"][i]["source"] in {1, 2, 3} and source["links"][i]["target"] in {4, 5}
               for i in match[0]["outgoingBoundaryEdgeIndices"])


def test_exact_source_board_reuse_is_recognized():
    source, templates = fixture()
    fingerprints = {canonical_fingerprint(source, templates): [str(source["sourceId"])]}
    assert exact_source_reuse(source, fingerprints, templates) == [str(source["sourceId"])]


def test_joint_graft_integrity_and_contact_tamper_rejection():
    source, templates = fixture()
    source_id = str(source["sourceId"])
    records = build_productions(source, templates)
    joint = next(r for r in records if r["importedPartIds"] == [1, 2, 3] and r["boundaryPartIds"] == [4, 5] and r["anchorSourcePartId"] == 4)
    model = SimpleNamespace(levels={source_id: source}, templates=templates, tiles=[])
    out = empty_level(81001, source["position"], {"targetParts": len(source["parts"])}, {})
    # Start with owner 6 only. Add 4/5 through their complete outbound edges;
    # incoming edges from absent owner 3 remain pending until the joint graft.
    source6 = next(p for p in source["parts"] if p["id"] == 6)
    seed_rotation = -source6["angle"]
    seed_translation = -pose_matrix(seed_rotation) @ np.asarray([source6["x"], source6["y"]])
    model.tiles.append({"sourceId": source_id, "partIds": [6]})
    try:
        instantiate_tile(model, 0, out, seed_rotation, seed_translation)
    finally:
        model.tiles.pop()
    for owner_id in (4, 5):
        record = next(r for r in records if r["importedPartIds"] == [owner_id] and r["boundaryPartIds"] == [6]
                      and r["anchorSourcePartId"] == 6)
        receiver = next(p for p in out["parts"] if p.get("atomicProvenance", {}).get("partId") == 6)
        out, error = graft_production(model, record, out, receiver, templates)
        assert error is None, error
        if owner_id == 4:
            duplicate, duplicate_error = graft_production(model, record, out, receiver, templates)
            assert duplicate is None and duplicate_error == "duplicateDonorSourceOwnerOrigin"
    receiver4 = next(p for p in out["parts"] if p.get("atomicProvenance", {}).get("partId") == 4)
    out, error = graft_production(model, joint, out, receiver4, templates)
    assert error is None, error
    # Add owner 7 after its targets 1/2 exist. This preserves its complete
    # outgoing fanout without seeding or duplicating owner 3.
    unit7 = next(r for r in records if r["importedPartIds"] == [7] and r["boundaryPartIds"] == [1, 2]
                 and r["anchorSourcePartId"] == 1)
    receiver1 = next(p for p in out["parts"] if p.get("atomicProvenance", {}).get("partId") == 1)
    out, error = graft_production(model, unit7, out, receiver1, templates)
    assert error is None, error
    audit = NativeAssemblyIntegrityAudit(templates, [source])
    clean = audit.check(out)
    assert clean["nativeAssemblyIntegrityPassed"], clean
    raw_geometry = NativeGeometrySeamAudit(templates, [source]).check(out)
    assert raw_geometry["rawGeometrySeamPassed"], raw_geometry
    origins = [(str(p["atomicProvenance"]["sourceId"]), int(p["atomicProvenance"]["partId"]))
               for p in out["parts"]]
    assert len(origins) == len(set(origins)) == len(source["parts"]), origins
    assert set(origins) == {(source_id, p["id"]) for p in source["parts"]}, origins
    assert len(out["parts"]) == len(source["parts"]), "Construction must contain each source owner exactly once"
    assert len(out["links"]) == len(source["links"]), "Final audit must recover every original native pin exactly once"
    duplicate = copy.deepcopy(out)
    clone = copy.deepcopy(duplicate["parts"][0])
    clone["id"] = max(p["id"] for p in duplicate["parts"]) + 1
    duplicate["parts"].append(clone)
    duplicate_audit = audit.check(duplicate)
    assert not duplicate_audit["nativeAssemblyIntegrityPassed"], duplicate_audit
    tampered = copy.deepcopy(out)
    decision = next(d for d in tampered["assemblyProvenance"]["decisions"]
                    if d.get("productionId") == joint["productionId"])
    imported3 = decision["newOwnerPartMapping"][3]
    boundary_ids = set(decision["boundaryPartMapping"].values())
    target_links = [e for e in tampered["links"] if e["source"] == imported3 and e["target"] in boundary_ids]
    assert target_links
    tampered["links"].remove(target_links[0])
    rejected = audit.check(tampered)
    assert not rejected["nativeAssemblyIntegrityPassed"], rejected


if __name__ == "__main__":
    test_10004_fork_is_one_joint_production()
    test_exact_source_board_reuse_is_recognized()
    test_joint_graft_integrity_and_contact_tamper_rejection()
    print("V8_SOURCE_INTEGRITY_TESTS_PASSED")

