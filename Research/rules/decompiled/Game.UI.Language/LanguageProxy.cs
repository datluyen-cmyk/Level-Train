using Game.UI.Language.Model;
using UniLog;
using XingYun.Proxy;
using XingYun.Save;

namespace Game.UI.Language;

public class LanguageProxy : BaseProxy
{
	private LangSave save;

	public LanguageProxy()
		: base("LanguageProxy")
	{
		save = SaveManager.Instance.GetOrCreate<LangSave>();
	}

	public void SaveLanguage(string languageCode)
	{
		if (string.IsNullOrEmpty(languageCode))
		{
			Log.LogError("LanguageProxy", "尝试保存空语言代码");
		}
		else
		{
			save.language = languageCode;
		}
	}

	public string GetSavedLanguage()
	{
		return save.language;
	}
}
