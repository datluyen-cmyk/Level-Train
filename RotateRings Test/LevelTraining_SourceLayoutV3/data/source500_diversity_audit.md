# Full 500 original level diversity audit

All 500 original progression entries are retained. This inventory is static source evidence, not original runtime parity or solved labels.

| Native item | Count | Levels containing |
|---|---:|---:|
| RingObject | 7295 | 495 |
| PullObject | 3982 | 406 |
| FixedObject | 3339 | 395 |
| FixedButterflyObject | 62 | 26 |
| BombRelative | 179 | 125 |
| chainLockedParts | 99 | 49 |
| paletteLockParts | 0 | 0 |
| paletteKeyParts | 0 | 0 |
| guideLinks | 3463 | 402 |
| ringLinks | 7918 | 480 |

Pull geometry: 1929 roundedL, 690 roundedU, 1363 straight. All adjacent path turns are ±90°.

Pull movement: 809 native type0 transform.up; 3173 type1 -transform.right. Both signed directions are allowed; the old axis=rotation projection was wrong.

All 14678 movable and fixture root transforms have positive uniform XY scales. Native Polygon/Box/Circle collider classes and source IDs are preserved by V3.

## Why the previous subset had only 62 scenes

437 scenes flagged by the reduced normalizer; 63 scenes unflagged; V2 all-ring eligibility also dropped source140004 (11rings+2straightPulls), leaving62. Nine further attachment cases were later excluded. This is implementation policy, not a native restriction.

These gaps cannot justify excluding 438 source levels, simplifying every shape to a ring, or learning original difficulty from the subset. Representative untouched source parity is required before training labels.

## Logical Pull feature aliases

The source has one actual activeCollider per Pull. All authored Pull endpoint strings are body or attachment.bent_N.solid. V3 keeps these strings as aliases to that actual collider. PullToRingLinkRegistrar resolves only the Ring endpoint feature when making passive bindings; it does not subdivide the Pull collider.

## Current gate

No fresh300 accepted pack exists. V2 remains a rejected pilot. Mixed shapes, catches, butterflies, bombs, chains, per-feature passive bindings and original signed movement must be validated before source labels, training or generation.
