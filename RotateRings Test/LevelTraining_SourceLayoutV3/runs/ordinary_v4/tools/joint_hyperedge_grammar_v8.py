"""Learned joint native pin-hyperedge productions for RotateRings.

Productions import a primary atomic owner unit and any observed subset of its
whole outbound receiver units. Every outbound relation from the imported set
must then terminate inside that production or at a complete native receiver
boundary. The corpus is the source of production shapes; no topology quotas are
encoded here. Static source geometry only: this module makes no gameplay claim.
"""
from __future__ import annotations

from collections import defaultdict
from itertools import combinations
from pathlib import Path
import argparse
import copy
import json
import math

import joblib
import networkx as nx
import numpy as np
from sklearn.neighbors import NearestNeighbors
from sklearn.preprocessing import StandardScaler

from layout_learning_v3 import level_context, shape_vector
from macro_assembly_v3 import tile_descriptor, pose_matrix, instantiate_tile
from native_asset_variant_v3 import state
from native_feature_socket_v3 import body_key, feature_shape_map, remap_donor_relation
from ordinary_source_filter_guard_v4 import preflight, validate_source_filter, sha
from refit_ordinary_models_v4 import payload_compact, local_dependency_hashes

SCHEMA_VERSION = 8


def atomic_units(level):
    """Collapse complete guides and authored directed cycles into import units."""
    graph = nx.Graph()
    graph.add_nodes_from(p["id"] for p in level["parts"])
    graph.add_edges_from((e["guideOwnerId"], e["lockedOwnerId"]) for e in level.get("guideLinks", []))
    while True:
        groups = list(nx.connected_components(graph))
        membership = {part_id: i for i, group in enumerate(groups) for part_id in group}
        collapsed = nx.DiGraph()
        collapsed.add_nodes_from(range(len(groups)))
        collapsed.add_edges_from((membership[e["source"]], membership[e["target"]]) for e in level["links"])
        cycles = [c for c in nx.strongly_connected_components(collapsed) if len(c) > 1]
        if not cycles:
            return [sorted(group) for group in groups]
        for cycle in cycles:
            owners = sorted(part_id for i in cycle for part_id in groups[i])
            graph.add_edges_from(zip(owners, owners[1:]))


def receiver_socket(part, templates):
    return body_key(part, templates) + ":" + json.dumps(state(part), sort_keys=True, separators=(",", ":"))


def _subsets(values):
    values = sorted(values)
    for size in range(len(values) + 1):
        yield from combinations(values, size)


def build_productions(level, templates):
    """Enumerate observed joint pin productions over whole atomic receiver units.

    Root unit plus selected immediate target units form the imported interior.
    All other outbound targets of every imported owner become external boundary
    owners. Incoming-only edges from omitted owners are intentionally not copied.
    """
    parts = {p["id"]: p for p in level["parts"]}
    units = atomic_units(level)
    unit_for = {part_id: i for i, unit in enumerate(units) for part_id in unit}
    if len(unit_for) != len(parts):
        raise ValueError("Atomic unit partition omitted source owners")
    outgoing_units = defaultdict(set)
    for edge in level["links"]:
        a, b = unit_for[edge["source"]], unit_for[edge["target"]]
        if a != b:
            outgoing_units[a].add(b)
    result = []
    source_id = str(level["sourceId"])
    for root_index, root in enumerate(units):
        targets = outgoing_units[root_index]
        for selected in _subsets(targets):
            interior_unit_indices = {root_index, *selected}
            interior = sorted(part_id for i in interior_unit_indices for part_id in units[i])
            # Whole-board reuse supplies no constructive composition.
            if len(interior) == len(parts):
                continue
            interior_set = set(interior)
            outbound = [(i, edge) for i, edge in enumerate(level["links"])
                        if edge["source"] in interior_set and edge["target"] not in interior_set]
            boundary = sorted({edge["target"] for _, edge in outbound})
            if not boundary:
                continue
            # One record per complete external owner socket supports joint
            # rigid alignment while retaining every other required boundary.
            descriptor = tile_descriptor([parts[i] for i in interior], templates)
            centroid = np.mean([[parts[i]["x"], parts[i]["y"]] for i in interior], axis=0)
            for anchor_id in boundary:
                anchor = parts[anchor_id]
                if anchor["kind"] not in ("ring", "slider"):
                    continue
                width = max(.01, float(templates[anchor["templateId"]].get("width", .5)))
                local = pose_matrix(-anchor["angle"]) @ (centroid - np.asarray([anchor["x"], anchor["y"]])) / width
                owner = next((parts[i] for i in interior if parts[i]["kind"] in ("ring", "slider")), parts[interior[0]])
                relative_angle = math.radians(owner["angle"] - anchor["angle"])
                result.append({
                    "sourceId": source_id,
                    "rootUnitIndex": root_index,
                    "importedUnitIndices": sorted(interior_unit_indices),
                    "importedPartIds": interior,
                    "boundaryPartIds": boundary,
                    "outgoingBoundaryEdgeIndices": [i for i, _ in outbound],
                    "anchorSourcePartId": anchor_id,
                    "anchorSocket": receiver_socket(anchor, templates),
                    "query": list(level_context(level)) + shape_vector(templates[anchor["templateId"]]),
                    "baselineQuery": list(level_context(level)),
                    "descriptor": descriptor,
                    "pose": [float(local[0]), float(local[1]), math.cos(relative_angle), math.sin(relative_angle)],
                    "productionId": f"{source_id}:{root_index}:{','.join(map(str, sorted(selected)))}:{anchor_id}",
                })
    return result


def build_seed_units(levels):
    seeds = []
    for level in levels:
        parts = {p["id"]: p for p in level["parts"]}
        for unit_index, unit in enumerate(atomic_units(level)):
            member = set(unit)
            if len(unit) == len(parts) or not any(parts[i]["kind"] in ("ring", "slider") for i in unit):
                continue
            if any(e["source"] in member and e["target"] not in member for e in level["links"]):
                continue
            seeds.append({"sourceId": str(level["sourceId"]), "unitIndex": unit_index,
                          "partIds": unit, "context": list(level_context(level))})
    return seeds


def _bank(records, scaler):
    return {"records": records, "scaler": scaler,
            "nn": NearestNeighbors(n_neighbors=min(32, len(records))).fit(scaler.transform([r["query"] for r in records]))}


def evaluate_family_folds(records, split_data):
    folds = []
    for split in split_data["splits"]:
        train_ids = set(map(str, split["trainSourceIds"]))
        held_ids = set(map(str, split["heldOutSourceIds"]))
        if {split_data["families"][s] for s in train_ids} & {split_data["families"][s] for s in held_ids}:
            raise ValueError("Family leakage in V8 production evaluation")
        train = [r for r in records if r["sourceId"] in train_ids]
        held = [r for r in records if r["sourceId"] in held_ids]
        train_sockets = {r["anchorSocket"] for r in train}
        supported = [r for r in held if r["anchorSocket"] in train_sockets]
        folds.append({"fold": split["fold"], "trainingRecords": len(train), "heldOutRecords": len(held),
                      "supportedHeldOutRecords": len(supported),
                      "unsupportedNativeAnchorClassRecords": len(held) - len(supported),
                      "trainFamilies": len({split_data["families"][s] for s in train_ids}),
                      "heldOutFamilies": len({split_data["families"][s] for s in held_ids}),
                      "interpretation": "Support accounting only; no accuracy or superiority claim."})
    return folds


def fit(workspace):
    w = Path(workspace).resolve()
    source_filter, bindings = preflight(w)
    pack = w / "data/source500_native.json"
    templates_path = w / "data/source_templates.json"
    split_path = w / "models/family_splits.json"
    model_path = w / "models/joint_hyperedge_grammar_v8.joblib"
    report_path = w / "models/joint_hyperedge_grammar_v8_evaluation.json"
    if model_path.exists() or report_path.exists():
        raise FileExistsError("Refuse to overwrite V8 grammar artifacts")
    all_levels = json.loads(pack.read_text(encoding="utf8"), object_hook=payload_compact)["levels"]
    allowed = validate_source_filter(source_filter, sha(pack), [l["sourceId"] for l in all_levels])
    levels = [l for l in all_levels if str(l["sourceId"]) in allowed]
    templates = {t["id"]: t for t in json.loads(templates_path.read_text(encoding="utf8"))["templates"]}
    productions = [r for level in levels for r in build_productions(level, templates)]
    seeds = build_seed_units(levels)
    if not productions:
        raise ValueError("Corpus contains no joint native outbound productions")
    split_data = json.loads(split_path.read_text(encoding="utf8"))
    folds = evaluate_family_folds(productions, split_data)
    grouped = defaultdict(list)
    for i, record in enumerate(productions):
        record["globalRecordIndex"] = i
        grouped[record["anchorSocket"]].append(record)
    scaler = StandardScaler().fit([r["query"] for r in productions])
    banks = {key: _bank(records, scaler) for key, records in grouped.items()}
    hashes = {"sourcePack": sha(pack), "templates": sha(templates_path), "familySplits": sha(split_path),
              "trainer": sha(Path(__file__)), **bindings}
    deps = local_dependency_hashes(Path(__file__).resolve())
    seed_scaler = StandardScaler().fit([r["context"] for r in seeds]) if seeds else None
    artifact = {"schemaVersion": SCHEMA_VERSION, "status": "JOINT_NATIVE_HYPEREDGE_PROPOSAL_ONLY",
                "sourceCount": len(allowed), "records": productions, "banks": banks, "queryScaler": scaler,
                "seedRecords": seeds, "seedContextScaler": seed_scaler,
                "seedNormalizedContexts": seed_scaler.transform([r["context"] for r in seeds]) if seeds else np.empty((0, 0)),
                "hashes": hashes, "localDependencySha256": deps}
    joblib.dump(artifact, model_path, compress=3)
    report = {"schemaVersion": SCHEMA_VERSION, "sourceCount": len(allowed), "productionRecordCount": len(productions),
              "seedRecordCount": len(seeds),
              "anchorSocketCount": len(banks), "folds": folds, "hashes": {**hashes, "model": sha(model_path)},
              "localDependencySha256": deps,
              "method": "Empirical native source production sampling with exact categorical receiver banks; family-fold split reported before final refit.",
              "limits": ["Production structures and poses are copied from ordinary native sources.",
                         "No Unity collision solver, gameplay replay, accepted-level, or difficulty claim.",
                         "Complete guide and authored cycle units are never split.",
                         "All outgoing links from imported owners must be internal or mapped to exact native boundary features."]}
    report_path.write_text(json.dumps(report, indent=2), encoding="utf8")
    return artifact, report


def graft_production(model, record, out, receiver, templates):
    """Import a joint production, proving every external outgoing native pin."""
    source = model.levels[record["sourceId"]]
    source_parts = {p["id"]: p for p in source["parts"]}
    interior = set(record["importedPartIds"])
    existing_origins = {(str(p.get("atomicProvenance", {}).get("sourceId")),
                         int(p.get("atomicProvenance", {}).get("partId", -1)))
                        for p in out["parts"] if p.get("atomicProvenance")}
    duplicates = sorted(int(part_id) for part_id in interior
                        if (str(record["sourceId"]), int(part_id)) in existing_origins)
    if duplicates:
        return None, "duplicateDonorSourceOwnerOrigin"
    anchor_id = int(record["anchorSourcePartId"])
    if receiver_socket(source_parts[anchor_id], templates) != receiver_socket(receiver, templates):
        return None, "receiverNativeBodyStateMismatch"
    rotation = float(receiver["angle"] - source_parts[anchor_id]["angle"])
    translation = np.asarray([receiver["x"], receiver["y"]]) - pose_matrix(rotation) @ np.asarray([source_parts[anchor_id]["x"], source_parts[anchor_id]["y"]])
    draft = copy.deepcopy(out)
    model.tiles.append({"sourceId": record["sourceId"], "partIds": record["importedPartIds"]})
    try:
        # The alignment owner is an external boundary, not an imported root.
        # Keep it out of the interior map so provenance can certify it exactly
        # once as a separate native boundary witness.
        mapping = instantiate_tile(model, len(model.tiles) - 1, draft, rotation, translation)
    finally:
        model.tiles.pop()
    draft["assemblyProvenance"]["tileImports"].pop()
    # Recompute the complete boundary mapping after tile instantiation has
    # unified exact coincident native roots with existing owners.
    boundary = {}
    current = {p["id"]: p for p in draft["parts"]}
    for old in record["boundaryPartIds"]:
        original = source_parts[int(old)]
        xy = pose_matrix(rotation) @ np.asarray([original["x"], original["y"]]) + translation
        candidate_ids = ([mapping[old]] if old in mapping and mapping[old] in current else [])
        candidate_ids += [p["id"] for p in draft["parts"] if receiver_socket(original, templates) == receiver_socket(p, templates)
                          and np.linalg.norm(xy - np.asarray([p["x"], p["y"]])) <= 1e-4
                          and abs((p["angle"] - original["angle"] - rotation + 180) % 360 - 180) <= .005]
        candidate_ids = list(dict.fromkeys(candidate_ids))
        if not candidate_ids:
            return None, "requiredNativeBoundaryOwnerUnavailable"
        boundary[int(old)] = candidate_ids[0]
        actual = current[boundary[int(old)]]
        angle_error = abs((actual["angle"] - original["angle"] - rotation + 180) % 360 - 180)
        if receiver_socket(original, templates) != receiver_socket(actual, templates):
            return None, "exactNativeBoundarySocketMismatch"
        if np.linalg.norm(xy - np.asarray([actual["x"], actual["y"]])) > 1e-4 or angle_error > .005:
            return None, "jointBoundaryPoseMismatch"
    if len(set(boundary.values())) != len(boundary):
        return None, "distinctNativeBoundaryOwnersCollapsed"
    owner_mapping = {**mapping, **boundary}
    shape_maps, relations = {}, defaultdict(list)
    for edge_index, edge in enumerate(source["links"]):
        if edge["source"] not in interior or edge["target"] in interior:
            continue
        boundary_old = edge["target"]
        if boundary_old not in boundary:
            return None, "outgoingNativeTargetOutsideProductionBoundary"
        old = boundary_old
        mapped = feature_shape_map(templates[source_parts[old]["templateId"]],
                                   templates[current[boundary[old]]["templateId"]], edge["targetShapeIndices"])
        if mapped is None:
            return None, "exactNativeReceiverFeatureMismatch"
        prior = shape_maps.setdefault(old, {})
        if any(i in prior and prior[i] != j for i, j in mapped.items()):
            return None, "inconsistentReceiverFeatureMapping"
        prior.update(mapped)
        relations[old].append(edge_index)
    witnesses = []
    for old, new in boundary.items():
        if old not in shape_maps:
            return None, "unprovenNativeReceiverBoundary"
        indices = sorted(shape_maps[old])
        witnesses.append({"kind": "pin", "sourcePartId": old, "targetPartId": new,
                          "sourceShapeIndices": indices,
                          "targetShapeIndices": [shape_maps[old][i] for i in indices],
                          "relationIndices": sorted(set(relations[old]))})
    draft["assemblyProvenance"].setdefault("featureBoundaryImports", []).append({
        "sourceId": record["sourceId"], "rotation": rotation, "translation": translation.tolist(),
        "interiorPartMapping": mapping, "boundaryWitnesses": witnesses, "compositionBoundaryPairs": []})
    actual = {p["id"]: p for p in draft["parts"]}
    seen = {(e["source"], e["target"], tuple(e["sourceShapeIndices"]), tuple(e["targetShapeIndices"])) for e in draft["links"]}
    copied_edges = []
    for edge_index, edge in enumerate(source["links"]):
        if edge["source"] not in interior or edge["target"] in interior:
            continue
        if edge["source"] not in owner_mapping or edge["target"] not in owner_mapping:
            return None, "outgoingNativeTargetUnmapped"
        new_edge = remap_donor_relation(edge, owner_mapping, shape_maps, templates, actual)
        key = (new_edge["source"], new_edge["target"], tuple(new_edge["sourceShapeIndices"]), tuple(new_edge["targetShapeIndices"]))
        if key not in seen:
            draft["links"].append(new_edge)
            seen.add(key)
        copied_edges.append(edge_index)
    expected = sorted(i for i, e in enumerate(source["links"])
                      if e["source"] in interior and e["target"] not in interior)
    if sorted(copied_edges) != expected:
        return None, "nativeBoundaryContactProvenanceIncomplete"
    draft["assemblyProvenance"]["decisions"].append({
        "kind": "learnedJointNativeHyperedgeProductionV8", "sourceId": record["sourceId"],
        "productionId": record["productionId"], "globalRecordIndex": record.get("globalRecordIndex"),
        "importedUnitIndices": record["importedUnitIndices"], "importedPartIds": record["importedPartIds"],
        "boundaryPartMapping": boundary, "newOwnerPartMapping": mapping,
        "allOutgoingBoundaryEdgeIndices": expected, "copiedExternalEdgeIndices": sorted(copied_edges),
        "rotation": rotation, "translation": translation.tolist()})
    return draft, None


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("--workspace", type=Path, required=True)
    p.add_argument("--fit", action="store_true", help="Fit the V8 proposal model from the configured ordinary corpus")
    a = p.parse_args()
    artifact, report = fit(a.workspace)
    print(json.dumps({"model": str((a.workspace / "models/joint_hyperedge_grammar_v8.joblib").resolve()),
                      "records": report["productionRecordCount"], "familiesEvaluated": len(report["folds"])}, indent=2))


if __name__ == "__main__":
    main()
