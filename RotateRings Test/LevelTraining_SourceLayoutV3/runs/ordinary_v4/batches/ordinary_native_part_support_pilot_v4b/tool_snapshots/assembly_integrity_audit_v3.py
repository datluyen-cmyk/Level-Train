"""Independent source-asset and rigid macro provenance gate.

This checks construction claims, not collisions, solvability or difficulty.
Source IDs and RGB tint may differ at an identical shared socket; native shape,
alpha, visual dimensions, input geometry, movement and guide closure may not.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path


def native_identity(value):
    if isinstance(value, dict):
        return {key: ({"a": child.get("a", 1.)} if key in ("sourceVertexColor", "vertexColor")
                      else native_identity(child)) for key, child in value.items()
                if key not in ("id", "sourceColliderId", "sourceComponentId", "sourceName", "fixtureVisualSource")}
    if isinstance(value, list):
        return [native_identity(child) for child in value]
    return value


def signature(value):
    return json.dumps(value, sort_keys=True, separators=(",", ":"))


def feature_identity(shape):
    return native_identity({key: value for key, value in shape.items() if key != "attachmentId"})


def posed_xy(x, y, rotation, translation):
    c, s = math.cos(math.radians(rotation)), math.sin(math.radians(rotation))
    return c*x-s*y+translation[0], s*x+c*y+translation[1]


def ring_edge(edge, mapping=None):
    mapping = mapping or {}
    return (mapping.get(edge["source"], edge["source"]), mapping.get(edge["target"], edge["target"]),
            tuple(edge["sourceShapeIndices"]), tuple(edge["targetShapeIndices"]),
            edge.get("relationKind", 0), edge.get("activityKind", 0),
            edge.get("sourceAttachmentId", -1), edge.get("targetAttachmentId", -1))


def guide_edge(edge, mapping=None):
    mapping = mapping or {}
    return (mapping.get(edge["guideOwnerId"], edge["guideOwnerId"]),
            mapping.get(edge["lockedOwnerId"], edge["lockedOwnerId"]), tuple(edge["guideShapeIndices"]))


class NativeAssemblyIntegrityAudit:
    def __init__(self, templates, sources, physical_composition_records=None):
        self.templates = templates
        self.sources = {str(level["sourceId"]): level for level in sources}
        self.template_identities = {tid: signature(native_identity(template)) for tid, template in templates.items()}
        self.composition_records = None if physical_composition_records is None else {
            str(sid): records for sid, records in physical_composition_records.items()}
        self.verified_composition_records = {}

    def composition_record_is_native(self, source_id, index, record):
        """Recompute cited static geometry evidence against this audit's sources.

        A saved proposal/model cannot grant its own seam exemption by inventing
        an edge. These remain composition proxies, never gameplay bindings.
        """
        if source_id not in self.verified_composition_records:
            from source_physical_composition_v3 import composition_edges
            self.verified_composition_records[source_id] = composition_edges(self.sources[source_id], self.templates)
        actual = self.verified_composition_records[source_id]
        return 0 <= index < len(actual) and actual[index] == record

    def check(self, level):
        errors = []
        parts = {part["id"]: part for part in level["parts"]}
        if len(parts) != len(level["parts"]):
            errors.append({"kind": "duplicatePartId"})
        represented, proven_origins = set(), set()
        expected_rings, expected_guides = set(), set()
        expected_bombs = []
        imports = level.get("assemblyProvenance", {}).get("tileImports", [])
        for index, imported in enumerate(imports):
            source_id = str(imported["sourceId"])
            source = self.sources.get(source_id)
            if source is None:
                errors.append({"kind": "unknownSourceImport", "importIndex": index})
                continue
            originals = {part["id"]: part for part in source["parts"]}
            mapping = {int(old): new for old, new in imported["partMapping"].items()}
            rotation, translation = imported.get("rotation", 0.), imported.get("translation", [0., 0.])
            if len(set(mapping.values())) != len(mapping):
                errors.append({"kind": "collapsedDistinctSourceRoots", "importIndex": index})
            for old_id, new_id in mapping.items():
                original, part = originals.get(old_id), parts.get(new_id)
                if original is None or part is None:
                    errors.append({"kind": "missingImportRoot", "importIndex": index, "part": new_id})
                    continue
                represented.add(new_id)
                proven_origins.add((new_id, source_id, old_id))
                x, y = posed_xy(original["x"], original["y"], rotation, translation)
                angle_error = abs((part["angle"] - original["angle"] - rotation + 180) % 360 - 180)
                if math.hypot(part["x"]-x, part["y"]-y) > 1e-4 or angle_error > .005:
                    errors.append({"kind": "nonRigidImportedRoot", "importIndex": index, "part": new_id})
                old_template, new_template = original["templateId"], part["templateId"]
                if new_template not in self.templates or self.template_identities.get(old_template) != self.template_identities.get(new_template):
                    errors.append({"kind": "changedNativeTemplate", "importIndex": index, "part": new_id})
                for field in ("inputShapes", "sourceRigidbody"):
                    if native_identity(original.get(field)) != native_identity(part.get(field)):
                        errors.append({"kind": "changedNativePartContract", "field": field, "part": new_id})
                for field in ("kind", "z", "chainLockCount", "collectible", "scoreEligible", "key", "lockKey"):
                    if original.get(field) != part.get(field):
                        errors.append({"kind": "changedNativePartContract", "field": field, "part": new_id})
                # Runtime uses exact source shape indices/roles, while fixture feature IDs are remapped.
                contract = lambda p: [(b["shapeIndex"], b.get("sourceRole")) for b in p.get("sourceColliderBindings", [])]
                if contract(original) != contract(part):
                    errors.append({"kind": "changedShapeBindingContract", "part": new_id})
            for edge in source.get("links", []):
                if edge["source"] in mapping and edge["target"] in mapping:
                    expected_rings.add(ring_edge(edge, mapping))
            for edge in source.get("guideLinks", []):
                endpoints = (edge["guideOwnerId"], edge["lockedOwnerId"])
                present = [owner in mapping for owner in endpoints]
                if any(present) and not all(present):
                    errors.append({"kind": "truncatedNativeGuideAssembly", "importIndex": index, "sourceOwners": endpoints})
                if all(present):
                    expected_guides.add(guide_edge(edge, mapping))
            for original in source.get("bombs", []):
                owner = original["ownerPartId"]
                nearest = owner if owner >= 0 else min(originals, key=lambda oid: math.hypot(
                    originals[oid]["x"] - original["worldPosition"]["x"],
                    originals[oid]["y"] - original["worldPosition"]["y"]))
                if nearest not in mapping:
                    continue
                shape = json.loads(json.dumps(original["physicalShape"]))
                if owner < 0:
                    center = shape["center"]
                    x, y = posed_xy(center["x"], center["y"], rotation, translation)
                    center.update(x=x, y=y)
                expected_bombs.append((mapping[owner] if owner >= 0 else -1, shape))
        feature_imports = level.get("assemblyProvenance", {}).get("featureBoundaryImports", [])
        for import_index, imported in enumerate(feature_imports):
            source_id = str(imported["sourceId"])
            source = self.sources.get(source_id)
            if source is None:
                errors.append({"kind": "unknownFeatureSourceImport", "importIndex": import_index})
                continue
            originals = {part["id"]: part for part in source["parts"]}
            interior = {int(old): new for old, new in imported["interiorPartMapping"].items()}
            mapping = dict(interior)
            shape_maps, witnesses = {}, {}
            rotation, translation = imported["rotation"], imported["translation"]
            for old_id, new_id in interior.items():
                original, part = originals.get(old_id), parts.get(new_id)
                if original is None or part is None:
                    errors.append({"kind": "missingFeatureInteriorRoot", "part": new_id})
                    continue
                represented.add(new_id)
                proven_origins.add((new_id, source_id, old_id))
                x, y = posed_xy(original["x"], original["y"], rotation, translation)
                if math.hypot(part["x"]-x, part["y"]-y) > 1e-4 or abs((part["angle"]-original["angle"]-rotation+180)%360-180) > .005:
                    errors.append({"kind": "nonRigidFeatureInterior", "part": new_id})
                if self.template_identities.get(original["templateId"]) != self.template_identities.get(part["templateId"]):
                    errors.append({"kind": "changedNativeFeatureInteriorTemplate", "part": new_id})
                for field in ("inputShapes", "sourceRigidbody", "kind", "z", "chainLockCount", "collectible", "scoreEligible", "key", "lockKey"):
                    if native_identity(original.get(field)) != native_identity(part.get(field)):
                        errors.append({"kind": "changedNativeFeatureInteriorContract", "field": field, "part": new_id})
                shape_maps[old_id] = {i: i for i in range(len(self.templates[original["templateId"]]["physicalShapes"]))}
            for witness in imported.get("boundaryWitnesses", []):
                old_id, new_id = witness["sourcePartId"], witness["targetPartId"]
                original, part = originals.get(old_id), parts.get(new_id)
                if witness.get("kind") not in ("pin", "compositionBody") or original is None or part is None or old_id in mapping or part["kind"] not in ("ring", "slider"):
                    errors.append({"kind": "unsupportedFeatureBoundary", "sourcePart": old_id})
                    continue
                mapping[old_id] = new_id
                witnesses[old_id] = witness
                x, y = posed_xy(original["x"], original["y"], rotation, translation)
                if original["kind"] != part["kind"] or math.hypot(part["x"]-x, part["y"]-y) > 1e-4 or abs((part["angle"]-original["angle"]-rotation+180)%360-180) > .005 or original["z"] != part["z"]:
                    errors.append({"kind": "nonRigidFeatureBoundary", "part": new_id})
                source_indices, target_indices = witness["sourceShapeIndices"], witness["targetShapeIndices"]
                if not source_indices or len(source_indices) != len(target_indices) or len(set(source_indices)) != len(source_indices) or len(set(target_indices)) != len(target_indices):
                    errors.append({"kind": "invalidBoundaryShapeMapping", "part": new_id})
                    continue
                source_shapes = self.templates[original["templateId"]]["physicalShapes"]
                target_shapes = self.templates[part["templateId"]]["physicalShapes"]
                shape_maps[old_id] = {}
                for si, ti in zip(source_indices, target_indices):
                    if not 0 <= si < len(source_shapes) or not 0 <= ti < len(target_shapes) or feature_identity(source_shapes[si]) != feature_identity(target_shapes[ti]):
                        errors.append({"kind": "changedNativeBoundaryFeature", "part": new_id, "sourceShape": si, "targetShape": ti})
                    else:
                        shape_maps[old_id][si] = ti
                for attachment in {source_shapes[i].get("attachmentId", -1) for i in source_indices if 0 <= i < len(source_shapes)} - {-1}:
                    complete = {i for i, shape in enumerate(source_shapes) if shape.get("attachmentId", -1) == attachment}
                    if not complete.issubset(shape_maps[old_id]):
                        errors.append({"kind": "incompleteNativePinFeature", "part": new_id})
                        continue
                    mapped = {shape_maps[old_id][i] for i in complete}
                    target_attachments = {target_shapes[i].get("attachmentId", -1) for i in mapped}
                    if len(target_attachments) != 1 or -1 in target_attachments:
                        errors.append({"kind": "splitNativePinFeature", "part": new_id})
                    elif mapped != {i for i, shape in enumerate(target_shapes) if shape.get("attachmentId", -1) in target_attachments}:
                        errors.append({"kind": "incompleteTargetPinFeature", "part": new_id})
                source_body = {i for i, shape in enumerate(source_shapes) if shape.get("role") == "body"}
                target_body = {i for i, shape in enumerate(target_shapes) if shape.get("role") == "body"}
                if witness.get("kind") == "compositionBody":
                    if set(source_indices) != source_body or set(target_indices) != target_body or witness.get("relationIndices", []):
                        errors.append({"kind": "invalidCompositionBodyWitness", "part": new_id})
                elif source_body.intersection(source_indices) and (not source_body.issubset(source_indices) or
                        {shape_maps[old_id].get(i) for i in source_body} != target_body):
                    errors.append({"kind": "incompleteBoundaryBody", "part": new_id})
            used_composition_boundaries, seen_composition_pairs = set(), set()
            for pair in imported.get("compositionBoundaryPairs", []):
                interior_id, boundary_id = pair.get("interiorSourcePartId"), pair.get("boundarySourcePartId")
                record_index = pair.get("sourceCompositionRecordIndex")
                records = (self.composition_records or {}).get(source_id, [])
                valid = interior_id in interior and boundary_id in witnesses and isinstance(record_index, int)
                valid = valid and 0 <= record_index < len(records)
                record = records[record_index] if valid else {}
                valid = valid and record.get("strong") is True and record.get("kind") == pair.get("kind")
                valid = valid and record.get("evidenceLabel") == "STATICcompositionproxy"
                valid = valid and {record.get("source"), record.get("target")} == {interior_id, boundary_id}
                valid = valid and self.composition_record_is_native(source_id, record_index, record)
                identity = (interior_id, boundary_id, record_index)
                if not valid or identity in seen_composition_pairs:
                    errors.append({"kind": "unprovenCompositionBoundaryPair", "importIndex": import_index,
                                   "interiorPart": interior_id, "boundaryPart": boundary_id})
                    continue
                seen_composition_pairs.add(identity)
                witness = witnesses[boundary_id]
                original, part = originals[boundary_id], parts[mapping[boundary_id]]
                source_body = {i for i, shape in enumerate(self.templates[original["templateId"]]["physicalShapes"]) if shape.get("role") == "body"}
                target_body = {i for i, shape in enumerate(self.templates[part["templateId"]]["physicalShapes"]) if shape.get("role") == "body"}
                shapes = shape_maps.get(boundary_id, {})
                if not source_body or not source_body.issubset(shapes) or {shapes.get(i) for i in source_body} != target_body:
                    errors.append({"kind": "incompleteCompositionBoundaryBody", "part": mapping[boundary_id]})
                used_composition_boundaries.add(boundary_id)
            for old_id, witness in witnesses.items():
                if witness.get("kind") == "compositionBody" and old_id not in used_composition_boundaries:
                    errors.append({"kind": "unprovenCompositionBodyWitness", "sourcePart": old_id})
            if len(set(mapping.values())) != len(mapping):
                errors.append({"kind": "collapsedDistinctFeatureRoots", "importIndex": import_index})
            used_relations = {old_id: set() for old_id in witnesses}
            for edge_index, edge in enumerate(source.get("links", [])):
                endpoints = (edge["source"], edge["target"])
                if not any(owner in interior for owner in endpoints) or not all(owner in mapping for owner in endpoints):
                    continue
                remapped = dict(edge)
                supported = True
                for side, old_id in zip(("source", "target"), endpoints):
                    required = edge[side+"ShapeIndices"]
                    if old_id in witnesses and edge_index not in witnesses[old_id].get("relationIndices", []):
                        supported = False
                    shapes = shape_maps.get(old_id, {})
                    if not all(index in shapes for index in required):
                        supported = False
                    if not supported:
                        errors.append({"kind": "incompleteNativeBoundaryRelation", "edgeIndex": edge_index, "part": mapping[old_id]})
                        continue
                    remapped[side] = mapping[old_id]
                    remapped[side+"ShapeIndices"] = [shapes[index] for index in required]
                    actual_shapes = self.templates[parts[mapping[old_id]]["templateId"]]["physicalShapes"]
                    attachments = [actual_shapes[index].get("attachmentId", -1) for index in remapped[side+"ShapeIndices"] if actual_shapes[index]["role"] == "head_trigger"]
                    remapped[side+"AttachmentId"] = attachments[0] if attachments else -1
                    if old_id in witnesses:
                        used_relations[old_id].add(edge_index)
                if supported:
                    expected_rings.add(ring_edge(remapped))
            for old_id, witness in witnesses.items():
                if set(witness.get("relationIndices", [])) != used_relations[old_id]:
                    errors.append({"kind": "unprovenBoundaryRelationIndex", "sourcePart": old_id})
            for edge in source.get("guideLinks", []):
                endpoints = (edge["guideOwnerId"], edge["lockedOwnerId"])
                if not any(owner in interior for owner in endpoints):
                    continue
                if not all(owner in interior for owner in endpoints):
                    errors.append({"kind": "truncatedFeatureNativeGuideAssembly", "importIndex": import_index})
                else:
                    expected_guides.add(guide_edge(edge, interior))
            for bomb in source.get("bombs", []):
                owner = bomb["ownerPartId"]
                nearest = owner if owner >= 0 else min(originals, key=lambda oid: math.hypot(originals[oid]["x"]-bomb["worldPosition"]["x"], originals[oid]["y"]-bomb["worldPosition"]["y"]))
                if nearest in interior:
                    shape = json.loads(json.dumps(bomb["physicalShape"]))
                    if owner < 0:
                        x, y = posed_xy(shape["center"]["x"], shape["center"]["y"], rotation, translation)
                        shape["center"].update(x=x, y=y)
                    expected_bombs.append((interior[owner] if owner >= 0 else -1, shape))
        for part in parts.values():
            provenance = part.get("atomicProvenance", {})
            origin = (part["id"], str(provenance.get("sourceId")), provenance.get("partId"))
            if origin not in proven_origins:
                errors.append({"kind": "unprovenAtomicRoot", "part": part["id"]})
            if part["id"] not in represented:
                errors.append({"kind": "rootOutsideNativeImports", "part": part["id"]})
        actual_rings = {ring_edge(edge) for edge in level.get("links", [])}
        actual_guides = {guide_edge(edge) for edge in level.get("guideLinks", [])}
        for name, expected, actual in [("ring", expected_rings, actual_rings), ("guide", expected_guides, actual_guides)]:
            for edge in expected - actual:
                errors.append({"kind": "missingNativeRelation", "relation": name, "edge": edge})
            for edge in actual - expected:
                errors.append({"kind": "fabricatedRelation", "relation": name, "edge": edge})
        physical_ids = [binding["sourceColliderId"] for part in parts.values() for binding in part.get("sourceColliderBindings", [])]
        physical_ids += [shape["sourceColliderId"] for part in parts.values() for shape in part.get("inputShapes", [])]
        physical_ids += [bomb["sourceColliderId"] for bomb in level.get("bombs", [])]
        if len(set(physical_ids)) != len(physical_ids):
            errors.append({"kind": "duplicateColliderIdentity"})
        def same_bomb_shape(expected, actual):
            center_equal = math.hypot(expected["center"]["x"]-actual["center"]["x"],
                                      expected["center"]["y"]-actual["center"]["y"]) < 1e-4
            return center_equal and {k: v for k, v in expected.items() if k != "center"} == {
                k: v for k, v in actual.items() if k != "center"}
        for owner, shape in expected_bombs:
            if not any(bomb["ownerPartId"] == owner and same_bomb_shape(shape, bomb["physicalShape"])
                       for bomb in level.get("bombs", [])):
                errors.append({"kind": "missingNativeBomb", "owner": owner})
        for bomb in level.get("bombs", []):
            origin = bomb.get("atomicProvenance", {})
            source = self.sources.get(str(origin.get("sourceId")), {})
            original = next((b for b in source.get("bombs", []) if b["sourceColliderId"] == origin.get("sourceColliderId")), None)
            if original is None:
                errors.append({"kind": "unprovenNativeBomb", "collider": bomb["sourceColliderId"]})
                continue
            owner = original["ownerPartId"]
            bomb_imports = imports + [{**i, "partMapping": i["interiorPartMapping"]} for i in feature_imports]
            candidates = [i for i in bomb_imports if str(i["sourceId"]) == str(source["sourceId"]) and
                          (owner < 0 or {int(k): v for k, v in i["partMapping"].items()}.get(owner) == bomb["ownerPartId"])]
            valid = False
            for imported in candidates:
                expected = json.loads(json.dumps(original["physicalShape"]))
                if owner < 0:
                    center = expected["center"]
                    x, y = posed_xy(center["x"], center["y"], imported["rotation"], imported["translation"])
                    center.update(x=x, y=y)
                actual = bomb["physicalShape"]
                if owner >= 0:
                    valid |= expected == actual
                else:
                    valid |= same_bomb_shape(expected, actual)
            if not valid:
                errors.append({"kind": "changedNativeBombGeometry", "collider": bomb["sourceColliderId"]})
        return {"nativeAssemblyIntegrityPassed": not errors, "nativeImportedRoots": len(represented),
                "nativeImports": len(imports), "errors": errors,
                "scope": "Source atomic asset, rigid macro pose, exact relation/guide closure and optional static composition-body certificate; no collision, active lock, solution, novelty or difficulty claim."}


def main():
    parser = argparse.ArgumentParser()
    for name in ("pack", "sources", "templates", "report"):
        parser.add_argument("--"+name, type=Path, required=True)
    parser.add_argument("--physical-composition", type=Path)
    args = parser.parse_args()
    load = lambda path: json.loads(path.read_text(encoding="utf8"))
    composition = {str(row["sourceId"]): row["relations"] for row in load(args.physical_composition)["levels"]} if args.physical_composition else None
    audit = NativeAssemblyIntegrityAudit({t["id"]: t for t in load(args.templates)["templates"]}, load(args.sources)["levels"], composition)
    rows = [{"levelId": level["id"], **audit.check(level)} for level in load(args.pack)["levels"]]
    report = {"passed": all(row["nativeAssemblyIntegrityPassed"] for row in rows), "levels": rows,
              "hashes": {str(path): hashlib.sha256(path.read_bytes()).hexdigest() for path in
                         [args.pack, args.sources, args.templates, Path(__file__)] + ([args.physical_composition] if args.physical_composition else [])}}
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps(report, indent=2), encoding="utf8")
    print(json.dumps({"passed": report["passed"], "levels": len(rows), "errors": sum(len(row["errors"]) for row in rows)}))


if __name__ == "__main__":
    main()
