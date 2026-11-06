using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.GamePlay.Save;

internal sealed class RestoreStep2_ApplyPartStates : IInLevelSaveStep<RotateRingRestoreContext>
{
	private const string Tag = "[RestoreStep2_ApplyPartStates]";

	public void Execute(RotateRingRestoreContext ctx)
	{
		if (!ctx.IsValid || ctx.SaveByPartId == null || ctx.LiveByPartId == null)
		{
			ctx.IsValid = false;
			return;
		}
		foreach (KeyValuePair<string, RotateRingPartSave> item in ctx.SaveByPartId)
		{
			if (ctx.LiveByPartId.TryGetValue(item.Key, out var value) && !TryApplyPartSave(value, item.Value))
			{
				ctx.IsValid = false;
				break;
			}
		}
	}

	private static bool TryApplyPartSave(IGameplayObject obj, RotateRingPartSave partSave)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (partSave.IsSolved)
		{
			ApplySolvedSilently(obj);
			return true;
		}
		switch (partSave.PartKind)
		{
		case RotateRingPartSave.PartKindType.Ring:
			if (obj is RingObject ringObject)
			{
				ringObject.ApplySavedRotation(partSave.RotationZ);
				ringObject.LockCount = partSave.LockCount;
				return true;
			}
			break;
		case RotateRingPartSave.PartKindType.Pull:
			if (obj is PullObject pullObject)
			{
				pullObject.ApplySavedPosition(new Vector3(partSave.PosX, partSave.PosY, ((Component)pullObject).transform.position.z));
				pullObject.LockCount = partSave.LockCount;
				return true;
			}
			break;
		case RotateRingPartSave.PartKindType.Fixed:
			if (obj is FixedObject fixedObject)
			{
				fixedObject.LockCount = partSave.LockCount;
				return true;
			}
			break;
		}
		return false;
	}

	private static void ApplySolvedSilently(IGameplayObject obj)
	{
		if (obj == null || obj.IsSolved)
		{
			return;
		}
		if (!(obj is RingObject ringObject))
		{
			if (!(obj is PullObject pullObject))
			{
				if (obj is FixedObject fixedObject)
				{
					fixedObject.SetSolved();
					fixedObject.DoFade();
					Object.Destroy((Object)(object)((Component)fixedObject).gameObject);
				}
			}
			else
			{
				pullObject.SetSolved();
				pullObject.DisableAllPhysicColliders();
				Object.Destroy((Object)(object)((Component)pullObject).gameObject);
			}
		}
		else
		{
			ringObject.SetSolved();
			ringObject.DisableAllPhysicColliders();
			Object.Destroy((Object)(object)((Component)ringObject).gameObject);
		}
	}
}
