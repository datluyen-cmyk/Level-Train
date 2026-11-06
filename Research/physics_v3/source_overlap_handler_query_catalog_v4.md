# Source overlap handler catalog v4

The v4 extraction supplements the existing v3 ring handler catalog with serialized `RingRelative.selfCollider` and `blockColliders` mappings. The original v3 JSON remains untouched to preserve its prior source/extractor hashes.

## Coverage and audit

- 500 source levels, 7,295 ring parts, 7,918 authored ring links, and 7,923 `RingRelative` handlers. Link count and handler count are separate quantities.
- 7,295 `RingObject` handlers and 7,923 `RingRelative` handlers map to 12,235 per-template query shapes across 5,005 templates.
- No template contract conflicts or unresolved references; `safeForRuntimeImport` is true.
- Every `RingRelative.selfCollider` equals its `Overlap2DHandler.collider2D` and maps to the same `shapeIndex` (7,923 / 7,923). Every self collider is also present in its serialized `blockColliders` list (7,923 / 7,923).
- Each `RingRelative` has three serialized block collider references: one eligible `Top` wall probe, one trigger self collider excluded from wall probing, and one `Bottom` wall collider excluded by name. Across all relatives: 7,923 eligible wall probes, 7,923 trigger exclusions, and 7,923 `Bottom` exclusions. No disabled or layer-6 exclusions occurred in source500.

Wall eligibility applies the native `RingObject.CollectOverlapCollidersFromWalls` gates: collider enabled, non-trigger, not on layer 6, and transform/GameObject name other than `Bottom`. The raw GameObject name/layer are retained per collider; physical enabled/trigger fields come from the source template shape. Full `blockShapeIndices` remains present separately from eligible `wallShapeIndices`, so source compatibility can test `ContainsPhysicalBlockCollider` even for the trigger self collider.

Each relative observation preserves its source-relative component ID, relative index, `featureId`, self collider ID/index, complete block collider IDs/indices, wall indices, and per-reference eligibility/provenance. Component/collider IDs remain observations rather than reusable contract identity, so identical template contracts do not conflict solely because source scene IDs or pin feature labels differ.

## Artifacts

- Extractor: `Tools/extract_overlap_handler_contracts_v3.py`
- Catalog: `Research/physics_v3/source_overlap_handler_query_catalog_v4.json`
- Stem/hard-block parity audit: `Research/physics_v3/source500_hard_block_stem_audit.md`

The separate stem audit confirms that all 8,257 physical `stem` shapes map exactly to serialized `hardBlockColliders` IDs and `HardBlockPin` bindings; it found no omissions or extras.
