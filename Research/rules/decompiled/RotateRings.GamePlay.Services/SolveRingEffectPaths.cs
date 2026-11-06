namespace RotateRings.GamePlay.Services;

public readonly struct SolveRingEffectPaths
{
	public string FragmentPath { get; }

	public string BasicFixedPath { get; }

	public string FixedPath { get; }

	public string FixedBridgePath { get; }

	public bool RequiresStyleFragmentMaterialSync { get; }

	public SolveRingEffectPaths(string fragmentPath, string fixedPath, string fixedBridgePath, string basicFixedPath, bool requiresStyleFragmentMaterialSync = false)
	{
		FragmentPath = fragmentPath;
		FixedPath = fixedPath;
		RequiresStyleFragmentMaterialSync = requiresStyleFragmentMaterialSync;
		FixedBridgePath = fixedBridgePath;
		BasicFixedPath = basicFixedPath;
	}
}
