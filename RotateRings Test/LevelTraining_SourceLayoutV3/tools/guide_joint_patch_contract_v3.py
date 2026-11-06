"""Independent saved-joint-patch certificate check, separate from native assets."""
import math
import numpy as np
import joblib

import guide_neighborhood_motifs_v3 as base
from layout_learning_v3 import hashfile


def selected_context(reference, templates, primary_ids):
    us = {u["primary"]: u for u in base.units(reference, templates)}
    selected = [us[i] for i in sorted(primary_ids)]
    anchor = selected[0]
    heading = base.board_heading(reference)
    width = float(np.median([u["width"] for u in selected]))
    xy = np.asarray([u["xy"] for u in selected])
    relative = base.rotate(xy-np.asarray(anchor["xy"]), -heading)/width
    order = [0]+sorted(range(1, len(selected)), key=lambda i: (math.atan2(relative[i, 1], relative[i, 0]), np.linalg.norm(relative[i])))
    xy = relative[order]
    units = [selected[i] for i in order]
    extent = np.ptp(xy, axis=0)
    pair = np.linalg.norm(xy[:, None, :]-xy[None, :, :], axis=2)
    np.fill_diagonal(pair, np.inf)
    spacing = np.quantile(pair.min(axis=1), [.1, .5, .9])
    region = [*extent.tolist(), float(max(extent)/max(min(extent), .01)), *spacing.tolist(), width]
    context = base.level_context(reference)+[v for u in units for v in u["descriptor"]]+region
    return {"sourceId": reference["sourceId"], "position": reference["position"], "size": len(units),
            "unitIds": [u["ids"] for u in units], "primaryIds": [u["primary"] for u in units],
            "heading": heading, "width": width, "context": context,
            "nativeReferencePrimaryXY": [u["xy"] for u in units]}


class JointGuidePatchAudit:
    def __init__(self, sources, templates, model_path, source_path):
        self.sources = {l["sourceId"]: l for l in sources}
        self.templates = {tid: base.compact_object(t) for tid, t in templates.items()}
        self.bundle = joblib.load(model_path)
        self.model_hash = hashfile(model_path)
        self.source_hash = hashfile(source_path)
        self.template_hash = hashfile(source_path.with_name("source_templates.json"))

    def check(self, candidate):
        errors = []
        cert = candidate["assemblyProvenance"].get("jointGuidePatchCertificate", {})
        if (self.bundle.get("sourcePackSha256") != self.source_hash
                or self.bundle.get("templatePackSha256") != self.template_hash):
            errors.append("saved_model_corpus_hash_mismatch")
        if cert.get("modelSha256") != self.model_hash or cert.get("sourcePackSha256") != self.source_hash:
            errors.append("model_or_source_hash_mismatch")
        reference = self.sources.get(cert.get("referenceSourceId"))
        if reference is None:
            return {"passed": False, "errors": errors+["unknown_reference"]}
        try:
            if (len(cert["selectedPrimaryIds"]) not in (4, 5)
                    or len(set(cert["selectedPrimaryIds"])) != len(cert["selectedPrimaryIds"])):
                return {"passed": False, "errors": errors+["invalid_distinct_native_unit_selection"]}
            context = selected_context(reference, self.templates, cert["selectedPrimaryIds"])
            if not np.allclose(context["context"], cert["recipientContext"], rtol=0, atol=1e-9):
                errors.append("forged_recipient_context")
            saved = self.bundle["models"][context["size"]]
            index = cert["learnedRecordIndex"]
            if index not in saved["model"].neighbors([context["context"]])[1][0]:
                errors.append("record_outside_conditional32_support")
            record = saved["records"][index]
            if cert.get("donorSourceId") != record["sourceId"]:
                errors.append("forged_donor_source_id")
            if record["sourceId"] == reference["sourceId"]:
                errors.append("same_reference_native_record_excluded")
            if abs(record["width"]-context["width"]) > 1e-5:
                errors.append("native_width_mismatch")
            lc = len(base.level_context(reference))
            if any(abs(record["context"][lc+i*15+6]-context["context"][lc+i*15+6]) > 1e-5
                   for i in range(context["size"])):
                errors.append("native_per_unit_width_mismatch")
            rotation = np.asarray(cert["rotationMatrixRowVector"], float)
            translation = np.asarray(cert["translation"], float)
            if (rotation.shape != (2, 2) or translation.shape != (2,) or not np.all(np.isfinite(rotation))
                    or not np.all(np.isfinite(translation)) or not np.allclose(rotation.T@rotation, np.eye(2), atol=1e-8)
                    or abs(np.linalg.det(rotation)-1) > 1e-8 or cert.get("scale") != 1.):
                errors.append("not_proper_common_rigid_scale1_transform")
                return {"passed": False, "errors": errors}
            ps = {p["id"]: p for p in candidate["parts"]}
            expected_xy = np.asarray(record["nativeXYInBoardFrame"])@rotation+translation
            phi = np.asarray(record["pose"]).reshape(context["size"], 4)
            angle = math.degrees(math.atan2(rotation[0, 1], rotation[0, 0]))
            expected_angles = np.degrees(np.arctan2(phi[:, 3], phi[:, 2]))+angle
            residuals = []
            for i, primary in enumerate(context["primaryIds"]):
                part = ps[primary]
                delta_xy = float(np.linalg.norm(expected_xy[i]-[part["x"], part["y"]]))
                delta_angle = abs((part["angle"]-expected_angles[i]+180)%360-180)
                residuals.append({"partId": primary, "nativeRecordXYResidual": delta_xy, "nativeRecordAngleResidualDegrees": delta_angle})
                if delta_xy > 1e-4 or delta_angle > .005:
                    errors.append("unit_not_on_same_rigid_native_record")
            # Asset audit separately proves all complete unit internal poses.
            claimed_groups = candidate["assemblyProvenance"]["learnedPlacementCertificate"]["guideRigidGroups"]
            if {tuple(sorted(g)) for g in claimed_groups} != {tuple(sorted(g)) for g in context["unitIds"]}:
                errors.append("complete_guide_unit_group_mismatch")
            return {"passed": not errors, "errors": errors, "unitResiduals": residuals,
                    "nativeRecordIndex": index, "donorSourceId": record["sourceId"],
                    "limits": "One saved joint native XY+angle pattern support only. Native asset, raw geometry, novelty, wholefigure and gameplay certificates remain separate."}
        except (KeyError, ValueError, IndexError, TypeError) as error:
            return {"passed": False, "errors": errors+[type(error).__name__+": "+str(error)]}
