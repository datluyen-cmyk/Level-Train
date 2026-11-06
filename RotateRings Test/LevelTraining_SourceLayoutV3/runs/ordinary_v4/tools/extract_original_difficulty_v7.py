"""Read shipped MemoryPack TbLevelDifficult; no fitting or Unity operations."""
from pathlib import Path
from collections import Counter
import argparse
import brotli
import hashlib
import json
import re
import struct


NAMES={0:"Normal",1:"Hard",2:"VeryHard"}


def digest(value):
    return hashlib.sha256(value).hexdigest()


def decode_difficulty_table(meta,raw,index=55):
    count=struct.unpack_from("<i",meta)[0]
    if not 0<=index<count:raise ValueError("Table index absent")
    entry=struct.unpack_from("<Biqii",meta,4+index*21)
    compressed=raw[entry[2]:entry[2]+entry[4]]
    payload=brotli.decompress(compressed)
    cursor=0
    def integer():
        nonlocal cursor
        if cursor+4>len(payload):raise ValueError("Truncated MemoryPack integer")
        value=struct.unpack_from("<i",payload,cursor)[0];cursor+=4;return value
    rows=[]
    row_count=integer()
    if row_count!=entry[1]:raise ValueError("Metadata rowcount differs")
    for _ in range(row_count):
        key=integer();header=payload[cursor];cursor+=1
        if header!=7:raise ValueError("Unexpected LevelDifficult property count")
        ident,mode,begin,end,divisor=[integer()for _ in range(5)]
        remainder_count=integer()
        if remainder_count < -1 or remainder_count>10000:raise ValueError("Unsupported remainder list")
        remainders=[integer()for _ in range(remainder_count)]if remainder_count>=0 else None
        difficulty=integer()
        if difficulty not in NAMES or ident!=key:raise ValueError("Unsupported enum or key identity")
        rows.append({"dictionaryKey":key,"Id":ident,"GameMode":mode,"Begin":begin,"End":end,
            "Divisor":divisor,"Remainder":remainders,"DifficultMode":difficulty,"difficultyName":NAMES[difficulty]})
    if cursor!=len(payload):raise ValueError("Trailing unparsed table bytes")
    return rows,{"metadataTableIndex":index,"metadataEntryRaw":list(entry),"compressedBytes":len(compressed),
        "compressedTableSha256":digest(compressed),"decompressedBytes":len(payload),"decompressedTableSha256":digest(payload),
        "consumedBytes":cursor,"rowCount":row_count,"memoryPackObjectPropertyCount":7}


def resolve_default(rules,position,mode=0):
    for rule in rules:
        if rule["GameMode"]==mode and rule["Begin"]<=position<=rule["End"] and rule["Divisor"]>0:
            if position%rule["Divisor"] in(rule["Remainder"]or[]):return rule["DifficultMode"],rule["Id"]
    return 0,None


def extract(root,workspace,output):
    root,workspace,output=Path(root).resolve(),Path(workspace).resolve(),Path(output).resolve()
    if output.exists():raise ValueError("Refuse existing difficulty extraction report")
    meta_path=root/"Research/rules/game_config_meta_bytes.bin"
    configs=list((root/"Research/extracted/assets/yoo/DefaultPackage").glob("*config_default*"))
    if len(configs)!=1:raise ValueError("Default config is ambiguous")
    config_path=configs[0]
    source_paths={"meta":meta_path,"defaultConfig":config_path,"sourcePack":workspace/"data/source500_native.json",
        "sourceFilter":workspace/"data/ordinary_source_filter_v4.json","progression":root/"Research/rules/main_progressions.json",
        "enum":root/"Research/rules/decompiled/RotateRings.Common/ELevelDifficulty.cs",
        "resolver":root/"Research/rules/decompiled/RotateRings.Common/LevelDifficultyUtil.cs",
        "schema":root/"Research/rules/decompiled/Example.Cfg/LevelDifficult.cs",
        "tableIndexMap":root/"Research/rules/decompiled/Example.Cfg/ExampleTables.cs","extractor":Path(__file__)}
    rules,table=decode_difficulty_table(meta_path.read_bytes(),config_path.read_bytes())
    levels=json.loads(source_paths["sourcePack"].read_text(encoding="utf8"))["levels"]
    ordinary_filter=json.loads(source_paths["sourceFilter"].read_text(encoding="utf8"))
    allowed=set(map(str,ordinary_filter["ordinarySourceIds"]))
    if {str(l["sourceId"])for l in levels}!=allowed:raise ValueError("Ordinary filter/source pack mismatch")
    progression=json.loads(source_paths["progression"].read_text(encoding="utf8"))["tables"]["1"]
    positions={r["position"]:r for r in progression};rows=[]
    for level in sorted(levels,key=lambda x:x["position"]):
        position=level["position"];sid=str(level["sourceId"])
        original=positions[position]
        if re.sub(r"^Level","",original["physics"])!=sid:raise ValueError("Source differs from shipped progression")
        mode,rule=resolve_default(rules,position)
        kinds=Counter(p["kind"]for p in level["parts"])
        # This conditional column is not an assertion that remote/random
        # experiments are enabled in the supplied APK or an active session.
        experimental=None if position<11 else NAMES[2 if position%10==9 else 1 if position%10==5 else 0]
        rows.append({"sourceId":sid,"progressionPosition":position,"defaultConfigDifficulty":NAMES[mode],
            "defaultConfigDifficultyMode":mode,"matchedDefaultRuleId":rule,
            "conditionalRandomExperimentFallbackDifficulty":experimental,
            "rootCount":len(level["parts"]),"kinds":dict(kinds),"pinCount":len(level["links"]),"guideCount":len(level["guideLinks"]),
            "bombCount":len(level["bombs"]),"chainLockedRootCount":sum(p.get("chainLockCount",0)>0 for p in level["parts"])})
    selected=[]
    for label in("Hard","VeryHard"):
        selected.extend([r for r in rows if r["defaultConfigDifficulty"]==label][:4])
    report={"schemaVersion":7,"status":"SHIPPED_DEFAULT_DIFFICULTY_RULES_EXTRACTED_NO_PLAYER_CALIBRATION",
        "table":table,"rulesInSerializedDictionaryInsertionOrder":rules,"enumMapping":NAMES,
        "sourceHashes":{name:{"path":str(path),"sha256":digest(path.read_bytes())}for name,path in source_paths.items()},
        "ordinarySourceCount":len(levels),"ordinaryCountsByDefaultTag":dict(Counter(r["defaultConfigDifficulty"]for r in rows)),
        "first100OrdinaryCountsByDefaultTag":dict(Counter(r["defaultConfigDifficulty"]for r in rows if r["progressionPosition"]<=100)),
        "earliestHardAndVeryHardCandidates":selected,"levels":rows,
        "resolverSemantics":{"mainMode":0,"endlessMode":1,"firstMatchingRuleWins":True,"unmatched":"Normal",
            "specialLevelModeOverride":"Normal when SpecialLevelModule.IsSpecialLevelModeFor(gameMode)",
            "randomExperimentOverride":"Only Main mode, level>=11, FeatureConfig.LevelRandom.ResolveEnable(): matched runtime bonus>=2VeryHard/1Hard/elseNormal; fallback lastdigit5Hard/9VeryHard.",
            "runtimeExperimentEnabled":"UNVERIFIED; no active-session or remote-config assumption"},
        "limits":["These are original shipped designer/UI difficulty tags, not measured human difficulty or proof of native solvability.",
            "Core JSON difficulty/ability fields belong to another model and are not substituted for these tags.",
            "All first18 main progression positions are default Normal; previousG/Htutorialprototypes are not originalHard/VeryHard references.",
            "Pictorial exclusions remain excluded; no training, level generation, Unity edits or new acceptance performed."]}
    output.parent.mkdir(parents=True,exist_ok=True);output.write_text(json.dumps(report,indent=2),encoding="utf8")
    print(json.dumps({"report":str(output),"reportSha256":digest(output.read_bytes()),"counts":report["ordinaryCountsByDefaultTag"],"first100":report["first100OrdinaryCountsByDefaultTag"],
        "earliest":[[r["progressionPosition"],r["sourceId"],r["defaultConfigDifficulty"]]for r in selected]}))


if __name__=="__main__":
    parser=argparse.ArgumentParser();parser.add_argument("--root",required=True,type=Path);parser.add_argument("--workspace",required=True,type=Path)
    parser.add_argument("--output",required=True,type=Path);args=parser.parse_args();extract(args.root,args.workspace,args.output)
