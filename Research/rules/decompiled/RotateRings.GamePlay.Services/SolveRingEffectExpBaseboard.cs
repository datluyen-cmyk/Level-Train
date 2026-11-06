namespace RotateRings.GamePlay.Services;

public static class SolveRingEffectExpBaseboard
{
	public const string FragmentPath = "eff_cycle_boom_style_fragment";

	public const string BasicFixedPath = "eff_cycle_boom_style_fragment_gp4_1";

	public const string FixedPath = "eff_cycle_boom_style_fragment_gp4_2";

	public const string FixedBridgePath = "eff_cycle_boom_style_fragment_gp4_3";

	public static readonly SolveRingEffectPaths Paths = new SolveRingEffectPaths("eff_cycle_boom_style_fragment", "eff_cycle_boom_style_fragment_gp4_2", "eff_cycle_boom_style_fragment_gp4_3", "eff_cycle_boom_style_fragment_gp4_1", requiresStyleFragmentMaterialSync: true);

	public static SolveRingEffectPaths Get()
	{
		return Paths;
	}
}
