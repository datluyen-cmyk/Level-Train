"""Bounded no-scale joint native XY+angle pilot inside source30 diamond.

This conservative pilot holds contour units1/7–12 and peripheral rays. It is
not a universal immutable-contour policy or a learned diamond family classifier.
"""
from pathlib import Path
from collections import Counter
import argparse
import copy
import itertools
import json
import math
import time

import joblib
import numpy as np
from shapely import union_all

import guide_neighborhood_motifs_v3 as base
from guide_joint_pose_motifs_v3 import kabsch
from guide_joint_patch_contract_v3 import selected_context, JointGuidePatchAudit
from guide_neighborhood_pilot_v3 import payload_compact
from native_asset_variant_v3 import clone_root, candidate, world
from assembly_contact_audit_v3 import NativeGeometrySeamAudit, shape_polygon
from learned_placement_audit_v3 import NativeLearnedPlacementAudit
from layout_novelty_semantic_v3 import LayoutNoveltyIndex
from orientation_semantics_v3 import meaningful_edit
from layout_learning_v3 import read, hashfile
from composition_field_v3 import stroke_heading, native_frame, posed_field


def construct_joint_candidate(reference, context, record, index, templates, sources, local, local_model,
                              source_path, local_model_path, joint_model_path, identifier):
    ps = {p["id"]: p for p in reference["parts"]}
    rotation, translation, angle, fit_error = kabsch(record["nativeXYInBoardFrame"], context["nativeReferencePrimaryXY"])
    positions = np.asarray(record["nativeXYInBoardFrame"])@rotation+translation
    phis = np.asarray(record["pose"]).reshape(context["size"], 4)
    angles = np.degrees(np.arctan2(phis[:, 3], phis[:, 2]))+angle
    choices = {}
    for slot, (primary, group) in enumerate(zip(context["primaryIds"], context["unitIds"])):
        original_primary = ps[primary]
        delta = angles[slot]-original_primary["angle"]
        origin = np.array([original_primary["x"], original_primary["y"]])
        for i in group:
            old = ps[i]
            xy = positions[slot]+base.rotate([[old["x"]-origin[0], old["y"]-origin[1]]], delta)[0]
            pose = {**old, "x": float(xy[0]), "y": float(xy[1]), "angle": old["angle"]+delta}
            actual = clone_root(old, reference["sourceId"], i, pose)
            choices[i] = {"sourceId": reference["sourceId"], "donorId": i, "part": actual,
                          "edges": [], "dependencySymmetricDifference": 0, "oldTargets": [], "newTargets": [],
                          "bodyPreserved": False, "angleDeltaDegrees": float(delta),
                          "angleSource": "one saved whole4unit native XY+angle patch under one common rigid no-scale fit",
                          "conditionalPoseNLL": None}
    c = candidate(reference, {}, templates, sources, local, local_model,
                  hashfile(source_path), hashfile(local_model_path), identifier, 0, selected_override=choices)
    ap = c["assemblyProvenance"]
    ap["method"] = "Joint saved local native XY+angle patch; proper rigid fit, no coordinate jitter or scale; original complete guide unit assets"
    ap["retargetedAssetImports"] = [{"sourceId": reference["sourceId"], "partMapping": {i: i for i in choices}}]
    ap["learnedPlacementCertificate"]["guideRigidGroups"] = context["unitIds"]
    ap["jointGuidePatchCertificate"] = {"modelSha256": hashfile(joint_model_path), "sourcePackSha256": hashfile(source_path),
        "referenceSourceId": reference["sourceId"], "selectedPrimaryIds": sorted(context["primaryIds"]),
        "recipientContext": context["context"], "learnedRecordIndex": int(index), "donorSourceId": record["sourceId"],
        "rotationMatrixRowVector": rotation.tolist(), "translation": translation.tolist(), "scale": 1.,
        "rigidFitRMSNativeWorldUnits": fit_error,
        "limits": "Local patch support, not a learned diamond family or native lock/release certificate."}
    return c


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--workspace", required=True, type=Path)
    p.add_argument("--output", required=True, type=Path)
    p.add_argument("--max-records", type=int, default=160)
    p.add_argument("--limit", type=int, default=4)
    args = p.parse_args()
    if args.max_records > 160:
        raise ValueError("First declared pilot budget is at most160conditional records")
    w = args.workspace.resolve()
    source_path = w/"data/source500_native.json"
    with source_path.open() as stream:
        levels = json.load(stream, object_hook=payload_compact)["levels"]
    templates = {t["id"]: t for t in read(w/"data/source_templates.json")["templates"]}
    compact = {tid: base.compact_object(t) for tid, t in templates.items()}
    reference = next(l for l in levels if l["position"] == 30)
    if reference["sourceId"] != "10157":
        raise ValueError("Pilot source30changed")
    sources = {l["sourceId"]: l for l in levels}
    ps = {p["id"]: p for p in reference["parts"]}
    local = {}
    for tid in {p["templateId"] for p in reference["parts"]}:
        t = templates[tid]
        shapes = [shape_polygon(s) for s in t["physicalShapes"]]
        local[tid] = {"solid": union_all([g for s, g in zip(t["physicalShapes"], shapes) if not s["trigger"] and s.get("sourceEnabled", True) and s.get("sourceLayer", 7) == 7]),
                      "body": union_all([g for s, g in zip(t["physicalShapes"], shapes) if s["role"] == "body"]), "shapes": shapes}
    original_solids = {i: world(local[p["templateId"]]["solid"], p) for i, p in ps.items()}
    guide_for = {e["lockedOwnerId"]: e["guideOwnerId"] for e in reference["guideLinks"]}
    contour_pull_ids = {1, 7, 8, 9, 10, 11, 12}
    contour_ids = contour_pull_ids | {guide_for[i] for i in contour_pull_ids}
    hull = union_all([original_solids[i] for i in contour_ids]).convex_hull
    local_model_path = w/"models/composition_pose.joblib"
    local_model = joblib.load(local_model_path)["model"]
    joint_model_path = w/"models/guide_joint_pose_motifs.joblib"
    bundle = joblib.load(joint_model_path)
    if bundle["sourcePackSha256"] != hashfile(source_path) or bundle["templatePackSha256"] != hashfile(w/"data/source_templates.json"):
        raise ValueError("Joint model corpus hash mismatch")
    saved = bundle["models"][4]
    joint_audit = JointGuidePatchAudit(levels, templates, joint_model_path, source_path)
    native = NativeLearnedPlacementAudit(levels, templates, local_model_path, source_path)
    seam = NativeGeometrySeamAudit(templates, levels)
    extra = read(w/"data/novelty_extras/source_parity_pack.json")["levels"]
    nt = dict(templates)
    nt.update({t["id"]: t for t in read(w/"data/novelty_extras/source_templates.json")["templates"]})
    novelty = LayoutNoveltyIndex(levels+extra, nt)
    heading = stroke_heading(reference)
    _, _, span, center, matrix = native_frame(reference, templates, heading)
    world_center = center@matrix.T
    def field(level):
        shifted = {**level, "parts": [{**p, "x": p["x"]-world_center[0], "y": p["y"]-world_center[1]} for p in level["parts"]]}
        return posed_field(shifted, heading, span, len(ps), templates)[0]
    source_field = field(reference)
    rejections, out, attempted, records = Counter(), [], 0, []
    started = time.monotonic()
    for selected in itertools.combinations([2, 3, 4, 5, 6], 4):
        context = selected_context(reference, compact, selected)
        distances, indices = saved["model"].neighbors([context["context"]])
        for distance, index in zip(distances[0], indices[0]):
            if attempted >= args.max_records or len(out) >= args.limit:
                break
            attempted += 1
            record = saved["records"][int(index)]
            note = {"selectedPrimaryIds": selected, "recordIndex": int(index), "donorSourceId": record["sourceId"], "contextDistance": float(distance)}
            if record["sourceId"] == reference["sourceId"]:
                rejections["sameReferenceSourceNativePatchExcluded"] += 1
                continue
            lc = len(base.level_context(reference))
            donor_widths = [record["context"][lc+i*15+6] for i in range(4)]
            reference_widths = [context["context"][lc+i*15+6] for i in range(4)]
            if abs(record["width"]-context["width"]) > 1e-5 or any(abs(a-b) > 1e-5 for a, b in zip(donor_widths, reference_widths)):
                rejections["nativeUnitWidthMismatchNoScaleAllowed"] += 1
                continue
            c = construct_joint_candidate(reference, context, record, int(index), templates, sources, local, local_model,
                                          source_path, local_model_path, joint_model_path, 7963000+len(out))
            actuals = {p["id"]: p for p in c["parts"]}
            moved = {i for g in context["unitIds"] for i in g}
            if any(actuals[i][k] != ps[i][k] for i in set(ps)-moved for k in ("templateId", "x", "y", "angle")):
                raise AssertionError("Pilot contour/peripheral source framework changed")
            meaningful = [i for i in moved if meaningful_edit(ps[i], actuals[i], templates)]
            if not max(1, math.ceil(len(ps)*.2)) <= len(meaningful) <= max(1, math.ceil(len(ps)*.25)):
                rejections["meaningfulRootBudget"] += 1
                continue
            geos = {i: world(local[actuals[i]["templateId"]]["solid"], actuals[i]) for i in moved}
            if any(g.difference(hull.buffer(1e-5)).area > 1e-5 for g in geos.values()):
                rejections["pilotDiamondContourHullContainment"] += 1
                continue
            groups = {i: number for number, group in enumerate(context["unitIds"]) for i in group}
            if any(g.intersection(geos.get(j, original_solids[j])).area > 1e-5
                   for i, g in geos.items() for j in ps if j != i and not (j in moved and groups[i] == groups[j])):
                rejections["nativeWholeUnitSolidConflict"] += 1
                continue
            proof = joint_audit.check(c)
            if not proof["passed"]:
                raise AssertionError("Independent joint patch certificate rejected producer: "+str(proof))
            native_result, raw = native.check(c), seam.check(c)
            if not native_result["nativeLearnedPlacementIntegrityPassed"]:
                rejections["independentNativeAssetGuideIntegrity"] += 1
                continue
            if not raw["rawGeometrySeamPassed"]:
                rejections["independentRawNativeSolidsSensors"] += 1
                continue
            delta = float(np.abs(field(c)-source_field).sum())
            if delta > 2*len(meaningful)/len(ps):
                rejections["historicalSharedReferenceWholeFieldLimit"] += 1
                continue
            novel = novelty.check(c)
            if not novel["passed"]:
                rejections["semanticWholeLayoutNovelty507"] += 1
                continue
            c["preliminaryJointNativePatchIntegrity"] = proof
            c["preliminaryNativeAssetIntegrity"] = native_result
            c["preliminaryRawNativeGeometry"] = raw
            c["preliminarySemanticNovelty507"] = novel
            c["preliminaryWholeFigure"] = {"family": "user-confirmed diamond", "pilotPreservedContourPullIds": sorted(contour_pull_ids),
                "replacedInteriorUnitPrimaryIds": selected, "sharedReferenceFieldDelta": delta,
                "meaningfulRootIds": meaningful, "nativeObstructionReleaseOrderStatus": "PENDING_CURRENT_UNITY_PROOF",
                "salientFacetEndpointAndDiamondReadabilityStatus": "PENDING_VISUAL_DESIGN_REVIEW_NOT_A_GAME_RULE",
                "limits": "Pilot-specific exact contour hold; does not establish all source family contours are immutable. Local crosssemantic donor is not learned diamond-family data."}
            out.append(c)
            note["preliminaryCandidateId"] = c["id"]
            records.append(note)
            print("JOINT_XY_GUIDE_PILOT_CANDIDATE", c["id"], selected, record["sourceId"], delta, flush=True)
        if attempted >= args.max_records or len(out) >= args.limit:
            break
    result = {"schemaVersion": 3, "complete": False, "acceptedCount": 0,
              "status": "BOUNDED_JOINT_XY_GUIDE_PILOT_NO_NATIVE_PLAY_ORDER_OR_WHOLE_DIAMOND_DESIGN_CERTIFICATE",
              "trainingSourceCount": 500, "sourcePosition": 30, "maximumRecordsDeclared": args.max_records,
              "attemptedConditionalRecords": attempted, "levels": out, "rejections": dict(rejections),
              "records": records, "elapsedSeconds": time.monotonic()-started,
              "hashes": {"source": hashfile(source_path), "templates": hashfile(w/"data/source_templates.json"),
                         "jointModel": hashfile(joint_model_path), "jointContract": hashfile(Path(__file__).with_name("guide_joint_patch_contract_v3.py")),
                         "tool": hashfile(Path(__file__))}}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, separators=(",", ":")), encoding="utf8")
    print("JOINT_XY_GUIDE_PILOT_RESULT", len(out), attempted, dict(rejections), flush=True)


if __name__ == "__main__":
    main()
