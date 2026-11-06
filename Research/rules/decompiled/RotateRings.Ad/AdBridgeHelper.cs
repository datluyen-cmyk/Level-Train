using RotateRings.Model;
using UnityEngine;
using XingYun.Aot;
using XingYun.Core.App;
using XingYun.Core.Singletons;
using XingYun.Experiment;
using XingYun.GM;
using XingYun.Level;
using XingYun.Proxy;

namespace RotateRings.Ad;

public class AdBridgeHelper
{
	public static bool IsEditorActive => false;

	public static string GetEffectiveChannel()
	{
		if (IsEditorActive)
		{
			string text = GMConfig.GetString("RR_AdDebug_GmChannel", string.Empty);
			if (!string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
		}
		return SDKManager.GetChannel();
	}

	public static RuntimePlatform GetEffectivePlatform()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (IsEditorActive && Application.isEditor)
		{
			return (RuntimePlatform)8;
		}
		return Application.platform;
	}

	public static float GetEffectiveTotalValidDurationSeconds()
	{
		if (IsEditorActive && AdDebugSettings.GmRealGameSeconds > 0f)
		{
			return AdDebugSettings.GmRealGameSeconds;
		}
		return GetTotalValidDurationSeconds();
	}

	public static float GetEffectiveColdLaunchValidDurationSeconds()
	{
		if (IsEditorActive && AdDebugSettings.GmRealGameSeconds > 0f)
		{
			return AdDebugSettings.GmRealGameSeconds;
		}
		return AdColdLaunchGameTimeTracker.GetCurrentSeconds();
	}

	public static float GetEffectiveLaunchDuration()
	{
		if (IsEditorActive && AdDebugSettings.GmRealGameSeconds > 0f)
		{
			return AdDebugSettings.GmRealGameSeconds;
		}
		return AppState.SessionDuration;
	}

	public static float GetTotalValidDurationSeconds()
	{
		float num = 0f;
		ProxyManager proxy = GameApp.Proxy;
		GameTrackProxy val = ((proxy != null) ? proxy.Get<GameTrackProxy>() : null);
		if (val != null)
		{
			num += val.GetPassTime(0.ToString());
			num += val.GetPassTime(2.ToString());
		}
		GameTimeTracker instance = MonoSingleton<GameTimeTracker>.Instance;
		if ((Object)(object)instance == (Object)null || !MainStageGameTimeGate.IsMainLevelSessionTracking)
		{
			return num;
		}
		return num + instance.ActualGameTime;
	}

	public static bool IsFirstInstallDay()
	{
		return GetInstallDays() == 1;
	}

	public static int GetInstallDays()
	{
		if (AdDebugSettings.TryGetGmInstallDaysOverride(out var installDays))
		{
			return installDays;
		}
		if (!Define.IsSdkBuild)
		{
			PlayerProxy obj = GameApp.Proxy.Get<PlayerProxy>();
			if (obj == null)
			{
				return 1;
			}
			return obj.InstallDays;
		}
		try
		{
			return SDKManager.GetInstallDays();
		}
		catch
		{
			PlayerProxy obj3 = GameApp.Proxy.Get<PlayerProxy>();
			return (obj3 != null) ? obj3.InstallDays : (-1);
		}
	}

	public static bool IsFirstColdLaunch()
	{
		return GetColdLaunchCount() <= 1;
	}

	public static int GetColdLaunchCount()
	{
		return GameApp.Proxy.Get<PlayerModelProxy>()?.Data?.ColdLaunchCount ?? 1;
	}
}
