using XingYun.BT;
using XingYun.Experiment;

namespace Game.Experiment.Strategy;

public abstract class AdInterStrategyNode : BTActionNode
{
	protected AdInterContext Context { get; private set; }

	protected InsertAdFunction Config => FunctionDataCenter.Get<InsertAdFunction>();

	protected AdInterDecision Result { get; private set; }

	public void SetContext(AdInterContext context)
	{
		Context = context;
	}

	public AdInterDecision GetResult()
	{
		return Result;
	}

	protected void Allow()
	{
		Result = AdInterDecision.Allow();
	}

	protected void Block(string reason)
	{
		Result = AdInterDecision.Block(reason);
	}

	public void Reset()
	{
		Result = default(AdInterDecision);
	}
}
