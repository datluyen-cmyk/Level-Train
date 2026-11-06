using RotateRings.Ad.CommerceLtv;
using XingYun.Experiment;

namespace RotateRings.Ad.Cd;

public static class InterstitialCdStrategyFactory
{
	public static InterstitialCdStrategyKind ResolveActiveKind()
	{
		if (EscapeIntensityCdExperiment.IsActive())
		{
			return InterstitialCdStrategyKind.EscapeIntensity;
		}
		if (CommerceLtvExperiment.IsValidGroup(CommerceLtvExperiment.GetGroup()))
		{
			return InterstitialCdStrategyKind.CommerceLtv;
		}
		if (InterstitialEscapeTracker.IsExperimentActive())
		{
			return InterstitialCdStrategyKind.EscapeRate;
		}
		if (string.IsNullOrWhiteSpace(FunctionDataCenter.Get<InterstitialFunction>()?.CdStrategy))
		{
			return InterstitialCdStrategyKind.Fixed;
		}
		return InterstitialCdStrategyKind.InstallDay;
	}

	public static IInterstitialCooldownStrategy Create(InterstitialCdStrategyKind kind)
	{
		return kind switch
		{
			InterstitialCdStrategyKind.EscapeIntensity => new EscapeIICdStrategy(), 
			InterstitialCdStrategyKind.EscapeRate => new EscapeRateInterstitialCooldownStrategy(), 
			InterstitialCdStrategyKind.CommerceLtv => new CommerceLtvInterstitialCooldownStrategy(CommerceLtvExperiment.GetGroup()), 
			InterstitialCdStrategyKind.InstallDay => new InstallDayInterstitialCooldownStrategy(FunctionDataCenter.Get<InterstitialFunction>()?.CdStrategy), 
			InterstitialCdStrategyKind.Gp39 => new Gp39InterstitialCooldownStrategy(), 
			InterstitialCdStrategyKind.ARR => new ARRInterstitialCooldownStrategy(), 
			_ => new FixedInterstitialCooldownStrategy(), 
		};
	}

	public static void LogResolvedStrategy(InterstitialCdStrategyKind kind, IInterstitialCooldownStrategy strategy)
	{
		switch (kind)
		{
		case InterstitialCdStrategyKind.InstallDay:
			FunctionDataCenter.Get<InterstitialFunction>();
			break;
		case InterstitialCdStrategyKind.Fixed:
		case InterstitialCdStrategyKind.Gp39:
		case InterstitialCdStrategyKind.EscapeRate:
		case InterstitialCdStrategyKind.CommerceLtv:
		case InterstitialCdStrategyKind.ARR:
		case InterstitialCdStrategyKind.EscapeIntensity:
			break;
		}
	}
}
