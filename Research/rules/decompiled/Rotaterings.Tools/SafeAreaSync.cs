using System;
using UnityEngine;
using XingYun.Core.Singletons;
using XingYun.UI.Adapt;
using XingYun.UI.Core;

namespace Rotaterings.Tools;

public class SafeAreaSync : MonoBehaviour
{
	private const float OffsetThreshold = 100f;

	[SerializeField]
	private SafeAreaOverlookMode syncMode;

	[SerializeField]
	private float topOffset;

	[SerializeField]
	private float bottomOffset;

	private RectTransform rectTransform;

	private Vector2 originalAnchoredPosition;

	private Vector2 originalSizeDelta;

	private void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		originalAnchoredPosition = rectTransform.anchoredPosition;
		originalSizeDelta = rectTransform.sizeDelta;
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

	public void SetBaseAndApply(Vector2 basePosition, Vector2 baseSizeDelta)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		originalAnchoredPosition = basePosition;
		originalSizeDelta = baseSizeDelta;
		Apply();
	}

	public void Apply()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		if (syncMode == SafeAreaOverlookMode.None)
		{
			rectTransform.anchoredPosition = originalAnchoredPosition;
			rectTransform.sizeDelta = originalSizeDelta;
			return;
		}
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
		if ((Object)(object)val == (Object)null)
		{
			rectTransform.anchoredPosition = originalAnchoredPosition;
			rectTransform.sizeDelta = originalSizeDelta;
			return;
		}
		float num = 0f - val.offsetMax.y;
		float y = val.offsetMin.y;
		float num2 = (rectTransform.anchorMin.y + rectTransform.anchorMax.y) * 0.5f;
		float num3 = rectTransform.anchorMax.y - rectTransform.anchorMin.y;
		Vector2 anchoredPosition = originalAnchoredPosition;
		Vector2 sizeDelta = originalSizeDelta;
		if (syncMode == SafeAreaOverlookMode.Top || syncMode == SafeAreaOverlookMode.Both)
		{
			anchoredPosition.y -= num * num2 + ((num >= 100f) ? topOffset : 0f);
			sizeDelta.y -= num * num3;
		}
		if (syncMode == SafeAreaOverlookMode.Bottom || syncMode == SafeAreaOverlookMode.Both)
		{
			anchoredPosition.y += y * (1f - num2) + ((y >= 100f) ? bottomOffset : 0f);
			sizeDelta.y -= y * num3;
		}
		rectTransform.anchoredPosition = anchoredPosition;
		rectTransform.sizeDelta = sizeDelta;
	}
}
