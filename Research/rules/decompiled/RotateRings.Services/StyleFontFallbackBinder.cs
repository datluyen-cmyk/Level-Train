using System;
using System.Collections.Generic;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using XingYun.Aot;
using XingYun.Font;
using XingYun.Utility;

namespace RotateRings.Services;

public static class StyleFontFallbackBinder
{
	private const string Tag = "[StyleFontFallbackBinder]";

	private static readonly Dictionary<string, TMP_FontAsset> sdfCache = new Dictionary<string, TMP_FontAsset>(16);

	private static readonly Dictionary<string, TMP_FontAsset> dynamicCache = new Dictionary<string, TMP_FontAsset>(16);

	public static void ReleaseHotDynamicFallbacks()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Invalid comparison between Unknown and I4
		if (dynamicCache.Count == 0)
		{
			return;
		}
		HashSet<TMP_FontAsset> hashSet = new HashSet<TMP_FontAsset>();
		foreach (TMP_FontAsset value in dynamicCache.Values)
		{
			if ((Object)(object)value != (Object)null)
			{
				hashSet.Add(value);
			}
		}
		dynamicCache.Clear();
		DetachFallbacksFromManagedMainAssets(hashSet);
		int num = 0;
		foreach (TMP_FontAsset item in hashSet)
		{
			if (!((Object)(object)item == (Object)null) && (int)item.atlasPopulationMode == 1)
			{
				Object.Destroy((Object)(object)item);
				num++;
			}
		}
	}

	public static TMP_FontAsset LoadSdfFont(string fontName)
	{
		if (string.IsNullOrEmpty(fontName))
		{
			return null;
		}
		if (sdfCache.TryGetValue(fontName, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		string defaultResPath = Res.GetDefaultResPath(fontName);
		TMP_FontAsset val = GameApp.Res.LoadAsset<TMP_FontAsset>(defaultResPath, "Font");
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		sdfCache[fontName] = val;
		return val;
	}

	public static bool MatchesMainSdfAsset(string resName, string mainAssetName)
	{
		if (string.IsNullOrEmpty(resName) || string.IsNullOrEmpty(mainAssetName))
		{
			return false;
		}
		if (resName == mainAssetName)
		{
			return true;
		}
		return resName == GetSdfAssetStem(mainAssetName);
	}

	public static bool TryGetLanguageFontName(FontStyleType style, string lang, out string fontName)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		fontName = null;
		if (string.IsNullOrEmpty(lang) || Define.LanguageFontList == null)
		{
			return false;
		}
		for (int i = 0; i < Define.LanguageFontList.Count; i++)
		{
			LanguageFontPair val = Define.LanguageFontList[i];
			if (val == null || val.LanguageCode != lang || val.FontStyleList == null)
			{
				continue;
			}
			for (int j = 0; j < val.FontStyleList.Count; j++)
			{
				FontStylePair val2 = val.FontStyleList[j];
				if (val2.Style == style && !string.IsNullOrEmpty(val2.FontName))
				{
					fontName = val2.FontName;
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public static List<string> BuildStandardFallbackList(FontStyleType style, string lang, string mainAssetName)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = new List<string>();
		if (TryGetLanguageFontName(style, lang, out var fontName))
		{
			if (!MatchesMainSdfAsset(fontName, mainAssetName))
			{
				list.Add(fontName);
			}
		}
		else
		{
			string defaultFallbackFont = FontRegistry.DefaultFallbackFont;
			if (!string.IsNullOrEmpty(defaultFallbackFont) && !MatchesMainSdfAsset(defaultFallbackFont, mainAssetName))
			{
				list.Add(defaultFallbackFont);
			}
		}
		return list;
	}

	public static List<string> BuildAllLanguageFallbackList(FontStyleType style, string mainAssetName)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		List<string> allFonts = FontRegistry.GetAllFonts(style);
		allFonts.RemoveAll((string name) => MatchesMainSdfAsset(name, mainAssetName));
		return allFonts;
	}

	private static string GetSdfAssetStem(string tmpAssetName)
	{
		if (tmpAssetName.EndsWith(" SDF Effect", StringComparison.Ordinal))
		{
			return tmpAssetName.Substring(0, tmpAssetName.Length - " SDF Effect".Length);
		}
		if (tmpAssetName.EndsWith(" SDF", StringComparison.Ordinal))
		{
			return tmpAssetName.Substring(0, tmpAssetName.Length - " SDF".Length);
		}
		return tmpAssetName;
	}

	public static void ApplyFallbackChain(TMP_FontAsset mainAsset, List<string> fallbackList)
	{
		if ((Object)(object)mainAsset == (Object)null)
		{
			return;
		}
		ClearDynamicData(mainAsset);
		if (mainAsset.fallbackFontAssetTable == null)
		{
			List<TMP_FontAsset> list = (mainAsset.fallbackFontAssetTable = new List<TMP_FontAsset>());
		}
		ClearFallbackTable(mainAsset);
		if (fallbackList == null || fallbackList.Count == 0)
		{
			TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, (Object)(object)mainAsset);
			return;
		}
		int num = 0;
		for (int i = 0; i < fallbackList.Count; i++)
		{
			TMP_FontAsset val = CreateFallbackFromTtf(fallbackList[i], mainAsset);
			if (!((Object)(object)val == (Object)null))
			{
				ClearDynamicData(val);
				if (!mainAsset.fallbackFontAssetTable.Contains(val))
				{
					mainAsset.fallbackFontAssetTable.Add(val);
					num++;
				}
			}
		}
		TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, (Object)(object)mainAsset);
	}

	private static void ClearFallbackTable(TMP_FontAsset mainAsset)
	{
		if (((mainAsset != null) ? mainAsset.fallbackFontAssetTable : null) == null)
		{
			return;
		}
		List<TMP_FontAsset> fallbackFontAssetTable = mainAsset.fallbackFontAssetTable;
		for (int num = fallbackFontAssetTable.Count - 1; num >= 0; num--)
		{
			TMP_FontAsset val = fallbackFontAssetTable[num];
			if ((Object)(object)val == (Object)null || !Define.NoClearFallbackName.Contains(((Object)val).name))
			{
				fallbackFontAssetTable.RemoveAt(num);
			}
		}
	}

	private static TMP_FontAsset CreateFallbackFromTtf(string fontName, TMP_FontAsset mainAsset)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(fontName))
		{
			return null;
		}
		if ((Object)(object)mainAsset == (Object)null)
		{
			return null;
		}
		int atlasPadding = mainAsset.atlasPadding;
		FaceInfo faceInfo = ((TMP_Asset)mainAsset).faceInfo;
		int num = Mathf.RoundToInt(((FaceInfo)(ref faceInfo)).pointSize);
		string key = $"{fontName}_{atlasPadding}_{num}";
		if (dynamicCache.TryGetValue(key, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		string defaultResPath = Res.GetDefaultResPath(fontName);
		Font val = GameApp.Res.LoadAsset<Font>(defaultResPath, "Font");
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		int num2 = ((mainAsset.atlasWidth > 0) ? mainAsset.atlasWidth : 2048);
		int num3 = ((mainAsset.atlasHeight > 0) ? mainAsset.atlasHeight : 2048);
		TMP_FontAsset val2 = TMP_FontAsset.CreateFontAsset(val, num, atlasPadding, (GlyphRenderMode)4165, num2, num3, (AtlasPopulationMode)1, true);
		if ((Object)(object)val2 == (Object)null)
		{
			Log.LogError("[StyleFontFallbackBinder]", "从 TTF 创建 FontAsset 失败: {0}", new object[1] { fontName });
			return null;
		}
		((Object)val2).name = $"{fontName} Dynamic P{atlasPadding}";
		val2.isMultiAtlasTexturesEnabled = true;
		dynamicCache[key] = val2;
		((Object)val2.atlasTexture).name = ((Object)val2).name + "_Atlas";
		return val2;
	}

	private static void ClearDynamicData(TMP_FontAsset fontAsset)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if (!((Object)(object)fontAsset == (Object)null) && (int)fontAsset.atlasPopulationMode == 1)
		{
			fontAsset.ClearFontAssetData(true);
		}
	}

	private static void DetachFallbacksFromManagedMainAssets(HashSet<TMP_FontAsset> toRelease)
	{
		if (toRelease != null && toRelease.Count != 0)
		{
			HashSet<string> seenResNames = new HashSet<string>();
			TryDetachFromFontList(Define.StyleFonts, seenResNames, toRelease);
			ExtraStyleFontConfig instance = ExtraStyleFontConfig.Instance;
			if (instance?.extraStyleFonts != null)
			{
				TryDetachFromFontList(instance.extraStyleFonts, seenResNames, toRelease);
			}
		}
	}

	private static void TryDetachFromFontList(List<FontStylePair> pairs, HashSet<string> seenResNames, HashSet<TMP_FontAsset> toRelease)
	{
		if (pairs == null)
		{
			return;
		}
		for (int i = 0; i < pairs.Count; i++)
		{
			string fontName = pairs[i].FontName;
			if (string.IsNullOrEmpty(fontName) || !seenResNames.Add(fontName))
			{
				continue;
			}
			TMP_FontAsset val = LoadSdfFont(fontName);
			if (((val != null) ? val.fallbackFontAssetTable : null) == null)
			{
				continue;
			}
			List<TMP_FontAsset> fallbackFontAssetTable = val.fallbackFontAssetTable;
			bool flag = false;
			for (int num = fallbackFontAssetTable.Count - 1; num >= 0; num--)
			{
				if ((Object)(object)fallbackFontAssetTable[num] != (Object)null && toRelease.Contains(fallbackFontAssetTable[num]))
				{
					fallbackFontAssetTable.RemoveAt(num);
					flag = true;
				}
			}
			if (flag)
			{
				TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, (Object)(object)val);
			}
		}
	}
}
