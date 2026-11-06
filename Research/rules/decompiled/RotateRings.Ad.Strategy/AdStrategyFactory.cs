using RotateRings.Ad.Strategy.Interstitial;
using RotateRings.Ad.Strategy.Reward;

namespace RotateRings.Ad.Strategy;

public static class AdStrategyFactory
{
	public static GameAdStrategy Create()
	{
		return new GameAdStrategy(CreateInterStrategy(), new DefaultRewardAdStrategy());
	}

	private static AdStrategyBase CreateInterStrategy()
	{
		ARRExperiment.GetGroup();
		if (ShouldUseArrStrategy(out var _))
		{
			return new ARRInterAdStrategy();
		}
		return new DefaultInterAdStrategy();
	}

	internal static bool ShouldUseArrStrategy()
	{
		bool installCampaignActive;
		return ShouldUseArrStrategy(out installCampaignActive);
	}

	private static bool ShouldUseArrStrategy(out bool installCampaignActive)
	{
		installCampaignActive = InstallCampaignPolicy.IsTakeOverActive();
		if (ARRExperiment.IsTakeOverActive())
		{
			return !installCampaignActive;
		}
		return false;
	}
}
