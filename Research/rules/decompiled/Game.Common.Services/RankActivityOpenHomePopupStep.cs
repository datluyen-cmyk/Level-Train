using RotateRings.GamePlay.Save;
using RotateRings.Model;
using Rotaterings.Rank;
using XingYun.UI.Core;

namespace Game.Common.Services;

public class RankActivityOpenHomePopupStep : IHomePopupStep, IActivityStep
{
	private readonly bool ignoreOpenScoreRequirement;

	public int Priority => 200;

	public RankActivityOpenHomePopupStep(bool ignoreOpenScoreRequirement = false)
	{
		this.ignoreOpenScoreRequirement = ignoreOpenScoreRequirement;
	}

	public bool ShouldRun()
	{
		int num = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0;
		if (num > 0)
		{
			RingLevelSaveProxy ringLevelSaveProxy = GameApp.Proxy.Get<RingLevelSaveProxy>();
			if (ringLevelSaveProxy != null && ringLevelSaveProxy.HasValidSave(num))
			{
				return false;
			}
		}
		return RankActivityEntry.CanShowActivityOpenPopup(ignoreOpenScoreRequirement);
	}

	public void Execute()
	{
		if (!RankActivityEntry.TryShowActivityOpenPopup(HomePopupPipeline.ContinueNext, enterLevelOnSubmit: true, (UILayer)2, ignoreOpenScoreRequirement))
		{
			HomePopupPipeline.ContinueNext();
		}
	}
}
