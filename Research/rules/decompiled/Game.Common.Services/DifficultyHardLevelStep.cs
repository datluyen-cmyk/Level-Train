using System;
using Hot.Model;
using RotateRings.Common;
using RotateRings.Model;
using RotateRings.UI.GameDifficultyToast;
using XingYun.UI.Core;

namespace Game.Common.Services;

public class DifficultyHardLevelStep : ILevelStartStep, IActivityStep
{
	private const string Tag = "[DifficultyHardLevelStep] ";

	public int Priority => 0;

	public bool ShouldRun()
	{
		int level = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 1;
		return LevelDifficultyUtil.IsHardLevel(GameInfo.CurrentGameMode, level);
	}

	public void Execute()
	{
		int level = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 1;
		ELevelDifficulty difficulty = LevelDifficultyUtil.GetDifficulty(GameInfo.CurrentGameMode, level);
		int styleId = HardLevelTipStyleResolver.ResolveAndSyncAudio();
		string text = HardLevelToastPreEnterService.ResolveLocation(styleId);
		int round = LevelStartPreEnterPipeline.CurrentRound;
		GameApp.UI.Open(text, (UILayer)2, (object)new UIGameDifficultyToastController.OpenParam
		{
			Difficulty = difficulty,
			StyleId = styleId,
			OnClosed = delegate
			{
				HardLevelToastPreEnterService.MarkCompleted();
				LevelStartPreEnterPipeline.ContinueNextForRound(round);
			}
		}, (Action)null);
		if (!GameApp.UI.IsOpen(text))
		{
			HardLevelToastPreEnterService.MarkCompleted();
			LevelStartPreEnterPipeline.ContinueNext();
		}
	}
}
