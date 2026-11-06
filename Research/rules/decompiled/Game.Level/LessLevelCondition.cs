using XingYun.Level;

namespace Game.Level;

public class LessLevelCondition : IMatchCondition
{
	public bool Evaluate(ILevelMatchRule rule, int levelIndex, LevelContext ctx)
	{
		return levelIndex < rule.IntParam1;
	}
}
