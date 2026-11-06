"""Independent source/context/rigid-unit certificate for anchored guide angles."""
from pathlib import Path
import math
import joblib
import numpy as np

import guide_neighborhood_motifs_v3 as base
from guide_anchor_role_motifs_v3 import pair_context


class AnchoredGuidePatchAudit:
    def __init__(self, sources, templates, source_path, model_path):
        self.sources = {l["sourceId"]: l for l in sources}
        self.templates = {tid: base.compact_object(t) for tid, t in templates.items()}
        self.bundle = joblib.load(model_path)
        self.source_hash = base.sha(source_path)
        self.template_hash = base.sha(Path(source_path).with_name("source_templates.json"))
        self.model_hash = base.sha(model_path)

    def check(self, candidate):
        errors = []
        cert = candidate.get("assemblyProvenance", {}).get("anchoredGuidePatchCertificate", {})
        if (cert.get("sourcePackSha256") != self.source_hash or cert.get("modelSha256") != self.model_hash
                or self.bundle.get("sourcePackSha256") != self.source_hash
                or self.bundle.get("templatePackSha256") != self.template_hash):
            errors.append("source_template_or_model_hash_mismatch")
        try:
            reference = self.sources[cert["referenceSourceId"]]
            anchor, other = cert["anchorPrimaryId"], cert["otherPrimaryId"]
            if anchor == other:
                return {"passed": False, "errors": errors+["anchor_and_other_must_be_distinct"]}
            context = pair_context(reference, self.templates, anchor, other)
            if not np.allclose(context["context"], cert["recipientContext"], rtol=0, atol=1e-9):
                errors.append("forged_recipient_context")
            index = cert["learnedRecordIndex"]
            if index not in self.bundle["model"].neighbors([context["context"]])[1][0]:
                errors.append("record_outside_conditional32")
            record = self.bundle["records"][index]
            if record["sourceId"] == reference["sourceId"]:
                errors.append("same_reference_source_record_excluded")
            if record["sourceId"] != cert["donorSourceId"]:
                errors.append("forged_donor_source_id")
            old = {p["id"]: p for p in reference["parts"]}
            actual = {p["id"]: p for p in candidate["parts"]}
            for i in context["anchorUnitIds"]:
                if any(abs(actual[i][k]-old[i][k]) > 1e-7 for k in ("x", "y", "angle")) or actual[i]["templateId"] != old[i]["templateId"]:
                    errors.append("fixed_complete_anchor_unit_changed")
            phi = math.degrees(math.atan2(record["pose"][1], record["pose"][0]))
            delta = old[anchor]["angle"]+phi-old[other]["angle"]
            rotation = math.radians(delta)
            c, s = math.cos(rotation), math.sin(rotation)
            for i in context["otherUnitIds"]:
                dx, dy = old[i]["x"]-old[other]["x"], old[i]["y"]-old[other]["y"]
                expected = (old[other]["x"]+c*dx-s*dy, old[other]["y"]+s*dx+c*dy)
                angle_error = abs((actual[i]["angle"]-old[i]["angle"]-delta+180)%360-180)
                if math.hypot(actual[i]["x"]-expected[0], actual[i]["y"]-expected[1]) > 1e-5 or angle_error > .005:
                    errors.append("other_complete_unit_not_on_saved_relative_angle_pattern")
                if actual[i]["templateId"] != old[i]["templateId"]:
                    errors.append("other_complete_unit_native_template_changed")
            groups = {tuple(sorted(g)) for g in candidate["assemblyProvenance"]["learnedPlacementCertificate"]["guideRigidGroups"]}
            if tuple(sorted(context["otherUnitIds"])) not in groups:
                errors.append("complete_other_native_guide_group_not_claimed")
            return {"passed": not errors, "errors": errors, "donorSourceId": record["sourceId"], "recordIndex": int(index),
                    "savedRelativeAngleDegrees": phi, "limits": "Saved angle provenance and complete rigid-unit mapping only, no native obstruction/release or difficulty proof."}
        except (KeyError, ValueError, TypeError, IndexError) as error:
            return {"passed": False, "errors": errors+[type(error).__name__+": "+str(error)]}
