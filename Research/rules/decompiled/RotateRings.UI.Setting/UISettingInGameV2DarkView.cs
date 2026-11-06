using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

public class UISettingInGameV2DarkView : UISettingInGameV2View
{
	private const string DarkThemeLangKey = "Common_Dark_Theme";

	public Button ToggleOpen;

	public Button ToggleClose;

	public TMP_Text txtm_DarkTheme;

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		ToggleOpen = behaviour.Get<Button>("ToggleOpen");
		ToggleClose = behaviour.Get<Button>("ToggleClose");
		txtm_DarkTheme = behaviour.Get<TMP_Text>("DarkTheme");
		if ((Object)(object)txtm_DarkTheme != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(txtm_DarkTheme, "Common_Dark_Theme", Array.Empty<object>());
		}
	}

	public void SetThemeToggleVisible(bool isDay)
	{
		if ((Object)(object)ToggleOpen != (Object)null)
		{
			((Component)ToggleOpen).gameObject.SetActive(!isDay);
		}
		if ((Object)(object)ToggleClose != (Object)null)
		{
			((Component)ToggleClose).gameObject.SetActive(isDay);
		}
	}
}
