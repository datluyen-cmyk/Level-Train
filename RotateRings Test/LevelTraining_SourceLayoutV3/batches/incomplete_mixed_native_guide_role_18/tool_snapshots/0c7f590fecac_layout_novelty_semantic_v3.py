"""Conservative semantic-orientation whole-layout near-copy check.

Versioned correction: closed round receivers ignore angle; straight symmetric
two-way Pulls use axes modulo180. C-gap, stem, T/L/U and asymmetric input retain
directional orientation. Existing numerical novelty thresholds are unchanged.

Aligns complete native part arrangements across translation, rotation, reflection,
and modest spacing rescale. It rejects cosmetic layout copies even if pin graphs
or palettes were changed. This is a geometric heuristic, not a novelty proof.
"""
import copy, hashlib, json, math
from collections import Counter
import numpy as np
from orientation_semantics_v3 import orientation_order
from scipy.optimize import linear_sum_assignment

CRITERIA={'minimumCoverage':.9,'maximumMeanCenterErrorWidths':.35,
          'maximumP90CenterErrorWidths':.7,'maximumMeanAngleErrorDegrees':15.,
          'maximumMeanShapeError':.15,'spacingScaleRange':[.85,1.15]}

def describe(level,templates):
    ps=level['parts'];xy=np.asarray([[p['x'],p['y']] for p in ps],float)
    features=[];labels=[]
    for p in ps:
        t=templates[p['templateId']];path=t.get('pullVisual',{}).get('pathPoints',[])
        labels.append(p['kind']+(':'+str(len(path)) if p['kind']=='slider' else ''))
        features.append([t.get('radius',0),t.get('width',0),t.get('gap',0)/90,
                         t.get('length',0),len(t.get('attachments',[]))*.25])
    width=float(np.median([templates[p['templateId']].get('width',0) for p in ps if p['kind'] in ('ring','slider')]))
    center=xy.mean(axis=0);_,vectors=np.linalg.eigh(np.cov((xy-center).T)) if len(ps)>1 else (None,np.eye(2))
    return {'xy':xy,'angles':np.radians([p['angle'] for p in ps]),'features':np.asarray(features),
            'labels':np.asarray(labels),'counts':Counter(labels),'width':max(.01,width),
            'axes':vectors[:,::-1],'center':center,'id':level.get('sourceId',level.get('id')),
            'orientationOrders':np.asarray([orientation_order(p,templates[p['templateId']]) for p in ps],int)}

def compare(a,b):
    matched=sum((a['counts']&b['counts']).values());coverage=matched/max(len(a['xy']),len(b['xy']))
    if coverage<CRITERIA['minimumCoverage']:return {'nearCopy':False,'coverage':coverage,'reason':'different shape-family inventory'}
    unit=max(a['width'],b['width']);best=None
    shape=np.linalg.norm(a['features'][:,None,:]-b['features'][None,:,:],axis=2)
    incompatible=a['labels'][:,None]!=b['labels'][None,:]
    shape=np.where(incompatible,1e5,shape)
    for reflect in (1.,-1.):
        for flip in (1.,-1.):
            matrix=a['axes']@np.diag([flip,flip*reflect])@b['axes'].T
            scale=1.;translation=a['center']-b['center']@matrix.T
            for _ in range(5):
                posed=b['xy']@matrix.T*scale+translation
                distance=np.linalg.norm(a['xy'][:,None,:]-posed[None,:,:],axis=2)/unit
                rows,cols=linear_sum_assignment(distance+shape*3)
                valid=~incompatible[rows,cols];rows,cols=rows[valid],cols[valid]
                if not len(rows):break
                # Trim extra pieces and refine alignment without changing any piece geometry.
                trim=np.argsort(distance[rows,cols])[:max(2,int(len(rows)*.9))];ra,rb=rows[trim],cols[trim]
                x=b['xy'][rb];y=a['xy'][ra];xc=x-x.mean(axis=0);yc=y-y.mean(axis=0)
                u,_,vh=np.linalg.svd(xc.T@yc);matrix=(u@vh).T
                scale=float(np.clip(np.sum((xc@matrix.T)*yc)/max(np.sum(xc*xc),1e-10),*CRITERIA['spacingScaleRange']))
                translation=y.mean(axis=0)-x.mean(axis=0)@matrix.T*scale
            if not len(rows):continue
            posed=b['xy']@matrix.T*scale+translation
            errors=np.linalg.norm(a['xy'][rows]-posed[cols],axis=1)/unit
            vectors=np.c_[np.cos(b['angles'][cols]),np.sin(b['angles'][cols])]@matrix.T
            angles=np.arctan2(vectors[:,1],vectors[:,0]);delta=a['angles'][rows]-angles
            oa,ob=a['orientationOrders'][rows],b['orientationOrders'][cols]
            multiplier=np.where((oa==2)&(ob==2),2.,1.)
            angle_error=np.degrees(np.abs(np.angle(np.exp(1j*delta*multiplier))))/multiplier
            angle_error=np.where((oa==0)|(ob==0),0.,angle_error)
            row={'coverage':len(rows)/max(len(a['xy']),len(b['xy'])),
                 'meanCenterErrorWidths':float(errors.mean()),'p90CenterErrorWidths':float(np.quantile(errors,.9)),
                 'meanAngleErrorDegrees':float(angle_error.mean()),'meanShapeError':float(shape[rows,cols].mean()),
                 'alignmentReflection':bool(np.linalg.det(matrix)<0),'spacingScale':scale}
            row['nearCopy']=bool(row['coverage']>=CRITERIA['minimumCoverage'] and row['meanCenterErrorWidths']<=CRITERIA['maximumMeanCenterErrorWidths'] and row['p90CenterErrorWidths']<=CRITERIA['maximumP90CenterErrorWidths'] and row['meanAngleErrorDegrees']<=CRITERIA['maximumMeanAngleErrorDegrees'] and row['meanShapeError']<=CRITERIA['maximumMeanShapeError'])
            score=row['meanCenterErrorWidths']+row['meanShapeError']*3+row['meanAngleErrorDegrees']/180
            if best is None or score<best[0]:best=(score,row)
    return best[1] if best else {'nearCopy':False,'coverage':0.,'reason':'no compatible assignment'}

def canonical_fingerprint(level,templates):
    """Rigid/cosmetic geometry key; tolerant near-copy matching is still required."""
    d=describe(level,templates);forms=[]
    def number(value,digits):
        value=round(round(float(value),8),digits);return 0. if value==0 else value
    for index,origin in enumerate(d['xy']):
        if d['orientationOrders'][index]:
            headings=[d['angles'][index]]
        else:
            headings=[math.atan2(q[1]-origin[1],q[0]-origin[0]) for q in d['xy'] if np.linalg.norm(q-origin)>1e-7] or [0.]
        for angle in headings:
            c,s=math.cos(angle),math.sin(angle)
            local=(d['xy']-origin)@np.array([[c,-s],[s,c]])
            relative=d['angles']-angle
            for mirror in (1.,-1.):
                rows=[]
                for i in range(len(local)):
                    order=int(d['orientationOrders'][i]);period=180. if order==2 else 360.
                    a=0. if order==0 else number(math.degrees(relative[i]*mirror)%period,3)%period
                    rows.append((str(d['labels'][i]),tuple(number(v,4) for v in d['features'][i]),number(local[i,0],4),number(local[i,1]*mirror,4),a))
                forms.append(json.dumps(sorted(rows),separators=(',',':')))
    return hashlib.sha256(min(forms).encode()).hexdigest()

class LayoutNoveltyIndex:
    def __init__(self,levels,templates):self.records=[describe(l,templates) for l in levels];self.templates=templates
    def check(self,level):
        candidate=describe(level,self.templates);matches=[];best=None
        for source in self.records:
            result=compare(candidate,source);result['sourceId']=source['id']
            if result['nearCopy']:matches.append(result)
            if 'meanCenterErrorWidths' in result and (best is None or result['meanCenterErrorWidths']<best['meanCenterErrorWidths']):best=result
        return {'passed':not matches,'sourcesCompared':len(self.records),'nearCopies':matches,'nearestCompatible':best,'criteria':CRITERIA,'limits':'Heuristic whole-layout geometry with physical/input orientation equivalences; does not certify intellectual originality or gameplay quality.'}

def regression(pack,templates):
    level=next(l for l in pack['levels'] if len(l['parts'])>=10)
    base=describe(level,templates);cases=[]
    for name,angle,mirror,spacing,jitter in [('identity',0,False,1,0),('rotateTranslate',73,False,1,0),('reflected',27,True,1,0),('smallSpacingChange',0,False,1.04,0),('tinyJitter',0,False,1,.02)]:
        l=copy.deepcopy(level);r=math.radians(angle);matrix=np.array([[math.cos(r),-math.sin(r)],[math.sin(r),math.cos(r)]])@np.diag([-1 if mirror else 1,1]);rng=np.random.default_rng(19)
        for p in l['parts']:
            xy=matrix@np.array([p['x'],p['y']])*spacing+np.array([7.,-4.])+rng.normal(0,jitter*base['width'],2);p['x'],p['y']=map(float,xy)
            vector=matrix@np.array([math.cos(math.radians(p['angle'])),math.sin(math.radians(p['angle']))]);p['angle']=math.degrees(math.atan2(vector[1],vector[0]))
        result=compare(base,describe(l,templates));canonical_expected=spacing==1 and jitter==0
        canonical_pass=not canonical_expected or canonical_fingerprint(l,templates)==canonical_fingerprint(level,templates)
        cases.append({'case':name,'expectedNearCopy':True,'passed':result['nearCopy'] and canonical_pass,'canonicalInvariantRequired':canonical_expected,'canonicalInvariantPassed':canonical_pass,'result':result})
    shuffled=copy.deepcopy(level);rng=np.random.default_rng(214);permutation=rng.permutation(len(shuffled['parts']));poses=[(p['x'],p['y'],p['angle']) for p in shuffled['parts']]
    for p,i in zip(shuffled['parts'],permutation):p['x'],p['y'],p['angle']=poses[i]
    result=compare(base,describe(shuffled,templates));cases.append({'case':'grossPoseShuffle','expectedNearCopy':False,'passed':not result['nearCopy'],'result':result})
    return {'passed':all(c['passed'] for c in cases),'cases':cases,'meaning':'Synthetic invariance regression only; not generated-level acceptance.'}

if __name__=='__main__':
    import argparse
    from pathlib import Path
    p=argparse.ArgumentParser();p.add_argument('--pack',type=Path,required=True);p.add_argument('--templates',type=Path,required=True);p.add_argument('--report',type=Path,required=True);args=p.parse_args()
    pack=json.loads(args.pack.read_text(encoding='utf8'));ts={t['id']:t for t in json.loads(args.templates.read_text(encoding='utf8'))['templates']};report=regression(pack,ts);report['hashes']={str(p):hashlib.sha256(p.read_bytes()).hexdigest() for p in [args.pack,args.templates,Path(__file__)]};args.report.write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps({'passed':report['passed'],'cases':len(report['cases'])}))
