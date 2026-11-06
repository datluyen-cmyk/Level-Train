using System.Collections.Generic;
using Script.Tools;
using UnityEngine;
using XingYun.Res;

namespace Basis.Tool;

public static class ResManagerSpriteLoadAssetExtensions
{
	public class GroupAssetInfo
	{
		public string AssetLocation { get; set; }

		public int Count { get; set; }
	}

	private static Dictionary<string, Dictionary<string, int>> loadedGroupAssetRecords;

	public static Sprite LoadAsset(this ResManager resManager, string location, string group = null)
	{
		if ((Object)(object)resManager == (Object)null || string.IsNullOrEmpty(location))
		{
			return null;
		}
		if (!MultipleSpriteAtlasLoader.Enabled)
		{
			return resManager.LoadAsset<Sprite>(location, group);
		}
		if (MultipleSpriteAtlasLoader.PreferMultipleSprites && MultipleSpriteAtlasLoader.TryLoadAtLocation(location, out var sprite))
		{
			return sprite;
		}
		Sprite val = resManager.LoadAsset<Sprite>(location, group);
		if ((Object)(object)val != (Object)null || MultipleSpriteAtlasLoader.PreferMultipleSprites)
		{
			return val;
		}
		if (!MultipleSpriteAtlasLoader.TryLoadAtLocation(location, out var sprite2))
		{
			return null;
		}
		return sprite2;
	}

	public static Sprite LoadSpriteGroup(this ResManager resManager, string location, string group)
	{
		if (string.IsNullOrEmpty(location))
		{
			return null;
		}
		Sprite obj = GameApp.Res.LoadAsset<Sprite>(location, group);
		if ((Object)(object)obj != (Object)null)
		{
			RecordAsset(location, group);
		}
		return obj;
	}

	private static void RecordAsset(string location, string group)
	{
		if (loadedGroupAssetRecords == null)
		{
			loadedGroupAssetRecords = new Dictionary<string, Dictionary<string, int>>();
		}
		if (!loadedGroupAssetRecords.TryGetValue(group, out var value))
		{
			value = new Dictionary<string, int>();
			loadedGroupAssetRecords[group] = value;
		}
		if (value.TryGetValue(location, out var value2))
		{
			value[location] = value2 + 1;
		}
		else
		{
			value[location] = 1;
		}
	}

	public static void UnloadGroupAsset(this ResManager resManager, string group)
	{
		if (loadedGroupAssetRecords == null || loadedGroupAssetRecords.Count == 0 || !loadedGroupAssetRecords.TryGetValue(group, out var value))
		{
			return;
		}
		foreach (KeyValuePair<string, int> item in value)
		{
			GameApp.Res.UnloadAsset(item.Key, group, item.Value);
		}
		value.Clear();
	}
}
