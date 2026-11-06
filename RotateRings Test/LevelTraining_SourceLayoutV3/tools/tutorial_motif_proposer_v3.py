"""Find native connected subgraph motifs matching tutorial progression 1-5.

This is a bounded proposal pilot. It keeps source poses and assets rigid, and
exports only incomplete candidates for native review; it makes no difficulty,
playability, or acceptance claim.
"""
from pathlib import Path
from collections import Counter
import argparse, copy, hashlib, itertools, json, math, sys

import joblib
import networkx as nx
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT))
from macro_assembly_v3 import instantiate_tile
from macro_rewrite_v3 import guide_close
from assembly_integrity_audit_v3 import NativeAssemblyIntegrityAudit
from assembly_contact_audit_v3 import NativeGeometrySeamAudit
from layout_novelty_v3 import LayoutNoveltyIndex
from layout_learning_v3 import hashfile, relation_rows
from render_layout_diagnostics_v3 import panel as native_solid_panel


def inventory(level):
    return (Counter(p['kind'] for p in level['parts']), len(level['links']),
            len(level.get('guideLinks', [])), Counter(p['chainLockCount'] for p in level['parts']),
            Counter(p['collectible'] for p in level['parts']), len(level.get('bombs', [])))


def native_shape_match(parts, target_parts, templates, tolerance=.35):
    """Match body class and scale family while forbidding dormant pin surplus."""
    def descriptor(part):
        t=templates[part['templateId']]
        return np.array([t.get('radius',0.),t.get('width',0.),t.get('visualGap',0.)/180.,t.get('length',0.)],float)
    for kind in {p['kind'] for p in target_parts}:
        a=sorted((p for p in parts if p['kind']==kind),key=lambda p:tuple(descriptor(p)))
        b=sorted((p for p in target_parts if p['kind']==kind),key=lambda p:tuple(descriptor(p)))
        if len(a)!=len(b): return False
        for p,q in zip(a,b):
            va,vb=descriptor(p),descriptor(q)
            # 20% floor makes zero-valued optional dimensions comparable.
            if np.any(np.abs(va-vb)>tolerance*np.maximum(np.abs(vb),.2)): return False
            ta,tb=templates[p['templateId']],templates[q['templateId']]
            if len(ta.get('attachments',[])) != len(tb.get('attachments',[])): return False
    return True


def induced(level, selected):
    selected = set(selected)
    return [e for e in level['links'] if e['source'] in selected and e['target'] in selected]


def authored_topology(level, selected, templates):
    """Directed multigraph over selected native roots and every induced pin/guide."""
    selected=set(selected); parts={p['id']:p for p in level['parts']}; graph=nx.MultiDiGraph()
    for pid in selected:
        p=parts[pid];t=templates[p['templateId']]
        graph.add_node(pid,kind=p['kind'],attachmentCount=len(t.get('attachments',[])),
                       chainLockCount=int(p.get('chainLockCount',0)),collectible=bool(p.get('collectible',False)))
    for e in level.get('links',[]):
        if e['source'] in selected and e['target'] in selected:
            graph.add_edge(e['source'],e['target'],nativeEdgeKind='pin',
                           relationKind=int(e.get('relationKind',0)),activityKind=int(e.get('activityKind',0)))
    for e in level.get('guideLinks',[]):
        if e['guideOwnerId'] in selected and e['lockedOwnerId'] in selected:
            graph.add_edge(e['guideOwnerId'],e['lockedOwnerId'],nativeEdgeKind='guide',
                           relationKind=-1,activityKind=-1)
    return graph


def same_authored_topology(level_a, ids_a, level_b, ids_b, templates):
    ga=authored_topology(level_a,ids_a,templates);gb=authored_topology(level_b,ids_b,templates)
    node_match=lambda a,b: all(a.get(k)==b.get(k) for k in ('kind','attachmentCount','chainLockCount','collectible'))
    edge_match=lambda a,b: all(a.get(k)==b.get(k) for k in ('nativeEdgeKind','relationKind','activityKind'))
    return nx.is_isomorphic(ga,gb,node_match=node_match,edge_match=edge_match)


def topology_regression():
    """Prove same counts cannot make a fanout-chain equal a shared receiver."""
    def graph(edges,labels):
        g=nx.MultiDiGraph()
        for i in range(4):g.add_node(i,kind='ring',attachmentCount=2,chainLockCount=0,collectible=True)
        for a,b in edges:g.add_edge(labels[a],labels[b],nativeEdgeKind='pin',relationKind=0,activityKind=0)
        return g
    target=graph([(0,1),(0,2),(3,1)],[0,1,2,3])
    fanout=graph([(0,2),(0,1),(2,3)],[0,1,2,3])
    relabel=graph([(0,1),(0,2),(3,1)],[2,0,3,1])
    nm=lambda a,b: all(a.get(k)==b.get(k) for k in ('kind','attachmentCount','chainLockCount','collectible'))
    em=lambda a,b: all(a.get(k)==b.get(k) for k in ('nativeEdgeKind','relationKind','activityKind'))
    rejects=not nx.is_isomorphic(target,fanout,node_match=nm,edge_match=em)
    accepts=nx.is_isomorphic(target,relabel,node_match=nm,edge_match=em)
    return {'passed':rejects and accepts,'rejectsFanoutChainWithSameCounts':rejects,
            'acceptsIsomorphicRelabeling':accepts,'targetEdges':3,'comparisonEdges':3}


def connected_subsets(level, target, templates, cap=8000):
    """Enumerate connected authored-pin subgraphs, pruning by mechanic inventory."""
    ps = {p['id']: p for p in level['parts']}
    graph = nx.Graph(); graph.add_nodes_from(ps)
    graph.add_edges_from((e['source'], e['target']) for e in level['links'])
    guide = level.get('guideLinks', [])
    wanted_kinds, wanted_edges, wanted_guides, wanted_locks, wanted_collect, wanted_bombs = inventory(target)
    wanted_n = len(target['parts'])
    seen, result, failures = set(), [], Counter()
    # Ring-only tutorials have no guides/bombs. Larger tutorial targets still
    # require exact guide closure and exact induced authored pin branch shape.
    for seed in sorted(ps):
        frontier = [frozenset([seed])]
        while frontier and len(seen) < cap:
            chosen = frontier.pop()
            if chosen in seen: continue
            seen.add(chosen)
            if len(chosen) == wanted_n:
                closed = guide_close(chosen, level)
                parts = [ps[i] for i in chosen]
                if len(closed) != wanted_n: failures['guideClosureAddsRoots'] += 1; continue
                if Counter(p['kind'] for p in parts) != wanted_kinds: failures['partKindInventoryMismatch'] += 1; continue
                if Counter(p['chainLockCount'] for p in parts) != wanted_locks: failures['chainLockInventoryMismatch'] += 1; continue
                if Counter(p['collectible'] for p in parts) != wanted_collect: failures['collectibleInventoryMismatch'] += 1; continue
                if len(induced(level, chosen)) != wanted_edges: failures['inducedPinEdgeCountMismatch'] += 1; continue
                if sum(e['guideOwnerId'] in chosen and e['lockedOwnerId'] in chosen for e in guide) != wanted_guides: failures['guideLinkCountMismatch'] += 1; continue
                if not native_shape_match(parts,target['parts'],templates): failures['nativeShapeOrDormantPinMismatch'] += 1; continue
                if not same_authored_topology(level,chosen,target,{p['id'] for p in target['parts']},templates):
                    failures['directedAuthoredTopologyMismatch'] += 1; continue
                if wanted_bombs:
                    bombs = [b for b in level.get('bombs', []) if b['ownerPartId'] in chosen]
                    if len(bombs) != wanted_bombs: failures['ownedBombCountMismatch'] += 1; continue
                result.append(tuple(sorted(chosen)))
                continue
            if len(chosen) > wanted_n: continue
            kinds = Counter(ps[i]['kind'] for i in chosen)
            if any(kinds[k] > wanted_kinds[k] for k in kinds): continue
            links = induced(level, chosen)
            if len(links) > wanted_edges: continue
            candidates = set().union(*(set(graph.neighbors(i)) for i in chosen)) - set(chosen)
            for nxt in sorted(candidates, reverse=True):
                nk = kinds.copy(); nk[ps[nxt]['kind']] += 1
                if any(nk[k] > wanted_kinds[k] for k in nk): continue
                frontier.append(frozenset((*chosen, nxt)))
    failures['searchStateCapReached'] += int(len(seen) >= cap)
    return result, failures


def make_candidate(model, source, selected, templates, pid):
    # instantiate_tile carries full native parts, all induced authored links,
    # guide closure and owned bombs; source subset stays in its exact pose.
    temp = {'sourceId': source['sourceId'], 'partIds': list(selected)}
    index = len(model.tiles); model.tiles.append(temp)
    model.levels[source['sourceId']] = source
    model.templates = templates
    out = {'id': pid, 'sourceId': 'tutorial-motif-' + str(pid), 'position': 0,
           'parts': [], 'links': [], 'guideLinks': [], 'bombs': [], 'complete': False,
           'sourceOriginal': False, 'validationStatus': 'INCOMPLETE_TUTORIAL_MOTIF_CANDIDATE',
           'assemblyProvenance': {'method': 'exact connected induced source motif; native poses/assets retained',
             'tileImports': [], 'decisions': [], 'nativeReplayRequired': True}}
    instantiate_tile(model, index, out, 0., np.array([0., 0.]))
    model.tiles.pop()
    out['assemblyProvenance']['tileImports'][0]['partMapping'] = {str(k): v for k, v in out['assemblyProvenance']['tileImports'][0]['partMapping'].items()}
    return out


def paired_sheet(reference, candidate, templates, path):
    """Pair the existing physical-solid diagnostic panels; no invented geometry."""
    a=native_solid_panel(reference,templates,width=400,height=550)
    b=native_solid_panel(candidate,templates,width=400,height=550)
    image=Image.new('RGB',(820,590),'#eee9dc');image.paste(a,(0,30));image.paste(b,(420,30))
    d=ImageDraw.Draw(image);d.text((12,7),f"SOURCE lesson position {reference['position']} | exact native solids",fill='#423422')
    d.text((432,7),f"MOTIF candidate {candidate['id']} | diagnostic only",fill='#423422')
    image.save(path)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--workspace', type=Path, required=True)
    ap.add_argument('--output', type=Path, required=True)
    ap.add_argument('--candidate-cap-per-source', type=int, default=8000)
    args = ap.parse_args(); w = args.workspace.resolve(); outdir = args.output.resolve(); outdir.mkdir(parents=True, exist_ok=True)
    data = w/'data'; levels = json.loads((data/'source500_native.json').read_text(encoding='utf8'))['levels']
    templates = {t['id']: t for t in json.loads((data/'source_templates.json').read_text(encoding='utf8'))['templates']}
    extras = data/'novelty_extras'; extra_levels = json.loads((extras/'source_parity_pack.json').read_text(encoding='utf8'))['levels']
    extra_templates = json.loads((extras/'source_templates.json').read_text(encoding='utf8'))['templates']
    merged = dict(templates); merged.update({t['id']: t for t in extra_templates})
    modelpath = w/'models/composition_pose.joblib'; payload = joblib.load(modelpath); pose_model = payload['model']
    macro = joblib.load(w/'models/macro_assembly.joblib')
    integrity = NativeAssemblyIntegrityAudit(merged, levels + extra_levels)
    seams = NativeGeometrySeamAudit(merged, levels + extra_levels)
    novelty = LayoutNoveltyIndex(levels + extra_levels, merged)
    targets = sorted((l for l in levels if l['position'] in range(1, 6)), key=lambda l:l['position'])
    report = {'schemaVersion': 3, 'complete': False, 'acceptedCount': 0,
              'status': 'INCOMPLETE_NATIVE_REPLAY_AND_HUMAN_DIFFICULTY_REVIEW_REQUIRED',
              'pilot': 'connected native tutorial motifs; target progression positions 1-5', 'targets': [], 'levels': [],
              'topologyRegression':topology_regression(),
              'limitations': ['Native poses and assets are retained exactly; this is structural matching, not native replay.',
                'Learned composition likelihood ranks authored source relation contexts; it does not predict difficulty.',
                'Raw geometry seams and novelty507 are screening gates only.', 'All proposals remain incomplete and unaccepted.']}
    pid = 4010001
    for target in targets:
        tcounts, tlinks, tguides, _, _, tbombs = inventory(target)
        found = []; failure_counts=Counter()
        for source in levels:
            if source['sourceId'] == target['sourceId'] or len(source['parts']) < len(target['parts']): continue
            selected_sets,source_failures=connected_subsets(source, target, merged, args.candidate_cap_per_source)
            failure_counts.update(source_failures)
            for selected in selected_sets:
                pseudo = {**source, 'parts':[p for p in source['parts'] if p['id'] in selected],
                          'links':induced(source, selected), 'guideLinks':[e for e in source.get('guideLinks',[]) if e['guideOwnerId'] in selected and e['lockedOwnerId'] in selected],
                          'bombs':[b for b in source.get('bombs',[]) if b['ownerPartId'] in selected]}
                # Avoid adding source external links or noise: exact induced inventory is mandatory.
                candidate=make_candidate(macro, source, selected, merged, pid)
                candidate['position']=target['position']; candidate['assemblyProvenance']['targetSourceId']=target['sourceId']
                rels=[r for r in relation_rows(candidate,merged) if r.get('relation')=='pin']
                ll=float(np.mean(pose_model.loglikelihood([r['context'] for r in rels],[r['pose'] for r in rels]))) if rels else float('-inf')
                candidate['assemblyProvenance']['sourceMotifPartIds']=list(selected)
                candidate['assemblyProvenance']['learnedCompositionMeanLogLikelihood']=ll
                candidate['assemblyProvenance']['compositionLikelihoodRelationCount']=len(rels)
                candidate['nativeAssemblyIntegrityAudit']=integrity.check(candidate)
                candidate['rawGeometrySeamAudit']=seams.check(candidate)
                candidate['novelty507']=novelty.check(candidate)
                candidate['screenPassed']=bool(candidate['nativeAssemblyIntegrityAudit']['nativeAssemblyIntegrityPassed'] and candidate['rawGeometrySeamAudit']['rawGeometrySeamPassed'] and candidate['novelty507']['passed'])
                found.append(candidate); pid += 1
        found.sort(key=lambda c:(not c['screenPassed'], -c['assemblyProvenance']['learnedCompositionMeanLogLikelihood']))
        for c in found[:8]:
            source_parts=set(c['assemblyProvenance']['sourceMotifPartIds'])
            source={**target,'parts':[p for p in target['parts'] if p['id'] in source_parts]}
            # The pair's left panel is the tutorial target; the right panel is
            # the extracted motif. Keep each panel's independently authored poses.
            sheet=outdir/f"paired_source_proposal_{c['id']}.png"
            paired_sheet(target,c,merged,sheet)
            c['pairedSheet']=sheet.name
            report['levels'].append(c)
        report['targets'].append({'position':target['position'],'sourceId':target['sourceId'],'parts':len(target['parts']),
          'kindInventory':dict(tcounts),'pinEdges':tlinks,'guideLinks':tguides,'bombs':tbombs,
          'candidateCount':len(found),'screenPassingCount':sum(c['screenPassed'] for c in found),
          'rejectedSubgraphCounts':dict(failure_counts),
          'subsetSearchComplete':failure_counts['searchStateCapReached']==0,
          'distinctLowDifficultyCandidatesExist':'unknown; human difficulty and native replay not measured',
          'reportedCandidateIds':[c['id'] for c in found[:8]]})
    hashes={str(p):hashfile(p) for p in [data/'source500_native.json',data/'source_templates.json', extras/'source_parity_pack.json', extras/'source_templates.json', modelpath, Path(__file__),Path(__file__).with_name('render_layout_diagnostics_v3.py')]}
    report['hashes']=hashes
    (outdir/'tutorial_motif_proposals.json').write_text(json.dumps(report,separators=(',',':')),encoding='utf8')
    (outdir/'tutorial_motif_report.json').write_text(json.dumps({k:v for k,v in report.items() if k!='levels'},indent=2),encoding='utf8')
    print(json.dumps({'targets':report['targets'],'proposalCount':len(report['levels']),'acceptedCount':0},indent=2))

if __name__ == '__main__': main()
