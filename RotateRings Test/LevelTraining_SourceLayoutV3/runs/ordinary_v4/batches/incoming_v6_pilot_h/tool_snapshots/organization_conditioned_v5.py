"""Cross-validated progression conditioning for sharp constructive field modes.

The kernel width is fitted to ordinary joint scale/mechanic profiles on whole
source families. It controls a statistical donor distribution, never an allowed
root count or a maximum distance/shape threshold for game content.
"""
from pathlib import Path
import argparse
import copy
import json
import sys

import joblib
import numpy as np
from sklearn.preprocessing import StandardScaler
from scipy.special import logsumexp

from organization_sharp_v5 import SharpOrganizationModelV5
from organization_model_v5 import atomic
from composition_field_v3 import CHANNELS, H, W
from ordinary_source_filter_guard_v4 import preflight, sha


def weights(query, positions, bandwidth, excluded=None):
    logw = -.5*((float(query)-np.asarray(positions))/bandwidth)**2
    if excluded is not None:
        logw = np.where(excluded, -np.inf, logw)
    return np.exp(logw-logsumexp(logw))


def choose_width(positions, profiles, families, candidates):
    # Entire query family is removed from its donor distribution, including
    # equivalent sibling layouts. Loss units are fitted on eligible train rows.
    scaler = StandardScaler().fit(profiles)
    values = scaler.transform(profiles)
    errors = []
    for bandwidth in candidates:
        pred = np.array([weights(q, positions, bandwidth, families == family)@values
            for q, family in zip(positions, families)])
        per_family = [np.abs(pred[families == f]-values[families == f]).mean()
            for f in set(families)]
        errors.append(float(np.mean(per_family)))
    index = int(np.argmin(errors))
    return float(candidates[index]), errors


class ConditionedOrganizationModelV5(SharpOrganizationModelV5):
    def sample(self, position, rng):
        probability = weights(position, self.training_positions, self.progression_bandwidth)
        index = int(rng.choice(len(self.source_ids), p=probability))
        vector, profile = self.sharp_fields[index], self.profiles[index]
        maps = vector[:CHANNELS*H*W].reshape(CHANNELS, H, W).copy()
        for a, b in ((0, 4), (4, 8)):
            maps[a:b] /= max(maps[a:b].sum(), 1e-12)
        context = profile[2:].copy()
        context[-1] = float(position)/500.
        counts = np.expm1(self.profiles[:, 0])
        count_mean = float(probability@counts)
        return {"field": maps, "orientation": vector[CHANNELS*H*W:].reshape(4, 24)/.2,
            "heading": 0., "span": float(np.exp(profile[1])), "targetParts": float(np.expm1(profile[0])),
            "context": context.tolist(), "position": int(position), "nativeWidthUnit": float(self.width_units[index]),
            "predictedPartCountStd": float(np.sqrt(probability@((counts-count_mean)**2))),
            "partCountDistribution": counts.tolist(), "partCountWeights": probability.tolist(),
            "channelNames": ["ringCenters", "sliderCenters", "fixedCenters", "butterflyCenters",
                "ringBodies", "sliderBodies", "fixedBodies", "butterflyBodies", "pinRelations", "guideRelations"],
            "fieldBoundsNormalized": [-.6, -.6, .6, .6],
            "latentSourceRecords": self.source_ids,
            "families": sorted(set(self.families)), "organizationFieldSourceId": self.source_ids[index],
            "organizationFieldRecordIndex": index, "sampledFieldProbability": float(probability[index]),
            "progressionKernelBandwidth": float(self.progression_bandwidth),
            "method": "family-calibrated progression kernel over empirical sharp full-field modes; from-empty grammar still required",
            "fieldTargetIsSourceBoard": False, "sourcePartsOrCoordinatesReturned": False,
            "numericContentGatesApplied": [], "difficultyStatus": "unmeasured; progression/profile conditioning is not actual difficulty"}


def create(workspace):
    workspace = Path(workspace).resolve()
    preflight(workspace)
    models = workspace/"models"
    source_path = models/"organization_sharp_v5.joblib"
    output = models/"organization_conditioned_v5.joblib"
    report_path = models/"organization_conditioned_v5_evaluation.json"
    if output.exists() or report_path.exists():
        raise ValueError("Refuse existing conditioned organization artifacts")
    base = joblib.load(source_path)
    positions = base.profiles[:, -1]*500.
    families = np.asarray(base.families)
    # Profile excludes supplied progression coordinate; predicting the input
    # coordinate itself would spuriously reward narrower kernels.
    targets = base.profiles[:, :-1]
    spacing = np.median(np.diff(np.unique(positions)))
    candidates = np.geomspace(spacing*.5, max(spacing, np.ptp(positions)*.25), 16)
    splits = json.loads((models/"family_splits.json").read_text(encoding="utf8"))
    by_id = {s: i for i, s in enumerate(base.source_ids)}
    folds = []
    for split in splits["splits"]:
        tr = np.array([by_id[str(s)] for s in split["trainSourceIds"]])
        te = np.array([by_id[str(s)] for s in split["heldOutSourceIds"]])
        bandwidth, _ = choose_width(positions[tr], targets[tr], families[tr], candidates)
        scaler = StandardScaler().fit(targets[tr])
        y = scaler.transform(targets[tr]); actual = scaler.transform(targets[te])
        pred = np.array([weights(q, positions[tr], bandwidth)@y for q in positions[te]])
        mean = np.tile(y.mean(axis=0), (len(te), 1))
        row = {"fold": split["fold"], "trainingLevels": len(tr), "heldOutLevels": len(te),
            "bandwidthChosenInsideTrainFamilies": bandwidth,
            "jointProfileNormalizedMAE": float(np.abs(pred-actual).mean()),
            "trainingMeanBaselineMAE": float(np.abs(mean-actual).mean())}
        folds.append(row)
        print("PROGRESSION_KERNEL_FOLD", json.dumps(row), flush=True)
    bandwidth, losses = choose_width(positions, targets, families, candidates)
    final = ConditionedOrganizationModelV5()
    final.__dict__.update(copy.deepcopy(base.__dict__))
    final.training_positions = positions
    final.progression_bandwidth = bandwidth
    final.conditioning_hashes = {"sharpModel": sha(source_path), "trainer": sha(Path(__file__)),
        "familySplits": sha(models/"family_splits.json")}
    joblib.dump(final, output, compress=3)
    atomic(report_path, {"schemaVersion": 5, "status": "EMPIRICAL_SHARP_FIELD_PROGRESSION_CONDITIONING_ONLY",
        "sourceCount": len(base.source_ids), "sourceIds": base.source_ids,
        "hashes": {**base.hashes, **final.sharp_mode_hashes, **final.conditioning_hashes, "model": sha(output)},
        "selectedBandwidth": bandwidth, "candidateBandwidths": candidates.tolist(), "familyCrossValidationLosses": losses,
        "folds": folds, "summary": {key: float(np.mean([r[key] for r in folds]))
            for key in ("jointProfileNormalizedMAE", "trainingMeanBaselineMAE")},
        "numericContentGatesApplied": [], "acceptedCount": 0,
        "limits": ["Nested whole-family profile prediction is not generated-level or difficulty evaluation.",
            "Sharp fields are empirical source layout modes used only as soft statistical targets.",
            "No source parts, root coordinates or existing level scaffold are returned.",
            "Actual novelty, native gameplay and visual design remain mandatory."]})
    print("PROGRESSION_KERNEL_COMPLETE", bandwidth, str(output), flush=True)


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--workspace", required=True, type=Path)
    args = p.parse_args()
    create(args.workspace)


if __name__ == "__main__":
    sys.modules.setdefault("organization_conditioned_v5", sys.modules[__name__])
    ConditionedOrganizationModelV5.__module__ = "organization_conditioned_v5"
    main()
