"""Source-relative visual, mechanic and difficulty evaluation for generated levels.

This module is intentionally dependency-light.  It evaluates a generated DTO
against the source level at the same progression position, so a candidate is
judged on the same three things a human reviewer sees:

* visual layout: span, density, orientation, local neighbourhood and symmetry;
* puzzle archetype: part/edge inventory, guide topology, locks, gaps and
  slider/ring interlock;
* difficulty: static difficulty proxy, optionally replaced/augmented by native
  Unity bot measurements.

The scores are *relative errors* (zero is a perfect source-relative match), not
claims that a static geometry analysis proves a level solvable.  Native replay
is still the authority for actual playability.
"""
from __future__ import annotations

import math
from collections import Counter, deque
from typing import Any


def _f(value: Any, default: float = 0.0) -> float:
    try:
        value = float(value)
        return value if math.isfinite(value) else default
    except (TypeError, ValueError):
        return default


def _angle(value: float) -> float:
    return (value + 180.0) % 360.0 - 180.0


def _part_unit(part: dict, templates: dict | None = None) -> float:
    data = part.get("sourcePartData") or {}
    template = (templates or {}).get(str(part.get("templateId")), {}) if templates else {}
    # Generated DTOs intentionally drop large source diagnostic blobs. Prefer
    # the shared native template dimensions so source and candidate use the
    # same visual scale even when only one side still has sourcePartData.
    template_unit = max(_f(template.get("radius"), 0.0),
                        _f(template.get("width"), 0.0),
                        _f(template.get("length"), 0.0) / 2.0)
    return max(1.0, template_unit, _f(data.get("OuterRadius"), 24.0) / 24.0,
               _f(part.get("width"), 0.0), _f(part.get("length"), 0.0) / 2.0)


def _roots(level: dict) -> list[dict]:
    return [p for p in level.get("parts", []) if p.get("kind") in ("ring", "slider")]


def _layout_features(level: dict, templates: dict | None = None) -> dict[str, float]:
    roots = _roots(level)
    if not roots:
        return {"rootCount": 0.0, "spanX": 0.0, "spanY": 0.0, "density": 0.0,
                "orientationDispersion": 0.0, "nearestMean": 0.0,
                "nearestStd": 0.0, "spacingCv": 0.0, "clusterCount": 0.0,
                "aspectRatio": 0.0, "symmetry": 0.0, "sliderFraction": 0.0}
    xs = [_f(p.get("x")) for p in roots]
    ys = [_f(p.get("y")) for p in roots]
    mx, my = sum(xs) / len(xs), sum(ys) / len(ys)
    unit = max(1.0, sum(_part_unit(p, templates) for p in roots) / len(roots))
    dx = [(x - mx) / unit for x in xs]
    dy = [(y - my) / unit for y in ys]
    span_x = max(dx) - min(dx)
    span_y = max(dy) - min(dy)
    # Principal axis is computed without numpy to keep the generator worker
    # cheap and deterministic.
    cxx = sum(x * x for x in dx) / len(dx)
    cyy = sum(y * y for y in dy) / len(dy)
    cxy = sum(x * y for x, y in zip(dx, dy)) / len(dx)
    principal = 0.5 * math.atan2(2.0 * cxy, cxx - cyy) if abs(cxy) + abs(cxx - cyy) > 1e-9 else 0.0
    cp, sp = math.cos(principal), math.sin(principal)
    long_axis = [x * cp + y * sp for x, y in zip(dx, dy)]
    short_axis = [-x * sp + y * cp for x, y in zip(dx, dy)]
    principal_span = max(long_axis) - min(long_axis)
    short_span = max(short_axis) - min(short_axis)
    area = max(1.0, principal_span * max(short_span, 1.0))
    density = len(roots) / area
    angles = [_f(p.get("angle")) for p in roots]
    orientation_dispersion = 1.0 - abs(sum(complex(math.cos(math.radians(a)), math.sin(math.radians(a))) for a in angles) / len(angles))
    nearest = []
    for i, (x, y) in enumerate(zip(dx, dy)):
        distances = [math.hypot(x - dx[j], y - dy[j]) for j in range(len(roots)) if i != j]
        if distances:
            nearest.append(min(distances))
    nearest_mean = sum(nearest) / len(nearest) if nearest else 0.0
    nearest_std = ((sum((v - nearest_mean) ** 2 for v in nearest) / len(nearest)) ** 0.5
                   if nearest else 0.0)
    # Spatial relation coverage catches the key failure mode of generated
    # boards: rings can have matching inventory yet belong to no meaningful
    # local puzzle group.  A pair is related when it is in the same compact
    # pocket or a slider's finite movement axis reaches a ring-sized
    # neighbourhood.  This is a layout prior only; native Unity remains the
    # authority for actual blocking and solvability.
    related = [set() for _ in roots]
    for i, left in enumerate(roots):
        for j in range(i):
            right = roots[j]
            ddx, ddy = dx[i] - dx[j], dy[i] - dy[j]
            threshold = 2.4 + 0.35 * (_part_unit(left, templates) + _part_unit(right, templates)) / max(1.0, unit)
            close = math.hypot(ddx, ddy) <= threshold
            slider, ring = (left, right) if left.get("kind") == "slider" and right.get("kind") == "ring" else ((right, left) if right.get("kind") == "slider" and left.get("kind") == "ring" else (None, None))
            axis_near = False
            if slider is not None:
                theta = math.radians(_f(slider.get("angle")))
                ax, ay = math.cos(theta), math.sin(theta)
                vx, vy = _f(ring.get("x")) - _f(slider.get("x")), _f(ring.get("y")) - _f(slider.get("y"))
                along, across = abs(vx * ax + vy * ay), abs(-vx * ay + vy * ax)
                axis_near = across <= 1.5 * threshold and along <= max(3.0 * unit, _f(slider.get("length"), 0.0) + threshold)
            if close or axis_near:
                related[i].add(j); related[j].add(i)
    isolated_fraction = sum(not neighbours for neighbours in related) / max(1, len(roots))
    relation_component_parent = list(range(len(roots)))
    def relation_find(i):
        while relation_component_parent[i] != i:
            relation_component_parent[i] = relation_component_parent[relation_component_parent[i]]
            i = relation_component_parent[i]
        return i
    def relation_union(a, b):
        ra, rb = relation_find(a), relation_find(b)
        if ra != rb: relation_component_parent[rb] = ra
    for i, neighbours in enumerate(related):
        for j in neighbours: relation_union(i, j)
    relation_components = len({relation_find(i) for i in range(len(roots))})
    # A source-like board is usually one coherent playfield with a few local
    # pockets.  Counting connected spatial pockets catches the generated-board
    # failure mode where otherwise valid parts drift into unrelated islands.
    cluster_parent = list(range(len(roots)))
    def find(i):
        while cluster_parent[i] != i:
            cluster_parent[i] = cluster_parent[cluster_parent[i]]
            i = cluster_parent[i]
        return i
    def union(a, b):
        ra, rb = find(a), find(b)
        if ra != rb: cluster_parent[rb] = ra
    for i in range(len(roots)):
        for j in range(i):
            if math.hypot(dx[i] - dx[j], dy[i] - dy[j]) <= 3.25:
                union(i, j)
    cluster_count = len({find(i) for i in range(len(roots))})
    # Reflection error across the principal axis.  This captures intentional
    # balance without requiring a copied coordinate scaffold.
    symmetry_errors = []
    for x, y in zip(long_axis, short_axis):
        symmetry_errors.append(min(math.hypot(x - x2, y + y2)
                                   for x2, y2 in zip(long_axis, short_axis)))
    guides = level.get("guideLinks", [])
    local_guide_offsets = []
    by_id = {int(p.get("id")): p for p in level.get("parts", []) if p.get("id") is not None}
    for guide in guides:
        fixed = by_id.get(int(guide.get("guideOwnerId", -1)))
        locked = by_id.get(int(guide.get("lockedOwnerId", -1)))
        if fixed and locked:
            vx = _f(fixed.get("x")) - _f(locked.get("x"))
            vy = _f(fixed.get("y")) - _f(locked.get("y"))
            theta = math.radians(_f(locked.get("angle")))
            local_guide_offsets.append((vx * math.cos(theta) + vy * math.sin(theta)) / unit)
    return {
        "rootCount": float(len(roots)), "spanX": float(span_x), "spanY": float(span_y),
        "principalSpan": float(principal_span), "shortSpan": float(short_span),
        "density": float(density), "orientationDispersion": float(orientation_dispersion),
        "nearestMean": float(nearest_mean),
        "nearestStd": float(nearest_std),
        "spacingCv": float(nearest_std / max(1.0, nearest_mean)),
        "clusterCount": float(cluster_count),
        "relationComponents": float(relation_components),
        "isolatedRootFraction": float(isolated_fraction),
        "aspectRatio": float(short_span / max(1.0, principal_span)),
        "symmetry": float(sum(symmetry_errors) / len(symmetry_errors)) if symmetry_errors else 0.0,
        "sliderFraction": sum(p.get("kind") == "slider" for p in roots) / len(roots),
        "guideOffsetMean": sum(local_guide_offsets) / len(local_guide_offsets) if local_guide_offsets else 0.0,
        "guideOffsetStd": (sum((v - sum(local_guide_offsets) / len(local_guide_offsets)) ** 2 for v in local_guide_offsets) / len(local_guide_offsets)) ** 0.5 if local_guide_offsets else 0.0,
    }


def _mechanic_features(level: dict, templates: dict | None = None) -> dict[str, float]:
    parts = level.get("parts", [])
    roots = _roots(level)
    kinds = Counter(p.get("kind") for p in parts)
    rings = [p for p in roots if p.get("kind") == "ring"]
    links = level.get("links", [])
    guides = level.get("guideLinks", [])
    bombs = level.get("bombs", [])
    chain_total = sum(max(0, int(_f(p.get("chainLockCount")))) for p in roots)
    gaps = []
    for p in rings:
        data = p.get("sourcePartData") or {}
        if "GapAngleDeg" in data:
            gaps.append(_f(data.get("GapAngleDeg")))
        elif templates:
            gaps.append(_f((templates.get(str(p.get("templateId"))) or {}).get("gap"),
                           _f((templates.get(str(p.get("templateId"))) or {}).get("visualGap"))))
    pin_counts = []
    for p in rings:
        bindings = p.get("sourceColliderBindings") or []
        pin_counts.append(sum(str(b.get("sourceRole", "")).lower().find("pin") >= 0 for b in bindings))
    # Authored dependency graph topology is a puzzle-archetype signal.  It is
    # deliberately only a signature; it is not treated as a solve order.
    nodes = {int(p["id"]): set() for p in parts if p.get("id") is not None}
    for edge in links:
        try:
            nodes[int(edge["source"])].add(int(edge["target"]))
        except (KeyError, TypeError, ValueError):
            pass
    for edge in guides:
        try:
            nodes[int(edge["guideOwnerId"])].add(int(edge["lockedOwnerId"]))
        except (KeyError, TypeError, ValueError):
            pass
    branch = sum(len(v) > 1 for v in nodes.values())
    indegree = Counter(v for values in nodes.values() for v in values)
    merge = sum(v > 1 for v in indegree.values())
    visited = set(); components = 0
    undirected = {n: set() for n in nodes}
    for a, values in nodes.items():
        for b in values:
            undirected.setdefault(b, set()).add(a); undirected[a].add(b)
    for n in undirected:
        if n in visited: continue
        components += 1; q = deque([n]); visited.add(n)
        while q:
            cur = q.popleft()
            for nxt in undirected[cur]:
                if nxt not in visited: visited.add(nxt); q.append(nxt)
    # Static interlock proxy is intentionally a cheap approximation.  The
    # native Unity bot remains responsible for physical truth.
    strong, weak = 0, 0
    for slider in [p for p in roots if p.get("kind") == "slider"]:
        sx, sy = _f(slider.get("x")), _f(slider.get("y"))
        angle = math.radians(_f(slider.get("angle")))
        ax, ay = math.cos(angle), math.sin(angle)
        for ring in rings:
            dx, dy = _f(ring.get("x")) - sx, _f(ring.get("y")) - sy
            along = abs(dx * ax + dy * ay)
            across = abs(-dx * ay + dy * ax)
            threshold = _part_unit(slider, templates) + _part_unit(ring, templates)
            if across <= threshold and along <= max(2.0 * threshold, _f(slider.get("length"), 0.0) + threshold):
                strong += 1
            elif math.hypot(dx, dy) <= 2.0 * threshold:
                weak += 1
    return {
        "partCount": float(len(parts)), "ringCount": float(kinds.get("ring", 0)),
        "sliderCount": float(kinds.get("slider", 0)), "fixedCount": float(kinds.get("fixed", 0)),
        "butterflyCount": float(kinds.get("butterfly", 0)), "linkCount": float(len(links)),
        "guideCount": float(len(guides)), "bombCount": float(len(bombs)),
        "chainLockTotal": float(chain_total), "ringGapMean": sum(gaps) / len(gaps) if gaps else 0.0,
        "ringGapStd": (sum((g - sum(gaps) / len(gaps)) ** 2 for g in gaps) / len(gaps)) ** 0.5 if gaps else 0.0,
        "pinMean": sum(pin_counts) / len(pin_counts) if pin_counts else 0.0,
        "branchNodes": float(branch), "mergeNodes": float(merge), "dependencyComponents": float(components),
        "interlockStrong": float(strong), "interlockWeak": float(weak),
        "guidePerRoot": len(guides) / max(1, len(roots)),
        "linkPerRoot": len(links) / max(1, len(roots)),
    }


def puzzle_archetypes(level: dict, templates: dict | None = None) -> list[str]:
    """Human-readable mechanic families used in reports and ranking."""
    features = _mechanic_features(level, templates)
    labels = []
    if features["guideCount"] and features["sliderCount"]:
        labels.append("guided_slider")
    if features["interlockStrong"]:
        labels.append("pull_ring_interlock")
    if features["chainLockTotal"]:
        labels.append("chain_lock")
    if features["bombCount"]:
        labels.append("bomb_hazard")
    if features["branchNodes"]:
        labels.append("branch_dependency")
    if features["mergeNodes"]:
        labels.append("merge_dependency")
    if features["dependencyComponents"] > 1:
        labels.append("multi_cluster")
    if not labels:
        labels.append("open_ring_arrangement")
    return labels


VISUAL_KEYS = ("spanX", "spanY", "principalSpan", "shortSpan", "density", "orientationDispersion",
               "nearestMean", "nearestStd", "spacingCv", "clusterCount", "aspectRatio",
               "relationComponents", "isolatedRootFraction", "symmetry", "sliderFraction", "guideOffsetMean", "guideOffsetStd")
MECHANIC_KEYS = ("partCount", "ringCount", "sliderCount", "fixedCount", "butterflyCount", "linkCount",
                 "guideCount", "bombCount", "chainLockTotal", "ringGapMean", "ringGapStd", "pinMean",
                 "branchNodes", "mergeNodes", "dependencyComponents", "interlockStrong", "interlockWeak",
                 "guidePerRoot", "linkPerRoot")


def _relative_error(a: float, b: float, floor: float = 1.0) -> float:
    return abs(a - b) / max(floor, abs(b), 1.0)


def _trace_shape_error(candidate: dict, source_trace: dict | None) -> tuple[float, dict]:
    """Compare cheap candidate proxies with a verified Unity turn signature.

    This is deliberately a ranking signal.  It never labels a generated board
    solvable; native replay remains the gate for that claim.
    """
    if not source_trace or not source_trace.get("verified"):
        return 0.0, {"status": "TRACE_PROFILE_MISSING_OR_UNVERIFIED"}
    mechanics = _mechanic_features(candidate)
    roots = max(1.0, mechanics["ringCount"] + mechanics["sliderCount"])
    steps = max(1.0, _f(source_trace.get("stepCount"), 1.0))
    source_slide_fraction = _f(source_trace.get("slideSteps")) / steps
    source_burst = max(1.0, _f(source_trace.get("meanRemovalBurst"), 1.0))
    source_binding_release = _f(source_trace.get("unlockDeltaTotal")) / max(
        1.0, _f(source_trace.get("activeBindingStart"), 1.0))
    candidate_slide_fraction = mechanics["sliderCount"] / roots
    candidate_burst = 1.0 + 0.25 * (mechanics["linkCount"] + mechanics["guideCount"] +
                                     mechanics["chainLockTotal"]) / roots
    candidate_binding_release = (mechanics["linkCount"] + mechanics["guideCount"] +
                                mechanics["chainLockTotal"]) / max(1.0, mechanics["partCount"])
    error = (_relative_error(candidate_slide_fraction, source_slide_fraction, 0.15) +
             _relative_error(candidate_burst, source_burst, 1.0) +
             _relative_error(candidate_binding_release, source_binding_release, 0.10)) / 3.0
    return float(error), {
        "status": "VERIFIED_UNITY_TURN_PROFILE",
        "sourceSlideFraction": source_slide_fraction,
        "candidateSlideFraction": candidate_slide_fraction,
        "sourceMeanRemovalBurst": source_burst,
        "candidateCascadeProxy": candidate_burst,
        "sourceBindingRelease": source_binding_release,
        "candidateBindingReleaseProxy": candidate_binding_release,
    }


def static_quality(candidate: dict, reference: dict, templates: dict | None = None,
                   source_trace: dict | None = None) -> dict:
    """Return source-relative visual/mechanic/difficulty scores.

    ``visualError`` and ``puzzleError`` are bounded to make them safe to mix
    with the generator's inventory score. ``difficultyProxyError`` compares a
    coarse static pressure signature and must be confirmed with native replay.
    """
    cv, rv = _layout_features(candidate, templates), _layout_features(reference, templates)
    cm, rm = _mechanic_features(candidate, templates), _mechanic_features(reference, templates)
    visual = sum(_relative_error(cv[k], rv[k], 1.0) for k in VISUAL_KEYS) / len(VISUAL_KEYS)
    # Inventory and topology are weighted more than absolute gap degrees: they
    # define the puzzle family a player experiences.
    mechanic_weights = {k: (2.0 if k in ("linkCount", "guideCount", "interlockStrong", "chainLockTotal") else 1.0) for k in MECHANIC_KEYS}
    candidate_archetypes = puzzle_archetypes(candidate, templates)
    reference_archetypes = puzzle_archetypes(reference, templates)
    archetype_mismatch = 0.0 if set(candidate_archetypes) == set(reference_archetypes) else 1.0
    mechanic = (sum(mechanic_weights[k] * _relative_error(cm[k], rm[k], 1.0) for k in MECHANIC_KEYS) /
                sum(mechanic_weights.values()) + 0.8 * archetype_mismatch) / 1.8
    pressure_keys = ("partCount", "linkCount", "guideCount", "chainLockTotal", "interlockStrong", "interlockWeak", "bombCount")
    difficulty = sum(_relative_error(cm[k], rm[k], 1.0) for k in pressure_keys) / len(pressure_keys)
    trace_error, trace_profile = _trace_shape_error(candidate, source_trace)
    # Keep detailed vectors in the artifact for diagnosis and future retraining.
    return {"visualError": float(visual), "puzzleError": float(mechanic),
            "archetypeMismatch": float(archetype_mismatch),
            "candidateArchetypes": candidate_archetypes, "referenceArchetypes": reference_archetypes,
            "difficultyProxyError": float(difficulty), "traceShapeError": float(trace_error),
            "traceProfile": trace_profile,
            "staticQualityScore": float(visual + 1.5 * mechanic + difficulty + 0.75 * trace_error),
            "candidateVisual": cv, "referenceVisual": rv,
            "candidateMechanics": cm, "referenceMechanics": rm,
            "nativeDifficulty": None,
            "status": "STATIC_SOURCE_RELATIVE_PENDING_NATIVE_REPLAY"}


def native_difficulty_quality(candidate_row: dict | None, reference_row: dict | None) -> dict:
    """Compare Unity virtual-player rows when available."""
    c, r = candidate_row or {}, reference_row or {}
    def q(row, key): return _f((row.get(key) or {}).get("p50"), 0.0)
    cw, rw = _f(c.get("winRateExcludingEngineErrors")), _f(r.get("winRateExcludingEngineErrors"))
    cb = _f(c.get("physicalBlocks")) / max(1.0, _f(c.get("sessions"), 1.0))
    rb = _f(r.get("physicalBlocks")) / max(1.0, _f(r.get("sessions"), 1.0))
    return {"winRateDelta": cw - rw, "actionP50Delta": q(c, "actionCountAllSessionQuantiles") - q(r, "actionCountAllSessionQuantiles"),
            "blockLogRatio": math.log((cb + 0.5) / (rb + 0.5)),
            "candidateWinRate": cw, "referenceWinRate": rw,
            "status": "NATIVE_UNITY_REPLAY" if candidate_row and reference_row else "NATIVE_REPLAY_MISSING"}


def compare(candidate: dict, reference: dict, templates: dict | None = None,
            candidate_row: dict | None = None, reference_row: dict | None = None,
            source_trace: dict | None = None) -> dict:
    result = static_quality(candidate, reference, templates, source_trace)
    native = native_difficulty_quality(candidate_row, reference_row)
    result["nativeDifficulty"] = native
    # Native difficulty is informative when available, but static visual and
    # puzzle identity remain the primary ranking signals.
    result["qualityScore"] = result["staticQualityScore"] + (0.35 * abs(native["blockLogRatio"]) + 0.25 * abs(native["winRateDelta"]) if native["status"] == "NATIVE_UNITY_REPLAY" else 0.0)
    result["status"] = "SOURCE_RELATIVE_NATIVE_AND_STATIC" if native["status"] == "NATIVE_UNITY_REPLAY" else result["status"]
    return result
