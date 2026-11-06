using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnlockRing.Core.Domain;
using UnlockRing.Core.Rules;
using UnlockRing.Core.Runtime;

namespace UnlockRing.Solver.Core;

public sealed class CoreLevelSolverDomain : ISolverDomain<BoardState>
{
	private sealed record EventSignature(string Constraints, string Released, string Removed, string Collision)
	{
		public static EventSignature FromState(BoardState state)
		{
			return new EventSignature(ConstraintKey(state.Constraints), string.Empty, string.Empty, string.Empty);
		}

		public static EventSignature FromResult(MotionResult result)
		{
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			return new EventSignature(ConstraintKey(result.State.Constraints), string.Join(",", from item in result.Releases
				select item.PartId into id
				orderby id
				select id), string.Join(",", result.Delta.RemovedParts.OrderBy((PartId id) => id)), (result.Collision == null) ? string.Empty : $"{result.Collision.ContactKind}:{result.Collision.BlockingPart}:{result.Collision.BlockingFeature}");
		}

		private static string ConstraintKey(IEnumerable<ConstraintRelation> constraints)
		{
			return string.Join("|", constraints.Select((ConstraintRelation relation) => $"{relation.Kind}:{relation.FirstPart}:{relation.FirstFeature}:{relation.SecondPart}:{relation.SecondFeature}").OrderBy<string, string>((string value) => value, StringComparer.Ordinal));
		}
	}

	private sealed record CriticalPreview(double ProbeAmount, MotionPreview Preview);

	private readonly LevelDefinition _level;

	private readonly CoreSolverOptions _options;

	private readonly BoardStateKeyBuilder _keys;

	private readonly IReadOnlyDictionary<PartId, PartDefinition> _parts;

	public CoreLevelSolverDomain(LevelDefinition level, CoreSolverOptions? options = null, BoardStateKeyBuilder? keys = null)
	{
		_level = level ?? throw new ArgumentNullException("level");
		_options = options ?? new CoreSolverOptions();
		_keys = keys ?? new BoardStateKeyBuilder();
		_parts = level.Parts.ToDictionary((PartDefinition part) => part.Id);
	}

	public string GetStableStateKey(BoardState state)
	{
		return _keys.Build(state);
	}

	public bool IsSolved(BoardState state)
	{
		PartState value;
		return ReleasableIds().All((PartId id) => state.Parts.TryGetValue(id, out value) && (value.Released || value.Removed));
	}

	public ActionExpansion Expand(BoardState state)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Invalid comparison between Unknown and I4
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Invalid comparison between Unknown and I4
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		Compatibility.ThrowIfNull(state, "state");
		List<SolverAction> list = new List<SolverAction>();
		SortedSet<string> sortedSet = new SortedSet<string>(StringComparer.Ordinal);
		bool flag = false;
		foreach (PartDefinition definition in from part in _level.Parts
			orderby part.AuthoredOrder, part.Id
			select part)
		{
			if (!state.Parts.TryGetValue(definition.Id, out var value) || value.Removed || value.Released)
			{
				continue;
			}
			if (definition.Behaviors.OfType<RotatableBehaviorDefinition>().Any())
			{
				flag = true;
				OperationEligibilityResult val = OperationEligibilityRules.Evaluate(_level, state, definition.Id, (MotionKind)0);
				if ((int)val.Status == 2)
				{
					sortedSet.Add(val.Reason);
				}
				if (val.CanStart)
				{
					DiscoverCriticalGestures(state, definition, (double amount) => MotionRequest.Rotate(definition.Id, amount), _options.RotationHorizonDegrees, list, sortedSet);
					DiscoverCriticalGestures(state, definition, (double amount) => MotionRequest.Rotate(definition.Id, 0.0 - amount), _options.RotationHorizonDegrees, list, sortedSet);
				}
			}
			OperationEligibilityResult val2 = OperationEligibilityRules.Evaluate(_level, state, definition.Id, (MotionKind)1);
			if ((int)val2.Status == 2 && definition.Behaviors.OfType<AxisSlidableBehaviorDefinition>().Any())
			{
				sortedSet.Add(val2.Reason);
			}
			IEnumerable<Vector2D> enumerable2;
			if (!val2.CanStart)
			{
				IEnumerable<Vector2D> enumerable = Array.Empty<Vector2D>();
				enumerable2 = enumerable;
			}
			else
			{
				enumerable2 = CandidateTranslationAxes(state, definition, value);
			}
			foreach (Vector2D item in enumerable2)
			{
				Vector2D axis = item;
				flag = true;
				DiscoverCriticalGestures(state, definition, (double amount) => MotionRequest.Translate(definition.Id, amount, axis), _options.TranslationHorizon, list, sortedSet);
				DiscoverCriticalGestures(state, definition, (double amount) => MotionRequest.Translate(definition.Id, amount, new Vector2D(0.0 - ((Vector2D)(ref axis)).X, 0.0 - ((Vector2D)(ref axis)).Y)), _options.TranslationHorizon, list, sortedSet);
			}
		}
		if (_level.Rules.ForceRemovalEnabled)
		{
			foreach (PartDefinition item2 in from part in _level.Parts
				orderby part.AuthoredOrder, part.Id
				select part)
			{
				if (state.Parts.TryGetValue(item2.Id, out var value2) && !value2.Removed && !value2.Released && item2.Behaviors.OfType<ForceRemovableBehaviorDefinition>().Any() && (int)CoreRuntime.ForceRemove(_level, state, item2.Id).Status == 0)
				{
					PartId id = item2.Id;
					string stableKey = "force:" + Escape(((PartId)(ref id)).Value);
					id = item2.Id;
					list.Add(new SolverAction(stableKey, SolverActionKind.ForceRemove, ((PartId)(ref id)).Value, 0.0, 0, "Core ForceRemove candidate.", null, null, item2.Id));
				}
			}
		}
		bool isComplete = !flag && sortedSet.Count == 0;
		if (flag)
		{
			sortedSet.Add("连续动作由确定性临界事件探针生成，但尚无动作集完备性证书。");
		}
		return new ActionExpansion(list, isComplete, string.Join(" ", sortedSet));
	}

	public SolverTransition<BoardState> Apply(BoardState state, SolverAction action)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		Compatibility.ThrowIfNull(state, "state");
		Compatibility.ThrowIfNull(action, "action");
		if (action.Motion != null)
		{
			MotionPreview val = CoreRuntime.Preview(_level, state, action.Motion);
			if ((int)val.GeometryStatus == 2 || (int)val.Result.Certainty == 2)
			{
				return SolverTransition<BoardState>.Unknown(NonEmpty(val.Reason, "Core motion geometry is incomplete."));
			}
			if (!val.CanApply)
			{
				return SolverTransition<BoardState>.Rejected(NonEmpty(val.Result.Reason, "Core rejected the motion."));
			}
			BombAttribution attribution = null;
			if (action.Kind == SolverActionKind.BombDecision)
			{
				string text = action.BombSourcePartId ?? FindTriggeredBombSource(state, val.Result.State);
				if (text == null)
				{
					return SolverTransition<BoardState>.Unknown("Bomb transition has no attributable source host.");
				}
				string[] removedPartIds = val.Result.Delta.RemovedParts.Select((PartId id) => ((PartId)(ref id)).Value).ToArray();
				attribution = new BombAttribution(text, removedPartIds, "Core direct-contact bomb aftermath.");
			}
			return SolverTransition<BoardState>.Applied(val.Result.State, NonEmpty(val.Result.Reason, Describe(val.Result)), attribution);
		}
		if (action.ForceRemovePart.HasValue)
		{
			ForceRemoveResult val2 = CoreRuntime.ForceRemove(_level, state, action.ForceRemovePart.Value);
			if ((int)val2.Status != 0)
			{
				return SolverTransition<BoardState>.Rejected(NonEmpty(val2.Reason, "Core rejected ForceRemove."));
			}
			return SolverTransition<BoardState>.Applied(val2.State, NonEmpty(val2.Reason, "Core ForceRemove applied."));
		}
		return SolverTransition<BoardState>.Unknown("Solver action does not contain a replayable Core command.");
	}

	public DeadEndAnalysis AnalyzeDeadEnd(BoardState state, IReadOnlyList<DecisionLiteral> decisionTrail)
	{
		if (IsSolved(state))
		{
			return new DeadEndAnalysis(isDeadEnd: false);
		}
		if (_level.Parts.Where((PartDefinition definition) => state.Parts.TryGetValue(definition.Id, out var value) && !value.Removed && !value.Released).ToArray().Any((PartDefinition definition) => definition.Behaviors.Any((BehaviorDefinition behavior) => behavior is RotatableBehaviorDefinition || behavior is AxisSlidableBehaviorDefinition) || (_level.Rules.ForceRemovalEnabled && definition.Behaviors.OfType<ForceRemovableBehaviorDefinition>().Any())))
		{
			return new DeadEndAnalysis(isDeadEnd: false);
		}
		ReliableConflictSet reliableConflict = ((decisionTrail.Count == 0) ? null : new ReliableConflictSet(decisionTrail, "Bomb aftermath left unresolved releasable parts with no command-capable active part."));
		return new DeadEndAnalysis(isDeadEnd: true, "Unresolved releasable parts remain, but no Core command kind is available.", reliableConflict);
	}

	private void DiscoverCriticalGestures(BoardState state, PartDefinition movingPart, Func<double, MotionRequest> requestAt, double horizon, ICollection<SolverAction> actions, ISet<string> incompleteReasons)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		EventSignature eventSignature = EventSignature.FromState(state);
		double low = 0.0;
		bool flag = false;
		for (int i = 1; i <= _options.CriticalProbeCount; i++)
		{
			double num = horizon * (double)i / (double)_options.CriticalProbeCount;
			MotionPreview val = CoreRuntime.Preview(_level, state, requestAt(num));
			if ((int)val.GeometryStatus == 2)
			{
				incompleteReasons.Add(NonEmpty(val.Reason, $"{movingPart.Id} motion geometry is incomplete."));
				return;
			}
			if (!val.CanApply)
			{
				low = num;
				continue;
			}
			if (i == 1 && !movingPart.Behaviors.Any((BehaviorDefinition behavior) => behavior is BombBehaviorDefinition || behavior is ReleasableBehaviorDefinition))
			{
				AddMotionAction(state, requestAt(num), val, actions);
			}
			EventSignature eventSignature2 = EventSignature.FromResult(val.Result);
			if (!flag && !eventSignature2.Equals(eventSignature))
			{
				CriticalPreview criticalPreview = RefineFirstEvent(state, requestAt, low, num, eventSignature);
				AddMotionAction(state, requestAt(criticalPreview.ProbeAmount), criticalPreview.Preview, actions);
				flag = true;
			}
			low = num;
		}
		MotionRequest val2 = requestAt(horizon);
		AddMotionAction(state, val2, CoreRuntime.Preview(_level, state, val2), actions);
	}

	private CriticalPreview RefineFirstEvent(BoardState state, Func<double, MotionRequest> requestAt, double low, double high, EventSignature baseline)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		MotionPreview preview = CoreRuntime.Preview(_level, state, requestAt(high));
		for (int i = 0; i < _options.CriticalRefinementIterations; i++)
		{
			double num = low + (high - low) * 0.5;
			MotionPreview val = CoreRuntime.Preview(_level, state, requestAt(num));
			if ((int)val.GeometryStatus == 2)
			{
				break;
			}
			if (val.CanApply && !EventSignature.FromResult(val.Result).Equals(baseline))
			{
				high = num;
				preview = val;
			}
			else
			{
				low = num;
			}
		}
		return new CriticalPreview(high, preview);
	}

	private void AddMotionAction(BoardState before, MotionRequest request, MotionPreview preview, ICollection<SolverAction> actions)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		if (!preview.CanApply || (int)preview.GeometryStatus == 2)
		{
			return;
		}
		BoardState state = preview.Result.State;
		if (string.Equals(_keys.Build(before), _keys.Build(state), StringComparison.Ordinal))
		{
			return;
		}
		PartId[] array = preview.Result.Delta.RemovedParts.ToArray();
		string text = FindTriggeredBombSource(before, state);
		bool flag = text != null && array.Length != 0;
		SolverActionKind kind = (flag ? SolverActionKind.BombDecision : ((preview.Result.Releases.Count <= 0) ? SolverActionKind.SetupMove : SolverActionKind.Release));
		DecisionLiteral decisionLiteral = null;
		if (flag)
		{
			string choiceId = string.Join(",", from id in array
				orderby id
				select ((PartId)(ref id)).Value);
			decisionLiteral = new DecisionLiteral("bomb:" + text, choiceId);
		}
		string key = MotionKey(request, flag ? text : null);
		if (!actions.Any((SolverAction action) => string.Equals(action.StableKey, key, StringComparison.Ordinal)))
		{
			string stableKey = key;
			PartId partId = request.PartId;
			string value = ((PartId)(ref partId)).Value;
			double movementDistance = Math.Abs(preview.Result.ActualAmount);
			int irreversibleRisk = (flag ? 1 : 0);
			string reason = Describe(preview.Result);
			DecisionLiteral decision = decisionLiteral;
			string bombSourcePartId = text;
			actions.Add(new SolverAction(stableKey, kind, value, movementDistance, irreversibleRisk, reason, decision, request, null, bombSourcePartId));
		}
	}

	private IEnumerable<Vector2D> CandidateTranslationAxes(BoardState state, PartDefinition definition, PartState partState)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		List<Vector2D> list = new List<Vector2D>();
		AxisSlidableBehaviorDefinition[] array = definition.Behaviors.OfType<AxisSlidableBehaviorDefinition>().ToArray();
		if (array.Length == 0)
		{
			return list;
		}
		AxisSlidableBehaviorDefinition[] array2 = array;
		Pose2D pose;
		foreach (AxisSlidableBehaviorDefinition val in array2)
		{
			Vector2D localAxis = val.LocalAxis;
			pose = partState.Pose;
			AddWorldAxis(list, localAxis, ((Pose2D)(ref pose)).RotationRadians);
		}
		foreach (ConstraintRelation item in state.Constraints.Where(delegate(ConstraintRelation relation)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			if ((int)relation.Kind == 2)
			{
				PartId val2 = relation.FirstPart;
				if (!((PartId)(ref val2)).Equals(definition.Id))
				{
					val2 = relation.SecondPart;
					return ((PartId)(ref val2)).Equals(definition.Id);
				}
				return true;
			}
			return false;
		}))
		{
			PartId firstPart = item.FirstPart;
			PartId key = (((PartId)(ref firstPart)).Equals(definition.Id) ? item.SecondPart : item.FirstPart);
			if (!_parts.TryGetValue(key, out var value) || !state.Parts.TryGetValue(key, out var value2))
			{
				continue;
			}
			foreach (LinearGuideBehaviorDefinition item2 in value.Behaviors.OfType<LinearGuideBehaviorDefinition>())
			{
				Vector2D localAxis2 = item2.LocalAxis;
				pose = value2.Pose;
				AddWorldAxis(list, localAxis2, ((Pose2D)(ref pose)).RotationRadians);
			}
		}
		return list.OrderBy((Vector2D axis) => Math.Atan2(((Vector2D)(ref axis)).Y, ((Vector2D)(ref axis)).X));
	}

	private static void AddWorldAxis(ICollection<Vector2D> axes, Vector2D local, double rotation)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		double num = Math.Cos(rotation);
		double num2 = Math.Sin(rotation);
		Vector2D world = Normalize(new Vector2D(((Vector2D)(ref local)).X * num - ((Vector2D)(ref local)).Y * num2, ((Vector2D)(ref local)).X * num2 + ((Vector2D)(ref local)).Y * num));
		if (((Vector2D)(ref world)).Length != 0.0)
		{
			if (((Vector2D)(ref world)).X < 0.0 || (Math.Abs(((Vector2D)(ref world)).X) <= 1E-12 && ((Vector2D)(ref world)).Y < 0.0))
			{
				world = new Vector2D(0.0 - ((Vector2D)(ref world)).X, 0.0 - ((Vector2D)(ref world)).Y);
			}
			if (!axes.Any((Vector2D existing) => Math.Abs(((Vector2D)(ref existing)).X * ((Vector2D)(ref world)).Y - ((Vector2D)(ref existing)).Y * ((Vector2D)(ref world)).X) <= 1E-09))
			{
				axes.Add(world);
			}
		}
	}

	private static Vector2D Normalize(Vector2D value)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!(((Vector2D)(ref value)).Length <= 1E-12))
		{
			return new Vector2D(((Vector2D)(ref value)).X / ((Vector2D)(ref value)).Length, ((Vector2D)(ref value)).Y / ((Vector2D)(ref value)).Length);
		}
		return Vector2D.Zero;
	}

	private IEnumerable<PartId> ReleasableIds()
	{
		return from part in _level.Parts
			where part.Behaviors.OfType<ReleasableBehaviorDefinition>().Any()
			select part.Id;
	}

	private string? FindTriggeredBombSource(BoardState before, BoardState after)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		foreach (PartId item in from part in _level.Parts
			where part.Behaviors.OfType<BombBehaviorDefinition>().Any()
			orderby part.AuthoredOrder, part.Id
			select part.Id)
		{
			PartId current = item;
			if (before.Parts.TryGetValue(current, out var value) && after.Parts.TryGetValue(current, out var value2) && !value.BombTriggered && value2.BombTriggered)
			{
				return ((PartId)(ref current)).Value;
			}
		}
		return null;
	}

	private static IEnumerable<PartId> NewlyRemoved(BoardState before, BoardState after)
	{
		PartState value;
		return from pair in after.Parts
			where pair.Value.Removed && before.Parts.TryGetValue(pair.Key, out value) && !value.Removed
			select pair.Key into id
			orderby id
			select id;
	}

	private static string MotionKey(MotionRequest request, string? bombSource)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		string text = request.RequestedAmount.ToString("R", CultureInfo.InvariantCulture);
		string text2;
		if (!request.Direction.HasValue)
		{
			text2 = string.Empty;
		}
		else
		{
			Vector2D value = request.Direction.Value;
			string text3 = ((Vector2D)(ref value)).X.ToString("R", CultureInfo.InvariantCulture);
			value = request.Direction.Value;
			text2 = ":" + text3 + "," + ((Vector2D)(ref value)).Y.ToString("R", CultureInfo.InvariantCulture);
		}
		string text4 = text2;
		object[] array = new object[4];
		PartId partId = request.PartId;
		array[0] = Escape(((PartId)(ref partId)).Value);
		array[1] = request.Kind;
		array[2] = text;
		array[3] = text4;
		return string.Format("motion:{0}:{1}:{2}{3}", array) + ((bombSource == null) ? string.Empty : (":bomb:" + Escape(bombSource)));
	}

	private static string Escape(string value)
	{
		return value.Length.ToString(CultureInfo.InvariantCulture) + ":" + value;
	}

	private static string NonEmpty(string? value, string fallback)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			return value;
		}
		return fallback;
	}

	private static string Describe(MotionResult result)
	{
		if (result.Releases.Count <= 0)
		{
			if (result.Delta.RemovedParts.Count <= 0)
			{
				if (!result.Truncated)
				{
					return "Core non-release setup motion.";
				}
				return "Core motion reached its first blocking contact.";
			}
			return "Core motion removed: " + string.Join(",", result.Delta.RemovedParts.Select((PartId id) => ((PartId)(ref id)).Value));
		}
		return "Core motion released: " + string.Join(",", result.Releases.Select(delegate(ReleaseWitness item)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			PartId partId = item.PartId;
			return ((PartId)(ref partId)).Value;
		}));
	}
}
