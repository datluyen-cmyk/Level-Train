using RotateRings.Ad;
using RotateRings.Model;
using XingYun.Aot;
using XingYun.Experiment;
using XingYun.Feature;

namespace Game.Feature.Ad;

public class RotateRingsInterstitialAdFeature : BaseFeature, IRotateRingsInterstitialAdGuardQuery
{
	private int sessionLevelCount;

	private bool currentLevelFailed;

	private bool failedAtLevelComplete;

	public override string Name => "解环插屏广告策略";

	public override int Priority => 500;

	public override bool CheckEnabled()
	{
		return true;
	}

	public bool ShouldSkipForFirstDay()
	{
		if (GetInstallDays() == 1)
		{
			return true;
		}
		return false;
	}

	private static int GetInstallDays()
	{
		try
		{
			return SDKManager.GetInstallDays();
		}
		catch
		{
			return -1;
		}
	}

	private static int GetActiveDays()
	{
		if (!Define.IsSdkBuild)
		{
			return GameApp.Proxy.Get<PlayerProxy>().ActiveDays;
		}
		try
		{
			return SDKManager.GetActiveDays();
		}
		catch
		{
			return GameApp.Proxy.Get<PlayerProxy>().ActiveDays;
		}
	}

	public bool ShouldSkipForColdStart(bool isAfterShow)
	{
		int num = 3;
		int activeDays = GetActiveDays();
		if (activeDays < 1 || activeDays > num)
		{
			return false;
		}
		if ((isAfterShow ? (sessionLevelCount - 1) : sessionLevelCount) == 0)
		{
			return true;
		}
		return false;
	}

	public void OnColdStartSession()
	{
		sessionLevelCount = 0;
		currentLevelFailed = false;
		failedAtLevelComplete = false;
	}

	public bool ShouldSkipForLevelThreshold(bool isAfterShow)
	{
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (levelModelProxy == null)
		{
			return true;
		}
		int interstitialOpenLv = AdService.GetInterstitialOpenLv();
		if ((isAfterShow ? (levelModelProxy.MainLevel - 1) : levelModelProxy.MainLevel) < interstitialOpenLv)
		{
			return true;
		}
		return false;
	}

	public bool ShouldSkipForFailRetry(bool isAfterShow)
	{
		if (isAfterShow ? failedAtLevelComplete : currentLevelFailed)
		{
			return true;
		}
		return false;
	}

	public void NotifyLevelCompleted()
	{
		sessionLevelCount++;
		failedAtLevelComplete = currentLevelFailed;
		currentLevelFailed = false;
	}

	public void NotifyLevelFailed()
	{
		currentLevelFailed = true;
	}

	public void TryRecordFirstInterstitialLevelAfterBackgroundEscape()
	{
		InterstitialBackgroundEscapeTracker.CheckFirstInterstitialLevelAfterEscape();
	}
}
