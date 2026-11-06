using RotateRings.Common.Performance;
using RotateRings.Services;
using XingYun.Ad;

namespace RotateRings.Ad.Bridge;

public sealed class RotateRingsAdEventHandler : GameAdEventHandler
{
	private const string Tag = "[AdStrategy]";

	public override void OnAdStarted(string adType, IAdConfiguration config)
	{
		((GameAdEventHandler)this).OnAdStarted(adType, config);
		AudioService.PauseBgmForAd();
		AudioService.PauseSoundForAd();
	}

	public override void OnAdClosed(string adType, AdResult result)
	{
		AudioService.ResumeBgmFromAd();
		AudioService.ResumeSoundFromAd();
		if (!(adType == "inter"))
		{
			((GameAdEventHandler)this).OnAdClosed(adType, result);
		}
		ProjectFrameRatePolicy.Apply("OnAdClosed");
	}
}
