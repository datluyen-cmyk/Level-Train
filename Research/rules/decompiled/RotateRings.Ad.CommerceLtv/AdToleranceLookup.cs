using UnityEngine;

namespace RotateRings.Ad.CommerceLtv;

public static class AdToleranceLookup
{
	private static readonly (int x, float p)[] RewardReverseAnchors = new(int, float)[25]
	{
		(0, 0.6596f),
		(1, 0.2301f),
		(2, 0.1092f),
		(3, 0.0611f),
		(4, 0.0358f),
		(5, 0.0212f),
		(6, 0.0127f),
		(7, 0.0075f),
		(8, 0.0045f),
		(9, 0.0028f),
		(10, 0.00177f),
		(11, 0.00114f),
		(12, 0.00076f),
		(13, 0.00052f),
		(14, 0.00036f),
		(15, 0.00025f),
		(16, 0.00018f),
		(17, 0.00013f),
		(18, 0.0001f),
		(19, 7.4E-05f),
		(20, 5.8E-05f),
		(21, 4.5E-05f),
		(22, 3.5E-05f),
		(23, 2.7E-05f),
		(24, 2.1E-05f)
	};

	private static readonly float[] R7MidRanks = new float[8] { 0.0598f, 0.131f, 0.159f, 0.2124f, 0.2929f, 0.409f, 0.5787f, 0.8381f };

	public static float PRewardReverse(float x)
	{
		if (x <= 0f)
		{
			return RewardReverseAnchors[0].p;
		}
		for (int i = 0; i < RewardReverseAnchors.Length - 1; i++)
		{
			float num = RewardReverseAnchors[i].x;
			float num2 = RewardReverseAnchors[i + 1].x;
			if (x >= num && x <= num2)
			{
				float num3 = (x - num) / (num2 - num);
				return RewardReverseAnchors[i].p + num3 * (RewardReverseAnchors[i + 1].p - RewardReverseAnchors[i].p);
			}
		}
		return Mathf.Max(1E-06f, 2.1E-05f * Mathf.Exp(-0.25f * (x - 24f)));
	}

	public static float PR7(int nonEscapeCount, int totalCount)
	{
		if (totalCount <= 0)
		{
			return R7MidRanks[0];
		}
		float num = (float)nonEscapeCount / (float)totalCount;
		if (num < 1f / 7f)
		{
			return R7MidRanks[0];
		}
		if (num < 0.2857143f)
		{
			return R7MidRanks[1];
		}
		if (num < 0.42857143f)
		{
			return R7MidRanks[2];
		}
		if (num < 0.5714286f)
		{
			return R7MidRanks[3];
		}
		if (num < 0.71428573f)
		{
			return R7MidRanks[4];
		}
		if (num < 0.85714287f)
		{
			return R7MidRanks[5];
		}
		if (num < 1f)
		{
			return R7MidRanks[6];
		}
		return R7MidRanks[7];
	}

	public static float NMultiplier(int lifetimeInterCount)
	{
		if (lifetimeInterCount <= 5)
		{
			return 1.5f;
		}
		if (lifetimeInterCount <= 15)
		{
			return 1.3f;
		}
		if (lifetimeInterCount <= 30)
		{
			return 1.2f;
		}
		if (lifetimeInterCount <= 50)
		{
			return 1.1f;
		}
		return 1.05f;
	}

	public static void ComputeCooldownSeconds(float minCd, float maxCd, float tolerance, out float sigmoidValue, out float finalCdSeconds)
	{
		sigmoidValue = 1f / (1f + Mathf.Exp(-5f * (tolerance - 0.5f)));
		finalCdSeconds = minCd + (maxCd - minCd) * (1f - sigmoidValue);
	}
}
