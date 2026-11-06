"""Extract a focused set of art/audio references from Rotate Rings Unity bundles."""
from pathlib import Path
import csv, hashlib, json, re
import UnityPy
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
BUNDLES = ROOT / "Research" / "extracted" / "assets" / "yoo" / "DefaultPackage"
OUT = ROOT / "Research" / "art"
ASSETS = ROOT / "RotateRings Test" / "Assets" / "RotateRings"

def name_of(data):
    return str(getattr(data, "name", None) or getattr(data, "m_Name", "") or "").strip()

def main():
    OUT.mkdir(parents=True, exist_ok=True)
    rows=[]
    type_counts={}
    for bundle in sorted(BUNDLES.iterdir()):
        if not bundle.is_file(): continue
        try:
            env=UnityPy.load(str(bundle))
        except Exception:
            continue
        for obj in env.objects:
            typ=obj.type.name
            type_counts[typ]=type_counts.get(typ,0)+1
            if typ in {"Texture2D","Sprite","AudioClip","Font","Material"}:
                try:
                    dat=obj.read()
                    n=name_of(dat)
                except Exception as e:
                    n=f"<read error: {type(e).__name__}>"
                rows.append((bundle.name,typ,obj.path_id,n))
    with (OUT/"asset_inventory.csv").open("w",newline="",encoding="utf-8-sig") as f:
        w=csv.writer(f); w.writerow(["bundle","type","path_id","name"]); w.writerows(rows)
    print("types",json.dumps(type_counts,sort_keys=True))
    for typ in ("Texture2D","Sprite","AudioClip","Font","Material"):
        print(typ, sum(1 for _,t,_,_ in rows if t==typ), "named", sum(1 for _,t,_,n in rows if t==typ and n))

    # Export compact source selections for the remake, retaining the exact bundle/path ID.
    wanted_tokens = ("ui_sprite_home", "ui_prefab_home", "ui_prefab_game", "ui_sprite_game",
                     "ui_sprite_public", "level_texture_ring", "effect_prefab_maingameplay")
    art_dir=ASSETS/"Art"; audio_dir=ASSETS/"Audio"
    art_dir.mkdir(parents=True,exist_ok=True); audio_dir.mkdir(parents=True,exist_ok=True)
    picks=[]; seen=set()
    for bundle in sorted(BUNDLES.iterdir()):
        low=bundle.name.lower()
        if not any(tok in low for tok in wanted_tokens): continue
        try: env=UnityPy.load(str(bundle))
        except Exception: continue
        for obj in env.objects:
            if obj.type.name not in ("Texture2D","Sprite"): continue
            try:
                data=obj.read(); nm=name_of(data) or obj.type.name
                image=data.image
                if image is None: continue
                # Skip tiny masks/noise utility textures from the project export, but retain in the reference sheet.
                if obj.type.name=="Texture2D" and "level_texture_ring" not in low and min(image.size)<16: continue
                key=(bundle.name,obj.path_id,obj.type.name)
                if key in seen: continue
                seen.add(key)
                safe=re.sub(r"[^A-Za-z0-9_.-]+","_",nm).strip("._")[:72] or "asset"
                dest=art_dir/f"{safe}_{str(obj.path_id).replace('-','n')}.png"
                image.save(dest)
                # Source bundle hash is intentionally repeated in each manifest row for provenance.
                picks.append({"file":str(dest.relative_to(ROOT)),"name":nm,"type":obj.type.name,
                              "bundle":bundle.name,"path_id":obj.path_id,
                              "bundle_sha256":hashlib.sha256(bundle.read_bytes()).hexdigest(),
                              "width":image.width,"height":image.height})
            except Exception as e:
                print("art skip",bundle.name,obj.path_id,type(e).__name__)
    with (OUT/"export_manifest.json").open("w",encoding="utf-8") as f:
        json.dump(picks,f,indent=2,ensure_ascii=False)
    # Contact sheets by bundle category; labels are enough to locate every source.
    for category in ("home", "game", "ring"):
        group=[p for p in picks if (category=="home" and ("home" in p["bundle"].lower() or "home" in p["name"].lower()))
               or (category=="game" and ("maingameplay" in p["bundle"].lower() or "game" in p["bundle"].lower()))
               or (category=="ring" and "level_texture_ring" in p["bundle"].lower())]
        if not group: continue
        cellw,cellh,cap=190,160,30; cols=5; rowsn=(len(group)+cols-1)//cols
        sheet=Image.new("RGB",(cols*cellw,rowsn*cellh),(42,42,48)); draw=ImageDraw.Draw(sheet)
        for i,p in enumerate(group):
            im=Image.open(ROOT/p["file"]).convert("RGBA"); im.thumbnail((cellw-12,cellh-cap-10))
            x=(i%cols)*cellw+(cellw-im.width)//2; y=(i//cols)*cellh+5
            sheet.paste(im,(x,y),im); label=p["name"][:24]
            draw.text(((i%cols)*cellw+5,(i//cols)*cellh+cellh-cap),label,fill="white")
        sheet.save(OUT/f"{category}_contact_sheet.png")
    # Include original ring material textures and a small selection of gameplay sound samples.
    # AudioClip samples are decoded by UnityPy; errors are kept out of the export.
    audio_wanted={"rr_sfx_rock_break_1","rr_sfx_wood_break_1","rr_sfx_jelly_break_1","rr_bgm_level2"}
    aud=[]
    for bundle in sorted(BUNDLES.iterdir()):
        if "audio" not in bundle.name.lower(): continue
        try: env=UnityPy.load(str(bundle))
        except Exception: continue
        for obj in env.objects:
            if obj.type.name!="AudioClip": continue
            try:
                data=obj.read(); nm=name_of(data)
                if nm not in audio_wanted: continue
                samples=data.samples
                for subname,raw in samples.items():
                    ext=Path(subname).suffix or ".wav"
                    dest=audio_dir/f"{nm}{ext}"
                    dest.write_bytes(raw)
                    aud.append({"file":str(dest.relative_to(ROOT)),"name":nm,"bundle":bundle.name,
                                "path_id":obj.path_id,"bundle_sha256":hashlib.sha256(bundle.read_bytes()).hexdigest(),"source_member":subname})
            except Exception as e: print("audio skip",bundle.name,type(e).__name__)
    with (OUT/"audio_manifest.json").open("w",encoding="utf-8") as f: json.dump(aud,f,indent=2,ensure_ascii=False)
    print("exported art",len(picks),"audio",len(aud))

if __name__=="__main__": main()
