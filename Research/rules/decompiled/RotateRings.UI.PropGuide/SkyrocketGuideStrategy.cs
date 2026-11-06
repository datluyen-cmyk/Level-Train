using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Experiment;

namespace RotateRings.UI.PropGuide;

public class SkyrocketGuideStrategy : IPropGuideStrategy
{
	private UIPropGuideView view;

	private PropGuideOpenData data;

	public void OnRegister(UIPropGuideView view, PropGuideOpenData data, Action onDismiss)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		this.view = view;
		this.data = data;
		view.ConfigureUseButton(isVisible: true);
		if ((Object)(object)view.BtnUse != (Object)null)
		{
			((UnityEvent)view.BtnUse.onClick).AddListener(new UnityAction(OnUseClick));
		}
		if (FunctionDataCenter.Get<PropFunction>().SkyrocketFireStrategy == 2)
		{
			view.UpdateTxtItemTip("Item_Skyrocket_desc");
		}
	}

	public void OnEnterSecondPhase(UIPropGuideView view)
	{
	}

	public void OnRemove(UIPropGuideView view)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		if ((Object)(object)view.BtnUse != (Object)null)
		{
			((UnityEvent)view.BtnUse.onClick).RemoveListener(new UnityAction(OnUseClick));
		}
	}

	private void OnUseClick()
	{
		if (!((Object)(object)view.BtnUse == (Object)null) && ((Selectable)view.BtnUse).interactable)
		{
			view.SetUseButtonInteractable(interactable: false);
			data?.OnUseButtonClicked?.Invoke();
		}
	}
}
