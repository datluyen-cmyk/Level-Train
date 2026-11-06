using UnityEngine;
using UnlockRing.Core.Domain;

public readonly struct GameplaySolveReasonContext
{
	public GameplaySolveReason Reason { get; }

	public PartId PassiveByPartId { get; }

	public PartId StepBatchPartId { get; }

	public GameplaySolveReasonContext(GameplaySolveReason reason, PartId passiveByPartId = default(PartId), PartId stepBatchPartId = default(PartId))
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Reason = reason;
		PassiveByPartId = passiveByPartId;
		StepBatchPartId = stepBatchPartId;
	}

	public static GameplaySolveReasonContext PointerUp()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return new GameplaySolveReasonContext(GameplaySolveReason.PointerUp);
	}

	public static GameplaySolveReasonContext Hammer()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return new GameplaySolveReasonContext(GameplaySolveReason.Hammer);
	}

	public static GameplaySolveReasonContext Skyrocket()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return new GameplaySolveReasonContext(GameplaySolveReason.Skyrocket);
	}

	public static GameplaySolveReasonContext Bomb()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return new GameplaySolveReasonContext(GameplaySolveReason.Bomb);
	}

	public static GameplaySolveReasonContext ColorLock(PartId batchPartId)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new GameplaySolveReasonContext(GameplaySolveReason.ColorLock, default(PartId), batchPartId);
	}

	public static GameplaySolveReasonContext PassivePointerUp(PartId passiveByPartId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new GameplaySolveReasonContext(GameplaySolveReason.PassivePointerUp, passiveByPartId);
	}

	public static GameplaySolveReasonContext PassiveColorLock(PartId passiveByPartId, PartId batchPartId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return new GameplaySolveReasonContext(GameplaySolveReason.PassiveColorLock, passiveByPartId, batchPartId);
	}

	public GameplaySolveReasonContext ToDeferredPassive(PartId passiveByPartId = default(PartId))
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		PartId passiveByPartId2 = (((PartId)(ref passiveByPartId)).Equals(default(PartId)) ? PassiveByPartId : passiveByPartId);
		switch (Reason)
		{
		case GameplaySolveReason.ColorLock:
			return PassiveColorLock(passiveByPartId2, StepBatchPartId);
		case GameplaySolveReason.Bomb:
			return this;
		case GameplaySolveReason.PointerUp:
		case GameplaySolveReason.Hammer:
		case GameplaySolveReason.Skyrocket:
			return PassivePointerUp(passiveByPartId2);
		default:
			return this;
		}
	}

	public GameplaySolveReasonContext WithDefaultReasonIfNone(BombRelative sourceBomb)
	{
		if (Reason != GameplaySolveReason.None)
		{
			return this;
		}
		if (!((Object)(object)sourceBomb != (Object)null))
		{
			return PointerUp();
		}
		return Bomb();
	}
}
