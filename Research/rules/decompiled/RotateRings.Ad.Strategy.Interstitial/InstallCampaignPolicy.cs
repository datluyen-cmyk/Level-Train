using System;
using RotateRings.Common;

namespace RotateRings.Ad.Strategy.Interstitial;

public sealed class InstallCampaignPolicy : InterstitialPolicyBase
{
	private sealed class SchemeData
	{
		public string Suffix;

		public int Scheme;

		public int FirstColdLaunchThreshold;

		public int NextColdLaunchThreshold;

		public int DurationSeconds;

		public float CdMultiplier = 1f;

		public int CdMultiplierMaxInstallDay;
	}

	private const string Tag = "<Ad>.InstallCampaignPolicy";

	private static string lastLoggedCampaign;

	private static readonly SchemeData[] Schemes = new SchemeData[20]
	{
		new SchemeData
		{
			Suffix = "first30next3",
			Scheme = 1,
			FirstColdLaunchThreshold = 30,
			NextColdLaunchThreshold = 3
		},
		new SchemeData
		{
			Suffix = "first20next10",
			Scheme = 2,
			FirstColdLaunchThreshold = 20,
			NextColdLaunchThreshold = 10
		},
		new SchemeData
		{
			Suffix = "first10next6",
			Scheme = 3,
			FirstColdLaunchThreshold = 10,
			NextColdLaunchThreshold = 6
		},
		new SchemeData
		{
			Suffix = "first5next3",
			Scheme = 4,
			FirstColdLaunchThreshold = 5,
			NextColdLaunchThreshold = 3
		},
		new SchemeData
		{
			Suffix = "first7next5",
			Scheme = 5,
			FirstColdLaunchThreshold = 7,
			NextColdLaunchThreshold = 5
		},
		new SchemeData
		{
			Suffix = "first3next2",
			Scheme = 6,
			FirstColdLaunchThreshold = 3,
			NextColdLaunchThreshold = 2
		},
		new SchemeData
		{
			Suffix = "first2next0",
			Scheme = 7,
			FirstColdLaunchThreshold = 2,
			NextColdLaunchThreshold = 0
		},
		new SchemeData
		{
			Suffix = "first300s",
			Scheme = 8,
			DurationSeconds = 300
		},
		new SchemeData
		{
			Suffix = "first180s",
			Scheme = 9,
			DurationSeconds = 180
		},
		new SchemeData
		{
			Suffix = "first30next5_f3dcd150",
			Scheme = 10,
			FirstColdLaunchThreshold = 30,
			NextColdLaunchThreshold = 5,
			CdMultiplier = 1.5f,
			CdMultiplierMaxInstallDay = 3
		},
		new SchemeData
		{
			Suffix = "first20next10_f3dcd150",
			Scheme = 11,
			FirstColdLaunchThreshold = 20,
			NextColdLaunchThreshold = 10,
			CdMultiplier = 1.5f,
			CdMultiplierMaxInstallDay = 3
		},
		new SchemeData
		{
			Suffix = "first10next6_f3dcd150",
			Scheme = 12,
			FirstColdLaunchThreshold = 10,
			NextColdLaunchThreshold = 6,
			CdMultiplier = 1.5f,
			CdMultiplierMaxInstallDay = 3
		},
		new SchemeData
		{
			Suffix = "first5next3_f3dcd150",
			Scheme = 13,
			FirstColdLaunchThreshold = 5,
			NextColdLaunchThreshold = 3,
			CdMultiplier = 1.5f,
			CdMultiplierMaxInstallDay = 3
		},
		new SchemeData
		{
			Suffix = "first7next5_f3dcd150",
			Scheme = 14,
			FirstColdLaunchThreshold = 7,
			NextColdLaunchThreshold = 5,
			CdMultiplier = 1.5f,
			CdMultiplierMaxInstallDay = 3
		},
		new SchemeData
		{
			Suffix = "first3next2_f3dcd150",
			Scheme = 15,
			FirstColdLaunchThreshold = 3,
			NextColdLaunchThreshold = 2,
			CdMultiplier = 1.5f,
			CdMultiplierMaxInstallDay = 3
		},
		new SchemeData
		{
			Suffix = "first2next0_f3dcd150",
			Scheme = 16,
			FirstColdLaunchThreshold = 2,
			NextColdLaunchThreshold = 0,
			CdMultiplier = 1.5f,
			CdMultiplierMaxInstallDay = 3
		},
		new SchemeData
		{
			Suffix = "first30next5_f3dcd125",
			Scheme = 17,
			FirstColdLaunchThreshold = 30,
			NextColdLaunchThreshold = 5,
			CdMultiplier = 1.25f,
			CdMultiplierMaxInstallDay = 3
		},
		new SchemeData
		{
			Suffix = "first20next10_f3dcd125",
			Scheme = 18,
			FirstColdLaunchThreshold = 20,
			NextColdLaunchThreshold = 10,
			CdMultiplier = 1.25f,
			CdMultiplierMaxInstallDay = 3
		},
		new SchemeData
		{
			Suffix = "first30next5_f1dcd150",
			Scheme = 19,
			FirstColdLaunchThreshold = 30,
			NextColdLaunchThreshold = 5,
			CdMultiplier = 1.5f,
			CdMultiplierMaxInstallDay = 1
		},
		new SchemeData
		{
			Suffix = "first20next10_f1dcd150",
			Scheme = 20,
			FirstColdLaunchThreshold = 20,
			NextColdLaunchThreshold = 10,
			CdMultiplier = 1.5f,
			CdMultiplierMaxInstallDay = 1
		}
	};

	public override string Name => "InstallCampaign";

	private static InstallCampaignRecord GetRecord()
	{
		return GameApp.Save.GetOrCreate<InstallCampaignRecord>();
	}

	private static bool TryMatchScheme(string campaign, out SchemeData scheme, out string matchedSuffix)
	{
		scheme = null;
		matchedSuffix = null;
		if (string.IsNullOrWhiteSpace(campaign))
		{
			return false;
		}
		for (int i = 0; i < Schemes.Length; i++)
		{
			if (campaign.EndsWith(Schemes[i].Suffix, StringComparison.Ordinal))
			{
				scheme = Schemes[i];
				matchedSuffix = Schemes[i].Suffix;
				return true;
			}
		}
		return false;
	}

	public static float GetCdMultiplier()
	{
		if ((FeatureConfig.InterstitialFunction?.InstallCampaign ?? 0) != 1)
		{
			return 1f;
		}
		InstallCampaignRecord record = GetRecord();
		if (!record.Matched && !EnsureMatched(InterstitialPolicyContext.ResolveCampaign()))
		{
			return 1f;
		}
		SchemeData scheme = GetScheme(record.Scheme);
		if (scheme == null || scheme.CdMultiplierMaxInstallDay <= 0)
		{
			return 1f;
		}
		int installDays = AdBridgeHelper.GetInstallDays();
		installDays = ((installDays < 1) ? 1 : installDays);
		if (installDays > scheme.CdMultiplierMaxInstallDay)
		{
			return 1f;
		}
		return scheme.CdMultiplier;
	}

	public static bool EnsureMatched(string campaign)
	{
		InstallCampaignRecord record = GetRecord();
		if (record.Matched)
		{
			return true;
		}
		if (!TryMatchScheme(campaign, out var scheme, out var matchedSuffix))
		{
			if (!string.Equals(lastLoggedCampaign, campaign, StringComparison.Ordinal))
			{
				lastLoggedCampaign = campaign;
			}
			return false;
		}
		record.Matched = true;
		record.MatchedSuffix = matchedSuffix;
		record.Scheme = scheme.Scheme;
		lastLoggedCampaign = campaign;
		return true;
	}

	public static bool ShouldTakeOver(InterstitialPolicyContext context)
	{
		return IsTakeOverActive(context.InterstitialConfig, context.Campaign);
	}

	public static bool IsTakeOverActive()
	{
		return IsTakeOverActive(FeatureConfig.InterstitialFunction, InterstitialPolicyContext.ResolveCampaign());
	}

	private static bool IsTakeOverActive(InterstitialFunction config, string campaign)
	{
		if ((config?.InstallCampaign ?? 0) != 1)
		{
			return false;
		}
		if (!EnsureMatched(campaign))
		{
			return false;
		}
		return GetScheme(GetRecord().Scheme) != null;
	}

	public override InterAdCheckResult Evaluate(InterstitialPolicyContext context)
	{
		if ((context.InterstitialConfig?.InstallCampaign ?? 0) != 1)
		{
			return InterAdCheckResult.Allow;
		}
		if (!EnsureMatched(context.Campaign))
		{
			return InterAdCheckResult.Allow;
		}
		InstallCampaignRecord record = GetRecord();
		SchemeData scheme = GetScheme(record.Scheme);
		if (scheme == null)
		{
			return InterAdCheckResult.Allow;
		}
		bool num = InterstitialEscapeTracker.ShowAdPostSuccessfulCount > 0;
		bool isPostAd = context.IsPostAd;
		if (!num && !isPostAd)
		{
			return InterAdCheckResult.Block("install campaign首个插屏必须是后插屏");
		}
		if (scheme.DurationSeconds > 0)
		{
			float totalValidDurationSeconds = context.TotalValidDurationSeconds;
			if (!(totalValidDurationSeconds >= (float)scheme.DurationSeconds))
			{
				return InterAdCheckResult.Block(AdBlockReason.FormatInstallCampaignDurationBlocked(record.MatchedSuffix, scheme.Scheme, scheme.DurationSeconds, totalValidDurationSeconds));
			}
			return InterAdCheckResult.Allow;
		}
		if (context.LevelProxy == null)
		{
			return InterAdCheckResult.Block("install campaign首插屏门槛不通过");
		}
		int coldLaunchCount = context.ColdLaunchCount;
		int num2 = (context.IsFirstColdLaunch ? scheme.FirstColdLaunchThreshold : scheme.NextColdLaunchThreshold);
		int mainLevel = context.MainLevel;
		int completedLevel = context.CompletedLevel;
		if (!(isPostAd ? (completedLevel >= num2) : (mainLevel >= num2)))
		{
			return InterAdCheckResult.Block(AdBlockReason.FormatInstallCampaignLevelBlocked(record.MatchedSuffix, scheme.Scheme, coldLaunchCount, isPostAd, num2, mainLevel, completedLevel));
		}
		return InterAdCheckResult.Allow;
	}

	public static void ResetStaticForDebug()
	{
		InstallCampaignRecord record = GetRecord();
		record.Matched = false;
		record.MatchedSuffix = string.Empty;
		record.Scheme = 0;
		lastLoggedCampaign = null;
		InterstitialEscapeTracker.ResetSuccessfulInterstitialCount();
		InstallCampaignAttributionHelper.ResetForDebug();
	}

	public override void ResetForDebug()
	{
		ResetStaticForDebug();
	}

	public static string GetDebugState()
	{
		InstallCampaignRecord record = GetRecord();
		string text = InterstitialPolicyContext.ResolveCampaign();
		return string.Format("matched={0} suffix={1} scheme={2} postSuccess={3} attr={4}", record.Matched, string.IsNullOrEmpty(record.MatchedSuffix) ? "none" : record.MatchedSuffix, record.Scheme, InterstitialEscapeTracker.ShowAdPostSuccessfulCount, string.IsNullOrEmpty(text) ? "none" : text);
	}

	public override string BuildDebugState()
	{
		return GetDebugState();
	}

	private static SchemeData GetScheme(int schemeNumber)
	{
		for (int i = 0; i < Schemes.Length; i++)
		{
			if (Schemes[i].Scheme == schemeNumber)
			{
				return Schemes[i];
			}
		}
		return null;
	}
}
