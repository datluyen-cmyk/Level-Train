using XingYun.Command;

namespace RotateRings.UI.Setting;

public class SettingInGameInitCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		SettingProxy settingProxy = GameApp.Proxy.Get<SettingProxy>();
		SettingInGameInit settingInGameInit = new SettingInGameInit
		{
			IsMusicOn = settingProxy.Data.IsMusicOn,
			IsSoundOn = settingProxy.Data.IsSoundOn,
			IsVibrateOn = settingProxy.Data.IsVibrateOn
		};
		((BaseCommand)this).SendEvent<SettingInGameInit>(settingInGameInit);
	}
}
