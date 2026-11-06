using System;
using Game.UI;
using RotateRings.Common;
using RotateRings.Services;
using UnityEngine.Events;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.SettingGame;

[UIPath("UISettingGame")]
public class UISettingGameController : GameBaseController<UISettingGameView>
{
	public UISettingGameController(UISettingGameView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		((UnityEvent)((BaseController<UISettingGameView>)this).view.Mask.onClick).AddListener(new UnityAction(OnMaskClick));
		((UnityEvent)((BaseController<UISettingGameView>)this).view.RefreshBtn.onClick).AddListener(new UnityAction(OnRefreshClick));
		((UnityEvent)((BaseController<UISettingGameView>)this).view.LanguageBtn.onClick).AddListener(new UnityAction(OnLanguageClick));
	}

	private void OnMaskClick()
	{
		AudioService.PlayUiSound(AudioKeys.Unlock.Value);
		((BaseController<UISettingGameView>)this).CloseView(false);
	}

	private void OnRefreshClick()
	{
		if (Gameplay.Active != null && Gameplay.Active.IsLevelCompleted)
		{
			((BaseController<UISettingGameView>)this).CloseView(false);
			return;
		}
		Gameplay.Active?.ReplayCurrentLevel();
		((BaseController<UISettingGameView>)this).CloseView(false);
	}

	private void OnLanguageClick()
	{
		AudioService.PlayUiSound(AudioKeys.Unlock.Value);
		((BaseController<UISettingGameView>)this).CloseView(false);
		GameApp.UI.Open("UILanguage", (UILayer)2, (object)null, (Action)null);
	}

	public override void OnRemove()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		base.OnRemove();
		((UnityEvent)((BaseController<UISettingGameView>)this).view.Mask.onClick).RemoveListener(new UnityAction(OnMaskClick));
		((UnityEvent)((BaseController<UISettingGameView>)this).view.RefreshBtn.onClick).RemoveListener(new UnityAction(OnRefreshClick));
		((UnityEvent)((BaseController<UISettingGameView>)this).view.LanguageBtn.onClick).RemoveListener(new UnityAction(OnLanguageClick));
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UISettingGameView>)this).OnActive(isActive);
	}

	public override void OnEscape()
	{
		((BaseController<UISettingGameView>)this).CloseView(false);
	}
}
