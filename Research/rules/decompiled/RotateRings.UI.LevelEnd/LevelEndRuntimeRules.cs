using System;
using UnityEngine;

namespace RotateRings.UI.LevelEnd;

public static class LevelEndRuntimeRules
{
	private static LevelWinAnimationConfig currentConfig;

	private static float? iqDelayOverride;

	private static float? nextDelayOverride;

	public static bool ForceSquirrelIdle3ForAcceptance { get; set; }

	public static float IqDisplayDelaySeconds
	{
		get
		{
			return iqDelayOverride ?? CurrentSettings.IqGrowth.OffsetSeconds;
		}
		set
		{
			iqDelayOverride = Mathf.Max(0f, value);
		}
	}

	public static float NextLevelDisplayDelaySeconds
	{
		get
		{
			return nextDelayOverride ?? CurrentSettings.NextButton.OffsetSeconds;
		}
		set
		{
			nextDelayOverride = Mathf.Max(0f, value);
		}
	}

	private static LevelWinAnimationSettings CurrentSettings
	{
		get
		{
			if (!((Object)(object)currentConfig != (Object)null))
			{
				return LevelWinAnimationSettings.Default;
			}
			return currentConfig.Settings;
		}
	}

	public static LevelWinAnimationSettings Capture(LevelWinAnimationConfig config, bool useExperiment = true)
	{
		if ((Object)(object)config == (Object)null)
		{
			throw new ArgumentNullException("config");
		}
		currentConfig = config;
		LevelWinAnimationSettings settings = config.GetSettings(useExperiment);
		if (!useExperiment)
		{
			return settings;
		}
		if (iqDelayOverride.HasValue)
		{
			settings.IqGrowth.OffsetSeconds = iqDelayOverride.Value;
		}
		if (nextDelayOverride.HasValue)
		{
			settings.NextButton.OffsetSeconds = nextDelayOverride.Value;
		}
		settings.Validate();
		return settings;
	}

	public static void ResetRules()
	{
		iqDelayOverride = null;
		nextDelayOverride = null;
	}
}
