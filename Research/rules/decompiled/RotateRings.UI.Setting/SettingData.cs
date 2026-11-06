using System;
using XingYun.Save;

namespace RotateRings.UI.Setting;

[Serializable]
public class SettingData : ISaveDefaultInitializer
{
	public bool IsMusicOn = true;

	public bool IsSoundOn = true;

	public bool IsVibrateOn = true;

	public bool IsPushOn = true;

	public bool HasUserCustomizedMusic;

	public bool HasAgreedStartupPrivacy;

	public bool HasOpenedUserSetting;

	public bool HasConsumedFirstNameInputFocus;

	public string NameEditDraft;

	public bool IsCardDragFaildTipOn = true;

	public bool IsIgnoredCardDragFailedTipsToday;

	public string CardDragFailedTipsIgnoreDate;

	public int CardDragFailedTipsDefaultExpandCount;

	public bool HasCompletedMultiTouchGuide;

	public int MultiTouchGuideReportedOpenSteps;

	public int MultiTouchGuideReportedCompleteSteps;

	public void OnCreateDefault()
	{
		SettingFunction settingFunction = SettingFunctionResolver.Resolve();
		IsMusicOn = settingFunction.DefaultMusicOn;
		IsSoundOn = settingFunction.DefaultSoundOn;
		IsVibrateOn = settingFunction.DefaultVibrateOn;
		IsPushOn = true;
		HasUserCustomizedMusic = false;
		HasAgreedStartupPrivacy = false;
		IsCardDragFaildTipOn = true;
		IsIgnoredCardDragFailedTipsToday = false;
		CardDragFailedTipsIgnoreDate = string.Empty;
		CardDragFailedTipsDefaultExpandCount = 0;
		HasCompletedMultiTouchGuide = false;
		MultiTouchGuideReportedOpenSteps = 0;
		MultiTouchGuideReportedCompleteSteps = 0;
	}
}
