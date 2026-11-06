using System;
using UnityEngine;
using XingYun.Core.Singletons;
using XingYun.UI.Adapt;
using XingYun.UI.Core;

namespace Rotaterings.Tools;

public class SafeAreaBgCover : MonoBehaviour
{
	private const float OffsetThreshold = 100f;

	private const float DesignAspect = 0.5f;

	[SerializeField]
	private SafeAreaOverlookMode overlookMode = SafeAreaOverlookMode.Both;

	[SerializeField]
	private float topOffset;

	[SerializeField]
	private float bottomOffset;

	[SerializeField]
	private float minAspect = 0.5f;

	[SerializeField]
	private float maxAspect = 0.5f;

	private RectTransform rectTransform;

	private Vector2 originalAnchoredPosition;

	private Vector2 originalSizeDelta;

	private Vector2 originalContentSize;

	private Vector3 originalScale;

	private void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		originalAnchoredPosition = rectTransform.anchoredPosition;
		originalSizeDelta = rectTransform.sizeDelta;
		Rect rect = rectTransform.rect;
		originalContentSize = ((Rect)(ref rect)).size;
		originalScale = ((Transform)rectTransform).localScale;
	}

	private void Start()
	{
		ScreenManager instance = MonoSingleton<ScreenManager>.Instance;
		instance.ScreenModeChanged = (Action<EScreenMode, int, int>)Delegate.Combine(instance.ScreenModeChanged, new Action<EScreenMode, int, int>(OnScreenModeChanged));
		Apply();
	}

	private void OnDestroy()
	{
		ScreenManager instance = MonoSingleton<ScreenManager>.Instance;
		instance.ScreenModeChanged = (Action<EScreenMode, int, int>)Delegate.Remove(instance.ScreenModeChanged, new Action<EScreenMode, int, int>(OnScreenModeChanged));
	}

	private void OnEnable()
	{
		Apply();
	}

	private void OnScreenModeChanged(EScreenMode screenMode, int width, int height)
	{
		Apply();
	}

	public void Apply()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rectTransform == (Object)null)
		{
			return;
		}
		if (minAspect > maxAspect)
		{
			float num = maxAspect;
			float num2 = minAspect;
			minAspect = num;
			maxAspect = num2;
		}
		float num3 = 0f;
		float num4 = 0f;
		if (overlookMode != SafeAreaOverlookMode.None)
		{
			UIRoot instance = UIRoot.Instance;
			object obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				SafeArea uISafeArea = instance.UISafeArea;
				obj = ((uISafeArea != null) ? ((Component)uISafeArea).transform : null);
			}
			RectTransform val = (RectTransform)((obj is RectTransform) ? obj : null);
			if ((Object)(object)val != (Object)null)
			{
				num3 = 0f - val.offsetMax.y;
				num4 = val.offsetMin.y;
			}
		}
		float num5 = (rectTransform.anchorMin.y + rectTransform.anchorMax.y) * 0.5f;
		float num6 = rectTransform.anchorMax.y - rectTransform.anchorMin.y;
		Vector2 anchoredPosition = originalAnchoredPosition;
		Vector2 sizeDelta = originalSizeDelta;
		if (overlookMode == SafeAreaOverlookMode.Top || overlookMode == SafeAreaOverlookMode.Both)
		{
			anchoredPosition.y += num3 * num5 + ((num3 >= 100f) ? topOffset : 0f);
			sizeDelta.y += num3 * num6;
		}
		if (overlookMode == SafeAreaOverlookMode.Bottom || overlookMode == SafeAreaOverlookMode.Both)
		{
			anchoredPosition.y -= num4 * (1f - num5) + ((num4 >= 100f) ? bottomOffset : 0f);
			sizeDelta.y += num4 * num6;
		}
		rectTransform.anchoredPosition = anchoredPosition;
		rectTransform.sizeDelta = sizeDelta;
		((Transform)rectTransform).localScale = CalculateCoverScale(num3, num4);
	}

	private Vector3 CalculateCoverScale(float topInset, float bottomInset)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (originalContentSize.x <= 0f || originalContentSize.y <= 0f)
		{
			return originalScale;
		}
		if (!TryGetCoverTargetSize(topInset, bottomInset, out var targetW, out var targetH))
		{
			return originalScale;
		}
		float num = targetW / targetH;
		if (num >= minAspect && num <= maxAspect)
		{
			return originalScale;
		}
		float num2 = Mathf.Max(targetW / originalContentSize.x, targetH / originalContentSize.y);
		return new Vector3(originalScale.x * num2, originalScale.y * num2, originalScale.z);
	}

	private bool TryGetCoverTargetSize(float topInset, float bottomInset, out float targetW, out float targetH)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		targetW = 0f;
		targetH = 0f;
		Transform parent = ((Transform)rectTransform).parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		Rect rect = val.rect;
		targetW = ((Rect)(ref rect)).width;
		rect = val.rect;
		targetH = ((Rect)(ref rect)).height;
		if (overlookMode == SafeAreaOverlookMode.Top || overlookMode == SafeAreaOverlookMode.Both)
		{
			targetH += topInset;
		}
		if (overlookMode == SafeAreaOverlookMode.Bottom || overlookMode == SafeAreaOverlookMode.Both)
		{
			targetH += bottomInset;
		}
		if (targetW > 0f)
		{
			return targetH > 0f;
		}
		return false;
	}
}
