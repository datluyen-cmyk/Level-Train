using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Common;
using RotateRings.Model;
using XingYun.Proxy;

namespace RotateRings.UI.User;

public class UserProxy : BaseProxy
{
	public UserProxy()
		: base("UserProxy")
	{
	}

	public List<HeadConfig> GetHeadConfigList()
	{
		return GameApp.Config.GetTable<ExampleTables>()?.TbHeadConfig?.DataList ?? new List<HeadConfig>(0);
	}

	public List<HeadFrameConfig> GetHeadFrameConfigList()
	{
		return new List<HeadFrameConfig>(0);
	}

	public void SaveUserInfo(int headId, int headFrameId, string playerName)
	{
		PlayerService.SetHeadId(headId);
		PlayerService.SetHeadFrameId(headFrameId);
		PlayerService.SetName(playerName);
		PlayerModelProxy playerModelProxy = GameApp.Proxy.Get<PlayerModelProxy>();
		if (playerModelProxy != null)
		{
			playerModelProxy.SetHeadId(headId);
			playerModelProxy.SetHeadFrameId(headFrameId);
			playerModelProxy.SetName(playerName);
			playerModelProxy.SetPlayerNameLanguage(PlayerModelProxy.ResolvePlayerNameLanguage(playerName));
		}
	}

	public void SaveAvatarInfo(int headId, int headFrameId)
	{
		PlayerService.SetHeadId(headId);
		PlayerService.SetHeadFrameId(headFrameId);
		PlayerModelProxy playerModelProxy = GameApp.Proxy.Get<PlayerModelProxy>();
		if (playerModelProxy != null)
		{
			playerModelProxy.SetHeadId(headId);
			playerModelProxy.SetHeadFrameId(headFrameId);
		}
	}

	public void SaveNameInfo(string playerName)
	{
		PlayerService.SetName(playerName);
		PlayerModelProxy playerModelProxy = GameApp.Proxy.Get<PlayerModelProxy>();
		if (playerModelProxy != null)
		{
			playerModelProxy.SetName(playerName);
			playerModelProxy.SetPlayerNameLanguage(PlayerModelProxy.ResolvePlayerNameLanguage(playerName));
		}
	}
}
