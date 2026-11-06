using XingYun.Aot;
using XingYun.Experiment;

namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class FrontAdVisibilityPolicy : InterstitialPolicyBase
{
	private const string Tag = "<Ad>.FrontAdVisibilityPolicy";

	public override string Name => "FrontAdVisibility";

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		if (!context.IsFrontAd)
		{
			return InterAdCheckResult.Allow;
		}
		int configuredStrategy = GetConfiguredStrategy();
		return Evaluate(configuredStrategy);
	}

	public int GetConfiguredStrategy()
	{
		return FunctionDataCenter.Get<InterstitialFunction>()?.NotShowFrontAdStrategy ?? 0;
	}

	public InterAdCheckResult Evaluate(int strategyValue)
	{
		switch (strategyValue)
		{
		case 1:
		case 2:
		{
			int installDays2 = AdBridgeHelper.GetInstallDays();
			InterAdCheckResult result3 = ((installDays2 == 1) ? InterAdCheckResult.Block("安装首日不展示前插屏(底板)") : InterAdCheckResult.Allow);
			LogDecision(strategyValue, "installDays", installDays2, result3);
			return result3;
		}
		case 3:
		case 4:
		{
			int installDays = AdBridgeHelper.GetInstallDays();
			InterAdCheckResult result2 = ((installDays <= 3) ? InterAdCheckResult.Block("安装前x天不展示前插屏策略拦截") : InterAdCheckResult.Allow);
			LogDecision(strategyValue, "installDays", installDays, result2);
			return result2;
		}
		case 5:
		case 6:
		{
			int activeDays = GetActiveDays();
			InterAdCheckResult result = ((activeDays <= 3) ? InterAdCheckResult.Block("安装前x天不展示前插屏策略拦截") : InterAdCheckResult.Allow);
			LogDecision(strategyValue, "activeDays", activeDays, result);
			return result;
		}
		default:
			return InterAdCheckResult.Allow;
		}
	}

	private static void LogDecision(int strategyValue, string dayField, int dayValue, InterAdCheckResult result)
	{
	}

	private static int GetActiveDays()
	{
		if (!Define.IsSdkBuild)
		{
			PlayerProxy obj = GameApp.Proxy.Get<PlayerProxy>();
			if (obj == null)
			{
				return 1;
			}
			return obj.ActiveDays;
		}
		try
		{
			return SDKManager.GetActiveDays();
		}
		catch
		{
			PlayerProxy obj3 = GameApp.Proxy.Get<PlayerProxy>();
			return (obj3 != null) ? obj3.ActiveDays : (-1);
		}
	}
}
