"""Labeled synthetic geometry regressions, not native-game validation."""
import copy
import json
import math
from pathlib import Path
import unittest

from straight_pull_obstruction_diagnostic_v3 import diagnose, edges


def rectangle(x0, y0, x1, y1, trigger=False):
    return {'role': 'guide_trigger' if trigger else 'body',
            'points': [{'x': x0, 'y': y0}, {'x': x1, 'y': y0},
                       {'x': x1, 'y': y1}, {'x': x0, 'y': y1}],
            'trigger': trigger, 'sourceEnabled': True, 'sourceLayer': 7}


def fixture(target_x=4.5, guide_x=0.):
    templates = {
        'bar': {'physicalShapes': [rectangle(-2., -.5, 2., .5)],
                'pullVisual': {'pathPoints': [{'x': -2., 'y': 0.}, {'x': 2., 'y': 0.}]},
                'movement': {'axisLocal': {'x': -1., 'y': 0.}}, 'width': 1.},
        'guide': {'physicalShapes': [rectangle(-.1, -1., .1, 1., True)]}}
    level = {'id': 1, 'sourceId': 'SYNTHETIC_REGRESSION_ONLY',
             'parts': [], 'guideLinks': [], 'links': [], 'bombs': []}
    for index, center in enumerate((0., target_x), 1):
        level['parts'].append({'id': index, 'kind': 'slider', 'templateId': 'bar',
                               'x': center, 'y': 0., 'angle': 0.,
                               'sourceColliderBindings': [{'shapeIndex': 0}]})
        level['parts'].append({'id': index+10, 'kind': 'fixed', 'templateId': 'guide',
                               'x': center+(guide_x if index == 1 else 0.),
                               'y': 0., 'angle': 0.})
        level['guideLinks'].append({'guideOwnerId': index+10, 'lockedOwnerId': index,
                                    'guideShapeIndices': [0]})
    return level, templates


class GeometryRegression(unittest.TestCase):
    def test_first_contact_and_signed_direction(self):
        level, templates = fixture()
        report = diagnose(level, templates)
        row = next(r for r in report['directions'] if r['sourceId'] == 1 and r['direction'] == -1)
        self.assertAlmostEqual(row['geometricOwnGuideExitDistance'], 2.1, places=5)
        self.assertAlmostEqual(row['potentialPullObstaclesBeforeGuideExit'][0]['firstEntryDistance'], .5, places=5)
        self.assertIn((1, 2, -1), edges(report))
        self.assertNotIn((1, 2, 1), edges(report))

    def test_obstacle_after_guide_exit_excluded(self):
        level, templates = fixture(target_x=6.5)
        self.assertNotIn((1, 2, -1), edges(diagnose(level, templates)))

    def test_guide_offset_changes_geometric_window(self):
        base, templates = fixture(target_x=6.5)
        changed, _ = fixture(target_x=6.5, guide_x=1.)
        self.assertNotIn((1, 2, -1), edges(diagnose(base, templates)))
        self.assertIn((1, 2, -1), edges(diagnose(changed, templates)))
        # Every Pull body pose remains identical in this synthetic example.
        self.assertEqual([p for p in base['parts'] if p['kind'] == 'slider'],
                         [p for p in changed['parts'] if p['kind'] == 'slider'])

    def test_rigid_transform_invariance(self):
        level, templates = fixture()
        expected = diagnose(level, templates)
        angle = math.radians(73.)
        for part in level['parts']:
            x, y = part['x'], part['y']
            part.update(x=x*math.cos(angle)-y*math.sin(angle)+7.,
                        y=x*math.sin(angle)+y*math.cos(angle)-3., angle=73.)
        actual = diagnose(level, templates)
        self.assertEqual(edges(expected), edges(actual))
        for first, second in zip(expected['directions'], actual['directions']):
            self.assertAlmostEqual(first['geometricOwnGuideExitDistance'],
                                   second['geometricOwnGuideExitDistance'], places=5)

    def test_unsupported_mechanics_rejected(self):
        level, templates = fixture()
        level['links'] = [{'source': 1, 'target': 2}]
        with self.assertRaises(ValueError):
            diagnose(level, templates)
        level['links'] = []
        templates['bar']['pullVisual']['pathPoints'].append({'x': 0., 'y': 1.})
        with self.assertRaises(ValueError):
            diagnose(level, templates)


if __name__ == '__main__':
    unittest.main(verbosity=2)
