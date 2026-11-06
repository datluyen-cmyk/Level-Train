using System;
using UnityEngine;

namespace Rotaterings.Tools;

[Serializable]
public struct RectLayoutSnapshot
{
	public Vector2 anchorMin;

	public Vector2 anchorMax;

	public Vector2 offsetMin;

	public Vector2 offsetMax;

	public bool isRecorded;

	public static RectLayoutSnapshot FromRectTransform(RectTransform rt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		return new RectLayoutSnapshot
		{
			anchorMin = rt.anchorMin,
			anchorMax = rt.anchorMax,
			offsetMin = rt.offsetMin,
			offsetMax = rt.offsetMax,
			isRecorded = true
		};
	}
}
