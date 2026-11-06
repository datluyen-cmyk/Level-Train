using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using RotateRings.UI.Setting;
using UniLog;
using XingYun.Experiment;

namespace RotateRings.Services;

public static class LevelEndBgmVolumeService
{
	private const string TAG = "[Audio][LevelEndBgmVolumeService] ";

	private const float VolumeFadeDuration = 0.5f;

	public const int KeepVolumeLevel = 0;

	private static float volumeBeforeSettlement = -1f;

	private static Tween volumeTween;

	public static int ResolveLevel()
	{
		int num = FunctionDataCenter.Get<AudioFunction>()?.LevelEndBgmVolume ?? 0;
		if (num < 1 || num > 3)
		{
			return 0;
		}
		return num;
	}

	public static float GetVolumeByLevel(int level)
	{
		return level switch
		{
			1 => 0.4f, 
			2 => 0.2f, 
			3 => 0f, 
			_ => 1f, 
		};
	}

	public static void ApplyOnVictory()
	{
		if (IsUserMusicOn() && !(volumeBeforeSettlement >= 0f))
		{
			int num = ResolveLevel();
			if (num != 0)
			{
				float volumeByLevel = GetVolumeByLevel(num);
				volumeBeforeSettlement = GameApp.Audio.MusicVolume;
				TweenMusicVolume(volumeBeforeSettlement, volumeByLevel, 0.5f, (Ease)9);
			}
		}
	}

	public static void RestoreAfterSettlement()
	{
		if (!(volumeBeforeSettlement < 0f))
		{
			float toVolume = volumeBeforeSettlement;
			volumeBeforeSettlement = -1f;
			StopVolumeTween();
			if (IsUserMusicOn())
			{
				TweenMusicVolume(GameApp.Audio.MusicVolume, toVolume, 0.5f, (Ease)8);
			}
		}
	}

	private static bool IsUserMusicOn()
	{
		return GameApp.Proxy.Get<SettingProxy>().Data.IsMusicOn;
	}

	private static void StopVolumeTween()
	{
		Tween obj = volumeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		volumeTween = null;
	}

	private static void TweenMusicVolume(float fromVolume, float toVolume, float duration, Ease ease)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		StopVolumeTween();
		if (duration <= 0f || Math.Abs(fromVolume - toVolume) < 0.0001f)
		{
			AudioService.SetMusicVolume(toVolume);
			return;
		}
		float volume = fromVolume;
		volumeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => volume), (DOSetter<float>)delegate(float value)
		{
			volume = value;
			AudioService.SetMusicVolume(value);
		}, toVolume, duration), ease), (TweenCallback)delegate
		{
			try
			{
				volumeTween = null;
				AudioService.SetMusicVolume(toVolume);
			}
			catch (Exception ex)
			{
				Log.LogError("[Audio][LevelEndBgmVolumeService] ", "BGM 音量渐变完成回调异常: {0}", new object[1] { ex.Message });
			}
		});
	}
}
