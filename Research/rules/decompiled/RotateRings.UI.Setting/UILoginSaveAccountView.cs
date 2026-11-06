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

public class UILoginSaveAccountView : GameBaseView
{
	private const string DefaultLevelText = "1";

	private const string LevelLabelKey = "MainGame_Level";

	private const string EndlessLevelLabelKey = "Level_Round";

	private const string LevelLabelSuffix = ":";

	private Animation uiAnimation;

	public Button Btn_Close;

	public Button Btn_No;

	public Button Btn_Yes;

	public TMP_Text levelTxt;

	public TMP_Text hammerNum;

	public TMP_Text fireworksNum;

	public Image heardIcon;

	public TMP_Text level;

	public void SetAccountPreview(CloudSaveDataSnapshot snapshot)
	{
		TMP_Text obj = level;
		if (obj != null)
		{
			TMPTextExtensions.SetLocalizedTextValue(obj, GetLevelLabel(snapshot));
		}
		if ((Object)(object)levelTxt != (Object)null)
		{
			levelTxt.text = GetLevelText(snapshot);
		}
		if ((Object)(object)hammerNum != (Object)null)
		{
			hammerNum.text = GetPropCount(snapshot, EPropType.Hammer).ToString();
		}
		if ((Object)(object)fireworksNum != (Object)null)
		{
			fireworksNum.text = GetPropCount(snapshot, EPropType.Skyrocket).ToString();
		}
		if ((Object)(object)heardIcon != (Object)null)
		{
			heardIcon.sprite = PlayerService.GetHeadSprite(snapshot?.HeadId ?? 0);
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
		Btn_No = behaviour.Get<Button>("Btn_No");
		Btn_Yes = behaviour.Get<Button>("Btn_Yes");
		levelTxt = behaviour.Get<TMP_Text>("levelTxt");
		hammerNum = behaviour.Get<TMP_Text>("hammerNum");
		fireworksNum = behaviour.Get<TMP_Text>("fireworksNum");
		heardIcon = behaviour.Get<Image>("heardIcon");
		level = behaviour.Get<TMP_Text>("level");
	}
}
