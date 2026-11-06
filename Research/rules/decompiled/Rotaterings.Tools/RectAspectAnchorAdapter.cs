using System;
using UnityEngine;
using XingYun.Core.Singletons;
using XingYun.UI.Adapt;

namespace Rotaterings.Tools;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class RectAspectAnchorAdapter : MonoBehaviour
{
	[SerializeField]
	private float aspectThreshold = 0.5625f;

	[HideInInspector]
	[SerializeField]
	private RectLayoutSnapshot wideLayout;

	[HideInInspector]
	[SerializeField]
	private RectLayoutSnapshot tallLayout;

	private RectTransform rectTransform;

	private Vector2 fixedAnchorMin;

	private Vector2 fixedAnchorMax;

	private void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		fixedAnchorMin = rectTransform.anchorMin;
		fixedAnchorMax = rectTransform.anchorMax;
	}

	private void Start()
	{
		ScreenManager instance = MonoSingleton<ScreenManager>.Instance;
		instance.ScreenModeChanged = (Action<EScreenMode, int, int>)Delegate.Combine(instance.ScreenModeChanged, new Action<EScreenMode, int, int>(OnScreenModeChanged));
		Apply();
	}

	private void OnDestroy()
	{
		if ((Object)(object)MonoSingleton<ScreenManager>.Instance != (Object)null)
		{
			ScreenManager instance = MonoSingleton<ScreenManager>.Instance;
			instance.ScreenModeChanged = (Action<EScreenMode, int, int>)Delegate.Remove(instance.ScreenModeChanged, new Action<EScreenMode, int, int>(OnScreenModeChanged));
		}
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
		if (!((Object)(object)rectTransform == (Object)null) && TrySelectLayoutSource(out var layout))
		{
			Transform parent = ((Transform)rectTransform).parent;
			RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
			if (!((Object)(object)val == (Object)null))
			{
				GetEdgesFromLayout(layout, val, out var left, out var right, out var bottom, out var top);
				ApplyOffsetFromEdges(val, left, right, bottom, top);
			}
		}
	}

	private bool TrySelectLayoutSource(out RectLayoutSnapshot layout)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		layout = default(RectLayoutSnapshot);
		Vector2 screenSize = ScreenManager.ScreenSize;
		if (screenSize.y <= 0f)
		{
			return false;
		}
		float num = screenSize.x / screenSize.y;
		layout = ((num >= aspectThreshold) ? wideLayout : tallLayout);
		return layout.isRecorded;
	}

	private static void GetEdgesFromLayout(RectLayoutSnapshot layout, RectTransform parent, out float left, out float right, out float bottom, out float top)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = parent.rect;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(Mathf.Lerp(((Rect)(ref rect)).xMin, ((Rect)(ref rect)).xMax, layout.anchorMin.x), Mathf.Lerp(((Rect)(ref rect)).yMin, ((Rect)(ref rect)).yMax, layout.anchorMin.y));
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(Mathf.Lerp(((Rect)(ref rect)).xMin, ((Rect)(ref rect)).xMax, layout.anchorMax.x), Mathf.Lerp(((Rect)(ref rect)).yMin, ((Rect)(ref rect)).yMax, layout.anchorMax.y));
		left = Mathf.Min(val.x, val2.x) + layout.offsetMin.x;
		right = Mathf.Max(val.x, val2.x) + layout.offsetMax.x;
		bottom = Mathf.Min(val.y, val2.y) + layout.offsetMin.y;
		top = Mathf.Max(val.y, val2.y) + layout.offsetMax.y;
	}

	private void ApplyOffsetFromEdges(RectTransform parent, float left, float right, float bottom, float top)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = parent.rect;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(Mathf.Lerp(((Rect)(ref rect)).xMin, ((Rect)(ref rect)).xMax, fixedAnchorMin.x), Mathf.Lerp(((Rect)(ref rect)).yMin, ((Rect)(ref rect)).yMax, fixedAnchorMin.y));
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(Mathf.Lerp(((Rect)(ref rect)).xMin, ((Rect)(ref rect)).xMax, fixedAnchorMax.x), Mathf.Lerp(((Rect)(ref rect)).yMin, ((Rect)(ref rect)).yMax, fixedAnchorMax.y));
		rectTransform.offsetMin = new Vector2(left - Mathf.Min(val.x, val2.x), bottom - Mathf.Min(val.y, val2.y));
		rectTransform.offsetMax = new Vector2(right - Mathf.Max(val.x, val2.x), top - Mathf.Max(val.y, val2.y));
	}
}
