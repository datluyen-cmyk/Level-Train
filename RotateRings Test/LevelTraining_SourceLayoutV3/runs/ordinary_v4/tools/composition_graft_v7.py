"""Learn complete native atomic units at source-authored strong composition poses.

Composition edges are STATICcompositionproxy evidence only. Generated imports keep
all native authored pins and guides and cite exact source composition boundary
records; the records are not gameplay contacts or solve/difficulty labels.
"""
from pathlib import Path
from collections import defaultdict
import copy, json, math, time, argparse

import joblib
import networkx as nx
import numpy as np
from sklearn.preprocessing import StandardScaler
from sklearn.neighbors import NearestNeighbors

from global_composition_v3 import context as topology_context
from layout_learning_v3 import shape_vector, frame
from macro_assembly_v3 import tile_descriptor, instantiate_tile, pose_matrix
from native_feature_socket_v3 import body_key, feature_shape_map, remap_donor_relation
from native_asset_variant_v3 import state
from joint_native_motif_v5 import JointNativeSampler
from source_physical_composition_v3 import composition_edges
from ordinary_source_filter_guard_v4 import preflight, validate_source_filter, sha
from refit_ordinary_models_v4 import payload_compact, local_dependency_hashes

SCHEMA=7

def socket(part, templates):
    return body_key(part,templates)+":"+json.dumps(state(part),sort_keys=True,separators=(",",":"))

def atomic_units(level):
    """Connected guide units collapsed with authored directed cycles."""
    graph=nx.Graph(); graph.add_nodes_from(p["id"] for p in level["parts"])
    graph.add_edges_from((e["guideOwnerId"],e["lockedOwnerId"]) for e in level["guideLinks"])
    while True:
        groups=list(nx.connected_components(graph)); membership={i:j for j,g in enumerate(groups) for i in g}
        collapsed=nx.DiGraph(); collapsed.add_nodes_from(range(len(groups)))
        collapsed.add_edges_from((membership[e["source"]],membership[e["target"]]) for e in level["links"])
        cycles=[c for c in nx.strongly_connected_components(collapsed) if len(c)>1]
        if not cycles:return [sorted(g) for g in groups]
        for cycle in cycles:
            owners=sorted(i for j in cycle for i in groups[j]); graph.add_edges_from(zip(owners,owners[1:]))

def receiver_query(context,receiver,templates):
    return list(context)+shape_vector(templates[receiver["templateId"]])

def build_library(levels,templates):
    units=[];records=[];seeds=[];physical_by_source={};stats={"strongCompositionEdges":0,"usableUnitBoundaryPairs":0}
    for level in levels:
        ps={p["id"]:p for p in level["parts"]}; units_for={}
        for members in atomic_units(level):
            interior=set(members); edges=[(i,e) for i,e in enumerate(level["links"]) if e["source"] in interior and e["target"] not in interior]
            unit={"sourceId":str(level["sourceId"]),"partIds":members,
                "descriptor":tile_descriptor([ps[i] for i in members],templates),
                "outgoingBoundaryPartIds":sorted({e["target"] for _,e in edges}),
                "outgoingBoundaryEdgeIndices":[i for i,_ in edges],"wholeSourceUnit":len(members)==len(level["parts"])}
            idx=len(units);units.append(unit)
            for i in members:units_for[i]=idx
            if not unit["wholeSourceUnit"] and not unit["outgoingBoundaryPartIds"] and any(ps[i]["kind"] in ("ring","slider") for i in members):
                seeds.append({"unitIndex":idx,"context":topology_context(level),"sourceId":str(level["sourceId"])})
        if len(units_for)!=len(ps):raise ValueError("Atomic unit partition lost source owners")
        physical=composition_edges(level,templates);physical_by_source[str(level["sourceId"])]=physical
        for record_index,edge in enumerate(physical):
            if edge.get("strong") is not True:continue
            stats["strongCompositionEdges"]+=1
            a,b=int(edge["source"]),int(edge["target"])
            # Strong composition source is the slider and target is the ring.
            interior_owner,boundary_owner=(a,b) if a in units_for and units_for[a] != units_for.get(b) else (b,a)
            if interior_owner not in units_for or boundary_owner not in units_for:continue
            unit_index=units_for[interior_owner];unit=units[unit_index]
            if unit["wholeSourceUnit"] or interior_owner not in unit["partIds"] or boundary_owner in unit["partIds"]:continue
            donor=ps[boundary_owner]
            if donor["kind"] not in ("ring","slider"):continue
            # Every outgoing pin boundary is retained alongside this body boundary.
            boundaries=set(unit["outgoingBoundaryPartIds"]);boundaries.add(boundary_owner)
            centroid=np.mean([[ps[i]["x"],ps[i]["y"]] for i in unit["partIds"]],axis=0)
            width=max(.01,float(templates[donor["templateId"]].get("width",.5)))
            rel=np.asarray(frame(*(centroid-np.asarray([donor["x"],donor["y"]])),donor["angle"]))/width
            moving=next(ps[i] for i in unit["partIds"] if ps[i]["kind"] in ("ring","slider"))
            phi=math.radians(moving["angle"]-donor["angle"])
            record={"sourceId":str(level["sourceId"]),"unitIndex":unit_index,
                "receiverSourcePartId":boundary_owner,"receiverSocket":socket(donor,templates),
                "query":receiver_query(topology_context(level),donor,templates),
                "baselineQuery":topology_context(level),"targetDescriptor":unit["descriptor"],
                "pose":[*rel.tolist(),math.cos(phi),math.sin(phi)],
                "compositionBoundaryPair":{"interiorSourcePartId":interior_owner,"boundarySourcePartId":boundary_owner,
                    "sourceCompositionRecordIndex":record_index,"kind":edge["kind"],"evidenceLabel":edge["evidenceLabel"]},
                "requiredBoundaryPartIds":sorted(boundaries)}
            records.append(record);stats["usableUnitBoundaryPairs"]+=1
    return units,records,seeds,stats,physical_by_source

def fitted_bank(records,scaler):
    bank=JointNativeSampler();bank.records=records;bank.scaler=scaler
    bank.nn=NearestNeighbors(n_neighbors=min(32,len(records))).fit(scaler.transform([r["query"] for r in records]))
    return bank

def graft(model,unit,record,out,receiver,templates):
    """Rigidly import one source atomic unit around a cited strong composition body pair."""
    source=model.levels[str(unit["sourceId"])];ps={p["id"]:p for p in source["parts"]};interior=set(unit["partIds"])
    source_boundary=ps[int(record["receiverSourcePartId"])]
    if socket(source_boundary,templates)!=socket(receiver,templates):return None,"receiverNativeBodyStateMismatch"
    rotation=float(receiver["angle"]-source_boundary["angle"])
    translation=np.asarray([receiver["x"],receiver["y"]])-pose_matrix(rotation)@np.asarray([source_boundary["x"],source_boundary["y"]])
    boundary={}
    required=set(map(int,record["requiredBoundaryPartIds"]))
    for old in sorted(required):
        original=ps[old];xy=pose_matrix(rotation)@np.asarray([original["x"],original["y"]])+translation
        matches=[p for p in out["parts"] if socket(p,templates)==socket(original,templates)
            and np.linalg.norm(xy-np.asarray([p["x"],p["y"]]))<=1e-4
            and abs((p["angle"]-original["angle"]-rotation+180)%360-180)<=.005]
        if old==source_boundary and receiver in matches:matches=[receiver]
        if not matches:return None,"notAllNativeBoundaryOwnersMapped"
        boundary[old]=min(matches,key=lambda p:p["id"])["id"]
    if len(set(boundary.values()))!=len(boundary):return None,"distinctNativeBoundaryOwnersCollapsed"
    existing={p["id"]:p for p in out["parts"]};shape_maps={};pin_relations=defaultdict(list)
    for edge_index,edge in enumerate(source["links"]):
        if edge["source"] in boundary and edge["target"] in interior:return None,"unsupportedReceiverOutgoingIntoNewOwner"
        if edge["source"] not in interior or edge["target"] not in boundary:continue
        old=edge["target"];mapped=feature_shape_map(templates[ps[old]["templateId"]],templates[existing[boundary[old]]["templateId"]],edge["targetShapeIndices"])
        if mapped is None:return None,"exactNativePinFeatureMismatch"
        prior=shape_maps.setdefault(old,{})
        if any(i in prior and prior[i]!=j for i,j in mapped.items()):return None,"inconsistentReceiverFeatureMapping"
        prior.update(mapped);pin_relations[old].append(edge_index)
    for old,new in boundary.items():
        if old in shape_maps:continue
        st=templates[ps[old]["templateId"]];tt=templates[existing[new]["templateId"]]
        source_body=[i for i,s in enumerate(st["physicalShapes"]) if s.get("role")=="body"]
        target_body=[i for i,s in enumerate(tt["physicalShapes"]) if s.get("role")=="body"]
        mapped=feature_shape_map(st,tt,source_body)
        if mapped is None or set(mapped)!=set(source_body) or set(mapped.values())!=set(target_body):return None,"exactNativeReceiverBodyMismatch"
        shape_maps[old]=mapped
    if any(old not in shape_maps for old in boundary):return None,"unprovenNativeReceiverBoundary"
    draft=copy.deepcopy(out);model.tiles.append({"sourceId":str(unit["sourceId"]),"partIds":unit["partIds"]})
    try:mapping=instantiate_tile(model,len(model.tiles)-1,draft,rotation,translation)
    finally:model.tiles.pop()
    if len(draft["parts"])<=len(out["parts"]):return None,"noNewNativeOwner"
    draft["assemblyProvenance"]["tileImports"].pop()
    composition=record["compositionBoundaryPair"];comp_boundary=int(composition["boundarySourcePartId"])
    # Pin boundaries retain complete feature witnesses; composition-only ones
    # expose only their complete native body, as required by the independent audit.
    witnesses=[]
    for old,new in boundary.items():
        if pin_relations.get(old):
            indices=sorted(shape_maps[old]);witnesses.append({"kind":"pin","sourcePartId":old,"targetPartId":new,
                "sourceShapeIndices":indices,"targetShapeIndices":[shape_maps[old][i] for i in indices],"relationIndices":sorted(set(pin_relations[old]))})
        else:
            indices=sorted(i for i,s in enumerate(templates[ps[old]["templateId"]]["physicalShapes"]) if s.get("role")=="body")
            witnesses.append({"kind":"compositionBody","sourcePartId":old,"targetPartId":new,
                "sourceShapeIndices":indices,"targetShapeIndices":[shape_maps[old][i] for i in indices],"relationIndices":[]})
    pair={"interiorSourcePartId":int(composition["interiorSourcePartId"]),"boundarySourcePartId":comp_boundary,
        "sourceCompositionRecordIndex":int(composition["sourceCompositionRecordIndex"]),"kind":composition["kind"]}
    draft["assemblyProvenance"].setdefault("featureBoundaryImports",[]).append({"sourceId":str(unit["sourceId"]),
        "rotation":rotation,"translation":translation.tolist(),"interiorPartMapping":mapping,
        "boundaryWitnesses":witnesses,"compositionBoundaryPairs":[pair]})
    owner_map={**mapping,**boundary};actual={p["id"]:p for p in draft["parts"]}
    for edge in source["links"]:
        if edge["source"] in interior and edge["target"] in boundary:
            draft["links"].append(remap_donor_relation(edge,owner_map,shape_maps,templates,actual))
    draft["assemblyProvenance"]["decisions"].append({"kind":"learnedStrongCompositionAtomicUnitGraft","sourceId":str(unit["sourceId"]),
        "unitIndex":int(record["unitIndex"]),"sourceCompositionRecord":pair,"receiverPartId":receiver["id"],
        "boundaryPartMapping":boundary,"newOwnerPartMapping":mapping,"rotation":rotation,"translation":translation.tolist(),
        "allOutgoingBoundaryEdgeIndices":unit["outgoingBoundaryEdgeIndices"],"capturedRelativePose":record["pose"]})
    return draft,None

def train(workspace,composition_records_path=None):
    w=Path(workspace).resolve();source_filter,bindings=preflight(w)
    pack=w/"data/source500_native.json";tp=w/"data/source_templates.json"
    model_path=w/"models/composition_graft_v7.joblib";report_path=w/"models/composition_graft_v7_evaluation.json"
    if model_path.exists() or report_path.exists():raise ValueError("Refuse to overwrite v7 artifacts")
    levels=json.loads(pack.read_text(encoding="utf8"),object_hook=payload_compact)["levels"]
    allowed=validate_source_filter(source_filter,sha(pack),[l["sourceId"] for l in levels]);levels=[l for l in levels if str(l["sourceId"]) in allowed]
    templates={t["id"]:t for t in json.loads(tp.read_text(encoding="utf8"))["templates"]}
    units,records,seeds,stats,physical_by_source=build_library(levels,templates)
    # Family-held-out receiver-conditional error, with unsupported receiver classes reported.
    split_path=w/"models/family_splits.json";splits=json.loads(split_path.read_text(encoding="utf8"));folds=[]
    for split in splits["splits"]:
        tr=set(map(str,split["trainSourceIds"]));te=set(map(str,split["heldOutSourceIds"]))
        if {splits["families"][s] for s in tr}&{splits["families"][s] for s in te}:raise ValueError("Family leakage")
        training=[r for r in records if r["sourceId"] in tr];held=[r for r in records if r["sourceId"] in te]
        sockets={r["receiverSocket"] for r in training};supported=[r for r in held if r["receiverSocket"] in sockets]
        errors=[];baseline_errors=[]
        for r in supported:
            pool=[x for x in training if x["receiverSocket"]==r["receiverSocket"]]
            for query_key,destination in (("query",errors),("baselineQuery",baseline_errors)):
                x=StandardScaler().fit([v[query_key] for v in pool]);dist=np.linalg.norm(x.transform([v[query_key] for v in pool])-x.transform([r[query_key]])[0],axis=1)
                near=np.argsort(dist)[:min(32,len(pool))];weights=np.exp(-.5*dist[near]**2);weights/=weights.sum()
                pose=np.asarray([pool[i]["pose"] for i in near]);destination.append([weights@np.linalg.norm(pose[:,:2]-r["pose"][:2],axis=1),weights@np.linalg.norm(pose[:,2:]-r["pose"][2:],axis=1)])
        folds.append({"fold":split["fold"],"trainRecords":len(training),"heldRecords":len(held),"unsupportedReceiverClassRecords":len(held)-len(supported),
            "supportedMeanPoseXYErrorWidthUnits":float(np.mean([e[0] for e in errors])) if errors else None,
            "supportedMeanPoseCosSinError":float(np.mean([e[1] for e in errors])) if errors else None,
            "topologyMechanicsBaselinePoseXYErrorWidthUnits":float(np.mean([e[0] for e in baseline_errors])) if baseline_errors else None,
            "topologyMechanicsBaselinePoseCosSinError":float(np.mean([e[1] for e in baseline_errors])) if baseline_errors else None})
    if not records:raise ValueError("No ordinary-source atomic units have strong cross-unit composition evidence")
    grouped=defaultdict(list)
    for i,r in enumerate(records):r["globalRecordIndex"]=i;grouped[r["receiverSocket"]].append(r)
    scaler=StandardScaler().fit([r["query"] for r in records]);banks={k:fitted_bank(v,scaler) for k,v in grouped.items()}
    seed_scaler=StandardScaler().fit([r["context"] for r in seeds]) if seeds else None
    hashes={"sourcePack":sha(pack),"templates":sha(tp),"familySplits":sha(split_path),"trainer":sha(Path(__file__)),**bindings}
    deps=local_dependency_hashes(Path(__file__).resolve())
    artifact={"schemaVersion":SCHEMA,"status":"SOURCE_STRONG_COMPOSITION_ATOMIC_UNIT_PROPOSAL_ONLY","units":units,"records":records,"seedRecords":seeds,
        "queryScaler":scaler,"banks":banks,"seedContextScaler":seed_scaler,
        "seedNormalizedContexts":seed_scaler.transform([r["context"] for r in seeds]) if seeds else np.empty((0,0)),
        "physicalCompositionRecords":physical_by_source,"hashes":hashes,"sourceCount":len(allowed),"localDependencySha256":deps}
    joblib.dump(artifact,model_path,compress=3)
    report={"schemaVersion":SCHEMA,"sourceCount":len(allowed),"unitCount":len(units),"recordCount":len(records),"seedCount":len(seeds),"stats":stats,
        "folds":folds,"hashes":{**hashes,"model":sha(model_path)},"localDependencySha256":deps,
        "limits":["STATICcompositionproxy denotes source-pose geometry candidates, not active contacts or gameplay labels.",
        "Only exact compatible native receiver body/state classes are sampled.","Strong composition evidence does not prove solver interaction, solvability, or difficulty."]}
    report_path.write_text(json.dumps(report,indent=2),encoding="utf8")
    print("COMPOSITION_GRAFT_V7",json.dumps({"sourceCount":len(allowed),"unitCount":len(units),"recordCount":len(records),"seedCount":len(seeds),"stats":stats}),flush=True)

if __name__=="__main__":
    p=argparse.ArgumentParser();p.add_argument("--workspace",type=Path,required=True);train(p.parse_args().workspace)
