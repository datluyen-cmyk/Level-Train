using UnityEngine;

public static class PartPalette
{
	public const int RingColorCount = 7;

	public const int LevelSpecifiedColorCount = 8;

	private static readonly Color[] RingColors = (Color[])(object)new Color[7]
	{
		RingTheme.Hex("32AFDD"),
		RingTheme.Hex("EC8F2E"),
		RingTheme.Hex("984BE0"),
		RingTheme.Hex("DD433E"),
		RingTheme.Hex("6AC53E"),
		RingTheme.Hex("4E77DD"),
		RingTheme.Hex("23A468")
	};

	private static readonly Color[] LevelSpecifiedRingColors = (Color[])(object)new Color[8]
	{
		RingTheme.Hex("32AFDD"),
		RingTheme.Hex("EC8F2E"),
		RingTheme.Hex("984BE0"),
		RingTheme.Hex("DD433E"),
		RingTheme.Hex("6AC53E"),
		RingTheme.Hex("4E77DD"),
		RingTheme.Hex("23A468"),
		RingTheme.Hex("646464")
	};

	public static PartPaletteColor FromPaletteIndex(int paletteIndex)
	{
		int num = paletteIndex % 7;
		if (num < 0)
		{
			num += 7;
		}
		return (PartPaletteColor)(num + 1);
	}

	public static int ToPaletteIndex(PartPaletteColor color)
	{
		if (color == PartPaletteColor.None)
		{
			return -1;
		}
		return (int)(color - 1);
	}

	public static Color For(PartPaletteColor color)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		int num = ToPaletteIndex(color);
		if (num < 0 || num >= RingColors.Length)
		{
			return Color.white;
		}
		return RingColors[num];
	}

	public static Color For(int authoredOrder, int typeId = 1)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (typeId == 10)
		{
			return RingTheme.Hex("0f766e");
		}
		return RingColors[authoredOrder % RingColors.Length];
	}

	public static bool IsValidLevelSpecifiedId(int colorId)
	{
		if (colorId >= 0)
		{
			return colorId < 8;
		}
		return false;
	}

	public static bool TryToLevelSpecifiedColor(int colorId, out PartLevelSpecifiedColor color)
	{
		if (!IsValidLevelSpecifiedId(colorId))
		{
			color = PartLevelSpecifiedColor.LightBlue;
			return false;
		}
		color = (PartLevelSpecifiedColor)colorId;
		return true;
	}

	public static int ToLevelSpecifiedId(PartLevelSpecifiedColor color)
	{
		return (int)color;
	}

	public static Color ForLevelSpecified(int colorId)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (!IsValidLevelSpecifiedId(colorId))
		{
			return Color.white;
		}
		return LevelSpecifiedRingColors[colorId];
	}

	public static Color ForLevelSpecified(PartLevelSpecifiedColor color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ForLevelSpecified((int)color);
	}
}
