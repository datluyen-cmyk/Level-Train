using System;
using System.Globalization;
using RotateRings.Common;
using UnityEngine;
using XingYun.Experiment;
using XingYun.GM;

namespace RotateRings.Ad;

public static class AdDebugSettings
{
	private const string KeyBypassInterstitialCd = "RR_AdDebug_BypassInterstitialCd";

	private const string KeyBypassBusinessGuards = "RR_AdDebug_BypassBusinessGuards";

	private const string KeyGmInstallDaysOverride = "RR_AdDebug_GmInstallDaysOverride";

	private const string KeyEnableBackgroundEscapeLevelSkip = "RR_AdDebug_EnableBackgroundEscapeLevelSkip";

	private const string KeyBypassNetworkCheck = "RR_AdDebug_BypassNetworkCheck";

	private const string KeyGmRemoveAds = "RR_AdDebug_GmRemoveAds";

	private const string KeyGmAdRevenueEcpm = "RR_AdDebug_GmAdRevenueEcpm";

	private static bool simulateNoNetwork;

	private const string KeyGmAdEscape = "RR_AdDebug_GmAdEscape";

	private const string KeyGmCountryCode = "RR_AdDebug_GmCountryCode";

	private const string KeyGmFrequencyTier = "RR_AdDebug_GmFrequencyTier";

	private const string KeyGmPreinstallUser = "RR_AdDebug_GmPreinstallUser";

	private const string KeyGmPreInstalledFirstAdTime = "RR_AdDebug_GmPreInstalledFirstAdTime";

	private const string KeyGmNonPreInstalledFirstAdTime = "RR_AdDebug_GmNonPreInstalledFirstAdTime";

	private const string KeyGmCdStrategy = "RR_AdDebug_GmCdStrategy";

	private const string KeyGmNotShowFrontAdStrategy = "RR_AdDebug_GmNotShowFrontAdStrategy";

	private const string KeyGmEscapeCdGroup = "RR_AdDebug_GmEscapeCdGroup";

	private const string KeyGmEscapeIntensityCdGroup = "RR_AdDebug_GmEscapeIntensityCdGroup";

	private const string KeyGmCommerceLTVRatio = "RR_AdDebug_GmCommerceLTVRatio";

	private const string KeyGmFirstEcpmTierCdGroup = "RR_AdDebug_GmFirstEcpmTierCdGroup";

	private const string KeyGmCommerceLtvNMultiplier = "RR_AdDebug_GmCommerceLtvNMultiplier";

	private const string KeyGmRewardInterExemptGroup = "RR_AdDebug_GmRewardInterExemptGroup";

	private const string KeyGmPropWinInterExemptGroup = "RR_AdDebug_GmPropWinInterExemptGroup";

	private const string KeyGmSkipNextLevelFrontAd = "RR_AdDebug_GmSkipNextLevelFrontAd";

	private const string KeyGmSetbackExemption = "RR_AdDebug_GmSetbackExemption";

	private const string KeyGmFreeDoubleReward = "RR_AdDebug_GmFreeDoubleReward";

	private const string KeyGmFirstAdEqulaTimeLTV = "RR_AdDebug_GmFirstAdEqulaTimeLTV";

	private const string KeyGmAdditionalExemptionLTV = "RR_AdDebug_GmAdditionalExemptionLTV";

	private const string KeyGmSpecialChanelFirstAdScheme = "RR_AdDebug_GmSpecialChanelFirstAdScheme";

	private const string KeyGmInstallCampaign = "RR_AdDebug_GmInstallCampaign";

	private const string KeyGmInstallCampaignSuffix = "RR_AdDebug_GmInstallCampaignSuffix";

	private const string KeyGmAccelerateRevenueRecovery = "RR_AdDebug_GmAccelerateRevenueRecovery";

	private const string KeyGmGpPreinstallFirstAdTimeScheme = "RR_AdDebug_GmGpPreinstallFirstAdTimeScheme";

	public const string KeyGmChannel = "RR_AdDebug_GmChannel";

	private const string InterstitialFunctionType = "InterstitialFunction";

	public static bool IsEditorActive => false;

	public static bool BypassInterstitialCd
	{
		get
		{
			if (IsEditorActive)
			{
				return GMConfig.GetInt("RR_AdDebug_BypassInterstitialCd", 0) == 1;
			}
			return false;
		}
		set
		{
			SetDebugFlag("RR_AdDebug_BypassInterstitialCd", value);
		}
	}

	public static bool BypassBusinessGuards
	{
		get
		{
			if (IsEditorActive)
			{
				return GMConfig.GetInt("RR_AdDebug_BypassBusinessGuards", 0) == 1;
			}
			return false;
		}
		set
		{
			SetDebugFlag("RR_AdDebug_BypassBusinessGuards", value);
		}
	}

	public static string GmInstallDaysOverride
	{
		get
		{
			if (!IsEditorActive)
			{
				return string.Empty;
			}
			return GMConfig.GetString("RR_AdDebug_GmInstallDaysOverride", string.Empty);
		}
		set
		{
			if (IsEditorActive)
			{
				string text = value?.Trim();
				int result;
				if (string.IsNullOrEmpty(text))
				{
					GMConfig.DeleteKey("RR_AdDebug_GmInstallDaysOverride");
				}
				else if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) && result >= 0)
				{
					GMConfig.SetString("RR_AdDebug_GmInstallDaysOverride", result.ToString(CultureInfo.InvariantCulture));
				}
			}
		}
	}

	public static bool EnableBackgroundEscapeLevelSkip
	{
		get
		{
			if (IsEditorActive)
			{
				return GMConfig.GetInt("RR_AdDebug_EnableBackgroundEscapeLevelSkip", 0) == 1;
			}
			return false;
		}
		set
		{
			SetDebugFlag("RR_AdDebug_EnableBackgroundEscapeLevelSkip", value);
		}
	}

	public static bool BypassNetworkCheck
	{
		get
		{
			if (IsEditorActive)
			{
				return GMConfig.GetInt("RR_AdDebug_BypassNetworkCheck", 0) == 1;
			}
			return false;
		}
		set
		{
			SetDebugFlag("RR_AdDebug_BypassNetworkCheck", value);
		}
	}

	public static bool SimulateNoNetwork
	{
		get
		{
			if (IsEditorActive)
			{
				return simulateNoNetwork;
			}
			return false;
		}
		set
		{
			if (IsEditorActive)
			{
				simulateNoNetwork = value;
			}
		}
	}

	public static bool GmRemoveAds
	{
		get
		{
			if (IsEditorActive)
			{
				return GMConfig.GetInt("RR_AdDebug_GmRemoveAds", 0) == 1;
			}
			return false;
		}
		set
		{
			SetDebugFlag("RR_AdDebug_GmRemoveAds", value);
		}
	}

	public static float GmAdRevenueEcpm
	{
		get
		{
			if (!IsEditorActive)
			{
				return 0f;
			}
			return GMConfig.GetFloat("RR_AdDebug_GmAdRevenueEcpm", 10f);
		}
		set
		{
			if (IsEditorActive)
			{
				GMConfig.SetFloat("RR_AdDebug_GmAdRevenueEcpm", value);
			}
		}
	}

	public static bool GmAdEscape
	{
		get
		{
			if (IsEditorActive)
			{
				return GMConfig.GetInt("RR_AdDebug_GmAdEscape", 0) == 1;
			}
			return false;
		}
		set
		{
			SetDebugFlag("RR_AdDebug_GmAdEscape", value);
		}
	}

	public static float LastRecordedWatchRevenueEcpm { get; private set; }

	public static bool LastRecordedWatchIsEscape { get; private set; }

	public static string GmCountryCode
	{
		get
		{
			if (!IsEditorActive)
			{
				return string.Empty;
			}
			return GMConfig.GetString("RR_AdDebug_GmCountryCode", string.Empty);
		}
		set
		{
			if (IsEditorActive)
			{
				GMConfig.SetString("RR_AdDebug_GmCountryCode", value?.Trim().ToUpperInvariant() ?? string.Empty);
			}
		}
	}

	public static bool HasGmFrequencyTierOverride
	{
		get
		{
			if (IsEditorActive)
			{
				return GMConfig.HasKey("RR_AdDebug_GmFrequencyTier");
			}
			return false;
		}
	}

	public static int GmFrequencyTier
	{
		get
		{
			if (!IsEditorActive || !HasGmFrequencyTierOverride)
			{
				return -1;
			}
			return Mathf.Clamp(GMConfig.GetInt("RR_AdDebug_GmFrequencyTier", -1), 0, 2);
		}
		set
		{
			if (IsEditorActive)
			{
				GMConfig.SetInt("RR_AdDebug_GmFrequencyTier", Mathf.Clamp(value, 0, 2));
			}
		}
	}

	public static int GmPreinstallChannelMode
	{
		get
		{
			if (!IsEditorActive || !GMConfig.HasKey("RR_AdDebug_GmPreinstallUser"))
			{
				return 0;
			}
			if (GMConfig.GetInt("RR_AdDebug_GmPreinstallUser", 0) != 1)
			{
				return 2;
			}
			return 1;
		}
		set
		{
			if (IsEditorActive)
			{
				int num = Mathf.Clamp(value, 0, 2);
				if (num == 0)
				{
					GMConfig.DeleteKey("RR_AdDebug_GmPreinstallUser");
				}
				else
				{
					GMConfig.SetInt("RR_AdDebug_GmPreinstallUser", (num == 1) ? 1 : 0);
				}
			}
		}
	}

	public static int GmPreInstalledFirstAdTime
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmPreInstalledFirstAdTime", () => FeatureConfig.InterstitialFunction.PreInstalledFirstAdTime);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmPreInstalledFirstAdTime", "PreInstalledFirstAdTime", value);
		}
	}

	public static int GmNonPreInstalledFirstAdTime
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmNonPreInstalledFirstAdTime", () => FeatureConfig.InterstitialFunction.NonPreInstalledFirstAdTime);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmNonPreInstalledFirstAdTime", "NonPreInstalledFirstAdTime", value);
		}
	}

	public static string GmCdStrategy
	{
		get
		{
			return ReadPersistedStringOrInterstitialFunction("RR_AdDebug_GmCdStrategy", () => FeatureConfig.InterstitialFunction.CdStrategy);
		}
		set
		{
			SetPersistedInterstitialString("RR_AdDebug_GmCdStrategy", "CdStrategy", value, rebindCooldownStrategy: true);
		}
	}

	public static int GmNotShowFrontAdStrategy
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmNotShowFrontAdStrategy", () => FeatureConfig.InterstitialFunction.NotShowFrontAdStrategy);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmNotShowFrontAdStrategy", "NotShowFrontAdStrategy", value);
		}
	}

	public static int GmEscapeCdGroup
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmEscapeCdGroup", () => FeatureConfig.InterstitialFunction.EscapeCdGroup);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmEscapeCdGroup", "EscapeCdGroup", Mathf.Clamp(value, 0, 5), rebindCooldownStrategy: true);
		}
	}

	public static int GmEscapeIntensityCdGroup
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmEscapeIntensityCdGroup", () => FeatureConfig.InterstitialFunction.EscapeIntensityCdGroup);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmEscapeIntensityCdGroup", "EscapeIntensityCdGroup", Mathf.Clamp(value, 0, 5), rebindCooldownStrategy: true);
		}
	}

	public static int GmCommerceLTVRatio
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmCommerceLTVRatio", () => FeatureConfig.InterstitialFunction.CommerceLTVRatio);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmCommerceLTVRatio", "CommerceLTVRatio", Mathf.Clamp(value, 0, 3), rebindCooldownStrategy: true);
		}
	}

	public static int GmFirstEcpmTierCdGroup
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmFirstEcpmTierCdGroup", () => FeatureConfig.InterstitialFunction.FirstEcpmTierCdGroup);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmFirstEcpmTierCdGroup", "FirstEcpmTierCdGroup", Mathf.Clamp(value, 0, 4));
		}
	}

	public static float GmCommerceLtvNMultiplier
	{
		get
		{
			if (!IsEditorActive)
			{
				return 0f;
			}
			return GMConfig.GetFloat("RR_AdDebug_GmCommerceLtvNMultiplier", 0f);
		}
		set
		{
			if (IsEditorActive)
			{
				GMConfig.SetFloat("RR_AdDebug_GmCommerceLtvNMultiplier", Mathf.Max(0f, value));
			}
		}
	}

	public static int GmRewardInterExemptGroup
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmRewardInterExemptGroup", () => FeatureConfig.InterstitialFunction.RewardInterExemptGroup);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmRewardInterExemptGroup", "RewardInterExemptGroup", Mathf.Clamp(value, 0, 4));
		}
	}

	public static int GmPropWinInterExemptGroup
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmPropWinInterExemptGroup", () => FeatureConfig.InterstitialFunction.PropWinInterExemptGroup);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmPropWinInterExemptGroup", "PropWinInterExemptGroup", Mathf.Clamp(value, 0, 2));
		}
	}

	public static int GmSkipNextLevelFrontAd
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmSkipNextLevelFrontAd", () => FeatureConfig.InterstitialFunction.SkipNextLevelFrontAd);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmSkipNextLevelFrontAd", "SkipNextLevelFrontAd", Mathf.Clamp(value, 0, 2));
		}
	}

	public static string GmSetbackExemption
	{
		get
		{
			return ReadPersistedStringOrInterstitialFunction("RR_AdDebug_GmSetbackExemption", () => FeatureConfig.InterstitialFunction.SetbackExemption);
		}
		set
		{
			SetPersistedInterstitialString("RR_AdDebug_GmSetbackExemption", "SetbackExemption", value, rebindCooldownStrategy: false);
		}
	}

	public static int GmFreeDoubleReward
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmFreeDoubleReward", () => FeatureConfig.InterstitialFunction.FreeDoubleReward);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmFreeDoubleReward", "FreeDoubleReward", Mathf.Clamp(value, 0, 2));
		}
	}

	public static int GmInstallCampaign
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmInstallCampaign", () => FeatureConfig.InterstitialFunction.InstallCampaign);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmInstallCampaign", "InstallCampaign", Mathf.Clamp(value, 0, 1));
		}
	}

	public static string GmInstallCampaignSuffix
	{
		get
		{
			if (!IsEditorActive)
			{
				return string.Empty;
			}
			return GMConfig.GetString("RR_AdDebug_GmInstallCampaignSuffix", string.Empty);
		}
		set
		{
			if (IsEditorActive)
			{
				string text = value?.Trim();
				if (string.IsNullOrEmpty(text))
				{
					GMConfig.DeleteKey("RR_AdDebug_GmInstallCampaignSuffix");
				}
				else
				{
					GMConfig.SetString("RR_AdDebug_GmInstallCampaignSuffix", text);
				}
			}
		}
	}

	public static int GmFirstAdEqulaTimeLTV
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmFirstAdEqulaTimeLTV", () => FeatureConfig.InterstitialFunction.FirstAdEqulaTimeLTV);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmFirstAdEqulaTimeLTV", "FirstAdEqulaTimeLTV", value);
		}
	}

	public static int GmGpPreinstallFirstAdTimeScheme
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmGpPreinstallFirstAdTimeScheme", () => FeatureConfig.InterstitialFunction.GpPreinstallFirstAdTimeScheme);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmGpPreinstallFirstAdTimeScheme", "GpPreinstallFirstAdTimeScheme", Mathf.Clamp(value, 0, 6));
		}
	}

	public static int GmAdditionalExemptionLTV
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmAdditionalExemptionLTV", () => FeatureConfig.InterstitialFunction.AdditionalExemptionLTV);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmAdditionalExemptionLTV", "AdditionalExemptionLTV", value);
		}
	}

	public static int GmAccelerateRevenueRecovery
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmAccelerateRevenueRecovery", () => FeatureConfig.InterstitialFunction.AccelerateRevenueRecovery);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmAccelerateRevenueRecovery", "AccelerateRevenueRecovery", Mathf.Clamp(value, 0, 5));
		}
	}

	public static int GmSpecialChanelFirstAdScheme
	{
		get
		{
			return ReadPersistedIntOrInterstitialFunction("RR_AdDebug_GmSpecialChanelFirstAdScheme", () => FeatureConfig.InterstitialFunction.SpecialChanelFirstAdScheme);
		}
		set
		{
			SetPersistedInterstitialInt("RR_AdDebug_GmSpecialChanelFirstAdScheme", "SpecialChanelFirstAdScheme", Mathf.Clamp(value, 0, 6));
		}
	}

	public static string GmChannel
	{
		get
		{
			if (!IsEditorActive)
			{
				return string.Empty;
			}
			return GMConfig.GetString("RR_AdDebug_GmChannel", string.Empty);
		}
		set
		{
			if (IsEditorActive)
			{
				string text = value?.Trim();
				if (string.IsNullOrEmpty(text))
				{
					GMConfig.DeleteKey("RR_AdDebug_GmChannel");
				}
				else
				{
					GMConfig.SetString("RR_AdDebug_GmChannel", text);
				}
			}
		}
	}

	public static float GmRealGameSeconds { get; set; }

	public static bool TryGetGmInstallDaysOverride(out int installDays)
	{
		installDays = 0;
		if (!IsEditorActive)
		{
			return false;
		}
		string text = GMConfig.GetString("RR_AdDebug_GmInstallDaysOverride", string.Empty);
		if (!string.IsNullOrWhiteSpace(text))
		{
			return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out installDays);
		}
		return false;
	}

	internal static void SetLastRecordedWatch(float revenueEcpm, bool isEscape)
	{
		LastRecordedWatchRevenueEcpm = revenueEcpm;
		LastRecordedWatchIsEscape = isEscape;
	}

	public static void ApplyPersistedGmInterstitialFunctionOverrides()
	{
		if (IsEditorActive)
		{
			if (GMConfig.HasKey("RR_AdDebug_GmPreInstalledFirstAdTime"))
			{
				ApplyInterstitialFieldOverride("PreInstalledFirstAdTime", GMConfig.GetInt("RR_AdDebug_GmPreInstalledFirstAdTime", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmNonPreInstalledFirstAdTime"))
			{
				ApplyInterstitialFieldOverride("NonPreInstalledFirstAdTime", GMConfig.GetInt("RR_AdDebug_GmNonPreInstalledFirstAdTime", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmCdStrategy"))
			{
				ApplyInterstitialFieldOverride("CdStrategy", GMConfig.GetString("RR_AdDebug_GmCdStrategy", string.Empty));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmNotShowFrontAdStrategy"))
			{
				ApplyInterstitialFieldOverride("NotShowFrontAdStrategy", GMConfig.GetInt("RR_AdDebug_GmNotShowFrontAdStrategy", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmEscapeCdGroup"))
			{
				ApplyInterstitialFieldOverride("EscapeCdGroup", GMConfig.GetInt("RR_AdDebug_GmEscapeCdGroup", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmEscapeIntensityCdGroup"))
			{
				ApplyInterstitialFieldOverride("EscapeIntensityCdGroup", GMConfig.GetInt("RR_AdDebug_GmEscapeIntensityCdGroup", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmCommerceLTVRatio"))
			{
				ApplyInterstitialFieldOverride("CommerceLTVRatio", GMConfig.GetInt("RR_AdDebug_GmCommerceLTVRatio", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmFirstEcpmTierCdGroup"))
			{
				ApplyInterstitialFieldOverride("FirstEcpmTierCdGroup", GMConfig.GetInt("RR_AdDebug_GmFirstEcpmTierCdGroup", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmRewardInterExemptGroup"))
			{
				ApplyInterstitialFieldOverride("RewardInterExemptGroup", GMConfig.GetInt("RR_AdDebug_GmRewardInterExemptGroup", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmPropWinInterExemptGroup"))
			{
				ApplyInterstitialFieldOverride("PropWinInterExemptGroup", GMConfig.GetInt("RR_AdDebug_GmPropWinInterExemptGroup", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmSkipNextLevelFrontAd"))
			{
				ApplyInterstitialFieldOverride("SkipNextLevelFrontAd", GMConfig.GetInt("RR_AdDebug_GmSkipNextLevelFrontAd", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmFreeDoubleReward"))
			{
				ApplyInterstitialFieldOverride("FreeDoubleReward", GMConfig.GetInt("RR_AdDebug_GmFreeDoubleReward", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmFirstAdEqulaTimeLTV"))
			{
				ApplyInterstitialFieldOverride("FirstAdEqulaTimeLTV", GMConfig.GetInt("RR_AdDebug_GmFirstAdEqulaTimeLTV", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmGpPreinstallFirstAdTimeScheme"))
			{
				ApplyInterstitialFieldOverride("GpPreinstallFirstAdTimeScheme", GMConfig.GetInt("RR_AdDebug_GmGpPreinstallFirstAdTimeScheme", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmAdditionalExemptionLTV"))
			{
				ApplyInterstitialFieldOverride("AdditionalExemptionLTV", GMConfig.GetInt("RR_AdDebug_GmAdditionalExemptionLTV", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmSpecialChanelFirstAdScheme"))
			{
				ApplyInterstitialFieldOverride("SpecialChanelFirstAdScheme", GMConfig.GetInt("RR_AdDebug_GmSpecialChanelFirstAdScheme", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmInstallCampaign"))
			{
				ApplyInterstitialFieldOverride("InstallCampaign", GMConfig.GetInt("RR_AdDebug_GmInstallCampaign", 0).ToString(CultureInfo.InvariantCulture));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmSetbackExemption"))
			{
				ApplyInterstitialFieldOverride("SetbackExemption", GMConfig.GetString("RR_AdDebug_GmSetbackExemption", string.Empty));
			}
			if (GMConfig.HasKey("RR_AdDebug_GmAccelerateRevenueRecovery"))
			{
				ApplyInterstitialFieldOverride("AccelerateRevenueRecovery", GMConfig.GetInt("RR_AdDebug_GmAccelerateRevenueRecovery", 0).ToString(CultureInfo.InvariantCulture));
			}
		}
	}

	private static int ReadPersistedIntOrInterstitialFunction(string key, Func<int> readFromFunction)
	{
		if (IsEditorActive && GMConfig.HasKey(key))
		{
			return GMConfig.GetInt(key, readFromFunction());
		}
		return readFromFunction();
	}

	private static string ReadPersistedStringOrInterstitialFunction(string key, Func<string> readFromFunction)
	{
		if (IsEditorActive && GMConfig.HasKey(key))
		{
			return GMConfig.GetString(key, readFromFunction() ?? string.Empty);
		}
		return readFromFunction();
	}

	private static void SetPersistedInterstitialInt(string key, string fieldName, int value, bool rebindCooldownStrategy = false)
	{
		if (IsEditorActive)
		{
			GMConfig.SetInt(key, value);
			ApplyInterstitialFieldOverride(fieldName, value.ToString(CultureInfo.InvariantCulture));
		}
	}

	private static void SetPersistedInterstitialString(string key, string fieldName, string value, bool rebindCooldownStrategy)
	{
		if (IsEditorActive)
		{
			GMConfig.SetString(key, value ?? string.Empty);
			ApplyInterstitialFieldOverride(fieldName, value ?? string.Empty);
		}
	}

	private static void ApplyInterstitialFieldOverride(string fieldName, string value)
	{
		FunctionDataCenter.ApplyOverride("InterstitialFunction." + fieldName, value);
		FunctionDataCenter.ClearCache();
	}

	private static void SetDebugFlag(string key, bool value)
	{
		if (IsEditorActive)
		{
			GMConfig.SetInt(key, value ? 1 : 0);
		}
	}
}
