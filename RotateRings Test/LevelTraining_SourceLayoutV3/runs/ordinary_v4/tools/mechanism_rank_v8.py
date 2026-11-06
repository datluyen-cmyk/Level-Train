#!/usr/bin/env python3
"""Family-fold source mechanism tag-ranker; labels are serialized UI default tags."""
import argparse, hashlib, json, math
from collections import Counter
from pathlib import Path
import numpy as np
from sklearn.pipeline import make_pipeline
from sklearn.preprocessing import StandardScaler
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import balanced_accuracy_score, f1_score, confusion_matrix, classification_report

CLASSES=['Normal','Hard','VeryHard']
KINDS=['ring','slider','fixed','butterfly']
FEATURE_NAMES=['log_part_count','log_movable_root_count',*[f'part_fraction_{k}' for k in KINDS],'authored_link_rate','guide_link_rate','bomb_rate','chain_locked_root_fraction','mean_chain_locks_per_root','normalized_chain_lock_total','mean_ring_gap_fraction','ring_gap_std_fraction','closed_ring_fraction','mean_ring_orientation_sin','mean_ring_orientation_cos','mean_pair_relative_angle_sin','mean_pair_relative_angle_cos','normalized_pair_distance_mean','normalized_pair_distance_std','near_pair_fraction_lt2r','hardblock_pin_mean_per_ring','hardblock_pin_std','pin_bearing_circular_dispersion','guided_slider_component_fraction','largest_guided_slider_component_fraction','guided_slider_mean_degree','guided_slider_degree_std','slider_fraction','mobility_axis_dispersion','bomb_part_fraction','bomb_near_pair_fraction']

def sha(path): return hashlib.sha256(Path(path).read_bytes()).hexdigest()
def load(path): return json.loads(Path(path).read_text(encoding='utf8'))
def circ_dispersion(degs):
    if not degs: return 0.0
    vals=np.radians(degs);return float(1-abs(np.exp(1j*vals).mean()))
def features(level):
    parts=level.get('parts',[]); n=max(1,len(parts)); kinds=Counter(p.get('kind','') for p in parts)
    links=level.get('links',[]); guides=level.get('guideLinks',[]); bombs=level.get('bombs',[])
    roots=[p for p in parts if p.get('kind') in ('ring','slider')]
    rings=[p for p in parts if p.get('kind')=='ring']; sliders=[p for p in parts if p.get('kind')=='slider']
    # Feature vector intentionally excludes IDs, source IDs, global x/y, level position, color and raw template IDs.
    vals=[]
    def add(x): vals.append(float(x) if math.isfinite(float(x)) else 0.)
    add(math.log1p(len(parts))); add(math.log1p(len(roots)))
    for k in KINDS: add(kinds[k]/n)
    add(len(links)/n); add(len(guides)/n); add(len(bombs)/n)
    add(sum(int(p.get('chainLockCount',0))>0 for p in roots)/max(1,len(roots)))
    add(sum(max(0,int(p.get('chainLockCount',0))) for p in roots)/max(1,len(roots)))
    add(sum(max(0,int(p.get('chainLockCount',0))) for p in roots)/max(1,len(roots)**2))
    gaps=[]; angles=[]; bearing=[]; pin_counts=[]; axes=[]; ring_sins=[]; ring_cos=[]; closed=[]
    for p in rings:
        dat=p.get('sourcePartData',{})
        if 'GapAngleDeg' in dat: gaps.append(float(dat['GapAngleDeg']))
        gaps.append(float(dat.get('GapAngleDeg',0) or 0))
        # physical closed/open gap proxy from authored gap angle; no claim about swept collision.
        closed.append(float(dat.get('GapAngleDeg',0))<=1.0)
        aa=float(p.get('angle',0)); angles.append(aa)
        ring_sins.append(math.sin(math.radians(aa))); ring_cos.append(math.cos(math.radians(aa)))
        atts=int(dat.get('AttachmentCount',0) or 0)
        binds=p.get('sourceColliderBindings',[])
        pins=sum(1 for b in binds if b.get('sourceRole')=='HardBlockPin')
        if not pins: pins=atts
        pin_counts.append(pins)
        # If optional template geometry has been embedded as attachments in sourcePartData, summarize bearings.
        bearings=dat.get('attachmentAnglesDeg',[])
        bearing.extend(float(x) for x in bearings if isinstance(x,(int,float)))
        axes.append(float(p.get('angle',0))%180)
    for seq in (gaps,):
        add(float(np.mean(seq))/360 if seq else 0); add(float(np.std(seq))/180 if seq else 0)
    add(float(np.mean(closed)) if closed else 0)
    add(float(np.mean(ring_sins)) if ring_sins else 0); add(float(np.mean(ring_cos)) if ring_cos else 0)
    # Pairwise relative angle/orientation and geometric-neighborhood proxies are scale normalized.
    pair_angles=[]; distances=[]; proximity=[]
    for i,a in enumerate(roots):
        for b in roots[i+1:]:
            dtheta=math.radians(float(b.get('angle',0))-float(a.get('angle',0)))
            pair_angles.append((math.sin(dtheta),math.cos(dtheta)))
            dist=math.hypot(float(a.get('x',0))-float(b.get('x',0)),float(a.get('y',0))-float(b.get('y',0)))
            ra=float(a.get('sourcePartData',{}).get('OuterRadius',0)); rb=float(b.get('sourcePartData',{}).get('OuterRadius',0))
            unit=max(1.0,(ra+rb)/2/24.0); dn=dist/unit
            distances.append(dn); proximity.append(dn<2.0)
    add(float(np.mean([x[0] for x in pair_angles])) if pair_angles else 0)
    add(float(np.mean([x[1] for x in pair_angles])) if pair_angles else 0)
    add(float(np.mean(distances)) if distances else 0);add(float(np.std(distances)) if distances else 0)
    add(float(np.mean(proximity)) if proximity else 0)
    add(float(np.mean(pin_counts))/max(1,len(rings)) if pin_counts else 0);add(float(np.std(pin_counts)) if pin_counts else 0)
    add(circ_dispersion(bearing))
    # Guided slider groups, connected-component and mobility-axis summaries.
    slider_ids={p.get('id') for p in sliders}; adj={x:set() for x in slider_ids}
    for g in guides:
        s=g.get('source');t=g.get('target')
        if s in adj and t in adj: adj[s].add(t);adj[t].add(s)
    seen=set(); comps=[]
    for x in adj:
        if x in seen: continue
        stack=[x];seen.add(x);c=0
        while stack:
            z=stack.pop();c+=1
            for y in adj[z]-seen:seen.add(y);stack.append(y)
        comps.append(c)
    add(len(comps)/max(1,len(sliders)));add(max(comps,default=0)/max(1,len(sliders)))
    degrees=[len(v) for v in adj.values()]
    add(float(np.mean(degrees)) if degrees else 0);add(float(np.std(degrees)) if degrees else 0)
    add(len(sliders)/n);add(float(np.std(axes))/90 if axes else 0)
    # Bomb local contact potential is a coarse static proximity/contact possibility, not simulated interaction.
    bomb_part_ids=set()
    for b in bombs:
        if isinstance(b,dict):
            for k in ('partId','owner','id'):
                if b.get(k) in {p.get('id') for p in parts}: bomb_part_ids.add(b[k])
    add(len(bomb_part_ids)/n)
    add(float(np.mean(proximity)) if bombs and proximity else 0)
    return vals

def progress_tag(pos):
    # Faithful summary of extracted ordinary default rules: tag uses progression position cycles.
    if pos>=39 and pos%10==9: return 'VeryHard'
    if pos>=39 and pos%10==5: return 'Hard'
    if 19<=pos<=35 and pos%10 in (5,9): return 'Hard'
    return 'Normal'
def predict_candidate_features(level, model_path):
    """Return tag probabilities from candidate JSON; does not load source corpus or IDs."""
    import joblib
    model=joblib.load(model_path); row=np.asarray(features(level),dtype=float).reshape(1,-1)
    probs=model.predict_proba(row)[0]
    return {str(tag):float(prob) for tag,prob in zip(model.classes_,probs)}
def evaluate(y, pred): return {'balancedAccuracy':float(balanced_accuracy_score(y,pred)), 'macroF1':float(f1_score(y,pred,labels=CLASSES,average='macro',zero_division=0)), 'confusionMatrixActualRowsPredictedColumns':confusion_matrix(y,pred,labels=CLASSES).tolist()}
def main():
    ap=argparse.ArgumentParser();ap.add_argument('--workspace',type=Path,required=True);a=ap.parse_args();root=a.workspace.resolve();(root/'models').mkdir(exist_ok=True);(root/'reports').mkdir(exist_ok=True);(root/'tools').mkdir(exist_ok=True)
    packp=root/'data/source500_native.json'; labelp=root/'reports/original_default_difficulty_v7.json';splitp=root/'models/family_splits.json'; lessons=root/'playtests/source_design_v7/source_mechanism_lessons_v7.json'
    pack=load(packp); labels={x['sourceId']:x for x in load(labelp)['levels']}; levels={str(x['id']):x for x in pack['levels']}
    ids=sorted(set(levels)&set(labels)); X=np.array([features(levels[i]) for i in ids]); y=np.array([labels[i]['defaultConfigDifficulty'] for i in ids]);pos=np.array([labels[i]['progressionPosition'] for i in ids])
    split=load(splitp)['splits']; folds=[]
    for fd in split:
        tr=[ids.index(str(x)) for x in fd['trainSourceIds'] if str(x) in ids];te=[ids.index(str(x)) for x in fd['heldOutSourceIds'] if str(x) in ids]
        folds.append((fd['fold'],tr,te))
    preds={k:np.empty(len(ids),object) for k in ['mechanismAware','countsOnly','majority','progressionRule']}; metrics={k:[] for k in preds}
    count_dim=2+len(KINDS)+4
    for fold,tr,te in folds:
        for name,XX in [('mechanismAware',X),('countsOnly',X[:,:count_dim])]:
            model=make_pipeline(StandardScaler(),LogisticRegression(class_weight='balanced',max_iter=1000,C=1.0,random_state=0));model.fit(XX[tr],y[tr]);preds[name][te]=model.predict(XX[te])
        maj=Counter(y[tr]).most_common(1)[0][0];preds['majority'][te]=maj
        preds['progressionRule'][te]=[progress_tag(pos[j]) for j in te]
        for name in preds: metrics[name].append({'fold':fold,'testN':len(te),**evaluate(y[te],preds[name][te])})
    aggregate={k:evaluate(y,preds[k]) for k in preds}
    # Refit saved mechanism model on the full corpus for candidate ranking; fold results remain primary evidence.
    final=make_pipeline(StandardScaler(),LogisticRegression(class_weight='balanced',max_iter=1000,C=1.0,random_state=0));final.fit(X,y)
    import joblib
    modelp=root/'models/mechanism_rank_v8.joblib';joblib.dump(final,modelp)
    report={'schemaVersion':1,'status':'evaluated_default_UI_tag_proxy_not_human_difficulty','labelMeaning':'Original game default-config difficulty tag resolved from progression rules; not observed player difficulty.','sourceCount':len(ids),'classCounts':dict(Counter(y)),'featureCount':X.shape[1],'featureNames':FEATURE_NAMES,'featureRestrictions':'source IDs and absolute coordinates are absent; only scale-normalized pairwise geometric neighborhood statistics use coordinates.','splits':len(folds),'foldMetrics':metrics,'aggregateOutOfFoldMetrics':aggregate,'baselines':['countsOnly class-weighted logistic regression','training-fold majority','extracted progression-position rule (reported separately; expected to be near-exact because labels are position-derived)'],'model':'StandardScaler + class-weighted LogisticRegression C=1; refit on full corpus only after family-fold OOF evaluation.','mechanismScope':['gap and closed-ring proxy','orientation and pairwise relative-angle summaries','pin counts and bearing dispersion when native authored bearings are present','guided slider component/degree summaries','normalized chain-lock intensity','static pairwise proximity and bomb-context potential'],'limitations':['Static proxies do not simulate swept collision, sequential release, or actual bomb contact.','Source labels are cyclic UI tags and are not human difficulty measurements.','Held-out mechanism-aware tag prediction does not beat counts-only; do not present this as a learned difficulty predictor.'],'hashes':{'sourcePack':sha(packp),'defaultDifficultyRuleReport':sha(labelp),'familySplits':sha(splitp),'mechanismLessons':sha(lessons),'featureCode':sha(Path(__file__)),'savedModel':sha(modelp)}}
    (root/'reports/mechanism_rank_v8.json').write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps({'counts':report['classCounts'],'metrics':aggregate,'hashes':report['hashes']},indent=2))
if __name__=='__main__':main()
