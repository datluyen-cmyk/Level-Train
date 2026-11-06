"""Finite from-empty composition-graft prototypes; intentionally unvalidated."""
from pathlib import Path
from types import SimpleNamespace
from collections import Counter
import argparse, copy, json, math
import joblib, numpy as np
from scipy.special import logsumexp

from composition_graft_v7 import graft, socket, receiver_query
from global_composition_v3 import context as topology_context
from layout_learning_v3 import level_context
from macro_assembly_v3 import instantiate_tile,pose_matrix
from constructive_layout_v5 import empty_level,score
from assembly_integrity_audit_v3 import NativeAssemblyIntegrityAudit
from assembly_contact_audit_v3 import NativeGeometrySeamAudit
from layout_novelty_semantic_v3 import canonical_fingerprint, LayoutNoveltyIndex
from ordinary_source_filter_guard_v4 import preflight,validate_source_filter,sha
from refit_ordinary_models_v4 import payload_compact,local_dependency_hashes

def _sample_seed(artifact,board_context,rng,count):
    seeds=artifact["seedRecords"]
    if not seeds:return []
    scaler=artifact.get("seedContextScaler")
    if scaler is None:return list(rng.choice(len(seeds),size=min(count,len(seeds)),replace=False))
    # Older v7 schemas may bind the normalized seed contexts directly.
    distances=np.linalg.norm(artifact["seedNormalizedContexts"]-scaler.transform([board_context])[0],axis=1)
    logits=-.5*distances**2;weights=np.exp(logits-logsumexp(logits))
    return list(map(int,rng.choice(len(seeds),size=min(count,len(seeds)),replace=False,p=weights)))

def _seed(artifact,model,base,index,board_context):
    record=artifact["seedRecords"][index];unit=artifact["units"][record["unitIndex"]]
    level=model.levels[str(unit["sourceId"])];ps={p["id"]:p for p in level["parts"]}
    anchor=next(ps[i] for i in unit["partIds"] if ps[i]["kind"] in ("ring","slider"))
    rotation=-anchor["angle"];translation=-pose_matrix(rotation)@np.asarray([anchor["x"],anchor["y"]])
    out=copy.deepcopy(base);mapping=instantiate_tile(model,record["unitIndex"],out,rotation,translation)
    out["assemblyProvenance"]["decisions"].append({"kind":"headlessNativeAtomicUnitSeed","unitIndex":record["unitIndex"],
        "sourceId":str(unit["sourceId"]),"partMapping":mapping,"rotation":float(rotation),"translation":translation.tolist(),
        "seedRecordIndex":int(index),"recipientTopologyContext":board_context})
    return out

def _grow(artifact,model,out,board_context,templates,rng,count,rejects,model_sha):
    options=[];receivers=[p for p in out["parts"] if p["kind"] in ("ring","slider")]
    if not receivers:return options
    for _ in range(count):
        receiver=receivers[int(rng.integers(len(receivers)))];bank=artifact["banks"].get(socket(receiver,templates))
        if bank is None:rejects["noStrongCompositionReceiverBank"]+=1;continue
        query=receiver_query(board_context,receiver,templates)
        selected=bank.sample(query,rng);record=selected["record"];unit=artifact["units"][record["unitIndex"]]
        draft,error=graft(model,unit,record,out,receiver,templates)
        if draft is None:rejects[error]+=1;continue
        draft["assemblyProvenance"]["decisions"][-1].update(grammarModelSha256=model_sha,
            learnedGlobalRecordIndex=record["globalRecordIndex"],learnedBankRecordIndex=selected["recordIndex"],
            neighborProbability=selected["neighborProbability"],recipientContext=query)
        options.append(draft)
    return options

def construct(artifact,model,reference,organization,critic,templates,integrity,seam,canonical,rng,number,budget,hashes):
    target=organization.sample(reference["position"],rng)
    target.update(targetParts=float(len(reference["parts"])),context=level_context(reference),referenceProfileSourceId=reference["sourceId"],
        profileConditioning="Soft organization target from source profile; no reference parts or coordinates copied.")
    base=empty_level(number,reference["position"],target,hashes);base["sourceId"]=f"composition-v7-{number}";base["sourceFamily"]="ordinary-composition-v7"
    base["assemblyProvenance"]["method"]="From-empty native atomic seed plus learned source strong-composition-pose unit grafts"
    base["assemblyProvenance"]["originalBoardUsedAsScaffold"]=False
    context=topology_context(reference);rejects=Counter();frontier=[]
    for seed_index in _sample_seed(artifact,context,rng,budget["seedOptions"]):
        out=_seed(artifact,model,base,seed_index,context)
        for draft in _grow(artifact,model,out,context,templates,rng,budget["initialGraftOptions"],rejects,hashes["compositionGraftModel"]):
            native=integrity.check(draft);raw=seam.check(draft)
            if not native["nativeAssemblyIntegrityPassed"]:rejects["initialNativeIntegrity"]+=1;continue
            if not raw["rawGeometrySeamPassed"]:rejects["initialRawGeometry"]+=1;continue
            value,metrics=score(draft,target,organization,templates,critic)
            if math.isfinite(value):frontier.append((value,draft,metrics,native,raw))
    if not frontier:return None,{"status":"NO_SUPPORTED_INITIAL_COMPOSITION_GRAFT","rejections":dict(rejects)}
    value,out,metrics,native,raw=min(frontier,key=lambda row:row[0]);best=None;best_value=float("inf")
    for step in range(budget["frontierSteps"]+1):
        fingerprint=canonical_fingerprint(out,templates)
        if fingerprint not in canonical and value<best_value:
            best=copy.deepcopy(out);best_value=value
            best["constructiveV7"]={"accepted":False,"nativeIntegrity":native,"rawGeometry":raw,"organizationScore":metrics,
                "sourceScaffoldUsed":False,"nativeReplayStatus":"PENDING","difficultyStatus":"UNMEASURED",
                "staticCompositionProxy":"SOURCE_POSE_CANDIDATE_ONLY"}
        elif fingerprint in canonical:rejects["wholeBoardCanonicalSourceOrPriorEquivalent"]+=1
        if step==budget["frontierSteps"]:break
        frontier=[]
        for draft in _grow(artifact,model,out,context,templates,rng,budget["graftOptionsPerStep"],rejects,hashes["compositionGraftModel"]):
            native=integrity.check(draft);raw=seam.check(draft)
            if not native["nativeAssemblyIntegrityPassed"]:rejects["frontierNativeIntegrity"]+=1;continue
            if not raw["rawGeometrySeamPassed"]:rejects["frontierRawGeometry"]+=1;continue
            value,metrics=score(draft,target,organization,templates,critic)
            if math.isfinite(value):frontier.append((value,draft,metrics,native,raw))
        if not frontier:break
        value,out,metrics,native,raw=min(frontier,key=lambda row:row[0])
    return best,{"status":"PRELIMINARY_COMPOSITION_GRAFT_PENDING_GAMEPLAY_REFERENCE_ANALYSIS" if best else "NO_NOVEL_PREFIX",
        "rejections":dict(rejects),"bestOrganizationScore":None if best is None else best["constructiveV7"]["organizationScore"]}

def main():
    p=argparse.ArgumentParser();p.add_argument("--workspace",type=Path,required=True);p.add_argument("--positions",nargs="+",type=int,required=True)
    p.add_argument("--seed",type=int,default=20261106);p.add_argument("--id-base",type=int,default=9700000)
    p.add_argument("--attempts-per-position",type=int,default=1);p.add_argument("--output",type=Path,required=True)
    p.add_argument("--seed-options",type=int,default=8);p.add_argument("--initial-graft-options",type=int,default=8)
    p.add_argument("--frontier-steps",type=int,default=8);p.add_argument("--graft-options-per-step",type=int,default=24)
    a=p.parse_args();w=a.workspace.resolve()
    if a.output.exists():raise ValueError("Refuse overwrite composition prototype pack")
    source_filter,bindings=preflight(w);pack=w/"data/source500_native.json";tp=w/"data/source_templates.json"
    levels=json.loads(pack.read_text(encoding="utf8"),object_hook=payload_compact)["levels"]
    allowed=validate_source_filter(source_filter,sha(pack),[l["sourceId"] for l in levels]);levels=[l for l in levels if str(l["sourceId"]) in allowed]
    bypos={l["position"]:l for l in levels};templates={t["id"]:t for t in json.loads(tp.read_text(encoding="utf8"))["templates"]}
    paths={"compositionGraftModel":w/"models/composition_graft_v7.joblib","organizationModel":w/"models/organization_conditioned_v5.joblib","interactionCriticModel":w/"models/interaction_critic_v5.joblib"}
    artifact=joblib.load(paths["compositionGraftModel"]);organization=joblib.load(paths["organizationModel"]);critic=joblib.load(paths["interactionCriticModel"])
    if artifact["hashes"]["sourcePack"]!=sha(pack) or artifact["hashes"]["templates"]!=sha(tp):raise ValueError("Stale v7 grammar")
    if any(r["sourceId"] not in allowed for r in artifact["records"]):raise ValueError("Excluded source influence in v7 grammar")
    model=SimpleNamespace(levels={str(l["sourceId"]):l for l in levels},templates=templates,tiles=copy.deepcopy(artifact["units"]))
    novelty_path=w/"data/novelty_extras/source_parity_pack.json";extra=json.loads(novelty_path.read_text(encoding="utf8"))["levels"]
    all_templates=dict(templates);all_templates.update({t["id"]:t for t in json.loads((w/"data/novelty_extras/source_templates.json").read_text(encoding="utf8"))["templates"]})
    canonical={canonical_fingerprint(l,all_templates) for l in levels+extra};novelty=LayoutNoveltyIndex(levels+extra,all_templates)
    integrity=NativeAssemblyIntegrityAudit(templates,levels,artifact["physicalCompositionRecords"]);seam=NativeGeometrySeamAudit(templates,levels)
    hashes={"sourcePack":sha(pack),"templates":sha(tp),**{k:sha(v) for k,v in paths.items()},"tool":sha(Path(__file__)),**bindings}
    rng=np.random.default_rng(a.seed);candidates=[];reports=[];ordinal=0
    budget={"seedOptions":a.seed_options,"initialGraftOptions":a.initial_graft_options,"frontierSteps":a.frontier_steps,"graftOptionsPerStep":a.graft_options_per_step}
    for position in a.positions:
        for attempt in range(a.attempts_per_position):
            candidate,report=construct(artifact,model,bypos[position],organization,critic,templates,integrity,seam,canonical,rng,a.id_base+ordinal,budget,hashes);ordinal+=1
            report.update(position=position,attempt=attempt)
            if candidate:
                candidate["constructiveV7"]["heuristicNearCopyReviewOnly"]=novelty.check(candidate)
                candidates.append(candidate);canonical.add(canonical_fingerprint(candidate,templates));report["candidateId"]=candidate["id"]
            reports.append(report);print("COMPOSITION_V7_ATTEMPT",position,attempt,report["status"],flush=True)
    result={"schemaVersion":7,"status":"PROTOTYPES_PENDING_INDEPENDENT_REPLAY_AND_HARD_REFERENCE_ANALYSIS","complete":False,
        "sourceScaffoldUsed":False,"numericContentGatesApplied":[],"levels":candidates,"reports":reports,"hashes":hashes,"seed":a.seed,
        "declaredSearchBudget":budget,"localDependencySha256":local_dependency_hashes(Path(__file__).resolve()),
        "limits":["No candidate is gameplay-validated, accepted, or difficulty-matched.","Source strong composition edges are static geometry proxies, not native release/contact labels.",
        "Run the independent native dynamic check and compare against original hard/very-hard references before any difficulty claim."]}
    a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(result,separators=(",",":")),encoding="utf8")
    print("COMPOSITION_V7_COMPLETE",len(candidates),flush=True)

if __name__=="__main__":main()
