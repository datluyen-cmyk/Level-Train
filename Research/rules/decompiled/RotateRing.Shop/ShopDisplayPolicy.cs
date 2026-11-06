using System;
using Example.Cfg;
using RotateRings.Shop;
using XingYun.Aot;

namespace RotateRing.Shop;

public static class ShopDisplayPolicy
{
	private const string LogTag = "ShopDisplayPolicy";

	public static bool CanDisplayGoods(int shopGoodsId)
	{
		ShopConfig orDefault = GameApp.Config.GetConfig<ExampleTables, TbShopConfig>((Func<ExampleTables, TbShopConfig>)((ExampleTables tb) => tb.TbShopConfig)).GetOrDefault(shopGoodsId);
		if (orDefault == null)
		{
			return false;
		}
		int buyCount = GameApp.Proxy.Get<ShopModelProxy>().GetBuyCount(shopGoodsId);
		if (buyCount == -1)
		{
			return false;
		}
		ShopBaseGoods orDefault2 = GameApp.Config.GetConfig<ExampleTables, TbShopBaseGoods>((Func<ExampleTables, TbShopBaseGoods>)((ExampleTables tb) => tb.TbShopBaseGoods)).GetOrDefault(shopGoodsId);
		if (orDefault2 == null)
		{
			return false;
		}
		if (orDefault2.LimitedCount > 0 && buyCount >= orDefault2.LimitedCount)
		{
			return false;
		}
		if (!IsReviewMode())
		{
			return orDefault.showItem;
		}
		return true;
	}

	public static bool IsReviewMode()
	{
		return SDKManager.IsNativeUnderR();
	}
}
