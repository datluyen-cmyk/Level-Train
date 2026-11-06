using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.AboutUs;

public class UIAboutUsView : UIBaseView
{
	public Animation UIAnimation;

	public Button BtnClose;

	public TMP_Text TexVersion;

	public Button BtnTerms;

	public Button BtnPrivacy;

	public Button BtnPrivacyPreference;

	public Button deleteBtn;

	public ScrollRect rscr_BtnRoot;

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		UIAnimation = behaviour.Get<Animation>("UIAnimation");
		BtnClose = behaviour.Get<Button>("BtnClose");
		TexVersion = behaviour.Get<TMP_Text>("TexVersion");
		BtnTerms = behaviour.Get<Button>("BtnTerms");
		BtnPrivacy = behaviour.Get<Button>("BtnPrivacy");
		BtnPrivacyPreference = behaviour.Get<Button>("BtnPrivacyPreference");
		deleteBtn = behaviour.Get<Button>("deleteBtn");
		rscr_BtnRoot = behaviour.Get<ScrollRect>("rscr_BtnRoot");
	}

	public void SetDeleteAccountVisible(bool visible)
	{
		if (!((Object)(object)deleteBtn == (Object)null))
		{
			((Component)deleteBtn).gameObject.SetActive(visible);
		}
	}

	public void SetPrivacyPreferenceVisible(bool visible)
	{
		if ((Object)(object)BtnPrivacyPreference != (Object)null)
		{
			((Behaviour)rscr_BtnRoot).enabled = visible;
			((Component)BtnPrivacyPreference).gameObject.SetActive(visible);
		}
	}
}
