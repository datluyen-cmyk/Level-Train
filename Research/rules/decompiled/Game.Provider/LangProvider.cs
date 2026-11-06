using Example.Cfg;
using XingYun.Aot.Lang;
using XingYun.Lang;

namespace Game.Provider;

public class LangProvider : ITextProvider
{
	private TbTextConfig tbTextConfig;

	public string GetText(string key)
	{
		if (tbTextConfig == null)
		{
			ExampleTables table = GameApp.Config.GetTable<ExampleTables>();
			tbTextConfig = table.TbTextConfig;
		}
		TextConfig orDefault = tbTextConfig.GetOrDefault(key);
		if (orDefault == null || orDefault.Text == null)
		{
			return string.Empty;
		}
		if (orDefault.Text.TryGetValue(LanguageContext.CurrentLanguage, out var value))
		{
			return value;
		}
		return string.Empty;
	}
}
