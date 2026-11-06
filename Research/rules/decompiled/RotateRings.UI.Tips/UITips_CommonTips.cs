using System;
using System.Collections;
using System.Collections.Generic;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.Tips;

public class UITips_CommonTips : GameWidgetView
{
	public enum TipsPlacement
	{
		Above,
		Below,
		Left,
		Right
	}

	private const float ArrowGap = 12f;

	private const float ScreenPadding = 20f;

	private const float ArrowEdgePadding = 45f;

	private RectTransform rectTransform;

	private RectTransform bgRect;

	private RectTransform contentRect;

	private CanvasGroup canvasGroup;

	private Coroutine refreshPositionCoroutine;

	private Coroutine outsideClickCoroutine;

	private UITipsOpenParam currentParam;

	private Action onRequestClose;

	private Action onPositionReady;

	private int lastOpenClickFrame = -1;

	private TipsPlacement currentPlacement;

	private bool isInitialized;

	public GameObject ArrowBottom;

	public GameObject ArrowTop;

	public GameObject ArrowLeft;

	public GameObject ArrowRight;

	public GameObject Content;

	public GameObject ItemCell;

	public GameObject HorLayoutContenCell;

	public GameObject HorLayoutContenCell_5;

	public GameObject HorLayoutContenDetail;

	public TMP_Text TxtDetailCell;

	public TMP_Text TxtTitleCell;

	public GameObject gameObject
	{
		get
		{
			if (!((Object)(object)((UIBaseView)this).Behaviour != (Object)null))
			{
				return null;
			}
			return ((Component)((UIBaseView)this).Behaviour).gameObject;
		}
	}

	public Transform transform
	{
		get
		{
			if (!((Object)(object)((UIBaseView)this).NodeRect != (Object)null))
			{
				if (!((Object)(object)((UIBaseView)this).Behaviour != (Object)null))
				{
					return null;
				}
				return ((Component)((UIBaseView)this).Behaviour).transform;
			}
			return (Transform)(object)((UIBaseView)this).NodeRect;
		}
	}

	private void EnsureInitialized()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			rectTransform = ((UIBaseView)this).NodeRect;
			contentRect = (RectTransform)(((Object)(object)Content != (Object)null) ? /*isinst with value type is only supported in some contexts*/: null);
			canvasGroup = ((Component)((UIBaseView)this).Behaviour).GetComponent<CanvasGroup>();
			if ((Object)(object)canvasGroup == (Object)null)
			{
				canvasGroup = ((Component)((UIBaseView)this).Behaviour).gameObject.AddComponent<CanvasGroup>();
			}
			HideAllArrows();
			ResetContent();
		}
	}

	public void Show(UITipsOpenParam param, Action onRequestClose, Action onPositionReady = null)
	{
		if (param != null && !((Object)(object)param.Target == (Object)null) && !((Object)(object)((UIBaseView)this).Behaviour == (Object)null))
		{
			EnsureInitialized();
			currentParam = param;
			this.onRequestClose = onRequestClose;
			this.onPositionReady = onPositionReady;
			lastOpenClickFrame = Time.frameCount;
			ResetContent();
			BuildContent(param);
			SetTipsVisible(visible: false);
			gameObject.SetActive(true);
			StopRefreshPositionCoroutine();
			StopOutsideClickCoroutine();
			refreshPositionCoroutine = ((MonoBehaviour)((UIBaseView)this).Behaviour).StartCoroutine(RefreshPositionEndOfFrame(param.Target));
			outsideClickCoroutine = ((MonoBehaviour)((UIBaseView)this).Behaviour).StartCoroutine(OutsideClickCheckRoutine());
		}
	}

	public void Hide()
	{
		StopRefreshPositionCoroutine();
		StopOutsideClickCoroutine();
		SetTipsVisible(visible: true);
		ResetContent();
		HideAllArrows();
		currentParam = null;
		onRequestClose = null;
		onPositionReady = null;
		if ((Object)(object)gameObject != (Object)null)
		{
			gameObject.SetActive(false);
		}
	}

	public override void Dispose()
	{
		Hide();
		DetachAllWidgets();
		base.Dispose();
	}

	private void ResetContent()
	{
		DetachAllWidgets();
		ClearHorLayoutDynamicChildren(HorLayoutContenCell);
		ClearHorLayoutDynamicChildren(HorLayoutContenCell_5);
		if ((Object)(object)ItemCell != (Object)null)
		{
			ItemCell.SetActive(false);
		}
		if ((Object)(object)HorLayoutContenCell != (Object)null)
		{
			HorLayoutContenCell.SetActive(false);
		}
		if ((Object)(object)HorLayoutContenCell_5 != (Object)null)
		{
			HorLayoutContenCell_5.SetActive(false);
		}
		if ((Object)(object)HorLayoutContenDetail != (Object)null)
		{
			HorLayoutContenDetail.SetActive(false);
		}
		if ((Object)(object)TxtDetailCell != (Object)null)
		{
			((Component)TxtDetailCell).gameObject.SetActive(false);
		}
		if ((Object)(object)TxtTitleCell != (Object)null)
		{
			((Component)TxtTitleCell).gameObject.SetActive(false);
		}
	}

	private void ClearHorLayoutDynamicChildren(GameObject horLayout)
	{
		if ((Object)(object)horLayout == (Object)null)
		{
			return;
		}
		Transform val = horLayout.transform;
		for (int num = val.childCount - 1; num >= 0; num--)
		{
			Transform child = val.GetChild(num);
			if (!IsTemplateHorLayoutChild(child))
			{
				Object.Destroy((Object)(object)((Component)child).gameObject);
			}
		}
	}

	private bool IsTemplateHorLayoutChild(Transform child)
	{
		if ((Object)(object)ItemCell != (Object)null && (Object)(object)((Component)child).gameObject == (Object)(object)ItemCell)
		{
			return true;
		}
		if ((Object)(object)TxtDetailCell != (Object)null && (Object)(object)((Component)child).gameObject == (Object)(object)((Component)TxtDetailCell).gameObject)
		{
			return true;
		}
		return false;
	}

	private void BuildContent(UITipsOpenParam param)
	{
		if ((Object)(object)Content == (Object)null)
		{
			return;
		}
		int num = CountValidRewards(param.Rewards);
		bool num2 = num > 0 && (Object)(object)ItemCell != (Object)null;
		bool flag = !string.IsNullOrEmpty(param.DetailText) && (Object)(object)TxtDetailCell != (Object)null;
		bool flag2 = flag && (Object)(object)HorLayoutContenCell != (Object)null && TxtDetailCell.transform.IsChildOf(HorLayoutContenCell.transform);
		bool flag3 = flag && (Object)(object)HorLayoutContenDetail != (Object)null && TxtDetailCell.transform.IsChildOf(HorLayoutContenDetail.transform);
		if (!string.IsNullOrEmpty(param.Title) && (Object)(object)TxtTitleCell != (Object)null)
		{
			((Component)TxtTitleCell).gameObject.SetActive(true);
			TMPTextExtensions.SetLocalizedTextValue(TxtTitleCell, param.Title);
		}
		if (flag && !flag2)
		{
			if (flag3)
			{
				HorLayoutContenDetail.SetActive(true);
			}
			ShowDetailText(param.DetailText);
		}
		if (num2)
		{
			BuildRewardContent(param.Rewards, num, flag && flag2, param.DetailText);
		}
		else if (flag && flag2)
		{
			ActivateHorLayout(HorLayoutContenCell, HorLayoutContenCell_5);
			ShowDetailText(param.DetailText);
			if ((Object)(object)ItemCell != (Object)null && (Object)(object)HorLayoutContenCell != (Object)null && ItemCell.transform.IsChildOf(HorLayoutContenCell.transform))
			{
				ItemCell.SetActive(false);
			}
		}
	}

	private static int CountValidRewards(List<ShowRewardItem> rewards)
	{
		if (rewards == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < rewards.Count; i++)
		{
			if (rewards[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	private bool ShouldUseCompactHorLayout(int validRewardCount)
	{
		if (validRewardCount == 5 || validRewardCount == 6)
		{
			return (Object)(object)HorLayoutContenCell_5 != (Object)null;
		}
		return false;
	}

	private void BuildRewardContent(List<ShowRewardItem> rewards, int validRewardCount, bool showDetailInHorLayout, string detailText)
	{
		bool flag = ShouldUseCompactHorLayout(validRewardCount);
		GameObject val = (flag ? HorLayoutContenCell_5 : HorLayoutContenCell);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		ActivateHorLayout(val, flag ? HorLayoutContenCell : HorLayoutContenCell_5);
		if (showDetailInHorLayout)
		{
			ShowDetailText(detailText);
		}
		Transform parent = val.transform;
		for (int i = 0; i < rewards.Count; i++)
		{
			ShowRewardItem showRewardItem = rewards[i];
			if (showRewardItem == null)
			{
				continue;
			}
			WidgetHandle widgetHandle = AttachWidget<UITips_CommonTips_ItemCell>(ItemCell, parent);
			if (widgetHandle != null)
			{
				UITips_CommonTips_ItemCell view = widgetHandle.GetView<UITips_CommonTips_ItemCell>();
				if (view == null)
				{
					DetachWidget(widgetHandle);
					continue;
				}
				view.gameObject.SetActive(true);
				view.SetData((int)showRewardItem.PropType, showRewardItem.Count);
			}
		}
		if ((Object)(object)ItemCell != (Object)null)
		{
			ItemCell.SetActive(false);
		}
	}

	private void ShowDetailText(string text)
	{
		if (!((Object)(object)TxtDetailCell == (Object)null))
		{
			((Component)TxtDetailCell).gameObject.SetActive(true);
			TMPTextExtensions.SetLocalizedTextValue(TxtDetailCell, text);
		}
	}

	private void ActivateHorLayout(GameObject activeHorLayout, GameObject inactiveHorLayout)
	{
		if ((Object)(object)inactiveHorLayout != (Object)null && (Object)(object)inactiveHorLayout != (Object)(object)activeHorLayout)
		{
			inactiveHorLayout.SetActive(false);
		}
		if ((Object)(object)activeHorLayout != (Object)null)
		{
			activeHorLayout.SetActive(true);
		}
	}

	public void SetTipsVisible(bool visible)
	{
		if (!((Object)(object)canvasGroup == (Object)null))
		{
			canvasGroup.alpha = (visible ? 1f : 0f);
			canvasGroup.blocksRaycasts = visible;
		}
	}

	private void RebuildLayout()
	{
		ForceRebuildActiveHorLayout(HorLayoutContenCell);
		ForceRebuildActiveHorLayout(HorLayoutContenCell_5);
		ForceRebuildActiveHorLayout(HorLayoutContenDetail);
		if ((Object)(object)TxtDetailCell != (Object)null && ((Component)TxtDetailCell).gameObject.activeInHierarchy)
		{
			TxtDetailCell.ForceMeshUpdate(false, false);
		}
		if ((Object)(object)TxtTitleCell != (Object)null && ((Component)TxtTitleCell).gameObject.activeInHierarchy)
		{
			TxtTitleCell.ForceMeshUpdate(false, false);
		}
		Canvas.ForceUpdateCanvases();
		if ((Object)(object)contentRect != (Object)null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
		}
		if ((Object)(object)rectTransform != (Object)null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		}
		Canvas.ForceUpdateCanvases();
	}

	private static void ForceRebuildActiveHorLayout(GameObject horLayout)
	{
		if (!((Object)(object)horLayout == (Object)null) && horLayout.activeInHierarchy)
		{
			Transform obj = horLayout.transform;
			RectTransform val = (RectTransform)(object)((obj is RectTransform) ? obj : null);
			if ((Object)(object)val != (Object)null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(val);
			}
		}
	}

	private void UpdatePosition(RectTransform target)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rectTransform == (Object)null || (Object)(object)target == (Object)null)
		{
			return;
		}
		Transform parent = ((Transform)rectTransform).parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		if (!((Object)(object)val == (Object)null))
		{
			Camera eventCamera = GetEventCamera();
			Camera canvasEventCamera = GetCanvasEventCamera(target);
			Rect safeScreenRect = GetSafeScreenRect();
			Vector2 rectCenterScreen = GetRectCenterScreen(target, canvasEventCamera);
			Rect rectScreenRect = GetRectScreenRect(target, canvasEventCamera);
			Vector2 panelScreenSize = GetPanelScreenSize();
			TipsPlacement placement = (currentPlacement = ((currentParam != null && currentParam.PreferredPlacement.HasValue) ? currentParam.PreferredPlacement.Value : SelectPlacement(rectCenterScreen, panelScreenSize, safeScreenRect, rectScreenRect)));
			Vector2 panelCenterScreen = GetPanelCenterScreen(placement, rectScreenRect, panelScreenSize);
			panelCenterScreen = ClampPanelCenter(panelCenterScreen, panelScreenSize, safeScreenRect);
			Vector2 pointInParentLocal = default(Vector2);
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(val, panelCenterScreen, eventCamera, ref pointInParentLocal))
			{
				SetAnchoredPositionLocal(val, rectTransform, pointInParentLocal);
			}
			ApplyArrow(placement, target);
		}
	}

	private IEnumerator RefreshPositionEndOfFrame(RectTransform target)
	{
		yield return null;
		if (!((Object)(object)target == (Object)null) && !((Object)(object)gameObject == (Object)null) && gameObject.activeInHierarchy)
		{
			RebuildLayout();
			UpdatePosition(target);
			onPositionReady?.Invoke();
			onPositionReady = null;
			refreshPositionCoroutine = null;
		}
	}

	private IEnumerator OutsideClickCheckRoutine()
	{
		while ((Object)(object)gameObject != (Object)null && gameObject.activeInHierarchy)
		{
			TryCloseOnOutsidePointerDown();
			yield return null;
		}
		outsideClickCoroutine = null;
	}

	private void StopRefreshPositionCoroutine()
	{
		if (refreshPositionCoroutine == null || (Object)(object)((UIBaseView)this).Behaviour == (Object)null)
		{
			refreshPositionCoroutine = null;
			return;
		}
		((MonoBehaviour)((UIBaseView)this).Behaviour).StopCoroutine(refreshPositionCoroutine);
		refreshPositionCoroutine = null;
	}

	private void StopOutsideClickCoroutine()
	{
		if (outsideClickCoroutine == null || (Object)(object)((UIBaseView)this).Behaviour == (Object)null)
		{
			outsideClickCoroutine = null;
			return;
		}
		((MonoBehaviour)((UIBaseView)this).Behaviour).StopCoroutine(outsideClickCoroutine);
		outsideClickCoroutine = null;
	}

	private static TipsPlacement SelectPlacement(Vector2 targetCenterScreen, Vector2 panelSize, Rect safeScreenRect, Rect targetScreenRect)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		TipsPlacement[] preferredPlacementOrder = GetPreferredPlacementOrder(targetCenterScreen, safeScreenRect);
		float num = panelSize.x * panelSize.y;
		TipsPlacement result = preferredPlacementOrder[0];
		bool flag = false;
		TipsPlacement result2 = preferredPlacementOrder[0];
		float num2 = float.MinValue;
		foreach (TipsPlacement tipsPlacement in preferredPlacementOrder)
		{
			float num3 = EvaluatePlacementScore(tipsPlacement, targetScreenRect, panelSize, safeScreenRect);
			if (num3 >= num - 1f)
			{
				return tipsPlacement;
			}
			if (num3 > 0f && !flag)
			{
				flag = true;
				result = tipsPlacement;
			}
			if (num3 > num2)
			{
				num2 = num3;
				result2 = tipsPlacement;
			}
		}
		if (!flag)
		{
			return result2;
		}
		return result;
	}

	private static float EvaluatePlacementScore(TipsPlacement placement, Rect targetScreenRect, Vector2 panelSize, Rect safeScreenRect)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return ScorePlacement(ClampPanelCenter(GetPanelCenterScreen(placement, targetScreenRect, panelSize), panelSize, safeScreenRect), panelSize, safeScreenRect);
	}

	private static TipsPlacement[] GetPreferredPlacementOrder(Vector2 targetCenterScreen, Rect safeScreenRect)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Rect)(ref safeScreenRect)).yMin + ((Rect)(ref safeScreenRect)).height * 0.5f;
		float num2 = ((Rect)(ref safeScreenRect)).xMin + ((Rect)(ref safeScreenRect)).width * 0.5f;
		if (targetCenterScreen.y < num)
		{
			if (targetCenterScreen.x < num2)
			{
				return new TipsPlacement[4]
				{
					TipsPlacement.Above,
					TipsPlacement.Below,
					TipsPlacement.Right,
					TipsPlacement.Left
				};
			}
			return new TipsPlacement[4]
			{
				TipsPlacement.Above,
				TipsPlacement.Below,
				TipsPlacement.Left,
				TipsPlacement.Right
			};
		}
		if (targetCenterScreen.x < num2)
		{
			return new TipsPlacement[4]
			{
				TipsPlacement.Below,
				TipsPlacement.Above,
				TipsPlacement.Right,
				TipsPlacement.Left
			};
		}
		return new TipsPlacement[4]
		{
			TipsPlacement.Below,
			TipsPlacement.Above,
			TipsPlacement.Left,
			TipsPlacement.Right
		};
	}

	private static Vector2 GetPanelCenterScreen(TipsPlacement placement, Rect targetScreenRect, Vector2 panelSize)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		return (Vector2)(placement switch
		{
			TipsPlacement.Above => new Vector2(((Rect)(ref targetScreenRect)).center.x, ((Rect)(ref targetScreenRect)).yMax + 12f + panelSize.y * 0.5f), 
			TipsPlacement.Below => new Vector2(((Rect)(ref targetScreenRect)).center.x, ((Rect)(ref targetScreenRect)).yMin - 12f - panelSize.y * 0.5f), 
			TipsPlacement.Left => new Vector2(((Rect)(ref targetScreenRect)).xMin - 12f - panelSize.x * 0.5f, ((Rect)(ref targetScreenRect)).center.y), 
			_ => new Vector2(((Rect)(ref targetScreenRect)).xMax + 12f + panelSize.x * 0.5f, ((Rect)(ref targetScreenRect)).center.y), 
		});
	}

	private static Vector2 ClampPanelCenter(Vector2 center, Vector2 panelSize, Rect safeScreenRect)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		float num = panelSize.x * 0.5f;
		float num2 = panelSize.y * 0.5f;
		return new Vector2(Mathf.Clamp(center.x, ((Rect)(ref safeScreenRect)).xMin + num, ((Rect)(ref safeScreenRect)).xMax - num), Mathf.Clamp(center.y, ((Rect)(ref safeScreenRect)).yMin + num2, ((Rect)(ref safeScreenRect)).yMax - num2));
	}

	private static float ScorePlacement(Vector2 center, Vector2 panelSize, Rect safeScreenRect)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Rect val = default(Rect);
		((Rect)(ref val))._002Ector(center.x - panelSize.x * 0.5f, center.y - panelSize.y * 0.5f, panelSize.x, panelSize.y);
		float num = Mathf.Min(((Rect)(ref val)).xMax, ((Rect)(ref safeScreenRect)).xMax) - Mathf.Max(((Rect)(ref val)).xMin, ((Rect)(ref safeScreenRect)).xMin);
		float num2 = Mathf.Min(((Rect)(ref val)).yMax, ((Rect)(ref safeScreenRect)).yMax) - Mathf.Max(((Rect)(ref val)).yMin, ((Rect)(ref safeScreenRect)).yMin);
		if (num <= 0f || num2 <= 0f)
		{
			return -1f;
		}
		return num * num2;
	}

	private void ApplyArrow(TipsPlacement placement, RectTransform target)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		HideAllArrows();
		RectTransform arrowRect = GetArrowRect(placement);
		if ((Object)(object)arrowRect == (Object)null)
		{
			return;
		}
		((Component)arrowRect).gameObject.SetActive(true);
		RectTransform val = GetBgRect();
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		Camera canvasEventCamera = GetCanvasEventCamera(target);
		Rect rect = target.rect;
		Vector2 val2 = RectTransformUtility.WorldToScreenPoint(canvasEventCamera, ((Transform)target).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center)));
		Camera eventCamera = GetEventCamera();
		Vector2 val3 = default(Vector2);
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(val, val2, eventCamera, ref val3))
		{
			Vector2 anchoredPosition = arrowRect.anchoredPosition;
			if ((uint)placement <= 1u)
			{
				float x = val3.x;
				rect = val.rect;
				float num = (0f - ((Rect)(ref rect)).width) * 0.5f + 45f;
				rect = val.rect;
				anchoredPosition.x = Mathf.Clamp(x, num, ((Rect)(ref rect)).width * 0.5f - 45f);
			}
			else
			{
				float y = val3.y;
				rect = val.rect;
				float num2 = (0f - ((Rect)(ref rect)).height) * 0.5f + 45f;
				rect = val.rect;
				anchoredPosition.y = Mathf.Clamp(y, num2, ((Rect)(ref rect)).height * 0.5f - 45f);
			}
			arrowRect.anchoredPosition = anchoredPosition;
		}
	}

	public bool TryGetActiveArrowRect(out RectTransform arrowRect)
	{
		arrowRect = GetArrowRect(currentPlacement);
		if ((Object)(object)arrowRect != (Object)null)
		{
			return ((Component)arrowRect).gameObject.activeInHierarchy;
		}
		return false;
	}

	public Vector2 GetAniPivot()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		return (Vector2)(currentPlacement switch
		{
			TipsPlacement.Above => new Vector2(0.5f, 0f), 
			TipsPlacement.Below => new Vector2(0.5f, 1f), 
			TipsPlacement.Left => new Vector2(1f, 0.5f), 
			_ => new Vector2(0f, 0.5f), 
		});
	}

	private RectTransform GetArrowRect(TipsPlacement placement)
	{
		switch (placement)
		{
		case TipsPlacement.Above:
		{
			if (!((Object)(object)ArrowBottom != (Object)null))
			{
				return null;
			}
			Transform obj3 = ArrowBottom.transform;
			return (RectTransform)(object)((obj3 is RectTransform) ? obj3 : null);
		}
		case TipsPlacement.Below:
		{
			if (!((Object)(object)ArrowTop != (Object)null))
			{
				return null;
			}
			Transform obj2 = ArrowTop.transform;
			return (RectTransform)(object)((obj2 is RectTransform) ? obj2 : null);
		}
		case TipsPlacement.Left:
		{
			if (!((Object)(object)ArrowRight != (Object)null))
			{
				return null;
			}
			Transform obj4 = ArrowRight.transform;
			return (RectTransform)(object)((obj4 is RectTransform) ? obj4 : null);
		}
		default:
		{
			if (!((Object)(object)ArrowLeft != (Object)null))
			{
				return null;
			}
			Transform obj = ArrowLeft.transform;
			return (RectTransform)(object)((obj is RectTransform) ? obj : null);
		}
		}
	}

	private void HideAllArrows()
	{
		if ((Object)(object)ArrowBottom != (Object)null)
		{
			ArrowBottom.SetActive(false);
		}
		if ((Object)(object)ArrowTop != (Object)null)
		{
			ArrowTop.SetActive(false);
		}
		if ((Object)(object)ArrowLeft != (Object)null)
		{
			ArrowLeft.SetActive(false);
		}
		if ((Object)(object)ArrowRight != (Object)null)
		{
			ArrowRight.SetActive(false);
		}
	}

	private RectTransform GetBgRect()
	{
		if ((Object)(object)bgRect != (Object)null)
		{
			return bgRect;
		}
		if ((Object)(object)ArrowBottom != (Object)null)
		{
			ref RectTransform reference = ref bgRect;
			Transform parent = ArrowBottom.transform.parent;
			reference = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		}
		return bgRect;
	}

	private Vector2 GetPanelScreenSize()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rectTransform == (Object)null)
		{
			return Vector2.zero;
		}
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Camera eventCamera = GetEventCamera();
		Vector2 val = RectTransformUtility.WorldToScreenPoint(eventCamera, array[0]);
		Vector2 val2 = RectTransformUtility.WorldToScreenPoint(eventCamera, array[2]);
		return new Vector2(Mathf.Abs(val2.x - val.x), Mathf.Abs(val2.y - val.y));
	}

	private static Rect GetSafeScreenRect()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 0f;
		if ((Object)(object)UIRoot.Instance?.UISafeArea != (Object)null)
		{
			num = UIRoot.Instance.UISafeArea.SafeAreaTop;
			num2 = UIRoot.Instance.UISafeArea.SafeAreaBottom;
		}
		return new Rect(20f, num2 + 20f, (float)Screen.width - 40f, (float)Screen.height - num - num2 - 40f);
	}

	private static Vector2 GetRectCenterScreen(RectTransform target, Camera eventCamera)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = target.rect;
		return RectTransformUtility.WorldToScreenPoint(eventCamera, ((Transform)target).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center)));
	}

	private static Rect GetRectScreenRect(RectTransform target, Camera eventCamera)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		target.GetWorldCorners(array);
		Vector2 val = RectTransformUtility.WorldToScreenPoint(eventCamera, array[0]);
		Vector2 val2 = RectTransformUtility.WorldToScreenPoint(eventCamera, array[2]);
		return Rect.MinMaxRect(val.x, val.y, val2.x, val2.y);
	}

	private static Camera GetCanvasEventCamera(RectTransform rect)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Canvas val = (((Object)(object)rect != (Object)null) ? ((Component)rect).GetComponentInParent<Canvas>() : null);
		if ((Object)(object)val == (Object)null || (int)val.renderMode == 0)
		{
			return null;
		}
		return val.worldCamera;
	}

	private Camera GetEventCamera()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Canvas val = (((Object)(object)rectTransform != (Object)null) ? ((Component)rectTransform).GetComponentInParent<Canvas>() : null);
		if ((Object)(object)val == (Object)null || (int)val.renderMode == 0)
		{
			return null;
		}
		return val.worldCamera;
	}

	internal static void SetAnchoredPositionLocal(RectTransform parentRect, RectTransform selfRect, Vector2 pointInParentLocal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = parentRect.rect;
		Vector2 anchorMin = selfRect.anchorMin;
		Vector2 anchorMax = selfRect.anchorMax;
		Vector2 pivot = parentRect.pivot;
		float num = ((Rect)(ref rect)).width * (0.5f * (anchorMin.x + anchorMax.x) - pivot.x);
		float num2 = ((Rect)(ref rect)).height * (0.5f * (anchorMin.y + anchorMax.y) - pivot.y);
		selfRect.anchoredPosition = new Vector2(pointInParentLocal.x - num, pointInParentLocal.y - num2);
	}

	private void TryCloseOnOutsidePointerDown()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (Time.frameCount != lastOpenClickFrame && TryGetPointerDownPosition(out var pointerPosition))
		{
			Camera eventCamera = GetEventCamera();
			if (!IsScreenPointInsideTips(pointerPosition, eventCamera) && !IsScreenPointInsideExcludeRects(pointerPosition, eventCamera))
			{
				onRequestClose?.Invoke();
			}
		}
	}

	private static bool TryGetPointerDownPosition(out Vector2 pointerPosition)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetMouseButtonDown(0))
		{
			pointerPosition = Vector2.op_Implicit(Input.mousePosition);
			return true;
		}
		if (Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);
			if ((int)((Touch)(ref touch)).phase == 0)
			{
				pointerPosition = ((Touch)(ref touch)).position;
				return true;
			}
		}
		pointerPosition = Vector2.zero;
		return false;
	}

	private bool IsScreenPointInsideTips(Vector2 screenPosition, Camera eventCamera)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = GetBgRect();
		if ((Object)(object)val != (Object)null)
		{
			return RectTransformUtility.RectangleContainsScreenPoint(val, screenPosition, eventCamera);
		}
		return false;
	}

	private bool IsScreenPointInsideExcludeRects(Vector2 screenPosition, Camera eventCamera)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (currentParam == null)
		{
			return false;
		}
		RectTransform[] excludeClickRects = currentParam.ExcludeClickRects;
		if (excludeClickRects == null)
		{
			return false;
		}
		foreach (RectTransform val in excludeClickRects)
		{
			if ((Object)(object)val != (Object)null && RectTransformUtility.RectangleContainsScreenPoint(val, screenPosition, GetCanvasEventCamera(val)))
			{
				return true;
			}
		}
		return false;
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		ArrowBottom = behaviour.Get<GameObject>("ArrowBottom");
		ArrowTop = behaviour.Get<GameObject>("ArrowTop");
		ArrowLeft = behaviour.Get<GameObject>("ArrowLeft");
		ArrowRight = behaviour.Get<GameObject>("ArrowRight");
		Content = behaviour.Get<GameObject>("Content");
		ItemCell = behaviour.Get<GameObject>("ItemCell");
		HorLayoutContenCell = behaviour.Get<GameObject>("HorLayoutContenCell");
		HorLayoutContenCell_5 = behaviour.Get<GameObject>("HorLayoutContenCell_5");
		TxtDetailCell = behaviour.Get<TMP_Text>("TxtDetailCell");
		if (behaviour.Contains("HorLayoutContenDetail"))
		{
			HorLayoutContenDetail = behaviour.Get<GameObject>("HorLayoutContenDetail");
		}
		if (behaviour.Contains("TxtTitleCell"))
		{
			TxtTitleCell = behaviour.Get<TMP_Text>("TxtTitleCell");
		}
	}
}
