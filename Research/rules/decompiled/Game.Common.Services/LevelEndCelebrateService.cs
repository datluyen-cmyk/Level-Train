using System;
using RotateRings.Common;
using RotateRings.Services;
using XingYun.Experiment;

namespace Game.Common.Services;

public static class LevelEndCelebrateService
{
	private readonly struct LevelEndCelebrateTimeline
	{
		public bool PlayPostAdBetweenPhases { get; }

		public float FullShowDuration { get; }

		public float TotalDuration { get; }

		public float ExitDuration => Math.Max(0f, TotalDuration - FullShowDuration);

		public LevelEndCelebrateTimeline(bool playPostAdBetweenPhases, float fullShowDuration, float totalDuration)
		{
			PlayPostAdBetweenPhases = playPostAdBetweenPhases;
			FullShowDuration = fullShowDuration;
			TotalDuration = totalDuration;
		}
	}

	private const string Tag = "[LevelEndCelebrateService] ";

	private const string CelebrateConfettiAudioVariantId = "variant_d37f989e";

	private const string CelebrateLogoAudioVariantId = "variant_676baec5";

	private const string CelebrateDeluxeAudioVariantId = "variant_02e9b029";

	private const string CelebrateTextAudioVariantId = "variant_d8b6be5f";

	private static readonly LevelEndCelebrateTimeline DefaultTimeline = new LevelEndCelebrateTimeline(playPostAdBetweenPhases: false, 0f, 0f);

	public static bool IsEnabled()
	{
		LevelEndFunction levelEndFunction = FunctionDataCenter.Get<LevelEndFunction>();
		if (levelEndFunction != null)
		{
			return levelEndFunction.WinCelebrateType > 0;
		}
		return false;
	}

	public static void ActivateTransitionAudioExperiment()
	{
		if (TryGetTransitionAudio(FunctionDataCenter.Get<LevelEndFunction>()?.WinCelebrateType ?? 0, out var variantId, out var _))
		{
			AudioService.ActivateExperiment(variantId);
		}
	}

	public static void PlayTransitionSound(int winCelebrateType)
	{
		if (TryGetTransitionAudio(winCelebrateType, out var _, out var audioKey))
		{
			AudioService.PlayUiSound(audioKey.Value);
		}
	}

	private static bool TryGetTransitionAudio(int winCelebrateType, out string variantId, out AudioKey audioKey)
	{
		switch (winCelebrateType)
		{
		case 1:
			variantId = "variant_d37f989e";
			audioKey = AudioKeys.CelebratePreResultConfetti;
			return true;
		case 2:
			variantId = "variant_676baec5";
			audioKey = AudioKeys.CelebratePreResultLogo;
			return true;
		case 3:
		case 5:
			variantId = "variant_02e9b029";
			audioKey = AudioKeys.CelebratePreResultDeluxe;
			return true;
		case 4:
		case 6:
			variantId = "variant_d8b6be5f";
			audioKey = AudioKeys.CelebratePreResultText;
			return true;
		default:
			variantId = null;
			audioKey = null;
			return false;
		}
	}

	public static bool ShouldPlayPostAdBetweenPhases()
	{
		return DefaultTimeline.PlayPostAdBetweenPhases;
	}

	public static void PlayEntrance(Action onCompleted)
	{
		if (!ShouldPlayPostAdBetweenPhases())
		{
			onCompleted?.Invoke();
		}
		else
		{
			GameApp.Timer.Delay(DefaultTimeline.FullShowDuration, onCompleted);
		}
	}

	public static void PlayExit(Action onCompleted)
	{
		GameApp.Timer.Delay(DefaultTimeline.ExitDuration, onCompleted);
	}
}
