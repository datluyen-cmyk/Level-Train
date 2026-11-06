using RotateRings.Model;
using RotateRings.UI.NewElementIntro;

namespace Game.Common.Services;

public class NewElementIntroLevelStartStep : ILevelStartStep, IActivityStep
{
	private const string Tag = "[NewElementIntroLevelStartStep] ";

	public int Priority => 20;

	public bool ShouldRun()
	{
		return true;
	}

	public void Execute()
	{
		int num = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0;
		int round = LevelStartPipeline.CurrentRound;
		if (num <= 0)
		{
			LevelStartPipeline.ContinueNextForRound(round);
			return;
		}
		NewElementIntroService.RunAfterCurrentPopup(delegate
		{
			LevelStartPipeline.ContinueNextForRound(round);
		});
		if (!NewElementIntroService.TryOpenForLevel(num))
		{
			LevelStartPipeline.ContinueNextForRound(round);
		}
	}
}
