"""Ordinary-only, from-empty board organization prior.

Learns complete native occupancy/relationship/orientation fields jointly with
scale and mechanic mixture. No original level is returned as a scaffold. The
prior is a proposal distribution, never a playability or difficulty label.
"""
from pathlib import Path
import argparse
import json
import math
import sys

import joblib
import numpy as np
from sklearn.decomposition import PCA
from sklearn.ensemble import ExtraTreesRegressor
from sklearn.linear_model import Ridge
from sklearn.preprocessing import StandardScaler

from composition_field_v3 import field, CHANNELS, H, W
from layout_learning_v3 import level_context
from ordinary_source_filter_guard_v4 import preflight, validate_source_filter, sha

SEED = 20261106


def read(path):
    return json.loads(Path(path).read_text(encoding="utf8"))


def compact(value):
    if "templateId" in value and "kind" in value and "id" in value:
        for key in ("sourceNativeData", "sourceRigidbodyNativeData", "sourcePartData"):
            value.pop(key, None)
    return value


def atomic(path, value):
    path = Path(path)
    temp = path.with_suffix(path.suffix+".tmp")
    temp.write_text(json.dumps(value, indent=2), encoding="utf8")
    temp.replace(path)


def estimator(seed=SEED):
    # Capacity/leaf size regularize the statistical model; they are not level
    # counts, component quotas, spacing bounds, or difficulty acceptance gates.
    return ExtraTreesRegressor(n_estimators=96, max_depth=10,
        min_samples_leaf=4, random_state=seed, n_jobs=1)


def encode(fields, profiles, train, components=32):
    pca = PCA(n_components=min(components, len(train)-1),
        svd_solver="randomized", random_state=SEED).fit(fields[train])
    latent = pca.transform(fields[train])
    scaler = StandardScaler().fit(np.c_[latent, profiles[train]])
    return pca, scaler, scaler.transform(np.c_[latent, profiles[train]])


def decode(pca, scaler, normalized):
    values = scaler.inverse_transform(normalized)
    n = pca.n_components_
    return np.maximum(0., pca.inverse_transform(values[:, :n])), values[:, n:]


class OrganizationModel:
    def sample(self, position, rng):
        """Sample a conditional tree-leaf field mixture, never a source board.

        Each leaf is a learned average of training fields. A tree is drawn to
        retain conditional variation hidden by the ensemble mean. Its exact
        contributors are retained to audit ordinary-only source influence.
        """
        query = np.array([[float(position)/self.position_scale]])
        index = int(rng.integers(len(self.forest.estimators_)))
        tree = self.forest.estimators_[index]
        leaf = int(tree.apply(query)[0])
        members = np.flatnonzero(self.leaves[:, index] == leaf)
        if not len(members):
            raise ValueError("Conditional organization leaf has no training support")
        decoded, profiles = decode(self.pca, self.scaler, tree.predict(query))
        maps = decoded[0, :CHANNELS*H*W].reshape(CHANNELS, H, W).copy()
        for a, b in ((0, 4), (4, 8)):
            maps[a:b] /= max(maps[a:b].sum(), 1e-12)
        orientation = decoded[0, CHANNELS*H*W:].reshape(4, 24)/.2
        profile = profiles[0]
        count = float(max(1., np.expm1(profile[0])))
        context = profile[2:].copy()
        context[0] = math.log1p(count)
        context[-1] = float(position)/500.
        return {"field": maps, "orientation": orientation, "heading": 0.,
            "span": float(np.exp(profile[1])), "targetParts": count,
            "predictedPartCountStd": float(np.std(np.expm1(self.profiles[members, 0]))),
            "partCountDistribution": np.expm1(self.profiles[members, 0]).tolist(),
            "channelNames": ["ringCenters", "sliderCenters", "fixedCenters", "butterflyCenters",
                "ringBodies", "sliderBodies", "fixedBodies", "butterflyBodies", "pinRelations", "guideRelations"],
            "fieldBoundsNormalized": [-.6, -.6, .6, .6],
            "spanContract": "scalar native-world span of learned canonical frame; motifs are never rescaled",
            "nativeWidthUnit": float(np.exp(np.mean(np.log(self.width_units[members])))),
            "context": context.tolist(), "position": int(position),
            "latentSourceRecords": [self.source_ids[i] for i in members],
            "families": sorted({self.families[i] for i in members}),
            "treeIndex": index, "leafId": leaf,
            "method": "conditional learned leaf mean of full native layout fields; no source scaffold",
            "numericContentGatesApplied": [],
            "difficultyStatus": "unmeasured; source progression is conditioning, not difficulty proof"}

    def predict_field(self, positions):
        x = np.asarray(positions, float).reshape(-1, 1)/self.position_scale
        return decode(self.pca, self.scaler, self.forest.predict(x))


def metrics(prediction, actual, profiles, actual_profiles):
    return {"kindFieldL1": float(np.abs(prediction[:, :4*H*W]-actual[:, :4*H*W]).sum(axis=1).mean()),
        "nativeBodyFieldL1": float(np.abs(prediction[:, 4*H*W:8*H*W]-actual[:, 4*H*W:8*H*W]).sum(axis=1).mean()),
        "relationFieldL1": float(np.abs(prediction[:, 8*H*W:CHANNELS*H*W]-actual[:, 8*H*W:CHANNELS*H*W]).sum(axis=1).mean()),
        "logPartCountMAE": float(np.abs(profiles[:, 0]-actual_profiles[:, 0]).mean()),
        "logSpanMAE": float(np.abs(profiles[:, 1]-actual_profiles[:, 1]).mean())}


def train(workspace):
    workspace = Path(workspace).resolve()
    source_filter, bindings = preflight(workspace)
    pack_path = workspace/"data/source500_native.json"
    template_path = workspace/"data/source_templates.json"
    split_path = workspace/"models/family_splits.json"
    with pack_path.open(encoding="utf8") as stream:
        levels = json.load(stream, object_hook=compact)["levels"]
    validate_source_filter(source_filter, sha(pack_path), [l["sourceId"] for l in levels])
    templates = {t["id"]: t for t in read(template_path)["templates"]}
    splits = read(split_path)
    model_path = workspace/"models/organization_v5.joblib"
    report_path = workspace/"models/organization_v5_evaluation.json"
    if model_path.exists() or report_path.exists():
        raise ValueError("Existing organization model/evaluation will not be overwritten")
    inputs = {"sourcePack": sha(pack_path), "templates": sha(template_path),
        "familySplits": sha(split_path), "trainer": sha(Path(__file__)), **bindings}
    from refit_ordinary_models_v4 import local_dependency_hashes
    dependencies = local_dependency_hashes(Path(__file__).resolve())
    cache = workspace/"models/organization_v5_native_fields.joblib"
    if cache.exists():
        cached = joblib.load(cache)
        if cached["hashes"] != inputs:
            raise ValueError("Native field extraction cache is stale")
        fields, profiles = cached["fields"], cached["profiles"]
    else:
        rows, profile_rows = [], []
        for i, level in enumerate(levels):
            value, frame = field(level, templates)
            rows.append(value)
            profile_rows.append([math.log1p(len(level["parts"])),
                math.log(max(frame["span"], .01)), *level_context(level)])
            if (i+1) % 25 == 0:
                print("ORGANIZATION_NATIVE_FIELDS", i+1, len(levels), flush=True)
        fields, profiles = np.asarray(rows), np.asarray(profile_rows)
        joblib.dump({"fields": fields, "profiles": profiles, "hashes": inputs}, cache, compress=3)
    source_ids = [str(l["sourceId"]) for l in levels]
    by_id = {sid: i for i, sid in enumerate(source_ids)}
    positions = np.array([[l["position"]/500.] for l in levels])
    folds = []
    for split in splits["splits"]:
        training = np.array([by_id[str(s)] for s in split["trainSourceIds"]])
        held = np.array([by_id[str(s)] for s in split["heldOutSourceIds"]])
        tr_f = {splits["families"][source_ids[i]] for i in training}
        te_f = {splits["families"][source_ids[i]] for i in held}
        if tr_f & te_f:
            raise ValueError("Organization evaluation contains source-family leakage")
        pca, scaler, targets = encode(fields, profiles, training)
        forest = estimator().fit(positions[training], targets)
        linear = Ridge(alpha=10).fit(positions[training], targets)
        row = {"fold": split["fold"], "trainingLevels": len(training),
            "heldOutLevels": len(held), "familyIntersection": []}
        for name, normalized in (("conditionalForest", forest.predict(positions[held])),
                ("linearProgressionBaseline", linear.predict(positions[held])),
                ("trainingMeanBaseline", np.tile(targets.mean(axis=0), (len(held), 1)))):
            pred, profile = decode(pca, scaler, normalized)
            row[name] = metrics(pred, fields[held], profile, profiles[held])
        folds.append(row)
        print("ORGANIZATION_FAMILY_FOLD", json.dumps(row), flush=True)
    all_indices = np.arange(len(levels))
    pca, scaler, targets = encode(fields, profiles, all_indices)
    final = OrganizationModel()
    final.pca, final.scaler = pca, scaler
    final.forest = estimator().fit(positions, targets)
    final.leaves = final.forest.apply(positions)
    final.source_ids = source_ids
    final.profiles = profiles
    final.width_units = np.array([max(.01, float(np.median([
        templates[p["templateId"]].get("width", 0.) for p in l["parts"]
        if p["kind"] in ("ring", "slider")] or [.5]))) for l in levels])
    final.families = [splits["families"][sid] for sid in source_ids]
    final.position_scale = 500.
    final.hashes = inputs
    final.local_dependency_hashes = dependencies
    if local_dependency_hashes(Path(__file__).resolve()) != dependencies:
        raise ValueError("Organization trainer changed during fitting")
    if sha(pack_path) != inputs["sourcePack"] or sha(template_path) != inputs["templates"]:
        raise ValueError("Ordinary sources changed during fitting")
    joblib.dump(final, model_path, compress=3)
    summary = {name: {metric: float(np.mean([r[name][metric] for r in folds]))
        for metric in folds[0][name]} for name in ("conditionalForest", "linearProgressionBaseline", "trainingMeanBaseline")}
    atomic(report_path, {"schemaVersion": 5, "status": "ORDINARY_CONSTRUCTIVE_ORGANIZATION_PRIOR_ONLY",
        "sourceCount": len(levels), "sourceIds": source_ids,
        "folds": folds, "summary": summary, "hashes": {**inputs, "model": sha(model_path)},
        "localDependencySha256": dependencies,
        "features": "10 full native center/body/relation maps plus 4 orientation histograms; scale and joint mechanic profile",
        "sampling": "random learned regression-tree leaf mean, constructed from multiple training records; never returns source parts or scaffold",
        "numericContentGatesApplied": [], "acceptedCount": 0,
        "limits": ["Progression and layout statistics are not observed player difficulty.",
            "PCA/leaf averaging can blur structure; native macro placement and whole-board visual review remain required.",
            "Family-separated conditional field reconstruction is not whole-level generation quality.",
            "No claim of 1000/10000 accepted levels or unlimited novelty capacity."]})
    print("ORGANIZATION_COMPLETE", json.dumps(summary), flush=True)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True, type=Path)
    args = parser.parse_args()
    train(args.workspace)


if __name__ == "__main__":
    sys.modules.setdefault("organization_model_v5", sys.modules[__name__])
    OrganizationModel.__module__ = "organization_model_v5"
    main()
