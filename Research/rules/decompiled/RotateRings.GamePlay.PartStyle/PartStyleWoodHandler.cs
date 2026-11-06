using RotateRings.Common;
using RotateRings.GamePlay.Events;
using RotateRings.Services;
using Vibration;

namespace RotateRings.GamePlay.PartStyle;

public sealed class PartStyleWoodHandler : PartStyleHandlerBase
{
	private const string RingMaterialPath = "mat_ring_wood";

	private const string PullMaterialPath = "mat_ring_wood";

	private const string FixedMaterialPath = "mat_catch_wood";

	private const string SolveFragmentMainTexturePath = "tex_posui_wood";

	public override PartStyleType Style => PartStyleType.Wood;

	protected override string GetRingMaterialPath()
	{
		return "mat_ring_wood";
	}

	protected override string GetPullMaterialPath()
	{
		return "mat_ring_wood";
	}

	protected override string GetFixedMaterialPath()
	{
		return "mat_catch_wood";
	}

	protected override string GetSolveFragmentMainTexturePath()
	{
		return "tex_posui_wood";
	}

	public override void PlaySolveBreakSound()
	{
		AudioService.PlayUiSound(AudioKeys.WoodBreak.Value);
		VibrateUtility.Vibrate(1102);
	}

	public override void OnDragCollisionStop(RingDragCollisionStopEvent evt)
	{
		AudioService.PlaySound(AudioKeys.WoodBlocked.Value);
	}
}
