using RotateRings.Common;
using UnityEngine;
using XingYun.Proxy;

namespace RotateRings.Model;

public class PlayerModelProxy : BaseProxy
{
	private const string ComboEnteredPrefsKey = "PlayerModelProxy.ComboEntered";

	public PlayerModelData Data { get; }

	public PlayerModelProxy()
		: base("PlayerModelProxy")
	{
		Data = GameApp.Save.GetOrCreate<PlayerModelData>();
		InitDefaultPlayerNameIfNeeded();
		MigratePlayerNameLanguageIfNeeded();
		PlayerService.SetName(Data.PlayerName);
		PlayerService.SetHeadId(Data.HeadId);
		PlayerService.SetHeadFrameId(Data.HeadFrameId);
	}

	private void InitDefaultPlayerNameIfNeeded()
	{
		if (string.IsNullOrEmpty(Data.PlayerName))
		{
			Data.PlayerName = GenerateDefaultName();
			Data.PlayerNameLanguage = ResolvePlayerNameLanguage(Data.PlayerName);
		}
	}

	private void MigratePlayerNameLanguageIfNeeded()
	{
		if (string.IsNullOrEmpty(Data.PlayerNameLanguage) && !string.IsNullOrEmpty(Data.PlayerName))
		{
			Data.PlayerNameLanguage = RoleNameLanguageResolver.ResolveFromConfig(Data.PlayerName);
		}
	}

	public string GetName()
	{
		return Data.PlayerName;
	}

	public void SetName(string name)
	{
		Data.PlayerName = name;
	}

	public string GetPlayerNameLanguage()
	{
		return Data.PlayerNameLanguage;
	}

	public void SetPlayerNameLanguage(string languageCode)
	{
		Data.PlayerNameLanguage = languageCode;
	}

	public static string ResolvePlayerNameLanguage(string playerName)
	{
		if (RoleNameGenerator.TryConsumeLanguageForName(playerName, out var languageCode))
		{
			return languageCode;
		}
		return RoleNameLanguageResolver.ResolveFromConfig(playerName);
	}

	public int GetHeadId()
	{
		return Data.HeadId;
	}

	public void SetHeadId(int id)
	{
		Data.HeadId = id;
	}

	public int GetHeadFrameId()
	{
		return Data.HeadFrameId;
	}

	public void SetHeadFrameId(int id)
	{
		Data.HeadFrameId = id;
	}

	public string GenerateDefaultName()
	{
		return RoleNameGenerator.GenerateDefaultName();
	}

	public void StatisticalRewardAdCount()
	{
		Data.GetAdRewardCount++;
	}

	public void MarkEnteredCombo()
	{
		PlayerPrefs.SetInt("PlayerModelProxy.ComboEntered", 1);
		PlayerPrefs.Save();
	}

	public bool GetEnteredCombo()
	{
		return PlayerPrefs.GetInt("PlayerModelProxy.ComboEntered", 0) == 1;
	}
}
