namespace RotateRings.GamePlay.PartStyle;

public static class PartStyleExpCycle
{
	public const int RotationStartLevel = 11;

	private static readonly PartStyleType[] Cycle = new PartStyleType[6]
	{
		PartStyleType.Jelly,
		PartStyleType.Wood,
		PartStyleType.MatteSmooth,
		PartStyleType.GlossyMetal,
		PartStyleType.Marble,
		PartStyleType.MatteFaceted
	};

	public static PartStyleType Resolve(int level, int interval)
	{
		if (level < 11)
		{
			return PartStyleType.MatteFaceted;
		}
		int num = (level - 11) / interval;
		return Cycle[num % Cycle.Length];
	}
}
