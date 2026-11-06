"""Read-only authored graph and sampled initial geometry review flags.

Neither authored links nor small geometric probes are native movement rules.
This tool must not serve as a solvability, difficulty or acceptance gate.
"""
from collections import Counter
from pathlib import Path
import argparse
import hashlib
import json
import math

import networkx as nx
from shapely import affinity, union_all

from assembly_contact_audit_v3 import shape_polygon


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def compact(value):
    if "templateId" in value and "kind" in value and "id" in value:
        return {k: value[k] for k in ("id", "templateId", "kind", "x", "y", "angle", "collectible",
                "chainLockCount", "key", "lockKey") if k in value}
    if str(value.get("id", "")).startswith("native3-") and "physicalShapes" in value:
        return {k: value[k] for k in ("id", "kind", "width", "physicalShapes", "attachments", "movement") if k in value}
    if "parts" in value and "position" in value:
        return {k: value[k] for k in ("id", "sourceId", "position", "parts", "links", "guideLinks", "bombs") if k in value}
    return value


def load_compact(path):
    with path.open(encoding="utf8") as stream:
        return json.load(stream, object_hook=compact)


def graph_summary(level, include_guides):
    ps = {p["id"]: p for p in level["parts"]}
    movable = {i for i, p in ps.items() if p["kind"] != "fixed"}
    graph = nx.DiGraph()
    graph.add_nodes_from(ps)
    graph.add_edges_from((e["source"], e["target"]) for e in level["links"])
    if include_guides:
        graph.add_edges_from((e["guideOwnerId"], e["lockedOwnerId"]) for e in level["guideLinks"])
    undirected = graph.to_undirected()
    components = sorted((sorted(group) for group in nx.connected_components(undirected)), key=lambda v: (-len(v), v))
    active_components = [g for g in components if movable.intersection(g)]
    return {"edgeCount": len(level["links"])+(len(level["guideLinks"]) if include_guides else 0),
            "distinctDirectedEdgeCount": graph.number_of_edges(),
            "weakComponents": [{"rootIds": c, "rootCount": len(c), "movableCount": len(movable.intersection(c)),
                                 "fixedCount": len(c)-len(movable.intersection(c))} for c in active_components],
            "weakComponentMovableSizes": sorted([len(movable.intersection(c)) for c in active_components], reverse=True),
            "isolatedMovableRootIds": sorted(i for i in movable if undirected.degree(i) == 0),
            "zeroIncomingMovableRootIds": sorted(i for i in movable if graph.in_degree(i) == 0),
            "zeroOutgoingMovableRootIds": sorted(i for i in movable if graph.out_degree(i) == 0),
            "leafMovableRootIds": sorted(i for i in movable if undirected.degree(i) == 1),
            "movableWeakDegreeHistogram": dict(sorted(Counter(undirected.degree(i) for i in movable).items())),
            "articulationRootIds": sorted(nx.articulation_points(undirected)),
            "strongComponentSizesAboveOne": sorted([len(c) for c in nx.strongly_connected_components(graph) if len(c) > 1], reverse=True)}


def graph_delta(before, after):
    return {"weakComponentCountChange": len(after["weakComponents"])-len(before["weakComponents"]),
            "weakComponentMovableSizesChanged": before["weakComponentMovableSizes"] != after["weakComponentMovableSizes"],
            "newlyIsolatedMovableRootIds": sorted(set(after["isolatedMovableRootIds"])-set(before["isolatedMovableRootIds"])),
            "noLongerIsolatedMovableRootIds": sorted(set(before["isolatedMovableRootIds"])-set(after["isolatedMovableRootIds"])),
            "newZeroIncomingMovableRootIds": sorted(set(after["zeroIncomingMovableRootIds"])-set(before["zeroIncomingMovableRootIds"])),
            "newZeroOutgoingMovableRootIds": sorted(set(after["zeroOutgoingMovableRootIds"])-set(before["zeroOutgoingMovableRootIds"]))}


def world(geometry, part):
    return affinity.translate(affinity.rotate(geometry, part["angle"], origin=(0, 0)), part["x"], part["y"])


def geometry_summary(level, templates, local):
    ps = {p["id"]: p for p in level["parts"]}
    solids = {i: world(local[p["templateId"]], p) for i, p in ps.items()}
    rows = []
    for i, p in ps.items():
        if p["kind"] == "fixed":
            continue
        template = templates[p["templateId"]]
        width = max(.01, template.get("width", .5))
        own = solids[i]
        pairs = [(j, own.distance(g)) for j, g in solids.items() if j != i and not g.is_empty]
        nearest = sorted(pairs, key=lambda v: (v[1], v[0]))[:3]
        t = template
        triggers = {s.get("attachmentId", -1) for s in t["physicalShapes"] if s["role"] == "head_trigger"}
        solid_attachments = [{"attachmentId": a.get("id", a.get("sourceAttachmentIndex")),
                              "nativeKind": a.get("kind"), "nativeType": a.get("type")}
                             for a in t.get("attachments", []) if a.get("id", a.get("sourceAttachmentIndex")) not in triggers]
        note = {"rootId": i, "kind": p["kind"], "nearestOtherSolidDistances": [{"rootId": j, "nativeWorldDistance": d, "widths": d/width} for j, d in nearest],
                "nativeNonHeadTriggerAttachmentCount": len(solid_attachments), "nativeNonHeadTriggerAttachments": solid_attachments,
                "sampledDirectionsWithoutNewSolidPatch": [], "sampledDirections": []}
        if p["kind"] not in ("ring", "slider") or own.is_empty:
            note["sampleStatus"] = "UNSUPPORTED_KIND_OR_EMPTY_SOLID"
            rows.append(note)
            continue
        axis = t.get("movement", {}).get("axisLocal", {})
        angle = math.radians(p["angle"])
        dx = axis.get("x", 0)*math.cos(angle)-axis.get("y", 0)*math.sin(angle)
        dy = axis.get("x", 0)*math.sin(angle)+axis.get("y", 0)*math.cos(angle)
        if p["kind"] == "slider" and math.hypot(dx, dy) < .99:
            note["sampleStatus"] = "UNSUPPORTED_MISSING_NATIVE_AXIS"
            rows.append(note)
            continue
        baseline = {j: own.intersection(g).buffer(1e-5) for j, g in solids.items() if j != i and not g.is_empty}
        for direction in (-1, 1):
            conflicts = {}
            maximum_new = 0.
            for step in range(1, 6):
                moved = (affinity.rotate(own, direction*step, origin=(p["x"], p["y"])) if p["kind"] == "ring"
                         else affinity.translate(own, dx*direction*width*step*.1, dy*direction*width*step*.1))
                for j, old_patch in baseline.items():
                    new_area = moved.intersection(solids[j]).difference(old_patch).area
                    maximum_new = max(maximum_new, new_area)
                    if new_area > 1e-5:
                        conflicts.setdefault(j, step)
            if not conflicts:
                note["sampledDirectionsWithoutNewSolidPatch"].append(direction)
            note["sampledDirections"].append({"direction": direction, "probe": "1to5degrees_at1degree_samples" if p["kind"] == "ring" else "0.1to0.5widths_at0.1width_samples",
                "newSolidPatchPartnerFirstSample": [{"rootId": j, "sample": s} for j, s in sorted(conflicts.items())],
                "maximumNewSolidPatchArea": maximum_new})
        note["sampleStatus"] = "GEOMETRIC_SAMPLES_ONLY_NO_CONTINUOUS_SWEEP_OR_NATIVE_CONTACT_FILTER"
        rows.append(note)
    return rows


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True, type=Path)
    parser.add_argument("--pack", required=True, type=Path)
    parser.add_argument("--expected-pack-sha256")
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    pack_sha = digest(args.pack)
    if args.expected_pack_sha256 and pack_sha != args.expected_pack_sha256:
        raise ValueError("Frozen pack hash differs")
    levels = load_compact(args.workspace/"data/source500_native.json")["levels"]
    sources = {l["position"]: l for l in levels}
    templates = {t["id"]: t for t in load_compact(args.workspace/"data/source_templates.json")["templates"]}
    proposals = load_compact(args.pack)["levels"]
    tids = {p["templateId"] for l in proposals for p in l["parts"]} | {p["templateId"] for c in proposals for p in sources[c["position"]]["parts"]}
    local = {tid: union_all([shape_polygon(s) for s in templates[tid]["physicalShapes"] if s.get("sourceEnabled", True) and s.get("sourceLayer", 7) == 7 and not s.get("trigger", False)]) for tid in tids}
    rows = []
    for candidate in proposals:
        source = sources[candidate["position"]]
        graphs = {}
        flags = []
        for guides, key in ((False, "authoredPins"), (True, "authoredPinsAndGuides")):
            before, after = graph_summary(source, guides), graph_summary(candidate, guides)
            delta = graph_delta(before, after)
            graphs[key] = {"source": before, "candidate": after, "delta": delta}
            if delta["newlyIsolatedMovableRootIds"]:
                flags.append(key+":NEW_AUTHORED_ISOLATED_MOVABLE")
            if delta["weakComponentCountChange"] > 0:
                flags.append(key+":MORE_AUTHORED_WEAK_COMPONENTS")
        before_geom = geometry_summary(source, templates, local)
        after_geom = geometry_summary(candidate, templates, local)
        bg = {r["rootId"]: r for r in before_geom}
        new_attachment_roots = [r["rootId"] for r in after_geom if r["nativeNonHeadTriggerAttachmentCount"] > bg[r["rootId"]]["nativeNonHeadTriggerAttachmentCount"]]
        new_geometric_clear = [r["rootId"] for r in after_geom if len(r["sampledDirectionsWithoutNewSolidPatch"]) > len(bg[r["rootId"]]["sampledDirectionsWithoutNewSolidPatch"])]
        if new_attachment_roots:
            flags.append("MORE_NATIVE_SOLID_ATTACHMENTS_WITHOUT_HEAD_TRIGGER")
        row = {"position": candidate["position"], "sourceId": source["sourceId"], "candidateId": candidate["id"],
               "accepted": False, "reviewFlags": flags, "graphs": graphs,
               "geometrySamples": {"source": before_geom, "candidate": after_geom,
                                   "rootsWithMoreSampledClearDirections": new_geometric_clear,
                                   "rootsWithMoreNativeNonHeadTriggerAttachments": new_attachment_roots}}
        rows.append(row)
        print("TOPOLOGY_REVIEW", row["position"], flags, flush=True)
    report = {"candidateCount": len(rows), "acceptedGeneratedCount": 0,
              "status": "STATIC_TOPOLOGY_AND_SAMPLED_GEOMETRY_REVIEW_FLAGS_NO_ACCEPTANCE_GATE",
              "hashes": {"pack": pack_sha, "source": digest(args.workspace/"data/source500_native.json"),
                         "templates": digest(args.workspace/"data/source_templates.json"), "tool": digest(Path(__file__))},
              "flagCounts": dict(Counter(flag for r in rows for flag in r["reviewFlags"])), "levels": rows,
              "limits": ["Authored pin/guide graphs omit physical clearance obstructions, trigger state and collision filters.",
                         "Weak components, degree zero and zero incoming/outgoing do not prove legal actions, removal freedom or solved difficulty.",
                         "Geometric probes are discrete small motion samples, not continuous collision checks or legal engine actions; native receiving-wall exception, skin, stateful rules and direction constraints are not simulated.",
                         "A solid attachment without head_trigger is a native physical obstacle, not a missing logical binding.",
                         "Source-family topology/difficulty and current native engine evidence are required before a gate can be introduced."]}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, separators=(",", ":")), encoding="utf8")
    print("TOPOLOGY_REVIEW_DONE", len(rows), report["flagCounts"], flush=True)


if __name__ == "__main__":
    main()
