# Native pull and catch runtime contracts

Source: `Research/rules/hot_all.il` (Hot APK gameplay assembly). This note records the behavior the Python/Unity QA fixture simulator must preserve for authored pulls, FixedObject catches, and butterfly catches. Geometry is from actual serialized collider paths and render components; mechanics are relation/collider driven.

## Pull motion and solve

- `.DraggableObjectWithLimitRange::GetLockedDragDirection` and `ResolveLockedDragDirection` (IL around 4000–4094): `lockDirectionType == 0` resolves to the normalized object `transform.up`; `lockDirectionType == 1` resolves to normalized `-transform.right`. A zero-length transformed axis falls back to world up. Other values use the legacy lock flags (`lockLeft`, `lockRight`, `lockUp`, `lockDown`), selecting the unique allowed axis/direction where possible and otherwise falling back to up.
- `LegacyMoveToward`, `TryMoveOneStep`, `MoveTowardAxisGap`, and `TryGetAxisGapToTarget` (around 4285–4775): translation is projected onto the locked axis as a **signed** scalar. The sign determines either direction along the axis; the source does not require a one-sided pull. Preserve the actual axis vector and signed displacement in replays rather than deriving a fixed positive threshold from path length.
- `.PullObject::OnPointerUp` / `FinishReleaseSettlement` (around 7165–7205 and 24864–24955): release resolves overlap, restores the body to dynamic simulation, finalizes stick contacts, then asks `RingObjectSolveService.CanSolve`. If allowed, it solves the pull. There is no generic “dragged far enough” solve check in `PullObjectSolver.CanSolve`.
- `PullObjectSolver.CanSolve` (around 356483–356510): returns false while `RingLockPassiveService.HasActivePullBindings(pull)` is true; otherwise true. `OnPreSolve` cleans up drag state and disables the pull’s physics colliders; then ring relatives are solved and `onSolved` is invoked.
- `PullPassiveLockDomain.BuildActiveForOwner` (around 344080–344132) calls `PullObject.CollectOverlappedPartIds`; that uses the actual serialized overlap handler (`Overlap2DHandler.GetOverlapColliders`) and converts current collider contacts into part IDs. Therefore initialize/reconcile passive locks from initial contacts as well as later enter events.

## FixedObject catch solve

- `FixedObjectSolver.CanSolve` (around 356431–356457) delegates to `RingLockPassiveService.CanCatchGuideSolve`; it is not a direct collision-distance or `CanCollect` check.
- `CatchGuideSolveBase.CanCatchGuideSolve` (around 343839–343910) resolves the FixedObject `PartData.PhysicPartId`, checks `CatchLockRegistry` for the guide’s caught part, and routes registered guide links through pull/ring passive domains. `PullPassiveLockDomain.CanCatchGuidePartSolve` and `RingPassiveLockDomain.CanCatchGuidePartSolve` (around 344559–344617 and 346950–347016) make solve eligibility depend on the caught part’s solved state and whether the guide still has an active passive binding to that part.
- Passive binding activation is collider-specific. `RingLockPassiveService.OnDragCollisionEnter` (around 347482–347504) acts only on new-enter events and passes the moving root and exact other collider into both passive domains. Each domain resolves the other collider’s instance ID against its authored static binding before activating it (`PullPassiveLockDomain.TryActivateBinding`, `RingPassiveLockDomain.TryActivateBinding`; around 344222–344250, 346642–346670). Exits remove the corresponding collider binding unless source preservation rules retain it; ring bindings also remain while `RingObject.HasRingOverlapWith` is true or `RingLockFeatureService.ShouldPreserveBindingOnDragExit` applies.
- A FixedObject’s native `shapeId` such as `Hinges2` identifies its authored catch/collider shape. Render support remains the serialized `QuadPlaneController` quad and its material reference. Do not replace the catch with a ring approximation.

## Butterfly catch solve

- `FixedButterflyObject.SetSlotGuidePartIds`, `RegisterSlotGuideBindings`, and `EnsureSlotRelative` (around 372954–373090) establish two slot guide IDs and register each slot’s actual guide transform as a relation. `TryGetSlotColliderIdByGuideId` resolves a guide to its own slot collider, so both slot/collider identities must be kept distinct.
- `CatchButterflySolver.CanCatchButterflySolve` (around 343672–343718) requires at least one slot guide to resolve to a caught-part relation; every configured/registered slot relation it evaluates must pass `CatchGuideSolveBase.CanCatchGuideSolve`. Treat butterfly as a two-slot bridge relation, not as an ordinary FixedObject with one guide.
- `FixedButterflyObject.DoFade` calls `FixedObject.DoFade` and disables all slot colliders plus the bridge collider (`SetSlotAndBridgeCollidersEnabled`; around 373153–373212). Solve therefore removes the full butterfly collision fixture together.
- Butterfly collision registration is per slot. `FixedButterflyObject.RegisterSlotGuideBindings`/`TryGetSlotColliderId` and `RingLockPassiveService.OnDragCollisionEnter/Exit` use specific colliders, so a broad “overlaps butterfly” Boolean loses real state.

## `CanCollect`, `LockCount`, and solve are separate

- `RingObjectsService.CollectFromLevel` and `AddRuntimeObject` (around 348214–348345) put objects in the collectible set only when `GameplayPartData.CanCollect` is true. They separately track positive `LockCount`, `RandomBombCanSelect`, and `HasBomb` in their respective registries.
- `PullObjectSolver`, `FixedObjectSolver`, and `FixedButterflyObjectSolver` do not inspect `CanCollect` in `CanSolve`. A non-collectible guide can still be solved when its relation gate permits it; `CanCollect` affects collection/progression accounting, not physics-contact solve eligibility.
- `LockCount` is not a substitute for the passive binding set. Relation eligibility is determined by active collider bindings/registries, while chain-lock count is separately tracked in `RingObjectsService`.

## Simulator implications

1. Retain separate authored static relation bindings and runtime active bindings, keyed by the contacted collider/slot identity. Seed the active set from initial overlaps and update it from new-enter/exit contacts.
2. On pull release, settle the actual physical pose and final collider contacts first. Then solve the pull only if no active pull bindings remain; do not solve by crossing a scalar distance threshold.
3. Solve Fixed catches and butterflies through their guide IDs and caught-part solved state. Preserve butterfly’s per-slot relations/colliders and bridge collider lifecycle.
4. Keep collectible flags, chain-lock counts, and solve eligibility as separate data and predicates.
5. Use original collider shapes for contact tests. Contact skin/overlap differences can alter active binding state, so source-pose and contact parity matter more than visual proximity.
