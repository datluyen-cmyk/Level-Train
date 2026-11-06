using System;
using RotateRings.GamePlay.PartStyle;
using UnityEngine;
using XingYun.Experiment;

namespace RotateRings.GamePlay.Services;

public static class SolveRingEffectExpResolve
{
	public const string SolveRingEffectKey = "GameBord.SolveRingEffect";

	public const string GameBordStyleKey = "GameBord.GameBordStyle";

	public static int ReadSolveRingEffect()
	{
		return ReadGameBordIntField("GameBord.SolveRingEffect", (GameBord cfg) => cfg.SolveRingEffect, 1, 1, 8);
	}

	public static int ReadGameBordStyle()
	{
		return GameBordPartStyleResolver.ClampStyleExp(ReadGameBordIntField("GameBord.GameBordStyle", (GameBord cfg) => cfg.GameBordStyle, 1, 1, 99));
	}

	public static SolveRingEffectPaths Resolve(int solveRingEffect)
	{
		switch (solveRingEffect)
		{
		case 2:
			return SolveRingEffectExpGp3.Resolve(1);
		case 3:
		case 7:
		case 8:
			return SolveRingEffectExpGp3.Resolve(2);
		case 4:
			return SolveRingEffectExpGp3.Resolve(3);
		case 5:
			return SolveRingEffectExpGp3.Resolve(4);
		case 6:
			return SolveRingEffectExpGp3.Resolve(5);
		default:
			return SolveRingEffectExpBaseboard.Get();
		}
	}

	internal static int ReadGameBordIntField(string fullKey, Func<GameBord, int> fromCfg, int fallback, int min, int max)
	{
		GameBord gameBord = FunctionDataCenter.Get<GameBord>();
		int num = ((fromCfg != null && gameBord != null) ? fromCfg(gameBord) : 0);
		if (num < min || num > max)
		{
			string text = FunctionDataCenter.GetDefault(fullKey);
			if (!string.IsNullOrEmpty(text) && int.TryParse(text, out var result))
			{
				num = result;
			}
		}
		if (num < min || num > max)
		{
			num = fallback;
		}
		return Mathf.Clamp(num, min, max);
	}
}
