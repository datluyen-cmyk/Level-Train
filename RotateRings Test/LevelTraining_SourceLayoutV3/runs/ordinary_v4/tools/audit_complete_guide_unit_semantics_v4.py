"""Independent role-aware world geometry audit with float32 precision bounds.

Spatial precision tolerance describes serialized/native arithmetic, never a
level-design minimum. This diagnostic does not certify gameplay equivalence.
"""
from pathlib import Path
from collections import defaultdict
import argparse
import json
import math

import numpy as np
from shapely import union_all

from assembly_contact_audit_v3 import shape_polygon
from native_asset_variant_v3 import world
from ordinary_source_filter_guard_v4 import sha


def stable(value):
    if isinstance(value, dict):
        return {k: stable(v) for k, v in value.items() if k not in ("sourceColliderId", "sourceComponentId")}
    if isinstance(value, list):
        return [stable(v) for v in value]
    return value


def axis(part, template):
    movement = template.get("movement")
    if not movement:
        return None
    v = movement["axisLocal"]
    radians = math.radians(part["angle"])
    x, y = v["x"]*math.cos(radians)-v["y"]*math.sin(radians), v["x"]*math.sin(radians)+v["y"]*math.cos(radians)
    length = math.hypot(x, y)
    return [x/length, y/length]


def compare_root(before, after, templates):
    t = templates[before["templateId"]]
    if after["templateId"] != before["templateId"]:
        return {"rootId": before["id"], "status": "DIFFERENT_NATIVE_TEMPLATE_NOT_AN_EQUALITY_PROOF", "physicalInputGeometryEqualWithinNativePrecision": False}
    groups = defaultdict(list)
    for kind, shapes in (("physical", t["physicalShapes"]), ("input", before.get("inputShapes", []))):
        for shape in shapes:
            metadata = stable({k: v for k, v in shape.items() if k not in ("points", "paths")})
            groups[(kind, json.dumps(metadata, sort_keys=True, separators=(",", ":")))].append(shape)
    measurements = []
    for (kind, metadata), shapes in groups.items():
        local = union_all([shape_polygon(s) for s in shapes])
        a, b = world(local, before), world(local, after)
        scale = max(1., *(abs(v) for v in (*a.bounds, *b.bounds)))
        local_radius = max(1., *(abs(v) for v in local.bounds))
        angle_scale = max(180., abs(before["angle"]), abs(after["angle"]))
        coordinate_ulp = float(np.spacing(np.float32(scale)))
        angle_ulp = float(np.spacing(np.float32(angle_scale)))
        # Four float32 operation-rounding units for serialized position,
        # rotation conversion and transformed polygon coordinates.
        tolerance = 4*(coordinate_ulp+local_radius*math.radians(angle_ulp))
        distance = float(a.hausdorff_distance(b))
        difference = float(a.symmetric_difference(b).area)
        area_bound = float((a.length+b.length)*tolerance+2*math.pi*tolerance*tolerance)
        measurements.append({"kind": kind, "shapeContract": json.loads(metadata), "shapeCount": len(shapes),
            "symmetricDifferenceArea": difference, "hausdorffDistance": distance,
            "coordinateFloat32Ulp": coordinate_ulp, "angleFloat32UlpDegrees": angle_ulp,
            "precisionHausdorffBound": tolerance, "precisionAreaBound": area_bound,
            "geometryEqualWithinNativePrecision": distance <= tolerance and difference <= area_bound})
    state_keys = ("kind", "z", "scoreEligible", "collectible", "chainLockCount", "key", "lockKey", "sourceRigidbody", "sourceColliderBindings", "inputShapes")
    state_same = all(stable(before.get(k)) == stable(after.get(k)) for k in state_keys)
    aa, bb = axis(before, t), axis(after, t)
    directed_error = math.dist(aa, bb) if aa else 0.
    undirected_error = min(math.dist(aa, bb), math.dist(aa, [-bb[0], -bb[1]])) if aa else 0.
    axis_bound = 4*math.radians(float(np.spacing(np.float32(max(180., abs(before["angle"]), abs(after["angle"]))))))
    return {"rootId": before["id"], "kind": before["kind"], "nativeTemplateUnchanged": True,
        "nonposeNativeInputRigidbodyStateContractUnchanged": state_same,
        "movement": {"sourceTemplateMovement": t.get("movement"), "beforeWorldAxis": aa, "afterWorldAxis": bb,
            "directedAxisError": directed_error, "unorientedAxisError": undirected_error, "nativePrecisionAxisBound": axis_bound,
            "unorientedSlideAxisEquivalent": undirected_error <= axis_bound,
            "directedSlideAxisEquivalent": directed_error <= axis_bound},
        "physicalInputGeometryEqualWithinNativePrecision": all(r["geometryEqualWithinNativePrecision"] for r in measurements),
        "perNativeRole": measurements}


def audit(reference, candidate, templates):
    old, new = ({p["id"]: p for p in l["parts"]} for l in (reference, candidate))
    ids = candidate["assemblyProvenance"]["learnedPlacementCertificate"]["guideRigidGroups"][0]
    roots = [compare_root(old[i], new[i], templates) for i in ids]
    links = lambda level, name: [e for e in level[name] if any(e.get(k) in ids for k in ("source", "target", "guideOwnerId", "lockedOwnerId"))]
    guide_same = links(reference, "guideLinks") == links(candidate, "guideLinks")
    pin_same = links(reference, "links") == links(candidate, "links")
    geometry_same = all(r["physicalInputGeometryEqualWithinNativePrecision"] for r in roots)
    state_same = all(r.get("nonposeNativeInputRigidbodyStateContractUnchanged", False) for r in roots)
    movement_same = all(r.get("movement", {}).get("unorientedSlideAxisEquivalent", False) for r in roots)
    return {"candidateId": candidate["id"], "sourcePosition": reference["position"], "referenceSourceId": reference["sourceId"], "unitRootIds": ids,
        "completeUnitPhysicalInputGeometryNoOp": geometry_same, "nativeStateInputRigidbodyContractUnchanged": state_same,
        "authoredGuideRelationContractUnchanged": guide_same, "authoredPinRelationContractUnchanged": pin_same,
        "unorientedMovementAxisEquivalent": movement_same,
        "completeUnitInitialPhysicalInputMovementContractNoOp": geometry_same and state_same and guide_same and pin_same and movement_same,
        "rootGeometryCosmeticWithinNativePrecision": [r["rootId"] for r in roots if r["physicalInputGeometryEqualWithinNativePrecision"]],
        "roots": roots, "accepted": False,
        "limits": "Initial native role/input geometry, unchanged native state/RB/relation contracts and bidirectional movement-axis reparameterization only; not stateful native release order, difficulty or player-label equivalence."}


def main():
    p = argparse.ArgumentParser()
    for name in ("pack", "sources", "templates", "output"):
        p.add_argument("--"+name, required=True, type=Path)
    args = p.parse_args()
    read = lambda path: json.loads(path.read_text(encoding="utf8"))
    sources = {l["position"]: l for l in read(args.sources)["levels"]}
    templates = {t["id"]: t for t in read(args.templates)["templates"]}
    rows = [audit(sources[c["position"]], c, templates) for c in read(args.pack)["levels"]]
    result = {"status": "COMPLETE_GUIDE_UNIT_NATIVE_ROLE_PRECISION_AUDIT", "acceptedCount": 0, "numericContentGatesApplied": [],
        "precisionPolicy": "Four float32 arithmetic ULPs propagated through serialized world coordinates and angular transform radius; not a level-design change minimum.",
        "candidateCount": len(rows), "wholeUnitInitialNoOpCandidateIds": [r["candidateId"] for r in rows if r["completeUnitInitialPhysicalInputMovementContractNoOp"]],
        "levels": rows, "hashes": {name: sha(path) for name, path in (("pack", args.pack), ("sources", args.sources), ("templates", args.templates), ("tool", Path(__file__)))}}
    args.output.write_text(json.dumps(result, indent=2), encoding="utf8")
    print("COMPLETE_GUIDE_UNIT_PRECISION_AUDIT", len(rows), result["wholeUnitInitialNoOpCandidateIds"], flush=True)


if __name__ == "__main__":
    main()
