using UnityEngine;

namespace RotateRings.GamePlay.Services;

public static class BoardFlipEffectUtility
{
	public static bool IsFlipped(GameplayManager mgr)
	{
		if ((Object)(object)mgr != (Object)null)
		{
			return mgr.CurrentLevelMapFlipMode == LevelMapFlipMode.FlipXY;
		}
		return false;
	}

	public static void ApplyFlipCompensation(Transform transform, GameplayManager mgr)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)transform == (Object)null) && IsFlipped(mgr))
		{
			Vector3 localEulerAngles = transform.localEulerAngles;
			localEulerAngles.z = Mathf.Repeat(localEulerAngles.z + 180f, 360f);
			transform.localEulerAngles = localEulerAngles;
		}
	}
}
