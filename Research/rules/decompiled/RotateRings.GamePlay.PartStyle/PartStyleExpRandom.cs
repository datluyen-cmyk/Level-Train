using System;

namespace RotateRings.GamePlay.PartStyle;

public static class PartStyleExpRandom
{
	private static readonly PartStyleType[] RandomPool = new PartStyleType[5]
	{
		PartStyleType.Jelly,
		PartStyleType.Wood,
		PartStyleType.MatteSmooth,
		PartStyleType.GlossyMetal,
		PartStyleType.Marble
	};

	public static PartStyleType Resolve(int level)
	{
		if (level <= 10)
		{
			return PartStyleType.MatteFaceted;
		}
		return PickRandomExcludingPrevious(Resolve(level - 1), level);
	}

	private static PartStyleType PickRandomExcludingPrevious(PartStyleType previous, int level)
	{
		int num = 0;
		for (int i = 0; i < RandomPool.Length; i++)
		{
			if (RandomPool[i] != previous)
			{
				num++;
			}
		}
		if (num <= 0)
		{
			return RandomPool[0];
		}
		int num2 = new Random(level).Next(num);
		int num3 = 0;
		for (int j = 0; j < RandomPool.Length; j++)
		{
			if (RandomPool[j] != previous)
			{
				if (num3 == num2)
				{
					return RandomPool[j];
				}
				num3++;
			}
		}
		return RandomPool[0];
	}
}
