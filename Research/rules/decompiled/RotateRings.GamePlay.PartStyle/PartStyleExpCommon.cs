using System;
using RotateRings.Model;
using UnityEngine;

namespace RotateRings.GamePlay.PartStyle;

public static class PartStyleExpCommon
{
	public const PartStyleType PrefixLevelStyle = PartStyleType.GlossyMetal;

	public static readonly PartStyleType[] EightMaterialCycle = new PartStyleType[8]
	{
		PartStyleType.Jelly,
		PartStyleType.MatteSmooth,
		PartStyleType.Marble,
		PartStyleType.Wood,
		PartStyleType.MatteFaceted,
		PartStyleType.Jade,
		PartStyleType.Gold,
		PartStyleType.GlossyMetal
	};

	public static int GetGlobalProgressionLevel(in PartStyleResolveContext context)
	{
		if (context.GameMode != GameMode.Endless)
		{
			return context.Level;
		}
		return (GameApp.Proxy.Get<LevelModelProxy>()?.MainLevel ?? context.Level) + context.Level - 1;
	}

	public static bool TryGetResumeStyle(in PartStyleResolveContext context, out PartStyleType style)
	{
		if (context.IsNewGame)
		{
			style = PartStyleType.GlossyMetal;
			return false;
		}
		return TryParseStyle(context.SavedLevelPartStyle, out style);
	}

	public static PartStyleType PickRandomExcluding(PartStyleType exclude)
	{
		int num = 0;
		for (int i = 0; i < EightMaterialCycle.Length; i++)
		{
			if (EightMaterialCycle[i] != exclude)
			{
				num++;
			}
		}
		if (num <= 0)
		{
			return EightMaterialCycle[0];
		}
		int num2 = Random.Range(0, num);
		int num3 = 0;
		for (int j = 0; j < EightMaterialCycle.Length; j++)
		{
			if (EightMaterialCycle[j] != exclude)
			{
				if (num3 == num2)
				{
					return EightMaterialCycle[j];
				}
				num3++;
			}
		}
		return EightMaterialCycle[0];
	}

	public static GameBordPartStyleModelData GetGameBordPartStyleSave()
	{
		return GameApp.Proxy.Get<GameBordPartStyleModelProxy>()?.Data;
	}

	public static bool TryParseStyle(int raw, out PartStyleType style)
	{
		if (Enum.IsDefined(typeof(PartStyleType), raw))
		{
			style = (PartStyleType)raw;
			return true;
		}
		style = PartStyleType.GlossyMetal;
		return false;
	}
}
