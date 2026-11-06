"""Reuse an old top80 option array only when semantic filtering removes none.

The new options function differs by one rejection predicate before ranking.
If every member of the old ordered top80 passes it, that top80 is unchanged.
Otherwise discard this cache and let the corrected generator enumerate again.
"""
from pathlib import Path
import argparse
import hashlib
import inspect
import json

import native_asset_variant_v3 as old
import native_asset_variant_semantic_v3 as new
from guide_neighborhood_pilot_v3 import payload_compact
from layout_learning_v3 import hashfile, read
from orientation_semantics_v3 import meaningful_edit


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--workspace", required=True, type=Path)
    p.add_argument("--old-cache", required=True, type=Path)
    p.add_argument("--output-cache", required=True, type=Path)
    args = p.parse_args()
    w = args.workspace.resolve()
    source = w/"data/source500_native.json"
    old_function, new_function = inspect.getsource(old.options), inspect.getsource(new.options)
    inserted = "    if not meaningful_edit(target,pose,templates,oldout,newout):reject['physicallyInputEquivalentOrientationEdit']+=1;continue\n"
    assert new_function.replace(inserted, "") == old_function, "Functions differ beyond pure semantic rejection"
    old_hash = hashlib.sha256(old_function.encode()).hexdigest()
    new_hash = hashlib.sha256(new_function.encode()).hexdigest()
    with source.open() as stream:
        levels = json.load(stream, object_hook=payload_compact)["levels"]
    by = {l["position"]: l for l in levels}
    templates = {t["id"]: t for t in read(w/"data/source_templates.json")["templates"]}
    source_hash = hashfile(source)
    rows = []
    args.output_cache.mkdir(parents=True, exist_ok=True)
    for path in sorted(args.old_cache.glob("*.json")):
        position = int(path.stem)
        saved = read(path)
        if saved.get("optionsFunctionSha256") != old_hash or saved.get("sourcePackSha256") != source_hash:
            rows.append({"position": position, "reused": False, "reason": "source_or_function_hash_mismatch"})
            continue
        ps = {p["id"]: p for p in by[position]["parts"]}
        cosmetic = []
        count = 0
        for key, choices in saved["options"].items():
            root = int(key)
            for index, choice in enumerate(choices):
                count += 1
                if not meaningful_edit(ps[root], choice["part"], templates, choice["oldTargets"], choice["newTargets"]):
                    cosmetic.append([root, index])
        if cosmetic:
            rows.append({"position": position, "reused": False, "reason": "must_regenerate_after_semantic_filtering", "cosmeticTopOptions": cosmetic, "checkedTopOptionCount": count})
            continue
        saved["optionsFunctionSha256"] = new_hash
        saved["semanticMigrationCertificate"] = {"oldOptionsFunctionSha256": old_hash, "newOptionsFunctionSha256": new_hash,
            "legacyCacheSha256": hashfile(path), "semanticModuleSha256": hashfile(Path(__file__).with_name("orientation_semantics_v3.py")),
            "migrationToolSha256": hashfile(Path(__file__)), "checkedTopOptionCount": count,
            "proof": "Only code change is one pre-ranking rejection. All old ordered top80 options pass it. Removing lower-ranked rejected candidates cannot alter top80."}
        (args.output_cache/path.name).write_text(json.dumps(saved, separators=(",", ":")), encoding="utf8")
        rows.append({"position": position, "reused": True, "checkedTopOptionCount": count})
    report = {"acceptedCount": 0, "reusedReferenceCacheCount": sum(r["reused"] for r in rows), "levels": rows,
              "hashes": {"legacyFunction": old_hash, "semanticFunction": new_hash, "sourcePack": source_hash, "tool": hashfile(Path(__file__))}}
    (args.output_cache.parent/"semantic_options_cache_migration.json").write_text(json.dumps(report, indent=2), encoding="utf8")
    print("SEMANTIC_CACHE_MIGRATION", report["reusedReferenceCacheCount"], "/", len(rows), flush=True)


if __name__ == "__main__":
    main()
