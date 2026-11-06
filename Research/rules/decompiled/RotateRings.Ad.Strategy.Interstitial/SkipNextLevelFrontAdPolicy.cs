using System.Collections.Generic;
using RotateRings.Model;
using XingYun.Experiment;

namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class SkipNextLevelFrontAdPolicy : InterstitialPolicyBase
{
	private const string Tag = "<Ad>.RestartSkipNextLevelFrontAd";

	private const int GroupEveryRestart = 1;

	private const int GroupFirstRestartPerLevel = 2;

	private int activeGroup;

	private readonly HashSet<int> skipFrontAdLevels = new HashSet<int>();

	private int observedLevel;

	private bool restartRecordedForCurrentLevel;

	public override string Name => "RestartSkipNextFrontAd";

	public void Reset()
	{
		activeGroup = 0;
		skipFrontAdLevels.Clear();
		observedLevel = 0;
		restartRecordedForCurrentLevel = false;
	}

	public override void OnInit()
	{
		Reset();
	}

	private int GetConfiguredGroup()
	{
		return FunctionDataCenter.Get<InterstitialFunction>()?.SkipNextLevelFrontAd ?? 0;
	}

	public static bool IsSupported(int group)
	{
		if (group != 1)
		{
			return group == 2;
		}
		return true;
	}

	public void NotifyRestartClicked(int level)
	{
		if (level <= 0)
		{
			return;
		}
		ObserveLevel(level);
		int configuredGroup = GetConfiguredGroup();
		if (!IsSupported(configuredGroup))
		{
			Reset();
			return;
		}
		if (activeGroup != 0 && activeGroup != configuredGroup)
		{
			Reset();
		}
		if (configuredGroup == 2 && restartRecordedForCurrentLevel)
		{
			skipFrontAdLevels.Remove(level + 1);
			return;
		}
		restartRecordedForCurrentLevel = true;
		activeGroup = configuredGroup;
		skipFrontAdLevels.Add(level + 1);
	}

	public override void OnRestartClicked(int level)
	{
		NotifyRestartClicked(level);
	}

	public InterAdCheckResult Evaluate(bool isFrontAd)
	{
		if (!isFrontAd)
		{
			return InterAdCheckResult.Allow;
		}
		int configuredGroup = GetConfiguredGroup();
		if (!IsSupported(configuredGroup))
		{
			Reset();
			return InterAdCheckResult.Allow;
		}
		int num = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 1;
		ObserveLevel(num);
		if (activeGroup != configuredGroup)
		{
			if (activeGroup != 0)
			{
				Reset();
			}
			return InterAdCheckResult.Allow;
		}
		if (!skipFrontAdLevels.Remove(num))
		{
			return InterAdCheckResult.Allow;
		}
		return InterAdCheckResult.Block("重新开始免下局前插屏");
	}

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		return Evaluate(context.IsFrontAd);
	}

	private void ObserveLevel(int level)
	{
		if (level > 0 && level != observedLevel)
		{
			observedLevel = level;
			restartRecordedForCurrentLevel = false;
			skipFrontAdLevels.RemoveWhere((int skipFrontAdLevel) => skipFrontAdLevel < level);
		}
	}
}
