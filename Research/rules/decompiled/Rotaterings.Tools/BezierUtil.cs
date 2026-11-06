using System;
using UniLog;
using UnityEngine;

namespace Rotaterings.Tools;

public static class BezierUtil
{
	public static Vector3[] GetBezier2Path(Vector3 startPos, Vector3 endPos, Vector3 ctrlPointPos, int pointCount)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Bezier2Path(startPos, ctrlPointPos, endPos, pointCount);
	}

	public static Vector3[] GetBezier2PathByOffset(Vector3 startPos, Vector3 endPos, Vector3 centerOffset, int pointCount)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 bezierCenterPoint = GetBezierCenterPoint(startPos, endPos, centerOffset);
		return Bezier2Path(startPos, bezierCenterPoint, endPos, pointCount);
	}

	private static Vector3 GetBezierCenterPoint(Vector3 startPos, Vector3 endPos, Vector3 centerPosOffset)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = endPos - startPos;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		float num = Vector3.Distance(startPos, endPos);
		float num2 = 0.5f;
		return normalized * (num * num2) + startPos + ((startPos.x > endPos.x) ? (centerPosOffset * -1f) : centerPosOffset);
	}

	private static Vector3 GetBetweenPoint(Vector3 start, Vector3 end, float percent = 0.5f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = end - start;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		float num = Vector3.Distance(start, end);
		return normalized * (num * percent) + start;
	}

	private static Vector3[] Bezier2Path(Vector3 startPos, Vector3 ctrlPoint, Vector3 endPos, int pointCount)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (pointCount < 2)
		{
			Log.LogError("Bezier", "Point count must be at least 2 to generate a Bezier path.");
			return Array.Empty<Vector3>();
		}
		Vector3[] array = (Vector3[])(object)new Vector3[pointCount];
		for (int i = 0; i < pointCount; i++)
		{
			float t = (float)i / (float)(pointCount - 1);
			array[i] = CalculateBezier2(startPos, ctrlPoint, endPos, t);
		}
		return array;
	}

	private static Vector3 CalculateBezier2(Vector3 startPos, Vector3 ctrlPoint, Vector3 endPos, float t)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		return (1f - t) * (1f - t) * startPos + 2f * t * (1f - t) * ctrlPoint + t * t * endPos;
	}

	public static Vector3[] GetBezier3Path(Vector3 startPos, Vector3 ctrlPos1, Vector3 ctrlPos2, Vector3 endPos, int pointCount)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if (pointCount < 2)
		{
			Log.LogError("Bezier", "Point count must be at least 2 to generate a Bezier path.");
			return Array.Empty<Vector3>();
		}
		Vector3[] array = (Vector3[])(object)new Vector3[pointCount];
		for (int i = 0; i < pointCount; i++)
		{
			float num = (float)i / (float)(pointCount - 1);
			float num2 = 1f - num;
			array[i] = num2 * num2 * num2 * startPos + 3f * num2 * num2 * num * ctrlPos1 + 3f * num2 * num * num * ctrlPos2 + num * num * num * endPos;
		}
		return array;
	}

	public static Vector3[] GetBezier4Path(Vector3 startPos, Vector3 endPos, Vector3[] ctrlArr, int pointCount)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		if (pointCount < 2)
		{
			pointCount = 2;
		}
		Vector3 val = ctrlArr[0];
		Vector3 val2 = ctrlArr[1];
		Vector3 val3 = ctrlArr[2];
		Vector3[] array = (Vector3[])(object)new Vector3[pointCount];
		for (int i = 0; i < pointCount; i++)
		{
			float num = (float)i / (float)(pointCount - 1);
			float num2 = 1f - num;
			array[i] = Mathf.Pow(num2, 4f) * startPos + 4f * Mathf.Pow(num2, 3f) * num * val + 6f * Mathf.Pow(num2, 2f) * Mathf.Pow(num, 2f) * val2 + 4f * num2 * Mathf.Pow(num, 3f) * val3 + Mathf.Pow(num, 4f) * endPos;
		}
		return array;
	}
}
