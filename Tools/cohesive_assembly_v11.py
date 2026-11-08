"""Source-relative design constraints shared by search and final review.

These are layout/provenance heuristics, not proof of solution or human fun.
Keep them in the proposal search so it cannot spend a whole round rebuilding
one donor and hiding it behind a small appendage.
"""
from collections import Counter, defaultdict
import math
import numpy as np
import networkx as nx
from scipy.sparse.csgraph import minimum_spanning_tree
from source_physical_composition_v3 import _posed_geometry, composition_edges


class CohesiveDesignPolicy:
    def __init__(self, reference, templates=None):
        self.reference = reference
        self.count = len(reference['parts'])
        self.inventory = Counter(p['kind'] for p in reference['parts'])
        self.target = self.spatial(reference)
        self.templates = templates
        self._geometry_cache = {}
        self.target_outlines = self.outlines(reference) if templates is not None else None
        self._mechanics_cache = {}
        self.target_mechanics = self.mechanics(reference) if templates is not None else None

    def mechanics(self, level):
        key = (tuple((p['id'], p['templateId'], p['x'], p['y'], p.get('angle', 0.)) for p in level['parts']),
               tuple((e['source'], e['target']) for e in level.get('links', [])),
               tuple((e['guideOwnerId'], e['lockedOwnerId']) for e in level.get('guideLinks', [])))
        if key not in self._mechanics_cache:
            edges = composition_edges(level, self.templates)
            graph = nx.Graph()
            graph.add_nodes_from(p['id'] for p in level['parts'])
            graph.add_edges_from((e['source'], e['target']) for e in level.get('links', []))
            graph.add_edges_from((e['guideOwnerId'], e['lockedOwnerId']) for e in level.get('guideLinks', []))
            graph.add_edges_from((e['source'], e['target']) for e in edges)
            components = list(nx.connected_components(graph))
            self._mechanics_cache[key] = {'total': len(edges), 'strong': sum(bool(e.get('strong')) for e in edges),
                                          'components': len(components),
                                          'largestFraction': max(map(len, components), default=0) / max(1, len(graph))}
        return self._mechanics_cache[key]

    def geometry(self, part):
        key = (part['templateId'], float(part['x']), float(part['y']), float(part.get('angle', 0.)))
        if key not in self._geometry_cache:
            self._geometry_cache[key] = _posed_geometry(part, self.templates[part['templateId']], {'body'})
        return self._geometry_cache[key]

    def outline_gap(self, a, b):
        width = sum(float(p.get('width', self.templates[p['templateId']].get('width', 0.))) for p in (a, b))
        return float(self.geometry(a).distance(self.geometry(b))) / max(.01, width)

    def outlines(self, level):
        parts = [p for p in level['parts'] if p['kind'] in ('ring', 'slider')]
        if len(parts) < 2:
            return {'bridgeMaxWidths': 0., 'nearestByKind': {}}
        distances = np.full((len(parts), len(parts)), np.inf)
        for i, a in enumerate(parts):
            for j, b in enumerate(parts[:i]):
                distances[i, j] = distances[j, i] = self.outline_gap(a, b)
        by_kind = defaultdict(list)
        for p, distance in zip(parts, distances.min(axis=1)):
            by_kind[p['kind']].append(float(distance))
        tree_distances = np.maximum(distances, 1e-7)
        np.fill_diagonal(tree_distances, 0.)
        tree = minimum_spanning_tree(tree_distances)
        return {'bridgeMaxWidths': float(tree.data.max()) if tree.nnz else 0.,
                'nearestByKind': {k: max(v) for k, v in by_kind.items()}}

    def extension_failure(self, previous, draft):
        """Grow a board from its boundary, never append a remote mini puzzle.

        Exact outlines account for large L/U sliders whose centers alone say
        little about whether they fit the neighboring shapes. This proximity
        condition is a layout constraint, never a gameplay lock assertion.
        """
        if self.target_outlines is None:
            return None
        old = [p for p in previous['parts'] if p['kind'] in ('ring', 'slider')]
        ids = {p['id'] for p in previous['parts']}
        added = [p for p in draft['parts'] if p['id'] not in ids and p['kind'] in ('ring', 'slider')]
        if not old or not added:
            return None
        limit = max(.20, self.target_outlines['bridgeMaxWidths'] * 1.30)
        if min(self.outline_gap(a, b) for a in old for b in added) > limit:
            return 'remoteMotifAppendage'
        return None

    @staticmethod
    def spatial(level):
        parts = [p for p in level['parts'] if p['kind'] in ('ring', 'slider')]
        if len(parts) < 2:
            return {'nearestMax': 0., 'bridgeMax': 0., 'span': 0., 'occupancy': np.zeros(24)}
        xy = np.array([[p['x'], p['y']] for p in parts], dtype=float)
        distances = np.linalg.norm(xy[:, None] - xy[None, :], axis=2)
        np.fill_diagonal(distances, np.inf)
        nearest = distances.min(axis=1)
        # Nearest-neighbour distance cannot see two tight but unrelated islands.
        # The longest edge in a Euclidean MST measures the bridge needed to
        # connect the entire board. Keep coincident centers as epsilon edges:
        # scipy treats literal zero as an absent edge in sparse graph input.
        tree_distances = np.maximum(distances, 1e-7)
        np.fill_diagonal(tree_distances, 0.)
        tree = minimum_spanning_tree(tree_distances)
        bridge = float(tree.data.max()) if tree.nnz else 0.
        extent = np.ptp(xy, axis=0)
        span = float(max(extent))
        # A soft board field measures overall balance rather than absolute
        # source coordinates. Native motifs remain rigid; never scale assets.
        centered = (xy - xy.mean(axis=0)) / max(span, .01)
        grid = np.array([(x, y) for x in np.linspace(-.5, .5, 4)
                         for y in np.linspace(-.5, .5, 6)])
        field = np.exp(-np.sum((centered[:, None] - grid[None, :]) ** 2, axis=2) / .035).sum(axis=0)
        field /= max(float(field.sum()), .001)
        return {'nearestMax': float(nearest.max()), 'bridgeMax': bridge,
                'span': span, 'occupancy': field}

    def seed_allowed(self, tile):
        return len(tile['partIds']) <= max(2, math.floor(self.count * .50))

    @staticmethod
    def contributions(level):
        sources = Counter(str(p.get('atomicProvenance', {}).get('sourceId', 'unknown'))
                          for p in level['parts'])
        rigid = defaultdict(set)
        for item in level.get('assemblyProvenance', {}).get('tileImports', []):
            angle = float(item.get('rotation', 0.)) % 360
            if abs(angle - 360) < .001: angle = 0.
            key = (str(item['sourceId']), round(angle, 2),
                   *(round(float(v), 3) for v in item['translation']))
            rigid[key].update(int(v) for v in item.get('partMapping', {}).values())
        return sources, max((len(ids) for ids in rigid.values()), default=0)

    def check(self, level):
        sources, rigid = self.contributions(level)
        # Thresholds use final target size during frontier expansion. Dividing
        # by an early draft's size would incorrectly reject every seed.
        budget_count = (min(self.count, len(level['parts']))
                        if len(level['parts']) >= self.count * .90 else self.count)
        if max(sources.values(), default=0) > math.floor(budget_count * .65):
            return 'dominantDonor'
        if rigid > math.floor(budget_count * .60):
            return 'reassembledRigidDonor'
        spatial = self.spatial(level)
        if spatial['span'] > max(.01, self.target['span']) * 1.35:
            return 'boardEnvelope'
        if len(level['parts']) >= self.count * .90:
            if spatial['bridgeMax'] > max(.01, self.target['bridgeMax']) * 1.30:
                return 'disconnectedIslands'
            if spatial['nearestMax'] > max(.01, self.target['nearestMax']) * 1.35:
                return 'isolatedMovable'
            if self.target_outlines is not None:
                outlines = self.outlines(level)
                if outlines['bridgeMaxWidths'] > max(.20, self.target_outlines['bridgeMaxWidths'] * 1.30):
                    return 'outlineDisconnectedIslands'
                for kind, gap in outlines['nearestByKind'].items():
                    if gap > max(.20, self.target_outlines['nearestByKind'].get(kind, 0.) * 1.35):
                        return 'unsupportedOutline:' + kind
                mechanics = self.mechanics(level)
                if (mechanics['largestFraction'] < self.target_mechanics['largestFraction'] * .75 or
                        mechanics['components'] > math.ceil(self.target_mechanics['components'] * 1.35)):
                    return 'fragmentedInteractionGraph'
        return None

    def score(self, level):
        n = len(level['parts'])
        counts = Counter(p['kind'] for p in level['parts'])
        # Match proportions throughout the frontier; the old absolute count
        # objective could prefer repeated two-part slider/guide motifs.
        inventory = sum(abs(counts[k] / max(1, n) - self.inventory[k] / self.count)
                        for k in set(counts) | set(self.inventory))
        spatial = self.spatial(level)
        field = float(np.abs(spatial['occupancy'] - self.target['occupancy']).sum())
        excess_gap = max(0., spatial['nearestMax'] / max(.01, self.target['nearestMax']) - 1.)
        excess_bridge = max(0., spatial['bridgeMax'] / max(.01, self.target['bridgeMax']) - 1.)
        outline_penalty = 0.
        if self.target_outlines is not None:
            outlines = self.outlines(level)
            outline_penalty = max(0., outlines['bridgeMaxWidths'] - self.target_outlines['bridgeMaxWidths'])
        mechanical_penalty = 0.
        if self.target_mechanics is not None:
            mechanics = self.mechanics(level)
            mechanical_penalty = sum(abs(mechanics[k] / max(1, n) - self.target_mechanics[k] / self.count)
                                     for k in ('total', 'strong'))
            mechanical_penalty += abs(mechanics['largestFraction'] - self.target_mechanics['largestFraction'])
        return (-inventory * .8 - field * .5 - excess_gap * .7 - excess_bridge * 1.2
                - outline_penalty - mechanical_penalty * 1.5)

    def audit(self, level):
        sources, rigid = self.contributions(level)
        n = max(1, len(level['parts']))
        failure = self.check(level)
        return {'policyVersion': 11, 'passed': failure is None, 'failure': failure,
                'sourcePartCounts': dict(sources),
                'dominantDonorFraction': max(sources.values(), default=0) / n,
                'largestRigidDonorFraction': rigid / n,
                'spatial': {k: v for k, v in self.spatial(level).items() if k != 'occupancy'},
                'referenceSpatial': {k: v for k, v in self.target.items() if k != 'occupancy'},
                'outlines': self.outlines(level) if self.target_outlines is not None else None,
                'referenceOutlines': self.target_outlines,
                'compositionProxy': self.mechanics(level) if self.target_mechanics is not None else None,
                'referenceCompositionProxy': self.target_mechanics,
                'score': self.score(level),
                'meaning': 'Provenance and spatial cohesion heuristics; solution and human design quality unverified.'}
