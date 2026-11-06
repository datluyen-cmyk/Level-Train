"""Extract unlinked Pull/Ring spatial relations from exact source collider outlines.

All emitted links are STATICcompositionproxy facts at captured source poses. They
describe outline proximity, source rail crossing, or convex-hull enclosure only;
they are not active Physics2D contacts, lock state, release order, or solve labels.
"""
from __future__ import annotations

from pathlib import Path
import hashlib
import json
import math
from typing import Any

from shapely import affinity, make_valid
from shapely.geometry import LineString, Point, Polygon
from shapely.ops import unary_union

ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / "Research/physics_v3/full500"
OUT = ROOT / "Research/physics_v3/source_physical_composition.json"
LABEL = "STATICcompositionproxy"
_BASE_GEOMETRY: dict[tuple[str, tuple[str, ...] | None], Any] = {}


def _read(path: Path) -> Any:
    return json.loads(path.read_text(encoding="utf-8"))


def _template_map(templates: Any) -> dict[str, dict]:
    if isinstance(templates, dict):
        return templates
    return {template["id"]: template for template in templates}


def _shape_geometry(shape: dict):
    """Build a source collider outline; boxes/polygons use serialized native vertices."""
    collider_type = shape.get("sourceColliderType", shape.get("shapeType", "PolygonCollider2D"))
    if collider_type in ("CircleCollider2D", "circle"):
        center = shape.get("center", {"x": 0.0, "y": 0.0})
        return Point(float(center["x"]), float(center["y"])).buffer(float(shape["radius"]), quad_segs=32)
    paths = shape.get("paths") or [{"points": shape.get("points", [])}]
    if not paths or len(paths[0].get("points", [])) < 3:
        return Polygon()
    exterior = [(float(p["x"]), float(p["y"])) for p in paths[0]["points"]]
    holes = [[(float(p["x"]), float(p["y"])) for p in path["points"]]
             for path in paths[1:] if len(path.get("points", [])) >= 3]
    geometry = Polygon(exterior, holes)
    return make_valid(geometry)


def _base_geometry(template: dict, roles: set[str] | None):
    key = (template["id"], tuple(sorted(roles)) if roles is not None else None)
    if key not in _BASE_GEOMETRY:
        shapes = []
        for shape in template.get("physicalShapes", []):
            if not shape.get("sourceEnabled", True) or shape.get("trigger", False):
                continue
            if roles is not None and shape.get("role") not in roles:
                continue
            geometry = _shape_geometry(shape)
            if not geometry.is_empty:
                shapes.append(geometry)
        _BASE_GEOMETRY[key] = unary_union(shapes) if shapes else Polygon()
    return _BASE_GEOMETRY[key]


def _posed_geometry(part: dict, template: dict, roles: set[str] | None = None):
    geometry = _base_geometry(template, roles)
    if geometry.is_empty:
        return geometry
    # Unity's Transform Euler z rotation is counter-clockwise in world XY.
    geometry = affinity.rotate(geometry, float(part.get("angle", 0.0)), origin=(0.0, 0.0))
    return affinity.translate(geometry, float(part.get("x", 0.0)), float(part.get("y", 0.0)))


def _pull_axis(part: dict, template: dict) -> tuple[float, float]:
    movement = template.get("movement") or {}
    axis = movement.get("axisLocal") or {"x": -1.0, "y": 0.0}
    x, y = float(axis.get("x", 0.0)), float(axis.get("y", 0.0))
    magnitude = math.hypot(x, y)
    if magnitude < 1e-8:
        return 0.0, 0.0
    x, y = x / magnitude, y / magnitude
    angle = math.radians(float(part.get("angle", 0.0)))
    return (x * math.cos(angle) - y * math.sin(angle),
            x * math.sin(angle) + y * math.cos(angle))


def _authored_pairs(level: dict) -> set[tuple[int, int]]:
    pairs = set()
    for link in level.get("links", []):
        pairs.add(tuple(sorted((int(link["source"]), int(link["target"])))) )
    for guide in level.get("guideLinks", []):
        pairs.add(tuple(sorted((int(guide["guideOwnerId"]), int(guide["lockedOwnerId"])))) )
    return pairs


def _width(part: dict, template: dict) -> float:
    return max(float(part.get("width", template.get("width", 0.0))), 0.0)


def composition_edges(level: dict, templates: Any) -> list[dict]:
    """Return unlinked slider/ring composition candidates with measured evidence.

    Strong relations:
      * pull_axis_crosses_ring: finite source Pull axis intersects the exact ring
        body outline or its convex hull at the captured pose.
      * pull_hull_contains_ring_center: the Pull body's convex hull contains the
        ring center, distinguishing enclosure/interleaving from generic nearness.

    Weak relation:
      * pull_near_ring: exact native solid-outline gap is at most 0.75 times the
        sum of source widths and the finite native Pull axis lies within one
        combined source width of the ring solid convex hull. It is not a contact
        or collision claim.
    """
    by_template = _template_map(templates)
    parts = {int(part["id"]): part for part in level.get("parts", [])}
    authored = _authored_pairs(level)
    rings = [part for part in parts.values() if part.get("kind") == "ring"]
    pulls = [part for part in parts.values() if part.get("kind") == "slider"]
    posed: dict[int, Any] = {}
    hulls: dict[int, Any] = {}
    for part in rings + pulls:
        template = by_template[part["templateId"]]
        roles = {"body"}
        posed[int(part["id"])] = _posed_geometry(part, template, roles)
        hulls[int(part["id"])] = posed[int(part["id"])].convex_hull

    relations = []
    for pull in pulls:
        pull_id = int(pull["id"])
        pull_template = by_template[pull["templateId"]]
        pull_solid = posed[pull_id]
        if pull_solid.is_empty:
            continue
        pull_hull = hulls[pull_id]
        axis_x, axis_y = _pull_axis(pull, pull_template)
        length = max(float(pull_template.get("length", 0.0)), 0.0)
        # Extend each end by the native half-width so the rail proxy covers the
        # center path through its end caps without turning into an infinite axis.
        half_span = length * 0.5 + _width(pull, pull_template) * 0.5
        x, y = float(pull.get("x", 0.0)), float(pull.get("y", 0.0))
        rail = LineString([(x - axis_x * half_span, y - axis_y * half_span),
                           (x + axis_x * half_span, y + axis_y * half_span)])
        for ring in rings:
            ring_id = int(ring["id"])
            if ring_id == pull_id or tuple(sorted((pull_id, ring_id))) in authored:
                continue
            ring_solid = posed[ring_id]
            if ring_solid.is_empty:
                continue
            ring_hull = hulls[ring_id]
            center = Point(float(ring.get("x", 0.0)), float(ring.get("y", 0.0)))
            solid_gap = float(pull_solid.distance(ring_solid))
            intersection_area = float(pull_solid.intersection(ring_solid).area)
            axis_solid_gap = float(rail.distance(ring_solid))
            axis_hull_gap = float(rail.distance(ring_hull))
            center_in_solid = bool(pull_solid.covers(center))
            center_in_hull = bool(pull_hull.covers(center))
            projection = ((float(ring["x"]) - x) * axis_x +
                          (float(ring["y"]) - y) * axis_y)
            width_sum = _width(pull, pull_template) + _width(ring, by_template[ring["templateId"]])
            near_threshold = max(0.0, 0.75 * width_sum)
            near_axis_threshold = width_sum
            near = solid_gap <= near_threshold + 1e-8 and axis_hull_gap <= near_axis_threshold + 1e-8
            axis_cross = axis_hull_gap <= 1e-8
            common = {
                "source": pull_id,
                "target": ring_id,
                "sourceId": pull.get("sourceId", str(pull_id)),
                "targetSourceId": ring.get("sourceId", str(ring_id)),
                "sourceKind": "slider",
                "targetKind": "ring",
                "linked": False,
                "evidenceLabel": LABEL,
                "evidence": {
                    "pullTemplateId": pull["templateId"],
                    "ringTemplateId": ring["templateId"],
                    "pullAxisWorld": [axis_x, axis_y],
                    "pullRailHalfSpan": half_span,
                    "ringCenterProjectionOnPullAxis": projection,
                    "exactSolidOutlineGap": solid_gap,
                    "pullAxisToRingSolidGap": axis_solid_gap,
                    "pullAxisToRingHullGap": axis_hull_gap,
                    "solidIntersectionArea": intersection_area,
                    "ringCenterInsidePullSolid": center_in_solid,
                    "ringCenterInsidePullConvexHull": center_in_hull,
                    "pullWidthPlusRingWidth": width_sum,
                    "weakNearThreshold": near_threshold,
                    "weakNearAxisToRingHullThreshold": near_axis_threshold,
                    "nativeColliderGeometry": "serialized exact PolygonCollider2D/BoxCollider2D outlines and holes; circles are polygonized at 32 segments per quadrant",
                },
            }
            if axis_cross:
                relations.append({**common, "kind": "pull_axis_crosses_ring", "strong": True, "weight": 2.0})
            if center_in_hull:
                relations.append({**common, "kind": "pull_hull_contains_ring_center", "strong": True, "weight": 2.0})
            if near:
                relations.append({**common, "kind": "pull_near_ring", "strong": False, "weight": 0.5})
    return relations


def graph_for_composition(level: dict, templates: Any):
    """Return a NetworkX composition graph containing only unlinked proxy edges."""
    import networkx as nx
    graph = nx.Graph()
    for part in level.get("parts", []):
        graph.add_node(int(part["id"]), sourceId=part.get("sourceId"), kind=part.get("kind"))
    for edge in composition_edges(level, templates):
        source, target = edge["source"], edge["target"]
        if graph.has_edge(source, target):
            data = graph[source][target]
            data["kinds"].append(edge["kind"])
            data["strong"] = data["strong"] or edge["strong"]
            data["weight"] = max(data["weight"], edge["weight"])
            data["evidence"].append(edge["evidence"])
        else:
            graph.add_edge(source, target, kind=edge["kind"], kinds=[edge["kind"]], weight=edge["weight"],
                           strong=edge["strong"], evidenceLabel=edge["evidenceLabel"], evidence=[edge["evidence"]])
    return graph


def _sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def build_report(pack_path: Path = DATA / "source_parity_pack.json",
                 template_path: Path = DATA / "source_templates.json") -> dict:
    pack, raw_templates = _read(pack_path), _read(template_path)["templates"]
    templates = _template_map(raw_templates)
    records = []
    totals = {"pull_axis_crosses_ring": 0, "pull_hull_contains_ring_center": 0, "pull_near_ring": 0}
    levels_with = {key: 0 for key in totals}
    examples_10010 = []
    for index, level in enumerate(pack["levels"], start=1):
        edges = composition_edges(level, templates)
        per_kind = {key: [edge for edge in edges if edge["kind"] == key] for key in totals}
        for key, values in per_kind.items():
            totals[key] += len(values)
            levels_with[key] += bool(values)
        record = {
            "sourceId": level.get("sourceId", str(level.get("id"))),
            "levelId": int(level["id"]),
            "sourceFamily": level.get("sourceFamily"),
            "relations": edges,
        }
        records.append(record)
        if str(level.get("sourceId", level.get("id"))) == "10010":
            examples_10010 = edges
        if index % 100 == 0:
            print(f"COMPOSITION {index}/500", flush=True)
    return {
        "schemaVersion": 1,
        "sourceCount": len(records),
        "sourcePackSha256": _sha256(pack_path),
        "templatePackSha256": _sha256(template_path),
        "status": "STATICcompositionproxy; source-pose geometric candidate relations only; not active physics, solver truth, or source solution labels",
        "criteria": {
            "scope": "Unlinked source sliders against ring body outlines at each captured source pose; authored ring and guide links are omitted from these proxy relations.",
            "exactGeometry": "Polygon and box colliders use native serialized paths/vertices, including holes; circles use a 128-segment Shapely polygon approximation.",
            "pull_axis_crosses_ring": "Finite pull axis extends half the native Pull length plus half-width on both ends; strong when it intersects the exact ring solid's convex hull at source pose.",
            "pull_hull_contains_ring_center": "Strong enclosure/interleaving candidate when the convex hull of exact Pull solid outlines covers the ring center.",
            "pull_near_ring": "Weak proximity candidate when exact solid-outline gap <= 0.75 * combined source widths and finite pull-axis to ring-convex-hull gap <= one combined source width; proximity is not contact/blocking proof.",
            "authoredEdges": "Only unlinked pairs are reported. The graph is composition-only and does not replace or infer native pin/guide edges.",
            "forbiddenInference": ["active Physics2D contact", "lock or binding state", "motion outcome", "release order", "solvability", "difficulty", "accepted generated layout"],
        },
        "summary": {"relationCounts": totals, "levelsWithRelations": levels_with},
        "source10010": {"sourceId": "10010", "relations": examples_10010},
        "levels": records,
    }


def main() -> None:
    report = build_report()
    OUT.write_text(json.dumps(report, separators=(",", ":"), ensure_ascii=False), encoding="utf-8")
    print(json.dumps({"sourceCount": report["sourceCount"], "summary": report["summary"],
                      "source10010RelationCount": len(report["source10010"]["relations"]),
                      "output": str(OUT), "bytes": OUT.stat().st_size}, indent=2))


if __name__ == "__main__":
    main()
