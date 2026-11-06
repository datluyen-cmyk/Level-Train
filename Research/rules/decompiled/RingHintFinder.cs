using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnlockRing.Core.Domain;
using UnlockRing.Core.Queries;
using UnlockRing.Core.Rules;
using UnlockRing.Core.Runtime;
using UnlockRing.Game.Gameplay;
using UnlockRing.Solver;
using UnlockRing.Solver.Core;

public class RingHintFinder
{
	private readonly ReleaseMotionSearchOptions HintReleaseSearchOptions = new ReleaseMotionSearchOptions(50.0, 180.0, 16);

	private const double DefaultRotateDegrees = 15.0;

	private const double DefaultTranslateDistance = 1.0;

	public Task<IReadOnlyList<RingHintResult>> QueryAllAsync(LevelDefinition level, BoardState state, CoreGameController controller = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return Task.Run(() => QueryAll(level, state, controller, cancellationToken), cancellationToken);
	}

	public Task<RingHintResult?> QueryBestAsync(LevelDefinition level, BoardState state, CoreGameController controller = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return Task.Run(() => QueryBest(level, state, controller, cancellationToken), cancellationToken);
	}

	private RingHintResult? QueryBest(LevelDefinition level, BoardState state, CoreGameController controller, CancellationToken cancellationToken)
	{
		IReadOnlyList<RingHintResult> readOnlyList = QueryAll(level, state, controller, cancellationToken);
		if (readOnlyList.Count == 0)
		{
			return null;
		}
		return readOnlyList[0];
	}

	private IReadOnlyList<RingHintResult> QueryAll(LevelDefinition level, BoardState state, CoreGameController controller, CancellationToken cancellationToken)
	{
		Stopwatch.StartNew();
		List<RingHintResult> list = new List<RingHintResult>();
		HashSet<PartId> applicablePartIds = BuildApplicablePartIds(controller);
		Stopwatch stopwatch = Stopwatch.StartNew();
		int releaseScanCount;
		int releaseSkipCount;
		HashSet<PartId> hashSet = CollectReleaseCandidates(level, state, applicablePartIds, list, cancellationToken, out releaseScanCount, out releaseSkipCount);
		_ = stopwatch.Elapsed.TotalMilliseconds;
		Stopwatch stopwatch2 = Stopwatch.StartNew();
		CollectMovableCandidates(level, state, controller, list, hashSet, cancellationToken);
		_ = stopwatch2.Elapsed.TotalMilliseconds;
		Stopwatch stopwatch3 = Stopwatch.StartNew();
		ApplySolverBonus(level, state, controller, list, hashSet.Count > 0, cancellationToken);
		_ = stopwatch3.Elapsed.TotalMilliseconds;
		return (from c in list
			orderby c.Weight descending, GetAuthoredOrder(level, c.PartId)
			select c).ThenBy<RingHintResult, string>(delegate(RingHintResult c)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			PartId partId = c.PartId;
			return ((PartId)(ref partId)).Value;
		}, StringComparer.Ordinal).ToList();
	}

	private HashSet<PartId> BuildApplicablePartIds(CoreGameController controller)
	{
		if (controller == null || controller.Operations.Count == 0)
		{
			return null;
		}
		return (from op in controller.Operations.Values
			where op.CanApply
			select op.PartId).ToHashSet();
	}

	private HashSet<PartId> CollectReleaseCandidates(LevelDefinition level, BoardState state, HashSet<PartId> applicablePartIds, List<RingHintResult> candidates, CancellationToken cancellationToken, out int releaseScanCount, out int releaseSkipCount)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		HashSet<PartId> hashSet = new HashSet<PartId>();
		releaseScanCount = 0;
		releaseSkipCount = 0;
		foreach (PartDefinition item in from p in level.Parts
			orderby p.AuthoredOrder, p.Id
			select p)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (!state.Parts.TryGetValue(item.Id, out var value) || value.Released || value.Removed)
			{
				continue;
			}
			if (!ShouldSearchReleaseMotion(item))
			{
				releaseSkipCount++;
				continue;
			}
			if (applicablePartIds != null && !applicablePartIds.Contains(item.Id))
			{
				releaseSkipCount++;
				continue;
			}
			releaseScanCount++;
			ReleaseMotionSearchResult obj = CoreQueries.FindReleaseMotions(level, state, item.Id, HintReleaseSearchOptions);
			MotionRequest val = null;
			foreach (ReleaseMotionCandidate candidate in obj.Candidates)
			{
				if (candidate.Motion != null && (val == null || IsBetterReleaseMotion(candidate.Motion, val)))
				{
					val = candidate.Motion;
				}
			}
			if (val != null)
			{
				hashSet.Add(item.Id);
				candidates.Add(RingHintResult.FromMotionRequest(item.Id, HitResultSource.KnownReleaseMotion, ScoreCandidate(500, item.AuthoredOrder, val), val, "FindReleaseMotions"));
			}
		}
		return hashSet;
	}

	private bool IsBetterReleaseMotion(MotionRequest candidate, MotionRequest current)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		double num = Math.Abs(candidate.RequestedAmount);
		double num2 = Math.Abs(current.RequestedAmount);
		if (num < num2 - 1E-06)
		{
			return true;
		}
		if (num2 < num - 1E-06)
		{
			return false;
		}
		if ((int)candidate.Kind == 0 && (int)current.Kind == 0)
		{
			return candidate.RequestedAmount >= current.RequestedAmount;
		}
		return false;
	}

	private bool ShouldSearchReleaseMotion(PartDefinition part)
	{
		if (BoardRenderUtils.IsPlayerPart(part))
		{
			return part.Behaviors.OfType<ReleasableBehaviorDefinition>().Any();
		}
		return false;
	}

	private void CollectMovableCandidates(LevelDefinition level, BoardState state, CoreGameController controller, List<RingHintResult> candidates, HashSet<PartId> partsWithRelease, CancellationToken cancellationToken)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		IEnumerable<AvailableOperation> enumerable = null;
		enumerable = ((controller == null || controller.Operations.Count <= 0) ? CoreQueries.QueryAvailableOperations(level, state, (GestureDomainOptions)null, (GestureDomainOptions)null, cancellationToken).ApplicableOperations : controller.Operations.Values.Where((AvailableOperation val2) => val2.CanApply));
		HashSet<PartId> hashSet = new HashSet<PartId>();
		foreach (AvailableOperation op in enumerable)
		{
			if (!partsWithRelease.Contains(op.PartId) && hashSet.Add(op.PartId))
			{
				PartDefinition val = ((IEnumerable<PartDefinition>)level.Parts).FirstOrDefault((Func<PartDefinition, bool>)delegate(PartDefinition p)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					//IL_000f: Unknown result type (might be due to invalid IL or missing references)
					PartId id = p.Id;
					return ((PartId)(ref id)).Equals(op.PartId);
				});
				if (val != null && BoardRenderUtils.IsPlayerPart(val))
				{
					MotionRequest motion;
					bool num = TryBuildFromReleaseBound(op.MotionDomain, out motion);
					MotionRequest motion2 = (num ? motion : BuildMovableFallback(op));
					string reason = (num ? $"Available:{op.Kind}+DomainReleased" : $"Available:{op.Kind}");
					candidates.Add(RingHintResult.FromMotionRequest(op.PartId, HitResultSource.OperableOnly, ScoreCandidate(100, val.AuthoredOrder, motion2), motion2, reason));
				}
			}
		}
	}

	private static bool TryBuildFromReleaseBound(GestureMotionDomain domain, out MotionRequest motion)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Invalid comparison between Unknown and I4
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		motion = null;
		if (domain == null || !OperationDomainRefiner.HasReleasedBound(domain))
		{
			return false;
		}
		GestureMotionBound val = (((int)domain.PositiveBound.Kind == 1) ? domain.PositiveBound : domain.NegativeBound);
		if ((int)domain.MotionKind == 0)
		{
			motion = MotionRequest.Rotate(domain.PartId, val.SignedCommitAmount);
			return true;
		}
		if ((int)domain.MotionKind != 1)
		{
			return false;
		}
		Vector2D? translationWorldAxis = domain.TranslationWorldAxis;
		if (!translationWorldAxis.HasValue)
		{
			return false;
		}
		double signedCommitAmount = val.SignedCommitAmount;
		Vector2D value = translationWorldAxis.Value;
		if (signedCommitAmount < 0.0)
		{
			((Vector2D)(ref value))._002Ector(0.0 - ((Vector2D)(ref value)).X, 0.0 - ((Vector2D)(ref value)).Y);
		}
		motion = MotionRequest.Translate(domain.PartId, Math.Abs(signedCommitAmount), value);
		return true;
	}

	private static MotionRequest BuildMovableFallback(AvailableOperation op)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if ((int)op.Kind == 1)
		{
			return MotionRequest.Rotate(op.PartId, 15.0);
		}
		GestureMotionDomain motionDomain = op.MotionDomain;
		if (motionDomain != null && (int)motionDomain.MotionKind == 1 && op.MotionDomain.PositiveBound.SafeAmount > 0.0)
		{
			Vector2D? translationWorldAxis = op.MotionDomain.TranslationWorldAxis;
			if (translationWorldAxis.HasValue)
			{
				return MotionRequest.Translate(op.PartId, 1.0, translationWorldAxis.Value);
			}
		}
		return MotionRequest.Rotate(op.PartId, 15.0);
	}

	private void ApplySolverBonus(LevelDefinition level, BoardState state, CoreGameController controller, List<RingHintResult> hintResults, bool hasDirectRelease, CancellationToken cancellationToken)
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (hasDirectRelease)
			{
				return;
			}
			CoreLevelSolver coreLevelSolver = new CoreLevelSolver(level);
			SolveBudget budget = new SolveBudget(TimeSpan.FromMilliseconds(3000.0));
			SolveResult solveResult = coreLevelSolver.Solve(state, budget, cancellationToken);
			if (solveResult.Status == SolveStatus.Solved)
			{
				MotionRequest solverMotion = solveResult.Hint?.Action.Motion;
				if (solverMotion != null)
				{
					int num = hintResults.FindIndex((RingHintResult c) => MotionsMatch(in c, solverMotion));
					if (num >= 0)
					{
						RingHintResult ringHintResult = hintResults[num];
						int weight = ringHintResult.Weight + 1000;
						hintResults[num] = new RingHintResult
						{
							PartId = ringHintResult.PartId,
							Source = HitResultSource.FromSolver,
							Weight = weight,
							Reason = "Solver+match",
							MotionKind = ringHintResult.MotionKind,
							MotionAmount = ringHintResult.MotionAmount,
							TranslateAxis = ringHintResult.TranslateAxis
						};
					}
					else
					{
						PartDefinition val = ((IEnumerable<PartDefinition>)level.Parts).FirstOrDefault((Func<PartDefinition, bool>)delegate(PartDefinition p)
						{
							//IL_0001: Unknown result type (might be due to invalid IL or missing references)
							//IL_0006: Unknown result type (might be due to invalid IL or missing references)
							//IL_000f: Unknown result type (might be due to invalid IL or missing references)
							PartId id = p.Id;
							return ((PartId)(ref id)).Equals(solverMotion.PartId);
						});
						int weight2 = 1000 + ScoreCandidate(0, (val != null) ? val.AuthoredOrder : 0, solverMotion);
						hintResults.Add(RingHintResult.FromMotionRequest(solverMotion.PartId, HitResultSource.FromSolver, weight2, solverMotion, "Solver"));
					}
					return;
				}
			}
			ApplyReleaseProgressBonus(level, state, controller, hintResults, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception)
		{
		}
	}

	private void ApplyReleaseProgressBonus(LevelDefinition level, BoardState state, CoreGameController controller, List<RingHintResult> hintResults, CancellationToken cancellationToken)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (hintResults.Count == 0)
		{
			return;
		}
		for (int i = 0; i < hintResults.Count; i++)
		{
			cancellationToken.ThrowIfCancellationRequested();
			RingHintResult ringHintResult = hintResults[i];
			if (ringHintResult.Source == HitResultSource.OperableOnly)
			{
				int num = ScoreBestReleaseProgressForPart(level, state, controller, ringHintResult.PartId, cancellationToken);
				if (num > 0)
				{
					int num2 = num;
					RingHintResult ringHintResult2 = hintResults[i];
					hintResults[i] = new RingHintResult
					{
						PartId = ringHintResult2.PartId,
						Source = ringHintResult2.Source,
						Weight = ringHintResult2.Weight + num2,
						Reason = ringHintResult2.Reason + "+ReleaseProgress",
						MotionKind = ringHintResult2.MotionKind,
						MotionAmount = ringHintResult2.MotionAmount,
						TranslateAxis = ringHintResult2.TranslateAxis
					};
				}
			}
		}
	}

	private int ScoreBestReleaseProgressForPart(LevelDefinition level, BoardState state, CoreGameController controller, PartId partId, CancellationToken cancellationToken)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		int num = 0;
		foreach (MotionRequest item in BuildProgressProbeMotions(controller, partId))
		{
			cancellationToken.ThrowIfCancellationRequested();
			MotionPreview val = CoreRuntime.Preview(level, state, item);
			if (val.CanApply && (int)val.GeometryStatus != 2)
			{
				int num2 = MeasureReleaseProgressScore(level, state, val.Result.State);
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	private static IEnumerable<MotionRequest> BuildProgressProbeMotions(CoreGameController controller, PartId partId)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (controller != null)
		{
			foreach (AvailableOperation item in from op in controller.GetOperations(partId)
				where op.CanApply && op.MotionDomain != null
				select op)
			{
				GestureMotionDomain domain = item.MotionDomain;
				if ((int)domain.MotionKind == 0)
				{
					if (domain.PositiveBound.SafeAmount > 1E-06)
					{
						yield return MotionRequest.Rotate(partId, domain.PositiveBound.SafeAmount);
					}
					if (domain.NegativeBound.SafeAmount > 1E-06)
					{
						yield return MotionRequest.Rotate(partId, 0.0 - domain.NegativeBound.SafeAmount);
					}
					yield break;
				}
				Vector2D axis;
				if ((int)domain.MotionKind == 1)
				{
					Vector2D? translationWorldAxis = domain.TranslationWorldAxis;
					if (translationWorldAxis.HasValue)
					{
						axis = translationWorldAxis.GetValueOrDefault();
						if (domain.PositiveBound.SafeAmount > 1E-06)
						{
							yield return MotionRequest.Translate(partId, domain.PositiveBound.SafeAmount, axis);
						}
						if (domain.NegativeBound.SafeAmount > 1E-06)
						{
							yield return MotionRequest.Translate(partId, domain.NegativeBound.SafeAmount, new Vector2D(0.0 - ((Vector2D)(ref axis)).X, 0.0 - ((Vector2D)(ref axis)).Y));
						}
						yield break;
					}
				}
				axis = default(Vector2D);
			}
		}
		yield return MotionRequest.Rotate(partId, 180.0);
		yield return MotionRequest.Rotate(partId, -180.0);
	}

	private static int MeasureReleaseProgressScore(LevelDefinition level, BoardState before, BoardState after)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Invalid comparison between Unknown and I4
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Invalid comparison between Unknown and I4
		int num = 0;
		foreach (PartDefinition part in level.Parts)
		{
			if (part.Behaviors.OfType<ReleasableBehaviorDefinition>().Any() && before.Parts.TryGetValue(part.Id, out var value) && !value.Released && !value.Removed)
			{
				ReleaseEvaluation val = CoreQueries.EvaluateRelease(level, before, part.Id);
				ReleaseEvaluation val2 = CoreQueries.EvaluateRelease(level, after, part.Id);
				if ((int)val2.Kind == 0 && (int)val.Kind != 0)
				{
					num += 100;
				}
				else if ((int)val.Kind == 1 && (int)val2.Kind == 1 && val2.Blockers.Count < val.Blockers.Count)
				{
					num += (val.Blockers.Count - val2.Blockers.Count) * 50;
				}
			}
		}
		return num;
	}

	private int ScoreCandidate(int baseWeight, int authoredOrder, MotionRequest motion)
	{
		int num = Math.Max(0, 50 - authoredOrder);
		int num2 = 0;
		if (motion != null)
		{
			double num3 = Math.Abs(motion.RequestedAmount);
			num2 = (int)Math.Round(30.0 * (1.0 - Math.Min(1.0, num3 / 90.0)));
		}
		return baseWeight + num + num2;
	}

	private int GetAuthoredOrder(LevelDefinition level, PartId partId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		PartDefinition? obj = ((IEnumerable<PartDefinition>)level.Parts).FirstOrDefault((Func<PartDefinition, bool>)delegate(PartDefinition p)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			PartId id = p.Id;
			return ((PartId)(ref id)).Equals(partId);
		});
		if (obj == null)
		{
			return int.MaxValue;
		}
		return obj.AuthoredOrder;
	}

	private bool MotionsMatch(in RingHintResult left, MotionRequest right)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (right != null)
		{
			PartId partId = left.PartId;
			if (((PartId)(ref partId)).Equals(right.PartId) && left.MotionKind == right.Kind)
			{
				if (Math.Abs(left.MotionAmount - right.RequestedAmount) > 0.001)
				{
					return false;
				}
				if ((int)left.MotionKind != 1)
				{
					return true;
				}
				if (!right.Direction.HasValue)
				{
					return false;
				}
				Vector2D translateAxis = left.TranslateAxis;
				Vector2D value = right.Direction.Value;
				return ((Vector2D)(ref translateAxis)).X * ((Vector2D)(ref value)).X + ((Vector2D)(ref translateAxis)).Y * ((Vector2D)(ref value)).Y > 0.99;
			}
		}
		return false;
	}
}
