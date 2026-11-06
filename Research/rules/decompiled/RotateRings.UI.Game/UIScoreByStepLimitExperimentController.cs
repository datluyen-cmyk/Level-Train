using System;
using Game.UI;
using Game.UI.Language;
using RotateRings.Model;
using XingYun.UI.Controller;

namespace RotateRings.UI.Game;

public class UIScoreByStepLimitExperimentController : GameBaseController<UIScoreByStepLimitExperimentView>
{
	public UIScoreByStepLimitExperimentController(UIScoreByStepLimitExperimentView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		((BaseController<UIScoreByStepLimitExperimentView>)this).ListenEvent<ScoreAdded>((Action<ScoreAdded>)OnScoreAdded);
		((BaseController<UIScoreByStepLimitExperimentView>)this).ListenEvent<ScoreReset>((Action<ScoreReset>)OnScoreReset);
		((BaseController<UIScoreByStepLimitExperimentView>)this).ListenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		int currentScore = GameApp.Proxy.Get<LevelModelProxy>().GetCurrentScore();
		((BaseController<UIScoreByStepLimitExperimentView>)this).view.SetScore(currentScore);
	}

	public override void OnRemove()
	{
		((BaseController<UIScoreByStepLimitExperimentView>)this).UnlistenEvent<ScoreAdded>((Action<ScoreAdded>)OnScoreAdded);
		((BaseController<UIScoreByStepLimitExperimentView>)this).UnlistenEvent<ScoreReset>((Action<ScoreReset>)OnScoreReset);
		((BaseController<UIScoreByStepLimitExperimentView>)this).UnlistenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		base.OnRemove();
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UIScoreByStepLimitExperimentView>)this).OnActive(isActive);
	}

	public override void OnEscape()
	{
	}

	private void OnScoreAdded(ScoreAdded eventData)
	{
		((BaseController<UIScoreByStepLimitExperimentView>)this).view?.AddScore(eventData.Score);
	}

	private void OnScoreReset(ScoreReset eventData)
	{
		((BaseController<UIScoreByStepLimitExperimentView>)this).view?.SetScore(eventData.Score);
	}

	private void OnLanguageChangeSuccess(LanguageChangeSuccess evt)
	{
		((BaseController<UIScoreByStepLimitExperimentView>)this).view?.RefreshTitle();
	}
}
