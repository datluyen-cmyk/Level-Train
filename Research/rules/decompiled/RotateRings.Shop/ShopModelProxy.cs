using System;
using System.Collections.Generic;
using Example.Cfg;
using XingYun.Proxy;

namespace RotateRings.Shop;

public class ShopModelProxy : BaseProxy
{
	public ShopModelData Data { get; }

	public ShopModelProxy()
		: base("ShopModelProxy")
	{
		Data = GameApp.Save.GetOrCreate<ShopModelData>();
		ShopModelData data = Data;
		if (data.GoodsBuyCount == null)
		{
			data.GoodsBuyCount = new Dictionary<int, int>();
		}
	}

	public bool EnsureShopEntryLevelState(int mode, int level, int configValue)
	{
		if (Data.HasShopEntryLevelState && Data.ShopEntryMode == mode && Data.ShopEntryLevel == level && Data.ShopEntryConfigValue == configValue)
		{
			return false;
		}
		Data.HasShopEntryLevelState = true;
		Data.ShopEntryMode = mode;
		Data.ShopEntryLevel = level;
		Data.ShopEntryConfigValue = configValue;
		Data.ShopEntryConditionMet = false;
		Data.ShopEntryRewardAdCompletedCount = 0;
		Data.ShopEntryLevelStartedUtcTicks = DateTime.UtcNow.Ticks;
		Data.ShopEntryLastOperationUtcTicks = 0L;
		return true;
	}

	public int GetBuyCount(int shopGoodsId)
	{
		if (!Data.GoodsBuyCount.TryGetValue(shopGoodsId, out var value))
		{
			return 0;
		}
		return value;
	}

	public void RecordBuy(int shopGoodsId)
	{
		int buyCount = GetBuyCount(shopGoodsId);
		Data.GoodsBuyCount[shopGoodsId] = ((buyCount < 0) ? 1 : (buyCount + 1));
		ShopBaseGoods shopBaseGoods = GameApp.Config.GetTable<ExampleTables>()?.TbShopBaseGoods?.GetOrDefault(shopGoodsId);
		if (shopBaseGoods != null && shopBaseGoods.ExclusionId > 0)
		{
			Data.GoodsBuyCount[shopBaseGoods.ExclusionId] = -1;
		}
	}
}
