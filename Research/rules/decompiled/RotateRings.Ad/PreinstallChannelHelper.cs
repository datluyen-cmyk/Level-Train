using System;
using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.Ad;

public static class PreinstallChannelHelper
{
	private static readonly HashSet<string> PreinstallChannels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"oppopaipreinstall_int", "vivootapreload_int", "xiaomipai_int", "aura_int", "digitalturbine_int", "xiaomipreload_int", "TCL", "honordevicm_int", "transsionpreinstall_int", "tctmobilewp_int",
		"huaweiadsglobal_int", "xiaomiglobal_int", "onedigitalturbine_int", "shalltrypai_int", "vivopreload_int", "lenovotabpai4p_int", "oppo_int", "vivoglobal_int", "appshot_int", "oppoglobal_int",
		"hihonor_int", "ztepai_int", "coolpadzhf_int", "nubia8rk_int", "ztesw_int", "TCL-Store", "shalltry_int", "XIAOMI-Onelink"
	};

	private static bool? cachedIsPreinstall;

	public static bool IsPreinstallChannel(string channel)
	{
		if (!string.IsNullOrWhiteSpace(channel))
		{
			return PreinstallChannels.Contains(channel);
		}
		return false;
	}

	public static bool IsCurrentUserPreinstall()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)Application.platform != 11)
		{
			return false;
		}
		string gmChannel = AdDebugSettings.GmChannel;
		if (!string.IsNullOrWhiteSpace(gmChannel))
		{
			return IsPreinstallChannel(gmChannel);
		}
		if (cachedIsPreinstall.HasValue)
		{
			return cachedIsPreinstall.Value;
		}
		string effectiveChannel = AdBridgeHelper.GetEffectiveChannel();
		if (string.IsNullOrWhiteSpace(effectiveChannel))
		{
			return false;
		}
		cachedIsPreinstall = IsPreinstallChannel(effectiveChannel);
		return cachedIsPreinstall.Value;
	}
}
