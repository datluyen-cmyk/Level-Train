"""Export APK scenes outside the training progression as native novelty exclusions.

These scenes are never training examples or solvability labels. Unsupported exports
remain explicit failures; the caller must not describe incomplete coverage as 507.
"""
from pathlib import Path
import hashlib
import json

import normalize_corpus
from export_native_physics_v2 import export_level
from export_source_parity_v3 import export_source

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Research/physics_v3/novelty_extras"


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    corpus_path = ROOT / "Research/normalized_corpus.json"
    trained = json.loads(corpus_path.read_text(encoding="utf8"))["levels"]
    trained_ids = {str(level["id"]) for level in trained}
    paths = sorted((p for p in (ROOT / "Research/levels").glob("*.json")
                    if p.stem.isdigit() and p.stem not in trained_ids), key=lambda p: int(p.stem))
    levels, templates, failed = [], {}, []
    for path in paths:
        try:
            source = json.loads(path.read_text(encoding="utf8"))
            normalized = normalize_corpus.normalize(source, 0)
            normalized["provenance"]["corpusRole"] = "novelty exclusion only; outside training progression"
            normalized["provenance"]["progressionPosition"] = None
            native = next(value for kind, value in export_level(normalized) if kind == "level")
            level, catalog = export_source(normalized, native)
            level["noveltyOnly"] = True
            level["parityStatus"] = "native novelty exclusion only; no verified solution"
            levels.append(level)
            templates.update({template["id"]: template for template in catalog})
            print("EXPORTED", path.stem, len(level["parts"]), flush=True)
        except Exception as error:
            failed.append({"sourceId": path.stem, "rawSha256": sha(path),
                           "errorType": type(error).__name__, "error": str(error)})
            print("UNSUPPORTED", path.stem, type(error).__name__, str(error), flush=True)
    documents = {
        "source_parity_pack.json": {"schemaVersion": 3, "sourceOriginalOnly": True,
            "complete": False, "noveltyOnly": True, "levels": levels},
        "source_templates.json": {"schemaVersion": 3, "sourceOriginalOnly": True,
            "templates": list(templates.values())},
    }
    for name, document in documents.items():
        (OUT / name).write_text(json.dumps(document, separators=(",", ":")), encoding="utf8")
    manifest = {"status": "complete" if not failed else "incomplete",
        "trainingCount": len(trained), "requestedExtraCount": len(paths),
        "exportedExtraCount": len(levels), "totalNativeNoveltyExclusions": len(trained) + len(levels),
        "exportedSourceIds": [level["sourceId"] for level in levels], "failed": failed,
        "semantics": "Untouched native source layouts, novelty-only; no training, solve or difficulty labels.",
        "hashes": {name: sha(OUT / name) for name in documents},
        "codeHashes": {str(path.relative_to(ROOT)): sha(path) for path in [Path(__file__),
            ROOT / "Tools/normalize_corpus.py", ROOT / "Tools/export_native_physics_v2.py",
            ROOT / "Tools/export_source_parity_v3.py"]}, "trainingCorpusSha256": sha(corpus_path)}
    (OUT / "manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf8")
    print(json.dumps(manifest, indent=2))


if __name__ == "__main__":
    main()
