using XingYun.Level;

namespace Game.Level;

public class WinRate10LevelGreaterCondition : IMatchCondition
{
	public bool Evaluate(ILevelMatchRule rule, int levelIndex, LevelContext ctx)
	{
		return RandomLevelFirstClearWindow.GetLevelRate() >= rule.FloatParam1;
	}
}
