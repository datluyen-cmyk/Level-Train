using UnityEngine;

namespace XingYun.Render;

public sealed class ArrowGuidePath
{
	public Vector3[] Positions;

	public Vector3[] Tangents;

	public float[] Distances;

	public int Count;

	public float OriginalLength;

	public float ExitGuideLength;

	public Vector3 PlaneNormal = Vector3.forward;

	public bool Sample(float dist, out Vector3 position, out Vector3 tangent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		position = Vector3.zero;
		tangent = Vector3.forward;
		if (Count < 2)
		{
			return false;
		}
		if (dist < 0f || dist > ExitGuideLength)
		{
			return false;
		}
		int num = 0;
		int num2 = Count - 1;
		while (num2 - num > 1)
		{
			int num3 = num + num2 >> 1;
			if (Distances[num3] <= dist)
			{
				num = num3;
			}
			else
			{
				num2 = num3;
			}
		}
		float num4 = Distances[num];
		float num5 = Distances[num2] - num4;
		float num6 = ((num5 > 1E-06f) ? ((dist - num4) / num5) : 0f);
		position = Vector3.LerpUnclamped(Positions[num], Positions[num2], num6);
		Vector3 val = Vector3.Slerp(Tangents[num], Tangents[num2], num6);
		tangent = ((Vector3)(ref val)).normalized;
		return true;
	}
}
