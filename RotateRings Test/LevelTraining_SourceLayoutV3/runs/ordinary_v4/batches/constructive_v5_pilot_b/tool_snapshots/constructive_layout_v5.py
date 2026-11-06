"""Construct ordinary native motif assemblies from an empty board.

The saved organization prior provides a soft whole-layout target. All geometry
comes from rigid complete native motifs; new placement comes from exact native
whole-root sockets or saved conditional macro-relation poses. No source board
is used as a scaffold, and no count/density/angle content quota is applied.
"""
from pathlib import Path
from collections import Counter
import argparse
import copy
import json
import math
import time

import joblib
import numpy as np
from scipy.special import logsumexp

from macro_assembly_v3 import instantiate_tile, pose_matrix, socket_key, tile_descriptor
from composition_field_v3 import posed_field
from assembly_integrity_audit_v3 import NativeAssemblyIntegrityAudit
from assembly_contact_audit_v3 import NativeGeometrySeamAudit
from layout_novelty_semantic_v3 import LayoutNoveltyIndex, canonical_fingerprint
from ordinary_source_filter_guard_v4 import preflight, validate_source_filter, sha
from refit_ordinary_models_v4 import payload_compact, local_dependency_hashes


def native_closure(level, initial):
    """Pilot support: preserve touched guide groups and all outgoing pin targets."""
    result = set(initial)
    while True:
        old = set(result)
        for edge in level["links"]:
            if edge["source"] in result:
                result.add(edge["target"])
        for edge in level["guideLinks"]:
            if edge["guideOwnerId"] in result or edge["lockedOwnerId"] in result:
                result.update((edge["guideOwnerId"], edge["lockedOwnerId"]))
        if result == old:
            return sorted(result)


def closed_library(saved, templates):
    """Keep learned prototype frames/descriptors; expand only native closure."""
    model = copy.copy(saved)
    model.templates = templates
    model.tiles = []
    eligible, unsupported = [], Counter()
    for index, original in enumerate(saved.tiles):
        tile = copy.deepcopy(original)
        source = saved.levels[tile["sourceId"]]
        tile["originalPrototypePartIds"] = list(tile["partIds"])
        tile["partIds"] = native_closure(source, tile["partIds"])
        tile["nativeOutgoingGuideClosurePartIds"] = list(tile["partIds"])
        model.tiles.append(tile)
        if len(tile["partIds"]) >= len(source["parts"]):
            unsupported["closureEqualsWholeSourceBoard"] += 1
        elif len(tile["partIds"]) < 2:
            unsupported["notMultiRootNativeMotif"] += 1
        else:
            eligible.append(index)
    return model, eligible, dict(unsupported)


def empty_level(number, position, target, hashes):
    serialized = {k: v.tolist() if isinstance(v, np.ndarray) else v for k, v in target.items()}
    return {"id": number, "sourceId": "constructive-v5-"+str(number), "sourceFamily": "ordinary-constructive-v5",
        "position": int(position), "sourceOriginal": False, "complete": False, "accepted": False,
        "validationStatus": "CONSTRUCTIVE_NATIVE_MOTIFS_PENDING_REPLAY_DIFFICULTY_AND_VISUAL_REVIEW",
        "parts": [], "links": [], "guideLinks": [], "bombs": [],
        "assemblyProvenance": {"method": "From-empty learned organization and native motif socket/relation assembly",
            "sourceScaffoldUsed": False, "emptyStart": {"parts": 0, "links": 0, "guideLinks": 0, "bombs": 0},
            "organizationTarget": serialized, "modelHashes": hashes, "tileImports": [], "decisions": []}}


def score(level, target, organization, templates):
    """Rank in the learned joint field/profile scaler, never by content cutoffs."""
    if not level["parts"]:
        return float("inf"), {}
    maps, orientation = posed_field(level, target["heading"], target["span"], max(1., target["targetParts"]), templates)
    vector = np.r_[maps.ravel(), orientation.ravel()*.2]
    # Profile scale is learned from ordinary originals. Coordinate span here is
    # the realized frame extent, not a requested exact part-count requirement.
    from composition_field_v3 import native_frame
    from layout_learning_v3 import level_context
    span = native_frame(level, templates, target["heading"])[2]
    profile = np.r_[math.log1p(len(level["parts"])), math.log(max(span, .01)), level_context(level)]
    expected = np.r_[math.log1p(target["targetParts"]), math.log(target["span"]), target["context"]]
    latent = organization.pca.transform([vector])[0]
    target_vector = np.r_[np.asarray(target["field"]).ravel(), np.asarray(target["orientation"]).ravel()*.2]
    target_latent = organization.pca.transform([target_vector])[0]
    actual_z = organization.scaler.transform([np.r_[latent, profile]])[0]
    target_z = organization.scaler.transform([np.r_[target_latent, expected]])[0]
    value = float(np.mean(np.abs(actual_z-target_z)))
    return value, {"jointLearnedStandardizedFieldProfileMAE": value,
        "kindFieldL1": float(np.abs(maps[:4]-target["field"][:4]).sum()),
        "nativeBodyFieldL1": float(np.abs(maps[4:8]-target["field"][4:8]).sum()),
        "relationFieldL1": float(np.abs(maps[8:]-target["field"][8:]).sum()),
        "actualRootCount": len(level["parts"]), "softTargetRootCount": target["targetParts"],
        "actualSpan": span, "softTargetSpan": target["span"], "limits": "Proposal organization only; not native difficulty or quality."}


def weighted_sample(rng, indices, costs, count):
    if not indices:
        return []
    weights = -.5*np.asarray(costs, float)**2
    weights = np.exp(weights-logsumexp(weights))
    count = min(count, len(indices))
    return [indices[int(i)] for i in rng.choice(len(indices), size=count, replace=False, p=weights)]


def place(model, out, option):
    draft = copy.deepcopy(out)
    tile = model.tiles[option["tileIndex"]]
    mapping = instantiate_tile(model, option["tileIndex"], draft, option["rotation"], option["translation"],
        (option["sourceAnchor"], option["targetAnchor"]) if option["kind"] == "exactWholeNativeSocket" else None)
    imported = draft["assemblyProvenance"]["tileImports"][-1]
    imported.update(originalPrototypePartIds=tile["originalPrototypePartIds"],
        nativeOutgoingGuideClosurePartIds=tile["nativeOutgoingGuideClosurePartIds"])
    decision = {**option, "sourceId": tile["sourceId"], "partMapping": mapping}
    draft["assemblyProvenance"]["decisions"].append(decision)
    center = pose_matrix(option["rotation"])@np.asarray(tile["center"])+np.asarray(option["translation"])
    cluster = {"tileIndex": option["tileIndex"], "center": center.tolist(),
        "heading": tile["heading"]+option["rotation"], "width": tile["width"], "prototypeDescriptor": tile["descriptor"]}
    return draft, cluster


def relation_options(model, out, clusters, eligible, costs, context, rng, count, relation_hash):
    options = []
    choices = weighted_sample(rng, eligible, [costs[i] for i in eligible], count)
    for index in choices:
        tile = model.tiles[index]
        anchor_index = int(rng.integers(len(clusters)))
        anchor = clusters[anchor_index]
        query = list(context)+anchor["prototypeDescriptor"]+tile["descriptor"]
        sampled = model.pose_model.sample_pose(query, rng)
        relative = np.asarray(sampled["pose"])
        heading = anchor["heading"]+math.degrees(math.atan2(relative[3], relative[2]))
        rotation = heading-tile["heading"]
        center = np.asarray(anchor["center"])+pose_matrix(anchor["heading"])@relative[:2]*max(anchor["width"], tile["width"])
        translation = center-pose_matrix(rotation)@np.asarray(tile["center"])
        options.append({"kind": "savedConditionalMacroRelation", "tileIndex": index,
            "rotation": float(rotation), "translation": translation.tolist(), "anchorClusterIndex": anchor_index,
            "anchorFrame": anchor, "relationModelSha256": relation_hash, "relationRecordIndex": sampled["learnedRecordIndex"],
            "recipientContext": query, "sampledRelativePose": relative.tolist()})
    return options


def socket_options(model, out, eligible_set, costs, rng, count):
    options = []
    for anchor in out["parts"]:
        if anchor["kind"] not in ("ring", "slider"):
            continue
        for index, old in model.by_socket.get(socket_key(anchor, model.templates, out), []):
            if index not in eligible_set:
                continue
            tile = model.tiles[index]
            source = next(p for p in model.levels[tile["sourceId"]]["parts"] if p["id"] == old)
            rotation = anchor["angle"]-source["angle"]
            translation = np.asarray([anchor["x"], anchor["y"]])-pose_matrix(rotation)@np.asarray([source["x"], source["y"]])
            options.append({"kind": "exactWholeNativeSocket", "tileIndex": index, "sourceAnchor": old,
                "targetAnchor": anchor["id"], "rotation": float(rotation), "translation": translation.tolist()})
    if len(options) > count:
        selected = weighted_sample(rng, list(range(len(options))), [costs[o["tileIndex"]] for o in options], count)
        options = [options[i] for i in selected]
    return options


def construct(model, eligible, organization, templates, target, rng, number, budgets, hashes, integrity, seam, canonical_sources):
    out = empty_level(number, target["position"], target, hashes)
    contexts = [list(target["context"])+t["descriptor"] for t in model.tiles]
    normalized = model.context_scaler.transform(contexts)
    costs = np.linalg.norm(normalized-model.normalized_tile_contexts, axis=1)
    rejects, clusters = Counter(), []
    best, best_score, best_metrics = None, float("inf"), None
    eligible_set = set(eligible)
    seeds = weighted_sample(rng, eligible, [costs[i] for i in eligible], budgets["seedOptions"])
    candidates = []
    for index in seeds:
        tile = model.tiles[index]
        rotation = target["heading"]-tile["heading"]
        translation = -(pose_matrix(rotation)@np.asarray(tile["center"]))
        option = {"kind": "properNativeMotifSeed", "tileIndex": index, "rotation": float(rotation), "translation": translation.tolist()}
        draft, cluster = place(model, out, option)
        raw = seam.check(draft)
        if not raw["rawGeometrySeamPassed"]:
            rejects["seedNativeRawGeometryUnsupported"] += 1
            continue
        value, metrics = score(draft, target, organization, templates)
        candidates.append((value, draft, cluster, metrics))
    if not candidates:
        return None, {"rejections": dict(rejects), "status": "NO_NATIVE_SEED_WITHIN_FINITE_SUPPORT"}
    _, out, cluster, _ = min(candidates, key=lambda row: row[0])
    clusters.append(cluster)
    for step in range(budgets["frontierSteps"]):
        options = socket_options(model, out, eligible_set, costs, rng, budgets["socketOptionsPerStep"])
        options += relation_options(model, out, clusters, eligible, costs, target["context"], rng,
            budgets["relationOptionsPerStep"], hashes["macroModel"])
        candidates = []
        for option in options:
            try:
                draft, cluster = place(model, out, option)
            except (ValueError, KeyError, StopIteration) as error:
                rejects["nativePlacementSupport:"+type(error).__name__] += 1
                continue
            if len(draft["parts"]) == len(out["parts"]):
                rejects["noNewNativeRoots"] += 1
                continue
            raw = seam.check(draft)
            if not raw["rawGeometrySeamPassed"]:
                rejects["frontierNativeRawGeometryUnsupported"] += 1
                continue
            native = integrity.check(draft)
            if not native["nativeAssemblyIntegrityPassed"]:
                rejects["frontierNativeClosureAssetIntegrity"] += 1
                continue
            value, metrics = score(draft, target, organization, templates)
            candidates.append((value, draft, cluster, metrics, raw, native))
        if not candidates:
            rejects["finiteFrontierNoSupportedContinuation"] += 1
            break
        value, out, cluster, metrics, raw, native = min(candidates, key=lambda row: row[0])
        clusters.append(cluster)
        fingerprint = canonical_fingerprint(out, templates)
        if fingerprint in canonical_sources:
            rejects["wholeBoardSemanticCanonicalSourceEquivalent"] += 1
            continue
        if value < best_score:
            best, best_score, best_metrics = copy.deepcopy(out), value, metrics
            best["constructiveV5"] = {"accepted": False, "sourceScaffoldUsed": False, "numericContentGatesApplied": [],
                "nativeIntegrity": native, "rawGeometry": raw, "organizationScore": metrics,
                "semanticCanonicalDuplicate": False, "nativeReplayStatus": "PENDING", "difficultyStatus": "UNMEASURED"}
        print("CONSTRUCTIVE_PREFIX", number, step+1, len(out["parts"]), round(value, 6), "best", round(best_score, 6), flush=True)
    return best, {"status": "PRELIMINARY_CONSTRUCTIVE_PREFIX" if best else "NO_NOVEL_MULTI_MOTIF_PREFIX_WITHIN_FINITE_SUPPORT",
        "rejections": dict(rejects), "bestOrganizationScore": best_metrics, "visitedMotifCount": len(clusters)}


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--workspace", type=Path, required=True)
    p.add_argument("--positions", nargs="+", type=int, required=True)
    p.add_argument("--attempts-per-position", type=int, default=2)
    p.add_argument("--frontier-steps", type=int, default=8)
    p.add_argument("--seed-options", type=int, default=8)
    p.add_argument("--socket-options-per-step", type=int, default=12)
    p.add_argument("--relation-options-per-step", type=int, default=12)
    p.add_argument("--seed", type=int, default=20261106)
    p.add_argument("--output", type=Path, required=True)
    args = p.parse_args()
    if args.output.exists():
        raise ValueError("Refuse existing constructive output")
    w = args.workspace.resolve()
    source_filter, bindings = preflight(w)
    source_path, template_path = w/"data/source500_native.json", w/"data/source_templates.json"
    with source_path.open(encoding="utf8") as stream:
        levels = json.load(stream, object_hook=payload_compact)["levels"]
    allowed = validate_source_filter(source_filter, sha(source_path), [l["sourceId"] for l in levels])
    templates = {t["id"]: t for t in json.loads(template_path.read_text(encoding="utf8"))["templates"]}
    saved = joblib.load(w/"models/macro_assembly.joblib")
    organization = joblib.load(w/"models/organization_v5.joblib")
    if set(map(str, saved.levels)) != allowed or set(map(str, organization.source_ids)) != allowed:
        raise ValueError("Models contain excluded or missing ordinary source influence")
    if organization.hashes["sourcePack"] != sha(source_path) or organization.hashes["templates"] != sha(template_path):
        raise ValueError("Stale organization model inputs")
    model, eligible, support = closed_library(saved, templates)
    extra = json.loads((w/"data/novelty_extras/source_parity_pack.json").read_text(encoding="utf8"))["levels"]
    novelty_templates = dict(templates)
    novelty_templates.update({t["id"]: t for t in json.loads((w/"data/novelty_extras/source_templates.json").read_text(encoding="utf8"))["templates"]})
    canonical_sources = {canonical_fingerprint(l, novelty_templates) for l in levels+extra}
    novelty = LayoutNoveltyIndex(levels+extra, novelty_templates)
    integrity, seam = NativeAssemblyIntegrityAudit(templates, levels), NativeGeometrySeamAudit(templates, levels)
    hashes = {"sourcePack": sha(source_path), "templates": sha(template_path), "macroModel": sha(w/"models/macro_assembly.joblib"),
        "organizationModel": sha(w/"models/organization_v5.joblib"), "tool": sha(Path(__file__)), **bindings}
    budgets = {"frontierSteps": args.frontier_steps, "seedOptions": args.seed_options,
        "socketOptionsPerStep": args.socket_options_per_step, "relationOptionsPerStep": args.relation_options_per_step}
    rng, candidates, reports = np.random.default_rng(args.seed), [], []
    started = time.monotonic()
    for position in args.positions:
        for attempt in range(args.attempts_per_position):
            target = organization.sample(position, rng)
            if any(str(s) not in allowed for s in target["latentSourceRecords"]):
                raise ValueError("Excluded organization source support")
            number = 9000000+position*100+attempt
            candidate, report = construct(model, eligible, organization, templates, target, rng, number, budgets, hashes, integrity, seam, canonical_sources)
            report.update(position=position, attempt=attempt)
            if candidate:
                fingerprint = canonical_fingerprint(candidate, templates)
                if fingerprint in canonical_sources:
                    raise AssertionError("Canonical duplicate escaped constructor")
                candidate["constructiveV5"]["heuristicNearCopyReviewOnly"] = novelty.check(candidate)
                candidates.append(candidate)
                canonical_sources.add(fingerprint)
                report["candidateId"] = candidate["id"]
            reports.append(report)
            print("CONSTRUCTIVE_ATTEMPT", position, attempt, report["status"], flush=True)
    result = {"schemaVersion": 5, "status": "FROM_EMPTY_NATIVE_MOTIF_RESEARCH_PENDING_INDEPENDENT_NATIVE_REPLAY_DIFFICULTY_VISUAL",
        "complete": False, "acceptedCount": 0, "trainingSourceCount": len(levels), "seed": args.seed,
        "sourceScaffoldUsed": False, "numericContentGatesApplied": [], "declaredSearchBudget": budgets,
        "nativeMotifSupport": {"eligibleClosedProperMotifs": len(eligible), "unsupported": support},
        "levels": candidates, "reports": reports, "elapsedSeconds": time.monotonic()-started, "hashes": hashes,
        "localDependencySha256": local_dependency_hashes(Path(__file__).resolve()),
        "limits": ["Whole-layout organization field has worse held-family field reconstruction than baseline; no quality superiority.",
            "Outgoing/guide closure is this pilot's conservative support rule, not a law of the original game.",
            "Proper multi-root motifs and learned whole-field ranking do not establish native solve, human difficulty or enjoyable design.",
            "Finite frontier/options are compute budgets, not content minimums, maximums or quotas.",
            "Scalable deterministic seeds do not certify 1000/10000 unique accepted playable levels; actual capacity remains unmeasured."]}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, separators=(",", ":")), encoding="utf8")
    print("CONSTRUCTIVE_COMPLETE", len(candidates), result["elapsedSeconds"], flush=True)


if __name__ == "__main__":
    main()
