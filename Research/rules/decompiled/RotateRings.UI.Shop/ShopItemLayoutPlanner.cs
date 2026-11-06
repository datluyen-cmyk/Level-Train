using System.Collections.Generic;
using Example.Cfg;
using RotateRing.Shop;

namespace RotateRings.UI.Shop;

public sealed class ShopItemLayoutPlanner
{
	public SortedDictionary<int, List<int>> BuildGroups(List<ShopConfig> configs)
	{
		SortedDictionary<int, List<int>> sortedDictionary = new SortedDictionary<int, List<int>>();
		if (configs == null)
		{
			return sortedDictionary;
		}
		foreach (ShopConfig config in configs)
		{
			if (config != null && ShopDisplayPolicy.CanDisplayGoods(config.ID))
			{
				if (!sortedDictionary.TryGetValue(config.giftType, out var value))
				{
					value = new List<int>();
					sortedDictionary[config.giftType] = value;
				}
				value.Add(config.ID);
			}
		}
		return sortedDictionary;
	}

	public int GetContentCount(SortedDictionary<int, List<int>> groups, EGiftType giftType)
	{
		if (groups == null || !groups.TryGetValue((int)giftType, out var value))
		{
			return 0;
		}
		return value.Count;
	}
}
