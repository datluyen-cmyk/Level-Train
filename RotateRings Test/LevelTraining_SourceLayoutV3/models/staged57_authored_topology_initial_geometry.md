# Frozen 57 proposal topology review

Frozen pack SHA256: `c638d752ab9bb64ff83d7db4be5720f2c757d65bfc82ef7f1bcc657832fe030a`.

57 proposals audited. 35 have more authored pin+guide weak components than their reference. 26 introduce at least one authored-isolated movable root. 37 add native solid attachments without a head trigger. These overlap and are review flags, not automatic rejection or acceptance gates.

New authored-isolated movable references: 4,15,16,17,19,23,24,27,32,34,35,37,41,42,43,44,46,47,48,56,61,62,65,73,74,76.

- Position4: source seven movable roots form one authored component; candidate partitions into six plus one. Root2 is newly isolated. Its nearest candidate solid is root1 at1.12127 native units (2.24253 bar widths). Root1 also gains one native non-head-trigger attachment. Root2 has no new solid patch in both directions at the small1–5degree samples in both source and candidate; this does not prove either can release.
- Position3: authored component remains all four roots, but root4 gains one native non-head-trigger attachment. Neither small rotation direction avoids new solid patches in either source or candidate. This is evidence for current-native difficulty review, not a measured increase in difficulty.
- Position8: component movable sizes change5+4 to4+3+2, despite no newly isolated movable.
- Position17: component count is unchanged, but sizes change4+3+3+3 to6+3+3+1 and root5 becomes authored-isolated. A total component-count gate would miss this.

Full machine-readable report contains pin-only and pin+guide component membership, degree summaries, zero incoming/outgoing roots, articulation points, strong components, per-root attachment counts, nearest solid distances and initial geometry samples.

Small geometry samples rotate a whole ring at1,2,3,4,5degrees in each direction, or translate a whole Pull by0.1,0.2,0.3,0.4,0.5 native widths along both signs of its native axis. They measure new solid intersection patches relative to the initial exact patch. They do not simulate native direction permissions, filters, receiving-wall exception, state, skin, collision resolution, continuous paths or release. Authored graphs omit physical-clearance dependencies. No runtime, candidate or acceptance state was changed.

Reproduce with `Tools/audit_authored_topology_freedom_v3.py`, frozen snapshot and explicit `--expected-pack-sha256`. Current native engine and source-family coherence evidence must determine whether any flag represents unacceptable puzzle structure.
