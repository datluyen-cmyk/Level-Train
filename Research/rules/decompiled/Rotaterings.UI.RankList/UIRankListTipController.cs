using System;
using Game.UI;
using RotateRings.Common;
using RotateRings.Services;
using Rotaterings.Rank;
using UnityEngine;
using UnityEngine.Events;
using XingYun.Extensions;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace Rotaterings.UI.RankList;

[UIPath("UIRankListTip")]
[UIPath("UIRankListTipHalloween")]
public class UIRankListTipController : GameBaseController<UIRankListTipView>
{
	private bool animFinished;

	private int countdownTimerId = -1;

	public UIRankListTipController(UIRankListTipView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		((UnityEvent)((BaseController<UIRankListTipView>)this).view.btnClose.onClick).AddListener(new UnityAction(OnBgClick));
		((BaseController<UIRankListTipView>)this).view.InitAnimation();
		animFinished = false;
		StartCountdown();
	}

	private void OnBgClick()
	{
		AudioService.PlayUiSound(AudioKeys.BtnClick.Value);
		if (!animFinished && ((BaseController<UIRankListTipView>)this).view.IsAnimationPlaying())
		{
			((BaseController<UIRankListTipView>)this).view.SkipAnimation();
			animFinished = true;
		}
		else
		{
			animFinished = true;
			((BaseController<UIRankListTipView>)this).CloseView(false);
		}
	}

	public override void OnRemove()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		StopCountdown();
		((UnityEvent)((BaseController<UIRankListTipView>)this).view.btnClose.onClick).RemoveListener(new UnityAction(OnBgClick));
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UIRankListTipView>)this).OnActive(isActive);
	}

	public override void OnEscape()
	{
		OnBgClick();
	}

	private void StartCountdown()
	{
		StopCountdown();
		UpdateCountdownText();
		countdownTimerId = GameApp.Timer.Repeat(1f, (Action)UpdateCountdownText);
	}

	private void StopCountdown()
	{
		if (countdownTimerId >= 0)
		{
			GameApp.Timer.Kill(countdownTimerId);
			countdownTimerId = -1;
		}
	}

	private void UpdateCountdownText()
	{
		if (!((Object)(object)((BaseController<UIRankListTipView>)this).view?.txtCountDown == (Object)null))
		{
			long num = RankActivityEntry.Service?.GetRemainSeconds() ?? 0;
			TMPTextExtensions.SetLocalizedTextValue(((BaseController<UIRankListTipView>)this).view.txtCountDown, RankActivityService.FormatCountdown(num));
			if (num <= 0)
			{
				StopCountdown();
			}
		}
	}
}
