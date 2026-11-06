# Curated ordinary refit

460 unique ordinary original levels were used; requested500 remains unmet because the available507-source APK inventory includes47 excluded identifiable pictorial levels. No duplication or padding. The exact source/filter and local trainer dependency bytes are bound to the final refit certificate. Historical models were never loaded.

All three proposal models were evaluated against declared simple baselines on five separated source-family folds before final refitting. Post-fit source membership and exact artifact bindings pass.

| Model | Learned mean NLL | Baseline mean NLL | Learned reconstruction | Baseline reconstruction |
|---|---:|---:|---:|---:|
| Local native relation pose | 5.05234 | 9.46129 | 0.31553 width units | 0.66518 width units |
| Native macro relation | 50.56106 | 50.36681 | 3.10791 width units | 3.15404 width units |
| Anchored guide relative angle | 2.11428 | 1.51186 | 3.02269 degrees | 4.27796 degrees |

Reconstruction reports oracle best-of32 candidates, not one-sample prediction. Local pose improves both held-family measures. Macro and anchored-guide NLL are worse than baseline, while oracle coverage improves; neither demonstrates overall model superiority. Anchor pairs exist in334 of460 inspected ordinary sources (5883 records). Local pose has10249 relations; macro has3628 tiles/7244 intertile relations.

Training took46.15seconds. No generated level, native solve, calibrated difficulty, player enjoyment or export was accepted by this stage. Generation configuration remains disabled pending main's subsequent proposal/replay workflow. Source filename source500_native.json is compatibility naming and not a500-source claim.
