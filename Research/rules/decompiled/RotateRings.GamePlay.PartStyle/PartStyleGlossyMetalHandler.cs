using RotateRings.Common;
using RotateRings.Services;
using Vibration;

namespace RotateRings.GamePlay.PartStyle;

public sealed class PartStyleGlossyMetalHandler : PartStyleHandlerBase
{
	private const string RingMaterialPath = "mat_ring_glossy_metal";

	private const string PullMaterialPath = "mat_ring_glossy_metal";

	private const string FixedMaterialPath = "mat_catch_glossy_metal";

	public override PartStyleType Style => PartStyleType.GlossyMetal;

	protected override string GetRingMaterialPath()
	{
		return "mat_ring_glossy_metal";
	}

	protected override string GetPullMaterialPath()
	{
		return "mat_ring_glossy_metal";
	}

	protected override string GetFixedMaterialPath()
	{
		return "mat_catch_glossy_metal";
	}

	public override void PlaySolveBreakSound()
	{
		AudioService.PlayUiSound(AudioKeys.MetalRingBreak.Value);
		VibrateUtility.Vibrate(1102);
	}
}
