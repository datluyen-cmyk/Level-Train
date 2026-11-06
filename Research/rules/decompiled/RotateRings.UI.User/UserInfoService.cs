using System;
using System.Collections.Generic;
using Example.Cfg;
using RotateRings.UI.Setting;
using Rotaterings.Rank;
using XingYun.Aot.Lang;
using XingYun.Config;
using XingYun.Experiment;

namespace RotateRings.UI.User;

public static class UserInfoService
{
	private const string EnglishUs = "EN_US";

	private const string EnglishGb = "EN_GB";

	private const string ValidationTag = "User";

	public const int MinNicknameCharacterCount = 2;

	public const int MaxNicknameCharacterCount = 12;

	private static string[] sensitiveWordsByLength;

	private static bool sensitiveWordsLoadAttempted;

	public static UserInfoFunction Get()
	{
		return FunctionDataCenter.Get<UserInfoFunction>() ?? new UserInfoFunction();
	}

	public static bool IsEnglishSystemLanguage()
	{
		string currentLanguage = LanguageContext.CurrentLanguage;
		if (!(currentLanguage == "EN_US"))
		{
			return currentLanguage == "EN_GB";
		}
		return true;
	}

	public static bool IsEnglishFreeNamingEnabled()
	{
		if (Get().InputName == 1)
		{
			return IsEnglishSystemLanguage();
		}
		return false;
	}

	public static bool ShouldShowHomeAvatarEntry()
	{
		return Get().HomeAvatar == 1;
	}

	public static bool ShouldAutoFocusNameInputOnFirstOpen()
	{
		if (Get().PopInputField == 1)
		{
			return IsEnglishSystemLanguage();
		}
		return false;
	}

	public static bool IsHomeAvatarUnlocked()
	{
		return RankActivityEntry.Service?.CanOpenRank() ?? false;
	}

	public static bool HasConsumedFirstNameInputFocus()
	{
		return GameApp.Save.GetOrCreate<SettingData>().HasConsumedFirstNameInputFocus;
	}

	public static void MarkConsumedFirstNameInputFocus()
	{
		SettingData orCreate = GameApp.Save.GetOrCreate<SettingData>();
		if (!orCreate.HasConsumedFirstNameInputFocus)
		{
			orCreate.HasConsumedFirstNameInputFocus = true;
		}
	}

	public static bool IsAllowedChar(char c)
	{
		if (c >= 'A' && c <= 'Z')
		{
			return true;
		}
		if (c >= 'a' && c <= 'z')
		{
			return true;
		}
		if (c >= '0' && c <= '9')
		{
			return true;
		}
		if (c >= '!')
		{
			return c <= '@';
		}
		return false;
	}

	public static bool TryValidate(string name, out string errorMessage)
	{
		errorMessage = null;
		if (string.IsNullOrWhiteSpace(name))
		{
			errorMessage = "Nickname cannot be empty or whitespace only.";
			return false;
		}
		string text = name.Trim();
		if (text.Length < 2 || text.Length > 12)
		{
			errorMessage = $"Nickname must be {2} to {12} characters.";
			return false;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (!IsAllowedChar(text[i]))
			{
				errorMessage = "Nickname contains invalid characters.";
				return false;
			}
		}
		EnsureSensitiveWordsLoaded();
		if (ContainsSensitiveWord(text))
		{
			errorMessage = "Nickname contains a blocked word. Please change it and try again.";
			return false;
		}
		return true;
	}

	public static bool IsRestorableDraft(string draft)
	{
		if (string.IsNullOrWhiteSpace(draft))
		{
			return false;
		}
		string text = draft.Trim();
		if (text.Length < 2 || text.Length > 12)
		{
			return false;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (!IsAllowedChar(text[i]))
			{
				return false;
			}
		}
		EnsureSensitiveWordsLoaded();
		return !ContainsSensitiveWord(text);
	}

	private static void EnsureSensitiveWordsLoaded()
	{
		if (sensitiveWordsLoadAttempted)
		{
			return;
		}
		sensitiveWordsLoadAttempted = true;
		sensitiveWordsByLength = Array.Empty<string>();
		ConfigManager config = GameApp.Config;
		List<PlayerNameSensitiveWordConfig> list = ((config != null) ? config.GetTable<ExampleTables>() : null)?.TbPlayerNameSensitiveWordConfig?.DataList;
		if (list == null || list.Count == 0)
		{
			return;
		}
		List<string> list2 = new List<string>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			PlayerNameSensitiveWordConfig playerNameSensitiveWordConfig = list[i];
			if (playerNameSensitiveWordConfig != null && !string.IsNullOrEmpty(playerNameSensitiveWordConfig.Word))
			{
				list2.Add(playerNameSensitiveWordConfig.Word.ToLowerInvariant());
			}
		}
		if (list2.Count != 0)
		{
			list2.Sort((string a, string b) => a.Length.CompareTo(b.Length));
			sensitiveWordsByLength = list2.ToArray();
		}
	}

	private static bool ContainsSensitiveWord(string name)
	{
		if (sensitiveWordsByLength == null || sensitiveWordsByLength.Length == 0)
		{
			return false;
		}
		string text = name.ToLowerInvariant();
		int length = text.Length;
		for (int i = 0; i < sensitiveWordsByLength.Length; i++)
		{
			string text2 = sensitiveWordsByLength[i];
			if (text2.Length > length)
			{
				break;
			}
			if (text.Contains(text2, StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}
}
