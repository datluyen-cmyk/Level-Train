namespace Game.Feature.Ad;

public interface IRotateRingsInterstitialAdGuardQuery
{
	bool ShouldSkipForFirstDay();

	bool ShouldSkipForColdStart(bool isAfterShow);

	bool ShouldSkipForLevelThreshold(bool isAfterShow);

	bool ShouldSkipForFailRetry(bool isAfterShow);
}
