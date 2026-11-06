using System;
using Game.DataTrack;
using Game.UI;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Rating;

[UIPath("UIRatingFeedback")]
public class UIRatingFeedbackController : GameBaseController<UIRatingFeedbackView>
{
	private const string FeedbackTitleKey = "Common_Evaluate_Title";

	private const string FeedbackPlaceholderKey = "Common_Evaluate_Feedback";

	private int starRating;

	private bool isClosing;

	public UIRatingFeedbackController(UIRatingFeedbackView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		starRating = ((param is RatingFeedbackParam ratingFeedbackParam) ? ratingFeedbackParam.Star : 0);
		((UnityEvent)((BaseController<UIRatingFeedbackView>)this).view.btn_Close.onClick).AddListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UIRatingFeedbackView>)this).view.btn_Submit.onClick).AddListener(new UnityAction(OnSubmitClick));
		((UnityEvent<string>)(object)((BaseController<UIRatingFeedbackView>)this).view.inp_Feedback.onValueChanged).AddListener((UnityAction<string>)OnInputChanged);
		((BaseController<UIRatingFeedbackView>)this).view.SetStarState(starRating);
		TMPTextExtensions.SetLocalizedText(((BaseController<UIRatingFeedbackView>)this).view.txtm_Title, "Common_Evaluate_Title", Array.Empty<object>());
		((BaseController<UIRatingFeedbackView>)this).view.inp_Feedback.text = string.Empty;
		Graphic placeholder = ((BaseController<UIRatingFeedbackView>)this).view.inp_Feedback.placeholder;
		Text val = (Text)(object)((placeholder is Text) ? placeholder : null);
		if (val != null)
		{
			val.text = GameApp.Lang.GetText("Common_Evaluate_Feedback", Array.Empty<object>());
		}
		((BaseController<UIRatingFeedbackView>)this).view.SetSubmitInteractable(interactable: false);
	}

	public override void OnRemove()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		((UnityEvent)((BaseController<UIRatingFeedbackView>)this).view.btn_Close.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UIRatingFeedbackView>)this).view.btn_Submit.onClick).RemoveListener(new UnityAction(OnSubmitClick));
		((UnityEvent<string>)(object)((BaseController<UIRatingFeedbackView>)this).view.inp_Feedback.onValueChanged).RemoveListener((UnityAction<string>)OnInputChanged);
		base.OnRemove();
	}

	public override void OnEscape()
	{
		OnCloseClick();
	}

	private void OnInputChanged(string text)
	{
		((BaseController<UIRatingFeedbackView>)this).view.SetSubmitInteractable(!string.IsNullOrWhiteSpace(text));
	}

	private void OnSubmitClick()
	{
		string feedbackText = ((BaseController<UIRatingFeedbackView>)this).view.GetFeedbackText();
		if (!string.IsNullOrWhiteSpace(feedbackText))
		{
			RRDataTrack.Instance.ReportRatingFeedbackSubmit(feedbackText);
			CloseRatingFlow();
		}
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UIRatingFeedbackView>)this).OnActive(isActive);
		((BaseController<UIRatingFeedbackView>)this).view.OnUiStackActiveChanged(isActive);
	}

	private void OnCloseClick()
	{
		if (!isClosing)
		{
			isClosing = true;
			RRDataTrack.Instance.ReportRatingFeedbackClose(((BaseController<UIRatingFeedbackView>)this).view.GetFeedbackText());
			CloseRatingFlow();
		}
	}

	private void CloseRatingFlow()
	{
		((BaseController<UIRatingFeedbackView>)this).view.PlayCloseAnimation(delegate
		{
			((BaseController<UIRatingFeedbackView>)this).CloseView(false);
			RatingPopupService.CompletePostSettlementFlow();
		});
	}
}
