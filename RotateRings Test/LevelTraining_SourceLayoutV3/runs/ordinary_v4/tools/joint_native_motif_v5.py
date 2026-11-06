"""Joint empirical native next-motif/pose proposal with known-frontier queries.

No requested target descriptor or target-dependent width enters a query. A
single selected observed record supplies both the next native tile and pose.
"""
from pathlib import Path
from collections import defaultdict
import argparse
import json
import math
import time

import joblib
import numpy as np
from scipy.special import logsumexp
from sklearn.neighbors import NearestNeighbors
from sklearn.preprocessing import StandardScaler

from macro_pattern_v3 import rows_from_model
from ordinary_source_filter_guard_v4 import preflight, validate_source_filter, sha
from refit_ordinary_models_v4 import local_dependency_hashes


def pair_query(context, parent):
    return list(context)+list(parent["descriptor"])


def triple_query(context, grandparent, parent, incoming_angle, spacing):
    phi = math.radians(parent["heading"]-incoming_angle)
    return list(context)+list(grandparent["descriptor"])+list(parent["descriptor"])+[
        math.cos(phi), math.sin(phi), math.log(max(spacing/max(grandparent["width"], parent["width"]), .001)),
        float(grandparent["descriptor"] == parent["descriptor"])]


class JointNativeSampler:
    def fit(self, records):
        self.records = records
        self.scaler = StandardScaler().fit([r["query"] for r in records])
        self.nn = NearestNeighbors(n_neighbors=min(32, len(records))).fit(self.scaler.transform([r["query"] for r in records]))
        return self

    def neighbors(self, queries):
        distances, indices = self.nn.kneighbors(self.scaler.transform(queries))
        logits = -.5*distances**2
        weights = np.exp(logits-logsumexp(logits, axis=1, keepdims=True))
        return indices, weights

    def sample(self, query, rng):
        indices, weights = self.neighbors([query])
        local = int(rng.choice(len(indices[0]), p=weights[0]))
        index = int(indices[0, local])
        return {"recordIndex": index, "record": self.records[index], "neighborProbability": float(weights[0, local])}


def rows(macro):
    pairs, triples = [], []
    for record in macro.relations:
        parent, target = macro.tiles[record["sourceTile"]], macro.tiles[record["targetTile"]]
        context = record["context"][:10]
        pairs.append({**record, "query": pair_query(context, parent), "baselineQuery": list(context)+[len(parent["partIds"])],
            "targetDescriptor": target["descriptor"]})
    for record in rows_from_model(macro):
        a, b, c = [macro.tiles[record[key]] for key in ("sourceTile", "centerTile", "targetTile")]
        vector = np.asarray(b["center"])-np.asarray(a["center"])
        spacing = float(np.linalg.norm(vector))
        angle = math.degrees(math.atan2(vector[1], vector[0]))
        context = record["context"][:10]
        triples.append({**record, "query": triple_query(context, a, b, angle, spacing),
            "baselineQuery": list(context)+[len(a["partIds"]), len(b["partIds"])], "targetDescriptor": c["descriptor"]})
    return {"pair": pairs, "triple": triples}


def evaluate(training, held, query_key):
    fitted_rows = [{**r, "query": r[query_key]} for r in training]
    estimator = JointNativeSampler().fit(fitted_rows)
    indices, weights = estimator.neighbors([r[query_key] for r in held])
    descriptors = np.asarray([r["targetDescriptor"] for r in training])
    descriptor_scale = StandardScaler().fit(descriptors)
    neighbor_descriptors = descriptor_scale.transform(descriptors)[indices]
    actual_descriptor = descriptor_scale.transform([r["targetDescriptor"] for r in held])
    descriptor_error = np.abs(neighbor_descriptors-actual_descriptor[:, None, :]).mean(axis=2)
    poses = np.asarray([r["pose"] for r in training])[indices]
    actual_pose = np.asarray([r["pose"] for r in held])
    xy_error = np.linalg.norm(poses[:, :, :2]-actual_pose[:, None, :2], axis=2)
    angular_error = np.linalg.norm(poses[:, :, 2:]-actual_pose[:, None, 2:], axis=2)
    by_source = defaultdict(list)
    for i, record in enumerate(held):
        by_source[str(record["sourceId"])].append([
            float(weights[i]@descriptor_error[i]), float(weights[i]@xy_error[i]), float(weights[i]@angular_error[i]),
            float(np.min(descriptor_error[i]+xy_error[i]))])
    values = np.mean([np.mean(v, axis=0) for v in by_source.values()], axis=0)
    return dict(zip(("expectedStandardizedDescriptorMAE", "expectedPoseXYError", "expectedPoseCosSinError",
        "oracle32DescriptorPlusXYError"), map(float, values)))


def train(workspace):
    w = Path(workspace).resolve()
    source_filter, bindings = preflight(w)
    model_path, report_path = w/"models/joint_native_motif_v5.joblib", w/"models/joint_native_motif_v5_evaluation.json"
    if model_path.exists() or report_path.exists():
        raise ValueError("Refuse existing joint native model or evaluation")
    macro_path = w/"models/macro_assembly.joblib"
    macro = joblib.load(macro_path)
    allowed = validate_source_filter(source_filter, sha(w/"data/source500_native.json"), macro.levels)
    corpus = rows(macro)
    if any(str(r["sourceId"]) not in allowed for records in corpus.values() for r in records):
        raise ValueError("Excluded joint source influence")
    split_path = w/"models/family_splits.json"
    splits = json.loads(split_path.read_text(encoding="utf8"))
    dependencies = local_dependency_hashes(Path(__file__).resolve())
    hashes = {"sourcePack": sha(w/"data/source500_native.json"), "templates": sha(w/"data/source_templates.json"),
        "macroModel": sha(macro_path), "familySplits": sha(split_path), "trainer": sha(Path(__file__)), **bindings}
    started, folds = time.monotonic(), []
    for split in splits["splits"]:
        training_ids, held_ids = set(map(str, split["trainSourceIds"])), set(map(str, split["heldOutSourceIds"]))
        if {splits["families"][s] for s in training_ids}&{splits["families"][s] for s in held_ids}:
            raise ValueError("Family leakage")
        result = {"fold": split["fold"], "sourceFamilyIntersection": [], "grammars": {}}
        for name, records in corpus.items():
            training = [r for r in records if str(r["sourceId"]) in training_ids]
            held = [r for r in records if str(r["sourceId"]) in held_ids]
            result["grammars"][name] = {"trainingRecords": len(training), "heldOutRecords": len(held),
                "jointKnownFrontier": evaluate(training, held, "query"),
                "countMechanicsProgressionBaseline": evaluate(training, held, "baselineQuery")}
        folds.append(result)
        print("JOINT_NATIVE_FOLD", json.dumps(result), flush=True)
    final = {name: JointNativeSampler().fit(records) for name, records in corpus.items()}
    if hashes["macroModel"] != sha(macro_path) or dependencies != local_dependency_hashes(Path(__file__).resolve()):
        raise ValueError("Inputs/code changed during training")
    joblib.dump({"schemaVersion": 5, "models": final, "hashes": hashes, "sourceCount": len(allowed),
        "localDependencySha256": dependencies, "status": "JOINT_NATIVE_TARGET_TILE_AND_POSE_PROPOSAL_ONLY"}, model_path, compress=3)
    summary = {name: {method: {metric: float(np.mean([f["grammars"][name][method][metric] for f in folds]))
        for metric in folds[0]["grammars"][name][method]} for method in ("jointKnownFrontier", "countMechanicsProgressionBaseline")}
        for name in corpus}
    report_path.write_text(json.dumps({"schemaVersion": 5, "sourceCount": len(allowed), "acceptedCount": 0,
        "recordCounts": {name: len(records) for name, records in corpus.items()}, "familyHoldoutBeforeFinalRefit": True,
        "summary": summary, "folds": folds, "hashes": {**hashes, "model": sha(model_path)},
        "localDependencySha256": dependencies, "elapsedSeconds": time.monotonic()-started,
        "method": "Known-frontier standardized32nearest-neighbor empirical probabilities jointly sample target native tile and its exact observed pose.",
        "targetDescriptorOrTargetWidthInQuery": False,
        "poseUnits": {"pair": "max(parent,target) native width, available only after joint record draw", "triple": "actual known incoming spacing"},
        "limits": ["Expected descriptor/pose distances measure held-family conditional prediction, not native solvability or player difficulty.",
            "Oracle32 descriptor+XY score is optimistic mixture coverage and not a single-draw result.",
            "Prototype descriptors precede conservative closure; closure assets remain exact, but arrangement support may shift.",
            "No novel accepted whole boards or whole-layout quality certified."]}, indent=2), encoding="utf8")
    print("JOINT_NATIVE_COMPLETE", json.dumps(summary), flush=True)


if __name__ == "__main__":
    import sys
    sys.modules.setdefault("joint_native_motif_v5", sys.modules[__name__])
    JointNativeSampler.__module__ = "joint_native_motif_v5"
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", type=Path, required=True)
    train(parser.parse_args().workspace)
