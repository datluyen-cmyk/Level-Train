using UnityEngine;
using XingYun.Render;

namespace RotateRings.GamePlay.Services;

public static class SolveFragmentPathLengthUtil
{
	public static bool TryCompute(IGameplayObject source, out float length)
	{
		length = 0f;
		if (source == null)
		{
			return false;
		}
		if (source is RingObject ringObject)
		{
			return ringObject.TryGetBandArcLength(out length);
		}
		if (source is PullObject pullObject)
		{
			ArrowRibbonController component = ((Component)pullObject).GetComponent<ArrowRibbonController>();
			if ((Object)(object)component != (Object)null && component.TryGetCenterlinePathLength(out length))
			{
				return true;
			}
		}
		return false;
	}
}
