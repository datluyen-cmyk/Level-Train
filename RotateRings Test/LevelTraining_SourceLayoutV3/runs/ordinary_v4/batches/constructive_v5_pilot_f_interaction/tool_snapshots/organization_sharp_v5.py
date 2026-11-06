"""Conditional empirical whole-field modes for constructive native placement.

Full reference field modes preserve rows, corridors and negative space that a
conditional pixel average blurs. Only a statistical target is returned; no
source parts, source coordinates, slot graph or existing board is instantiated.
New levels still require from-empty native grammar, novelty and actual play.
"""
from pathlib import Path
import argparse
import copy
import json
import math
import sys

import joblib
import numpy as np

from composition_field_v3 import CHANNELS, H, W
from organization_model_v5 import OrganizationModel, atomic
from ordinary_source_filter_guard_v4 import preflight, sha


class SharpOrganizationModelV5(OrganizationModel):
    def sample(self, position, rng):
        target = super().sample(position, rng)
        tree = self.forest.estimators_[target["treeIndex"]]
        members = np.flatnonzero(self.leaves[:, target["treeIndex"]] == target["leafId"])
        index = int(rng.choice(members))
        vector = self.sharp_fields[index]
        maps = vector[:CHANNELS*H*W].reshape(CHANNELS, H, W).copy()
        for a, b in ((0, 4), (4, 8)):
            maps[a:b] /= max(maps[a:b].sum(), 1e-12)
        profile = self.profiles[index]
        context = profile[2:].copy()
        context[-1] = float(position)/500.
        target.update(field=maps, orientation=vector[CHANNELS*H*W:].reshape(4, 24)/.2,
            span=float(np.exp(profile[1])), targetParts=float(np.expm1(profile[0])),
            context=context.tolist(), nativeWidthUnit=float(self.width_units[index]),
            method="saved conditional empirical sharp whole-field mode; statistical target only, no geometry scaffold",
            organizationFieldSourceId=self.source_ids[index], organizationFieldRecordIndex=index,
            fieldTargetIsSourceBoard=False, sourcePartsOrCoordinatesReturned=False,
            fieldModeSemantics="An observed field supplies soft arrangement/space statistics, not a board to edit or a novelty certificate.")
        return target


def create(workspace):
    workspace = Path(workspace).resolve()
    source_filter, bindings = preflight(workspace)
    models = workspace/"models"
    model_path = models/"organization_sharp_v5.joblib"
    report_path = models/"organization_sharp_v5_evaluation.json"
    if model_path.exists() or report_path.exists():
        raise ValueError("Refuse to overwrite sharp organization artifacts")
    base_path = models/"organization_v5.joblib"
    cache_path = models/"organization_v5_native_fields.joblib"
    base = joblib.load(base_path)
    cached = joblib.load(cache_path)
    if cached["hashes"] != base.hashes:
        raise ValueError("Native whole-field cache differs from base fit inputs")
    allowed = set(map(str, source_filter["ordinarySourceIds"]))
    if set(base.source_ids) != allowed or len(cached["fields"]) != len(allowed):
        raise ValueError("Sharp prior contains excluded/missing source fields")
    final = SharpOrganizationModelV5()
    final.__dict__.update(copy.deepcopy(base.__dict__))
    final.sharp_fields = cached["fields"]
    final.sharp_mode_hashes = {"baseModel": sha(base_path), "nativeFieldCache": sha(cache_path),
        "trainer": sha(Path(__file__)), **bindings}
    joblib.dump(final, model_path, compress=3)
    old_evaluation = json.loads((models/"organization_v5_evaluation.json").read_text(encoding="utf8"))
    atomic(report_path, {"schemaVersion": 5, "status": "CONDITIONAL_EMPIRICAL_SHARP_FIELD_PRIOR_NO_GENERATION_CERTIFICATION",
        "sourceCount": len(allowed), "sourceIds": base.source_ids,
        "hashes": {**base.hashes, **final.sharp_mode_hashes, "model": sha(model_path)},
        "method": "Sample a conditional learned leaf then an actual whole-field mode within its source support; complete native assets are composed separately from an empty board.",
        "baseFamilySeparatedEvaluation": old_evaluation["summary"],
        "heldFamilySharpSamplingQuality": "NOT_MEASURED; base conditional mean field reconstruction is worse than linear baseline",
        "numericContentGatesApplied": [], "acceptedCount": 0,
        "limits": ["Empirical conditional sampling, not a neural generator or fresh source-independent field fit.",
            "Target field reuses a learned source arrangement style; it must never be delivered as a new level or copied root-by-root.",
            "Target count/span are soft profile features; exact source content is not a constructor quota.",
            "Whole candidate novelty, native replay, meaningful interactions, virtual-player difficulty and visual design remain required."]})
    print(json.dumps({"model": str(model_path), "sourceCount": len(allowed), "status": "READY_FOR_FROM_EMPTY_PROPOSAL_ONLY"}))


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--workspace", required=True, type=Path)
    args = p.parse_args()
    create(args.workspace)


if __name__ == "__main__":
    sys.modules.setdefault("organization_sharp_v5", sys.modules[__name__])
    SharpOrganizationModelV5.__module__ = "organization_sharp_v5"
    main()
