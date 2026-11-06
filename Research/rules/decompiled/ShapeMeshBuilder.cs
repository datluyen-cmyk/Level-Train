using System;
using System.Collections.Generic;
using UnityEngine;
using UnlockRing.Core.Geometry;

public static class ShapeMeshBuilder
{
	private const int ArcSegments = 64;

	private const float MinLineHalfWidth = 0.02f;

	public static Mesh Build(IGeometryShape shape)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		if (!(shape is Circle val))
		{
			if (!(shape is Segment val2))
			{
				if (!(shape is Capsule val3))
				{
					if (!(shape is Obb obb))
					{
						ConvexPolygon val4 = (ConvexPolygon)(object)((shape is ConvexPolygon) ? shape : null);
						if (val4 == null)
						{
							if (!(shape is Arc val5))
							{
								if (!(shape is AnnularArc val6))
								{
									CompoundShape val7 = (CompoundShape)(object)((shape is CompoundShape) ? shape : null);
									if (val7 == null)
									{
										return null;
									}
									for (int i = 0; i < val7.ChildCount; i++)
									{
										IGeometryShape child = val7.GetChild(i);
										int count = list.Count;
										Append(child, list, list2, count);
									}
								}
								else
								{
									FillAnnularArc(V2(((AnnularArc)(ref val6)).Center), (float)((AnnularArc)(ref val6)).InnerRadius, (float)((AnnularArc)(ref val6)).OuterRadius, (float)((AnnularArc)(ref val6)).StartClockwiseRadians, (float)((AnnularArc)(ref val6)).ClockwiseSweepRadians, list, list2);
								}
							}
							else
							{
								FillArcBand(V2(((Arc)(ref val5)).Center), (float)((Arc)(ref val5)).Radius, (float)((Arc)(ref val5)).StartClockwiseRadians, (float)((Arc)(ref val5)).ClockwiseSweepRadians, 0.02f, list, list2);
							}
						}
						else
						{
							FillConvexPolygon(val4, list, list2);
						}
					}
					else
					{
						FillObb(obb, list, list2);
					}
				}
				else
				{
					Segment spine = ((Capsule)(ref val3)).Spine;
					Vector2 a = V2(((Segment)(ref spine)).Start);
					spine = ((Capsule)(ref val3)).Spine;
					FillCapsule(a, V2(((Segment)(ref spine)).End), (float)((Capsule)(ref val3)).Radius, list, list2);
				}
			}
			else
			{
				FillSegment(V2(((Segment)(ref val2)).Start), V2(((Segment)(ref val2)).End), 0.02f, list, list2);
			}
		}
		else
		{
			FillCircle(V2(((Circle)(ref val)).Center), (float)((Circle)(ref val)).Radius, list, list2);
		}
		if (list.Count == 0)
		{
			return null;
		}
		Mesh val8 = new Mesh
		{
			name = ((object)shape).GetType().Name
		};
		val8.SetVertices(list);
		val8.SetTriangles(list2, 0);
		val8.RecalculateBounds();
		return val8;
	}

	private static void Append(IGeometryShape shape, List<Vector3> verts, List<int> tris, int offset)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		if (!(shape is Circle val))
		{
			if (!(shape is Segment val2))
			{
				if (!(shape is Capsule val3))
				{
					if (!(shape is Obb obb))
					{
						ConvexPolygon val4 = (ConvexPolygon)(object)((shape is ConvexPolygon) ? shape : null);
						if (val4 == null)
						{
							if (!(shape is Arc val5))
							{
								if (!(shape is AnnularArc val6))
								{
									return;
								}
								FillAnnularArc(V2(((AnnularArc)(ref val6)).Center), (float)((AnnularArc)(ref val6)).InnerRadius, (float)((AnnularArc)(ref val6)).OuterRadius, (float)((AnnularArc)(ref val6)).StartClockwiseRadians, (float)((AnnularArc)(ref val6)).ClockwiseSweepRadians, list, list2);
							}
							else
							{
								FillArcBand(V2(((Arc)(ref val5)).Center), (float)((Arc)(ref val5)).Radius, (float)((Arc)(ref val5)).StartClockwiseRadians, (float)((Arc)(ref val5)).ClockwiseSweepRadians, 0.02f, list, list2);
							}
						}
						else
						{
							FillConvexPolygon(val4, list, list2);
						}
					}
					else
					{
						FillObb(obb, list, list2);
					}
				}
				else
				{
					Segment spine = ((Capsule)(ref val3)).Spine;
					Vector2 a = V2(((Segment)(ref spine)).Start);
					spine = ((Capsule)(ref val3)).Spine;
					FillCapsule(a, V2(((Segment)(ref spine)).End), (float)((Capsule)(ref val3)).Radius, list, list2);
				}
			}
			else
			{
				FillSegment(V2(((Segment)(ref val2)).Start), V2(((Segment)(ref val2)).End), 0.02f, list, list2);
			}
		}
		else
		{
			FillCircle(V2(((Circle)(ref val)).Center), (float)((Circle)(ref val)).Radius, list, list2);
		}
		verts.AddRange(list);
		for (int i = 0; i < list2.Count; i++)
		{
			tris.Add(list2[i] + offset);
		}
	}

	private static Vector2 V2(Vector2 v)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2((float)((Vector2)(ref v)).X, (float)((Vector2)(ref v)).Y);
	}

	private static void FillCircle(Vector2 center, float radius, List<Vector3> verts, List<int> tris)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		int count = verts.Count;
		verts.Add(Vector2.op_Implicit(center));
		for (int i = 0; i <= 64; i++)
		{
			float num = (float)i / 64f * MathF.PI * 2f;
			verts.Add(new Vector3(center.x + Mathf.Cos(num) * radius, center.y + Mathf.Sin(num) * radius, 0f));
		}
		for (int j = 0; j < 64; j++)
		{
			tris.Add(count);
			tris.Add(count + 1 + j);
			tris.Add(count + 1 + j + 1);
		}
	}

	private static void FillSegment(Vector2 a, Vector2 b, float halfWidth, List<Vector3> verts, List<int> tris)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		FillCapsule(a, b, halfWidth, verts, tris);
	}

	private static void FillCapsule(Vector2 a, Vector2 b, float radius, List<Vector3> verts, List<int> tris)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = b - a;
		float magnitude = ((Vector2)(ref val)).magnitude;
		if (magnitude < 1E-06f)
		{
			FillCircle(a, radius, verts, tris);
			return;
		}
		Vector2 val2 = val / magnitude;
		Vector2 val3 = new Vector2(0f - val2.y, val2.x) * radius;
		int count = verts.Count;
		verts.Add(Vector2.op_Implicit(a + val3));
		verts.Add(Vector2.op_Implicit(b + val3));
		verts.Add(Vector2.op_Implicit(b - val3));
		verts.Add(Vector2.op_Implicit(a - val3));
		tris.Add(count);
		tris.Add(count + 1);
		tris.Add(count + 2);
		tris.Add(count);
		tris.Add(count + 2);
		tris.Add(count + 3);
		FillArc(a, radius, AngleOf(val2) + MathF.PI, MathF.PI, verts, tris);
		FillArc(b, radius, AngleOf(-val2) + MathF.PI, MathF.PI, verts, tris);
	}

	private static float AngleOf(Vector2 v)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Atan2(v.y, v.x);
	}

	private static void FillArc(Vector2 center, float radius, float startRad, float sweepRad, List<Vector3> verts, List<int> tris)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		int count = verts.Count;
		verts.Add(Vector2.op_Implicit(center));
		int num = Mathf.Max(4, Mathf.CeilToInt(Mathf.Abs(sweepRad) / (MathF.PI * 2f) * 64f));
		for (int i = 0; i <= num; i++)
		{
			float num2 = startRad + sweepRad * ((float)i / (float)num);
			verts.Add(new Vector3(center.x + Mathf.Cos(num2) * radius, center.y + Mathf.Sin(num2) * radius, 0f));
		}
		for (int j = 0; j < num; j++)
		{
			tris.Add(count);
			tris.Add(count + 1 + j);
			tris.Add(count + 1 + j + 1);
		}
	}

	private static void FillArcBand(Vector2 center, float radius, float startRad, float sweepRad, float halfWidth, List<Vector3> verts, List<int> tris)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		FillAnnularArc(center, radius - halfWidth, radius + halfWidth, startRad, sweepRad, verts, tris);
	}

	private static void FillAnnularArc(Vector2 center, float inner, float outer, float startRad, float sweepRad, List<Vector3> verts, List<int> tris)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (inner < 0f)
		{
			inner = 0f;
		}
		if (outer <= inner)
		{
			outer = inner + 0.001f;
		}
		int num = Mathf.Max(4, Mathf.CeilToInt(Mathf.Abs(sweepRad) / (MathF.PI * 2f) * 64f));
		int count = verts.Count;
		Vector2 val = default(Vector2);
		for (int i = 0; i <= num; i++)
		{
			float num2 = startRad + sweepRad * ((float)i / (float)num);
			((Vector2)(ref val))._002Ector(Mathf.Cos(num2), Mathf.Sin(num2));
			verts.Add(new Vector3(center.x + val.x * inner, center.y + val.y * inner, 0f));
			verts.Add(new Vector3(center.x + val.x * outer, center.y + val.y * outer, 0f));
		}
		for (int j = 0; j < num; j++)
		{
			int num3 = count + j * 2;
			tris.Add(num3);
			tris.Add(num3 + 1);
			tris.Add(num3 + 2);
			tris.Add(num3 + 1);
			tris.Add(num3 + 3);
			tris.Add(num3 + 2);
		}
	}

	private static void FillObb(Obb obb, List<Vector3> verts, List<int> tris)
	{
		FillConvexPolygon(((Obb)(ref obb)).ToPolygon(), verts, tris);
	}

	private static void FillConvexPolygon(ConvexPolygon polygon, List<Vector3> verts, List<int> tris)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<Vector2> vertices = polygon.Vertices;
		int count = verts.Count;
		foreach (Vector2 item in vertices)
		{
			Vector2 current = item;
			verts.Add(new Vector3((float)((Vector2)(ref current)).X, (float)((Vector2)(ref current)).Y, 0f));
		}
		for (int i = 1; i < vertices.Count - 1; i++)
		{
			tris.Add(count);
			tris.Add(count + i);
			tris.Add(count + i + 1);
		}
	}
}
