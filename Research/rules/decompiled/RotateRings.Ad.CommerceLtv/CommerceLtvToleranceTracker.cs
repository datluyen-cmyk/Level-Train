using System.Collections.Generic;
using XingYun.Tag;

namespace RotateRings.Ad.CommerceLtv;

public static class CommerceLtvToleranceTracker
{
	private static CommerceAdToleranceRecord GetRecord()
	{
		return GameApp.Save.GetOrCreate<CommerceAdToleranceRecord>();
	}

	public static void OnRewardAdFinished()
	{
		if (CommerceLtvExperiment.IsActive())
		{
			GetRecord().CurrentGameRewardShows++;
		}
	}

	public static void OnGameEnded()
	{
		if (CommerceLtvExperiment.IsActive())
		{
			CommerceAdToleranceRecord record = GetRecord();
			record.RewardShowsPerGame.Add(record.CurrentGameRewardShows);
			if (record.RewardShowsPerGame.Count > 7)
			{
				record.RewardShowsPerGame.RemoveAt(0);
			}
			record.CurrentGameRewardShows = 0;
		}
	}

	public static float GetRewardExposureSumX()
	{
		CommerceAdToleranceRecord record = GetRecord();
		int num = record.CurrentGameRewardShows;
		for (int i = 0; i < record.RewardShowsPerGame.Count; i++)
		{
			num += record.RewardShowsPerGame[i];
		}
		return num;
	}

	public static float ComputePR7()
	{
		Last7AdPlayInfoTag last7AdPlayInfo = GameApp.Tag.Last7AdPlayInfo;
		if (last7AdPlayInfo?.last7_inter_ad_results == null || last7AdPlayInfo.last7_inter_ad_results.Count == 0)
		{
			return AdToleranceLookup.PR7(0, 0);
		}
		List<bool> last7_inter_ad_results = last7AdPlayInfo.last7_inter_ad_results;
		int count = last7_inter_ad_results.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (last7_inter_ad_results[i])
			{
				num++;
			}
		}
		return AdToleranceLookup.PR7(num, count);
	}

	public static double GetEBase()
	{
		return InterEcpmBaseline.GetMedian(GameApp.Tag.Last3InterIncomeList);
	}
}
