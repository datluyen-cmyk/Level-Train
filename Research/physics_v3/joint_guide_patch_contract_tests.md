# Joint guide patch provenance contract tests

Command: `python Tools/test_joint_guide_patch_contract_v3.py --workspace "RotateRings Test/LevelTraining_SourceLayoutV3"`

Latest result: 11 tests passed in 10.121 seconds, process exit code 0 (session 86382). Initial 7-test pass took 8.765 seconds (session 55523).

- A constructed transform of an actual saved joint model record passes the provenance contract.
- Independent per-unit XY jitter is rejected.
- Independent per-unit angle changes are rejected.
- Scale other than 1 is rejected.
- Forged recipient context is rejected.
- Forged donor source identity is rejected.
- Duplicate selected native units are rejected.
- Stale saved-model corpus hash bindings are rejected.
- Per-unit native width mismatch is rejected even when the saved patch median width agrees.
- Truncated native guide unit membership is rejected.
- Kabsch recovers a proper rigid transform without scale or reflection.

Scope: these tests certify joint pattern provenance and complete guide-unit transform membership. The constructed positive fixture is not a playable level certificate and may fail raw geometry. Native asset/input/RB/state, raw solid/sensor contacts, current native obstruction/release order, difficulty, novelty and whole-figure visual acceptance are separate gates. The source30 XY+angle pilot produced zero candidates from 160 records; accepted generated count remains zero.
