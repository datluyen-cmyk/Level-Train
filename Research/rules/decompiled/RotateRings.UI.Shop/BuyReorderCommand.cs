using System;
using System.Collections.Generic;
using Example.Cfg;
using UniLog;
using XingYun.Command;
using XingYun.UI.Core;

namespace RotateRings.UI.Shop;

public sealed class BuyReorderCommand : BaseCommand
{
	public sealed class BuyReorderParams
	{
		public int ShopItemId { get; init; }

		public Action OnConfirm { get; init; }
	}

	private const string Tag = "BuyReorderCommand";

	public override void Execute(object param = null)
	{
		if (!(param is BuyReorderParams { OnConfirm: not null } buyReorderParams))
		{
			return;
		}
		ShopBaseGoods shopBaseGoods = GameApp.Config.GetTable<ExampleTables>()?.TbShopBaseGoods?.GetOrDefault(buyReorderParams.ShopItemId);
		if (shopBaseGoods?.BuyItems == null || shopBaseGoods.BuyItems.Count == 0)
		{
			Log.LogError("BuyReorderCommand", "补单商品配置不存在或奖励为空，shopItemId={0}", new object[1] { buyReorderParams.ShopItemId });
			return;
		}
		List<ShopRewardItem> list = new List<ShopRewardItem>(shopBaseGoods.BuyItems.Count);
		foreach (KeyValuePair<int, int> buyItem in shopBaseGoods.BuyItems)
		{
			list.Add(new ShopRewardItem
			{
				PropId = buyItem.Key,
				Count = buyItem.Value
			});
		}
		BuyReorderUIParam buyReorderUIParam = new BuyReorderUIParam
		{
			Items = list,
			OnConfirm = buyReorderParams.OnConfirm
		};
		GameApp.UI.Open("UIBuyReorder", (UILayer)2, (object)buyReorderUIParam, (Action)null);
	}
}
