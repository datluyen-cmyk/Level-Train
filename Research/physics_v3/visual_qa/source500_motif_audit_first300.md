# source500 whole-board motif audit: positions 1–300

Review source500 positions 1–300 using the exact solids renderer. Panels are labeled with source ID and source position in the sheets under `source500_motif_sheets_first300/`; the manifest maps all panels. This is a visual audit of silhouettes, not a semantic/content truth set. Small/open components can make a board suggest an object without fully depicting it.

## Observed whole-board silhouettes

| Position | Source ID | Observed silhouette | Confidence |
|---:|---:|---|---|
| 20 | 10109 | Face-like layout: head contour, paired eye details, central nose/mouth | High |
| 30 | 10157 | Diamond/triangle outline with a central interior arrangement | High; intentional per apk_rules |
| 40 | 10042 | Vehicle-like outline with two wheel-like circles and connecting frame | Medium |
| 80 | 10072 | Rocket, with pointed nose, central body, side fins, and exhaust detail | High |
| 100 | 10160 | Face/sun: circular face, facial features, and radiating crown rays | High |
| 110 | 10125 | Butterfly with paired wings, body, and antenna-like details | High |
| 120 | 10159 | Rectangular device/camera-like frame with circular center and protruding controls | Medium |
| 130 | 72002 | Gift box with bow and wrapped box body | High |
| 140 | 10158 | Sailboat: mast, sail shapes, and horizontal hull/water bands | High |
| 150 | 10156 | Snowflake/starburst with six-fold radial arms | High |
| 170 | 10155 | Row of house/building outlines with pitched roofs and openings | High |
| 180 | 10028 | Boxy robot/device face with antenna-like stalks, eye circles, and side controls | Medium-high |
| 190 | 10095 | Round face with two eyes and mouth, with short rays around the head | High |
| 260 | 72004 | Radial flower/sunburst around a center, with a structured base | Medium-high |
| 280 | 72014 | Circular six-fold geometric/star motif | High |
| 286 | 73009 | Molecular/atom-like diagram: linked nodes around a central hub | Medium-high |
| 289 | 60061 | Tall monument/church-like silhouette with peaked top and base | Medium |
| 290 | 60047 | Symmetric winged/person-like outline with a central body | Medium |
| 296 | 60043 | Radial polygon/flower outline with connected interior nodes | Medium-high |
| 300 | 72005 | Umbrella-like upper canopy and central shaft/handle | Medium |

## Inferred clusters for review

These are candidate families inferred from the observed silhouettes. They are useful for preserving shape intent during donor/learner generation, but need native-root or source-family confirmation before being treated as labels:

- **Face/head layouts:** positions 20/source 10109, 100/source 10160, and 190/source 10095. Similar semantic silhouette, distinct internal and outer details.
- **Radial/symmetry motifs:** positions 150/source 10156, 260/source 72004, 280/source 72014, and 296/source 60043. Share radial structure; do not assume interchangeable geometry.
- **Built-object outlines:** positions 120/source 10159, 130/source 72002, 140/source 10158, 170/source 10155, 180/source 10028, 289/source 60061, and 300/source 72005. These read as specific objects, rather than ordinary scattered-piece boards.
- **Grid/lattice-like boards:** positions 14/source 60023, 34/source 10050, 38/source 60008, and 72/source 60001 show repeated regular rows/columns or grid/maze structure. Their apparent regularity is itself an observed structural feature; it should not be collapsed into a generic high-part-count category.

## Handling guidance

Preserve intentional outer contours and salient interior structure for these boards. Vary meaningful internal obstructions only where the source retains the motif. Do not infer a special motif from counts, density, or component classes alone. Keep additional inferred groups separate from observed labels until verified against native source roots.

## Evidence

- Contact sheets: `Research/physics_v3/visual_qa/source500_motif_sheets_first300/`
- Manifest: `source500_positions_001-300_manifest.json`
- Renderer: `Tools/render_layout_diagnostics_v3.py`
- Sheet generator: `Tools/render_source500_motif_sheets.py`

## Source 10157 / pilot 7953000 role note

The annotated Pull-root overlay is `Research/physics_v3/visual_qa/source30_diamond_root_ids.png`. apk_rules' saved-certificate comparison says the pilot changes Pull roots 27, 7, 33, and 25, each paired with guide roots 29, 22, 34, and 26. That is 8 of 34 total roots: three visually peripheral rods (27, 33, 25) and one upper-right sloping core/contour rod (7). Interior-facet roots 2–6 are unchanged. The pilot therefore does **not** demonstrate whole-shape preservation or meaningful interior variation; treat it as research-only evidence. The proposed broad partition (peripheral 25,27,28,31,33 vs central roots 1–12) is provisional: root 1's role and the exact contour/core split need confirmation from source-family/native structure. Do not turn this observation into a hard game rule.

## Additional positions 301–500

The same visual pass identified further candidates in the second contact-sheet set. Strong examples include 320/source 72006 (radial sunburst), 346/source 73008 and 436/source 73014 (linked-node networks), 350/source 10207 (standing person), 380/source 72011 (crown), 400/source 10206 (smiling face/sun), 429/source 880003 (lightbulb), 480/source 90024 (train/locomotive), 490/source 150005 (butterfly), and 496/source 90015 (diamond/hexagonal radial motif). The full position-level record, with empty labels retained where no confidently named motif was seen, is `source500_motif_inventory_first500.json`.
