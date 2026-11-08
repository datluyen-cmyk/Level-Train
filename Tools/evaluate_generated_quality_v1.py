#!/usr/bin/env python3
"""Evaluate generated proposals against same-position source levels.

Examples:
  python Tools/evaluate_generated_quality_v1.py \
    --candidate-pack batches/proposals.json \
    --source-pack data/source500_native.json \
    --templates data/source_templates.json \
    --output reports/generated_quality.json

``--native-summary`` is optional.  When supplied, its ``byLevel`` rows are
matched by candidate/source IDs and native win/action/block deltas are included.
"""
from __future__ import annotations

import argparse
import json
from pathlib import Path

from quality_evaluator_v1 import compare


def read(path: Path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def rows_by_id(path: Path | None):
    if path is None or not path.is_file():
        return {}
    payload = read(path)
    return {(str(row.get("levelId")), str(row.get("role"))): row
            for row in payload.get("byLevel", [])}


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--candidate-pack", type=Path, required=True)
    parser.add_argument("--source-pack", type=Path, required=True)
    parser.add_argument("--templates", type=Path)
    parser.add_argument("--native-summary", type=Path)
    parser.add_argument("--difficulty-map", type=Path,
                        help="original_default_difficulty_v7.json (optional source band labels)")
    parser.add_argument("--turn-signatures", type=Path,
                        help="source_turn_signatures_v1.json from verified Unity replays")
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    candidate_pack, source_pack = read(args.candidate_pack), read(args.source_pack)
    templates = None
    if args.templates and args.templates.is_file():
        raw = read(args.templates)
        templates = {str(t["id"]): t for t in raw.get("templates", [])}
    sources = {int(level.get("position")): level for level in source_pack.get("levels", [])}
    native = rows_by_id(args.native_summary)
    difficulty_by_id = {}
    if args.difficulty_map and args.difficulty_map.is_file():
        difficulty_by_id = {str(row.get("sourceId")): row.get("defaultConfigDifficulty")
                             for row in read(args.difficulty_map).get("levels", [])}
    turn_by_id = {}
    if args.turn_signatures and args.turn_signatures.is_file():
        turn_by_id = {str(row.get("sourceId")): row for row in read(args.turn_signatures).get("traces", [])
                      if row.get("verified") and row.get("sourceId") is not None}
    evaluated = []
    for candidate in candidate_pack.get("levels", []):
        position = int(candidate.get("position", -1))
        reference = sources.get(position)
        if reference is None:
            evaluated.append({"candidateId": candidate.get("id"), "position": position,
                              "status": "SOURCE_POSITION_MISSING"})
            continue
        result = compare(candidate, reference, templates,
                         native.get((str(candidate.get("id")), "candidate")),
                         native.get((str(reference.get("id")), "reference")),
                         turn_by_id.get(str(reference.get("id"))))
        evaluated.append({"candidateId": candidate.get("id"), "sourceId": reference.get("id"),
                          "position": position,
                          "sourceDifficultyBand": difficulty_by_id.get(str(reference.get("id"))),
                          **result})
    scores = [row["qualityScore"] for row in evaluated if "qualityScore" in row]
    report = {
        "schemaVersion": 1,
        "status": "SOURCE_RELATIVE_QUALITY_EVALUATED",
        "contract": {
            "visualLayout": "span/density/orientation/neighborhood/symmetry/guide placement",
            "puzzleArchetype": "part and dependency inventory, guides, locks, gaps, pins and static interlock",
            "difficulty": "native Unity bot deltas when supplied; static pressure proxy otherwise; source band is copied from the optional difficulty map",
            "scoreMeaning": "zero is closest to the same-position source; this is a ranking signal, not a solvability proof",
        },
        "candidateCount": len(candidate_pack.get("levels", [])),
        "evaluatedCount": len(scores),
        "meanQualityScore": sum(scores) / len(scores) if scores else None,
        "levels": evaluated,
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8")
    print(json.dumps({"output": str(args.output), "candidateCount": report["candidateCount"],
                      "evaluatedCount": report["evaluatedCount"],
                      "meanQualityScore": report["meanQualityScore"]}, indent=2))


if __name__ == "__main__":
    main()
