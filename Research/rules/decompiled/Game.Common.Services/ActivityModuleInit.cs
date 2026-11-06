using System.Collections.Generic;
using RotateRings.DailyFirstWin;
using RotateRings.GameWinIQ;
using RotateRings.Services;
using RotateRings.UI.DailyFirstWin;
using RotateRings.UI.Rating;
using Rotaterings.Rank;

namespace Game.Common.Services;

public static class ActivityModuleInit
{
	private static bool _initialized;

	private static bool hasBuiltHomePopupPipeline;

	private static readonly List<IHomePopupStep> homePopupStepsBuffer = new List<IHomePopupStep>();

	public static void EnsureInitialized()
	{
		if (!_initialized)
		{
			_initialized = true;
			DailyFirstWinModule.Initialize(new DailyFirstWinHostImpl(), new DailyFirstWinPresentationHostImpl());
			RankCooldownExitRecorder.EnsureInitialized();
			AudioService.BeginBatchExperiments();
			try
			{
				DailyFirstWinModule.CompleteStartup(GameApp.Proxy.Get<DailyFirstWinProxy>());
				GameWinIQService.Instance.SyncAudioExperiment();
			}
			finally
			{
				AudioService.CommitExperiments();
			}
			SettlementPipeline.SetTerminalAction(delegate
			{
				LevelVictoryService.DoGradeSettlement();
			});
			PostSettlementPipeline.SetTerminalAction(delegate
			{
			});
			HomePopupPipeline.SetTerminalAction(delegate
			{
			});
			LevelStartPipeline.SetTerminalAction(delegate
			{
			});
			PostSettlementPipeline.Register(new RatingPostSettlementStep());
			PostSettlementPipeline.Register(new RewardedLuckyDrawPostSettlementStep());
			SettlementPipeline.Register(new CelebratePreAdStep());
			SettlementPipeline.Register(new PostAdSettlementStep());
			SettlementPipeline.Register(new CelebratePostAdStep());
			SettlementPipeline.Register(new DailyFirstWinSettlementStep());
			SettlementPipeline.Register(new RankSettlementStep());
			PostSettlementPipeline.Register(new CloudSaveApplySettlementStep());
			LevelStartPreEnterPipeline.Register(new ActivityOpenLevelStartStep());
			LevelStartPreEnterPipeline.Register(new DifficultyHardLevelStep());
			LevelStartPipeline.Register(new LevelEntryBannerStep());
			LevelStartPipeline.Register(new NewElementIntroLevelStartStep());
			LevelStartPipeline.Register(new LevelStepLimitExhaustedPopStep());
		}
	}

	public static bool AnyHomePopupWouldRun(bool isReturningFromLevel)
	{
		PrepareHomePopupEvaluation(isReturningFromLevel, homePopupStepsBuffer);
		for (int i = 0; i < homePopupStepsBuffer.Count; i++)
		{
			if (homePopupStepsBuffer[i].ShouldRun())
			{
				return true;
			}
		}
		return false;
	}

	public static void BuildAndExecuteHomePopups(bool isReturningFromLevel)
	{
		PrepareHomePopupEvaluation(isReturningFromLevel, homePopupStepsBuffer);
		HomePopupPipeline.Reset();
		for (int i = 0; i < homePopupStepsBuffer.Count; i++)
		{
			HomePopupPipeline.Register(homePopupStepsBuffer[i]);
		}
		hasBuiltHomePopupPipeline = true;
		HomePopupPipeline.BuildAndExecute();
	}

	private static void PrepareHomePopupEvaluation(bool isReturningFromLevel, List<IHomePopupStep> steps)
	{
		DailyFirstWinService.Instance.RefreshOnHome();
		steps.Clear();
		CollectHomePopupSteps(isReturningFromLevel, steps);
	}

	private static void CollectHomePopupSteps(bool isReturningFromLevel, List<IHomePopupStep> steps)
	{
		steps.Add(new CloudSaveLoginApplyHomePopupStep());
		steps.Add(new ReissueRewardHomePopupStep());
		steps.Add(new RankClaimHomePopupStep());
		steps.Add(new DailyFirstWinHomePopupStep());
		bool num = RankActivityEntry.Service?.IsPeriodSettlePending ?? false;
		bool ignoreOpenScoreRequirement = isReturningFromLevel || (!hasBuiltHomePopupPipeline && RankActivityEntry.HasCooldownOfflineDurationReached());
		if (!num)
		{
			steps.Add(new RankActivityOpenHomePopupStep(ignoreOpenScoreRequirement));
		}
	}
}
