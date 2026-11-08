"""Source native macro evidence including unlinked static Pull/ring composition.

These source-only geometrical edges describe candidates for interaction. They
are never exported as native pin/guide links or labelled verified active locks.
"""
from pathlib import Path
from collections import Counter
import argparse,json,sys
import numpy as np,networkx as nx,joblib
from scipy.special import logsumexp
from macro_assembly_v3 import MacroAssemblyModel,macro_tiles,evaluate_macro
from global_composition_v3 import graph_for
from layout_learning_v3 import read,hashfile,level_context
from source_physical_composition_v3 import composition_edges

def composition_graphs(level,templates):
 native=graph_for(level).to_undirected();full=nx.Graph();full.add_nodes_from(native);protected=nx.Graph();protected.add_nodes_from(native)
 for a,b in native.edges:
  full.add_edge(a,b,weight=4.,kind='authoredNativeRelation',strong=True);protected.add_edge(a,b,weight=4.,kind='authoredNativeRelation',strong=True)
 records=composition_edges(level,templates)
 for e in records:
  a,b=e['source'],e['target']
  if not full.has_edge(a,b):full.add_edge(a,b,**{k:v for k,v in e.items() if k not in ['source','target']})
  if e.get('strong',False) and not protected.has_edge(a,b):protected.add_edge(a,b,**{k:v for k,v in e.items() if k not in ['source','target']})
 return full,protected,records

def semantic_tiles(level,templates,graphs=None):
 full,protected,records=graphs or composition_graphs(level,templates);tiles=macro_tiles(level,templates,composition_graph=full,retain_all_boundary=True,boundary_graph=protected)
 for t in tiles:
  ids=set(t['partIds']);r=[e for e in records if e['source'] in ids and e['target'] in ids];g=protected.subgraph(ids);n=max(1,len(ids));counts=Counter(e['kind'] for e in r);t['descriptor'] += [sum(e.get('strong',False) for e in r)/n,counts['pull_axis_crosses_ring']/n,counts['pull_hull_contains_ring_center']/n,counts['pull_near_ring']/n,(g.number_of_edges()-n+nx.number_connected_components(g))/n,nx.number_connected_components(g)/n];t['sourcePhysicalCompositionEvidence']=r;t['compositionEvidenceMeaning']='static native source geometry candidates; not active native lock/release labels'
 return tiles

class SemanticMacroAssemblyModel(MacroAssemblyModel):
 def fit(self,levels,templates,families):
  self.analysis_graphs={};self.composition_graphs={};self.physical_composition_records={}
  for l in levels:
   full,protected,records=composition_graphs(l,templates);self.analysis_graphs[l['sourceId']]=full;self.composition_graphs[l['sourceId']]=protected;self.physical_composition_records[l['sourceId']]=records
  return super().fit(levels,templates,families,tile_builder=lambda l,t:semantic_tiles(l,t,(self.analysis_graphs[l['sourceId']],self.composition_graphs[l['sourceId']],self.physical_composition_records[l['sourceId']])))
 def sample_tile(self,reference,rng):
  # Source references are immutable for the duration of a generation run.
  # Rebuilding exact collider composition and communities for every sampled
  # option repeated the same expensive analysis hundreds of times per level.
  cache=getattr(self,'_reference_sampling_contexts',None)
  if cache is None:cache={};self._reference_sampling_contexts=cache
  key=(id(reference),id(self.templates))
  if key not in cache:
   tiles=semantic_tiles(reference,self.templates);context=level_context(reference)
   distances,indices=self.nn.kneighbors(self.context_scaler.transform([context+t['descriptor'] for t in tiles]))
   cache[key]=(distances,indices)
  distances,indices=cache[key];choice=int(rng.integers(len(indices)));w=-.5*distances[choice]**2
  return int(rng.choice(indices[choice],p=np.exp(w-logsumexp(w))))

def main():
 p=argparse.ArgumentParser();p.add_argument('--workspace',required=True,type=Path);a=p.parse_args();w=a.workspace;levels=read(w/'data/source500_native.json')['levels'];ts={t['id']:t for t in read(w/'data/source_templates.json')['templates']};splits=read(w/'models/family_splits.json');model=SemanticMacroAssemblyModel().fit(levels,ts,splits['families']);report=evaluate_macro(model,splits);path=w/'models/macro_physical_assembly.joblib';joblib.dump(model,path,compress=3);report.update(sourceCount=len(levels),tileCount=len(model.tiles),macroRelationCount=len(model.relations),compositionEdgeCounts=dict(Counter(e['kind'] for rows in model.physical_composition_records.values() for e in rows)),sourceClosure='All native guide bindings and all native/authored plus strong static physical-composition neighbors become macro boundaries. Weak near-body candidates affect community grouping only.',physicalMeaning='Static source geometry candidates; no active lock, native physics, difficulty or quality certification',hashes={'sourcePack':hashfile(w/'data/source500_native.json'),'templates':hashfile(w/'data/source_templates.json'),'familySplits':hashfile(w/'models/family_splits.json'),'physicalAnalyzer':hashfile(Path(__file__).with_name('source_physical_composition_v3.py')),'trainer':hashfile(Path(__file__)),'macroImplementation':hashfile(Path(__file__).with_name('macro_assembly_v3.py')),'model':hashfile(path)});(w/'models/macro_physical_assembly_evaluation.json').write_text(json.dumps(report,indent=2));print(json.dumps({k:report[k] for k in ['sourceCount','tileCount','macroRelationCount','compositionEdgeCounts','summary']},indent=2),flush=True)
if __name__=='__main__':
 sys.modules.setdefault('semantic_macro_assembly_v3',sys.modules[__name__]);SemanticMacroAssemblyModel.__module__='semantic_macro_assembly_v3';main()
