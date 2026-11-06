using System;
using System.Collections.Generic;
using UnlockRing.Core.Domain;

namespace UnlockRing.Solver;

public sealed class BoardStateSolverDomain : ISolverDomain<BoardState>
{
	private readonly IBoardStateTransitionAdapter _adapter;

	private readonly BoardStateKeyBuilder _stateKeys;

	public BoardStateSolverDomain(IBoardStateTransitionAdapter adapter, BoardStateKeyBuilder? stateKeys = null)
	{
		_adapter = adapter ?? throw new ArgumentNullException("adapter");
		_stateKeys = stateKeys ?? new BoardStateKeyBuilder();
	}

	public string GetStableStateKey(BoardState state)
	{
		return _stateKeys.Build(state);
	}

	public bool IsSolved(BoardState state)
	{
		return _adapter.IsSolved(state);
	}

	public ActionExpansion Expand(BoardState state)
	{
		return _adapter.Expand(state);
	}

	public SolverTransition<BoardState> Apply(BoardState state, SolverAction action)
	{
		return _adapter.Apply(state, action);
	}

	public DeadEndAnalysis AnalyzeDeadEnd(BoardState state, IReadOnlyList<DecisionLiteral> decisionTrail)
	{
		return _adapter.AnalyzeDeadEnd(state, decisionTrail);
	}
}
