using RotateRings.Common;
using RotateRings.GamePlay.Events;
using RotateRings.Services;
using Vibration;

namespace RotateRings.GamePlay.PartStyle;

public sealed class PartStyleMarbleHandler : PartStyleHandlerBase
{
	private const string RingMaterialPath = "RingMarbleExtra";

	private const string PullMaterialPath = "RingMarbleExtra";

	private const string FixedMaterialPath = "mat_catch_wood";

	public override PartStyleType Style => PartStyleType.Marble;

	protected override string GetRingMaterialPath()
	{
		return "RingMarbleExtra";
	}

	protected override string GetPullMaterialPath()
	{
		return "RingMarbleExtra";
	}

	protected override string GetFixedMaterialPath()
	{
		return "mat_catch_wood";
	}

	public override void PlaySolveBreakSound()
	{
		AudioService.PlayUiSound(AudioKeys.RockBreak.Value);
		VibrateUtility.Vibrate(1102);
	}

	public override void OnDragCollisionStop(RingDragCollisionStopEvent evt)
	{
		AudioService.PlaySound(AudioKeys.RockBlocked.Value);
	}
}
