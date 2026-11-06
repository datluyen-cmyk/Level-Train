using System;
using Example.Cfg;
using UnityEngine;

namespace RotateRings.RewardedLuckyDraw;

public static class RewardedLuckyDrawRewardResolver
{
	public static bool TryGetPool(int groupId, out RewardedLuckyDrawProbability[] slots)
	{
		slots = new RewardedLuckyDrawProbability[6];
		TbRewardedLuckyDrawProbability tbRewardedLuckyDrawProbability = GameApp.Config.GetTable<ExampleTables>()?.TbRewardedLuckyDrawProbability;
		if (tbRewardedLuckyDrawProbability == null || groupId < 0 || groupId > 1)
		{
			return false;
		}
		long num = 0L;
		int num2 = 0;
		foreach (RewardedLuckyDrawProbability data in tbRewardedLuckyDrawProbability.DataList)
		{
			if (data.GroupId == groupId)
			{
				if (data.SlotId < 1 || data.SlotId > 6 || slots[data.SlotId - 1] != null || data.PropType != ((data.SlotId % 2 == 1) ? 202 : 201) || data.Count != (data.SlotId + 1) / 2 + 1 || data.Weight < 0)
				{
					return false;
				}
				slots[data.SlotId - 1] = data;
				num += data.Weight;
				num2++;
			}
		}
		if (num2 == 6 && num > 0)
		{
			return num <= int.MaxValue;
		}
		return false;
	}

	public static RewardedLuckyDrawProbability Resolve(RewardedLuckyDrawProbability[] slots, bool firstClaim)
	{
		if (firstClaim)
		{
			return slots[4];
		}
		int num = 0;
		foreach (RewardedLuckyDrawProbability rewardedLuckyDrawProbability in slots)
		{
			num += rewardedLuckyDrawProbability.Weight;
		}
		return SelectByWeight(slots, Random.Range(0, num));
	}

	public static RewardedLuckyDrawProbability SelectByWeight(RewardedLuckyDrawProbability[] slots, int roll)
	{
		if (roll < 0)
		{
			throw new ArgumentOutOfRangeException("roll");
		}
		foreach (RewardedLuckyDrawProbability rewardedLuckyDrawProbability in slots)
		{
			if (roll < rewardedLuckyDrawProbability.Weight)
			{
				return rewardedLuckyDrawProbability;
			}
			roll -= rewardedLuckyDrawProbability.Weight;
		}
		throw new InvalidOperationException("RewardedLuckyDraw weight selection failed");
	}
}
