# Original difficulty extraction V7

Reads the actual shipped Main-mode TbLevelDifficult binary table (table index 55), with exact Brotli/MemoryPack consumption and insertion-order first-match resolution. Does not infer labels from root counts, solver segments or the unrelated core difficulty fields.

Reproduce from D:\RotateRings:

```powershell
python Tools/extract_original_difficulty_v7.py --root D:\RotateRings --workspace "D:\RotateRings\RotateRings Test\LevelTraining_SourceLayoutV3\runs\ordinary_v4" --output "D:\RotateRings\RotateRings Test\LevelTraining_SourceLayoutV3\runs\ordinary_v4\reports\original_default_difficulty_v7.json"
python -m unittest discover -s Tools -p test_extract_original_difficulty_v7.py -v
```

Ordinary references: 460 = 365 Normal / 50 Hard / 45 VeryHard. First 18 default Main positions are Normal. Runtime LevelRandom experiment enablement is unverified; its conditional fallback is a separate column and never substitutes for default tags. Tags are designer/UI intent, not calibrated human difficulty.

Historical checkpoint71 cross-join: only VeryHard position49/source60011 has a solved and replayGoal witness, with32 solver action segments under historical rule1aaa48490ae656c86f1627f1f372589605ce972bdff9c0dc2502d052eac56571. Current d5f8... replay is required before any current solved claim. All other completed Hard/VeryHard rows are unknown, not unsolvable. Full95-tag join and evidence hashes are in reports/original_difficulty_historical_witness_join_v7.json; tool hashes in models/original_difficulty_extraction_checkpoint_v7.json.

Generation is frozen. No Unity or Assets changes. No newly accepted generated levels.
