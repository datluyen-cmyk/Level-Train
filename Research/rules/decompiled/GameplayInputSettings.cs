using RotateRings;
using UnityEngine;

public static class GameplayInputSettings
{
	private static LayerMask inputLayerMask = GameConst.RingInputLayerMask;

	public static LayerMask InputLayerMask
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return inputLayerMask;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			inputLayerMask = value;
		}
	}

	public static LayerMask DefaultRingInputLayerMask => GameConst.RingInputLayerMask;
}
