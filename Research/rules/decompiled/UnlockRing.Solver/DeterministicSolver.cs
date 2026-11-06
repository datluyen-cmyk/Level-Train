using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace UnlockRing.Solver;

public sealed class DeterministicSolver<TState>
{
	private sealed record SearchNode(TState State, string StateKey, string SearchIdentity, SolverCost Cost, IReadOnlyList<SolutionStep> Steps, IReadOnlyList<DecisionLiteral> Decisions);

	private readonly struct SearchPriority : IComparable<SearchPriority>, IEquatable<SearchPriority>
	{
		public SolverCost Cost { get; }

		public string StateKey { get; }

		public long Sequence { get; }

		public SearchPriority(SolverCost cost, string stateKey, long sequence)
		{
			Cost = cost;
			StateKey = stateKey;
			Sequence = sequence;
		}

		public bool Equals(SearchPriority other)
		{
			if (EqualityComparer<SolverCost>.Default.Equals(Cost, other.Cost) && EqualityComparer<string>.Default.Equals(StateKey, other.StateKey))
			{
				return EqualityComparer<long>.Default.Equals(Sequence, other.Sequence);
			}
			return false;
		}

		public override bool Equals(object? obj)
		{
			if (obj is SearchPriority other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			HashCode hashCode = default(HashCode);
			hashCode.Add(Cost);
			hashCode.Add(StateKey);
			hashCode.Add(Sequence);
			return hashCode.ToHashCode();
		}

		public static bool operator ==(SearchPriority left, SearchPriority right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SearchPriority left, SearchPriority right)
		{
			return !left.Equals(right);
		}

		public int CompareTo(SearchPriority other)
		{
			int num = Cost.CompareTo(other.Cost);
			if (num != 0)
			{
				return num;
			}
			num = string.CompareOrdinal(StateKey, other.StateKey);
			if (num == 0)
			{
				return Sequence.CompareTo(other.Sequence);
			}
			return num;
		}
	}

	private readonly ISolverDomain<TState> _domain;

	private readonly ISolverClock _clock;

	public DeterministicSolver(ISolverDomain<TState> domain, ISolverClock? clock = null)
	{
		_domain = domain ?? throw new ArgumentNullException("domain");
		_clock = clock ?? StopwatchSolverClock.Instance;
	}

	public SolveResult Solve(TState initialState, SolveBudget budget, CancellationToken cancellationToken = default(CancellationToken))
	{
		Compatibility.ThrowIfNull(initialState, "initialState");
		Compatibility.ThrowIfNull(budget, "budget");
		long timestamp = _clock.GetTimestamp();
		NogoodStore nogoodStore = new NogoodStore();
		string stableStateKey = _domain.GetStableStateKey(initialState);
		if (string.IsNullOrWhiteSpace(stableStateKey))
		{
			throw new InvalidOperationException("Domain state keys must not be empty.");
		}
		DecisionLiteral[] decisions = Array.Empty<DecisionLiteral>();
		SearchNode searchNode = new SearchNode(initialState, stableStateKey, BuildSearchIdentity(stableStateKey, decisions), SolverCost.Zero, Array.Empty<SolutionStep>(), decisions);
		MinPriorityQueue<SearchNode, SearchPriority> minPriorityQueue = new MinPriorityQueue<SearchNode, SearchPriority>();
		long num = 0L;
		minPriorityQueue.Enqueue(searchNode, new SearchPriority(searchNode.Cost, searchNode.SearchIdentity, num++));
		Dictionary<string, SolverCost> dictionary = new Dictionary<string, SolverCost>(StringComparer.Ordinal) { [searchNode.SearchIdentity] = SolverCost.Zero };
		long num2 = 0L;
		long num3 = 0L;
		bool flag = false;
		SortedSet<string> sortedSet = new SortedSet<string>(StringComparer.Ordinal);
		SearchNode element;
		SearchPriority priority;
		while (minPriorityQueue.TryDequeue(out element, out priority))
		{
			if (IsTimeOrCancellationExceeded(timestamp, budget, cancellationToken, out var reason))
			{
				return Result(SolveStatus.Unknown, null, reason, num2, num3, timestamp, nogoodStore);
			}
			if ((dictionary.TryGetValue(element.SearchIdentity, out var value) && element.Cost.CompareTo(value) > 0) || nogoodStore.FindTriggered(element.Decisions) != null)
			{
				continue;
			}
			if (_domain.IsSolved(element.State))
			{
				return Result(SolveStatus.Solved, new SolutionWitness(element.Steps, element.Cost), "找到经验证解。", num2, num3, timestamp, nogoodStore);
			}
			if (num2 >= budget.MaxExpandedStates)
			{
				return Result(SolveStatus.Unknown, null, "已达到展开状态预算；结果未知。", num2, num3, timestamp, nogoodStore);
			}
			num2++;
			DeadEndAnalysis deadEndAnalysis = _domain.AnalyzeDeadEnd(element.State, element.Decisions);
			if (deadEndAnalysis.IsDeadEnd)
			{
				if (deadEndAnalysis.ReliableConflict != null)
				{
					ValidateConflictAttribution(deadEndAnalysis.ReliableConflict, element.Decisions);
					nogoodStore.Learn(deadEndAnalysis.ReliableConflict);
				}
				continue;
			}
			ActionExpansion actionExpansion = _domain.Expand(element.State);
			if (!actionExpansion.IsComplete)
			{
				flag = true;
				sortedSet.Add(string.IsNullOrWhiteSpace(actionExpansion.Reason) ? "动作枚举不完整。" : actionExpansion.Reason);
			}
			foreach (SolverAction action in actionExpansion.Actions)
			{
				if (IsTimeOrCancellationExceeded(timestamp, budget, cancellationToken, out reason))
				{
					return Result(SolveStatus.Unknown, null, reason, num2, num3, timestamp, nogoodStore);
				}
				if (num3 >= budget.MaxGeneratedTransitions)
				{
					return Result(SolveStatus.Unknown, null, "已达到状态迁移预算；结果未知。", num2, num3, timestamp, nogoodStore);
				}
				if (!TryAppendDecision(element.Decisions, action.Decision, out var result) || nogoodStore.FindTriggered(result) != null)
				{
					continue;
				}
				num3++;
				SolverTransition<TState> solverTransition = _domain.Apply(element.State, action);
				if (solverTransition.Status == TransitionEvaluationStatus.Unknown)
				{
					flag = true;
					sortedSet.Add(string.IsNullOrWhiteSpace(solverTransition.Reason) ? "状态迁移结果未知。" : solverTransition.Reason);
					continue;
				}
				if (solverTransition.Status == TransitionEvaluationStatus.Rejected)
				{
					if (solverTransition.ReliableConflict != null)
					{
						ValidateConflictAttribution(solverTransition.ReliableConflict, result);
						nogoodStore.Learn(solverTransition.ReliableConflict);
					}
					continue;
				}
				if (solverTransition.State == null)
				{
					throw new InvalidOperationException("Applied transition must provide a state.");
				}
				if (action.Kind == SolverActionKind.BombDecision)
				{
					if ((object)solverTransition.Attribution == null)
					{
						throw new InvalidOperationException("Applied bomb decisions must provide attribution.");
					}
					if (!string.Equals(solverTransition.Attribution.BombId, action.BombSourcePartId ?? action.PartId, StringComparison.Ordinal))
					{
						throw new InvalidOperationException("Bomb attribution must identify the action's bomb part.");
					}
				}
				string stableStateKey2 = _domain.GetStableStateKey(solverTransition.State);
				if (string.IsNullOrWhiteSpace(stableStateKey2))
				{
					throw new InvalidOperationException("Domain state keys must not be empty.");
				}
				string text = BuildSearchIdentity(stableStateKey2, result);
				SolverCost solverCost = element.Cost.Add(action);
				if (!dictionary.TryGetValue(text, out value) || value.CompareTo(solverCost) > 0)
				{
					dictionary[text] = solverCost;
					SolutionStep item = new SolutionStep(element.Steps.Count + 1, action, solverTransition.Reason, stableStateKey2, solverCost, solverTransition.Attribution);
					IReadOnlyList<SolutionStep> steps = Append(element.Steps, item);
					SearchNode element2 = new SearchNode(solverTransition.State, stableStateKey2, text, solverCost, steps, result);
					minPriorityQueue.Enqueue(element2, new SearchPriority(solverCost, text, num++));
				}
			}
		}
		if (flag)
		{
			string reason2 = ((sortedSet.Count == 0) ? "搜索证据不完整，不能判定无解。" : string.Join(" ", sortedSet));
			return Result(SolveStatus.Unknown, null, reason2, num2, num3, timestamp, nogoodStore);
		}
		return Result(SolveStatus.Unsolvable, null, "已穷尽完整动作空间，未找到解。", num2, num3, timestamp, nogoodStore);
	}

	private bool IsTimeOrCancellationExceeded(long start, SolveBudget budget, CancellationToken cancellationToken, out string reason)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			reason = "搜索已取消；结果未知。";
			return true;
		}
		if (_clock.GetElapsedTime(start) >= budget.TimeLimit)
		{
			reason = "已达到时间预算；结果未知。";
			return true;
		}
		reason = string.Empty;
		return false;
	}

	private SolveResult Result(SolveStatus status, SolutionWitness? solution, string reason, long expanded, long generated, long start, NogoodStore nogoods)
	{
		return new SolveResult(status, solution, reason, expanded, generated, _clock.GetElapsedTime(start), nogoods.Learned);
	}

	private static IReadOnlyList<T> Append<T>(IReadOnlyList<T> source, T item)
	{
		T[] array = new T[source.Count + 1];
		for (int i = 0; i < source.Count; i++)
		{
			array[i] = source[i];
		}
		array[^1] = item;
		return array;
	}

	private static bool TryAppendDecision(IReadOnlyList<DecisionLiteral> source, DecisionLiteral? decision, out IReadOnlyList<DecisionLiteral> result)
	{
		if ((object)decision == null)
		{
			result = source;
			return true;
		}
		DecisionLiteral decisionLiteral = source.FirstOrDefault((DecisionLiteral x) => string.Equals(x.DecisionId, decision.DecisionId, StringComparison.Ordinal));
		if ((object)decisionLiteral != null)
		{
			result = source;
			return decisionLiteral == decision;
		}
		result = Append<DecisionLiteral>(source, decision);
		result = result.OrderBy<DecisionLiteral, string>((DecisionLiteral x) => x.StableKey, StringComparer.Ordinal).ToArray();
		return true;
	}

	private static string BuildSearchIdentity(string stateKey, IReadOnlyList<DecisionLiteral> decisions)
	{
		string text = string.Join("|", from x in decisions.OrderBy<DecisionLiteral, string>((DecisionLiteral x) => x.StableKey, StringComparer.Ordinal)
			select x.StableKey);
		return stateKey.Length + ":" + stateKey + text.Length + ":" + text;
	}

	private static void ValidateConflictAttribution(ReliableConflictSet conflict, IReadOnlyList<DecisionLiteral> decisions)
	{
		if (!conflict.IsTriggeredBy(decisions.ToHashSet()))
		{
			throw new InvalidOperationException("A learned conflict set must be attributable to the current decision trail.");
		}
	}
}
