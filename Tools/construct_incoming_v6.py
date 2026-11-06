"""Finite from-empty incoming native socket prototype constructor; no Unity."""
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

from incoming_root_grammar_v6 import graft,compatible_samples
from global_composition_v3 import context as topology_context
from layout_learning_v3 import level_context
from composition_field_v3 import native_frame,stroke_heading
from macro_assembly_v3 import instantiate_tile,pose_matrix
from constructive_layout_v5 import empty_level,score
from assembly_integrity_audit_v3 import NativeAssemblyIntegrityAudit
from assembly_contact_audit_v3 import NativeGeometrySeamAudit
from layout_novelty_semantic_v3 import canonical_fingerprint,LayoutNoveltyIndex
from ordinary_source_filter_guard_v4 import preflight,validate_source_filter,sha
from refit_ordinary_models_v4 import payload_compact,local_dependency_hashes


def source_profile_target(reference,organization,templates,rng):
    target=organization.sample(reference["position"],rng)
    target.update(targetParts=float(len(reference["parts"])),span=native_frame(reference,templates,stroke_heading(reference))[2],
        context=level_context(reference),referenceProfileSourceId=reference["sourceId"],
        profileConditioning="Exact reference scalar count/span/mechanics only; no source parts/coordinates/scaffold copied.")
    return target


def grow_options(artifact,model,units,out,board_context,templates,rng,count,rejects):
    receivers=[p for p in out["parts"]if p["kind"]in("ring","slider")]
    options=[]
    for _ in range(count):
        receiver=receivers[int(rng.integers(len(receivers)))]
        selected=compatible_samples(artifact,receiver,board_context,templates,rng,1)
        if not selected:
            rejects["noLearnedCompatibleReceiverBank"]+=1;continue
        selected=selected[0]
        draft,error=graft(model,units,selected["record"],out,receiver,templates)
        if draft is None:
            rejects[error]+=1;continue
        draft["assemblyProvenance"]["decisions"][-1].update(grammarModelSha256=out["assemblyProvenance"]["modelHashes"]["incomingRootGrammar"],
            learnedGlobalRecordIndex=selected["record"]["globalRecordIndex"],learnedBankRecordIndex=selected["recordIndex"],
            neighborProbability=selected["neighborProbability"],recipientContext=selected["recipientContext"])
        options.append(draft)
    return options


def construct(artifact,model,reference,organization,critic,templates,integrity,seam,canonical,rng,number,budget,hashes):
    target=source_profile_target(reference,organization,templates,rng)
    base=empty_level(number,reference["position"],target,hashes)
    base["sourceId"]="incoming-v6-"+str(number);base["sourceFamily"]="ordinary-incoming-native-v6"
    base["assemblyProvenance"]["method"]="From-empty complete native headless receiver followed by learned incoming owner/guide-unit socket grafts"
    base["assemblyProvenance"]["originalBoardUsedAsScaffold"]=False
    board_context=topology_context(reference)
    seeds=artifact["seedRecords"];normalized=artifact["seedContextScaler"].transform([board_context])[0]
    distance=np.linalg.norm(artifact["seedNormalizedContexts"]-normalized,axis=1)
    logits=-.5*distance**2;probability=np.exp(logits-logsumexp(logits))
    seed_indices=rng.choice(len(seeds),size=min(budget["seedOptions"],len(seeds)),replace=False,p=probability)
    rejects=Counter();frontier=[]
    for index in seed_indices:
        seed=seeds[int(index)];unit=artifact["units"][seed["unitIndex"]]
        ps={p["id"]:p for p in model.levels[unit["sourceId"]]["parts"]}
        anchor=next(ps[i]for i in unit["partIds"]if ps[i]["kind"]in("ring","slider"))
        rotation=-anchor["angle"];translation=-pose_matrix(rotation)@np.asarray([anchor["x"],anchor["y"]])
        out=copy.deepcopy(base);mapping=instantiate_tile(model,seed["unitIndex"],out,rotation,translation)
        out["assemblyProvenance"]["decisions"].append({"kind":"headlessNativeReceiverUnitSeed","unitIndex":seed["unitIndex"],
            "sourceId":unit["sourceId"],"partMapping":mapping,"rotation":float(rotation),"translation":translation.tolist(),
            "seedRecordIndex":int(index),"seedProbability":float(probability[index]),"recipientTopologyContext":board_context})
        for draft in grow_options(artifact,model,artifact["units"],out,board_context,templates,rng,budget["initialGraftOptions"],rejects):
            native,raw=integrity.check(draft),seam.check(draft)
            if not native["nativeAssemblyIntegrityPassed"]:
                rejects["initialNativeIntegrity"]+=1;continue
            if not raw["rawGeometrySeamPassed"]:
                rejects["initialRawGeometry"]+=1;continue
            value,metrics=score(draft,target,organization,templates,critic)
            if not math.isfinite(value):
                rejects["unsupportedCriticPrefix"]+=1;continue
            frontier.append((value,draft,metrics,native,raw))
    if not frontier:return None,{"status":"NO_SUPPORTED_INITIAL_NATIVE_GRAFT","rejections":dict(rejects)}
    value,out,metrics,native,raw=min(frontier,key=lambda x:x[0])
    best=None;best_value=float("inf");best_metrics=None
    for step in range(budget["frontierSteps"]+1):
        fingerprint=canonical_fingerprint(out,templates)
        if fingerprint not in canonical and value<best_value:
            best,best_value,best_metrics=copy.deepcopy(out),value,metrics
            best["constructiveV6"]={"accepted":False,"nativeIntegrity":native,"rawGeometry":raw,"organizationScore":metrics,
                "sourceScaffoldUsed":False,"numericContentGatesApplied":[],"semanticCanonicalDuplicate":False,
                "nativeReplayStatus":"PENDING","difficultyStatus":"UNMEASURED","completeOutgoingContactsMapped":True}
        elif fingerprint in canonical:rejects["wholeBoardCanonicalSourceOrPreviousEquivalent"]+=1
        print("INCOMING_PREFIX",number,step,len(out["parts"]),round(value,6),"best",round(best_value,6),flush=True)
        if step==budget["frontierSteps"]:break
        frontier=[]
        for draft in grow_options(artifact,model,artifact["units"],out,board_context,templates,rng,budget["graftOptionsPerStep"],rejects):
            native,raw=integrity.check(draft),seam.check(draft)
            if not native["nativeAssemblyIntegrityPassed"]:rejects["frontierNativeIntegrity"]+=1;continue
            if not raw["rawGeometrySeamPassed"]:rejects["frontierRawGeometry"]+=1;continue
            value,metrics=score(draft,target,organization,templates,critic)
            if math.isfinite(value):frontier.append((value,draft,metrics,native,raw))
        if not frontier:break
        value,out,metrics,native,raw=min(frontier,key=lambda x:x[0])
    return best,{"status":"PRELIMINARY_NATIVE_INCOMING_GRAFT"if best else"NO_NOVEL_PREFIX_WITHIN_FINITE_SUPPORT",
        "rejections":dict(rejects),"bestOrganizationScore":best_metrics}


def main():
    p=argparse.ArgumentParser();p.add_argument("--workspace",type=Path,required=True);p.add_argument("--positions",nargs="+",type=int,required=True)
    p.add_argument("--attempts-per-position",type=int,default=1);p.add_argument("--seed",type=int,default=20261113)
    p.add_argument("--id-base",type=int,default=9600000);p.add_argument("--seed-options",type=int,default=12)
    p.add_argument("--initial-graft-options",type=int,default=12);p.add_argument("--frontier-steps",type=int,default=12)
    p.add_argument("--graft-options-per-step",type=int,default=48);p.add_argument("--output",type=Path,required=True)
    args=p.parse_args();w=args.workspace.resolve()
    if args.output.exists():raise ValueError("Refuse existing incoming prototype pack")
    source_filter,bindings=preflight(w);source_path=w/"data/source500_native.json";template_path=w/"data/source_templates.json"
    with source_path.open(encoding="utf8")as stream:levels=json.load(stream,object_hook=payload_compact)["levels"]
    allowed=validate_source_filter(source_filter,sha(source_path),[l["sourceId"]for l in levels]);by_position={l["position"]:l for l in levels}
    templates={t["id"]:t for t in json.loads(template_path.read_text(encoding="utf8"))["templates"]}
    paths={"incomingRootGrammar":w/"models/incoming_root_grammar_v6b.joblib","organizationModel":w/"models/organization_conditioned_v5.joblib",
        "interactionCriticModel":w/"models/interaction_critic_v5.joblib"}
    artifact=joblib.load(paths["incomingRootGrammar"]);organization=joblib.load(paths["organizationModel"]);critic=joblib.load(paths["interactionCriticModel"])
    if artifact["hashes"]["sourcePack"]!=sha(source_path)or artifact["hashes"]["templates"]!=sha(template_path):raise ValueError("Stale native incoming model")
    if any(str(r["sourceId"])not in allowed for r in artifact["records"]):raise ValueError("Excluded incoming source influence")
    model=SimpleNamespace(levels={str(l["sourceId"]):l for l in levels},templates=templates,tiles=copy.deepcopy(artifact["units"]))
    extra=json.loads((w/"data/novelty_extras/source_parity_pack.json").read_text(encoding="utf8"))["levels"]
    all_templates=dict(templates);all_templates.update({t["id"]:t for t in json.loads((w/"data/novelty_extras/source_templates.json").read_text(encoding="utf8"))["templates"]})
    canonical={canonical_fingerprint(l,all_templates)for l in levels+extra};novelty=LayoutNoveltyIndex(levels+extra,all_templates)
    integrity,seam=NativeAssemblyIntegrityAudit(templates,levels),NativeGeometrySeamAudit(templates,levels)
    hashes={"sourcePack":sha(source_path),"templates":sha(template_path),**{k:sha(v)for k,v in paths.items()},"tool":sha(Path(__file__)),**bindings}
    budget={"seedOptions":args.seed_options,"initialGraftOptions":args.initial_graft_options,"frontierSteps":args.frontier_steps,"graftOptionsPerStep":args.graft_options_per_step}
    rng=np.random.default_rng(args.seed);candidates=[];reports=[];started=time.monotonic();ordinal=0
    for position in args.positions:
        for attempt in range(args.attempts_per_position):
            candidate,report=construct(artifact,model,by_position[position],organization,critic,templates,integrity,seam,canonical,rng,args.id_base+ordinal,budget,hashes)
            ordinal+=1;report.update(position=position,attempt=attempt)
            if candidate:
                candidate["constructiveV6"]["heuristicNearCopyReviewOnly"]=novelty.check(candidate)
                candidates.append(candidate);canonical.add(canonical_fingerprint(candidate,templates));report["candidateId"]=candidate["id"]
            reports.append(report);print("INCOMING_ATTEMPT",position,attempt,report["status"],flush=True)
            args.output.parent.mkdir(parents=True,exist_ok=True)
            args.output.with_suffix(".checkpoint.json").write_text(json.dumps({"schemaVersion":6,"complete":False,"acceptedCount":0,"levels":candidates,"reports":reports,
                "hashes":hashes,"seed":args.seed,"declaredSearchBudget":budget},separators=(",",":")),encoding="utf8")
    args.output.write_text(json.dumps({"schemaVersion":6,"status":"FROM_EMPTY_INCOMING_NATIVE_SOCKET_PROTOTYPES_PENDING_REPLAY_DIFFICULTY","complete":False,
        "acceptedCount":0,"sourceScaffoldUsed":False,"numericContentGatesApplied":[],"levels":candidates,"reports":reports,"hashes":hashes,"seed":args.seed,
        "declaredSearchBudget":budget,"elapsedSeconds":time.monotonic()-started,"localDependencySha256":local_dependency_hashes(Path(__file__).resolve()),
        "limits":["All authored outgoing native targets map compatible existing sockets; no claim of dynamic release or solved native play.",
            "Prototype difficulty and15percent reference agreement remain unmeasured; source profile scalars are soft conditioning only.",
            "Critic probability is a static synthetic proxy; user feedback and actual production replay remain necessary.",
            "Sourcepartial restoration and tinytutorial novelty limits need independent review."]},separators=(",",":")),encoding="utf8")
    print("INCOMING_COMPLETE",len(candidates),time.monotonic()-started,flush=True)


if __name__=="__main__":main()
