using System;
using Game.UI;
using RotateRings.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.BuySuccess;

public class UIBuySuccessView : GameBaseView
{
	private const string OpenAnimName = "anim_popup_open";

	private const string CloseAnimName = "anim_popup_close";

	private const float SingleRowSpacingX = 38f;

	private const int NeedScrollThreshold = 8;

	public Animation UIAnimation;

	public TMP_Text txtTitle;

	public Button btnClose;

	public Button btnContinue;

	public TMP_Text txtContent;

	public Image imgGift;

	public void Init()
	{
	}

	public void PlayOpenAnimation()
	{
		UIAnimUtil.PlayUIOpenAnimation(UIAnimation, "anim_popup_open");
	}

	public void PlayCloseAnimation(Action onComplete)
	{
		if ((Object)(object)UIAnimation == (Object)null || (Object)(object)UIAnimation.GetClip("anim_popup_close") == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		UIAnimation.Stop();
		UIAnimation.Rewind("anim_popup_close");
		UIAnimation.Play("anim_popup_close");
		float length = UIAnimation.GetClip("anim_popup_close").length;
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

	public void RefreshRewardListLayout(int itemCount)
	{
	}

	public override void Dispose()
	{
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		UIAnimation = behaviour.Get<Animation>("UIAnimation");
		txtTitle = behaviour.Get<TMP_Text>("txtTitle");
		btnClose = behaviour.Get<Button>("btnClose");
		btnContinue = behaviour.Get<Button>("btnContinue");
		txtContent = behaviour.Get<TMP_Text>("txtContent");
		imgGift = behaviour.Get<Image>("imgGift");
	}
}
