using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Common;
using UnityEngine;
using XingYun.Command;

namespace RotateRings.UI.User;

public class UserInitCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		List<HeadConfig> headConfigList = ((BaseCommand)this).GetProxy<UserProxy>().GetHeadConfigList();
		List<UserIconData> list = new List<UserIconData>();
		foreach (HeadConfig item in headConfigList)
		{
			Sprite headSprite = PlayerService.GetHeadSprite(item.ID);
			list.Add(new UserIconData
			{
				Id = item.ID,
				Sprite = headSprite
			});
		}
		UserInitData userInitData = new UserInitData
		{
			PlayerName = PlayerService.GetName(),
			SelectedHeadId = PlayerService.GetHeadId(),
			SelectedHeadFrameId = PlayerService.GetHeadFrameId(),
			HeadList = list
		};
		((BaseCommand)this).SendEvent<UserInitData>(userInitData);
	}
}
