"""Conservative geometric/input orientation semantics for proposal counting.

Native asset audits still compare complete native identities. This module only
prevents orientation-equivalent parts from inflating novelty or edited-root counts.
"""
import json
import math


def _points(shape):
    return [path.get("points", []) for path in shape.get("paths", [{"points": shape.get("points", [])}])]


def _centered_round(shape):
    kind = shape.get("sourceColliderType", shape.get("shapeType", ""))
    if kind in ("CircleCollider2D", "circle"):
        center = shape.get("center", {"x": 0., "y": 0.})
        return math.hypot(center.get("x", 0.), center.get("y", 0.)) < 1e-6
    paths = _points(shape)
    if not paths or any(len(points) < 12 for points in paths):
        return False
    for points in paths:
        radii = [math.hypot(p["x"], p["y"]) for p in points]
        radius = max(radii)
        if radius < 1e-6 or max(radii)-min(radii) > max(1e-5, radius*1e-5):
            return False
        angles = sorted(math.atan2(p["y"], p["x"]) % (2*math.pi) for p in points)
        largest_gap = max(b-a for a, b in zip(angles, angles[1:]+[angles[0]+2*math.pi]))
        if largest_gap > math.pi/3:
            return False
    return True


def _half_turn_symmetric(shape):
    kind = shape.get("sourceColliderType", shape.get("shapeType", ""))
    if kind in ("CircleCollider2D", "circle"):
        return _centered_round(shape)
    paths = _points(shape)
    if not paths or any(not points for points in paths):
        return False
    # Point-set symmetry is conservative for simple capsule/polygon outlines.
    # Require the transformed loop also to have the same cyclic ordering.
    for points in paths:
        n = len(points)
        match = False
        for offset in range(n):
            if all(math.hypot(points[i]["x"]+points[(i+offset) % n]["x"],
                              points[i]["y"]+points[(i+offset) % n]["y"]) <= 2e-5
                   for i in range(n)):
                match = True
                break
        if not match:
            return False
    return True


def orientation_order(part, template):
    """0: free round angle;2: half turn;1: full directional orientation.

    Full-circle polygon tessellation is treated as the same round receiver;
    tiny authored facet differences are not a new puzzle. Any C-gap, native
    stem/head, nonround input area, directional lock or bent rail keeps an axis.
    """
    shapes = [s for s in template.get("physicalShapes", []) if s.get("sourceEnabled", True)]
    inputs = [s for s in part.get("inputShapes", []) if s.get("sourceEnabled", True)]
    if not shapes or not inputs:
        return 1
    ordinary = not (part.get("chainLockCount", 0) or part.get("key") or part.get("lockKey"))
    if (ordinary and part.get("kind") == "ring" and abs(template.get("gap", 1)) < 1e-7
            and abs(template.get("visualGap", 1)) < 1e-7 and not template.get("attachments")
            and all(s.get("role") == "body" for s in shapes)
            and all(_centered_round(s) for s in shapes+inputs)):
        return 0
    if ordinary and part.get("kind") == "slider" and all(s.get("role") == "body" for s in shapes):
        path = template.get("pullVisual", {}).get("pathPoints", [])
        movement = template.get("movement", {})
        two_way = (movement.get("lockLeft", False) == movement.get("lockRight", False)
                   and movement.get("lockUp", False) == movement.get("lockDown", False))
        straight_centered = (len(path) == 2
            and math.hypot(path[0]["x"]+path[1]["x"], path[0]["y"]+path[1]["y"]) < 2e-5)
        if two_way and straight_centered and all(_half_turn_symmetric(s) for s in shapes+inputs):
            return 2
    return 1


def angle_error_degrees(a, b, order_a=1, order_b=1):
    if order_a == 0 or order_b == 0:
        return 0.
    period = 180. if order_a == order_b == 2 else 360.
    return abs((a-b+period/2) % period-period/2)


def _clean_contract(value):
    if isinstance(value, dict):
        return {k: _clean_contract(v) for k, v in value.items()
                if k not in ("id", "sourceColliderId", "sourceComponentId", "sourceName", "attachmentId")}
    if isinstance(value, list):
        return [_clean_contract(v) for v in value]
    return value


def effective_local_contract(part, template):
    shapes = [_clean_contract(s) for s in template.get("physicalShapes", [])]
    inputs = [_clean_contract(s) for s in part.get("inputShapes", [])]
    # RGB tint, mesh style/source IDs and donor IDs do not create mechanics.
    value = {"kind": part["kind"], "shapes": sorted(shapes, key=lambda s: json.dumps(s, sort_keys=True)),
             "inputs": sorted(inputs, key=lambda s: json.dumps(s, sort_keys=True)),
             "movement": template.get("movement"),
             "state": {k: part.get(k) for k in ("z", "chainLockCount", "collectible", "scoreEligible", "key", "lockKey")}}
    return json.dumps(value, sort_keys=True, separators=(",", ":"))


def meaningful_edit(original, actual, templates, old_targets=(), new_targets=()):
    if set(old_targets) != set(new_targets):
        return True
    if math.hypot(original["x"]-actual["x"], original["y"]-actual["y"]) > 1e-4:
        return True
    a, b = templates[original["templateId"]], templates[actual["templateId"]]
    if effective_local_contract(original, a) != effective_local_contract(actual, b):
        return True
    return angle_error_degrees(original["angle"], actual["angle"],
                               orientation_order(original, a), orientation_order(actual, b)) > .005
