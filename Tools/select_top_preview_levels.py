"""Select the strongest local candidates and stage a Unity Training Preview pack.

The selector is deliberately a review aid. It ranks candidates by similarity to
their source pair and copies no data into Resources or production exports.
"""
from __future__ import annotations

import argparse
import datetime as dt
import json
import math
import shutil
from collections import Counter
from pathlib import Path

from quality_evaluator_v1 import compare as compare_source_quality
from cohesive_assembly_v11 import CohesiveDesignPolicy


ROOT = Path(__file__).resolve().parents[1]
DEFAULT_RUNS = ROOT / "RotateRings Test/LevelTraining_SourceLayoutV3/runs"


def read(path: Path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def is_native_macro(candidate: dict) -> bool:
    return bool(candidate.get('assemblyProvenance', {}).get('tileImports'))


def row_by_id(summary: dict, level_id: int, role: str):
    for row in summary.get("byLevel", []):
        if str(row.get("levelId")) == str(level_id) and row.get("role") == role:
            return row
    return None


def q50(row: dict | None, field: str) -> float:
    return float(((row or {}).get(field) or {}).get("p50", 0.0))


def _delta_degrees(value: float) -> float:
    return ((float(value) + 180.0) % 360.0) - 180.0


def _movement_axis_angle(part: dict, templates: dict[str, dict]) -> float:
    template = templates.get(str(part.get("templateId")), {})
    movement = template.get("movement") or {}
    axis = movement.get("axisLocal") or {}
    x = float(axis.get("x", 1.0))
    y = float(axis.get("y", 0.0))
    if abs(x) < 1e-9 and abs(y) < 1e-9:
        return 0.0
    return math.degrees(math.atan2(y, x))


def align_preview_guide_orientations(candidate: dict, source: dict,
                                     templates: dict[str, dict]) -> list[dict]:
    """Carry the source guide-to-slider facing class into the preview copy.

    Source levels legitimately use 0, 90 and 180 degree guide offsets. The
    preview must preserve the relation to the slider's *actual movement axis*;
    using only part.angle makes templates with a 90-degree local axis appear
    correct while their physical guide collider blocks the slider. This only
    edits the staged preview DTO, leaving the generated batch evidence intact.
    """
    source_parts = {int(p["id"]): p for p in source.get("parts", [])}
    source_deltas = []
    for link in source.get("guideLinks", []):
        guide = source_parts.get(int(link.get("guideOwnerId", -1)))
        slider = source_parts.get(int(link.get("lockedOwnerId", -1)))
        if guide and slider:
            source_axis = _movement_axis_angle(slider, templates)
            source_deltas.append(_delta_degrees(float(guide.get("angle", 0.0)) -
                                                 (float(slider.get("angle", 0.0)) + source_axis)))
    if not source_deltas:
        return []
    candidate_parts = {int(p["id"]): p for p in candidate.get("parts", [])}
    adjustments = []
    for index, link in enumerate(candidate.get("guideLinks", [])):
        guide = candidate_parts.get(int(link.get("guideOwnerId", -1)))
        slider = candidate_parts.get(int(link.get("lockedOwnerId", -1)))
        if not guide or not slider:
            continue
        wanted = source_deltas[index % len(source_deltas)]
        candidate_axis = _movement_axis_angle(slider, templates)
        before = _delta_degrees(float(guide.get("angle", 0.0)) -
                                (float(slider.get("angle", 0.0)) + candidate_axis))
        if abs(_delta_degrees(before - wanted)) <= 0.01:
            continue
        guide["angle"] = float(slider.get("angle", 0.0)) + candidate_axis + wanted
        adjustments.append({"guideOwnerId": int(guide["id"]), "lockedOwnerId": int(slider["id"]),
                            "beforeDelta": before, "afterDelta": wanted,
                            "candidateMovementAxis": candidate_axis})
    return adjustments


def describe_candidate(candidate: dict, source: dict, cand_row: dict | None, ref_row: dict | None,
                       source_trace: dict | None = None):
    wanted = Counter(p.get("kind") for p in source.get("parts", []))
    actual = Counter(p.get("kind") for p in candidate.get("parts", []))
    composition_error = sum(abs(actual.get(k, 0) - value) for k, value in wanted.items())
    link_error = abs(len(candidate.get("links", [])) - len(source.get("links", [])))
    guide_error = abs(len(candidate.get("guideLinks", [])) - len(source.get("guideLinks", [])))
    candidate_win = float((cand_row or {}).get("winRateExcludingEngineErrors") or 0.0)
    source_win = float((ref_row or {}).get("winRateExcludingEngineErrors") or 0.0)
    win_error = abs(candidate_win - source_win)
    action_error = abs(q50(cand_row, "actionCountAllSessionQuantiles") -
                       q50(ref_row, "actionCountAllSessionQuantiles"))
    censor = int((cand_row or {}).get("budgetCensoredSessions", 0))
    engines = int((cand_row or {}).get("engineErrorSessions", 0))
    bot_zero_win = candidate_win <= 0.0
    source_has_observed_win = source_win > 0.0
    # Per session: early-stopped candidates and cached references can have
    # different session counts.
    cand_blocks = float((cand_row or {}).get("physicalBlocks", 0) or 0) / max(1, int((cand_row or {}).get("sessions", 0) or 1))
    ref_blocks = float((ref_row or {}).get("physicalBlocks", 0) or 0) / max(1, int((ref_row or {}).get("sessions", 0) or 1))
    # Interlock fidelity. From progression ~13 the bot wins ~0% on source and
    # candidate alike, so win rate/action count (capped by the step limit) carry
    # no difficulty signal there. Physical blocks (collisions/blocked pulls) still
    # separate levels: round-1 candidates had ~0.35x the source's blocks.
    block_log_ratio = math.log((cand_blocks + 0.5) / (ref_blocks + 0.5))
    saturated = source_win <= 0.2 and candidate_win <= 0.2
    too_easy_win = candidate_win - source_win > 0.2
    interlock_error = abs(block_log_ratio) * (1.5 if block_log_ratio < 0 else 1.0)
    win_weight = 0.0 if saturated else 8.0 * (2.0 if too_easy_win else 1.0)
    action_weight = 0.0 if saturated else 0.08
    # Lower is better. Mechanism mismatch is weighted above bot noise because
    # the virtual player is a proxy and the source composition is authoritative.
    # The generator already uses this source-relative signal for structural
    # reranking. Recompute it here so selection across old rounds also accounts
    # for visual layout and puzzle archetype, rather than only counts/bot rows.
    try:
        quality = compare_source_quality(candidate, source, source_trace=source_trace)
    except Exception as error:
        quality = {"qualityScore": 10.0, "staticQualityScore": 10.0,
                   "visualError": 10.0, "puzzleError": 10.0,
                   "difficultyProxyError": 10.0,
                   "status": "QUALITY_EVALUATION_FAILED", "error": repr(error)}
    score = (2.0 * composition_error + 0.7 * link_error + 1.5 * guide_error +
             win_weight * win_error + action_weight * action_error +
             4.0 * interlock_error + 2.25 * float(quality.get("staticQualityScore", 10.0)) +
             10.0 * engines + 0.05 * censor)
    # A candidate that never wins while the matching source does is not safe to
    # stage for manual play. Keep it in the report for diagnosis, but let the
    # selector block that progression position until a later round produces a
    # playable proposal.
    # A structural proposal with a different kind/edge inventory is not a
    # faithful difficulty sample.  Keep it in the diagnostic report so the
    # next training round can learn from the failure, but never put it in the
    # Unity manual-review pack.  This prevents a visually plausible, yet
    # mechanically under-constrained, board from being mistaken for a 1:1
    # source-relative level.
    if composition_error or guide_error:
        review_status = "COMPOSITION_MISMATCH_REVIEW_REQUIRED"
    elif bot_zero_win and source_has_observed_win:
        review_status = "BOT_ZERO_WIN_SOURCE_PLAYABLE"
    else:
        review_status = "OK"
    return {"score": float(score), "compositionError": int(composition_error),
            "linkError": int(link_error), "guideError": int(guide_error),
            "candidateWinRate": candidate_win, "sourceWinRate": source_win,
            "candidateActionP50": q50(cand_row, "actionCountAllSessionQuantiles"),
            "sourceActionP50": q50(ref_row, "actionCountAllSessionQuantiles"),
            "candidateBudgetCensoredSessions": censor, "candidateEngineErrors": engines,
            "candidateBlocks": cand_blocks, "sourceBlocks": ref_blocks,
            "blockLogRatio": float(block_log_ratio), "saturated": bool(saturated),
            "tooEasyVsSource": bool(too_easy_win),
            "botZeroWin": bot_zero_win, "sourceHasObservedWin": source_has_observed_win,
            "visualError": float(quality.get("visualError", 10.0)),
            "puzzleError": float(quality.get("puzzleError", 10.0)),
            "difficultyProxyError": float(quality.get("difficultyProxyError", 10.0)),
            "traceShapeError": float(quality.get("traceShapeError", 0.0)),
            "staticQualityScore": float(quality.get("staticQualityScore", 10.0)),
            "qualityStatus": quality.get("status", "UNKNOWN"),
            "reviewStatus": review_status}


def collect(runs: Path, positions: set[int], limit: int,
            include_all: bool = False, round_dir: Path | None = None):
    rows = []
    manifests = ([round_dir.resolve() / "round_manifest.json"] if round_dir else
                 sorted(runs.glob("ordinary_auto_round_*/round_manifest.json")))
    for manifest_path in manifests:
        try:
            manifest = read(manifest_path)
            batch_path = Path(manifest["batch"])
            bot_dir = Path(manifest["botOutput"])
            request = read(bot_dir / "request.json")
            summary = read(bot_dir / "summary/summary.json")
            batch = read(batch_path)
            source = read(Path(request["sourcePackPath"]))
            templates = {t['id']: t for t in read(Path(request['templatesPath']))['templates']}
            trace_by_id = {}
            trace_path = batch_path.parent.parent / "data/source_turn_signatures_v1.json"
            if trace_path.is_file():
                trace_by_id = {str(row.get("sourceId")): row for row in
                               read(trace_path).get("traces", [])
                               if row.get("verified") and row.get("sourceId") is not None}
            source_by_id = {int(level["id"]): level for level in source["levels"]}
            by_candidate = {int(level["id"]): level for level in batch.get("levels", [])}
            for pair in request.get("pairs", []):
                candidate_id = int(pair["candidateId"])
                candidate = by_candidate.get(candidate_id)
                reference = source_by_id.get(int(pair["referenceId"]))
                if candidate is None or reference is None:
                    continue
                position = int(candidate.get("position", -1))
                if position not in positions:
                    continue
                cand_row = row_by_id(summary, candidate_id, "candidate")
                ref_row = row_by_id(summary, int(pair["referenceId"]), "reference")
                metrics = describe_candidate(candidate, reference, cand_row, ref_row,
                                             trace_by_id.get(str(reference.get("id"))))
                if is_native_macro(candidate):
                    metrics['cohesiveDesign'] = CohesiveDesignPolicy(reference, templates).audit(candidate)
                    if not metrics['cohesiveDesign']['passed']:
                        metrics['reviewStatus'] = 'COHESIVE_DESIGN_REJECTED'
                rows.append({"position": position, "candidate": candidate,
                             "sourceId": int(reference["id"]), "candidateId": candidate_id,
                             "roundManifest": str(manifest_path), "metrics": metrics})
        except (OSError, KeyError, TypeError, ValueError, json.JSONDecodeError):
            # A stopped/incomplete round is simply unavailable to the selector.
            continue
    if include_all:
        # Training Preview mode: expose every bot-evaluated candidate from the
        # requested round so the developer can inspect failures and near-misses
        # manually. This intentionally bypasses the top-per-position gate.
        return sorted(rows, key=lambda item: (item["position"], item["candidateId"])), rows, {}
    # Keep the best candidate per progression position. Prefer any candidate
    # that demonstrates at least one bot win when the source itself is known to
    # be playable. A zero-win candidate in that case is retained only as a
    # blocked diagnostic and never staged into the Unity pack.
    winners = {}
    blocked = {}
    grouped = {}
    for row in rows:
        grouped.setdefault(row["position"], []).append(row)
    for position, candidates in grouped.items():
        design_rejected = [item for item in candidates
                           if item['metrics']['reviewStatus'] == 'COHESIVE_DESIGN_REJECTED']
        candidates = [item for item in candidates
                      if item['metrics']['reviewStatus'] != 'COHESIVE_DESIGN_REJECTED']
        if not candidates:
            blocked[position] = {'reason': 'COHESIVE_DESIGN_REJECTED', 'candidates': design_rejected}
            continue
        candidates.sort(key=lambda item: (item["metrics"]["score"], item["candidateId"]))
        source_playable = any(item["metrics"]["sourceHasObservedWin"] for item in candidates)
        eligible = [item for item in candidates
                    if item["metrics"]["candidateWinRate"] > 0.0
                    and item["metrics"]["reviewStatus"] == "OK"]
        mismatch = [item for item in candidates
                    if item["metrics"]["reviewStatus"] == "COMPOSITION_MISMATCH_REVIEW_REQUIRED"]
        if mismatch and not eligible:
            blocked[position] = {"reason": "COMPOSITION_MISMATCH_REVIEW_REQUIRED",
                                 "candidates": candidates}
            continue
        if source_playable and not eligible:
            blocked[position] = {"reason": "BOT_ZERO_WIN_SOURCE_PLAYABLE",
                                 "candidates": candidates}
            continue
        winners[position] = (eligible or candidates)[0]
    return [winners[p] for p in sorted(winners)][:limit], rows, blocked


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--runs", type=Path, default=DEFAULT_RUNS)
    parser.add_argument("--positions", nargs="+", type=int, default=list(range(1, 31)))
    parser.add_argument("--limit", type=int, default=30)
    parser.add_argument("--output", type=Path)
    parser.add_argument("--set-current", action="store_true",
                        help="Point ordinary_v4/preview/current_preview.json at the new pack")
    parser.add_argument("--all-candidates", action="store_true",
                        help="Stage every bot-evaluated candidate instead of one winner per position")
    parser.add_argument("--round-dir", type=Path,
                        help="Read candidates only from this completed round directory")
    args = parser.parse_args()
    runs = args.runs.resolve()
    selected, all_rows, blocked = collect(runs, set(args.positions), args.limit,
                                          include_all=args.all_candidates,
                                          round_dir=args.round_dir)
    if not selected:
        raise SystemExit("No complete bot-evaluated candidates found for the requested positions.")
    source_templates = None
    for row in selected:
        request = read(Path(read(Path(row["roundManifest"]))["botOutput"]) / "request.json")
        candidate_templates = Path(request["templatesPath"])
        if candidate_templates.is_file():
            source_templates = candidate_templates
            break
    if source_templates is None:
        raise SystemExit("No source template catalog found in selected runs.")
    stamp = dt.datetime.now().strftime("%Y%m%d_%H%M%S")
    prefix = "auto_all_candidates" if args.all_candidates else "auto_top30"
    output = (args.output or (runs / "ordinary_v4/preview" / f"{prefix}_{stamp}")).resolve()
    output.mkdir(parents=True, exist_ok=False)
    template_catalog = read(source_templates)
    templates_by_id = {str(item["id"]): item for item in template_catalog.get("templates", []) if item}
    source_levels = []
    levels = []
    full_source_pack_path = None
    full_source_templates_path = None
    used_ids: set[int] = set()
    for row in selected:
        request = read(Path(read(Path(row["roundManifest"]))["botOutput"]) / "request.json")
        full_source_pack_path = full_source_pack_path or str(Path(request["sourcePackPath"]).resolve())
        full_source_templates_path = full_source_templates_path or str(Path(request["templatesPath"]).resolve())
        source = read(Path(request["sourcePackPath"]))
        source_level = next(item for item in source["levels"] if int(item["id"]) == row["sourceId"])
        level = json.loads(json.dumps(row["candidate"]))
        # Each imported native motif carries its own correct guide relation.
        # Cycling offsets from an unrelated progression source rotates audited
        # physical colliders after the bot run and invalidates that evidence.
        orientation_adjustments = ([] if is_native_macro(level) else
                                   align_preview_guide_orientations(level, source_level, templates_by_id))
        # Rounds from different GUI sessions can reuse proposal IDs; the
        # preview needs unique level IDs, so renumber only on collision.
        if int(level["id"]) in used_ids:
            level["previewOriginalCandidateId"] = int(level["id"])
            level["id"] = 9_900_000 + row["position"] * 100
            while level["id"] in used_ids:
                level["id"] += 1
        used_ids.add(int(level["id"]))
        level["referenceId"] = str(row["sourceId"])
        level["validationStatus"] = "MANUAL_REVIEW_PENDING"
        level["previewSelection"] = {"position": row["position"], "score": row["metrics"]["score"],
                                       "sourceId": row["sourceId"], "candidateId": row["candidateId"],
                                       "guideOrientationAdjustments": orientation_adjustments}
        levels.append(level)
        source_level = json.loads(json.dumps(source_level))
        source_level["sourceOriginal"] = False
        source_level["sourceOnlyParityFixture"] = False
        source_level["sourceReference"] = True
        source_level["title"] = f"Original reference — progression {row['position']}"
        source_levels.append(source_level)
    pack = {"schemaVersion": 3, "complete": True, "sourceOriginalOnly": False, "levels": levels}
    (output / "manual_test_pack.json").write_text(json.dumps(pack, indent=2), encoding="utf-8")
    used_template_ids = {str(part.get("templateId")) for level in levels for part in level.get("parts", []) if part.get("templateId")}
    compact_templates = {"schemaVersion": 3,
                         "templates": [item for item in template_catalog.get("templates", [])
                                       if str(item.get("id")) in used_template_ids]}
    (output / "source_templates.json").write_text(json.dumps(compact_templates, separators=(",", ":")), encoding="utf-8")
    source_pack = {"schemaVersion": 3, "complete": True, "sourceOriginalOnly": False, "levels": source_levels}
    (output / "source_reference_pack.json").write_text(json.dumps(source_pack, indent=2), encoding="utf-8")
    report = {"schemaVersion": 1, "status": "MANUAL_REVIEW_PENDING", "selectedCount": len(levels),
              "requestedPositions": sorted(args.positions), "selected": selected,
              "availableCandidateCount": len(all_rows),
              "blockedPositions": {str(position): {"reason": data["reason"],
                                                     "candidateIds": [int(row["candidateId"]) for row in data["candidates"]],
                                                     "metrics": [row["metrics"] for row in data["candidates"]]}
                                   for position, data in sorted(blocked.items())},
              "limits": ["Bot similarity is a proxy, not human difficulty.",
                         "Manual Unity review remains required; no acceptance is recorded."]}
    (output / "selection_report.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    manifest = {"schemaVersion": 1, "revision": f"auto-top30-{stamp}",
                "packPath": "manual_test_pack.json", "sourcePackPath": full_source_pack_path or "source_reference_pack.json",
                "templatesPath": "source_templates.json", "sourceTemplatesPath": full_source_templates_path or str(source_templates)}
    (output / "preview_manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    current = None
    if args.set_current:
        # The Unity Training Preview window loads this file by default.
        current = runs / "ordinary_v4/preview/current_preview.json"
        current.write_text(json.dumps({"schemaVersion": 1, "revision": f"auto-top30-{stamp}-current",
                                       "packPath": str(output / "manual_test_pack.json"),
                                       "sourcePackPath": full_source_pack_path or str(output / "source_reference_pack.json"),
                                       "templatesPath": str(output / "source_templates.json"),
                                       "sourceTemplatesPath": full_source_templates_path or str(source_templates)}, indent=2),
                           encoding="utf-8")
    print("PREVIEW_PACK " + str(output / "manual_test_pack.json"), flush=True)
    print("PREVIEW_MANIFEST " + str(current or (output / "preview_manifest.json")), flush=True)
    print(json.dumps({"output": str(output), "selectedCount": len(levels),
                      "positions": [row["position"] for row in selected],
                      "blockedPositions": sorted(int(p) for p in blocked)}, indent=2))


if __name__ == "__main__":
    main()
