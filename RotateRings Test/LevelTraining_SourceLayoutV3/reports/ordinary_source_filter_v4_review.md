# Ordinary source filter v4 curation review

Status: curated source-ID list ready for the isolated ordinary-v4 run. This file records review decisions; it does not certify old models or claim difficulty labels. The run builder must materialize the filtered native pack and bind its hash as `sourcePackSha256` before the guard permits training. Historical mixed models remain uncertified.

## Partition

- **460 ordinary source500 originals retained**: 449 with no confidently named special silhouette in the prior contact-sheet audit; 4 regular grid/lattice structures; and 7 abstract maze/network structures retained after exact-solid review.
- **47 pictorial levels excluded**: 40 from source500 and 7 additional native scenes (10210–10216).
- **0 quarantined IDs** after reviewing the seven extra scenes.
- **507 total source scenes novelty-excluded** for candidate-vs-source comparison: all 500 source500 originals plus the 7 locally available extras.
- The source request for 500 training sources is **unmet**. Maximum curated unique ordinary source count is 460. The prescribed output sample count is 300, selected as the first 300 eligible source500 originals in original position order; their original source positions are preserved and are not renumbered. No progression order is inferred for the extra seven.

## Source500 pictorial exclusions

| Position | Source ID | Review reason |
|---:|---:|---|
| 20 | 10109 | Face with eyes and central facial features |
| 30 | 10157 | Explicit diamond/triangle whole-board figure with internal arrangement |
| 40 | 10042 | Vehicle outline with wheel-like circles and frame |
| 80 | 10072 | Rocket with nose, body, fins and exhaust |
| 100 | 10160 | Face/sun with facial features and crown rays |
| 110 | 10125 | Butterfly with paired wings and central body |
| 120 | 10159 | Rectangular camera/device icon with central lens |
| 130 | 72002 | Gift box with bow |
| 140 | 10158 | Sailboat with mast, sails and hull/water bands |
| 150 | 10156 | Snowflake/starburst emblem |
| 170 | 10155 | Row of houses/buildings with roofs and openings |
| 180 | 10028 | Robot/device face with antenna stalks and eyes |
| 190 | 10095 | Face with eyes, mouth and rays |
| 260 | 72004 | Flower/sunburst with structured base |
| 280 | 72014 | Six-fold star emblem |
| 289 | 60061 | Monument/church-like outline |
| 290 | 60047 | Winged/person-like figure |
| 296 | 60043 | Radial flower figure with connected interior |
| 300 | 72005 | Umbrella with shaft/handle |
| 306 | 60092 | Diamond emblem inside circular outline |
| 310 | 72007 | Four-fold star/flower with radial arms |
| 316 | 10193 | Radial emblem with nested geometric figures |
| 320 | 72006 | Sunburst emblem with central hub |
| 336 | 73010 | Circular geometric emblem with internal network |
| 340 | 10208 | Dashboard/device-like panel with repeated bands and controls |
| 350 | 10207 | Standing person with head, torso and limbs |
| 360 | 60044 | Person/face figure with body and limbs |
| 366 | 10192 | Diamond badge with crossed central geometry |
| 380 | 72011 | Crown with pointed top and base |
| 386 | 60091 | Framed medallion with central face/radial figure |
| 390 | 60080 | Face/mask arrangement |
| 400 | 10206 | Smiling face with surrounding rays |
| 420 | 100005 | Robot/astronaut-like figure with central body |
| 429 | 880003 | Lightbulb outline with filament and base |
| 430 | 140004 | Radial flower/atom pictogram with center and satellite nodes |
| 460 | 90022 | Sailing ship with sails, mast and water bands |
| 480 | 90024 | Train/locomotive with cab, wheel and track base |
| 490 | 150005 | Butterfly with paired wings and central body |
| 496 | 90015 | Diamond/hexagon emblem with central radial spokes |
| 500 | 150013 | Split scene with two faces above a house/building |

This classification excludes recognizable figures, scenes, objects and emblems. It does not reject a level merely for being symmetric, radial, grid-like or graph-like.

## Source500 labeled structures retained

| Position | Source ID | Reason retained |
|---:|---:|---|
| 14 | 60023 | Regular 3×4 grid |
| 21 | 10111 | Parallel-corridor maze; no recognizable pictorial subject |
| 34 | 10050 | Regular row-and-column lattice |
| 38 | 60008 | Square grid/maze arrangement |
| 72 | 60001 | Regular repeated row-and-column arrangement |
| 286 | 73009 | Abstract linked-node network; no recognizable figure silhouette |
| 346 | 73008 | Abstract connected network diagram; no recognizable figure silhouette |
| 356 | 10191 | Abstract triangular network; no recognizable figure/emblem subject |
| 436 | 73014 | Abstract central-node network; no recognizable figure silhouette |
| 443 | 71147 | Abstract radial network diagram; no recognizable figure silhouette |
| 446 | 73012 | Abstract two-hub network; no recognizable figure silhouette |

Ordinary radial or graph arrangements are retained only where review sees no recognizable figure, object, scene or decorative emblem. A radial figure or emblem is excluded.

## Additional native scenes 10210–10216

These scenes are native exact-solid exports in the local novelty extras pack. They were reviewed from `visual_qa/extras_10210_10216_native_montage.png`; none is treated as ordinary. Scene IDs do not establish progression positions.

| Scene ID | Decision | Observed subject |
|---:|---|---|
| 10210 | Exclude pictorial | Smiling face/sun character with rays and decorative accessories |
| 10211 | Exclude pictorial | Stylized flying bird/winged animal composition |
| 10212 | Exclude pictorial | Rocket-like object with trail and starburst decorations |
| 10213 | Exclude pictorial | Ghost character with face, arms and scalloped body |
| 10214 | Exclude pictorial | Cat character wearing a pointed hat and bow |
| 10215 | Exclude pictorial | Bat character with face, crescent moon and stars |
| 10216 | Exclude pictorial | Jack-o-lantern face with candy/lollipop-like decorations |

## Evidence and hashes

The exact per-source curation list and provenance are in `ordinary_source_filter_v4.curation.json`. It binds the source500 catalog, source templates, original source manifest, all-level catalog, summary, seven extra scene JSONs, source500 motif inventory and contact-sheet manifests. Current curation JSON SHA256: `c69c771e734d5b866447cbc2fcf7e0c9d7262b2cff1d1e5903b031b68cdd5250`. Its `sourcePackSha256` is intentionally pending: the parent run builder will bind the filtered-pack hash after writing the isolated run corpus.

Exact-solid review sheets: `visual_qa/source500_review_queue_montage_v2.png` and `visual_qa/extras_10210_10216_native_montage.png`. These are local diagnostic renders, not Unity screenshots.
