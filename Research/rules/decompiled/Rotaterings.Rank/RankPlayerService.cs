using System;
using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Common;
using RotateRings.Model;
using Script.Tools;
using UniLog;
using UnityEngine;

namespace Rotaterings.Rank;

public static class RankPlayerService
{
	private const string Tag = "Rank";

	private static readonly Dictionary<int, Sprite> headSpriteCache = new Dictionary<int, Sprite>(32);

	private static readonly Dictionary<int, Sprite> headFrameSpriteCache = new Dictionary<int, Sprite>(8);

	private static readonly Dictionary<string, Sprite> frameSpriteNameCache = new Dictionary<string, Sprite>(4);

	public static string GetName()
	{
		string text = GameApp.Proxy.Get<PlayerModelProxy>()?.GetName();
		if (string.IsNullOrEmpty(text))
		{
			text = GameApp.Save.GetOrCreate<PlayerModelData>().PlayerName;
		}
		if (string.IsNullOrEmpty(text))
		{
			text = PlayerService.GetName();
		}
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return "You";
	}

	public static int GetHeadId()
	{
		int num = GameApp.Proxy.Get<PlayerModelProxy>()?.GetHeadId() ?? 0;
		if (num > 0)
		{
			return num;
		}
		return GameApp.Save.GetOrCreate<PlayerModelData>().HeadId;
	}

	public static int GetHeadFrameId()
	{
		int num = GameApp.Proxy.Get<PlayerModelProxy>()?.GetHeadFrameId() ?? 0;
		if (num > 0)
		{
			return num;
		}
		return GameApp.Save.GetOrCreate<PlayerModelData>().HeadFrameId;
	}

	public static void GetHeadSpriteAsync(int headId, Action<Sprite> onLoaded)
	{
		using (new ProfilerScope("RankPlayerService.GetHeadSpriteAsync"))
		{
			int resolvedId = ((headId <= 0) ? GetHeadId() : headId);
			if (resolvedId <= 0)
			{
				onLoaded?.Invoke(null);
				return;
			}
			if (headSpriteCache.TryGetValue(resolvedId, out var value))
			{
				onLoaded?.Invoke(value);
				return;
			}
			HeadConfig orDefault = GameApp.Config.GetConfig<ExampleTables, TbHeadConfig>((Func<ExampleTables, TbHeadConfig>)((ExampleTables tb) => tb.TbHeadConfig)).GetOrDefault(resolvedId);
			if (orDefault == null || string.IsNullOrEmpty(orDefault.Icon))
			{
				Log.LogError("Rank", "HeadConfig id={0} 不存在或 Icon 为空", new object[1] { resolvedId });
				onLoaded?.Invoke(null);
				return;
			}
			SpriteManager.LoadSpriteAsync(orDefault.Icon, delegate(Sprite sprite)
			{
				if ((Object)(object)sprite != (Object)null)
				{
					headSpriteCache[resolvedId] = sprite;
				}
				onLoaded?.Invoke(sprite);
			});
		}
	}

	public static void GetHeadFrameSpriteAsync(int headFrameId, Action<Sprite> onLoaded)
	{
		int resolvedId = ((headFrameId <= 0) ? GetHeadFrameId() : headFrameId);
		if (resolvedId <= 0)
		{
			onLoaded?.Invoke(null);
			return;
		}
		if (headFrameSpriteCache.TryGetValue(resolvedId, out var value))
		{
			onLoaded?.Invoke(value);
			return;
		}
		HeadConfig orDefault = GameApp.Config.GetConfig<ExampleTables, TbHeadConfig>((Func<ExampleTables, TbHeadConfig>)((ExampleTables tb) => tb.TbHeadConfig)).GetOrDefault(resolvedId);
		if (orDefault == null || string.IsNullOrEmpty(orDefault.Frame))
		{
			Log.LogError("Rank", "HeadConfig id={0} 不存在或 Frame 为空", new object[1] { resolvedId });
			onLoaded?.Invoke(null);
			return;
		}
		SpriteManager.LoadSpriteAsync(orDefault.Frame, delegate(Sprite sprite)
		{
			if ((Object)(object)sprite != (Object)null)
			{
				headFrameSpriteCache[resolvedId] = sprite;
			}
			onLoaded?.Invoke(sprite);
		});
	}

	public static void GetFrameSpriteByNameAsync(string spriteName, Action<Sprite> onLoaded)
	{
		if (string.IsNullOrEmpty(spriteName))
		{
			onLoaded?.Invoke(null);
			return;
		}
		if (frameSpriteNameCache.TryGetValue(spriteName, out var value))
		{
			onLoaded?.Invoke(value);
			return;
		}
		SpriteManager.LoadSpriteAsync(spriteName, delegate(Sprite sprite)
		{
			if ((Object)(object)sprite != (Object)null)
			{
				frameSpriteNameCache[spriteName] = sprite;
			}
			onLoaded?.Invoke(sprite);
		});
	}
}
