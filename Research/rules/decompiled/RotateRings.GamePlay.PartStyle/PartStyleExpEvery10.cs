namespace RotateRings.GamePlay.PartStyle;

public static class PartStyleExpEvery10
{
	private const int Interval = 10;

	public static PartStyleType Resolve(int level)
	{
		return PartStyleExpCycle.Resolve(level, 10);
	}
}
