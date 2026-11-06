using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Example.Cfg;
using RotateRings.Model;
using RotateRings.Prop;
using UniLog;
using XingYun.Core.Singletons;
using XingYun.Track;
using XingYun.Utility;

namespace RotateRings.UI.Shop;

public class IAPTrackerProvider : Singleton<IAPTrackerProvider>
{
	public class IAPTrackingContext
	{
		public int EnterPath = 1;

		public int Level;

		public int GameMode;

		public int ModeNum;

		public int ModeNumAll;
	}

	private Dictionary<string, IAPTrackingContext> trackingContexts = new Dictionary<string, IAPTrackingContext>();

	public IAPTrackingContext CreateContext(EIapEnterPath enterPath)
	{
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		IAPTrackingContext iAPTrackingContext = new IAPTrackingContext
		{
			EnterPath = (int)enterPath
		};
		if (levelModelProxy != null)
		{
			iAPTrackingContext.Level = levelModelProxy.Data.GameModeData.level;
			iAPTrackingContext.GameMode = levelModelProxy.Data.GameModeData.game_mode;
			iAPTrackingContext.ModeNum = levelModelProxy.Data.GameModeData.mode_num;
			iAPTrackingContext.ModeNumAll = levelModelProxy.Data.GetAllModeNum();
		}
		return iAPTrackingContext;
	}

	public string AllocateLinkUniqueId(IAPTrackingContext context)
	{
		string text = Guid.NewGuid().ToString();
		trackingContexts[text] = context;
		return text;
	}

	public IAPTrackingContext GetTrackingContext(string linkUniqueId)
	{
		if (string.IsNullOrEmpty(linkUniqueId))
		{
			return null;
		}
		trackingContexts.TryGetValue(linkUniqueId, out var value);
		return value;
	}

	public void ReleaseTrackingContext(string linkUniqueId)
	{
		if (!string.IsNullOrEmpty(linkUniqueId) && trackingContexts.ContainsKey(linkUniqueId))
		{
			trackingContexts.Remove(linkUniqueId);
		}
	}

	public void TrackMarketData(string dataName, string infoJson)
	{
		GameApp.Track.ReportEvent(dataName, infoJson);
	}

	private string GetHistoryPaidUserFlag()
	{
		PlayerModelData orCreate = GameApp.Save.GetOrCreate<PlayerModelData>();
		if (orCreate != null)
		{
			if (!orCreate.IsPaidUser)
			{
				return "n";
			}
			return "y";
		}
		return "n";
	}

	private double ParsePriceFromFormatted(string formattedPrice)
	{
		if (string.IsNullOrEmpty(formattedPrice))
		{
			return 0.0;
		}
		double result = default(double);
		if (Parser.TryParse(Regex.Replace(formattedPrice, "[^\\d\\.]", ""), ref result))
		{
			return result;
		}
		return 0.0;
	}

	public void TrackIapPageLoad(string status, string sku, string skuType)
	{
	}

	public void TrackIapAction(string scene, string sku, string skuType, string label, double price)
	{
	}

	public void TrackIapPurchase(string sku, string skuType, string result, string label, double price, string gameOrderId, string orderId, int itemCount, int shopItemId, string linkUniqueId = null)
	{
		GIapPurchase gIapPurchase = new GIapPurchase();
		gIapPurchase.sku = sku;
		gIapPurchase.sku_type = skuType;
		gIapPurchase.result = result;
		gIapPurchase.label = label;
		gIapPurchase.price = price;
		gIapPurchase.history_paid_user = GetHistoryPaidUserFlag();
		gIapPurchase.game_order_id = gameOrderId;
		gIapPurchase.order_id = orderId;
		gIapPurchase.item_count = itemCount;
		FillTrackModelData((BaseTrackConfig)(object)gIapPurchase, linkUniqueId);
		FillItemDetails((BaseTrackConfig)(object)gIapPurchase, shopItemId);
		SendEvent("g_iap_purchase", gIapPurchase);
		switch (result)
		{
		case "purchase_report_success":
		case "purchase_report_failed":
		case "purchase_cancel":
			ReleaseTrackingContext(linkUniqueId);
			break;
		}
	}

	public void TrackGameIapCreateOrder(string sku, string skuType, string label, string price, string currencyCode, string gameOrderId, int shopItemId, string linkUniqueId = null)
	{
		GGameIapCreateOrder gGameIapCreateOrder = new GGameIapCreateOrder();
		((BaseTrackConfig)gGameIapCreateOrder).Reset();
		gGameIapCreateOrder.sku = sku;
		gGameIapCreateOrder.sku_type = skuType;
		gGameIapCreateOrder.price = ParsePriceFromFormatted(price);
		gGameIapCreateOrder.currency_code = currencyCode;
		gGameIapCreateOrder.history_paid_user = GetHistoryPaidUserFlag();
		gGameIapCreateOrder.game_order_id = gameOrderId;
		gGameIapCreateOrder.item_count = 1.0;
		gGameIapCreateOrder.label = label;
		FillTrackModelData((BaseTrackConfig)(object)gGameIapCreateOrder, linkUniqueId);
		FillItemDetails((BaseTrackConfig)(object)gGameIapCreateOrder, shopItemId);
		SendEvent("g_game_iap_create_order", gGameIapCreateOrder);
	}

	private void FillTrackModelData(BaseTrackConfig evt, string linkUniqueId)
	{
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (levelModelProxy != null)
		{
			IAPTrackingContext trackingContext = GetTrackingContext(linkUniqueId);
			int level = levelModelProxy.Data.GameModeData.level;
			int game_mode = levelModelProxy.Data.GameModeData.game_mode;
			int mode_num = levelModelProxy.Data.GameModeData.mode_num;
			int mode_num_all = levelModelProxy.Data.GetAllModeNum();
			int enter_path = 1;
			if (trackingContext != null)
			{
				level = trackingContext.Level;
				game_mode = trackingContext.GameMode;
				mode_num = trackingContext.ModeNum;
				mode_num_all = trackingContext.ModeNumAll;
				enter_path = trackingContext.EnterPath;
			}
			if (evt is GGameIapCreateOrder gGameIapCreateOrder)
			{
				gGameIapCreateOrder.level = level;
				gGameIapCreateOrder.game_mode = game_mode;
				gGameIapCreateOrder.mode_num = mode_num;
				gGameIapCreateOrder.mode_num_all = mode_num_all;
				gGameIapCreateOrder.enter_path = enter_path;
			}
			else if (evt is GIapPurchase gIapPurchase)
			{
				gIapPurchase.level = level;
				gIapPurchase.game_mode = game_mode;
				gIapPurchase.mode_num = mode_num;
				gIapPurchase.mode_num_all = mode_num_all;
				gIapPurchase.enter_path = enter_path;
			}
		}
	}

	private void FillItemDetails(BaseTrackConfig evt, int shopItemId)
	{
		ShopBaseGoods orDefault = GameApp.Config.GetTable<ExampleTables>().TbShopBaseGoods.GetOrDefault(shopItemId);
		if (orDefault?.BuyItems == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (KeyValuePair<int, int> buyItem in orDefault.BuyItems)
		{
			EPropType key = (EPropType)buyItem.Key;
			int value = buyItem.Value;
			switch (key)
			{
			case EPropType.Skyrocket:
				num += value;
				break;
			case EPropType.Hammer:
				num2 += value;
				break;
			}
		}
		if (!(evt is GGameIapCreateOrder))
		{
			_ = evt is GIapPurchase;
		}
	}

	private void SendEvent<T>(string eventName, T eventData)
	{
		try
		{
			string infoJson = GameApp.Json.Serialize<T>(eventData);
			TrackMarketData(eventName, infoJson);
		}
		catch (Exception ex)
		{
			Log.Error((object)$"[IAPTracker] Failed to send event {eventName}: {ex.Message} {ex.StackTrace} eventData: is null: {eventData == null} ", Array.Empty<object>());
		}
	}
}
