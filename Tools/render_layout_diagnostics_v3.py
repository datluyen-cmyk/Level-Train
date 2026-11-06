"""Solid-collider composition sheet, explicitly separate from Unity visual QA."""
from pathlib import Path
import argparse,json,math
from PIL import Image,ImageDraw,ImageChops

PALETTE=['#32AFDD','#EC8F2E','#984BE0','#DD433E','#6AC53E','#4E77DD','#23A468']

def outlines(level,templates):
    result=[]
    for part in level['parts']:
        radians=math.radians(part['angle']);c,s=math.cos(radians),math.sin(radians)
        for shape in templates[part['templateId']]['physicalShapes']:
            if shape['trigger'] or not shape.get('sourceEnabled',True):continue
            points=shape.get('points',[])
            if shape.get('shapeType')=='circle':
                center=shape['center'];radius=shape['radius'];points=[{'x':center['x']+radius*math.cos(i*math.pi/32),'y':center['y']+radius*math.sin(i*math.pi/32)} for i in range(64)]
            if not points:continue
            paths=shape.get('paths') or [{'points':points}]
            posed=[[(part['x']+c*p['x']-s*p['y'],part['y']+s*p['x']+c*p['y']) for p in path['points']] for path in paths]
            result.append((posed,'#969696' if part['kind'] in ('fixed','butterfly') else PALETTE[part.get('color',0)%len(PALETTE)]))
    return result

def panel(level,templates,width=400,height=550):
    out=Image.new('RGB',(width*2,height*2),'#fffdf0');draw=ImageDraw.Draw(out);polygons=outlines(level,templates)
    points=[p for paths,_ in polygons for polygon in paths for p in polygon]
    if points:
        xmin=min(p[0] for p in points);xmax=max(p[0] for p in points);ymin=min(p[1] for p in points);ymax=max(p[1] for p in points)
        scale=min((width*2-50)/max(xmax-xmin,.01),(height*2-130)/max(ymax-ymin,.01));cx=(xmin+xmax)/2;cy=(ymin+ymax)/2
        for paths,color in polygons:
            # Even-odd fill retains native interior paths rather than turning
            # a large closed ring into an opaque solid disc in this diagnostic.
            mask=Image.new('1',out.size,0)
            for polygon in paths:
                layer=Image.new('1',out.size,0);ImageDraw.Draw(layer).polygon([((x-cx)*scale+width,(cy-y)*scale+height+25) for x,y in polygon],fill=1)
                mask=ImageChops.logical_xor(mask,layer)
            out.paste(color,(0,0,*out.size),mask)
    label=f"{level.get('sourceId',level.get('id'))} | parts {len(level['parts'])}"
    draw.text((20,12),label,fill='black');draw.text((20,40),'SOLID COLLIDER DIAGNOSTIC',fill='#8e3300')
    if not level.get('sourceOriginal',False):
        succeeded=level.get('assemblyProvenance',{}).get('rewriteSucceeded')
        status='FAILED REWRITE / UNCHANGED SOURCE' if succeeded is False else 'UNVALIDATED PROPOSAL / NOT ACCEPTED'
        draw.text((20,66),status,fill='#b11c1c')
    return out.resize((width,height),Image.Resampling.LANCZOS)

def main():
    p=argparse.ArgumentParser();p.add_argument('--pack',type=Path,required=True);p.add_argument('--templates',type=Path,required=True);p.add_argument('--output',type=Path,required=True);p.add_argument('--source',type=Path);a=p.parse_args()
    pack=json.loads(a.pack.read_text(encoding='utf8'));ts={t['id']:t for t in json.loads(a.templates.read_text(encoding='utf8'))['templates']};levels=pack['levels'][:12]
    images=[]
    sources=json.loads(a.source.read_text(encoding='utf8'))['levels'] if a.source else []
    for level in levels:
        if sources:
            ref=next((l for l in sources if l['position']==level['position']),None)
            if ref:images.append(panel(ref,ts))
        images.append(panel(level,ts))
    columns=4;rows=math.ceil(len(images)/columns);sheet=Image.new('RGB',(columns*400,rows*550),'#eeeeee')
    for i,im in enumerate(images):sheet.paste(im,((i%columns)*400,(i//columns)*550))
    a.output.parent.mkdir(parents=True,exist_ok=True);sheet.save(a.output);print(a.output)

if __name__=='__main__':main()
