using System;
using Game.DataTrack;
using RotateRings.UI.Level;
using Rotaterings.UI.RankList;
using UnityEngine;
using UnityEngine.Events;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace Game.UI.RankListPopup;

[UIPath("UIRankListPopup")]
[UIPath("UIRankListPopupHalloween")]
public class UIRankListPopupController : GameBaseController<UIRankListPopupView>
{
	private Action onClosed;

	private bool isClosing;

	private bool enterLevelOnSubmit;

	public UIRankListPopupController(UIRankListPopupView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		UIRankListPopupParam uIRankListPopupParam = param as UIRankListPopupParam;
		onClosed = uIRankListPopupParam?.OnClosed;
		enterLevelOnSubmit = uIRankListPopupParam?.EnterLevelOnSubmit ?? true;
		isClosing = false;
		RRDataTrack.Instance.ReportBtnClick("排行榜-活动弹窗-打开");
		if ((Object)(object)((BaseController<UIRankListPopupView>)this).view.btnClose != (Object)null)
		{
			((Component)((BaseController<UIRankListPopupView>)this).view.btnClose).gameObject.SetActive(!ShouldHideCloseButton());
		}
		((UnityEvent)((BaseController<UIRankListPopupView>)this).view.btnClose.onClick).AddListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UIRankListPopupView>)this).view.btnSubmit.onClick).AddListener(new UnityAction(OnSubmitClick));
	}

	private static bool ShouldHideCloseButton()
	{
		int rankPlayerName = RankFunctionResolver.GetRankPlayerName();
		if (rankPlayerName != 1)
		{
			return rankPlayerName == 3;
		}
		return true;
	}

	private void OnCloseClick()
	{
		CloseWithAnimation(onClosed);
	}

	private void OnSubmitClick()
	{
		if (isClosing)
		{
			return;
		}
		RRDataTrack.Instance.ReportBtnClick("排行榜-活动弹窗-参赛");
		if (!enterLevelOnSubmit)
		{
			CloseWithAnimation(onClosed);
			return;
		}
		CloseWithAnimation(delegate
		{
			((BaseController<UIRankListPopupView>)this).ExecuteCommand<EnterLevelCommand>((object)null);
		});
	}

	private void CloseWithAnimation(Action onComplete)
	{
		if (!isClosing)
		{
			isClosing = true;
			((BaseController<UIRankListPopupView>)this).view.PlayCloseAnimation(delegate
			{
				((BaseController<UIRankListPopupView>)this).CloseView(false);
				onComplete?.Invoke();
			});
		}
	}

	public override void OnRemove()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		((UnityEvent)((BaseController<UIRankListPopupView>)this).view.btnClose.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UIRankListPopupView>)this).view.btnSubmit.onClick).RemoveListener(new UnityAction(OnSubmitClick));
		base.OnRemove();
	}

	public override void OnEscape()
	{
		CloseWithAnimation(onClosed);
	}
}
