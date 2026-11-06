# Gameplay and composition novelty diagnostic: frozen positions 2 and 7

## Scope and evidence

This is a source-relative diagnostic for frozen candidates `7800002` and `7800007`, compared with source levels `10002` and `10012` at positions 2 and 7. It is not an acceptance verdict. The remaining pilot candidate at position 26 is outside this review; its bounded solve is unknown.

Inputs: `full500/source_parity_pack.json`, `full500/source_templates.json`, and `visual_qa/staged_semantic_review_20261106_0524/proposals_snapshot.json`. Solver evidence is from `source_solver_v3_pairs_2_7_26_sources_237a0cccbec3486b_report.json` and `source_solver_v3_pairs_2_7_26_candidates_237a0cccbec3486b_report.json`. Both use rules hash `237a0cccbec3486b2e43c948a6b637bfb7698cdcbc75588b6b0be46434414e82` (Unity `6000.0.78f1`, fixed delta `0.01666666753590107`). These are bounded production-physics searches. A solved row is a replayed witness, not an optimal path or human-difficulty estimate; a failed bounded search is unknown.

Geometry method: for each part, transform the template `physicalShapes` polygon vertices by the authored part angle and center, union polygons by physical role, and measure Shapely symmetric-difference area in world units squared. Colors are treated as palette labels, not geometry. The coordinates are authored coordinates; this is not Unity visual or animation evidence.

## Global transform, relabeling, and palette checks

All same-ID part centers in both source/candidate pairs are exactly coincident in the pack data. The many unchanged, distinct centers and poses anchor the board frame; no whole-board rotation or reflection explains the local changes. Parts other than the listed changed rings retain their templates, poses and colors, except where noted below. Changed geometry remains at the same center and is not just a coordinate-frame change.

A palette relabel can explain color-ID changes, but not the geometry, orientation, or links. Position 2 changes ring 3 color `3→1`; position 7 changes ring 3 `3→1`, ring 5 `5→2`, and ring 10 `3→2`.

Directed authored link topology also changes, independently of color. For position 2, source links are `2→1, 3→2` (chain); candidate links are `2→1, 3→1` (fan-in). Their directed degree distributions differ, so no root/part relabeling makes these graphs isomorphic. For position 7, source has `1→{2,4,6,10}, 3→2, 5→4`; candidate has `1→{2,4,6,10}, 3→5, 5→4`. The source has two targets of indegree 2; candidate has one, so the directed graphs are not isomorphic under relabeling. The guide links at position 7 are unchanged. These are authored pin/guide relationships; this observation does not claim static geometry proves any particular dynamic clearance effect.

## Position 2: source 10002 → candidate 7800002

Rings 1 and 2 retain their exact templates, poses, and colors. Ring 3 stays at the same center but changes template and angle from `269.9265°` to `224.8812°` (about `−45.0454°`), as well as color. Role-wise world-polygon symmetric differences are: body `1.395088`, head trigger `0.894505`, head wall `0.136667`, and stem `0.416667`. Thus this is a physical-shape/orientation change, not just RGB relabeling.

The solver replays both boards in 3 actions and removes all 3 parts. Their early actions are the same: rotate part 1 by `105.193°`, then `44.815°`. At the final release, the source rotates part 2 by `149.927°`, while the candidate rotates part 1 by `59.959°`. This is a different release path after the shared opening, alongside the graph and ring-3 changes. Equal replay length does not establish equal player difficulty.

## Position 7: source 10012 → candidate 7800007

The part centers are unchanged. Rings 3, 5, and 10 change template and angle:

| Part | Source angle → candidate angle | Angle delta | Body sym-diff | Other changed physical roles |
|---|---:|---:|---:|---|
| 3 | `247.7325° → 99.2700°` | `−148.4625°` | `2.871546` | head trigger `0.916813`; head wall `0.136667`; stem `0.367332` |
| 5 | `266.3088° → 164.4752°` | `−101.8337°` | `2.862673` | head trigger `0.117401`; head wall `0.136667`; stem `0.035793` |
| 10 | `110.0000° → 61.9300°` | `−48.0700°` | `1.470085` | — |

Symmetric-difference area is nonzero for the body on all three, while position, shape symmetry, and rotational orientation interact. These geometry deltas document changed static solids; they do not quantify dynamic clearance or prove which release is harder.

Both solver witnesses use 9 actions and remove all 13 parts. Shared steps include the initial `rotate 4 −2°`, `slide 7 to −2.4`, `rotate 4 +83.309°`, `rotate 6 +135°`, `rotate 4 +143.691°`, `slide 11 to −2.4`, and `rotate 2 +80°`. Key path differences:

- The source first releases part 10 with `+141.122°`; the candidate uses `−125.993°`.
- Before the shared `rotate 4 +143.691°`, the source rotates part 4 by `+143.691°` after part 6; the candidate instead rotates part 5 by `+199.795°`, then part 4 by `+143.691°`.
- Source final release is part 2 `+117.733°`; candidate ends after part 2 `+80°`.

These distinct action witnesses corroborate that the physical/topological changes alter at least one solver route. They do not measure how many people would find either board harder.

Search diagnostics: position 7 source generated 562 actions and recorded 8 binding-changing transitions; candidate generated 571 actions and recorded 16. These counters depend on bounded search and are not calibrated difficulty measures or thresholds. Candidate position 26 is unknown in the candidate report and is not inferred from these results.

## Diagnostic summary

Both reviewed candidates differ from their paired sources in meaningful authored topology and physical solids, and each has a replayed solver path that diverges from the source. Palette changes and whole-board transforms do not account for those differences. This supports a concrete novelty/composition distinction at positions 2 and 7; it does not certify comparable human difficulty, dynamic clearance importance, or visual/animation quality.
