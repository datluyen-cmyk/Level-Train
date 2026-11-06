using RotateRings.UI.Setting;

namespace RotateRings.GamePlay.MultiTouchGuide;

public static class MultiTouchGuideSave
{
	public static bool HasCompleted()
	{
		return GameApp.Save.GetOrCreate<SettingData>().HasCompletedMultiTouchGuide;
	}

	public static void MarkCompleted()
	{
		SettingData orCreate = GameApp.Save.GetOrCreate<SettingData>();
		if (!orCreate.HasCompletedMultiTouchGuide)
		{
			orCreate.HasCompletedMultiTouchGuide = true;
			GameApp.Save.SaveAll();
		}
	}

	public static void ClearCompleted()
	{
		SettingData orCreate = GameApp.Save.GetOrCreate<SettingData>();
		orCreate.HasCompletedMultiTouchGuide = false;
		orCreate.MultiTouchGuideReportedOpenSteps = 0;
		orCreate.MultiTouchGuideReportedCompleteSteps = 0;
		GameApp.Save.SaveAll();
	}

	public static bool TryMarkStepCompleteReported(MultiTouchGuideStep step)
	{
		int num = 1 << (int)step;
		if (num == 0)
		{
			return true;
		}
		SettingData orCreate = GameApp.Save.GetOrCreate<SettingData>();
		if ((orCreate.MultiTouchGuideReportedCompleteSteps & num) != 0)
		{
			return false;
		}
		orCreate.MultiTouchGuideReportedCompleteSteps |= num;
		GameApp.Save.SaveAll();
		return true;
	}

	public static bool TryMarkStepOpenReported(MultiTouchGuideStep step)
	{
		int num = 1 << (int)step;
		if (num == 0)
		{
			return true;
		}
		SettingData orCreate = GameApp.Save.GetOrCreate<SettingData>();
		if ((orCreate.MultiTouchGuideReportedOpenSteps & num) != 0)
		{
			return false;
		}
		orCreate.MultiTouchGuideReportedOpenSteps |= num;
		GameApp.Save.SaveAll();
		return true;
	}
}
