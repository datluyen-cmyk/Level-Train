using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.SpineAnimSequence;
using XingYun.UI.Core;

namespace RotateRings.UI.HowToPlay;

public class UIHowToPlayView : UIBaseView
{
	private const string OpenAnimName = "anim_popup_open";

	private const string CloseAnimName = "anim_popup_close";

	private const int PageCount = 3;

	private const string Content1OpenAnimName = "ui_content1_open";

	private const string Content2OpenAnimName = "ui_content2_open";

	private const string Content3OpenAnimName = "ui_content3_open";

	public bool playContentLoopAnim = true;

	private List<Toggle> pageDotItems = new List<Toggle>(3);

	private int currentPageIndex = -1;

	private Tweener moveTweener;

	private PageSwipeHandler swipeHandler;

	private int lastAnimPlayedPageIndex = -1;

	private int lastPlayedSpineHandPageIndex = -1;

	public Action<int> OnSwipePageChanged;

	public Button BtnClose;

	public Button BtnNextFirst;

	public Button BtnNextMid;

	public Button BtnCompletePage;

	public TMP_Text TxtContent;

	public Animation anim;

	public Animation Content1Anim;

	public Animation Content2Anim;

	public Animation Content3Anim;

	public SpineAnimSequencePlayer SpineHand1;

	public SpineAnimSequencePlayer SpineHand2;

	public SpineAnimSequencePlayer SpineHand3;

	public Toggle Toggle_Dot1;

	public Toggle Toggle_Dot2;

	public Toggle Toggle_Dot3;

	public RectTransform ShowContent;

	public Button BtnPreviousPage;

	public void InitToggleDot()
	{
		pageDotItems.Clear();
		pageDotItems.Add(Toggle_Dot1);
		pageDotItems.Add(Toggle_Dot2);
		pageDotItems.Add(Toggle_Dot3);
	}

	public void InitSwipeDetection(int totalPages)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Canvas componentInParent = ((Component)ShowContent).GetComponentInParent<Canvas>();
		Camera eventCamera = null;
		if ((Object)(object)componentInParent != (Object)null && (int)componentInParent.renderMode != 0)
		{
			eventCamera = componentInParent.worldCamera;
		}
		swipeHandler = ((Component)ShowContent).gameObject.GetComponent<PageSwipeHandler>();
		if ((Object)(object)swipeHandler == (Object)null)
		{
			swipeHandler = ((Component)ShowContent).gameObject.AddComponent<PageSwipeHandler>();
		}
		int currentPage = ((currentPageIndex >= 0) ? currentPageIndex : 0);
		swipeHandler.Setup(ShowContent, totalPages, currentPage, eventCamera);
		swipeHandler.OnDragStarted = delegate
		{
			Tweener obj = moveTweener;
			if (obj != null)
			{
				TweenExtensions.Kill((Tween)(object)obj, false);
			}
		};
		swipeHandler.OnSwipeComplete = OnSwipeCompleteHandler;
	}

	private void OnSwipeCompleteHandler(int direction)
	{
		if (direction != 0)
		{
			OnSwipePageChanged?.Invoke(direction);
		}
		else
		{
			ViewMove(currentPageIndex);
		}
	}

	public void ShowPage(int pageIndex, int totalPages)
	{
		if (totalPages <= 0)
		{
			return;
		}
		pageIndex = Mathf.Clamp(pageIndex, 0, totalPages - 1);
		pageIndex = Mathf.Min(pageIndex, pageDotItems.Count - 1);
		for (int i = 0; i < pageDotItems.Count; i++)
		{
			if ((Object)(object)pageDotItems[i] != (Object)null)
			{
				pageDotItems[i].isOn = i == pageIndex;
			}
		}
		ViewMove(pageIndex);
		RefeshContentText(pageIndex);
		currentPageIndex = pageIndex;
		swipeHandler?.SetCurrentPage(pageIndex);
		SetNavButtonState(pageIndex, totalPages);
	}

	public void SetNavButtonState(int pageIndex, int totalPages)
	{
		bool flag = pageIndex <= 0;
		bool flag2 = pageIndex >= totalPages - 1;
		SetActive(BtnNextFirst, flag);
		SetActive(BtnNextMid, !flag && !flag2);
		SetActive(BtnPreviousPage, !flag);
		SetActive(BtnCompletePage, flag2);
	}

	private void ViewMove(int pageIndex)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		Tweener obj = moveTweener;
		if (obj != null)
		{
			TweenExtensions.Kill((Tween)(object)obj, false);
		}
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector((float)(-1 * pageIndex) * ShowContent.sizeDelta.x, 0f);
		moveTweener = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPos(ShowContent, val, 0.35f, false), (Ease)1);
		TweenSettingsExtensions.OnComplete<Tweener>(moveTweener, (TweenCallback)delegate
		{
			try
			{
				OnPageMoveComplete(pageIndex);
			}
			catch (Exception ex)
			{
				Log.Error((object)("[UIHowToPlayView] 页面移动完成回调异常: " + ex.Message), Array.Empty<object>());
			}
		});
	}

	private void RefeshContentText(int pageIndex)
	{
		TMPTextExtensions.SetLocalizedText(TxtContent, $"Tutorial_Setting_TipText{pageIndex + 1}", Array.Empty<object>());
	}

	private static void SetActive(Button button, bool isActive)
	{
		if ((Object)(object)button != (Object)null)
		{
			((Component)button).gameObject.SetActive(isActive);
		}
	}

	public void PlayOpenAnimation()
	{
		PlayAnim("anim_popup_open");
	}

	public void PlayCloseAnimation(Action onComplete = null)
	{
		if ((Object)(object)anim == (Object)null || !Object.op_Implicit((Object)(object)anim.GetClip("anim_popup_close")))
		{
			if ((Object)(object)anim != (Object)null)
			{
				Log.Error((object)"[UIHowToPlayView] 动画 anim_popup_close 不存在", Array.Empty<object>());
			}
			onComplete?.Invoke();
			return;
		}
		anim.Rewind("anim_popup_close");
		anim.Play("anim_popup_close");
		float length = anim.GetClip("anim_popup_close").length;
		GameApp.Timer.Delay(length, (Action)delegate
		{
			onComplete?.Invoke();
		});
	}

	private void PlayAnim(string animName)
	{
		if (!((Object)(object)anim == (Object)null) && !string.IsNullOrEmpty(animName))
		{
			if (!Object.op_Implicit((Object)(object)anim.GetClip(animName)))
			{
				Log.Error((object)("[UIHowToPlayView] 动画 " + animName + " 不存在"), Array.Empty<object>());
				return;
			}
			anim.Rewind(animName);
			anim.Play(animName);
		}
	}

	private void OnPageMoveComplete(int pageIndex)
	{
		if (playContentLoopAnim)
		{
			StopPageSpineHandAnimation(lastPlayedSpineHandPageIndex);
			PlayPageSpineHandAnimation(pageIndex);
			lastPlayedSpineHandPageIndex = pageIndex;
			StopPageContentAnimation(lastAnimPlayedPageIndex);
			PlayPageContentAnimation(pageIndex);
			lastAnimPlayedPageIndex = pageIndex;
		}
	}

	private void PlayPageSpineHandAnimation(int pageIndex)
	{
		SpineAnimSequencePlayer pageSpineHand = GetPageSpineHand(pageIndex);
		if (!((Object)(object)pageSpineHand == (Object)null))
		{
			pageSpineHand.Play();
		}
	}

	private void StopPageSpineHandAnimation(int pageIndex)
	{
		if (pageIndex >= 0)
		{
			SpineAnimSequencePlayer pageSpineHand = GetPageSpineHand(pageIndex);
			if (!((Object)(object)pageSpineHand == (Object)null) && pageSpineHand.IsPlaying)
			{
				pageSpineHand.Stop();
			}
		}
	}

	private void PlayPageContentAnimation(int pageIndex)
	{
		Animation pageContentAnimation = GetPageContentAnimation(pageIndex);
		string pageContentAnimName = GetPageContentAnimName(pageIndex);
		if (!((Object)(object)pageContentAnimation == (Object)null) && !string.IsNullOrEmpty(pageContentAnimName))
		{
			if (!Object.op_Implicit((Object)(object)pageContentAnimation.GetClip(pageContentAnimName)))
			{
				Log.Error((object)("[UIHowToPlayView] 动画 " + pageContentAnimName + " 不存在"), Array.Empty<object>());
				return;
			}
			pageContentAnimation.wrapMode = (WrapMode)2;
			pageContentAnimation.Rewind(pageContentAnimName);
			pageContentAnimation.Play(pageContentAnimName);
		}
	}

	private void StopPageContentAnimation(int pageIndex)
	{
		if (pageIndex >= 0)
		{
			Animation pageContentAnimation = GetPageContentAnimation(pageIndex);
			string pageContentAnimName = GetPageContentAnimName(pageIndex);
			if (!((Object)(object)pageContentAnimation == (Object)null) && !string.IsNullOrEmpty(pageContentAnimName) && pageContentAnimation.IsPlaying(pageContentAnimName))
			{
				pageContentAnimation.Stop(pageContentAnimName);
			}
		}
	}

	private void StopAllPageContentAnimations()
	{
		if ((Object)(object)Content1Anim != (Object)null)
		{
			Content1Anim.Stop();
		}
		if ((Object)(object)Content2Anim != (Object)null)
		{
			Content2Anim.Stop();
		}
		if ((Object)(object)Content3Anim != (Object)null)
		{
			Content3Anim.Stop();
		}
	}

	private Animation GetPageContentAnimation(int pageIndex)
	{
		return (Animation)(pageIndex switch
		{
			0 => Content1Anim, 
			1 => Content2Anim, 
			2 => Content3Anim, 
			_ => null, 
		});
	}

	private string GetPageContentAnimName(int pageIndex)
	{
		return pageIndex switch
		{
			0 => "ui_content1_open", 
			1 => "ui_content2_open", 
			2 => "ui_content3_open", 
			_ => null, 
		};
	}

	private SpineAnimSequencePlayer GetPageSpineHand(int pageIndex)
	{
		return (SpineAnimSequencePlayer)(pageIndex switch
		{
			0 => SpineHand1, 
			1 => SpineHand2, 
			2 => SpineHand3, 
			_ => null, 
		});
	}

	public override void Dispose()
	{
		if ((Object)(object)swipeHandler != (Object)null)
		{
			swipeHandler.OnSwipeComplete = null;
			swipeHandler.OnDragStarted = null;
		}
		OnSwipePageChanged = null;
		Tweener obj = moveTweener;
		if (obj != null)
		{
			TweenExtensions.Kill((Tween)(object)obj, false);
		}
		moveTweener = null;
		StopAllPageContentAnimations();
		lastAnimPlayedPageIndex = -1;
		lastPlayedSpineHandPageIndex = -1;
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		BtnClose = behaviour.Get<Button>("BtnClose");
		BtnNextFirst = behaviour.Get<Button>("BtnNextFirst");
		BtnNextMid = behaviour.Get<Button>("BtnNextMid");
		BtnCompletePage = behaviour.Get<Button>("BtnCompletePage");
		TxtContent = behaviour.Get<TMP_Text>("TxtContent");
		anim = behaviour.Get<Animation>("anim");
		Content1Anim = behaviour.Get<Animation>("Content1");
		Content2Anim = behaviour.Get<Animation>("Content2");
		Content3Anim = behaviour.Get<Animation>("Content3");
		SpineHand1 = behaviour.Get<SpineAnimSequencePlayer>("SpineHand1");
		SpineHand2 = behaviour.Get<SpineAnimSequencePlayer>("SpineHand2");
		SpineHand3 = behaviour.Get<SpineAnimSequencePlayer>("SpineHand3");
		if (!playContentLoopAnim)
		{
			SpineHand1.Stop();
			SpineHand2.Stop();
			SpineHand3.Stop();
		}
		Toggle_Dot1 = behaviour.Get<Toggle>("Toggle_Dot1");
		Toggle_Dot2 = behaviour.Get<Toggle>("Toggle_Dot2");
		Toggle_Dot3 = behaviour.Get<Toggle>("Toggle_Dot3");
		ShowContent = behaviour.Get<RectTransform>("ShowContent");
		BtnPreviousPage = behaviour.Get<Button>("BtnPreviousPage");
	}
}
