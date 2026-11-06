using System;
using RotateRings.Common;
using UnityEngine;
using XingYun.Command;

namespace RotateRings.UI.Setting;

public class SettingHomeInitCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		SettingProxy settingProxy = GameApp.Proxy.Get<SettingProxy>();
		Sprite headSprite = null;
		try
		{
			headSprite = PlayerService.GetHeadSprite();
		}
		catch (Exception)
		{
		}
		SettingHomeInit settingHomeInit = new SettingHomeInit
		{
			IsMusicOn = settingProxy.Data.IsMusicOn,
			IsSoundOn = settingProxy.Data.IsSoundOn,
			IsVibrateOn = settingProxy.Data.IsVibrateOn,
			PlayerName = PlayerService.GetName(),
			HeadSprite = headSprite,
			HeadFrameSprite = PlayerService.GetHeadFrameSprite()
		};
		((BaseCommand)this).SendEvent<SettingHomeInit>(settingHomeInit);
	}
}
