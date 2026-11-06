"""Exact saved transform reuse diagnostic; no donor quotas or content cutoffs."""
import math
import numpy as np


def same_transform(a, b):
    angle = (float(a["rotation"])-float(b["rotation"])+180.)%360.-180.
    av, bv = np.asarray(a["translation"], float), np.asarray(b["translation"], float)
    scale = max(1., float(np.max(np.abs(np.r_[av, bv]))), abs(float(a["rotation"])), abs(float(b["rotation"])))
    tolerance = 32*np.finfo(float).eps*scale
    return abs(angle)<=tolerance and bool(np.max(np.abs(av-bv))<=tolerance)


def rigid_source_reuse(level):
    imports = level.get("assemblyProvenance", {}).get("tileImports", [])
    groups = []
    for index, item in enumerate(imports):
        group = next((g for g in groups if g["sourceId"]==item["sourceId"] and same_transform(g["frame"], item)), None)
        if group is None:
            group = {"sourceId": item["sourceId"], "frame": item, "importIndices": [], "rootIds": set()}
            groups.append(group)
        group["importIndices"].append(index)
        group["rootIds"].update(int(i) for i in item["partMapping"].values())
    n = max(1, len(level["parts"]))
    rows = [{"sourceId": g["sourceId"], "rotation": g["frame"]["rotation"], "translation": g["frame"]["translation"],
        "importIndices": g["importIndices"], "candidateRootCount": len(g["rootIds"]), "candidateRootFraction": len(g["rootIds"])/n}
        for g in groups]
    multi = [g for g in rows if len(g["importIndices"])>1]
    fraction = max((g["candidateRootFraction"] for g in multi), default=0.)
    return {"rigidSourceGroups": rows, "largestMultiImportRigidSourceRootFraction": fraction,
        "softSourceReconstructionCost": -math.log1p(-fraction) if fraction<1. else None,
        "entireCandidateIsExactMultiImportSourceSubset": fraction==1.,
        "requiresPartialSourceOriginalityReview": bool(multi),
        "meaning": "Multiple native motifs placed with identical source transform restore an exact rigid source subset; evidence is provenance-based and must be independently checked.",
        "numericDonorQuotaApplied": False}
