using XingYun.Level;

namespace Game.Level.Examples;

public class BusinessCustomCondition : IMatchCondition
{
	public bool Evaluate(ILevelMatchRule rule, int levelIndex, LevelContext ctx)
	{
		return false;
	}
}
