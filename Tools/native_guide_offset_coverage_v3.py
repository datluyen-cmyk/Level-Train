"""Read-only coverage of whole native Pull/guide units with alternate slots.

Exact complete Pull template, input, Rigidbody and state contracts are matched.
Candidate guide locations are captured native unit transforms, never invented
translations. Coverage is not evidence of a changed puzzle or release sequence.
"""
from pathlib import Path
from collections import defaultdict, Counter
import argparse
import hashlib
import json
import math

import networkx as nx

from assembly_integrity_audit_v3 import native_identity
from guide_neighborhood_pilot_v3 import payload_compact
from layout_learning_v3 import read, hashfile


STATE = ("kind", "z", "chainLockCount", "collectible", "scoreEligible", "key", "lockKey")


def digest(value):
    return hashlib.sha256(json.dumps(value, sort_keys=True, separators=(",", ":")).encode()).hexdigest()


def local_xy(part, anchor):
    x, y = part["x"]-anchor["x"], part["y"]-anchor["y"]
    a = math.radians(anchor["angle"])
    return [math.cos(a)*x+math.sin(a)*y, -math.sin(a)*x+math.cos(a)*y]


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--workspace", required=True, type=Path)
    p.add_argument("--output", required=True, type=Path)
    args = p.parse_args()
    w = args.workspace.resolve()
    with (w/"data/source500_native.json").open() as stream:
        levels = json.load(stream, object_hook=payload_compact)["levels"]
    templates = {t["id"]: t for t in read(w/"data/source_templates.json")["templates"]}
    template_keys = {tid: digest(native_identity(t)) for tid, t in templates.items()}
    units = []
    pool = defaultdict(list)
    for level in levels:
        ps = {p["id"]: p for p in level["parts"]}
        graph = nx.Graph()
        graph.add_nodes_from(ps)
        graph.add_edges_from((e["guideOwnerId"], e["lockedOwnerId"]) for e in level["guideLinks"])
        bombs = {b["ownerPartId"] for b in level["bombs"]}
        for component in nx.connected_components(graph):
            if len(component) != 2:
                continue
            members = [ps[i] for i in sorted(component)]
            pulls = [p for p in members if p["kind"] == "slider"]
            fixtures = [p for p in members if p["kind"] == "fixed"]
            if len(pulls) != 1 or len(fixtures) != 1:
                continue
            if any(p["id"] in bombs or p.get("chainLockCount", 0) or p.get("key") or p.get("lockKey") for p in members):
                continue
            if any(e["source"] in component or e["target"] in component for e in level["links"]):
                continue
            pull, fixture = pulls[0], fixtures[0]
            guide_links = [e for e in level["guideLinks"] if e["guideOwnerId"] in component]
            strict_bar_key = digest({"nativeTemplate": template_keys[pull["templateId"]],
                "input": native_identity(pull.get("inputShapes")), "rigidbody": native_identity(pull.get("sourceRigidbody")),
                "state": {key: pull.get(key) for key in STATE}})
            fixture_key = digest({"nativeTemplate": template_keys[fixture["templateId"]],
                "input": native_identity(fixture.get("inputShapes")), "rigidbody": native_identity(fixture.get("sourceRigidbody")),
                "state": {key: fixture.get(key) for key in STATE},
                "guideShapes": [e["guideShapeIndices"] for e in guide_links]})
            xy = local_xy(fixture, pull)
            angle = (fixture["angle"]-pull["angle"]+180)%360-180
            record = {"sourceId": level["sourceId"], "position": level["position"],
                      "pullId": pull["id"], "guideId": fixture["id"], "barKey": strict_bar_key,
                      "fixtureKey": fixture_key, "pullTemplateId": pull["templateId"], "guideTemplateId": fixture["templateId"],
                      "nativeGuideLocalXY": xy, "nativeGuideRelativeAngleDegrees": angle,
                      "barWidth": templates[pull["templateId"]].get("width", 0),
                      "barLength": templates[pull["templateId"]].get("length", 0),
                      "completeGuideBindings": guide_links}
            pool[strict_bar_key].append(record)
            units.append(record)
    coverage = []
    for reference in units:
        if reference["position"] > 300:
            continue
        same_fixture = []
        other_fixture = []
        width = max(.01, reference["barWidth"])
        for donor in pool[reference["barKey"]]:
            if donor["sourceId"] == reference["sourceId"] and donor["pullId"] == reference["pullId"]:
                continue
            delta = [donor["nativeGuideLocalXY"][i]-reference["nativeGuideLocalXY"][i] for i in range(2)]
            angle_delta = abs((donor["nativeGuideRelativeAngleDegrees"]-reference["nativeGuideRelativeAngleDegrees"]+180)%360-180)
            if abs(delta[1]) > 1e-4 or angle_delta > .005 or abs(delta[0]) <= 1e-4:
                continue
            target = same_fixture if donor["fixtureKey"] == reference["fixtureKey"] else other_fixture
            target.append({"sourceId": donor["sourceId"], "position": donor["position"], "pullId": donor["pullId"],
                           "guideId": donor["guideId"], "nativeGuideLocalXY": donor["nativeGuideLocalXY"],
                           "longitudinalDelta": delta[0], "longitudinalDeltaBarWidths": delta[0]/width,
                           "crossSource": donor["sourceId"] != reference["sourceId"],
                           "guideTemplateId": donor["guideTemplateId"]})
        coverage.append({**reference, "sameCompleteFixtureLongitudinalDonorCount": len(same_fixture),
                         "differentCompleteNativeFixtureLongitudinalDonorCount": len(other_fixture),
                         "sameFixtureAtLeastQuarterWidthCrossSourceDonorCount": sum(x["crossSource"] and abs(x["longitudinalDeltaBarWidths"]) >= .25 for x in same_fixture),
                         "sameFixtureAtLeastHalfWidthCrossSourceDonorCount": sum(x["crossSource"] and abs(x["longitudinalDeltaBarWidths"]) >= .5 for x in same_fixture),
                         "sameFixtureAtLeastOneWidthCrossSourceDonorCount": sum(x["crossSource"] and abs(x["longitudinalDeltaBarWidths"]) >= 1. for x in same_fixture),
                         "sameFixtureDonors": same_fixture, "otherNativeFixtureDonors": other_fixture})
    per_position = []
    for position in sorted({r["position"] for r in coverage}):
        rows = [r for r in coverage if r["position"] == position]
        per_position.append({"position": position, "sourceId": rows[0]["sourceId"], "eligibleIntactNativeUnits": len(rows),
            "unitsWithSameFixtureQuarterWidthCrossSourceDonors": sum(r["sameFixtureAtLeastQuarterWidthCrossSourceDonorCount"] > 0 for r in rows),
            "unitsWithSameFixtureHalfWidthCrossSourceDonors": sum(r["sameFixtureAtLeastHalfWidthCrossSourceDonorCount"] > 0 for r in rows),
            "unitsWithSameFixtureOneWidthCrossSourceDonors": sum(r["sameFixtureAtLeastOneWidthCrossSourceDonorCount"] > 0 for r in rows)})
    report = {"schemaVersion": 1, "trainingSourceCount": len(levels), "completeNativePairUnitsIn500": len(units),
              "targetRange": [1, 300], "targetNativeUnits": len(coverage), "acceptedCount": 0,
              "status": "READ_ONLY_STRICT_NATIVE_DONOR_COVERAGE_NOT_GENERATED_LEVELS_OR_CHANGED_RELEASE_PROOF",
              "criteria": {"sameFullNativePullTemplateInputRigidbodyState": True,
                           "completeNativePullFixedGuideComponentOnly": True, "noAuthoredPinEdges": True,
                           "transverseDeltaTolerance": 1e-4, "relativeGuideAngleToleranceDegrees": .005,
                           "minimumLongitudinalDifferenceForCoverage": 1e-4,
                           "quarterHalfOneWidthCounts": "Descriptive strata only, not meaningful gameplay or acceptance thresholds"},
              "perPosition": per_position, "units": coverage,
              "limits": ["Whole donor native pair transforms only. No synthetic fixture sliding or resized bars.",
                         "A different slot can remain mechanically equivalent; current native replay/blocking/order evidence is mandatory.",
                         "Same-fixture stratum also requires complete native fixture template/input/Rigidbody/state/binding identity.",
                         "Exact Rigidbody material file references remain distinct here; pointer interpretation was not relaxed.",
                         "Three-root/multi-guide units and authored pin-connected units are excluded from this first coverage audit."],
              "hashes": {"source": hashfile(w/"data/source500_native.json"), "templates": hashfile(w/"data/source_templates.json"),
                         "tool": hashfile(Path(__file__))}}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, separators=(",", ":")), encoding="utf8")
    print("NATIVE_GUIDE_OFFSET_COVERAGE", len(units), "units500", len(coverage), "units300", flush=True)
    print(json.dumps([r for r in per_position if r["position"] in (21, 30, 130, 150, 170, 240, 300)]), flush=True)


if __name__ == "__main__":
    main()
