using System.Collections.Generic;

namespace UnlockRing.Solver;

public interface ISolverDomain<TState>
{
	string GetStableStateKey(TState state);

	bool IsSolved(TState state);

	ActionExpansion Expand(TState state);

	SolverTransition<TState> Apply(TState state, SolverAction action);

	DeadEndAnalysis AnalyzeDeadEnd(TState state, IReadOnlyList<DecisionLiteral> decisionTrail);
}
