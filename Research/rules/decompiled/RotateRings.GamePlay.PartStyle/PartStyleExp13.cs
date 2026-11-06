using RotateRings.Model;

namespace RotateRings.GamePlay.PartStyle;

public static class PartStyleExp13
{
	private const int MainPrefixEndLevel = 19;

	public static PartStyleType Resolve(in PartStyleResolveContext context)
	{
		if (PartStyleExpCommon.GetGlobalProgressionLevel(in context) <= 19)
		{
			return PartStyleType.GlossyMetal;
		}
		if (!context.IsModelLevel)
		{
			return PartStyleType.GlossyMetal;
		}
		if (!context.IsNewGame)
		{
			if (PartStyleExpCommon.TryGetResumeStyle(in context, out var style))
			{
				return style;
			}
			PartStyleType lastModelRandomStyle = GetLastModelRandomStyle();
			if (lastModelRandomStyle != PartStyleType.GlossyMetal)
			{
				return lastModelRandomStyle;
			}
			return PartStyleType.GlossyMetal;
		}
		PartStyleType num = PartStyleExpCommon.PickRandomExcluding(GetLastModelRandomStyle());
		SetLastModelRandomStyle(num);
		return num;
	}

	private static PartStyleType GetLastModelRandomStyle()
	{
		GameBordPartStyleModelData gameBordPartStyleSave = PartStyleExpCommon.GetGameBordPartStyleSave();
		if (gameBordPartStyleSave == null || gameBordPartStyleSave.Exp5LastRandom < 0)
		{
			return PartStyleType.GlossyMetal;
		}
		if (!PartStyleExpCommon.TryParseStyle(gameBordPartStyleSave.Exp5LastRandom, out var style))
		{
			return PartStyleType.GlossyMetal;
		}
		return style;
	}

	private static void SetLastModelRandomStyle(PartStyleType style)
	{
		GameBordPartStyleModelData gameBordPartStyleSave = PartStyleExpCommon.GetGameBordPartStyleSave();
		if (gameBordPartStyleSave != null)
		{
			gameBordPartStyleSave.Exp5LastRandom = (int)style;
		}
	}
}
