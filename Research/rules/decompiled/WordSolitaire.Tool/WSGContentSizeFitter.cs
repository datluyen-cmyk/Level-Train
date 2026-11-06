using UnityEngine;
using UnityEngine.UI;

namespace WordSolitaire.Tool;

[AddComponentMenu("Layout/WSG Content Size Fitter", 140)]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class WSGContentSizeFitter : ContentSizeFitter
{
	public bool IsFittingAxis(Axis axis)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		return (int)GetFitMode(axis) > 0;
	}

	public bool IsPreferredSizeAxis(Axis axis)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		return (int)GetFitMode(axis) == 2;
	}

	public bool IsMinSizeAxis(Axis axis)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		return (int)GetFitMode(axis) == 1;
	}

	private FitMode GetFitMode(Axis axis)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		if ((int)axis != 0)
		{
			return ((ContentSizeFitter)this).verticalFit;
		}
		return ((ContentSizeFitter)this).horizontalFit;
	}

	public override void SetLayoutHorizontal()
	{
		((ContentSizeFitter)this).SetLayoutHorizontal();
		ApplyMinMaxClamp((Axis)0);
	}

	public override void SetLayoutVertical()
	{
		((ContentSizeFitter)this).SetLayoutVertical();
		ApplyMinMaxClamp((Axis)1);
	}

	private void ApplyMinMaxClamp(Axis axis)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if ((int)(((int)axis == 0) ? ((ContentSizeFitter)this).horizontalFit : ((ContentSizeFitter)this).verticalFit) == 0)
		{
			return;
		}
		Transform transform = ((Component)this).transform;
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if (!((Object)(object)val == (Object)null))
		{
			Rect rect;
			float num;
			if ((int)axis != 0)
			{
				rect = val.rect;
				num = ((Rect)(ref rect)).height;
			}
			else
			{
				rect = val.rect;
				num = ((Rect)(ref rect)).width;
			}
			float num2 = num;
			if ((int)axis == 0)
			{
				num2 = WSGLayoutUtility.ClampMinWidth(val, num2);
				num2 = WSGLayoutUtility.ClampWidth(val, num2);
			}
			else
			{
				num2 = WSGLayoutUtility.ClampMinHeight(val, num2);
				num2 = WSGLayoutUtility.ClampHeight(val, num2);
			}
			val.SetSizeWithCurrentAnchors(axis, num2);
		}
	}
}
