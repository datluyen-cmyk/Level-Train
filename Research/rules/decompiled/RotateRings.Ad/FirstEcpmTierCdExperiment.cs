using System.Collections.Generic;
using RotateRings.Common;
using XingYun.Aot;
using XingYun.Experiment;
using XingYun.Tag;

namespace RotateRings.Ad;

public static class FirstEcpmTierCdExperiment
{
	public const int MaxGroup = 4;

	public const int TierNone = 0;

	public const int TierHigh = 1;

	public const int TierMiddle = 2;

	public const int TierLow = 3;

	private const string Tag = "<Ad>[FirstEcpmTierCd] ";

	private const double HighEcpmThreshold = 20.066;

	private const double MiddleEcpmThreshold = 1.867;

	private const int TierEligibleActiveDay = 1;

	private const int EffectiveMaxActiveDay = 5;

	private static readonly float[][] CdMultipliers = new float[4][]
	{
		new float[3] { 1.2f, 0.8f, 1.2f },
		new float[3] { 1.2f, 0.8f, 0.8f },
		new float[3] { 1.3f, 0.7f, 0.5f },
		new float[3] { 0.8f, 0.8f, 1.2f }
	};

	public static int GetGroup()
	{
		return FeatureConfig.InterstitialFunction?.FirstEcpmTierCdGroup ?? 0;
	}

	public static bool IsActive()
	{
		int num = GetGroup();
		if (num >= 1)
		{
			return num <= 4;
		}
		return false;
	}

	public static float GetCdMultiplier()
	{
		if (!IsActive())
		{
			return 1f;
		}
		int activeDays = GetActiveDays();
		int num = ResolveTier(activeDays);
		if (num == 0 || activeDays > 5)
		{
			return 1f;
		}
		return CdMultipliers[GetGroup() - 1][num - 1];
	}

	private static int ResolveTier(int activeDay)
	{
		FirstEcpmTierRecord orCreate = GameApp.Save.GetOrCreate<FirstEcpmTierRecord>();
		if (orCreate.Resolved)
		{
			return orCreate.Tier;
		}
		TagManager tag = GameApp.Tag;
		List<double> list = ((tag != null) ? tag.First3InterIncomeList : null);
		if (list == null || list.Count == 0 || list[0] <= 0.0)
		{
			return 0;
		}
		orCreate.Tier = ((activeDay <= 1) ? ClassifyTier(list[0]) : 0);
		orCreate.Resolved = true;
		return orCreate.Tier;
	}

	private static int ClassifyTier(double ecpm)
	{
		if (ecpm >= 20.066)
		{
			return 1;
		}
		if (!(ecpm >= 1.867))
		{
			return 3;
		}
		return 2;
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
			return (obj3 == null) ? 1 : obj3.ActiveDays;
		}
	}
}
