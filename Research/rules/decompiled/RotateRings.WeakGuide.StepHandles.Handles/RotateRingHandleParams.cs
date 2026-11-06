using System;

namespace RotateRings.WeakGuide.StepHandles.Handles;

[Serializable]
public sealed class RotateRingHandleParams
{
	public string TargetRingPhysicPartId = "ring_2";

	public float TotalRotation = 180f;

	public float RotationDuration = 0.7f;

	public float HighlightFadeDuration = 0.3f;

	public RotationDirection Direction = RotationDirection.Clockwise;

	public string GuideTextKey = "guide_text_drag_ring";

	public float HighlightZOffset = -0.015f;

	public string AdjustOtherRingPhysicPartId = "";

	public float AdjustOtherRingCrossBarZ = -0.01f;

	public float AdjustOtherRingZ;

	public float HandAngleFromGapDeg = 136f;
}
