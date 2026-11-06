using System;

namespace RotateRings.RestartRetention;

public static class RestartRetentionRules
{
	public static RestartRetentionDecision Evaluate(RestartRetentionContext context)
	{
		return Decide(RestartRetentionConfigResolver.Resolve(), context);
	}

	public static RestartRetentionDecision Decide(RestartRetentionConfig config, RestartRetentionContext context)
	{
		int num = Math.Max(0, context.Streak);
		bool canRestart = context.CanRestart;
		RestartRetentionContent restartRetentionContent = RestartRetentionContent.None;
		bool useBaseArt = true;
		if ((canRestart && context.SupportedMode) || context.ForcePreview)
		{
			bool num2 = num >= config.StreakMin;
			restartRetentionContent = (num2 ? config.HighStreakContent : config.LowStreakContent);
			useBaseArt = (num2 ? config.HighUseBaseArt : config.LowUseBaseArt);
			if (restartRetentionContent != RestartRetentionContent.None && !context.RankActive)
			{
				restartRetentionContent = config.RankInactiveContent;
				useBaseArt = config.RankInactiveUseBaseArt;
			}
		}
		if (context.ForcePreview && restartRetentionContent == RestartRetentionContent.None)
		{
			restartRetentionContent = RestartRetentionContent.Basic;
			useBaseArt = true;
		}
		return new RestartRetentionDecision(config, num, restartRetentionContent, canRestart, useBaseArt);
	}
}
