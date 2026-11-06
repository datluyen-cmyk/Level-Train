from __future__ import annotations
import collections, hashlib, json, math
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / "Research" / "normalized_corpus.json"
OUTPUT = ROOT / "Research" / "source_attachment_catalog_audit.json"

def quantiles(values):
    ordered = sorted(float(v) for v in values)
    if not ordered:
        return {}
    return {name: ordered[int((len(ordered) - 1) * q)] for name, q in
            (("min", 0.0), ("p10", 0.1), ("median", 0.5), ("p90", 0.9), ("max", 1.0))}

def main():
    raw = SOURCE.read_bytes()
    corpus = json.loads(raw)
    levels = corpus["levels"]
    kinds = collections.Counter()
    all_attachments = []
    per_ring_tbar_count = collections.Counter()
    tbar_gaps = collections.Counter()
    tbar_solid_angle = collections.Counter()
    dimension_values = collections.defaultdict(list)
    tbar_joint_signatures = collections.Counter()
    tbar_levels = set()
    rings = 0
    for level in levels:
        for part in level.get("state", {}).get("parts", []):
            if part.get("kind") != "ring":
                continue
            rings += 1
            visual = part.get("sourceRenderData") or {}
            native = part.get("sourcePartData") or {}
            attachments = visual.get("_attachments") or []
            tbar_in_piece = []
            for attachment in attachments:
                # RingAttachment.AttachType: HeadedBar=0, TBar=1.
                kind = int(attachment.get("type", 0))
                kinds[kind] += 1
                all_attachments.append(attachment)
                if kind != 1:
                    continue
                tbar_levels.add(level["id"])
                tbar_in_piece.append(attachment)
                for key in ("stemLength", "stemWidth", "crossWidth", "crossLength", "stemInsertOffset", "crossCapLength"):
                    dimension_values[key].append(float(attachment.get(key, 0.0)))
                physical_gap = abs(float(native.get("GapAngleDeg", part.get("gap", 0.0)))) % 360.0
                gap_direction = float(visual.get("_gapDirectionDeg", 0.0))
                delta = (float(attachment.get("angleDeg", 0.0)) - gap_direction + 180.0) % 360.0 - 180.0
                inside_gap = physical_gap > 0.0 and abs(delta) < physical_gap * .5
                tbar_solid_angle["insidePhysicalGap" if inside_gap else "onSolidArc"] += 1
            if tbar_in_piece:
                per_ring_tbar_count[len(tbar_in_piece)] += 1
                physical = {key: native.get(key) for key in ("InnerRadius", "OuterRadius", "GapAngleDeg")}
                render = {key: visual.get(key) for key in ("_radius", "_width", "_gapAngleDeg", "_gapDirectionDeg", "_capRatio", "_extraEnabled", "_styleIndex", "_attachments")}
                # Hash the full per-piece pair; this preserves mount/angle/size associations.
                payload = json.dumps({"sourcePartData": physical,
                                      "sourceNativeData": part.get("sourceNativeData") or {},
                                      "sourceRenderData": render},
                                     sort_keys=True, separators=(",", ":")).encode("utf-8")
                tbar_joint_signatures[hashlib.sha256(payload).hexdigest()] += 1
    # Radius-normalized render dimensions are reported as audit aids, not generation ranges.
    normalized = collections.defaultdict(list)
    for level in levels:
        for part in level.get("state", {}).get("parts", []):
            if part.get("kind") != "ring":
                continue
            visual = part.get("sourceRenderData") or {}
            radius = float(visual.get("_radius", part.get("radius", 0.0)))
            for attachment in visual.get("_attachments") or []:
                if int(attachment.get("type", 0)) != 1 or radius <= 0:
                    continue
                for key in ("stemLength", "stemWidth", "crossWidth", "crossLength"):
                    normalized[key + "PerVisualRadius"].append(float(attachment.get(key, 0.0)) / radius)
    report = {
        "schemaVersion": 1,
        "sourceCorpus": str(SOURCE.relative_to(ROOT)).replace("\\", "/"),
        "sourceCorpusSha256": hashlib.sha256(raw).hexdigest(),
        "sourceLevels": len(levels),
        "sourceRingPieces": rings,
        "attachmentTypeEnum": {"0": "HeadedBar", "1": "TBar"},
        "serializedAttachmentCounts": {
            "HeadedBar_type0": kinds[0],
            "TBar_type1": kinds[1],
            "all": sum(kinds.values()),
        },
        "tBarSourceFacts": {
            "levelsWithAtLeastOne": len(tbar_levels),
            "ringPiecesWithAtLeastOne": sum(per_ring_tbar_count.values()),
            "mountCounts": {
                "inner": sum(bool(a.get("isInnerMount", False)) for a in all_attachments if int(a.get("type", 0)) == 1),
                "outer": sum(not bool(a.get("isInnerMount", False)) for a in all_attachments if int(a.get("type", 0)) == 1),
            },
            "mountAngleVsPhysicalGap": dict(tbar_solid_angle),
            "tBarsPerSourceRing": {str(k): v for k, v in sorted(per_ring_tbar_count.items())},
            "dimensionQuantiles": {key: quantiles(values) for key, values in dimension_values.items()},
            "dimensionPerVisualRadiusQuantiles": {key: quantiles(values) for key, values in normalized.items()},
            "uniqueJointPhysicalVisualPieceSignatures": len(tbar_joint_signatures),
            "jointPieceSignatureFrequencyHistogram": {
                str(freq): sum(1 for count in tbar_joint_signatures.values() if count == freq)
                for freq in sorted(set(tbar_joint_signatures.values()))
            },
        },
        "method": "Read only exact normalized source records. TBar fields come from each ring's serialized sourceRenderData._attachments (enum type 1); physical radii/gap come from the same part's sourcePartData. Joint signatures hash that ring's complete sourceNativeData plus complete attachment array and visual radius/width/gap/direction/cap/extra/style. No fields are synthesized or independently mixed.",
        "limitations": ["This is a source catalog audit, not a solvability audit.", "Quantiles describe serialized render fields; they are not generation ranges.", "Physical collider geometry is separately exported by the physics catalog."],
    }
    OUTPUT.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"Wrote {OUTPUT}")
    print(f"TBars: {kinds[1]} in {len(tbar_levels)} levels / {sum(per_ring_tbar_count.values())} ring pieces; inner mounts {report['tBarSourceFacts']['mountCounts']['inner']}")

if __name__ == "__main__":
    main()
