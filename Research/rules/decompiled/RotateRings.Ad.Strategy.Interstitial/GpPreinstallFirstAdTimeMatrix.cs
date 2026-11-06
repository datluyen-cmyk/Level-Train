using System;
using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.Ad.Strategy.Interstitial;

internal static class GpPreinstallFirstAdTimeMatrix
{
	private const string Tag = "<Ad>.GpPreinstallFirstAdTimePolicy";

	private const string RemainingPreinstallChannel = "*";

	private const string OrganicChannel = "organic";

	private static readonly IReadOnlyDictionary<int, IReadOnlyDictionary<string, int>> Matrices = new Dictionary<int, IReadOnlyDictionary<string, int>>
	{
		[1] = CreateScheme1(),
		[2] = CreateScheme2(),
		[3] = CreateScheme3(),
		[4] = CreateScheme4(),
		[5] = CreateScheme5(),
		[6] = CreateScheme6()
	};

	public static bool IsSupportedScheme(int scheme)
	{
		return Matrices.ContainsKey(scheme);
	}

	public static bool IsOrganic(string channel)
	{
		if (!string.IsNullOrWhiteSpace(channel))
		{
			return string.Equals(channel, "organic", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public static bool TryGetThresholdSeconds(int scheme, RuntimePlatform platform, string channel, out int requiredSeconds)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		requiredSeconds = 0;
		if ((int)platform != 11)
		{
			return false;
		}
		if (!Matrices.TryGetValue(scheme, out var value))
		{
			return false;
		}
		if (!string.IsNullOrWhiteSpace(channel) && value.TryGetValue(channel, out requiredSeconds))
		{
			return true;
		}
		if (PreinstallChannelHelper.IsPreinstallChannel(channel) && value.TryGetValue("*", out requiredSeconds))
		{
			return true;
		}
		return false;
	}

	private static IReadOnlyDictionary<string, int> CreateScheme1()
	{
		return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
		{
			["onedigitalturbine_int"] = 750,
			["aura_int"] = 600,
			["oppoglobal_int"] = 900,
			["vivoglobal_int"] = 900,
			["xiaomiglobal_int"] = 480,
			["honordevicm_int"] = 1000,
			["vivootapreload_int"] = 900,
			["oppopaipreinstall_int"] = 480,
			["shalltry_int"] = 240,
			["*"] = 750,
			["organic"] = 350
		};
	}

	private static IReadOnlyDictionary<string, int> CreateScheme2()
	{
		return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
		{
			["onedigitalturbine_int"] = 500,
			["aura_int"] = 400,
			["oppoglobal_int"] = 600,
			["vivoglobal_int"] = 600,
			["xiaomiglobal_int"] = 300,
			["honordevicm_int"] = 700,
			["vivootapreload_int"] = 600,
			["oppopaipreinstall_int"] = 300,
			["shalltry_int"] = 180,
			["*"] = 450,
			["organic"] = 250
		};
	}

	private static IReadOnlyDictionary<string, int> CreateScheme3()
	{
		return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
		{
			["onedigitalturbine_int"] = 250,
			["aura_int"] = 200,
			["oppoglobal_int"] = 300,
			["vivoglobal_int"] = 300,
			["xiaomiglobal_int"] = 160,
			["honordevicm_int"] = 350,
			["vivootapreload_int"] = 300,
			["oppopaipreinstall_int"] = 160,
			["shalltry_int"] = 80,
			["*"] = 250,
			["organic"] = 120
		};
	}

	private static IReadOnlyDictionary<string, int> CreateScheme4()
	{
		return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
		{
			["onedigitalturbine_int"] = 750,
			["aura_int"] = 600,
			["oppoglobal_int"] = 900,
			["vivoglobal_int"] = 900,
			["xiaomiglobal_int"] = 480,
			["honordevicm_int"] = 1000,
			["vivootapreload_int"] = 900,
			["oppopaipreinstall_int"] = 480,
			["shalltry_int"] = 240,
			["*"] = 750
		};
	}

	private static IReadOnlyDictionary<string, int> CreateScheme5()
	{
		return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
		{
			["onedigitalturbine_int"] = 500,
			["aura_int"] = 400,
			["oppoglobal_int"] = 600,
			["vivoglobal_int"] = 600,
			["xiaomiglobal_int"] = 300,
			["honordevicm_int"] = 700,
			["vivootapreload_int"] = 600,
			["oppopaipreinstall_int"] = 300,
			["shalltry_int"] = 180,
			["*"] = 450
		};
	}

	private static IReadOnlyDictionary<string, int> CreateScheme6()
	{
		return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
		{
			["onedigitalturbine_int"] = 250,
			["aura_int"] = 200,
			["oppoglobal_int"] = 300,
			["vivoglobal_int"] = 300,
			["xiaomiglobal_int"] = 160,
			["honordevicm_int"] = 350,
			["vivootapreload_int"] = 300,
			["oppopaipreinstall_int"] = 160,
			["shalltry_int"] = 80,
			["*"] = 250
		};
	}
}
