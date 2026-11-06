using UnityEngine;

namespace XY.Common.ParticleImage;

public static class Extensions
{
	public static float Remap(this float value, float from1, float to1, float from2, float to2)
	{
		float result = (value - from1) / (to1 - from1) * (to2 - from2) + from2;
		if (Mathf.Approximately(from1, to1))
		{
			return to1;
		}
		return result;
	}
}
