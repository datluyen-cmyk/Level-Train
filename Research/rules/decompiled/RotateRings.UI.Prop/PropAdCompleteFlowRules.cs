using RotateRings.Prop;
using XingYun.Experiment;

namespace RotateRings.UI.Prop;

public static class PropAdCompleteFlowRules
{
	public static int GetFlowGroupId()
	{
		PropFunction propFunction = FunctionDataCenter.Get<PropFunction>();
		if (propFunction == null)
		{
			return 0;
		}
		return ClampFlowGroupId(propFunction.PropAdCompleteFlowGroupId);
	}

	public static int GetSkyrocketFireStrategyId()
	{
		return FunctionDataCenter.Get<PropFunction>()?.SkyrocketFireStrategy ?? 0;
	}

	public static PropAdRewardSuccessMode ResolveSuccessMode(EPropType propType)
	{
		return ResolveSuccessMode(propType, GetFlowGroupId(), GetSkyrocketFireStrategyId());
	}

	public static PropAdRewardSuccessMode ResolveSuccessMode(EPropType propType, int flowGroupId, int skyrocketFireStrategy)
	{
		flowGroupId = ClampFlowGroupId(flowGroupId);
		skyrocketFireStrategy = ((skyrocketFireStrategy >= 0) ? ((skyrocketFireStrategy > 2) ? 2 : skyrocketFireStrategy) : 0);
		switch (propType)
		{
		case EPropType.Hammer:
			if (flowGroupId != 2)
			{
				return PropAdRewardSuccessMode.BeginUseAfterGrant;
			}
			return PropAdRewardSuccessMode.GrantOnly;
		default:
			return PropAdRewardSuccessMode.BeginUseAfterGrant;
		case EPropType.Skyrocket:
			return flowGroupId switch
			{
				1 => PropAdRewardSuccessMode.DirectUseInRound, 
				2 => PropAdRewardSuccessMode.GrantOnly, 
				_ => (skyrocketFireStrategy != 0) ? PropAdRewardSuccessMode.GrantOnly : PropAdRewardSuccessMode.BeginUseAfterGrant, 
			};
		}
	}

	private static int ClampFlowGroupId(int flowGroupId)
	{
		if (flowGroupId < 0)
		{
			return 0;
		}
		if (flowGroupId <= 2)
		{
			return flowGroupId;
		}
		return 2;
	}
}
