using RotateRings.Prop;
using XingYun.Experiment;

namespace RotateRings.Ad;

public static class FreeDoubleRewardPolicy
{
	private const string Tag = "<Ad>.FreeDoubleReward";

	public static bool CanClaimWithoutAd()
	{
		int quota = GetQuota();
		if (quota <= 0)
		{
			return false;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy != null && propModelProxy.NoAd)
		{
			return false;
		}
		return GetRecord().UsedFreeCount < quota;
	}

	public static void MarkClaimedFree()
	{
		GetRecord().UsedFreeCount++;
		GameApp.Save.SaveAll();
	}

	public static int GetRemainingFreeCount()
	{
		int num = GetQuota() - GetRecord().UsedFreeCount;
		if (num <= 0)
		{
			return 0;
		}
		return num;
	}

	public static void ResetForDebug()
	{
		GetRecord().UsedFreeCount = 0;
		GameApp.Save.SaveAll();
	}

	private static int GetQuota()
	{
		int num = FunctionDataCenter.Get<InterstitialFunction>()?.FreeDoubleReward ?? 0;
		if (num != 1 && num != 2)
		{
			return 0;
		}
		return num;
	}

	private static FreeDoubleRewardRecord GetRecord()
	{
		return GameApp.Save.GetOrCreate<FreeDoubleRewardRecord>();
	}
}
