namespace RotateRings.GamePlay.Services;

public static class SolveRingEffectExpGp3
{
	private static readonly string[] FragmentPaths = new string[5] { "eff_cycle_boom_style_fragment_gp3_1", "eff_cycle_boom_style_fragment_gp3_2", "eff_cycle_boom_style_fragment_gp3_3", "eff_cycle_boom_style_fragment_gp3_4", "eff_cycle_boom_style_fragment_gp3_5" };

	private static readonly string[] FixedPaths = new string[5] { "eff_cycle_boom_gp3_1", "eff_cycle_boom_gp3_2", "eff_cycle_boom_gp3_3", "eff_cycle_boom_gp3_4", "eff_cycle_boom_gp3_5" };

	private static readonly SolveRingEffectPaths[] CachedPaths = CreateCachedPaths();

	public static SolveRingEffectPaths Resolve(int variantIndex)
	{
		int num = variantIndex - 1;
		if (num < 0 || num >= CachedPaths.Length)
		{
			return SolveRingEffectExpBaseboard.Paths;
		}
		return CachedPaths[num];
	}

	private static SolveRingEffectPaths[] CreateCachedPaths()
	{
		SolveRingEffectPaths[] array = new SolveRingEffectPaths[FragmentPaths.Length];
		for (int i = 0; i < array.Length; i++)
		{
			bool requiresStyleFragmentMaterialSync = i <= 1;
			array[i] = new SolveRingEffectPaths(FragmentPaths[i], FixedPaths[i], FixedPaths[i], FixedPaths[i], requiresStyleFragmentMaterialSync);
		}
		return array;
	}
}
