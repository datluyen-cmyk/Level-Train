using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XingYun.Core.App;

namespace RotateRings.UI.Shop;

public sealed class ShopCarousel
{
	private const float AutoScrollInterval = 15f;

	private const float ScrollTweenDuration = 0.5f;

	private const float DragSnapDuration = 0.25f;

	private ScrollRect scrollRect;

	private ScrollRect outerScrollRect;

	private RectTransform content;

	private ShopCarouselDragRelay dragRelay;

	private int pageCount;

	private int currentPage = 1;

	private bool increaseIndex = true;

	private bool isDragging;

	private bool isPaused;

	private bool appEventsBound;

	private int autoScrollTimerId = -1;

	private float pageUnitDistance;

	private float page1AnchorX;

	public void Setup(ScrollRect firstAreaScrollView, ScrollRect showScrollView)
	{
		scrollRect = firstAreaScrollView;
		outerScrollRect = showScrollView;
		content = (((Object)(object)scrollRect != (Object)null) ? scrollRect.content : null);
		BindDragRelay();
		BindAppStateEventsIfNeeded();
	}

	public void Build(int count)
	{
		pageCount = Mathf.Max(0, count);
		currentPage = 1;
		increaseIndex = true;
		isDragging = false;
		StopAutoScrollTimer();
		KillTween();
		if ((Object)(object)scrollRect != (Object)null)
		{
			scrollRect.StopMovement();
			((Behaviour)scrollRect).enabled = pageCount > 1;
			scrollRect.horizontal = pageCount > 1;
			scrollRect.vertical = false;
		}
	}

	public void SyncLayout()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)scrollRect == (Object)null)
		{
			return;
		}
		content = scrollRect.content;
		if (!((Object)(object)content == (Object)null))
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(content);
			RectTransform viewport = scrollRect.viewport;
			float num;
			if (!((Object)(object)viewport != (Object)null))
			{
				num = 1016f;
			}
			else
			{
				Rect rect = viewport.rect;
				num = ((Rect)(ref rect)).width;
			}
			float num2 = num;
			float num3 = ResolvePageWidth();
			pageUnitDistance = Mathf.Max(1f, Mathf.Max(num2, num3));
			page1AnchorX = 0f;
			currentPage = Mathf.Clamp(currentPage, 1, Mathf.Max(1, pageCount));
			SetContentX(GetAnchorX(currentPage));
			scrollRect.StopMovement();
			((Behaviour)scrollRect).enabled = pageCount > 1;
			scrollRect.horizontal = pageCount > 1;
			scrollRect.vertical = false;
			RestartAutoScrollTimer();
		}
	}

	public void PauseForExternalOverlay()
	{
		isPaused = true;
		StopAutoScrollTimer();
		KillTween();
		SnapToNearestPage(immediate: true);
	}

	public void ResumeAfterExternalOverlay()
	{
		isPaused = false;
		RestartAutoScrollTimer();
	}

	public void Clear()
	{
		StopAutoScrollTimer();
		KillTween();
		isDragging = false;
		isPaused = false;
		pageCount = 0;
		currentPage = 1;
		increaseIndex = true;
		pageUnitDistance = 0f;
		page1AnchorX = 0f;
		if ((Object)(object)scrollRect != (Object)null)
		{
			scrollRect.StopMovement();
		}
		if ((Object)(object)outerScrollRect != (Object)null)
		{
			((Behaviour)outerScrollRect).enabled = true;
		}
	}

	public void Dispose()
	{
		Clear();
		UnbindAppStateEvents();
		if ((Object)(object)dragRelay != (Object)null)
		{
			dragRelay.BeginDrag -= OnBeginDrag;
			dragRelay.EndDrag -= OnEndDrag;
			Object.Destroy((Object)(object)dragRelay);
			dragRelay = null;
		}
	}

	private float ResolvePageWidth()
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)content == (Object)null)
		{
			return 0f;
		}
		HorizontalLayoutGroup component = ((Component)content).GetComponent<HorizontalLayoutGroup>();
		float num = (((Object)(object)component != (Object)null) ? ((HorizontalOrVerticalLayoutGroup)component).spacing : 0f);
		Rect rect;
		for (int i = 0; i < ((Transform)content).childCount; i++)
		{
			Transform child = ((Transform)content).GetChild(i);
			if (((Component)child).gameObject.activeSelf)
			{
				RectTransform val = (RectTransform)(object)((child is RectTransform) ? child : null);
				float num2;
				if (!((Object)(object)val != (Object)null))
				{
					num2 = 0f;
				}
				else
				{
					rect = val.rect;
					num2 = ((Rect)(ref rect)).width;
				}
				float num3 = num2;
				if (num3 <= 0f)
				{
					rect = content.rect;
					num3 = ((Rect)(ref rect)).width;
				}
				return num3 + num;
			}
		}
		rect = content.rect;
		return ((Rect)(ref rect)).width;
	}

	private void RestartAutoScrollTimer()
	{
		StopAutoScrollTimer();
		if (!isPaused && !isDragging && pageCount > 1)
		{
			autoScrollTimerId = GameApp.Timer.Repeat(15f, (Action)AutoScrollTick);
		}
	}

	private void StopAutoScrollTimer()
	{
		if (autoScrollTimerId != -1)
		{
			GameApp.Timer.Kill(autoScrollTimerId);
			autoScrollTimerId = -1;
		}
	}

	private void AutoScrollTick()
	{
		if (isPaused || isDragging || pageCount <= 1)
		{
			return;
		}
		int num = currentPage;
		if (increaseIndex)
		{
			num++;
			if (num > pageCount)
			{
				increaseIndex = false;
				num = Mathf.Max(1, currentPage - 1);
			}
		}
		else
		{
			num--;
			if (num < 1)
			{
				increaseIndex = true;
				num = Mathf.Min(pageCount, currentPage + 1);
			}
		}
		if (num != currentPage)
		{
			currentPage = num;
			ScrollToPage(currentPage);
		}
	}

	private void ScrollToPage(int page)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		KillTween();
		float anchorX = GetAnchorX(page);
		TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(content, anchorX, 0.5f, false), (Ease)9), ((Component)scrollRect).gameObject, (LinkBehaviour)5), new TweenCallback(RestartAutoScrollTimer));
	}

	private void SnapToNearestPage(bool immediate)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		if (!((Object)(object)content == (Object)null) && pageCount > 1 && !(pageUnitDistance <= 0f))
		{
			float num = page1AnchorX - content.anchoredPosition.x;
			currentPage = Mathf.Clamp(Mathf.RoundToInt(num / pageUnitDistance) + 1, 1, pageCount);
			float anchorX = GetAnchorX(currentPage);
			KillTween();
			if (immediate)
			{
				SetContentX(anchorX);
			}
			else
			{
				TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(content, anchorX, 0.25f, false), (Ease)9), ((Component)scrollRect).gameObject, (LinkBehaviour)5), new TweenCallback(RestartAutoScrollTimer));
			}
		}
	}

	private float GetAnchorX(int page)
	{
		return page1AnchorX - (float)(Mathf.Max(1, page) - 1) * pageUnitDistance;
	}

	private void SetContentX(float x)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 anchoredPosition = content.anchoredPosition;
		anchoredPosition.x = x;
		content.anchoredPosition = anchoredPosition;
	}

	private void KillTween()
	{
		RectTransform obj = content;
		if (obj != null)
		{
			ShortcutExtensions.DOKill((Component)(object)obj, false);
		}
	}

	private void BindDragRelay()
	{
		if (!((Object)(object)scrollRect == (Object)null) && !((Object)(object)dragRelay != (Object)null))
		{
			dragRelay = ((Component)scrollRect).GetComponent<ShopCarouselDragRelay>() ?? ((Component)scrollRect).gameObject.AddComponent<ShopCarouselDragRelay>();
			dragRelay.BeginDrag -= OnBeginDrag;
			dragRelay.EndDrag -= OnEndDrag;
			dragRelay.BeginDrag += OnBeginDrag;
			dragRelay.EndDrag += OnEndDrag;
		}
	}

	private void OnBeginDrag(PointerEventData eventData)
	{
		if (!((Object)(object)scrollRect == (Object)null) && !((Object)(object)content == (Object)null) && pageCount > 1)
		{
			isDragging = true;
			StopAutoScrollTimer();
			KillTween();
			scrollRect.StopMovement();
			if ((Object)(object)outerScrollRect != (Object)null)
			{
				((Behaviour)outerScrollRect).enabled = false;
			}
		}
	}

	private void OnEndDrag(PointerEventData eventData)
	{
		if (!((Object)(object)scrollRect == (Object)null) && !((Object)(object)content == (Object)null))
		{
			isDragging = false;
			scrollRect.StopMovement();
			if ((Object)(object)outerScrollRect != (Object)null)
			{
				((Behaviour)outerScrollRect).enabled = true;
			}
			SnapToNearestPage(immediate: false);
		}
	}

	private void BindAppStateEventsIfNeeded()
	{
		if (!appEventsBound)
		{
			AppState.OnEnterBackground += OnAppEnterBackground;
			AppState.OnEnterForeground += OnAppEnterForeground;
			AppState.OnLoseFocus += OnAppLoseFocus;
			AppState.OnGainFocus += OnAppGainFocus;
			appEventsBound = true;
		}
	}

	private void UnbindAppStateEvents()
	{
		if (appEventsBound)
		{
			AppState.OnEnterBackground -= OnAppEnterBackground;
			AppState.OnEnterForeground -= OnAppEnterForeground;
			AppState.OnLoseFocus -= OnAppLoseFocus;
			AppState.OnGainFocus -= OnAppGainFocus;
			appEventsBound = false;
		}
	}

	private void OnAppEnterBackground()
	{
		PauseForExternalOverlay();
	}

	private void OnAppEnterForeground()
	{
		ResumeAfterExternalOverlay();
	}

	private void OnAppLoseFocus()
	{
		PauseForExternalOverlay();
	}

	private void OnAppGainFocus()
	{
		ResumeAfterExternalOverlay();
	}
}
