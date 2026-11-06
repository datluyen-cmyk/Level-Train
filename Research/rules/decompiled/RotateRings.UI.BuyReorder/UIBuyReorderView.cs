using System;
using Game.UI;
using RotateRings.Common;
using UnityEngine;
using UnityEngine.UI;
using WordSolitaire.UI.Shop;
using XingYun.UI.Core;

namespace RotateRings.UI.BuyReorder;

public class UIBuyReorderView : GameBaseView
{
	private const string OpenAnimName = "anim_popup_open";

	private const string CloseAnimName = "anim_popup_close";

	public Animation UIAnimation;

	public Button btnClaim;

	public Button btnClose;

	public GridLayoutGroup RewardListLayout;

	public BuyItemView itemTemp;

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
		btnClaim = behaviour.Get<Button>("btnClaim");
		btnClose = behaviour.Get<Button>("btnClose");
		RewardListLayout = behaviour.Get<GridLayoutGroup>("RewardListLayout");
		itemTemp = behaviour.Get<BuyItemView>("itemTemp");
	}
}
