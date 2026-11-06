"""Independent physical/input/native-state no-op screen across native templates.

Attachment ordinals are labels: compare their complete per-role world geometry
and source/target relation semantics instead of manufacturing geometric novelty
from a different serialized native template ID.
"""
from collections import defaultdict
from pathlib import Path
import argparse
import copy
import json
import math

import numpy as np
from shapely import union_all

from audit_complete_guide_unit_semantics_v4 import stable, axis
from assembly_contact_audit_v3 import shape_polygon
from native_asset_variant_v3 import world
from ordinary_source_filter_guard_v4 import sha


def groups(part, templates):
    result = defaultdict(list)
    for kind, shapes in (("physical", templates[part["templateId"]]["physicalShapes"]), ("input", part.get("inputShapes", []))):
        for shape in shapes:
            metadata = stable({k: v for k, v in shape.items() if k not in ("points", "paths", "attachmentId")})
            result[(kind, json.dumps(metadata, sort_keys=True, separators=(",", ":")))].append(shape_polygon(shape))
    return result


def relation_semantics(level):
    return sorted((e["source"], e["target"], e.get("relationKind", 0), e.get("activityKind", 0)) for e in level["links"])


def compare(reference, candidate, templates):
    old, new = ({p["id"]: p for p in l["parts"]} for l in (reference, candidate))
    ids = candidate["softNativePartPolicyV4"]["sourceRelativeRawMetrics"]["changedRootIds"]
    rows = []
    for i in ids:
        a, b = old[i], new[i]
        ga, gb = groups(a, templates), groups(b, templates)
        measurements = []
        keys_same = set(ga) == set(gb)
        for key in sorted(set(ga)&set(gb)):
            la, lb = union_all(ga[key]), union_all(gb[key])
            x, y = world(la, a), world(lb, b)
            scale = max(1., *(abs(v) for v in (*x.bounds, *y.bounds)))
            radius = max(1., *(abs(v) for v in (*la.bounds, *lb.bounds)))
            angle_scale = max(180., abs(a["angle"]), abs(b["angle"]))
            tolerance = 4*(float(np.spacing(np.float32(scale)))+radius*math.radians(float(np.spacing(np.float32(angle_scale)))))
            hd, area = float(x.hausdorff_distance(y)), float(x.symmetric_difference(y).area)
            area_bound = float((x.length+y.length)*tolerance+2*math.pi*tolerance*tolerance)
            measurements.append({"kind": key[0], "shapeContractIgnoringAttachmentOrdinal": json.loads(key[1]),
                "hausdorffDistance": hd, "symmetricDifferenceArea": area, "nativePrecisionHausdorffBound": tolerance,
                "nativePrecisionAreaBound": area_bound, "equalWithinNativePrecision": hd <= tolerance and area <= area_bound})
        state_same = all(stable(a.get(k)) == stable(b.get(k)) for k in ("kind", "z", "scoreEligible", "collectible", "chainLockCount", "key", "lockKey", "sourceRigidbody"))
        aa, bb = axis(a, templates[a["templateId"]]), axis(b, templates[b["templateId"]])
        movement_same = aa is None and bb is None
        if aa is not None and bb is not None:
            axis_bound = 4*math.radians(float(np.spacing(np.float32(max(180., abs(a["angle"]), abs(b["angle"]))))))
            movement_same = min(math.dist(aa, bb), math.dist(aa, [-bb[0], -bb[1]])) <= axis_bound
        rows.append({"rootId": i, "beforeTemplateId": a["templateId"], "afterTemplateId": b["templateId"],
            "sameRoleInputPhysicalShapeContractsIgnoringAttachmentOrdinal": keys_same,
            "physicalInputGeometryEqualWithinNativePrecision": keys_same and all(r["equalWithinNativePrecision"] for r in measurements),
            "nativeStateRigidbodyContractUnchanged": state_same, "unorientedMovementEquivalent": movement_same, "perRole": measurements})
    relation_same = relation_semantics(reference) == relation_semantics(candidate) and reference["guideLinks"] == candidate["guideLinks"]
    no_op = relation_same and all(r["physicalInputGeometryEqualWithinNativePrecision"] and r["nativeStateRigidbodyContractUnchanged"] and r["unorientedMovementEquivalent"] for r in rows)
    return {"candidateId": candidate["id"], "sourcePosition": reference["position"], "nativeInitialSemanticNoOpWithinPrecision": no_op,
        "authoredRelationOwnerTypeSemanticsUnchanged": relation_same, "roots": rows, "accepted": False,
        "limits": "Initial world physical/input native flags, state/RB and relation-owner/type equivalence; not stateful native difficulty or human-player quality."}


def main():
    p = argparse.ArgumentParser()
    for name in ("pack", "sources", "templates", "output"):
        p.add_argument("--"+name, type=Path, required=True)
    p.add_argument("--cleaned-output", type=Path)
    args = p.parse_args()
    read = lambda path: json.loads(path.read_text(encoding="utf8"))
    pack = read(args.pack)
    sources = {l["position"]: l for l in read(args.sources)["levels"]}
    templates = {t["id"]: t for t in read(args.templates)["templates"]}
    rows = [compare(sources[c["position"]], c, templates) for c in pack["levels"]]
    no_ops = {r["candidateId"] for r in rows if r["nativeInitialSemanticNoOpWithinPrecision"]}
    report = {"status": "WHOLE_NATIVE_PART_INDEPENDENT_PRECISION_SCREEN", "acceptedCount": 0, "candidateCount": len(rows),
        "semanticNoOpCandidateIds": sorted(no_ops), "remainingPreliminaryCount": len(rows)-len(no_ops), "levels": rows,
        "precisionPolicy": "Four propagated float32 operation ULPs in coordinate and angular transform radius; no design-minimum gate.",
        "hashes": {name: sha(path) for name, path in (("pack", args.pack), ("sources", args.sources), ("templates", args.templates), ("tool", Path(__file__)))}}
    args.output.write_text(json.dumps(report, indent=2), encoding="utf8")
    if args.cleaned_output:
        if args.cleaned_output.exists():
            raise ValueError("Refuse overwrite existing screened pack")
        screened = copy.deepcopy(pack)
        screened["levels"] = [c for c in pack["levels"] if c["id"] not in no_ops]
        screened["independentPrecisionScreen"] = {"reportSha256": sha(args.output), "reportPath": str(args.output.resolve()),
            "inputPackSha256": sha(args.pack), "excludedNativeSemanticNoOpIds": sorted(no_ops), "acceptedCount": 0}
        screened["status"] = "PRECISION_SCREENED_PRELIMINARY_NATIVE_PARTS_PENDING_VISUAL_CURRENT_REPLAY_DIFFICULTY"
        args.cleaned_output.write_text(json.dumps(screened, separators=(",", ":")), encoding="utf8")
    print("NATIVE_PART_PRECISION", len(rows), "noops", sorted(no_ops), "remaining", len(rows)-len(no_ops), flush=True)


if __name__ == "__main__":
    main()
