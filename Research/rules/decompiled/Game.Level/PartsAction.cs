using System;
using XingYun.Level;
using XingYun.Level.Action;

namespace Game.Level;

public class PartsAction : IMatchAction
{
	private const string TAG = "[RL][PartsAction] ";

	private readonly IMatchAction fallback = (IMatchAction)new LessAction();

	public void Execute(ILevelMatchRule rule, LevelContext ctx)
	{
		if (rule == null || ctx == null)
		{
			return;
		}
		int result;
		if (!string.Equals(rule.Target, "Parts", StringComparison.Ordinal))
		{
			fallback.Execute(rule, ctx);
		}
		else if (int.TryParse(rule.StringValue1, out result) && result > 0)
		{
			int result2 = 0;
			if (ctx.Data.TryGetValue("Parts_MustNotHave_Fixed", out var value) && value != null)
			{
				int.TryParse(value.ToString(), out result2);
			}
			int num = result2 | result;
			ctx.Data["Parts_MustNotHave_Fixed"] = num;
		}
	}
}
