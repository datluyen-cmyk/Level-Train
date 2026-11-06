namespace Game.Experiment.Strategy;

public static class AdInterStrategyFactory
{
	public static AdInterStrategyNode Create(int strategyId)
	{
		if (strategyId == 1)
		{
			return new DefaultAdInterStrategy();
		}
		return new DefaultAdInterStrategy();
	}
}
