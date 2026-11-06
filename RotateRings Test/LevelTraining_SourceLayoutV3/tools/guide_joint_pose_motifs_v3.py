"""Whole native guide-neighborhood XY+angle empirical proposals, no rescaling.

One saved record supplies every unit's center and angle. Rigid Kabsch fitting
aligns the complete joint patch; individual units never receive random XY edits.
"""
from pathlib import Path
import argparse
import json
import math
import sys

import joblib
import numpy as np
from scipy.special import logsumexp
from sklearn.model_selection import GroupKFold
from sklearn.neighbors import NearestNeighbors
from sklearn.preprocessing import StandardScaler

import guide_neighborhood_motifs_v3 as base


def kabsch(source, target):
    source, target = np.asarray(source, float), np.asarray(target, float)
    a, b = source-source.mean(axis=0), target-target.mean(axis=0)
    u, _, vt = np.linalg.svd(a.T@b)
    rotation = u@vt
    if np.linalg.det(rotation) < 0:
        u[:, -1] *= -1
        rotation = u@vt
    translation = target.mean(axis=0)-source.mean(axis=0)@rotation
    actual = source@rotation+translation
    angle = math.degrees(math.atan2(rotation[0, 1], rotation[0, 0]))
    return rotation, translation, angle, float(np.sqrt(np.mean(np.sum((actual-target)**2, axis=1))))


def records_for(level, templates, size):
    records = []
    for old in base.motif_records(level, templates, size):
        lc = len(old["baseline"])
        unit_width = (len(old["context"])-lc)//size
        descriptors, xy = [], []
        for i in range(size):
            value = old["context"][lc+i*unit_width:lc+(i+1)*unit_width]
            descriptors.extend(value[:-2])
            xy.append(value[-2:])
        xy = np.asarray(xy)
        # Statistical region conditioning, not individual coordinate matching.
        # Output XY stays the entire native recorded patch.
        extent = np.ptp(xy, axis=0)
        pair = np.linalg.norm(xy[:, None, :]-xy[None, :, :], axis=2)
        np.fill_diagonal(pair, np.inf)
        spacing = np.quantile(pair.min(axis=1), [.1, .5, .9])
        region = [*extent.tolist(), float(max(extent)/max(min(extent), .01)),
                  *spacing.tolist(), old["width"]]
        angles = np.asarray(old["pose"]).reshape(size, 2)
        pose = np.c_[xy, angles].reshape(-1).tolist()
        records.append({**old, "context": old["baseline"]+descriptors+region,
                        "baseline": old["baseline"]+[size, old["width"], *region],
                        "pose": pose, "region": region,
                        "nativeXYInBoardFrame": (xy*old["width"]).tolist()})
    return records


class JointGuidePoseModel:
    def fit(self, x, y):
        self.y = np.asarray(y, float)
        self.scaler = StandardScaler().fit(x)
        self.nn = NearestNeighbors(n_neighbors=min(32, len(y)), n_jobs=1).fit(self.scaler.transform(x))
        return self

    def neighbors(self, x):
        return self.nn.kneighbors(self.scaler.transform(np.asarray(x, float)))

    def loglikelihood(self, x, y):
        distance, indices = self.neighbors(x)
        bandwidth = np.tile([.5, .5, .35, .35], self.y.shape[1]//4)
        z = (np.asarray(y)[:, None, :]-self.y[indices])/bandwidth
        weights = -.5*distance**2
        weights -= logsumexp(weights, axis=1)[:, None]
        normalizer = np.log(bandwidth).sum()+len(bandwidth)/2*math.log(2*math.pi)
        return logsumexp(weights-.5*np.sum(z*z, axis=2)-normalizer, axis=1)


def fit_metrics(model, train, held, key):
    nll = -model.loglikelihood([r[key] for r in held], [r["pose"] for r in held])
    _, neighbors = model.neighbors([r[key] for r in held])
    rows = []
    for reference, score, indices in zip(held, nll, neighbors):
        expected = np.asarray(reference["nativeXYInBoardFrame"])
        native_angles = np.asarray(reference["pose"]).reshape(reference["size"], 4)[:, 2:]
        phi = np.arctan2(native_angles[:, 1], native_angles[:, 0])
        choices = []
        for index in indices:
            donor = train[int(index)]
            # No fit scale; native donor absolute XY coordinates are used.
            _, _, angle, error = kabsch(donor["nativeXYInBoardFrame"], expected)
            y = np.asarray(donor["pose"]).reshape(reference["size"], 4)
            other_phi = np.arctan2(y[:, 3], y[:, 2])+math.radians(angle)
            angle_error = float(np.degrees(np.abs(np.angle(np.exp(1j*(other_phi-phi))))).mean())
            choices.append({"rmsWidths": error/reference["width"], "meanAngleDegrees": angle_error,
                            "sameNativeWidth": abs(donor["width"]-reference["width"]) <= 1e-5})
        compatible = [v for v in choices if v["sameNativeWidth"]]
        best = min(choices, key=lambda v: v["rmsWidths"])
        bounded = min(compatible, key=lambda v: v["rmsWidths"]) if compatible else None
        rows.append({"sourceId": reference["sourceId"], "nll": float(score),
                     "oracle32RigidFitRMSWidths": best["rmsWidths"],
                     "angleErrorAtBestRigidFitDegrees": best["meanAngleDegrees"],
                     "sameWidthSupport": bool(compatible),
                     "oracleSameWidthRigidFitRMSWidths": None if bounded is None else bounded["rmsWidths"]})
    aggregate = []
    for source in {r["sourceId"] for r in rows}:
        selected = [r for r in rows if r["sourceId"] == source]
        aggregate.append({"sourceId": source,
                          **{key: float(np.mean([r[key] for r in selected])) for key in
                             ("nll", "oracle32RigidFitRMSWidths", "angleErrorAtBestRigidFitDegrees", "sameWidthSupport")},
                          "oracleSameWidthRigidFitRMSWidths": float(np.mean([r["oracleSameWidthRigidFitRMSWidths"] for r in selected if r["sameWidthSupport"]]))
                          if any(r["sameWidthSupport"] for r in selected) else None})
    return {"meanHeldSourceNLL": float(np.mean([r["nll"] for r in aggregate])),
            "meanHeldSourceOracle32RigidFitRMSWidths": float(np.mean([r["oracle32RigidFitRMSWidths"] for r in aggregate])),
            "meanHeldSourceAngleErrorAtBestRigidFitDegrees": float(np.mean([r["angleErrorAtBestRigidFitDegrees"] for r in aggregate])),
            "meanHeldSourceSameWidthSupportFraction": float(np.mean([r["sameWidthSupport"] for r in aggregate])),
            "perHeldSource": aggregate}


def train(workspace):
    levels = base.read_compact(workspace/"data/source500_native.json")["levels"]
    templates = {t["id"]: t for t in base.read_compact(workspace/"data/source_templates.json")["templates"]}
    families = json.loads((workspace/"models/family_splits.json").read_text())["families"]
    result, reports = {}, []
    for size in (4, 5):
        records = [r for l in levels for r in records_for(l, templates, size)]
        groups = [families[r["sourceId"]] for r in records]
        folds = []
        for fold, (a, b) in enumerate(GroupKFold(n_splits=5).split(records, groups=groups)):
            assert not ({groups[i] for i in a} & {groups[i] for i in b})
            learning, held = [records[i] for i in a], [records[i] for i in b]
            row = {"fold": fold, "familyIntersection": [], "trainingSourceIds": sorted({r["sourceId"] for r in learning}),
                   "heldSourceIds": sorted({r["sourceId"] for r in held})}
            for key, name in (("context", "nativeUnitGeometryRegion"), ("baseline", "countsRegionProgressionBaseline")):
                model = JointGuidePoseModel().fit([r[key] for r in learning], [r["pose"] for r in learning])
                row[name] = fit_metrics(model, learning, held, key)
            folds.append(row)
            print("JOINT_GUIDE_XY_FOLD", size, fold,
                  {name: {k: v for k, v in row[name].items() if k != "perHeldSource"}
                   for name in ("nativeUnitGeometryRegion", "countsRegionProgressionBaseline")}, flush=True)
        final = JointGuidePoseModel().fit([r["context"] for r in records], [r["pose"] for r in records])
        result[size] = {"model": final, "records": records}
        reports.append({"size": size, "records": len(records), "sourceCountWithEligibleUnits": len(set(r["sourceId"] for r in records)), "folds": folds})
    path = workspace/"models/guide_joint_pose_motifs.joblib"
    joblib.dump({"models": result, "trainingSourceCount": len(levels),
                 "sourcePackSha256": base.sha(workspace/"data/source500_native.json"),
                 "templatePackSha256": base.sha(workspace/"data/source_templates.json"),
                 "toolSha256": base.sha(__file__), "method": "Empirical whole4/5nativeguide-unit jointXY+angle patterns; one rigid no-scale fit per patch",
                 "specialFamilyStatus": "No learned diamond family classification; only one confirmed example. Crosssemantic local records require separate wholefigure constraints/review.",
                 "physicsDifficultyStatus": "UNVERIFIED_PROPOSAL_ONLY"}, path, compress=3)
    report = {"trainingSourceCount": len(levels), "models": reports,
              "declaredBeforeEvaluation": {"neighbors": 32, "kernelBandwidthXYCosSin": [.5, .5, .35, .35],
                  "alignment": "Proper rigid Kabsch rotation+translation, scale fixed1, no reflection",
                  "evaluation": "Five existing whole-source-family folds; average records per source then sources equally",
                  "baseline": "Same empirical estimator and region dimensions/spacing/progression, without complete-unit geometry descriptors"},
              "limits": ["Oracle32fit is pattern coverage, not one-sample accuracy or learned difficulty.",
                         "All Kabsch fits preserve native donor distances. Same-width support reported separately; no scale or native asset resizing.",
                         "Wholefigure semantic identity/readability is not learned from one diamond example.",
                         "No accepted generation, native locks, release ordering or human difficulty certified."],
              "hashes": {"source": base.sha(workspace/"data/source500_native.json"), "templates": base.sha(workspace/"data/source_templates.json"),
                         "families": base.sha(workspace/"models/family_splits.json"), "model": base.sha(path), "tool": base.sha(__file__)}}
    (workspace/"models/guide_joint_pose_motifs_evaluation.json").write_text(json.dumps(report, indent=2), encoding="utf8")
    print("JOINT_GUIDE_XY_TRAINED", len(levels), [(r["size"], r["records"]) for r in reports], flush=True)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True, type=Path)
    args = parser.parse_args()
    train(args.workspace.resolve())


if __name__ == "__main__":
    sys.modules.setdefault("guide_joint_pose_motifs_v3", sys.modules[__name__])
    JointGuidePoseModel.__module__ = "guide_joint_pose_motifs_v3"
    main()
