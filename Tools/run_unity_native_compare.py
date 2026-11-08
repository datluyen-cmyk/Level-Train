"""Run a rapid candidate/source comparison through Unity MCP.

This is the small diagnostic pass for training rounds: it uses the real
SourceSolverV3 Physics2D fixture, records action traces and blocker owners, and
never labels a timeout as unsolved.  The full virtual-user suite remains the
calibration pass for a completed round.
"""
from __future__ import annotations

import argparse
import json
import sys
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(Path(__file__).resolve().parent))
from run_native_virtual_playtest_v5 import execute  # noqa: E402
from unity_mcp import McpClient, DEFAULT_URL  # noqa: E402


def run_batch(client, request: Path, report: Path, max_states: int, budget_ms: int):
    payload = json.loads(request.read_text(encoding="utf-8-sig"))
    payload["maxStates"] = max_states
    payload["wallTimeBudgetMs"] = budget_ms
    request.write_text(json.dumps(payload, indent=2), encoding="utf-8")
    execute(client, "return SourceSolverV3.StartBatch(" + json.dumps(str(request.resolve())) + "," +
            json.dumps(str(report.resolve())) + ");")
    while True:
        status = execute(client,
            'var tick=typeof(SourceSolverV3).GetMethod("TickAsyncBatch",'
            'System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);'
            'int n=0; while(n<20 && SourceSolverV3.GetBatchStatus().IndexOf("idle",'
            'StringComparison.OrdinalIgnoreCase)<0){tick.Invoke(null,null);n++;}'
            'return SourceSolverV3.GetBatchStatus();')
        if '"status":"idle"' in status:
            return
        time.sleep(0.05)


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--candidate-pack", type=Path, required=True)
    p.add_argument("--source-pack", type=Path, required=True)
    p.add_argument("--templates", type=Path, required=True)
    p.add_argument("--positions", nargs="+", type=int, required=True)
    p.add_argument("--output-dir", type=Path, required=True)
    p.add_argument("--max-states", type=int, default=256)
    p.add_argument("--budget-ms", type=int, default=1000)
    args = p.parse_args()
    args.output_dir.mkdir(parents=True, exist_ok=False)
    candidate = json.loads(args.candidate_pack.read_text(encoding="utf-8-sig"))
    source = json.loads(args.source_pack.read_text(encoding="utf-8-sig"))
    by_position = lambda pack: {int(level["position"]): level for level in pack.get("levels", [])}
    cp, sp = by_position(candidate), by_position(source)
    positions = [pos for pos in args.positions if pos in cp and pos in sp]
    if not positions:
        raise SystemExit("No requested positions exist in both packs.")
    for label, pack, index in (("candidate", candidate, cp), ("source", source, sp)):
        levels = [index[pos] for pos in positions]
        pack_path = args.output_dir / f"{label}_pack.json"
        pack_path.write_text(json.dumps({"schemaVersion": 3, "complete": True,
                                         "sourceOriginalOnly": False, "levels": levels},
                                        ensure_ascii=False), encoding="utf-8")
        request = {"schemaVersion": 3, "sourcePackPath": str(pack_path.resolve()),
                   "templatesPath": str(args.templates.resolve()),
                   "sourceIds": [int(level["id"]) for level in levels],
                   "maxStates": args.max_states, "wallTimeBudgetMs": args.budget_ms}
        (args.output_dir / f"{label}_request.json").write_text(json.dumps(request, indent=2), encoding="utf-8")
    client = McpClient(DEFAULT_URL)
    run_batch(client, args.output_dir / "candidate_request.json",
              args.output_dir / "candidate_report.json", args.max_states, args.budget_ms)
    run_batch(client, args.output_dir / "source_request.json",
              args.output_dir / "source_report.json", args.max_states, args.budget_ms)
    summary = args.output_dir / "comparison_summary.json"
    import subprocess
    subprocess.run([sys.executable, str(Path(__file__).with_name("summarize_native_compare.py")),
                    "--candidate", str(args.output_dir / "candidate_report.json"),
                    "--source", str(args.output_dir / "source_report.json"),
                    "--output", str(summary)], check=True)
    print(json.dumps({"positions": positions, "output": str(args.output_dir.resolve()),
                      "summary": str(summary.resolve())}, indent=2))


if __name__ == "__main__":
    main()
