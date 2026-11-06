using System;
using XingYun.Proxy;

namespace RotateRings.UI.Setting;

public class SettingProxy : BaseProxy
{
	private SettingData data;

	public SettingData Data
	{
		get
		{
			if (data == null)
			{
				data = GameApp.Save.GetOrCreate<SettingData>();
			}
			return data;
		}
	}

	public bool HasAgreedStartupPrivacy => Data.HasAgreedStartupPrivacy;

	public SettingProxy()
		: base("SettingProxy")
	{
	}

	public void SetMusicState(bool isOn)
	{
		Data.IsMusicOn = isOn;
		Data.HasUserCustomizedMusic = true;
	}

	public void SetSoundState(bool isOn)
	{
		Data.IsSoundOn = isOn;
	}

	public void SetVibrationState(bool isOn)
	{
		Data.IsVibrateOn = isOn;
	}

	public void SetPushState(bool isOn)
	{
		Data.IsPushOn = isOn;
	}

	public void MarkMusicCustomizedByUser()
	{
		Data.HasUserCustomizedMusic = true;
	}

	public void MarkStartupPrivacyAgreed()
	{
		Data.HasAgreedStartupPrivacy = true;
	}

	public bool GetCardDragFailedTipsSetting()
	{
		return Data.IsCardDragFaildTipOn;
	}

	public void SetCardDragFailedTipsSetting(bool on)
	{
		Data.IsCardDragFaildTipOn = on;
	}

	public void SetCardDragFailedTipsIgnoreToday()
	{
		Data.IsIgnoredCardDragFailedTipsToday = true;
		Data.CardDragFailedTipsIgnoreDate = DateTime.Today.ToString("yyyy-MM-dd");
	}

	public bool GetCardDragFailedTipsIgnoreForToday()
	{
		if (!Data.IsIgnoredCardDragFailedTipsToday)
		{
			return false;
		}
		return Data.CardDragFailedTipsIgnoreDate == DateTime.Today.ToString("yyyy-MM-dd");
	}

	public bool TryConsumeCardDragFailedTipsDefaultExpand()
	{
		if (Data.CardDragFailedTipsDefaultExpandCount > 0)
		{
			Data.CardDragFailedTipsDefaultExpandCount--;
			return true;
		}
		return false;
	}
}
