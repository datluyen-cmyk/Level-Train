using XingYun.Proxy;

namespace RotateRings.Ad.Strategy;

public class AdStrategyRuntimeProxy : BaseProxy
{
	public bool FirstRemoveFrontScreenAd { get; set; }

	public AdStrategyRuntimeProxy()
		: base("AdStrategyRuntimeProxy")
	{
	}

	public void OnColdStart()
	{
		FirstRemoveFrontScreenAd = true;
	}

	public void RecordInterAdPlayed()
	{
	}

	public void RecordRewardAdPlayed(int level)
	{
	}
}
