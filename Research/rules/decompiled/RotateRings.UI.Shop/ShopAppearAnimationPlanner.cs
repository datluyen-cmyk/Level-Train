using System.Collections.Generic;

namespace RotateRings.UI.Shop;

public static class ShopAppearAnimationPlanner
{
	public static List<List<ShopItemView>> BuildGroups(List<ShopItemView> orderedItems, int maxAnimatedRows, int propItemsPerRow, List<ShopItemView> itemsToSetVisible)
	{
		List<List<ShopItemView>> list = new List<List<ShopItemView>>();
		itemsToSetVisible?.Clear();
		if (orderedItems == null || maxAnimatedRows <= 0)
		{
			return list;
		}
		int num = 0;
		List<ShopItemView> list2 = null;
		foreach (ShopItemView orderedItem in orderedItems)
		{
			if (orderedItem?.ShopConfig == null)
			{
				continue;
			}
			if (num >= maxAnimatedRows)
			{
				itemsToSetVisible?.Add(orderedItem);
				continue;
			}
			switch ((EGiftType)orderedItem.ShopConfig.giftType)
			{
			case EGiftType.SpecialOffer:
				num++;
				list.Add(new List<ShopItemView> { orderedItem });
				break;
			case EGiftType.Prop:
				if (list2 == null)
				{
					list2 = new List<ShopItemView>();
				}
				list2.Add(orderedItem);
				if (list2.Count >= propItemsPerRow)
				{
					num++;
					list.Add(list2);
					list2 = null;
				}
				break;
			default:
				itemsToSetVisible?.Add(orderedItem);
				break;
			}
		}
		if (list2 == null || list2.Count == 0)
		{
			return list;
		}
		if (num < maxAnimatedRows)
		{
			list.Add(list2);
		}
		else
		{
			itemsToSetVisible?.AddRange(list2);
		}
		return list;
	}
}
