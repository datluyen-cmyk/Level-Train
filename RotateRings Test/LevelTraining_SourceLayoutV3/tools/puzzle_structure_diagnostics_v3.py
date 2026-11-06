"""Informational puzzle/scaffold comparison, independent of acceptance gates.

Matches typed native roots by position against the intended reference. Measures
directed pin-graph changes and whole-asset changes; it does not infer release
order or waive geometric near-copy rejection. Native replay remains necessary.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path

import numpy as np
from scipy.optimize import linear_sum_assignment

from assembly_integrity_audit_v3 import feature_identity
from source_physical_composition_v3 import composition_edges


def diagnose(candidate, reference, templates):
    cp, rp = candidate["parts"], reference["parts"]
    distance = np.asarray([[math.hypot(a["x"]-b["x"], a["y"]-b["y"]) +
                            (0 if a["kind"] == b["kind"] else 1e6) for b in cp] for a in rp])
    rows, cols = linear_sum_assignment(distance)
    mapping = {rp[i]["id"]: cp[j]["id"] for i, j in zip(rows, cols) if rp[i]["kind"] == cp[j]["kind"]}
    inverse = {new: old for old, new in mapping.items()}
    byc, byr = {p["id"]: p for p in cp}, {p["id"]: p for p in rp}
    before = {(e["source"], e["target"]) for e in reference["links"]}
    after = {(inverse.get(e["source"], ("new", e["source"])), inverse.get(e["target"], ("new", e["target"]))) for e in candidate["links"]}
    changed_branches = []
    changed_assets = []
    changed_orientations = []
    def native_geometry(p):
        return [feature_identity(s) for s in templates[p["templateId"]]["physicalShapes"]]
    for old, new in mapping.items():
        if {b for a,b in before if a == old} != {b for a,b in after if a == old}:
            changed_branches.append(new)
        if native_geometry(byr[old]) != native_geometry(byc[new]):
            changed_assets.append(new)
        if abs((byr[old]["angle"]-byc[new]["angle"]+180)%360-180) >= 1.:
            changed_orientations.append(new)
    def composition(level, remap=None):
        remap = remap or {}
        return {(remap.get(e["source"], ("new",e["source"])) if remap else e["source"],
                 remap.get(e["target"], ("new",e["target"])) if remap else e["target"], e["kind"])
                for e in composition_edges(level, templates) if e.get("strong")}
    physical_before, physical_after = composition(reference), composition(candidate, inverse)
    matches = len(mapping)
    return {
        "levelId": candidate["id"], "referenceSourceId": reference["sourceId"],
        "sourcePosition": reference["position"], "typedRootCoverage": matches/max(len(cp),len(rp)),
        "maximumRootCenterDistanceWorldUnits": max((distance[i,j] for i,j in zip(rows,cols) if rp[i]["kind"]==cp[j]["kind"]), default=0.),
        "referencePinEdges": len(before), "candidatePinEdges": len(after),
        "sharedDirectedPinEdges": len(before & after), "directedPinEdgeSymmetricDifference": len(before ^ after),
        "directedPinEdgeJaccard": len(before & after)/max(1,len(before | after)),
        "changedOutgoingBranchRootIds": sorted(changed_branches),
        "changedNativeAssetRootIds": sorted(changed_assets),
        "changedRootOrientationIds": sorted(changed_orientations),
        "sourceStrongCompositionRelations": len(physical_before),
        "candidateStrongCompositionRelations": len(physical_after),
        "sharedStrongCompositionRelations": len(physical_before & physical_after),
        "scope": "Informational native geometry and directed graph divergence at the intended reference scaffold. No novelty exemption, active lock/release order, solve, quality or difficulty claim."
    }


def main():
    parser = argparse.ArgumentParser()
    for name in ("pack", "sources", "templates", "report"):
        parser.add_argument("--"+name, type=Path, required=True)
    args = parser.parse_args()
    load = lambda path: json.loads(path.read_text(encoding="utf8"))
    sources = {l["position"]: l for l in load(args.sources)["levels"]}
    templates = {t["id"]:t for t in load(args.templates)["templates"]}
    rows = [diagnose(level, sources[level["position"]], templates) for level in load(args.pack)["levels"]]
    report = {"status": "INFORMATIONAL_NOT_AN_ACCEPTANCE_GATE", "levels": rows,
              "hashes": {str(p): hashlib.sha256(p.read_bytes()).hexdigest() for p in (args.pack,args.sources,args.templates,Path(__file__))}}
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps(report, indent=2), encoding="utf8")
    print(json.dumps({"levels":len(rows),"report":str(args.report)}))


if __name__ == "__main__":
    main()
