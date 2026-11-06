using RotateRings.UI.ClaimReward;
using XingYun.Command;

namespace RotateRings.Chest;

public class ChestAutoClaimCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		ChestProxy proxy = ((BaseCommand)this).GetProxy<ChestProxy>();
		if (proxy.HasPendingClaimable())
		{
			int currentCycleIndex = proxy.GetCurrentCycleIndex();
			int currentRewardConfigId = proxy.GetCurrentRewardConfigId();
			ClaimRewardService.Grant(new UIClaimRewardParam
			{
				RewardId = currentRewardConfigId,
				RewardReasonId = "关卡宝箱自动补发",
				RewardReasonDetail = $"第{currentCycleIndex}轮宝箱玩家未在结算页领取，自动补发单倍奖励",
				HideBgMaskOnOpen = true
			});
			proxy.MarkClaimed(isAutoClaim: true);
		}
	}
}
