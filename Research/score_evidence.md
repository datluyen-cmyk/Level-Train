# Current HUD release score evidence

Recovered Hot.dll static code, `Research/rules/decompiled`:

- `RingObject.cs:2272–2274`: base100 passed to ThreadingScoreCalculator.
- `PullObject.cs:269–271`: base50 passed to the same calculator.
- `ThreadingScoreCalculator.cs:3–15`: `base*(1+initialPartData.LockedBy.Count+initialPartData.Locks.Count)+(initialPartData.LockCount>0 ? base :0)`.
- `RotateRings.GamePlay.Services/RotateRingOperationScoreFacade.cs:294–312`: Error/NoEffect increments invalidStepsSinceLastRelease; UnlockRelease settles one batch then resets that counter.
- Same file`:315–339`: for **every** released part implementing IThreadingScoreProvider, sum GetReleaseScore; when invalidStepsSinceLastRelease>=3 use `max(releaseScore−50,50)` per part. Add sum to LevelModelProxy. This path does not check CanCollect.
- Same file`:209–235`: GameplayOperationScore evaluation selects encouragement and a display position; it does not directly add HUD score. Do not confuse fallback30/60/100 encouragement scores with the100/50 base release values.

Initial Lock/LockedBy counts are serialized geometry/progression data, not the remaining live links at removal time. CanCollect identifies a separate collectable reward set; it does not suppress current facade HUD batch scoring.
