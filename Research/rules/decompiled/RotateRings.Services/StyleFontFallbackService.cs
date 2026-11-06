using System;
using System.Collections.Generic;
using RotateRings.Model;
using TMPro;
using UnityEngine;
using XingYun.Aot;
using XingYun.Core.Singletons;
using XingYun.Font;

namespace RotateRings.Services;

public static class StyleFontFallbackService
{
	private const string Tag = "[StyleFontFallback]";

	public static void RebuildWithPlayerName(string currentLang)
	{
		string playerNameLang = GameApp.Proxy.Get<PlayerModelProxy>()?.GetPlayerNameLanguage();
		RebuildAllManagedFonts((FontStyleType style, string _, string mainName) => BuildFallbackListWithPlayerName(style, currentLang, playerNameLang, mainName));
		Singleton<FontManager>.Instance.ApplyToAllTexts();
	}

	public static void ReleaseLanguagePreviewDynamicFallbacks()
	{
		StyleFontFallbackBinder.ReleaseHotDynamicFallbacks();
	}

	public static void RebuildExtrasWithAllLanguages()
	{
		RebuildAllManagedFonts((FontStyleType style, string _, string mainName) => StyleFontFallbackBinder.BuildAllLanguageFallbackList(style, mainName));
	}

	private static IEnumerable<FontStylePair> EnumerateManagedFonts()
	{
		HashSet<string> seen = new HashSet<string>();
		if (Define.StyleFonts != null)
		{
			for (int i = 0; i < Define.StyleFonts.Count; i++)
			{
				FontStylePair val = Define.StyleFonts[i];
				if (!string.IsNullOrEmpty(val.FontName) && seen.Add(val.FontName))
				{
					yield return val;
				}
			}
		}
		ExtraStyleFontConfig config = ExtraStyleFontConfig.Instance;
		if (config?.extraStyleFonts == null)
		{
			yield break;
		}
		for (int i = 0; i < config.extraStyleFonts.Count; i++)
		{
			FontStylePair val2 = config.extraStyleFonts[i];
			if (!string.IsNullOrEmpty(val2.FontName) && seen.Add(val2.FontName))
			{
				yield return val2;
			}
		}
	}

	private static int RebuildAllManagedFonts(Func<FontStyleType, string, string, List<string>> buildFallbackList)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (FontStylePair item in EnumerateManagedFonts())
		{
			TMP_FontAsset val = StyleFontFallbackBinder.LoadSdfFont(item.FontName);
			if (!((Object)(object)val == (Object)null))
			{
				val.isMultiAtlasTexturesEnabled = true;
				List<string> fallbackList = buildFallbackList(item.Style, item.FontName, ((Object)val).name);
				StyleFontFallbackBinder.ApplyFallbackChain(val, fallbackList);
				num++;
			}
		}
		return num;
	}

	private static List<string> BuildFallbackListWithPlayerName(FontStyleType style, string currentLang, string playerNameLang, string mainAssetName)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = StyleFontFallbackBinder.BuildStandardFallbackList(style, currentLang, mainAssetName);
		if (string.IsNullOrEmpty(playerNameLang) || playerNameLang == currentLang)
		{
			return list;
		}
		if (!StyleFontFallbackBinder.TryGetLanguageFontName(style, playerNameLang, out var fontName))
		{
			fontName = FontRegistry.GetFont(style, playerNameLang);
		}
		if (string.IsNullOrEmpty(fontName) || StyleFontFallbackBinder.MatchesMainSdfAsset(fontName, mainAssetName) || list.Contains(fontName))
		{
			return list;
		}
		StyleFontFallbackBinder.TryGetLanguageFontName(style, currentLang, out var fontName2);
		if (!string.IsNullOrEmpty(fontName2) && !StyleFontFallbackBinder.MatchesMainSdfAsset(fontName2, mainAssetName) && list.Contains(fontName2))
		{
			int num = list.IndexOf(fontName2);
			list.Insert(num + 1, fontName);
		}
		else
		{
			list.Insert(0, fontName);
		}
		return list;
	}
}
