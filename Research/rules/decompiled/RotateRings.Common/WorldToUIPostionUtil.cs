using UnityEngine;

namespace RotateRings.Common;

public static class WorldToUIPostionUtil
{
	public static bool WorldToLocalPoint(Vector3 worldPosition, RectTransform parent, out Vector2 localPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		localPoint = default(Vector2);
		if ((Object)(object)parent == (Object)null)
		{
			return false;
		}
		Camera gameplayCamera = GameplayCameraManager.GameplayCamera;
		if ((Object)(object)gameplayCamera == (Object)null)
		{
			return false;
		}
		Vector3 val = gameplayCamera.WorldToScreenPoint(worldPosition);
		Canvas componentInParent = ((Component)parent).GetComponentInParent<Canvas>();
		Camera val2 = (((Object)(object)componentInParent != (Object)null) ? componentInParent.rootCanvas.worldCamera : null);
		return RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Vector2.op_Implicit(val), val2, ref localPoint);
	}

	public static float WorldRadiusToLocal(Vector3 worldCenter, float worldRadius, RectTransform parent)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (worldRadius <= 0f)
		{
			return 0f;
		}
		Vector3 worldPosition = worldCenter + Vector3.up * worldRadius;
		if (!WorldToLocalPoint(worldCenter, parent, out var localPoint))
		{
			return worldRadius;
		}
		if (!WorldToLocalPoint(worldPosition, parent, out var localPoint2))
		{
			return worldRadius;
		}
		return Mathf.Abs(localPoint2.y - localPoint.y);
	}
}
