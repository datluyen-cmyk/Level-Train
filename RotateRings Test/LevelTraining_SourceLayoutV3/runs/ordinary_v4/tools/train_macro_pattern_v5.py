"""Ordinary-only source-family evaluated native motif triple continuations."""
from pathlib import Path
import argparse
import json
import time
from collections import defaultdict

import joblib
import numpy as np

from layout_learning_v3 import ConditionalPoseModel
from macro_pattern_v3 import rows_from_model
from ordinary_source_filter_guard_v4 import preflight, validate_source_filter, sha
from refit_ordinary_models_v4 import local_dependency_hashes


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--workspace", type=Path, required=True)
    args = p.parse_args()
    w = args.workspace.resolve()
    source_filter, bindings = preflight(w)
    target, report_path = w/"models/macro_pattern_v5.joblib", w/"models/macro_pattern_v5_evaluation.json"
    if target.exists() or report_path.exists():
        raise ValueError("Existing V5 pattern model/evaluation will not be overwritten")
    macro_path = w/"models/macro_assembly.joblib"
    model = joblib.load(macro_path)
    allowed = validate_source_filter(source_filter, sha(w/"data/source500_native.json"), model.levels)
    if any(str(t["sourceId"]) not in allowed for t in model.tiles):
        raise ValueError("Excluded source influence in native motif corpus")
    rows = rows_from_model(model)
    splits_path = w/"models/family_splits.json"
    splits = json.loads(splits_path.read_text(encoding="utf8"))
    dependencies = local_dependency_hashes(Path(__file__).resolve())
    hashes = {"sourcePack": sha(w/"data/source500_native.json"), "templates": sha(w/"data/source_templates.json"),
        "macroModel": sha(macro_path), "familySplits": sha(splits_path), "trainer": sha(Path(__file__)), **bindings}
    started, folds = time.monotonic(), []
    for split in splits["splits"]:
        training_ids, held_ids = set(map(str, split["trainSourceIds"])), set(map(str, split["heldOutSourceIds"]))
        if {splits["families"][s] for s in training_ids}&{splits["families"][s] for s in held_ids}:
            raise ValueError("Source-family leakage")
        training = [r for r in rows if str(r["sourceId"]) in training_ids]
        held = [r for r in rows if str(r["sourceId"]) in held_ids]
        if not training or not held:
            raise ValueError("Finite ordinary triple support missing for a declared family fold")
        record = {"fold": split["fold"], "sourceFamilyIntersection": [], "trainingTriples": len(training), "heldOutTriples": len(held)}
        for name, key in (("nativeTripleContinuation", "context"), ("countMechanicsProgressionBaseline", "baseline")):
            estimator = ConditionalPoseModel().fit([r[key] for r in training], [r["pose"] for r in training])
            nll = -estimator.loglikelihood([r[key] for r in held], [r["pose"] for r in held])
            error = estimator.reconstruction_error([r[key] for r in held], [r["pose"] for r in held])
            by_source = defaultdict(list)
            for row, n, e in zip(held, nll, error):
                by_source[str(row["sourceId"])].append((float(n), float(e)))
            record[name] = {"levelMeanNLL": float(np.mean([np.mean(v, axis=0)[0] for v in by_source.values()])),
                "levelMeanOracle32XYErrorIncomingSpacingUnits": float(np.mean([np.mean(v, axis=0)[1] for v in by_source.values()]))}
        folds.append(record)
        print("ORDINARY_NATIVE_TRIPLE_FOLD", json.dumps(record), flush=True)
    final = ConditionalPoseModel().fit([r["context"] for r in rows], [r["pose"] for r in rows])
    if dependencies != local_dependency_hashes(Path(__file__).resolve()) or hashes["macroModel"] != sha(macro_path):
        raise ValueError("Training code/native motif inputs changed")
    artifact = {"schemaVersion": 5, "model": final, "records": rows, "sourceCount": len(allowed), "hashes": hashes,
        "localDependencySha256": dependencies, "status": "ORDINARY_NATIVE_TRIPLE_CONTINUATION_PROPOSAL_ONLY"}
    joblib.dump(artifact, target, compress=3)
    summary = {name: {key: float(np.mean([r[name][key] for r in folds])) for key in folds[0][name]}
        for name in ("nativeTripleContinuation", "countMechanicsProgressionBaseline")}
    report = {"schemaVersion": 5, "status": artifact["status"], "sourceCount": len(allowed), "sourceTripleCount": len(rows),
        "contributingSources": len({r["sourceId"] for r in rows}), "familyHoldoutBeforeFinalRefit": True,
        "folds": folds, "summary": summary, "hashes": {**hashes, "model": sha(target)}, "localDependencySha256": dependencies,
        "elapsedSeconds": time.monotonic()-started, "acceptedCount": 0,
        "method": "Actual ordered native macro triples; relative outgoing vector normalized by incoming motif spacing. Conditional native geometry and incoming frame versus counts/mechanics baseline.",
        "limits": ["Conditional next-motif rows/branches are proposal support, not a whole-source scaffold.",
            "Oracle best32 error is mixture coverage, not single-draw accuracy or gameplay quality.",
            "No stateful native solutions, player difficulty or accepted novel levels are learned by these labels."]}
    report_path.write_text(json.dumps(report, indent=2), encoding="utf8")
    print("ORDINARY_NATIVE_TRIPLE_COMPLETE", len(rows), json.dumps(summary), flush=True)


if __name__ == "__main__":
    main()
