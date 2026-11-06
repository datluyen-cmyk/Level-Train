using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RotateRings.UI.Shop;

internal sealed class ShopCarouselDragRelay : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler
{
	public event Action<PointerEventData> BeginDrag;

	public event Action<PointerEventData> EndDrag;

	public void OnBeginDrag(PointerEventData eventData)
	{
		this.BeginDrag?.Invoke(eventData);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		this.EndDrag?.Invoke(eventData);
	}
}
