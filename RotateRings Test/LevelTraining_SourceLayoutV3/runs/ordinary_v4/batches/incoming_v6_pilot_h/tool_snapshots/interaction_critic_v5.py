"""Ordinary native static composition critic, evaluated on whole source families.

Labels distinguish original arrangements from declared synthetic controls only.
They are not fun, human difficulty, collision, release-order or solve labels.
"""
from pathlib import Path
from collections import Counter
import argparse
import copy
import json
import math
import time

import joblib
import networkx as nx
import numpy as np
from sklearn.ensemble import ExtraTreesClassifier
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import roc_auc_score
from sklearn.pipeline import make_pipeline
from sklearn.preprocessing import StandardScaler

from global_composition_v3 import Geometry, NAMES, arrangement, graph_for, mutate
from source_physical_composition_v3 import composition_edges
from layout_learning_v3 import level_context
from ordinary_source_filter_guard_v4 import preflight, validate_source_filter, sha
from refit_ordinary_models_v4 import local_dependency_hashes, payload_compact


EDGE_KINDS = ("pull_axis_crosses_ring", "pull_hull_contains_ring_center", "pull_near_ring")
GRAPH_NAMES = ("componentsPerRoot", "largestComponentFraction", "isolatedRootFraction", "degreeMean", "degreeP90",
    "maxDegree", "cycleRankPerRoot", "bridgeFraction", "articulationFraction", "meanCoreNumber", "meanClustering",
    "reachablePairFraction", "meanConnectedPathLength")
FEATURE_NAMES = NAMES+[prefix+name for prefix in ("authored_", "staticComposition_") for name in GRAPH_NAMES]+[
    "proxy_"+kind+"PerRoot" for kind in EDGE_KINDS]+["proxyStrongPerRoot", "combinedAuthoredCrossComponentEdgesPerRoot"]
CONTROLS = ("clusterSpacing", "clusterOrientation", "grossPoseShuffle", "separateNativeComponents")


def graph_statistics(graph):
    graph = nx.Graph(graph)
    graph.remove_edges_from(nx.selfloop_edges(graph))
    n, m = max(1, graph.number_of_nodes()), graph.number_of_edges()
    components = list(nx.connected_components(graph))
    sizes = [len(c) for c in components]
    degree = np.asarray([d for _, d in graph.degree()], float)
    paths = [nx.average_shortest_path_length(graph.subgraph(c)) for c in components if len(c)>1]
    return [len(components)/n, max(sizes, default=0)/n, float(np.mean(degree==0)) if len(degree) else 0.,
        float(degree.mean()) if len(degree) else 0., float(np.quantile(degree, .9)) if len(degree) else 0.,
        float(degree.max()) if len(degree) else 0., (m-n+len(components))/n,
        len(list(nx.bridges(graph)))/max(1, m), len(list(nx.articulation_points(graph)))/n,
        float(np.mean(list(nx.core_number(graph).values()))) if graph else 0., nx.average_clustering(graph) if graph else 0.,
        sum(s*(s-1) for s in sizes)/max(1, n*(n-1)), float(np.mean(paths)) if paths else 0.]


def features(level, templates, geometry=None):
    geometry = geometry or Geometry(templates)
    style = arrangement(level, templates, geometry)
    authored = graph_for(level).to_undirected()
    proxies = composition_edges(level, templates)
    combined = authored.copy()
    combined.add_edges_from((e["source"], e["target"]) for e in proxies)
    membership = {i: j for j, c in enumerate(nx.connected_components(authored)) for i in c}
    counts = Counter(e["kind"] for e in proxies)
    n = max(1, len(level["parts"]))
    result = np.r_[style, graph_statistics(authored), graph_statistics(combined),
        [counts[k]/n for k in EDGE_KINDS], sum(e["strong"] for e in proxies)/n,
        len({tuple(sorted((e["source"], e["target"]))) for e in proxies if membership[e["source"]]!=membership[e["target"]]})/n]
    if len(result)!=len(FEATURE_NAMES) or not np.isfinite(result).all():
        raise ValueError("Nonfinite or inconsistent exact native composition features")
    return result


def separated_components(level, rng):
    """Declared synthetic negative: intact source components moved radially out."""
    out = copy.deepcopy(level)
    ps = {p["id"]: p for p in out["parts"]}
    components = list(nx.weakly_connected_components(graph_for(out)))
    if len(components)==1:
        # Spatial halves are a synthetic control, never a generation grammar.
        axis = int(np.argmax(np.ptp([[p["x"], p["y"]] for p in ps.values()], axis=0)))
        ordered = sorted(ps, key=lambda i: ps[i][("x", "y")[axis]])
        components = [set(ordered[:len(ordered)//2]), set(ordered[len(ordered)//2:])]
    center = np.mean([[p["x"], p["y"]] for p in ps.values()], axis=0)
    for component in components:
        c = np.mean([[ps[i]["x"], ps[i]["y"]] for i in component], axis=0)
        shift = c-center
        for i in component:
            ps[i]["x"] += float(shift[0])
            ps[i]["y"] += float(shift[1])
    return out


class InteractionCritic:
    def probability(self, level, templates, context):
        vector = features(level, templates)
        probability = float(self.classifier.predict_proba([np.r_[context, vector]])[0, 1])
        return probability, vector


def train(workspace):
    w = Path(workspace).resolve()
    source_filter, bindings = preflight(w)
    model_path, report_path = w/"models/interaction_critic_v5.joblib", w/"models/interaction_critic_v5_evaluation.json"
    cache_path = w/"models/interaction_critic_v5_features.joblib"
    if model_path.exists() or report_path.exists():
        raise ValueError("Refuse existing interaction critic artifacts")
    pack_path, templates_path = w/"data/source500_native.json", w/"data/source_templates.json"
    with pack_path.open(encoding="utf8") as stream:
        levels = json.load(stream, object_hook=payload_compact)["levels"]
    allowed = validate_source_filter(source_filter, sha(pack_path), [l["sourceId"] for l in levels])
    templates = {t["id"]: t for t in json.loads(templates_path.read_text(encoding="utf8"))["templates"]}
    splits_path = w/"models/family_splits.json"
    splits = json.loads(splits_path.read_text(encoding="utf8"))
    hashes = {"sourcePack": sha(pack_path), "templates": sha(templates_path), "familySplits": sha(splits_path),
        "trainer": sha(Path(__file__)), **bindings}
    dependencies = local_dependency_hashes(Path(__file__).resolve())
    started = time.monotonic()
    if cache_path.exists():
        cached = joblib.load(cache_path)
        if cached["hashes"]!=hashes:
            raise ValueError("Stale critic native feature cache")
        originals, controls, contexts = cached["originals"], cached["controls"], cached["contexts"]
    else:
        originals, controls, contexts = [], {k: [] for k in CONTROLS}, []
        geometry = Geometry(templates)
        rng = np.random.default_rng(20261110)
        for i, level in enumerate(levels):
            originals.append(features(level, templates, geometry))
            contexts.append(level_context(level))
            for kind in CONTROLS:
                perturbed = separated_components(level, rng) if kind=="separateNativeComponents" else mutate(level, kind, rng, templates)
                controls[kind].append(features(perturbed, templates, geometry))
            if (i+1)%20==0:
                print("INTERACTION_NATIVE_FEATURES", i+1, len(levels), flush=True)
        originals, contexts = np.asarray(originals), np.asarray(contexts)
        controls = {k: np.asarray(v) for k, v in controls.items()}
        joblib.dump({"hashes": hashes, "originals": originals, "controls": controls, "contexts": contexts}, cache_path, compress=3)
    by_id = {str(l["sourceId"]): i for i, l in enumerate(levels)}
    folds = []
    for split in splits["splits"]:
        tr_ids, te_ids = set(map(str, split["trainSourceIds"])), set(map(str, split["heldOutSourceIds"]))
        if {splits["families"][s] for s in tr_ids}&{splits["families"][s] for s in te_ids}:
            raise ValueError("Critic source family leakage")
        tr = np.asarray([by_id[s] for s in tr_ids]); te = np.asarray([by_id[s] for s in te_ids])
        tx = np.concatenate([np.c_[contexts[tr], originals[tr]]]+[np.c_[contexts[tr], controls[k][tr]] for k in CONTROLS])
        labels = np.r_[np.ones(len(tr)), np.zeros(len(tr)*len(CONTROLS))]
        result = {"fold": split["fold"], "sourceFamilyIntersection": [], "trainingSources": len(tr), "heldOutSources": len(te), "models": {}}
        for name, estimator in (("nativeInteractionTrees", ExtraTreesClassifier(n_estimators=192, max_depth=12,
                min_samples_leaf=4, class_weight="balanced", random_state=20261110, n_jobs=1)),
            ("linearNativeFeaturesBaseline", make_pipeline(StandardScaler(), LogisticRegression(C=.5, class_weight="balanced", max_iter=1000)))):
            estimator.fit(tx, labels)
            source_p = estimator.predict_proba(np.c_[contexts[te], originals[te]])[:, 1]
            result["models"][name] = {}
            for kind in CONTROLS:
                control_p = estimator.predict_proba(np.c_[contexts[te], controls[kind][te]])[:, 1]
                result["models"][name][kind] = {"sourceVsSyntheticAUC": float(roc_auc_score(np.r_[np.ones(len(te)), np.zeros(len(te))], np.r_[source_p, control_p])),
                    "pairedSourceRanksHigherFraction": float(np.mean((source_p>control_p)+.5*(source_p==control_p)))}
        result["countMechanicsOnlyPairedBaseline"] = .5
        folds.append(result)
        print("INTERACTION_FOLD", json.dumps(result), flush=True)
    summary = {name: {kind: {metric: float(np.mean([f["models"][name][kind][metric] for f in folds]))
        for metric in folds[0]["models"][name][kind]} for kind in CONTROLS} for name in folds[0]["models"]}
    improvement = float(np.mean([summary["nativeInteractionTrees"][k]["pairedSourceRanksHigherFraction"] for k in CONTROLS])) > max(.5,
        float(np.mean([summary["linearNativeFeaturesBaseline"][k]["pairedSourceRanksHigherFraction"] for k in CONTROLS])))
    tx = np.concatenate([np.c_[contexts, originals]]+[np.c_[contexts, controls[k]] for k in CONTROLS])
    labels = np.r_[np.ones(len(levels)), np.zeros(len(levels)*len(CONTROLS))]
    final = InteractionCritic()
    final.classifier = ExtraTreesClassifier(n_estimators=192, max_depth=12, min_samples_leaf=4,
        class_weight="balanced", random_state=20261110, n_jobs=1).fit(tx, labels)
    final.hashes, final.source_ids, final.feature_names = hashes, [str(l["sourceId"]) for l in levels], FEATURE_NAMES
    final.measured_mean_paired_improvement_over_linear_baseline = improvement
    if dependencies!=local_dependency_hashes(Path(__file__).resolve()):
        raise ValueError("Critic code changed during extraction/fit")
    joblib.dump(final, model_path, compress=3)
    report_path.write_text(json.dumps({"schemaVersion":5,"status":"STATIC_NATIVE_COMPOSITION_SYNTHETIC_PROXY_ONLY","acceptedCount":0,
        "sourceCount":len(allowed),"featureNames":FEATURE_NAMES,"familyHoldoutBeforeFinalRefit":True,"folds":folds,"summary":summary,
        "countMechanicsOnlyPairedBaseline":.5,"measuredMeanPairedImprovementOverLinearBaseline":improvement,
        "hashes":{**hashes,"model":sha(model_path),"featureCache":sha(cache_path)},"localDependencySha256":dependencies,
        "elapsedSeconds":time.monotonic()-started,"syntheticControls":{"clusterSpacing":"intact authored clusters centroidradialscale.88or1.12; singlecomponent spatialhalves",
            "clusterOrientation":"one intactcluster ±18degrees; singlecomponent spatialhalf","grossPoseShuffle":"movable native poses permuted",
            "separateNativeComponents":"intactauthoredclusters centroidradialscale2;singlecomponent spatialhalves"},
        "limits":["Static pull/ring composition proxy thresholds describe geometry evidence only, not collision or release dependencies.",
            "Synthetic controls can remain good playable levels; positive ranking is original-like arrangement evidence, not quality/fun.",
            "No real newlevel negative labels or human difficulty labels; probability is not playability confidence.",
            "Original rigid sourcepartial assemblies require separate exact-copy evidence; classifier cannot certify novelty."]},indent=2),encoding="utf8")
    print("INTERACTION_COMPLETE",json.dumps({"summary":summary,"improvement":improvement}),flush=True)


if __name__=="__main__":
    import sys
    sys.modules.setdefault("interaction_critic_v5",sys.modules[__name__])
    InteractionCritic.__module__="interaction_critic_v5"
    parser=argparse.ArgumentParser();parser.add_argument("--workspace",type=Path,required=True)
    train(parser.parse_args().workspace)
