using System;
using Example.Cfg;
using Game.UI;
using Rotaterings.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.RewardedLuckyDraw;

public class UIRewardedLuckyDrawStaticView : GameBaseView, IRewardedLuckyDrawView
{
	private TMP_Text adClaimText;

	public Button btn_Close;

	public Button btn_Ad;

	public GameObject go_Content;

	public Button CloseButton => btn_Close;

	public Button AdButton => btn_Ad;

	public void InitializePresentation(RewardedLuckyDrawProbability[] slots, string[] iconPaths)
	{
		StopPresentation();
		go_Content.SetActive(true);
		TMP_Text[] componentsInChildren = ((Component)((UIBaseView)this).Behaviour).GetComponentsInChildren<TMP_Text>(true);
		foreach (TMP_Text val in componentsInChildren)
		{
			if (((Object)val).name == "txtm_Title")
			{
				TMPTextExtensions.SetLocalizedText(val, "Rewarded_LuckyDrawTitle", Array.Empty<object>());
			}
			else if (((Object)val).name == "txtm_Desc")
			{
				TMPTextExtensions.SetLocalizedText(val, "Rewarded_LuckyDrawContent", Array.Empty<object>());
			}
			else if (((Object)val).name == "txt_Claim")
			{
				adClaimText = val;
				TMPTextExtensions.SetLocalizedText(val, "Rewarded_LuckyDrawButton", Array.Empty<object>());
			}
		}
		RewardedLuckyDrawAdButtonLayout.Refresh(adClaimText);
	}

	public void HideContentForClaim()
	{
		go_Content.SetActive(false);
		if (!GameApp.UI.IsOpen("UILevelAdPop"))
		{
			NotifyPopupMaskStackActive(isActive: true);
		}
	}

	public void SetButtonsInteractable(bool interactable)
	{
		((Selectable)btn_Close).interactable = interactable;
		((Selectable)btn_Ad).interactable = interactable;
	}

	public void NotifyPopupMaskStackActive(bool isActive)
	{
		UIBehaviour behaviour = ((UIBaseView)this).Behaviour;
		if (behaviour != null)
		{
			((Component)behaviour).GetComponent<UIPopupMaskFader>()?.OnUiStackActiveChanged(isActive);
		}
		if (isActive)
		{
			RewardedLuckyDrawAdButtonLayout.Refresh(adClaimText);
		}
	}

	public void PlayResult(int slotId, RewardedLuckyDrawWheelAnimationSettings animation, Action onCompleted)
	{
		onCompleted?.Invoke();
	}

	public void StopPresentation()
	{
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		btn_Close = behaviour.Get<Button>("btn_Close");
		btn_Ad = behaviour.Get<Button>("btn_Ad");
		go_Content = behaviour.Get<GameObject>("go_Content");
	}
}
