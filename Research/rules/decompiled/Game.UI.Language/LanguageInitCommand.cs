using System.Collections.Generic;
using System.Linq;
using Example.Cfg;
using XingYun.Aot.Lang;
using XingYun.Command;

namespace Game.UI.Language;

public class LanguageInitCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		List<SettingLanguage> settingLanguageList = (from lang in GameApp.Config.GetTable<ExampleTables>().TbSettingLanguage.DataList
			where lang.IsShow == 1
			orderby lang.SortId
			select lang).ToList();
		((BaseCommand)this).SendEvent<LanguageInitSuccess>(new LanguageInitSuccess
		{
			languageInitData = new LanguageInitData
			{
				SettingLanguageList = settingLanguageList,
				CurrentLanguage = LanguageContext.CurrentLanguage
			}
		});
	}
}
