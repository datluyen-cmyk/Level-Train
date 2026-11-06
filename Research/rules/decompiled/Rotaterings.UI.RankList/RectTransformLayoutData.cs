using UnityEngine;

namespace Rotaterings.UI.RankList;

public readonly struct RectTransformLayoutData
{
	public Vector2 AnchorMin { get; }

	public Vector2 AnchorMax { get; }

	public Vector2 AnchoredPosition { get; }

	public RectTransformLayoutData(Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		AnchorMin = anchorMin;
		AnchorMax = anchorMax;
		AnchoredPosition = anchoredPosition;
	}

	public bool TryApply(RectTransform target)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target == (Object)null || !IsValid())
		{
			return false;
		}
		if (target.anchorMin != AnchorMin)
		{
			target.anchorMin = AnchorMin;
		}
		if (target.anchorMax != AnchorMax)
		{
			target.anchorMax = AnchorMax;
		}
		if (target.anchoredPosition != AnchoredPosition)
		{
			target.anchoredPosition = AnchoredPosition;
		}
		return true;
	}

	public bool IsValid()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if (IsFinite(AnchorMin) && IsFinite(AnchorMax) && IsFinite(AnchoredPosition) && AnchorMin.x >= 0f && AnchorMin.y >= 0f && AnchorMax.x <= 1f && AnchorMax.y <= 1f && AnchorMin.x <= AnchorMax.x)
		{
			return AnchorMin.y <= AnchorMax.y;
		}
		return false;
	}

	public static bool IsFinite(Vector2 value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!float.IsNaN(value.x) && !float.IsInfinity(value.x) && !float.IsNaN(value.y))
		{
			return !float.IsInfinity(value.y);
		}
		return false;
	}
}
