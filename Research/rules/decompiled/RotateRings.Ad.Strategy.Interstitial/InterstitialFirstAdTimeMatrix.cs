using System;
using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.Ad.Strategy.Interstitial;

internal static class InterstitialFirstAdTimeMatrix
{
	private sealed class MatrixSet
	{
		public IReadOnlyDictionary<string, int[]> Android { get; }

		public IReadOnlyDictionary<string, int[]> Ios { get; }

		public MatrixSet(IReadOnlyDictionary<string, int[]> android, IReadOnlyDictionary<string, int[]> ios)
		{
			Android = android;
			Ios = ios;
		}
	}

	private const string Tag = "<Ad>.InterstitialFirstAdTimePolicy";

	private const string RemainingChannel = "剩余渠道";

	private static readonly IReadOnlyDictionary<int, MatrixSet> Matrices = new Dictionary<int, MatrixSet>
	{
		[1] = CreateExperiment1(),
		[2] = CreateExperiment2(),
		[3] = CreateExperiment3(),
		[4] = CreateExperiment4(),
		[5] = CreateExperiment5()
	};

	public static bool IsSupportedExperiment(int experimentId)
	{
		return Matrices.ContainsKey(experimentId);
	}

	public static bool TryGetCurrentThresholdSeconds(int experimentId, int activationDay, out int requiredSeconds)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		RuntimePlatform effectivePlatform = AdBridgeHelper.GetEffectivePlatform();
		string effectiveChannel = AdBridgeHelper.GetEffectiveChannel();
		string.IsNullOrWhiteSpace(effectiveChannel);
		return TryGetThresholdSeconds(experimentId, effectivePlatform, effectiveChannel, activationDay, out requiredSeconds);
	}

	public static bool TryGetThresholdSeconds(int experimentId, RuntimePlatform platform, string channel, int activationDay, out int requiredSeconds)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		requiredSeconds = 0;
		if (!Matrices.TryGetValue(experimentId, out var value))
		{
			return false;
		}
		IReadOnlyDictionary<string, int[]> channelTable;
		if ((int)platform != 8)
		{
			if ((int)platform != 11)
			{
				return false;
			}
			channelTable = value.Android;
		}
		else
		{
			channelTable = value.Ios;
		}
		if (!TryResolveChannel(channelTable, channel, out var thresholds))
		{
			return false;
		}
		int num = ResolveDayIndex(activationDay, thresholds.Length);
		requiredSeconds = thresholds[num];
		return true;
	}

	private static int ResolveDayIndex(int activationDay, int dayCount)
	{
		if (activationDay <= 1)
		{
			return 0;
		}
		if (activationDay < dayCount)
		{
			return activationDay - 1;
		}
		return dayCount - 1;
	}

	private static bool TryResolveChannel(IReadOnlyDictionary<string, int[]> channelTable, string channel, out int[] thresholds)
	{
		if (!string.IsNullOrWhiteSpace(channel) && channelTable.TryGetValue(channel, out thresholds))
		{
			return true;
		}
		return channelTable.TryGetValue("剩余渠道", out thresholds);
	}

	private static MatrixSet CreateExperiment1()
	{
		Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["Facebook Ads"] = new int[7] { 3400, 2880, 2380, 1940, 1420, 1000, 900 };
		dictionary["applovin_int"] = new int[7] { 3120, 2640, 2180, 1780, 1300, 920, 900 };
		dictionary["googleadwords_int"] = new int[7] { 2860, 2420, 2000, 1620, 1180, 900, 900 };
		dictionary["moloco_int"] = new int[7] { 2600, 2200, 1820, 1480, 1080, 900, 900 };
		dictionary["aura_int"] = new int[7] { 2520, 2300, 2080, 1680, 1240, 860, 640 };
		dictionary["tiktokglobal_int"] = new int[7] { 2520, 2140, 1760, 1440, 1040, 900, 900 };
		dictionary["unityads_int"] = new int[7] { 2440, 2060, 1720, 1400, 1020, 900, 900 };
		dictionary["剩余渠道"] = new int[7] { 2340, 1980, 1640, 1340, 980, 900, 900 };
		dictionary["organic"] = new int[7] { 2340, 1980, 1640, 1340, 980, 900, 900 };
		dictionary["xiaomiglobal_int"] = new int[7] { 2100, 1920, 1720, 1400, 1020, 720, 600 };
		dictionary["xiaomipreload_int"] = new int[7] { 2080, 1900, 1720, 1400, 1020, 720, 600 };
		dictionary["oppopaipreinstall_int"] = new int[7] { 2020, 1840, 1660, 1340, 980, 700, 600 };
		dictionary["xiaomipai_int"] = new int[7] { 1980, 1820, 1640, 1340, 980, 680, 600 };
		dictionary["digitalturbine_int"] = new int[7] { 1940, 1780, 1600, 1300, 960, 660, 600 };
		dictionary["vivopreload_int"] = new int[7] { 1820, 1660, 1500, 1220, 880, 620, 600 };
		dictionary["vivootapreload_int"] = new int[7] { 1800, 1640, 1480, 1200, 880, 620, 600 };
		dictionary["shalltrypai_int"] = new int[7] { 1740, 1600, 1440, 1160, 860, 600, 600 };
		dictionary["oppo_int"] = new int[7] { 1660, 1520, 1360, 1120, 800, 600, 600 };
		dictionary["hihonor_int"] = new int[7] { 1660, 1520, 1360, 1120, 800, 600, 600 };
		dictionary["transsionpreinstall_int"] = new int[7] { 1380, 1260, 1140, 920, 680, 600, 600 };
		dictionary["shalltry_int"] = new int[7] { 1240, 1140, 1020, 820, 600, 600, 600 };
		dictionary["oppoglobal_int"] = new int[7] { 1120, 1040, 920, 760, 600, 600, 600 };
		dictionary["vivoglobal_int"] = new int[7] { 1080, 1000, 900, 720, 600, 600, 600 };
		Dictionary<string, int[]> android = dictionary;
		dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["applovin_int"] = new int[7] { 4140, 3540, 2940, 2380, 1740, 1220, 900 };
		dictionary["Facebook Ads"] = new int[7] { 3520, 3000, 2480, 2020, 1460, 1040, 960 };
		dictionary["snapchat_int"] = new int[7] { 3440, 2940, 2440, 1980, 1440, 1020, 940 };
		dictionary["mediago_int"] = new int[7] { 3400, 2900, 2400, 1960, 1420, 1000, 930 };
		dictionary["tiktokglobal_int"] = new int[7] { 3380, 2880, 2380, 1940, 1420, 1000, 930 };
		dictionary["organic"] = new int[7] { 3100, 2640, 2180, 1780, 1300, 920, 920 };
		dictionary["剩余渠道"] = new int[7] { 3100, 2640, 2180, 1780, 1300, 920, 920 };
		dictionary["googleadwords_int"] = new int[7] { 3100, 2640, 2180, 1780, 1300, 920, 920 };
		dictionary["liftoff_int"] = new int[7] { 3080, 2620, 2160, 1760, 1280, 900, 900 };
		dictionary["Apple Search Ads"] = new int[7] { 2940, 2520, 2100, 1700, 1240, 900, 900 };
		dictionary["moloco_int"] = new int[7] { 2620, 2240, 1860, 1500, 1100, 900, 900 };
		dictionary["unityads_int"] = new int[7] { 2120, 1820, 1520, 1220, 900, 900, 900 };
		Dictionary<string, int[]> ios = dictionary;
		return new MatrixSet(android, ios);
	}

	private static MatrixSet CreateExperiment2()
	{
		Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["Facebook Ads"] = new int[6] { 2200, 1880, 1540, 1260, 920, 900 };
		dictionary["applovin_int"] = new int[6] { 2020, 1720, 1420, 1160, 900, 900 };
		dictionary["googleadwords_int"] = new int[6] { 1860, 1580, 1300, 1060, 900, 900 };
		dictionary["moloco_int"] = new int[6] { 1680, 1440, 1180, 960, 900, 900 };
		dictionary["aura_int"] = new int[6] { 1640, 1500, 1360, 1100, 800, 600 };
		dictionary["tiktokglobal_int"] = new int[6] { 1640, 1400, 1140, 940, 900, 900 };
		dictionary["unityads_int"] = new int[6] { 1580, 1340, 1120, 920, 900, 900 };
		dictionary["剩余渠道"] = new int[6] { 1520, 1280, 1060, 900, 900, 900 };
		dictionary["organic"] = new int[6] { 1520, 1280, 1060, 900, 900, 900 };
		dictionary["xiaomiglobal_int"] = new int[6] { 1360, 1240, 1120, 920, 660, 600 };
		dictionary["xiaomipreload_int"] = new int[6] { 1360, 1240, 1120, 920, 660, 600 };
		dictionary["oppopaipreinstall_int"] = new int[6] { 1320, 1200, 1080, 880, 640, 600 };
		dictionary["xiaomipai_int"] = new int[6] { 1280, 1180, 1060, 880, 640, 600 };
		dictionary["digitalturbine_int"] = new int[6] { 1260, 1160, 1040, 840, 620, 600 };
		dictionary["vivopreload_int"] = new int[6] { 1180, 1080, 980, 800, 600, 600 };
		dictionary["vivootapreload_int"] = new int[6] { 1160, 1060, 960, 780, 600, 600 };
		dictionary["shalltrypai_int"] = new int[6] { 1140, 1040, 940, 760, 600, 600 };
		dictionary["oppo_int"] = new int[6] { 1080, 980, 880, 720, 600, 600 };
		dictionary["hihonor_int"] = new int[6] { 1080, 980, 880, 720, 600, 600 };
		dictionary["transsionpreinstall_int"] = new int[6] { 900, 820, 740, 600, 600, 600 };
		dictionary["shalltry_int"] = new int[6] { 800, 740, 660, 600, 600, 600 };
		dictionary["oppoglobal_int"] = new int[6] { 720, 680, 600, 600, 600, 600 };
		dictionary["vivoglobal_int"] = new int[6] { 700, 640, 600, 600, 600, 600 };
		Dictionary<string, int[]> android = dictionary;
		dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["applovin_int"] = new int[6] { 2680, 2300, 1920, 1540, 1140, 900 };
		dictionary["Facebook Ads"] = new int[6] { 2300, 1960, 1620, 1320, 940, 900 };
		dictionary["snapchat_int"] = new int[6] { 2260, 1920, 1580, 1280, 940, 900 };
		dictionary["mediago_int"] = new int[6] { 2200, 1880, 1560, 1280, 920, 900 };
		dictionary["tiktokglobal_int"] = new int[6] { 2200, 1880, 1540, 1260, 920, 900 };
		dictionary["organic"] = new int[6] { 2020, 1720, 1420, 1160, 900, 900 };
		dictionary["剩余渠道"] = new int[6] { 2020, 1720, 1420, 1160, 900, 900 };
		dictionary["googleadwords_int"] = new int[6] { 2020, 1720, 1420, 1160, 900, 900 };
		dictionary["liftoff_int"] = new int[6] { 2000, 1700, 1400, 1140, 900, 900 };
		dictionary["Apple Search Ads"] = new int[6] { 1920, 1640, 1360, 1100, 900, 900 };
		dictionary["moloco_int"] = new int[6] { 1720, 1460, 1200, 980, 900, 900 };
		dictionary["unityads_int"] = new int[6] { 1380, 1180, 980, 900, 900, 900 };
		Dictionary<string, int[]> ios = dictionary;
		return new MatrixSet(android, ios);
	}

	private static MatrixSet CreateExperiment3()
	{
		Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["Facebook Ads"] = new int[6] { 1700, 1440, 1200, 960, 720, 700 };
		dictionary["applovin_int"] = new int[6] { 1560, 1320, 1080, 880, 700, 700 };
		dictionary["googleadwords_int"] = new int[6] { 1440, 1200, 1000, 800, 700, 700 };
		dictionary["moloco_int"] = new int[6] { 1300, 1100, 920, 740, 700, 700 };
		dictionary["aura_int"] = new int[6] { 1260, 1160, 1040, 840, 620, 500 };
		dictionary["tiktokglobal_int"] = new int[6] { 1260, 1080, 880, 720, 700, 700 };
		dictionary["unityads_int"] = new int[6] { 1220, 1040, 860, 700, 700, 700 };
		dictionary["剩余渠道"] = new int[6] { 1160, 1000, 820, 700, 700, 700 };
		dictionary["organic"] = new int[6] { 1160, 1000, 820, 700, 700, 700 };
		dictionary["xiaomiglobal_int"] = new int[6] { 1040, 960, 860, 700, 520, 500 };
		dictionary["xiaomipreload_int"] = new int[6] { 1040, 960, 860, 700, 520, 500 };
		dictionary["oppopaipreinstall_int"] = new int[6] { 1000, 920, 840, 680, 500, 500 };
		dictionary["xiaomipai_int"] = new int[6] { 1000, 920, 820, 680, 500, 500 };
		dictionary["digitalturbine_int"] = new int[6] { 960, 880, 800, 640, 500, 500 };
		dictionary["vivopreload_int"] = new int[6] { 920, 840, 760, 600, 500, 500 };
		dictionary["vivootapreload_int"] = new int[6] { 900, 820, 740, 600, 500, 500 };
		dictionary["shalltrypai_int"] = new int[6] { 880, 800, 720, 580, 500, 500 };
		dictionary["oppo_int"] = new int[6] { 840, 760, 680, 560, 500, 500 };
		dictionary["hihonor_int"] = new int[6] { 840, 760, 680, 560, 500, 500 };
		dictionary["transsionpreinstall_int"] = new int[6] { 680, 640, 560, 500, 500, 500 };
		dictionary["shalltry_int"] = new int[6] { 620, 560, 520, 500, 500, 500 };
		dictionary["oppoglobal_int"] = new int[6] { 560, 520, 500, 500, 500, 500 };
		dictionary["vivoglobal_int"] = new int[6] { 540, 500, 500, 500, 500, 500 };
		Dictionary<string, int[]> android = dictionary;
		dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["applovin_int"] = new int[6] { 2040, 1760, 1480, 1200, 880, 700 };
		dictionary["Facebook Ads"] = new int[6] { 1760, 1500, 1240, 1000, 720, 700 };
		dictionary["snapchat_int"] = new int[6] { 1740, 1480, 1220, 1000, 720, 700 };
		dictionary["mediago_int"] = new int[6] { 1680, 1440, 1200, 980, 720, 700 };
		dictionary["tiktokglobal_int"] = new int[6] { 1680, 1440, 1200, 960, 720, 700 };
		dictionary["organic"] = new int[6] { 1560, 1320, 1080, 880, 700, 700 };
		dictionary["剩余渠道"] = new int[6] { 1560, 1320, 1080, 880, 700, 700 };
		dictionary["googleadwords_int"] = new int[6] { 1560, 1320, 1080, 880, 700, 700 };
		dictionary["liftoff_int"] = new int[6] { 1560, 1320, 1080, 880, 700, 700 };
		dictionary["Apple Search Ads"] = new int[6] { 1480, 1260, 1040, 840, 700, 700 };
		dictionary["moloco_int"] = new int[6] { 1320, 1120, 920, 760, 700, 700 };
		dictionary["unityads_int"] = new int[6] { 1080, 920, 760, 700, 700, 700 };
		Dictionary<string, int[]> ios = dictionary;
		return new MatrixSet(android, ios);
	}

	private static MatrixSet CreateExperiment4()
	{
		Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["Facebook Ads"] = new int[6] { 2720, 2300, 1900, 1560, 1140, 900 };
		dictionary["applovin_int"] = new int[6] { 2500, 2120, 1740, 1420, 1040, 900 };
		dictionary["googleadwords_int"] = new int[6] { 2280, 1940, 1600, 1300, 940, 900 };
		dictionary["moloco_int"] = new int[6] { 2080, 1760, 1460, 1180, 900, 900 };
		dictionary["aura_int"] = new int[6] { 2020, 1840, 1660, 1340, 1000, 680 };
		dictionary["tiktokglobal_int"] = new int[6] { 2020, 1720, 1400, 1160, 900, 900 };
		dictionary["unityads_int"] = new int[6] { 1960, 1640, 1380, 1120, 900, 900 };
		dictionary["剩余渠道"] = new int[6] { 1880, 1580, 1320, 1080, 900, 900 };
		dictionary["organic"] = new int[6] { 1880, 1580, 1320, 1080, 900, 900 };
		dictionary["xiaomiglobal_int"] = new int[6] { 1680, 1540, 1380, 1120, 820, 600 };
		dictionary["xiaomipreload_int"] = new int[6] { 1660, 1520, 1380, 1120, 820, 600 };
		dictionary["oppopaipreinstall_int"] = new int[6] { 1620, 1480, 1320, 1080, 780, 600 };
		dictionary["xiaomipai_int"] = new int[6] { 1580, 1460, 1320, 1080, 780, 600 };
		dictionary["digitalturbine_int"] = new int[6] { 1560, 1420, 1280, 1040, 760, 600 };
		dictionary["vivopreload_int"] = new int[6] { 1460, 1320, 1200, 980, 700, 600 };
		dictionary["vivootapreload_int"] = new int[6] { 1440, 1320, 1180, 960, 700, 600 };
		dictionary["shalltrypai_int"] = new int[6] { 1400, 1280, 1160, 920, 680, 600 };
		dictionary["oppo_int"] = new int[6] { 1320, 1220, 1080, 900, 640, 600 };
		dictionary["hihonor_int"] = new int[6] { 1320, 1220, 1080, 900, 640, 600 };
		dictionary["transsionpreinstall_int"] = new int[6] { 1100, 1000, 920, 740, 600, 600 };
		dictionary["shalltry_int"] = new int[6] { 1000, 920, 820, 660, 600, 600 };
		dictionary["oppoglobal_int"] = new int[6] { 900, 840, 740, 600, 600, 600 };
		dictionary["vivoglobal_int"] = new int[6] { 860, 800, 720, 600, 600, 600 };
		Dictionary<string, int[]> android = dictionary;
		dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["applovin_int"] = new int[6] { 3320, 2840, 2360, 1900, 1400, 900 };
		dictionary["Facebook Ads"] = new int[6] { 2820, 2400, 1980, 1620, 1160, 900 };
		dictionary["snapchat_int"] = new int[6] { 2760, 2360, 1960, 1580, 1160, 900 };
		dictionary["mediago_int"] = new int[6] { 2720, 2320, 1920, 1560, 1140, 900 };
		dictionary["tiktokglobal_int"] = new int[6] { 2700, 2300, 1900, 1560, 1140, 900 };
		dictionary["organic"] = new int[6] { 2500, 2120, 1740, 1420, 1040, 900 };
		dictionary["剩余渠道"] = new int[6] { 2500, 2120, 1740, 1420, 1040, 900 };
		dictionary["googleadwords_int"] = new int[6] { 2500, 2120, 1740, 1420, 1040, 900 };
		dictionary["liftoff_int"] = new int[6] { 2480, 2100, 1720, 1400, 1020, 900 };
		dictionary["Apple Search Ads"] = new int[6] { 2360, 2020, 1680, 1360, 1000, 900 };
		dictionary["moloco_int"] = new int[6] { 2120, 1800, 1480, 1200, 900, 900 };
		dictionary["unityads_int"] = new int[6] { 1700, 1460, 1220, 980, 900, 900 };
		Dictionary<string, int[]> ios = dictionary;
		return new MatrixSet(android, ios);
	}

	private static MatrixSet CreateExperiment5()
	{
		Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["Facebook Ads"] = new int[7] { 3120, 2640, 2180, 1780, 1300, 920, 900 };
		dictionary["applovin_int"] = new int[7] { 2880, 2420, 2000, 1640, 1200, 900, 900 };
		dictionary["googleadwords_int"] = new int[7] { 2640, 2220, 1840, 1500, 1080, 900, 900 };
		dictionary["moloco_int"] = new int[7] { 2400, 2020, 1680, 1360, 1000, 900, 900 };
		dictionary["aura_int"] = new int[7] { 2320, 2120, 1920, 1540, 1140, 800, 600 };
		dictionary["tiktokglobal_int"] = new int[7] { 2320, 1960, 1620, 1320, 960, 900, 900 };
		dictionary["unityads_int"] = new int[7] { 2240, 1900, 1580, 1280, 940, 900, 900 };
		dictionary["剩余渠道"] = new int[7] { 2160, 1820, 1500, 1240, 900, 900, 900 };
		dictionary["organic"] = new int[7] { 2160, 1820, 1500, 1240, 900, 900, 900 };
		dictionary["xiaomiglobal_int"] = new int[7] { 1940, 1760, 1580, 1280, 940, 660, 600 };
		dictionary["xiaomipreload_int"] = new int[7] { 1920, 1740, 1580, 1280, 940, 660, 600 };
		dictionary["oppopaipreinstall_int"] = new int[7] { 1860, 1700, 1520, 1240, 900, 640, 600 };
		dictionary["xiaomipai_int"] = new int[7] { 1820, 1680, 1500, 1240, 900, 620, 600 };
		dictionary["digitalturbine_int"] = new int[7] { 1780, 1640, 1480, 1200, 880, 600, 600 };
		dictionary["vivopreload_int"] = new int[7] { 1680, 1520, 1380, 1120, 800, 600, 600 };
		dictionary["vivootapreload_int"] = new int[7] { 1660, 1500, 1360, 1100, 800, 600, 600 };
		dictionary["shalltrypai_int"] = new int[7] { 1600, 1480, 1320, 1060, 800, 600, 600 };
		dictionary["oppo_int"] = new int[7] { 1520, 1400, 1260, 1040, 740, 600, 600 };
		dictionary["hihonor_int"] = new int[7] { 1520, 1400, 1260, 1040, 740, 600, 600 };
		dictionary["transsionpreinstall_int"] = new int[7] { 1260, 1160, 1040, 840, 620, 600, 600 };
		dictionary["shalltry_int"] = new int[7] { 1140, 1040, 940, 760, 600, 600, 600 };
		dictionary["oppoglobal_int"] = new int[7] { 1040, 960, 840, 700, 600, 600, 600 };
		dictionary["vivoglobal_int"] = new int[7] { 1000, 920, 820, 660, 600, 600, 600 };
		Dictionary<string, int[]> android = dictionary;
		dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["applovin_int"] = new int[7] { 3820, 3260, 2700, 2180, 1600, 1120, 900 };
		dictionary["Facebook Ads"] = new int[7] { 3240, 2760, 2280, 1860, 1340, 960, 900 };
		dictionary["snapchat_int"] = new int[7] { 3160, 2700, 2240, 1820, 1320, 940, 900 };
		dictionary["mediago_int"] = new int[7] { 3120, 2660, 2200, 1800, 1300, 920, 900 };
		dictionary["tiktokglobal_int"] = new int[7] { 3100, 2640, 2180, 1780, 1300, 920, 900 };
		dictionary["organic"] = new int[7] { 2840, 2420, 2000, 1640, 1200, 900, 900 };
		dictionary["剩余渠道"] = new int[7] { 2840, 2420, 2000, 1640, 1200, 900, 900 };
		dictionary["googleadwords_int"] = new int[7] { 2840, 2420, 2000, 1640, 1200, 900, 900 };
		dictionary["liftoff_int"] = new int[7] { 2840, 2420, 1980, 1620, 1180, 900, 900 };
		dictionary["Apple Search Ads"] = new int[7] { 2700, 2320, 1940, 1560, 1140, 900, 900 };
		dictionary["moloco_int"] = new int[7] { 2400, 2060, 1720, 1380, 1020, 900, 900 };
		dictionary["unityads_int"] = new int[7] { 1960, 1680, 1400, 1120, 900, 900, 900 };
		Dictionary<string, int[]> ios = dictionary;
		return new MatrixSet(android, ios);
	}
}
