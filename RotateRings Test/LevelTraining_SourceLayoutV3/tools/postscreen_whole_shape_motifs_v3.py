"""Read-only review flags joining frozen/current proposals to visual inventory.

Observed labels and inferred clusters are never automated shape-acceptance gates.
No stage writer, candidate, acceptance status or source annotation is modified.
"""
from pathlib import Path
from collections import Counter
import argparse
import hashlib
import json


def digest(data):
    return hashlib.sha256(data).hexdigest()


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--inventory", required=True, type=Path)
    p.add_argument("--pack", required=True, type=Path)
    p.add_argument("--additional-pack", action="append", type=Path, default=[])
    p.add_argument("--output", required=True, type=Path)
    args = p.parse_args()
    inventory_bytes = args.inventory.read_bytes()
    inventory = json.loads(inventory_bytes)
    by_position = {r["position"]: r for r in inventory["levels"]}
    by_id = {r["sourceId"]: r for r in inventory["levels"]}
    rows = []
    pack_hashes = {}
    for path in [args.pack, *args.additional_pack]:
        # The producer replaces files atomically. Bind these exact read bytes;
        # a later producer revision simply requires a new read-only post-screen.
        raw = path.read_bytes()
        pack = json.loads(raw)
        pack_hashes[str(path.resolve())] = digest(raw)
        for c in pack["levels"]:
            ref = by_position.get(c["position"])
            if not ref:
                raise ValueError("Reference position missing from visual inventory")
            ap = c["assemblyProvenance"]
            claimed_ref = c.get("stagingProvenance", {}).get("sourceReferenceId")
            if claimed_ref is not None and claimed_ref != ref["sourceId"]:
                raise ValueError("Candidate source ID differs from inventoried reference")
            decision = ap.get("decisions", [{}])[0]
            modified = set(decision.get("modifiedRootIds", []))
            parts = {p["id"]: p for p in c["parts"]}
            special = bool(ref.get("observedLabel"))
            row = {"id": c["id"], "referencePosition": c["position"], "referenceSourceId": ref["sourceId"],
                   "pack": str(path.resolve()), "observedWholeBoardLabel": ref.get("observedLabel"),
                   "observedConfidence": ref.get("observedConfidence"), "inferredClusters": ref.get("inferredClusters", []),
                   "specialWholeShapeReviewRequired": special, "inventoryReviewStatus": ref["reviewStatus"],
                   "reviewFlag": "OBSERVED_MOTIF_REQUIRE_WHOLE_SHAPE_AND_RELEVANT_STRUCTURE_REVIEW" if special
                                 else "NO_CONFIDENT_SPECIAL_LABEL_IS_NOT_AN_ORDINARY_LAYOUT_CERTIFICATE",
                   "claimedModifiedRootIds": sorted(modified),
                   "changedNativeDependencySymmetricDifference": decision.get("authoredDependencySymmetricDifference"),
                   "nativeRelevantInteriorReleaseChangeStatus": "UNVERIFIED",
                   "visualWholeShapePreservationStatus": "UNVERIFIED",
                   "accepted": False}
            motif = ap.get("guideNeighborhoodCertificate")
            if motif:
                donor = by_id.get(motif["donorSourceId"], {})
                row["jointGuideMotifDonor"] = {"sourceId": motif["donorSourceId"], "position": donor.get("position"),
                    "observedWholeBoardLabel": donor.get("observedLabel"), "inferredClusters": donor.get("inferredClusters", []),
                    "reviewNote": "A local donor motif from another semantic figure is not proof of learned reference whole-shape variation."}
            if ref["sourceId"] == "10157":
                # Root labels were inspected on the sourceID overlay and parent
                # explicitly resolved1as topcontour,3/4as internalfacets. This is
                # one pilot-specific review annotation, not a universal freeze.
                facets = {2, 3, 4, 5, 6}
                contour = {1, 7, 8, 9, 10, 11, 12}
                peripheral = {25, 27, 28, 31, 33}
                changed_pulls = {i for i in modified if parts[i]["kind"] == "slider"}
                guide_by_pull = {e["lockedOwnerId"]: e["guideOwnerId"] for e in c["guideLinks"]}
                affected_facets = {i for i in facets if i in modified or guide_by_pull.get(i) in modified}
                row["diamondPilotReviewAnnotation"] = {
                    "specialFamily": "user-confirmed intentional diamond",
                    "annotationScope": "Source10157 pilot only; preserve readable whole silhouette, not a universal immutable-contour policy",
                    "contourPullIds": sorted(contour), "interiorFacetPullIds": sorted(facets), "peripheralPullIds": sorted(peripheral),
                    "claimedModifiedInteriorFacetPullIds": sorted(changed_pulls & facets),
                    "claimedAffectedInteriorFacetGuideUnitIds": sorted(affected_facets),
                    "claimedModifiedContourPullIds": sorted(changed_pulls & contour),
                    "claimedModifiedPeripheralPullIds": sorted(changed_pulls & peripheral),
                    "relevantInteriorStructureStatus": "NO_INTERIOR_FACET_UNIT_EDIT_CLAIMED" if not affected_facets else "REQUIRES_ACTUAL_NATIVE_OBSTRUCTION_RELEASE_ORDER_COMPARISON",
                    "limits": "Claimed unit edits are not measured gameplay changes. Unchanged silhouette alone is not a rejection; decoration-only changes are not a new puzzle."}
            rows.append(row)
    report = {"schemaVersion": 1, "acceptedGeneratedCount": 0,
              "status": "READ_ONLY_WHOLE_MOTIF_REVIEW_FLAGS_NO_AUTOMATIC_LABEL_ACCEPTANCE_GATE",
              "candidateCount": len(rows), "observedMotifReviewCount": sum(r["specialWholeShapeReviewRequired"] for r in rows),
              "levels": rows, "hashes": {"inventory": digest(inventory_bytes), "packs": pack_hashes,
                                          "tool": digest(Path(__file__).read_bytes())},
              "limits": ["Named silhouettes are observed contact-sheet labels; inferred clusters remain separate and provisional.",
                         "Blank labels do not prove a reference lacks a special shape.",
                         "Field distance, 20percent edited roots and a changed static sweep cannot certify a relevant new puzzle.",
                         "Special whole-family preservation and meaningful interior obstruction/release order require dedicated visual/current-native evidence."]}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2), encoding="utf8")
    print("WHOLE_MOTIF_POSTSCREEN", len(rows), "candidates", report["observedMotifReviewCount"], "observedmotifreviews", flush=True)


if __name__ == "__main__":
    main()
