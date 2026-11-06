using System.Collections.Generic;
using Newtonsoft.Json;
using XingYun.Ad;
using XingYun.Aot;

namespace RotateRings.Ad.Bridge;

public sealed class RotateRingsAdDataTracker : GameAdDataTracker
{
	public override void TrackAdIncomeRevenue(string adType, IAdConfiguration config)
	{
		if (((config != null) ? config.AdPosition : null) != "无法加载广告--抽奖")
		{
			((GameAdDataTracker)this).TrackAdIncomeRevenue(adType, config);
			return;
		}
		SDKManager.TrackAdIncomeRevenue(JsonConvert.SerializeObject((object)new Dictionary<string, object>(config.GetCustomData()) { ["ad_position"] = "结算激励--抽奖" }));
	}
}
