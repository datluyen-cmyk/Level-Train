"""Fit cohort-outcome predictions from selection-phase synthetic telemetry."""
import argparse,hashlib,json,math,pathlib
import joblib,numpy as np
from sklearn.compose import TransformedTargetRegressor
from sklearn.ensemble import ExtraTreesRegressor
from sklearn.preprocessing import StandardScaler
from level_training import digest,write,groups_split

def run(labels_path,seed=20261105):
    path=pathlib.Path(labels_path).resolve();data=json.loads(path.read_text(encoding='utf8'))
    if not data.get('synthetic') or data.get('phase')!='selection':
        raise ValueError('Fit only explicitly synthetic selection data; held-out telemetry is evaluation-only until a new partition is declared')
    levels=data['levels'];keys=sorted(levels[0]['features']);cohorts=sorted(levels[0]['metricsByCohort'])
    target_names=['winRate','budgetExhaustionRate','allSessionActionsMedian','hintsMean','undoMean']+['winRate:'+c for c in cohorts]
    x=[];y=[]
    for l in levels:
        if sorted(l['features'])!=keys or sorted(l['metricsByCohort'])!=cohorts:raise ValueError('Inconsistent feature/cohort schema')
        x.append([float(l['features'][k]) for k in keys]);y.append([float(l['metrics'][k]) for k in target_names[:5]]+[float(l['metricsByCohort'][c]['winRate']) for c in cohorts])
    x=np.asarray(x);y=np.asarray(y)
    if not np.isfinite(x).all() or not np.isfinite(y).all():raise ValueError('Nonfinite feature/telemetry')
    class CanonicalLabels:
        @staticmethod
        def fingerprint(l):return l['fingerprint']
    train,test,split=groups_split(levels,CanonicalLabels,seed)
    def estimator():return TransformedTargetRegressor(regressor=ExtraTreesRegressor(n_estimators=128,min_samples_leaf=1,random_state=seed,n_jobs=-1),transformer=StandardScaler())
    model=estimator();model.fit(x[train],y[train]);prediction=model.predict(x[test]);baseline=np.tile(y[train].mean(axis=0),(len(test),1))
    metrics={}
    for j,name in enumerate(target_names):
        actual=y[test,j];p=prediction[:,j];b=baseline[:,j]
        row={'mae':float(np.abs(actual-p).mean()),'meanBaselineMAE':float(np.abs(actual-b).mean())}
        if name=='winRate' or name.startswith('winRate:') or name=='budgetExhaustionRate':
            p=np.clip(p,1e-8,1-1e-8);b=np.clip(b,1e-8,1-1e-8)
            row.update(aggregateRateMSE=float(np.square(actual-p).mean()),baselineAggregateRateMSE=float(np.square(actual-b).mean()),
                aggregateCrossEntropy=float(-(actual*np.log(p)+(1-actual)*np.log(1-p)).mean()),
                baselineAggregateCrossEntropy=float(-(actual*np.log(b)+(1-actual)*np.log(1-b)).mean()))
        metrics[name]=row
    evaluation={'synthetic':True,'split':split,'targets':target_names,'metrics':metrics,'seed':seed,'labelsSha256':digest(path),
        'runnerSha256':digest(__file__),'limits':'Family-held-out prediction of aggregate virtual-player metrics, not human calibration or individual-outcome Brier score. Better-than-baseline performance is not guaranteed.'}
    out=path.parent/'outcome-model';out.mkdir(exist_ok=True);write(out/'validation.json',evaluation)
    model.fit(x,y);joblib.dump({'model':model,'features':keys,'targets':target_names,'validation':evaluation},out/'model.joblib')
    write(out/'manifest.json',{'synthetic':True,'labelsSha256':digest(path),'modelSha256':digest(out/'model.joblib'),'validationSha256':digest(out/'validation.json')})
    print('Trained virtual-player outcome model:',out,flush=True);return evaluation
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--labels',required=True);p.add_argument('--seed',type=int,default=20261105);a=p.parse_args();run(a.labels,a.seed)
