using System;
using Game.UI;
using RotateRings.Common;
using RotateRings.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.Home;

public class UIHomeBtnOKView : GameWidgetView
{
	private const string LevelTextKey = "Button_LevelXX";

	private const string RoundTextKey = "Common_Round";

	private const string ContinueTextKey = "Button_Continue";

	private const string HardTextKey = "Common_Hard";

	private const string VeryHardTextKey = "Common_SuperHard";

	private const string HalloweenLevelTextKey = "Ring_Halloween_Level_Out";

	private const string ButtonIdleAnim = "anim_uihomebutton_idle";

	private const float ButtonIdleInterval = 5f;

	private int buttonIdleTimerId = -1;

	public Button btnOK;

	public GameObject btnNormal;

	public TMP_Text textLevel;

	public GameObject btnContinue;

	public TMP_Text textContinue;

	public TMP_Text textLevelContinue;

	public GameObject btnHard;

	public TMP_Text textLevelHard;

	public TMP_Text textHard;

	public GameObject btnVeryHard;

	public TMP_Text textLevelVeryHard;

	public TMP_Text textVeryHard;

	public GameObject roundNormal;

	public TMP_Text textRoundNormal;

	public GameObject roundContinue;

	public TMP_Text textRoundContinue;

	public TMP_Text textRoundLevel;

	public GameObject roundHard;

	public TMP_Text textRoundHardLevel;

	public TMP_Text textRoundHard;

	public GameObject roundVeryHard;

	public TMP_Text textRoundVeryHardLevel;

	public TMP_Text textRoundVeryHard;

	public Animation anim;

	public void Initialize(bool playIdleAnimation)
	{
		StopButtonIdleLoop();
		if (playIdleAnimation)
		{
			PlayButtonIdleAnimation();
			buttonIdleTimerId = GameApp.Timer.Repeat((Object)(object)((UIBaseView)this).Behaviour, 5f, (Action)PlayButtonIdleAnimation);
		}
	}

	public void StopButtonIdleLoop()
	{
		if (buttonIdleTimerId != -1)
		{
			GameApp.Timer.Kill(buttonIdleTimerId);
			buttonIdleTimerId = -1;
		}
	}

	private void PlayButtonIdleAnimation()
	{
		if ((Object)(object)anim == (Object)null)
		{
			return;
		}
		AnimationState val = anim["anim_uihomebutton_idle"];
		if (!((TrackedReference)(object)val == (TrackedReference)null))
		{
			if (anim.IsPlaying("anim_uihomebutton_idle"))
			{
				anim.Stop("anim_uihomebutton_idle");
				val.time = 0f;
				anim.Sample();
			}
			val.wrapMode = (WrapMode)8;
			anim.Play("anim_uihomebutton_idle");
		}
	}

	public override void Dispose()
	{
		StopButtonIdleLoop();
		base.Dispose();
	}

	public void Refresh(ELevelDifficulty difficulty, bool hasUnfinishedLevel, GameMode gameMode, int level)
	{
		HideAllSkins();
		bool flag = difficulty == ELevelDifficulty.Hard;
		bool flag2 = difficulty == ELevelDifficulty.VeryHard;
		bool isNormal = !flag && !flag2;
		string levelText = ((gameMode == GameMode.Halloween) ? GetHalloweenLevelText() : GameApp.Lang.GetText("Button_LevelXX", new object[1] { level }));
		string text = GameApp.Lang.GetText("Common_Round", new object[1] { level });
		string text2 = GameApp.Lang.GetText("Button_Continue", Array.Empty<object>());
		string text3 = GameApp.Lang.GetText("Common_Hard", Array.Empty<object>());
		string text4 = GameApp.Lang.GetText("Common_SuperHard", Array.Empty<object>());
		if (gameMode == GameMode.Endless)
		{
			RefreshEndless(isNormal, flag, flag2, hasUnfinishedLevel, text, text2, text3, text4);
		}
		else
		{
			RefreshMainline(isNormal, flag, flag2, hasUnfinishedLevel, levelText, text2, text3, text4);
		}
		RefreshContinueLabels();
	}

	private static string GetHalloweenLevelText()
	{
		string text = GameApp.Lang.GetText("Ring_Halloween_Level_Out", Array.Empty<object>());
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return "Ring_Halloween_Level_Out";
	}

	public void RefreshSettlementContinue()
	{
		HideAllSkins();
		SetActive(btnNormal, active: true);
		SetText(textLevel, GameApp.Lang.GetText("Button_Continue", Array.Empty<object>()));
	}

	public void SetInteractable(bool interactable)
	{
		if ((Object)(object)btnOK != (Object)null)
		{
			((Selectable)btnOK).interactable = interactable;
		}
	}

	private void HideAllSkins()
	{
		SetActive(btnNormal, active: false);
		SetActive(btnContinue, active: false);
		SetActive(btnHard, active: false);
		SetActive(btnVeryHard, active: false);
		SetActive(roundNormal, active: false);
		SetActive(roundContinue, active: false);
		SetActive(roundHard, active: false);
		SetActive(roundVeryHard, active: false);
	}

	private void RefreshMainline(bool isNormal, bool isHard, bool isVeryHard, bool hasUnfinishedLevel, string levelText, string continueText, string hardText, string veryHardText)
	{
		if (isNormal)
		{
			if (hasUnfinishedLevel)
			{
				SetActive(btnContinue, active: true);
				SetText(textLevelContinue, levelText);
			}
			else
			{
				SetActive(btnNormal, active: true);
				SetText(textLevel, levelText);
			}
		}
		else if (isHard)
		{
			SetActive(btnHard, active: true);
			SetText(textLevelHard, hasUnfinishedLevel ? continueText : levelText);
			SetText(textHard, hasUnfinishedLevel ? levelText : hardText);
		}
		else if (isVeryHard)
		{
			SetActive(btnVeryHard, active: true);
			SetText(textLevelVeryHard, hasUnfinishedLevel ? continueText : levelText);
			SetText(textVeryHard, hasUnfinishedLevel ? levelText : veryHardText);
		}
	}

	private void RefreshEndless(bool isNormal, bool isHard, bool isVeryHard, bool hasUnfinishedLevel, string roundText, string continueText, string hardText, string veryHardText)
	{
		if (isNormal)
		{
			if (hasUnfinishedLevel)
			{
				SetActive(roundContinue, active: true);
				SetText(textRoundLevel, roundText);
			}
			else
			{
				SetActive(roundNormal, active: true);
				SetText(textRoundNormal, roundText);
			}
		}
		else if (isHard)
		{
			SetActive(roundHard, active: true);
			SetText(textRoundHardLevel, hasUnfinishedLevel ? continueText : roundText);
			SetText(textRoundHard, hasUnfinishedLevel ? roundText : hardText);
		}
		else if (isVeryHard)
		{
			SetActive(roundVeryHard, active: true);
			SetText(textRoundVeryHardLevel, hasUnfinishedLevel ? continueText : roundText);
			SetText(textRoundVeryHard, hasUnfinishedLevel ? roundText : veryHardText);
		}
	}

	private void RefreshContinueLabels()
	{
		SetLocalizedTextKey(textContinue, "Button_Continue");
		SetLocalizedTextKey(textRoundContinue, "Button_Continue");
	}

	private static void SetLocalizedTextKey(TMP_Text text, string key)
	{
		if ((Object)(object)text != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(text, key, Array.Empty<object>());
		}
	}

	private static void SetText(TMP_Text text, string value)
	{
		if ((Object)(object)text != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(text, value);
		}
	}

	private static void SetActive(GameObject target, bool active)
	{
		if ((Object)(object)target != (Object)null)
		{
			target.SetActive(active);
		}
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		btnOK = behaviour.Get<Button>("btnOK");
		btnNormal = behaviour.Get<GameObject>("btnNormal");
		textLevel = behaviour.Get<TMP_Text>("textLevel");
		btnContinue = behaviour.Get<GameObject>("btnContinue");
		textContinue = behaviour.Get<TMP_Text>("textContinue");
		textLevelContinue = behaviour.Get<TMP_Text>("textLevelContinue");
		btnHard = behaviour.Get<GameObject>("btnHard");
		textLevelHard = behaviour.Get<TMP_Text>("textLevelHard");
		textHard = behaviour.Get<TMP_Text>("textHard");
		btnVeryHard = behaviour.Get<GameObject>("btnVeryHard");
		textLevelVeryHard = behaviour.Get<TMP_Text>("textLevelVeryHard");
		textVeryHard = behaviour.Get<TMP_Text>("textVeryHard");
		roundNormal = behaviour.Get<GameObject>("roundNormal");
		textRoundNormal = behaviour.Get<TMP_Text>("textRoundNormal");
		roundContinue = behaviour.Get<GameObject>("roundContinue");
		textRoundContinue = behaviour.Get<TMP_Text>("textRoundContinue");
		textRoundLevel = behaviour.Get<TMP_Text>("textRoundLevel");
		roundHard = behaviour.Get<GameObject>("roundHard");
		textRoundHardLevel = behaviour.Get<TMP_Text>("textRoundHardLevel");
		textRoundHard = behaviour.Get<TMP_Text>("textRoundHard");
		roundVeryHard = behaviour.Get<GameObject>("roundVeryHard");
		textRoundVeryHardLevel = behaviour.Get<TMP_Text>("textRoundVeryHardLevel");
		textRoundVeryHard = behaviour.Get<TMP_Text>("textRoundVeryHard");
		anim = behaviour.Get<Animation>("anim");
	}
}
