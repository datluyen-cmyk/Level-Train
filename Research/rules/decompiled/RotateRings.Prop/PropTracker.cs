using System;
using System.Collections.Generic;
using Hot.Model;
using UnityEngine;

namespace RotateRings.Prop;

public class PropTracker
{
	private static readonly (string reasonId, string reasonInfo) Unknown = (reasonId: "未知原因", reasonInfo: "未知原因");

	private readonly PropModelProxy _proxy;

	private readonly Dictionary<TrackPropGainReason, Func<(string reasonId, string reasonInfo)>> GainProviders = new Dictionary<TrackPropGainReason, Func<(string, string)>>();

	private readonly Dictionary<TrackPropUseReason, Func<(string reasonId, string reasonInfo)>> UseProviders = new Dictionary<TrackPropUseReason, Func<(string, string)>>();

	public PropTracker(PropModelProxy proxy)
	{
		_proxy = proxy;
		RegisterDefaults();
	}

	public void Register(TrackPropGainReason reason, string reasonId, string reasonInfo = "")
	{
		WarnIfOverwrite(GainProviders, reason);
		GainProviders[reason] = () => (reasonId: reasonId, reasonInfo: reasonInfo);
	}

	public void Register(TrackPropUseReason reason, string reasonId, string reasonInfo = "")
	{
		WarnIfOverwrite(UseProviders, reason);
		UseProviders[reason] = () => (reasonId: reasonId, reasonInfo: reasonInfo);
	}

	public void Register(TrackPropGainReason reason, Func<(string reasonId, string reasonInfo)> provider)
	{
		WarnIfOverwrite(GainProviders, reason);
		GainProviders[reason] = provider ?? throw new ArgumentNullException("provider");
	}

	public void Register(TrackPropUseReason reason, Func<(string reasonId, string reasonInfo)> provider)
	{
		WarnIfOverwrite(UseProviders, reason);
		UseProviders[reason] = provider ?? throw new ArgumentNullException("provider");
	}

	private void WarnIfOverwrite<TReason>(Dictionary<TReason, Func<(string reasonId, string reasonInfo)>> providers, TReason reason)
	{
		providers.ContainsKey(reason);
	}

	public bool IsRegistered(TrackPropGainReason reason)
	{
		return GainProviders.ContainsKey(reason);
	}

	public bool IsRegistered(TrackPropUseReason reason)
	{
		return UseProviders.ContainsKey(reason);
	}

	public (string reasonId, string reasonInfo) Get(TrackPropGainReason reason, string customReasonId = null, string customReasonInfo = null)
	{
		Func<(string, string)> value;
		return ApplyCustom(GainProviders.TryGetValue(reason, out value) ? value() : Unknown, customReasonId, customReasonInfo);
	}

	public (string reasonId, string reasonInfo) Get(TrackPropUseReason reason, string customReasonId = null, string customReasonInfo = null)
	{
		Func<(string, string)> value;
		return ApplyCustom(UseProviders.TryGetValue(reason, out value) ? value() : Unknown, customReasonId, customReasonInfo);
	}

	public (string reasonId, string reasonInfo) GetReasonIdAndInfo(TrackPropGainReason reason, string customReasonId = null, string customReasonInfo = null)
	{
		return Get(reason, customReasonId, customReasonInfo);
	}

	public (string reasonId, string reasonInfo) GetReasonIdAndInfo(TrackPropUseReason reason, string customReasonId = null, string customReasonInfo = null)
	{
		return Get(reason, customReasonId, customReasonInfo);
	}

	private static (string reasonId, string reasonInfo) ApplyCustom((string reasonId, string reasonInfo) result, string customReasonId, string customReasonInfo)
	{
		if (!string.IsNullOrEmpty(customReasonId))
		{
			result.reasonId = customReasonId;
		}
		if (!string.IsNullOrEmpty(customReasonInfo))
		{
			result.reasonInfo = customReasonInfo;
		}
		return result;
	}

	private void RegisterDefaults()
	{
		RegisterGainDefaults();
		RegisterUseDefaults();
	}

	private void RegisterGainDefaults()
	{
		Register(TrackPropGainReason.None, "未知原因", "未知原因");
		Register(TrackPropGainReason.GMReward, "GM获取", "GM获取");
		Register(TrackPropGainReason.NewBieReward, "新手奖励");
		Register(TrackPropGainReason.AdRetrieval, "广告获取", "激励广告");
		Register(TrackPropGainReason.FallAdRetrieval, "广告获取", "兜底广告");
		Register(TrackPropGainReason.FirstRechargeReward, "首充奖励");
		Register(TrackPropGainReason.PassportReward, "通行证奖励");
		Register(TrackPropGainReason.ShopBuyReward, "商场购买");
		Register(TrackPropGainReason.GameBuyReward, "主线局内金币购买");
		Register(TrackPropGainReason.LevelUpReward, "等级奖励--只金币");
		Register(TrackPropGainReason.RankReward, "排行榜奖励");
		Register(TrackPropGainReason.LevelSuccessStep2Coin, "步数折算");
		Register(TrackPropGainReason.MultiplicationOfRewards, "多倍奖励");
		Register(TrackPropGainReason.LoginBonusReward, "每日登录奖励");
		Register(TrackPropGainReason.ComboReward, "连击奖励");
		Register(TrackPropGainReason.AdFreeCoin, "激励视频金币包");
		Register(TrackPropGainReason.NewbieGiftPackReward, "新手礼包", "新手礼包_道具奖励");
		Register(TrackPropGainReason.DailyFirstWinReward, "每日首胜奖励", "每日首胜打卡");
		Register(TrackPropGainReason.FirstFreeUse, "首次免费赠送");
		Register(TrackPropGainReason.LevelSuccessReward, delegate
		{
			int levelId = GameInfo.LevelId;
			return (reasonId: "通关奖励", reasonInfo: (levelId > 0) ? $"通关奖励_第{levelId}关" : "通关奖励");
		});
	}

	private void RegisterUseDefaults()
	{
		Register(TrackPropUseReason.None, "未知原因", "未知原因");
		Register(TrackPropUseReason.Free, "免费使用");
		Register(TrackPropUseReason.Stock, "库存使用");
		Register(TrackPropUseReason.Coin, "金币购买使用");
		Register(TrackPropUseReason.Ad, "广告使用");
	}

	public void TrackGain(EPropType propType, int num, int beforePropNum, TrackPropGainReason reason, int afterPropNum, string customReasonId = null, string customReasonInfo = null)
	{
		TrackCore(isGain: true, propType, num, beforePropNum, afterPropNum, Get(reason, customReasonId, customReasonInfo));
	}

	public void TrackUse(EPropType propType, int num, int beforePropNum, TrackPropUseReason reason, int afterPropNum, string customReasonId = null, string customReasonInfo = null)
	{
		TrackCore(isGain: false, propType, num, beforePropNum, afterPropNum, Get(reason, customReasonId, customReasonInfo));
	}

	public void ReportNewBiePropAdd()
	{
		if (_proxy.Data.IsNewbieProp)
		{
			return;
		}
		foreach (KeyValuePair<EPropType, int> allProp in _proxy.GetAllProps())
		{
			if (allProp.Value > 0)
			{
				TrackGain(allProp.Key, allProp.Value, 0, TrackPropGainReason.NewBieReward, _proxy.GetPropCount(allProp.Key));
			}
		}
	}

	private void TrackCore(bool isGain, EPropType propType, int num, int beforePropNum, int afterPropNum, (string reasonId, string reasonInfo) reasonData)
	{
		if (num != 0)
		{
			_proxy.GetPropConfig((int)propType);
			_proxy.GetPropName(propType);
			if (!isGain || num <= 0)
			{
				Mathf.Abs(num);
			}
		}
	}
}
