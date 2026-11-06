using UnityEngine;

namespace RotateRings.Tool;

public class NetUtil
{
	public static string connectionType = "Unknown";

	public static bool IsHaveNet
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Invalid comparison between Unknown and I4
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Invalid comparison between Unknown and I4
			NetworkReachability internetReachability = Application.internetReachability;
			bool result = (int)internetReachability > 0;
			if ((int)internetReachability == 1)
			{
				connectionType = "数据网络";
			}
			else if ((int)internetReachability == 2)
			{
				connectionType = "Wi-Fi网络";
			}
			return result;
		}
	}
}
