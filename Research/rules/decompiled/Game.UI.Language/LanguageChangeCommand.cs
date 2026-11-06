using Example.Cfg;
using Game.DataTrack;
using RotateRings.Services;
using UniLog;
using XingYun.Aot.Lang;
using XingYun.Command;
using XingYun.Core.Singletons;
using XingYun.Font;

namespace Game.UI.Language;

public class LanguageChangeCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		if (!(param is LanguageChangeRequest languageChangeRequest) || string.IsNullOrEmpty(languageChangeRequest.LanguageKey))
		{
			Log.LogError("LanguageChangeCommand", "无效的语言Key");
			((BaseCommand)this).SendEvent<LanguageFailed>(new LanguageFailed
			{
				ErrorMessage = "无效的语言Key"
			});
			return;
		}
		SettingLanguage settingLanguage = GameApp.Config.GetTable<ExampleTables>().TbSettingLanguage.Get(languageChangeRequest.LanguageKey);
		if (settingLanguage == null || settingLanguage.AllLanguageCode == null || settingLanguage.AllLanguageCode.Length == 0)
		{
			Log.LogError("LanguageChangeCommand", "找不到语言配置: " + languageChangeRequest.LanguageKey);
			((BaseCommand)this).SendEvent<LanguageFailed>(new LanguageFailed
			{
				ErrorMessage = "找不到语言配置"
			});
			return;
		}
		string text = settingLanguage.AllLanguageCode[0];
		((BaseCommand)this).GetProxy<LanguageProxy>().SaveLanguage(text);
		LanguageContext.CurrentLanguage = text;
		Singleton<FontManager>.Instance.SetLanguage(text);
		StyleFontFallbackService.RebuildWithPlayerName(text);
		GameApp.Lang.RefreshAllBindTexts();
		RRDataTrack.Instance.ReportUserSettingLanguage(text);
		((BaseCommand)this).SendEvent<LanguageChangeSuccess>(new LanguageChangeSuccess
		{
			LanguageCode = text
		});
	}
}
