using Example.Cfg;
using UnityEngine;

namespace RotateRings.GamePlay.PartStyle.ColorProvider;

public sealed class ConfigRingColorProvider : IColorProvider
{
	private const float DefaultSaturation = 1f;

	private readonly Color[] colors;

	private readonly float saturation;

	public ConfigRingColorProvider(RingColorConfig cfg)
	{
		colors = BuildColors(cfg);
		saturation = ((cfg != null && cfg.Saturation > 0f) ? cfg.Saturation : 1f);
	}

	public Color[] GetColors()
	{
		return colors;
	}

	public float GetSaturation()
	{
		return saturation;
	}

	private static Color[] BuildColors(RingColorConfig cfg)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (cfg == null)
		{
			return null;
		}
		return (Color[])(object)new Color[7]
		{
			ParseColor(cfg.Color1),
			ParseColor(cfg.Color2),
			ParseColor(cfg.Color3),
			ParseColor(cfg.Color4),
			ParseColor(cfg.Color5),
			ParseColor(cfg.Color6),
			ParseColor(cfg.Color7)
		};
	}

	private static Color ParseColor(string hex)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(hex))
		{
			return Color.clear;
		}
		return RingTheme.Hex(hex);
	}
}
