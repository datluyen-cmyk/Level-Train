using System;
using Game.Common.Services;
using Game.Feature.Ad;
using Hot.Model;
using RotateRings.Ad;
using RotateRings.Chest;
using RotateRings.Common;
using RotateRings.DailyFirstWin;
using RotateRings.DailyScore;
using RotateRings.GameWinIQ;
using RotateRings.Model;
using RotateRings.Prop;
using RotateRings.RewardedLuckyDraw;
using RotateRings.SpecialLevel;
using RotateRings.UI.Game;
using RotateRings.UI.LevelEnd;
using RotateRings.UI.RewardedLuckyDraw;
using Rotaterings.Rank;
using UniLog;
using UnityEngine;
using XingYun.Core.Singletons;
using XingYun.Experiment;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.Services;

public static class LevelVictoryService
{
	public struct VictoryProgressSnapshot
	{
		public int CompletedLevel;

		public int CompletedScore;

		public int CompletedPropUses;

		public bool IsMainToEndlessTransition;

		public GameWinIQSettlementSnapshot GameWinIqSnapshot;
	}

	private const string Tag = "[LevelVictoryService]";

	private static UILevelWinParam _pendingParam;

	public static VictoryProgressSnapshot PersistVictoryProgress()
	{
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		GameApp.Proxy.Get<PlayerProxy>().RecordLevelEnd();
		int level = levelModelProxy.Level;
		int currentScore = levelModelProxy.GetCurrentScore();
		int tryLevelNum = levelModelProxy.TryLevelNum;
		int completedPropUses = levelModelProxy.GetPropUseCount(EPropType.Hammer) + levelModelProxy.GetPropUseCount(EPropType.Skyrocket);
		GameMode currentGameMode = GameInfo.CurrentGameMode;
		bool num = SpecialLevelModule.IsSpecialLevelMode();
		levelModelProxy.OnLevelWin();
		if (!num && levelModelProxy.IsFirstEnterCurrentLevel())
		{
			levelModelProxy.AddWiningStreak();
		}
		if (!num)
		{
			levelModelProxy.AddLevel();
			levelModelProxy.AddGlobalLevelCount();
		}
		GameMode currentGameMode2 = GameInfo.CurrentGameMode;
		if (!num && (currentGameMode2 == GameMode.Main || currentGameMode2 == GameMode.Endless))
		{
			SpecialLevelModule.RefreshNextLevelGameModeAfterVictory(levelModelProxy.Level);
		}
		RewardInterExemptExperiment.OnLevelCompleted();
		PropWinInterExemptExperiment.OnLevelCompleted();
		bool isMainToEndlessTransition = currentGameMode == GameMode.Main && currentGameMode2 == GameMode.Endless;
		if (num)
		{
			SpecialLevelModule.MarkCompletedAndRestoreBaseMode();
		}
		RankActivityEntry.Service?.HandleLevelVictory(currentGameMode, level);
		Singleton<AdFeatureManager>.Instance?.GetRotateRingsInterstitialAdFeature()?.NotifyLevelCompleted();
		ActivityModuleInit.EnsureInitialized();
		DailyFirstWinService.Instance.RecordLevelVictory(isMainMode: true, level);
		ChestProxy chestProxy = GameApp.Proxy.Get<ChestProxy>();
		if (chestProxy != null)
		{
			chestProxy.GetProgressLitCount();
			chestProxy.OnLevelWin(levelModelProxy.MainLevel);
		}
		DailyScoreService.Instance.RecordOnLevelWin(currentScore);
		GameWinIQSettlementSnapshot gameWinIqSnapshot = GameWinIQService.Instance.RecordOnLevelWin(level, tryLevelNum);
		GameApp.Proxy.Get<RewardedLuckyDrawProxy>()?.RecordVictory();
		return new VictoryProgressSnapshot
		{
			CompletedLevel = level,
			CompletedScore = currentScore,
			CompletedPropUses = completedPropUses,
			IsMainToEndlessTransition = isMainToEndlessTransition,
			GameWinIqSnapshot = gameWinIqSnapshot
		};
	}

	public static void SetPendingParam(UILevelWinParam param)
	{
		_pendingParam = param;
	}

	public static void AutoClaimPendingChest()
	{
		ChestProxy chestProxy = GameApp.Proxy.Get<ChestProxy>();
		if (chestProxy != null && chestProxy.HasPendingClaimable())
		{
			ReissueRewardService.ReserveClaimableChestReward(chestProxy);
		}
	}

	public static void ContinueToSettlement(int completedLevel, int score, bool isMainToEndlessTransition, int completedPropUses, GameWinIQSettlementSnapshot gameWinIqSnapshot = default(GameWinIQSettlementSnapshot))
	{
		using (new ProfilerScope("LevelVictoryService.ContinueToSettlement"))
		{
			if (GameApp.UI.IsOpen("UIPropAward"))
			{
				GameApp.UI.CloseView("UIPropAward", (object)null, false);
			}
			LevelEndBgmVolumeService.ApplyOnVictory();
			AdService.ClearPendingFrontAd("VictorySettlement");
			TryPlayUIGameHideAnimationOnVictory();
			ActivityModuleInit.EnsureInitialized();
			completedLevel = Mathf.Max(0, completedLevel);
			if (_pendingParam == null)
			{
				_pendingParam = new UILevelWinParam();
			}
			_pendingParam.CompletedLevel = completedLevel;
			_pendingParam.Score = score;
			_pendingParam.IsVictorySnapshot = true;
			_pendingParam.IsMainToEndlessTransition = isMainToEndlessTransition;
			_pendingParam.GameWinIqSnapshot = gameWinIqSnapshot;
			UILevelWinParam pendingParam = _pendingParam;
			if (pendingParam.OnNext == null)
			{
				pendingParam.OnNext = delegate
				{
					Gameplay.Active?.GoToNextLevel();
				};
			}
			RewardedLuckyDrawService.PrepareSettlement(completedPropUses);
			LevelWinPreloadService.StartPreload();
			SettlementPipeline.BuildAndExecute();
		}
	}

	public static void DoGradeSettlement()
	{
		UILevelWinParam pendingParam = _pendingParam;
		_pendingParam = null;
		if (pendingParam == null)
		{
			Log.LogError("[LevelVictoryService]", "DoGradeSettlement: pendingParam is null");
		}
		else
		{
			GameApp.UI.Open("UILevelWin", (UILayer)2, (object)pendingParam, (Action)null);
		}
	}

	private static void TryPlayUIGameHideAnimationOnVictory()
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIGame", ref val) && val is UIGameController uIGameController)
		{
			uIGameController.PropUseService?.PrepareForSettlement();
			uIGameController.PlayHideAnimationForSettlement();
		}
	}
}
