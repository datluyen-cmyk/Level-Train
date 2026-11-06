using System;
using Game.DataTrack;
using Game.UI;
using RotateRings.Common;
using RotateRings.Services;
using UnityEngine;
using UnityEngine.Events;
using XingYun.Aot;
using XingYun.Extensions;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Rating;

[UIPath("UIRating")]
public class UIRatingController : GameBaseController<UIRatingView>
{
	private const string RatingPopupAudioVariantId = "variant_947d0004";

	private int selectedStar;

	private int animalStyle = 1;

	private int playIdleTimerId = -1;

	private bool isClosing;

	public UIRatingController(UIRatingView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		selectedStar = 0;
		((UnityEvent)((BaseController<UIRatingView>)this).view.btn_Star1.onClick).AddListener(new UnityAction(OnStar1Click));
		((UnityEvent)((BaseController<UIRatingView>)this).view.btn_Star2.onClick).AddListener(new UnityAction(OnStar2Click));
		((UnityEvent)((BaseController<UIRatingView>)this).view.btn_Star3.onClick).AddListener(new UnityAction(OnStar3Click));
		((UnityEvent)((BaseController<UIRatingView>)this).view.btn_Star4.onClick).AddListener(new UnityAction(OnStar4Click));
		((UnityEvent)((BaseController<UIRatingView>)this).view.btn_Star5.onClick).AddListener(new UnityAction(OnStar5Click));
		((UnityEvent)((BaseController<UIRatingView>)this).view.btn_Close.onClick).AddListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UIRatingView>)this).view.btn_Submit.onClick).AddListener(new UnityAction(OnSubmitClick));
		((BaseController<UIRatingView>)this).view.SetStarState(0);
		((BaseController<UIRatingView>)this).view.SetSubmitInteractable(interactable: false);
		TMPTextExtensions.SetLocalizedText(((BaseController<UIRatingView>)this).view.txtm_Title, "Common_Evaluate_Title", Array.Empty<object>());
		TMPTextExtensions.SetLocalizedText(((BaseController<UIRatingView>)this).view.txtm_Desc, "Common_Evaluate_Des", Array.Empty<object>());
		TMPTextExtensions.SetLocalizedText(((BaseController<UIRatingView>)this).view.txt_Submit, "Common_Evaluate_Button_Send", Array.Empty<object>());
		animalStyle = RatingAnimalStyleResolver.Resolve();
		((BaseController<UIRatingView>)this).view.SetAnimalStyle(animalStyle);
		AudioService.ActivateExperiment("variant_947d0004");
		AudioService.PlayUiSound(AudioKeys.RatingPopup.Value);
		((BaseController<UIRatingView>)this).view.PlayAnimalOpenIdleAnimation();
		((BaseController<UIRatingView>)this).view.PlayOpenAnimation();
		PlayIdleAfterOpenAnimation();
		RRDataTrack.Instance.ReportRatingPopupShow(animalStyle);
		GameApp.Proxy.Get<RatingProxy>()?.RecordPopupShown();
	}

	public override void OnRemove()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		CancelPendingIdleAnimation();
		((UnityEvent)((BaseController<UIRatingView>)this).view.btn_Star1.onClick).RemoveListener(new UnityAction(OnStar1Click));
		((UnityEvent)((BaseController<UIRatingView>)this).view.btn_Star2.onClick).RemoveListener(new UnityAction(OnStar2Click));
		((UnityEvent)((BaseController<UIRatingView>)this).view.btn_Star3.onClick).RemoveListener(new UnityAction(OnStar3Click));
		((UnityEvent)((BaseController<UIRatingView>)this).view.btn_Star4.onClick).RemoveListener(new UnityAction(OnStar4Click));
		((UnityEvent)((BaseController<UIRatingView>)this).view.btn_Star5.onClick).RemoveListener(new UnityAction(OnStar5Click));
		((UnityEvent)((BaseController<UIRatingView>)this).view.btn_Close.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UIRatingView>)this).view.btn_Submit.onClick).RemoveListener(new UnityAction(OnSubmitClick));
		base.OnRemove();
	}

	public override void OnEscape()
	{
		OnCloseClick();
	}

	private void OnStar1Click()
	{
		SelectStar(1);
	}

	private void OnStar2Click()
	{
		SelectStar(2);
	}

	private void OnStar3Click()
	{
		SelectStar(3);
	}

	private void OnStar4Click()
	{
		SelectStar(4);
	}

	private void OnStar5Click()
	{
		SelectStar(5);
	}

	private void SelectStar(int star)
	{
		CancelPendingIdleAnimation();
		((BaseController<UIRatingView>)this).view.StopIdleAnimation();
		selectedStar = star;
		((BaseController<UIRatingView>)this).view.SetStarState(star);
		((BaseController<UIRatingView>)this).view.SetSubmitInteractable(interactable: true);
		bool flag = GameApp.Proxy.Get<RatingProxy>()?.IsHighStar(star) ?? false;
		TMPTextExtensions.SetLocalizedText(((BaseController<UIRatingView>)this).view.txt_Submit, flag ? "Common_Evaluate_Button_Send" : "Common_Evaluate_Button_ToTease", Array.Empty<object>());
		RRDataTrack.Instance.ReportRatingStarSelected(star);
	}

	private void OnSubmitClick()
	{
		if (selectedStar <= 0 || isClosing)
		{
			return;
		}
		RRDataTrack.Instance.ReportRatingStarSubmit(selectedStar);
		RatingProxy ratingProxy = GameApp.Proxy.Get<RatingProxy>();
		if (ratingProxy != null && ratingProxy.IsHighStar(selectedStar))
		{
			isClosing = true;
			CancelPendingIdleAnimation();
			((BaseController<UIRatingView>)this).view.StopIdleAnimation();
			GameApp.Proxy.Get<RatingProxy>()?.MarkHighStarGiven();
			((BaseController<UIRatingView>)this).view.PlayCloseAnimation(delegate
			{
				((BaseController<UIRatingView>)this).CloseView(false);
				SDKManager.RateUs();
				RatingPopupService.CompletePostSettlementFlow();
			});
		}
		else
		{
			GameApp.Proxy.Get<RatingProxy>()?.RecordLowStarSubmit();
			GameApp.UI.Open("UIRatingFeedback", (UILayer)4, (object)new RatingFeedbackParam(selectedStar), (Action)null);
			((BaseController<UIRatingView>)this).CloseView(false);
		}
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UIRatingView>)this).OnActive(isActive);
		((BaseController<UIRatingView>)this).view.OnUiStackActiveChanged(isActive);
	}

	private void OnCloseClick()
	{
		if (!isClosing)
		{
			isClosing = true;
			CancelPendingIdleAnimation();
			((BaseController<UIRatingView>)this).view.StopIdleAnimation();
			RRDataTrack.Instance.ReportRatingPopupClose(animalStyle);
			GameApp.Proxy.Get<RatingProxy>()?.RecordClose();
			((BaseController<UIRatingView>)this).view.PlayCloseAnimation(delegate
			{
				((BaseController<UIRatingView>)this).CloseView(false);
				RatingPopupService.CompletePostSettlementFlow();
			});
		}
	}

	private void PlayIdleAfterOpenAnimation()
	{
		CancelPendingIdleAnimation();
		float openAnimationDuration = ((BaseController<UIRatingView>)this).view.GetOpenAnimationDuration();
		if (openAnimationDuration <= 0f)
		{
			((BaseController<UIRatingView>)this).view.PlayIdleAnimation();
		}
		else
		{
			playIdleTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIRatingView>)this).view).NodeRect, openAnimationDuration, (Action)TryPlayIdleAnimation);
		}
	}

	private void TryPlayIdleAnimation()
	{
		playIdleTimerId = -1;
		if (selectedStar <= 0)
		{
			((BaseController<UIRatingView>)this).view.PlayIdleAnimation();
		}
	}

	private void CancelPendingIdleAnimation()
	{
		if (playIdleTimerId >= 0)
		{
			GameApp.Timer.Kill(playIdleTimerId);
			playIdleTimerId = -1;
		}
	}
}
