# Native guide-binding contract audit, source500 positions 1–500

## Scope and method

Read-only comparison of `source500_native.json`, `source_templates.json`, `SourceOverlapQueries.json`, the staged `RotateRingsCollisionWorld.cs`, and the decompiled native registration, contact, release, and solve methods. No Unity/runtime session was used and no runtime code was changed. This is a static contract audit; it does not claim a dynamic contact/release replay.

## Corpus counts and static shape checks

| Check | Result |
|---|---:|
| Source levels | 500 |
| Levels with guide links | 402 |
| Guide links | 3,463 |
| Slider/Pull owner → guide links | 3,117 |
| Ring owner → guide links | 346 |
| Fixed guide targets | 3,339 |
| Butterfly slot guide targets | 124 |
| Missing guide/locked owners | 0 |
| Missing or out-of-range guide shape indices | 0 |
| Selected target shapes not `CatchGuideBody`, layer 7, trigger, and enabled | 0 |
| Linked owners with empty source collider bindings | 0 |

Each native guide relation names exactly one `guideShapeIndex`. In butterfly targets, the indices split evenly: 62 links to slot shape 1 and 62 to slot shape 2. `FixedButterflyObject.TryGetSlotColliderIdByGuideId` resolves the link's guide part ID to that specific slot collider. The bridge and guide walls are separate shapes and are not registered as slot binding colliders.

### Pull owner shapes

All 3,117 slider-owned guide links have exactly one source binding. It matches a `PullObject` overlap query on the `StickBody` shape, with `useTriggers=1` and layer mask 128 (layer 7). The target is a layer 7 `CatchGuideBody` trigger. There are no static Pull shape-index, source-role, or query-filter mismatches.

### Ring owner shapes

All 346 ring-owned guide links have at least one binding shape that is an input to a relevant `RingObject` or `RingRelative` query and can see the target layer 7 trigger with `useTriggers=1`, mask 128. However, 41 relations also include source binding indices that are outside those guide-body overlap-query paths:

| Target | Relations with extra indices | Extra index references |
|---|---:|---:|
| Fixed guide | 5 | 5 |
| Butterfly slot | 36 | 112 |
| **Total** | **41** | **117** |

Those extra input indices are 45 references to `HardBlockPin` (`stem`) and 72 to `RingRelativeWall` (`head_wall`). They are layer 7 but non-trigger shapes. The native `RingObject.CollectCatchGuideBodyContactColliders` path reads the ring's overlap handler and each relative's overlap handler, retaining colliders marked `CatchGuideBody`. The current collision-world builder instead derives `lockedShapes` from every `sourceColliderBinding`; `RefreshBindings` tests all those enabled owner shapes against the guide trigger using `ColliderDistance2D.isOverlapped`, with no query-owner or role filter. This is a real static shape-scope mismatch for 41 relations. Without evaluating native-pose geometry or replaying the event path, this audit cannot claim an extra shape actually overlaps a linked guide in any of those positions.

The 41 relations occur at positions 250, 285, 295, 314, 317, 326, 348, 352, 354, 357, 358, 359, 368, 369, 370, 375, 378, 388, 398, 399, 402, and 423.

### Native-pose polygon check for the 41 relations

I transformed source collider points/paths by each part's authored x/y/angle and checked polygons with Shapely 2.2.0. Opposite-winding enclosed paths were treated as holes. All extra shapes in the 41 links are simple zero-edge-radius `PolygonCollider2D` or `BoxCollider2D` paths; all selected guide shapes are layer 7 `CatchGuideBody` boxes.

- Of 117 extra shape references, **0 overlap** the selected guide trigger at the authored pose. The nearest extra shape is 0.450706 units away (position 370, source 10209, guide_2, HardBlockPin index 1); the farthest is 6.444241 units (position 250, source 72013, guide_18, HardBlockPin index 1).
- All **41/41** linked relations have a valid native guide-query shape overlap at the authored pose, via the ring's `RingBody` query collider at shape index 0. The additional queried `RingRelativeBody` shapes do not overlap those selected guide shapes at that pose.
- Therefore the all-binding shape scope is a genuine static scope mismatch, but it creates no extra overlap on these 41 initial poses. Extra shapes could enter contact during a rotation; sampled rotation poses were not evaluated here.

The row-by-row list contains source ID, position, guide part ID, extra shape indices/roles/distances, and valid query shape indices/overlaps in `Research/physics_v3/native_guide_binding_initialpose_extras_first500.json`. The repeatable static audit is `Tools/audit_native_guide_binding_contract.py`.

### Input hashes

| Input | SHA-256 |
|---|---|
| `source500_native.json` | `effda7077f6d709f4afb977656decd6476adf5c8a2879792958bb040f29a218f` |
| `source_templates.json` | `c44adce33201c46d8dbf5e827de719794589bf89b64943a569bb0265be1b89ee` |
| `SourceOverlapQueries.json` | `2a53f933d75aab362b7aeff34501c298bd67629d207f0bc845831d9929023a22` |
| `RotateRingsCollisionWorld.cs` | `8c6d849f72e420ea72c10c6eeca28716c4b9fd61a6dbda350de3d08fc57e9ec3` |

## Native activation, persistence, release, and solve contract

- `PullCatchGuideRegistrar.RegisterPullObjectFixedBinding` registers the guide's `FixedObject` self collider. For a `FixedButterflyObject`, it instead calls `TryGetSlotColliderIdByGuideId` and registers only that slot collider.
- `RingCatchGuideRegistrar.RegisterRingObjectFixedBinding` uses the same fixed-self / butterfly-slot distinction for ring owners.
- `PullPassiveLockDomain` and `RingPassiveLockDomain` activate and deactivate passive bindings from drag collision enter/exit events. Bindings are keyed by the exact overlapped guide collider instance ID and associated guide part ID.
- Pull separation cleanup uses `PullObject.FillCatchGuideOverlapInstanceIds`; that routine keeps only colliders marked `CatchGuideBody` from the pull overlap handler. Ring separation cleanup uses `RingObject.FillCatchGuideOverlapInstanceIds`, which gathers `CatchGuideBody` overlaps from the ring overlap handler and each `RingRelative`.
- An exit can be preserved when `RingLockFeatureService.ShouldPreserveBindingOnDragExit` returns true. The registered feature is `RingPaletteLockService`, whose `ShouldPreserveBindingOnDragExit` delegates to `ShouldKeepLocksOnExit` / `HasColorLock`.
- In this source500 corpus, every guide-linked owner has `IsPaletteLock=0` and `IsPaletteLockKey=0`, and all 500 `paletteLockKeyBindings` lists are empty. Therefore there are **zero authored guide-owner cases** exercising this preserve-on-exit branch. Its native behavior exists, but is not demonstrated by these 500 levels.
- `CatchGuideSolveBase.CanCatchGuidePartSolve` blocks guide solving while the guide's exact per-guide active binding remains. `CatchButterflySolver.CanCatchButterflySolve` checks each configured slot guide separately and permits the butterfly to solve only when all registered slot guides can solve.

The pre-fix `RotateRingsCollisionWorld.RefreshBindings` at the file hash recorded above recomputed guide activity as “any configured owner-shape/guide-shape pair currently overlaps” whenever bindings refreshed. It did not retain native collider-ID binding state or implement feature-controlled exit preservation. Since the reviewed source500 data has zero palette-lock preserve cases, the latter branch is a native contract gap without a corpus witness; the ring extra-shape paths are the concrete static scope issue described above.

After this audit, the runtime was changed to read the original main/relative/Pull
overlap probe hits with their native filters and match both the selected guide
owner and shape index. This excludes the extra pin/wall shapes and distinguishes
the butterfly slots. Fresh runtime QA is recorded under its own changed rules
hash; the static geometry findings above remain tied to their original inputs.

## Native evidence files

- `Research/rules/decompiled/FixedObject.cs`
- `Research/rules/decompiled/PullObject.cs`
- `Research/rules/decompiled/RotateRings.GamePlay.Objects/FixedButterflyObject.cs`
- `Research/rules/decompiled/ButterflyColliderBuilder.cs`
- `Research/rules/decompiled/RingColliderBuilder.cs`
- `Research/rules/decompiled/RotateRings.GamePlay.Services/PullCatchGuideRegistrar.cs`
- `Research/rules/decompiled/RotateRings.GamePlay.Services/RingCatchGuideRegistrar.cs`
- `Research/rules/decompiled/RotateRings.GamePlay.Services/PullPassiveLockDomain.cs`
- `Research/rules/decompiled/RotateRings.GamePlay.Services/RingPassiveLockDomain.cs`
- `Research/rules/decompiled/RotateRings.GamePlay.Services/RingLockPassiveService.cs`
- `Research/rules/decompiled/RotateRings.GamePlay.Services/RingLockFeatureService.cs`
- `Research/rules/decompiled/RotateRings.GamePlay.Services/RingPaletteLockService.cs`
- `Research/rules/decompiled/RotateRings.GamePlay.Services/CatchGuideSolveBase.cs`
- `Research/rules/decompiled/RotateRings.GamePlay.Services/CatchButterflySolver.cs`
- `Research/rules/decompiled/RingObject.cs`
- `RotateRings Test/Assets/RotateRings/Scripts/RotateRingsCollisionWorld.cs` (`Build`, `RefreshBindings`)
- `RotateRings Test/Assets/RotateRings/Resources/SourceOverlapQueries.json`
- `Research/physics_v3/native_guide_binding_initialpose_extras_first500.json`
- `Tools/audit_native_guide_binding_contract.py`
