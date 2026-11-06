"""Independently re-audit frozen preliminary layouts with semantic orientations."""
from pathlib import Path
import argparse
import json
import math

from guide_neighborhood_pilot_v3 import payload_compact
from layout_learning_v3 import hashfile, read
from layout_novelty_semantic_v3 import LayoutNoveltyIndex
from orientation_semantics_v3 import meaningful_edit, orientation_order, angle_error_degrees


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--workspace", required=True, type=Path)
    p.add_argument("--pack", required=True, type=Path)
    p.add_argument("--report", required=True, type=Path)
    args = p.parse_args()
    w = args.workspace.resolve()
    with (w/"data/source500_native.json").open() as stream:
        levels = json.load(stream, object_hook=payload_compact)["levels"]
    templates = {t["id"]: t for t in read(w/"data/source_templates.json")["templates"]}
    nt = dict(templates)
    nt.update({t["id"]: t for t in read(w/"data/novelty_extras/source_templates.json")["templates"]})
    extra = read(w/"data/novelty_extras/source_parity_pack.json")["levels"]
    by = {l["position"]: l for l in levels}
    candidates = read(args.pack)["levels"]
    index = LayoutNoveltyIndex(levels+extra, nt)
    rows = []
    for c in candidates:
        reference = by[c["position"]]
        originals = {p["id"]: p for p in reference["parts"]}
        decision = c["assemblyProvenance"]["decisions"][0]
        claimed = set(decision["modifiedRootIds"])
        details = []
        for actual in c["parts"]:
            i = actual["id"]
            old = originals[i]
            old_targets = [e["target"] for e in reference["links"] if e["source"] == i]
            new_targets = [e["target"] for e in c["links"] if e["source"] == i]
            meaningful = meaningful_edit(old, actual, templates, old_targets, new_targets)
            if i not in claimed and not meaningful:
                continue
            order_a, order_b = orientation_order(old, templates[old["templateId"]]), orientation_order(actual, templates[actual["templateId"]])
            details.append({"partId": i, "kind": actual["kind"], "claimedModified": i in claimed,
                            "sourceOrientationOrder": order_a, "actualOrientationOrder": order_b,
                            "rawAngleDelta": actual["angle"]-old["angle"],
                            "semanticAngleDelta": angle_error_degrees(actual["angle"], old["angle"], order_a, order_b),
                            "meaningful": meaningful})
        novelty = index.check(c)
        n = len(c["parts"])
        count = sum(r["meaningful"] for r in details)
        cosmetic = [r["partId"] for r in details if r["claimedModified"] and not r["meaningful"]]
        row = {"position": c["position"], "id": c["id"], "nativeRoots": n,
               "claimedModifiedRootCount": len(claimed), "meaningfulModifiedRootCount": count,
               "meaningfulRootFraction": count/n, "minimumMeaningfulRootCount": max(1, math.ceil(n*.2)),
               "legacyPreferredMaximumRootCountCeil25percent": max(1, math.ceil(n*.25)),
               "meaningfulRootGatePassed": count >= max(1, math.ceil(n*.2)) and not cosmetic,
               "cosmeticClaimedModifiedRootIds": cosmetic,
               "literal25percentExceededByLegacyQuantization": len(claimed)/n > .25,
               "semanticNovelty507": novelty, "roots": details, "accepted": False}
        rows.append(row)
        print("SEMANTIC_AUDIT", c["position"], count, len(claimed), row["meaningfulRootGatePassed"], novelty["passed"], flush=True)
    # Actual corpus checks supplement the labeled synthetic invariance suite.
    real_round = next((p, templates[p["templateId"]], level) for level in levels for p in level["parts"]
                      if orientation_order(p, templates[p["templateId"]]) == 0)
    p, template, level = real_round
    real_round_pass = not meaningful_edit(p, {**p, "angle": p["angle"]+73.}, templates)
    real_straight = next((p, templates[p["templateId"]], level) for level in levels for p in level["parts"]
                         if orientation_order(p, templates[p["templateId"]]) == 2)
    pull, _, pull_level = real_straight
    real_pull_pass = not meaningful_edit(pull, {**pull, "angle": pull["angle"]+180.}, templates)
    report = {"acceptedGeneratedCount": 0, "frozenStagedCount": len(candidates),
              "meaningfulRootGateFailures": [r["position"] for r in rows if not r["meaningfulRootGatePassed"]],
              "semanticNoveltyFailures": [r["position"] for r in rows if not r["semanticNovelty507"]["passed"]],
              "physicallyInputEquivalentEdits": [{"position": r["position"], "partId": d["partId"]}
                                                 for r in rows for d in r["roots"] if not d["meaningful"]],
              "actualSourceChecks": {"closedRoundReceiver": {"passed": real_round_pass, "position": level["position"], "partId": p["id"]},
                                     "straightTwoWayPull": {"passed": real_pull_pass, "position": pull_level["position"], "partId": pull["id"]}},
              "levels": rows,
              "hashes": {"frozenPack": hashfile(args.pack), "semanticModule": hashfile(Path(__file__).with_name("orientation_semantics_v3.py")),
                         "semanticNovelty": hashfile(Path(__file__).with_name("layout_novelty_semantic_v3.py")), "tool": hashfile(Path(__file__))},
              "limits": ["Physical/input geometry and heuristic507novelty only; no gameplay proof or difficulty.",
                         "Native Rigidbody/state identities remain independently audited; differing material pointers do not manufacture a spatial structural edit.",
                         "Historical root budget uses ceil(.20*n) minimum, ceil(.25*n) preferred maximum; exact fractions are reported honestly."]}
    assert real_round_pass and real_pull_pass
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps(report, indent=2), encoding="utf8")
    print("SEMANTIC_RESULT", report["meaningfulRootGateFailures"], report["semanticNoveltyFailures"], flush=True)


if __name__ == "__main__":
    main()
