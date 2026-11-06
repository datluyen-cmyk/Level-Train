using XingYun.Level;

namespace Game.Level;

public class WinRate10GameGreaterCondition : IMatchCondition
{
	public bool Evaluate(ILevelMatchRule rule, int levelIndex, LevelContext ctx)
	{
		return RandomLevelFirstClearWindow.GetGameRate() >= rule.FloatParam1;
	}
}
