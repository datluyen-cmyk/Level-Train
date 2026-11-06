using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnlockRing.Core.Domain;
using UnlockRing.Core.Runtime;
using UnlockRing.Core.Serialization;

namespace UnlockRing.Solver.Core;

public sealed class CoreLevelSolver
{
	private readonly LevelDefinition _level;

	private readonly DeterministicSolver<BoardState> _solver;

	private readonly BoardState _initialState;

	public BoardState InitialState => _initialState;

	public CoreLevelSolver(LevelDefinition level, CoreSolverOptions? options = null, ISolverClock? clock = null)
	{
		_level = level ?? throw new ArgumentNullException("level");
		InitialValidationResult val = CoreRuntime.Initialize(level);
		if (!val.IsValid || val.State == null)
		{
			string text = string.Join("; ", val.Errors.Select((ValidationError error) => error.Code + ": " + error.Message));
			throw new ArgumentException("Level cannot be initialized by CoreRuntime. " + text, "level");
		}
		_initialState = val.State;
		_solver = new DeterministicSolver<BoardState>(new CoreLevelSolverDomain(level, options), clock);
	}

	public SolveResult Solve(SolveBudget? budget = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _solver.Solve(_initialState, budget ?? SolveBudget.For(SolveBudgetProfile.Standard), cancellationToken);
	}

	public SolveResult Solve(BoardState state, SolveBudget? budget = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _solver.Solve(state ?? throw new ArgumentNullException("state"), budget ?? SolveBudget.For(SolveBudgetProfile.Standard), cancellationToken);
	}

	public ReplayLog CreateReplay(SolutionWitness witness)
	{
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Expected O, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Invalid comparison between Unknown and I4
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Expected O, but got Unknown
		Compatibility.ThrowIfNull(witness, "witness");
		List<ReplayStep> list = new List<ReplayStep>(witness.Steps.Count);
		BoardState val = _initialState;
		foreach (SolutionStep step in witness.Steps)
		{
			string text = CanonicalJsonV1.ComputeSha256(val);
			ReplayCommand val3;
			BoardState state;
			if (step.Action.Motion != null)
			{
				MotionPreview val2 = CoreRuntime.Preview(_level, val, step.Action.Motion);
				if (!val2.CanApply || (int)val2.GeometryStatus == 2)
				{
					throw new InvalidOperationException($"Witness step {step.Index} is no longer replayable: {val2.Reason}");
				}
				val3 = ReplayCommand.FromMotion(step.Action.Motion);
				state = val2.Result.State;
			}
			else
			{
				if (!step.Action.ForceRemovePart.HasValue)
				{
					throw new InvalidOperationException($"Witness step {step.Index} has no Core command payload.");
				}
				ForceRemoveResult val4 = CoreRuntime.ForceRemove(_level, val, step.Action.ForceRemovePart.Value);
				if ((int)val4.Status != 0)
				{
					throw new InvalidOperationException($"Witness step {step.Index} ForceRemove is no longer replayable: {val4.Reason}");
				}
				val3 = ReplayCommand.ForceRemove(step.Action.ForceRemovePart.Value);
				state = val4.State;
			}
			if (!string.Equals(new BoardStateKeyBuilder().Build(state), step.ResultingStateKey, StringComparison.Ordinal))
			{
				throw new InvalidOperationException($"Witness step {step.Index} resulting state does not match its recorded state key.");
			}
			list.Add(new ReplayStep(list.Count, val3, text, CanonicalJsonV1.ComputeSha256(state)));
			val = state;
		}
		return new ReplayLog(1, _level.LevelId, CanonicalJsonV1.ComputeSha256(_level), _level.RulesVersion, (IEnumerable<ReplayStep>)list);
	}
}
