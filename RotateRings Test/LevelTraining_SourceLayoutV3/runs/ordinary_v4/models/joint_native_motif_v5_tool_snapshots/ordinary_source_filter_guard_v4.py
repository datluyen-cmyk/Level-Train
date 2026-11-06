"""Fail-closed user scope guard for curated ordinary source/refitted models.

Categorical exclusions are explicit reviewed source IDs. This module never
infers pictures from generic radial/grid geometry or statistical clusters.
"""
from pathlib import Path
import hashlib
import json


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def validate_source_filter(manifest, source_pack_sha256, source_ids=None):
    if manifest.get("curationStatus") != "CURATED_ORDINARY_TRAINING_CORPUS":
        raise ValueError("Ordinary corpus curation is pending; pictorial-source models/proposals cannot be used")
    if manifest.get("sourcePackSha256") != source_pack_sha256:
        raise ValueError("Curated ordinary source filter is stale for the source pack")
    ordinary = list(map(str, manifest.get("ordinarySourceIds", [])))
    excluded = set(map(str, manifest.get("excludedSourceIds", [])))
    if len(ordinary) != len(set(ordinary)):
        raise ValueError("Duplicate ordinary source IDs")
    if not ordinary:
        raise ValueError("Curated corpus contains no eligible ordinary source")
    if manifest.get("actualUniqueOrdinaryCount", len(ordinary)) != len(ordinary):
        raise ValueError("Actual unique ordinary source count provenance does not match IDs")
    if set(ordinary)&excluded:
        raise ValueError("Excluded identifiable pictorial source remains in ordinary training IDs")
    if source_ids is not None and set(map(str, source_ids)) != set(ordinary):
        raise ValueError("Actual source corpus differs from curated ordinary IDs")
    return set(ordinary)


def validate_refit_manifest(manifest, source_pack_sha256, filter_sha256, model_hashes):
    if manifest.get("status") != "REFIT_ON_CURATED_ORDINARY_CORPUS":
        raise ValueError("Saved models have not been certified refitted on the curated ordinary corpus")
    if manifest.get("sourcePackSha256") != source_pack_sha256 or manifest.get("sourceFilterSha256") != filter_sha256:
        raise ValueError("Ordinary model refit certificate is stale for source/filter")
    for name, actual_sha in model_hashes.items():
        if manifest.get("models", {}).get(name, {}).get("sha256") != actual_sha:
            raise ValueError("Missing/stale ordinary refit model binding: "+name)


def preflight(workspace, source_filter_path=None):
    workspace = Path(workspace)
    source_filter_path = Path(source_filter_path or workspace/"data/ordinary_source_filter_v4.json")
    if not source_filter_path.exists():
        raise ValueError("Missing reviewed ordinary source filter; await source curation before proposals")
    source_hash = sha(workspace/"data/source500_native.json")
    manifest = json.loads(source_filter_path.read_text(encoding="utf8"))
    validate_source_filter(manifest, source_hash)
    refit_path = workspace/"models/ordinary_refit_manifest_v4.json"
    if not refit_path.exists():
        raise ValueError("Missing ordinary model refit certificate; historical mixed/pictorial-trained models cannot propose levels")
    model_names = ("macro_assembly.joblib", "guide_anchor_role_motifs.joblib", "composition_pose.joblib")
    model_hashes = {name: sha(workspace/"models"/name) for name in model_names}
    refit = json.loads(refit_path.read_text(encoding="utf8"))
    filter_hash = sha(source_filter_path)
    validate_refit_manifest(refit, source_hash, filter_hash, model_hashes)
    return manifest, {"sourceFilter": filter_hash, "ordinaryRefitManifest": sha(refit_path)}
