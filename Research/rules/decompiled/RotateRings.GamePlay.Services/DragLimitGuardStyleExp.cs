using XingYun.Experiment;

namespace RotateRings.GamePlay.Services;

public static class DragLimitGuardStyleExp
{
	public static int ReadStyleExp()
	{
		return FunctionDataCenter.Get<GameBord>()?.GameBordDragLimitStyle ?? 0;
	}

	public static void ApplyPreset(int styleExp)
	{
		switch (styleExp)
		{
		case 1:
			WriteConfigs(35f, 0.5f);
			break;
		case 2:
			WriteConfigs(50f, 0.8f);
			break;
		}
	}

	private static void WriteConfigs(float maxFingerDeltaDegPerFrame, float maxMovePerFrameWorld)
	{
		RingRotateGuardConfig.ResetToDefaults();
		PullMoveGuardConfig.ResetToDefaults();
		RingRotateGuardConfig.MaxFingerDeltaDegPerFrame = maxFingerDeltaDegPerFrame;
		PullMoveGuardConfig.MaxMovePerFrameWorld = maxMovePerFrameWorld;
	}
}
