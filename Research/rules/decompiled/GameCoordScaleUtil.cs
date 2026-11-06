using UnityEngine;

public static class GameCoordScaleUtil
{
	public const float CoordScale = 1f / 24f;

	public static float CoreToUnity(float coreValue)
	{
		return coreValue * (1f / 24f);
	}

	public static bool TryGetInnerDiameterWorld(float innerRadius, float outerRadius, float worldScale, out float innerDiameterWorld)
	{
		innerDiameterWorld = 0f;
		if (outerRadius <= 0.0001f)
		{
			return false;
		}
		float num = (3f * innerRadius - outerRadius) * (1f / 24f);
		innerDiameterWorld = num * worldScale;
		return true;
	}

	public static bool TryGetRingInnerDiameterWorld(RingObject ring, out float innerDiameterWorld)
	{
		innerDiameterWorld = 0f;
		if ((Object)(object)ring == (Object)null)
		{
			return false;
		}
		GameplayPartData partData = ring.PartData;
		if (partData.OuterRadius <= 0.0001f)
		{
			return false;
		}
		return TryGetInnerDiameterWorld(partData.InnerRadius, partData.OuterRadius, 1f, out innerDiameterWorld);
	}
}
