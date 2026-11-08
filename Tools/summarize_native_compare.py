"""Summarize two Unity SourceSolverV3 reports aligned by progression position.

The solver reports are bounded native Physics2D searches.  ``unknown`` means the
budget expired, so this report never turns it into an unsolved label.  It is
intended to be called after a Unity MCP StartBatch run and gives the training
loop a small, stable quality signal without replaying the full 1000-user pass.
"""
from __future__ import annotations

import argparse
import json
from pathlib import Path


def load(path: Path):
    data = json.loads(path.read_text(encoding="utf-8-sig"))
    by_position = {int(row["sourcePosition"]): row for row in data.get("levels", [])}
    return data, by_position


def solved(row):
    return bool(row and row.get("status") == "solved" and row.get("replayGoal"))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--candidate", type=Path, required=True)
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    candidate, crows = load(args.candidate)
    source, srows = load(args.source)
    positions = sorted(set(crows) | set(srows))
    rows = []
    for position in positions:
        c, s = crows.get(position), srows.get(position)
        csolved, ssolved = solved(c), solved(s)
        cparts = c.get("partCount") if c else None
        sparts = s.get("partCount") if s else None
        rows.append({
            "position": position,
            "candidateId": c.get("sourceId") if c else None,
            "sourceId": s.get("sourceId") if s else None,
            "candidateStatus": c.get("status") if c else "missing",
            "sourceStatus": s.get("status") if s else "missing",
            "candidateSolved": csolved,
            "sourceSolved": ssolved,
            "candidateMoves": c.get("moveCount", -1) if c else -1,
            "sourceMoves": s.get("moveCount", -1) if s else -1,
            "candidateParts": cparts,
            "sourceParts": sparts,
            "candidateRemovedBest": c.get("maximumRemovedParts", 0) if c else 0,
            "sourceRemovedBest": s.get("maximumRemovedParts", 0) if s else 0,
            "candidateActionTrace": c.get("actionTrace", []) if c else [],
            "sourceActionTrace": s.get("actionTrace", []) if s else [],
        })
    total = len(rows)
    summary = {
        "schemaVersion": 1,
        "method": "Unity MCP SourceSolverV3 bounded native Physics2D search; unknown is inconclusive",
        "candidateReport": str(args.candidate.resolve()),
        "sourceReport": str(args.source.resolve()),
        "candidate": {
            "total": total,
            "solved": sum(r["candidateSolved"] for r in rows),
            "unknown": sum(r["candidateStatus"] == "unknown" for r in rows),
        },
        "source": {
            "total": total,
            "solved": sum(r["sourceSolved"] for r in rows),
            "unknown": sum(r["sourceStatus"] == "unknown" for r in rows),
        },
        "rows": rows,
    }
    summary["candidate"]["boundedSolveRate"] = summary["candidate"]["solved"] / total if total else 0.0
    summary["source"]["boundedSolveRate"] = summary["source"]["solved"] / total if total else 0.0
    summary["candidate"]["partRatioVsSource"] = sum(
        r["candidateParts"] / r["sourceParts"] for r in rows
        if r["candidateParts"] is not None and r["sourceParts"]
    ) / max(1, sum(1 for r in rows if r["candidateParts"] is not None and r["sourceParts"]))
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8")
    print(json.dumps({k: summary[k] for k in ("candidate", "source")}, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
