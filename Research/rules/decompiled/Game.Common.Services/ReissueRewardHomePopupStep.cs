using System;
using System.Collections.Generic;
using RotateRings.Services;
using RotateRings.UI.ReissueReward;
using XingYun.UI.Core;

namespace Game.Common.Services;

public class ReissueRewardHomePopupStep : IHomePopupStep, IActivityStep
{
	private const string Tag = "[ReissueRewardHomePopupStep] ";

	public int Priority => 40;

	public bool ShouldRun()
	{
		return ReissueRewardService.HasPendingReissueReward();
	}

	public void Execute()
	{
		List<ReissueRewardTransaction> pendingTransactions = ReissueRewardService.GetPendingTransactions();
		if (pendingTransactions.Count == 0)
		{
			HomePopupPipeline.ContinueNext();
			return;
		}
		GameApp.UI.Open("UIReissueReward", (UILayer)2, (object)new UIReissueRewardController.OpenParam
		{
			Transactions = pendingTransactions,
			OnClosed = HomePopupPipeline.ContinueNext
		}, (Action)null);
	}
}
