using UnityEngine;

namespace Game.DataTrack;

internal static class GameplayMoveTrackThresholds
{
	private const float MinPullTranslationMillimeters = 2f;

	private const float MinRotationDegrees = 3f;

	public static bool ShouldRecordMoveTime(IGameplayObject gameplayObject)
	{
		if (gameplayObject is DragRotateObject dragRotateObject)
		{
			return dragRotateObject.GetGestureRotationDegreesFromStart() >= 3f;
		}
		if (gameplayObject is DraggableObjectWithLimitRange draggableObjectWithLimitRange)
		{
			float num = MillimetersToWorldUnits(2f);
			return draggableObjectWithLimitRange.GetGestureTranslationWorldFromStart() >= num;
		}
		return false;
	}

	private static float MillimetersToWorldUnits(float millimeters)
	{
		if (millimeters <= 0f)
		{
			return 0f;
		}
		Camera gameplayCamera = GameplayCameraManager.GameplayCamera;
		if ((Object)(object)gameplayCamera == (Object)null || !gameplayCamera.orthographic || Screen.height <= 0)
		{
			return 0f;
		}
		float num = ((Screen.dpi > 0f) ? Screen.dpi : 160f);
		float num2 = millimeters / 25.4f * num;
		float num3 = 2f * gameplayCamera.orthographicSize / (float)Screen.height;
		return num2 * num3;
	}
}
