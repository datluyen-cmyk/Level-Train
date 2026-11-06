using UnityEngine;
using XingYun.Utility;

namespace Rotaterings.Rank;

public static class RankScoreService
{
	public const int DisplayTierCount = 5;

	private static readonly int[] Thresholds = new int[6] { 0, 1, 2, 3, 4, 5 };

	private static readonly int[] MultiplierValues = new int[6] { 0, 1, 2, 4, 8, 10 };

	public static int GetMultiplier(int winStreak)
	{
		for (int num = Thresholds.Length - 1; num >= 0; num--)
		{
			if (winStreak >= Thresholds[num])
			{
				return MultiplierValues[num];
			}
		}
		return 0;
	}

	public static int GetTierByIndex(int idx)
	{
		if (idx < 0 || idx >= MultiplierValues.Length)
		{
			return 0;
		}
		return MultiplierValues[idx];
	}

	public static int GetTierIndex(int winStreak)
	{
		int result = 0;
		for (int i = 0; i < Thresholds.Length; i++)
		{
			if (winStreak >= Thresholds[i])
			{
				result = i;
			}
		}
		return result;
	}

	public static int SettleOnVictory(RankModelData data)
	{
		data.LastMultiplierTier = data.MultiplierTier;
		int num = data.MultiplierTier + 1;
		int multiplier = GetMultiplier(num);
		int num2 = Mathf.FloorToInt((float)(data.StarCountOneRound * multiplier));
		if (num2 > 0)
		{
			data.MultiplierTier = num;
		}
		return num2;
	}

	public static void ResetMultiplierIfActive()
	{
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		if (rankModelProxy != null)
		{
			long currentTimestamp = Time.GetCurrentTimestamp();
			if (rankModelProxy.Data.EndTimeStamp > 0 && currentTimestamp < rankModelProxy.Data.EndTimeStamp)
			{
				rankModelProxy.Data.LastMultiplierTier = rankModelProxy.Data.MultiplierTier;
				rankModelProxy.Data.MultiplierTier = 0;
			}
		}
	}
}
