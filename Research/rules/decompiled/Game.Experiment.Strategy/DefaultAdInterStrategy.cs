using RotateRings.Ad;

namespace Game.Experiment.Strategy;

public class DefaultAdInterStrategy : AdInterStrategyNode
{
	protected override void ExecuteAction()
	{
		if (base.Context.LevelCount < base.Config.PlayCount)
		{
			Block(AdBlockReason.FormatLegacyLevelBlocked(base.Config.PlayCount));
		}
		else if (base.Context.TimeSinceLast < (float)base.Config.CD)
		{
			Block(AdBlockReason.FormatLegacyCooldownBlocked(base.Config.CD));
		}
		else
		{
			Allow();
		}
	}
}
