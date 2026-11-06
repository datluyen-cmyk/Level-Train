"""Isolated ordinary-corpus refit; never imports historical learned weights.

Run only after source audit prepares runs/ordinary_v4/data and its curated
source filter. Existing trainer functions are reused with actual corpus counts.
Source-family baselines are evaluated before each final model is refitted/saved.
"""
from pathlib import Path
from types import SimpleNamespace
import argparse
import ast
from importlib.metadata import version
import json
import math
import platform
import time

import joblib
import numpy as np
from sklearn.model_selection import GroupKFold

import layout_learning_v3 as local_pose
import macro_assembly_v3 as macro_native
import guide_anchor_role_motifs_v3 as guide_anchor
import guide_neighborhood_motifs_v3 as guide_base
from ordinary_source_filter_guard_v4 import validate_source_filter, sha


MODEL_NAMES = ("composition_pose.joblib", "macro_assembly.joblib", "guide_anchor_role_motifs.joblib")


def payload_compact(value):
    # Statistical fit keeps every gameplay/input/asset field except these
    # diagnostic serialized blobs; original input pack stays unchanged.
    if "templateId" in value and "kind" in value and "id" in value:
        for key in ("sourceNativeData", "sourceRigidbodyNativeData", "sourcePartData"):
            value.pop(key, None)
    return value


def local_dependency_hashes(entry):
    root, seen, pending = entry.parent, {}, [entry]
    while pending:
        path = pending.pop()
        if path.name in seen:
            continue
        seen[path.name] = sha(path)
        for node in ast.walk(ast.parse(path.read_text(encoding="utf-8-sig"))):
            names = ([alias.name.split(".")[0] for alias in node.names] if isinstance(node, ast.Import)
                     else [node.module.split(".")[0]] if isinstance(node, ast.ImportFrom) and node.module else [])
            pending.extend(root/(name+".py") for name in names if (root/(name+".py")).is_file())
    return dict(sorted(seen.items()))


def atomic_json(path, value):
    temporary = path.with_suffix(path.suffix+".tmp")
    temporary.write_text(json.dumps(value, indent=2), encoding="utf8")
    temporary.replace(path)


def validate_run_destination(workspace):
    workspace = Path(workspace).resolve()
    if workspace.parent.name != "runs" or not workspace.name.startswith("ordinary"):
        raise ValueError("Refit destination must be a dedicated runs/ordinary* folder; historical root is protected")
    for name in (*MODEL_NAMES, "composition_evaluation.json", "macro_assembly_evaluation.json", "guide_anchor_role_motifs_evaluation.json",
                 "ordinary_refit_manifest_v4.json", "ordinary_training_run_v4.json"):
        if (workspace/"models"/name).exists():
            raise ValueError("Existing ordinary run artifacts will not be overwritten; use a new dedicated run folder: "+name)
    return workspace


def macro_relation_records(levels, templates):
    """Per-source native extraction only, no full-corpus scaler/model fit."""
    relations, tiles = [], []
    for level in levels:
        native_tiles = macro_native.macro_tiles(level, templates)
        base_index = len(tiles)
        tiles.extend(native_tiles)
        for i, a in enumerate(native_tiles):
            others = sorted((j for j in range(len(native_tiles)) if j != i),
                            key=lambda j: math.dist(a["center"], native_tiles[j]["center"]))[:2]
            for j in others:
                b = native_tiles[j]
                width = max(a["width"], b["width"])
                xy = local_pose.frame(b["center"][0]-a["center"][0], b["center"][1]-a["center"][1], a["heading"])
                angle = math.radians(b["heading"]-a["heading"])
                relations.append({"sourceId": level["sourceId"], "sourceTile": base_index+i, "targetTile": base_index+j,
                    "context": local_pose.level_context(level)+a["descriptor"]+b["descriptor"],
                    "baseline": local_pose.level_context(level)+[len(a["partIds"]), len(b["partIds"])],
                    "pose": [xy[0]/width, xy[1]/width, math.cos(angle), math.sin(angle)]})
    return relations, tiles


def refit_macro(levels, templates, split_manifest, models, hashes):
    relations, tiles = macro_relation_records(levels, templates)
    # Existing baseline evaluator fits each estimator on training families only.
    evaluation = macro_native.evaluate_macro(SimpleNamespace(relations=relations), split_manifest)
    print("ORDINARY_MACRO_FAMILY_EVALUATION_COMPLETE", evaluation["summary"], flush=True)
    final = macro_native.MacroAssemblyModel().fit(levels, templates, split_manifest["families"])
    if len(final.relations) != len(relations) or len(final.tiles) != len(tiles):
        raise AssertionError("Native macro extraction changed between evaluation and final fit")
    target = models/"macro_assembly.joblib"
    joblib.dump(final, target, compress=3)
    evaluation.update(sourceCount=len(levels), actualUniqueOrdinaryCount=len({l["sourceId"] for l in levels}),
        tileCount=len(final.tiles), macroRelationCount=len(final.relations),
        evaluation="Five source-family folds with baseline before final refit on all eligible ordinary sources",
        hashes={**hashes, "sourcePack": hashes["sourcePack"], "templates": hashes["templatePack"], "model": sha(target)})
    atomic_json(models/"macro_assembly_evaluation.json", evaluation)
    return evaluation


def refit_anchor(levels, templates, split_manifest, models, hashes):
    compact_templates = {tid: guide_base.compact_object(t) for tid, t in templates.items()}
    records = [record for level in levels for record in guide_anchor.records_for(level, compact_templates)]
    families = split_manifest["families"]
    groups = [families[r["sourceId"]] for r in records]
    if len(set(groups)) < 5:
        raise ValueError("Insufficient distinct eligible guide source families for the declared five-fold evaluation; no historical model fallback")
    folds = []
    for fold, (a, b) in enumerate(GroupKFold(n_splits=5).split(records, groups=groups)):
        intersection = {groups[i] for i in a}&{groups[i] for i in b}
        if intersection:
            raise AssertionError("Source-family leakage")
        training, held = [records[i] for i in a], [records[i] for i in b]
        report = {"fold": fold, "sourceFamilyIntersection": [], "trainingSourceIds": sorted({r["sourceId"] for r in training}),
                  "heldSourceIds": sorted({r["sourceId"] for r in held})}
        for key, label in (("context", "nativeUnitGeometryAndRole"), ("baseline", "countsRelativeCentersAndGlobalProfileBaseline")):
            estimator = guide_anchor.GuideAnchorRoleModel().fit([r[key] for r in training], [r["pose"] for r in training])
            report[label] = estimator.metrics([r[key] for r in held], [r["pose"] for r in held], [r["sourceId"] for r in held])
        folds.append(report)
        print("ORDINARY_GUIDE_ANCHOR_FOLD", fold, {name: {k: v for k, v in report[name].items() if k != "perHeldSource"}
              for name in ("nativeUnitGeometryAndRole", "countsRelativeCentersAndGlobalProfileBaseline")}, flush=True)
    final = guide_anchor.GuideAnchorRoleModel().fit([r["context"] for r in records], [r["pose"] for r in records])
    target = models/"guide_anchor_role_motifs.joblib"
    joblib.dump({"model": final, "records": records, "trainingSourceCount": len(levels), "actualUniqueOrdinaryCount": len({l["sourceId"] for l in levels}),
        "sourcePackSha256": hashes["sourcePack"], "templatePackSha256": hashes["templatePack"], "sourceFilterSha256": hashes["sourceFilter"],
        "toolSha256": hashes["trainer"], "method": "Captured two-complete-native-guide-unit relative-angle proposals, ordinary curated sources only",
        "roleLimits": "Authored component/geometric role context, not a semantic-pictorial model or native gameplay difficulty certificate"}, target, compress=3)
    evaluation = {"trainingSourceCount": len(levels), "actualUniqueOrdinaryCount": len({l["sourceId"] for l in levels}),
        "recordCount": len(records), "sourceCountWithEligiblePairs": len({r["sourceId"] for r in records}), "folds": folds,
        "declaredBeforeEvaluation": {"neighbors": 32, "angularCosSinBandwidth": .35, "nearestOtherUnitsPerAnchor": 3,
            "evaluation": "Five source-family folds; average records within source then sources equally; final refit only afterward"},
        "limits": ["Ordinary source proposal support only, not native release/difficulty or player enjoyment labels.",
                   "Any below-requested training count is reported truthfully; no padding or retained pictorial influence.",
                   "Report baseline comparison honestly; no quality improvement claim when held-family metrics are worse."],
        "hashes": {**hashes, "source": hashes["sourcePack"], "templates": hashes["templatePack"], "model": sha(target)}}
    atomic_json(models/"guide_anchor_role_motifs_evaluation.json", evaluation)
    return evaluation


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", required=True, type=Path)
    args = parser.parse_args()
    w = validate_run_destination(args.workspace)
    source_path, template_path = w/"data/source500_native.json", w/"data/source_templates.json"
    filter_path = w/"data/ordinary_source_filter_v4.json"
    source_filter = json.loads(filter_path.read_text(encoding="utf8"))
    source_hash = sha(source_path)
    validate_source_filter(source_filter, source_hash)
    with source_path.open(encoding="utf8") as stream:
        pack = json.load(stream, object_hook=payload_compact)
    levels = pack["levels"]
    if any("sourceFamily" not in level for level in levels):
        raise ValueError("Filtered originals must retain sourceFamily provenance")
    ids = [str(l["sourceId"]) for l in levels]
    if len(ids) != len(set(ids)) or len({l["position"] for l in levels}) != len(levels):
        raise ValueError("Duplicate source IDs or progression positions in ordinary corpus")
    validate_source_filter(source_filter, source_hash, ids)
    templates = {t["id"]: t for t in json.loads(template_path.read_text(encoding="utf8"))["templates"]}
    missing = {p["templateId"] for l in levels for p in l["parts"]}-set(templates)
    if missing:
        raise ValueError("Ordinary source native templates missing")
    models = w/"models"
    models.mkdir(exist_ok=True)
    hashes = {"sourcePack": source_hash, "templatePack": sha(template_path), "sourceFilter": sha(filter_path), "trainer": sha(Path(__file__))}
    dependency_hashes = local_dependency_hashes(Path(__file__).resolve())
    environment = {"python": platform.python_version(),
        **{package: version(package) for package in ("numpy", "scipy", "scikit-learn", "networkx", "shapely", "joblib")}}
    started = time.monotonic()
    status = {"status": "RUNNING_LOCAL_POSE_FAMILY_EVALUATION", "workspace": str(w), "generationEnabled": False,
        "requestedTrainingSourceCount": source_filter.get("requestedTrainingSourceCount", 500), "actualUniqueOrdinaryCount": len(ids),
        "datasetExhausted": source_filter.get("datasetExhausted"), "knownApkDistinctSourceCount": source_filter.get("knownApkDistinctSourceCount"),
        "legacySourceFilename": "source500_native.json is compatibility naming, not a count claim", "hashes": hashes,
        "historicalModelsLoaded": False, "localDependencySha256": dependency_hashes,
        "environment": environment, "completedStages": []}
    progress_path = models/"ordinary_training_run_v4.json"
    atomic_json(progress_path, status)
    # Existing evaluator splits families before fitting fold estimators and only
    # refits/saves the final local pose model after held-family baseline reports.
    local_report = local_pose.evaluate(pack, templates, w)
    local_target = models/"composition_pose.joblib"
    local_report.update(sourceCount=len(levels), actualUniqueOrdinaryCount=len(ids),
        evaluation=f"Five held-out whole-source-family folds; final model refit all {len(ids)} curated ordinary originals after evaluation",
        hashes={**hashes, "model": sha(local_target), "familySplits": sha(models/"family_splits.json")})
    atomic_json(models/"composition_evaluation.json", local_report)
    status["completedStages"].append("localPoseFamilyEvaluationAndFinalRefit")
    status["status"] = "RUNNING_MACRO_FAMILY_EVALUATION"
    atomic_json(progress_path, status)
    split_manifest = json.loads((models/"family_splits.json").read_text(encoding="utf8"))
    macro_report = refit_macro(levels, templates, split_manifest, models, hashes)
    status["completedStages"].append("nativeMacroFamilyEvaluationAndFinalRefit")
    status["status"] = "RUNNING_GUIDE_ANCHOR_FAMILY_EVALUATION"
    atomic_json(progress_path, status)
    anchor_report = refit_anchor(levels, templates, split_manifest, models, hashes)
    if sha(source_path) != source_hash or sha(filter_path) != hashes["sourceFilter"] or sha(template_path) != hashes["templatePack"]:
        raise ValueError("Ordinary inputs/filter changed during training; no refit certificate will be issued")
    if local_dependency_hashes(Path(__file__).resolve()) != dependency_hashes:
        raise ValueError("Trainer/dependency source changed during training; no refit certificate will be issued")
    status["completedStages"].append("guideAnchorFamilyEvaluationAndFinalRefit")
    model_hashes = {name: {"sha256": sha(models/name), "actualUniqueOrdinaryCount": len(ids)} for name in MODEL_NAMES}
    certificate = {"schemaVersion": 4, "status": "REFIT_ON_CURATED_ORDINARY_CORPUS", "generationEnabled": False,
        "sourcePackSha256": source_hash, "sourceFilterSha256": hashes["sourceFilter"], "templatePackSha256": hashes["templatePack"],
        "trainerSha256": hashes["trainer"], "requestedTrainingSourceCount": status["requestedTrainingSourceCount"],
        "localDependencySha256": dependency_hashes, "environment": environment,
        "actualUniqueOrdinaryCount": len(ids), "datasetExhausted": status["datasetExhausted"], "ordinarySourceIds": ids,
        "models": model_hashes, "historicalModelsLoaded": False, "familyHoldoutBeforeFinalRefit": True,
        "evaluationFiles": {name: sha(models/name) for name in ("composition_evaluation.json", "macro_assembly_evaluation.json", "guide_anchor_role_motifs_evaluation.json", "family_splits.json")},
        "limits": "Curated ordinary-source training provenance only; native replay/difficulty and accepted generated levels remain unverified."}
    atomic_json(models/"ordinary_refit_manifest_v4.json", certificate)
    status.update(status="COMPLETE_REFIT_NO_GENERATION_ACCEPTANCE", elapsedSeconds=time.monotonic()-started, models=model_hashes)
    atomic_json(progress_path, status)
    print("ORDINARY_REFIT_COMPLETE", len(ids), status["elapsedSeconds"], flush=True)


if __name__ == "__main__":
    main()
