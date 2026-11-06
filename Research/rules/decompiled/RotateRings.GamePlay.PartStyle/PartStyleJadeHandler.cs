using RotateRings.Common;
using RotateRings.GamePlay.Events;
using RotateRings.Services;
using Vibration;

namespace RotateRings.GamePlay.PartStyle;

public sealed class PartStyleJadeHandler : PartStyleHandlerBase
{
	private const string RingMaterialPath = "RingJadeExtra";

	private const string PullMaterialPath = "RingJadeExtra";

	private const string PaletteLockGapMaterialPath = "RingJadeExtra_Palette";

	private const string FixedMaterialPath = "MatCatch";

	private const string SolveFragmentMainTexturePath = "tex_posui_jade";

	public override PartStyleType Style => PartStyleType.Jade;

	protected override string GetRingMaterialPath()
	{
		return "RingJadeExtra";
	}

	protected override string GetPullMaterialPath()
	{
		return "RingJadeExtra";
	}

	protected override string GetFixedMaterialPath()
	{
		return "MatCatch";
	}

	protected override string GetPaletteLockGapMaterialPath()
	{
		return "RingJadeExtra_Palette";
	}

	protected override string GetSolveFragmentMainTexturePath()
	{
		return "tex_posui_jade";
	}

	public override void PlaySolveBreakSound()
	{
		AudioService.PlayUiSound(AudioKeys.JadeBreak.Value);
		VibrateUtility.Vibrate(1102);
	}

	public override void OnDragCollisionStop(RingDragCollisionStopEvent evt)
	{
		AudioService.PlaySound(AudioKeys.RockBlocked.Value);
	}
}
