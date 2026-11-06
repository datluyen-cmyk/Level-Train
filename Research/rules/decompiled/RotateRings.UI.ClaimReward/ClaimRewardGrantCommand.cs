using UniLog;
using XingYun.Command;

namespace RotateRings.UI.ClaimReward;

public class ClaimRewardGrantCommand : BaseCommand
{
	private const string Tag = "ClaimRewardGrant";

	public override void Execute(object param = null)
	{
		if (!(param is ClaimRewardGrantRequest claimRewardGrantRequest))
		{
			Log.LogError("ClaimRewardGrant", "Execute failed: invalid request");
			((BaseCommand)this).SendEvent<ClaimRewardGrantFailed>(new ClaimRewardGrantFailed
			{
				Param = null,
				TargetMultiplier = 0
			});
			return;
		}
		if (claimRewardGrantRequest.Param == null)
		{
			Log.LogError("ClaimRewardGrant", "Execute failed: param is null");
			((BaseCommand)this).SendEvent<ClaimRewardGrantFailed>(new ClaimRewardGrantFailed
			{
				Param = null,
				TargetMultiplier = claimRewardGrantRequest.TargetMultiplier
			});
			return;
		}
		_ = claimRewardGrantRequest.Param.GrantedMultiplier;
		if (!ClaimRewardService.Grant(claimRewardGrantRequest.Param, claimRewardGrantRequest.TargetMultiplier))
		{
			((BaseCommand)this).SendEvent<ClaimRewardGrantFailed>(new ClaimRewardGrantFailed
			{
				Param = claimRewardGrantRequest.Param,
				TargetMultiplier = claimRewardGrantRequest.TargetMultiplier
			});
		}
		else
		{
			((BaseCommand)this).SendEvent<ClaimRewardGrantSuccess>(new ClaimRewardGrantSuccess
			{
				Param = claimRewardGrantRequest.Param,
				TargetMultiplier = claimRewardGrantRequest.TargetMultiplier
			});
		}
	}
}
