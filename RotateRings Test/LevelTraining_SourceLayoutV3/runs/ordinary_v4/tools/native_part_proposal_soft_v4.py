"""Whole native single-part proposals selected through saved macro/local models.

No historical options/CSP is called. Native donor roots remain complete; no
asset resizing, XY noise, angle jitter, edit quota or density/angle/count gate.
Only nonguide, nonbomb, nonspecial roots are supported by this finite branch.
"""
from collections import Counter
from pathlib import Path
import argparse
import copy
import json
import math
import time

import joblib
import numpy as np
from shapely import union_all

from native_proposal_soft_v4 import empirical_choice, world_geometry_changed
from native_asset_variant_v3 import body_key, state, special_root, clone_root, candidate, world
from native_feature_socket_v3 import feature_shape_map
from assembly_contact_audit_v3 import NativeGeometrySeamAudit, shape_polygon
from learned_placement_audit_v3 import NativeLearnedPlacementAudit
from layout_learning_v3 import level_context, relation_rows, hashfile, read
from orientation_semantics_v3 import meaningful_edit
from layout_novelty_semantic_v3 import LayoutNoveltyIndex, canonical_fingerprint
from ordinary_source_filter_guard_v4 import preflight, validate_source_filter
from refit_ordinary_models_v4 import payload_compact, local_dependency_hashes


class LazyNativeGeometry(dict):
    def __init__(self, templates):
        super().__init__()
        self.templates = templates

    def __missing__(self, tid):
        shapes = self.templates[tid]["physicalShapes"]
        polygons = [shape_polygon(s) for s in shapes]
        result = {"shapes": polygons,
            "solid": union_all([g for g, s in zip(polygons, shapes) if not s["trigger"] and s.get("sourceEnabled", True) and s.get("sourceLayer", 7) == 7]),
            "body": union_all([g for g, s in zip(polygons, shapes) if s["role"] == "body"])}
        self[tid] = result
        return result


def eligible_ids(level):
    unsupported = {i for e in level["guideLinks"] for i in (e["guideOwnerId"], e["lockedOwnerId"])}
    unsupported.update(b["ownerPartId"] for b in level["bombs"])
    return {p["id"] for p in level["parts"] if p["kind"] in ("ring", "slider") and p["id"] not in unsupported and not special_root(p)}


def captured_angles(reference, target, donor_source, donor, templates, model, rng):
    """Return captured source-direction and conditional local-angle hypotheses."""
    ps = {p["id"]: p for p in reference["parts"]}
    ds = {p["id"]: p for p in donor_source["parts"]}
    angles = [(target["angle"], {"kind": "recipientNativeBodyPose"})]
    outgoing = [(i, e) for i, e in enumerate(donor_source["links"]) if e["source"] == donor["id"]]
    for index, edge in outgoing:
        old = ds[edge["target"]]
        dx, dy = old["x"]-donor["x"], old["y"]-donor["y"]
        if math.hypot(dx, dy) < 1e-9:
            continue
        for q in ps.values():
            if q["id"] == target["id"] or body_key(q, templates) != body_key(old, templates):
                continue
            ax, ay = q["x"]-target["x"], q["y"]-target["y"]
            if math.hypot(ax, ay) < 1e-9:
                continue
            angle = donor["angle"]+math.degrees(math.atan2(ay, ax)-math.atan2(dy, dx))
            angles.append((angle, {"kind": "capturedDonorNativeRelativeCenterDirection", "sourceEdgeIndex": index, "alignedRecipientTargetId": q["id"]}))
    # A no-outgoing native receiver may use a saved conditional incoming
    # relative-angle record. XY centers and complete native dimensions stay fixed.
    if not outgoing:
        probe = {**reference, "parts": [clone_root(donor, donor_source["sourceId"], target["id"], target) if p["id"] == target["id"] else p for p in reference["parts"]],
            "links": [edge for edge in reference["links"] if edge["source"] != target["id"]]}
        for row in relation_rows(probe, templates):
            if row["relation"] != "pin" or row["targetPart"] != target["id"]:
                continue
            distances, indices = model.neighbors([row["context"]])
            record = empirical_choice(rng, indices[0], distances[0])
            pose = model.y[record]
            angles.append((ps[row["sourcePart"]]["angle"]+math.degrees(math.atan2(pose[3], pose[2])),
                {"kind": "savedConditionalIncomingRelativeAngle", "learnedRecordIndex": record, "recipientContext": row["context"], "parentPartId": row["sourcePart"]}))
    unique = {}
    for angle, evidence in angles:
        unique.setdefault(round(angle % 360, 6), (angle, evidence))
    return list(unique.values())


def map_complete_outgoing(reference, target, donor_source, donor, pose, templates, local):
    """Map every authored outgoing donor relation to actual native body contact."""
    ps = {p["id"]: p for p in reference["parts"]}
    ds = {p["id"]: p for p in donor_source["parts"]}
    dt = templates[donor["templateId"]]
    mapping, edges = {}, []
    for index, edge in enumerate(donor_source["links"]):
        if edge["source"] != donor["id"]:
            continue
        old = ds[edge["target"]]
        head = world(union_all([local[donor["templateId"]]["shapes"][i] for i in edge["sourceShapeIndices"] if dt["physicalShapes"][i]["role"] == "head_trigger"]), pose)
        contacts = []
        for q in ps.values():
            if q["id"] == target["id"] or body_key(q, templates) != body_key(old, templates) or (old["id"] in mapping and mapping[old["id"]] != q["id"]):
                continue
            body = world(local[q["templateId"]]["body"], q)
            shape_map = feature_shape_map(templates[old["templateId"]], templates[q["templateId"]], edge["targetShapeIndices"])
            if shape_map is not None and head.distance(body) <= 1e-5:
                contacts.append((float(head.intersection(body).area), q["id"], shape_map))
        if not contacts:
            return None
        _, qid, shape_map = max(contacts, key=lambda c: (c[0], -c[1]))
        mapping[old["id"]] = qid
        actual = copy.deepcopy(edge)
        actual.update(source=target["id"], target=qid, targetShapeIndices=[shape_map[i] for i in edge["targetShapeIndices"]], targetAttachmentId=-1)
        edges.append({"edge": actual, "sourcePatternSourceId": donor_source["sourceId"], "sourcePatternEdgeIndex": index, "targetOriginalId": old["id"]})
    return edges


def sampled_macro_audit(evidence, reference, actual, macro, templates, local_model):
    """Recompute saved macro support and exact native donor/recipient identity."""
    source_tile = macro.tiles[evidence["recipientMacroIndex"]]
    donor_tile = macro.tiles[evidence["donorMacroIndex"]]
    if source_tile["sourceId"] != reference["sourceId"]:
        raise ValueError("Wrong recipient macro source")
    context = level_context(reference)+source_tile["descriptor"]
    _, neighbors = macro.nn.kneighbors(macro.context_scaler.transform([context]))
    if evidence["donorMacroIndex"] not in neighbors[0]:
        raise ValueError("Donor macro outside saved conditional neighborhood")
    tid, did = evidence["recipientPartId"], evidence["donorPartId"]
    donor_level = macro.levels[donor_tile["sourceId"]]
    target = next(p for p in reference["parts"] if p["id"] == tid)
    donor = next(p for p in donor_level["parts"] if p["id"] == did)
    if evidence["donorSourceId"] != donor_level["sourceId"]:
        raise ValueError("Reported donor source differs from saved macro")
    if tid not in source_tile["partIds"] or did not in donor_tile["partIds"] or tid not in eligible_ids(reference) or did not in eligible_ids(donor_level):
        raise ValueError("Wrong or unsupported complete native part membership")
    if body_key(target, templates) != body_key(donor, templates) or state(target) != state(donor):
        raise ValueError("Native body/state mismatch")
    result = next(p for p in actual["parts"] if p["id"] == tid)
    if result["templateId"] != donor["templateId"] or result["atomicProvenance"]["sourceId"] != donor_level["sourceId"] or result["atomicProvenance"]["partId"] != did:
        raise ValueError("Candidate donor provenance mismatch")
    angle = evidence["capturedAngle"]
    if angle["kind"] == "recipientNativeBodyPose":
        expected_angle = target["angle"]
    elif angle["kind"] == "capturedDonorNativeRelativeCenterDirection":
        edge = donor_level["links"][angle["sourceEdgeIndex"]]
        if edge["source"] != did:
            raise ValueError("Captured direction has wrong donor relation")
        old = next(p for p in donor_level["parts"] if p["id"] == edge["target"])
        aligned = next(p for p in reference["parts"] if p["id"] == angle["alignedRecipientTargetId"])
        if aligned["id"] == tid or body_key(aligned, templates) != body_key(old, templates):
            raise ValueError("Captured direction has wrong recipient body")
        expected_angle = donor["angle"]+math.degrees(math.atan2(aligned["y"]-target["y"], aligned["x"]-target["x"])-math.atan2(old["y"]-donor["y"], old["x"]-donor["x"]))
    elif angle["kind"] == "savedConditionalIncomingRelativeAngle":
        if any(e["source"] == did for e in donor_level["links"]):
            raise ValueError("Receiver angle applied to outgoing native pattern")
        probe = {**reference, "parts": [clone_root(donor, donor_level["sourceId"], tid, target) if p["id"] == tid else p for p in reference["parts"]],
            "links": [edge for edge in reference["links"] if edge["source"] != tid]}
        rows = [r for r in relation_rows(probe, templates) if r["relation"] == "pin" and r["targetPart"] == tid and r["sourcePart"] == angle["parentPartId"]]
        if len(rows) != 1 or not np.allclose(rows[0]["context"], angle["recipientContext"], rtol=0, atol=1e-9):
            raise ValueError("Saved local angle context mismatch")
        record = angle["learnedRecordIndex"]
        if record not in local_model.neighbors([rows[0]["context"]])[1][0]:
            raise ValueError("Saved local angle record outside conditional support")
        parent = next(p for p in reference["parts"] if p["id"] == angle["parentPartId"])
        y = local_model.y[record]
        expected_angle = parent["angle"]+math.degrees(math.atan2(y[3], y[2]))
    else:
        raise ValueError("Unrecognized empirical angle derivation")
    if abs((result["angle"]-expected_angle+180)%360-180) > 1e-5 or abs(result["x"]-target["x"]) > 1e-8 or abs(result["y"]-target["y"]) > 1e-8:
        raise ValueError("Candidate pose differs from captured source/local pattern")
    return {"passed": True, "conditionalMacroSupportRecomputed": True, "completeNativeBodyStateMatch": True,
        "capturedSourceOrSavedLocalAngleRecomputed": True,
        "limits": "Empirical saved-macro selection and native part correspondence only, not gameplay or difficulty."}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True, type=Path)
    parser.add_argument("--positions", nargs="+", required=True, type=int)
    parser.add_argument("--attempts-per-reference", type=int, default=80)
    parser.add_argument("--seed", type=int, default=20261106)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    w = args.workspace.resolve()
    if args.output.exists():
        raise ValueError("Refuse to overwrite an existing proposal pack")
    if len(args.positions) != len(set(args.positions)):
        raise ValueError("Duplicate requested positions")
    source_filter, bindings = preflight(w)
    source_path, template_path = w/"data/source500_native.json", w/"data/source_templates.json"
    with source_path.open(encoding="utf8") as stream:
        levels = json.load(stream, object_hook=payload_compact)["levels"]
    allowed = validate_source_filter(source_filter, hashfile(source_path), [l["sourceId"] for l in levels])
    sources = {l["sourceId"]: l for l in levels}
    templates = {t["id"]: t for t in read(template_path)["templates"]}
    macro = joblib.load(w/"models/macro_assembly.joblib")
    if set(map(str, macro.levels)) != allowed or any(str(t["sourceId"]) not in allowed for t in macro.tiles):
        raise ValueError("Excluded source influence in saved macro model")
    model_path = w/"models/composition_pose.joblib"
    bundle = joblib.load(model_path)
    if any(str(r["sourceId"]) not in allowed for r in bundle["relationRecords"]):
        raise ValueError("Excluded source influence in saved local model")
    model = bundle["model"]
    native = NativeLearnedPlacementAudit(levels, templates, model_path, source_path)
    seam = NativeGeometrySeamAudit(templates, levels)
    extras = read(w/"data/novelty_extras/source_parity_pack.json")["levels"]
    all_templates = dict(templates)
    all_templates.update({t["id"]: t for t in read(w/"data/novelty_extras/source_templates.json")["templates"]})
    novelty = LayoutNoveltyIndex(levels+extras, all_templates)
    fingerprints = {canonical_fingerprint(l, all_templates) for l in levels+extras}
    local = LazyNativeGeometry(templates)
    rng, output, reports = np.random.default_rng(args.seed), [], []
    started = time.monotonic()
    source_hash, model_hash = hashfile(source_path), hashfile(model_path)
    eligible = {sid: eligible_ids(l) for sid, l in sources.items()}
    for position in args.positions:
        reference = next(l for l in levels if l["position"] == position)
        ps = {p["id"]: p for p in reference["parts"]}
        tiles = [i for i, t in enumerate(macro.tiles) if t["sourceId"] == reference["sourceId"] and eligible[reference["sourceId"]]&set(t["partIds"])]
        rejects, generated = Counter(), []
        for attempt in range(args.attempts_per_reference):
            if not tiles:
                rejects["noNonguideNonbombNonspecialNativePartSupport"] += 1
                break
            tile_index = int(rng.choice(tiles))
            tile = macro.tiles[tile_index]
            target = ps[int(rng.choice(sorted(eligible[reference["sourceId"]]&set(tile["partIds"]))))]
            context = level_context(reference)+tile["descriptor"]
            distances, neighbors = macro.nn.kneighbors(macro.context_scaler.transform([context]))
            donor_index = empirical_choice(rng, neighbors[0], distances[0])
            donor_tile = macro.tiles[donor_index]
            donor_source = sources[donor_tile["sourceId"]]
            donors = [p for p in donor_source["parts"] if p["id"] in set(donor_tile["partIds"])&eligible[donor_source["sourceId"]]
                and body_key(p, templates) == body_key(target, templates) and state(p) == state(target)]
            if not donors:
                rejects["sampledConditionalMacroHasNoWholeNativeBodyStateDonor"] += 1
                continue
            donor = donors[int(rng.integers(len(donors)))]
            angles = captured_angles(reference, target, donor_source, donor, templates, model, rng)
            angle, angle_evidence = angles[int(rng.integers(len(angles)))]
            pose = clone_root(donor, donor_source["sourceId"], target["id"], {**target, "angle": angle})
            if not world_geometry_changed(target, pose, templates):
                rejects["actualWorldPhysicalInputGeometryNoOp"] += 1
                continue
            edges = map_complete_outgoing(reference, target, donor_source, donor, pose, templates, local)
            if edges is None:
                rejects["completeDonorNativeOutgoingPatternHasNoActualContact"] += 1
                continue
            old_targets = sorted({e["target"] for e in reference["links"] if e["source"] == target["id"]})
            new_targets = sorted({e["edge"]["target"] for e in edges})
            rotated = abs((angle-target["angle"]+180)%360-180) > 1e-6
            choice = {"sourceId": donor_source["sourceId"], "donorId": donor["id"], "part": pose, "edges": edges,
                "dependencySymmetricDifference": len(set(old_targets)^set(new_targets)), "oldTargets": old_targets, "newTargets": new_targets,
                "bodyPreserved": not rotated, "angleDeltaDegrees": angle-target["angle"], "angleSource": angle_evidence["kind"], "conditionalPoseNLL": None}
            try:
                c = candidate(reference, {}, templates, sources, local, model, source_hash, model_hash, 8200000+position*1000+len(generated), attempt, selected_override={target["id"]: choice})
            except ValueError as error:
                rejects["nativePatternConstruction:"+str(error)] += 1
                continue
            evidence = {"recipientMacroIndex": tile_index, "donorMacroIndex": donor_index,
                "recipientPartId": target["id"], "donorPartId": donor["id"], "donorSourceId": donor_source["sourceId"], "capturedAngle": angle_evidence}
            independent = sampled_macro_audit(evidence, reference, c, macro, templates, model)
            nr, rr = native.check(c), seam.check(c)
            if not nr["nativeLearnedPlacementIntegrityPassed"]:
                rejects["nativeAssetInputRigidbodySourcePatternCorrectness"] += 1
                continue
            if not rr["rawGeometrySeamPassed"]:
                rejects["nativeRawSolidSensorCorrectness"] += 1
                continue
            fingerprint = canonical_fingerprint(c, all_templates)
            if fingerprint in fingerprints:
                rejects["semanticInvariantCanonicalDuplicate"] += 1
                continue
            actual = next(p for p in c["parts"] if p["id"] == target["id"])
            relation = relation_rows(c, templates)
            affected = [r for r in relation if target["id"] in (r["sourcePart"], r["targetPart"])]
            nll = float(-model.loglikelihood([r["context"] for r in affected], [r["pose"] for r in affected]).mean()) if affected else None
            c["assemblyProvenance"]["method"] = "SOFT_V4 saved conditional native macro sampling, complete native nonguide part and captured source/local relation poses"
            c["softNativePartPolicyV4"] = {"accepted": False, "numericContentGatesApplied": [], "nativeAssetIntegrity": nr,
                "rawNativeGeometry": rr, "independentSavedMacroIntegrity": independent, "empiricalSelection": evidence,
                "conditionalLocalPoseNLL": nll, "canonicalSemanticDuplicate": False, "semanticCanonicalSourcesCompared": len(levels)+len(extras),
                "heuristicNearCopyReviewOnly": novelty.check(c), "sourceRelativeRawMetrics": {"changedRootIds": [target["id"]],
                    "meaningfulEditRootIds": [target["id"]] if meaningful_edit(target, actual, templates) else [], "angleDeltaDegrees": angle-target["angle"],
                    "sourceRootCount": len(reference["parts"]), "candidateRootCount": len(c["parts"]), "sourceAuthoredPinCount": len(reference["links"]), "candidateAuthoredPinCount": len(c["links"])},
                "limits": "One captured complete native nonguide root; source-native framework centers preserved. Nearcopy/difficulty/visual review required; no native replay or human quality claim."}
            generated.append(c)
            fingerprints.add(fingerprint)
            print("SOFT_NATIVE_PART_PROPOSAL", position, c["id"], target["id"], donor_source["sourceId"], donor["id"], angle-target["angle"], flush=True)
        generated.sort(key=lambda c: (c["softNativePartPolicyV4"]["conditionalLocalPoseNLL"] is None, c["softNativePartPolicyV4"]["conditionalLocalPoseNLL"] or 0))
        output.extend(generated)
        reports.append({"position": position, "declaredAttempts": args.attempts_per_reference, "generatedPreliminary": len(generated), "rejectionsOrUnsupportedSupport": dict(rejects), "numericContentGatesApplied": []})
        print("SOFT_NATIVE_PART_REFERENCE", reports[-1], flush=True)
    result = {"schemaVersion": 4, "complete": False, "acceptedCount": 0, "generationEnabled": False, "trainingSourceCount": len(levels),
        "status": "PRELIMINARY_COMPLETE_NATIVE_PART_PATTERNS_PENDING_CURRENT_REPLAY_DIFFICULTY_VISUAL_REVIEW", "seed": args.seed,
        "declaredBudget": {"referencePositions": args.positions, "attemptsPerReference": args.attempts_per_reference}, "numericContentGatesApplied": [],
        "levels": output, "reports": reports, "elapsedSeconds": time.monotonic()-started,
        "hashes": {"sourcePack": source_hash, "templates": hashfile(template_path), "localPoseModel": model_hash,
            "macroModel": hashfile(w/"models/macro_assembly.joblib"), "tool": hashfile(Path(__file__)), **bindings},
        "localDependencySha256": local_dependency_hashes(Path(__file__).resolve()),
        "limits": ["Complete source native assets and authored donor relation patterns only; no random XY/angle jitter.",
            "No root quota, angle magnitude, pin density, spacing-ratio or component-profile gate. Native geometry tolerances are correctness checks.",
            "Source-relative framework can remain a heuristic near-copy; every such result retained for review.",
            "Saved empirical proposal support and NLL are not a native solve, difficulty, human player behavior or acceptance."]}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, separators=(",", ":")), encoding="utf8")
    print("SOFT_NATIVE_PART_DONE", len(output), flush=True)


if __name__ == "__main__":
    main()
