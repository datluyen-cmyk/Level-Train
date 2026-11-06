using System.Collections.Generic;
using Example.Cfg;
using Script.Tools;
using UnityEngine;
using XingYun.Config;

namespace RotateRings.Common;

public static class PlayerService
{
	public const string DefaultPlayerName = "Player";

	private static readonly List<int> DefaultHeadIds = new List<int> { 1, 2, 3, 4, 5 };

	private static string playerName = "Player";

	private static int headId = 1;

	private static int headFrameId = 1;

	private static readonly Dictionary<int, Sprite> headSpriteCache = new Dictionary<int, Sprite>();

	private static readonly Dictionary<int, Sprite> headFrameSpriteCache = new Dictionary<int, Sprite>();

	public static void Init()
	{
		playerName = "Player";
		headId = 1;
		headFrameId = 1;
		headSpriteCache.Clear();
		headFrameSpriteCache.Clear();
	}

	public static string GenerateDefaultName()
	{
		return RoleNameGenerator.GenerateDefaultName();
	}

	public static string GenerateRandomName()
	{
		return RoleNameGenerator.GenerateRandomName();
	}

	public static string GetName()
	{
		return playerName;
	}

	public static void SetName(string name)
	{
		playerName = name ?? "Player";
	}

	public static int GetHeadId()
	{
		return headId;
	}

	public static void SetHeadId(int id)
	{
		headId = id;
	}

	public static int GetHeadFrameId()
	{
		return headFrameId;
	}

	public static void SetHeadFrameId(int id)
	{
		headFrameId = id;
	}

	public static Sprite GetHeadSprite()
	{
		return GetHeadSprite(headId);
	}

	public static Sprite GetHeadSprite(int id)
	{
		if (headSpriteCache.TryGetValue(id, out var value))
		{
			return value;
		}
		ConfigManager config = GameApp.Config;
		HeadConfig headConfig = ((config != null) ? config.GetTable<ExampleTables>() : null)?.TbHeadConfig?.Get(id);
		if (headConfig == null || string.IsNullOrEmpty(headConfig.Icon))
		{
			headSpriteCache[id] = null;
			return null;
		}
		Sprite val = SpriteManager.LoadSprite(headConfig.Icon);
		headSpriteCache[id] = val;
		return val;
	}

	public static Sprite GetHeadFrameSprite()
	{
		return GetHeadFrameSprite(headFrameId);
	}

	public static Sprite GetHeadFrameSprite(int id)
	{
		if (headFrameSpriteCache.TryGetValue(id, out var value))
		{
			return value;
		}
		headFrameSpriteCache[id] = null;
		return null;
	}

	public static IReadOnlyList<int> GetAllHeadIds()
	{
		return DefaultHeadIds;
	}
}
