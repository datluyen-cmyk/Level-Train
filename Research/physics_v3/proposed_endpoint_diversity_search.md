# Proposed Editor-Only Endpoint Diversity Portfolio

## Purpose and boundary

This is a review proposal for a separate diagnostic policy. It does not change `FixtureWorld.ApplyAction`, collision/bomb behavior, action replay, or the meaning of `unknown`. A bounded run that finds no goal remains `unknown`; it is never reported as unsolvable. The proposal is source-only and has not been added to `Assets` or run in Unity.

The existing solver is useful as a bounded witness search, but its order can starve setup paths. `CompareSearchNodes` ranks removal count, binding count, priority/guidance, blockage, then *minimum* total pose travel. Search also stops a parent's sibling scan after adding the first prioritized unblocked child and trims the global frontier to at most 128. Thus a productive short setup can consume the frontier while longer or differently oriented setups disappear. For source 10010, the 45-second diagnostic explored 47 states while generating 6,502 actions and reached the wall-clock limit; the 58-state source500 checkpoint had 17 solved and 41 unknown. These are bounded-policy outcomes, not evidence of impossibility.

`EnumerateActions` currently samples fixed rotation magnitudes and slider travel increments (plus authored gap-guided rotations and a geometry-derived slider reach). A requested endpoint can be clamped by production physics, so ordering must be based on the resulting state after `ApplyAction`, not on requested delta/target alone. `StateKey` already captures quantized poses, removals, keys, spent bombs, locks, and binding snapshots; keep it as exact duplicate suppression within each lane.

## Proposed QA endpoint

Add a distinct Editor-only entry point, for example `StartDiversityBatch(requestPath, responsePath)`, alongside `StartBatch`. Reuse its source/template loading, one-level-per-editor-update checkpointing, hashes, and atomic checkpoint writes. Parse a new request schema (or schema 4) containing source IDs, state and wall-time limits, lane budgets, and optional target IDs. Do not route this endpoint through the existing default policy.

The report should identify `policyId: source-solver-v3-endpoint-diversity-diagnostic-v1`, and record Unity version, input/rules hashes, fixed delta time, limits, each lane's explored/generated/applied/no-op counts, duplicate drops, diversity-bucket drops, maximum frontier, best progress, and final status. Keep solved actions in the existing action schema and pass every candidate solution through the existing independent `Replay`. A replay failure remains `unknown` with the current validation error fields. Include a short `unknownReason`/proof string such as `wall budget`, `state budget`, or `all sampled lane frontiers exhausted; unsampled continuous actions remain unproven`.

Suggested initial run request: source 10010 only, 45 seconds per lane, 256 states per lane, and then a small fixed sentinel set of the 41 unknown rows from the 58-level checkpoint. The policy is diagnostic; do not replace the baseline result. If a lane solves, compare its replay witness to the baseline and retain both reports.

## Search lanes and setup diversity

Run three deterministic lanes with independent `FixtureWorld` instances so mutable Physics2D state and random/order effects cannot cross-contaminate them. All lanes call the unchanged `ApplyAction` and existing action enumerator; lane differences are search scheduling and state admission only.

1. **Baseline lane**: preserve current ordering and current candidate set. This is the control and should reproduce the current policy under the same build and budgets.
2. **Endpoint lane**: prioritize transitions whose *actual resulting pose* lands in a newly observed endpoint/critical bucket. For sliders, classify reached positions as negative extreme, interior, positive extreme, and quantized offsets (e.g. 0.12 units); extremes are empirical reachable limits witnessed by blocked movement or repeated same-direction probes in the lane, not assumed design limits. For rings, classify reached angle by 15-degree sector and by proximity to a current authored gap-bearing alignment. Rank new endpoint buckets ahead of repeated interior travel. An action is still executed only once through `ApplyAction`; never infer a release/removal from target values.
3. **Diversity lane**: retain distinct setup signatures across parts and mechanism topology. Signature includes removed/key/spent-bomb/chain-lock sets, active binding/contact/guide signature, each slider's coarse endpoint/interior bin, and each ring's 30-degree sector. Permit at most two frontier representatives per signature, with deterministic tie-breaking by removal progress, active bindings, guided status, blockage, and travel. This limit only controls beam admission; it is not a proof boundary.

Interleave lanes fairly (round-robin one expansion per nonempty lane) and give each an explicit equal share of the total state and wall-time budgets, configurable but recorded. Do not let a single lane consume the entire portfolio budget. Within endpoint/diversity lanes, schedule candidate actions round-robin by `(part, kind, endpoint/sector bucket)` after executing them, so a long list of short rotation increments cannot delay all slider endpoint actions. Keep the existing action cursor and exact `StateKey` suppression within each lane. Avoid the current `prioritizedBranch` sibling early-break in these two lanes; enqueue all useful siblings, subject to per-bucket admission, rather than following one child immediately. Keep all ordering deterministic.

For candidate ranking, prefer: goal; newly removed/key/bomb/lock progress; binding-topology change; unseen endpoint bucket; unseen setup signature; then lower travel and depth. Do not rank a state solely on travel or `guided` status. `quickRelease` may remain an early probe, but it must not starve the other buckets.

### Empirical endpoints without physics changes

The endpoint lane can learn actual limits using already-generated slide candidates. Record for each slider and visited nonterminal setup the most positive/negative reached slide, whether the last action was blocked, blocker owner/role, and whether the same direction has already saturated (movement consumed below tolerance or blocked). Bucket endpoints relative to these observed limits; do not assert a global limit from a single configuration because other parts may move and open a route. If a new setup changes blockers or bindings, reset/namespace those empirical limits by the setup's binding/topology signature. This adds diagnostic evidence without querying or changing the movement solver.

For rings, use candidate resulting angles and active authored contact/binding state. Gap alignment can be a target bucket only when the existing guidance computation supplies the bearing; all acceptance/removal claims still come from `ApplyAction` and `CanSolve`.

## Required metrics and interpretation

Per lane, persist:

- candidate counts by kind and part; candidate attempts, successful state changes, no-ops, and exact duplicate drops;
- number of distinct slider endpoint bins and ring sectors reached, and number of setup signatures seen/admitted/dropped;
- binding-topology-changing transitions and removal/key/bomb progress;
- frontier occupancy and budget stop reason;
- best state summary and any replayed witness.

Aggregate `solved` only when the ordinary replay reaches the goal. If every lane ends without a replayed goal, aggregate `unknown`, with lane stop reasons. Frontier exhaustion only means that this finite candidate portfolio exhausted. It does not prove that continuous gestures or other unenumerated paths cannot solve the level.

## Validation gate before considering adoption

1. Run baseline and diagnostic lane on a small fixed set containing known solved levels and the 41 unknown rows; preserve both policy IDs and hashes.
2. Confirm baseline lane emits the same solved/unknown outcome and replay witnesses as the current baseline on identical inputs.
3. Confirm every new solved row replays under the unchanged `Replay` and actions retain actual movement/blocker/removal witnesses.
4. Compare solved count, wall time, action/no-op counts, endpoint/setup coverage, and per-level status. Do not convert unknown to unsolvable, and do not use improved solved count alone to replace the default policy.
5. Only then decide whether to expand the diagnostic set or keep the endpoint as QA-only.
