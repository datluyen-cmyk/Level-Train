using System;
using XingYun.Experiment;
using XingYun.Utility;

namespace Rotaterings.Rank;

public static class RankActivityDataHelper
{
	public static (int activeGamesTotal, int activeDays, int totalGames) GetRecentActivityData(int dayCount)
	{
		if (dayCount <= 0)
		{
			return (activeGamesTotal: 0, activeDays: 0, totalGames: 0);
		}
		DailyRecordsWrapper orCreate;
		try
		{
			orCreate = GameApp.Save.GetOrCreate<DailyRecordsWrapper>();
		}
		catch (Exception)
		{
			return (activeGamesTotal: 0, activeDays: 0, totalGames: 0);
		}
		if (orCreate?.records == null || orCreate.records.Count == 0)
		{
			return (activeGamesTotal: 0, activeDays: 0, totalGames: 0);
		}
		DateTime date = DateTime.UtcNow.Date;
		DateTime dateTime = date.AddDays(-dayCount);
		int num = 0;
		int num2 = 0;
		DateTime dateTime2 = default(DateTime);
		for (int i = 0; i < orCreate.records.Count; i++)
		{
			DailyRecord val = orCreate.records[i];
			if (val != null && !string.IsNullOrEmpty(val.date) && Parser.TryParse(val.date, ref dateTime2, "yyyy-MM-dd") && dateTime2 >= dateTime && dateTime2 < date)
			{
				num += val.gameEndCount;
				if (val.isActive)
				{
					num2++;
				}
			}
		}
		return (activeGamesTotal: Math.Max(1, num), activeDays: num2, totalGames: num);
	}
}
