# Proposed original ring mesh submission-order correction

Status: review-only patch. It is outside `Assets`; do not apply until the source-460 baseline capture is complete. No Unity/runtime calls were made for this audit.

Patch: `proposed_ring_mesh_submission_order.patch`
SHA256: `49c42c92bfe4b75f3c9798027327d658da89db7d8e4c5345f687aa9c45b29bcc`
Target: `RotateRings Test/Assets/RotateRings/Original3D/OriginalRingVisualAssets.cs`

## Source proof and impact

Decompiled `XingYun.RingGame/RingSegmentController.cs` builds the mesh in this order: all attachment stems (`AppendAttachmentStems`, around lines 659–681), ring arc and endpoint caps (around 559–617), then all T-bar crossbars (`AppendAttachmentCrossBars`, around 683–755). The current helper creates the arc first, then interleaves each stem with that attachment's crossbar (`OriginalRingVisualAssets.cs`, `CreatePieceMesh`). The proposed patch makes the call order stems → arc/caps → crossbars, matching that source sequence.

The authored source material is `RingExtra`, shader `HsApp/Effect/XYStyleLine`. The local shader declares `Queue="Transparent-9"`, `ZWrite [_ZWrite]`, `ZTest [_ZTest]`, `Blend [_SrcBlend] [_DstBlend]`. The extracted material values are `_ZWrite=1`, `_ZTest=4` (LEqual), `_SrcBlend=5` (SrcAlpha), `_DstBlend=10` (OneMinusSrcAlpha), and `_AlphaClipEnable=1`. Templates used by source levels serialize ringZ=0 and stemZ=0, so pin roots and ring overlap at equal depth. Under these settings later equal-depth fragments can pass LEqual, blend, and write depth. Submission order can therefore affect overdraw at pin roots; this is not only a bookkeeping difference.

The material catalog's `_RenderQueue=3000` is a shader float property. It does not record Unity's separate `Material.renderQueue` field. The shader default queue is Transparent-9; an original material override to effective queue 3000 is not proven by the local extracted catalog. This uncertainty does not change the equal-Z order finding within the mesh.

## Static preservation verification across source500

A source-data-driven emission-call audit compared the current call sequence with the proposed sequence across every ring instance in `full500/source_parity_pack.json` using `full500/source_templates.json`:

- 500 source levels, 7,295 ring instances, and 5,005 distinct ring templates.
- 8,257 attachment stem calls: 334 headed-bar and 7,923 T-bar crossbar calls.
- 23,475 total geometry emission chunks on each side.
- The multiset of per-instance chunk inputs is identical before and after reordering (SHA256 `8a0739a8bf512f81b241ccfc74ce609eed92418b47a5a76b346c2c2e5aae58ab`).

The patch only moves calls; it leaves `AppendSourceArc`, `AppendStraightRibbon`, `AppendCrossBar`, vertex creation, UV/color/Z generation and triangle construction untouched. Since each geometry helper receives the same arguments, each chunk's vertex payload and local triangle winding are unchanged. Its global vertex offsets and draw order necessarily differ to match the native sequence. Source data hashes: pack `effda7077f6d709f4afb977656decd6476adf5c8a2879792958bb040f29a218f`; templates `c44adce33201c46d8dbf5e827de719794589bf89b64943a569bb0265be1b89ee`.

This check is a static call/input multiset proof, not a Unity render comparison. Native baseline render evidence should be captured before applying the patch; follow with the planned source-460 native visual QA.

## Motion evidence freshness

The current `Research/motion_qa/motion_qa.json` says `NOT_RUN_AFTER_DURATION_CORRECTION`. The separate `Research/physics_v2/motion_qa/motion_qa.json` contains a prior PASS, but it is not current post-correction verification. Queue that Unity motion check after the source-460 baseline batch; do not infer a motion mismatch from the stale status alone.
