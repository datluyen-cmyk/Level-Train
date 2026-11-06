using RotateRings.Services;
using XingYun.Aot;
using XingYun.Command;
using XingYun.Core.Singletons;
using XingYun.Font;

namespace Game.UI.Language;

public class LoadAllFontCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		Singleton<FontManager>.Instance.RebuildFontChainWithAllFonts((FontStyleType)0);
		Singleton<FontManager>.Instance.RebuildFontChainWithAllFonts((FontStyleType)1);
		StyleFontFallbackService.RebuildExtrasWithAllLanguages();
		Singleton<FontManager>.Instance.ApplyToAllTexts();
	}
}
