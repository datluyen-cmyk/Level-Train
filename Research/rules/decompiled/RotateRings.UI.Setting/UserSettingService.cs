using RotateRings.Common;

namespace RotateRings.UI.Setting;

public static class UserSettingService
{
	private static int gmHeadInfoTypeOverride = -1;

	private static int gmCardDragFailedTipsOverride = -1;

	public static int GetHeadInfoType()
	{
		if (gmHeadInfoTypeOverride >= 0)
		{
			return gmHeadInfoTypeOverride;
		}
		return FeatureConfig.UI.UserSetting.HeadInfoType;
	}

	public static bool ShouldShowRedPoint()
	{
		if (GetHeadInfoType() < 2)
		{
			return false;
		}
		return !GameApp.Save.GetOrCreate<SettingData>().HasOpenedUserSetting;
	}

	public static void MarkUserSettingOpened()
	{
		SettingData orCreate = GameApp.Save.GetOrCreate<SettingData>();
		if (!orCreate.HasOpenedUserSetting)
		{
			orCreate.HasOpenedUserSetting = true;
		}
	}

	public static void SetGMHeadInfoType(int type)
	{
		gmHeadInfoTypeOverride = type;
	}

	public static int GetCardDragFailedToastType()
	{
		if (gmCardDragFailedTipsOverride >= 0)
		{
			return gmCardDragFailedTipsOverride;
		}
		return FeatureConfig.Gameplay.GameBord.CardDragFailedTipsStrategyId;
	}

	public static void SetGMCardDragFailedToastType(int type)
	{
		if (type >= 0 && type <= 2)
		{
			gmCardDragFailedTipsOverride = type;
		}
	}

	public static bool ExptBlockCardDragFailedTip()
	{
		return GetCardDragFailedToastType() == 2;
	}

	public static bool IsCardDragFailedTipsCheckStyle()
	{
		return GetCardDragFailedToastType() == 1;
	}

	public static bool CanShowCardDragFailedTips()
	{
		if (ExptBlockCardDragFailedTip())
		{
			return false;
		}
		if (IsCardDragFailedTipsCheckStyle())
		{
			SettingProxy settingProxy = GameApp.Proxy.Get<SettingProxy>();
			if (settingProxy != null && !settingProxy.GetCardDragFailedTipsSetting())
			{
				return false;
			}
			if (settingProxy != null && settingProxy.GetCardDragFailedTipsIgnoreForToday())
			{
				return false;
			}
		}
		return true;
	}

	public static void ToggleCardDragFailedState(bool openStatus)
	{
		GameApp.Proxy.Get<SettingProxy>().SetCardDragFailedTipsSetting(openStatus);
	}
}
