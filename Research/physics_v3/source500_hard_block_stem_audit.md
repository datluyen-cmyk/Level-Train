# Source500 hard-block stem audit

Read-only comparison of every source500 ring template's `physicalShapes` against each part's `sourceColliderBindings` and serialized `sourceNativeData.hardBlockColliders` references.

## Result

- Source pack: 500 levels, 7,295 ring parts.
- Physical `role: stem` shapes: **8,257**.
- Stems whose binding maps to `sourceRole: HardBlockPin` and whose exact source collider ID appears in `hardBlockColliders`: **8,257 / 8,257**.
- Serialized hard-block collider references: **8,257**.
- Serialized references mapping back to a physical `stem` with `sourceRole: HardBlockPin`: **8,257 / 8,257**.
- Missing bindings, omitted stems, extra non-stem hard-block IDs, and mismatches: **0**.

Across this source500 corpus, `role: stem` is an exact match for the native `RingObject.hardBlockColliders` set. The runtime can use either the explicit serialized list or this verified role mapping; it should preserve the hard-pin check as its own unconditional ring-interlock rule.

## Input hashes

- `RotateRings Test/LevelTraining_SourceLayoutV3/data/source500_native.json`: `effda7077f6d709f4afb977656decd6476adf5c8a2879792958bb040f29a218f`
- `RotateRings Test/LevelTraining_SourceLayoutV3/data/source_templates.json`: `c44adce33201c46d8dbf5e827de719794589bf89b64943a569bb0265be1b89ee`

## Reproduction method

For each ring part, resolve every binding by `shapeIndex` into its template's `physicalShapes`; compare all shapes with `role == stem` to the part's serialized `hardBlockColliders` path IDs. Then resolve every serialized hard-block path ID back to its binding and shape. The audit used source collider IDs as the join key and checked both directions.
