using System;
using RotateRings.Model;
using XingYun.Experiment;
using XingYun.Proxy;

namespace RotateRings.DailyScore;

public class DailyScoreProxy : BaseProxy
{
	private const string TAG = "[DailyScoreProxy] ";

	private DailyScoreSettlementSnapshot lastSettlementSnapshot = DailyScoreSettlementSnapshot.Empty;

	public DailyScoreData Data { get; }

	public DailyScoreProxy()
		: base("DailyScoreProxy")
	{
		Data = GameApp.Save.GetOrCreate<DailyScoreData>();
	}

	public DailyScoreSettlementSnapshot GetLastSettlementSnapshot()
	{
		return lastSettlementSnapshot;
	}

	public DailyScoreSettlementSnapshot RecordOnLevelWin(int settlementScore)
	{
		lastSettlementSnapshot = DailyScoreSettlementSnapshot.Empty;
		DailyScoreFunction config = GetConfig();
		if (config.ShowMode <= 0)
		{
			return lastSettlementSnapshot;
		}
		if (!Data.FeatureUnlocked && !MeetsUnlockThreshold(config))
		{
			return lastSettlementSnapshot;
		}
		if (!Data.FeatureUnlocked)
		{
			Data.FeatureUnlocked = true;
		}
		int localDateYyyyMmDd = GetLocalDateYyyyMmDd();
		if (EvaluateAndPersistTodayBest(settlementScore, localDateYyyyMmDd))
		{
			lastSettlementSnapshot = new DailyScoreSettlementSnapshot
			{
				ShouldShowBestUi = true,
				ShowMode = config.ShowMode
			};
		}
		return lastSettlementSnapshot;
	}

	private static DailyScoreFunction GetConfig()
	{
		return FunctionDataCenter.Get<DailyScoreFunction>() ?? new DailyScoreFunction();
	}

	private static bool MeetsUnlockThreshold(DailyScoreFunction cfg)
	{
		if (cfg.OpenLevel > 0)
		{
			return GetCompletedMainLevel() >= cfg.OpenLevel;
		}
		if (cfg.OpenDay > 0)
		{
			return GetActiveDays() >= cfg.OpenDay;
		}
		return false;
	}

	private static int GetCompletedMainLevel()
	{
		int num = GameApp.Proxy.Get<LevelModelProxy>()?.MainLevel ?? 1;
		return Math.Max(0, num - 1);
	}

	private static int GetActiveDays()
	{
		PlayerProxy obj = GameApp.Proxy.Get<PlayerProxy>();
		if (obj == null)
		{
			return 1;
		}
		return obj.ActiveDays;
	}

	private bool EvaluateAndPersistTodayBest(int settlementScore, int today)
	{
		if (Data.BestScoreDate != today)
		{
			Data.BestScoreDate = today;
			Data.TodayBestScore = settlementScore;
			return true;
		}
		if (Data.TodayBestScore <= 0 || settlementScore > Data.TodayBestScore)
		{
			Data.TodayBestScore = settlementScore;
			return true;
		}
		return false;
	}

	private static int GetLocalDateYyyyMmDd()
	{
		DateTime now = DateTime.Now;
		return now.Year * 10000 + now.Month * 100 + now.Day;
	}
}
