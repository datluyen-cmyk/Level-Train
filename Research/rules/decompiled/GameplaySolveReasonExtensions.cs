using UnlockRing.Core.Domain;

public static class GameplaySolveReasonExtensions
{
	public static bool IsPassive(this GameplaySolveReason reason)
	{
		if (reason != GameplaySolveReason.PassivePointerUp)
		{
			return reason == GameplaySolveReason.PassiveColorLock;
		}
		return true;
	}

	public static bool IsActiveForStepScoring(this GameplaySolveReason reason)
	{
		if (reason != GameplaySolveReason.PointerUp && reason != GameplaySolveReason.Hammer && reason != GameplaySolveReason.Skyrocket && reason != GameplaySolveReason.Bomb)
		{
			return reason == GameplaySolveReason.ColorLock;
		}
		return true;
	}

	public static bool IsLegacyGestureStepScorable(this GameplaySolveReason reason)
	{
		if (reason != GameplaySolveReason.PointerUp && reason != GameplaySolveReason.Hammer && reason != GameplaySolveReason.Skyrocket && reason != GameplaySolveReason.Bomb)
		{
			return reason == GameplaySolveReason.PassivePointerUp;
		}
		return true;
	}

	public static bool IsColorLockBatchRelated(this GameplaySolveReason reason)
	{
		if (reason != GameplaySolveReason.ColorLock)
		{
			return reason == GameplaySolveReason.PassiveColorLock;
		}
		return true;
	}

	public static bool IsColorLockBatchStepScorable(this GameplaySolveReason reason)
	{
		return reason.IsColorLockBatchRelated();
	}

	public static bool TryGetColorLockBatchId(this IGameplayObject obj, out PartId batchPartId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		batchPartId = default(PartId);
		if (!(obj is IGameplaySolveReason gameplaySolveReason))
		{
			return false;
		}
		if (!gameplaySolveReason.SolveReason.IsColorLockBatchRelated())
		{
			return false;
		}
		PartId stepBatchPartId = gameplaySolveReason.StepBatchPartId;
		if (((PartId)(ref stepBatchPartId)).Equals(default(PartId)))
		{
			return false;
		}
		batchPartId = gameplaySolveReason.StepBatchPartId;
		return true;
	}

	public static PartId ResolveCascadeTriggerAfterSolved(this IGameplayObject gameplayObject, PartId solvedPartIdFallback = default(PartId))
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		PartId val;
		if (gameplayObject is IGameplaySolveReason gameplaySolveReason)
		{
			if (gameplaySolveReason.SolveReason.IsPassive())
			{
				val = gameplaySolveReason.PassiveByPartId;
				if (!((PartId)(ref val)).Equals(default(PartId)))
				{
					return gameplaySolveReason.PassiveByPartId;
				}
			}
			val = gameplaySolveReason.StepBatchPartId;
			if (!((PartId)(ref val)).Equals(default(PartId)))
			{
				return gameplaySolveReason.StepBatchPartId;
			}
		}
		val = default(PartId);
		if (!((PartId)(ref solvedPartIdFallback)).Equals(val))
		{
			return solvedPartIdFallback;
		}
		if (gameplayObject == null || !gameplayObject.TryGetPartId(out var partId))
		{
			val = default(PartId);
			return val;
		}
		return partId;
	}

	public static string ToReleasedPartReason(this GameplaySolveReason reason)
	{
		switch (reason)
		{
		case GameplaySolveReason.Hammer:
			return "Hammer";
		case GameplaySolveReason.Skyrocket:
			return "Skyrocket";
		case GameplaySolveReason.Bomb:
			return "Bomb";
		case GameplaySolveReason.ColorLock:
		case GameplaySolveReason.PassiveColorLock:
			return "ColorLock";
		default:
			return "LegacySolve";
		}
	}
}
