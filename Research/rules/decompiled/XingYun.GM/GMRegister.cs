using System;
using RotateRings.Theme;
using UniLog;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using XingYun.Aot;
using XingYun.Utility;

namespace XingYun.GM;

[Preserve]
public static class GMRegister
{
	private const string TAG = "[GM][GMRegister] ";

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	public static void Init()
	{
		RuntimeGM.Register("SDK", "广告/插屏", (Action<Transform>)BuildAdsInterPanel);
		RuntimeGM.Register("SDK", "广告/激励视频", (Action<Transform>)BuildAdsRewardPanel);
		RuntimeGM.Register("SDK", "广告/横幅", (Action<Transform>)BuildAdsBannerPanel);
		RuntimeGM.Register("SDK", "广告/高级", (Action<Transform>)BuildAdsAdvancedPanel);
		RuntimeGM.Register("SDK", "通用/通用", (Action<Transform>)BuildSDKCommonPanel);
		RuntimeGM.Register("SDK", "通用/隐私", (Action<Transform>)BuildSDKPrivacyPanel);
		RuntimeGM.Register("SDK", "系统/信息", (Action<Transform>)BuildSystemPanel);
		RuntimeGM.Register("SDK", "系统/设备", (Action<Transform>)BuildDevicePanel);
		RuntimeGM.Register("SDK", "系统/用户", (Action<Transform>)BuildUserPanel);
		RuntimeGM.Register("SDK", "系统/震动", (Action<Transform>)BuildVibrationPanel);
		RuntimeGM.Register("SDK", "系统/支付", (Action<Transform>)BuildPaymentPanel);
		RuntimeGM.Register("SDK", "系统/推送", (Action<Transform>)BuildPushPanel);
		RuntimeGM.Register("SDK", "分析/上报", (Action<Transform>)BuildReportPanel);
		RuntimeGM.Register("SDK", "分析/高级上报", (Action<Transform>)BuildReportAdvancedPanel);
		RuntimeGM.Register("分析", "分析/Tags", (Action<Transform>)TagGM.BuildTagPanel);
		RuntimeGM.Register("主题", "主题/切换按钮位置", (Action<Transform>)BuildThemeButtonPlacementPanel);
		RuntimeGM.Register("主题", "主题/切换", (Action<Transform>)BuildThemeSwitchPanel);
		RuntimeGM.EnsureInstance();
	}

	private static void BuildAdsInterPanel(Transform root)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		RuntimeGUIBuilder.CreateLabel(root, "--- 插屏广告 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "初始化", (Action)delegate
		{
			SDKManager.InitAd();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "加载插屏", (Action)delegate
		{
			SDKManager.LoadInterAd();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "展示插屏", (Action)delegate
		{
			SDKManager.ShowAd("inter", "gm");
		}, 80f, (TextAnchor)4);
		Text infoText = RuntimeGUIBuilder.CreateLabel(root, "Info: ", 24, (Color?)null, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "CanShowInterAd", (Action)delegate
		{
			infoText.text = $"CanShow: {SDKManager.CanShowInterAd}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Check First ECPM", (Action)delegate
		{
			double num = SDKManager.CallNativeGetInterAdFirstEcpm();
			infoText.text = $"First ECPM: {num}";
		}, 80f, (TextAnchor)4);
	}

	private static void BuildAdsRewardPanel(Transform root)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		RuntimeGUIBuilder.CreateLabel(root, "--- 激励视频 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "加载激励视频", (Action)delegate
		{
			SDKManager.LoadRewardAd();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "展示激励视频", (Action)delegate
		{
			SDKManager.ShowAd("reward", "gm");
		}, 80f, (TextAnchor)4);
		Text infoText = RuntimeGUIBuilder.CreateLabel(root, "Info: ", 24, (Color?)null, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "CanShowRewardAd", (Action)delegate
		{
			infoText.text = $"CanShow: {SDKManager.CanShowRewardAd}";
		}, 80f, (TextAnchor)4);
	}

	private static void BuildAdsBannerPanel(Transform root)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		RuntimeGUIBuilder.CreateLabel(root, "--- 横幅广告 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "初始化横幅", (Action)delegate
		{
			SDKManager.InitBanner();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "展示横幅", (Action)delegate
		{
			SDKManager.ShowBanner();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "隐藏横幅", (Action)delegate
		{
			SDKManager.HideBanner();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "暂停横幅", (Action)delegate
		{
			SDKManager.PauseBanner();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "恢复横幅", (Action)delegate
		{
			SDKManager.ResumeBanner();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "横幅置顶", (Action)delegate
		{
			SDKManager.SetBannerLocation(false);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "横幅置底", (Action)delegate
		{
			SDKManager.SetBannerLocation(true);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateLabel(root, "Banner Space:", 28, (Color?)null, (TextAnchor)3);
		InputField spaceInput = RuntimeGUIBuilder.CreateInput(root, "space_val", "0", 80f);
		RuntimeGUIBuilder.CreateButton(root, "Set Banner Space", (Action)delegate
		{
			float bannerSpace = default(float);
			if (Parser.TryParse(spaceInput.text, ref bannerSpace))
			{
				SDKManager.SetBannerSpace(bannerSpace);
			}
		}, 80f, (TextAnchor)4);
		Text infoText = RuntimeGUIBuilder.CreateLabel(root, "Info: ", 24, (Color?)null, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "Get Revenue Sum", (Action)delegate
		{
			infoText.text = $"Revenue: {SDKManager.GetBannerRevenueSum()}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "CanShowBanner", (Action)delegate
		{
			infoText.text = $"CanShow: {SDKManager.CanShowBanner}";
		}, 80f, (TextAnchor)4);
	}

	private static void BuildAdsAdvancedPanel(Transform root)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		RuntimeGUIBuilder.CreateLabel(root, "--- 广告高级 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Set Ad Muted (True)", (Action)delegate
		{
			SDKManager.SetAdMuted(true);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Set Ad Muted (False)", (Action)delegate
		{
			SDKManager.SetAdMuted(false);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateLabel(root, "Track Income Info:", 28, (Color?)null, (TextAnchor)3);
		InputField incomeInput = RuntimeGUIBuilder.CreateInput(root, "income_info", "{}", 80f);
		RuntimeGUIBuilder.CreateButton(root, "Track Income", (Action)delegate
		{
			SDKManager.TrackAdIncomeRevenue(incomeInput.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateLabel(root, "Ad Type (inter/reward/banner):", 28, (Color?)null, (TextAnchor)3);
		InputField typeInput = RuntimeGUIBuilder.CreateInput(root, "ad_type", "inter", 80f);
		Text infoText = RuntimeGUIBuilder.CreateLabel(root, "Result: ", 24, (Color?)null, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "Get Ready By Type", (Action)delegate
		{
			infoText.text = $"Ready: {SDKManager.GetReadyByAdType(typeInput.text)}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Get Worth By Type", (Action)delegate
		{
			infoText.text = $"Worth: {SDKManager.GetWorthByAdType(typeInput.text)}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Get Last Ad Info", (Action)delegate
		{
			infoText.text = "Last Info: " + SDKManager.GetLastAdInfo();
		}, 80f, (TextAnchor)4);
	}

	private static void BuildSDKCommonPanel(Transform root)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		RuntimeGUIBuilder.CreateLabel(root, "--- SDK 通用 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "SDK Init", (Action)delegate
		{
			SDKManager.Init(Define.IsSdkBuild);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Hide Splash", (Action)delegate
		{
			SDKManager.HideSplash();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Rate Us", (Action)delegate
		{
			SDKManager.RateUs();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Feedback Us", (Action)delegate
		{
			SDKManager.FeedbackUs();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "About Us", (Action)delegate
		{
			SDKManager.AboutUs();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Show Terms", (Action)delegate
		{
			SDKManager.ShowTermsOfService();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Show Privacy", (Action)delegate
		{
			SDKManager.ShowPrivacyPolicy();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Show UMP", (Action)delegate
		{
			SDKManager.CallNativeShowUmp();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Jump Net Setting", (Action)delegate
		{
			SDKManager.CallNativeJumpNetSetting();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Authenticate Player", (Action)delegate
		{
			SDKManager.authenticatePlayer();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateLabel(root, "Store View Info:", 28, (Color?)null, (TextAnchor)3);
		InputField starInput = RuntimeGUIBuilder.CreateInput(root, "stars", "5", 80f);
		InputField commentInput = RuntimeGUIBuilder.CreateInput(root, "comment", "Good", 80f);
		RuntimeGUIBuilder.CreateButton(root, "Upload Store Info (Stars)", (Action)delegate
		{
			if (int.TryParse(starInput.text, out var result))
			{
				SDKManager.UserStoreViewInfo(result, commentInput.text);
			}
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Upload Store Info (Text)", (Action)delegate
		{
			SDKManager.UserStoreViewInfo(commentInput.text);
		}, 80f, (TextAnchor)4);
	}

	private static void BuildSDKPrivacyPanel(Transform root)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		RuntimeGUIBuilder.CreateLabel(root, "--- 隐私与合规 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		Text infoText = RuntimeGUIBuilder.CreateLabel(root, "Info: ", 24, (Color?)null, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "Get Privacy Code", (Action)delegate
		{
			infoText.text = $"Code: {SDKManager.GetPrivacyPreferenceCode()}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Get DoNotSell Status", (Action)delegate
		{
			infoText.text = $"DoNotSell: {SDKManager.GetDoNotSellStatus()}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Has Agreed Privacy", (Action)delegate
		{
			infoText.text = $"Agreed: {SDKManager.HasAgreedPrivacyDialog()}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Notify Privacy Agreed", (Action)delegate
		{
			SDKManager.NotifyPrivacyDialogAgreed();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Set DoNotSell (True)", (Action)delegate
		{
			SDKManager.CallNativeSetDoNotSell(true);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Set DoNotSell (False)", (Action)delegate
		{
			SDKManager.CallNativeSetDoNotSell(false);
		}, 80f, (TextAnchor)4);
	}

	private static void BuildSystemPanel(Transform root)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		Text infoText = RuntimeGUIBuilder.CreateLabel(root, "点击刷新...", 24, (Color?)null, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "刷新信息", (Action)delegate
		{
			string text = "GameId: " + SDKManager.SDKGameId + "\nNetwork: " + SDKManager.NetworkType + "\nCountry: " + SDKManager.GetCountry() + "\nVersion: " + SDKManager.GetAppVersion() + "\nIdentifier: " + Application.identifier + "\n" + $"Screen: {Screen.width}x{Screen.height} ({Screen.dpi} dpi)\n" + $"Audio Busy: {SDKManager.GetIsAudioBusy()}";
			infoText.text = text;
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "退出游戏", (Action)delegate
		{
			SDKManager.ExitGame();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "切到后台", (Action)delegate
		{
			SDKManager.MoveTaskToBack();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateLabel(root, "--- 音频 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Set Sound On", (Action)delegate
		{
			SDKManager.SetGameSoundStatus(true);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Set Sound Off", (Action)delegate
		{
			SDKManager.SetGameSoundStatus(false);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Get Sound Status", (Action)delegate
		{
			Text obj = infoText;
			obj.text += $"\nSound: {SDKManager.GetGameSoundStatus()}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Set Music On", (Action)delegate
		{
			SDKManager.SetGameMusicStatus(true);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Set Music Off", (Action)delegate
		{
			SDKManager.SetGameMusicStatus(false);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Get Music Status", (Action)delegate
		{
			Text obj = infoText;
			obj.text += $"\nMusic: {SDKManager.GetGameMusicStatus()}";
		}, 80f, (TextAnchor)4);
	}

	private static void BuildDevicePanel(Transform root)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		RuntimeGUIBuilder.CreateLabel(root, "--- 设备信息 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		Text infoText = RuntimeGUIBuilder.CreateLabel(root, "Result: ", 24, (Color?)null, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "Native Info", (Action)delegate
		{
			infoText.text = SDKManager.NativeInfo();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "System Font Size", (Action)delegate
		{
			infoText.text = $"Size: {SDKManager.SystemFontSize()}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "UI Style", (Action)delegate
		{
			infoText.text = $"Style: {SDKManager.CurrentUserInterfaceStyle()}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "SDK Int", (Action)delegate
		{
			infoText.text = $"Int: {SDKManager.GetSDKInt()}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Sim Country", (Action)delegate
		{
			infoText.text = "Sim: " + SDKManager.GetSimCountry();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Network Country", (Action)delegate
		{
			infoText.text = "Net: " + SDKManager.GetNetworkCountry();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Is Pure", (Action)delegate
		{
			infoText.text = $"Pure: {SDKManager.IsPure()}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Channel", (Action)delegate
		{
			infoText.text = "Channel: " + SDKManager.GetChannel();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "AB Traffic", (Action)delegate
		{
			infoText.text = "AB: " + SDKManager.GetNativeAbTestTrafficResult();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Is Debug", (Action)delegate
		{
			infoText.text = $"Debug: {SDKManager.IsNativeDebug()}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Is UnderR", (Action)delegate
		{
			infoText.text = $"UnderR: {SDKManager.IsNativeUnderR()}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Expt State", (Action)delegate
		{
			infoText.text = "Expt: " + SDKManager.GetExptStateJson();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "App Name", (Action)delegate
		{
			infoText.text = "Name: " + SDKManager.GetAppName();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Physics Screen", (Action)delegate
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			PhysicsScreenSize physicsScreenSize = SDKManager.GetPhysicsScreenSize();
			infoText.text = $"Trust:{physicsScreenSize.IsTrust}, W:{physicsScreenSize.PhysicsWidth}, H:{physicsScreenSize.PhysicsHeight}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateLabel(root, "Set Inactive Icon:", 28, (Color?)null, (TextAnchor)3);
		InputField iconInput = RuntimeGUIBuilder.CreateInput(root, "icon_scheme", "broken", 80f);
		RuntimeGUIBuilder.CreateButton(root, "Set Icon Scheme", (Action)delegate
		{
			infoText.text = $"Set: {SDKManager.SetInactiveIconScheme(iconInput.text)}";
		}, 80f, (TextAnchor)4);
	}

	private static void BuildUserPanel(Transform root)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		RuntimeGUIBuilder.CreateLabel(root, "--- 用户属性 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		InputField inputVal = RuntimeGUIBuilder.CreateInput(root, "user_val_int", "0", 80f);
		InputField inputStr = RuntimeGUIBuilder.CreateInput(root, "user_val_str", "", 80f);
		RuntimeGUIBuilder.CreateButton(root, "Set User Tier", (Action)delegate
		{
			if (int.TryParse(inputVal.text, out var result))
			{
				SDKManager.SetUserTier(result);
			}
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Set Game Version", (Action)delegate
		{
			SDKManager.SetGameVersion(inputStr.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Set Ad Num", (Action)delegate
		{
			float adNum = default(float);
			if (Parser.TryParse(inputVal.text, ref adNum))
			{
				SDKManager.SetAdNum(adNum);
			}
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Set Game Num", (Action)delegate
		{
			float gameNum = default(float);
			if (Parser.TryParse(inputVal.text, ref gameNum))
			{
				SDKManager.SetGameNum(gameNum);
			}
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Set ECPM", (Action)delegate
		{
			float ecpm = default(float);
			if (Parser.TryParse(inputVal.text, ref ecpm))
			{
				SDKManager.SetEcpm(ecpm);
			}
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Set A Day", (Action)delegate
		{
			if (int.TryParse(inputVal.text, out var result))
			{
				SDKManager.SetADay(result);
			}
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Set Not New User", (Action)delegate
		{
			SDKManager.SetNotNewUser();
		}, 80f, (TextAnchor)4);
		Text infoText = RuntimeGUIBuilder.CreateLabel(root, "Info: ", 24, (Color?)null, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "Get Active Days", (Action)delegate
		{
			infoText.text = $"Active: {SDKManager.GetActiveDays()}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Get Install Days", (Action)delegate
		{
			infoText.text = $"Install: {SDKManager.GetInstallDays()}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Get Distinct Id", (Action)delegate
		{
			infoText.text = "ID: " + SDKManager.GetDistinctId();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Call Conversion Data", (Action)delegate
		{
			infoText.text = "Conv: " + SDKManager.CallNativeUserConversionData();
		}, 80f, (TextAnchor)4);
	}

	private static void BuildPaymentPanel(Transform root)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		RuntimeGUIBuilder.CreateLabel(root, "--- 支付 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Create Client Order Id", (Action)delegate
		{
			SDKManager.CreateClientOrderId();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Check Uncompleted Orders", (Action)delegate
		{
			SDKManager.CheckUncompletedOrders();
		}, 80f, (TextAnchor)4);
		InputField skuInput = RuntimeGUIBuilder.CreateInput(root, "sku_ids", "id1,id2", 80f);
		RuntimeGUIBuilder.CreateButton(root, "Get Sku Details (Type 0)", (Action)delegate
		{
			SDKManager.GetSkuDetails(skuInput.text, 0);
		}, 80f, (TextAnchor)4);
		InputField payInput = RuntimeGUIBuilder.CreateInput(root, "pay_param", "{}", 80f);
		RuntimeGUIBuilder.CreateButton(root, "Pay", (Action)delegate
		{
			SDKManager.Pay(payInput.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Upload Order Delivered", (Action)delegate
		{
			SDKManager.UploadOrderDelivered(payInput.text);
		}, 80f, (TextAnchor)4);
		InputField marketInput1 = RuntimeGUIBuilder.CreateInput(root, "market_name", "name", 80f);
		InputField marketInput2 = RuntimeGUIBuilder.CreateInput(root, "market_json", "{}", 80f);
		RuntimeGUIBuilder.CreateButton(root, "Call Native Market Data", (Action)delegate
		{
			SDKManager.CallNativeMarketData(marketInput1.text, marketInput2.text);
		}, 80f, (TextAnchor)4);
	}

	private static void BuildPushPanel(Transform root)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		RuntimeGUIBuilder.CreateLabel(root, "--- 推送 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		InputField input = RuntimeGUIBuilder.CreateInput(root, "push_param", "", 80f);
		Text infoText = RuntimeGUIBuilder.CreateLabel(root, "Result: ", 24, (Color?)null, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "Get Game Push Info", (Action)delegate
		{
			infoText.text = "Info: " + SDKManager.GetGamePushInfo(input.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Send Game Push", (Action)delegate
		{
			SDKManager.SendGamePush(input.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Remove Game Push", (Action)delegate
		{
			SDKManager.SendGamePushRemoved();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Open App Opeway", (Action)delegate
		{
			infoText.text = "Opeway: " + SDKManager.CallNativeOpenAppOpeway();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Is Match Wf Name", (Action)delegate
		{
			WfConfigSetting val = SDKManager.IsMatchWfName();
			infoText.text = $"Matched: {val.NameMatched}, Name: {val.WfName}, Delay: {val.DelayTime}";
		}, 80f, (TextAnchor)4);
	}

	private static void BuildReportPanel(Transform root)
	{
		RuntimeGUIBuilder.CreateLabel(root, "事件名:", 28, (Color?)null, (TextAnchor)3);
		InputField inputKey = RuntimeGUIBuilder.CreateInput(root, "event_name", "test_event", 80f);
		RuntimeGUIBuilder.CreateLabel(root, "事件值:", 28, (Color?)null, (TextAnchor)3);
		InputField inputVal = RuntimeGUIBuilder.CreateInput(root, "event_value", "1", 80f);
		RuntimeGUIBuilder.CreateButton(root, "用户上报 (String)", (Action)delegate
		{
			SDKManager.ReportUser(inputKey.text, inputVal.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "用户上报 (Int)", (Action)delegate
		{
			if (int.TryParse(inputVal.text, out var result))
			{
				SDKManager.ReportUserInt(inputKey.text, result);
			}
			else
			{
				Log.LogError("[GM][GMRegister] ", "Value is not int");
			}
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "用户上报 (Json)", (Action)delegate
		{
			SDKManager.ReportUserJson(inputKey.text, inputVal.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "用户上报 (Append)", (Action)delegate
		{
			SDKManager.ReportUserAppend(inputKey.text, inputVal.text);
		}, 80f, (TextAnchor)4);
	}

	private static void BuildReportAdvancedPanel(Transform root)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		RuntimeGUIBuilder.CreateLabel(root, "--- 高级上报 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		InputField inputKey = RuntimeGUIBuilder.CreateInput(root, "adv_key", "key", 80f);
		InputField inputVal = RuntimeGUIBuilder.CreateInput(root, "adv_val", "{}", 80f);
		RuntimeGUIBuilder.CreateButton(root, "Report Json Str", (Action)delegate
		{
			SDKManager.ReportJsonStr(inputKey.text, inputVal.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Report Json Str Flush", (Action)delegate
		{
			SDKManager.ReportJsonStrFlush(inputKey.text, inputVal.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Report Json Str Common", (Action)delegate
		{
			SDKManager.ReportJsonStrCommon(inputVal.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Report Common Unset", (Action)delegate
		{
			SDKManager.ReportJsonStrCommonUnSetProperty(inputKey.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Report Common Clear", (Action)delegate
		{
			SDKManager.ReportJsonStrCommonClear();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateLabel(root, "--- Data Warehouse ---", 24, (Color?)Color.white, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "Report Common to Warehouse", (Action)delegate
		{
			SDKManager.ReportCommonJsonStrToDataWareHouse(inputVal.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Report Event to Warehouse", (Action)delegate
		{
			SDKManager.ReportEventJsonStrToDataWareHouse(inputKey.text, inputVal.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Report User to Warehouse", (Action)delegate
		{
			SDKManager.ReportUserJsonStrToDataWareHouse(inputKey.text, inputVal.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Upload Init", (Action)delegate
		{
			SDKManager.UploadInit();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateLabel(root, "--- Bugsnag ---", 24, (Color?)Color.white, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "Set Context", (Action)delegate
		{
			SDKManager.SetBugSnagContext(inputVal.text);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Report Bugsnag Json", (Action)delegate
		{
			SDKManager.ReportBugSnagJsonStr(inputKey.text, inputVal.text);
		}, 80f, (TextAnchor)4);
	}

	private static void BuildVibrationPanel(Transform root)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		RuntimeGUIBuilder.CreateLabel(root, "--- 预定义 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		for (int i = 1; i <= 5; i++)
		{
			int type = i;
			RuntimeGUIBuilder.CreateButton(root, $"震动类型 {type}", (Action)delegate
			{
				SDKManager.PredefineVibrateByType(type);
			}, 80f, (TextAnchor)4);
		}
		RuntimeGUIBuilder.CreateLabel(root, "--- 自定义 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Play Haptic (iOS)", (Action)delegate
		{
			SDKManager.PlayHaptic(1u);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "重复震动 (Config)", (Action)delegate
		{
			SDKManager.VibrateRepeat("");
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "取消震动", (Action)delegate
		{
			SDKManager.VibrateCancel();
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateLabel(root, "--- 持续震动 (iOS) ---", 24, (Color?)Color.white, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "Start Continuous", (Action)delegate
		{
			SDKManager.VibrateHapticContinuous(0.8f, 0.5f, 2f);
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Stop Continuous", (Action)delegate
		{
			SDKManager.StopHapticContinuous();
		}, 80f, (TextAnchor)4);
		Text infoText = RuntimeGUIBuilder.CreateLabel(root, "Info: ", 24, (Color?)null, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "Is Android Enable", (Action)delegate
		{
			infoText.text = $"Android Enable: {SDKManager.IsAndroidVibrateEnable()}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "Is Support Predefined", (Action)delegate
		{
			infoText.text = $"Support: {SDKManager.IsSupportPredefinedType()}";
		}, 80f, (TextAnchor)4);
	}

	private static void BuildThemeButtonPlacementPanel(Transform root)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		RuntimeGUIBuilder.CreateLabel(root, "--- 切换按钮位置 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		Text infoText = RuntimeGUIBuilder.CreateLabel(root, $"当前: {ThemeButtonConfig.CurrentPlacement}", 24, (Color?)null, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "None (不显示)", (Action)delegate
		{
			ThemeButtonConfig.SetPlacement(ThemeButtonPlacement.None);
			infoText.text = $"当前: {ThemeButtonConfig.CurrentPlacement}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "InGameHUD (局内顶部)", (Action)delegate
		{
			ThemeButtonConfig.SetPlacement(ThemeButtonPlacement.InGameHUD);
			infoText.text = $"当前: {ThemeButtonConfig.CurrentPlacement}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "InSettingPanel (设置面板内)", (Action)delegate
		{
			ThemeButtonConfig.SetPlacement(ThemeButtonPlacement.InSettingPanel);
			infoText.text = $"当前: {ThemeButtonConfig.CurrentPlacement}";
		}, 80f, (TextAnchor)4);
	}

	private static void BuildThemeSwitchPanel(Transform root)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		RuntimeGUIBuilder.CreateLabel(root, "--- 主题切换 ---", 32, (Color?)Color.yellow, (TextAnchor)4);
		Text infoText = RuntimeGUIBuilder.CreateLabel(root, $"当前主题: {ThemeService.CurrentThemeId}", 24, (Color?)null, (TextAnchor)3);
		RuntimeGUIBuilder.CreateButton(root, "切换到 Day", (Action)delegate
		{
			ThemeService.SetTheme(ThemeId.Day);
			infoText.text = $"当前主题: {ThemeService.CurrentThemeId}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "切换到 Night", (Action)delegate
		{
			ThemeService.SetTheme(ThemeId.Night);
			infoText.text = $"当前主题: {ThemeService.CurrentThemeId}";
		}, 80f, (TextAnchor)4);
		RuntimeGUIBuilder.CreateButton(root, "CycleNext (下一个)", (Action)delegate
		{
			ThemeService.CycleNextTheme();
			infoText.text = $"当前主题: {ThemeService.CurrentThemeId}";
		}, 80f, (TextAnchor)4);
	}
}
