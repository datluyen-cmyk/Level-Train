using System;
using Game.DataTrack;
using RotateRings.Common;
using RotateRings.Services;
using RotateRings.Theme;
using UniLog;
using UnityEngine;
using UnityEngine.Events;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

[UIPath("UISettingInGame_DarkModeBtnLocationExperiment")]
public class UISettingInGameV2DarkController : UISettingInGameV2Controller
{
	private readonly UISettingInGameV2DarkView darkView;

	public UISettingInGameV2DarkController(UISettingInGameV2DarkView view)
		: base(view)
	{
		darkView = view;
	}

	public override void OnRegister(object param = null)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		base.OnRegister(param);
		if ((Object)(object)darkView.ToggleOpen == (Object)null || (Object)(object)darkView.ToggleClose == (Object)null)
		{
			Log.LogError("UI", "[UISettingInGameV2DarkController] ToggleOpen/ToggleClose 未在 UIBehaviour 中绑定，请用 SAJ 工具收集变体 Prefab 组件");
			return;
		}
		((UnityEvent)darkView.ToggleOpen.onClick).AddListener(new UnityAction(OnToggleOpenClick));
		((UnityEvent)darkView.ToggleClose.onClick).AddListener(new UnityAction(OnToggleCloseClick));
		((BaseController<UISettingInGameV2View>)this).ListenEvent<ThemeChangedEvent>((Action<ThemeChangedEvent>)OnThemeChanged);
		ApplyThemeToggleVisibility();
	}

	public override void OnRemove()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		if ((Object)(object)darkView.ToggleOpen != (Object)null)
		{
			((UnityEvent)darkView.ToggleOpen.onClick).RemoveListener(new UnityAction(OnToggleOpenClick));
		}
		if ((Object)(object)darkView.ToggleClose != (Object)null)
		{
			((UnityEvent)darkView.ToggleClose.onClick).RemoveListener(new UnityAction(OnToggleCloseClick));
		}
		((BaseController<UISettingInGameV2View>)this).UnlistenEvent<ThemeChangedEvent>((Action<ThemeChangedEvent>)OnThemeChanged);
		base.OnRemove();
	}

	private void ApplyThemeToggleVisibility()
	{
		bool themeToggleVisible = ThemeService.CurrentThemeId == ThemeId.Day;
		darkView.SetThemeToggleVisible(themeToggleVisible);
	}

	private void OnThemeChanged(ThemeChangedEvent evt)
	{
		ApplyThemeToggleVisibility();
	}

	private void OnToggleOpenClick()
	{
		if (!isClosing)
		{
			AudioService.PlayUiSound(AudioKeys.BtnGroupClick.Value);
			RRDataTrack.Instance.ReportBtnClick("游戏内设置-黑暗模式切换");
			ThemeService.SetTheme(ThemeId.Day);
		}
	}

	private void OnToggleCloseClick()
	{
		if (!isClosing)
		{
			AudioService.PlayUiSound(AudioKeys.BtnGroupClick.Value);
			RRDataTrack.Instance.ReportBtnClick("游戏内设置-黑暗模式切换");
			ThemeService.SetTheme(ThemeId.Night);
		}
	}
}
