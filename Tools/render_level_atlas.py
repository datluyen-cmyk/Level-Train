"""Static level-design contact sheet, independent of Unity runtime screenshots."""
from PIL import Image,ImageDraw,ImageFont
from pathlib import Path
import json,math
root=Path(__file__).resolve().parents[1]
path=root/'RotateRings Test/LevelTraining/batches/final-300/generated_levels.json'
levels=json.loads(path.read_text(encoding='utf8'))['levels']
selection=[1,2,3,5,10,20,35,50,75,100,125,150,175,200,225,250,275,285,295,300]
W,H=2400,2100;out=Image.new('RGB',(W,H),'#faf6ec');draw=ImageDraw.Draw(out)
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',23);small=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',18)
draw.text((40,20),'ROTATE RINGS / 300 NEW LEVELS',font=font,fill='#423422')
palette=['#32AFDD','#EC8F2E','#984BE0','#DD433E','#6AC53E','#4E77DD','#23A468']
for k,idx in enumerate(selection):
    level=levels[idx-1];s=level['state'];ps={p['id']:p for p in s['parts']};left=30+k%5*475;top=80+k//5*500
    draw.rounded_rectangle((left,top,left+450,top+470),radius=20,fill='#fffdf7',outline='#e4d8be',width=2)
    draw.text((left+18,top+12),f'{idx:03d}   {len(ps)} pieces / {len(level["solution"])} gestures',font=small,fill='#59472d')
    minx=min(p['x']-p['radius']-p['width'] for p in ps.values());maxx=max(p['x']+p['radius']+p['width'] for p in ps.values())
    miny=min(p['y']-p['radius']-p['width'] for p in ps.values());maxy=max(p['y']+p['radius']+p['width'] for p in ps.values())
    scale=min(400/max(1,maxx-minx),380/max(1,maxy-miny));cx=(minx+maxx)/2;cy=(miny+maxy)/2
    def point(x,y):return (left+225+(x-cx)*scale,top+255-(y-cy)*scale)
    for e in s['links']:
        a,b=ps[e['source']],ps[e['target']];dx=b['x']-a['x'];dy=b['y']-a['y'];length=math.hypot(dx,dy);dx/=length;dy/=length
        x1,y1=point(a['x']+dx*a['radius'],a['y']+dy*a['radius']);x2,y2=point(b['x']-dx*(b['radius']-b['width']*.65),b['y']-dy*(b['radius']-b['width']*.65))
        color=palette[a['color']%len(palette)];draw.line((x1,y1,x2,y2),fill=color,width=max(3,int(scale*.25)))
        px,py=-dy*.22*scale,-dx*.22*scale;draw.line((x2-px,y2-py,x2+px,y2+py),fill=color,width=max(3,int(scale*.20)))
    for p in ps.values():
        x,y=point(p['x'],p['y']);r=p['radius']*scale;color=palette[p['color']%len(palette)];width=max(3,int(p['width']*scale))
        if p['kind']=='slider':
            a=math.radians(p['axis']);v=(math.cos(a)*p['length']*.5*scale,-math.sin(a)*p['length']*.5*scale)
            draw.line((x-v[0],y-v[1],x+v[0],y+v[1]),fill=color,width=width)
            draw.regular_polygon((x+v[0],y+v[1],max(4,scale*.25)),3,rotation=-p['axis']+90,fill=color)
        elif p['gap']==0:draw.ellipse((x-r,y-r,x+r,y+r),outline=color,width=width)
        else:
            start=-(p['angle']-p['gap']/2);end=start+360-p['gap'];draw.arc((x-r,y-r,x+r,y+r),start,end,fill=color,width=width)
        if p.get('key'):draw.text((x-7,y-13),'K',font=small,fill='#73540d')
        if p.get('lockKey'):draw.text((x-7,y-13),'L',font=small,fill='#73540d')
draw.text((35,2070),'Design preview only. Solvability: reconstructed pin/gap rules. Human difficulty and native collision parity unmeasured.',font=small,fill='#796d58')
out.save(root/'RotateRings Test/LevelTraining/Level Design Preview.png')
print('Saved Level Design Preview.png')
