using RotateRings.Services;
using XingYun.Experiment;

namespace RotateRings.Common;

public static class HardLevelTipStyleResolver
{
	private const string TAG = "[HardLevelTipStyleResolver] ";

	public const int BaselineStyleId = 1;

	public const int SquirrelStyleId = 2;

	public const int DemonRingStyleId = 3;

	public const string AudioVariantId = "variant_f80becfb";

	public static int Resolve()
	{
		int num = FunctionDataCenter.Get<HardLevelFunction>()?.BannerStyleId ?? 1;
		if (num != 1 && num != 2 && num != 3)
		{
			return 1;
		}
		return num;
	}

	public static int ResolveAndSyncAudio()
	{
		int num = Resolve();
		SyncAudioExperiment(num);
		return num;
	}

	private static void SyncAudioExperiment(int styleId)
	{
		if (styleId != 1)
		{
			AudioService.ActivateExperiment("variant_f80becfb");
		}
		else
		{
			AudioService.DeactivateExperiment("variant_f80becfb");
		}
	}
}
