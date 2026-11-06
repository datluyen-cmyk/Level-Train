using System;
using Newtonsoft.Json;
using RotateRings.Common;
using XingYun.Aot;

namespace RotateRings.Ad;

public static class InstallCampaignAttributionHelper
{
	private const string Tag = "<Ad>.InstallCampaignPolicy";

	private static string cachedCampaign = string.Empty;

	private static string cachedNativeConversionData = string.Empty;

	private static bool requestInFlight;

	private static bool hydratedFromSave;

	private static bool attributionResolved;

	private const string ADStatus = "Organic";

	public static string CachedCampaign
	{
		get
		{
			EnsureHydratedFromSave();
			return cachedCampaign ?? string.Empty;
		}
	}

	public static void RequestIfNeeded()
	{
		if (IsExperimentEnabled())
		{
			EnsureHydratedFromSave();
			if (AdDebugSettings.IsEditorActive && !string.IsNullOrWhiteSpace(AdDebugSettings.GmInstallCampaignSuffix))
			{
				cachedCampaign = AdDebugSettings.GmInstallCampaignSuffix;
				cachedNativeConversionData = string.Empty;
				requestInFlight = false;
			}
			else if (!requestInFlight && !attributionResolved)
			{
				requestInFlight = true;
				RequestAppsflyerCampaign();
			}
		}
	}

	private static bool IsExperimentEnabled()
	{
		return (FeatureConfig.InterstitialFunction?.InstallCampaign ?? 0) == 1;
	}

	public static void ResetForDebug()
	{
		cachedCampaign = string.Empty;
		cachedNativeConversionData = string.Empty;
		requestInFlight = false;
		attributionResolved = false;
		hydratedFromSave = true;
		InstallCampaignRecord orCreate = GameApp.Save.GetOrCreate<InstallCampaignRecord>();
		orCreate.AttributionResolved = false;
		orCreate.RawAttribution = string.Empty;
		orCreate.Campaign = string.Empty;
	}

	private static void OnRequestCompleted(string nativeConversionData, string campaign, bool parseSucceeded)
	{
		bool flag = parseSucceeded && !string.IsNullOrWhiteSpace(campaign);
		requestInFlight = false;
		cachedNativeConversionData = nativeConversionData ?? string.Empty;
		cachedCampaign = campaign ?? string.Empty;
		attributionResolved = flag;
		if (!string.IsNullOrEmpty(cachedNativeConversionData))
		{
			InstallCampaignRecord orCreate = GameApp.Save.GetOrCreate<InstallCampaignRecord>();
			orCreate.AttributionResolved = flag;
			orCreate.RawAttribution = cachedNativeConversionData;
			orCreate.Campaign = cachedCampaign;
		}
	}

	private static void EnsureHydratedFromSave()
	{
		if (!hydratedFromSave)
		{
			hydratedFromSave = true;
			InstallCampaignRecord orCreate = GameApp.Save.GetOrCreate<InstallCampaignRecord>();
			if (orCreate.AttributionResolved)
			{
				attributionResolved = true;
				cachedNativeConversionData = orCreate.RawAttribution ?? string.Empty;
				cachedCampaign = orCreate.Campaign ?? string.Empty;
			}
		}
	}

	private static void RequestAppsflyerCampaign()
	{
		string text = SDKManager.CallNativeUserConversionData();
		string campaign = string.Empty;
		bool parseSucceeded = false;
		try
		{
			UserConversionData userConversionData = JsonConvert.DeserializeObject<UserConversionData>(text);
			if (userConversionData != null)
			{
				parseSucceeded = true;
				campaign = userConversionData.campaign ?? string.Empty;
			}
		}
		catch (Exception)
		{
		}
		OnRequestCompleted(text, campaign, parseSucceeded);
	}

	public static string GetAppsflyerCampaign()
	{
		return CachedCampaign;
	}
}
