# Source mechanism research: VeryHard positions 39 and 49

## Scope and evidence

This report reads the saved programmatic gesture traces for source 70033 (progression position 39) and source 60011 (position 49). The runs exercised the live runtime's collision clamps, motion samples, release, cascade and goal logic. They did not exercise OS pointer/raycast input, do not prove shortest solutions, and are not human difficulty ratings.

The shipped difficulty labels come from [original_default_difficulty_v7.json](</D:/RotateRings/RotateRings Test/LevelTraining_SourceLayoutV3/runs/ordinary_v4/reports/original_default_difficulty_v7.json>), whose status is `SHIPPED_DEFAULT_DIFFICULTY_RULES_EXTRACTED_NO_PLAYER_CALIBRATION`. It assigns source 70033 / position 39 and source 60011 / position 49 **VeryHard**. The playtest harness's `difficulty` float is `0.0` for these original levels; use the extracted table for their labels. Their live preview-list indices were 37 and 46 respectively after corpus filtering, so those indices are not progression positions.

## Direct observations

### Source 60011 / position 49

[The counterfactual report](</D:/RotateRings/RotateRings Test/LevelTraining_SourceLayoutV3/runs/ordinary_v4/playtests/source_design_v7/source60011_veryhard49_counterfactual.json>) contains an initial one-move trial, a restart, then a goal-reaching path of 32 moves. The initial action, part 11 rotated by −101.56°, removed part 14 immediately. After the restart, the successful path later used that same part 11 rotation at step 6, also removing part 14.

After restart, steps 3–5 moved part 17 by +24.862°, part 17 by +44.815°, and part 18 by −69.677°. None removed a part or recorded a collision blocker. Authored-contact counts changed during these motions, showing geometry-dependent contact changes while the pieces remained present. The trace therefore disproves the narrow claim that these preparations are required to make part 11 remove part 14. It does not establish whether an alternate continuation can solve the whole puzzle without them.

The saved goal path has 32 moves, but the report provides one successful path, not a minimum. The 15 non-removing gestures in that path should not be counted as mandatory steps from this evidence alone.

### Source 70033 / position 39

Three completed paths are available: [25-move exploration](</D:/RotateRings/RotateRings Test/LevelTraining_SourceLayoutV3/runs/ordinary_v4/playtests/source_design_v7/source70033_veryhard39_after_restart_v7.json>), [21-move counterfactual](</D:/RotateRings/RotateRings Test/LevelTraining_SourceLayoutV3/runs/ordinary_v4/playtests/source_design_v7/source70033_veryhard39_counterfactual_v7.json>), and [clean 17-move witness](</D:/RotateRings/RotateRings Test/LevelTraining_SourceLayoutV3/runs/ordinary_v4/playtests/source_design_v7/source70033_veryhard39_clean17_v7.json>). Each reaches the goal through native collision and release logic without injected removals. The move-count spread shows that exploration path length is not intrinsic board difficulty or a lower bound. The 17-move witness still includes three accepted no-removal parking moves (steps 1, 2 and 7); those are part of this witness, not proven necessities.

The clearest mechanism result is in counterfactual steps 11–12. Part 7's +150° rotation stopped at 308.087° against part 21's `body`; part 8's −150° rotation stopped at 234.178° against part 20's `body`. Neither move removed anything. At that time the active authored contacts were in separate groups: 3–7 and 6–8 for the rotating rings, 19–20 and 21–22 for the bottom sliders. There was no authored pin/guide edge between these groups. Counterfactual steps 20–21 then slid part 21 and part 20 out, removing their paired parts 22 and 19. The next rotations of parts 7 and 8 succeeded and removed parts 3, 7, and 8. This is direct evidence that an authored-binding graph alone misses physical body obstructions between otherwise disconnected authored groups.

Other logged blockers give concrete parking constraints: source 70033's clean witness records part 11 blocked by part 5 (`body`), part 10 by part 1 (`head_wall`), part 12 by part 11 (`body`), part 10 by part 13 (`body`), part 13 by part 6 (`body`), and part 14 by part 13 (`body`). Partial movement can still remove parts in the same gesture, so a blocker is a sampled physical limit, not necessarily a failed action.

## Refuted claims and limits

- **“The bottom slider group is independent because it has no authored edge to rings 7–8.”** Refuted for source 70033: bodies 21 and 20 physically stop those ring rotations, and removing the bottom group precedes successful retries.
- **“Parts 17/18 must move before part 11 can remove part 14.”** Refuted as stated for source 60011: the initial p11 action removes p14 before those preparations. Whether p17/p18 are needed for a full continuation remains open.
- **“Every no-removal action is mandatory.”** Not established. Source 70033 has goal-reaching paths of 25, 21 and 17 moves, and source 60011's 32-move witness includes many non-removing motions. No trace here is a minimality proof.
- **“A path's move count is the shipped difficulty value.”** Refuted by provenance: shipped labels are progression-rule outputs. The extracted report explicitly has no player calibration.

## Working hypotheses for further review

1. Treat physical clearance as an interaction layer alongside authored pins and guides. Record blocker owner and shape role for failed and partial motions; graph reachability on authored contacts alone is insufficient.
2. Treat unremoved sliders/rings as candidate parking geometry. Vary their placement and action order in fresh restarts, then compare downstream reachability and exact blocker records before describing a move as necessary.
3. For source 60011, test full continuations from the initial state after omitting p17/p18 parking, not just whether p11 can remove p14. The initial p11 trial established removal only.
4. Confirm these routes with visible input before making claims about human affordance, perceived difficulty, or shortest solutions.

## Suggested next boards

These are review candidates from the retained 460-level ordinary source corpus. Their tags and structural counts are from the extracted shipped-default report; `butterfly` counts are source part kinds in the native pack. They are a coverage-oriented shortlist, not quotas or claims of verified solve quality.

| Source / position | Shipped tag | Parts | Bombs | Chain-locked roots | Butterfly parts | Why sample it |
|---|---:|---:|---:|---:|---:|---|
| 71029 / 175 | Hard | 29 | 1 | 2 | 0 | Bomb plus chain interactions at Hard tier. |
| 10187 / 285 | Hard | 30 | 1 | 0 | 2 | Bomb and butterfly parts without chain locks. |
| 10201 / 359 | VeryHard | 41 | 1 | 2 | 2 | Combined bomb, chain and butterfly mechanisms. |
| 10203 / 369 | VeryHard | 42 | 2 | 2 | 1 | Higher-count combined-mechanic case. |

All four are in the curated ordinary source pack; none is in the source filter's pictorial exclusion set. Inspect the rendered board during playtest as usual. For simpler comparison boards, source 10015 / position 55 is Hard with 22 roots and no bombs or chain-locked roots; source 60036 / position 59 is VeryHard with 29 roots and no bombs or chain-locked roots.

## Trace integrity

All trace files report the same Unity, runtime and harness build per their embedded hashes. Their SHA-256 values are recorded in [mechanism_lessons_v7.json](</D:/RotateRings/RotateRings Test/LevelTraining_SourceLayoutV3/runs/ordinary_v4/playtests/source_design_v7/mechanism_lessons_v7.json>). The source difficulty report SHA-256 is `250e9da2477870aaee6a58b4707a2cb022f1856b64118452ad4ecf5dd42e730d`.
