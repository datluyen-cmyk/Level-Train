"""Bounded mixed native guide-angle + ordinary native-asset pilot.

Uses frozen ordinary option subsets with current independent native/raw/semantic
checks. No stage writer modules or candidate acceptance states are modified.
"""
from collections import Counter
from pathlib import Path
import argparse
import copy
import itertools
import json
import math
import time

import joblib
from shapely import union_all

import guide_neighborhood_motifs_v3 as base
from guide_anchor_role_motifs_v3 import pair_context
from guide_anchor_patch_contract_v3 import AnchoredGuidePatchAudit
from guide_neighborhood_pilot_v3 import payload_compact
from native_asset_variant_v3 import clone_root, candidate, world
from assembly_contact_audit_v3 import NativeGeometrySeamAudit, shape_polygon
from learned_placement_audit_v3 import NativeLearnedPlacementAudit
from orientation_semantics_v3 import meaningful_edit
from layout_novelty_semantic_v3 import LayoutNoveltyIndex
from layout_learning_v3 import read, hashfile
from audit_authored_topology_freedom_v3 import graph_summary, graph_delta


def guide_choice(reference, context, record):
    ps = {p["id"]: p for p in reference["parts"]}
    anchor, other = ps[context["anchorPrimaryId"]], ps[context["otherPrimaryId"]]
    desired = anchor["angle"]+math.degrees(math.atan2(record["pose"][1], record["pose"][0]))
    delta = desired-other["angle"]
    out = {}
    for i in context["otherUnitIds"]:
        old = ps[i]
        xy = base.rotate([[old["x"]-other["x"], old["y"]-other["y"]]], delta)[0]
        pose = {**old, "x": float(other["x"]+xy[0]), "y": float(other["y"]+xy[1]), "angle": old["angle"]+delta}
        out[i] = {"sourceId": reference["sourceId"], "donorId": i, "part": clone_root(old, reference["sourceId"], i, pose),
                  "edges": [], "dependencySymmetricDifference": 0, "oldTargets": [], "newTargets": [], "bodyPreserved": False,
                  "angleDeltaDegrees": delta, "angleSource": "One saved complete native pair relative angle, complete anchor unit held and recipient Pull centers fixed", "conditionalPoseNLL": None}
    return out


def ordinary_solutions(reference, opts, preset, templates, local, budget, max_solutions=8):
    """Small finite supplement search; same pair/contact/density constraints."""
    ps = {p["id"]: p for p in reference["parts"]}
    minimum, preferred = math.ceil(len(ps)*.2), math.ceil(len(ps)*.25)
    moved = {i for i, c in preset.items() if meaningful_edit(ps[i], c["part"], templates)}
    ids = sorted(opts)
    geometries = {}
    def data(c):
        p = c["part"]
        key = id(c)
        if key not in geometries:
            t = templates[p["templateId"]]
            geometries[key] = {"solid": world(local[p["templateId"]]["solid"], p), "body": world(local[p["templateId"]]["body"], p),
                               "heads": {j: world(g, p) for j, g in enumerate(local[p["templateId"]]["shapes"]) if t["physicalShapes"][j]["role"] == "head_trigger"}}
        return geometries[key]
    preset_solids = {i: data(c)["solid"] for i, c in preset.items()}
    filtered = {i: [c for c in pool if meaningful_edit(ps[i], c["part"], templates, [e["target"] for e in reference["links"] if e["source"] == i], c["newTargets"])
                     and all(data(c)["solid"].intersection(g).area <= 1e-5 for g in preset_solids.values())] for i, pool in opts.items()}
    for count in range(min(len(ids), preferred-len(moved)), max(0, minimum-len(moved))-1, -1):
        for selected in itertools.combinations(ids, count):
            if any(not filtered[i] for i in selected):
                continue
            fixed_count = sum(e["source"] not in selected for e in reference["links"])
            found = 0
            for choices in itertools.product(*(filtered[i] for i in selected)):
                if budget["nodes"] >= budget["maximum"]:
                    return
                budget["nodes"] += 1
                if not .9*len(reference["links"]) <= fixed_count+sum(len(c["edges"]) for c in choices) <= 1.1*len(reference["links"]):
                    budget["rejections"]["sourceRelativePinDensity"] += 1
                    continue
                if not any(c["dependencySymmetricDifference"] for c in choices):
                    budget["rejections"]["noChangedOrdinaryActiveBranch"] += 1
                    continue
                compatible = True
                for a, b in itertools.combinations(choices, 2):
                    ga, gb = data(a), data(b)
                    if ga["solid"].intersection(gb["solid"]).area > 1e-5:
                        compatible = False
                        break
                    if a["sourceId"] == b["sourceId"]:
                        ma = {a["donorId"]: a["part"]["id"], **{e["targetOriginalId"]: e["edge"]["target"] for e in a["edges"]}}
                        mb = {b["donorId"]: b["part"]["id"], **{e["targetOriginalId"]: e["edge"]["target"] for e in b["edges"]}}
                        if any(ma[k] != mb[k] for k in ma.keys()&mb.keys()):
                            compatible = False
                            break
                    for src, dst, other in ((a, b, b["part"]["id"]), (b, a, a["part"]["id"])):
                        gs, gd = data(src), data(dst)
                        edges = [e["edge"] for e in src["edges"] if e["edge"]["target"] == other]
                        if any(union_all([gs["heads"][i] for i in edge["sourceShapeIndices"] if i in gs["heads"]]).distance(gd["body"]) > 1e-5 for edge in edges):
                            compatible = False
                        if not edges and any(head.intersection(gd["solid"]).area > 1e-5 for head in gs["heads"].values()):
                            compatible = False
                if not compatible:
                    budget["rejections"]["jointOrdinaryNativeGeometryContact"] += 1
                    continue
                yield {**preset, **dict(zip(selected, choices))}
                found += 1
                if found >= max_solutions:
                    return


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True, type=Path)
    parser.add_argument("--position", type=int, default=126)
    parser.add_argument("--ordinary-options", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--max-guide-records", type=int, default=160)
    parser.add_argument("--max-ordinary-nodes", type=int, default=20000)
    parser.add_argument("--limit", type=int, default=3)
    args = parser.parse_args()
    w = args.workspace.resolve()
    with (w/"data/source500_native.json").open() as stream:
        levels = json.load(stream, object_hook=payload_compact)["levels"]
    templates = {t["id"]: t for t in read(w/"data/source_templates.json")["templates"]}
    sources = {l["sourceId"]: l for l in levels}
    reference = next(l for l in levels if l["position"] == args.position)
    ps = {p["id"]: p for p in reference["parts"]}
    cached = read(args.ordinary_options)
    if cached["sourcePackSha256"] != hashfile(w/"data/source500_native.json"):
        raise ValueError("Frozen option corpus hash differs")
    opts = {int(i): pool for i, pool in cached["options"].items()}
    tids = {p["templateId"] for p in ps.values()}|{c["part"]["templateId"] for pool in opts.values() for c in pool}
    local = {}
    for tid in tids:
        t = templates[tid]
        shapes = [shape_polygon(s) for s in t["physicalShapes"]]
        local[tid] = {"solid": union_all([g for s, g in zip(t["physicalShapes"], shapes) if not s["trigger"] and s.get("sourceEnabled", True) and s.get("sourceLayer", 7) == 7]),
                      "body": union_all([g for s, g in zip(t["physicalShapes"], shapes) if s["role"] == "body"]), "shapes": shapes}
    compact_templates = {tid: base.compact_object(t) for tid, t in templates.items()}
    units = base.units(reference, compact_templates)
    anchor_model_path = w/"models/guide_anchor_role_motifs.joblib"
    bundle = joblib.load(anchor_model_path)
    modelpath = w/"models/composition_pose.joblib"
    model = joblib.load(modelpath)["model"]
    anchor_audit = AnchoredGuidePatchAudit(levels, templates, w/"data/source500_native.json", anchor_model_path)
    native = NativeLearnedPlacementAudit(levels, templates, modelpath, w/"data/source500_native.json")
    raw = NativeGeometrySeamAudit(templates, levels)
    extra = read(w/"data/novelty_extras/source_parity_pack.json")["levels"]
    nt = dict(templates)
    nt.update({t["id"]: t for t in read(w/"data/novelty_extras/source_templates.json")["templates"]})
    novelty = LayoutNoveltyIndex(levels+extra, nt)
    original_solids = {i: world(local[p["templateId"]]["solid"], p) for i, p in ps.items()}
    original_graph = graph_summary(reference, True)
    reject, candidates = Counter(), []
    budget = {"nodes": 0, "maximum": args.max_ordinary_nodes, "rejections": Counter()}
    attempted, started = 0, time.monotonic()
    # Near guide neighbors are source-native local pairs, not arbitrary pairings.
    for other in units:
        anchors = sorted([u for u in units if u["primary"] != other["primary"]], key=lambda u: (math.dist(u["xy"], other["xy"]), u["primary"]))[:3]
        for anchor in anchors:
            context = pair_context(reference, compact_templates, anchor["primary"], other["primary"])
            _, indices = bundle["model"].neighbors([context["context"]])
            for index in indices[0]:
                if attempted >= args.max_guide_records or len(candidates) >= args.limit or budget["nodes"] >= budget["maximum"]:
                    break
                attempted += 1
                record = bundle["records"][int(index)]
                if record["sourceId"] == reference["sourceId"]:
                    reject["sameReferenceSavedPatternExcluded"] += 1
                    continue
                preset = guide_choice(reference, context, record)
                delta = preset[other["primary"]]["angleDeltaDegrees"]
                if abs((delta+90)%180-90) < 45:
                    reject["lessThan45DegreeAxisChange"] += 1
                    continue
                if any(world(local[c["part"]["templateId"]]["solid"], c["part"]).intersection(g).area > 1e-5
                       for i, c in preset.items() for j, g in original_solids.items() if j not in preset):
                    reject["newCompleteGuideUnitFrameworkSolidConflict"] += 1
                    continue
                for chosen in ordinary_solutions(reference, opts, preset, templates, local, budget):
                    try:
                        c = candidate(reference, {}, templates, sources, local, model, hashfile(w/"data/source500_native.json"), hashfile(modelpath), 7970000+len(candidates), 0, selected_override=chosen)
                    except ValueError:
                        reject["candidateNativeHeadContactOrMapping"] += 1
                        continue
                    ap = c["assemblyProvenance"]
                    ap["method"] = "Bounded complete anchored guide-angle pattern plus frozen native ordinary option subset; current independent integrity/geometry/semantic checks"
                    ap["retargetedAssetImports"] = [imp for imp in ap["retargetedAssetImports"] if not set(imp["partMapping"].values())&set(preset)]
                    ap["retargetedAssetImports"].append({"sourceId": reference["sourceId"], "partMapping": {i: i for i in preset}})
                    ap["learnedPlacementCertificate"]["guideRigidGroups"] = [context["otherUnitIds"]]+[[i] for i in chosen if i not in preset]
                    ap["anchoredGuidePatchCertificate"] = {"sourcePackSha256": hashfile(w/"data/source500_native.json"), "modelSha256": hashfile(anchor_model_path),
                        "referenceSourceId": reference["sourceId"], "anchorPrimaryId": anchor["primary"], "otherPrimaryId": other["primary"],
                        "recipientContext": context["context"], "learnedRecordIndex": int(index), "donorSourceId": record["sourceId"]}
                    proof = anchor_audit.check(c)
                    if not proof["passed"]:
                        raise AssertionError(str(proof))
                    actual = {p["id"]: p for p in c["parts"]}
                    meaningful = [i for i in chosen if meaningful_edit(ps[i], actual[i], templates, [e["target"] for e in reference["links"] if e["source"] == i], [e["target"] for e in c["links"] if e["source"] == i])]
                    if len(meaningful) != len(chosen) or not math.ceil(len(ps)*.2) <= len(meaningful) <= math.ceil(len(ps)*.25):
                        reject["semanticMeaningfulRootBudget"] += 1
                        continue
                    nr, rr = native.check(c), raw.check(c)
                    if not nr["nativeLearnedPlacementIntegrityPassed"]:
                        reject["independentNativeAssetGuideIntegrity"] += 1
                        continue
                    if not rr["rawGeometrySeamPassed"]:
                        reject["independentRawNativeSolidsSensors"] += 1
                        continue
                    novel = novelty.check(c)
                    if not novel["passed"]:
                        reject["semanticNovelty507"] += 1
                        continue
                    c["preliminaryAnchoredGuidePatternIntegrity"] = proof
                    c["preliminaryNativeAssetIntegrity"] = nr
                    c["preliminaryRawNativeGeometry"] = rr
                    c["preliminarySemanticNovelty507"] = novel
                    c["preliminaryAuthoredTopologyReview"] = graph_delta(original_graph, graph_summary(c, True))
                    candidates.append(c)
                    print("MIXED_GUIDE_PRELIMINARY", c["id"], context["otherUnitIds"], int(index), c["preliminaryAuthoredTopologyReview"], flush=True)
                    if len(candidates) >= args.limit:
                        break
            if attempted >= args.max_guide_records or len(candidates) >= args.limit or budget["nodes"] >= budget["maximum"]:
                break
        if attempted >= args.max_guide_records or len(candidates) >= args.limit or budget["nodes"] >= budget["maximum"]:
            break
    report = {"schemaVersion": 3, "complete": False, "acceptedCount": 0, "status": "BOUNDED_MIXED_GUIDE_ROLE_ANGLE_PROPOSALS_CURRENT_NATIVE_ORDER_DIFFICULTY_VISUAL_PENDING",
              "sourcePosition": args.position, "trainingSourceCount": len(levels), "attemptedGuideRecords": attempted,
              "maximumGuideRecordsDeclared": args.max_guide_records, "maximumOrdinaryNodesDeclared": args.max_ordinary_nodes,
              "ordinaryNodes": budget["nodes"], "rejections": dict(reject), "ordinaryRejections": dict(budget["rejections"]), "levels": candidates,
              "elapsedSeconds": time.monotonic()-started, "hashes": {"source": hashfile(w/"data/source500_native.json"), "templates": hashfile(w/"data/source_templates.json"),
                  "ordinaryOptionSubset": hashfile(args.ordinary_options), "anchorModel": hashfile(anchor_model_path), "localPoseModel": hashfile(modelpath), "tool": hashfile(Path(__file__))},
              "limits": ["Frozen ordinary options are a finite historical proposal subset, not a claim to exhaustive current model ranking.",
                         "One saved pair supplies relative angle; original recipient centers and complete anchor unit stay fixed, other complete native guide unit rigidly rotates.",
                         "Authored graph profiles and geometric hull role proxies are model context and review evidence, not semantic shape or acceptance gates.",
                         "No native obstruction/release ordering, source-relative difficulty or whole-shape acceptance is certified."]}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, separators=(",", ":")), encoding="utf8")
    print("MIXED_GUIDE_DONE", len(candidates), attempted, budget["nodes"], dict(reject), flush=True)


if __name__ == "__main__":
    main()
