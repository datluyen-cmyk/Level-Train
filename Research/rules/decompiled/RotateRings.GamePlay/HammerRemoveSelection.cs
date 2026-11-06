using UnityEngine;

namespace RotateRings.GamePlay;

public readonly struct HammerRemoveSelection
{
	public IGameplayObject Target { get; }

	public Vector3 WorldPosition { get; }

	public HammerRemoveSelection(IGameplayObject target, Vector3 worldPosition)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Target = target;
		WorldPosition = worldPosition;
	}
}
