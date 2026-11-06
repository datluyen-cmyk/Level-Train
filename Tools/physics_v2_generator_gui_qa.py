"""Run controlled GUI-path smoke and cancellation QA for PhysicsV2.

This script is deliberately inert unless --run is supplied. It does not fit
models; it requires a finalized, internally consistent model manifest.
"""
from __future__ import annotations

import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import time
import traceback


REPO = Path(__file__).resolve().parents[1]
ROOT = REPO / "RotateRings Test" / "LevelTraining_PhysicsV2"
CONFIG = ROOT / "level_training.json"
GUI_FILE = ROOT / "tools" / "level_generator_gui.py"
FINAL_PACK = ROOT / "batches" / "final-300"
SMOKE_ROOT = ROOT / "batches" / "gui-smoke"
AUDIT = ROOT / "audits" / "gui-smoke" / "generator_gui_qa.json"


def sha256(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def snapshot(paths: list[Path]) -> dict[str, str | None]:
    result = {}
    for path in paths:
        if path.is_file():
            result[str(path.resolve())] = sha256(path)
        elif path.is_dir():
            for item in sorted(p for p in path.rglob("*") if p.is_file()):
                result[str(item.resolve())] = sha256(item)
        else:
            result[str(path.resolve())] = None
    return result


def require_finalized_manifest() -> dict:
    manifest_path = ROOT / "models" / "manifest.json"
    if not manifest_path.is_file():
        raise RuntimeError(f"PhysicsV2 model manifest is not ready: {manifest_path}")
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    if manifest.get("sourceCount", 0) < 500 or not manifest.get("hashes"):
        raise RuntimeError("Model manifest is incomplete; waiting for final fitting output.")
    for rel, expected in manifest["hashes"].items():
        path = ROOT / rel
        if not path.is_file() or sha256(path) != expected:
            raise RuntimeError(f"Manifest input is absent or stale: {rel}")
    for name in ("difficulty.joblib", "proposal.joblib"):
        if not (ROOT / "models" / name).is_file():
            raise RuntimeError(f"Missing finalized model file: models/{name}")
    validation = ROOT / "model_validation.json"
    if not validation.is_file():
        raise RuntimeError("model_validation.json is missing; fitting is not finalized.")
    return {"path": str(manifest_path), "sha256": sha256(manifest_path), "sourceCount": manifest["sourceCount"]}


def load_gui():
    spec = importlib.util.spec_from_file_location("physics_v2_level_generator_gui", GUI_FILE)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Cannot import GUI from {GUI_FILE}")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def pump(root, duration=0.1):
    root.update()
    time.sleep(duration)


def run_case(gui, tk, count: int, seed: int, wait_for_pass: bool, timeout: float, config: Path = CONFIG) -> dict:
    window = tk.Tk()
    window.withdraw()
    app = gui.GeneratorApp(window, str(config), str(SMOKE_ROOT))
    app.count.set(str(count))
    app.seed.set(str(seed))
    app.start()
    process = app.process
    started = time.monotonic()
    seen_pass = False
    observed_lines = []
    while app.process is not None and time.monotonic() - started < timeout:
        pump(window)
        # GUI poll moves worker output into the visible text widget and updates
        # progress; the widget text is our observation of the normal UI path.
        text = app.log.get("1.0", "end-1c")
        if text:
            lines = text.splitlines()
            observed_lines = lines[-20:]
            if any(": PASS" in line for line in lines):
                seen_pass = True
        if wait_for_pass and seen_pass:
            app.cancel()
            break
        if not wait_for_pass and app.process is None:
            break
    if app.process is not None and app.process.poll() is None:
        if wait_for_pass:
            app.cancel()
        else:
            raise TimeoutError(f"Generation did not finish within {timeout}s")
    # Allow Tk's queued done event to be handled so finish() restores controls.
    settle_deadline = time.monotonic() + 15
    while app.process is not None and time.monotonic() < settle_deadline:
        pump(window, 0.05)
    output = app.last_output
    result = {
        "requestedCount": count,
        "seed": seed,
        "output": str(output) if output else None,
        "cancelRequested": bool(app.cancelled),
        "observedPass": seen_pass,
        "returnCode": process.poll() if process is not None else None,
        "startEnabledAfter": str(app.start_button["state"]),
        "cancelEnabledAfter": str(app.cancel_button["state"]),
        "logTail": observed_lines,
        "files": {},
    }
    if output and output.is_dir():
        result["files"] = {p.name: sha256(p) for p in output.iterdir() if p.is_file()}
        report_path = output / "validation_report.json"
        if report_path.is_file():
            result["validationReport"] = json.loads(report_path.read_text(encoding="utf-8"))
    result["controlsRestored"] = result["startEnabledAfter"] == "normal" and result["cancelEnabledAfter"] == "disabled"
    # GeneratorApp polls with a recurring Tk `after`; remove its pending timer
    # after the process finishes to avoid Tcl callbacks firing after destroy.
    for callback in window.tk.call("after", "info"):
        window.after_cancel(callback)
    window.destroy()
    return result


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--run", action="store_true", help="execute GUI generation and cancellation QA")
    parser.add_argument("--timeout", type=float, default=900)
    args = parser.parse_args()
    if not args.run:
        print("Prepared only; pass --run after apk_rules confirms the PhysicsV2 model manifest is finalized.")
        return 0

    report = {"qa": "PhysicsV2 generator GUI smoke and cancellation", "startedAt": time.strftime("%Y-%m-%dT%H:%M:%S%z"), "status": "running"}
    try:
        model = require_finalized_manifest()
        report["modelManifest"] = model
        protected = [ROOT / "models", FINAL_PACK]
        report["protectedBefore"] = snapshot(protected)
        SMOKE_ROOT.mkdir(parents=True, exist_ok=True)
        import tkinter as tk
        gui = load_gui()
        report["generation"] = run_case(gui, tk, 1, 20261105, False, args.timeout)
        report["cancellation"] = run_case(gui, tk, 300, 20261106, True, args.timeout)
        invalid_config = SMOKE_ROOT / "failure-invalid-config.json"
        invalid_config.write_text("{", encoding="utf-8")
        report["failure"] = run_case(gui, tk, 1, 20261107, False, args.timeout, invalid_config)
        invalid_config.unlink(missing_ok=True)
        report["protectedAfter"] = snapshot(protected)
        report["protectedUnchanged"] = report["protectedBefore"] == report["protectedAfter"]
        report["status"] = "pass" if (
            report["generation"].get("controlsRestored")
            and report["cancellation"].get("cancelRequested")
            and report["cancellation"].get("controlsRestored")
            and report["cancellation"].get("observedPass")
            and "partial_levels.json" in report["cancellation"].get("files", {})
            and report["failure"].get("controlsRestored")
            and report["failure"].get("returnCode") not in (None, 0)
            and report["protectedUnchanged"]
        ) else "fail"
    except Exception as exc:
        report["status"] = "blocked_or_failed"
        report["error"] = str(exc)
        report["traceback"] = traceback.format_exc()
    finally:
        report["finishedAt"] = time.strftime("%Y-%m-%dT%H:%M:%S%z")
        AUDIT.parent.mkdir(parents=True, exist_ok=True)
        AUDIT.write_text(json.dumps(report, indent=2), encoding="utf-8")
        print(json.dumps(report, indent=2))
        print(f"Audit: {AUDIT}")
    return 0 if report["status"] == "pass" else 1


if __name__ == "__main__":
    raise SystemExit(main())
