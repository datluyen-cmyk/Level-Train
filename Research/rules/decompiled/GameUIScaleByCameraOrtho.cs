using UnityEngine;

public static class GameUIScaleByCameraOrtho
{
	public static float DefaultReferenceOrthoSize = 8f;

	public static float BiggerItemScaleFactor = 1.2f;

	public static float GetScaleFactor(bool isBiggerItem = false, float referenceOrthoSize = -1f)
	{
		if (referenceOrthoSize < 0f)
		{
			referenceOrthoSize = DefaultReferenceOrthoSize;
		}
		float num = (isBiggerItem ? BiggerItemScaleFactor : 1f);
		if (referenceOrthoSize <= 0f)
		{
			return num;
		}
		Camera gameplayCamera = GameplayCameraManager.GameplayCamera;
		if ((Object)(object)gameplayCamera == (Object)null)
		{
			return num;
		}
		return referenceOrthoSize / gameplayCamera.orthographicSize * num;
	}
}
