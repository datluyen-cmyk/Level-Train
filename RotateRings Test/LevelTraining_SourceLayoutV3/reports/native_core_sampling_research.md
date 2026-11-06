# Native Core critical gesture sampling

This note describes static decompilation from the supplied APK. It does not
observe Android execution or recover the separate native Core AOT geometry.

`Research/rules/decompiled/UnlockRing.Solver.Core/CoreLevelSolverDomain.cs`
contains the relevant implementation. `CoreSolverOptions.cs` declares defaults:
32 probes per direction, a 360-degree rotation horizon, a 100-unit translation
horizon, and 28 refinement iterations. These horizons belong to its own Core
runtime; copying the translation distance into the Unity reconstruction would
not establish equivalent behavior.

The event signature includes exact constraint relation kinds and both part and
feature identities, released IDs, removed IDs, and collision kind, blocking
part and blocking feature. The sampler probes from the same initial state at
increasing requested amounts. It brackets the first signature change, refines
that boundary by bisection, and adds the corresponding replayable command.
It also offers the horizon endpoint and an early setup move for some parts.
The expansion explicitly says that its continuous action set has no completeness
certificate. A failed search remains unknown.

`RingHintFinder.cs` first calls `CoreQueries.FindReleaseMotions`, then considers
setup operations and solver bonuses. Its geometry implementation is in the
missing Core AOT dependency. It is evidence for a strategy, not a callable
original physics oracle.

## Application to the current bounded Unity solver

The current action family includes ring gap bearings, fixed angular magnitudes
starting at 5 degrees, slider magnitudes starting at .06 units and blocked
endpoints reached through production frame/substep methods. State keys quantize
angles to .5 degrees and slides to .01 units. These are bounded search choices;
they can merge or miss narrow intermediate setup states.

Useful follow-up experiments, without changing gameplay rules:

1. Compare exact active/passive/guide/contact-identity signatures, bomb state,
   removed IDs and collision features after production actions. A decrease in
   contact count alone misses a changed contact identity.
2. Add production-replayed boundary setup gestures around a detected event.
   Preserve setup moves that change geometric clearance even when masks do not
   change. Coordinate alternating slide/rotate actions at blocked endpoints.
3. Avoid repeatedly applying longer requests from the same parent after an
   unchanged, genuinely blocked gesture, only if event/state histories are
   identical. This is a search optimization requiring regression evidence;
   removal, bomb, binding or key changes prevent that shortcut.
4. Hash and report any changed action family or state quantization. Compare
   fixed node and wall-time budgets on the same sources and independently replay
   every found witness. Report a computer search proxy, not human difficulty.

Source 10010 specifically needs an experiment alternating slider2 and ring3
after legally removing the lower assembly. Earlier forced-removal probes are
diagnostics only and cannot seed a valid solution certificate.
