using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using XingYun.Aot;
using YooAsset;

namespace Script.Tools;

public static class MultipleSpriteAtlasLoader
{
	private readonly struct SpriteAtlasPathMapping
	{
		public string Style { get; }

		public string Atlas { get; }

		public string Path { get; }

		public SpriteAtlasPathMapping(string style, string atlas, string path)
		{
			Style = style;
			Atlas = atlas;
			Path = path;
		}
	}

	private const string LogTag = "MultipleSpriteAtlasLoader";

	private const string DefaultStyleName = "DefaultStyle";

	private const string AtlasRoot = "Assets/02_Res/Art/ImagePathMap";

	private const string SpriteAtlasMapAddress = "ImagePathMap_SpriteAtlasPathMap";

	private const string SpriteAtlasMapAssetPath = "Assets/02_Res/Art/ImagePathMap/SpriteAtlasPathMap.json";

	private static readonly Dictionary<string, SubAssetsOperationHandle> atlasHandleCache = new Dictionary<string, SubAssetsOperationHandle>();

	private static readonly Dictionary<string, List<SpriteAtlasPathMapping>> spriteAtlasPathMap = new Dictionary<string, List<SpriteAtlasPathMapping>>(StringComparer.OrdinalIgnoreCase);

	private static AssetOperationHandle spriteAtlasMapHandle;

	private static bool hasTriedLoadSpriteAtlasPathMap;

	private static bool hasLoggedYooAssetsDefaultPackageNull;

	public static bool Enabled { get; set; } = true;

	public static bool PreferMultipleSprites { get; set; } = true;

	public static bool TryLoadSprite(string atlasName, string spriteName, out Sprite sprite)
	{
		sprite = null;
		if (!Enabled || string.IsNullOrEmpty(spriteName))
		{
			return false;
		}
		string currentStyleName = GetCurrentStyleName();
		if (TryLoadSpriteFromMap(currentStyleName, atlasName, spriteName, out sprite))
		{
			return true;
		}
		if (currentStyleName != "DefaultStyle" && TryLoadSpriteFromMap("DefaultStyle", atlasName, spriteName, out sprite))
		{
			return true;
		}
		if (string.IsNullOrEmpty(atlasName))
		{
			return false;
		}
		if (TryLoadSpriteFromStyle(currentStyleName, atlasName, spriteName, out sprite))
		{
			return true;
		}
		if (currentStyleName != "DefaultStyle")
		{
			return TryLoadSpriteFromStyle("DefaultStyle", atlasName, spriteName, out sprite);
		}
		return false;
	}

	public static bool TryLoadSprite(string spriteName, out Sprite sprite)
	{
		sprite = null;
		if (!Enabled || string.IsNullOrEmpty(spriteName))
		{
			return false;
		}
		string currentStyleName = GetCurrentStyleName();
		if (TryLoadSpriteFromMap(currentStyleName, null, spriteName, out sprite))
		{
			return true;
		}
		if (currentStyleName != "DefaultStyle")
		{
			return TryLoadSpriteFromMap("DefaultStyle", null, spriteName, out sprite);
		}
		return false;
	}

	public static bool TryLoadAtLocation(string location, out Sprite sprite)
	{
		sprite = null;
		if (!Enabled || string.IsNullOrEmpty(location))
		{
			return false;
		}
		if (location.IndexOf('/') < 0 && location.IndexOf('\\') < 0)
		{
			if (TryLoadSprite(location, out sprite))
			{
				return true;
			}
			if (!string.IsNullOrEmpty(location))
			{
				return TryLoadSpriteFromAtlasPath(location, location, out sprite);
			}
			return false;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(location);
		if (!string.IsNullOrEmpty(fileNameWithoutExtension) && TryLoadSprite(fileNameWithoutExtension, out sprite))
		{
			return true;
		}
		return TryLoadSpriteFromAtlasPath(location, fileNameWithoutExtension, out sprite);
	}

	public static void ReleaseAll()
	{
		foreach (SubAssetsOperationHandle value in atlasHandleCache.Values)
		{
			if (value != null)
			{
				value.Release();
			}
		}
		atlasHandleCache.Clear();
		AssetOperationHandle obj = spriteAtlasMapHandle;
		if (obj != null)
		{
			obj.Release();
		}
		spriteAtlasMapHandle = null;
		hasTriedLoadSpriteAtlasPathMap = false;
		hasLoggedYooAssetsDefaultPackageNull = false;
		spriteAtlasPathMap.Clear();
	}

	public static void ReleaseLoadedAtlases()
	{
		foreach (SubAssetsOperationHandle value in atlasHandleCache.Values)
		{
			if (value != null)
			{
				value.Release();
			}
		}
		atlasHandleCache.Clear();
	}

	public static void ReleaseAtlas(string atlasName)
	{
		if (!string.IsNullOrEmpty(atlasName))
		{
			ReleaseAtlasByPath(BuildAtlasPath(GetCurrentStyleName(), atlasName));
			ReleaseAtlasByPath(BuildAtlasPath("DefaultStyle", atlasName));
		}
	}

	private static bool TryLoadSpriteFromStyle(string styleName, string atlasName, string spriteName, out Sprite sprite)
	{
		sprite = null;
		return TryLoadSpriteFromAtlasPath(BuildAtlasPath(styleName, atlasName), spriteName, out sprite);
	}

	private static bool TryLoadSpriteFromMap(string styleName, string atlasName, string spriteName, out Sprite sprite)
	{
		sprite = null;
		if (!TryGetAtlasPathsFromMap(styleName, atlasName, spriteName, out var atlasPaths))
		{
			return false;
		}
		for (int i = 0; i < atlasPaths.Count; i++)
		{
			if (TryLoadSpriteFromAtlasPath(atlasPaths[i], spriteName, out sprite))
			{
				return true;
			}
		}
		return false;
	}

	private static bool TryLoadSpriteFromAtlasPath(string atlasPath, string spriteName, out Sprite sprite)
	{
		sprite = null;
		SubAssetsOperationHandle orLoadAtlasHandle = GetOrLoadAtlasHandle(atlasPath);
		if (orLoadAtlasHandle == null)
		{
			return false;
		}
		sprite = orLoadAtlasHandle.GetSubAssetObject<Sprite>(spriteName);
		return (Object)(object)sprite != (Object)null;
	}

	private static bool TryGetAtlasPathsFromMap(string styleName, string atlasName, string spriteName, out List<string> atlasPaths)
	{
		atlasPaths = null;
		EnsureSpriteAtlasPathMap();
		if (!spriteAtlasPathMap.TryGetValue(spriteName, out var value))
		{
			return false;
		}
		for (int i = 0; i < value.Count; i++)
		{
			SpriteAtlasPathMapping spriteAtlasPathMapping = value[i];
			string a = spriteAtlasPathMapping.Style;
			if (string.IsNullOrEmpty(spriteAtlasPathMapping.Style))
			{
				a = GetCurrentStyleName();
			}
			if (string.Equals(a, styleName, StringComparison.OrdinalIgnoreCase) && (string.IsNullOrEmpty(atlasName) || string.Equals(spriteAtlasPathMapping.Atlas, atlasName, StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrEmpty(spriteAtlasPathMapping.Path))
			{
				if (atlasPaths == null)
				{
					atlasPaths = new List<string>();
				}
				atlasPaths.Add(spriteAtlasPathMapping.Path);
			}
		}
		if (atlasPaths != null)
		{
			return atlasPaths.Count > 0;
		}
		return false;
	}

	private static ResourcePackage GetLevelImagePackage()
	{
		return GameApp.Res.GetPackage((string)null, Define.DefaultPackage);
	}

	private static ResourcePackage GetSpriteAtlasMapPackage()
	{
		return GetLevelImagePackage();
	}

	private static bool TryLoadSpriteAtlasMapJsonAtLocation(ResourcePackage package, string location, out string jsonText)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		jsonText = null;
		if (package == null || string.IsNullOrEmpty(location))
		{
			return false;
		}
		AssetOperationHandle obj = spriteAtlasMapHandle;
		if (obj != null)
		{
			obj.Release();
		}
		spriteAtlasMapHandle = null;
		AssetOperationHandle val = package.LoadAssetSync<TextAsset>(location);
		if (val == null || (int)((OperationHandleBase)val).Status != 2)
		{
			if (val != null)
			{
				val.Release();
			}
			return false;
		}
		Object assetObject = val.AssetObject;
		TextAsset val2 = (TextAsset)(object)((assetObject is TextAsset) ? assetObject : null);
		if ((Object)(object)val2 == (Object)null || string.IsNullOrEmpty(val2.text))
		{
			val.Release();
			return false;
		}
		spriteAtlasMapHandle = val;
		jsonText = val2.text;
		return true;
	}

	private static bool TryLoadSpriteAtlasMapJson(ResourcePackage package, out string jsonText)
	{
		jsonText = null;
		if (package == null)
		{
			return false;
		}
		if (TryLoadSpriteAtlasMapJsonAtLocation(package, "ImagePathMap_SpriteAtlasPathMap", out jsonText) || TryLoadSpriteAtlasMapJsonAtLocation(package, "Assets/02_Res/Art/ImagePathMap/SpriteAtlasPathMap.json", out jsonText))
		{
			return true;
		}
		return false;
	}

	private static void EnsureSpriteAtlasPathMap()
	{
		if (hasTriedLoadSpriteAtlasPathMap)
		{
			return;
		}
		ResourcePackage spriteAtlasMapPackage = GetSpriteAtlasMapPackage();
		if (spriteAtlasMapPackage == null)
		{
			if (!hasLoggedYooAssetsDefaultPackageNull)
			{
				hasLoggedYooAssetsDefaultPackageNull = true;
			}
			return;
		}
		string jsonText = null;
		if (!TryLoadSpriteAtlasMapJson(spriteAtlasMapPackage, out jsonText))
		{
			return;
		}
		hasTriedLoadSpriteAtlasPathMap = true;
		try
		{
			ParseSpriteAtlasPathMap(jsonText);
		}
		catch (Exception)
		{
			spriteAtlasPathMap.Clear();
			hasTriedLoadSpriteAtlasPathMap = false;
		}
	}

	private static void ParseSpriteAtlasPathMap(string json)
	{
		spriteAtlasPathMap.Clear();
		JToken obj = JObject.Parse(json)["sprites"];
		JObject val = (JObject)(object)((obj is JObject) ? obj : null);
		if (val == null)
		{
			return;
		}
		foreach (JProperty item in val.Properties())
		{
			JToken value = item.Value;
			JArray val2 = (JArray)(object)((value is JArray) ? value : null);
			if (val2 == null)
			{
				continue;
			}
			List<SpriteAtlasPathMapping> list = new List<SpriteAtlasPathMapping>(((JContainer)val2).Count);
			for (int i = 0; i < ((JContainer)val2).Count; i++)
			{
				JToken obj2 = val2[i];
				JObject val3 = (JObject)(object)((obj2 is JObject) ? obj2 : null);
				if (val3 != null)
				{
					JToken obj3 = val3["path"];
					string text = ((obj3 != null) ? Extensions.Value<string>((IEnumerable<JToken>)obj3) : null);
					if (!string.IsNullOrEmpty(text))
					{
						JToken obj4 = val3["style"];
						string style = ((obj4 != null) ? Extensions.Value<string>((IEnumerable<JToken>)obj4) : null) ?? string.Empty;
						JToken obj5 = val3["atlas"];
						list.Add(new SpriteAtlasPathMapping(style, ((obj5 != null) ? Extensions.Value<string>((IEnumerable<JToken>)obj5) : null) ?? string.Empty, text));
					}
				}
			}
			if (list.Count > 0)
			{
				spriteAtlasPathMap[item.Name] = list;
			}
		}
	}

	private static SubAssetsOperationHandle GetOrLoadAtlasHandle(string atlasPath)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		if (atlasHandleCache.TryGetValue(atlasPath, out var value))
		{
			if (value != null && ((OperationHandleBase)value).IsValid && (int)((OperationHandleBase)value).Status == 2)
			{
				return value;
			}
			if (value != null)
			{
				value.Release();
			}
			atlasHandleCache.Remove(atlasPath);
		}
		ResourcePackage levelImagePackage = GetLevelImagePackage();
		if (levelImagePackage == null)
		{
			if (!hasLoggedYooAssetsDefaultPackageNull)
			{
				hasLoggedYooAssetsDefaultPackageNull = true;
			}
			return null;
		}
		SubAssetsOperationHandle val = levelImagePackage.LoadSubAssetsSync<Sprite>(atlasPath);
		if (val == null || (int)((OperationHandleBase)val).Status != 2)
		{
			if (val != null)
			{
				val.Release();
			}
			return null;
		}
		atlasHandleCache[atlasPath] = val;
		return val;
	}

	private static string GetCurrentStyleName()
	{
		return "DefaultStyle";
	}

	private static string BuildAtlasPath(string styleName, string atlasName)
	{
		return "Assets/02_Res/Art/ImagePathMap/" + styleName + "/" + atlasName + ".png";
	}

	private static void ReleaseAtlasByPath(string atlasPath)
	{
		if (atlasHandleCache.TryGetValue(atlasPath, out var value))
		{
			if (value != null)
			{
				value.Release();
			}
			atlasHandleCache.Remove(atlasPath);
		}
	}
}
