using System;
using Hot.Model;
using RotateRings.Common;
using RotateRings.Model;
using RotateRings.SpecialLevel;
using RotateRings.UI.GameEnterToast;
using RotateRings.UI.GameEnterToast.Core;
using XingYun.UI.Core;

namespace Game.Common.Services;

public class LevelEntryBannerStep : ILevelStartStep, IActivityStep
{
	private const string TAG = "[LevelEntryBannerStep] ";

	public int Priority => 10;

	public bool ShouldRun()
	{
		if (SpecialLevelModule.IsSpecialLevelMode())
		{
			return true;
		}
		if (!LevelEntryBannerConfig.ResolveOpenFlg())
		{
			return false;
		}
		return (GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0) >= LevelEntryBannerConfig.ResolveOpenLevel();
	}

	public void Execute()
	{
		int round = LevelStartPipeline.CurrentRound;
		if (SpecialLevelModule.TryGetActiveBanner(out var styleId, out var langKey))
		{
			ExecuteSpecialLevelBanner(round, styleId, langKey);
			return;
		}
		int level = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0;
		ELevelDifficulty difficulty = LevelDifficultyUtil.GetDifficulty(GameInfo.CurrentGameMode, level);
		bool flag = LevelEntryBannerConfig.ShouldShowShapeBanner(level);
		int styleId2 = LevelEntryBannerConfig.ResolveStyleId(flag);
		string text = LevelEntryBannerConfig.ResolveLocation(styleId2, flag);
		bool showBgMask = LevelEntryBannerConfig.ResolveShowBgMask(styleId2);
		var (text2, args) = LevelEntryBannerDataSource.Pick(level, difficulty);
		if (text2 == null)
		{
			LevelStartPipeline.ContinueNextForRound(round);
			return;
		}
		GameApp.UI.Open(text, (UILayer)4, (object)new UIGameEnterToastController.OpenParam
		{
			LangKey = text2,
			Args = args,
			StyleId = styleId2,
			ShowBgMask = showBgMask,
			ShowShapeBanner = flag,
			OnClosed = delegate
			{
				LevelStartPipeline.ContinueNextForRound(round);
			}
		}, (Action)null);
		if (!GameApp.UI.IsOpen(text))
		{
			LevelStartPipeline.ContinueNextForRound(round);
		}
	}

	private void ExecuteSpecialLevelBanner(int round, int styleId, string langKey)
	{
		bool flag = LevelEntryBannerConfig.ShouldShowShapeBanner(GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0);
		string text = LevelEntryBannerConfig.ResolveLocation(styleId, flag);
		GameApp.UI.Open(text, (UILayer)4, (object)new UIGameEnterToastController.OpenParam
		{
			LangKey = langKey,
			Args = null,
			StyleId = styleId,
			ShowBgMask = LevelEntryBannerConfig.ResolveShowBgMask(styleId),
			ShowShapeBanner = flag,
			OnClosed = delegate
			{
				LevelStartPipeline.ContinueNextForRound(round);
			}
		}, (Action)null);
		if (!GameApp.UI.IsOpen(text))
		{
			LevelStartPipeline.ContinueNextForRound(round);
		}
	}
}
