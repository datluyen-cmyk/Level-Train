using System;
using Rotaterings.SO;
using SuperScrollView;
using UnityEngine;

namespace Rotaterings.UI.RankList;

public sealed class RankMainListSelfSticky
{
	private const string RankItemPrefabName = "RankItem";

	private readonly LoopListView2 list;

	private readonly Func<int> getSelfListIndex;

	private readonly Func<RankItemData> getSelfEntry;

	private readonly Func<int> getCollectIconSide;

	private readonly Vector3[] viewportCorners = (Vector3[])(object)new Vector3[4];

	private LoopListViewItem2 overlayListItem;

	private RankItem overlayRankItem;

	private float overlayHeight;

	private RankItemData lastRenderedEntry;

	private float SelfStickyBottomOffset
	{
		get
		{
			RankListConfig instance = RankListConfig.Instance;
			if (!((Object)(object)instance != (Object)null))
			{
				return 25f;
			}
			return instance.mainSelfStickyBottomOffset;
		}
	}

	public RankMainListSelfSticky(LoopListView2 list, Func<int> getSelfListIndex, Func<RankItemData> getSelfEntry, Func<int> getCollectIconSide)
	{
		this.list = list;
		this.getSelfListIndex = getSelfListIndex;
		this.getSelfEntry = getSelfEntry;
		this.getCollectIconSide = getCollectIconSide;
	}

	public void Setup()
	{
		LoopListView2 obj = list;
		if (!((Object)(object)((obj != null) ? obj.ViewPortTrans : null) == (Object)null))
		{
			EnsureOverlayItem();
		}
	}

	public void Teardown()
	{
		HideOverlay();
		lastRenderedEntry = null;
		if (!((Object)(object)overlayListItem == (Object)null))
		{
			Object.Destroy((Object)(object)((Component)overlayListItem).gameObject);
			overlayListItem = null;
			overlayRankItem = null;
		}
	}

	public void Update()
	{
		if ((Object)(object)list == (Object)null)
		{
			return;
		}
		RankItemData rankItemData = getSelfEntry?.Invoke();
		if (rankItemData == null || (rankItemData.RankIndex > 0 && rankItemData.RankIndex <= 3))
		{
			HideOverlay();
			return;
		}
		int num = getSelfListIndex?.Invoke() ?? (-1);
		int firstIndex;
		int lastIndex;
		if (num < 0)
		{
			HideOverlay();
		}
		else if (!TryGetVisibleItemIndexRange(out firstIndex, out lastIndex))
		{
			HideOverlay();
		}
		else if (num < firstIndex)
		{
			HideOverlay();
		}
		else if (!ShouldStickSelfToBottom(num, lastIndex))
		{
			HideOverlay();
			RestoreSelfListItemVisible(num);
		}
		else
		{
			ShowOverlay(rankItemData);
			HideSelfListItemVisual(num);
		}
	}

	private void EnsureOverlayItem()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)overlayRankItem != (Object)null))
		{
			overlayListItem = list.NewListViewItem("RankItem");
			if (!((Object)(object)overlayListItem == (Object)null))
			{
				overlayRankItem = ((Component)overlayListItem).GetComponent<RankItem>();
				RectTransform cachedRectTransform = overlayListItem.CachedRectTransform;
				overlayHeight = cachedRectTransform.sizeDelta.y;
				((Transform)cachedRectTransform).SetParent((Transform)(object)list.ViewPortTrans, false);
				cachedRectTransform.anchorMin = new Vector2(0f, 0f);
				cachedRectTransform.anchorMax = new Vector2(1f, 0f);
				cachedRectTransform.pivot = new Vector2(0.5f, 0f);
				ApplyOverlayBottomOffset(cachedRectTransform);
				cachedRectTransform.sizeDelta = new Vector2(0f, overlayHeight);
				((Transform)cachedRectTransform).localScale = Vector3.one;
				((Component)overlayListItem).gameObject.SetActive(false);
			}
		}
	}

	private void ApplyOverlayBottomOffset(RectTransform rtf = null)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (rtf == null)
		{
			LoopListViewItem2 obj = overlayListItem;
			rtf = ((obj != null) ? obj.CachedRectTransform : null);
		}
		if (!((Object)(object)rtf == (Object)null))
		{
			rtf.anchoredPosition = new Vector2(0f, 0f - SelfStickyBottomOffset);
		}
	}

	private float GetStickyTriggerBottomY()
	{
		return GetViewportBottomY() - SelfStickyBottomOffset;
	}

	private bool ShouldStickSelfToBottom(int selfListIndex, int lastVisibleIndex)
	{
		if (selfListIndex > lastVisibleIndex)
		{
			return true;
		}
		LoopListViewItem2 shownItemByItemIndex = list.GetShownItemByItemIndex(selfListIndex);
		if ((Object)(object)shownItemByItemIndex == (Object)null)
		{
			return false;
		}
		return shownItemByItemIndex.BottomY < GetStickyTriggerBottomY();
	}

	private void ShowOverlay(RankItemData selfEntry)
	{
		EnsureOverlayItem();
		if (!((Object)(object)overlayRankItem == (Object)null))
		{
			if (lastRenderedEntry != selfEntry)
			{
				overlayRankItem.SetSettleTopThree(value: false);
				overlayRankItem.SetRankType(ERankContent.Main);
				overlayRankItem.InitializeCollectPosition(topHeader: false, getCollectIconSide?.Invoke() ?? 0);
				overlayRankItem.Render(selfEntry);
				lastRenderedEntry = selfEntry;
			}
			ApplyOverlayBottomOffset();
			if (!((Component)overlayListItem).gameObject.activeSelf)
			{
				((Component)overlayListItem).gameObject.SetActive(true);
			}
			((Transform)overlayListItem.CachedRectTransform).SetAsLastSibling();
		}
	}

	private void HideOverlay()
	{
		if ((Object)(object)overlayListItem != (Object)null)
		{
			((Component)overlayListItem).gameObject.SetActive(false);
		}
	}

	private void HideSelfListItemVisual(int selfListIndex)
	{
		LoopListViewItem2 shownItemByItemIndex = list.GetShownItemByItemIndex(selfListIndex);
		if (shownItemByItemIndex != null)
		{
			((Component)shownItemByItemIndex).GetComponent<RankItem>()?.SetVisible(visible: false);
		}
	}

	private void RestoreSelfListItemVisible(int selfListIndex)
	{
		LoopListViewItem2 shownItemByItemIndex = list.GetShownItemByItemIndex(selfListIndex);
		if (shownItemByItemIndex != null)
		{
			((Component)shownItemByItemIndex).GetComponent<RankItem>()?.SetVisible(visible: true);
		}
	}

	private float GetViewportBottomY()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		list.ViewPortTrans.GetWorldCorners(viewportCorners);
		return ((Transform)list.ContainerTrans).InverseTransformPoint(viewportCorners[0]).y;
	}

	private bool TryGetVisibleItemIndexRange(out int firstIndex, out int lastIndex)
	{
		firstIndex = int.MaxValue;
		lastIndex = int.MinValue;
		int shownItemCount = list.ShownItemCount;
		if (shownItemCount == 0)
		{
			return false;
		}
		for (int i = 0; i < shownItemCount; i++)
		{
			LoopListViewItem2 shownItemByIndex = list.GetShownItemByIndex(i);
			if (!((Object)(object)shownItemByIndex == (Object)null))
			{
				if (shownItemByIndex.ItemIndex < firstIndex)
				{
					firstIndex = shownItemByIndex.ItemIndex;
				}
				if (shownItemByIndex.ItemIndex > lastIndex)
				{
					lastIndex = shownItemByIndex.ItemIndex;
				}
			}
		}
		return firstIndex != int.MaxValue;
	}
}
