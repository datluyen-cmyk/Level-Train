"""Learn complete native incoming owner/guide-unit grafts, without XY scaffold.

Exact compatible receiver bodies anchor a captured native relation. Every new
owner's outgoing target must map to an existing receiver; pins are never cut.
"""
from pathlib import Path
from collections import defaultdict
from types import SimpleNamespace
import argparse
import copy
import json
import math
import time

import joblib
import networkx as nx
import numpy as np

from global_composition_v3 import context as topology_context, graph_for
from layout_learning_v3 import shape_vector, frame
from macro_assembly_v3 import tile_descriptor, instantiate_tile, pose_matrix
from native_feature_socket_v3 import body_key, feature_shape_map, remap_donor_relation
from native_asset_variant_v3 import state
from joint_native_motif_v5 import JointNativeSampler, evaluate
from sklearn.preprocessing import StandardScaler
from sklearn.neighbors import NearestNeighbors
from ordinary_source_filter_guard_v4 import preflight, validate_source_filter, sha
from refit_ordinary_models_v4 import payload_compact, local_dependency_hashes


def socket(part, templates):
    return body_key(part, templates)+":"+json.dumps(state(part))


def receiver_query(board_context, receiver, templates):
    return list(board_context)+shape_vector(templates[receiver["templateId"]])


def atomic_units(level):
    """Keep every guide and authored directed cycle complete, not full DAGs."""
    graph = nx.Graph()
    graph.add_nodes_from(p["id"] for p in level["parts"])
    graph.add_edges_from((e["guideOwnerId"], e["lockedOwnerId"]) for e in level["guideLinks"])
    while True:
        groups = list(nx.connected_components(graph))
        membership = {i: j for j, g in enumerate(groups) for i in g}
        collapsed = nx.DiGraph()
        collapsed.add_nodes_from(range(len(groups)))
        collapsed.add_edges_from((membership[e["source"]], membership[e["target"]]) for e in level["links"])
        cycles = [c for c in nx.strongly_connected_components(collapsed) if len(c)>1]
        if not cycles:
            return [sorted(g) for g in groups]
        for cycle in cycles:
            owners = sorted(i for j in cycle for i in groups[j])
            graph.add_edges_from(zip(owners, owners[1:]))


def build_library(levels, templates):
    units, grafts, seeds = [], [], []
    for level in levels:
        ps = {p["id"]: p for p in level["parts"]}
        for members in atomic_units(level):
            interior = set(members)
            external_edges = [(i,e) for i,e in enumerate(level["links"]) if e["source"] in interior and e["target"] not in interior]
            targets = sorted({e["target"] for _,e in external_edges})
            unit_index = len(units)
            unit = {"sourceId": level["sourceId"], "partIds": members,
                "descriptor": tile_descriptor([ps[i] for i in members], templates), "outgoingBoundaryPartIds": targets,
                "outgoingBoundaryEdgeIndices": [i for i,_ in external_edges], "wholeSourceUnit": len(members)==len(level["parts"])}
            units.append(unit)
            if unit["wholeSourceUnit"]:
                continue
            if not targets:
                if any(ps[i]["kind"] in ("ring", "slider") for i in members):
                    seeds.append({"unitIndex": unit_index, "context": topology_context(level), "sourceId": level["sourceId"]})
                continue
            for anchor_id in targets:
                anchor = ps[anchor_id]
                if anchor["kind"] not in ("ring", "slider"):
                    continue
                centroid = np.mean([[ps[i]["x"],ps[i]["y"]] for i in members],axis=0)
                width = max(.01, float(templates[anchor["templateId"]].get("width",.5)))
                xy = np.asarray(frame(*(centroid-np.asarray([anchor["x"],anchor["y"]])),anchor["angle"]))/width
                owner = ps[next(i for i in members if ps[i]["kind"] in ("ring","slider"))]
                angle = math.radians(owner["angle"]-anchor["angle"])
                context = topology_context(level)
                grafts.append({"sourceId": level["sourceId"], "unitIndex": unit_index, "receiverSourcePartId": anchor_id,
                    "receiverSocket": socket(anchor,templates), "query": receiver_query(context,anchor,templates),
                    "baselineQuery": context, "targetDescriptor": unit["descriptor"], "pose": [*xy.tolist(),math.cos(angle),math.sin(angle)]})
    return units,grafts,seeds


def compatible_samples(artifact, receiver, board_context, templates, rng, count):
    bank = artifact["banks"].get(socket(receiver,templates))
    if bank is None:
        return []
    query = receiver_query(board_context, receiver, templates)
    return [{**bank.sample(query,rng),"recipientContext":query} for _ in range(count)]


def fitted_bank(records, query_scaler, query_key="query"):
    bank=JointNativeSampler()
    bank.records=records;bank.scaler=query_scaler
    bank.nn=NearestNeighbors(n_neighbors=min(32,len(records))).fit(query_scaler.transform([r[query_key]for r in records]))
    return bank


def evaluate_supported(training, held, key):
    query_scaler=StandardScaler().fit([r[key]for r in training])
    descriptor_scaler=StandardScaler().fit([r["targetDescriptor"]for r in training])
    by_source=defaultdict(list)
    for receiver_socket in sorted({r["receiverSocket"]for r in held}):
        tr=[r for r in training if r["receiverSocket"]==receiver_socket]
        te=[r for r in held if r["receiverSocket"]==receiver_socket]
        bank=fitted_bank(tr,query_scaler,key)
        indices,weights=bank.neighbors([r[key]for r in te])
        desc=descriptor_scaler.transform([r["targetDescriptor"]for r in tr])[indices]
        actual_desc=descriptor_scaler.transform([r["targetDescriptor"]for r in te])
        descriptor_error=np.abs(desc-actual_desc[:,None,:]).mean(axis=2)
        poses=np.asarray([r["pose"]for r in tr])[indices];actual_pose=np.asarray([r["pose"]for r in te])
        xy=np.linalg.norm(poses[:,:,:2]-actual_pose[:,None,:2],axis=2)
        angular=np.linalg.norm(poses[:,:,2:]-actual_pose[:,None,2:],axis=2)
        for i,r in enumerate(te):
            by_source[str(r["sourceId"])].append([float(weights[i]@descriptor_error[i]),float(weights[i]@xy[i]),
                float(weights[i]@angular[i]),float(np.min(descriptor_error[i]+xy[i]))])
    values=np.mean([np.mean(v,axis=0)for v in by_source.values()],axis=0)
    return dict(zip(("expectedStandardizedDescriptorMAE","expectedPoseXYError","expectedPoseCosSinError","oracle32DescriptorPlusXYError"),map(float,values)))


def graft(model, units, record, out, receiver, templates):
    unit = units[record["unitIndex"]]
    source = model.levels[unit["sourceId"]]
    ps = {p["id"]:p for p in source["parts"]}
    interior = set(unit["partIds"])
    source_anchor = ps[record["receiverSourcePartId"]]
    if socket(source_anchor,templates)!=socket(receiver,templates):
        return None,"receiverBodyStateMovementMismatch"
    rotation = receiver["angle"]-source_anchor["angle"]
    translation = np.asarray([receiver["x"],receiver["y"]])-pose_matrix(rotation)@np.asarray([source_anchor["x"],source_anchor["y"]])
    boundary = {}
    for old in unit["outgoingBoundaryPartIds"]:
        original = ps[old]
        xy = pose_matrix(rotation)@np.asarray([original["x"],original["y"]])+translation
        matches = [p for p in out["parts"] if socket(p,templates)==socket(original,templates)
            and np.linalg.norm(xy-np.asarray([p["x"],p["y"]]))<=1e-4
            and abs((p["angle"]-original["angle"]-rotation+180)%360-180)<=.005]
        if not matches:
            return None,"notAllOutgoingNativeTargetsMapped"
        boundary[old] = min(matches,key=lambda p:p["id"])["id"]
    if len(set(boundary.values()))!=len(boundary):
        return None,"distinctNativeTargetsCollapsed"
    shape_maps, relations = {}, defaultdict(list)
    existing = {p["id"]:p for p in out["parts"]}
    for edge_index,edge in enumerate(source["links"]):
        if edge["source"] in boundary and edge["target"] in interior:
            return None,"unsupportedReceiverOutgoingIntoNewOwner"
        if edge["source"] not in interior or edge["target"] not in boundary:
            continue
        old = edge["target"]
        mapping = feature_shape_map(templates[ps[old]["templateId"]],templates[existing[boundary[old]]["templateId"]],edge["targetShapeIndices"])
        if mapping is None:
            return None,"exactNativeReceiverFeatureMismatch"
        prior = shape_maps.setdefault(old,{})
        if any(i in prior and prior[i]!=j for i,j in mapping.items()):
            return None,"inconsistentReceiverFeatureMapping"
        prior.update(mapping);relations[old].append(edge_index)
    if any(old not in shape_maps for old in boundary):
        return None,"unprovenReceiverSocket"
    draft = copy.deepcopy(out)
    temporary = {"sourceId":unit["sourceId"],"partIds":unit["partIds"]}
    model.tiles.append(temporary)
    try:
        mapping = instantiate_tile(model,len(model.tiles)-1,draft,rotation,translation)
    finally:
        model.tiles.pop()
    if len(draft["parts"])==len(out["parts"]):
        return None,"noNewNativeOwner"
    draft["assemblyProvenance"]["tileImports"].pop()
    witnesses = [{"kind":"pin","sourcePartId":old,"targetPartId":new,
        "sourceShapeIndices":sorted(shape_maps[old]),"targetShapeIndices":[shape_maps[old][i] for i in sorted(shape_maps[old])],
        "relationIndices":sorted(set(relations[old]))} for old,new in boundary.items()]
    draft["assemblyProvenance"].setdefault("featureBoundaryImports",[]).append({"sourceId":unit["sourceId"],
        "rotation":float(rotation),"translation":translation.tolist(),"interiorPartMapping":mapping,"boundaryWitnesses":witnesses,"compositionBoundaryPairs":[]})
    owner_map = {**mapping,**boundary}
    actual = {p["id"]:p for p in draft["parts"]}
    for edge in source["links"]:
        if edge["source"] in interior and edge["target"] in boundary:
            draft["links"].append(remap_donor_relation(edge,owner_map,shape_maps,templates,actual))
    draft["assemblyProvenance"]["decisions"].append({"kind":"learnedIncomingNativeOwnerUnitGraft","unitIndex":record["unitIndex"],
        "sourceId":unit["sourceId"],"receiverSourcePartId":record["receiverSourcePartId"],"receiverPartId":receiver["id"],
        "boundaryPartMapping":boundary,"newOwnerPartMapping":mapping,"rotation":float(rotation),"translation":translation.tolist(),
        "allOutgoingBoundaryEdgeIndices":unit["outgoingBoundaryEdgeIndices"],"capturedRelativePose":record["pose"]})
    return draft,None


def train(workspace):
    w=Path(workspace).resolve();source_filter,bindings=preflight(w)
    target=w/"models/incoming_root_grammar_v6b.joblib";report_path=w/"models/incoming_root_grammar_v6b_evaluation.json"
    if target.exists() or report_path.exists():raise ValueError("Refuse existing native incoming grammar artifacts")
    pack=w/"data/source500_native.json";template_path=w/"data/source_templates.json"
    with pack.open(encoding="utf8")as stream:levels=json.load(stream,object_hook=payload_compact)["levels"]
    allowed=validate_source_filter(source_filter,sha(pack),[l["sourceId"]for l in levels])
    templates={t["id"]:t for t in json.loads(template_path.read_text(encoding="utf8"))["templates"]}
    units,records,seeds=build_library(levels,templates)
    split_path=w/"models/family_splits.json";splits=json.loads(split_path.read_text(encoding="utf8"))
    hashes={"sourcePack":sha(pack),"templates":sha(template_path),"familySplits":sha(split_path),"trainer":sha(Path(__file__)),**bindings}
    dependencies=local_dependency_hashes(Path(__file__).resolve());started=time.monotonic();folds=[]
    for split in splits["splits"]:
        tr_ids=set(map(str,split["trainSourceIds"]));te_ids=set(map(str,split["heldOutSourceIds"]))
        if {splits["families"][s]for s in tr_ids}&{splits["families"][s]for s in te_ids}:raise ValueError("Family leakage")
        training=[r for r in records if str(r["sourceId"])in tr_ids];held=[r for r in records if str(r["sourceId"])in te_ids]
        compatible={r["receiverSocket"]for r in training};supported=[r for r in held if r["receiverSocket"]in compatible]
        # Same exact categorical compatibility applies to both estimators.
        fold={"fold":split["fold"],"familyIntersection":[],"trainingRecords":len(training),"heldRecords":len(held),
            "unsupportedNativeReceiverClassRecords":len(held)-len(supported),"models":{}}
        for method,key in (("knownReceiverNativeGeometry","query"),("topologyMechanicsProgressionBaseline","baselineQuery")):
            fold["models"][method]=evaluate_supported(training,supported,key)
        folds.append(fold);print("INCOMING_ROOT_FOLD",json.dumps(fold),flush=True)
    grouped=defaultdict(list)
    for index,record in enumerate(records):record["globalRecordIndex"]=index;grouped[record["receiverSocket"]].append(record)
    query_scaler=StandardScaler().fit([r["query"]for r in records])
    banks={key:fitted_bank(value,query_scaler)for key,value in grouped.items()}
    seed_scaler=StandardScaler().fit([r["context"]for r in seeds])
    summary={method:{metric:float(np.mean([f["models"][method][metric]for f in folds]))for metric in folds[0]["models"][method]}for method in folds[0]["models"]}
    if dependencies!=local_dependency_hashes(Path(__file__).resolve()):raise ValueError("Grammar source changed during fit")
    joblib.dump({"schemaVersion":6,"status":"EXACT_NATIVE_RECEIVER_TO_OWNER_UNIT_JOINT_PROPOSAL_ONLY","units":units,"records":records,
        "seedRecords":seeds,"seedContextScaler":seed_scaler,"seedNormalizedContexts":seed_scaler.transform([r["context"]for r in seeds]),
        "banks":banks,"hashes":hashes,"sourceCount":len(allowed),"localDependencySha256":dependencies},target,compress=3)
    report_path.write_text(json.dumps({"schemaVersion":6,"sourceCount":len(allowed),"acceptedCount":0,"unitCount":len(units),
        "properSeedCount":len(seeds),"jointGraftRecordCount":len(records),"compatibleReceiverClasses":len(banks),"summary":summary,"folds":folds,
        "familyHoldoutBeforeFinalRefit":True,"hashes":{**hashes,"model":sha(target)},"localDependencySha256":dependencies,"elapsedSeconds":time.monotonic()-started,
        "gameplayWitnessContext":"NOT_USED_CURRENT_RULE_REPLAY_PENDING; known topology/mechanics counts used instead",
        "normalization":"Query and targetdescriptor scaler pooled over ALL trainingfold records before exact compatiblebody banking; avoids nearconstant perbank numerical amplification.",
        "limits":["Atomic native guides/directed cycles are conservative support units, not asserted original game rules.",
            "All external outgoing native pin targets must map existing compatible receiver bodies; unsupported multipin poses are rejected.",
            "Categorical body/state/movement matching is exact asset support, not a rootcount/density/donor quota.",
            "No fresh physical block/release/difficulty labels; static relational reconstruction only.",
            "Tiny tutorials may have very little whole-board novelty; actual generation capacity unmeasured."]},indent=2),encoding="utf8")
    print("INCOMING_ROOT_COMPLETE",len(units),len(records),len(seeds),json.dumps(summary),flush=True)


if __name__=="__main__":
    parser=argparse.ArgumentParser();parser.add_argument("--workspace",type=Path,required=True);train(parser.parse_args().workspace)
