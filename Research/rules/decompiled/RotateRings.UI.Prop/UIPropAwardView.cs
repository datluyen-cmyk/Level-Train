using System;
using Game.UI;
using RotateRings.Prop;
using RotateRings.Services;
using Rotaterings.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.Prop;

public class UIPropAwardView : GameBaseView
{
	private const string TitleFireworkKey = "Rewarded_FreeFirework";

	private const string TitleHammerKey = "Rewarded_FreeHammer";

	private const string ContentFireworkKey = "Rewarded_FireworkContent";

	private const string ContentHammerKey = "Rewarded_HammerContent";

	private const string AcceptButtonTextKey = "chest_claim";

	private UIPopupMaskFader maskFader;

	public TMP_Text TxtTitle;

	public TMP_Text TxtDesc;

	public Button Btn_Close;

	public Button Btn_Accept;

	public Image Img_CommonItemSkyrocket256;

	public void BindPresentation(EPropType propType, int grantCount)
	{
		int num = ((grantCount < 1) ? 1 : grantCount);
		string text = ((propType == EPropType.Hammer) ? "Rewarded_FreeHammer" : "Rewarded_FreeFirework");
		string text2 = ((propType == EPropType.Hammer) ? "Rewarded_HammerContent" : "Rewarded_FireworkContent");
		if ((Object)(object)TxtTitle != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TxtTitle, text, Array.Empty<object>());
		}
		if ((Object)(object)TxtDesc != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TxtDesc, text2, new object[1] { num });
		}
		if ((Object)(object)Img_CommonItemSkyrocket256 != (Object)null)
		{
			string iconNameLarge = PropIconService.GetIconNameLarge(propType);
			if (!string.IsNullOrEmpty(iconNameLarge))
			{
				SetSprite(Img_CommonItemSkyrocket256, iconNameLarge);
			}
		}
		if ((Object)(object)Btn_Accept != (Object)null)
		{
			TMP_Text componentInChildren = ((Component)Btn_Accept).GetComponentInChildren<TMP_Text>(true);
			if ((Object)(object)componentInChildren != (Object)null)
			{
				TMPTextExtensions.SetLocalizedText(componentInChildren, "chest_claim", Array.Empty<object>());
			}
		}
	}

	public void SetButtonsInteractable(bool interactable)
	{
		if ((Object)(object)Btn_Accept != (Object)null)
		{
			((Selectable)Btn_Accept).interactable = interactable;
		}
		if ((Object)(object)Btn_Close != (Object)null)
		{
			((Selectable)Btn_Close).interactable = interactable;
		}
	}

	public void OnUiStackActiveChanged(bool isActive)
	{
		if ((Object)(object)maskFader == (Object)null)
		{
			maskFader = ((Component)((UIBaseView)this).Behaviour).GetComponent<UIPopupMaskFader>();
		}
		maskFader?.OnUiStackActiveChanged(isActive);
	}

	public override void Dispose()
	{
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		TxtTitle = behaviour.Get<TMP_Text>("title");
		TxtDesc = behaviour.Get<TMP_Text>("desc");
		Btn_Close = behaviour.Get<Button>("Btn_Close");
		Btn_Accept = behaviour.Get<Button>("Btn_Accept");
		Img_CommonItemSkyrocket256 = behaviour.Get<Image>("Img_CommonItemSkyrocket256");
	}
}
