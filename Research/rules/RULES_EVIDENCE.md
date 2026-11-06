# Rotate Rings 1.2.0 static rule and progression evidence

Research date: 2026-11-05. No original APK runtime session was available to this research task. These are static facts from the supplied APK, not measured human difficulty.

## Reproduction and identity

`Tools/research_rules.py` decodes the Brotli-compressed `.rawfile` assemblies and configuration metadata, parses 21 original progression tables and writes a CIL disassembly. The hot assembly is **compressed, not encrypted**. `game_assembly_hot_hot_bytes.dll` recovers Hot.dll (3,142,656 bytes). REA `inspect-managed-artifact` output is retained in `rea_managed_evidence.json`, SHA256 `b83cac16e0ad6e81d81a6fecd1ee014a679f908f4f54f9deb37c654899ecb7d8`.

Portable ILSpy 9.1.0.7988 is under `ilspy/tools/net8.0/any/ilspycmd.dll`. It runs with the existing .NET 8 runtime, without an SDK installation. The one-file-per-type decompilation is under `decompiled/`; `hot_all.il` retains corresponding method RVAs and decoded operand names. Decompilation contains unknown-type comments because several dependencies are AOT assemblies.

## Actual progression order

`main_progressions.json` records every row of every TbLevelConfig0..20 table. Each row gives position, physics prefab and core identifier. The order is explicit and differs from numerical scene IDs.

Table 1 contains **500 distinct original levels**, and all 500 IDs are present among the 507 extracted physical scenes. Tables 12, 13, 14 and 15 likewise each contain 500 unique matching IDs. Table0 contains 400, Table11 contains 450, several others 380, and tables16..20 are two-position development tables. Table1 is a suitable deterministic 500-position research corpus; the actual installed player's chosen experiment value is not known.

First ten Table1 identifiers: 10001, 10002, 10003, 10005, 10004, 10010, 10012, 10007, 10008, 10154. MainLevelConfigMode chooses the table with LevelGapDegExp, normalizing outside 0..20 to zero. Beyond a selected table's count, player positions cycle from position21 through the end; they do not represent an endlessly harder continuation.

## Ring interaction and release

The ring moves by continuous rotation about its center, while a `PullObject` moves along its permitted axis. Drag rotation is substepped and collision-checked. The original contains explicit bounds/probe correction and rollback after blocked movement; a single final-angle overlap test is insufficient to reproduce collision behavior.

`RingObject.CheckLockedByRelatives` prevents beginning a rotation while its own pin/relative feature is actively held by another linked part. The other ring may still turn to disengage that pin. `RingObject.IsLockedImmovable` also considers positive chain LockCount and pending palette-key unlock presentation.

All 7,918 authored ringLinks in the 507 extracted scenes have relationKind0 and activityKind0. The active lock service initializes **sourceRingId → targetPartId** using a specific source pinFeatureId. This relationship is a physical connection, not an immutable removal-order DAG. Contacts and separation activate/deactivate the relationship, and removal clears relevant relationships. RingToRingLinkRegistrar registers a reciprocal passive binding on the target ring to the source ring's matching relative colliders. PullToRingLinkRegistrar supports ring-to-bar and bar-to-ring links as well.

`RingObjectSolver.CanSolve` succeeds only after both active outgoing lock targets and passive ring bindings are absent. `PullObjectSolver.CanSolve` succeeds after its passive bindings are absent. Neither condition is equivalent to merely rotating a gap toward an arbitrary exit direction. Removal is automatic when released; the game schedules release-settlement after a pointer gesture, and it can solve cascades.

### Corrected CanCollect interpretation (normalizer 1.1)

`GameplayPartData.CanCollect` controls score/reward collection eligibility; it does **not** make a ring or slider a permanent anchor or exempt it from winning. `RingObjectsService` uses the flag only to populate its separate `collectableObjects` list. `RingObjectSolveService.RegisterObject` registers every unsolved gameplay object without a CanCollect test, and `GameplayManager.ObjectsSolveAndCheckVictory` wins when `SolveService.UnsolvedCount == 0`. `CoreLevelSolverDomain.ReleasableIds` instead selects `ReleasableBehaviorDefinition` or `ForceRemovableBehaviorDefinition`.

The initial normalizer 1.0 incorrectly mapped CanCollect to the reduced runtime's `collectible` goal flag. That hypothesis is rejected by the recovered method bodies. Normalizer 1.1 sets all movable RingObject/PullObject entries to `collectible:true`, preserves the original flag separately as `scoreEligible` and in `sourcePartData.CanCollect`, and leaves actual catch/butterfly fixtures in `state.fixtures`. Consequently sliders with source CanCollect=false still count toward reduced-runtime removal goals. Source labels or model artifacts trained with normalizer 1.0 must be invalidated and recomputed.

`FixedObjectSolver` defers to the passive-lock catch-guide solver. `FixedButterflyObjectSolver` defers to CatchButterflySolver, which checks both registered slot-guide relationships. A fixed butterfly component is a real mechanic, not decoration.

Relevant readable source:

- `decompiled/RingObject.cs`: CheckLockedByRelatives, OnPointerDown, OnPointerUp, IsBlockedForRotation, substep collision queries.
- `decompiled/DragRotateObject.cs` and RingRotateGuardSubstepRotator.cs: continuous input and path collision guards.
- `decompiled/RotateRings.GamePlay.Services/RingObjectSolver.cs`, PullObjectSolver.cs, FixedObjectSolver.cs, FixedButterflyObjectSolver.cs.
- `decompiled/RotateRings.GamePlay.Services/RingLockActiveService.cs`: contact-sensitive outgoing locks.
- `decompiled/RotateRings.GamePlay.Services/RingPassiveLockDomain.cs`, PullPassiveLockDomain.cs, RingToRingLinkRegistrar.cs, PullToRingLinkRegistrar.cs.
- `decompiled/RotateRings.GamePlay.Services/RingDraggingSolveService.cs`: settle and release triggering.
- `decompiled/RotateRings.GamePlay.Services/RingObjectSolveService.cs`: cascading solve bookkeeping.

## Geometry and parts

The 507 scenes contain 7,512 RingObject, 4,099 PullObject, 3,668 FixedObject, 66 FixedButterflyObject and 179 BombRelative components. Rendering meshes, collider geometry and relative features are additional components and must not be counted as independent movable game pieces.

LevelComponentFactory.CoordSpace maps authored coordinates with a configurable scalar, negates Y, and negates authored rotations into Unity Z. Ring centerline radius is `(inner+outer)/2`, band width derives from `outer-inner`, with cap/gap corrections in the renderer/collider construction. Use actual baked geometry or original mapping; mixing authored radii with Unity coordinates directly is invalid. See `decompiled/LevelComponentFactory.cs` and RingColliderBuilder.cs.

The original core supports rings, sliding sticks, fixed catches, butterfly clips and bombs, plus chain and palette/key variations introduced by services/config. Unsupported fields must be excluded or explicitly modeled before using solved labels.

## Original solver limitations and missing dependency

Hot.dll includes CoreLevelSolverDomain, which generates continuous-motion candidates using critical-event probes and calls `CoreRuntime.Preview`, plus exact replayable motion/force-remove commands. Its own action expansion states it has **no completeness certificate** for the continuous action set. Failure to find a solution does not prove unsolvability.

`UnlockRing.Core.Domain`, Geometry, Rules, Queries, Runtime and Authoring types are references to the separate `Core` AOT assembly. They are not CIL bodies within Hot.dll and there is no Core rawfile among the recovered managed assemblies. Full original pure-core simulation cannot simply call the recovered Hot.dll. Static C# reconstruction of Hot services does not recover the absent native Core geometry implementation. A new simulator must be checked against available geometry/runtime evidence and describe its assumptions honestly.

Observed rule support is strong for the static contact/release structure above. Exact collision tolerances, edge cases under simultaneous contacts, and equivalence to original runtime remain unverified until replayed against the original game or equivalent native core.
