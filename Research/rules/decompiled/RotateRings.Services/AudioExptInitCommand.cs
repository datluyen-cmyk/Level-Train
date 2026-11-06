using Game.Common.Services;
using RotateRings.Halloween;
using XingYun.Command;
using XingYun.Experiment;

namespace RotateRings.Services;

public class AudioExptInitCommand : BaseCommand
{
	private const string DefaultEncouragementVoVariantId = "variant_9d3bd990";

	private const string SweetEncouragementVoVariantId = "variant_1b82d5d8";

	private const string CartoonEncouragementVoVariantId = "variant_9a018da9";

	private const int DefaultEncouragementVoType = 0;

	private const int SweetEncouragementVoType = 1;

	private const int CartoonEncouragementVoType = 2;

	private const int BgmStrategySwap = 1;

	private const int BgmStrategyNewLevel = 2;

	private const int BgmStrategySwapNew = 3;

	private const string BgmNewLevelVariantId = "variant_3bd53e32";

	private const string BgmSwapVariantId = "variant_4cd1476c";

	private const string BgmSwapNewVariantId = "variant_59e133f2";

	private const string HalloweenBgmVariantId = "variant_de058372";

	private const string FixObjectVariantId = "variant_058dfc36";

	private const string CumulativeReleaseFeedbackVariantId = "variant_e886a02a";

	private const string MovesAudioVariantId = "variant_fbb0f393";

	public override void Execute(object param = null)
	{
		AudioService.BeginBatchExperiments();
		try
		{
			SyncEncouragementVoExperiment();
			AudioService.ActivateExperiment("variant_058dfc36");
			AudioService.ActivateExperiment("variant_fbb0f393");
			ActivateCumulativeReleaseFeedback();
			LevelEndCelebrateService.ActivateTransitionAudioExperiment();
			SyncBgmAudioExperiments();
		}
		finally
		{
			AudioService.CommitExperiments();
		}
	}

	public static void SyncEncouragementVoExperiment()
	{
		int num = 0;
		OperationScoreFunction operationScoreFunction = FunctionDataCenter.Get<OperationScoreFunction>();
		if (operationScoreFunction != null)
		{
			num = operationScoreFunction.OperationScoreSound;
		}
		switch (num)
		{
		case 1:
			AudioService.ActivateExperiment("variant_1b82d5d8");
			break;
		case 2:
			AudioService.ActivateExperiment("variant_9a018da9");
			break;
		default:
			AudioService.ActivateExperiment("variant_9d3bd990");
			break;
		}
	}

	public static void SwitchEncouragementVoExperimentForGm(int soundType)
	{
		switch (soundType)
		{
		case 1:
			AudioService.DeactivateExperiment("variant_9d3bd990");
			AudioService.DeactivateExperiment("variant_9a018da9");
			AudioService.ActivateExperiment("variant_1b82d5d8");
			break;
		case 2:
			AudioService.DeactivateExperiment("variant_9d3bd990");
			AudioService.DeactivateExperiment("variant_1b82d5d8");
			AudioService.ActivateExperiment("variant_9a018da9");
			break;
		default:
			AudioService.DeactivateExperiment("variant_1b82d5d8");
			AudioService.DeactivateExperiment("variant_9a018da9");
			AudioService.ActivateExperiment("variant_9d3bd990");
			break;
		}
	}

	public static void ActivateCumulativeReleaseFeedback()
	{
		OperationScoreFunction operationScoreFunction = FunctionDataCenter.Get<OperationScoreFunction>();
		if (operationScoreFunction != null && operationScoreFunction.CumulativeReleaseType > 0)
		{
			AudioService.ActivateExperiment("variant_e886a02a");
		}
	}

	public static void SyncBgmAudioExperiments()
	{
		bool flag = HalloweenFeatureService.IsBgmEnabled();
		int num = FunctionDataCenter.Get<AudioFunction>()?.BgmStrategy ?? 0;
		SyncBgmVariant("variant_3bd53e32", !flag && num == 2);
		SyncBgmVariant("variant_4cd1476c", !flag && num == 1);
		SyncBgmVariant("variant_59e133f2", !flag && num == 3);
		SyncBgmVariant("variant_de058372", flag);
	}

	private static void SyncBgmVariant(string variantId, bool shouldActivate)
	{
		if (shouldActivate)
		{
			AudioService.ActivateExperiment(variantId);
		}
		else
		{
			AudioService.DeactivateExperiment(variantId);
		}
	}
}
