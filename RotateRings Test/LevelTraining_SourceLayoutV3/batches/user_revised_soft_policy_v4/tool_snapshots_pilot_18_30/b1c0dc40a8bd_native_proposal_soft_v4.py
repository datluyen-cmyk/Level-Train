"""User-revised soft content policy, saved native macro/guide proposals only.

No historical options() or CSP generator is called. There are no root quotas,
pin-density bands, minimum angle edits or authored-branch-delta content gates.
Randomness selects captured native discrete structures, never pose jitter.
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
from scipy.special import logsumexp
from shapely import union_all

import guide_neighborhood_motifs_v3 as base
from guide_anchor_role_motifs_v3 import pair_context
from guide_anchor_patch_contract_v3 import AnchoredGuidePatchAudit
from guide_neighborhood_pilot_v3 import payload_compact
from native_asset_variant_v3 import clone_root, candidate, world
from layout_learning_v3 import read, hashfile, level_context
from assembly_contact_audit_v3 import NativeGeometrySeamAudit, shape_polygon
from learned_placement_audit_v3 import NativeLearnedPlacementAudit
from orientation_semantics_v3 import meaningful_edit
from layout_novelty_semantic_v3 import LayoutNoveltyIndex, canonical_fingerprint
from audit_authored_topology_freedom_v3 import graph_summary, graph_delta


def captured_guide_pose(reference, context, record):
    ps = {p["id"]: p for p in reference["parts"]}
    anchor, other = ps[context["anchorPrimaryId"]], ps[context["otherPrimaryId"]]
    phi = math.degrees(math.atan2(record["pose"][1], record["pose"][0]))
    delta = anchor["angle"]+phi-other["angle"]
    choices = {}
    for i in context["otherUnitIds"]:
        old = ps[i]
        xy = base.rotate([[old["x"]-other["x"], old["y"]-other["y"]]], delta)[0]
        pose = {**old, "x": float(other["x"]+xy[0]), "y": float(other["y"]+xy[1]), "angle": old["angle"]+delta}
        choices[i] = {"sourceId": reference["sourceId"], "donorId": i, "part": clone_root(old, reference["sourceId"], i, pose),
                      "edges": [], "dependencySymmetricDifference": 0, "oldTargets": [], "newTargets": [], "bodyPreserved": False,
                      "angleDeltaDegrees": delta, "angleSource": "Saved complete native guide-pair relative-angle pattern; source centers fixed and complete anchor preserved", "conditionalPoseNLL": None}
    return choices


def empirical_choice(rng, indices, distances):
    # The saved macro/pair estimators already use this conditional kernel.
    log_weight = -.5*np.asarray(distances, float)**2
    return int(rng.choice(indices, p=np.exp(log_weight-logsumexp(log_weight))))


def world_geometry_changed(old, actual, templates):
    """Exact-current-world geometry/input no-op diagnostic, no angle cutoff."""
    if old["templateId"] != actual["templateId"]:
        return True
    groups = [templates[old["templateId"]]["physicalShapes"], old.get("inputShapes", [])]
    for shapes in groups:
        for shape in shapes:
            if not shape.get("sourceEnabled", True):
                continue
            geometry = shape_polygon(shape)
            a, b = world(geometry, old), world(geometry, actual)
            if a.symmetric_difference(b).area > 1e-7:
                return True
    return False


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True, type=Path)
    parser.add_argument("--positions", nargs="+", type=int, default=[18, 30])
    parser.add_argument("--attempts-per-reference", type=int, default=80)
    parser.add_argument("--seed", type=int, default=20261106)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    if len(args.positions) != len(set(args.positions)):
        raise ValueError("Duplicate reference positions")
    w = args.workspace.resolve()
    source_path, template_path = w/"data/source500_native.json", w/"data/source_templates.json"
    with source_path.open() as stream:
        levels = json.load(stream, object_hook=payload_compact)["levels"]
    sources = {l["sourceId"]: l for l in levels}
    templates = {t["id"]: t for t in read(template_path)["templates"]}
    compact = {tid: base.compact_object(t) for tid, t in templates.items()}
    pair_path = w/"models/guide_anchor_role_motifs.joblib"
    pair = joblib.load(pair_path)
    macro_path = w/"models/macro_assembly.joblib"
    macro_evaluation = read(w/"models/macro_assembly_evaluation.json")
    if (macro_evaluation["hashes"]["sourcePack"] != hashfile(source_path)
            or macro_evaluation["hashes"]["templates"] != hashfile(template_path)
            or macro_evaluation["hashes"]["model"] != hashfile(macro_path)):
        raise ValueError("Saved native macro model/corpus binding differs")
    macro = joblib.load(macro_path)
    if pair["sourcePackSha256"] != hashfile(source_path) or pair["templatePackSha256"] != hashfile(template_path):
        raise ValueError("Saved pair model/corpus binding differs")
    units_by_source = {l["sourceId"]: base.units(l, compact) for l in levels}
    model_path = w/"models/composition_pose.joblib"
    pose_model = joblib.load(model_path)["model"]
    native = NativeLearnedPlacementAudit(levels, templates, model_path, source_path)
    seam = NativeGeometrySeamAudit(templates, levels)
    pattern_audit = AnchoredGuidePatchAudit(levels, templates, source_path, pair_path, allow_reference_patterns=True)
    extra = read(w/"data/novelty_extras/source_parity_pack.json")["levels"]
    nt = dict(templates)
    nt.update({t["id"]: t for t in read(w/"data/novelty_extras/source_templates.json")["templates"]})
    novelty = LayoutNoveltyIndex(levels+extra, nt)
    canonical_sources = {}
    for index, source in enumerate(levels+extra):
        canonical_sources.setdefault(canonical_fingerprint(source, nt), []).append(source["sourceId"])
        if index%100 == 0:
            print("SOFT_CANONICAL_SOURCE_INDEX", index, flush=True)
    rng = np.random.default_rng(args.seed)
    candidates, reports = [], []
    portfolio = set()
    started = time.monotonic()
    for position in args.positions:
        reference = next(l for l in levels if l["position"] == position)
        ps = {p["id"]: p for p in reference["parts"]}
        native_units = units_by_source[reference["sourceId"]]
        macro_indices = [i for i, tile in enumerate(macro.tiles) if tile["sourceId"] == reference["sourceId"]]
        # Explicit semantic annotation, not inferred from the convex hull.
        eligible_primaries = {2, 3, 4, 5, 6} if position == 30 and reference["sourceId"] == "10157" else {u["primary"] for u in native_units}
        rejects, traces, generated = Counter(), [], []
        local = {}
        for tid in {p["templateId"] for p in ps.values()}:
            t = templates[tid]
            shapes = [shape_polygon(s) for s in t["physicalShapes"]]
            local[tid] = {"solid": union_all([g for g, s in zip(shapes, t["physicalShapes"]) if not s["trigger"] and s.get("sourceEnabled", True) and s.get("sourceLayer", 7) == 7]),
                          "body": union_all([g for g, s in zip(shapes, t["physicalShapes"]) if s["role"] == "body"]), "shapes": shapes}
        old_graph = graph_summary(reference, True)
        for attempt in range(args.attempts_per_reference):
            if not macro_indices or not native_units:
                rejects["savedGrammarHasNoCompleteGuideUnitOrNativeMacro"] += 1
                break
            selected_macro = int(rng.choice(macro_indices))
            tile = macro.tiles[selected_macro]
            context = level_context(reference)+tile["descriptor"]
            distances, neighbors = macro.nn.kneighbors(macro.context_scaler.transform([context]))
            donor_macro_index = empirical_choice(rng, neighbors[0], distances[0])
            donor_macro = macro.tiles[donor_macro_index]
            donor_units = [u for u in units_by_source[donor_macro["sourceId"]] if set(u["ids"]).issubset(donor_macro["partIds"])]
            if not donor_units:
                rejects["sampledNativeMacroHasNoSupportedAtomicGuideUnit"] += 1
                continue
            donor_unit = donor_units[int(rng.integers(len(donor_units)))]
            # Edit arity/roles derive from a captured donor atomic unit, then a
            # matching whole native recipient unit in the sampled source macro.
            targets = [u for u in native_units if u["primary"] in eligible_primaries and set(u["ids"]).issubset(tile["partIds"])
                       and u["descriptor"][:5] == donor_unit["descriptor"][:5]]
            if not targets:
                rejects["sampledMacroAtomicSizeRoleHasNoRecipientSupport"] += 1
                continue
            other = targets[int(rng.integers(len(targets)))]
            original_pairs = [r for r in pair["records"] if r["sourceId"] == reference["sourceId"] and r["otherPrimaryId"] == other["primary"]]
            if not original_pairs:
                rejects["recipientHasNoCapturedNativePairAnchor"] += 1
                continue
            native_pair = original_pairs[int(rng.integers(len(original_pairs)))]
            anchor = native_pair["anchorPrimaryId"]
            recipient = pair_context(reference, compact, anchor, other["primary"])
            pair_distances, pair_neighbors = pair["model"].neighbors([recipient["context"]])
            record_index = empirical_choice(rng, pair_neighbors[0], pair_distances[0])
            record = pair["records"][record_index]
            choices = captured_guide_pose(reference, recipient, record)
            geometry_changed = [i for i, choice in choices.items() if world_geometry_changed(ps[i], choice["part"], templates)]
            if not geometry_changed:
                rejects["actualWorldPhysicalInputGeometryNoOp"] += 1
                continue
            c = candidate(reference, {}, templates, sources, local, pose_model, hashfile(source_path), hashfile(model_path), 7980000+position*1000+len(generated), 0, selected_override=choices)
            ap = c["assemblyProvenance"]
            ap["method"] = "SOFT_V4 saved empirical native macro atomic-size/role selection and one captured guide-pair angle; full native closure, no numeric content gates"
            ap["retargetedAssetImports"] = [{"sourceId": reference["sourceId"], "partMapping": {i: i for i in choices}}]
            ap["learnedPlacementCertificate"]["guideRigidGroups"] = [recipient["otherUnitIds"]]
            ap["anchoredGuidePatchCertificate"] = {"sourcePackSha256": hashfile(source_path), "modelSha256": hashfile(pair_path),
                "referenceSourceId": reference["sourceId"], "anchorPrimaryId": anchor, "otherPrimaryId": other["primary"],
                "recipientContext": recipient["context"], "learnedRecordIndex": record_index, "donorSourceId": record["sourceId"]}
            independent = pattern_audit.check(c)
            if not independent["passed"]:
                raise AssertionError(str(independent))
            nr, rr = native.check(c), seam.check(c)
            if not nr["nativeLearnedPlacementIntegrityPassed"]:
                rejects["nativeMechanicAssetInputRigidbodyGuideCorrectness"] += 1
                continue
            if not rr["rawGeometrySeamPassed"]:
                rejects["nativeRawSolidSensorCorrectness"] += 1
                continue
            fingerprint = canonical_fingerprint(c, nt)
            if fingerprint in canonical_sources or fingerprint in portfolio:
                rejects["semanticInvariantCanonicalDuplicate"] += 1
                continue
            review = novelty.check(c)  # Every result is retained as review evidence.
            actual = {p["id"]: p for p in c["parts"]}
            meaningful = [i for i in choices if meaningful_edit(ps[i], actual[i], templates)]
            phi = math.radians(actual[other["primary"]]["angle"]-ps[anchor]["angle"])
            nll = float(pair["model"].metrics([recipient["context"]], [[math.cos(phi), math.sin(phi)]], [reference["sourceId"]])["meanHeldSourceNLL"])
            c["softPolicyV4"] = {"numericContentGatesApplied": [], "accepted": False,
                "nativeAssetIntegrity": nr, "rawNativeGeometry": rr, "independentSavedPatternIntegrity": independent,
                "canonicalSemanticDuplicate": False, "semanticCanonicalSourcesCompared": len(levels)+len(extra),
                "heuristicNearCopyReviewOnly": review, "sourceRelativeRawMetrics": {"nativeRootCount": len(ps),
                    "sampledAtomicEditRootCount": len(choices), "meaningfulEditRootIds": meaningful, "meaningfulRootFraction": len(meaningful)/len(ps),
                    "worldPhysicalInputChangedRootIds": geometry_changed, "angleDeltaDegrees": choices[other["primary"]]["angleDeltaDegrees"],
                    "sourceAuthoredPinCount": len(reference["links"]), "candidateAuthoredPinCount": len(c["links"]),
                    "authoredPinDensityRatio": len(c["links"])/len(reference["links"]) if reference["links"] else None,
                    "authoredTopologyDelta": graph_delta(old_graph, graph_summary(c, True))},
                "empiricalSelection": {"sourceMacroIndex": selected_macro, "sourceMacroPartIds": tile["partIds"],
                    "donorMacroIndex": donor_macro_index, "donorMacroSourceId": donor_macro["sourceId"], "donorMacroPartIds": donor_macro["partIds"],
                    "sampledDonorAtomicUnitIds": donor_unit["ids"], "sampledDonorAtomicRoles": donor_unit["descriptor"][:5],
                    "recipientCompleteUnitIds": recipient["otherUnitIds"], "recipientWholeAnchorIds": recipient["anchorUnitIds"],
                    "savedRelativeAngleRecordIndex": record_index, "savedRelativeAngleRecordSourceId": record["sourceId"], "conditionalPatternNLL": nll},
                "wholeFigureReview": {"annotatedFamily": "intentional diamond" if position == 30 else None,
                    "editedAnnotatedInteriorPrimaryId": other["primary"] if position == 30 else None,
                    "recognizableWholeFigureStatus": "PENDING_VISUAL_REVIEW", "meaningfulNativeObstructionReleaseOrderStatus": "PENDING_CURRENT_PRODUCTION_REPLAY"},
                "limits": "One atomic complete guide unit only; model support/NLL is not gameplay quality, difficulty or acceptance. Heuristic nearest-layout thresholds never reject this branch."}
            generated.append(c)
            portfolio.add(fingerprint)
            print("SOFT_V4_PROPOSAL", position, c["id"], recipient["otherUnitIds"], choices[other["primary"]]["angleDeltaDegrees"], "nearcopy_review", len(review["nearCopies"]), flush=True)
        generated.sort(key=lambda c: c["softPolicyV4"]["empiricalSelection"]["conditionalPatternNLL"])
        candidates.extend(generated)
        reports.append({"position": position, "declaredAttempts": args.attempts_per_reference, "generatedPreliminary": len(generated),
                        "rejectionsOrUnsupportedSupport": dict(rejects), "numericContentGatesApplied": []})
        print("SOFT_V4_REFERENCE", reports[-1], flush=True)
    result = {"schemaVersion": 4, "complete": False, "generationEnabled": False, "acceptedCount": 0, "trainingSourceCount": len(levels),
              "status": "SOFT_NATIVE_PATTERN_PROPOSALS_PENDING_CURRENT_REPLAY_DIFFICULTY_WHOLEFIGURE_REVIEW", "seed": args.seed,
              "declaredBudget": {"attemptsPerReference": args.attempts_per_reference, "referencePositions": args.positions},
              "numericContentGatesApplied": [], "levels": candidates, "reports": reports, "elapsedSeconds": time.monotonic()-started,
              "hashes": {"source": hashfile(source_path), "templates": hashfile(template_path), "macroModel": hashfile(macro_path),
                         "pairModel": hashfile(pair_path), "localPoseModel": hashfile(model_path), "tool": hashfile(Path(__file__))},
              "limits": ["Saved grammar supports one complete2/3-root guide unit per proposal, not arbitrary whole-macro multi-unit edits.",
                         "Macro selection and atomic arity/role come from captured empirical native records; discrete saved angle patterns only, no random XY/angle shuffle.",
                         "Native/raw correctness and semantic invariant canonical duplicate checks are hard; numeric design features and heuristic nearcopy are review-only.",
                         "Canonicalization is the existing semantic-orientation representation; exact world physical/input no-ops are also rejected. It is not a proof of intellectual originality.",
                         "Learned proposal NLL/support has previously worse held-family NLL than baseline; no quality superiority claim. Current native gameplay/difficulty and visual review remain required."]}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, separators=(",", ":")), encoding="utf8")
    print("SOFT_V4_DONE", len(candidates), flush=True)


if __name__ == "__main__":
    main()
