using System;
using Game.UI;
using Rotaterings.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.RestartRetention;

public class UIRestartRetentionView : GameBaseView
{
	private const string OpenAnimName = "anim_popup_open";

	private const string CloseAnimName = "anim_popup_close";

	private UIPopupMaskFader maskFader;

	private GameObject[] greenBackgrounds;

	private TMP_Text[] tierLabels;

	private TMP_Text[] greenLabels;

	private RectTransform[] arrowGroups;

	public TMP_Text txtm_Title;

	public TMP_Text txtm_Desc;

	public Button btn_Continue;

	public Button btn_Restart;

	public TMP_Text txtm_Restart;

	public TMP_Text txtm_Continue;

	public RectTransform rect_Leaderboard;

	public RectTransform rect_MultiplierBar;

	public RectTransform rect_ArrowHeart;

	public TMP_Text txtm_ResetDesc;

	public GameObject go_greenbg1;

	public GameObject go_greenbg2;

	public GameObject go_greenbg3;

	public GameObject go_greenbg4;

	public GameObject go_greenbg5;

	public TMP_Text txtm_txtX1;

	public TMP_Text txtm_txtX2;

	public TMP_Text txtm_txtX3;

	public TMP_Text txtm_txtX4;

	public TMP_Text txtm_txtX5;

	public RectTransform rect_01;

	public RectTransform rect_02;

	public RectTransform rect_03;

	public RectTransform rect_04;

	public Animation ani_Panel;

	public Color HighlightColor { get; private set; } = Color32.op_Implicit(new Color32((byte)47, (byte)146, (byte)0, byte.MaxValue));

	public bool LeaderboardAvailable { get; private set; }

	public void InitializePresentation()
	{
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		maskFader = ((Component)((UIBaseView)this).Behaviour).GetComponent<UIPopupMaskFader>();
		greenBackgrounds = (GameObject[])(object)new GameObject[5] { go_greenbg1, go_greenbg2, go_greenbg3, go_greenbg4, go_greenbg5 };
		tierLabels = (TMP_Text[])(object)new TMP_Text[5] { txtm_txtX1, txtm_txtX2, txtm_txtX3, txtm_txtX4, txtm_txtX5 };
		arrowGroups = (RectTransform[])(object)new RectTransform[4] { rect_01, rect_02, rect_03, rect_04 };
		greenLabels = (TMP_Text[])(object)new TMP_Text[tierLabels.Length];
		LeaderboardAvailable = (Object)(object)rect_Leaderboard != (Object)null && (Object)(object)rect_MultiplierBar != (Object)null && (Object)(object)rect_ArrowHeart != (Object)null && (Object)(object)txtm_ResetDesc != (Object)null;
		for (int i = 0; i < tierLabels.Length; i++)
		{
			Transform val = (((Object)(object)tierLabels[i] != (Object)null) ? tierLabels[i].transform.Find("green") : null);
			greenLabels[i] = (((Object)(object)val != (Object)null) ? ((Component)val).GetComponent<TMP_Text>() : null);
			LeaderboardAvailable &= (Object)(object)tierLabels[i] != (Object)null && (Object)(object)greenLabels[i] != (Object)null && (Object)(object)greenBackgrounds[i] != (Object)null;
			if ((Object)(object)greenLabels[i] != (Object)null)
			{
				HighlightColor = ((Graphic)greenLabels[i]).color;
			}
		}
		RectTransform[] array = arrowGroups;
		foreach (RectTransform val2 in array)
		{
			LeaderboardAvailable &= (Object)(object)val2 != (Object)null;
		}
	}

	public void Show(RestartRetentionPresentation data)
	{
		TMPTextExtensions.SetLocalizedTextValue(txtm_Title, data.Title);
		TMPTextExtensions.SetLocalizedTextValue(txtm_Continue, data.Continue);
		TMPTextExtensions.SetLocalizedTextValue(txtm_Restart, data.Restart);
		if ((Object)(object)txtm_Desc != (Object)null)
		{
			txtm_Desc.richText = true;
			((Component)txtm_Desc).gameObject.SetActive(!data.Leaderboard);
			TMPTextExtensions.SetLocalizedTextValue(txtm_Desc, data.Body);
		}
		if ((Object)(object)rect_Leaderboard != (Object)null)
		{
			((Component)rect_Leaderboard).gameObject.SetActive(data.Leaderboard);
		}
		if ((Object)(object)txtm_ResetDesc != (Object)null)
		{
			txtm_ResetDesc.richText = true;
			TMPTextExtensions.SetLocalizedTextValue(txtm_ResetDesc, data.Body);
		}
		for (int i = 0; i < tierLabels.Length; i++)
		{
			bool flag = data.Leaderboard && i == data.SelectedIndex;
			if ((Object)(object)greenBackgrounds[i] != (Object)null)
			{
				greenBackgrounds[i].SetActive(flag);
			}
			if ((Object)(object)tierLabels[i] != (Object)null)
			{
				((Behaviour)tierLabels[i]).enabled = !flag;
				TMPTextExtensions.SetLocalizedTextValue(tierLabels[i], data.TierTexts[i]);
			}
			if ((Object)(object)greenLabels[i] != (Object)null)
			{
				((Behaviour)greenLabels[i]).enabled = true;
				TMPTextExtensions.SetLocalizedTextValue(greenLabels[i], data.TierTexts[i]);
				((Component)greenLabels[i]).gameObject.SetActive(flag);
				CanvasGroup component = ((Component)greenLabels[i]).GetComponent<CanvasGroup>();
				if ((Object)(object)component != (Object)null)
				{
					component.alpha = 1f;
				}
			}
		}
		for (int j = 0; j < arrowGroups.Length; j++)
		{
			if ((Object)(object)arrowGroups[j] != (Object)null)
			{
				((Component)arrowGroups[j]).gameObject.SetActive(data.Leaderboard && j == data.ArrowIndex);
			}
		}
		SetButtonsInteractable(interactable: true);
	}

	public void SetButtonsInteractable(bool interactable)
	{
		if ((Object)(object)btn_Continue != (Object)null)
		{
			((Selectable)btn_Continue).interactable = interactable;
		}
		if ((Object)(object)btn_Restart != (Object)null)
		{
			((Selectable)btn_Restart).interactable = interactable;
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
		maskFader?.OnUiStackActiveChanged(isActive);
	}

	public override void Dispose()
	{
		greenBackgrounds = null;
		tierLabels = null;
		greenLabels = null;
		arrowGroups = null;
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		txtm_Title = behaviour.Get<TMP_Text>("txtm_Title");
		txtm_Desc = behaviour.Get<TMP_Text>("txtm_Desc");
		btn_Continue = behaviour.Get<Button>("btn_Continue");
		btn_Restart = behaviour.Get<Button>("btn_Restart");
		txtm_Restart = behaviour.Get<TMP_Text>("txtm_Restart");
		txtm_Continue = behaviour.Get<TMP_Text>("txtm_Continue");
		rect_Leaderboard = behaviour.Get<RectTransform>("rect_Leaderboard");
		rect_MultiplierBar = behaviour.Get<RectTransform>("rect_MultiplierBar");
		rect_ArrowHeart = behaviour.Get<RectTransform>("rect_ArrowHeart");
		txtm_ResetDesc = behaviour.Get<TMP_Text>("txtm_ResetDesc");
		go_greenbg1 = behaviour.Get<GameObject>("go_greenbg1");
		go_greenbg2 = behaviour.Get<GameObject>("go_greenbg2");
		go_greenbg3 = behaviour.Get<GameObject>("go_greenbg3");
		go_greenbg4 = behaviour.Get<GameObject>("go_greenbg4");
		go_greenbg5 = behaviour.Get<GameObject>("go_greenbg5");
		txtm_txtX1 = behaviour.Get<TMP_Text>("txtm_txtX1");
		txtm_txtX2 = behaviour.Get<TMP_Text>("txtm_txtX2");
		txtm_txtX3 = behaviour.Get<TMP_Text>("txtm_txtX3");
		txtm_txtX4 = behaviour.Get<TMP_Text>("txtm_txtX4");
		txtm_txtX5 = behaviour.Get<TMP_Text>("txtm_txtX5");
		rect_01 = behaviour.Get<RectTransform>("rect_01");
		rect_02 = behaviour.Get<RectTransform>("rect_02");
		rect_03 = behaviour.Get<RectTransform>("rect_03");
		rect_04 = behaviour.Get<RectTransform>("rect_04");
		ani_Panel = behaviour.Get<Animation>("ani_Panel");
	}
}
