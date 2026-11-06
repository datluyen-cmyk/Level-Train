using UnityEngine;

public struct GameplayPointerData
{
	public int FingerId;

	public Vector2 ScreenPosition;

	public Vector3 WorldPosition;

	public GameplayPointerData(int fingerId, Vector2 screenPosition, Vector3 worldPosition)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		FingerId = fingerId;
		ScreenPosition = screenPosition;
		WorldPosition = worldPosition;
	}
}
