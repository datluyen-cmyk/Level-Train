using System;
using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Ad;
using RotateRings.Prop;
using RotateRings.Services;
using RotateRings.Shop;
using RotateRings.UI.BuySuccess;
using UniLog;
using XingYun.Command;
using XingYun.Core.Singletons;
using XingYun.UI.Core;

namespace RotateRings.UI.Shop;

public sealed class PurchaseResultCommand : BaseCommand
{
	private const string Tag = "PurchaseResultCommand";

	public override void Execute(object param = null)
	{
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Invalid comparison between Unknown and I4
		if (!(param is PurchaseResultParam purchaseResultParam))
		{
			Log.LogError("PurchaseResultCommand", "购买结果参数无效。");
			return;
		}
		purchaseResultParam.IsDelivered = false;
		if (purchaseResultParam.Order == null || purchaseResultParam.Order.count <= 0)
		{
			Log.LogError("PurchaseResultCommand", "购买订单或购买数量无效。");
			return;
		}
		ShopBaseGoods storeItemConfig = Singleton<StoreManager>.Instance.GetStoreItemConfig(purchaseResultParam.Order.shopItemId);
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		ShopModelProxy shopModelProxy = GameApp.Proxy.Get<ShopModelProxy>();
		if (storeItemConfig?.BuyItems == null || propModelProxy == null || shopModelProxy == null)
		{
			Log.LogError("PurchaseResultCommand", "购买商品配置、道具代理或商城代理无效，shopItemId={0}", new object[1] { purchaseResultParam.Order.shopItemId });
			return;
		}
		List<ShopRewardItem> list = new List<ShopRewardItem>(storeItemConfig.BuyItems.Count);
		foreach (KeyValuePair<int, int> buyItem in storeItemConfig.BuyItems)
		{
			EPropType key = (EPropType)buyItem.Key;
			if (!Enum.IsDefined(typeof(EPropType), key) || buyItem.Value <= 0)
			{
				Log.LogError("PurchaseResultCommand", "商城奖励配置无效，shopItemId={0} propId={1} count={2}", new object[3]
				{
					purchaseResultParam.Order.shopItemId,
					buyItem.Key,
					buyItem.Value
				});
				return;
			}
			int count = buyItem.Value * purchaseResultParam.Order.count;
			list.Add(new ShopRewardItem
			{
				PropId = buyItem.Key,
				Count = count
			});
		}
		if (list.Count == 0)
		{
			Log.LogError("PurchaseResultCommand", "商城商品没有可发放奖励，shopItemId={0}", new object[1] { purchaseResultParam.Order.shopItemId });
			return;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>(list.Count);
		foreach (ShopRewardItem item in list)
		{
			EPropType propId = (EPropType)item.PropId;
			propModelProxy.AddProp(propId, item.Count, TrackPropGainReason.ShopBuyReward, purchaseResultParam.Order.shopItemId.ToString(), purchaseResultParam.Order.orderId);
			dictionary.TryGetValue(item.PropId, out var value);
			dictionary[item.PropId] = value + item.Count;
		}
		CloudSaveCache.AddPendingPurchasePropCounts(dictionary);
		shopModelProxy.RecordBuy(purchaseResultParam.Order.shopItemId);
		CloudSaveCache.Data.HasEverPurchased = true;
		purchaseResultParam.IsDelivered = true;
		PurchaseService.RequestCloudSaveSyncAfterPurchase();
		if (propModelProxy.NoAd)
		{
			AdService.HideBanner();
		}
		if ((int)purchaseResultParam.ProductType == 2)
		{
			GameApp.Event.Send<NonConsumablePurchaseSuccessEvent>(new NonConsumablePurchaseSuccessEvent
			{
				Order = purchaseResultParam.Order
			});
		}
		GameApp.Event.Send<ShopBuyResultEvent>(new ShopBuyResultEvent
		{
			BuyGoodsId = purchaseResultParam.Order.shopItemId,
			ExclusionId = storeItemConfig.ExclusionId,
			IsSuccess = true
		});
		GameApp.UI.Open("UIBuySuccess", (UILayer)2, (object)new UIBuySuccessParam
		{
			ShopId = storeItemConfig.ID,
			Rewards = list
		}, (Action)null);
	}
}
