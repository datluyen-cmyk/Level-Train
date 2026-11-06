"""Beam native incoming socket generation with learned soft interaction stats.

Reference supplies scalar statistics only. Beam width/options are compute
budgets; they do not set root counts, density, source quotas or difficulty.
"""
from pathlib import Path
from types import SimpleNamespace
from collections import Counter
import argparse
import copy
import json
import math
import time

import joblib
import numpy as np
from scipy.special import logsumexp
from sklearn.preprocessing import StandardScaler

from construct_incoming_v6 import source_profile_target,grow_options
from constructive_layout_v5 import empty_level,score
from global_composition_v3 import context as topology_context
from interaction_critic_v5 import features
from layout_learning_v3 import level_context
from composition_field_v3 import native_frame,stroke_heading
from macro_assembly_v3 import instantiate_tile,pose_matrix
from assembly_integrity_audit_v3 import NativeAssemblyIntegrityAudit
from assembly_contact_audit_v3 import NativeGeometrySeamAudit
from layout_novelty_semantic_v3 import canonical_fingerprint,LayoutNoveltyIndex
from ordinary_source_filter_guard_v4 import preflight,validate_source_filter,sha
from refit_ordinary_models_v4 import payload_compact,local_dependency_hashes


def scalar_profile(level,templates,native_features=None):
    value=features(level,templates)if native_features is None else native_features
    span=native_frame(level,templates,stroke_heading(level))[2]
    return np.r_[math.log(max(span,.01)),topology_context(level),value]


def load_statistical_scale(w,levels,templates):
    cache_path=w/"models/interaction_critic_v5_features.joblib"
    path=w/"models/incoming_interaction_profile_scale_v6.joblib"
    bindings={"sourcePack":sha(w/"data/source500_native.json"),"templates":sha(w/"data/source_templates.json"),
        "nativeFeatureCache":sha(cache_path),"extractor":sha(Path(__file__))}
    if path.exists():
        model=joblib.load(path)
        if model["hashes"]!=bindings:raise ValueError("Stale interaction scalar-profile normalization")
        return model,path
    cache=joblib.load(cache_path)
    if cache["hashes"]["sourcePack"]!=bindings["sourcePack"]:raise ValueError("Stale ordinary feature cache")
    vectors=np.asarray([scalar_profile(l,templates,v)for l,v in zip(levels,cache["originals"])])
    model={"scaler":StandardScaler().fit(vectors),"hashes":bindings,"sourceIds":[l["sourceId"]for l in levels],
        "profileVectors":vectors,"status":"LEARNED_SCALAR_NORMALIZATION_NOT_DIFFICULTY_MODEL","acceptedCount":0}
    joblib.dump(model,path,compress=3)
    return model,path


def construct(artifact,model,reference,organization,critic,profile_scale,templates,integrity,seam,canonical,rng,number,budget,hashes):
    target=source_profile_target(reference,organization,templates,rng)
    target_scalar=scalar_profile(reference,templates)
    target_z=profile_scale["scaler"].transform([target_scalar])[0]
    base=empty_level(number,reference["position"],target,hashes)
    base["sourceId"]="incoming-beam-v6-"+str(number);base["sourceFamily"]="ordinary-incoming-native-beam-v6"
    base["assemblyProvenance"].update(method="From-empty multi-frontier native incoming owner-unit beam with soft scalar interaction statistics",
        referenceScalarInteractionStatistics=target_scalar.tolist(),referenceOriginalCoordinatesUsed=False)
    board_context=topology_context(reference)
    seeds=artifact["seedRecords"];normalized=artifact["seedContextScaler"].transform([board_context])[0]
    distance=np.linalg.norm(artifact["seedNormalizedContexts"]-normalized,axis=1)
    logits=-.5*distance**2;probability=np.exp(logits-logsumexp(logits))
    seed_indices=rng.choice(len(seeds),size=min(budget["seedOptions"],len(seeds)),replace=False,p=probability)
    rejects=Counter();pending=[]
    for index in seed_indices:
        seed=seeds[int(index)];unit=artifact["units"][seed["unitIndex"]]
        ps={p["id"]:p for p in model.levels[unit["sourceId"]]["parts"]}
        anchor=next(ps[i]for i in unit["partIds"]if ps[i]["kind"]in("ring","slider"))
        rotation=-anchor["angle"];translation=-pose_matrix(rotation)@np.asarray([anchor["x"],anchor["y"]])
        out=copy.deepcopy(base);mapping=instantiate_tile(model,seed["unitIndex"],out,rotation,translation)
        out["assemblyProvenance"]["decisions"].append({"kind":"headlessNativeReceiverUnitSeed","unitIndex":seed["unitIndex"],
            "sourceId":unit["sourceId"],"partMapping":mapping,"rotation":float(rotation),"translation":translation.tolist(),
            "seedRecordIndex":int(index),"seedProbability":float(probability[index]),"recipientTopologyContext":board_context})
        pending.extend(grow_options(artifact,model,artifact["units"],out,board_context,templates,rng,budget["initialGraftOptions"],rejects))
    best=None;best_value=float("inf");visited=set();prefixes=[]
    for step in range(budget["frontierSteps"]+1):
        ranked=[]
        for draft in pending:
            fingerprint=canonical_fingerprint(draft,templates)
            if fingerprint in visited:continue
            visited.add(fingerprint)
            native,raw=integrity.check(draft),seam.check(draft)
            if not native["nativeAssemblyIntegrityPassed"]:rejects["nativeIntegrity"]+=1;continue
            if not raw["rawGeometrySeamPassed"]:rejects["rawGeometry"]+=1;continue
            old_value,metrics=score(draft,target,organization,templates,critic)
            if not math.isfinite(old_value):rejects["unsupportedCriticPrefix"]+=1;continue
            actual_span=native_frame(draft,templates,stroke_heading(draft))[2]
            actual=np.r_[math.log(max(actual_span,.01)),topology_context(draft),metrics["staticInteractionFeatures"]]
            z=profile_scale["scaler"].transform([actual])[0]
            scalar_error=float(np.abs(z-target_z).mean())
            value=-math.log(max(metrics["nativeStaticCompositionProbability"],np.finfo(float).tiny))+scalar_error
            metrics.update(rankStrategy="Beam joint native incoming grammar; original-vs-synthetic critic plus learned-standardized exact-reference scalar interaction profile",
                rankScore=value,referenceScalarProfileMAE=scalar_error,scalarProfileContributions={"logSpan":float(abs(z[0]-target_z[0])/len(z)),
                    "knownTopologyMechanics":float(np.abs(z[1:17]-target_z[1:17]).sum()/len(z)),
                    "nativeArrangementAndStaticInteractions":float(np.abs(z[17:]-target_z[17:]).sum()/len(z))},
                referenceCoordinatesUsed=False,referenceScalarStatisticsNotDifficultyProof=True)
            ranked.append((value,draft,metrics,native,raw,fingerprint))
            if fingerprint not in canonical and value<best_value:
                best,best_value=copy.deepcopy(draft),value
                best["constructiveV6"]={"accepted":False,"nativeIntegrity":native,"rawGeometry":raw,"organizationScore":metrics,
                    "sourceScaffoldUsed":False,"numericContentGatesApplied":[],"semanticCanonicalDuplicate":False,
                    "nativeReplayStatus":"PENDING","difficultyStatus":"UNMEASURED","completeOutgoingContactsMapped":True}
            elif fingerprint in canonical:rejects["wholeBoardCanonicalSourceOrPreviousEquivalent"]+=1
        if not ranked:break
        ranked.sort(key=lambda row:row[0]);beam=ranked[:budget["beamWidth"]]
        prefixes.append({"step":step,"supportedPrefixes":len(ranked),"beamRootCounts":[len(r[1]["parts"])for r in beam],
            "bestScore":best_value if math.isfinite(best_value)else None})
        print("INCOMING_BEAM",number,step,prefixes[-1],flush=True)
        if step==budget["frontierSteps"]:break
        pending=[]
        for _,out,*_ in beam:
            pending.extend(grow_options(artifact,model,artifact["units"],out,board_context,templates,rng,budget["graftOptionsPerBeam"],rejects))
    return best,{"status":"PRELIMINARY_NATIVE_INCOMING_BEAM"if best else"NO_NOVEL_SUPPORTED_PREFIX",
        "rejections":dict(rejects),"beamProgress":prefixes,"visitedCanonicalPrefixes":len(visited)}


def main():
    p=argparse.ArgumentParser();p.add_argument("--workspace",type=Path,required=True);p.add_argument("--positions",nargs="+",type=int,required=True)
    p.add_argument("--seed",type=int,default=20261115);p.add_argument("--id-base",type=int,default=9800000)
    p.add_argument("--seed-options",type=int,default=12);p.add_argument("--initial-graft-options",type=int,default=8)
    p.add_argument("--beam-width",type=int,default=6);p.add_argument("--frontier-steps",type=int,default=14)
    p.add_argument("--graft-options-per-beam",type=int,default=16);p.add_argument("--output",type=Path,required=True)
    args=p.parse_args();w=args.workspace.resolve()
    if args.output.exists():raise ValueError("Refuse existing incoming beam prototype pack")
    source_filter,bindings=preflight(w);source_path=w/"data/source500_native.json";template_path=w/"data/source_templates.json"
    with source_path.open(encoding="utf8")as stream:levels=json.load(stream,object_hook=payload_compact)["levels"]
    allowed=validate_source_filter(source_filter,sha(source_path),[l["sourceId"]for l in levels]);by_position={l["position"]:l for l in levels}
    templates={t["id"]:t for t in json.loads(template_path.read_text(encoding="utf8"))["templates"]}
    paths={"incomingRootGrammar":w/"models/incoming_root_grammar_v6b.joblib","organizationModel":w/"models/organization_conditioned_v5.joblib",
        "interactionCriticModel":w/"models/interaction_critic_v5.joblib"}
    artifact=joblib.load(paths["incomingRootGrammar"]);organization=joblib.load(paths["organizationModel"]);critic=joblib.load(paths["interactionCriticModel"])
    if artifact["hashes"]["sourcePack"]!=sha(source_path)or artifact["hashes"]["templates"]!=sha(template_path):raise ValueError("Stale native incoming model")
    if any(str(r["sourceId"])not in allowed for r in artifact["records"]):raise ValueError("Excluded incoming source influence")
    profile_scale,profile_path=load_statistical_scale(w,levels,templates);paths["scalarInteractionProfileScale"]=profile_path
    model=SimpleNamespace(levels={str(l["sourceId"]):l for l in levels},templates=templates,tiles=copy.deepcopy(artifact["units"]))
    extra=json.loads((w/"data/novelty_extras/source_parity_pack.json").read_text(encoding="utf8"))["levels"]
    all_templates=dict(templates);all_templates.update({t["id"]:t for t in json.loads((w/"data/novelty_extras/source_templates.json").read_text(encoding="utf8"))["templates"]})
    canonical={canonical_fingerprint(l,all_templates)for l in levels+extra};novelty=LayoutNoveltyIndex(levels+extra,all_templates)
    integrity,seam=NativeAssemblyIntegrityAudit(templates,levels),NativeGeometrySeamAudit(templates,levels)
    hashes={"sourcePack":sha(source_path),"templates":sha(template_path),**{k:sha(v)for k,v in paths.items()},"tool":sha(Path(__file__)),**bindings}
    budget={"seedOptions":args.seed_options,"initialGraftOptions":args.initial_graft_options,"frontierSteps":args.frontier_steps,
        "beamWidth":args.beam_width,"graftOptionsPerBeam":args.graft_options_per_beam}
    rng=np.random.default_rng(args.seed);candidates=[];reports=[];started=time.monotonic()
    for ordinal,position in enumerate(args.positions):
        candidate,report=construct(artifact,model,by_position[position],organization,critic,profile_scale,templates,integrity,seam,canonical,rng,args.id_base+ordinal,budget,hashes)
        report.update(position=position)
        if candidate:
            candidate["constructiveV6"]["heuristicNearCopyReviewOnly"]=novelty.check(candidate)
            candidates.append(candidate);canonical.add(canonical_fingerprint(candidate,templates));report["candidateId"]=candidate["id"]
        reports.append(report);print("INCOMING_BEAM_ATTEMPT",position,report["status"],flush=True)
        args.output.parent.mkdir(parents=True,exist_ok=True)
        args.output.with_suffix(".checkpoint.json").write_text(json.dumps({"schemaVersion":6,"complete":False,"acceptedCount":0,"levels":candidates,"reports":reports,
            "hashes":hashes,"seed":args.seed,"declaredSearchBudget":budget},separators=(",",":")),encoding="utf8")
    args.output.write_text(json.dumps({"schemaVersion":6,"status":"FROM_EMPTY_INCOMING_NATIVE_BEAM_PROTOTYPES_PENDING_REPLAY_DIFFICULTY","complete":False,
        "acceptedCount":0,"sourceScaffoldUsed":False,"numericContentGatesApplied":[],"levels":candidates,"reports":reports,"hashes":hashes,"seed":args.seed,
        "declaredSearchBudget":budget,"elapsedSeconds":time.monotonic()-started,"localDependencySha256":local_dependency_hashes(Path(__file__).resolve()),
        "limits":["Learned scalar normalization is not a player difficulty model; direct reference scalar statistics provide no original coordinate scaffold.",
            "Native contacts, original-vs-synthetic critic and beam search do not establish solution, enjoyment or15percent difficulty match.",
            "Beam width/options/steps are finite compute budgets, never target root count/density/donor limits."]},separators=(",",":")),encoding="utf8")
    print("INCOMING_BEAM_COMPLETE",len(candidates),time.monotonic()-started,flush=True)


if __name__=="__main__":main()
