"""Exact native feature sockets; unrelated owner attachments are not merged."""
import copy,json,hashlib
from macro_assembly_v3 import socket_clean

def exact_shape(shape):
 return {k:v for k,v in shape.items() if k not in ['sourceColliderId','attachmentId']}

def shape_key(shape):return json.dumps(exact_shape(shape),sort_keys=True,separators=(',',':'))

def complete_feature_indices(template,indices):
 selected=set(indices);attachments={template['physicalShapes'][i].get('attachmentId',-1) for i in indices};attachments.discard(-1)
 for i,s in enumerate(template['physicalShapes']):
  if s.get('attachmentId',-1) in attachments:selected.add(i)
 return sorted(selected)

def feature_shape_map(source_template,target_template,indices):
 """Exact each full attachment shape, preserving actual source polygon data."""
 mapping={};used=set()
 for source_index in complete_feature_indices(source_template,indices):
  key=shape_key(source_template['physicalShapes'][source_index]);matches=[i for i,s in enumerate(target_template['physicalShapes']) if i not in used and shape_key(s)==key]
  if not matches:return None
  mapping[source_index]=matches[0];used.add(matches[0])
 return mapping

def body_key(part,templates):
 t=templates[part['templateId']];body=[exact_shape(s) for s in t['physicalShapes'] if s['role']=='body'];geometry={k:t.get(k) for k in ['kind','radius','width','gap','length','visualRadius','visualWidth','visualGap','gapDirectionDeg','arcSubdivisions','movement']};geometry['body']=body
 if part['kind']=='slider':geometry['pullVisual']=socket_clean(t.get('pullVisual',{}))
 return hashlib.sha256(json.dumps(geometry,sort_keys=True,separators=(',',':')).encode()).hexdigest()

def remap_donor_relation(edge,owner_mapping,shape_maps,templates,parts):
 result=copy.deepcopy(edge)
 for side in ['source','target']:
  old=edge[side];result[side]=owner_mapping[old]
  if old not in shape_maps:continue
  indexmap=shape_maps[old];result[side+'ShapeIndices']=[indexmap[i] for i in edge[side+'ShapeIndices']];t=templates[parts[result[side]]['templateId']];heads=[t['physicalShapes'][i].get('attachmentId',-1) for i in result[side+'ShapeIndices'] if t['physicalShapes'][i]['role']=='head_trigger'];result[side+'AttachmentId']=heads[0] if heads else -1
 return result
