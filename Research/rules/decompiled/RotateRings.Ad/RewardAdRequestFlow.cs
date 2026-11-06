using System;
using Game.DataTrack;
using RotateRings.Ad.Bridge;
using XingYun.UI.Core;

namespace RotateRings.Ad;

internal sealed class RewardAdRequestFlow
{
	private readonly GameAdBridge bridge;

	public RewardAdRequestFlow(GameAdBridge bridge)
	{
		this.bridge = bridge;
	}

	public void ShowRewardAd(RewardAdRequest request, Action onPopClosed)
	{
		if (!bridge.IsRewardReady())
		{
			if (request.PopPosition == AdPopPosition.RewardedLuckyDraw)
			{
				RRDataTrack.Instance.ReportAdVisit(RRDataTrack.AdVisitType.Reward, AdPositionMap.GetAdPosition(request.PopPosition));
			}
			bridge.PlayRewardAdDirect(AdPositionMap.GetAdPosition(request.PopPosition), null);
			OpenRewardWaitPopup(request.PopPosition, request.Callback, onPopClosed);
		}
		else if (!bridge.TryShowRewardAd(request))
		{
			OpenRewardWaitPopup(request.PopPosition, request.Callback, onPopClosed);
		}
	}

	public void PlayRewardAdDirect(string adPosition, Action<RewardAdResult> callback)
	{
		bridge.PlayRewardAdDirect(adPosition, callback);
	}

	public void ShowRewardDirect(string adPosition, Action<RRShowAdResult> callback)
	{
		bridge.PlayRewardAdDirect(adPosition, delegate(RewardAdResult result)
		{
			callback?.Invoke(new RRShowAdResult(result.IsSuccess, result.IsSuccess ? "Success" : "Failed", adPosition, isGuarantee: false, result.IsFinishedAd));
		});
	}

	private static void OpenRewardWaitPopup(AdPopPosition popPosition, Action<RewardAdResult> callback, Action onPopClosed)
	{
		UIAdPopParam uIAdPopParam = new UIAdPopParam
		{
			PopPosition = popPosition,
			OnAdResult = callback,
			OnPopClosed = onPopClosed
		};
		GameApp.UI.Open("UILevelAdPop", (UILayer)2, (object)uIAdPopParam, (Action)null);
	}
}
