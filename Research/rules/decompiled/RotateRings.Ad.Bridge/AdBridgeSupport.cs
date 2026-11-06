using System;
using System.Collections.Generic;
using System.Text;
using RotateRings.Prop;
using RotateRings.Tool;
using XingYun.Ad;

namespace RotateRings.Ad.Bridge;

internal static class AdBridgeSupport
{
	public static bool HaveRemoveAd()
	{
		return GameApp.Proxy.Get<PropModelProxy>()?.NoAd ?? false;
	}

	public static bool HasNetworkForAd()
	{
		return NetUtil.IsHaveNet;
	}

	public static bool TryHandleRemoveAds(string adPosition, Action<RRShowAdResult> callback)
	{
		if (!HaveRemoveAd())
		{
			return false;
		}
		callback?.Invoke(new RRShowAdResult(isSuccess: true, "RemoveAds", adPosition));
		return true;
	}

	public static bool TryHandleRewardRemoveAds(string adPosition, bool isFinishedAd, Action<RRShowAdResult> callback)
	{
		if (!HaveRemoveAd())
		{
			return false;
		}
		callback?.Invoke(new RRShowAdResult(isSuccess: true, "RemoveAds", adPosition, isGuarantee: false, isFinishedAd));
		return true;
	}

	public static bool IsInterstitialReady(AdSdkGateway sdkGateway)
	{
		if (HaveRemoveAd())
		{
			return true;
		}
		return sdkGateway.IsInterAdReady;
	}

	public static bool IsRewardReady(AdSdkGateway sdkGateway)
	{
		if (HaveRemoveAd())
		{
			return true;
		}
		return sdkGateway.IsRewardAdReady;
	}

	public static AdConfiguration CreateTrackingConfig(string adPosition)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		RRAdContext rRAdContext = RRAdContext.FromLevelProxy();
		AdConfiguration val = new AdConfiguration(adPosition);
		val.GetCustomData().Clear();
		val.SetCustomData("ad_position", (object)adPosition);
		if (rRAdContext == null)
		{
			return val;
		}
		val.SetCustomData("level", (object)rRAdContext.Level);
		val.SetCustomData("game_mode", (object)rRAdContext.GameMode);
		val.SetCustomData("mode_num", (object)rRAdContext.ModeNum);
		val.SetCustomData("mode_num_all", (object)rRAdContext.ModeNumAll);
		val.SetCustomData("game_score", (object)rRAdContext.GameScore);
		val.SetCustomData("game_time", (object)rRAdContext.GameTime);
		val.SetCustomData("game_time_real", (object)rRAdContext.GameTimeReal);
		val.SetCustomData("step", (object)rRAdContext.Step);
		return val;
	}

	public static void PrintCustomData(AdConfiguration adConfig)
	{
		if (adConfig == null)
		{
			return;
		}
		Dictionary<string, object> customData = adConfig.GetCustomData();
		if (customData == null || customData.Count == 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder(128);
		stringBuilder.Append("<Ad> 自定义参数: ");
		bool flag = true;
		foreach (KeyValuePair<string, object> item in customData)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(item.Key);
			stringBuilder.Append('=');
			stringBuilder.Append(item.Value);
			stringBuilder.AppendLine();
			flag = false;
		}
	}

	public static bool ShouldBlockFrontAd(bool isFrontAd)
	{
		if (!isFrontAd)
		{
			return false;
		}
		if (!GameApp.UI.IsOpen("UILevelWin"))
		{
			return GameApp.UI.IsOpen("UIClaimReward");
		}
		return true;
	}
}
