using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace Game.UI.RankListPopup;

public class UIRankListPopupView : GameBaseView
{
	private const string CloseAnimationName = "anim_popup_close";

	public Button btnClose;

	public TMP_Text txtTitle;

	public TMP_Text txtDesc;

	public Button btnSubmit;

	public Animation UIAnimation;

	public TMP_Text BtnText;

	public void PlayCloseAnimation(Action onComplete)
	{
		AnimationClip val = (((Object)(object)UIAnimation != (Object)null) ? UIAnimation.GetClip("anim_popup_close") : null);
		if ((Object)(object)val == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		UIAnimation.Stop();
		UIAnimation.Rewind("anim_popup_close");
		UIAnimation.Play("anim_popup_close");
		GameApp.Timer.Delay((Object)(object)((UIBaseView)this).Behaviour, val.length, (Action)delegate
		{
			onComplete?.Invoke();
		});
	}

	public override void Dispose()
	{
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		btnClose = behaviour.Get<Button>("btnClose");
		txtTitle = behaviour.Get<TMP_Text>("txtTitle");
		txtDesc = behaviour.Get<TMP_Text>("txtDesc");
		btnSubmit = behaviour.Get<Button>("btnSubmit");
		UIAnimation = behaviour.Get<Animation>("UIAnimation");
		BtnText = behaviour.Get<TMP_Text>("BtnText");
	}
}
