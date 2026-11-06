using System;
using UnityEngine;
using XingYun.Core.Singletons;
using XingYun.UI.Adapt;

namespace Rotaterings.Tools;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class RectTargetStretch : MonoBehaviour
{
	private const string Tag = "UI";

	[SerializeField]
	private RectStretchOrientation stretchOrientation;

	[SerializeField]
	private RectTransform topTarget;

	[SerializeField]
	private RectTransform bottomTarget;

	[SerializeField]
	private RectTransform leftTarget;

	[SerializeField]
	private RectTransform rightTarget;

	private RectTransform rectTransform;

	private Vector2 originalAnchorMin;

	private Vector2 originalAnchorMax;

	private Vector2 originalOffsetMin;

	private Vector2 originalOffsetMax;

	private void Awake()
	{
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		CacheOriginalLayout();
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
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rectTransform == (Object)null)
		{
			return;
		}
		if (!HasAnyTarget())
		{
			RestoreOriginalLayout();
			return;
		}
		Transform parent = ((Transform)rectTransform).parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		GetEdgesFromOriginalLayout(val, out var left, out var right, out var bottom, out var top);
		if (stretchOrientation == RectStretchOrientation.Portrait)
		{
			if ((Object)(object)topTarget != (Object)null)
			{
				top = GetTargetPivotInParentLocal(val, topTarget).y;
			}
			if ((Object)(object)bottomTarget != (Object)null)
			{
				bottom = GetTargetPivotInParentLocal(val, bottomTarget).y;
			}
		}
		else
		{
			if ((Object)(object)leftTarget != (Object)null)
			{
				left = GetTargetPivotInParentLocal(val, leftTarget).x;
			}
			if ((Object)(object)rightTarget != (Object)null)
			{
				right = GetTargetPivotInParentLocal(val, rightTarget).x;
			}
		}
		if (!(left >= right) && !(bottom >= top))
		{
			SetRectEdges(rectTransform, val, left, bottom, right, top);
		}
	}

	private void CacheOriginalLayout()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		originalAnchorMin = rectTransform.anchorMin;
		originalAnchorMax = rectTransform.anchorMax;
		originalOffsetMin = rectTransform.offsetMin;
		originalOffsetMax = rectTransform.offsetMax;
	}

	private void RestoreOriginalLayout()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		rectTransform.anchorMin = originalAnchorMin;
		rectTransform.anchorMax = originalAnchorMax;
		rectTransform.offsetMin = originalOffsetMin;
		rectTransform.offsetMax = originalOffsetMax;
	}

	private bool HasAnyTarget()
	{
		if (stretchOrientation == RectStretchOrientation.Portrait)
		{
			if (!((Object)(object)topTarget != (Object)null))
			{
				return (Object)(object)bottomTarget != (Object)null;
			}
			return true;
		}
		if (!((Object)(object)leftTarget != (Object)null))
		{
			return (Object)(object)rightTarget != (Object)null;
		}
		return true;
	}

	private void GetEdgesFromOriginalLayout(RectTransform parent, out float left, out float right, out float bottom, out float top)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = parent.rect;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(Mathf.Lerp(((Rect)(ref rect)).xMin, ((Rect)(ref rect)).xMax, originalAnchorMin.x), Mathf.Lerp(((Rect)(ref rect)).yMin, ((Rect)(ref rect)).yMax, originalAnchorMin.y));
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(Mathf.Lerp(((Rect)(ref rect)).xMin, ((Rect)(ref rect)).xMax, originalAnchorMax.x), Mathf.Lerp(((Rect)(ref rect)).yMin, ((Rect)(ref rect)).yMax, originalAnchorMax.y));
		left = Mathf.Min(val.x, val2.x) + originalOffsetMin.x;
		right = Mathf.Max(val.x, val2.x) + originalOffsetMax.x;
		bottom = Mathf.Min(val.y, val2.y) + originalOffsetMin.y;
		top = Mathf.Max(val.y, val2.y) + originalOffsetMax.y;
	}

	private static Vector2 GetTargetPivotInParentLocal(RectTransform parent, RectTransform target)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Transform)parent).InverseTransformPoint(((Transform)target).position);
		return new Vector2(val.x, val.y);
	}

	private static void SetRectEdges(RectTransform target, RectTransform parent, float left, float bottom, float right, float top)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = parent.rect;
		float width = ((Rect)(ref rect)).width;
		float height = ((Rect)(ref rect)).height;
		if (!(width <= 0f) && !(height <= 0f))
		{
			float xMin = ((Rect)(ref rect)).xMin;
			float yMin = ((Rect)(ref rect)).yMin;
			target.anchorMin = new Vector2((left - xMin) / width, (bottom - yMin) / height);
			target.anchorMax = new Vector2((right - xMin) / width, (top - yMin) / height);
			target.offsetMin = Vector2.zero;
			target.offsetMax = Vector2.zero;
		}
	}
}
