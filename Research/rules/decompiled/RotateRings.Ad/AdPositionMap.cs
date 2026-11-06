using System.Collections.Generic;

namespace RotateRings.Ad;

public static class AdPositionMap
{
	public const string MidRewardHammer = "局中激励--锤子激励";

	public const string MidRewardRocket = "局中激励--小礼炮激励";

	public const string MidRewardAddStep10 = "局中激励--添加10步";

	public const string LevelStepLimitAddSteps = "局中激励--限定步数加步";

	public const string FailedRewardJoker = "复活激励--万能牌激励";

	public const string FailedRewardMagnet = "复活激励--吸铁石激励";

	public const string FailedRewardShuffle = "复活激励--洗牌激励";

	public const string FailedRewardAddStep10 = "复活激励--添加10步";

	public const string CQFailedRewardJoker = "复活激励--万能牌激励";

	public const string RewardRankDouble = "排行榜活动多倍领奖激励";

	public const string RewardChestMulti = "关卡宝箱多倍领奖激励";

	public const string RewardDailyChallenge = "每日挑战-解锁历史关卡激励";

	public const string RewardLuckySpin = "每日抽奖激励";

	public const string RewardedLuckyDraw = "结算激励--抽奖";

	public const string RewardedLuckyDrawWait = "无法加载广告--抽奖";

	public const string RewardDailyTaskV2 = "每日任务多倍领奖激励";

	public const string RewardDailyFirstWin = "每日首胜多倍领奖激励";

	public const string ShopFreeCoinReward = "商店激励--免费金币";

	public const string BannerGame = "横幅";

	public const string FrontAdGameStart = "前插屏--游戏新开";

	public const string FrontAdGameContinue = "前插屏--游戏继续";

	public const string FrontAdGameRestart = "前插屏--主动重开";

	public const string FrontAdGameFailedRestart = "前插屏--失败重开";

	public const string PostAdSuccess = "后插屏--成功";

	public const string RevenueOutDailyFirstWin = "局外激励--每日首胜领奖多倍激励";

	public const string RevenueOutRankReward = "局外激励--排行榜领奖多倍激励";

	public const string RevenueOutChestMulti = "局外激励--关卡宝箱领奖多倍激励";

	public const string RevenuePostInterSuccess = "后插屏--游戏成功结束";

	private static readonly Dictionary<string, string> NotNetOverrideMap = new Dictionary<string, string>
	{
		{ "局中激励--锤子激励", "无法加载广告--锤子激励" },
		{ "局中激励--小礼炮激励", "无法加载广告--小礼炮激励" },
		{ "局中激励--添加10步", "无法加载广告--添加10步" },
		{ "局中激励--限定步数加步", "无法加载广告--限定步数加步" },
		{ "结算激励--抽奖", "无法加载广告--抽奖" }
	};

	private static readonly Dictionary<string, string> RevenuePositionMap = new Dictionary<string, string>
	{
		{ "每日首胜多倍领奖激励", "局外激励--每日首胜领奖多倍激励" },
		{ "排行榜活动多倍领奖激励", "局外激励--排行榜领奖多倍激励" },
		{ "关卡宝箱多倍领奖激励", "局外激励--关卡宝箱领奖多倍激励" },
		{ "后插屏--成功", "后插屏--游戏成功结束" },
		{ "无法加载广告--抽奖", "结算激励--抽奖" }
	};

	public static string GetAdPosition(AdPopPosition popPosition)
	{
		return popPosition switch
		{
			AdPopPosition.MidRewardHammer => "局中激励--锤子激励", 
			AdPopPosition.MidRewardRocket => "局中激励--小礼炮激励", 
			AdPopPosition.LevelStepLimitAddSteps => "局中激励--限定步数加步", 
			AdPopPosition.RankReward => "排行榜活动多倍领奖激励", 
			AdPopPosition.ChestMultiReward => "关卡宝箱多倍领奖激励", 
			AdPopPosition.DailyChallenge => "每日挑战-解锁历史关卡激励", 
			AdPopPosition.ShopFreeCoin => "商店激励--免费金币", 
			AdPopPosition.LuckySpin => "每日抽奖激励", 
			AdPopPosition.RewardedLuckyDraw => "结算激励--抽奖", 
			AdPopPosition.DailyTaskV2MultiReward => "每日任务多倍领奖激励", 
			AdPopPosition.DailyFirstWinMultiReward => "每日首胜多倍领奖激励", 
			_ => $"未找到{popPosition}", 
		};
	}

	public static string GetRewardWaitAdPosition(AdPopPosition popPosition)
	{
		if (popPosition != AdPopPosition.RewardedLuckyDraw)
		{
			return GetAdPosition(popPosition);
		}
		return "无法加载广告--抽奖";
	}

	public static string GetInterstitialPosition(InterstitialTrigger trigger)
	{
		return trigger switch
		{
			InterstitialTrigger.NewGame => "前插屏--游戏新开", 
			InterstitialTrigger.Continue => "前插屏--游戏继续", 
			InterstitialTrigger.Restart => "前插屏--主动重开", 
			InterstitialTrigger.FailedRestart => "前插屏--失败重开", 
			InterstitialTrigger.RankContinue => "前插屏--游戏继续", 
			InterstitialTrigger.AfterWin => "后插屏--成功", 
			_ => "前插屏--游戏新开", 
		};
	}

	public static string GetAdPositionNotNet(string hasNetPosition)
	{
		if (!NotNetOverrideMap.TryGetValue(hasNetPosition, out var value))
		{
			return "无法加载广告--" + hasNetPosition;
		}
		return value;
	}

	public static string GetAdPositionForRevenue(string trackingPosition)
	{
		if (string.IsNullOrEmpty(trackingPosition))
		{
			return string.Empty;
		}
		if (!RevenuePositionMap.TryGetValue(trackingPosition, out var value))
		{
			return trackingPosition;
		}
		return value;
	}
}
