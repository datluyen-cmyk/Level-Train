using System;
using RotateRings.Model;
using RotateRings.Services;
using XingYun.Command;

namespace RotateRings.Chest;

public class ChestClaimCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		ChestProxy proxy = ((BaseCommand)this).GetProxy<ChestProxy>();
		if (proxy.IsClaimable())
		{
			int num = Math.Max(0, GameApp.Proxy.Get<LevelModelProxy>().MainLevel - 1);
			ReissueRewardService.ReserveClaimableChestReward(proxy, isAutoClaim: false, $"到达第{num}关");
		}
	}
}
