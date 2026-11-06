using System;
using Game.DataTrack;
using RotateRings.Services;
using RotateRings.UI.Privacy;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Account;
using XingYun.Aot;
using XingYun.Extensions;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.AboutUs;

[UIPath("UIAboutUs")]
[UIPath("UIAboutUsV2")]
public class UIAboutUsController : BaseController<UIAboutUsView>
{
	public UIAboutUsController(UIAboutUsView view)
		: base(view)
	{
	}

	public static bool ShouldShowPrivacyPreferenceEntry()
	{
		if (SDKManager.GetPrivacyPreferenceCode() != 0)
		{
			return true;
		}
		return false;
	}

	public override void OnRegister(object param = null)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		Button btnClose = base.view.BtnClose;
		if (btnClose != null)
		{
			((UnityEvent)btnClose.onClick).AddListener(new UnityAction(OnCloseClick));
		}
		Button btnTerms = base.view.BtnTerms;
		if (btnTerms != null)
		{
			((UnityEvent)btnTerms.onClick).AddListener(new UnityAction(OnTermsClick));
		}
		Button btnPrivacy = base.view.BtnPrivacy;
		if (btnPrivacy != null)
		{
			((UnityEvent)btnPrivacy.onClick).AddListener(new UnityAction(OnPrivacyClick));
		}
		Button btnPrivacyPreference = base.view.BtnPrivacyPreference;
		if (btnPrivacyPreference != null)
		{
			((UnityEvent)btnPrivacyPreference.onClick).AddListener(new UnityAction(OnPrivacyPreferenceClick));
		}
		Button deleteBtn = base.view.deleteBtn;
		if (deleteBtn != null)
		{
			((UnityEvent)deleteBtn.onClick).AddListener(new UnityAction(OnDeleteAccountClick));
		}
		TMP_Text texVersion = base.view.TexVersion;
		if (texVersion != null)
		{
			TMPTextExtensions.SetLocalizedText(texVersion, "Common_Version", new object[1] { Application.version });
		}
		base.view.SetPrivacyPreferenceVisible(ShouldShowPrivacyPreferenceEntry());
		RefreshDeleteAccountEntryVisibility();
	}

	public override void OnActive(bool isActive)
	{
		base.OnActive(isActive);
		if (isActive)
		{
			RefreshDeleteAccountEntryVisibility();
		}
	}

	public override void OnRemove()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		Button btnClose = base.view.BtnClose;
		if (btnClose != null)
		{
			((UnityEvent)btnClose.onClick).RemoveListener(new UnityAction(OnCloseClick));
		}
		Button btnTerms = base.view.BtnTerms;
		if (btnTerms != null)
		{
			((UnityEvent)btnTerms.onClick).RemoveListener(new UnityAction(OnTermsClick));
		}
		Button btnPrivacy = base.view.BtnPrivacy;
		if (btnPrivacy != null)
		{
			((UnityEvent)btnPrivacy.onClick).RemoveListener(new UnityAction(OnPrivacyClick));
		}
		Button btnPrivacyPreference = base.view.BtnPrivacyPreference;
		if (btnPrivacyPreference != null)
		{
			((UnityEvent)btnPrivacyPreference.onClick).RemoveListener(new UnityAction(OnPrivacyPreferenceClick));
		}
		Button deleteBtn = base.view.deleteBtn;
		if (deleteBtn != null)
		{
			((UnityEvent)deleteBtn.onClick).RemoveListener(new UnityAction(OnDeleteAccountClick));
		}
	}

	public override void OnEscape()
	{
		OnCloseClick();
	}

	private void OnCloseClick()
	{
		base.CloseView(false);
	}

	private void OnTermsClick()
	{
		RRDataTrack.Instance.ReportBtnClick("主页设置-条款");
		base.ExecuteCommand<TurnToTermsCommand>((object)null);
	}

	private void OnPrivacyClick()
	{
		RRDataTrack.Instance.ReportBtnClick("主页设置-隐私协议");
		base.ExecuteCommand<TurnToPrivacyCommand>((object)null);
	}

	private void OnPrivacyPreferenceClick()
	{
		SDKManager.CallNativeShowPrivacySdk();
	}

	private void RefreshDeleteAccountEntryVisibility()
	{
		bool deleteAccountVisible = CloudSaveFlowService.IsCloudSaveEnabled() && IsThirdPartyAccountBound();
		base.view.SetDeleteAccountVisible(deleteAccountVisible);
	}

	private static bool IsThirdPartyAccountBound()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		ThirdPartyType platformAccountType = GetPlatformAccountType();
		if ((int)platformAccountType == 0)
		{
			return false;
		}
		AccountComponent account = GameApp.Account;
		if ((Object)(object)account != (Object)null && account.IsInitialized)
		{
			return account.IsBound(platformAccountType);
		}
		return false;
	}

	private static ThirdPartyType GetPlatformAccountType()
	{
		return (ThirdPartyType)3;
	}

	private void OnDeleteAccountClick()
	{
		GameApp.UI.Open("UILoginDeleteAccount", (UILayer)2, (object)null, (Action)null);
	}
}
