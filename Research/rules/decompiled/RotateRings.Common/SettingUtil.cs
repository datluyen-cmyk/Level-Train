using RotateRings.Services;
using RotateRings.UI.Setting;
using XingYun.Core.Singletons;
using XingYun.Vibrate;

namespace RotateRings.Common;

public static class SettingUtil
{
	public static bool GetMusicState()
	{
		return GameApp.Proxy.Get<SettingProxy>().Data.IsMusicOn;
	}

	public static void ToggleMusicState(bool isOn, bool userInitiated = false)
	{
		GameApp.Proxy.Get<SettingProxy>().SetMusicState(isOn);
		if (userInitiated)
		{
			GameApp.Proxy.Get<SettingProxy>().MarkMusicCustomizedByUser();
		}
		ApplyMusicVolume(isOn);
		BgmController.RefreshAfterMusicToggle();
	}

	public static bool GetSoundState()
	{
		return GameApp.Proxy.Get<SettingProxy>().Data.IsSoundOn;
	}

	public static void ToggleSoundState(bool isOn)
	{
		GameApp.Proxy.Get<SettingProxy>().SetSoundState(isOn);
		ApplySoundVolume(isOn);
	}

	public static bool GetVibrationState()
	{
		return GameApp.Proxy.Get<SettingProxy>().Data.IsVibrateOn;
	}

	public static void ToggleVibrationState(bool isOn)
	{
		GameApp.Proxy.Get<SettingProxy>().SetVibrationState(isOn);
		ApplyVibrationState(isOn);
	}

	public static void TogglePushState(bool isOn)
	{
		GameApp.Proxy.Get<SettingProxy>().SetPushState(isOn);
	}

	public static void ApplyAllSettings()
	{
		SettingData data = GameApp.Proxy.Get<SettingProxy>().Data;
		ApplyMusicVolume(data.IsMusicOn);
		ApplySoundVolume(data.IsSoundOn);
		ApplyVibrationState(data.IsVibrateOn);
	}

	private static void ApplyMusicVolume(bool isOn)
	{
		LevelEndBgmVolumeService.RestoreAfterSettlement();
		AudioService.SetMusicMuted(!isOn);
	}

	private static void ApplySoundVolume(bool isOn)
	{
		AudioService.SetSoundMuted(!isOn);
	}

	private static void ApplyVibrationState(bool isOn)
	{
		MonoSingleton<VibrateManager>.Instance.EnableVibrateSequence = isOn;
	}
}
