# Source collision and binding contract, revision 2

The v1 300-level pack was validated only by a bearing graph. It is **not physically certified**. Revision 2 uses actual native collider paths from all 500 original progression scenes; template geometry is immutable and link edges do not create render geometry.

## Authoritative flags and geometry

`RingColliderBuilder.BuildPhysic` assigns the ring body PolygonCollider2D (nontrigger) and creates a separate nontrigger `HardBlockPin` polygon for every radial attachment. `SpawnRingRelative` creates a `RingRelativeBody` BoxCollider2D trigger for T-bar catch sensing plus two nontrigger thin `RingRelativeWall` boxes. Level10001's actual serialized shapes are retained in `Research/rules/level10001_native_colliders.json`: stem width .4166667; head trigger size (.6708384,.6833334); walls size (.05,.6833334), offsets Â±.375. Source layer6 DragArea is excluded from physical collision.

`RingAttachment.AttachType` is **HeadedBar=0, TBar=1**. The source renderer enum must not be reversed. Each entire original ring item, including all attachments, physical polygons, visual dimensions, physical and visual gap, remains one template. Native physical band width .5 at radius1.75 differs from serialized renderer width1. These are separate fields.

`BuildRadialPinStemPath` (RingColliderBuilder.cs988â€“1002) uses native draft angle `-MountAngle`, outerMount=MountSide0, width=max(.01,2*StemRadius*coordScale), startR=outerRadius or innerRadius. TipR=centerRadiusÂ±StemLength*coordScale; inner clamps0. Native draft Kind1 additionally extends tip by width/2+.125âˆ’.03 and rounds it. `SpawnRingRelative` (923â€“968) places head center at centerRadiusÂ±(stemLength+headLength/2), trigger dimensions(headLength,max(.01,headWidthâˆ’.15)), wall dimensions(.05,headWidthâˆ’.15), radial wall offsetsÂ±(headLength/2+.0395808). Exact polygons supersede any approximate formula in the runtime export.

## Contacts and release

`RingObjectSolver.CanSolve` (7â€“24) requires no active outgoing lock targets and no active passive ring bindings. Reward eligibility `CanCollect` does not decide release or victory. `RingLockActiveService.CloneLockedToActive` retains original links only while their source/target colliders are still connected; target removal disables its colliders. A pin shape belongs to its **source owner** and remains until that owner is solved, even when the head contact becomes unlocked or a target disappears.

Head triggers sense bindings and are not solid obstacles. A target body may overlap its designated source head trigger. Solid stems cannot be ignored because their associated logical edge became open. Non-designated new contacts cannot be silently admitted. For current generated content, roles `body`, `stem`, `head_wall` are physical, `head_trigger` is binding, and `guide` is reserved for verified supported fixtures.

## Original rotation guard

`RingObject.HandlePointerDown` (642â€“669) caches starting external collider IDs and maximum penetration against external, hard-block-stick and partner-body geometry. `IsBlockedForRotation` (1227â€“1272) rejects new external obstacles, external penetration >start+.02, hard-block overlap, partner-main-body penetration >start+.02, new unbound interlocks, and bomb contacts. `TryResolveHardBlockInterlockOverlap` (1556â€“1608) rejects any valid negative-distance hardstem-to-other-ring collision. `MeasureHardBlockStickPenetration` uses an extra .03 clearance for sticks.

Static passive bindings are collider/source specific, not an all-part whitelist. `RingPassiveBlockCheck.IsInBinds/IsSourceCompatible` checks exact bound collider and RingRelative source. A bound nonwall collider can remain exempt while the partner exists. Partner walls receive the exemption only while the partner remains within **5 degrees of its captured baseline angle** (`IsPartnerInAngle`, line241). This supports existing authored threading; it does not allow a new stem or unrelated body to cross a ring.

`DragRotateObject.FindLastValidRotationZ` (379â€“425) probes both endpoint and path, then binary searches up to12 iterations until interval<.5degrees. `ProbeRotationPathBlocked` samples none for<3degrees, midpoint for3..8degrees, and two interior fractions forâ‰¥8degrees. `RingRotateGuardConfig` defaults max step4degrees, max finger delta35degrees. RingObject backs off .1degrees after a stop. Revision2 generator verification may conservatively sample everyâ‰¤.5degrees; that is a stricter generated-content gate, not a claim of identical original numerical physics.

## Export schema

`physicalShapes` use part-local coordinates with gap direction +X; the runtime applies part x/y and angle. `paths` is an array of `{points:[{x,y},...]}` wrappers because Unity JsonUtility cannot read jagged arrays. `points` repeats first path for convenience. Multiple paths retain original ring holes. `attachmentId` is the zero-based source attachment ordinal, bodyâˆ’1. `sourceAttachmentId` retains native feature naming. `attachments` keep the original renderer field names, integer type enum and boolean isInnerMount. Root visualRadius/visualWidth/visualGap/capRatio/extraEnabled/styleIndex preserve renderer settings independently of physical radius/width/gap.

All500 scenes are inspected and retained. Unsupported butterfly, bombs, fixtures, chains, palettes and complex sliders are explicitly rejected from physical labels until their own rules are implemented. Structural training statistics can include all500; solvability training must identify its smaller supported subset honestly. Source replay and generated replay remain separate evidence.

## Verified Unity kernel clearance (2026-11-05)

The original raw polygon gap between pilot part1/body and part5/body is `0.005447619155374216`; live Unity `Collider2D.Distance` returns `-0.01455232`. The difference is `0.02`, matching a `0.01` polygon radius on each side. Python tests raw geometry distance `<0.0195`, corresponding to Unity penetration `<-0.0005`. This applies to solid collisions and head contact tests. The first pilot is retained as a failing regression; `pilot_level_skin.json` and `unity_pilot_skin_replay.json` record the corrected seven-piece/six-gesture pilot passing the real engine.

The new-content subset permits a `head_trigger`/`body` contact only for the exact authored source-owner, zero-based attachment ordinal and target-owner triple. Head triggers may never overlap a foreign stem, head wall or another trigger in this subset. Initial poses and every swept pose use the same guard. These conservative restrictions do not invent source mechanics or certify every original authored exception.
