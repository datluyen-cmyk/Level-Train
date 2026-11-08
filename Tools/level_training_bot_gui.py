"""Desktop controller for repeated statistical training + native bot evaluation.

The controller deliberately runs one phase at a time.  It never calls an LLM and
never writes the Unity Resources pack.  Each round gets an isolated run directory;
immutable source/tool files are hard-linked when possible, while model outputs and
bot evidence are written normally.
"""
from __future__ import annotations

import argparse
import datetime as dt
import json
import math
import os
from pathlib import Path
import queue
import shutil
import subprocess
import sys
import threading
import time
import tkinter as tk
from tkinter import messagebox, ttk


ROOT = Path(__file__).resolve().parents[1]
DEFAULT_WORKSPACE = ROOT / "RotateRings Test/LevelTraining_SourceLayoutV3/runs/ordinary_v4"
# Candidate tasks that lose this many sessions in a row stop early; 75% of bot
# time was spent on <=4% win-rate levels where more sessions add no signal.
ZERO_WIN_STOP_AFTER = 10
TRAIN_STAGES = (
    "refit_ordinary_models_v4.py",
    "organization_model_v5.py",
    "organization_sharp_v5.py",
    "organization_conditioned_v5.py",
    "train_macro_pattern_v5.py",
    "joint_native_motif_v5.py",
    "joint_hyperedge_grammar_v8.py",
    "interaction_critic_v5.py",
    "semantic_macro_assembly_v3.py",
    "macro_pattern_v3.py",
    "composition_field_v3.py",
)


class Tooltip:
    """Small hover help bubble for the compact training controls."""
    def __init__(self, widget, text: str):
        self.widget = widget
        self.text = text
        self.tip = None
        widget.bind("<Enter>", self.show, add="+")
        widget.bind("<Leave>", self.hide, add="+")

    def show(self, _event=None):
        if self.tip is not None:
            return
        self.tip = tk.Toplevel(self.widget)
        self.tip.wm_overrideredirect(True)
        self.tip.attributes("-topmost", True)
        label = tk.Label(self.tip, text=self.text, justify="left", wraplength=430,
                         bg="#fff8d8", fg="#202020", relief="solid", borderwidth=1,
                         padx=8, pady=6)
        label.pack()
        x = self.widget.winfo_rootx() + self.widget.winfo_width() + 6
        y = self.widget.winfo_rooty()
        self.tip.geometry(f"+{x}+{y}")

    def hide(self, _event=None):
        if self.tip is not None:
            self.tip.destroy()
            self.tip = None


def link_tree(source: Path, target: Path) -> None:
    """Materialize a read-only training input tree without duplicating large files."""
    source, target = source.resolve(), target.resolve()
    if not source.is_dir():
        raise FileNotFoundError(source)
    for path in source.rglob("*"):
        relative = path.relative_to(source)
        destination = target / relative
        if path.is_dir():
            destination.mkdir(parents=True, exist_ok=True)
            continue
        destination.parent.mkdir(parents=True, exist_ok=True)
        try:
            os.link(path, destination)
        except OSError:
            shutil.copy2(path, destination)


def block_log_ratio(row: dict) -> float:
    """log(candidate/source physical blocks per session); rows without session
    counts (written before early stopping existed) used 30 sessions on both sides."""
    cs = max(1, int(row.get("candidateSessions") or 30))
    rs = max(1, int(row.get("referenceSessions") or 30))
    return math.log((float(row.get("candidatePhysicalBlocks", 0)) / cs + 0.5) /
                    (float(row.get("referencePhysicalBlocks", 0)) / rs + 0.5))


def parse_positions(value: str) -> list[int]:
    positions = [int(token) for token in value.replace(",", " ").split()]
    if not positions:
        raise ValueError("At least one progression position is required")
    if any(position < 0 for position in positions):
        raise ValueError("Positions must be non-negative")
    return positions


def parse_level_range(first: str, last: str) -> list[int]:
    start, end = int(first), int(last)
    if start < 0 or end < 0:
        raise ValueError("Level positions must be non-negative")
    if end < start:
        raise ValueError("To level must be greater than or equal to From level")
    return list(range(start, end + 1))


class RoundController:
    def __init__(self, workspace: Path, rounds: int, positions: list[int], proposals: int,
                 users: int, seed: int, difficulty: str, policy: str, step_limit: int,
                 train_models: bool, online_feedback: bool, output_root: Path, log,
                 refit_each_round: bool = False):
        self.workspace = workspace.resolve()
        self.rounds = rounds
        self.positions = positions
        self.proposals = proposals
        self.users = users
        self.seed = seed
        self.difficulty = difficulty
        self.policy = policy
        self.step_limit = step_limit
        self.train_models = train_models
        self.refit_each_round = refit_each_round
        self.online_feedback = online_feedback
        self.output_root = output_root.resolve()
        self.log = log
        self.stop_requested = threading.Event()
        self.pause_requested = threading.Event()
        self.process = None
        self.processes: list = []
        self.last_preview: dict | None = None
        # Each worker loads the model bundle (~100 MB) and runs shapely audits.
        self.gen_workers = max(1, min(6, (os.cpu_count() or 2) // 2))
        self.phase = "idle"
        self.current_round = 0
        self.model_bundle: Path | None = None

    def find_latest_model_bundle(self) -> Path | None:
        """Reuse the newest complete fitted bundle across GUI restarts."""
        required = (
            "ordinary_refit_manifest_v4.json",
            "organization_conditioned_v5.joblib",
            "joint_hyperedge_grammar_v8.joblib",
            "interaction_critic_v5.joblib",
            "macro_physical_assembly.joblib", "macro_physical_assembly_evaluation.json",
            "macro_physical_pattern.joblib", "macro_physical_pattern_evaluation.json",
            "whole_board_field.joblib", "whole_board_field_evaluation.json",
        )
        candidates = []
        for model_dir in self.output_root.glob("ordinary_auto_round_*/models"):
            if all((model_dir / name).is_file() for name in required):
                candidates.append(model_dir)
        if not candidates:
            return None
        return max(candidates, key=lambda path: (path / "ordinary_refit_manifest_v4.json").stat().st_mtime)

    @property
    def python(self) -> str:
        return sys.executable

    def request_stop(self):
        self.stop_requested.set()
        if self.process and self.phase != "bot":
            self.process.terminate()
        for process in list(self.processes):
            process.terminate()

    def toggle_pause(self) -> bool:
        if self.pause_requested.is_set():
            self.pause_requested.clear()
            return False
        self.pause_requested.set()
        return True

    def wait_if_paused(self):
        while self.pause_requested.is_set() and not self.stop_requested.is_set():
            time.sleep(0.25)

    def emit(self, message: str):
        self.log(message)

    def run_process(self, args: list[str], phase: str, cwd: Path) -> None:
        self.phase = phase
        self.emit("$ " + subprocess.list2cmdline(args))
        environment = dict(os.environ)
        environment.update(PYTHONIOENCODING="utf-8", PYTHONUNBUFFERED="1")
        flags = getattr(subprocess, "CREATE_NO_WINDOW", 0)
        self.process = subprocess.Popen(args, cwd=str(cwd), stdout=subprocess.PIPE,
                                        stderr=subprocess.STDOUT, text=True,
                                        encoding="utf-8", errors="replace", env=environment,
                                        creationflags=flags)
        try:
            assert self.process.stdout is not None
            for line in self.process.stdout:
                self.emit(line.rstrip())
            code = self.process.wait()
        finally:
            self.process = None
        if code != 0:
            raise RuntimeError(f"{phase} failed with exit code {code}")

    def create_round(self, index: int, fit_models: bool) -> Path:
        # refit_ordinary_models_v4 intentionally accepts only dedicated
        # runs/ordinary* destinations, so each round must be a direct child of
        # the runs folder rather than a nested output directory.
        name = f"ordinary_auto_round_{index:03d}_{dt.datetime.now().strftime('%Y%m%d_%H%M%S')}"
        round_dir = self.output_root / name
        if round_dir.exists():
            round_dir = self.output_root / f"{name}_{time.time_ns()}"
        round_dir.mkdir(parents=True)
        link_tree(self.workspace / "data", round_dir / "data")
        link_tree(self.workspace / "tools", round_dir / "tools")
        (round_dir / "models").mkdir()
        family_splits = self.workspace / "models/family_splits.json"
        if not family_splits.is_file():
            raise FileNotFoundError(family_splits)
        if fit_models:
            try:
                os.link(family_splits, round_dir / "models/family_splits.json")
            except OSError:
                shutil.copy2(family_splits, round_dir / "models/family_splits.json")
        elif self.model_bundle is not None:
            # Reuse a verified model bundle when the immutable source corpus has
            # not changed. Re-fitting identical data only repeats the same fit
            # and was the largest avoidable local time sink.
            link_tree(self.model_bundle, round_dir / "models")
        else:
            # Quick generation-only smoke mode reuses the immutable model bundle,
            # including its ordinary-refit certificate, without mutating it.
            link_tree(self.workspace / "models", round_dir / "models")
        # V10's macro composer must use the model trained on this exact round
        # corpus/template catalog. Keep it available even when the compact
        # refit bundle above intentionally links only its four required files.
        for artifact in ("macro_assembly.joblib", "macro_assembly_evaluation.json",
                         "macro_physical_assembly.joblib", "macro_physical_assembly_evaluation.json",
                         "macro_physical_pattern.joblib", "macro_physical_pattern_evaluation.json",
                         "whole_board_field.joblib", "whole_board_field_evaluation.json"):
            source_artifact = self.workspace / "models" / artifact
            destination_artifact = round_dir / "models" / artifact
            # A fit must start with fresh output files. Linking a writable
            # joblib destination would overwrite the shared base model, and
            # refit_ordinary_models_v4 explicitly refuses existing artifacts.
            if not fit_models and source_artifact.is_file() and not destination_artifact.exists():
                try:
                    os.link(source_artifact, destination_artifact)
                except OSError:
                    shutil.copy2(source_artifact, destination_artifact)
        (round_dir / "reports").mkdir()
        difficulty = self.workspace / "reports/original_default_difficulty_v7.json"
        if difficulty.is_file():
            try:
                os.link(difficulty, round_dir / "reports/original_default_difficulty_v7.json")
            except OSError:
                shutil.copy2(difficulty, round_dir / "reports/original_default_difficulty_v7.json")
        return round_dir

    def train_round(self, round_dir: Path, fit_models: bool):
        if not fit_models:
            self.emit("ML refit disabled; using the round's existing model artifacts only.")
            return
        for filename in TRAIN_STAGES:
            self.wait_if_paused()
            if self.stop_requested.is_set():
                return
            script = round_dir / "tools" / filename
            if not script.is_file():
                raise FileNotFoundError(script)
            command = [self.python, str(script), "--workspace", str(round_dir)]
            if filename == "joint_hyperedge_grammar_v8.py":
                command.append("--fit")
            if filename == "macro_pattern_v3.py":
                command.extend(["--macro-model-name", "macro_physical_assembly.joblib",
                                "--output-name", "macro_physical_pattern.joblib",
                                "--report-name", "macro_physical_pattern_evaluation.json"])
            self.run_process(command, "train:" + filename, round_dir)

    def generate_round(self, round_dir: Path, index: int) -> tuple[Path, dict]:
        batch_dir = round_dir / "batches"
        batch_dir.mkdir()
        batch_path = batch_dir / f"proposals_round_{index:03d}.json"
        # V10 composes several source-native motifs with learned inter-motif
        # poses and whole-board style. V9 is retained in the run tree for
        # comparison, but is no longer used because a complete donor graph is
        # a near-copy even when its board is rotated.
        tool = round_dir / "tools/relational_level_generator_v10.py"
        # Unique across GUI sessions: rounds restart at 1 every launch, so
        # number by the count of round folders already on disk.
        global_round = len(list(self.output_root.glob("ordinary_auto_round_*")))
        proposal_id = 9800000 + global_round * 1000
        source = json.loads((round_dir / "data/source500_native.json").read_text(encoding="utf-8"))
        source_positions = {int(level["position"]) for level in source.get("levels", [])}
        generation_positions = [position for position in self.positions if position in source_positions]
        skipped = [position for position in self.positions if position not in source_positions]
        if not generation_positions:
            raise ValueError(f"No requested positions exist in ordinary corpus: {skipped}")
        if skipped:
            self.emit("Skipping positions absent from ordinary corpus: " + ", ".join(map(str, skipped)))
        # Proposals = positions x N gives N candidates per position (best-of-N);
        # the generator cycles positions, and the preview selector keeps the best.
        per_position = max(1, math.ceil(self.proposals / max(1, len(self.positions))))
        # A broad progression request is a discovery pass. Running the
        # maximum structural search budget for every position makes seam audit
        # cost grow superlinearly. Keep the deeper budget for small targeted
        # batches; native bot playtest then spends the saved time on actual
        # candidate quality comparison.
        if len(generation_positions) > 10:
            seed_options, graft_tries, max_steps = 8, 16, 16
            self.emit("Broad-range generation: using balanced structural search budget (8/16/16).")
        else:
            seed_options, graft_tries, max_steps = 16, 32, 20
        # Positions are independent, so split them over worker processes. Cost
        # grows with board size, so balance chunks by source part count.
        sizes = {int(level["position"]): len(level.get("parts", [])) for level in source.get("levels", [])}
        workers = max(1, min(self.gen_workers, len(generation_positions)))
        chunks = [[] for _ in range(workers)]
        loads = [0.0] * workers
        for position in sorted(generation_positions, key=lambda p: -sizes.get(p, 1)):
            k = loads.index(min(loads))
            chunks[k].append(position)
            loads[k] += max(1, sizes.get(position, 1)) ** 1.5 * per_position
        chunks = [sorted(c) for c in chunks if c]
        self.emit(f"Generation: {len(chunks)} parallel worker(s), {per_position} candidate(s) per position.")
        commands, outputs, offset = [], [], 0
        for k, chunk in enumerate(chunks):
            out = batch_dir / f"proposals_round_{index:03d}_part{k:02d}.json"
            count = len(chunk) * per_position
            commands.append([self.python, str(tool), "--workspace", str(round_dir),
                             "--positions", *map(str, chunk), "--count", str(count),
                             "--seed", str(self.seed + index * 101 + k), "--id-base", str(proposal_id + offset),
                             "--seed-options", str(seed_options), "--graft-tries", str(graft_tries),
                             "--max-steps", str(max_steps),
                             "--difficulty-target", self.difficulty, "--output", str(out)])
            outputs.append(out)
            offset += count
        if offset >= 1000:
            raise ValueError("More than 999 proposals per round would collide with the next round's IDs")
        self.run_processes_parallel(commands, "generate", round_dir)
        parts = [json.loads(path.read_text(encoding="utf-8")) for path in outputs]
        payload = dict(parts[0])
        payload["levels"] = [level for part in parts for level in part.get("levels", [])]
        payload["reports"] = [report for part in parts for report in part.get("reports", [])]
        payload["requestedCount"] = sum(int(part.get("requestedCount", 0)) for part in parts)
        payload["requestedPositions"] = generation_positions
        payload["parallelParts"] = [str(path) for path in outputs]
        payload["candidatesPerPosition"] = per_position
        batch_path.write_text(json.dumps(payload, separators=(",", ":")), encoding="utf-8")
        if not payload.get("levels"):
            raise RuntimeError("Không có màn vượt kiểm tra cấu trúc/bố cục/chống sao chép. "
                               "Xem reports trong file proposals; chưa thay bộ test Unity.")
        return batch_path, payload

    def run_processes_parallel(self, commands: list[list[str]], phase: str, cwd: Path) -> None:
        if len(commands) == 1:
            self.run_process(commands[0], phase, cwd)
            return
        self.phase = phase
        environment = dict(os.environ)
        environment.update(PYTHONIOENCODING="utf-8", PYTHONUNBUFFERED="1")
        flags = getattr(subprocess, "CREATE_NO_WINDOW", 0)
        processes = []
        for args in commands:
            self.emit("$ " + subprocess.list2cmdline(args))
            processes.append(subprocess.Popen(args, cwd=str(cwd), stdout=subprocess.PIPE,
                                              stderr=subprocess.STDOUT, text=True, encoding="utf-8",
                                              errors="replace", env=environment, creationflags=flags))
        self.processes = processes

        def pump(k, process):
            for line in process.stdout:
                # Per-position JSON reports are long; keep the log readable.
                if len(line) < 400:
                    self.emit(f"[gen{k}] " + line.rstrip())
        threads = [threading.Thread(target=pump, args=(k, p), daemon=True) for k, p in enumerate(processes)]
        for thread in threads:
            thread.start()
        try:
            codes = [process.wait() for process in processes]
            for thread in threads:
                thread.join(timeout=5)
        finally:
            self.processes = []
        if any(code != 0 for code in codes):
            raise RuntimeError(f"{phase} failed with exit codes {codes}")

    def build_bot_request(self, round_dir: Path, index: int, batch_path: Path, payload: dict) -> Path:
        source_pack = round_dir / "data/source500_native.json"
        templates = round_dir / "data/source_templates.json"
        source = json.loads(source_pack.read_text(encoding="utf-8"))
        by_position = {int(level["position"]): level for level in source["levels"]}
        candidates = payload["levels"]
        candidate_pack = round_dir / "batches" / f"candidate_pack_round_{index:03d}.json"
        candidate_pack.write_text(json.dumps({"schemaVersion": 3, "complete": True,
                                               "sourceOriginalOnly": False, "levels": candidates}),
                                  encoding="utf-8")
        pairs = []
        for candidate in candidates:
            position = int(candidate["position"])
            reference = by_position.get(position)
            if reference is None:
                raise ValueError(f"No ordinary source at generated position {position}")
            pairs.append({"referenceId": int(reference["id"]), "candidateId": int(candidate["id"]),
                          "referencePairId": f"round{index:03d}-position{position}-candidate{candidate['id']}"})
        # A fixed bot seed keeps reference (source) episodes identical across
        # rounds, so the runner can reuse them from its cache instead of
        # replaying the same originals every round (56% of bot time before).
        request = {"schemaVersion": 5, "users": self.users, "seed": self.seed,
                   "zeroWinStopAfter": ZERO_WIN_STOP_AFTER if self.users > ZERO_WIN_STOP_AFTER else 0,
                   "stepLimit": self.step_limit, "wallTimeLimitMs": 10000,
                   "transitionCacheLimit": 50000, "phase": f"auto-round-{index:03d}",
                   "sourcePackPath": str(source_pack), "candidatePackPath": str(candidate_pack),
                   "templatesPath": str(templates), "pairs": pairs, "policyVariant": self.policy}
        request_path = round_dir / "batches" / f"bot_request_round_{index:03d}.json"
        request_path.write_text(json.dumps(request, indent=2), encoding="utf-8")
        return request_path

    def bot_round(self, round_dir: Path, index: int, request_path: Path) -> Path:
        output_dir = round_dir / "batches" / f"bot_round_{index:03d}"
        runner = ROOT / "Tools/run_native_virtual_playtest_v5.py"
        self.run_process([self.python, str(runner), "--request", str(request_path),
                          "--output-dir", str(output_dir)], "bot", ROOT)
        return output_dir

    def evaluate_generated_quality(self, round_dir: Path, index: int, payload: dict,
                                   summary: dict | None = None) -> dict:
        """Write the unified visual/mechanic/native quality report for a round.

        This is deliberately a cheap post-generation pass.  It does not rerun
        Unity: the native rows are already available in ``summary`` and the
        static evaluator compares every proposal with its same-position source.
        The same evaluator is also called inside the generator reranker, so the
        report is an auditable copy of the signal that affected selection.
        """
        try:
            sys.path.insert(0, str(ROOT / "Tools"))
            from quality_evaluator_v1 import compare
            source = json.loads((round_dir / "data/source500_native.json").read_text(encoding="utf-8"))
            templates_raw = json.loads((round_dir / "data/source_templates.json").read_text(encoding="utf-8"))
            templates = {str(item["id"]): item for item in templates_raw.get("templates", [])}
            difficulty_path = round_dir / "reports/original_default_difficulty_v7.json"
            difficulty_by_id = {}
            if difficulty_path.is_file():
                difficulty_by_id = {str(item.get("sourceId")): item.get("defaultConfigDifficulty")
                                     for item in json.loads(difficulty_path.read_text(encoding="utf-8")).get("levels", [])}
            source_by_position = {int(level["position"]): level for level in source.get("levels", [])}
            turn_signatures_path = round_dir / "data/source_turn_signatures_v1.json"
            turn_by_id = {}
            if turn_signatures_path.is_file():
                turn_by_id = {str(row.get("sourceId")): row for row in
                              json.loads(turn_signatures_path.read_text(encoding="utf-8-sig")).get("traces", [])
                              if row.get("verified") and row.get("sourceId") is not None}
            native_rows = {(str(row.get("levelId")), str(row.get("role"))): row
                           for row in (summary or {}).get("byLevel", [])}
            rows = []
            for candidate in payload.get("levels", []):
                reference = source_by_position.get(int(candidate.get("position", -1)))
                if reference is None:
                    continue
                result = compare(candidate, reference, templates,
                                 native_rows.get((str(candidate.get("id")), "candidate")),
                                 native_rows.get((str(reference.get("id")), "reference")),
                                 turn_by_id.get(str(reference.get("id"))))
                rows.append({"candidateId": candidate.get("id"), "sourceId": reference.get("id"),
                             "position": candidate.get("position"),
                             "sourceDifficultyBand": difficulty_by_id.get(str(reference.get("id"))), **result})
            scores = [float(row["qualityScore"]) for row in rows]
            report = {"schemaVersion": 1, "status": "SOURCE_RELATIVE_QUALITY_EVALUATED",
                      "candidateCount": len(payload.get("levels", [])), "evaluatedCount": len(rows),
                      "meanQualityScore": sum(scores) / len(scores) if scores else None,
                      "levels": rows}
            path = round_dir / "reports" / f"generated_quality_round_{index:03d}.json"
            path.write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8")
            if scores:
                self.emit("Quality evaluator: " +
                          f"mean={report['meanQualityScore']:.3f}, " +
                          f"visual={sum(r['visualError'] for r in rows)/len(rows):.3f}, " +
                          f"puzzle={sum(r['puzzleError'] for r in rows)/len(rows):.3f}, " +
                          f"difficultyProxy={sum(r['difficultyProxyError'] for r in rows)/len(rows):.3f}")
            return {"status": report["status"], "path": str(path),
                    "evaluatedCount": len(rows), "meanQualityScore": report["meanQualityScore"]}
        except Exception as error:
            self.emit(f"Quality evaluator skipped: {error!r}")
            return {"status": "FAILED", "error": repr(error)}

    def record_online_feedback(self, round_dir: Path, index: int, payload: dict, summary: dict) -> dict:
        """Store paired bot deltas and fit a small ranking critic when support exists.

        This critic is deliberately advisory: it ranks future candidates and does not
        change source data, mark levels accepted, or replace native/visual gates.
        """
        by_level = {(str(row.get("levelId")), row.get("role")): row
                    for row in summary.get("byLevel", [])}
        source_pack = json.loads((round_dir / "data/source500_native.json").read_text(encoding="utf-8"))
        source_by_position = {int(level["position"]): str(level["id"]) for level in source_pack.get("levels", [])}
        rows = []
        for candidate in payload.get("levels", []):
            cid = str(candidate.get("id"))
            position = int(candidate.get("position", -1))
            reference_id = source_by_position.get(position)
            # The summary's byLevel table is keyed by IDs; the source ID is recovered
            # from the request pair when the richer row is unavailable.
            ref_rows = [row for (level_id, role), row in by_level.items() if role == "reference"]
            cand = by_level.get((cid, "candidate"))
            ref = by_level.get((reference_id, "reference")) if reference_id else (ref_rows[0] if len(ref_rows) == 1 else None)
            if cand is None or ref is None:
                continue
            cand_p50 = float((cand.get("actionCountAllSessionQuantiles") or {}).get("p50", 0.0))
            ref_p50 = float((ref.get("actionCountAllSessionQuantiles") or {}).get("p50", 0.0))
            row = {"schemaVersion": 1, "round": index, "candidateId": int(candidate["id"]),
                   "position": position, "difficultyTarget": self.difficulty,
                   "partCount": len(candidate.get("parts", [])),
                   "linkCount": len(candidate.get("links", [])),
                   "guideLinkCount": len(candidate.get("guideLinks", [])),
                   "candidateWinRate": float(cand.get("winRateExcludingEngineErrors") or 0.0),
                   "referenceWinRate": float(ref.get("winRateExcludingEngineErrors") or 0.0),
                   "candidateActionP50": cand_p50, "referenceActionP50": ref_p50,
                   "candidatePhysicalBlocks": int(cand.get("physicalBlocks", 0)),
                   "referencePhysicalBlocks": int(ref.get("physicalBlocks", 0)),
                   "candidateSessions": int(cand.get("sessions", 0)),
                   "referenceSessions": int(ref.get("sessions", 0))}
            row["candidateMinusReferenceWinRate"] = row["candidateWinRate"] - row["referenceWinRate"]
            # Win rate saturates at 0 from mid progression; block ratio does not.
            # Per-session, because early-stopped candidates have fewer sessions.
            row["candidateBlockLogRatio"] = block_log_ratio(row)
            rows.append(row)
        feedback_path = self.output_root / "online_feedback.jsonl"
        if rows:
            with feedback_path.open("a", encoding="utf-8") as stream:
                for row in rows:
                    stream.write(json.dumps(row, ensure_ascii=False) + "\n")
            (round_dir / "online_feedback_rows.json").write_text(json.dumps(rows, indent=2), encoding="utf-8")
        critic_status = self.fit_online_feedback(feedback_path)
        return {"rows": len(rows), "feedbackPath": str(feedback_path), "critic": critic_status}

    def fit_online_feedback(self, feedback_path: Path) -> dict:
        if not feedback_path.is_file():
            return {"status": "NO_ROWS"}
        rows = [json.loads(line) for line in feedback_path.read_text(encoding="utf-8").splitlines() if line.strip()]
        if len(rows) < 8:
            return {"status": "INSUFFICIENT_ROWS", "rows": len(rows), "minimumRows": 8}
        try:
            import joblib
            import numpy as np
            from sklearn.ensemble import ExtraTreesRegressor
            names = ("position", "partCount", "linkCount", "guideLinkCount", "candidateWinRate",
                     "referenceWinRate", "candidateActionP50", "referenceActionP50",
                     "candidatePhysicalBlocks", "referencePhysicalBlocks")
            x = np.asarray([[float(row.get(name, 0.0)) for name in names] for row in rows], dtype=float)
            # Target is the interlock gap (log candidate/source physical blocks);
            # rows written before this field existed are recomputed from raw counts.
            y = np.asarray([block_log_ratio(row) for row in rows], dtype=float)
            model = ExtraTreesRegressor(n_estimators=96, min_samples_leaf=2,
                                        random_state=20261107, n_jobs=1).fit(x, y)
            model_path = self.output_root / "online_difficulty_critic_v1.joblib"
            joblib.dump({"schemaVersion": 1, "featureNames": list(names), "rows": len(rows),
                         "model": model, "meaning": "advisory candidate-minus-reference bot win-rate ranking"},
                        model_path, compress=3)
            return {"status": "UPDATED", "rows": len(rows), "model": str(model_path)}
        except Exception as error:
            return {"status": "FAILED", "rows": len(rows), "error": repr(error)}

    def stage_preview(self, round_dir: Path | None = None):
        """Pick the best candidate per position over all rounds, point the
        Unity Training Preview at it and reload the window. Never fatal."""
        selector = ROOT / "Tools/select_top_preview_levels.py"
        lines: list[str] = []
        original_log = self.log
        try:
            self.log = lambda message: (lines.append(message), original_log(message))
            command = [self.python, str(selector), "--runs", str(self.output_root),
                       "--positions", *map(str, self.positions),
                       "--limit", str(len(self.positions)), "--set-current"]
            if round_dir is not None:
                command += ["--all-candidates", "--round-dir", str(round_dir)]
            self.run_process(command,
                             "stage-preview", ROOT)
        except Exception as error:
            self.emit(f"Preview staging failed: {error!r}")
            return
        finally:
            self.log = original_log
        manifest = next((l.split(" ", 1)[1] for l in lines if l.startswith("PREVIEW_MANIFEST ")), None)
        pack = next((l.split(" ", 1)[1] for l in lines if l.startswith("PREVIEW_PACK ")), None)
        self.last_preview = {"pack": pack, "manifest": manifest}
        self.emit("=" * 60)
        self.emit("Unity manifest (chọn file này trong Training Preview): " + str(manifest))
        self.emit("Level data (chỉ để xem, không chọn file này): " + str(pack))
        try:
            sys.path.insert(0, str(ROOT / "Tools"))
            from unity_mcp import McpClient, DEFAULT_URL
            code = "return TrainingPreviewWindow.OpenAndReload(" + json.dumps(manifest) + ");"
            result = McpClient(DEFAULT_URL).command(
                "call", "execute_code " + json.dumps({"action": "execute", "code": code}))
            value = result.get("structuredContent") or json.loads(
                next(c["text"] for c in result["content"] if c["type"] == "text"))
            if value.get("success"):
                self.emit("Unity Training Preview reloaded: " + str(value["data"]["result"]))
                self.emit("In Unity: press 'Start first / sample' (or pick a level) to play.")
            else:
                self.emit("Unity reload skipped (" + str(value.get("data") or value.get("message")) +
                          "). Open Rotate Rings > Training Preview and press Reload manifest.")
        except Exception as error:
            self.emit(f"Unity reload skipped ({error}). Open Rotate Rings > Training Preview and press Reload manifest.")
        self.emit("=" * 60)

    def run(self):
        self.output_root.mkdir(parents=True, exist_ok=True)
        self.emit(f"Output root: {self.output_root}")
        self.emit(f"Python: {self.python}")
        if not self.train_models:
            self.model_bundle = self.find_latest_model_bundle()
            if self.model_bundle is not None:
                self.emit(f"Reusing latest fitted model bundle: {self.model_bundle}")
        for index in range(1, self.rounds + 1):
            if self.stop_requested.is_set():
                break
            self.current_round = index
            self.wait_if_paused()
            fit_models = self.train_models and (self.model_bundle is None or self.refit_each_round)
            round_dir = self.create_round(index, fit_models)
            self.emit(f"\n=== ROUND {index}/{self.rounds} ===\n{round_dir}")
            self.train_round(round_dir, fit_models)
            if fit_models:
                self.model_bundle = round_dir / "models"
            if self.stop_requested.is_set():
                break
            batch_path, payload = self.generate_round(round_dir, index)
            request_path = self.build_bot_request(round_dir, index, batch_path, payload)
            bot_dir = self.bot_round(round_dir, index, request_path)
            summary = json.loads((bot_dir / "summary/summary.json").read_text(encoding="utf-8"))
            quality = self.evaluate_generated_quality(round_dir, index, payload, summary)
            feedback = self.record_online_feedback(round_dir, index, payload, summary) if self.online_feedback else {"status": "DISABLED"}
            manifest = {"schemaVersion": 1, "round": index, "roundDirectory": str(round_dir),
                        "difficultyTarget": self.difficulty, "requestedUsers": self.users,
                        "proposalCount": len(payload["levels"]), "batch": str(batch_path),
                        "botOutput": str(bot_dir), "botSummary": summary, "qualityEvaluation": quality,
                        "onlineFeedback": feedback,
                        "accepted": False,
                        "limits": ["Bot Monte Carlo is not human difficulty.",
                                    "Visual review and independent novelty gates remain pending."]}
            (round_dir / "round_manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
            self.emit(f"Round {index} complete: {len(payload['levels'])} proposals; bot sessions={summary.get('actualSessions')}; feedback={feedback.get('status', 'DISABLED')}")
            self.stage_preview(round_dir)
        self.phase = "idle"
        if self.stop_requested.is_set():
            self.emit("Stopped after the current safe phase.")
        else:
            self.emit("All requested rounds complete.")


class TrainingBotApp:
    def __init__(self, window: tk.Tk, workspace: Path):
        self.window = window
        self.events = queue.Queue()
        self.worker: threading.Thread | None = None
        self.controller: RoundController | None = None
        self.workspace = tk.StringVar(value=str(workspace))
        self.rounds = tk.StringVar(value="3")
        self.position_from = tk.StringVar(value="1")
        self.position_to = tk.StringVar(value="30")
        self.proposals = tk.StringVar(value="30")
        self.users = tk.StringVar(value="100")
        self.seed = tk.StringVar(value="20261107")
        self.difficulty = tk.StringVar(value="Source")
        self.policy = tk.StringVar(value="cached-visible-v2")
        self.step_limit = tk.StringVar(value="60")
        self.train_models = tk.BooleanVar(value=False)
        self.refit_each_round = tk.BooleanVar(value=False)
        self.online_feedback = tk.BooleanVar(value=True)
        self.auto_seed = tk.BooleanVar(value=True)
        self.status = tk.StringVar(value="Sẵn sàng; chưa chạy round nào.")
        self.build_ui()
        self.window.after(100, self.poll)
        self.window.protocol("WM_DELETE_WINDOW", self.close)

    def build_ui(self):
        self.window.title("Rotate Rings · local ML training + native bot playtest")
        self.window.geometry("1000x760")
        self.window.minsize(880, 650)
        style = ttk.Style(self.window)
        if "clam" in style.theme_names():
            style.theme_use("clam")
        frame = ttk.Frame(self.window, padding=22)
        frame.pack(fill="both", expand=True)
        frame.columnconfigure(1, weight=1)
        ttk.Label(frame, text="Rotate Rings training runner", font=("Segoe UI", 20, "bold")).grid(
            row=0, column=0, columnspan=3, sticky="w")
        ttk.Label(frame, text="ML refit → from-empty proposal → Unity native bot replay; không ghi Resources/generated_levels.json").grid(
            row=1, column=0, columnspan=3, sticky="w", pady=(2, 16))
        helps = {
            "Workspace": "Thư mục run chứa dữ liệu JSON, model và kết quả training. Thường giữ nguyên giá trị mặc định.",
            "From level": "Progression bắt đầu. Ví dụ 100 nghĩa là bắt đầu sinh level tương đương level gốc ở vị trí 100.",
            "To level": "Progression kết thúc, có tính cả vị trí này. Ví dụ From=100, To=200 sẽ xử lý 101 vị trí.",
            "Số round": "Số vòng sinh và playtest. Nhiều round tạo thêm candidate khác nhau; model đã fit sẽ được tái sử dụng nếu chưa có dữ liệu mới.",
            "Proposal/round": "Số candidate sinh trong mỗi round. Khi dùng một đoạn level, nên đặt ít nhất bằng số vị trí để mỗi vị trí có candidate.",
            "Bot users": "Số virtual user native Unity dùng để playtest mỗi cặp source/candidate. 8–20 để lọc nhanh, 100 để calibrate, 1000 để kiểm tra rộng.",
            "Seed": "Seed tái lập ngẫu nhiên. Giữ nguyên để chạy lại cùng một thí nghiệm; đổi seed để sinh biến thể mới.",
            "Step limit": "Số thao tác tối đa cho một bot trong một level. Tăng lên giúp quan sát level khó hơn nhưng làm batch lâu hơn.",
            "Difficulty target": "Source là khuyến nghị: lấy difficulty mặc định của level gốc cùng progression. Normal/Hard/VeryHard là ép cả batch vào một band.",
            "Bot policy": "cached-visible-v2 là policy mặc định, chỉ dùng thông tin hình học quan sát được và cache transition native; legacy dùng đường policy cũ.",
            "Fit ML models": "Fit lại các model thống kê từ toàn bộ corpus source ordinary hiện có. Đây không phải epoch neural; fit lại cùng dữ liệu không tự tạo thêm kiến thức.",
            "Refit every round": "Bật khi đã thêm source/feedback mới. Nếu corpus không đổi, tắt để tránh lặp lại bước fit tốn thời gian.",
            "Online feedback": "Lưu chênh lệch bot giữa candidate và source để xếp hạng tư vấn. Không tự đánh dấu accepted và không ghi vào pack production.",
            "Auto seed": "Tự tạo seed mới mỗi lần bấm Start để lần chạy sau sinh biến thể khác. Tắt nếu cần tái lập đúng một lần chạy cũ.",
        }

        def add_field(row, label, variable):
            label_frame = ttk.Frame(frame)
            label_frame.grid(row=row, column=0, sticky="w", padx=(0, 10), pady=4)
            ttk.Label(label_frame, text=label).pack(side="left")
            help_button = ttk.Label(label_frame, text="?", foreground="#1769aa", cursor="question_arrow")
            help_button.pack(side="left", padx=(5, 0))
            Tooltip(label_frame, helps[label])
            ttk.Entry(frame, textvariable=variable).grid(row=row, column=1, columnspan=2, sticky="ew", pady=4)

        add_field(2, "Workspace", self.workspace)
        add_field(3, "From level", self.position_from)
        add_field(4, "To level", self.position_to)
        add_field(5, "Số round", self.rounds)
        add_field(6, "Proposal/round", self.proposals)
        add_field(7, "Bot users", self.users)
        add_field(8, "Seed", self.seed)
        add_field(9, "Step limit", self.step_limit)
        difficulty_label = ttk.Frame(frame)
        difficulty_label.grid(row=10, column=0, sticky="w", padx=(0, 10), pady=4)
        ttk.Label(difficulty_label, text="Difficulty target").pack(side="left")
        difficulty_help = ttk.Label(difficulty_label, text="?", foreground="#1769aa", cursor="question_arrow")
        difficulty_help.pack(side="left", padx=(5, 0))
        Tooltip(difficulty_label, helps["Difficulty target"])
        ttk.Combobox(frame, textvariable=self.difficulty, values=("Source", "Normal", "Hard", "VeryHard"), state="readonly").grid(
            row=10, column=1, sticky="w", pady=4)
        policy_label = ttk.Frame(frame)
        policy_label.grid(row=11, column=0, sticky="w", padx=(0, 10), pady=4)
        ttk.Label(policy_label, text="Bot policy").pack(side="left")
        policy_help = ttk.Label(policy_label, text="?", foreground="#1769aa", cursor="question_arrow")
        policy_help.pack(side="left", padx=(5, 0))
        Tooltip(policy_label, helps["Bot policy"])
        ttk.Combobox(frame, textvariable=self.policy, values=("cached-visible-v2", "legacy-stochastic-v1"), state="readonly").grid(
            row=11, column=1, sticky="w", pady=4)
        fit_help = ttk.Label(frame, text="?", foreground="#1769aa", cursor="question_arrow")
        fit_help.grid(row=12, column=2, sticky="w", padx=(6, 0), pady=(8, 12))
        Tooltip(fit_help, helps["Fit ML models"])
        fit_check = ttk.Checkbutton(frame, text="Fit ML models before the first round (chỉ cần lần đầu)", variable=self.train_models)
        fit_check.grid(row=12, column=0, columnspan=2, sticky="w", pady=(8, 12))
        Tooltip(fit_check, helps["Fit ML models"])
        refit_help = ttk.Label(frame, text="?", foreground="#1769aa", cursor="question_arrow")
        refit_help.grid(row=13, column=2, sticky="w", padx=(6, 0), pady=(0, 8))
        Tooltip(refit_help, helps["Refit every round"])
        refit_check = ttk.Checkbutton(frame, text="Refit every round (slower; use only after new data)", variable=self.refit_each_round)
        refit_check.grid(row=13, column=0, columnspan=2, sticky="w", pady=(0, 8))
        Tooltip(refit_check, helps["Refit every round"])
        feedback_help = ttk.Label(frame, text="?", foreground="#1769aa", cursor="question_arrow")
        feedback_help.grid(row=14, column=2, sticky="w", padx=(6, 0), pady=(0, 12))
        Tooltip(feedback_help, helps["Online feedback"])
        feedback_check = ttk.Checkbutton(frame, text="Record online bot feedback for advisory ranking", variable=self.online_feedback)
        feedback_check.grid(row=14, column=0, columnspan=2, sticky="w", pady=(0, 12))
        Tooltip(feedback_check, helps["Online feedback"])
        seed_help = ttk.Label(frame, text="?", foreground="#1769aa", cursor="question_arrow")
        seed_help.grid(row=15, column=2, sticky="w", padx=(6, 0), pady=(0, 12))
        Tooltip(seed_help, helps["Auto seed"])
        seed_check = ttk.Checkbutton(frame, text="Tự đổi seed mỗi lần Start", variable=self.auto_seed)
        seed_check.grid(row=15, column=0, columnspan=2, sticky="w", pady=(0, 12))
        Tooltip(seed_check, helps["Auto seed"])
        actions = ttk.Frame(frame)
        actions.grid(row=16, column=0, columnspan=3, sticky="ew")
        self.start_button = ttk.Button(actions, text="Start", command=self.start)
        self.start_button.pack(side="left")
        self.preview_button = ttk.Button(actions, text="Stage top 30 preview", command=self.prepare_preview)
        self.preview_button.pack(side="left", padx=8)
        self.pause_button = ttk.Button(actions, text="Pause after phase", command=self.pause, state="disabled")
        self.pause_button.pack(side="left", padx=8)
        self.stop_button = ttk.Button(actions, text="Stop safely", command=self.stop, state="disabled")
        self.stop_button.pack(side="left")
        ttk.Label(frame, textvariable=self.status, wraplength=900).grid(row=17, column=0, columnspan=3, sticky="w", pady=12)
        self.log_widget = tk.Text(frame, height=25, bg="#101827", fg="#dce8f8", relief="flat", state="disabled", wrap="word")
        self.log_widget.grid(row=18, column=0, columnspan=3, sticky="nsew")
        frame.rowconfigure(18, weight=1)

    def append_log(self, text: str):
        self.log_widget.configure(state="normal")
        self.log_widget.insert("end", text + "\n")
        self.log_widget.see("end")
        self.log_widget.configure(state="disabled")

    def read_int(self, variable: tk.StringVar, label: str, minimum=1) -> int:
        value = int(variable.get())
        if value < minimum:
            raise ValueError(f"{label} must be >= {minimum}")
        return value

    def start(self):
        if self.worker and self.worker.is_alive():
            return
        try:
            workspace = Path(self.workspace.get()).resolve()
            if not workspace.is_dir():
                raise ValueError("Workspace does not exist")
            rounds = self.read_int(self.rounds, "Rounds")
            proposals = self.read_int(self.proposals, "Proposals")
            users = self.read_int(self.users, "Bot users")
            step_limit = self.read_int(self.step_limit, "Step limit")
            positions = parse_level_range(self.position_from.get(), self.position_to.get())
            if self.auto_seed.get():
                generated_seed = int(time.time_ns() & 0x7FFFFFFF)
                self.seed.set(str(generated_seed))
                self.append_log(f"Auto seed for this run: {generated_seed}")
            # The refit trainer requires the run directory to be directly under
            # .../runs and to start with ordinary.
            output = workspace.parent
        except (ValueError, OSError) as error:
            messagebox.showerror("Invalid configuration", str(error))
            return
        self.controller = RoundController(workspace, rounds, positions, proposals, users,
                                          int(self.seed.get()), self.difficulty.get(), self.policy.get(),
                                          step_limit, self.train_models.get(), self.online_feedback.get(),
                                          output, self.events.put, self.refit_each_round.get())
        self.worker = threading.Thread(target=self.run_worker, daemon=True)
        self.worker.start()
        self.start_button.configure(state="disabled")
        self.preview_button.configure(state="disabled")
        self.pause_button.configure(state="normal")
        self.stop_button.configure(state="normal")
        self.status.set("Đang chạy…")

    def run_worker(self):
        try:
            assert self.controller is not None
            self.controller.run()
            self.events.put(("done", None))
        except Exception as error:
            self.events.put(("error", repr(error)))

    def pause(self):
        if not self.controller:
            return
        paused = self.controller.toggle_pause()
        self.pause_button.configure(text="Resume" if paused else "Pause after phase")
        self.status.set("Đã yêu cầu pause sau phase hiện tại." if paused else "Tiếp tục chạy.")

    def stop(self):
        if self.controller:
            self.controller.request_stop()
            self.status.set("Đang dừng sau phase an toàn…")
            self.stop_button.configure(state="disabled")

    def poll(self):
        try:
            while True:
                event = self.events.get_nowait()
                if isinstance(event, tuple):
                    kind, value = event
                    if kind == "done":
                        self.status.set("Hoàn tất các round đã yêu cầu.")
                    elif kind == "error":
                        self.status.set("Có lỗi; xem log.")
                        self.append_log("ERROR: " + value)
                else:
                    self.append_log(str(event))
        except queue.Empty:
            pass
        if self.worker and not self.worker.is_alive():
            self.start_button.configure(state="normal")
            self.preview_button.configure(state="normal")
            self.pause_button.configure(state="disabled", text="Pause after phase")
            self.stop_button.configure(state="disabled")
        self.window.after(100, self.poll)

    def prepare_preview(self):
        if self.worker and self.worker.is_alive():
            messagebox.showinfo("Training active", "Đợi round hiện tại kết thúc rồi mới stage preview.")
            return
        try:
            workspace = Path(self.workspace.get()).resolve()
            positions = parse_level_range(self.position_from.get(), self.position_to.get())
            stager = RoundController(workspace, 1, positions, len(positions), 1, 0, "Source",
                                     "cached-visible-v2", 1, False, False, workspace.parent,
                                     self.append_log)
            stager.stage_preview()
            if not stager.last_preview or not stager.last_preview.get("pack"):
                raise RuntimeError("selector produced no pack; see log")
            self.status.set("Pack test: " + stager.last_preview["pack"])
        except Exception as error:
            self.append_log("ERROR: " + repr(error))
            self.status.set("Stage preview lỗi; xem log.")

    def close(self):
        if self.worker and self.worker.is_alive():
            self.stop()
            messagebox.showinfo("Finishing current phase", "Đã yêu cầu dừng an toàn; đóng cửa sổ sau khi phase Unity hiện tại kết thúc.")
            return
        self.window.destroy()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--workspace", type=Path, default=DEFAULT_WORKSPACE)
    args = parser.parse_args()
    window = tk.Tk()
    TrainingBotApp(window, args.workspace)
    window.mainloop()


if __name__ == "__main__":
    main()
