using RotateRings.GamePlay.Services.LevelStepLimit;

namespace Game.Common.Services;

public class LevelStepLimitExhaustedPopStep : ILevelStartStep, IActivityStep
{
	private const string Tag = "[LevelStepLimitExhaustedPopStep] ";

	public int Priority => 30;

	public bool ShouldRun()
	{
		LevelStepLimitService levelStepLimitService = SingletonComponent<GameplayManager>.Instance?.GetGameService<LevelStepLimitService>();
		if (levelStepLimitService != null && levelStepLimitService.IsEnabled)
		{
			return levelStepLimitService.RemainingSteps <= 0;
		}
		return false;
	}

	public void Execute()
	{
		int round = LevelStartPipeline.CurrentRound;
		LevelStepLimitService levelStepLimitService = SingletonComponent<GameplayManager>.Instance?.GetGameService<LevelStepLimitService>();
		if (levelStepLimitService == null)
		{
			LevelStartPipeline.ContinueNextForRound(round);
		}
		else if (levelStepLimitService.IsExhausted)
		{
			levelStepLimitService.OnExhausted(delegate
			{
				LevelStartPipeline.ContinueNextForRound(round);
			});
		}
		else
		{
			LevelStartPipeline.ContinueNextForRound(round);
		}
	}
}
