using System;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.AdPop;

public class AdPopHaveNetWidget : GameWidgetView
{
	private const string DescInitialLangKey = "AdsLoading_Word_Disable";

	private const string DescLoadingLangKey = "Ads_NoNetwork_Desc_2";

	public Button btnContinue;

	public Button btnRestart;

	public Button btnClose;

	public TMP_Text txtTitle;

	public TMP_Text txtDesc;

	public TMP_Text txtRetry;

	public TMP_Text txtReplay;

	public GameObject goGreenRetry;

	public GameObject goAdBadge;

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		btnContinue = behaviour.Get<Button>("btnContinue");
		btnRestart = behaviour.Get<Button>("btnRestart");
		btnClose = behaviour.Get<Button>("btnClose");
		txtTitle = behaviour.Get<TMP_Text>("txtTitle");
		txtDesc = behaviour.Get<TMP_Text>("txtDesc");
		txtRetry = behaviour.Get<TMP_Text>("txtRetry");
		txtReplay = behaviour.Get<TMP_Text>("txtReplay");
		goGreenRetry = behaviour.Get<GameObject>("goGreenRetry");
		goAdBadge = behaviour.Get<GameObject>("goAdBadge");
	}

	public void SetGreenRetryVisible(bool visible)
	{
		if ((Object)(object)goGreenRetry != (Object)null)
		{
			goGreenRetry.SetActive(visible);
		}
	}

	public void SetAdBadgeVisible(bool visible)
	{
		if ((Object)(object)goAdBadge != (Object)null)
		{
			goAdBadge.SetActive(visible);
		}
	}

	public void SetButtonsInteractable(bool interactable)
	{
		if ((Object)(object)btnContinue != (Object)null)
		{
			((Selectable)btnContinue).interactable = interactable;
		}
		if ((Object)(object)btnRestart != (Object)null)
		{
			((Selectable)btnRestart).interactable = interactable;
		}
		if ((Object)(object)btnClose != (Object)null)
		{
			((Selectable)btnClose).interactable = interactable;
		}
	}

	public void SetDescInitial()
	{
		TMPTextExtensions.SetLocalizedText(txtDesc, "AdsLoading_Word_Disable", Array.Empty<object>());
	}

	public void SetDescLoading()
	{
		TMPTextExtensions.SetLocalizedText(txtDesc, "Ads_NoNetwork_Desc_2", Array.Empty<object>());
	}

	public void SetRetryLocalized(string langKey)
	{
		TMPTextExtensions.SetLocalizedText(txtRetry, langKey, Array.Empty<object>());
	}

	public void SetReplayLabel(string text)
	{
		TMPTextExtensions.SetLocalizedText(txtReplay, text, Array.Empty<object>());
	}
}
