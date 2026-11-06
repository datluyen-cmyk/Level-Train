using RotateRings.Common;
using RotateRings.Services;
using Vibration;

namespace RotateRings.GamePlay.PartStyle;

public sealed class PartStyleGoldHandler : PartStyleHandlerBase
{
	private const string RingMaterialPath = "mat_ring_gold";

	private const string PullMaterialPath = "mat_ring_gold";

	private const string FixedMaterialPath = "mat_catch_gold";

	private const string SolveFragmentMainTexturePath = "tex_posui_gold";

	private const string SolveFragmentAddonEffectPath = "eff_cycle_boom_style_fragment_gp5_1";

	public override PartStyleType Style => PartStyleType.Gold;

	protected override string GetRingMaterialPath()
	{
		return "mat_ring_gold";
	}

	protected override string GetPullMaterialPath()
	{
		return "mat_ring_gold";
	}

	protected override string GetFixedMaterialPath()
	{
		return "mat_catch_gold";
	}

	protected override string GetSolveFragmentMainTexturePath()
	{
		return "tex_posui_gold";
	}

	public override string GetSolveFragmentAddonEffectPath()
	{
		return "eff_cycle_boom_style_fragment_gp5_1";
	}

	public override void PlaySolveBreakSound()
	{
		AudioService.PlayUiSound(AudioKeys.GoldBreak.Value);
		VibrateUtility.Vibrate(1102);
	}
}
