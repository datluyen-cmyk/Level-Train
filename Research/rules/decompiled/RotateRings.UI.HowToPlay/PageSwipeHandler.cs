using System;
using UniLog;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RotateRings.UI.HowToPlay;

public class PageSwipeHandler : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	private const float SwipeThresholdRatio = 0.15f;

	private const float EdgeDamping = 0.2f;

	private RectTransform content;

	private RectTransform parentRect;

	private Camera eventCamera;

	private int totalPages;

	private float pageWidth;

	private int currentPage;

	private Vector2 dragStartLocalPos;

	private Vector2 contentStartPos;

	private bool isDragging;

	public Action<int> OnSwipeComplete;

	public Action OnDragStarted;

	public void Setup(RectTransform content, int totalPages, int currentPage, Camera eventCamera)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		this.content = content;
		ref RectTransform reference = ref parentRect;
		Transform parent = ((Transform)content).parent;
		reference = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		if ((Object)(object)parentRect == (Object)null)
		{
			Log.Error((object)"[PageSwipeHandler] content 的父节点不是 RectTransform，滑动翻页将无法工作", Array.Empty<object>());
		}
		this.totalPages = totalPages;
		this.currentPage = currentPage;
		pageWidth = content.sizeDelta.x;
		this.eventCamera = eventCamera;
	}

	public void SetCurrentPage(int page)
	{
		currentPage = page;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)parentRect == (Object)null || Mathf.Abs(eventData.delta.x) < Mathf.Abs(eventData.delta.y))
		{
			isDragging = false;
			return;
		}
		isDragging = true;
		OnDragStarted?.Invoke();
		RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventCamera, ref dragStartLocalPos);
		contentStartPos = content.anchoredPosition;
	}

	public void OnDrag(PointerEventData eventData)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = default(Vector2);
		if (isDragging && !((Object)(object)content == (Object)null) && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventCamera, ref val))
		{
			float num = val.x - dragStartLocalPos.x;
			float num2 = contentStartPos.x + num;
			float num3 = (float)(-(totalPages - 1)) * pageWidth;
			if (num2 > 0f)
			{
				num2 *= 0.2f;
			}
			else if (num2 < num3)
			{
				num2 = num3 + (num2 - num3) * 0.2f;
			}
			content.anchoredPosition = new Vector2(num2, contentStartPos.y);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (!isDragging)
		{
			return;
		}
		isDragging = false;
		Vector2 val = default(Vector2);
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventCamera, ref val))
		{
			OnSwipeComplete?.Invoke(0);
			return;
		}
		float num = val.x - dragStartLocalPos.x;
		float num2 = pageWidth * 0.15f;
		int obj = 0;
		if (num < 0f - num2 && currentPage < totalPages - 1)
		{
			obj = 1;
		}
		else if (num > num2 && currentPage > 0)
		{
			obj = -1;
		}
		OnSwipeComplete?.Invoke(obj);
	}
}
