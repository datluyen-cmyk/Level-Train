"""Export actual original 2D collider paths, never reconstruct them from link bearings."""
from pathlib import Path
import collections,copy,hashlib,json,math
import UnityPy

ROOT=Path(__file__).resolve().parents[1]
SOURCE=ROOT/'Research/extracted/assets/yoo/DefaultPackage'
OUT=ROOT/'Research/physics_v2'

def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def rotation(q):return math.atan2(2*(q['w']*q['z']+q['x']*q['y']),1-2*(q['y']**2+q['z']**2))
def mul(a,b):
    return (a[0]*b[0]+a[2]*b[1],a[1]*b[0]+a[3]*b[1],a[0]*b[2]+a[2]*b[3],a[1]*b[2]+a[3]*b[3],a[0]*b[4]+a[2]*b[5]+a[4],a[1]*b[4]+a[3]*b[5]+a[5])
def point(m,p):return (m[0]*p['x']+m[2]*p['y']+m[4],m[1]*p['x']+m[3]*p['y']+m[5])
def export_level(old):
    bundle=SOURCE/old['provenance']['bundle'];env=UnityPy.load(str(bundle))
    wanted={'Transform','GameObject','BoxCollider2D','PolygonCollider2D','CircleCollider2D','CapsuleCollider2D'}
    objects={o.path_id:{'type':o.type.name,'tree':o.read_typetree()} for o in env.objects if o.type.name in wanted}
    transforms={o['tree']['m_GameObject']['m_PathID']:o['tree'] for o in objects.values() if o['type']=='Transform'}
    transformids={i:o['tree'] for i,o in objects.items() if o['type']=='Transform'}
    cache={}
    def world(t):
        go=t['m_GameObject']['m_PathID']
        if go in cache:return cache[go]
        a=rotation(t['m_LocalRotation']);s=t['m_LocalScale'];p=t['m_LocalPosition'];c=math.cos(a);sn=math.sin(a)
        m=(c*s['x'],sn*s['x'],-sn*s['y'],c*s['y'],p['x'],p['y'])
        parent=transformids.get(t['m_Father']['m_PathID'])
        if parent:m=mul(world(parent),m)
        cache[go]=m;return m
    raw=json.loads((ROOT/'Research/levels'/f"{old['id']}.json").read_text(encoding='utf8'))
    relatives={c['data']['m_GameObject']['m_PathID']:c for c in raw['components'] if c['class']=='RingRelative'}
    out=copy.deepcopy(old);out['state']['parts']=[]
    for oldpart in old['state']['parts']:
        p={k:v for k,v in oldpart.items() if not k.startswith('source') and not k.startswith('normalized')}
        p['sourceId']=oldpart['sourceId'];p['physicalShapes']=[];p['attachments']=[]
        native=oldpart['sourceNativeData'];angle=math.radians(oldpart['angle']);c=math.cos(angle);sn=math.sin(angle)
        selected={native.get('activeCollider',{}).get('m_PathID'):('body','')}
        for ref in native.get('hardBlockColliders',[]):
            oid=ref['m_PathID'];col=objects.get(oid)
            if not col:continue
            goid=col['tree']['m_GameObject']['m_PathID'];name=objects[goid]['tree']['m_Name']
            selected[oid]=('stem',name.removeprefix('HardBlock_'))
        rel_ids={ref['m_PathID'] for ref in native.get('ringRelatives',[])}
        relcomps=[c for c in raw['components'] if c['class']=='RingRelative' and any(str(ref['m_PathID']) in raw['objects'] and raw['objects'][str(ref['m_PathID'])]['data'].get('m_GameObject')==c['data']['m_GameObject'] for ref in native.get('ringRelatives',[]))]
        for rel in relcomps:
            d=rel['data'];attachment=d.get('featureId','').removeprefix('attachment.')
            head=d['selfCollider']['m_PathID'];selected[head]=('head_trigger',attachment)
            for ref in d.get('blockColliders',[]):
                if ref['m_PathID']!=head:selected[ref['m_PathID']]=('head_wall',attachment)
        def local(pt,m):
            x,y=point(m,pt);x-=p['x'];y-=p['y']
            return {'x':round(c*x+sn*y,7),'y':round(-sn*x+c*y,7)}
        for oid,(role,attid) in selected.items():
            obj=objects.get(oid)
            if not obj:continue
            d=obj['tree'];goid=d['m_GameObject']['m_PathID'];go=objects[goid]['tree'];m=world(transforms[goid]);off=d.get('m_Offset',{'x':0,'y':0})
            if go.get('m_Layer')==6:continue
            if obj['type']=='PolygonCollider2D':paths=d['m_Points']['m_Paths']
            elif obj['type']=='BoxCollider2D':
                sx=d['m_Size']['x']/2;sy=d['m_Size']['y']/2
                paths=[[{'x':x,'y':y} for x,y in [(-sx,-sy),(sx,-sy),(sx,sy),(-sx,sy)]]]
            else:
                p.setdefault('unsupportedColliderTypes',[]).append(obj['type']);continue
            paths=[[local({'x':v['x']+off['x'],'y':v['y']+off['y']},m) for v in path] for path in paths]
            p['physicalShapes'].append({'role':role,'attachmentId':attid,'trigger':bool(d.get('m_IsTrigger')),'points':paths[0],'paths':[{'points':path} for path in paths],'sourceColliderPathId':oid,'sourceColliderType':obj['type'],'sourceLayer':go.get('m_Layer'),'sourceEnabled':bool(d.get('m_Enabled',True))})
        render=oldpart['sourceRenderData'];sx,sy=oldpart['sourceWorldScale'];scale=(abs(sx)+abs(sy))/2
        p.update(visualRadius=render.get('_radius',p['radius'])*scale,visualWidth=render.get('_width',p['width'])*scale,visualGap=render.get('_gapAngleDeg',p['gap']),capRatio=render.get('_capRatio',0),extraEnabled=bool(render.get('_extraEnabled',False)),styleIndex=render.get('_styleIndex',0))
        gapdir=render.get('_gapDirectionDeg',0)
        for i,att in enumerate(render.get('_attachments',[])):
            a=copy.deepcopy(att);a['angleDeg']=(a['angleDeg']-gapdir)%360
            for key in ['stemWidth','stemLength','stemInsertOffset','crossWidth','crossLength','crossCapLength']:
                a[key]*=scale
            a['sourceAttachmentIndex']=i;a['id']=i;a['mountSide']='inner' if a['isInnerMount'] else 'outer';a['isInnerMount']=bool(a['isInnerMount'])
            a['kind']='TBar' if a['type']==1 else 'HeadedBar'
            a['sourceVisualSettings']={k:render[k] for k in render if k in ['_extraEnabled','_crossBarZ','_capRatio','_width','_radius','_gapAngleDeg']}
            p['attachments'].append(a)
        aliases={}
        for shape in p['physicalShapes']:
            if shape['role']!='stem':continue
            pts=shape['points'];bearing=math.degrees(math.atan2(sum(v['y'] for v in pts),sum(v['x'] for v in pts)))%360
            if p['attachments']:
                closest=min(p['attachments'],key=lambda a:abs((a['angleDeg']-bearing+180)%360-180))
                aliases[shape['attachmentId']]=closest['id']
        for shape in p['physicalShapes']:
            source_id=shape['attachmentId'];shape['sourceAttachmentId']=source_id
            shape['attachmentId']=aliases.get(source_id,-1)
        if p['kind']=='ring':
            body=next((s for s in p['physicalShapes'] if s['role']=='body'),None)
            if body:
                radii=[math.hypot(v['x'],v['y']) for path in body['paths'] for v in path['points']]
                outer=max(radii);nativeouter=oldpart['sourceNativeOuterRadius'];nativeinner=oldpart['sourceNativeInnerRadius']
                coord=outer/nativeouter if nativeouter else 1
                p['radius']=(nativeinner+nativeouter)*.5*coord;p['width']=(nativeouter-nativeinner)*coord
                p['nativeInnerRadius']=nativeinner;p['nativeOuterRadius']=nativeouter;p['sourceCoordScale']=coord
        template={k:copy.deepcopy(p[k]) for k in ['kind','radius','width','gap','length','visualRadius','visualWidth','visualGap','capRatio','extraEnabled','styleIndex','physicalShapes','attachments']}
        for shape in template['physicalShapes']:
            for key in ['sourceColliderPathId','sourceLayer','sourceEnabled']:shape.pop(key,None)
        canonical=json.dumps(template,sort_keys=True,separators=(',',':'))
        tid='native-'+hashlib.sha256(canonical.encode()).hexdigest()[:20];p['templateId']=tid
        p['sourceTemplateId']=tid;out['state']['parts'].append(p)
        yield 'template',(tid,template,{'levelId':old['id'],'partId':p['id'],'sourceId':oldpart['sourceId']})
    byid={p['id']:p for p in out['state']['parts']}
    for link in out['state']['links']:
        source_feature=link.get('sourceFeature','').removeprefix('attachment.')
        source=byid[link['source']]
        matches=[s['attachmentId'] for s in source['physicalShapes'] if s.get('sourceAttachmentId')==source_feature]
        link['attachmentId']=matches[0] if matches else -1
    unsupported=old['projection']['unsupportedMechanics']
    out['physicsV2']={'source':'actual serialized collider paths','eligible':not unsupported and all(p['kind']=='ring' and not p.get('unsupportedColliderTypes') for p in out['state']['parts']),'unsupportedMechanics':unsupported,'status':'not yet solved under physical rules'}
    yield 'level',out

def main():
    OUT.mkdir(exist_ok=True)
    corpus=json.loads((ROOT/'Research/normalized_corpus.json').read_text(encoding='utf8'))
    catalog={};levels=[]
    for i,old in enumerate(corpus['levels']):
        for kind,value in export_level(old):
            if kind=='level':levels.append(value)
            else:
                tid,t,ref=value
                if tid not in catalog:catalog[tid]={'id':tid,**t,'sourceRefs':[]}
                catalog[tid]['sourceRefs'].append(ref)
        if i%25==0:print('exported',i+1,'templates',len(catalog),flush=True)
    manifest={'schemaVersion':2,'ruleStatus':'native geometry exported; dynamic policy pending physical replay','sourceCount':len(levels),'eligibleSourceCount':sum(l['physicsV2']['eligible'] for l in levels),'sourceCorpusSha256':sha(ROOT/'Research/normalized_corpus.json'),'exporterSha256':sha(Path(__file__))}
    (OUT/'corpus500.json').write_text(json.dumps({**manifest,'levels':levels},separators=(',',':')),encoding='utf8')
    (OUT/'templates.json').write_text(json.dumps({**manifest,'templates':list(catalog.values())},separators=(',',':')),encoding='utf8')
    print(json.dumps({**manifest,'templateCount':len(catalog),'corpusSha256':sha(OUT/'corpus500.json'),'templatesSha256':sha(OUT/'templates.json')},indent=2))
if __name__=='__main__':main()
