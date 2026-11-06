using System;

namespace UnlockRing.Solver;

public sealed class SolverTransition<TState>
{
	public TransitionEvaluationStatus Status { get; }

	public TState? State { get; }

	public string Reason { get; }

	public BombAttribution? Attribution { get; }

	public ReliableConflictSet? ReliableConflict { get; }

	private SolverTransition(TransitionEvaluationStatus status, TState? state, string reason, BombAttribution? attribution, ReliableConflictSet? conflict)
	{
		Status = status;
		State = state;
		Reason = reason;
		Attribution = attribution;
		ReliableConflict = conflict;
	}

	public static SolverTransition<TState> Applied(TState state, string reason = "", BombAttribution? attribution = null)
	{
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		return new SolverTransition<TState>(TransitionEvaluationStatus.Applied, state, reason ?? string.Empty, attribution, null);
	}

	public static SolverTransition<TState> Rejected(string reason, ReliableConflictSet? reliableConflict = null)
	{
		return new SolverTransition<TState>(TransitionEvaluationStatus.Rejected, default(TState), reason ?? string.Empty, null, reliableConflict);
	}

	public static SolverTransition<TState> Unknown(string reason)
	{
		return new SolverTransition<TState>(TransitionEvaluationStatus.Unknown, default(TState), reason ?? string.Empty, null, null);
	}
}
