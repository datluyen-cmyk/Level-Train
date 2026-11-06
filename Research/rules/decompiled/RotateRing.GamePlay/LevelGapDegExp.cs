using XingYun.Experiment;

namespace RotateRing.GamePlay;

public static class LevelGapDegExp
{
	private const string TAG = "[GamePlay][LevelGapDegExp] ";

	public const int LevelGapIndexMin = 0;

	public const int LevelGapIndexMax = 20;

	public static int LevelGapTableCount => 20;

	public static int Execute()
	{
		return NormalizeLevelGapIndex(FunctionDataCenter.Get<GameBord>().LevelGapDeg);
	}

	public static int NormalizeLevelGapIndex(int levelGapDeg)
	{
		if (levelGapDeg >= 0 && levelGapDeg <= LevelGapTableCount)
		{
			return levelGapDeg;
		}
		return 0;
	}
}
