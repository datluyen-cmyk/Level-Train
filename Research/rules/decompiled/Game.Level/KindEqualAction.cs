using System;
using XingYun.Level;
using XingYun.Level.Action;

namespace Game.Level;

public class KindEqualAction : IMatchAction
{
	private const string TAG = "[RL][KindEqual] ";

	private readonly IMatchAction fallback = (IMatchAction)new EqualAction();

	public void Execute(ILevelMatchRule rule, LevelContext ctx)
	{
		if (rule != null && ctx != null)
		{
			if (!string.Equals(rule.Target, "LevelType", StringComparison.Ordinal))
			{
				fallback.Execute(rule, ctx);
				return;
			}
			string key = "Kind_Fixed";
			ctx.Data[key] = rule.StringValue1;
		}
	}
}
