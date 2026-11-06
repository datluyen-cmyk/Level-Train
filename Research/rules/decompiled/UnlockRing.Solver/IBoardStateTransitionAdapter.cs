using System.Collections.Generic;
using UnlockRing.Core.Domain;

namespace UnlockRing.Solver;

public interface IBoardStateTransitionAdapter
{
	bool IsSolved(BoardState state);

	ActionExpansion Expand(BoardState state);

	SolverTransition<BoardState> Apply(BoardState state, SolverAction action);

	DeadEndAnalysis AnalyzeDeadEnd(BoardState state, IReadOnlyList<DecisionLiteral> decisionTrail);
}
