using System;
using Game.UI;
using Rotaterings.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.Rating;

public class UIRatingFeedbackView : GameBaseView
{
	private const string OpenAnimName = "anim_popup_open";

	private const string CloseAnimName = "anim_popup_close";

	private UIPopupMaskFader maskFader;

	private GameObject[] starLights;

	public GameObject img_StarLight1;

	public GameObject img_StarLight2;

	public GameObject img_StarLight3;

	public GameObject img_StarLight4;

	public GameObject img_StarLight5;

	public TMP_Text txtm_Title;

	public InputField inp_Feedback;

	public Button btn_Close;

	public Button btn_SubmitGray;

	public Button btn_Submit;

	public Animation ani_Panel;

	public void SetStarState(int count)
	{
		if ((Object)(object)maskFader == (Object)null)
		{
			maskFader = ((Component)((UIBaseView)this).Behaviour).GetComponent<UIPopupMaskFader>();
		}
		GameObject[] array = GetStarLights();
		for (int i = 0; i < array.Length; i++)
		{
			if ((Object)(object)array[i] != (Object)null)
			{
				SetStarVisual(array[i], i < count);
			}
		}
	}

	private void SetStarVisual(GameObject light, bool visible)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Image component = light.GetComponent<Image>();
		if ((Object)(object)component != (Object)null)
		{
			Color color = ((Graphic)component).color;
			color.a = (visible ? 1f : 0f);
			((Graphic)component).color = color;
		}
		light.transform.localScale = Vector3.one;
	}

	public void SetSubmitInteractable(bool interactable)
	{
		if ((Object)(object)btn_Submit != (Object)null)
		{
			((Component)btn_Submit).gameObject.SetActive(interactable);
		}
		if ((Object)(object)btn_SubmitGray != (Object)null)
		{
			((Component)btn_SubmitGray).gameObject.SetActive(!interactable);
		}
	}

	public string GetFeedbackText()
	{
		if (!((Object)(object)inp_Feedback != (Object)null))
		{
			return string.Empty;
		}
		return inp_Feedback.text.Trim();
	}

	public void PlayOpenAnimation()
	{
		if (!((Object)(object)ani_Panel == (Object)null) && !((Object)(object)ani_Panel.GetClip("anim_popup_open") == (Object)null))
		{
			ani_Panel.Rewind("anim_popup_open");
			ani_Panel.Play("anim_popup_open");
		}
	}

	public void PlayCloseAnimation(Action onComplete = null)
	{
		if ((Object)(object)ani_Panel == (Object)null || (Object)(object)ani_Panel.GetClip("anim_popup_close") == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		ani_Panel.Rewind("anim_popup_close");
		ani_Panel.Play("anim_popup_close");
		float length = ani_Panel.GetClip("anim_popup_close").length;
		if ((Object)(object)((UIBaseView)this).NodeRect != (Object)null)
		{
			GameApp.Timer.Delay((Object)(object)((UIBaseView)this).NodeRect, length, (Action)delegate
			{
				onComplete?.Invoke();
			});
		}
		else
		{
			GameApp.Timer.Delay(length, (Action)delegate
			{
				onComplete?.Invoke();
			});
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
		starLights = null;
		((UIBaseView)this).Dispose();
	}

	private GameObject[] GetStarLights()
	{
		if (starLights == null)
		{
			starLights = (GameObject[])(object)new GameObject[5] { img_StarLight1, img_StarLight2, img_StarLight3, img_StarLight4, img_StarLight5 };
		}
		return starLights;
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		img_StarLight1 = behaviour.Get<GameObject>("img_StarLight1");
		img_StarLight2 = behaviour.Get<GameObject>("img_StarLight2");
		img_StarLight3 = behaviour.Get<GameObject>("img_StarLight3");
		img_StarLight4 = behaviour.Get<GameObject>("img_StarLight4");
		img_StarLight5 = behaviour.Get<GameObject>("img_StarLight5");
		txtm_Title = behaviour.Get<TMP_Text>("txtm_Title");
		inp_Feedback = behaviour.Get<InputField>("inp_Feedback");
		btn_Close = behaviour.Get<Button>("btn_Close");
		btn_SubmitGray = behaviour.Get<Button>("btn_SubmitGray");
		btn_Submit = behaviour.Get<Button>("btn_Submit");
		ani_Panel = behaviour.Get<Animation>("ani_Panel");
	}
}
