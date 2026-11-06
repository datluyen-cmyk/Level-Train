using System;
using Game.DataTrack;
using RotateRings.Ad;
using RotateRings.Prop;
using RotateRings.UI.Game;
using UniLog;
using XingYun.UI.Core;

namespace RotateRings.UI.Prop;

public static class PropAdRewardService
{
	private const string Tag = "PropAdReward";

	public static void RequestAcquireByAd(PropAdRewardRequest request)
	{
		if (request?.EventData != null && request.Host != null && !GameApp.UI.IsOpen("UIPropAward"))
		{
			PropAdRewardDecision propAdRewardDecision = PropAdRewardRules.Resolve(request.EventData.PropType);
			if (!propAdRewardDecision.ShowAwardPopup)
			{
				PlayRewardAdDirect(request, propAdRewardDecision.GrantCount);
			}
			else
			{
				OpenAwardPopup(request, propAdRewardDecision.GrantCount);
			}
		}
	}

	private static void OpenAwardPopup(PropAdRewardRequest request, int grantCount)
	{
		request.Host.SendPropUsingState(request.EventData.PropType, isUsing: true);
		AdPopPosition adPopPosition = ResolveMidRewardPosition(request.EventData.PropType);
		RRDataTrack.Instance.ReportAdRewardEnter(RRDataTrack.AdAction.Show, adPopPosition);
		UIPropAwardParam uIPropAwardParam = new UIPropAwardParam
		{
			PropType = request.EventData.PropType,
			GrantCount = grantCount,
			OnAccept = delegate
			{
				PlayRewardAdDirect(request, grantCount);
			},
			OnClose = delegate
			{
				request.Host.SendPropUsingState(EPropType.None, isUsing: false);
				request.Host.NotifyUnlockIntroCompleteIfNeeded(request.EventData.EntrySource);
			}
		};
		GameApp.UI.Open("UIPropAward", (UILayer)2, (object)uIPropAwardParam, (Action)null);
	}

	private static void PlayRewardAdDirect(PropAdRewardRequest request, int grantCount)
	{
		PropUseEvent eventData = request.EventData;
		IPropUseServiceHost host = request.Host;
		host.IsUsingByAd = true;
		host.SendPropUsingState(eventData.PropType, isUsing: true);
		AdPopPosition adPopPosition = ResolveMidRewardPosition(eventData.PropType);
		RRDataTrack.Instance.ReportAdRewardEnter(RRDataTrack.AdAction.Click, adPopPosition);
		AdService.ShowRewardAd(new RewardAdRequest
		{
			PopPosition = adPopPosition,
			Callback = delegate(RewardAdResult result)
			{
				host.IsUsingByAd = false;
				if (!result.IsSuccess)
				{
					host.SendPropUsingState(EPropType.None, isUsing: false);
				}
				else
				{
					PropAdRewardSuccessMode successMode = PropAdCompleteFlowRules.ResolveSuccessMode(eventData.PropType);
					GrantReward(host, eventData, grantCount, successMode, request.OnGrantOnlyComplete);
				}
			}
		}, delegate
		{
			host.IsUsingByAd = false;
			host.SendPropUsingState(EPropType.None, isUsing: false);
		});
	}

	private static AdPopPosition ResolveMidRewardPosition(EPropType propType)
	{
		if (propType != EPropType.Skyrocket)
		{
			return AdPopPosition.MidRewardHammer;
		}
		return AdPopPosition.MidRewardRocket;
	}

	private static void GrantReward(IPropUseServiceHost host, PropUseEvent eventData, int grantCount, PropAdRewardSuccessMode successMode, Action onGrantOnlyComplete)
	{
		int num = grantCount;
		if (successMode == PropAdRewardSuccessMode.DirectUseInRound)
		{
			host.SendPropUsingState(EPropType.None, isUsing: false);
			if (host.TryDirectUseSkyrocketAfterAd(eventData))
			{
				num = grantCount - 1;
				if (num < 1)
				{
					return;
				}
				successMode = PropAdRewardSuccessMode.GrantOnly;
			}
			else
			{
				host.NotifyUnlockIntroCompleteIfNeeded(eventData.EntrySource);
				successMode = PropAdRewardSuccessMode.GrantOnly;
			}
		}
		int addCount = ((num < 1) ? 1 : num);
		GameApp.Proxy.Get<PropModelProxy>()?.AddProp(eventData.PropType, addCount, TrackPropGainReason.AdRetrieval, "广告获取", "激励广告");
		if (successMode == PropAdRewardSuccessMode.BeginUseAfterGrant)
		{
			host.BeginUseProp(eventData, TrackPropUseReason.Ad);
			return;
		}
		host.SendPropUsingState(EPropType.None, isUsing: false);
		try
		{
			onGrantOnlyComplete?.Invoke();
		}
		catch (Exception ex)
		{
			Log.LogError("PropAdReward", "GrantOnly 回调异常: {0}", new object[1] { ex });
		}
	}
}
