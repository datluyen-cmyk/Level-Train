"""Check original atomic assets and saved-model witnesses for learned placements.

This deliberately does not grant contact exemptions or certify a level playable.
An actual native replay, collision validation and composition review remain gates.
"""
import argparse
import copy
import hashlib
import json
import math
from pathlib import Path

import joblib
import networkx as nx
import numpy as np

from assembly_integrity_audit_v3 import NativeAssemblyIntegrityAudit, ring_edge, posed_xy, feature_identity
from layout_learning_v3 import relation_rows


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def relative_pose(a, b, templates):
    unit = max(templates[a["templateId"]].get("width", 0), templates[b["templateId"]].get("width", 0), 1.)
    r = math.radians(a["angle"])
    x, y = b["x"]-a["x"], b["y"]-a["y"]
    angle = math.radians(b["angle"]-a["angle"])
    return np.asarray([(math.cos(r)*x+math.sin(r)*y)/unit,
                       (-math.sin(r)*x+math.cos(r)*y)/unit, math.cos(angle), math.sin(angle)])


class NativeLearnedPlacementAudit:
    def __init__(self, sources, templates, model_path, source_pack_path):
        self.sources = {str(level["sourceId"]): level for level in sources}
        self.templates = templates
        self.asset_audit = NativeAssemblyIntegrityAudit(templates, sources)
        self.bundle = joblib.load(model_path)
        self.model_hash = sha(model_path)
        self.source_hash = sha(source_pack_path)
        self.vocabulary = set()
        for level in sources:
            ps = {p["id"]: p for p in level["parts"]}
            for edge in level["links"]:
                self.vocabulary.add(self.edge_type(edge, ps))

    def edge_type(self, edge, parts):
        a, b = parts[edge["source"]], parts[edge["target"]]
        return (a["kind"], b["kind"], edge.get("relationKind", 0), edge.get("activityKind", 0),
                tuple(self.templates[a["templateId"]]["physicalShapes"][i]["role"] for i in edge["sourceShapeIndices"]),
                tuple(self.templates[b["templateId"]]["physicalShapes"][i]["role"] for i in edge["targetShapeIndices"]))

    def check(self, level):
        errors = []
        virtual = copy.deepcopy(level)
        provenance = virtual["assemblyProvenance"]
        parts = {part["id"]: part for part in virtual["parts"]}
        scaffold_roots = set()
        for scaffold in provenance.get("bodyScaffoldSubstitutions", []):
            source = self.sources.get(str(scaffold.get("sourceId")), {})
            old, new = scaffold.get("sourcePartId"), scaffold.get("actualPartId")
            original = next((p for p in source.get("parts", []) if p["id"] == old), None)
            actual = parts.get(new)
            if original is None or actual is None or new in scaffold_roots:
                errors.append({"kind": "invalidBodyScaffoldRoot", "part": new})
                continue
            scaffold_roots.add(new)
            if original["kind"] != actual["kind"] or original.get("z") != actual.get("z") or math.hypot(original["x"]-actual["x"], original["y"]-actual["y"]) > 1e-4 or abs((original["angle"]-actual["angle"]+180)%360-180) > .005:
                errors.append({"kind": "nonNativeBodyScaffoldPose", "part": new})
            source_shapes = self.templates[original["templateId"]]["physicalShapes"]
            target_shapes = self.templates[actual["templateId"]]["physicalShapes"]
            source_indices, target_indices = scaffold.get("sourceBodyShapeIndices", []), scaffold.get("targetBodyShapeIndices", [])
            if not source_indices or set(source_indices) != {i for i,s in enumerate(source_shapes) if s.get("role") == "body"} or set(target_indices) != {i for i,s in enumerate(target_shapes) if s.get("role") == "body"} or len(source_indices) != len(target_indices):
                errors.append({"kind": "incompleteNativeBodyScaffold", "part": new})
            for si, ti in zip(source_indices, target_indices):
                if not 0 <= si < len(source_shapes) or not 0 <= ti < len(target_shapes) or feature_identity(source_shapes[si]) != feature_identity(target_shapes[ti]):
                    errors.append({"kind": "changedNativeBodyScaffold", "part": new})
        moved = set()
        origin_mapping = {}
        origin_shape_mapping = {}
        for imported in provenance.get("retargetedAssetImports", []):
            source_id = str(imported["sourceId"])
            source = self.sources.get(source_id)
            if source is None:
                errors.append({"kind": "unknownRetargetedAssetSource"})
                continue
            originals = {p["id"]: p for p in source["parts"]}
            mapping = {int(old): new for old, new in imported["partMapping"].items()}
            graph = nx.Graph()
            graph.add_nodes_from(mapping)
            for edge in source["guideLinks"]:
                if edge["guideOwnerId"] in mapping and edge["lockedOwnerId"] in mapping:
                    graph.add_edge(edge["guideOwnerId"], edge["lockedOwnerId"])
            for group in nx.connected_components(graph):
                old = min(group)
                if old not in originals or mapping[old] not in parts:
                    errors.append({"kind": "missingRetargetedRoot"})
                    continue
                a, b = originals[old], parts[mapping[old]]
                rotation = b["angle"]-a["angle"]
                x, y = posed_xy(a["x"], a["y"], rotation, [0., 0.])
                translation = [b["x"]-x, b["y"]-y]
                provenance.setdefault("tileImports", []).append({"sourceId": source_id,
                    "partMapping": {i: mapping[i] for i in group}, "rotation": rotation, "translation": translation})
            moved.update(mapping.values())
            for old, new in mapping.items():
                origin_mapping[(source_id, old)] = new
                if old in originals:
                    origin_shape_mapping[(source_id, old)] = {i: i for i in range(len(self.templates[originals[old]["templateId"]]["physicalShapes"]))}
        if not moved:
            errors.append({"kind": "missingRetargetedNativeAssets"})
        for imported in provenance.get("learnedBoundaryFeatureWitnesses", []):
            source_id = str(imported["sourceId"])
            originals = {p["id"]: p for p in self.sources.get(source_id, {}).get("parts", [])}
            interior = {int(old): new for old, new in imported.get("interiorPartMapping", {}).items()}
            for edge in self.sources.get(source_id, {}).get("guideLinks", []):
                if (edge["guideOwnerId"] in interior) != (edge["lockedOwnerId"] in interior):
                    errors.append({"kind": "truncatedLearnedBoundaryGuideAssembly", "sourceId": source_id})
            for witness in imported.get("boundaryWitnesses", []):
                old, new = witness["sourcePartId"], witness["targetPartId"]
                original, actual = originals.get(old), parts.get(new)
                if original is None or actual is None:
                    errors.append({"kind": "unknownLearnedBoundaryRoot"})
                    continue
                if original["kind"] != actual["kind"] or original.get("z") != actual.get("z"):
                    errors.append({"kind": "changedLearnedBoundaryContract", "part": new})
                source_shapes = self.templates[original["templateId"]]["physicalShapes"]
                target_shapes = self.templates[actual["templateId"]]["physicalShapes"]
                source_body = {i for i, shape in enumerate(source_shapes) if shape.get("role") == "body"}
                target_body = {i for i, shape in enumerate(target_shapes) if shape.get("role") == "body"}
                body_source, body_target = witness.get("sourceBodyShapeIndices", []), witness.get("targetBodyShapeIndices", [])
                if not source_body or set(body_source) != source_body or set(body_target) != target_body or len(body_source) != len(body_target):
                    errors.append({"kind": "incompleteLearnedBoundaryBody", "part": new})
                source_indices, target_indices = witness.get("sourceShapeIndices", []), witness.get("targetShapeIndices", [])
                maps = {}
                for sis, tis in ((source_indices, target_indices), (body_source, body_target)):
                    if len(sis) != len(tis) or len(set(sis)) != len(sis) or len(set(tis)) != len(tis):
                        errors.append({"kind": "invalidLearnedBoundaryFeatureMap", "part": new})
                    for si, ti in zip(sis, tis):
                        if not 0 <= si < len(source_shapes) or not 0 <= ti < len(target_shapes) or feature_identity(source_shapes[si]) != feature_identity(target_shapes[ti]):
                            errors.append({"kind": "changedLearnedBoundaryFeature", "part": new, "sourceShape": si, "targetShape": ti})
                            continue
                        if si in maps and maps[si] != ti:
                            errors.append({"kind": "conflictingLearnedBoundaryFeatureMap", "part": new})
                        maps[si] = ti
                previous = origin_mapping.get((source_id, old))
                if previous is not None and previous != new:
                    errors.append({"kind": "conflictingLearnedBoundaryRootMap", "part": new})
                origin_mapping[(source_id, old)] = new
                origin_shape_mapping[(source_id, old)] = maps
        native_edges = set()
        for imported in provenance.get("tileImports", []):
            mapping = {int(old): new for old, new in imported["partMapping"].items()}
            source = self.sources.get(str(imported["sourceId"]), {})
            native_edges.update(ring_edge(edge, mapping) for edge in source.get("links", [])
                                if edge["source"] in mapping and edge["target"] in mapping)
        # Asset verification sees only relations claimed rigid and exact. New
        # learned relations receive their own checks below, never exemptions.
        virtual["links"] = [edge for edge in virtual["links"] if ring_edge(edge) in native_edges]
        provenance.pop("featureBoundaryImports", None)
        asset = self.asset_audit.check(virtual)
        errors.extend({"kind": "nativeAssetContract", "detail": error} for error in asset["errors"])
        certificate = level["assemblyProvenance"].get("learnedPlacementCertificate", {})
        if certificate.get("sourcePackSha256") != self.source_hash or certificate.get("localPoseModelSha256") != self.model_hash:
            errors.append({"kind": "learnedModelOrCorpusHashMismatch"})
        records = self.bundle.get("relationRecords", [])
        relation_contexts = {(row["sourcePart"], row["targetPart"], row["relation"]): row["context"]
                             for row in relation_rows(level, self.templates)}
        actual_edges = {ring_edge(edge): edge for edge in level["links"]}
        witnessed = set()
        measured = []
        reported = certificate.get("relationWitnesses", [])
        for claim in level["assemblyProvenance"].get("learnedRelationPlacements", []):
            source_id = str(claim["sourceRelationSourceId"])
            source = self.sources.get(source_id, {})
            edge_index = claim["sourceRelationIndex"]
            if not 0 <= edge_index < len(source.get("links", [])):
                errors.append({"kind": "unknownSourcePatternRelation"})
                continue
            original = source["links"][edge_index]
            a, b = claim["actualSource"], claim["actualTarget"]
            if (origin_mapping.get((source_id, original["source"])) != a or
                origin_mapping.get((source_id, original["target"])) != b):
                errors.append({"kind": "unprovenSourcePatternTopology", "owners": [a, b]})
            matches = [(key, edge) for key, edge in actual_edges.items() if edge["source"] == a and edge["target"] == b and
                       edge["sourceShapeIndices"] == claim["actualSourceShapeIndices"] and edge["targetShapeIndices"] == claim["actualTargetShapeIndices"]]
            if len(matches) != 1 or a not in parts or b not in parts:
                errors.append({"kind": "missingOrAmbiguousLearnedRelation", "owners": [a, b]})
                continue
            key, actual = matches[0]
            witnessed.add(key)
            for side in ("source", "target"):
                shape_map = origin_shape_mapping.get((source_id, original[side]), {})
                expected = [shape_map.get(index) for index in original[side+"ShapeIndices"]]
                if expected != actual[side+"ShapeIndices"]:
                    errors.append({"kind": "unprovenLearnedPatternFeatureMapping", "owners": [a, b], "side": side})
            if any(original.get(field, 0) != actual.get(field, 0) for field in ("relationKind", "activityKind")):
                errors.append({"kind": "changedLearnedPatternRelationContract", "owners": [a, b]})
            if self.edge_type(actual, parts) not in self.vocabulary:
                errors.append({"kind": "unsupportedNativeRelationType", "owners": [a, b]})
            source_shapes = self.templates[parts[a]["templateId"]]["physicalShapes"]
            attachment_ids = {source_shapes[i].get("attachmentId", -1) for i in actual["sourceShapeIndices"]}
            if len(attachment_ids) != 1 or -1 in attachment_ids or set(actual["sourceShapeIndices"]) != {
                i for i, shape in enumerate(source_shapes) if shape.get("attachmentId", -1) in attachment_ids}:
                errors.append({"kind": "incompleteLearnedNativePinFeature", "owners": [a, b]})
            record_index = claim["learnedRecordIndex"]
            if claim.get("modelSha256") != self.model_hash or not 0 <= record_index < len(records):
                errors.append({"kind": "unboundLearnedRelationRecord", "owners": [a, b]})
                continue
            record = records[record_index]
            if record["relation"] != "pin":
                errors.append({"kind": "wrongLearnedRelationRecordType", "owners": [a, b]})
            context = relation_contexts.get((a, b, "pin"))
            if context is None or record_index not in self.bundle["model"].neighbors([context])[1][0]:
                errors.append({"kind": "learnedRecordOutsideConditionalSupport", "owners": [a, b]})
            actual_pose = relative_pose(parts[a], parts[b], self.templates)
            learned_pose = np.asarray(record["pose"])
            residual = float(np.linalg.norm(actual_pose-learned_pose))
            row = next((r for r in reported if r["sourcePart"] == a and r["targetPart"] == b and r["relation"] == "pin"), None)
            if (row is None or row["learnedRelationRecordIndex"] != record_index or
                not np.allclose(row["learnedPose"], learned_pose, atol=1e-8, rtol=0) or
                not np.allclose(row["actualPose"], actual_pose, atol=1e-8, rtol=0) or
                abs(row["poseResidualL2"]-residual) > 1e-8):
                errors.append({"kind": "incorrectReportedLearnedPoseResidual", "owners": [a, b]})
            measured.append({"owners": [a, b], "sourcePatternId": source_id,
                             "learnedRecordIndex": record_index, "measuredPoseResidualL2": residual})
        for key in actual_edges.keys()-native_edges-witnessed:
            errors.append({"kind": "unwitnessedLearnedRelation", "edge": key})
        return {"nativeLearnedPlacementIntegrityPassed": not errors, "errors": errors,
                "retargetedNativeRoots": len(moved), "learnedRelationMeasurements": measured,
                "scope": "Original whole assets, rigid native guide assemblies, source-pattern topology and recomputed saved-model pose witnesses; residual is measured, with no quality, physics, solvability or difficulty acceptance."}


def main():
    parser = argparse.ArgumentParser()
    for name in ("pack", "sources", "templates", "model", "report"):
        parser.add_argument("--"+name, type=Path, required=True)
    args = parser.parse_args()
    load = lambda path: json.loads(path.read_text(encoding="utf8"))
    audit = NativeLearnedPlacementAudit(load(args.sources)["levels"], {t["id"]: t for t in load(args.templates)["templates"]}, args.model, args.sources)
    rows = [{"levelId": level["id"], **audit.check(level)} for level in load(args.pack)["levels"]]
    report = {"passed": all(row["nativeLearnedPlacementIntegrityPassed"] for row in rows), "levels": rows,
              "hashes": {str(path): sha(path) for path in [args.pack, args.sources, args.templates, args.model, Path(__file__)]}}
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps(report, indent=2), encoding="utf8")
    print(json.dumps({"passed": report["passed"], "levels": len(rows), "errors": sum(len(row["errors"]) for row in rows)}))


if __name__ == "__main__":
    main()
