using System;
using Example.Cfg;
using Game.UI.RankListPopup;
using Rotaterings.SO;
using UniLog;
using UnityEngine;
using XingYun.UI.Core;
using XingYun.Utility;

namespace Rotaterings.Rank;

public static class RankActivityEntry
{
	private const string Tag = "Rank";

	private static RankActivityService service;

	public static RankActivityService Service
	{
		get
		{
			if (service == null)
			{
				service = CreateService();
			}
			return service;
		}
	}

	public static void InvalidateService()
	{
		service = null;
	}

	public static bool IsFeatureOpen()
	{
		return true;
	}

	public static string GetRankListUIPath()
	{
		if (!IsHalloweenRankTheme())
		{
			return "UIRankList";
		}
		return "UIRankListHalloween";
	}

	public static string GetRankListPopupUIPath()
	{
		if (!IsHalloweenRankTheme())
		{
			return "UIRankListPopup";
		}
		return "UIRankListPopupHalloween";
	}

	public static string GetRankListTipUIPath()
	{
		if (!IsHalloweenRankTheme())
		{
			return "UIRankListTip";
		}
		return "UIRankListTipHalloween";
	}

	private static bool IsHalloweenRankTheme()
	{
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		if (rankModelProxy == null)
		{
			return false;
		}
		return rankModelProxy.Data?.IsHalloweenTheme == true;
	}

	public static void OpenRankList(UILayer layer = (UILayer)2, object param = null, Action closeCallback = null)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		string rankListUIPath = GetRankListUIPath();
		ClearOtherRankListCache(rankListUIPath);
		GameApp.UI.Open(rankListUIPath, layer, param, closeCallback);
	}

	private static void ClearOtherRankListCache(string currentUIPath)
	{
		string text = ((currentUIPath == "UIRankListHalloween") ? "UIRankList" : "UIRankListHalloween");
		GameApp.UI.ClearCache(text);
	}

	public static bool EnsureBuildActivityData()
	{
		return Service?.EnsureBuildActivityData() ?? false;
	}

	public static void ForceRebuildActivityData()
	{
		Service?.BuildActivityData();
	}

	public static bool HasCooldownOfflineDurationReached()
	{
		return Service?.HasCooldownOfflineDurationReached() ?? false;
	}

	public static bool CanShowActivityOpenPopup(bool ignoreOpenScoreRequirement = false)
	{
		if (!IsFeatureOpen())
		{
			return false;
		}
		RankModelData rankModelData = GameApp.Proxy.Get<RankModelProxy>()?.Data;
		if (rankModelData == null)
		{
			return false;
		}
		RankActivityService rankActivityService = Service;
		if (rankActivityService == null || !rankActivityService.CanOpenRank())
		{
			return false;
		}
		switch (rankActivityService.GetActivityState(ignoreOpenScoreRequirement))
		{
		case EnsureResult.Ongoing:
			return !rankModelData.HasShownActivityOpenPopup;
		default:
			return false;
		case EnsureResult.None:
		case EnsureResult.OpenedNew:
			return rankActivityService.CanOpenNextActivity(Time.GetCurrentTimestamp(), ignoreOpenScoreRequirement);
		}
	}

	public static bool IsAnyRankListPopupOpen()
	{
		if (!GameApp.UI.IsOpen("UIRankListPopup"))
		{
			return GameApp.UI.IsOpen("UIRankListPopupHalloween");
		}
		return true;
	}

	public static void CloseAnyRankListPopup()
	{
		CloseRankListPopupIfOpen("UIRankListPopup");
		CloseRankListPopupIfOpen("UIRankListPopupHalloween");
	}

	private static void CloseRankListPopupIfOpen(string popupUIPath)
	{
		if (GameApp.UI.IsOpen(popupUIPath))
		{
			GameApp.UI.CloseView(popupUIPath, (object)null, false);
		}
	}

	public static bool TryShowActivityOpenPopup(Action onClosed = null, bool enterLevelOnSubmit = true, UILayer layer = (UILayer)2, bool ignoreOpenScoreRequirement = false)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (IsAnyRankListPopupOpen())
		{
			return false;
		}
		if (!CanShowActivityOpenPopup(ignoreOpenScoreRequirement))
		{
			return false;
		}
		RankActivityService rankActivityService = Service;
		if (rankActivityService == null)
		{
			return false;
		}
		rankActivityService.EnsureBuildActivityData(ignoreOpenScoreRequirement);
		if (!rankActivityService.IsActivityOngoing(Time.GetCurrentTimestamp()))
		{
			return false;
		}
		RankModelData rankModelData = GameApp.Proxy.Get<RankModelProxy>()?.Data;
		if (rankModelData != null)
		{
			rankModelData.HasShownActivityOpenPopup = true;
		}
		string rankListPopupUIPath = GetRankListPopupUIPath();
		GameApp.UI.Open(rankListPopupUIPath, layer, (object)new UIRankListPopupParam
		{
			OnClosed = onClosed,
			EnterLevelOnSubmit = enterLevelOnSubmit
		}, (Action)null);
		return true;
	}

	private static RankActivityService CreateService()
	{
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		ExampleTables table = GameApp.Config.GetTable<ExampleTables>();
		RankRobotScoreConfig instance = RankRobotScoreConfig.Instance;
		if (rankModelProxy?.Data == null || table == null || (Object)(object)instance == (Object)null)
		{
			Log.LogError("Rank", "[RankActivityEntry] 服务初始化失败");
			return null;
		}
		return new RankActivityService(rankModelProxy.Data, table, instance);
	}
}
