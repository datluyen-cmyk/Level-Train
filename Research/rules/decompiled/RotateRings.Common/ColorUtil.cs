using UnityEngine;

namespace RotateRings.Common;

public static class ColorUtil
{
	public static bool TryParseHtmlColor(string colorHex, out Color color)
	{
		return ColorUtility.TryParseHtmlString(colorHex, ref color);
	}

	public static Color ParseHtmlColorOrDefault(string colorHex)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (!TryParseHtmlColor(colorHex, out var color))
		{
			return Color.white;
		}
		return color;
	}
}
