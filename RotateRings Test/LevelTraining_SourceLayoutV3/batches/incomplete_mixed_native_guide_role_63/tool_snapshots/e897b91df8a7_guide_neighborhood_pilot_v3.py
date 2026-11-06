"""Strict incomplete pilot: joint saved guide-neighborhood angular motifs.

Whole original guide-connected units rotate rigidly at their Pull centers.
All gates remain preliminary, and zero candidates is an ordinary valid result.
"""
from collections import Counter
from pathlib import Path
import argparse
import copy
import json
import math
import time

import joblib
import numpy as np
from shapely import affinity, union_all

import guide_neighborhood_motifs_v3 as motif
from assembly_contact_audit_v3 import NativeGeometrySeamAudit, shape_polygon
from composition_field_v3 import native_frame, posed_field, stroke_heading
from layout_learning_v3 import hashfile, read
from layout_novelty_v3 import LayoutNoveltyIndex
from learned_placement_audit_v3 import NativeLearnedPlacementAudit
from native_asset_variant_v3 import candidate, clone_root, world


def payload_compact(value):
    # These diagnostic raw serialization fields are stripped by clone_root as
    # well. Input, rigidbody, state, shapes, native template visuals and binding
    # contracts remain intact for the independent production audits.
    if "templateId" in value and "kind" in value and "id" in value:
        for key in ("sourceNativeData", "sourceRigidbodyNativeData", "sourcePartData"):
            value.pop(key, None)
    return value


def static_pull_sweep_proxy(level, templates, local, distance):
    """Convex native solid translation sweep: composition evidence, not motion."""
    pulls = [p for p in level["parts"] if p["kind"] == "slider"]
    solids = {p["id"]: world(local[p["templateId"]]["solid"], p) for p in pulls}
    edges = []
    for source in pulls:
        axis = templates[source["templateId"]].get("movement", {}).get("axisLocal", {"x": -1., "y": 0.})
        a = math.radians(source["angle"])
        vector = np.array([axis["x"]*math.cos(a)-axis["y"]*math.sin(a),
                           axis["x"]*math.sin(a)+axis["y"]*math.cos(a)])
        vector /= max(1e-12, np.linalg.norm(vector))
        solid = solids[source["id"]]
        for direction in (-1, 1):
            delta = vector*distance*direction
            sweep = union_all([solid, affinity.translate(solid, *delta)]).convex_hull
            for target in pulls:
                if target["id"] != source["id"] and sweep.intersection(solids[target["id"]]).area > 1e-5:
                    edges.append([source["id"], target["id"], direction])
    return edges


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True, type=Path)
    parser.add_argument("--position", type=int, default=21)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--limit", type=int, default=4)
    args = parser.parse_args()
    w = args.workspace.resolve()
    args.output.parent.mkdir(parents=True, exist_ok=True)
    source_path = w/"data/source500_native.json"
    with source_path.open(encoding="utf8") as stream:
        levels = json.load(stream, object_hook=payload_compact)["levels"]
    templates = {t["id"]: t for t in read(w/"data/source_templates.json")["templates"]}
    compact_templates = {tid: motif.compact_object(t) for tid, t in templates.items()}
    sources = {l["sourceId"]: l for l in levels}
    reference = next(l for l in levels if l["position"] == args.position)
    ps = {p["id"]: p for p in reference["parts"]}
    model_path = w/"models/guide_neighborhood_motifs.joblib"
    bundle = joblib.load(model_path)
    if bundle["sourcePackSha256"] != hashfile(source_path):
        raise ValueError("Guide motif source hash mismatch")
    if bundle["templatePackSha256"] != hashfile(w/"data/source_templates.json"):
        raise ValueError("Guide motif template hash mismatch")
    local = {}
    for tid in {p["templateId"] for p in reference["parts"]}:
        shapes = [shape_polygon(s) for s in templates[tid]["physicalShapes"]]
        local[tid] = {"solid": union_all([g for s, g in zip(templates[tid]["physicalShapes"], shapes)
                                         if not s["trigger"] and s.get("sourceEnabled", True) and s.get("sourceLayer", 7) == 7]),
                      "body": union_all([g for s, g in zip(templates[tid]["physicalShapes"], shapes) if s["role"] == "body"]),
                      "shapes": shapes}
    old_solids = {i: world(local[p["templateId"]]["solid"], p) for i, p in ps.items()}
    local_model_path = w/"models/composition_pose.joblib"
    local_model = joblib.load(local_model_path)["model"]
    seam = NativeGeometrySeamAudit(templates, levels)
    native = NativeLearnedPlacementAudit(levels, templates, local_model_path, source_path)
    extra = read(w/"data/novelty_extras/source_parity_pack.json")["levels"]
    nt = dict(templates)
    nt.update({t["id"]: t for t in read(w/"data/novelty_extras/source_templates.json")["templates"]})
    novelty = LayoutNoveltyIndex(levels+extra, nt)
    heading = stroke_heading(reference)
    _, _, span, center, matrix = native_frame(reference, templates, heading)
    world_center = center@matrix.T
    def fixed_field(level):
        shifted = {**level, "parts": [{**p, "x": p["x"]-world_center[0], "y": p["y"]-world_center[1]} for p in level["parts"]]}
        return posed_field(shifted, heading, span, len(ps), templates)[0]
    source_field = fixed_field(reference)
    source_proxy = static_pull_sweep_proxy(reference, templates, local, span*2)
    source_proxy_set = {tuple(e) for e in source_proxy}
    rejections = Counter()
    out = []
    seen = set()
    tried = 0
    started = time.monotonic()
    for size, saved in sorted(bundle["models"].items()):
        model = saved["model"]
        for context in motif.motif_records(reference, compact_templates, size):
            distances, neighbors = model.neighbors([context["context"]])
            for distance, record_index in zip(distances[0], neighbors[0]):
                record = saved["records"][int(record_index)]
                tried += 1
                if record["sourceId"] == reference["sourceId"]:
                    rejections["sameReferenceSourceMotifExcluded"] += 1
                    continue
                phi = np.asarray(record["pose"]).reshape(size, 2)
                angles = np.degrees(np.arctan2(phi[:, 1], phi[:, 0]))+context["heading"]
                changes = []
                for group, primary_id, angle in zip(context["unitIds"], context["primaryIds"], angles):
                    delta = (angle-ps[primary_id]["angle"]+180)%360-180
                    axis_delta = abs((delta+90)%180-90)
                    if axis_delta >= 45.-1e-5:
                        changes.append((group, primary_id, float(delta)))
                    elif abs(delta) > 1e-4:
                        # Do not fill changed-root quota with180degree body-
                        # equivalent Pull flips or small cosmetic angle changes.
                        changes = []
                        rejections["nonMeaningfulAxisChangeInJointMotif"] += 1
                        break
                moved_ids = {i for group, _, _ in changes for i in group}
                fraction = len(moved_ids)/len(ps)
                if not .2 <= fraction <= .25:
                    rejections["wholeMotifChangedRootFraction"] += 1
                    continue
                signature = tuple((primary, round(delta, 4)) for _, primary, delta in changes)
                if signature in seen:
                    rejections["duplicateJointNativeMotif"] += 1
                    continue
                seen.add(signature)
                moved = {}
                group_for = {}
                for number, (group, primary_id, delta) in enumerate(changes):
                    origin = np.array([ps[primary_id]["x"], ps[primary_id]["y"]])
                    for i in group:
                        old = ps[i]
                        xy = origin+motif.rotate([[old["x"]-origin[0], old["y"]-origin[1]]], delta)[0]
                        pose = {**old, "x": float(xy[0]), "y": float(xy[1]), "angle": old["angle"]+delta}
                        moved[i] = clone_root(old, reference["sourceId"], i, pose)
                        group_for[i] = number
                actual = {**ps, **moved}
                solids = {i: world(local[p["templateId"]]["solid"], p) for i, p in moved.items()}
                invalid = False
                for i, solid in solids.items():
                    for j, other in actual.items():
                        if j == i or (j in moved and group_for[i] == group_for[j]):
                            continue
                        target = solids[j] if j in moved else old_solids[j]
                        if solid.intersection(target).area > 1e-5:
                            invalid = True
                            break
                    if invalid:
                        break
                if invalid:
                    rejections["wholeJointMotifNativeSolidConflict"] += 1
                    continue
                root_choices = {}
                for i, part in moved.items():
                    edges = [{"edge": copy.deepcopy(e), "sourcePatternSourceId": reference["sourceId"],
                              "sourcePatternEdgeIndex": index, "targetOriginalId": e["target"]}
                             for index, e in enumerate(reference["links"]) if e["source"] == i]
                    targets = sorted(r["edge"]["target"] for r in edges)
                    root_choices[i] = {"sourceId": reference["sourceId"], "donorId": i, "part": part,
                                       "edges": edges, "dependencySymmetricDifference": 0,
                                       "oldTargets": targets, "newTargets": targets, "bodyPreserved": False,
                                       "angleDeltaDegrees": part["angle"]-ps[i]["angle"],
                                       "angleSource": "saved joint complete-guide-neighborhood native motif", "conditionalPoseNLL": None}
                try:
                    c = candidate(reference, {}, templates, sources, local, local_model,
                                  hashfile(source_path), hashfile(local_model_path), 7950000+args.position*100+len(out), 0,
                                  selected_override=root_choices)
                except ValueError:
                    rejections["nativePinContactConstruction"] += 1
                    continue
                ap = c["assemblyProvenance"]
                ap["method"] = "Saved joint native guide-neighborhood motif angles; complete rigid source guide units at fixed Pull centers"
                ap["retargetedAssetImports"] = [{"sourceId": reference["sourceId"], "partMapping": {i: i for i in moved_ids}}]
                ap["learnedPlacementCertificate"]["guideRigidGroups"] = [group for group, _, _ in changes]
                ap["guideNeighborhoodCertificate"] = {"modelSha256": hashfile(model_path), "size": size,
                    "nativeRecordIndex": int(record_index), "donorSourceId": record["sourceId"],
                    "recipientContext": context, "conditionalNeighborDistance": float(distance),
                    "jointNativeCosSinAngles": record["pose"], "changedUnitTransforms": changes,
                    "changedRootFraction": fraction, "minimumAxisChangeModulo180Degrees": 45.,
                    "limits": "Joint saved native angle pattern proposal; conditional support is not motion/difficulty evidence."}
                field_delta = float(np.abs(fixed_field(c)-source_field).sum())
                if field_delta > 2*fraction:
                    rejections["historicalSharedSourceFieldDelta"] += 1
                    continue
                native_result = native.check(c)
                raw = seam.check(c)
                if not native_result["nativeLearnedPlacementIntegrityPassed"]:
                    rejections["independentNativeGuideUnitIntegrity"] += 1
                    continue
                if not raw["rawGeometrySeamPassed"]:
                    rejections["independentRawGeometrySensors"] += 1
                    continue
                novel = novelty.check(c)
                if not novel["passed"]:
                    rejections["unchangedWholeLayoutNovelty507"] += 1
                    continue
                proxy = static_pull_sweep_proxy(c, templates, local, span*2)
                difference = len({tuple(e) for e in proxy} ^ source_proxy_set)
                if not difference:
                    rejections["noChangedStaticPullSweepProxyRelation"] += 1
                    continue
                c["preliminaryNativeIntegrity"] = native_result
                c["preliminaryRawGeometry"] = raw
                c["preliminaryNovelty507"] = novel
                c["preliminaryComposition"] = {"sharedReferenceFieldL1Delta": field_delta,
                    "historicalLimit2TimesChangedRootFraction": 2*fraction,
                    "sourceStaticPullSweepRelationCount": len(source_proxy), "staticPullSweepRelationCount": len(proxy),
                    "staticPullSweepRelationSymmetricDifference": difference,
                    "staticPullSweepProxy": proxy, "sweepDistance": span*2,
                    "limits": "Convex swept native Pull solid against other Pull solids; composition-only finite translation proxy, excludes fixtures and runtime masks. No lock/solvability/difficulty label."}
                out.append(c)
                print("GUIDE_JOINT_MOTIF_CANDIDATE", c["id"], args.position, len(moved_ids), field_delta, difference, flush=True)
                if len(out) >= args.limit:
                    break
            if len(out) >= args.limit:
                break
        if len(out) >= args.limit:
            break
    report = {"schemaVersion": 3, "complete": False, "acceptedCount": 0,
              "status": "INCOMPLETE_JOINT_GUIDE_MOTIF_PILOT_NATIVE_REPLAY_DIFFICULTY_VISUAL_PENDING",
              "sourcePosition": args.position, "trainingSourceCount": 500,
              "levels": out, "triedConditionalRecords": tried, "rejections": dict(rejections),
              "elapsedSeconds": time.monotonic()-started,
              "declaredCriteria": {"changedRootFraction": [.2, .25], "minimumAxisChangeModulo180Degrees": 45,
                  "jointWholeNativeRecordOnly": True, "excludeReferenceSourceDonor": True,
                  "sourceFieldDeltaLimit": "2*changedRootFraction", "noveltyExclusions": 507},
              "hashes": {"sourcePack": hashfile(source_path), "templates": hashfile(w/"data/source_templates.json"),
                         "motifModel": hashfile(model_path), "tool": hashfile(Path(__file__))}}
    args.output.write_text(json.dumps(report, separators=(",", ":")), encoding="utf8")
    print("GUIDE_JOINT_MOTIF_RESULT", len(out), "accepted", 0, dict(rejections), flush=True)


if __name__ == "__main__":
    main()
