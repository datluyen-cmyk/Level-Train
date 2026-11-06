using System;
using RotateRings.Prop;
using RotateRings.UI.Game;
using TMPro;
using UnityEngine;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.PropItem;

public class UIRocketPropItemView : UIPropItemView
{
	private UIGamePropUseService propUseService;

	public void SetPropUseService(UIGamePropUseService service)
	{
		propUseService = service;
	}

	protected override void OnWidgetFirstLoaded()
	{
		base.OnWidgetFirstLoaded();
		if ((Object)(object)((UIBaseView)this).Behaviour != (Object)null && ((UIBaseView)this).Behaviour.Contains("TextBtnName"))
		{
			TMP_Text val = ((UIBaseView)this).Behaviour.Get<TMP_Text>("TextBtnName");
			if ((Object)(object)val != (Object)null)
			{
				TMPTextExtensions.SetLocalizedText(val, "Button_Use", Array.Empty<object>());
			}
		}
	}

	protected override void OnPropClick()
	{
		if (propUseService != null && base.PropType != EPropType.None && !base.IsPropUsing && !base.IsCountTweenActive)
		{
			propUseService.FireSkyrocketFromPanel(base.PropType);
		}
	}
}
