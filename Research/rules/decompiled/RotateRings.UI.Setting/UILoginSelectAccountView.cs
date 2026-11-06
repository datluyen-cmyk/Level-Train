using System;
using Game.UI;
using RotateRing.GamePlay;
using RotateRings.Common;
using RotateRings.Prop;
using RotateRings.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Core.Singletons;
using XingYun.Extensions;
using XingYun.Lang;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

public class UILoginSelectAccountView : GameBaseView
{
	private const string DefaultLevelText = "1";

	private const string LevelLabelKey = "MainGame_Level";

	private const string EndlessLevelLabelKey = "Level_Round";

	private const string LevelLabelSuffix = ":";

	private Animation uiAnimation;

	public Button Btn_Close;

	public Button continueBtn;

	public TMP_Text TxtM_Text;

	public GameObject account_select_box1;

	public GameObject account_select_box2;

	public TMP_Text levelTxt1;

	public TMP_Text hammerNum1;

	public TMP_Text fireworksNum1;

	public Image heardIcon1;

	public TMP_Text levelTxt2;

	public TMP_Text hammerNum2;

	public TMP_Text fireworksNum2;

	public Image heardIcon2;

	public Button bindBtn1;

	public Button bindBtn2;

	public TMP_Text level1;

	public TMP_Text level2;

	public void SetAccountPreviews(CloudSaveDataSnapshot cloudSnapshot, CloudSaveDataSnapshot localSnapshot)
	{
		SetAccountPreview(cloudSnapshot, level1, levelTxt1, hammerNum1, fireworksNum1, heardIcon1);
		SetAccountPreview(localSnapshot, level2, levelTxt2, hammerNum2, fireworksNum2, heardIcon2);
	}

	public void SetCurrentAccountSelected()
	{
		SetAccountSelected(isFirstAccount: true);
	}

	public void SetAccountSelected(bool isFirstAccount)
	{
		if ((Object)(object)account_select_box1 != (Object)null)
		{
			account_select_box1.SetActive(isFirstAccount);
		}
		if ((Object)(object)account_select_box2 != (Object)null)
		{
			account_select_box2.SetActive(!isFirstAccount);
		}
	}

	private static void SetAccountPreview(CloudSaveDataSnapshot snapshot, TMP_Text levelLabelText, TMP_Text levelText, TMP_Text hammerText, TMP_Text fireworksText, Image headIcon)
	{
		if ((Object)(object)levelLabelText != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(levelLabelText, GetLevelLabel(snapshot));
		}
		if ((Object)(object)levelText != (Object)null)
		{
			levelText.text = GetLevelText(snapshot);
		}
		if ((Object)(object)hammerText != (Object)null)
		{
			hammerText.text = GetPropCount(snapshot, EPropType.Hammer).ToString();
		}
		if ((Object)(object)fireworksText != (Object)null)
		{
			fireworksText.text = GetPropCount(snapshot, EPropType.Skyrocket).ToString();
		}
		if ((Object)(object)headIcon != (Object)null)
		{
			headIcon.sprite = PlayerService.GetHeadSprite(snapshot?.HeadId ?? 0);
		}
	}

	private static string GetLevelLabel(CloudSaveDataSnapshot snapshot)
	{
		string text = (IsEndlessSnapshot(snapshot) ? "Level_Round" : "MainGame_Level");
		return Singleton<LangManager>.Instance.GetText(text, Array.Empty<object>()) + ":";
	}

	private static string GetLevelText(CloudSaveDataSnapshot snapshot)
	{
		int num = (IsEndlessSnapshot(snapshot) ? snapshot.EndlessLevelId : (snapshot?.MainLevelId ?? 0));
		if (num <= 0)
		{
			return "1";
		}
		return num.ToString();
	}

	private static bool IsEndlessSnapshot(CloudSaveDataSnapshot snapshot)
	{
		if (snapshot != null)
		{
			return snapshot.MainLevelId > LevelConfigService.MainLevelMax;
		}
		return false;
	}

	private static int GetPropCount(CloudSaveDataSnapshot snapshot, EPropType propType)
	{
		if (snapshot?.PropCounts == null)
		{
			return 0;
		}
		for (int i = 0; i < snapshot.PropCounts.Count; i++)
		{
			CloudSavePropCount cloudSavePropCount = snapshot.PropCounts[i];
			if (cloudSavePropCount != null && cloudSavePropCount.PropId == (int)propType)
			{
				if (cloudSavePropCount.Count <= 0)
				{
					return 0;
				}
				return cloudSavePropCount.Count;
			}
		}
		return 0;
	}

	public void PlayOpenAnimation()
	{
		UILoginPopupAnimationHelper.PlayOpen(ResolveUIAnimation());
	}

	public void PlayCloseAnimation(Action onComplete = null)
	{
		UILoginPopupAnimationHelper.PlayClose(ResolveUIAnimation(), ((UIBaseView)this).NodeRect, onComplete);
	}

	private Animation ResolveUIAnimation()
	{
		if ((Object)(object)uiAnimation != (Object)null)
		{
			return uiAnimation;
		}
		uiAnimation = UILoginPopupAnimationHelper.ResolveAnimation(((UIBaseView)this).Behaviour);
		return uiAnimation;
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		Btn_Close = behaviour.Get<Button>("Btn_Close");
		continueBtn = behaviour.Get<Button>("continueBtn");
		TxtM_Text = behaviour.Get<TMP_Text>("TxtM_Text");
		account_select_box1 = behaviour.Get<GameObject>("account_select_box1");
		account_select_box2 = behaviour.Get<GameObject>("account_select_box2");
		levelTxt1 = behaviour.Get<TMP_Text>("levelTxt1");
		hammerNum1 = behaviour.Get<TMP_Text>("hammerNum1");
		fireworksNum1 = behaviour.Get<TMP_Text>("fireworksNum1");
		heardIcon1 = behaviour.Get<Image>("heardIcon1");
		levelTxt2 = behaviour.Get<TMP_Text>("levelTxt2");
		hammerNum2 = behaviour.Get<TMP_Text>("hammerNum2");
		fireworksNum2 = behaviour.Get<TMP_Text>("fireworksNum2");
		heardIcon2 = behaviour.Get<Image>("heardIcon2");
		bindBtn1 = behaviour.Get<Button>("bindBtn1");
		bindBtn2 = behaviour.Get<Button>("bindBtn2");
		level1 = behaviour.Get<TMP_Text>("level1");
		level2 = behaviour.Get<TMP_Text>("level2");
	}
}
