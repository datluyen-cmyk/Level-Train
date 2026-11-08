"""Source-grounded multi-motif level generator.

V9 copied one complete source graph and then rotated it.  This module keeps
the native macro assembly learner, but makes it the production path: a
candidate is composed from several *source-native* motifs using learned
inter-motif poses, source topology and whole-board geometry.  Native links,
guides, collider shapes and template values are copied only inside their
verified motif; no free random ring/slider is introduced.

The output remains a proposal until the native replay/bot gates accept it.
"""
from __future__ import annotations

import argparse
import copy
import json
import math
import sys
import warnings
import hashlib
from collections import Counter
from pathlib import Path

import joblib
import networkx as nx
import numpy as np

warnings.filterwarnings("ignore", category=RuntimeWarning, module="(numpy|global_composition_v3)")

# Joblib stores the concrete semantic model's module name. Import it before
# unpickling so the round's isolated tools directory is on the module path.
try:
    import semantic_macro_assembly_v3  # noqa: F401
except ImportError:
    pass
from macro_assembly_v3 import proposal
from layout_novelty_semantic_v3 import LayoutNoveltyIndex, canonical_fingerprint, describe, compare
from source_physical_composition_v3 import composition_edges
from cohesive_assembly_v11 import CohesiveDesignPolicy


def read(path: Path):
    return json.loads(path.read_text(encoding="utf-8"))


def verify_model_inputs(workspace: Path, report_name: str, macro_model: Path | None = None):
    """Reject stale source/template catalogs, even if joblib loads successfully."""
    report_path = workspace / 'models' / report_name
    if not report_path.is_file():
        raise ValueError(f'Missing model input certificate {report_path}; fit ML models again.')
    hashes = read(report_path).get('hashes', {})
    required = {'sourcePack': workspace / 'data/source500_native.json',
                'templates': workspace / 'data/source_templates.json'}
    if macro_model is not None:
        required['macroModel'] = macro_model
    for key, path in required.items():
        actual = hashlib.sha256(path.read_bytes()).hexdigest()
        if hashes.get(key) != actual:
            raise ValueError(f'{report_name}: {key} changed since fitting; fit ML models again before generation.')


def locate(w: Path, name: str, parent: Path | None = None) -> Path:
    """Use the round's model first, then the immutable trained bundle."""
    options = [w / "models" / name]
    if parent is not None:
        options.append(parent / "models" / name)
    for path in options:
        if path.is_file():
            return path
    raise FileNotFoundError(f"Missing trained artifact: {name}")


def graph_stats(level: dict) -> dict:
    graph = nx.Graph()
    graph.add_nodes_from(int(p["id"]) for p in level.get("parts", []))
    graph.add_edges_from((int(e["source"]), int(e["target"])) for e in level.get("links", []))
    graph.add_edges_from((int(e["guideOwnerId"]), int(e["lockedOwnerId"]))
                        for e in level.get("guideLinks", []))
    components = sorted((len(c) for c in nx.connected_components(graph)), reverse=True)
    kinds = Counter(p.get("kind") for p in level.get("parts", []))
    roots = max(1, sum(kinds[k] for k in ("ring", "slider")))
    movable = [p for p in level.get("parts", []) if p.get("kind") in ("ring", "slider")]
    if movable:
        span = max(max(p.get("x", 0.0) for p in movable) - min(p.get("x", 0.0) for p in movable),
                   max(p.get("y", 0.0) for p in movable) - min(p.get("y", 0.0) for p in movable))
    else:
        span = 0.0
    nearest = []
    for index, part in enumerate(movable):
        nearest.append(min((math.hypot(part.get("x", 0.0) - other.get("x", 0.0),
                                     part.get("y", 0.0) - other.get("y", 0.0))
                            for j, other in enumerate(movable) if j != index), default=0.0))
    centroids = []
    for component in nx.connected_components(graph):
        members = [p for p in level.get("parts", []) if int(p["id"]) in component]
        if members:
            centroids.append((sum(float(p.get("x", 0.0)) for p in members) / len(members),
                              sum(float(p.get("y", 0.0)) for p in members) / len(members)))
    component_gaps = []
    for index, center in enumerate(centroids):
        component_gaps.append(min((math.hypot(center[0] - other[0], center[1] - other[1])
                                   for j, other in enumerate(centroids) if j != index), default=0.0))
    return {
        "parts": len(level.get("parts", [])),
        "links": len(level.get("links", [])),
        "guides": len(level.get("guideLinks", [])),
        "components": len(components),
        "largestComponentFraction": (components[0] / max(1, len(graph)) if components else 0.0),
        "ringFraction": kinds.get("ring", 0) / roots,
        "sliderFraction": kinds.get("slider", 0) / roots,
        "movableSpan": float(span),
        "nearestP90": float(np.quantile(nearest, 0.90)) if nearest else 0.0,
        "nearestMax": float(max(nearest or [0.0])),
        "componentGapP90": float(np.quantile(component_gaps, 0.90)) if component_gaps else 0.0,
        "largestComponent": int(components[0]) if components else 0,
        "kinds": dict(kinds),
    }


def structural_distance(candidate: dict, reference: dict) -> float:
    """Rank style-preserving assemblies without rewarding graph copies."""
    a, b = graph_stats(candidate), graph_stats(reference)
    inventory = sum(abs(a["kinds"].get(k, 0) - b["kinds"].get(k, 0))
                    for k in set(a["kinds"]) | set(b["kinds"]))
    return (abs(a["parts"] - b["parts"]) / max(1, b["parts"]) +
            0.55 * inventory / max(1, b["parts"]) +
            0.35 * abs(a["links"] - b["links"]) / max(1, b["parts"]) +
            0.35 * abs(a["guides"] - b["guides"]) / max(1, b["parts"]) +
            0.18 * abs(a["components"] - b["components"]) / max(1, b["components"]) +
            0.25 * abs(a["largestComponentFraction"] - b["largestComponentFraction"]) +
            0.20 * abs(a["movableSpan"] - b["movableSpan"]) / max(1.0, b["movableSpan"]))


def composition_profile(level: dict, templates: dict) -> dict:
    """Measure source-native static obstruction motifs used by the solver."""
    edges = composition_edges(level, templates)
    counts = Counter(str(edge.get("kind")) for edge in edges)
    strong = sum(bool(edge.get("strong")) for edge in edges)
    return {"total": len(edges), "strong": strong, "counts": dict(counts)}


def prepare_candidate(candidate: dict, reference: dict, candidate_id: int,
                      novelty: LayoutNoveltyIndex, source_ids: set[str]) -> tuple[dict | None, dict]:
    candidate = copy.deepcopy(candidate)
    candidate["id"] = int(candidate_id)
    candidate["sourceId"] = f"relational-v10-{candidate_id}"
    candidate["sourceOriginal"] = False
    candidate["sourceFamily"] = "ordinary-macro-assembly-v10"
    candidate["validationStatus"] = "RELATIONAL_V10_PENDING_NATIVE_REPLAY"
    provenance = candidate.setdefault("assemblyProvenance", {})
    decisions = provenance.get("decisions", [])
    used = sorted({str(d.get("sourceId")) for d in decisions if d.get("sourceId")})
    provenance.update({
        "method": "source-native-macro-assembly-v10",
        "completeGraphDonor": None,
        "sourceIds": used,
        "sourceFamilyCount": len(used),
        "multipleSourceFamilies": len(used) >= 2,
        "nativeReplayRequired": True,
        "qualityPolicy": "v11 donor budgets + native outline boundary growth + whole-board bridge + novelty",
    })
    candidate_fp = canonical_fingerprint(candidate, novelty.templates)
    if candidate_fp in source_ids:
        return None, {"status": "CANONICAL_SOURCE_COPY_REJECTED"}
    novelty_row = novelty.check(candidate)
    if not novelty_row.get("passed", False):
        return None, {"status": "WHOLE_LAYOUT_NEAR_COPY_REJECTED", "novelty": novelty_row}
    target = graph_stats(reference)
    actual = graph_stats(candidate)
    design_audit = CohesiveDesignPolicy(reference, novelty.templates).audit(candidate)
    if not design_audit['passed']:
        return None, {'status': 'COHESIVE_DESIGN_REJECTED', 'audit': design_audit}
    # A candidate can evade the strict whole-layout novelty threshold by
    # appending a small unrelated tile to an otherwise copied donor. Treat a
    # very high-coverage, low-error match as a copy even when the extra tile
    # makes the strict matcher return false.
    candidate_description = describe(candidate, novelty.templates)
    partial_copy = None
    for source_description in novelty.records:
        match = compare(candidate_description, source_description)
        if (match.get("coverage", 0.0) >= 0.90 and
                match.get("meanCenterErrorWidths", 99.0) <= 1.50 and
                match.get("p90CenterErrorWidths", 99.0) <= 3.00 and
                match.get("meanAngleErrorDegrees", 99.0) <= 15.0 and
                match.get("meanShapeError", 99.0) <= 0.12):
            partial_copy = {"sourceId": source_description.get("id"), **match}
            break
    if partial_copy is not None:
        return None, {"status": "PARTIAL_SOURCE_NEAR_COPY_REJECTED", "match": partial_copy}
    target_composition = composition_profile(reference, novelty.templates)
    actual_composition = composition_profile(candidate, novelty.templates)
    # Keep at least 90% of the target board scale. The earlier 78% floor let
    # a 21-part candidate stand in for a 24-part source and made the bot
    # difficulty collapse even though its edge counts looked plausible.
    enough = actual["parts"] >= max(2, math.ceil(target["parts"] * 0.90))
    # Large boards often need five or six native motifs. The learned pose and
    # topology gates below decide quality; family count alone must not force a
    # tiny board or produce an empty batch.
    multiple = 2 <= len(used) <= 12
    score = structural_distance(candidate, reference)
    # Part count alone can hide a mechanically wrong inventory (for example a
    # 22-part board containing one ring and ten sliders). Keep the source
    # design language in the hard gate so retries search for a better motif
    # combination instead of accepting a visually unrelated board.
    root_inventory_ok = (
        abs(actual["ringFraction"] - target["ringFraction"]) <= 0.25 and
        abs(actual["sliderFraction"] - target["sliderFraction"]) <= 0.25 and
        abs(actual["kinds"].get("fixed", 0) - target["kinds"].get("fixed", 0))
        <= max(2, math.ceil(target["kinds"].get("fixed", 0) * 0.45))
    )
    topology_ok = score <= 0.45
    span_ratio = actual["movableSpan"] / max(0.01, target["movableSpan"])
    # Pure-ring boards are compact lattice puzzles; macro tiles can require a
    # little more envelope while retaining the same ring-only visual grammar.
    span_upper = 1.60 if target_composition["total"] == 0 else 1.35
    layout_scale_ok = 0.68 <= span_ratio <= span_upper
    cohesion_ok = (
        actual["nearestMax"] <= max(0.01, target["nearestMax"] * 1.45) and
        actual["componentGapP90"] <= max(0.01, target["componentGapP90"] * 1.60) and
        actual["largestComponent"] <= max(1, math.ceil(target["largestComponent"] * 1.60))
    )
    # A board with the right counts can still be a set of unrelated islands.
    # Preserve at least 70% of the source's native pull/ring obstruction
    # evidence, including at least one strong relation whenever the source has
    # one. This is the gate that prevents the low-block, visually sparse V10
    # candidates found in the first validation batch.
    composition_floor = 0 if target_composition["total"] == 0 else max(1, math.ceil(target_composition["total"] * 0.70))
    composition_ceiling = (None if target_composition["total"] == 0
                           else math.ceil(target_composition["total"] * 1.80))
    composition_ok = (
        actual_composition["total"] >= composition_floor and
        (composition_ceiling is None or actual_composition["total"] <= composition_ceiling) and
        actual_composition["strong"] >= (math.ceil(target_composition["strong"] * 0.70)
                                           if target_composition["strong"] else 0)
    )
    if (not enough or not multiple or not root_inventory_ok or not topology_ok or
            not layout_scale_ok or not composition_ok or not cohesion_ok):
        return None, {"status": "STRUCTURAL_TARGET_REJECTED", "parts": actual["parts"],
                      "targetParts": target["parts"], "sourceFamilyCount": len(used), "score": score,
                      "checks": {'enoughParts': enough, 'multipleSources': multiple,
                                 'inventory': root_inventory_ok, 'topology': topology_ok,
                                 'layoutScale': layout_scale_ok, 'composition': composition_ok,
                                 'cohesion': cohesion_ok},
                      'candidateGraphStats': actual, 'targetGraphStats': target,
                      "movableSpanRatio": span_ratio, "composition": actual_composition,
                      "targetComposition": target_composition, "cohesion": {
                          "candidate": {k: actual[k] for k in ("nearestMax", "componentGapP90", "largestComponent")},
                          "target": {k: target[k] for k in ("nearestMax", "componentGapP90", "largestComponent")}}}
    candidate["constructiveV10"] = {
        "accepted": False,
        "nativeReplayStatus": "PENDING",
        "structuralDistance": score,
        "targetGraphStats": target,
        "candidateGraphStats": actual,
        "movableSpanRatio": span_ratio,
        "targetComposition": target_composition,
        "candidateComposition": actual_composition,
        "novelty": novelty_row,
        "cohesiveDesign": design_audit,
    }
    return candidate, {"status": "MULTI_MOTIF_PROPOSAL", "candidateId": candidate_id,
                       "sourceIds": used, "sourceFamilyCount": len(used),
                       "structuralDistance": score, "parts": actual["parts"]}


def main() -> None:
    p = argparse.ArgumentParser()
    p.add_argument("--workspace", type=Path, required=True)
    p.add_argument("--positions", nargs="+", type=int, required=True)
    p.add_argument("--count", type=int, default=1)
    p.add_argument("--seed", type=int, default=20261108)
    p.add_argument("--id-base", type=int, default=9950000)
    p.add_argument("--seed-options", type=int, default=8)
    p.add_argument("--graft-tries", type=int, default=12)
    p.add_argument("--max-steps", type=int, default=10)
    p.add_argument("--difficulty-target", choices=("Source", "Normal", "Hard", "VeryHard"))
    p.add_argument("--output", type=Path, required=True)
    p.add_argument('--diagnostics-dir', type=Path,
                   help='Optional rejected draft snapshots; never included in playable levels')
    args = p.parse_args()
    if args.output.exists():
        raise FileExistsError("Refuse existing V10 output")
    w = args.workspace.resolve()
    immutable = w.parent.parent if (w.parent.parent / "models").is_dir() else None
    source = read(w / "data/source500_native.json")
    templates = {t["id"]: t for t in read(w / "data/source_templates.json")["templates"]}
    levels = source["levels"]
    by_position = {int(l["position"]): l for l in levels}
    positions = list(dict.fromkeys(args.positions))
    available = [pos for pos in positions if pos in by_position]
    missing = [pos for pos in positions if pos not in by_position]
    if not available:
        raise ValueError(f"No requested positions in corpus: {missing}")
    # Prefer the round-local model: its corpus and template catalog are the
    # exact files later consumed by the Unity bot.  The newer physical model
    # is only safe when the round was built from the same expanded catalog.
    if (w / "models/macro_physical_assembly.joblib").is_file():
        model_path = w / "models/macro_physical_assembly.joblib"
        verify_model_inputs(w, 'macro_physical_assembly_evaluation.json')
    elif (w / "models/macro_assembly.joblib").is_file():
        model_path = w / "models/macro_assembly.joblib"
    else:
        model_path = locate(w, "macro_physical_assembly.joblib", immutable) if (immutable and (immutable / "models/macro_physical_assembly.joblib").is_file()) else locate(w, "macro_assembly.joblib", immutable)
    model = joblib.load(model_path)
    model.templates = templates
    global_model = joblib.load(locate(w, "global_composition.joblib", immutable))
    # The semantic model and its continuation model are fitted on the same
    # round corpus.  Their scaler widths therefore match; never graft a
    # pattern artifact from the immutable 500-level bundle onto an ordinary
    # round with a different template catalog.
    pattern_path = w / "models/macro_physical_pattern.joblib"
    if "macro_physical_assembly" in model_path.name and pattern_path.is_file():
        verify_model_inputs(w, 'macro_physical_pattern_evaluation.json', model_path)
        model.pattern_model = joblib.load(pattern_path)
    field_path = w / 'models/whole_board_field.joblib'
    if field_path.is_file():
        verify_model_inputs(w, 'whole_board_field_evaluation.json')
        model.field_model = joblib.load(field_path)
    novelty = LayoutNoveltyIndex(levels, templates)
    source_fps = {canonical_fingerprint(level, templates) for level in levels}
    rng = np.random.default_rng(args.seed)
    proposals, reports = [], []
    for ordinal in range(args.count):
        position = available[ordinal % len(available)]
        reference = by_position[position]
        best, best_report, best_score = None, None, float("inf")
        tries = max(1, int(args.graft_tries))
        design_policy = CohesiveDesignPolicy(reference, templates)
        attempts = []
        target_tile_sizes = [len(tile.get("partIds", [])) for tile in model.tiles
                             if str(tile.get("sourceId")) == str(reference.get("sourceId", ""))]
        # If the target source itself has a near-complete macro tile, anchoring
        # it would recreate the exact donor skeleton. Use corpus-wide learned
        # seeds for that case and let the target enter only through the style
        # and topology context.
        target_anchor_allowed = (not target_tile_sizes or
                                 max(target_tile_sizes) <= len(reference.get("parts", [])) * 0.50)
        for attempt in range(tries):
            # Spend the first half of the budget anchored to the target source
            # motif. If native seams cannot continue that anchor, use the
            # learned corpus-wide seed as a fallback rather than returning an
            # incomplete board.
            seed_hint = (str(reference.get("sourceId", ""))
                         if target_anchor_allowed and attempt < max(1, tries // 2) else None)
            raw = proposal(model, reference, global_model, templates, rng,
                           int(args.id_base + ordinal),
                           max_tiles=max(4, min(20, int(args.max_steps))),
                           seed_source_id=seed_hint, design_policy=design_policy,
                           completion_validator=lambda draft: prepare_candidate(
                               draft, reference, int(args.id_base + ordinal), novelty, source_fps)[0] is not None)
            candidate, report = prepare_candidate(raw, reference, int(args.id_base + ordinal), novelty, source_fps)
            if args.diagnostics_dir is not None:
                args.diagnostics_dir.mkdir(parents=True, exist_ok=True)
                snapshot = args.diagnostics_dir / f'position{position}_candidate{args.id_base + ordinal}_attempt{attempt + 1}.json'
                if snapshot.exists():
                    raise FileExistsError(snapshot)
                snapshot.write_text(json.dumps({'schemaVersion': 3, 'complete': False,
                                                'diagnosticOnly': True, 'review': report,
                                                'levels': [raw]}), encoding='utf-8')
            attempts.append({'attempt': attempt + 1, 'status': report['status'],
                             'parts': len(raw['parts']),
                             'review': report.copy(),
                             'searchRejections': raw.get('assemblyProvenance', {}).get('rejectionCounts', {})})
            print(json.dumps({'position': position, **attempts[-1]}), flush=True)
            if candidate is not None:
                score = candidate["constructiveV10"]["structuralDistance"]
                if score < best_score:
                    best, best_report, best_score = candidate, report, score
            elif best_report is None:
                best_report = report
        if best is not None:
            proposals.append(best)
            best_report.update(position=position, ordinal=ordinal)
        else:
            best_report = best_report or {"status": "NO_VALID_MULTI_MOTIF_PROPOSAL"}
            best_report.update(position=position, ordinal=ordinal)
        reports.append(best_report)
        best_report['attempts'] = attempts
    result = {
        "schemaVersion": 10,
        "status": "RELATIONAL_V10_NATIVE_REPLAY_PENDING",
        "complete": False,
        "sourceScaffoldUsed": False,
        "levels": proposals,
        "reports": reports,
        "seed": args.seed,
        "requestedCount": args.count,
        "difficultyTarget": args.difficulty_target,
        "requestedPositions": positions,
        "availablePositions": available,
        "skippedPositions": missing,
        "generator": "multi-source native macro composition with learned inter-motif poses",
        "cohesivePolicyVersion": 11,
        "wholeBoardFieldEnabled": hasattr(model, 'field_model'),
        "limits": ["Native Unity replay and human review remain required.",
                   "Every accepted candidate uses at least two source-native motifs.",
                   "Novelty is a whole-layout gate; it does not claim copyright or human fun."],
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, separators=(",", ":")), encoding="utf-8")
    print(json.dumps({"status": result["status"], "proposals": len(proposals),
                      "reports": reports, "output": str(args.output.resolve())}, indent=2))


if __name__ == "__main__":
    main()
