namespace RotateRings.Ad.CommerceLtv;

public static class CommerceLtvCooldownHooks
{
	public static void OnInterstitialClosed(bool isSuccess, string adPosition)
	{
		if (CommerceLtvExperiment.IsActive())
		{
			CommerceInterCacheEcpmSession.SyncFromSdk();
		}
	}

	public static void OnRewardAdFinished()
	{
		if (CommerceLtvExperiment.IsActive())
		{
			CommerceLtvToleranceTracker.OnRewardAdFinished();
			InterstitialEscapeTracker.CooldownStrategy?.CalculateCooldown();
		}
	}

	public static void OnGameEnded()
	{
		if (CommerceLtvExperiment.IsActive())
		{
			CommerceLtvToleranceTracker.OnGameEnded();
			InterstitialEscapeTracker.CooldownStrategy?.CalculateCooldown();
		}
	}

	public static void OnColdStart()
	{
		CommerceInterCacheEcpmSession.ClearOnColdStart();
	}
}
