# Saved level generator

Run **Open Generator.vbs** on this Windows machine. Choose count, seed, output parent and previous packs to exclude. The generator creates a new batch directory; `generated_levels.json` contains the adapter data and `unity_levels.json` is the Unity import format. A complete accepted batch has a hash-bound `validation_report.json`. A stopped/incomplete run keeps `partial_levels.json` and must not replace the Unity pack.

The bundle contains 500 distinct original corpus stages, 300 ordered reference targets, the empirical proposal, ExtraTrees difficulty predictor, pinned dependencies, adapter, frozen runners, GUI and reports. It is not a neural generator. Corpus `CanCollect` flags were correctly interpreted as scoring eligibility in parser1.1; every original movable ring/slider is a removal goal. Earlier pilot labels were invalidated and retrained.

Training is reproducible with:

```powershell
python tools/level_training.py --config level_training.json --train-only
```

Later generation without refitting:

```powershell
python tools/generate_batch.py --config level_training.json --output batches/my-new-pack --count 300 --seed 20261106 --exclude batches/final-300/generated_levels.json
```

Use a NEW output path. The default wrapper automatically excludes the initial final-300 pack. A new seed does not guarantee enough unseen tutorial states; the generator reports INCOMPLETE rather than lowering gates. To use a different interpreter, install `requirements.txt` and run `Open Generator.pyw`; the VBS launcher records this machine's training Python path.

The exact original native simulator and human difficulty remain unmeasured. See the project playtest README and `model_validation.json` for the distinction between source structure, reduced-rule solvability and human behavior. Whole source graph families were separated before augmentation, with an independent audit checking that the executed synthetic labels contain no held-out-equivalent graphs. Keep `models/manifest.json`, corpus, references, adapter and config intact; generation checks their hashes before loading trusted local joblib files.
