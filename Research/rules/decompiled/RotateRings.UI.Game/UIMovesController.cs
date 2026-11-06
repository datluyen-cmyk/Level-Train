using System;
using Game.UI;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Services.LevelStepLimit;
using RotateRings.Theme;
using RotateRings.UI.Moves;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Game;

[UIPath("Moves")]
public class UIMovesController : GameBaseController<UIMovesView>
{
	public UIMovesController(UIMovesView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		((BaseController<UIMovesView>)this).ListenEvent<LevelStepLimitChangedEvent>((Action<LevelStepLimitChangedEvent>)OnLevelStepLimitChanged);
		((BaseController<UIMovesView>)this).ListenEvent<ThemeChangedEvent>((Action<ThemeChangedEvent>)OnThemeChanged);
	}

	public override void OnRemove()
	{
		((BaseController<UIMovesView>)this).UnlistenEvent<LevelStepLimitChangedEvent>((Action<LevelStepLimitChangedEvent>)OnLevelStepLimitChanged);
		((BaseController<UIMovesView>)this).UnlistenEvent<ThemeChangedEvent>((Action<ThemeChangedEvent>)OnThemeChanged);
		base.OnRemove();
	}

	public override void OnEscape()
	{
	}

	private void OnLevelStepLimitChanged(LevelStepLimitChangedEvent eventData)
	{
		if (eventData.IsConsumed)
		{
			((BaseController<UIMovesView>)this).view.PlayMoveAnimation(eventData.RemainingSteps);
		}
		else if (eventData.AddedSteps > 0)
		{
			((BaseController<UIMovesView>)this).view.PlayAddStepsFlyAnimation(eventData.AddedSteps, eventData.RemainingSteps, OnAddStepsAnimationComplete);
		}
	}

	private void OnAddStepsAnimationComplete()
	{
		LevelStepLimitService levelStepLimitService = SingletonComponent<GameplayManager>.Instance?.GetGameService<LevelStepLimitService>();
		if (levelStepLimitService != null)
		{
			((BaseController<UIMovesView>)this).view?.SetRemaining(levelStepLimitService.RemainingSteps);
		}
	}

	private void OnThemeChanged(ThemeChangedEvent eventData)
	{
		((BaseController<UIMovesView>)this).view.RefreshMovesColor();
	}
}
