"""Learn joint native guide-neighborhood angles, preserving complete units.

This is a proposal model over captured source layouts. No output is a physical
lock, solvability, difficulty, or fun label. Native engine replay remains required.
"""
from __future__ import annotations

from collections import Counter
from pathlib import Path
import argparse
import hashlib
import json
import math

import joblib
import networkx as nx
import numpy as np
from scipy.special import logsumexp
from sklearn.model_selection import GroupKFold
from sklearn.neighbors import NearestNeighbors
from sklearn.preprocessing import StandardScaler

KINDS = ["ring", "slider", "fixed", "butterfly"]
BANDWIDTH = .35
NEIGHBORS = 32


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def compact_object(value):
    """Discard native payloads only for this statistical source index."""
    if "templateId" in value and "kind" in value and "id" in value:
        keys = ("id", "templateId", "kind", "x", "y", "angle", "z",
                "chainLockCount", "key", "lockKey")
        return {key: value[key] for key in keys if key in value}
    if str(value.get("id", "")).startswith("native3-") and "physicalShapes" in value:
        points = [p for shape in value["physicalShapes"]
                  for path in shape.get("paths", [{"points": shape.get("points", [])}])
                  for p in path["points"]]
        extent = [max((p[k] for p in points), default=0)-min((p[k] for p in points), default=0)
                  for k in ("x", "y")]
        return {"id": value["id"], "kind": value["kind"],
                "length": value.get("length", 0), "width": value.get("width", 0),
                "radius": value.get("radius", 0), "gap": value.get("gap", 0),
                "extent": extent}
    if "parts" in value and "position" in value and "sourceId" in value:
        return {key: value[key] for key in ("sourceId", "position", "parts", "guideLinks", "bombs", "links")}
    return value


def read_compact(path):
    with Path(path).open(encoding="utf8") as stream:
        return json.load(stream, object_hook=compact_object)


def rotate(xy, angle):
    angle = math.radians(angle)
    c, s = math.cos(angle), math.sin(angle)
    return np.asarray(xy) @ np.array([[c, s], [-s, c]])


def board_heading(level):
    xy = np.asarray([[p["x"], p["y"]] for p in level["parts"]
                     if p["kind"] == "slider"])
    if len(xy) < 2:
        return 0.
    values, vectors = np.linalg.eigh(np.cov((xy-xy.mean(axis=0)).T))
    axis = vectors[:, np.argmax(values)]
    if axis[1] < -1e-7 or (abs(axis[1]) <= 1e-7 and axis[0] < 0):
        axis = -axis
    return math.degrees(math.atan2(axis[1], axis[0]))


def units(level, templates):
    ps = {p["id"]: p for p in level["parts"]}
    graph = nx.Graph()
    graph.add_nodes_from(ps)
    graph.add_edges_from((e["guideOwnerId"], e["lockedOwnerId"]) for e in level["guideLinks"])
    bombs = {b["ownerPartId"] for b in level["bombs"]}
    result = []
    for component in nx.connected_components(graph):
        members = sorted(component)
        pulls = [ps[i] for i in members if ps[i]["kind"] == "slider"]
        if not 2 <= len(members) <= 3 or len(pulls) != 1:
            continue
        if any(i in bombs or ps[i].get("chainLockCount", 0) or ps[i].get("key") or ps[i].get("lockKey") for i in members):
            continue
        if any((e["source"] in component) != (e["target"] in component) for e in level["links"]):
            continue
        primary = pulls[0]
        template = templates[primary["templateId"]]
        fixture_parts = [ps[i] for i in members if i != primary["id"]]
        fixture_xy = rotate([[p["x"]-primary["x"], p["y"]-primary["y"]]
                             for p in fixture_parts], -primary["angle"])
        counts = Counter(ps[i]["kind"] for i in members)
        fixture_extent = np.mean([templates[p["templateId"]]["extent"] for p in fixture_parts], axis=0)
        offsets = np.radians([p["angle"]-primary["angle"] for p in fixture_parts])
        descriptor = [len(members), *[counts[k] for k in KINDS],
                      template["length"], template["width"], template["radius"], template["gap"]/180,
                      *fixture_xy.mean(axis=0).tolist(), float(np.cos(offsets).mean()),
                      float(np.sin(offsets).mean()), *fixture_extent.tolist()]
        result.append({"ids": members, "primary": primary["id"],
                       "xy": [primary["x"], primary["y"]], "angle": primary["angle"],
                       "descriptor": descriptor, "length": template["length"],
                       "width": max(.01, template["width"])})
    return result


def level_context(level):
    n = len(level["parts"])
    counts = Counter(p["kind"] for p in level["parts"])
    return [math.log1p(n), *[counts[k]/max(1, n) for k in KINDS],
            len(level["guideLinks"])/max(1, n), len(level["links"])/max(1, n),
            float(bool(level["bombs"])), level["position"]/500]


def motif_records(level, templates, size):
    us = units(level, templates)
    if len(us) < size:
        return []
    heading = board_heading(level)
    xy = np.asarray([u["xy"] for u in us])
    width = float(np.median([u["width"] for u in us]))
    records = []
    for anchor, unit in enumerate(us):
        nearest = sorted((i for i in range(len(us)) if i != anchor),
                         key=lambda i: (np.linalg.norm(xy[i]-xy[anchor]), us[i]["primary"]))[:size-1]
        relative = rotate(xy-xy[anchor], -heading)/width
        neighbors = sorted(nearest, key=lambda i: (math.atan2(relative[i, 1], relative[i, 0]), np.linalg.norm(relative[i])))
        order = [anchor, *neighbors]
        context = level_context(level)
        for i in order:
            context += us[i]["descriptor"]+relative[i].tolist()
        phi = np.radians([us[i]["angle"]-heading for i in order])
        records.append({"sourceId": level["sourceId"], "position": level["position"],
                        "size": size, "unitIds": [us[i]["ids"] for i in order],
                        "primaryIds": [us[i]["primary"] for i in order],
                        "heading": heading, "width": width,
                        "context": context, "baseline": level_context(level),
                        "pose": np.c_[np.cos(phi), np.sin(phi)].reshape(-1).tolist()})
    return records


class GuideMotifModel:
    def fit(self, x, y):
        self.y = np.asarray(y, float)
        self.scaler = StandardScaler().fit(x)
        self.nn = NearestNeighbors(n_neighbors=min(NEIGHBORS, len(y)), n_jobs=1).fit(self.scaler.transform(x))
        return self

    def neighbors(self, x):
        return self.nn.kneighbors(self.scaler.transform(np.asarray(x, float)))

    def loglikelihood(self, x, y):
        distances, indices = self.neighbors(x)
        z = (np.asarray(y)[:, None, :]-self.y[indices])/BANDWIDTH
        weights = -.5*distances**2
        weights -= logsumexp(weights, axis=1)[:, None]
        d = self.y.shape[1]
        return logsumexp(weights-.5*np.sum(z*z, axis=2)-d*math.log(BANDWIDTH)-d/2*math.log(2*math.pi), axis=1)

    def best32_angle_error(self, x, y):
        _, indices = self.neighbors(x)
        expected = np.asarray(y).reshape(len(y), -1, 2)
        actual = self.y[indices].reshape(len(y), indices.shape[1], -1, 2)
        phi = np.arctan2(expected[:, :, 1], expected[:, :, 0])
        other = np.arctan2(actual[:, :, :, 1], actual[:, :, :, 0])
        error = np.degrees(np.abs(np.angle(np.exp(1j*(other-phi[:, None, :]))))).mean(axis=2)
        return error.min(axis=1)


def train(workspace):
    source = workspace/"data/source500_native.json"
    template_path = workspace/"data/source_templates.json"
    levels = read_compact(source)["levels"]
    templates = {t["id"]: t for t in read_compact(template_path)["templates"]}
    families = json.loads((workspace/"models/family_splits.json").read_text())["families"]
    models = {}
    reports = []
    for size in (4, 5):
        records = [r for level in levels for r in motif_records(level, templates, size)]
        source_ids = sorted({r["sourceId"] for r in records})
        groups = [families[r["sourceId"]] for r in records]
        folds = []
        for fold, (train_indices, held_indices) in enumerate(GroupKFold(n_splits=5).split(records, groups=groups)):
            train_records = [records[i] for i in train_indices]
            held = [records[i] for i in held_indices]
            row = {"fold": fold, "trainSourceCount": len({r["sourceId"] for r in train_records}),
                   "heldSourceCount": len({r["sourceId"] for r in held}), "familyIntersection": []}
            assert not ({groups[i] for i in train_indices} & {groups[i] for i in held_indices})
            for key, label in (("context", "nativeWholeNeighborhood"), ("baseline", "countsMechanicsProgressionBaseline")):
                model = GuideMotifModel().fit([r[key] for r in train_records], [r["pose"] for r in train_records])
                nll = -model.loglikelihood([r[key] for r in held], [r["pose"] for r in held])
                error = model.best32_angle_error([r[key] for r in held], [r["pose"] for r in held])
                per_source = []
                for sid in {r["sourceId"] for r in held}:
                    selected = [i for i, r in enumerate(held) if r["sourceId"] == sid]
                    per_source.append([float(nll[selected].mean()), float(error[selected].mean())])
                mean = np.mean(per_source, axis=0)
                row[label] = {"meanHeldSourceNLL": float(mean[0]), "oracleBest32MeanUnitAngleErrorDegrees": float(mean[1])}
            folds.append(row)
            print("GUIDE_MOTIF_FOLD", size, fold, row, flush=True)
        model = GuideMotifModel().fit([r["context"] for r in records], [r["pose"] for r in records])
        models[size] = {"model": model, "records": records}
        reports.append({"size": size, "recordCount": len(records), "sourceCountWithEligibleNativeUnits": len(source_ids),
                        "folds": folds})
    target = workspace/"models/guide_neighborhood_motifs.joblib"
    bundle = {"models": models, "trainingSourceCount": len(levels), "sourcePackSha256": sha(source),
              "templatePackSha256": sha(template_path), "toolSha256": sha(__file__),
              "method": "Conditional empirical whole native4/5guide-unit joint-angle motifs; center geometry and complete unit descriptors condition32native patterns",
              "status": "PROPOSAL_ONLY_NO_PHYSICS_DIFFICULTY_OR_QUALITY_CERTIFICATION"}
    joblib.dump(bundle, target, compress=3)
    report = {"trainingSourceCount": len(levels), "models": reports,
              "hyperparametersDeclaredBeforeEvaluation": {"neighbors": NEIGHBORS, "angularCosSinBandwidth": BANDWIDTH},
              "evaluation": "5fold source-family holdout, averaged per held source; refit all500 only afterward",
              "limits": ["Source snapshot joint-angle patterns, not physical lock or motion labels.",
                         "Oracle32angle metric is proposal coverage, not single-sample accuracy or difficulty.",
                         "No claim of improvement unless held-family metrics beat the baseline."],
              "hashes": {"source": sha(source), "templates": sha(template_path), "model": sha(target), "tool": sha(__file__)}}
    (workspace/"models/guide_neighborhood_motifs_evaluation.json").write_text(json.dumps(report, indent=2), encoding="utf8")
    print("GUIDE_MOTIF_TRAINED", [dict(size=r["size"], records=r["recordCount"], sources=r["sourceCountWithEligibleNativeUnits"]) for r in reports], flush=True)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True, type=Path)
    args = parser.parse_args()
    train(args.workspace.resolve())


if __name__ == "__main__":
    import sys
    sys.modules.setdefault("guide_neighborhood_motifs_v3", sys.modules[__name__])
    GuideMotifModel.__module__ = "guide_neighborhood_motifs_v3"
    main()
