using UnityEngine;

namespace RotateRings.GamePlay.MultiTouchGuide;

public static class MultiTouchInputUtility
{
	public static void EnsureMultiTouchEnabled()
	{
		if (!Input.multiTouchEnabled)
		{
			Input.multiTouchEnabled = true;
		}
	}
}
