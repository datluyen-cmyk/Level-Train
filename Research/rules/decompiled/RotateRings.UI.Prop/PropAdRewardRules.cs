using RotateRings.Prop;
using XingYun.Experiment;

namespace RotateRings.UI.Prop;

public static class PropAdRewardRules
{
	public static int GetGroupId()
	{
		PropFunction propFunction = FunctionDataCenter.Get<PropFunction>();
		if (propFunction == null)
		{
			return 0;
		}
		if (propFunction.PropAdRewardGroupId >= 0)
		{
			return propFunction.PropAdRewardGroupId;
		}
		return 0;
	}

	public static PropAdRewardDecision Resolve(EPropType propType)
	{
		int groupId = GetGroupId();
		return Resolve(propType, groupId, experimentEnabled: true);
	}

	public static PropAdRewardDecision Resolve(EPropType propType, int groupId, bool experimentEnabled)
	{
		if (!experimentEnabled)
		{
			return new PropAdRewardDecision
			{
				ExperimentEnabled = false,
				ShowAwardPopup = false,
				GrantCount = 1
			};
		}
		groupId = ((groupId >= 0) ? ((groupId > 4) ? 4 : groupId) : 0);
		switch (propType)
		{
		case EPropType.Hammer:
			if (groupId == 3 || groupId == 4)
			{
				return new PropAdRewardDecision
				{
					ExperimentEnabled = true,
					ShowAwardPopup = true,
					GrantCount = 1
				};
			}
			return new PropAdRewardDecision
			{
				ExperimentEnabled = true,
				ShowAwardPopup = false,
				GrantCount = 1
			};
		default:
			return new PropAdRewardDecision
			{
				ExperimentEnabled = true,
				ShowAwardPopup = false,
				GrantCount = 1
			};
		case EPropType.Skyrocket:
			return groupId switch
			{
				1 => new PropAdRewardDecision
				{
					ExperimentEnabled = true,
					ShowAwardPopup = false,
					GrantCount = 2
				}, 
				2 => new PropAdRewardDecision
				{
					ExperimentEnabled = true,
					ShowAwardPopup = false,
					GrantCount = 3
				}, 
				3 => new PropAdRewardDecision
				{
					ExperimentEnabled = true,
					ShowAwardPopup = true,
					GrantCount = 1
				}, 
				4 => new PropAdRewardDecision
				{
					ExperimentEnabled = true,
					ShowAwardPopup = true,
					GrantCount = 2
				}, 
				_ => new PropAdRewardDecision
				{
					ExperimentEnabled = true,
					ShowAwardPopup = false,
					GrantCount = 1
				}, 
			};
		}
	}
}
