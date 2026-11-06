# Source500 Pull overlap contracts

Read-only extraction from the source500 parity pack and decompiled native gameplay code. The serialized PullObject query fields are now resolved in `source500_pull_overlap_query_catalog_v3.json` by `Tools/extract_pull_overlap_contracts_v3.py`; this is a focused supplement to `source_overlap_handler_query_catalog_v3.json` and does not modify the ring catalog or runtime code.

## Source500 coverage

- The 500-source pack contains 1,501 directed ring-to-slider links; all 1,501 targets have `sourceClass: PullObject`.
- Each linked PullObject has one physical binding: `shapeIndex: 0`, `sourceFeatureId: body`, `sourceRole: StickBody`. Its one `inputShapes` entry is a non-trigger `PolygonCollider2D` marked `InputArea` on layer 6; that entry is input geometry, not the passive overlap query contract.
- Link records use the ring's serialized pin feature shape indices on `sourceShapeIndices`; PullObject target shape indices are `[0]`. The parity pack does not serialize PullObject's `Overlap2DHandler`, but the normalized raw MonoBehaviour trees do; the supplement catalog resolves all 3,982 PullObject handler records with no conflicts or unresolved references.
- No source500 part has `IsPaletteLock` or `IsPaletteLockKey` set. `LockFeature.ArePartsConnected` dispatches registered `IPartLockFeature`s; the relevant implementation is palette color-gap overlap, which requires an unsolved color-locked ring. It cannot explain source500 ring-to-Pull links at the source checkpoint.

## Native Pull overlap behavior

- `PullObject.CollectOverlappedPartIds` queries its `overlap2DHandler` into a reusable 32-collider buffer. For every result it first tries the special catch-guide mapping, then resolves the collider owner in this order: parent `FixedObject`, parent `RingObject`, parent `PullObject`. It adds the resolved unsolved owner's `PartId`. See `Research/rules/decompiled/PullObject.cs:274-354`.
- `Overlap2DHandler.GetOverlapColliders` calls `collider2D.Overlap(contactFilter, buffer)` and returns Unity's filtered overlap count. Its filter and collider are serialized fields; see `Research/rules/decompiled/Overlap2DHandler.cs:6-56`.
- `PullPassiveLockDomain.BuildActiveForOwner` builds the active binding set by running that PullObject query and retaining static partner bindings whose `PartId` is in the resulting overlap set. `CleanSeparatedActiveBindings` reruns the same query before removing separated bindings (with catch-guide collider-ID handling as a special case). See `Research/rules/decompiled/RotateRings.GamePlay.Services/PullPassiveLockDomain.cs:59-80,145-187`.
- For ring-to-Pull links, `PullToRingLinkRegistrar` registers the source ring's own relative block collider IDs for the linked pin feature as the Pull owner's static partner collider IDs. See `Research/rules/decompiled/RotateRings.GamePlay.Services/PullToRingLinkRegistrar.cs:31-61` and `RingObject.CollectSelfRelativeBlockColliderIds` at `Research/rules/decompiled/RingObject.cs:364-389`.
- Therefore passive cleanup direction is PullObject's own overlap probe against external colliders, while the registered passive bindings are the source ring feature's physical colliders. Preserve this as a separate query contract from active ring-lock cleanup.

## Ring active cleanup and corrections

- `RingLockActiveService.CloneLockedToActive` handles a non-ring locker by probing the locked ring's unsolved `RingRelative` handlers and resolving overlap colliders to the target `PartId`; it does not call the two-ring `RingsStillConnected` check on that branch. `LockerStillOverlapsLockedRing` uses the same relative-probe direction during pruning. See `Research/rules/decompiled/RotateRings.GamePlay.Services/RingLockActiveService.cs:80-151,348-394`.
- `RingsStillConnected` is specifically a two-ring path: it first checks `BothRingsStillOverlap`, then `LockFeature.ArePartsConnected`. See `RingLockActiveService.cs:399-421`.
- A reimplementation should keep distinct paths: (1) ring-source relative probe(s) against the PullObject target body for active ring-lock clone/prune; (2) PullObject overlap-handler probe against the registered ring feature colliders for passive binding build/cleanup. Do not gate a ring-to-Pull link on a both-rings predicate, and do not reuse one direction's query mask for the other.
- The primary source500 parity pack records the Pull body shape but omits PullObject's serialized `overlap2DHandler` fields. Those are captured from normalized raw components in the supplement catalog: all PullObject records use `StickBody` shape 0, trigger-inclusive `ContactFilter2D`, layer mask bit 128, and `maxOverlapCollider: 10`. `inputShapes` must not be substituted for the handler data.

## Bomb ownership note

`PullObject.ResolveThreadingTarget` resolves an overlapping bomb collider to `RingObject` or `PullObject` only when that collider is a child of that gameplay object; a standalone bomb has no gameplay `PartId` and resolves to null. `BombRelative.ResolveHostGameplayObject` uses the same parent-root pattern (`Research/rules/decompiled/BombRelative.cs:425-445`). Source500 has 179 bomb records, including 21 with `ownerPartId: -1`; attached bombs use nonnegative part IDs. A generic overlap-to-part implementation must preserve owner-root resolution and must not treat every bomb collider as a RingObject.
