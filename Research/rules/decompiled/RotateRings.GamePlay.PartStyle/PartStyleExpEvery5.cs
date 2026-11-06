namespace RotateRings.GamePlay.PartStyle;

public static class PartStyleExpEvery5
{
	private const int Interval = 5;

	public static PartStyleType Resolve(int level)
	{
		return PartStyleExpCycle.Resolve(level, 5);
	}
}
