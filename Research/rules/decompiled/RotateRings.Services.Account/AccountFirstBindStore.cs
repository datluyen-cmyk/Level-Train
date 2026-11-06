namespace RotateRings.Services.Account;

internal static class AccountFirstBindStore
{
	public static bool ConsumeFirstBindFlag()
	{
		CloudSaveCacheData data = CloudSaveCache.Data;
		bool result = !data.HasBoundThirdPartyOnce;
		if (!data.HasBoundThirdPartyOnce)
		{
			data.HasBoundThirdPartyOnce = true;
		}
		return result;
	}
}
