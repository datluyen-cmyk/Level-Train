using Example.Cfg;
using UnityEngine;
using XingYun.Aot.Lang;

namespace Game.Lang;

public static class LanguageCompat
{
	private const string Tag = "Lang";

	public static void MigrateHiddenLanguageIfNeeded()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		string currentLanguage = LanguageContext.CurrentLanguage;
		string text = ((object)(LanguageType)0/*cast due to .constrained prefix*/).ToString();
		if (!string.Equals(currentLanguage, text) && !IsVisibleLanguage(currentLanguage))
		{
			LanguageContext.CurrentLanguage = text;
			PlayerPrefs.Save();
		}
	}

	private static bool IsVisibleLanguage(string current)
	{
		if (string.IsNullOrEmpty(current))
		{
			return false;
		}
		TbSettingLanguage tbSettingLanguage = GameApp.Config.GetTable<ExampleTables>()?.TbSettingLanguage;
		if (tbSettingLanguage?.DataList == null)
		{
			return true;
		}
		for (int i = 0; i < tbSettingLanguage.DataList.Count; i++)
		{
			SettingLanguage settingLanguage = tbSettingLanguage.DataList[i];
			if (settingLanguage == null || settingLanguage.IsShow != 1 || settingLanguage.AllLanguageCode == null)
			{
				continue;
			}
			string[] allLanguageCode = settingLanguage.AllLanguageCode;
			for (int j = 0; j < allLanguageCode.Length; j++)
			{
				if (allLanguageCode[j] == current)
				{
					return true;
				}
			}
		}
		return false;
	}
}
