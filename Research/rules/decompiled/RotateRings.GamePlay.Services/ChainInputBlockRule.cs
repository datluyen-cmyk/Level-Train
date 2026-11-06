using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public class ChainInputBlockRule : IInputBlockRule
{
	private readonly List<RingObject> lockedRingsBuffer = new List<RingObject>();

	public bool IsBlocking(Vector2 screenPosition)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		Camera gameplayCamera = GameplayCameraManager.GameplayCamera;
		RingLockChainService ringLockChainService = SingletonComponent<GameplayManager>.Instance?.LockChain;
		if ((Object)(object)gameplayCamera == (Object)null || ringLockChainService == null)
		{
			return false;
		}
		ringLockChainService.CollectLockedRings(lockedRingsBuffer);
		if (lockedRingsBuffer.Count == 0)
		{
			return false;
		}
		Vector2 val = ScreenToWorldPoint(gameplayCamera, screenPosition);
		for (int i = 0; i < lockedRingsBuffer.Count; i++)
		{
			RingObject ringObject = lockedRingsBuffer[i];
			if (!((Object)(object)ringObject == (Object)null) && !(ringObject.PartData.OuterRadius <= 0.0001f))
			{
				Vector2 val2 = Vector2.op_Implicit(((Component)ringObject).transform.position);
				float num = GameCoordScaleUtil.CoreToUnity(ringObject.PartData.InnerRadius);
				Vector2 val3 = val - val2;
				if (((Vector2)(ref val3)).sqrMagnitude <= num * num)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static Vector2 ScreenToWorldPoint(Camera camera, Vector2 screenPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Abs(((Component)camera).transform.position.z);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(screenPosition.x, screenPosition.y, num);
		Vector3 val2 = camera.ScreenToWorldPoint(val);
		val2.z = 0f;
		return Vector2.op_Implicit(val2);
	}
}
