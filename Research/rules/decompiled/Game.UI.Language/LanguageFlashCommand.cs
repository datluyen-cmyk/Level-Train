using RotateRings.Services;
using XingYun.Aot.Lang;
using XingYun.Command;
using XingYun.Core.Singletons;
using XingYun.Font;

namespace Game.UI.Language;

public class LanguageFlashCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		Singleton<FontManager>.Instance.SetLanguage(LanguageContext.CurrentLanguage);
		StyleFontFallbackService.RebuildWithPlayerName(LanguageContext.CurrentLanguage);
		GameApp.Lang.RefreshAllBindTexts();
	}
}
