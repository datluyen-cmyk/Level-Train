namespace UnlockRing.Solver;

public sealed record SolutionStep(int Index, SolverAction Action, string TransitionReason, string ResultingStateKey, SolverCost CumulativeCost, BombAttribution? BombAttribution);
