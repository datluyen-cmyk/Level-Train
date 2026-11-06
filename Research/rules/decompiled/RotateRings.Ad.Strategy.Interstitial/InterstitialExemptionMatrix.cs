using System;
using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.Ad.Strategy.Interstitial;

internal static class InterstitialExemptionMatrix
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

	private const string Tag = "<Ad>.AdditionalExemption";

	public const int ExemptionDisabled = -1;

	private const string RemainingChannel = "organic/其他+未归因";

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

	public static bool TryGetCurrentExemptionSeconds(int experimentId, int activationDay, out int exemptionSeconds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return TryGetExemptionSeconds(experimentId, AdBridgeHelper.GetEffectivePlatform(), AdBridgeHelper.GetEffectiveChannel(), activationDay, out exemptionSeconds);
	}

	public static bool TryGetExemptionSeconds(int experimentId, RuntimePlatform platform, string channel, int activationDay, out int exemptionSeconds)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		exemptionSeconds = 0;
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
		if (!TryResolveChannel(channelTable, channel, out var exemptions))
		{
			return false;
		}
		int num = ResolveDayIndex(activationDay, exemptions.Length);
		exemptionSeconds = exemptions[num];
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

	private static bool TryResolveChannel(IReadOnlyDictionary<string, int[]> channelTable, string channel, out int[] exemptions)
	{
		if (!string.IsNullOrWhiteSpace(channel) && channelTable.TryGetValue(channel, out exemptions))
		{
			return true;
		}
		return channelTable.TryGetValue("organic/其他+未归因", out exemptions);
	}

	private static MatrixSet CreateExperiment1()
	{
		Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["Facebook Ads"] = new int[7] { 1745, 875, 435, 220, 110, 55, -1 };
		dictionary["applovin_int"] = new int[7] { 1600, 800, 400, 200, 100, 50, -1 };
		dictionary["aura_int"] = new int[7] { 1520, 760, 380, 190, 95, 50, -1 };
		dictionary["googleadwords_int"] = new int[7] { 1465, 735, 365, 185, 90, 45, -1 };
		dictionary["moloco_int"] = new int[7] { 1335, 665, 335, 165, 85, 40, -1 };
		dictionary["tiktokglobal_int"] = new int[7] { 1295, 645, 325, 160, 80, 40, -1 };
		dictionary["xiaomiglobal_int"] = new int[7] { 1265, 635, 315, 160, 80, 40, -1 };
		dictionary["unityads_int"] = new int[7] { 1255, 625, 315, 155, 80, 40, -1 };
		dictionary["xiaomipreload_int"] = new int[7] { 1255, 625, 315, 155, 80, 40, -1 };
		dictionary["oppopaipreinstall_int"] = new int[7] { 1215, 605, 305, 150, 75, 40, -1 };
		dictionary["organic/其他+未归因"] = new int[7] { 1200, 600, 300, 150, 75, 40, -1 };
		dictionary["xiaomipai_int"] = new int[7] { 1200, 600, 300, 150, 75, 40, -1 };
		dictionary["digitalturbine_int"] = new int[7] { 1175, 585, 295, 145, 75, 35, -1 };
		dictionary["vivopreload_int"] = new int[7] { 1095, 545, 275, 135, 70, 35, -1 };
		dictionary["shalltrypai_int"] = new int[7] { 1055, 525, 265, 130, 65, 35, -1 };
		dictionary["shalltry_int"] = new int[7] { 745, 375, 185, 95, 45, 25, -1 };
		dictionary["oppoglobal_int"] = new int[7] { 680, 340, 170, 85, 45, 20, -1 };
		dictionary["vivoglobal_int"] = new int[7] { 655, 325, 165, 80, 40, 20, -1 };
		Dictionary<string, int[]> android = dictionary;
		dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["applovin_int"] = new int[7] { 1610, 805, 405, 200, 100, 50, -1 };
		dictionary["Facebook Ads"] = new int[7] { 1360, 680, 340, 170, 85, 45, -1 };
		dictionary["snapchat_int"] = new int[7] { 1340, 670, 335, 170, 85, 40, -1 };
		dictionary["mediago_int"] = new int[7] { 1320, 660, 330, 165, 85, 40, -1 };
		dictionary["tiktokglobal_int"] = new int[7] { 1310, 655, 330, 165, 80, 40, -1 };
		dictionary["organic/其他+未归因"] = new int[7] { 1200, 600, 300, 150, 75, 40, -1 };
		dictionary["googleadwords_int"] = new int[7] { 1200, 600, 300, 150, 75, 40, -1 };
		dictionary["liftoff_int"] = new int[7] { 1190, 595, 300, 150, 75, 35, -1 };
		dictionary["Apple Search Ads"] = new int[7] { 1150, 575, 290, 145, 70, 35, -1 };
		dictionary["moloco_int"] = new int[7] { 1020, 510, 255, 130, 65, 30, -1 };
		dictionary["unityads_int"] = new int[7] { 830, 415, 210, 105, 50, 25, -1 };
		Dictionary<string, int[]> ios = dictionary;
		return new MatrixSet(android, ios);
	}

	private static MatrixSet CreateExperiment2()
	{
		Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["Facebook Ads"] = new int[7] { 1165, 580, 290, 145, 75, 35, -1 };
		dictionary["applovin_int"] = new int[7] { 1065, 535, 265, 135, 65, 35, -1 };
		dictionary["aura_int"] = new int[7] { 1015, 505, 255, 125, 65, 30, -1 };
		dictionary["googleadwords_int"] = new int[7] { 980, 490, 245, 120, 60, 30, -1 };
		dictionary["moloco_int"] = new int[7] { 890, 445, 220, 110, 55, 30, -1 };
		dictionary["tiktokglobal_int"] = new int[7] { 860, 430, 215, 110, 55, 25, -1 };
		dictionary["xiaomiglobal_int"] = new int[7] { 845, 420, 210, 105, 55, 25, -1 };
		dictionary["unityads_int"] = new int[7] { 835, 420, 210, 105, 50, 25, -1 };
		dictionary["xiaomipreload_int"] = new int[7] { 835, 420, 210, 105, 50, 25, -1 };
		dictionary["oppopaipreinstall_int"] = new int[7] { 810, 405, 200, 100, 50, 25, -1 };
		dictionary["organic/其他+未归因"] = new int[7] { 800, 400, 200, 100, 50, 25, -1 };
		dictionary["xiaomipai_int"] = new int[7] { 800, 400, 200, 100, 50, 25, -1 };
		dictionary["digitalturbine_int"] = new int[7] { 780, 390, 195, 100, 50, 25, -1 };
		dictionary["vivopreload_int"] = new int[7] { 730, 365, 180, 90, 45, 25, -1 };
		dictionary["shalltrypai_int"] = new int[7] { 700, 350, 175, 90, 45, 20, -1 };
		dictionary["shalltry_int"] = new int[7] { 500, 250, 125, 60, 30, 15, -1 };
		dictionary["oppoglobal_int"] = new int[7] { 455, 225, 115, 55, 30, 15, -1 };
		dictionary["vivoglobal_int"] = new int[7] { 435, 220, 110, 55, 25, 15, -1 };
		Dictionary<string, int[]> android = dictionary;
		dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["applovin_int"] = new int[7] { 1070, 535, 270, 135, 65, 35, -1 };
		dictionary["Facebook Ads"] = new int[7] { 910, 455, 225, 115, 55, 30, -1 };
		dictionary["snapchat_int"] = new int[7] { 890, 445, 225, 110, 55, 30, -1 };
		dictionary["mediago_int"] = new int[7] { 880, 440, 220, 110, 55, 30, -1 };
		dictionary["tiktokglobal_int"] = new int[7] { 870, 435, 220, 110, 55, 25, -1 };
		dictionary["organic/其他+未归因"] = new int[7] { 800, 400, 200, 100, 50, 25, -1 };
		dictionary["googleadwords_int"] = new int[7] { 800, 400, 200, 100, 50, 25, -1 };
		dictionary["liftoff_int"] = new int[7] { 790, 395, 200, 100, 50, 25, -1 };
		dictionary["Apple Search Ads"] = new int[7] { 770, 385, 190, 95, 50, 25, -1 };
		dictionary["moloco_int"] = new int[7] { 680, 340, 170, 85, 45, 20, -1 };
		dictionary["unityads_int"] = new int[7] { 550, 275, 140, 70, 35, 15, -1 };
		Dictionary<string, int[]> ios = dictionary;
		return new MatrixSet(android, ios);
	}

	private static MatrixSet CreateExperiment3()
	{
		Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["Facebook Ads"] = new int[4] { 1745, 875, 435, -1 };
		dictionary["applovin_int"] = new int[4] { 1600, 800, 400, -1 };
		dictionary["aura_int"] = new int[4] { 1520, 760, 380, -1 };
		dictionary["googleadwords_int"] = new int[4] { 1465, 735, 365, -1 };
		dictionary["moloco_int"] = new int[4] { 1335, 665, 335, -1 };
		dictionary["tiktokglobal_int"] = new int[4] { 1295, 645, 325, -1 };
		dictionary["xiaomiglobal_int"] = new int[4] { 1265, 635, 315, -1 };
		dictionary["unityads_int"] = new int[4] { 1255, 625, 315, -1 };
		dictionary["xiaomipreload_int"] = new int[4] { 1255, 625, 315, -1 };
		dictionary["oppopaipreinstall_int"] = new int[4] { 1215, 605, 305, -1 };
		dictionary["organic/其他+未归因"] = new int[4] { 1200, 600, 300, -1 };
		dictionary["xiaomipai_int"] = new int[4] { 1200, 600, 300, -1 };
		dictionary["digitalturbine_int"] = new int[4] { 1175, 585, 295, -1 };
		dictionary["vivopreload_int"] = new int[4] { 1095, 545, 275, -1 };
		dictionary["shalltrypai_int"] = new int[4] { 1055, 525, 265, -1 };
		dictionary["shalltry_int"] = new int[4] { 745, 375, 185, -1 };
		dictionary["oppoglobal_int"] = new int[4] { 680, 340, 170, -1 };
		dictionary["vivoglobal_int"] = new int[4] { 655, 325, 165, -1 };
		Dictionary<string, int[]> android = dictionary;
		dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["applovin_int"] = new int[4] { 1610, 805, 405, -1 };
		dictionary["Facebook Ads"] = new int[4] { 1360, 680, 340, -1 };
		dictionary["snapchat_int"] = new int[4] { 1340, 670, 335, -1 };
		dictionary["mediago_int"] = new int[4] { 1320, 660, 330, -1 };
		dictionary["tiktokglobal_int"] = new int[4] { 1310, 655, 330, -1 };
		dictionary["organic/其他+未归因"] = new int[4] { 1200, 600, 300, -1 };
		dictionary["googleadwords_int"] = new int[4] { 1200, 600, 300, -1 };
		dictionary["liftoff_int"] = new int[4] { 1190, 595, 300, -1 };
		dictionary["Apple Search Ads"] = new int[4] { 1150, 575, 290, -1 };
		dictionary["moloco_int"] = new int[4] { 1020, 510, 255, -1 };
		dictionary["unityads_int"] = new int[4] { 830, 415, 210, -1 };
		Dictionary<string, int[]> ios = dictionary;
		return new MatrixSet(android, ios);
	}

	private static MatrixSet CreateExperiment4()
	{
		Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["Facebook Ads"] = new int[4] { 1450, 725, 360, -1 };
		dictionary["applovin_int"] = new int[4] { 1385, 695, 345, -1 };
		dictionary["aura_int"] = new int[4] { 1350, 675, 340, -1 };
		dictionary["googleadwords_int"] = new int[4] { 1325, 665, 330, -1 };
		dictionary["moloco_int"] = new int[4] { 1265, 630, 315, -1 };
		dictionary["tiktokglobal_int"] = new int[4] { 1245, 625, 310, -1 };
		dictionary["xiaomiglobal_int"] = new int[4] { 1235, 615, 310, -1 };
		dictionary["unityads_int"] = new int[4] { 1225, 615, 305, -1 };
		dictionary["xiaomipreload_int"] = new int[4] { 1225, 615, 305, -1 };
		dictionary["oppopaipreinstall_int"] = new int[4] { 1205, 605, 300, -1 };
		dictionary["organic/其他+未归因"] = new int[4] { 1200, 600, 300, -1 };
		dictionary["xiaomipai_int"] = new int[4] { 1200, 600, 300, -1 };
		dictionary["digitalturbine_int"] = new int[4] { 1185, 595, 295, -1 };
		dictionary["vivopreload_int"] = new int[4] { 1145, 575, 285, -1 };
		dictionary["shalltrypai_int"] = new int[4] { 1125, 560, 280, -1 };
		dictionary["shalltry_int"] = new int[4] { 945, 475, 235, -1 };
		dictionary["oppoglobal_int"] = new int[4] { 905, 450, 225, -1 };
		dictionary["vivoglobal_int"] = new int[4] { 885, 445, 220, -1 };
		Dictionary<string, int[]> android = dictionary;
		dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["applovin_int"] = new int[4] { 1390, 695, 345, -1 };
		dictionary["Facebook Ads"] = new int[4] { 1280, 640, 320, -1 };
		dictionary["snapchat_int"] = new int[4] { 1270, 635, 315, -1 };
		dictionary["mediago_int"] = new int[4] { 1260, 630, 315, -1 };
		dictionary["tiktokglobal_int"] = new int[4] { 1250, 625, 315, -1 };
		dictionary["organic/其他+未归因"] = new int[4] { 1200, 600, 300, -1 };
		dictionary["googleadwords_int"] = new int[4] { 1200, 600, 300, -1 };
		dictionary["liftoff_int"] = new int[4] { 1190, 595, 300, -1 };
		dictionary["Apple Search Ads"] = new int[4] { 1170, 585, 295, -1 };
		dictionary["moloco_int"] = new int[4] { 1110, 555, 275, -1 };
		dictionary["unityads_int"] = new int[4] { 1000, 500, 250, -1 };
		Dictionary<string, int[]> ios = dictionary;
		return new MatrixSet(android, ios);
	}

	private static MatrixSet CreateExperiment5()
	{
		Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["Facebook Ads"] = new int[4] { 1165, 580, 290, -1 };
		dictionary["applovin_int"] = new int[4] { 1065, 535, 265, -1 };
		dictionary["aura_int"] = new int[4] { 1015, 505, 255, -1 };
		dictionary["googleadwords_int"] = new int[4] { 980, 490, 245, -1 };
		dictionary["moloco_int"] = new int[4] { 890, 445, 220, -1 };
		dictionary["tiktokglobal_int"] = new int[4] { 860, 430, 215, -1 };
		dictionary["xiaomiglobal_int"] = new int[4] { 845, 420, 210, -1 };
		dictionary["unityads_int"] = new int[4] { 835, 420, 210, -1 };
		dictionary["xiaomipreload_int"] = new int[4] { 835, 420, 210, -1 };
		dictionary["oppopaipreinstall_int"] = new int[4] { 810, 405, 200, -1 };
		dictionary["organic/其他+未归因"] = new int[4] { 800, 400, 200, -1 };
		dictionary["xiaomipai_int"] = new int[4] { 800, 400, 200, -1 };
		dictionary["digitalturbine_int"] = new int[4] { 780, 390, 195, -1 };
		dictionary["vivopreload_int"] = new int[4] { 730, 365, 180, -1 };
		dictionary["shalltrypai_int"] = new int[4] { 700, 350, 175, -1 };
		dictionary["shalltry_int"] = new int[4] { 500, 250, 125, -1 };
		dictionary["oppoglobal_int"] = new int[4] { 455, 225, 115, -1 };
		dictionary["vivoglobal_int"] = new int[4] { 435, 220, 110, -1 };
		Dictionary<string, int[]> android = dictionary;
		dictionary = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
		dictionary["applovin_int"] = new int[4] { 1070, 535, 270, -1 };
		dictionary["Facebook Ads"] = new int[4] { 910, 455, 225, -1 };
		dictionary["snapchat_int"] = new int[4] { 890, 445, 225, -1 };
		dictionary["mediago_int"] = new int[4] { 880, 440, 220, -1 };
		dictionary["tiktokglobal_int"] = new int[4] { 870, 435, 220, -1 };
		dictionary["organic/其他+未归因"] = new int[4] { 800, 400, 200, -1 };
		dictionary["googleadwords_int"] = new int[4] { 800, 400, 200, -1 };
		dictionary["liftoff_int"] = new int[4] { 790, 395, 200, -1 };
		dictionary["Apple Search Ads"] = new int[4] { 770, 385, 190, -1 };
		dictionary["moloco_int"] = new int[4] { 680, 340, 170, -1 };
		dictionary["unityads_int"] = new int[4] { 500, 275, 140, -1 };
		Dictionary<string, int[]> ios = dictionary;
		return new MatrixSet(android, ios);
	}
}
