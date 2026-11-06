using System;
using Basis.Tool;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Aot;
using XingYun.Res;
using YooAsset;

namespace Script.Tools;

public static class SpriteManager
{
	public static SpriteCacher CardIconSpriteCacher { get; } = new SpriteCacher();

	public static SpriteCacher CardBagSpriteCacher { get; } = new SpriteCacher();

	public static bool HasAsset(this ResManager resManager, string path)
	{
		return resManager.HasAsset(path, Define.DefaultPackage);
	}

	public static bool HasAssetInLevelImagePackage(this ResManager resManager, string path)
	{
		return resManager.HasAsset(path);
	}

	public static bool HasAsset(this ResManager resManager, string path, string packageName = null)
	{
		if (string.IsNullOrEmpty(packageName))
		{
			packageName = Define.DefaultPackage;
		}
		ResourcePackage package = resManager.GetPackage(path, packageName);
		if (package == null)
		{
			return false;
		}
		return package.CheckLocationValid(path);
	}

	public static void SetAtlasSprite(this Image img, string spriteName)
	{
		if (!string.IsNullOrEmpty(spriteName))
		{
			Sprite val = LoadSprite(spriteName);
			if ((Object)(object)val != (Object)null)
			{
				img.sprite = val;
			}
		}
	}

	public static void SetAtlasSprite(this Image img, string atlasName, string spriteName)
	{
		if (!string.IsNullOrEmpty(spriteName))
		{
			Sprite val = LoadSprite(spriteName);
			if ((Object)(object)val != (Object)null)
			{
				img.sprite = val;
			}
		}
	}

	public static void SetLarge(this Image img, string spriteName)
	{
		Sprite val = LoadSprite(spriteName);
		if ((Object)(object)val != (Object)null)
		{
			img.sprite = val;
		}
	}

	public static Sprite LoadIconSprite(string spriteName, string group = null)
	{
		return GameApp.Res.LoadSpriteGroup(spriteName, group);
	}

	public static Sprite LoadSprite(string spriteName, string group = null)
	{
		if (string.IsNullOrEmpty(spriteName))
		{
			return null;
		}
		if (TryGetSkinSpriteName(spriteName, out var finalSpriteName))
		{
			return GameApp.Res.LoadAsset<Sprite>(finalSpriteName, group);
		}
		return null;
	}

	public static void LoadSpriteAsync(string spriteName, Action<Sprite> onLoaded, string group = null)
	{
		string finalSpriteName;
		if (string.IsNullOrEmpty(spriteName))
		{
			onLoaded?.Invoke(null);
		}
		else if (TryGetSkinSpriteName(spriteName, out finalSpriteName))
		{
			GameApp.Res.LoadAssetAsync<Sprite>(finalSpriteName, onLoaded, group);
		}
		else
		{
			onLoaded?.Invoke(null);
		}
	}

	public static bool TryGetSkinSpriteName(string spriteName, out string finalSpriteName)
	{
		finalSpriteName = "";
		if (string.IsNullOrEmpty(spriteName))
		{
			return false;
		}
		finalSpriteName = spriteName;
		if (finalSpriteName == null)
		{
			return false;
		}
		return true;
	}

	public static bool HasSprite(string spriteName)
	{
		string finalSpriteName;
		return TryGetSkinSpriteName(spriteName, out finalSpriteName);
	}
}
