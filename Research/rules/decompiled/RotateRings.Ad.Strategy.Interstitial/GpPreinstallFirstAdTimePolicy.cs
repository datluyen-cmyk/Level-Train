using UnityEngine;

namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class GpPreinstallFirstAdTimePolicy : InterstitialPolicyBase
{
	private const string Tag = "<Ad>.GpPreinstallFirstAdTimePolicy";

	private static bool hasLoggedExperimentState;

	private static int lastLoggedScheme;

	private static RuntimePlatform lastLoggedPlatform;

	private static string lastLoggedChannel;

	private static bool lastLoggedHit;

	public override string Name => "GpPreinstallFirstAdTime";

	public static int GetConfiguredScheme()
	{
		return AdDebugSettings.GmGpPreinstallFirstAdTimeScheme;
	}

	public static bool IsSupported(int scheme)
	{
		bool num = GpPreinstallFirstAdTimeMatrix.IsSupportedScheme(scheme);
		if (!num)
		{
		}
		return num;
	}

	public static bool ShouldTakeOver()
	{
		if (InterstitialEscapeTracker.HasPlayedInterstitial)
		{
			return false;
		}
		return IsUserInExperiment();
	}

	public static bool IsUserInExperiment()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		int configuredScheme = GetConfiguredScheme();
		if (!GpPreinstallFirstAdTimeMatrix.IsSupportedScheme(configuredScheme))
		{
			LogExperimentStateIfChanged(configuredScheme, Application.platform, null, hit: false, 0);
			return false;
		}
		RuntimePlatform effectivePlatformForAndroidOnly = GetEffectivePlatformForAndroidOnly();
		string effectiveChannel = AdBridgeHelper.GetEffectiveChannel();
		int requiredSeconds;
		bool flag = GpPreinstallFirstAdTimeMatrix.TryGetThresholdSeconds(configuredScheme, effectivePlatformForAndroidOnly, effectiveChannel, out requiredSeconds);
		LogExperimentStateIfChanged(configuredScheme, effectivePlatformForAndroidOnly, effectiveChannel, flag, requiredSeconds);
		return flag;
	}

	private static void LogExperimentStateIfChanged(int scheme, RuntimePlatform platform, string channel, bool hit, int requiredSeconds)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!hasLoggedExperimentState || scheme != lastLoggedScheme || platform != lastLoggedPlatform || hit != lastLoggedHit || !string.Equals(channel, lastLoggedChannel))
		{
			hasLoggedExperimentState = true;
			lastLoggedScheme = scheme;
			lastLoggedPlatform = platform;
			lastLoggedChannel = channel;
			lastLoggedHit = hit;
		}
	}

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		int configuredScheme = GetConfiguredScheme();
		if (!IsSupported(configuredScheme))
		{
			return InterAdCheckResult.Allow;
		}
		return Evaluate(configuredScheme);
	}

	private static RuntimePlatform GetEffectivePlatformForAndroidOnly()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Application.platform;
	}

	public InterAdCheckResult Evaluate(int scheme)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (InterstitialEscapeTracker.HasPlayedInterstitial)
		{
			return InterAdCheckResult.Allow;
		}
		RuntimePlatform effectivePlatformForAndroidOnly = GetEffectivePlatformForAndroidOnly();
		string effectiveChannel = AdBridgeHelper.GetEffectiveChannel();
		string.IsNullOrWhiteSpace(effectiveChannel);
		if (!GpPreinstallFirstAdTimeMatrix.TryGetThresholdSeconds(scheme, effectivePlatformForAndroidOnly, effectiveChannel, out var requiredSeconds))
		{
			return InterAdCheckResult.Allow;
		}
		float effectiveTotalValidDurationSeconds = AdBridgeHelper.GetEffectiveTotalValidDurationSeconds();
		if (!(effectiveTotalValidDurationSeconds < (float)requiredSeconds))
		{
			return InterAdCheckResult.Allow;
		}
		return InterAdCheckResult.Block(AdBlockReason.FormatGpPreinstallFirstAdTimeBlocked(scheme, requiredSeconds, effectiveTotalValidDurationSeconds));
	}
}
