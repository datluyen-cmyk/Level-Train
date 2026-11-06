using UnityEngine;

public static class RingGapGeometry
{
	public const float DefaultTailLength = 1f;

	public const float DefaultHeadLength = 1f;

	public static void ReadCapLengths(Material material, out float tailLength, out float headLength)
	{
		tailLength = 1f;
		headLength = 1f;
		if (!((Object)(object)material == (Object)null))
		{
			if (material.HasFloat("_TailLength"))
			{
				tailLength = material.GetFloat("_TailLength");
			}
			if (material.HasFloat("_HeadLength"))
			{
				headLength = material.GetFloat("_HeadLength");
			}
		}
	}

	public static float ComputeCapShrinkDeg(float centerRadius, float tailLength, float headLength, float capRatio)
	{
		if (centerRadius <= 0.001f || capRatio <= 0.001f)
		{
			return 0f;
		}
		return (tailLength + headLength) / 2f * capRatio / centerRadius * 57.29578f;
	}

	public static float ComputeRenderGapDeg(float physicalGapDeg, float centerRadius, Material material, float capRatio)
	{
		if (physicalGapDeg <= 0.1f)
		{
			return 0f;
		}
		ReadCapLengths(material, out var tailLength, out var headLength);
		float num = ComputeCapShrinkDeg(centerRadius, tailLength, headLength, capRatio);
		return Mathf.Max(0f, physicalGapDeg - num);
	}
}
