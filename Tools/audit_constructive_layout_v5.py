"""Independent audit for the from-empty native-motif v5 pilot.

This checks provenance, exact native imports, the pilot's conservative motif
closure rule, raw native seams, and whole-board source equivalence. It does not
claim that closure is a game rule or that a candidate is solvable/difficult.
"""
from pathlib import Path
import argparse
import hashlib
import json

import joblib
import numpy as np

from assembly_integrity_audit_v3 import NativeAssemblyIntegrityAudit
from assembly_contact_audit_v3 import NativeGeometrySeamAudit
from layout_novelty_semantic_v3 import canonical_fingerprint
from ordinary_source_filter_guard_v4 import validate_source_filter, validate_refit_manifest, sha
from macro_assembly_v3 import pose_matrix, socket_key
from constructive_layout_v5 import closed_library
from organization_conditioned_v5 import weights as progression_weights
from joint_native_motif_v5 import pair_query, triple_query


def read(path):
    return json.loads(Path(path).read_text(encoding="utf8"))


def native_closure(source, initial):
    """Compute the constructor's outgoing-link and touched-guide closure."""
    result = set(map(int, initial))
    while True:
        before = set(result)
        for edge in source.get("links", []):
            if edge["source"] in result:
                result.add(edge["target"])
        for edge in source.get("guideLinks", []):
            if edge["guideOwnerId"] in result or edge["lockedOwnerId"] in result:
                result.update((edge["guideOwnerId"], edge["lockedOwnerId"]))
        if before == result:
            return result


def check_model_bindings(workspace, organization_model_name="organization_v5.joblib"):
    data, models = workspace / "data", workspace / "models"
    pack_path = data / "source500_native.json"
    templates_path = data / "source_templates.json"
    filter_path = data / "ordinary_source_filter_v4.json"
    filter_doc = read(filter_path)
    pack_hash = sha(pack_path)
    source_pack = read(pack_path)
    source_ids = [str(level["sourceId"]) for level in source_pack["levels"]]
    allowed = validate_source_filter(filter_doc, pack_hash, source_ids)
    expected_count = int(filter_doc.get("actualUniqueOrdinaryCount", len(allowed)))
    if len(allowed) != expected_count:
        raise ValueError(f"Curated ordinary source count mismatch: {len(allowed)} != {expected_count}")
    excluded = set(map(str, filter_doc.get("excludedSourceIds", [])))
    if allowed & excluded:
        raise ValueError("Curated ordinary sources overlap explicit exclusions")

    # Existing refit manifest certifies the supporting proposal models. The
    # v5 organization model has its own source list, input bindings and report.
    refit = read(models / "ordinary_refit_manifest_v4.json")
    supporting = ("macro_assembly.joblib", "guide_anchor_role_motifs.joblib",
                  "composition_pose.joblib")
    model_hashes = {name: sha(models / name) for name in supporting}
    validate_refit_manifest(refit, pack_hash, sha(filter_path), model_hashes)
    if set(map(str, refit.get("ordinarySourceIds", []))) != allowed:
        raise ValueError("Supporting-model refit manifest does not bind exactly the ordinary corpus")

    org_path = models / organization_model_name
    org_report_path = models / (organization_model_name.removesuffix(".joblib") + "_evaluation.json")
    org = joblib.load(org_path)
    macro = joblib.load(models / "macro_assembly.joblib")
    continuation_path = models / "macro_pattern_v5.joblib"
    continuation = joblib.load(continuation_path) if continuation_path.exists() else None
    joint_path = models / "joint_native_motif_v5.joblib"
    joint = joblib.load(joint_path) if joint_path.exists() else None
    org_report = read(org_report_path)
    org_ids = set(map(str, getattr(org, "source_ids", [])))
    if org_ids != allowed:
        raise ValueError("Organization model is not fit on exactly the curated ordinary corpus")
    if getattr(org, "hashes", {}).get("sourcePack") != pack_hash:
        raise ValueError("Organization model source-pack binding is stale")
    template_hash = sha(templates_path)
    if getattr(org, "hashes", {}).get("templates") != template_hash:
        raise ValueError("Organization model template binding is stale")
    if org_report.get("sourceCount") != expected_count or set(map(str, org_report.get("sourceIds", []))) != allowed:
        raise ValueError("Organization model evaluation does not document all eligible originals")
    report_model_hash = org_report.get("hashes", {}).get("model")
    if report_model_hash != sha(org_path):
        raise ValueError("Organization evaluation report is stale for saved model")
    sharp_bindings = {}
    if hasattr(org, "sharp_fields"):
        cache_path = models / "organization_v5_native_fields.joblib"
        sharp_hashes = getattr(org, "sharp_mode_hashes", {})
        if sharp_hashes.get("baseModel") != sha(models/"organization_v5.joblib"):
            raise ValueError("Sharp organization prior is stale for base organization model")
        if sharp_hashes.get("nativeFieldCache") != sha(cache_path):
            raise ValueError("Sharp organization prior is stale for native field cache")
        if org_report.get("hashes", {}).get("nativeFieldCache") != sha(cache_path):
            raise ValueError("Sharp organization evaluation is stale for native field cache")
        sharp_bindings = {"sharpBaseModelSha256": sharp_hashes["baseModel"],
                          "sharpNativeFieldCacheSha256": sharp_hashes["nativeFieldCache"]}
    if hasattr(org, "training_positions"):
        conditioning = getattr(org, "conditioning_hashes", {})
        if conditioning.get("sharpModel") != sha(models/"organization_sharp_v5.joblib"):
            raise ValueError("Conditioned organization model is stale for sharp field prior")
        if org_report.get("hashes", {}).get("model") != sha(org_path):
            raise ValueError("Conditioned organization evaluation is stale for saved model")
        sharp_bindings.update({"conditionedSharpModelSha256": conditioning["sharpModel"],
                               "conditionedFamilySplitsSha256": conditioning["familySplits"]})
    local_repairs = set(map(str, org_report.get("localRepairSourceIds", [])))
    if local_repairs or any("local-repair" in sid.lower() for sid in org_ids):
        raise ValueError("Organization model includes local-repair candidates")
    if continuation is not None:
        records = continuation.get("records", [])
        continuation_sources = {str(row["sourceId"]) for row in records}
        if not continuation_sources <= allowed or continuation.get("sourceCount") != expected_count:
            raise ValueError("Native continuation model source support is not the curated ordinary corpus")
    if joint is not None:
        if joint.get("hashes", {}).get("sourcePack") != pack_hash or joint.get("hashes", {}).get("macroModel") != model_hashes["macro_assembly.joblib"]:
            raise ValueError("Joint native motif model is stale for source or macro corpus")
        if any(str(record["sourceId"]) not in allowed
               for estimator in joint.get("models", {}).values() for record in estimator.records):
            raise ValueError("Joint native motif model contains ineligible source influence")
    return source_pack["levels"], read(templates_path), allowed, excluded, org, macro, continuation, joint, {
        "ordinarySourceCount": expected_count,
        "sourcePackSha256": pack_hash, "sourceFilterSha256": sha(filter_path),
        "organizationModelName": organization_model_name,
        "organizationModelSha256": sha(org_path), "organizationEvaluationSha256": sha(org_report_path),
        "continuationModelSha256": sha(continuation_path) if continuation is not None else None,
        "jointNativeModelSha256": sha(joint_path) if joint is not None else None,
        **sharp_bindings,
        "supportingModelHashes": model_hashes,
    }


def close_enough(actual, expected, atol=2e-5):
    try:
        return bool(np.allclose(np.asarray(actual, dtype=float), np.asarray(expected, dtype=float),
                                rtol=1e-7, atol=atol))
    except (TypeError, ValueError):
        return False


def frame_matches(saved, reconstructed):
    if not isinstance(saved, dict) or not isinstance(reconstructed, dict):
        return False
    for key in ("tileIndex", "center", "heading", "width", "prototypeDescriptor"):
        if key not in saved or not close_enough(saved[key], reconstructed.get(key)):
            return False
    if "parentClusterIndex" in saved and saved["parentClusterIndex"] != reconstructed.get("parentClusterIndex"):
        return False
    return True


def audit_organization_target(target, position, organization, allowed):
    """Recompute the saved tree/leaf contributors and all sampled target fields."""
    errors = []
    conditioned = hasattr(organization, "training_positions")
    try:
        if conditioned:
            members = np.arange(len(organization.source_ids))
            ids = list(map(str, organization.source_ids))
            if list(map(str, target.get("latentSourceRecords", []))) != ids:
                errors.append({"kind": "organizationProgressionSupportMismatch"})
            if not set(ids) <= allowed:
                errors.append({"kind": "organizationProgressionContainsIneligibleSource"})
            values = None
            components = organization.pca.n_components_
            decoded = None
        else:
            tree_index = int(target["treeIndex"])
            query = np.array([[float(position) / organization.position_scale]])
            tree = organization.forest.estimators_[tree_index]
            leaf_id = int(tree.apply(query)[0])
            members = np.flatnonzero(organization.leaves[:, tree_index] == leaf_id)
            ids = [str(organization.source_ids[i]) for i in members]
            if int(target["leafId"]) != leaf_id:
                errors.append({"kind": "organizationLeafMismatch", "expected": leaf_id, "actual": target.get("leafId")})
            if list(map(str, target.get("latentSourceRecords", []))) != ids:
                errors.append({"kind": "organizationContributorMismatch"})
            if not ids or not set(ids) <= allowed:
                errors.append({"kind": "organizationLeafContainsIneligibleSource"})
            predicted = tree.predict(query)
            values = organization.scaler.inverse_transform(predicted)
            components = organization.pca.n_components_
            decoded = np.maximum(0., organization.pca.inverse_transform(values[:, :components]))
        channels = int(np.asarray(target["field"]).shape[0])
        field_size = int(np.asarray(target["field"]).size)
        expected = {"position": int(position)}
        if not conditioned:
            maps = decoded[0, :field_size].reshape(np.asarray(target["field"]).shape).copy()
            for first, last in ((0, 4), (4, 8)):
                maps[first:last] /= max(float(maps[first:last].sum()), 1e-12)
            orientation = decoded[0, field_size:].reshape(np.asarray(target["orientation"]).shape) / .2
            profile = values[0, components:]
            count = max(1., float(np.expm1(profile[0])))
            context = profile[2:].copy()
            context[0] = np.log1p(count)
            context[-1] = float(position) / 500.
            expected.update(field=maps, orientation=orientation, heading=0., span=float(np.exp(profile[1])),
                targetParts=count, context=context,
                predictedPartCountStd=float(np.std(np.expm1(organization.profiles[members, 0]))),
                partCountDistribution=np.expm1(organization.profiles[members, 0]).tolist())
        sharp = hasattr(organization, "sharp_fields")
        if conditioned:
            probability = progression_weights(position, organization.training_positions,
                                              organization.progression_bandwidth)
            record_index = int(target.get("organizationFieldRecordIndex", -1))
            if not 0 <= record_index < len(organization.source_ids):
                errors.append({"kind": "organizationEmpiricalFieldRecordOutOfRange"})
                record_index = 0
            source_id = str(target.get("organizationFieldSourceId"))
            if source_id != str(organization.source_ids[record_index]):
                errors.append({"kind": "organizationEmpiricalFieldSourceMismatch"})
            if not close_enough(target.get("partCountWeights"), probability, atol=1e-8):
                errors.append({"kind": "organizationProgressionWeightsMismatch"})
            if not close_enough(target.get("sampledFieldProbability"), probability[record_index], atol=1e-8):
                errors.append({"kind": "organizationProgressionProbabilityMismatch"})
            if not close_enough(target.get("progressionKernelBandwidth"), organization.progression_bandwidth):
                errors.append({"kind": "organizationProgressionBandwidthMismatch"})
            if list(map(str, target.get("latentSourceRecords", []))) != list(map(str, organization.source_ids)):
                errors.append({"kind": "organizationProgressionSupportMismatch"})
            if source_id not in allowed or (not conditioned and record_index not in set(map(int, members))):
                errors.append({"kind": "organizationEmpiricalFieldOutsideEligibleLeafSupport"})
            if target.get("fieldTargetIsSourceBoard") is not False or target.get("sourcePartsOrCoordinatesReturned") is not False:
                errors.append({"kind": "organizationEmpiricalFieldNotSeparatedFromBoardGeometry"})
            if any(key in target for key in ("parts", "sourceCoordinates", "sourcePartPoses", "sourceLinks")):
                errors.append({"kind": "organizationEmpiricalFieldLeaksBoardGeometry"})
            source_vector = np.asarray(organization.sharp_fields[record_index], float)
            maps = source_vector[:field_size].reshape(np.asarray(target["field"]).shape).copy()
            for first, last in ((0, 4), (4, 8)):
                maps[first:last] /= max(float(maps[first:last].sum()), 1e-12)
            orientation = source_vector[field_size:].reshape(np.asarray(target["orientation"]).shape) / .2
            profile = organization.profiles[record_index]
            context = profile[2:].copy()
            context[-1] = float(position)/500.
            counts = np.expm1(organization.profiles[:, 0])
            count_mean = float(probability @ counts)
            expected.update(field=maps, orientation=orientation, span=float(np.exp(profile[1])),
                targetParts=float(np.expm1(profile[0])), context=context,
                nativeWidthUnit=float(organization.width_units[record_index]),
                predictedPartCountStd=float(np.sqrt(probability @ ((counts-count_mean)**2))),
                partCountDistribution=counts.tolist(), partCountWeights=probability.tolist(),
                sampledFieldProbability=float(probability[record_index]),
                progressionKernelBandwidth=float(organization.progression_bandwidth),
                organizationFieldRecordIndex=record_index, organizationFieldSourceId=source_id,
                fieldTargetIsSourceBoard=False, sourcePartsOrCoordinatesReturned=False)
        elif sharp:
            source_id = str(target.get("organizationFieldSourceId"))
            source_indices = [i for i, sid in enumerate(organization.source_ids) if str(sid) == source_id]
            record_index = int(target.get("organizationFieldRecordIndex", -1))
            if len(source_indices) != 1 or record_index != source_indices[0]:
                errors.append({"kind": "organizationEmpiricalFieldSourceMismatch"})
            if record_index not in set(map(int, members)):
                errors.append({"kind": "organizationEmpiricalFieldOutsideLeafSupport"})
            if source_id not in allowed:
                errors.append({"kind": "organizationEmpiricalFieldIneligibleSource"})
            if target.get("fieldTargetIsSourceBoard") is not False or target.get("sourcePartsOrCoordinatesReturned") is not False:
                errors.append({"kind": "organizationEmpiricalFieldNotSeparatedFromBoardGeometry"})
            if any(key in target for key in ("parts", "sourceCoordinates", "sourcePartPoses", "sourceLinks")):
                errors.append({"kind": "organizationEmpiricalFieldLeaksBoardGeometry"})
            source_vector = np.asarray(organization.sharp_fields[record_index], float)
            sharp_maps = source_vector[:field_size].reshape(np.asarray(target["field"]).shape).copy()
            for first, last in ((0, 4), (4, 8)):
                sharp_maps[first:last] /= max(float(sharp_maps[first:last].sum()), 1e-12)
            sharp_orientation = source_vector[field_size:].reshape(np.asarray(target["orientation"]).shape) / .2
            selected_profile = organization.profiles[record_index]
            sharp_context = selected_profile[2:].copy()
            sharp_context[-1] = float(position) / 500.
            expected.update(field=sharp_maps, orientation=sharp_orientation,
                span=float(np.exp(selected_profile[1])), targetParts=float(np.expm1(selected_profile[0])),
                context=sharp_context, nativeWidthUnit=float(organization.width_units[record_index]))
        else:
            expected["nativeWidthUnit"] = float(np.exp(np.mean(np.log(organization.width_units[members]))))
        for key, value in expected.items():
            if not close_enough(target.get(key), value):
                errors.append({"kind": "organizationSampleFieldMismatch", "field": key})
        expected_families = sorted(set(organization.families)) if conditioned else sorted({organization.families[i] for i in members})
        if list(target.get("families", [])) != expected_families:
            errors.append({"kind": "organizationSampleFieldMismatch", "field": "families"})
        if int(target.get("position", -1)) != int(position):
            errors.append({"kind": "organizationPositionMismatch"})
    except (IndexError, KeyError, TypeError, ValueError) as error:
        errors.append({"kind": "invalidOrganizationSampleProvenance", "detail": str(error)})
    return errors


def audit_learned_placement(level, sources, organization, macro_saved, templates, allowed, hashes,
                            continuation=None, joint=None):
    """Reconstruct imported motif indices and learned/socket placement transforms."""
    errors = []
    provenance = level.get("assemblyProvenance", {})
    imports, decisions = provenance.get("tileImports", []), provenance.get("decisions", [])
    target = provenance.get("organizationTarget", {})
    errors.extend(audit_organization_target(target, level.get("position"), organization, allowed))
    model, eligible, _ = closed_library(macro_saved, templates)
    has_triple = any(d.get("kind") == "savedNativeTripleContinuation" for d in decisions)
    if has_triple and continuation is None:
        errors.append({"kind": "missingNativeTripleContinuationModel"})
    if any(d.get("kind") in ("jointNativePairTargetAndPose", "jointNativeTripleTargetAndPose") for d in decisions) and joint is None:
        errors.append({"kind": "missingJointNativeMotifModel"})
    if len(imports) != len(decisions):
        errors.append({"kind": "tileDecisionImportCountMismatch"})
    if set(map(str, model.levels)) != allowed:
        errors.append({"kind": "macroModelSourceSetMismatch"})

    clusters = []
    previous_root_ids = set()
    parts = {part["id"]: part for part in level.get("parts", [])}
    source_parts = {sid: {part["id"]: part for part in src["parts"]} for sid, src in sources.items()}
    for i, (imported, decision) in enumerate(zip(imports, decisions)):
        try:
            tile_index = int(imported["tileIndex"])
            if int(decision.get("tileIndex", -1)) != tile_index:
                errors.append({"kind": "decisionTileIndexMismatch", "importIndex": i})
            tile = model.tiles[tile_index]
            sid = str(tile["sourceId"])
            mapping = {int(old): new for old, new in imported["partMapping"].items()}
            decision_mapping = {int(old): new for old, new in decision.get("partMapping", {}).items()}
            if str(imported["sourceId"]) != sid or str(decision.get("sourceId")) != sid:
                errors.append({"kind": "decisionSourceMismatch", "importIndex": i})
            if decision_mapping != mapping:
                errors.append({"kind": "decisionPartMappingMismatch", "importIndex": i})
            if set(mapping) != set(map(int, tile["partIds"])):
                errors.append({"kind": "importDoesNotMatchSavedClosedMotif", "importIndex": i})
            if set(map(int, imported.get("originalPrototypePartIds", []))) != set(map(int, tile["originalPrototypePartIds"])):
                errors.append({"kind": "savedPrototypeRootMismatch", "importIndex": i})
            if decision.get("kind") != ("properNativeMotifSeed" if i == 0 else decision.get("kind")):
                errors.append({"kind": "invalidSeedDecisionKind", "importIndex": i})
            if i > 0 and decision.get("kind") not in ("exactWholeNativeSocket", "savedConditionalMacroRelation",
                    "savedNativeTripleContinuation", "jointNativePairTargetAndPose", "jointNativeTripleTargetAndPose"):
                errors.append({"kind": "unsupportedContinuationDecision", "importIndex": i})

            rotation = float(imported.get("rotation", 0.))
            translation = np.asarray(imported.get("translation", []), float)
            if len(translation) != 2 or not close_enough(decision.get("rotation"), rotation) or not close_enough(decision.get("translation"), translation):
                errors.append({"kind": "decisionTransformDiffersFromImport", "importIndex": i})
            if decision.get("kind") == "properNativeMotifSeed":
                expected_rotation = float(target["heading"]) - float(tile["heading"])
                expected_translation = -(pose_matrix(expected_rotation) @ np.asarray(tile["center"], float))
            elif decision.get("kind") == "savedConditionalMacroRelation":
                anchor_index = int(decision["anchorClusterIndex"])
                if not 0 <= anchor_index < len(clusters):
                    errors.append({"kind": "relationAnchorOutsidePriorFrontier", "importIndex": i})
                    continue
                anchor = clusters[anchor_index]
                if not frame_matches(decision.get("anchorFrame"), anchor):
                    errors.append({"kind": "relationAnchorFrameMismatch", "importIndex": i})
                record_index = int(decision["relationRecordIndex"])
                if decision.get("relationModelSha256") != hashes["supportingModelHashes"]["macro_assembly.joblib"]:
                    errors.append({"kind": "staleRelationModelHash", "importIndex": i})
                if not 0 <= record_index < len(model.pose_model.y):
                    errors.append({"kind": "relationRecordOutOfRange", "importIndex": i})
                    continue
                relpose = np.asarray(decision.get("sampledRelativePose", []), float)
                if not close_enough(relpose, model.pose_model.y[record_index], atol=1e-8):
                    errors.append({"kind": "sampledPoseDoesNotMatchSavedRecord", "importIndex": i})
                query_context = (list(target["context"])+list(anchor["prototypeDescriptor"])+list(tile["descriptor"]))
                if not close_enough(decision.get("recipientContext"), query_context, atol=1e-8):
                    errors.append({"kind": "relationRecipientContextMismatch", "importIndex": i})
                distances, neighbors = model.pose_model.neighbors([query_context])
                if record_index not in set(map(int, neighbors[0])):
                    errors.append({"kind": "relationRecordNotInConditionalNeighborSet", "importIndex": i})
                heading = anchor["heading"] + np.degrees(np.arctan2(relpose[3], relpose[2]))
                center = np.asarray(anchor["center"]) + pose_matrix(anchor["heading"]) @ relpose[:2] * max(anchor["width"], tile["width"])
                expected_rotation = float(heading - tile["heading"])
                expected_translation = center - pose_matrix(expected_rotation) @ np.asarray(tile["center"])
            elif decision.get("kind") == "savedNativeTripleContinuation":
                parent_index = int(decision["parentClusterIndex"])
                anchor_index = int(decision["anchorClusterIndex"])
                if not (0 <= parent_index < len(clusters) and 0 <= anchor_index < len(clusters)):
                    errors.append({"kind": "tripleFrontierOutsidePriorClusters", "importIndex": i})
                    continue
                parent, anchor = clusters[parent_index], clusters[anchor_index]
                if not frame_matches(decision.get("parentFrame"), parent) or not frame_matches(decision.get("anchorFrame"), anchor):
                    errors.append({"kind": "tripleFrontierFrameMismatch", "importIndex": i})
                if decision.get("continuationModelSha256") != hashes.get("continuationModelSha256"):
                    errors.append({"kind": "staleContinuationModelHash", "importIndex": i})
                record_index = int(decision["continuationRecordIndex"])
                continuation_model = continuation.get("model") if continuation else None
                if continuation_model is None or not 0 <= record_index < len(continuation_model.y):
                    errors.append({"kind": "continuationRecordOutOfRange", "importIndex": i})
                    continue
                relative = np.asarray(decision.get("sampledRelativePose", []), float)
                if not close_enough(relative, continuation_model.y[record_index], atol=1e-8):
                    errors.append({"kind": "sampledPoseDoesNotMatchContinuationRecord", "importIndex": i})
                vector = np.asarray(anchor["center"]) - np.asarray(parent["center"])
                spacing = float(np.linalg.norm(vector))
                incoming = float(np.degrees(np.arctan2(vector[1], vector[0])))
                angle = np.radians(anchor["heading"] - incoming)
                width = max(parent["width"], anchor["width"], tile["width"])
                da, db, dc = parent["prototypeDescriptor"], anchor["prototypeDescriptor"], tile["descriptor"]
                query_context = (list(target["context"])+da+db+dc+
                    [np.cos(angle), np.sin(angle), np.log(max(spacing/width, .001)),
                     float(da == db), float(db == dc)])
                if not close_enough(decision.get("recipientContext"), query_context, atol=1e-8):
                    errors.append({"kind": "continuationRecipientContextMismatch", "importIndex": i})
                _, neighbors = continuation_model.neighbors([query_context])
                if record_index not in set(map(int, neighbors[0])):
                    errors.append({"kind": "continuationRecordNotInConditionalNeighborSet", "importIndex": i})
                if not close_enough(decision.get("incomingAngleDegrees"), incoming) or not close_enough(decision.get("incomingSpacing"), spacing):
                    errors.append({"kind": "continuationIncomingFrameMismatch", "importIndex": i})
                center = np.asarray(anchor["center"]) + pose_matrix(incoming) @ relative[:2] * spacing
                heading = incoming + np.degrees(np.arctan2(relative[3], relative[2]))
                expected_rotation = float(heading - tile["heading"])
                expected_translation = center - pose_matrix(expected_rotation) @ np.asarray(tile["center"])
            elif decision.get("kind") in ("jointNativePairTargetAndPose", "jointNativeTripleTargetAndPose"):
                grammar = "pair" if decision["kind"] == "jointNativePairTargetAndPose" else "triple"
                if decision.get("jointGrammar") != grammar:
                    errors.append({"kind": "jointGrammarMismatch", "importIndex": i})
                if decision.get("jointModelSha256") != hashes.get("jointNativeModelSha256"):
                    errors.append({"kind": "staleJointNativeModelHash", "importIndex": i})
                estimator = joint.get("models", {}).get(grammar) if joint else None
                if estimator is None:
                    errors.append({"kind": "missingJointGrammarModel", "importIndex": i})
                    continue
                anchor_index = int(decision["anchorClusterIndex"])
                if not 0 <= anchor_index < len(clusters):
                    errors.append({"kind": "jointAnchorOutsidePriorFrontier", "importIndex": i})
                    continue
                anchor = clusters[anchor_index]
                if not frame_matches(decision.get("anchorFrame"), anchor):
                    errors.append({"kind": "jointAnchorFrameMismatch", "importIndex": i})
                if grammar == "pair":
                    query_context = pair_query(target["context"], {"descriptor": anchor["prototypeDescriptor"]})
                    relative_scale = max(anchor["width"], tile["width"])
                    heading_base = anchor["heading"]
                    center = None
                else:
                    parent_index = int(decision["grandparentClusterIndex"])
                    if not 0 <= parent_index < len(clusters):
                        errors.append({"kind": "jointGrandparentOutsidePriorFrontier", "importIndex": i})
                        continue
                    parent = clusters[parent_index]
                    if not frame_matches(decision.get("grandparentFrame"), parent):
                        errors.append({"kind": "jointGrandparentFrameMismatch", "importIndex": i})
                    vector = np.asarray(anchor["center"]) - np.asarray(parent["center"])
                    spacing = float(np.linalg.norm(vector))
                    incoming = float(np.degrees(np.arctan2(vector[1], vector[0])))
                    query_context = triple_query(target["context"],
                        {**parent, "descriptor": parent["prototypeDescriptor"]},
                        {**anchor, "descriptor": anchor["prototypeDescriptor"]}, incoming, spacing)
                    relative_scale = spacing
                    heading_base = incoming
                    if not close_enough(decision.get("incomingAngle"), incoming) or not close_enough(decision.get("incomingSpacing"), spacing):
                        errors.append({"kind": "jointIncomingFrameMismatch", "importIndex": i})
                if not close_enough(decision.get("recipientContext"), query_context, atol=1e-8):
                    errors.append({"kind": "jointRecipientContextMismatch", "importIndex": i})
                indices, probabilities = estimator.neighbors([query_context])
                record_index = int(decision["jointRecordIndex"])
                if record_index not in set(map(int, indices[0])):
                    errors.append({"kind": "jointRecordNotInConditionalNeighborSet", "importIndex": i})
                    continue
                local = list(map(int, indices[0])).index(record_index)
                if not close_enough(decision.get("jointNeighborProbability"), probabilities[0, local], atol=1e-8):
                    errors.append({"kind": "jointNeighborProbabilityMismatch", "importIndex": i})
                record = estimator.records[record_index]
                relative = np.asarray(decision.get("sampledRelativePose", []), float)
                if int(record.get("targetTile", -1)) != tile_index:
                    errors.append({"kind": "jointRecordTargetTileMismatch", "importIndex": i})
                if not close_enough(relative, record.get("pose"), atol=1e-8):
                    errors.append({"kind": "sampledPoseDoesNotMatchJointRecord", "importIndex": i})
                heading = heading_base + np.degrees(np.arctan2(relative[3], relative[2]))
                center = np.asarray(anchor["center"]) + pose_matrix(heading_base) @ relative[:2] * relative_scale
                expected_rotation = float(heading - tile["heading"])
                expected_translation = center - pose_matrix(expected_rotation) @ np.asarray(tile["center"])
            else:
                sid = str(tile["sourceId"])
                source_anchor = int(decision["sourceAnchor"])
                target_anchor = decision["targetAnchor"]
                if source_anchor not in mapping or mapping[source_anchor] != target_anchor or target_anchor not in previous_root_ids:
                    errors.append({"kind": "socketAnchorMappingMismatch", "importIndex": i})
                original = source_parts[sid][source_anchor]
                target_part = parts[target_anchor]
                expected_rotation = float(target_part["angle"] - original["angle"])
                expected_translation = np.asarray([target_part["x"], target_part["y"]]) - pose_matrix(expected_rotation) @ np.asarray([original["x"], original["y"]])
                if socket_key(original, templates, sources[sid]) != socket_key(target_part, templates, level):
                    errors.append({"kind": "socketTemplateMismatch", "importIndex": i})
            if not close_enough(rotation, expected_rotation) or not close_enough(translation, expected_translation):
                errors.append({"kind": "learnedPlacementTransformMismatch", "importIndex": i})

            center = pose_matrix(rotation) @ np.asarray(tile["center"]) + translation
            cluster = {"tileIndex": tile_index, "center": center.tolist(),
                       "heading": tile["heading"]+rotation, "width": tile["width"],
                       "prototypeDescriptor": tile["descriptor"]}
            if "anchorClusterIndex" in decision:
                cluster["parentClusterIndex"] = decision.get("anchorClusterIndex")
            clusters.append(cluster)
            previous_root_ids.update(mapping.values())
        except (IndexError, KeyError, TypeError, ValueError) as error:
            errors.append({"kind": "invalidLearnedPlacementProvenance", "importIndex": i, "detail": str(error)})
    return errors


def audit_tile_closures(level, sources):
    errors = []
    provenance = level.get("assemblyProvenance", {})
    for import_index, imported in enumerate(provenance.get("tileImports", [])):
        sid = str(imported.get("sourceId"))
        source = sources.get(sid)
        if source is None:
            errors.append({"kind": "unknownTileSource", "importIndex": import_index, "sourceId": sid})
            continue
        mapping = {int(old): new for old, new in imported.get("partMapping", {}).items()}
        prototype = set(map(int, imported.get("originalPrototypePartIds", [])))
        declared = set(map(int, imported.get("nativeOutgoingGuideClosurePartIds", [])))
        if not prototype:
            errors.append({"kind": "missingOriginalPrototypeRoots", "importIndex": import_index})
        expected = native_closure(source, prototype)
        if declared != expected:
            errors.append({"kind": "incorrectNativeClosureDeclaration", "importIndex": import_index,
                           "missing": sorted(expected-declared), "extra": sorted(declared-expected)})
        if set(mapping) != declared:
            errors.append({"kind": "tileMappingDiffersFromClosedRoots", "importIndex": import_index,
                           "missing": sorted(declared-set(mapping)), "extra": sorted(set(mapping)-declared)})
    return errors


def partial_source_embedding_diagnostics(level, sources):
    """Report coherent rigid source-board subsets without applying a quota.

    A small native motif can remain useful even when it came from a known
    source. This reports how many distinct roots from each donor align under a
    single source-board transform; reviewers can inspect the geometry when a
    large partial embedding appears.
    """
    parts = {p["id"]: p for p in level.get("parts", [])}
    by_source = {}
    for imported in level.get("assemblyProvenance", {}).get("tileImports", []):
        sid = str(imported.get("sourceId"))
        transform = (float(imported.get("rotation", 0.)),
                     np.asarray(imported.get("translation", [0., 0.]), float))
        mapping = {int(old): new for old, new in imported.get("partMapping", {}).items()}
        for old, new in mapping.items():
            by_source.setdefault(sid, []).append((old, new, transform))
    rows = []
    candidate_root_count = max(1, len(parts))
    for sid, entries in by_source.items():
        source_parts = {p["id"]: p for p in sources.get(sid, {}).get("parts", [])}
        unique = {}
        for old, new, transform in entries:
            unique.setdefault((old, new), transform)
        best = {"consistentRootCount": 0, "sourceRootCoverage": 0., "candidateRootCoverage": 0.}
        source = sources.get(sid, {})
        source_links = source.get("links", [])
        source_guides = source.get("guideLinks", [])
        for _, (rotation, translation) in unique.items():
            fitted = []
            for (old, new), (candidate_rotation, candidate_translation) in unique.items():
                source = source_parts.get(old)
                candidate = parts.get(new)
                if source is None or candidate is None:
                    continue
                angle_error = abs((candidate["angle"]-source["angle"]-rotation+180.) % 360.-180.)
                xy = pose_matrix(rotation) @ np.asarray([source["x"], source["y"]]) + translation
                if (angle_error <= .005 and
                        np.linalg.norm(np.asarray([candidate["x"], candidate["y"]])-xy) <= 1e-4):
                    fitted.append((old, new))
            if len(fitted) > best["consistentRootCount"]:
                fitted_old = {old for old, _ in fitted}
                fitted_map = {old: new for old, new in fitted}
                induced_links = [edge for edge in source_links
                                 if edge["source"] in fitted_old and edge["target"] in fitted_old]
                induced_guides = [edge for edge in source_guides
                                  if edge["guideOwnerId"] in fitted_old and edge["lockedOwnerId"] in fitted_old]
                candidate_links = {(edge["source"], edge["target"], tuple(edge["sourceShapeIndices"]), tuple(edge["targetShapeIndices"]))
                                   for edge in level.get("links", [])}
                candidate_guides = {(edge["guideOwnerId"], edge["lockedOwnerId"], tuple(edge["guideShapeIndices"]))
                                    for edge in level.get("guideLinks", [])}
                retained_links = sum((fitted_map[e["source"]], fitted_map[e["target"]],
                    tuple(e["sourceShapeIndices"]), tuple(e["targetShapeIndices"])) in candidate_links for e in induced_links)
                retained_guides = sum((fitted_map[e["guideOwnerId"]], fitted_map[e["lockedOwnerId"]],
                    tuple(e["guideShapeIndices"])) in candidate_guides for e in induced_guides)
                best = {"consistentRootCount": len(fitted),
                        "sourceRootCoverage": len(fitted_old)/max(1, len(source_parts)),
                        "candidateRootCoverage": len({new for _, new in fitted})/candidate_root_count,
                        "sourcePartIds": sorted(fitted_old),
                        "candidatePartIds": sorted({new for _, new in fitted}),
                        "inducedNativeLinkCount": len(induced_links),
                        "retainedNativeLinkCount": retained_links,
                        "retainedNativeLinkFraction": retained_links/max(1, len(induced_links)),
                        "inducedNativeGuideCount": len(induced_guides),
                        "retainedNativeGuideCount": retained_guides,
                        "retainedNativeGuideFraction": retained_guides/max(1, len(induced_guides))}
        # This is a high-signal review trigger, not a donor quota: a single
        # exact embedding dominates the candidate while preserving nearly all
        # induced authored structure. Small embedded motifs remain diagnostic
        # only, even when every motif root has a source origin.
        review_required = (best["candidateRootCoverage"] >= .5 and
            (best["inducedNativeLinkCount"] + best["inducedNativeGuideCount"] > 0) and
            best["retainedNativeLinkFraction"] >= .8 and best["retainedNativeGuideFraction"] >= .8)
        rows.append({"sourceId": sid, "sourceRootCount": len(source_parts),
                     "distinctImportedSourceRootCount": len({old for old, _ in unique}), **best,
                     "partialSourceCopyReviewRequired": bool(review_required),
                     "reviewMeaning": "Exact rigid source-board subset diagnostic only; not an automatic rejection or content quota."})
    return sorted(rows, key=lambda row: (row["consistentRootCount"], row["sourceRootCoverage"]), reverse=True)


def audit_candidate(level, sources, templates, integrity, seam, canonical_sources, allowed, bound_hashes,
                    organization_model, macro_model, continuation_model, joint_model):
    errors = []
    provenance = level.get("assemblyProvenance", {})
    if provenance.get("sourceScaffoldUsed") is not False:
        errors.append({"kind": "sourceScaffoldNotExplicitlyDisabled"})
    if provenance.get("emptyStart") != {"parts": 0, "links": 0, "guideLinks": 0, "bombs": 0}:
        errors.append({"kind": "missingEmptyStartCertificate"})
    organization = provenance.get("organizationTarget", {})
    latent = set(map(str, organization.get("latentSourceRecords", [])))
    if not latent or not latent <= allowed:
        errors.append({"kind": "organizationSupportOutsideOrdinaryCorpus",
                       "unknown": sorted(latent-allowed)})
    model_hashes = provenance.get("modelHashes", {})
    if not model_hashes.get("organizationModel") or not model_hashes.get("macroModel"):
        errors.append({"kind": "missingLearnedModelBindings"})
    if model_hashes.get("organizationModel") != bound_hashes["organizationModelSha256"]:
        errors.append({"kind": "staleOrganizationModelBinding"})
    if model_hashes.get("macroModel") != bound_hashes["supportingModelHashes"]["macro_assembly.joblib"]:
        errors.append({"kind": "staleMacroModelBinding"})
    errors.extend(audit_tile_closures(level, sources))
    errors.extend(audit_learned_placement(level, sources, organization_model, macro_model,
                                          templates, allowed, bound_hashes, continuation_model, joint_model))

    native = integrity.check(level)
    raw = seam.check(level)
    if not native.get("nativeAssemblyIntegrityPassed"):
        errors.append({"kind": "nativeAssemblyIntegrityFailed", "details": native.get("errors", [])})
    if not raw.get("rawGeometrySeamPassed"):
        errors.append({"kind": "rawNativeGeometrySeamFailed", "details": raw.get("errors", [])})

    fingerprint = canonical_fingerprint(level, templates)
    exact_sources = canonical_sources.get(fingerprint, [])
    if exact_sources:
        errors.append({"kind": "wholeBoardCanonicalSourceEquivalent", "sourceIds": exact_sources})
    if not level.get("parts"):
        errors.append({"kind": "emptyCandidate"})
    partial_diagnostics = partial_source_embedding_diagnostics(level, sources)
    partial_flags = [row for row in partial_diagnostics if row["partialSourceCopyReviewRequired"]]
    return {"id": level.get("id"), "position": level.get("position"),
            "passed": not errors and not partial_flags,
            "structuralPassed": not errors,
            "partialSourceCopyReviewPassed": not partial_flags,
            "partialSourceCopyReviewRequired": bool(partial_flags),
            "partialSourceCopyMatches": partial_diagnostics,
            "errors": errors + ([{"kind": "substantialExactRigidPartialSourceEmbeddingReviewRequired",
                                   "sourceIds": [row["sourceId"] for row in partial_flags]}] if partial_flags else []),
            "nativeAssemblyIntegrityPassed": bool(native.get("nativeAssemblyIntegrityPassed")),
            "rawGeometrySeamPassed": bool(raw.get("rawGeometrySeamPassed")),
            "canonicalSourceEquivalent": bool(exact_sources),
            "partCount": len(level.get("parts", [])),
            "tileImportCount": len(provenance.get("tileImports", [])),
            "guideLinkCount": len(level.get("guideLinks", [])), "bombCount": len(level.get("bombs", []))}


def audit(workspace, pilot_path, report_path, organization_model_name=None):
    workspace = Path(workspace).resolve()
    pilot = read(pilot_path)
    pilot_model_hash = pilot.get("hashes", {}).get("organizationModel")
    if organization_model_name is None:
        model_names = ("organization_v5.joblib", "organization_sharp_v5.joblib",
                       "organization_conditioned_v5.joblib")
        matched = [name for name in model_names if (workspace/"models"/name).exists()
                   and sha(workspace/"models"/name) == pilot_model_hash]
        if len(matched) != 1:
            raise ValueError("Cannot uniquely resolve saved organization model from pilot hash; pass --organization-model-name")
        organization_model_name = matched[0]
    if Path(organization_model_name).name != organization_model_name or not organization_model_name.endswith(".joblib"):
        raise ValueError("Organization model name must be a model filename")
    levels, template_doc, allowed, excluded, organization_model, macro_model, continuation_model, joint_model, hashes = check_model_bindings(workspace, organization_model_name)
    templates = {template["id"]: template for template in template_doc["templates"]}
    novelty_dir = workspace / "data/novelty_extras"
    extra_pack = read(novelty_dir / "source_parity_pack.json")["levels"]
    extra_templates = read(novelty_dir / "source_templates.json")["templates"]
    all_templates = dict(templates)
    all_templates.update({template["id"]: template for template in extra_templates})
    sources = {str(level["sourceId"]): level for level in levels + extra_pack}
    canonical_sources = {}
    for source in levels + extra_pack:
        canonical_sources.setdefault(canonical_fingerprint(source, all_templates), []).append(str(source["sourceId"]))
    integrity = NativeAssemblyIntegrityAudit(templates, levels)
    seam = NativeGeometrySeamAudit(templates, levels)
    pilot_hashes = pilot.get("hashes", {})
    if pilot_hashes.get("sourcePack") != hashes["sourcePackSha256"]:
        raise ValueError("Pilot source-pack hash does not match the audited corpus")
    if pilot_hashes.get("organizationModel") != hashes["organizationModelSha256"]:
        raise ValueError("Pilot organization-model hash does not match the saved ordinary model")
    if pilot_hashes.get("macroModel") != hashes["supportingModelHashes"]["macro_assembly.joblib"]:
        raise ValueError("Pilot macro-model hash does not match the refitted ordinary model")
    if (pilot_hashes.get("continuationModel") is not None and
            pilot_hashes.get("continuationModel") != hashes.get("continuationModelSha256")):
        raise ValueError("Pilot continuation-model hash does not match the saved ordinary model")
    if (pilot_hashes.get("continuationModel") is None and
            any(d.get("kind") == "savedNativeTripleContinuation"
                for level in pilot.get("levels", [])
                for d in level.get("assemblyProvenance", {}).get("decisions", []))):
        raise ValueError("Pilot uses native triple continuation without binding its saved model")
    if (pilot_hashes.get("jointNativeModel") is not None and
            pilot_hashes.get("jointNativeModel") != hashes.get("jointNativeModelSha256")):
        raise ValueError("Pilot joint native model hash does not match the saved model")
    if (pilot_hashes.get("jointNativeModel") is None and
            any(d.get("kind", "").startswith("jointNative")
                for level in pilot.get("levels", [])
                for d in level.get("assemblyProvenance", {}).get("decisions", []))):
        raise ValueError("Pilot uses joint native proposal without binding its saved model")
    candidates = pilot.get("levels", [])
    rows = []
    seen_fingerprints = {}
    for level in candidates:
        row = audit_candidate(level, sources, templates, integrity, seam, canonical_sources, allowed, hashes,
                              organization_model, macro_model, continuation_model, joint_model)
        fingerprint = canonical_fingerprint(level, all_templates)
        prior_candidate = seen_fingerprints.get(fingerprint)
        if prior_candidate is not None:
            row["passed"] = False
            row["errors"].append({"kind": "duplicateEarlierPilotCandidate", "candidateId": prior_candidate})
        seen_fingerprints[fingerprint] = level.get("id")
        rows.append(row)
    report = {
        "schemaVersion": 1,
        "status": "INDEPENDENT_CONSTRUCTIVE_V5_AUDIT",
        "passed": bool(rows) and all(row["passed"] for row in rows),
        "generationAvailability": ("CANDIDATES_AUDITED" if rows else
                                    "NO_CANDIDATE_OBSERVED_SEARCH_REMAINS_UNKNOWN"),
        "candidateCount": len(rows), "passedCandidateCount": sum(row["passed"] for row in rows),
        "ordinaryTrainingSourceCount": len(allowed), "explicitlyExcludedSourceCount": len(excluded),
        "excludedSourceIds": sorted(excluded), "pilotStatus": pilot.get("status"),
        "closureInterpretation": "Conservative outgoing authored-link and touched-guide closure used by this pilot; not asserted as original game law.",
        "originalityInterpretation": "Reject whole-board canonical source equivalence. Native source motifs inside a meaningfully larger composition are allowed.",
        "partialSourceCopyReviewCriteria": {"candidateRootCoverageAtLeast": 0.5,
            "requiresInducedNativeRelations": True, "retainedNativeLinkFractionAtLeast": 0.8,
            "retainedNativeGuideFractionAtLeast": 0.8,
            "meaning": "High-signal exact rigid source-subset review trigger; not a donor-count quota or game rule."},
        "limits": ["Raw seam and native provenance checks do not establish runtime replay or solvability.",
                   "Canonical fingerprint rejects exact rigid/cosmetic whole-board equivalence; near-copy review remains heuristic.",
                   "Failure to find motifs within finite search support means unknown, not impossible."],
        "hashes": {**hashes, "pilotSha256": sha(pilot_path), "auditSha256": sha(Path(__file__))},
        "candidates": rows,
    }
    Path(report_path).parent.mkdir(parents=True, exist_ok=True)
    Path(report_path).write_text(json.dumps(report, indent=2), encoding="utf8")
    return report


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace", type=Path, required=True)
    parser.add_argument("--pilot", type=Path, required=True)
    parser.add_argument("--report", type=Path, required=True)
    parser.add_argument("--organization-model-name", type=str)
    args = parser.parse_args()
    report = audit(args.workspace, args.pilot, args.report, args.organization_model_name)
    print(json.dumps({"passed": report["passed"], "candidateCount": report["candidateCount"],
                      "passedCandidateCount": report["passedCandidateCount"], "report": str(args.report)}))


if __name__ == "__main__":
    main()
