"""Labeled synthetic invariance regressions for semantic orientation handling."""
import copy
import math
import unittest

from orientation_semantics_v3 import orientation_order, meaningful_edit
from layout_novelty_semantic_v3 import describe, compare, canonical_fingerprint, CRITERIA
from layout_novelty_v3 import CRITERIA as HISTORICAL_CRITERIA


def shape(points, role="body"):
    return {"role": role, "trigger": False, "sourceEnabled": True,
            "points": [{"x": x, "y": y} for x, y in points]}


def circle(radius=2.):
    return shape([(radius*math.cos(i*2*math.pi/64), radius*math.sin(i*2*math.pi/64)) for i in range(64)])


def fixture(kind, name):
    if name == "round":
        template = {"kind": kind, "gap": 0., "visualGap": 0., "radius": 2., "width": .5,
                    "attachments": [], "physicalShapes": [circle()]}
        input_shape = circle(2.5)
    elif name == "C":
        points = [(r*math.cos(a), r*math.sin(a)) for r, angles in
                  [(2., [math.radians(45+i*270/48) for i in range(49)]),
                   (1.5, [math.radians(315-i*270/48) for i in range(49)])] for a in angles]
        template = {"kind": kind, "gap": 90., "visualGap": 90., "radius": 2., "width": .5,
                    "attachments": [], "physicalShapes": [shape(points)]}
        input_shape = shape(points)
    else:
        paths = {"straight": [(-3, 0), (3, 0)], "L": [(-3, 0), (2, 0), (2, 3)],
                 "T": [(-3, 0), (3, 0), (0, 0), (0, 3)], "U": [(-2, 3), (-2, 0), (2, 0), (2, 3)]}
        outlines = {"straight": [(-3, -.3), (3, -.3), (3, .3), (-3, .3)],
                    "L": [(-3, -.3), (2.3, -.3), (2.3, 3), (1.7, 3), (1.7, .3), (-3, .3)],
                    "T": [(-3, -.3), (3, -.3), (3, .3), (.3, .3), (.3, 3), (-.3, 3), (-.3, .3), (-3, .3)],
                    "U": [(-2.3, 3), (-2.3, -.3), (2.3, -.3), (2.3, 3), (1.7, 3), (1.7, .3), (-1.7, .3), (-1.7, 3)]}
        template = {"kind": kind, "gap": 0., "length": 6., "width": .6,
                    "attachments": [], "physicalShapes": [shape(outlines[name])],
                    "pullVisual": {"pathPoints": [{"x": x, "y": y} for x, y in paths[name]]},
                    "movement": {"axisLocal": {"x": -1., "y": 0.}, "lockLeft": False, "lockRight": False,
                                 "lockUp": True, "lockDown": True}}
        input_shape = shape(outlines[name])
    template["id"] = name
    part = {"id": 1, "templateId": name, "kind": kind, "x": 0., "y": 0., "angle": 0.,
            "inputShapes": [input_shape], "sourceRigidbody": {"present": True}, "z": 0., "chainLockCount": 0}
    return part, template


def level(part):
    positions = [(0., 0.), (17., 1.), (4., 31.), (-9., 8.)]
    return {"sourceId": "synthetic-invariance-only", "parts": [{**copy.deepcopy(part), "id": i+1, "x": x, "y": y}
                                                               for i, (x, y) in enumerate(positions)]}


class OrientationInvariance(unittest.TestCase):
    def test_thresholds_unchanged(self):
        self.assertEqual(CRITERIA, HISTORICAL_CRITERIA)

    def test_round_receiver_arbitrary_rotation_is_equivalent(self):
        part, template = fixture("ring", "round")
        actual = {**part, "angle": 73.}
        self.assertEqual(orientation_order(part, template), 0)
        self.assertFalse(meaningful_edit(part, actual, {"round": template}))
        original = level(part)
        changed = copy.deepcopy(original)
        changed["parts"][0]["angle"] = 73.
        result = compare(describe(original, {"round": template}), describe(changed, {"round": template}))
        self.assertTrue(result["nearCopy"])
        self.assertEqual(result["meanAngleErrorDegrees"], 0.)
        self.assertEqual(canonical_fingerprint(original, {"round": template}), canonical_fingerprint(changed, {"round": template}))

    def test_c_gap_rotation_remains_meaningful(self):
        part, template = fixture("ring", "C")
        self.assertEqual(orientation_order(part, template), 1)
        self.assertTrue(meaningful_edit(part, {**part, "angle": 90.}, {"C": template}))
        original = level(part)
        changed = copy.deepcopy(original)
        changed["parts"][0]["angle"] = 90.
        result = compare(describe(original, {"C": template}), describe(changed, {"C": template}))
        self.assertGreater(result["meanAngleErrorDegrees"], CRITERIA["maximumMeanAngleErrorDegrees"])

    def test_straight_two_way_half_turn_is_equivalent(self):
        part, template = fixture("slider", "straight")
        self.assertEqual(orientation_order(part, template), 2)
        self.assertFalse(meaningful_edit(part, {**part, "angle": 180.}, {"straight": template}))
        self.assertTrue(meaningful_edit(part, {**part, "angle": 90.}, {"straight": template}))

    def test_bent_t_l_u_shapes_keep_direction(self):
        for name in ("T", "L", "U"):
            with self.subTest(name=name):
                part, template = fixture("slider", name)
                self.assertEqual(orientation_order(part, template), 1)
                self.assertTrue(meaningful_edit(part, {**part, "angle": 180.}, {name: template}))

    def test_asymmetric_input_prevents_round_equivalence(self):
        part, template = fixture("ring", "round")
        part["inputShapes"] = [shape([(0, 0), (3, 0), (0, 2)])]
        self.assertEqual(orientation_order(part, template), 1)
        self.assertTrue(meaningful_edit(part, {**part, "angle": 90.}, {"round": template}))

    def test_directional_pull_prevents_half_turn_equivalence(self):
        part, template = fixture("slider", "straight")
        template["movement"]["lockLeft"] = True
        self.assertEqual(orientation_order(part, template), 1)
        self.assertTrue(meaningful_edit(part, {**part, "angle": 180.}, {"straight": template}))

    def test_native_stem_prevents_round_equivalence(self):
        part, template = fixture("ring", "round")
        template["physicalShapes"].append(shape([(2., -.2), (4., -.2), (4., .2), (2., .2)], "stem"))
        self.assertEqual(orientation_order(part, template), 1)
        self.assertTrue(meaningful_edit(part, {**part, "angle": 90.}, {"round": template}))

    def test_dependency_branch_change_remains_meaningful(self):
        part, template = fixture("ring", "round")
        self.assertTrue(meaningful_edit(part, part, {"round": template}, [2], [3]))

    def test_rigidbody_pointer_cannot_manufacture_spatial_edit(self):
        part, template = fixture("slider", "straight")
        actual = copy.deepcopy(part)
        actual["angle"] = 180.
        actual["sourceRigidbody"]["physicsMaterial"] = {"fileId": 3, "pathId": 99}
        self.assertFalse(meaningful_edit(part, actual, {"straight": template}))


if __name__ == "__main__":
    unittest.main()
