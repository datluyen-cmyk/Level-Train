using System;
using Game.UI;
using Rotaterings.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.LevelStepLimit;

public class UILevelStepLimitPopView : GameBaseView
{
	private const string OpenAnimName = "anim_popup_open";

	private const string CloseAnimName = "anim_popup_close";

	private UIPopupMaskFader maskFader;

	public TMP_Text txtm_RemainAdCount;

	public TMP_Text txtm_MaxSteps;

	public Animation ani_Panel;

	public Button BtnRestart;

	public Button BtnAddFreeMoves;

	public Button BtnAddMovesByAd;

	public Button BtnClose;

	public TMP_Text TextDesc;

	public TMP_Text TextTitle;

	public TMP_Text TextAddMoves;

	public TMP_Text TextRestart;

	public TMP_Text TextFree;

	public TMP_Text TextAddMoves2;

	public void SetMaxSteps(int maxSteps)
	{
		if (!((Object)(object)txtm_MaxSteps == (Object)null))
		{
			txtm_MaxSteps.text = maxSteps.ToString();
		}
	}

	public void SetupLocalizedTexts(int maxSteps, int addStepsPerAd)
	{
		if ((Object)(object)TextTitle != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TextTitle, "Level_OutOfMoves", Array.Empty<object>());
		}
		if ((Object)(object)TextDesc != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TextDesc, "Level_AddStepHelp", new object[1] { addStepsPerAd });
		}
		if ((Object)(object)TextAddMoves != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TextAddMoves, "Common_addMoves", new object[1] { addStepsPerAd });
		}
		if ((Object)(object)TextAddMoves2 != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TextAddMoves2, "Common_addMoves", new object[1] { addStepsPerAd });
		}
		if ((Object)(object)TextFree != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TextFree, "Common_Free", Array.Empty<object>());
		}
		if ((Object)(object)TextRestart != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TextRestart, "Bottom_Restart", Array.Empty<object>());
		}
	}

	public void SetFreeGiftVisible(bool isFreeGift)
	{
		if ((Object)(object)BtnAddFreeMoves != (Object)null)
		{
			((Component)BtnAddFreeMoves).gameObject.SetActive(isFreeGift);
		}
		if ((Object)(object)BtnAddMovesByAd != (Object)null)
		{
			((Component)BtnAddMovesByAd).gameObject.SetActive(!isFreeGift);
		}
	}

	public void SetButtonsInteractable(bool interactable)
	{
		if ((Object)(object)BtnAddFreeMoves != (Object)null)
		{
			((Selectable)BtnAddFreeMoves).interactable = interactable;
		}
		if ((Object)(object)BtnAddMovesByAd != (Object)null)
		{
			((Selectable)BtnAddMovesByAd).interactable = interactable;
		}
		if ((Object)(object)BtnRestart != (Object)null)
		{
			((Selectable)BtnRestart).interactable = interactable;
		}
	}

	public void PlayOpenAnimation()
	{
		if (!((Object)(object)ani_Panel == (Object)null) && !((Object)(object)ani_Panel.GetClip("anim_popup_open") == (Object)null))
		{
			ani_Panel.Rewind("anim_popup_open");
			ani_Panel.Play("anim_popup_open");
		}
	}

	public void PlayCloseAnimation(Action onComplete)
	{
		if ((Object)(object)ani_Panel == (Object)null || (Object)(object)ani_Panel.GetClip("anim_popup_close") == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		ani_Panel.Rewind("anim_popup_close");
		ani_Panel.Play("anim_popup_close");
		GameApp.Timer.Delay((Object)(object)((UIBaseView)this).NodeRect, ani_Panel.GetClip("anim_popup_close").length, (Action)delegate
		{
			onComplete?.Invoke();
		});
	}

	public void OnUiStackActiveChanged(bool isActive)
	{
		if ((Object)(object)maskFader == (Object)null)
		{
			maskFader = ((Component)((UIBaseView)this).Behaviour).GetComponent<UIPopupMaskFader>();
		}
		maskFader?.OnUiStackActiveChanged(isActive);
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		BtnRestart = behaviour.Get<Button>("BtnRestart");
		BtnAddFreeMoves = behaviour.Get<Button>("BtnAddFreeMoves");
		BtnAddMovesByAd = behaviour.Get<Button>("BtnAddMovesByAd");
		BtnClose = behaviour.Get<Button>("BtnClose");
		TextDesc = behaviour.Get<TMP_Text>("TextDesc");
		TextTitle = behaviour.Get<TMP_Text>("TextTitle");
		TextAddMoves = behaviour.Get<TMP_Text>("TextAddMoves");
		TextRestart = behaviour.Get<TMP_Text>("TextRestart");
		TextFree = behaviour.Get<TMP_Text>("TextFree");
		TextAddMoves2 = behaviour.Get<TMP_Text>("TextAddMoves2");
	}
}
