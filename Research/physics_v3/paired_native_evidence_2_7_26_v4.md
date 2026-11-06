# Paired bounded computer evidence

Source/candidate input bundles verify against the C# BinaryWriter hash convention. Both reports have the same reported rules hash `237a0cccbec3486b2e43c948a6b637bfb7698cdcbc75588b6b0be46434414e82` and budgets128 states/15000ms per level. Current rules differ: this is historical same-reported-rules comparison; old rules bytes are not available here for independent snapshot verification. It is not current-runtime certification.

| Reference | Source solver segments | Candidate solver segments | Distinct acted roots source→candidate | Part switches source→candidate | Requested rotation magnitude source→candidate |
|---|---:|---:|---:|---:|---:|
|2|3|3|2→1|1→0|299.93→209.97degrees|
|7|9|9|6→7|7→8|702.85→769.79degrees|
|26|35|Unknown, censored|12→Missing|22→Missing|3092.84→Missing|

Position2's identical segment count hides a different acted-root/action-switch pattern. Position7 has one per-root requested rotation sign reversal in both witnesses, but switches, acted roots and angular requests differ. Position26 candidate reached the wall-time bound after23 explored states/877 generated actions; its `moveCount=-1` is missing evidence, never zero. All source/candidate ratios for26 are null/censored. The source witness has six per-root requested rotation reversals and two segments whose reported blocked flag may represent contact or a spent-bomb event.

These are bounded computer-solver segments, not human drags, minimal solutions or human difficulty. Requested rotation sums are not measured contact-truncated travel. Search effort/timing ratios are descriptive and depend on solver/editor state. Same segment count does not establish difficulty equivalence. No content cutoff, acceptance or equivalence claim is applied. Current native guide-scope QA and new paired replays are pending with the parent/Unity agent.

Machine-readable full metrics, report/input/rule-file hashes and limitations: `Research/physics_v3/paired_native_evidence_2_7_26_v4.json`.

Tests: `python Tools/test_paired_native_evidence_v4.py` —12 tests PASS in0.002s. Includes an independent .NET BinaryWriter hash fixture with a multibyte UTF8 name exceeding127bytes, stale input, different rules, historical same-rules labeling, different state/time budgets, unknown/-1 censoring, missing rows, duplicate positions, input identity mismatch, inconsistent solved action count and zero denominators.

Guide-angle contract evidence, historical proposal research only: `python Tools/test_guide_anchor_patch_contract_v3.py --workspace "RotateRings Test/LevelTraining_SourceLayoutV3"` —8 tests PASS in9.088s. Positive saved-record/whole-unit transform fixture, fixed anchor change, independent XY, guide rigidity, forged context/donor, truncated unit and stale corpus binding. These certify provenance, not native gameplay.
