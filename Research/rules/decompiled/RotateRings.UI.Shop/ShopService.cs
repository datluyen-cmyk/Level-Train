using System;
using System.Collections.Generic;
using Example.Cfg;
using Hot.Model;
using RotateRings.Common;
using RotateRings.GamePlay.Services;
using RotateRings.IAP;
using RotateRings.Model;
using RotateRings.Prop;
using RotateRings.Shop;
using UniLog;
using XingYun.IAP.Core;

namespace RotateRings.UI.Shop;

public static class ShopService
{
	private static bool isInit;

	private static Dictionary<string, XingYunSkuDetailItem> _skuDetailItems = new Dictionary<string, XingYunSkuDetailItem>();

	public static void Init()
	{
		if (!isInit)
		{
			isInit = true;
			GameApp.Event.Listen<SkuDetailsUpdatedEvent>((Action<SkuDetailsUpdatedEvent>)SkuDetailsUpdatedHandler);
		}
	}

	public static void Release()
	{
		isInit = false;
		GameApp.Event.Unlisten<SkuDetailsUpdatedEvent>((Action<SkuDetailsUpdatedEvent>)SkuDetailsUpdatedHandler);
	}

	public static XingYunSkuDetailItem GetSkuDetailItem(string skuId)
	{
		return _skuDetailItems.GetValueOrDefault(skuId);
	}

	public static void SetSkuDetailItem(Dictionary<string, XingYunSkuDetailItem> skuDetailItems)
	{
		_skuDetailItems = skuDetailItems;
	}

	private static void SkuDetailsUpdatedHandler(SkuDetailsUpdatedEvent obj)
	{
		if (obj == null || obj.SkuDetails == null)
		{
			return;
		}
		List<XingYunSkuDetailItem> skuDetails = obj.SkuDetails;
		Dictionary<string, XingYunSkuDetailItem> dictionary = new Dictionary<string, XingYunSkuDetailItem>(skuDetails.Count);
		foreach (XingYunSkuDetailItem item in skuDetails)
		{
			if (item != null && !string.IsNullOrEmpty(item.skuId))
			{
				dictionary[item.skuId] = item;
			}
		}
		_skuDetailItems = dictionary;
	}

	public static string FormatCurrencyWithSpace(string priceText)
	{
		if (string.IsNullOrEmpty(priceText))
		{
			return priceText;
		}
		int num = -1;
		for (int i = 0; i < priceText.Length; i++)
		{
			if (char.IsDigit(priceText[i]))
			{
				num = i;
				break;
			}
		}
		if (num <= 0)
		{
			return priceText;
		}
		if (char.IsWhiteSpace(priceText[num - 1]))
		{
			return priceText;
		}
		return priceText.Insert(num, " ");
	}

	public static string GetPopularContent(int popularId)
	{
		return popularId switch
		{
			1 => GameApp.Lang.GetText("Shop_Pack_Popular_Tag", Array.Empty<object>()), 
			2 => GameApp.Lang.GetText("Shop_Pack_Hot_Tag", Array.Empty<object>()), 
			3 => GameApp.Lang.GetText("Shop_Tag_3", Array.Empty<object>()), 
			_ => string.Empty, 
		};
	}

	public static string GetSkuId(ShopBaseGoods config)
	{
		if (config == null)
		{
			return null;
		}
		string empty = string.Empty;
		if (IAPDebugSetting.UseTestSku)
		{
			return config.TestGPStoreId;
		}
		return config.GPStoreId;
	}

	public static string GetNonConsumptionSkus()
	{
		TbShopConfig tbShopConfig = GameApp.Config.GetTable<ExampleTables>()?.TbShopConfig;
		if (tbShopConfig == null)
		{
			Log.LogError("ShopService", "获取配置表TbShopConfig失败");
			return string.Empty;
		}
		List<string> list = new List<string>();
		foreach (ShopConfig data in tbShopConfig.DataList)
		{
			if (data != null && data.NonConsumption)
			{
				string skuId = GetSkuId(GameApp.Config.GetConfig<ExampleTables, TbShopBaseGoods>((Func<ExampleTables, TbShopBaseGoods>)((ExampleTables tb) => tb.TbShopBaseGoods)).GetOrDefault(data.ID));
				if (!string.IsNullOrEmpty(skuId))
				{
					list.Add(skuId);
				}
			}
		}
		return string.Join(",", list);
	}

	public static ShopConfig GetShopConfig(int id)
	{
		return GameApp.Config.GetConfig<ExampleTables, TbShopConfig>((Func<ExampleTables, TbShopConfig>)((ExampleTables tb) => tb.TbShopConfig)).GetOrDefault(id);
	}

	public static bool ShowBtnShopEntry()
	{
		if (!ShopToggle())
		{
			return false;
		}
		int showEntryConfigValue = GetShowEntryConfigValue();
		if (showEntryConfigValue != 1 && showEntryConfigValue != 2)
		{
			return false;
		}
		ShopModelProxy shopModelProxy = EnsureShopEntryLevelState(showEntryConfigValue);
		ShopModelData data = shopModelProxy.Data;
		switch (showEntryConfigValue)
		{
		case 1:
			if (!HasAnyShopPropUnlocked())
			{
				return false;
			}
			if (!data.ShopEntryConditionMet && HasEmptyShopProp())
			{
				UnlockCurrentLevelEntry(shopModelProxy);
			}
			return data.ShopEntryConditionMet;
		case 2:
			if (!HasAnyShopPropUnlocked())
			{
				return false;
			}
			if (!data.ShopEntryConditionMet && HasEmptyShopProp() && (HasFiveConsecutiveNoReleaseSteps() || HasIdleForShopEntryDuration(data)))
			{
				UnlockCurrentLevelEntry(shopModelProxy);
			}
			return data.ShopEntryConditionMet;
		default:
			return false;
		}
	}

	public static bool ShowPopUIShop()
	{
		if (!ShopToggle())
		{
			return false;
		}
		int showEntryConfigValue = GetShowEntryConfigValue();
		if (showEntryConfigValue != 3 && showEntryConfigValue != 4)
		{
			return false;
		}
		ShopModelData data = EnsureShopEntryLevelState(showEntryConfigValue).Data;
		return showEntryConfigValue switch
		{
			3 => true, 
			4 => data.ShopEntryRewardAdCompletedCount < 2, 
			_ => false, 
		};
	}

	public static bool ShopToggle()
	{
		return FeatureConfig.ShopFunction.ShopToggle == 1;
	}

	public static void RecordGameplayOperation()
	{
		int showEntryConfigValue = GetShowEntryConfigValue();
		if (showEntryConfigValue != 0)
		{
			EnsureShopEntryLevelState(showEntryConfigValue).Data.ShopEntryLastOperationUtcTicks = DateTime.UtcNow.Ticks;
		}
	}

	public static void OnRewardAdFinished(int level)
	{
		int showEntryConfigValue = GetShowEntryConfigValue();
		if (showEntryConfigValue == 4 && GameApp.UI.IsOpen("UIGame"))
		{
			ShopModelProxy shopModelProxy = EnsureShopEntryLevelState(showEntryConfigValue);
			if (shopModelProxy.Data.ShopEntryLevel == level)
			{
				shopModelProxy.Data.ShopEntryRewardAdCompletedCount++;
				NotifyVisibilityChanged();
			}
		}
	}

	public static void OnShopOpened()
	{
		if (GetShowEntryConfigValue() == 4)
		{
			EnsureShopEntryLevelState(4).Data.ShopEntryRewardAdCompletedCount = 0;
			NotifyVisibilityChanged();
		}
	}

	public static int GetShowEntryConfigValue()
	{
		return FeatureConfig.ShopFunction?.ShowEntry ?? 0;
	}

	private static ShopModelProxy EnsureShopEntryLevelState(int showEntryValue)
	{
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		ShopModelProxy shopModelProxy = GameApp.Proxy.Get<ShopModelProxy>();
		shopModelProxy.EnsureShopEntryLevelState((int)GameInfo.CurrentGameMode, levelModelProxy?.Level ?? 0, showEntryValue);
		return shopModelProxy;
	}

	private static void UnlockCurrentLevelEntry(ShopModelProxy proxy)
	{
		if (!proxy.Data.ShopEntryConditionMet)
		{
			proxy.Data.ShopEntryConditionMet = true;
			NotifyVisibilityChanged();
		}
	}

	private static bool HasEmptyShopProp()
	{
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy != null)
		{
			if (propModelProxy.GetPropCount(EPropType.Hammer) != 0)
			{
				return propModelProxy.GetPropCount(EPropType.Skyrocket) == 0;
			}
			return true;
		}
		return false;
	}

	private static bool HasAnyShopPropUnlocked()
	{
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy != null)
		{
			if (!propModelProxy.IsPropUnlock(EPropType.Hammer))
			{
				return propModelProxy.IsPropUnlock(EPropType.Skyrocket);
			}
			return true;
		}
		return false;
	}

	private static bool HasFiveConsecutiveNoReleaseSteps()
	{
		RotateRingOperationScoreFacade rotateRingOperationScoreFacade = SingletonComponent<GameplayManager>.Instance?.GetGameService<RotateRingOperationScoreFacade>();
		if (rotateRingOperationScoreFacade != null && rotateRingOperationScoreFacade.RecentStepCount >= 5)
		{
			return rotateRingOperationScoreFacade.CountRecentReleaseSteps() == 0;
		}
		return false;
	}

	private static bool HasIdleForShopEntryDuration(ShopModelData data)
	{
		long num = ((data.ShopEntryLastOperationUtcTicks > 0) ? data.ShopEntryLastOperationUtcTicks : data.ShopEntryLevelStartedUtcTicks);
		if (num <= 0)
		{
			return false;
		}
		return DateTime.UtcNow.Ticks - num >= 300000000;
	}

	private static void NotifyVisibilityChanged()
	{
		GameApp.Event.Send<ShopEntryVisibilityChangedEvent>(new ShopEntryVisibilityChangedEvent
		{
			IsVisible = ShowBtnShopEntry()
		});
	}
}
