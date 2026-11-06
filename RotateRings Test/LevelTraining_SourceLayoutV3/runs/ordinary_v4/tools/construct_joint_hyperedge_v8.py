"""Produce a labeled, integrity-gated V8 structural research pilot; no export."""
from pathlib import Path
from collections import Counter
from types import SimpleNamespace
import argparse
import copy
import json

import joblib
import numpy as np
from scipy.special import logsumexp

from joint_hyperedge_grammar_v8 import graft_production, receiver_socket
from layout_learning_v3 import shape_vector
from macro_assembly_v3 import instantiate_tile, pose_matrix
from constructive_layout_v5 import empty_level
from assembly_integrity_audit_v3 import NativeAssemblyIntegrityAudit
from assembly_contact_audit_v3 import NativeGeometrySeamAudit
from layout_novelty_semantic_v3 import canonical_fingerprint
from ordinary_source_filter_guard_v4 import preflight, validate_source_filter, sha
from refit_ordinary_models_v4 import payload_compact, local_dependency_hashes


def _soft_indices(values, scaler, normalized, query, rng, count):
    if not values:
        return []
    delta = normalized - scaler.transform([query])[0]
    distance = np.linalg.norm(delta, axis=1)
    logits = -.5 * distance ** 2
    probabilities = np.exp(logits - logsumexp(logits))
    return list(map(int, rng.choice(len(values), size=min(count, len(values)), replace=False, p=probabilities)))


def _seed(model, artifact, base, index, context):
    record = artifact["seedRecords"][index]
    model.tiles.append({"sourceId": record["sourceId"], "partIds": record["partIds"]})
    try:
        level = model.levels[record["sourceId"]]
        parts = {p["id"]: p for p in level["parts"]}
        anchor = next(parts[i] for i in record["partIds"] if parts[i]["kind"] in ("ring", "slider"))
        rotation = -anchor["angle"]
        translation = -pose_matrix(rotation) @ np.asarray([anchor["x"], anchor["y"]])
        out = copy.deepcopy(base)
        mapping = instantiate_tile(model, len(model.tiles) - 1, out, rotation, translation)
    finally:
        model.tiles.pop()
    out["assemblyProvenance"]["decisions"].append({"kind": "learnedNativeAtomicSeedV8", "sourceId": record["sourceId"],
        "unitIndex": record["unitIndex"], "partMapping": mapping, "rotation": float(rotation),
        "translation": translation.tolist(), "seedRecordIndex": int(index), "recipientTopologyContext": list(context)})
    return out


def _seam_summary(result):
    return {"unexpectedSolidOverlapCount": result["unexpectedSolidOverlapCount"],
            "unregisteredSensorPartnerCount": result["unregisteredSensorPartnerCount"],
            "duplicateWholeRootPairCount": len(result["duplicateWholeRootPairs"]),
            "missingFeatureBindingCount": len(result["missingFeatureBindings"]),
            "newBombInitialOverlapCount": result["newBombInitialOverlapCount"]}


def _grow(model, artifact, out, context, templates, seam, rng, tries, rejects):
    receivers = [p for p in out["parts"] if p["kind"] in ("ring", "slider")]
    if not receivers:
        rejects["noReceiverOwner"] += 1
        return None
    for _ in range(tries):
        receiver = receivers[int(rng.integers(len(receivers)))]
        bank = artifact["banks"].get(receiver_socket(receiver, templates))
        if bank is None:
            rejects["noLearnedNativeAnchorBank"] += 1
            continue
        query = list(context) + shape_vector(templates[receiver["templateId"]])
        transformed = bank["scaler"].transform([query])
        distances, indices = bank["nn"].kneighbors(transformed)
        logits = -.5 * distances[0] ** 2
        probabilities = np.exp(logits - logsumexp(logits))
        selected = int(rng.choice(len(indices[0]), p=probabilities))
        record_index = int(indices[0][selected])
        record = bank["records"][record_index]
        draft, error = graft_production(model, record, out, receiver, templates)
        if draft is None:
            rejects[error] += 1
            continue
        native_geometry = seam.check(draft)
        if not native_geometry["rawGeometrySeamPassed"]:
            for key, value in _seam_summary(native_geometry).items():
                if value:
                    rejects["nativeGeometry:" + key] += int(value)
            continue
        draft["assemblyProvenance"]["decisions"][-1].update(
            learnedBankRecordIndex=record_index,
            neighborProbability=float(probabilities[selected]),
            recipientTopologyContext=list(context),
            proposalModelSha256=artifact["hashes"].get("model"))
        return draft
    return None


def exact_source_reuse(level, source_fingerprints, templates):
    """Return exact canonical source match IDs; heuristic near-copy remains separate."""
    return source_fingerprints.get(canonical_fingerprint(level, templates), [])


def construct_one(artifact, model, reference, organization, templates, integrity, seam, source_fingerprints,
                  rng, number, seed_options, graft_tries, max_steps, hashes):
    target = organization.sample(reference["position"], rng)
    if "targetParts" not in target or not np.isfinite(float(target["targetParts"])):
        raise ValueError("Fitted organization model must supply a finite empirical targetParts value")
    target.update(referenceProfileSourceId=str(reference["sourceId"]),
                  referencePartCountDiagnostic=int(len(reference["parts"])),
                  profileConditioning="Learned organization sample conditioned on progression position; reference part count is diagnostic only.")
    base = empty_level(number, reference["position"], target, hashes)
    base["sourceId"] = f"joint-hyperedge-v8-{number}"
    base["sourceFamily"] = "ordinary-joint-hyperedge-v8-research"
    base["assemblyProvenance"]["method"] = "From-empty atomic seed plus learned joint native pin-hyperedge productions"
    base["assemblyProvenance"]["sourceScaffoldUsed"] = False
    context = list(target["context"])
    rejects = Counter()
    seed_indices = _soft_indices(artifact["seedRecords"], artifact["seedContextScaler"],
                                 artifact["seedNormalizedContexts"], context, rng, seed_options)
    best = None
    best_error = float("inf")
    unchanged_seeds = 0
    for seed_index in seed_indices:
        out = _seed(model, artifact, base, seed_index, context)
        check = integrity.check(out)
        if not check["nativeAssemblyIntegrityPassed"]:
            rejects["seedNativeIntegrity"] += 1
            continue
        raw = seam.check(out)
        if not raw["rawGeometrySeamPassed"]:
            rejects["seedNativeGeometry"] += 1
            continue
        seed_size = len(out["parts"])
        for step in range(max_steps + 1):
            check = integrity.check(out)
            if not check["nativeAssemblyIntegrityPassed"]:
                rejects["frontierNativeIntegrity"] += 1
                break
            size_error = abs(len(out["parts"]) - float(target["targetParts"]))
            if len(out["parts"]) > seed_size:
                fingerprint_matches = exact_source_reuse(out, source_fingerprints, templates)
                if fingerprint_matches:
                    rejects["exactWholeSourceReuse"] += 1
                elif size_error < best_error:
                    best, best_error = copy.deepcopy(out), size_error
            if step == max_steps:
                break
            draft = _grow(model, artifact, out, context, templates, seam, rng, graft_tries, rejects)
            if draft is None:
                break
            check = integrity.check(draft)
            if not check["nativeAssemblyIntegrityPassed"]:
                rejects["graftNativeIntegrity"] += 1
                continue
            raw = seam.check(draft)
            if not raw["rawGeometrySeamPassed"]:
                rejects["frontierNativeGeometry"] += 1
                continue
            out = draft
        if len(out["parts"]) <= seed_size:
            unchanged_seeds += 1
    if best is None:
        return None, {"status": "NO_CHANGED_NOVEL_INTEGRITY_PASS", "rejections": dict(rejects),
                      "unchangedSeedCount": unchanged_seeds}
    best["constructiveV8"] = {"accepted": False, "nativeIntegrity": integrity.check(best),
                               "nativeGeometryPreflight": _seam_summary(seam.check(best)),
                               "sourceScaffoldUsed": False,
                               "structuralScaffoldUsed": False,
                               "nativeReplayStatus": "NOT_RUN_UNITY_FORBIDDEN_IN_THIS_TASK",
                               "difficultyStatus": "UNMEASURED",
                               "label": "RESEARCH_PILOT_STRUCTURAL_ONLY_NO_EXACT_SOURCE_REUSE"}
    parts_by_kind = dict(Counter(p["kind"] for p in best["parts"]))
    productions = [d for d in best["assemblyProvenance"]["decisions"]
                   if d.get("kind") == "learnedJointNativeHyperedgeProductionV8"]
    best["constructiveV8"].update({"actualPartCount": len(best["parts"]), "partsByKind": parts_by_kind,
        "nativeLinkCount": len(best["links"]), "guideLinkCount": len(best["guideLinks"]),
        "productionCount": len(productions), "productionIds": [d["productionId"] for d in productions]})
    return best, {"status": "INTEGRITY_GATED_STRUCTURAL_RESEARCH_PROPOSAL", "rejections": dict(rejects),
                  "actualPartCount": len(best["parts"]), "partsByKind": parts_by_kind,
                  "nativeLinkCount": len(best["links"]), "guideLinkCount": len(best["guideLinks"]),
                  "productionCount": len(productions), "softLearnedTargetParts": float(target["targetParts"]),
                  "referencePartCountDiagnostic": int(len(reference["parts"])),
                  "nativeAssemblyIntegrityPassed": True,
                  "nativeGeometryPreflight": _seam_summary(seam.check(best)),
                  "structuralScaffoldUsed": False}


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("--workspace", type=Path, required=True)
    p.add_argument("--positions", nargs="+", type=int, required=True)
    p.add_argument("--count", type=int, default=1)
    p.add_argument("--seed", type=int, default=20261106)
    p.add_argument("--id-base", type=int, default=9800000)
    p.add_argument("--seed-options", type=int, default=8)
    p.add_argument("--graft-tries", type=int, default=16)
    p.add_argument("--max-steps", type=int, default=12)
    p.add_argument("--output", type=Path, required=True)
    a = p.parse_args()
    if a.count < 1:
        raise ValueError("--count must be positive")
    if a.output.exists():
        raise FileExistsError("Refuse to overwrite V8 research pilot output")
    w = a.workspace.resolve()
    source_filter, bindings = preflight(w)
    pack = w / "data/source500_native.json"
    templates_path = w / "data/source_templates.json"
    all_levels = json.loads(pack.read_text(encoding="utf8"), object_hook=payload_compact)["levels"]
    allowed = validate_source_filter(source_filter, sha(pack), [l["sourceId"] for l in all_levels])
    levels = [l for l in all_levels if str(l["sourceId"]) in allowed]
    by_position = {int(l["position"]): l for l in levels}
    missing = sorted(set(a.positions) - set(by_position))
    if missing:
        raise ValueError(f"Requested positions not in ordinary corpus: {missing}")
    templates = {t["id"]: t for t in json.loads(templates_path.read_text(encoding="utf8"))["templates"]}
    artifact_path = w / "models/joint_hyperedge_grammar_v8.joblib"
    organization_path = w / "models/organization_conditioned_v5.joblib"
    artifact = joblib.load(artifact_path)
    organization = joblib.load(organization_path)
    if artifact["hashes"]["sourcePack"] != sha(pack) or artifact["hashes"]["templates"] != sha(templates_path):
        raise ValueError("Stale V8 grammar against current source corpus/templates")
    if any(r["sourceId"] not in allowed for r in artifact["records"] + artifact["seedRecords"]):
        raise ValueError("Excluded source influenced V8 model")
    model = SimpleNamespace(levels={str(l["sourceId"]): l for l in levels}, templates=templates, tiles=[])
    integrity = NativeAssemblyIntegrityAudit(templates, levels)
    seam = NativeGeometrySeamAudit(templates, levels)
    fingerprints = {}
    for source in levels:
        fingerprints.setdefault(canonical_fingerprint(source, templates), []).append(str(source["sourceId"]))
    hashes = {"sourcePack": sha(pack), "templates": sha(templates_path),
              "grammarModel": sha(artifact_path), "organizationModel": sha(organization_path),
              "tool": sha(Path(__file__)), **bindings}
    rng = np.random.default_rng(a.seed)
    proposals, reports = [], []
    for ordinal in range(a.count):
        position = a.positions[ordinal % len(a.positions)]
        proposal, report = construct_one(artifact, model, by_position[position], organization, templates,
                                         integrity, seam, fingerprints, rng, a.id_base + ordinal, a.seed_options,
                                         a.graft_tries, a.max_steps, hashes)
        report.update(position=position, ordinal=ordinal)
        if proposal is not None:
            proposals.append(proposal)
            report["proposalId"] = proposal["id"]
        reports.append(report)
    result = {"schemaVersion": 8, "status": "RESEARCH_PILOT_STRUCTURAL_ONLY_NO_PRODUCTION_EXPORT",
              "complete": False, "sourceScaffoldUsed": False, "levels": proposals, "reports": reports,
              "seed": a.seed, "requestedCount": a.count,
              "hashes": {**hashes, "output": "written-after-serialization"},
              "localDependencySha256": local_dependency_hashes(Path(__file__).resolve()),
              "limits": ["Native source construction integrity only; Unity collision solver/replay not run.",
                         "Difficulty, solvability, originality, and human experience are unmeasured.",
                         "Failures and partial integrity-passing proposals remain in the report.",
                         "NativeGeometrySeamAudit allows only exact source-pair contact patches evidenced by imported source structure; unauthored solid overlaps and sensor contacts are rejected."]}
    a.output.parent.mkdir(parents=True, exist_ok=True)
    a.output.write_text(json.dumps(result, separators=(",", ":")), encoding="utf8")
    result["hashes"]["output"] = sha(a.output)
    # Bind the serialized output hash in a sidecar because modifying the file to
    # include its own hash would be self-referential.
    (a.output.with_suffix(a.output.suffix + ".sha256.json")).write_text(
        json.dumps({"outputSha256": sha(a.output), "seed": a.seed, "requestedCount": a.count}, indent=2), encoding="utf8")
    print(json.dumps({"status": result["status"], "proposals": len(proposals), "reports": reports,
                      "output": str(a.output.resolve())}, indent=2))


if __name__ == "__main__":
    main()
