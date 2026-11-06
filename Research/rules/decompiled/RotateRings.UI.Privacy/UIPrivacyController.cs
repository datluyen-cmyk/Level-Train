using System;
using Game.DataTrack;
using UnityEngine;
using UnityEngine.Events;
using XingYun.Command;
using XingYun.Core.Singletons;

namespace RotateRings.UI.Privacy;

public class UIPrivacyController
{
	private readonly UIPrivacyView view;

	private readonly Action onClosed;

	public UIPrivacyController(UIPrivacyView view, Action onClosed)
	{
		this.view = view;
		this.onClosed = onClosed;
	}

	public void OnShow()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		view.RefreshPrivacyDescription();
		view.RegisterDescLinkClick(OnTermsLinkClick, OnPrivacyLinkClick);
		((UnityEvent)view.BtnAccept.onClick).AddListener(new UnityAction(OnAcceptClick));
		view.PlayOpenAnimation();
		RRDataTrack.Instance.ReportBtnClick("隐私条款-弹窗展示");
	}

	public void OnDispose()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		((UnityEvent)view.BtnAccept.onClick).RemoveListener(new UnityAction(OnAcceptClick));
		view.UnregisterDescLinkClick();
	}

	private void OnAcceptClick()
	{
		RRDataTrack.Instance.ReportPrivacyAgree();
		RRDataTrack.Instance.ReportBtnClick("隐私条款-确定关闭弹窗");
		float num = view.PlayCloseAnimation();
		GameApp.Timer.Delay((Object)(object)((Component)view).gameObject, num, (Action)delegate
		{
			UIPrivacyGate.MarkAgreed();
			onClosed?.Invoke();
		});
	}

	private void OnTermsLinkClick()
	{
		RRDataTrack.Instance.ReportPrivacyTermsClick();
		Singleton<CommandManager>.Instance.Execute<TurnToTermsCommand>((object)null);
	}

	private void OnPrivacyLinkClick()
	{
		RRDataTrack.Instance.ReportPrivacyPolicyClick();
		Singleton<CommandManager>.Instance.Execute<TurnToPrivacyCommand>((object)null);
	}
}
