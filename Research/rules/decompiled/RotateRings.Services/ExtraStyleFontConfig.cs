using System;
using System.Collections.Generic;
using UnityEngine;
using XingYun.Aot;
using XingYun.Utility;

namespace RotateRings.Services;

[CreateAssetMenu(fileName = "ExtraStyleFontConfig", menuName = "Config/ExtraStyleFontConfig")]
public class ExtraStyleFontConfig : ScriptableObject
{
	private const string Tag = "[ExtraStyleFontConfig]";

	private static ExtraStyleFontConfig instance;

	[Header("额外变体字体（需独立 fallback 的 SDF 字体）")]
	public List<FontStylePair> extraStyleFonts = new List<FontStylePair>();

	public static ExtraStyleFontConfig Instance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)instance))
			{
				try
				{
					instance = GameApp.Res.LoadAsset<ExtraStyleFontConfig>(Res.GetDefaultResPath("ExtraStyleFontConfig"), (string)null);
				}
				catch (Exception)
				{
				}
			}
			return instance;
		}
	}
}
