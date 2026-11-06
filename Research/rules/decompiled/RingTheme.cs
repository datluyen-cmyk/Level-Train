using UnityEngine;

public static class RingTheme
{
	public static readonly Color Canvas = Hex("ffffff");

	public static readonly Color Surface = Hex("f4f6f8");

	public static readonly Color Ink = Hex("20252b");

	public static readonly Color Line = Hex("c7ced6");

	public static readonly Color Selected = Hex("2563eb");

	public static readonly Color Fixed = Hex("59636e");

	public static readonly Color Bomb = Hex("e4572e");

	public static readonly Color Released = Hex("2e9d68");

	public static readonly Color MutedInk = Hex("68737f");

	public static Color Hex(string html)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Color result = default(Color);
		ColorUtility.TryParseHtmlString("#" + html, ref result);
		return result;
	}
}
