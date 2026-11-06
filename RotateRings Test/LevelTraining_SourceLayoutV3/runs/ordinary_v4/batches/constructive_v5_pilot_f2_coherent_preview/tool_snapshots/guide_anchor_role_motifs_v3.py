"""Saved joint two-unit relative angles with authored graph/geometric-role context.

One complete native guide unit is the fixed anchor. A saved donor pair supplies
the other unit's relative angle; original recipient centers remain fixed. This
is an angle proposal distribution, not donor XY transplantation or gameplay.
"""
from collections import Counter
from pathlib import Path
import argparse
import json
import math
import sys

import joblib
import networkx as nx
import numpy as np
from scipy.special import logsumexp
from shapely.geometry import MultiPoint, Point
from sklearn.model_selection import GroupKFold
from sklearn.neighbors import NearestNeighbors
from sklearn.preprocessing import StandardScaler

import guide_neighborhood_motifs_v3 as base


def role_contexts(level, templates):
    ps = {p["id"]: p for p in level["parts"]}
    movable = {i for i, p in ps.items() if p["kind"] != "fixed"}
    graph = nx.Graph()
    graph.add_nodes_from(ps)
    graph.add_edges_from((e["source"], e["target"]) for e in level["links"])
    graph.add_edges_from((e["guideOwnerId"], e["lockedOwnerId"]) for e in level["guideLinks"])
    memberships = {i: set(c)&movable for c in nx.connected_components(graph) for i in c}
    sizes = sorted([len(set(c)&movable) for c in nx.connected_components(graph) if set(c)&movable], reverse=True)
    global_role = [v/max(1, len(movable)) for v in (sizes+[0]*6)[:6]]
    global_role += [sum(graph.degree(i) == 0 for i in movable)/max(1, len(movable))]
    xy = np.asarray([[ps[i]["x"], ps[i]["y"]] for i in sorted(movable)])
    center = xy.mean(axis=0)
    span = max(1e-5, np.ptp(xy, axis=0).max())
    hull = MultiPoint(xy).convex_hull
    result = {}
    for i in movable:
        p = ps[i]
        width = max(.01, templates[p["templateId"]].get("width", .5))
        nearest = sorted([j for j in movable if j != i], key=lambda j: ((p["x"]-ps[j]["x"])**2+(p["y"]-ps[j]["y"])**2, j))[:4]
        kinds = Counter(ps[j]["kind"] for j in nearest)
        distance = math.hypot(p["x"]-ps[nearest[0]]["x"], p["y"]-ps[nearest[0]]["y"]) if nearest else 0.
        result[i] = [len(memberships[i])/max(1, len(movable)), graph.degree(i),
                     sum(e["target"] == i for e in level["links"]), sum(e["source"] == i for e in level["links"]),
                     float(hull.boundary.distance(Point(p["x"], p["y"])) < 1e-5) if hull.geom_type == "Polygon" else 1.,
                     float(np.linalg.norm(np.asarray([p["x"], p["y"]])-center)/span), distance/width,
                     *[kinds[k]/max(1, len(nearest)) for k in base.KINDS]]
    return global_role, result


def pair_context(level, templates, anchor, other, roles=None):
    units = {u["primary"]: u for u in base.units(level, templates)}
    a, b = units[anchor], units[other]
    global_role, role = roles or role_contexts(level, templates)
    width = a["width"]
    xy = base.rotate([[b["xy"][0]-a["xy"][0], b["xy"][1]-a["xy"][1]]], -a["angle"])[0]/width
    baseline = base.level_context(level)+xy.tolist()+global_role
    return {"context": baseline+a["descriptor"]+b["descriptor"]+role[anchor]+role[other],
            "baseline": baseline, "anchorPrimaryId": anchor, "otherPrimaryId": other,
            "anchorUnitIds": a["ids"], "otherUnitIds": b["ids"], "anchorWidth": width}


def records_for(level, templates):
    us = base.units(level, templates)
    roles = role_contexts(level, templates)
    out = []
    for anchor in us:
        nearest = sorted([u for u in us if u["primary"] != anchor["primary"]],
                         key=lambda u: (np.linalg.norm(np.asarray(u["xy"])-anchor["xy"]), u["primary"]))[:3]
        for other in nearest:
            context = pair_context(level, templates, anchor["primary"], other["primary"], roles)
            phi = math.radians(other["angle"]-anchor["angle"])
            out.append({**context, "sourceId": level["sourceId"], "position": level["position"],
                        "pose": [math.cos(phi), math.sin(phi)], "relativeAngleDegrees": other["angle"]-anchor["angle"]})
    return out


class GuideAnchorRoleModel:
    def fit(self, x, y):
        self.y = np.asarray(y, float)
        self.scaler = StandardScaler().fit(x)
        self.nn = NearestNeighbors(n_neighbors=min(32, len(y)), n_jobs=1).fit(self.scaler.transform(x))
        return self

    def neighbors(self, x):
        return self.nn.kneighbors(self.scaler.transform(np.asarray(x, float)))

    def metrics(self, x, y, source_ids):
        distances, indices = self.neighbors(x)
        z = (np.asarray(y)[:, None, :]-self.y[indices])/.35
        weights = -.5*distances**2
        weights -= logsumexp(weights, axis=1)[:, None]
        nll = -logsumexp(weights-.5*np.sum(z*z, axis=2)-2*math.log(.35)-math.log(2*math.pi), axis=1)
        actual = np.arctan2(self.y[indices, 1], self.y[indices, 0])
        expected = np.arctan2(np.asarray(y)[:, 1], np.asarray(y)[:, 0])
        oracle = np.degrees(np.abs(np.angle(np.exp(1j*(actual-expected[:, None]))))).min(axis=1)
        aggregate = [{"sourceId": sid, "nll": float(np.mean(nll[np.asarray(source_ids) == sid])),
                      "oracle32RelativeAngleErrorDegrees": float(np.mean(oracle[np.asarray(source_ids) == sid]))} for sid in sorted(set(source_ids))]
        return {"meanHeldSourceNLL": float(np.mean([v["nll"] for v in aggregate])),
                "meanHeldSourceOracle32RelativeAngleErrorDegrees": float(np.mean([v["oracle32RelativeAngleErrorDegrees"] for v in aggregate])),
                "perHeldSource": aggregate}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True, type=Path)
    args = parser.parse_args()
    w = args.workspace.resolve()
    levels = base.read_compact(w/"data/source500_native.json")["levels"]
    templates = {t["id"]: t for t in base.read_compact(w/"data/source_templates.json")["templates"]}
    records = [r for l in levels for r in records_for(l, templates)]
    families = json.loads((w/"models/family_splits.json").read_text())["families"]
    groups = [families[r["sourceId"]] for r in records]
    folds = []
    for fold, (a, b) in enumerate(GroupKFold(n_splits=5).split(records, groups=groups)):
        assert not ({groups[i] for i in a}&{groups[i] for i in b})
        training, held = [records[i] for i in a], [records[i] for i in b]
        report = {"fold": fold, "sourceFamilyIntersection": [], "trainingSourceIds": sorted({r["sourceId"] for r in training}),
                  "heldSourceIds": sorted({r["sourceId"] for r in held})}
        for key, label in (("context", "nativeUnitGeometryAndRole"), ("baseline", "countsRelativeCentersAndGlobalProfileBaseline")):
            model = GuideAnchorRoleModel().fit([r[key] for r in training], [r["pose"] for r in training])
            report[label] = model.metrics([r[key] for r in held], [r["pose"] for r in held], [r["sourceId"] for r in held])
        folds.append(report)
        print("GUIDE_ANCHOR_ROLE_FOLD", fold, {k: {a: b for a, b in v.items() if a != "perHeldSource"} for k, v in report.items() if isinstance(v, dict)}, flush=True)
    final = GuideAnchorRoleModel().fit([r["context"] for r in records], [r["pose"] for r in records])
    target = w/"models/guide_anchor_role_motifs.joblib"
    joblib.dump({"model": final, "records": records, "trainingSourceCount": len(levels),
                 "sourcePackSha256": base.sha(w/"data/source500_native.json"), "templatePackSha256": base.sha(w/"data/source_templates.json"),
                 "toolSha256": base.sha(__file__), "method": "One saved two-complete-guide-unit relative angle pattern, recipient centers fixed and one whole unit anchored exactly",
                 "roleLimits": "Authored component and geometric convex-hull role proxies, not learned semantic silhouette identities or native obstruction/release labels"}, target, compress=3)
    evaluation = {"trainingSourceCount": len(levels), "recordCount": len(records), "sourceCountWithEligiblePairs": len({r["sourceId"] for r in records}),
                  "folds": folds, "declaredBeforeEvaluation": {"neighbors": 32, "angularCosSinBandwidth": .35, "nearestOtherUnitsPerAnchor": 3,
                      "evaluation": "Five existing source-family folds; average records within each source, then average sources equally"},
                  "limits": ["Empirical relative-angle pattern proposals only, not release/difficulty or fun labels.",
                             "Recipient centers remain fixed; this model does not claim donor native XY transplantation.",
                             "Whole source graph component profile and geometric hull role are conditioning features, not universal connectivity rules or semantic contour annotation.",
                             "One fixed anchor plus one moved complete unit is a local pattern; whole-board semantics, meaningful native obstruction/order changes and novelty require separate evidence."],
                  "hashes": {"source": base.sha(w/"data/source500_native.json"), "templates": base.sha(w/"data/source_templates.json"),
                             "families": base.sha(w/"models/family_splits.json"), "model": base.sha(target), "tool": base.sha(__file__)}}
    (w/"models/guide_anchor_role_motifs_evaluation.json").write_text(json.dumps(evaluation, separators=(",", ":")), encoding="utf8")
    print("GUIDE_ANCHOR_ROLE_TRAINED", len(levels), len(records), evaluation["sourceCountWithEligiblePairs"], flush=True)


if __name__ == "__main__":
    sys.modules.setdefault("guide_anchor_role_motifs_v3", sys.modules[__name__])
    GuideAnchorRoleModel.__module__ = "guide_anchor_role_motifs_v3"
    main()
