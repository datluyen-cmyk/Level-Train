using System;
using XingYun.Ad;

namespace RotateRings.Ad;

public static class RRAdConfigurationFactory
{
	public static IAdConfiguration CreateReward(AdPopPosition popPosition, RRAdContext context = null)
	{
		return (IAdConfiguration)(object)new RRAdConfiguration(AdPositionMap.GetAdPosition(popPosition), "rewardAd", ResolveRewardBusinessType(popPosition), context);
	}

	private static string ResolveRewardBusinessType(AdPopPosition popPosition)
	{
		switch (popPosition)
		{
		case AdPopPosition.Failed:
			return "Revive";
		case AdPopPosition.RankReward:
		case AdPopPosition.DailyChallenge:
		case AdPopPosition.ShopFreeCoin:
		case AdPopPosition.LuckySpin:
		case AdPopPosition.DailyTaskV2MultiReward:
		case AdPopPosition.DailyFirstWinMultiReward:
		case AdPopPosition.ChestMultiReward:
		case AdPopPosition.RewardedLuckyDraw:
			return "ClaimX2";
		default:
			return "Prop";
		}
	}

	public static IAdConfiguration CreateInterstitial(InterstitialTrigger trigger, RRAdContext context = null)
	{
		bool flag = trigger == InterstitialTrigger.AfterWin;
		return (IAdConfiguration)(object)new RRAdConfiguration(AdPositionMap.GetInterstitialPosition(trigger), "interAd", flag ? "AfterShow" : "BeforeShow", context, !flag);
	}

	public static IAdConfiguration CreateBanner(RRBannerAdType type, RRAdContext context = null)
	{
		if (type == RRBannerAdType.Game)
		{
			return (IAdConfiguration)(object)new RRAdConfiguration("横幅", string.Empty, "Game", context);
		}
		throw new ArgumentOutOfRangeException("type", type, null);
	}
}
